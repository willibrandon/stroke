namespace Stroke.Examples.Choices;

/// <summary>
/// Entry point for choice examples with command-line routing.
/// </summary>
/// <remarks>
/// <para>
/// Usage: dotnet run -- [example-name]
/// </para>
/// <para>
/// Available examples:
/// - simple-selection: Basic 3-option selection
/// - default: Pre-selected default + HTML message
/// - color: Custom styling with colored text
/// - with-frame: Frame border that hides on accept
/// - frame-and-bottom-toolbar: Frame + navigation instructions
/// - gray-frame-on-accept: Frame color changes when accepted
/// - many-choices: 99 scrollable options
/// - mouse-support: Click to select options
/// </para>
/// </remarks>
internal static class Program
{
    /// <summary>
    /// Dictionary mapping example names to their run functions.
    /// </summary>
    private static readonly Dictionary<string, Action> Examples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["simple-selection"] = SimpleSelection.Run,
        ["default"] = Default.Run,
        ["color"] = Color.Run,
        ["with-frame"] = WithFrame.Run,
        ["frame-and-bottom-toolbar"] = FrameAndBottomToolbar.Run,
        ["gray-frame-on-accept"] = GrayFrameOnAccept.Run,
        ["many-choices"] = ManyChoices.Run,
        ["mouse-support"] = MouseSupport.Run,
    };

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return;
        }

        var exampleName = args[0];

        if (Examples.TryGetValue(exampleName, out var runAction))
        {
            try
            {
                runAction();
            }
            catch (OperationCanceledException)
            {
                // User cancelled with Ctrl+C or Ctrl+D - exit gracefully with no output.
                // This follows the standard .NET pattern (used by Spectre.Console, etc.)
                // where cancellation is signaled via OperationCanceledException.
            }
        }
        else
        {
            Console.Error.WriteLine($"Unknown example: '{exampleName}'");
            Console.Error.WriteLine();
            ShowUsage();
            Environment.Exit(1);
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Stroke Choice Examples");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run --project examples/Stroke.Examples.Choices -- <example-name>");
        Console.WriteLine();
        Console.WriteLine("Available examples:");
        foreach (var name in Examples.Keys.Order())
        {
            Console.WriteLine($"  {name}");
        }
    }
}
