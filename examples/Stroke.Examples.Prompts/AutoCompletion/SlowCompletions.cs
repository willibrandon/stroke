using Stroke.Completion;
using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Shortcuts;
using CompletionRecord = Stroke.Completion.Completion;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates threaded completion for slow completers with a loading indicator toolbar.
/// Port of Python Prompt Toolkit's slow-completions.py example.
/// </summary>
public static class SlowCompletions
{
    private static readonly string[] Words =
    [
        "alligator", "ant", "ape", "bat", "bear", "beaver", "bee", "bison",
        "butterfly", "cat", "chicken", "crocodile", "dinosaur", "dog",
        "dolphin", "dove", "duck", "eagle", "elephant", "fish", "goat",
        "gorilla", "kangaroo", "leopard", "lion", "mouse", "rabbit", "rat",
        "snake", "spider", "turkey", "turtle",
    ];

    private sealed class SlowCompleter : ICompleter
    {
        private int _loading;

        public bool IsLoading => Volatile.Read(ref _loading) > 0;

        public IEnumerable<CompletionRecord> GetCompletions(Document document, CompleteEvent completeEvent)
        {
            Interlocked.Increment(ref _loading);
            var wordBeforeCursor = document.GetWordBeforeCursor();
            try
            {
                foreach (var word in Words)
                {
                    if (word.StartsWith(wordBeforeCursor, StringComparison.OrdinalIgnoreCase))
                    {
                        Thread.Sleep(200); // Simulate slowness.
                        yield return new CompletionRecord(word, -wordBeforeCursor.Length);
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref _loading);
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
            var slowCompleter = new SlowCompleter();

            AnyFormattedText BottomToolbar() =>
                slowCompleter.IsLoading ? " Loading completions... " : "";

            var text = Prompt.RunPrompt(
                "Give some animals: ",
                completer: slowCompleter,
                completeInThread: true,
                completeWhileTyping: true,
                bottomToolbar: (Func<AnyFormattedText>)BottomToolbar,
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
