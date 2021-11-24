using CommandLine;

public class CommandLineOptions
{
    [Option(shortName: 'w', longName: "week", Required = false, Default = true, HelpText = "Get current arbitration mode")]
    public bool Arbitration { get; set; }

    [Option(shortName: 'l', longName: "livestandings", Required = false, Default = false, HelpText = "Get current live standings")]
    public bool LiveStandings { get; set; }
}