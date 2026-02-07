using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Prompt with Vi keybindings enabled.
/// Port of Python Prompt Toolkit's get-input-vi-mode.py example.
/// </summary>
public static class GetInputViMode
{
    public static void Run()
    {
        try
        {
            Console.WriteLine("You have Vi keybindings here. Press [Esc] to go to navigation mode.");
            var answer = Prompt.RunPrompt(
                "Give me some input: ",
                viMode: true);
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
