using Stroke.Shortcuts;


namespace Stroke.Examples.DialogExamples;

/// <summary>
/// Example of a password input dialog with masked input.
/// Port of Python Prompt Toolkit's password_dialog.py example.
/// </summary>
internal static class PasswordDialog
{
    public static void Run()
    {
        try
        {
            var result = Dialogs.InputDialog(
                title: "Password dialog example",
                text: "Please type your password:",
                password: true
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
