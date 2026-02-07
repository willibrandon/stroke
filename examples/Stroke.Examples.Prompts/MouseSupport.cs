using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Multiline prompt with mouse support for text selection.
/// Port of Python Prompt Toolkit's mouse-support.py example.
/// </summary>
public static class MouseSupport
{
    public static void Run()
    {
        try
        {
            Console.WriteLine("This is multiline input. Press [Meta+Enter] or [Esc] followed by [Enter] to accept input.");
            Console.WriteLine("You can click with the mouse in order to select text.");
            var answer = Prompt.RunPrompt(
                "Multiline input: ",
                multiline: true,
                mouseSupport: true);
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
