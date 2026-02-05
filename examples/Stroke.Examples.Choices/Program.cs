namespace Stroke.Examples.Choices;

/// <summary>
/// Entry point for choice selection examples with command-line routing.
/// </summary>
/// <remarks>
/// <para>
/// Usage: dotnet run -- [example-name]
/// </para>
/// <para>
/// Available examples:
/// - simple-selection: Basic Dialogs.Choice() with 3 options
/// - default: Pre-selected default value
/// - color: Custom styling with Style.FromDict()
/// - with-frame: Frame border using ~AppFilters.IsDone
/// - frame-and-bottom-toolbar: Frame with instructional toolbar
/// - gray-frame-on-accept: Frame color transition on accept
/// - many-choices: Scrollable list with 99 options
/// - mouse-support: Mouse click selection
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
        // Default to simple-selection when no arguments provided
        var exampleName = args.Length > 0 ? args[0] : "simple-selection";

        if (Examples.TryGetValue(exampleName, out var runAction))
        {
            runAction();
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
