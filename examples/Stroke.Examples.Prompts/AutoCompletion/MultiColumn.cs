using Stroke.Completion;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates multi-column completion display.
/// Port of Python Prompt Toolkit's multi-column-autocompletion.py example.
/// </summary>
public static class MultiColumn
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

    public static void Run()
    {
        try
        {
            var text = Prompt.RunPrompt(
                "Give some animals: ",
                completer: AnimalCompleter,
                completeStyle: CompleteStyle.MultiColumn);
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
