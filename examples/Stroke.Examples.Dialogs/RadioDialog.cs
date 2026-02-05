using Stroke.FormattedText;
using Stroke.Shortcuts;


namespace Stroke.Examples.DialogExamples;

/// <summary>
/// Example of a radio list selection dialog.
/// Port of Python Prompt Toolkit's radio_dialog.py example.
/// Demonstrates both plain text and HTML-styled options.
/// </summary>
internal static class RadioDialog
{
    public static void Run()
    {
        try
        {
            // First dialog with plain text options
            var result = Dialogs.RadioListDialog<string>(
                title: "Radiolist dialog example",
                text: "Please select a color:",
                values:
                [
                    ("red", "Red"),
                    ("green", "Green"),
                    ("blue", "Blue"),
                    ("orange", "Orange"),
                ]
            ).Run();

            Console.WriteLine($"Result = {result}");

            // Second dialog with HTML-styled options
            result = Dialogs.RadioListDialog<string>(
                title: new Html("Radiolist dialog example <reverse>with colors</reverse>"),
                text: "Please select a color:",
                values:
                [
                    ("red", new Html("<style bg=\"red\" fg=\"white\">Red</style>")),
                    ("green", new Html("<style bg=\"green\" fg=\"white\">Green</style>")),
                    ("blue", new Html("<style bg=\"blue\" fg=\"white\">Blue</style>")),
                    ("orange", new Html("<style bg=\"orange\" fg=\"white\">Orange</style>")),
                ]
            ).Run();

            Console.WriteLine($"Result = {result}");
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
