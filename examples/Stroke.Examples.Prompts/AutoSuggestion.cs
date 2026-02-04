using Stroke.AutoSuggest;
using Stroke.History;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Simple example of a CLI that demonstrates fish-style auto suggestion.
/// </summary>
/// <remarks>
/// <para>
/// When you type some input, it will match the input against the history. If one
/// entry of the history starts with the given input, then it will show the
/// remaining part as a suggestion. Pressing the right arrow will insert this
/// suggestion.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's auto-suggestion.py example.
/// </para>
/// </remarks>
public static class AutoSuggestion
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
        Console.WriteLine("This CLI has fish-style auto-suggestion enable.");
        Console.WriteLine("Type for instance \"pri\", then you'll see a suggestion.");
        Console.WriteLine("Press the right arrow to insert the suggestion.");
        Console.WriteLine("Press Control-C to retry. Control-D to exit.");
        Console.WriteLine();

        var session = new PromptSession<string>(
            history: history,
            autoSuggest: new AutoSuggestFromHistory(),
            enableHistorySearch: true);

        while (true)
        {
            try
            {
                var text = session.Prompt("Say something: ");
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
