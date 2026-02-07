using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Sets the terminal window title via escape sequence.
/// Port of Python Prompt Toolkit's terminal-title.py example.
/// </summary>
public static class TerminalTitle
{
    public static void Run()
    {
        try
        {
            TerminalUtils.SetTitle("This is the terminal title");
            var answer = Prompt.RunPrompt("Give me some input: ");
            TerminalUtils.SetTitle("");
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
