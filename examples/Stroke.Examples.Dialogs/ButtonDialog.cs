using Stroke.Shortcuts;


namespace Stroke.Examples.DialogExamples;

/// <summary>
/// Example of a button dialog with custom choices.
/// Port of Python Prompt Toolkit's button_dialog.py example.
/// </summary>
internal static class ButtonDialog
{
    public static void Run()
    {
        try
        {
            var result = Dialogs.ButtonDialog<bool?>(
                title: "Button dialog example",
                text: "Are you sure?",
                buttons:
                [
                    ("Yes", true),
                    ("No", false),
                    ("Maybe...", null),
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
