using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Prompt with placeholder text displayed when no input is given.
/// Port of Python Prompt Toolkit's placeholder-text.py example.
/// </summary>
public static class PlaceholderText
{
    public static void Run()
    {
        try
        {
            var answer = Prompt.RunPrompt(
                "Give me some input: ",
                placeholder: new Html("<style color=\"#888888\">(please type something)</style>"));
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
