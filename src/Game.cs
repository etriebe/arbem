using System;
using System.Text.RegularExpressions;
/*
Week 2
	games = [
        [37816,3,5,-3.5,1,0,"NY Giants (0-1)","Washington (0-1)",0,null,0,0,null],
        [37817,21,6,-2.5,1,0,"Cincinnati (1-0)","Chicago (0-1)",0,null,0,0,null],
        [37818,192,22,-12.5,1,0,"Houston (1-0)","Cleveland (0-1)",0,null,0,0,null],
        [37819,15,17,4.5,1,0,"LA Rams (1-0)","Indianapolis (0-1)",0,null,0,0,null],
        [37820,16,18,3.5,1,0,"Buffalo (0-1)","Miami (1-0)",0,null,0,0,null],
        [37821,19,20,5.5,1,0,"New England (0-1)","NY Jets (0-1)",0,null,0,0,null],
        [37822,14,4,3.5,1,0,"San Francisco (1-0)","Philadelphia (1-0)",0,null,0,0,null],
        [37823,29,24,-5.5,1,0,"Las Vegas (1-0)","Pittsburgh (1-0)",0,null,0,0,null],
        [37824,13,12,3.5,1,0,"New Orleans (1-0)","Carolina (1-0)",0,null,0,0,null],
        [37825,27,23,6.5,1,0,"Denver (1-0)","Jacksonville (0-1)",0,null,0,0,null],
        [37826,9,1,-4.5,1,0,"Minnesota (0-1)","Arizona (1-0)",0,null,0,0,null],
        [37827,11,10,-12.5,1,0,"Atlanta (0-1)","Tampa Bay (1-0)",0,null,0,0,null],
        [37828,2,30,-2.5,1,0,"Dallas (0-1)","LA Chargers (1-0)",0,null,0,0,null],
        [37829,26,31,-5.5,1,0,"Tennessee (0-1)","Seattle (1-0)",0,null,0,0,null],
        [37830,28,25,3.5,1,0,"Kansas City (1-0)","Baltimore (0-1)",0,null,0,0,null],
        [37831,7,8,-10.5,1,0,"Detroit (0-1)","Green Bay (0-1)",0,null,0,0,null]]
week 3
    games = [
        [37832,12,192,0.0,1,0,"Carolina (1-0)","Houston (1-0)",0,null,0,0,null],
        [37833,5,16,0.0,1,0,"Washington (0-1)","Buffalo (0-1)",0,null,0,0,null],
        [37834,6,22,0.0,1,0,"Chicago (0-1)","Cleveland (0-1)",0,null,0,0,null],
        [37835,25,7,0.0,1,0,"Baltimore (0-1)","Detroit (0-1)",0,null,0,0,null],
        [37836,17,26,0.0,1,0,"Indianapolis (0-1)","Tennessee (0-1)",0,null,0,0,null],
        [37837,30,28,0.0,1,0,"LA Chargers (1-0)","Kansas City (1-0)",0,null,0,0,null],
        [37838,13,19,0.0,1,0,"New Orleans (1-0)","New England (0-1)",0,null,0,0,null],
        [37839,11,3,0.0,1,0,"Atlanta (0-1)","NY Giants (0-1)",0,null,0,0,null],
        [37840,21,24,0.0,1,0,"Cincinnati (1-0)","Pittsburgh (1-0)",0,null,0,0,null],
        [37841,1,23,0.0,1,0,"Arizona (1-0)","Jacksonville (0-1)",0,null,0,0,null],
        [37842,20,27,0.0,1,0,"NY Jets (0-1)","Denver (1-0)",0,null,0,0,null],
        [37843,18,29,0.0,1,0,"Miami (1-0)","Las Vegas (1-0)",0,null,0,0,null],
        [37844,10,15,0.0,1,0,"Tampa Bay (1-0)","LA Rams (1-0)",0,null,0,0,null],
        [37845,31,9,0.0,1,0,"Seattle (1-0)","Minnesota (0-1)",0,null,0,0,null],
        [37846,8,14,0.0,1,0,"Green Bay (0-1)","San Francisco (1-0)",0,null,0,0,null],
        [37847,4,2,0.0,1,0,"Philadelphia (1-0)","Dallas (0-1)",0,null,0,0,null]]
*/
public class Game
{
    public int GameID;
    public int AwayTeamID;
    public int HomeTeamID;
    public string AwayTeamName;
    public string HomeTeamName;
    public double HomeTeamSpread;

    public DateTime GameStartTime;

    public static string GetTeamNameFromPickEntry(string unparsedTeamName)
    {
        // string result = unparsedTeamName.Trim("\"".ToCharArray());
        Match teamName = Regex.Match(unparsedTeamName, "^\\\"(?<TeamName>[\\w\\s]+)\\((?<Record>(?<Wins>[\\d])+-(?<Losses>[\\d]+))(-(?<Ties>[\\d]+))?\\)\\\"$");
        string result = teamName.Groups["TeamName"].Value;
        return result.Trim();
    }
}