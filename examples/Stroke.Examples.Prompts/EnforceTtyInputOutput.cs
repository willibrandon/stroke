using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Prompt that always uses the terminal for input/output, even when stdin/stdout are piped.
/// Port of Python Prompt Toolkit's enforce-tty-input-output.py example.
/// </summary>
/// <remarks>
/// For testing, run as:
///   cat /dev/null | dotnet run --project examples/Stroke.Examples.Prompts -- enforce-tty-input-output > /dev/null
/// </remarks>
public static class EnforceTtyInputOutput
{
    public static void Run()
    {
        try
        {
            using (Stroke.Application.AppContext.CreateAppSessionFromTty())
            {
                Prompt.RunPrompt(">");
            }
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
