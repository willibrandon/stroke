using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Multiline prompt with indented continuation.
/// Port of Python Prompt Toolkit's multiline-prompt.py example.
/// </summary>
public static class MultilinePrompt
{
    public static void Run()
    {
        try
        {
            var answer = Prompt.RunPrompt(
                "Give me some input: (ESCAPE followed by ENTER to accept)\n > ",
                multiline: true);
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
