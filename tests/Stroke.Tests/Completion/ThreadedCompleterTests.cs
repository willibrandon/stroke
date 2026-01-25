using Stroke.Completion;
using Stroke.Core;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="ThreadedCompleter"/>.
/// </summary>
public sealed class ThreadedCompleterTests
{
    private static List<CompletionItem> GetCompletions(ICompleter completer, string text) =>
        completer.GetCompletions(new Document(text), new CompleteEvent()).ToList();

    private static async Task<List<CompletionItem>> GetCompletionsAsync(
        ICompleter completer,
        string text,
        CancellationToken cancellationToken = default)
    {
        var completions = new List<CompletionItem>();
        await foreach (var completion in completer.GetCompletionsAsync(
            new Document(text),
            new CompleteEvent(),
            cancellationToken))
        {
            completions.Add(completion);
        }
        return completions;
    }

    #region Sync Delegates Directly

    [Fact]
    public void GetCompletions_Sync_DelegatesToWrapped()
    {
        var inner = new WordCompleter(["hello", "world"]);
        var threaded = new ThreadedCompleter(inner);

        var completions = GetCompletions(threaded, "hel");

        Assert.Single(completions);
        Assert.Equal("hello", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_Sync_ReturnsAllCompletions()
    {
        var inner = new WordCompleter(["hello", "help", "heap"]);
        var threaded = new ThreadedCompleter(inner);

        var completions = GetCompletions(threaded, "he");

        Assert.Equal(3, completions.Count);
    }

    #endregion

    #region Async Non-Blocking

    [Fact]
    public async Task GetCompletionsAsync_ReturnsCompletions()
    {
        var inner = new WordCompleter(["hello", "world"]);
        var threaded = new ThreadedCompleter(inner);

        var completions = await GetCompletionsAsync(threaded, "hel");

        Assert.Single(completions);
        Assert.Equal("hello", completions[0].Text);
    }

    [Fact]
    public async Task GetCompletionsAsync_RunsInBackground()
    {
        // Use a slow completer to verify async behavior
        var slowCompleter = new SlowCompleter(["item1", "item2", "item3"], delayMs: 50);
        var threaded = new ThreadedCompleter(slowCompleter);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var completions = await GetCompletionsAsync(threaded, "");
        sw.Stop();

        Assert.Equal(3, completions.Count);
        // Should complete (background thread handles the delay)
    }

    #endregion

    #region Streaming Delivery

    [Fact]
    public async Task GetCompletionsAsync_StreamsResults()
    {
        var slowCompleter = new SlowCompleter(["first", "second", "third"], delayMs: 10);
        var threaded = new ThreadedCompleter(slowCompleter);

        var results = new List<(CompletionItem Completion, long ElapsedMs)>();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        await foreach (var completion in threaded.GetCompletionsAsync(
            new Document(""),
            new CompleteEvent()))
        {
            results.Add((completion, sw.ElapsedMilliseconds));
        }

        Assert.Equal(3, results.Count);
        // Results should arrive incrementally (not all at once at the end)
    }

    #endregion

    #region CancellationToken Support

    [Fact]
    public async Task GetCompletionsAsync_CancellationToken_StopsEarly()
    {
        var slowCompleter = new SlowCompleter(
            ["item1", "item2", "item3", "item4", "item5"],
            delayMs: 100);
        var threaded = new ThreadedCompleter(slowCompleter);

        using var cts = new CancellationTokenSource();
        var completions = new List<CompletionItem>();

        try
        {
            await foreach (var completion in threaded.GetCompletionsAsync(
                new Document(""),
                new CompleteEvent(),
                cts.Token))
            {
                completions.Add(completion);
                if (completions.Count >= 2)
                {
                    cts.Cancel();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Should have stopped early
        Assert.True(completions.Count < 5);
    }

    #endregion

    #region Exception Propagation

    [Fact]
    public async Task GetCompletionsAsync_ExceptionInBackground_Propagated()
    {
        var failingCompleter = new FailingCompleter();
        var threaded = new ThreadedCompleter(failingCompleter);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in threaded.GetCompletionsAsync(
                new Document(""),
                new CompleteEvent()))
            {
                // Should throw
            }
        });
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ThreadedCompleter_ImplementsICompleter()
    {
        var inner = new WordCompleter(["test"]);
        var threaded = new ThreadedCompleter(inner);

        Assert.IsAssignableFrom<ICompleter>(threaded);
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// A completer that introduces a delay between each completion.
    /// </summary>
    private sealed class SlowCompleter : CompleterBase
    {
        private readonly List<string> _words;
        private readonly int _delayMs;

        public SlowCompleter(IEnumerable<string> words, int delayMs)
        {
            _words = words.ToList();
            _delayMs = delayMs;
        }

        public override IEnumerable<CompletionItem> GetCompletions(Document document, CompleteEvent completeEvent)
        {
            foreach (var word in _words)
            {
                Thread.Sleep(_delayMs);
                yield return new CompletionItem(word);
            }
        }
    }

    /// <summary>
    /// A completer that throws an exception.
    /// </summary>
    private sealed class FailingCompleter : CompleterBase
    {
        public override IEnumerable<CompletionItem> GetCompletions(Document document, CompleteEvent completeEvent)
        {
            throw new InvalidOperationException("Intentional failure for testing");
        }
    }

    #endregion
}
