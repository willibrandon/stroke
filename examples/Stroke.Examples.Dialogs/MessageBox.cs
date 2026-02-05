using Stroke.Shortcuts;


namespace Stroke.Examples.DialogExamples;

/// <summary>
/// Example of a simple message box dialog.
/// Port of Python Prompt Toolkit's messagebox.py example.
/// </summary>
internal static class MessageBox
{
    public static void Run()
    {
        try
        {
            Dialogs.MessageDialog(
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
