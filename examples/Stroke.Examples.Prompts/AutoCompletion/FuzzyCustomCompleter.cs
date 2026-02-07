using Stroke.Completion;
using Stroke.Core;
using Stroke.Shortcuts;
using CompletionRecord = Stroke.Completion.Completion;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates a custom completer wrapped in FuzzyCompleter for fuzzy matching.
/// Port of Python Prompt Toolkit's fuzzy-custom-completer.py example.
/// </summary>
public static class FuzzyCustomCompleter
{
    private static readonly string[] Colors =
        ["red", "blue", "green", "orange", "purple", "yellow", "cyan", "magenta", "pink"];

    private sealed class ColorCompleter : ICompleter
    {
        public IEnumerable<CompletionRecord> GetCompletions(Document document, CompleteEvent completeEvent)
        {
            var word = document.GetWordBeforeCursor();
            foreach (var color in Colors)
            {
                if (color.StartsWith(word, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new CompletionRecord(
                        color,
                        startPosition: -word.Length,
                        style: "fg:" + color,
                        selectedStyle: "fg:white bg:" + color);
                }
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
            Prompt.RunPrompt(
                "Type a color: ",
                completer: new Stroke.Completion.FuzzyCompleter(new ColorCompleter()));

            Prompt.RunPrompt(
                "Type a color: ",
                completer: new Stroke.Completion.FuzzyCompleter(new ColorCompleter()),
                completeStyle: CompleteStyle.MultiColumn);

            Prompt.RunPrompt(
                "Type a color: ",
                completer: new Stroke.Completion.FuzzyCompleter(new ColorCompleter()),
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
