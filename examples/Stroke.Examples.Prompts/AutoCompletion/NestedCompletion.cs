using Stroke.Completion;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates hierarchical nested auto-completion for command structures.
/// Port of Python Prompt Toolkit's nested-autocompletion.py example.
/// </summary>
public static class NestedCompletion
{
    public static void Run()
    {
        try
        {
            var completer = NestedCompleter.FromNestedDict(
                new Dictionary<string, object?>
                {
                    ["show"] = new Dictionary<string, object?>
                    {
                        ["version"] = null,
                        ["clock"] = null,
                        ["ip"] = new Dictionary<string, object?>
                        {
                            ["interface"] = new Dictionary<string, object?>
                            {
                                ["brief"] = null,
                            },
                        },
                    },
                    ["exit"] = null,
                });

            var text = Prompt.RunPrompt("Type a command: ", completer: completer);
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
