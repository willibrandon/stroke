using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates system prompt (Meta-!), suspend (Ctrl-Z), and open-in-editor features.
/// Port of Python Prompt Toolkit's system-prompt.py example.
/// </summary>
public static class SystemPrompt
{
    public static void Run()
    {
        try
        {
            Console.WriteLine("System prompt example.");
            Console.WriteLine("Press Meta-! for system prompt, Ctrl-Z to suspend.");

            var text = Prompt.RunPrompt(
                "Say something: ",
                enableSystemPrompt: true,
                enableSuspend: true,
                enableOpenInEditor: true);
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
