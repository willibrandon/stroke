using Stroke.Completion;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates combining multiple completers into one using CompletionUtils.Merge().
/// Port of Python Prompt Toolkit's combine-multiple-completers.py example.
/// </summary>
public static class MergedCompleters
{
    private static readonly WordCompleter AnimalCompleter = new(
        [
            "alligator", "ant", "ape", "bat", "bear", "beaver", "bee", "bison",
            "butterfly", "cat", "chicken", "crocodile", "dinosaur", "dog",
            "dolphin", "dove", "duck", "eagle", "elephant", "fish", "goat",
            "gorilla", "kangaroo", "leopard", "lion", "mouse", "rabbit", "rat",
            "snake", "spider", "turkey", "turtle",
        ],
        ignoreCase: true);

    private static readonly WordCompleter ColorCompleter = new(
        [
            "red", "green", "blue", "yellow", "white", "black", "orange",
            "gray", "pink", "purple", "cyan", "magenta", "violet",
        ],
        ignoreCase: true);

    public static void Run()
    {
        try
        {
            var completer = CompletionUtils.Merge([AnimalCompleter, ColorCompleter]);

            var text = Prompt.RunPrompt(
                "Give some animals: ",
                completer: completer,
                completeWhileTyping: false);
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
