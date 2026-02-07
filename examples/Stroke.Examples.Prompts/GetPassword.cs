using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Password input that displays asterisks instead of actual characters.
/// Port of Python Prompt Toolkit's get-password.py example.
/// </summary>
public static class GetPassword
{
    public static void Run()
    {
        try
        {
            var password = Prompt.RunPrompt("Password: ", isPassword: true);
            Console.WriteLine($"You said: {password}");
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
