using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Prompt with a pre-filled default value that the user can edit.
/// Port of Python Prompt Toolkit's get-input-with-default.py example.
/// </summary>
public static class GetInputWithDefault
{
    public static void Run()
    {
        try
        {
            var answer = Prompt.RunPrompt(
                "What is your name: ",
                default_: Environment.UserName);
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
