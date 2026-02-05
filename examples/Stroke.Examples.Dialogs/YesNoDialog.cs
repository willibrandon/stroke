using Stroke.Shortcuts;


namespace Stroke.Examples.DialogExamples;

/// <summary>
/// Example of a confirmation (Yes/No) dialog.
/// Port of Python Prompt Toolkit's yes_no_dialog.py example.
/// </summary>
internal static class YesNoDialog
{
    public static void Run()
    {
        try
        {
            var result = Dialogs.YesNoDialog(
                title: "Yes/No dialog example",
                text: "Do you want to confirm?"
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
