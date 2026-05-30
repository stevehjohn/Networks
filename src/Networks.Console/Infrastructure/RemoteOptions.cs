// ReSharper disable UnusedAutoPropertyAccessor.Global

using CommandLine;
using JetBrains.Annotations;
using Networks.Engine.Infrastructure;

namespace Networks.Console.Infrastructure;

[UsedImplicitly]
[Verb("remote", HelpText = "Run puzzles from Puzzle Madness.")]
public class RemoteOptions
{
    [Option('d', "difficulty", Required = true, HelpText = "The class of puzzles to solve.")]
    public Difficulty Difficulty { get; set; }

    [Option('q', "quantity", Required = false, HelpText = "The number of puzzles to solve.")]
    public int? Quantity { get; set; }

    [Option('y', "year", Required = false, HelpText = "The year of the puzzle.")]
    public int? Year { get; [UsedImplicitly] set; }

    [Option('m', "month", Required = false, HelpText = "The month of the puzzle.")]
    public int? Month { get; [UsedImplicitly] set; }

    [Option('w', "day", Required = false, HelpText = "The day of the puzzle.")]
    public int? Day { get; [UsedImplicitly] set; }

    public (bool IsValid, string Message) Validate()
    {
        var dateSpecified = Day.HasValue;

        if (! dateSpecified && ! Quantity.HasValue)
        {
            return (false, "Either --quantity or a date must be specified.");
        }

        if (Day.HasValue)
        {
            Month ??= DateTime.Now.Month;
            
            Year ??= DateTime.Now.Year;

            Quantity = 1;
        }

        return (true, null);
    }
}