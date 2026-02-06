namespace Stroke.Examples.FullScreenExamples;

/// <summary>
/// Entry point for full-screen examples with command-line routing.
/// </summary>
/// <remarks>
/// <para>
/// Usage: dotnet run -- [example-name]
/// </para>
/// <para>
/// Run with --help to see all available examples.
/// </para>
/// </remarks>
internal static class Program
{
    /// <summary>
    /// Dictionary mapping example names to their run functions.
    /// Names use kebab-case matching Python Prompt Toolkit filenames.
    /// </summary>
    private static readonly Dictionary<string, Action> Examples =
        new(StringComparer.OrdinalIgnoreCase)
    {
        // Main examples
        ["hello-world"] = HelloWorld.Run,
        ["dummy-app"] = DummyApp.Run,
        ["no-layout"] = NoLayout.Run,
        ["buttons"] = Buttons.Run,
        ["calculator"] = Calculator.Run,
        ["split-screen"] = SplitScreen.Run,
        ["pager"] = Pager.Run,
        ["full-screen-demo"] = FullScreenDemo.Run,
        ["text-editor"] = TextEditor.Run,
        ["ansi-art-and-textarea"] = AnsiArtAndTextArea.Run,

        // ScrollablePanes
        ["simple-example"] = ScrollablePanes.SimpleExample.Run,
        ["with-completion-menu"] = ScrollablePanes.WithCompletionMenu.Run,

        // SimpleDemos
        ["horizontal-split"] = SimpleDemos.HorizontalSplit.Run,
        ["vertical-split"] = SimpleDemos.VerticalSplit.Run,
        ["alignment"] = SimpleDemos.Alignment.Run,
        ["horizontal-align"] = SimpleDemos.HorizontalAlign.Run,
        ["vertical-align"] = SimpleDemos.VerticalAlign.Run,
        ["floats"] = SimpleDemos.Floats.Run,
        ["float-transparency"] = SimpleDemos.FloatTransparency.Run,
        ["focus"] = SimpleDemos.Focus.Run,
        ["margins"] = SimpleDemos.Margins.Run,
        ["line-prefixes"] = SimpleDemos.LinePrefixes.Run,
        ["colorcolumn"] = SimpleDemos.ColorColumn.Run,
        ["cursorcolumn-cursorline"] = SimpleDemos.CursorHighlight.Run,
        ["autocompletion"] = SimpleDemos.AutoCompletion.Run,
    };

    public static void Main(string[] args)
    {
        var exampleName = args.Length > 0 ? args[0] : null;

        if (exampleName == "--help" || exampleName == "-h")
        {
            ShowUsage();
            return;
        }

        if (exampleName is null)
        {
            Console.Error.WriteLine("Error: No example name provided.");
            Console.Error.WriteLine();
            ShowUsage();
            Environment.Exit(1);
            return;
        }

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
        Console.WriteLine("Stroke Full-Screen Examples");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run -- <example-name>");
        Console.WriteLine();
        Console.WriteLine("Available examples:");

        Console.WriteLine();
        Console.WriteLine("  Main:");
        foreach (var name in new[] { "hello-world", "dummy-app", "no-layout", "buttons",
            "calculator", "split-screen", "pager", "full-screen-demo", "text-editor",
            "ansi-art-and-textarea" })
        {
            Console.WriteLine($"    {name}");
        }

        Console.WriteLine();
        Console.WriteLine("  Scrollable Panes:");
        foreach (var name in new[] { "simple-example", "with-completion-menu" })
        {
            Console.WriteLine($"    {name}");
        }

        Console.WriteLine();
        Console.WriteLine("  Simple Demos:");
        foreach (var name in new[] { "horizontal-split", "vertical-split", "alignment",
            "horizontal-align", "vertical-align", "floats", "float-transparency", "focus",
            "margins", "line-prefixes", "colorcolumn", "cursorcolumn-cursorline",
            "autocompletion" })
        {
            Console.WriteLine($"    {name}");
        }
    }
}
