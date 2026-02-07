using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demo of "operate-and-get-next" — a REPL that keeps the same session.
/// Port of Python Prompt Toolkit's operate-and-get-next.py example.
/// </summary>
public static class OperateAndGetNext
{
    public static void Run()
    {
        var session = new PromptSession<string>("prompt> ");

        while (true)
        {
            try
            {
                session.Prompt();
            }
            catch (KeyboardInterruptException)
            {
                // Ctrl+C pressed - continue loop
            }
            catch (EOFException)
            {
                // Ctrl+D pressed - exit loop
                break;
            }
        }
    }
}
