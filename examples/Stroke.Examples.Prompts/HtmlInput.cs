using Stroke.Lexers;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates HTML syntax highlighting using PygmentsLexer.
/// Port of Python Prompt Toolkit's html-input.py example.
/// </summary>
public static class HtmlInput
{
    public static void Run()
    {
        try
        {
            var text = Prompt.RunPrompt(
                "Enter HTML: ",
                lexer: PygmentsLexer.FromFilename("example.html"),
                multiline: true);
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
