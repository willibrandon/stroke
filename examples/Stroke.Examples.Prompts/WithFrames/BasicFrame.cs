using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates a prompt with a frame border decoration.
/// Port of Python Prompt Toolkit's with-frame.py example.
/// </summary>
public static class BasicFrame
{
    public static void Run()
    {
        try
        {
            var style = new Style(
            [
                ("frame.border", "#888888"),
            ]);

            var text = Prompt.RunPrompt(
                "Say something: ",
                showFrame: true,
                style: style);
            Console.WriteLine($"You said: {text}");
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
