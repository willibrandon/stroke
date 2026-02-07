using Stroke.Completion;
using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Shortcuts;
using FText = Stroke.FormattedText.FormattedText;
using CompletionRecord = Stroke.Completion.Completion;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates a custom completer with per-item color styling.
/// Port of Python Prompt Toolkit's colored-completions.py example.
/// </summary>
public static class ColoredCompletions
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
            {
                yield return completion;
            }
            await Task.CompletedTask;
        }
    }

    public static void Run()
    {
        try
        {
            Console.WriteLine("(The completion menu displays colors.)");
            Prompt.RunPrompt("Type a color: ", completer: new ColorCompleter());

            Prompt.RunPrompt(
                "Type a color: ",
                completer: new ColorCompleter(),
                completeStyle: CompleteStyle.MultiColumn);

            Prompt.RunPrompt(
                "Type a color: ",
                completer: new ColorCompleter(),
                completeStyle: CompleteStyle.ReadlineLike);

            // True color prompt.
            var message = new FText(
                ("#cc2244", "T"), ("#bb4444", "r"), ("#996644", "u"), ("#cc8844", "e "),
                ("#ccaa44", "C"), ("#bbaa44", "o"), ("#99aa44", "l"), ("#778844", "o"),
                ("#55aa44", "r "), ("#33aa44", "p"), ("#11aa44", "r"), ("#11aa66", "o"),
                ("#11aa88", "m"), ("#11aaaa", "p"), ("#11aacc", "t"), ("#11aaee", ": "));

            Prompt.RunPrompt(
                message,
                completer: new ColorCompleter(),
                colorDepth: Stroke.Output.ColorDepth.Depth24Bit);
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
