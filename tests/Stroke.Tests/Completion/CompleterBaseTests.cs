using Stroke.Completion;
using Stroke.Core;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="CompleterBase"/>.
/// </summary>
public sealed class CompleterBaseTests
{
    /// <summary>
    /// Test implementation of CompleterBase for testing.
    /// </summary>
    private sealed class TestCompleter : CompleterBase
    {
        private readonly IEnumerable<CompletionItem> _completions;

        public TestCompleter(IEnumerable<CompletionItem> completions)
        {
            _completions = completions;
        }

        public override IEnumerable<CompletionItem> GetCompletions(Document document, CompleteEvent completeEvent)
        {
            return _completions;
        }
    }

    /// <summary>
    /// Test implementation that tracks calls.
    /// </summary>
    private sealed class TrackingCompleter : CompleterBase
    {
        public int GetCompletionsCallCount { get; private set; }
        public Document? LastDocument { get; private set; }
        public CompleteEvent? LastEvent { get; private set; }

        public override IEnumerable<CompletionItem> GetCompletions(Document document, CompleteEvent completeEvent)
        {
            GetCompletionsCallCount++;
            LastDocument = document;
            LastEvent = completeEvent;
            yield return new CompletionItem("test");
        }
    }

    [Fact]
    public void GetCompletions_IsAbstract_MustBeImplemented()
    {
        var completions = new[] { new CompletionItem("hello"), new CompletionItem("world") };
        var completer = new TestCompleter(completions);

        var result = completer.GetCompletions(new Document("test"), new CompleteEvent()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("hello", result[0].Text);
        Assert.Equal("world", result[1].Text);
    }

    [Fact]
    public async Task GetCompletionsAsync_Default_YieldsSyncResults()
    {
        var completions = new[] { new CompletionItem("a"), new CompletionItem("b"), new CompletionItem("c") };
        var completer = new TestCompleter(completions);

        var results = new List<CompletionItem>();
        await foreach (var completion in completer.GetCompletionsAsync(new Document("test"), new CompleteEvent()))
        {
            results.Add(completion);
        }

        Assert.Equal(3, results.Count);
        Assert.Equal("a", results[0].Text);
        Assert.Equal("b", results[1].Text);
        Assert.Equal("c", results[2].Text);
    }

    [Fact]
    public async Task GetCompletionsAsync_Default_CallsGetCompletions()
    {
        var completer = new TrackingCompleter();
        var document = new Document("input");
        var evt = new CompleteEvent(TextInserted: true);

        var results = new List<CompletionItem>();
        await foreach (var completion in completer.GetCompletionsAsync(document, evt))
        {
            results.Add(completion);
        }

        Assert.Equal(1, completer.GetCompletionsCallCount);
        Assert.Same(document, completer.LastDocument);
        Assert.Equal(evt, completer.LastEvent);
    }

    [Fact]
    public async Task GetCompletionsAsync_EmptySync_ReturnsEmptyAsync()
    {
        var completer = new TestCompleter([]);

        var results = new List<CompletionItem>();
        await foreach (var completion in completer.GetCompletionsAsync(new Document(""), new CompleteEvent()))
        {
            results.Add(completion);
        }

        Assert.Empty(results);
    }

    [Fact]
    public void ImplementsICompleter()
    {
        var completer = new TestCompleter([]);

        Assert.IsAssignableFrom<ICompleter>(completer);
    }

    /// <summary>
    /// Test that subclasses can override GetCompletionsAsync for custom async behavior.
    /// </summary>
    private sealed class AsyncOverrideCompleter : CompleterBase
    {
        public override IEnumerable<CompletionItem> GetCompletions(Document document, CompleteEvent completeEvent)
        {
            yield return new CompletionItem("sync");
        }

        public override async IAsyncEnumerable<CompletionItem> GetCompletionsAsync(
            Document document,
            CompleteEvent completeEvent,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken); // Simulate async work
            yield return new CompletionItem("async");
        }
    }

    [Fact]
    public async Task GetCompletionsAsync_CanBeOverridden()
    {
        var completer = new AsyncOverrideCompleter();

        // Sync returns "sync"
        var syncResults = completer.GetCompletions(new Document(""), new CompleteEvent()).ToList();
        Assert.Single(syncResults);
        Assert.Equal("sync", syncResults[0].Text);

        // Async returns "async"
        var asyncResults = new List<CompletionItem>();
        await foreach (var completion in completer.GetCompletionsAsync(new Document(""), new CompleteEvent()))
        {
            asyncResults.Add(completion);
        }
        Assert.Single(asyncResults);
        Assert.Equal("async", asyncResults[0].Text);
    }
}
