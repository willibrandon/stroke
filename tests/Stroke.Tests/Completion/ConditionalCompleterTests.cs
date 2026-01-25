using Stroke.Completion;
using Stroke.Core;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="ConditionalCompleter"/>.
/// </summary>
public sealed class ConditionalCompleterTests
{
    private static List<CompletionItem> GetCompletions(ICompleter completer, string text) =>
        completer.GetCompletions(new Document(text), new CompleteEvent()).ToList();

    #region Filter True Delegates

    [Fact]
    public void GetCompletions_FilterTrue_DelegatesToWrapped()
    {
        var inner = new WordCompleter(["hello", "world"]);
        var conditional = new ConditionalCompleter(inner, () => true);

        var completions = GetCompletions(conditional, "hel");

        Assert.Single(completions);
        Assert.Equal("hello", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_FilterTrue_ReturnsAllCompletions()
    {
        var inner = new WordCompleter(["apple", "apricot", "banana"]);
        var conditional = new ConditionalCompleter(inner, () => true);

        var completions = GetCompletions(conditional, "ap");

        Assert.Equal(2, completions.Count);
    }

    #endregion

    #region Filter False Returns Empty

    [Fact]
    public void GetCompletions_FilterFalse_ReturnsEmpty()
    {
        var inner = new WordCompleter(["hello", "world"]);
        var conditional = new ConditionalCompleter(inner, () => false);

        var completions = GetCompletions(conditional, "hel");

        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_FilterFalse_DoesNotCallInner()
    {
        var callCount = 0;
        var inner = new CountingCompleter(() => callCount++);
        var conditional = new ConditionalCompleter(inner, () => false);

        GetCompletions(conditional, "test");

        Assert.Equal(0, callCount);
    }

    #endregion

    #region Filter Evaluation Timing

    [Fact]
    public void GetCompletions_FilterEvaluatedOncePerCall()
    {
        var evaluationCount = 0;
        var inner = new WordCompleter(["a", "b", "c"]);
        var conditional = new ConditionalCompleter(inner, () =>
        {
            evaluationCount++;
            return true;
        });

        GetCompletions(conditional, "");

        Assert.Equal(1, evaluationCount);
    }

    [Fact]
    public void GetCompletions_FilterChangesOverTime_ReflectsChange()
    {
        var isEnabled = true;
        var inner = new WordCompleter(["hello"]);
        var conditional = new ConditionalCompleter(inner, () => isEnabled);

        // Enabled - returns completions
        var completions1 = GetCompletions(conditional, "hel");
        Assert.Single(completions1);

        // Disable
        isEnabled = false;

        // Disabled - returns empty
        var completions2 = GetCompletions(conditional, "hel");
        Assert.Empty(completions2);
    }

    #endregion

    #region Async Support

    [Fact]
    public async Task GetCompletionsAsync_FilterTrue_DelegatesToWrapped()
    {
        var inner = new WordCompleter(["async1", "async2"]);
        var conditional = new ConditionalCompleter(inner, () => true);

        var completions = new List<CompletionItem>();
        await foreach (var c in conditional.GetCompletionsAsync(new Document("asy"), new CompleteEvent()))
        {
            completions.Add(c);
        }

        Assert.Equal(2, completions.Count);
    }

    [Fact]
    public async Task GetCompletionsAsync_FilterFalse_ReturnsEmpty()
    {
        var inner = new WordCompleter(["async1", "async2"]);
        var conditional = new ConditionalCompleter(inner, () => false);

        var completions = new List<CompletionItem>();
        await foreach (var c in conditional.GetCompletionsAsync(new Document("asy"), new CompleteEvent()))
        {
            completions.Add(c);
        }

        Assert.Empty(completions);
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ConditionalCompleter_ImplementsICompleter()
    {
        var inner = new WordCompleter(["test"]);
        var conditional = new ConditionalCompleter(inner, () => true);

        Assert.IsAssignableFrom<ICompleter>(conditional);
    }

    #endregion

    #region Helper Classes

    private sealed class CountingCompleter : CompleterBase
    {
        private readonly Action _onGetCompletions;

        public CountingCompleter(Action onGetCompletions)
        {
            _onGetCompletions = onGetCompletions;
        }

        public override IEnumerable<CompletionItem> GetCompletions(Document document, CompleteEvent completeEvent)
        {
            _onGetCompletions();
            yield break;
        }
    }

    #endregion
}
