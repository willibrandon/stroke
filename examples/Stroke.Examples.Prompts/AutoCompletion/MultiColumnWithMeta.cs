using Stroke.Completion;
using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates multi-column completion with metadata descriptions.
/// Port of Python Prompt Toolkit's multi-column-autocompletion-with-meta.py example.
/// </summary>
public static class MultiColumnWithMeta
{
    private static readonly WordCompleter AnimalCompleter = new(
        [
            "alligator", "ant", "ape", "bat", "bear", "beaver", "bee", "bison",
            "butterfly", "cat", "chicken", "crocodile", "dinosaur", "dog",
            "dolphin", "dove", "duck", "eagle", "elephant",
        ],
        metaDict: new Dictionary<string, AnyFormattedText>
        {
            ["alligator"] = "An alligator is a crocodilian in the genus Alligator of the family Alligatoridae.",
            ["ant"] = "Ants are eusocial insects of the family Formicidae",
            ["ape"] = "Apes (Hominoidea) are a branch of Old World tailless anthropoid catarrhine primates",
            ["bat"] = "Bats are mammals of the order Chiroptera",
        },
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
