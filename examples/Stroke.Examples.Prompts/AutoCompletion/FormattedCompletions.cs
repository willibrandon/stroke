using Stroke.Completion;
using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Shortcuts;
using CompletionRecord = Stroke.Completion.Completion;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates HTML-formatted display text and meta descriptions in completions.
/// Port of Python Prompt Toolkit's colored-completions-with-formatted-text.py example.
/// </summary>
public static class FormattedCompletions
{
    private static readonly string[] Animals =
    [
        "alligator", "ant", "ape", "bat", "bear", "beaver", "bee", "bison",
        "butterfly", "cat", "chicken", "crocodile", "dinosaur", "dog",
        "dolphin", "dove", "duck", "eagle", "elephant",
    ];

    private static readonly Dictionary<string, string> AnimalFamily = new()
    {
        ["alligator"] = "reptile", ["ant"] = "insect", ["ape"] = "mammal",
        ["bat"] = "mammal", ["bear"] = "mammal", ["beaver"] = "mammal",
        ["bee"] = "insect", ["bison"] = "mammal", ["butterfly"] = "insect",
        ["cat"] = "mammal", ["chicken"] = "bird", ["crocodile"] = "reptile",
        ["dinosaur"] = "reptile", ["dog"] = "mammal", ["dolphin"] = "mammal",
        ["dove"] = "bird", ["duck"] = "bird", ["eagle"] = "bird",
        ["elephant"] = "mammal",
    };

    private static readonly Dictionary<string, string> FamilyColors = new()
    {
        ["mammal"] = "ansimagenta", ["insect"] = "ansigreen",
        ["reptile"] = "ansired", ["bird"] = "ansiyellow",
    };

    private static readonly Dictionary<string, AnyFormattedText> Meta = new()
    {
        ["alligator"] = new Html("An <ansired>alligator</ansired> is a <u>crocodilian</u> in the genus Alligator."),
        ["ant"] = new Html("<ansired>Ants</ansired> are eusocial <u>insects</u> of the family Formicidae."),
        ["ape"] = new Html("<ansired>Apes</ansired> (Hominoidea) are Old World tailless <u>primates</u>."),
        ["bat"] = new Html("<ansired>Bats</ansired> are mammals of the order <u>Chiroptera</u>."),
        ["bee"] = new Html("<ansired>Bees</ansired> are flying <u>insects</u> related to wasps and ants."),
        ["beaver"] = new Html("The <ansired>beaver</ansired> is a large, <u>nocturnal</u>, semiaquatic <u>rodent</u>."),
        ["bear"] = new Html("<ansired>Bears</ansired> are carnivoran <u>mammals</u> of the family Ursidae."),
        ["butterfly"] = new Html("<ansiblue>Butterflies</ansiblue> are <u>insects</u> in the clade Rhopalocera."),
    };

    private sealed class AnimalCompleter : ICompleter
    {
        public IEnumerable<CompletionRecord> GetCompletions(Document document, CompleteEvent completeEvent)
        {
            var word = document.GetWordBeforeCursor();
            foreach (var animal in Animals)
            {
                if (!animal.StartsWith(word, StringComparison.OrdinalIgnoreCase))
                    continue;

                AnyFormattedText? display = null;
                if (AnimalFamily.TryGetValue(animal, out var family))
                {
                    var familyColor = FamilyColors.GetValueOrDefault(family, "default");
                    display = new Html(
                        $"{animal}<b>:</b> <ansired>(<{familyColor}>{family}</{familyColor}>)</ansired>");
                }

                yield return new CompletionRecord(
                    animal,
                    startPosition: -word.Length,
                    display: display,
                    displayMeta: Meta.GetValueOrDefault(animal));
            }
        }

        public async IAsyncEnumerable<CompletionRecord> GetCompletionsAsync(
            Document document,
            CompleteEvent completeEvent,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var completion in GetCompletions(document, completeEvent))
                yield return completion;
            await Task.CompletedTask;
        }
    }

    public static void Run()
    {
        try
        {
            Console.WriteLine("(The completion menu displays colors.)");
            Prompt.RunPrompt("Type an animal: ", completer: new AnimalCompleter());

            Prompt.RunPrompt(
                "Type an animal: ",
                completer: new AnimalCompleter(),
                completeStyle: CompleteStyle.MultiColumn);

            Prompt.RunPrompt(
                "Type an animal: ",
                completer: new AnimalCompleter(),
                completeStyle: CompleteStyle.ReadlineLike);
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
