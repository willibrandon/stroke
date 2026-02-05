using Stroke.Shortcuts;

using static Stroke.Shortcuts.Dialogs;

namespace Stroke.Examples.DialogExamples;

/// <summary>
/// Example of a simple message box dialog.
/// Port of Python Prompt Toolkit's messagebox.py example.
/// </summary>
internal static class MessageBoxExample
{
    public static void Run()
    {
        try
        {
            MessageDialog(
                title: "Example dialog window",
                text: "Do you want to continue?\nPress ENTER to quit."
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
