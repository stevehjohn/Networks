using CommandLine;
using Networks.Console.Infrastructure;
using Networks.Console.Runners;

namespace Networks.Console;

public static class EntryPoint
{
    public static void Main(string[] arguments)
    {
        var parser = new Parser(settings =>
        {
            settings.CaseInsensitiveEnumValues = true;
            settings.HelpWriter = System.Console.Out;
        });
        
        parser.ParseArguments<LocalOptions, RemoteOptions>(arguments)
            .WithParsed<LocalOptions>(options => new Local().Run(options.PuzzleNumber))
            .WithParsed<RemoteOptions>(options => new Remote().Run(options));
    }
}