using Stroke.Completion;
using Stroke.Core;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="DynamicCompleter"/>.
/// </summary>
public sealed class DynamicCompleterTests
{
    private static List<CompletionItem> GetCompletions(ICompleter completer, string text) =>
        completer.GetCompletions(new Document(text), new CompleteEvent()).ToList();

    #region Dynamic Resolution

    [Fact]
    public void GetCompletions_DynamicResolution_UsesReturnedCompleter()
    {
        var completer1 = new WordCompleter(["alpha", "beta"]);
        var dynamic = new DynamicCompleter(() => completer1);

        var completions = GetCompletions(dynamic, "al");

        Assert.Single(completions);
        Assert.Equal("alpha", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_NullReturned_UsesDummyCompleter()
    {
        var dynamic = new DynamicCompleter(() => null);

        var completions = GetCompletions(dynamic, "test");

        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_CompleterChangesOverTime_ReflectsChange()
    {
        var completer1 = new WordCompleter(["first"]);
        var completer2 = new WordCompleter(["second"]);
        var currentCompleter = completer1;

        var dynamic = new DynamicCompleter(() => currentCompleter);

        // First call returns completer1's results
        var completions1 = GetCompletions(dynamic, "fir");
        Assert.Single(completions1);
        Assert.Equal("first", completions1[0].Text);

        // Change the completer
        currentCompleter = completer2;

        // Second call returns completer2's results
        var completions2 = GetCompletions(dynamic, "sec");
        Assert.Single(completions2);
        Assert.Equal("second", completions2[0].Text);
    }

    #endregion

    #region Async Support

    [Fact]
    public async Task GetCompletionsAsync_DynamicResolution_Works()
    {
        var completer = new WordCompleter(["async1", "async2"]);
        var dynamic = new DynamicCompleter(() => completer);

        var completions = new List<CompletionItem>();
        await foreach (var c in dynamic.GetCompletionsAsync(new Document("asy"), new CompleteEvent(), TestContext.Current.CancellationToken))
        {
            completions.Add(c);
        }

        Assert.Equal(2, completions.Count);
    }

    [Fact]
    public async Task GetCompletionsAsync_NullReturned_ReturnsEmpty()
    {
        var dynamic = new DynamicCompleter(() => null);

        var completions = new List<CompletionItem>();
        await foreach (var c in dynamic.GetCompletionsAsync(new Document("test"), new CompleteEvent(), TestContext.Current.CancellationToken))
        {
            completions.Add(c);
        }

        Assert.Empty(completions);
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void DynamicCompleter_ImplementsICompleter()
    {
        var dynamic = new DynamicCompleter(() => null);
        Assert.IsAssignableFrom<ICompleter>(dynamic);
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_ContainsClassName()
    {
        var inner = new WordCompleter(["test"]);
        var dynamic = new DynamicCompleter(() => inner);

        var str = dynamic.ToString()!;

        Assert.Contains("DynamicCompleter", str);
    }

    [Fact]
    public void ToString_NullResolver_ContainsDynamicCompleter()
    {
        var dynamic = new DynamicCompleter(() => null);

        var str = dynamic.ToString()!;

        Assert.Contains("DynamicCompleter", str);
    }

    #endregion

    #region Callback Called Each Time

    [Fact]
    public void GetCompletions_CallbackInvokedEachTime()
    {
        var callCount = 0;
        var completer = new WordCompleter(["test"]);
        var dynamic = new DynamicCompleter(() =>
        {
            callCount++;
            return completer;
        });

        GetCompletions(dynamic, "");
        GetCompletions(dynamic, "");
        GetCompletions(dynamic, "");

        Assert.Equal(3, callCount);
    }

    #endregion
}
