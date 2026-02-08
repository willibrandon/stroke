using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Entry point for progress bar examples with command-line routing.
/// </summary>
/// <remarks>
/// <para>
/// Usage: dotnet run -- [example-name]
/// </para>
/// <para>
/// 15 progress bar examples demonstrating iteration tracking, styling,
/// parallel tasks, nested bars, custom formatters, and key bindings.
/// </para>
/// </remarks>
internal static class Program
{
    /// <summary>
    /// Dictionary mapping example names to their async run functions.
    /// </summary>
    private static readonly Dictionary<string, Func<Task>> Examples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["simple-progress-bar"] = SimpleProgressBar.Run,
        ["two-tasks"] = TwoTasks.Run,
        ["unknown-length"] = UnknownLength.Run,
        ["nested-progress-bars"] = NestedProgressBars.Run,
        ["colored-title-label"] = ColoredTitleLabel.Run,
        ["scrolling-task-name"] = ScrollingTaskName.Run,
        ["styled1"] = Styled1.Run,
        ["styled2"] = Styled2.Run,
        ["styled-apt-get"] = StyledAptGet.Run,
        ["styled-rainbow"] = StyledRainbow.Run,
        ["styled-tqdm1"] = StyledTqdm1.Run,
        ["styled-tqdm2"] = StyledTqdm2.Run,
        ["custom-key-bindings"] = CustomKeyBindings.Run,
        ["many-parallel-tasks"] = ManyParallelTasks.Run,
        ["lot-of-parallel-tasks"] = LotOfParallelTasks.Run,
    };

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return 0;
        }

        var exampleName = args[0];

        if (!Examples.TryGetValue(exampleName, out var runFunc))
        {
            Console.Error.WriteLine($"Unknown example: '{exampleName}'");
            Console.Error.WriteLine();
            ShowUsage();
            return 1;
        }

        try
        {
            await runFunc();
        }
        catch (KeyboardInterruptException)
        {
            // Graceful exit on Ctrl-C
        }
        catch (EOFException)
        {
            // Graceful exit on EOF
        }

        return 0;
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Stroke Progress Bar Examples");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run --project examples/Stroke.Examples.ProgressBar -- <example-name>");
        Console.WriteLine();
        Console.WriteLine("Available examples:");
        foreach (var name in Examples.Keys.Order())
        {
            Console.WriteLine($"  {name}");
        }
    }
}
