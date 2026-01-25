using Stroke.Completion;
using Stroke.Core;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="DummyCompleter"/>.
/// </summary>
public sealed class DummyCompleterTests
{
    [Fact]
    public void Instance_ReturnsSingleton()
    {
        var instance1 = DummyCompleter.Instance;
        var instance2 = DummyCompleter.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void GetCompletions_ReturnsEmpty()
    {
        var completer = DummyCompleter.Instance;
        var document = new Document("test");
        var evt = new CompleteEvent();

        var completions = completer.GetCompletions(document, evt);

        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_WithTextInserted_ReturnsEmpty()
    {
        var completer = DummyCompleter.Instance;
        var document = new Document("test input");
        var evt = new CompleteEvent(TextInserted: true);

        var completions = completer.GetCompletions(document, evt);

        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_WithCompletionRequested_ReturnsEmpty()
    {
        var completer = DummyCompleter.Instance;
        var document = new Document("test");
        var evt = new CompleteEvent(CompletionRequested: true);

        var completions = completer.GetCompletions(document, evt);

        Assert.Empty(completions);
    }

    [Fact]
    public async Task GetCompletionsAsync_ReturnsEmpty()
    {
        var completer = DummyCompleter.Instance;
        var document = new Document("test");
        var evt = new CompleteEvent();

        var completions = new List<CompletionItem>();
        await foreach (var completion in completer.GetCompletionsAsync(document, evt))
        {
            completions.Add(completion);
        }

        Assert.Empty(completions);
    }

    [Fact]
    public void ImplementsICompleter()
    {
        var completer = DummyCompleter.Instance;

        Assert.IsAssignableFrom<ICompleter>(completer);
    }
}
