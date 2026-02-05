using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

using static Stroke.Shortcuts.Dialogs;

namespace Stroke.Examples.DialogExamples;

/// <summary>
/// Example of a styled message dialog with custom colors.
/// Port of Python Prompt Toolkit's styled_messagebox.py example.
/// Demonstrates Style.FromDict() and HTML-formatted title.
/// </summary>
internal static class StyledMessageBoxExample
{
    /// <summary>
    /// Custom color scheme with green terminal aesthetic.
    /// </summary>
    private static readonly IStyle ExampleStyle = Style.FromDict(new Dictionary<string, string>
    {
        ["dialog"] = "bg:#88ff88",
        ["dialog frame-label"] = "bg:#ffffff #000000",
        ["dialog.body"] = "bg:#000000 #00ff00",
        ["dialog shadow"] = "bg:#00aa00",
    });

    public static void Run()
    {
        try
        {
            MessageDialog(
                title: new Html(
                    "<style bg=\"blue\" fg=\"white\">Styled</style> " +
                    "<style fg=\"ansired\">dialog</style> window"),
                text: "Do you want to continue?\nPress ENTER to quit.",
                style: ExampleStyle
            ).Run();
        }
        catch (KeyboardInterrupt)
        {
            // Ctrl+C pressed - exit gracefully
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully
        }
    }
}
