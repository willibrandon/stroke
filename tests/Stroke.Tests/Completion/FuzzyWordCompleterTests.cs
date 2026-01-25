using Stroke.Completion;
using Stroke.Core;
using Stroke.FormattedText;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="FuzzyWordCompleter"/>.
/// </summary>
public sealed class FuzzyWordCompleterTests
{
    private static List<CompletionItem> GetCompletions(ICompleter completer, string text) =>
        completer.GetCompletions(new Document(text), new CompleteEvent()).ToList();

    #region Convenience Wrapper Behavior

    [Fact]
    public void GetCompletions_WrapsWordCompleterInFuzzyCompleter()
    {
        var completer = new FuzzyWordCompleter(["leopard", "gorilla", "dinosaur"]);

        // Fuzzy match should work
        var completions = GetCompletions(completer, "oar");

        Assert.Equal(2, completions.Count);
        Assert.Contains(completions, c => c.Text == "leopard");
        Assert.Contains(completions, c => c.Text == "dinosaur");
    }

    [Fact]
    public void GetCompletions_EmptyInput_ReturnsAll()
    {
        var completer = new FuzzyWordCompleter(["hello", "world"]);

        var completions = GetCompletions(completer, "");

        Assert.Equal(2, completions.Count);
    }

    #endregion

    #region MetaDict Pass-Through

    [Fact]
    public void GetCompletions_MetaDict_PassedToWordCompleter()
    {
        var metaDict = new Dictionary<string, AnyFormattedText>
        {
            ["hello"] = "greeting",
            ["world"] = "planet"
        };
        var completer = new FuzzyWordCompleter(["hello", "world"], metaDict: metaDict);

        var completions = GetCompletions(completer, "hel");

        Assert.Single(completions);
        // Note: Fuzzy matching may modify display, but meta should pass through
    }

    #endregion

    #region WORD Mode Pass-Through

    [Fact]
    public void GetCompletions_WordMode_PassedThrough()
    {
        var completer = new FuzzyWordCompleter(
            ["my-command", "my-other-command"],
            WORD: true);

        var completions = GetCompletions(completer, "my-c");

        Assert.Equal(2, completions.Count);
    }

    #endregion

    #region Dynamic Words

    [Fact]
    public void GetCompletions_DynamicWords_Works()
    {
        var words = new List<string> { "initial" };
        var completer = new FuzzyWordCompleter(() => words);

        var completions1 = GetCompletions(completer, "");
        Assert.Single(completions1);

        words.Add("added");
        var completions2 = GetCompletions(completer, "");
        Assert.Equal(2, completions2.Count);
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void FuzzyWordCompleter_ImplementsICompleter()
    {
        var completer = new FuzzyWordCompleter(["test"]);

        Assert.IsAssignableFrom<ICompleter>(completer);
    }

    #endregion
}
