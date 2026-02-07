using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Prompt that automatically accepts the default value without allowing editing.
/// Port of Python Prompt Toolkit's accept-default.py example.
/// </summary>
public static class AcceptDefault
{
    public static void Run()
    {
        try
        {
            var answer = Prompt.RunPrompt(
                new Html("<b>Type <u>some input</u>: </b>"),
                acceptDefault: true,
                default_: "test");
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
