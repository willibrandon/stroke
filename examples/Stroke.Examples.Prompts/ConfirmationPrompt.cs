using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Simple yes/no confirmation prompt.
/// Port of Python Prompt Toolkit's confirmation-prompt.py example.
/// </summary>
public static class ConfirmationPrompt
{
    public static void Run()
    {
        try
        {
            var answer = Prompt.Confirm("Should we do that?");
            Console.WriteLine($"You said: {answer}");
        }
        catch (KeyboardInterruptException)
        {
            // Ctrl+C pressed - exit gracefully
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully
        }
    }
}
