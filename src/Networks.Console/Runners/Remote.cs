using System.Diagnostics;
using System.Net;
using Networks.Console.Infrastructure;
using Networks.Engine;
using Networks.Engine.Board;
using Networks.Engine.Infrastructure;
using static System.Console;

namespace Networks.Console.Runners;

public class Remote
{
    private int _top;

    private int _count;

    private readonly Stopwatch _stopwatch = new();

    public void Run(RemoteOptions options)
    {
        var validationResult = options.Validate();

        if (! validationResult.IsValid)
        {
            WriteLine($"\n{validationResult.Message}\n");
            
            return;
        }
        
        var client = new PuzzleClient();

        var solver = new Solver
        {
            StepCallback = VisualiseStep
        };

        Clear();

        var startTime = DateTime.Now;

        for (var i = 0; i < options.Quantity; i++)
        {
            WriteLine();

            WriteLine($"Fetching {options.Difficulty.ToString().ToLowerInvariant()} puzzle {i + 1:N0} of {options.Quantity:N0}...");

            WriteLine();

            (DateOnly Date, Grid Grid, int Variant)? puzzle = null;

            for (var retry = 1; retry < 21; retry++)
            {
                try
                {
                    puzzle = options.Day > 0 
                        ? client.GetPuzzle(options.Difficulty, new DateOnly(options.Year.Value, options.Month.Value, options.Day.Value)) 
                        : client.GetNextPuzzle(options.Difficulty);
                }
                catch
                {
                    //
                }

                if (puzzle != null)
                {
                    break;
                }

                var sleep = (int) Math.Pow(retry, 2);

                for (var timer = 0; timer < sleep; timer++)
                {
                    if (retry > 1)
                    {
                        CursorTop -= 2;
                    }

                    WriteLine($"Waiting for {sleep - timer:N0}s before attempt {retry}.  ");

                    WriteLine();

                    Thread.Sleep(1_000);

                    CursorTop -= 2;
                    
                    WriteLine("Retrying...                         ");
                    
                    WriteLine();
                }
            }

            Clear();

            if (puzzle == null)
            {
                WriteLine("No puzzles available.");

                break;
            }

            WriteLine();
            
            WriteLine($"Started: {startTime:F}, runtime: {DateTime.Now - startTime:h\\:mm\\:ss\\.fff}.");

            WriteLine();

            WriteLine($"Solving puzzle for {puzzle.Value.Date:D} ({puzzle.Value.Grid.Width}x{puzzle.Value.Grid.Height}).");

            WriteLine();

            WriteLine(puzzle.Value.Grid.ToString());

            WriteLine();

            _top = CursorTop;

            _count = 0;

            _stopwatch.Restart();

            CursorVisible = false;

            var result = solver.Solve(puzzle.Value.Grid);

            CursorVisible = true;

            _stopwatch.Stop();
            
            if (! result)
            {
                WriteLine("Unable to solve the puzzle.");
            
                WriteLine();
            
                break;
            }

            CursorTop = _top;

            WriteLine(puzzle.Value.Grid.ToString());

            WriteLine(@$"Solved in {_stopwatch.Elapsed:h\:mm\:ss\.fff}, with {_count:N0} iterations.");

            WriteLine();

            WriteLine("Sending result...");

            WriteLine();

            for (var retry = 1; retry < 21; retry++)
            {
                (HttpStatusCode StatusCode, PuzzleSolvedResponse Response) response;
                
                try
                {
                    response = client.SendResult(puzzle.Value.Date, puzzle.Value.Grid, puzzle.Value.Variant);
                }
                catch
                {
                    response = (HttpStatusCode.InternalServerError, null);
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    WriteLine($"Result not accepted. Status code: {(int) response.StatusCode}.");
                    
                    WriteLine();
            
                    var sleep = (int) Math.Pow(retry, 2);
            
                    for (var timer = 0; timer < sleep; timer++)
                    {
                        if (retry > 1)
                        {
                            CursorTop -= 2;
                        }
            
                        WriteLine($"Waiting for {sleep - timer:N0}s before attempt {retry}.  ");
            
                        WriteLine();
            
                        Thread.Sleep(1_000);
            
                        CursorTop -= 2;
                    
                        WriteLine("Retrying...                         ");
                    
                        WriteLine();
                    }
                }
                else
                {
                    WriteLine("Result accepted.");
                    
                    WriteLine();

                    try
                    {
                        var userResult = response.Response!.GlobalLeaderboard.Single(p => p.Username.Equals("StevöHJ", StringComparison.InvariantCultureIgnoreCase));
                    
                        WriteLine($"Position: {userResult.Position:N0}, score: {userResult.Score:N0}.");
                    }
                    catch
                    {
                        continue;
                    }

                    break;
                }
            }
        }

        WriteLine();
    }

    private void VisualiseStep(Grid grid)
    {
        if (_count++ % 10_000 != 0)
        {
            return;
        }

        CursorTop = _top;

        WriteLine(grid.ToString());

        WriteLine($@"Elapsed: {_stopwatch.Elapsed:h\:mm\:ss\.fff}, Steps: {_count:N0}.    ");
    }
}