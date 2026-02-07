using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Prompt with line wrapping disabled for horizontal scrolling.
/// Port of Python Prompt Toolkit's no-wrapping.py example.
/// </summary>
public static class NoWrapping
{
    public static void Run()
    {
        try
        {
            var answer = Prompt.RunPrompt(
                "Give me some input: ",
                wrapLines: false,
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
