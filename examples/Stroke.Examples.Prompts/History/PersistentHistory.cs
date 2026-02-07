using Stroke.History;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates cross-session persistent history using FileHistory with a temp file.
/// Port of Python Prompt Toolkit's persistent-history.py example.
/// </summary>
public static class PersistentHistory
{
    public static void Run()
    {
        try
        {
            var historyFile = Path.Combine(Path.GetTempPath(), ".stroke-example-history");
            var ourHistory = new FileHistory(historyFile);
            var session = new PromptSession<string>("Say something: ", history: ourHistory);

            while (true)
            {
                try
                {
                    var text = session.Prompt();
                    Console.WriteLine($"You said: {text}");
                }
                catch (EOFException)
                {
                    break;
                }
            }
        }
        catch (KeyboardInterruptException)
        {
            // Ctrl+C pressed - exit gracefully
        }
    }
}
