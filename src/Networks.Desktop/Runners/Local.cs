using Networks.Desktop.Presentation;
using Networks.Engine.Infrastructure;

namespace Networks.Desktop.Runners;

public static class Local
{
    public static void Run(int puzzleNumber)
    {
        PuzzleManager.Path = "Data/Puzzles.json";

        using var renderer = new PuzzleRenderer();

        renderer.Grid = PuzzleManager.Instance.GetPuzzle(puzzleNumber);

        renderer.Run();
    }
}