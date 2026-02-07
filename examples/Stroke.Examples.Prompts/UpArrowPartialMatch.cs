using Stroke.History;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates up-arrow partial string matching.
/// When you type some input, it's possible to use the up arrow to filter the
/// history on the items starting with the given input text.
/// Port of Python Prompt Toolkit's up-arrow-partial-string-matching.py example.
/// </summary>
public static class UpArrowPartialMatch
{
    public static void Run()
    {
        // Create some history first. (Easy for testing.)
        var history = new InMemoryHistory();
        history.AppendString("import os");
        history.AppendString("print(\"hello\")");
        history.AppendString("print(\"world\")");
        history.AppendString("import path");

        // Print help.
        Console.WriteLine("This CLI has up-arrow partial string matching enabled.");
        Console.WriteLine("Type for instance \"pri\" followed by up-arrow and you");
        Console.WriteLine("get the last items starting with \"pri\".");
        Console.WriteLine("Press Control-C to retry. Control-D to exit.");
        Console.WriteLine();

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
                break;
            }
            catch (KeyboardInterruptException)
            {
                // Ctrl-C pressed. Try again.
            }
            catch (EOFException)
            {
                // Ctrl-D pressed. Exit.
                break;
            }
        }
    }
}
