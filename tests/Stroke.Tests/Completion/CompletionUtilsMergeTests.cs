using Stroke.Completion;
using Stroke.Core;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="CompletionUtils.Merge"/>.
/// </summary>
public sealed class CompletionUtilsMergeTests
{
    [Fact]
    public void Merge_EmptyList_ReturnsDummyCompleter()
    {
        var merged = CompletionUtils.Merge([]);

        Assert.IsType<DummyCompleter>(merged);
    }

    [Fact]
    public void Merge_SingleCompleter_ReturnsMergedCompleter()
    {
        var wordCompleter = new WordCompleter(["hello", "world"]);
        var merged = CompletionUtils.Merge([wordCompleter]);

        var completions = merged.GetCompletions(
            new Document("h", cursorPosition: 1),
            new CompleteEvent(TextInserted: true)).ToList();

        Assert.Single(completions);
        Assert.Equal("hello", completions[0].Text);
    }

    [Fact]
    public void Merge_MultipleCompleters_CombinesResults()
    {
        var completer1 = new WordCompleter(["apple"]);
        var completer2 = new WordCompleter(["avocado"]);
        var merged = CompletionUtils.Merge([completer1, completer2]);

        var completions = merged.GetCompletions(
            new Document("a", cursorPosition: 1),
            new CompleteEvent(TextInserted: true)).ToList();

        Assert.Equal(2, completions.Count);
    }

    [Fact]
    public void Merge_WithDeduplicate_RemovesDuplicates()
    {
        var completer1 = new WordCompleter(["hello"]);
        var completer2 = new WordCompleter(["hello"]);
        var merged = CompletionUtils.Merge([completer1, completer2], deduplicate: true);

        Assert.IsType<DeduplicateCompleter>(merged);
    }

    [Fact]
    public async Task Merge_Async_CombinesResults()
    {
        var completer1 = new WordCompleter(["apple"]);
        var completer2 = new WordCompleter(["avocado"]);
        var merged = CompletionUtils.Merge([completer1, completer2]);

        var completions = new List<CompletionItem>();
        await foreach (var c in merged.GetCompletionsAsync(
            new Document("a", cursorPosition: 1),
            new CompleteEvent(TextInserted: true),
            TestContext.Current.CancellationToken))
        {
            completions.Add(c);
        }

        Assert.Equal(2, completions.Count);
    }

    [Fact]
    public async Task Merge_Async_MultipleCompleters_PreservesOrder()
    {
        var completer1 = new WordCompleter(["alpha"]);
        var completer2 = new WordCompleter(["apex"]);
        var merged = CompletionUtils.Merge([completer1, completer2]);

        var completions = new List<CompletionItem>();
        await foreach (var c in merged.GetCompletionsAsync(
            new Document("a", cursorPosition: 1),
            new CompleteEvent(TextInserted: true),
            TestContext.Current.CancellationToken))
        {
            completions.Add(c);
        }

        Assert.Equal(2, completions.Count);
        Assert.Equal("alpha", completions[0].Text);
        Assert.Equal("apex", completions[1].Text);
    }

    [Fact]
    public void Merge_ToString_ContainsMergedCompleter()
    {
        var merged = CompletionUtils.Merge([new WordCompleter(["test"])]);

        Assert.Contains("MergedCompleter", merged.ToString());
    }
}
