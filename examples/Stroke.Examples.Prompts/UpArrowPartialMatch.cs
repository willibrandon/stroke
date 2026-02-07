using Stroke.History;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates partial string matching on up-arrow using enableHistorySearch.
/// Port of Python Prompt Toolkit's up-arrow-partial-string-matching.py example.
/// </summary>
public static class UpArrowPartialMatch
{
    public static void Run()
    {
        try
        {
            // Pre-populate history so there's something to search.
            var history = new InMemoryHistory();
            history.AppendString("import os");
            history.AppendString("move cursor to end");
            history.AppendString("import shutil");
            history.AppendString("the quick brown fox");

            Console.WriteLine("Type a partial command and press up-arrow to match.");
            Console.WriteLine("Try typing 'import' then pressing up.");

            var session = new PromptSession<string>(
                "Say something: ",
                history: history,
                enableHistorySearch: true);

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
