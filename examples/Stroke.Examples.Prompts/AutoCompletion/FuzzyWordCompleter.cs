using Stroke.Completion;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Fuzzy word completer example with automatic completion while typing.
/// </summary>
/// <remarks>
/// <para>
/// Completions appear automatically as you type (no Tab required).
/// Fuzzy matching allows partial/out-of-order character matching.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's fuzzy-word-completer.py example.
/// </para>
/// </remarks>
public static class FuzzyWordCompleterExample
{
    private static readonly FuzzyWordCompleter AnimalCompleter = new(
    [
        "alligator",
        "ant",
        "ape",
        "bat",
        "bear",
        "beaver",
        "bee",
        "bison",
        "butterfly",
        "cat",
        "chicken",
        "crocodile",
        "dinosaur",
        "dog",
        "dolphin",
        "dove",
        "duck",
        "eagle",
        "elephant",
        "fish",
        "goat",
        "gorilla",
        "kangaroo",
        "leopard",
        "lion",
        "mouse",
        "rabbit",
        "rat",
        "snake",
        "spider",
        "turkey",
        "turtle",
    ]);

    public static void Run()
    {
        var text = Prompt.RunPrompt(
            "Give some animals: ",
            completer: AnimalCompleter,
            completeWhileTyping: true);
        Console.WriteLine($"You said: {text}");
    }
}
