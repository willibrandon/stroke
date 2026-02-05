using Stroke.Shortcuts;


namespace Stroke.Examples.DialogExamples;

/// <summary>
/// Example of an input box dialog for text entry.
/// Port of Python Prompt Toolkit's input_dialog.py example.
/// </summary>
internal static class InputDialog
{
    public static void Run()
    {
        try
        {
            var result = Dialogs.InputDialog(
                title: "Input dialog example",
                text: "Please type your name:"
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
