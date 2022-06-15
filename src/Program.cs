using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Dynamic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CommandLine;

namespace arbem
{
    class Program
    {
        public static CookieContainer cookieContainer = new CookieContainer();
        public static IConfigurationRoot appSettings;

        private static string username;

        private static string password;

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
            username = builder["Username"];
            password = builder["Password"];

            ParserResult<CommandLineOptions> options = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(RunOptions);
        }
        

        static void RunOptions(CommandLineOptions options)
        {
            if (options.Arbitration)
            {
                HttpWebResponse response;

                if (LoginOfficeFootballPool(out response))
                {
                    HttpWebResponse standingsResponse;
                    Week currentVegasOdds = GetCurrentVegasOdds();
                    Week currentOfpWeek = GetOfficeFootballPoolSpreads(out standingsResponse);
                    GetBestArb(currentOfpWeek, currentVegasOdds);

                    standingsResponse.Close();
                    response.Close();
                }
            }
            else
            {
                HttpWebResponse response;

                if (LoginOfficeFootballPool(out response))
                {
                    HttpWebResponse standingsResponse;
                    GetWeekStandings(out standingsResponse);
                    standingsResponse.Close();
                    response.Close();
                }
            }
        }

        private static void GetBestArb(Week ofpSpreads, Week currentSpreads)
        {
            List<Arbitration> allArbitration = new List<Arbitration>();
            foreach (Game currentOfbGame in ofpSpreads.Games)
            {
                string modifiedHomeTeamName = currentOfbGame.HomeTeamName.Replace("NY", "New York");
                modifiedHomeTeamName = modifiedHomeTeamName.Replace("LA", "Los Angeles");
                Game currentSpreadGame = currentSpreads.Games.Where(g => 
                    g.HomeTeamName.Contains(modifiedHomeTeamName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                Arbitration currentArbitration = new Arbitration(currentOfbGame, currentSpreadGame);
                allArbitration.Add(currentArbitration);
            }

            IEnumerable<Arbitration> sortedArbitration = from arb in allArbitration
                                                        orderby Math.Abs(arb.CurrentHomeTeamArbitration) descending
                                                        select arb;
            foreach (Arbitration arb in sortedArbitration)
            {
                string teamToBetOn = string.Empty;
                if (arb.CurrentHomeTeamArbitration < 0)
                {
                    teamToBetOn = arb.OriginalGameSpread.HomeTeamName;
                }
                else
                {
                    teamToBetOn = arb.OriginalGameSpread.AwayTeamName;
                }

                Console.WriteLine($"Arb: {arb.CurrentHomeTeamArbitration.ToString("0.00")}, Bet on {teamToBetOn}, Home: {arb.OriginalGameSpread.HomeTeamName}, Away: {arb.OriginalGameSpread.AwayTeamName}, Original Spread: {arb.OriginalGameSpread.HomeTeamSpread}, Current Spread: {arb.CurrentGameSpread.HomeTeamSpread.ToString("0.00")}");
            }
        }

        public static Week GetCurrentVegasOdds()
        {
            HttpWebResponse response;
            string apiKey = "3731c8156595debf12abe253ee167551";
            string sportKey = "americanfootball_nfl";
            string currentSpreadsURL = $"https://api.the-odds-api.com/v4/sports/{sportKey}/odds?api_key={apiKey}&regions=us&markets=spreads&oddsFormat=american&dateFormat=iso";
            if (WebUtils.GetBasicRequest(
                    out response,
                    currentSpreadsURL))
            {
                Stream receiveStream = response.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                StringBuilder fullResponse = new StringBuilder();
                using (StreamReader readStream = new StreamReader(receiveStream, encode))
                {
                    string line;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    while ((line = readStream.ReadLine()) != null)
                    {
                        fullResponse.AppendLine(line);
                    }
                }
                string fullResponseString = fullResponse.ToString();
                dynamic allGames = JValue.Parse(fullResponseString);
                Week currentWeek = new Week();
                foreach (var game in allGames)
                {
                    string homeTeamName = game.home_team;
                    string awayTeamName = game.away_team;
                    DateTime gameStartTime = game.commence_time;

                    List<double> allSpreads = new List<double>();
                    foreach (var bookmaker in game.bookmakers)
                    {
                        foreach (var outcome in bookmaker.markets[0].outcomes)
                        {
                            string currentOutcomeTeam = outcome.name;
                            if (currentOutcomeTeam == homeTeamName)
                            {
                                if (outcome.point == null)
                                {
                                    continue;
                                }
                                allSpreads.Add(outcome.point.Value);
                            }
                        }
                    }
                    double averageSpread = allSpreads.Average();

                    Game currentGame = new Game();
                    currentGame.HomeTeamName = homeTeamName;
                    currentGame.AwayTeamName = awayTeamName;
                    currentGame.HomeTeamSpread = averageSpread;
                    currentGame.GameStartTime = gameStartTime;
                    DateTime gameCutoff = GetGameCutoff();
                    if (currentGame.GameStartTime > gameCutoff)
                    {
                        Console.WriteLine($"Skipping game {currentGame.HomeTeamName} vs {currentGame.HomeTeamName} because its start time ({currentGame.GameStartTime}) is greater than the cutoff ({gameCutoff})");
                        continue;
                    }
                    currentWeek.Games.Add(currentGame);
                }
                return currentWeek;
            }

            throw new Exception("Failed to get current Vegas spreads"); 
        }

        private static DateTime GetGameCutoff()
        {
            DateTime currentTime = DateTime.Now;
            if (currentTime.DayOfWeek == DayOfWeek.Monday)
            {
                return currentTime.AddDays(1);
            }
            else
            {
                int daysToOffset = (9 - (int) currentTime.DayOfWeek) % 7;
                if (daysToOffset == 0)
                {
                    daysToOffset += 7;
                }
                return currentTime.AddDays(daysToOffset + 1);
            }
        }

        private static bool LoginOfficeFootballPool(out HttpWebResponse response)
        {
            response = null;
            if (WebUtils.OfficeFootballPoolLoginPostRequest(
                    out response,
                    "https://www.officefootballpool.com/members.cfm?p=1",
                    cookieContainer,
                    username,
                    password))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Week GetOfficeFootballPoolSpreads(out HttpWebResponse response, int requestedWeekID = -1)
        {
            response = null;
            int weekID = GetWeekID(requestedWeekID);
            string currentSpreadsURL = $"https://www.officefootballpool.com/picks.cfm?weekid={weekID}";
            if (WebUtils.GetRequest(
                    out response,
                    currentSpreadsURL,
                    cookieContainer))
            {
                Stream receiveStream = response.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                using (StreamReader readStream = new StreamReader(receiveStream, encode))
                {
                    string line;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    while ((line = readStream.ReadLine()) != null)
                    {
                        if (line.Trim().StartsWith("games"))
                        {
                            Week week = GetWeekFromSource(line);
                            return week;
                        }
                    }
                }
            }

            throw new Exception("Failed to query for OfficeFootballPool spreads"); 
        }
        public static bool GetWeekStandings(out HttpWebResponse response, int requestedWeekID = -1)
        {
            response = null;
            int weekID = GetWeekID(requestedWeekID);
            string liveStandingsRequestURL = $"https://www.officefootballpool.com/getPWPicksJSON.cfm?weekid={weekID}&seasonview=0&memberid_list=0&dummy={GetDummyDate()}";
            if (WebUtils.GetRequest(
                    out response,
                    liveStandingsRequestURL,
                    cookieContainer))
            {
                Stream receiveStream = response.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                StringBuilder sb = new StringBuilder();
                using (StreamReader readStream = new StreamReader(receiveStream, encode))
                {
                    string line;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    while ((line = readStream.ReadLine()) != null)
                    {
                        sb.AppendLine(line);
                    }
                }
                Console.WriteLine(sb.ToString());
                return true;
            }
            else
            {
                return false;
            }
        }

        private static Week GetWeekFromSource(string line)
        {
            // games = [[37832,12,192,0.0,1,0,"Carolina (1-0)","Houston (1-0)",0,null,0,0,null],[37833,5,16,0.0,1,0,"Washington (0-1)","Buffalo (0-1)",0,null,0,0,null],[37834,6,22,0.0,1,0,"Chicago (0-1)","Cleveland (0-1)",0,null,0,0,null],[37835,25,7,0.0,1,0,"Baltimore (0-1)","Detroit (0-1)",0,null,0,0,null],[37836,17,26,0.0,1,0,"Indianapolis (0-1)","Tennessee (0-1)",0,null,0,0,null],[37837,30,28,0.0,1,0,"LA Chargers (1-0)","Kansas City (1-0)",0,null,0,0,null],[37838,13,19,0.0,1,0,"New Orleans (1-0)","New England (0-1)",0,null,0,0,null],[37839,11,3,0.0,1,0,"Atlanta (0-1)","NY Giants (0-1)",0,null,0,0,null],[37840,21,24,0.0,1,0,"Cincinnati (1-0)","Pittsburgh (1-0)",0,null,0,0,null],[37841,1,23,0.0,1,0,"Arizona (1-0)","Jacksonville (0-1)",0,null,0,0,null],[37842,20,27,0.0,1,0,"NY Jets (0-1)","Denver (1-0)",0,null,0,0,null],[37843,18,29,0.0,1,0,"Miami (1-0)","Las Vegas (1-0)",0,null,0,0,null],[37844,10,15,0.0,1,0,"Tampa Bay (1-0)","LA Rams (1-0)",0,null,0,0,null],[37845,31,9,0.0,1,0,"Seattle (1-0)","Minnesota (0-1)",0,null,0,0,null],[37846,8,14,0.0,1,0,"Green Bay (0-1)","San Francisco (1-0)",0,null,0,0,null],[37847,4,2,0.0,1,0,"Philadelphia (1-0)","Dallas (0-1)",0,null,0,0,null]]
            // var options = new JsonSerializerOptions { WriteIndented = true,  };
            // JsonSerializer.Deserialize("[37832,12,192,0.0,1,0,\"Carolina (1-0)\",\"Houston (1-0)\",0,null,0,0,null]");

            line = line.Trim().Replace("games = ", string.Empty);
            string[] games = line.Split("],[");

            Week currentWeek = new Week();
            foreach (string game in games)
            {
                string trimmedGame = game.Trim("[]".ToCharArray());
                string[] properties = trimmedGame.Split(",".ToCharArray());
                Game currentGame = new Game();
                currentGame.GameID = int.Parse(properties[0]);
                currentGame.AwayTeamID = int.Parse(properties[1]);
                currentGame.HomeTeamID = int.Parse(properties[2]);
                currentGame.HomeTeamSpread = double.Parse(properties[3]);
                currentGame.AwayTeamName = Game.GetTeamNameFromPickEntry(properties[6]);
                currentGame.HomeTeamName = Game.GetTeamNameFromPickEntry(properties[7]);
                currentWeek.Games.Add(currentGame);
            }
            return currentWeek;
        }
        private static int GetWeekID(int requestedWeekID = -1)
        {
            int weekOneID = 527;
            if (requestedWeekID == -1)
            {
                DateTime startOfWeekOne = new DateTime(2021, 9, 6);
                DateTime endOfWeekOne = new DateTime(2021, 9, 13);
                DateTime currentTime = DateTime.Now;
                TimeSpan timeSinceStart = currentTime - startOfWeekOne;
                return (int) (timeSinceStart.TotalDays / 7) + weekOneID;
            }
            else
            {
                return weekOneID + (requestedWeekID - 1);
            }
        }

        private static string GetDummyDate()
        {
            return DateTime.Now.Ticks.ToString();
        }

    }
}
