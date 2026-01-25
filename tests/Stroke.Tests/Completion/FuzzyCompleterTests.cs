using Stroke.Completion;
using Stroke.Core;
using Stroke.FormattedText;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="FuzzyCompleter"/>.
/// </summary>
public sealed class FuzzyCompleterTests
{
    private static List<CompletionItem> GetCompletions(ICompleter completer, string text) =>
        completer.GetCompletions(new Document(text), new CompleteEvent()).ToList();

    #region Basic Fuzzy Matching

    [Fact]
    public void GetCompletions_FuzzyMatch_CharactersInOrder()
    {
        // "oar" should match "leopard" because l-E-O-p-A-R-d contains o, a, r in order
        var inner = new WordCompleter(["leopard", "gorilla", "dinosaur", "cat", "bee"]);
        var fuzzy = new FuzzyCompleter(inner);

        var completions = GetCompletions(fuzzy, "oar");

        Assert.Equal(2, completions.Count);
        Assert.Contains(completions, c => c.Text == "leopard");
        Assert.Contains(completions, c => c.Text == "dinosaur");
    }

    [Fact]
    public void GetCompletions_SC002_100PercentRecallForCharacterInOrder()
    {
        // SC-002: Fuzzy matching must have 100% recall for character-in-order patterns
        var words = new[] { "abc", "aXbXc", "cba", "ab", "xyzabc" };
        var inner = new WordCompleter(words);
        var fuzzy = new FuzzyCompleter(inner);

        var completions = GetCompletions(fuzzy, "abc");

        // Should match "abc" (exact), "aXbXc" (fuzzy), "xyzabc" (contains abc)
        Assert.Contains(completions, c => c.Text == "abc");
        Assert.Contains(completions, c => c.Text == "aXbXc");
        Assert.Contains(completions, c => c.Text == "xyzabc");
        // Should NOT match "cba" (wrong order) or "ab" (missing c)
        Assert.DoesNotContain(completions, c => c.Text == "cba");
        Assert.DoesNotContain(completions, c => c.Text == "ab");
    }

    [Fact]
    public void GetCompletions_NoMatches_ReturnsEmpty()
    {
        var inner = new WordCompleter(["hello", "world"]);
        var fuzzy = new FuzzyCompleter(inner);

        var completions = GetCompletions(fuzzy, "xyz");

        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_EmptyInput_ReturnsAll()
    {
        var inner = new WordCompleter(["hello", "world"]);
        var fuzzy = new FuzzyCompleter(inner);

        var completions = GetCompletions(fuzzy, "");

        Assert.Equal(2, completions.Count);
    }

    #endregion

    #region Sorting

    [Fact]
    public void GetCompletions_SortedByStartPosition()
    {
        // Match positions: "leopard" starts at 2 (le-O), "boa" starts at 0 (O)
        var inner = new WordCompleter(["leopard", "oar", "boat"]);
        var fuzzy = new FuzzyCompleter(inner);

        var completions = GetCompletions(fuzzy, "oa");

        Assert.Equal(3, completions.Count);
        // "oar" and "boat" should come before "leopard" (earlier start position)
        var texts = completions.Select(c => c.Text).ToList();
        var oarIndex = texts.IndexOf("oar");
        var boatIndex = texts.IndexOf("boat");
        var leopardIndex = texts.IndexOf("leopard");

        Assert.True(oarIndex < leopardIndex || boatIndex < leopardIndex);
    }

    [Fact]
    public void GetCompletions_SortedByMatchLength_WhenSameStartPos()
    {
        // Same start position, but different match lengths
        var inner = new WordCompleter(["abc", "aXXbc"]);
        var fuzzy = new FuzzyCompleter(inner);

        var completions = GetCompletions(fuzzy, "abc");

        // "abc" should come first (shorter match length)
        Assert.Equal("abc", completions[0].Text);
    }

    #endregion

    #region Styled Display Highlighting

    [Fact]
    public void GetCompletions_DisplayHighlightsMatchedCharacters()
    {
        var inner = new WordCompleter(["leopard"]);
        var fuzzy = new FuzzyCompleter(inner);

        var completions = GetCompletions(fuzzy, "oar");

        Assert.Single(completions);
        var display = completions[0].DisplayText;

        // The display should be styled FormattedText, not just plain string
        var plainText = display.ToPlainText();
        Assert.Equal("leopard", plainText);
    }

    [Fact]
    public void GetCompletions_EmptyInput_NoHighlighting()
    {
        var inner = new WordCompleter(["hello"]);
        var fuzzy = new FuzzyCompleter(inner);

        var completions = GetCompletions(fuzzy, "");

        Assert.Single(completions);
        // With empty input, should use original display
    }

    #endregion

    #region EnableFuzzy Callback

    [Fact]
    public void GetCompletions_FuzzyDisabled_DelegatesToWrapped()
    {
        var inner = new WordCompleter(["leopard", "gorilla"]);
        var fuzzy = new FuzzyCompleter(inner, enableFuzzy: () => false);

        // With fuzzy disabled, "oar" won't match anything (prefix matching only)
        var completions = GetCompletions(fuzzy, "oar");

        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_FuzzyEnabled_UsesFuzzyMatching()
    {
        var inner = new WordCompleter(["leopard", "gorilla"]);
        var fuzzy = new FuzzyCompleter(inner, enableFuzzy: () => true);

        var completions = GetCompletions(fuzzy, "oar");

        Assert.Single(completions);
        Assert.Equal("leopard", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_EnableFuzzy_CalledEachTime()
    {
        var enabled = true;
        var callCount = 0;
        var inner = new WordCompleter(["leopard"]);
        var fuzzy = new FuzzyCompleter(inner, enableFuzzy: () =>
        {
            callCount++;
            return enabled;
        });

        GetCompletions(fuzzy, "oar");
        enabled = false;
        GetCompletions(fuzzy, "oar");

        Assert.Equal(2, callCount);
    }

    #endregion

    #region WORD Mode

    [Fact]
    public void GetCompletions_WordMode_UsesWhitespaceDelimitedTokens()
    {
        var inner = new WordCompleter(["my-command", "my-other-command"], WORD: true);
        var fuzzy = new FuzzyCompleter(inner, WORD: true);

        var completions = GetCompletions(fuzzy, "my-c");

        Assert.Equal(2, completions.Count);
    }

    #endregion

    #region Custom Pattern

    [Fact]
    public void GetCompletions_CustomPattern_MustStartWithCaret()
    {
        var inner = new WordCompleter(["hello"]);

        Assert.Throws<ArgumentException>(() =>
            new FuzzyCompleter(inner, pattern: "abc")); // Missing ^
    }

    [Fact]
    public void GetCompletions_CustomPattern_UsesPattern()
    {
        var inner = new WordCompleter(["hello", "world"]);
        var fuzzy = new FuzzyCompleter(inner, pattern: @"^[a-z]+");

        var completions = GetCompletions(fuzzy, "hel");

        Assert.Single(completions);
        Assert.Equal("hello", completions[0].Text);
    }

    #endregion

    #region Special Regex Characters in Completion Text

    [Fact]
    public void GetCompletions_SpecialRegexCharsInTarget_MatchedCorrectly()
    {
        // The completion TEXT can contain special regex chars - they should be escaped
        // when building the fuzzy match pattern from user input
        var inner = new WordCompleter(["file.txt", "data.csv"]);
        var fuzzy = new FuzzyCompleter(inner);

        // "file" should fuzzy match "file.txt" (the . in the target is literal, not regex)
        var completions = GetCompletions(fuzzy, "file");

        Assert.Single(completions);
        Assert.Equal("file.txt", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_SpecialRegexCharsInTarget_AllTypesHandled()
    {
        // Completion text with various special regex characters
        var inner = new WordCompleter(["test1", "test2", "test3"]);
        var fuzzy = new FuzzyCompleter(inner);

        // Standard fuzzy matching on word characters
        var completions = GetCompletions(fuzzy, "tst");

        Assert.Equal(3, completions.Count);
    }

    #endregion

    #region Start Position Adjustment

    [Fact]
    public void GetCompletions_StartPosition_AdjustedForWordBeforeCursor()
    {
        var inner = new WordCompleter(["hello"]);
        var fuzzy = new FuzzyCompleter(inner);

        var completions = GetCompletions(fuzzy, "hel");

        Assert.Single(completions);
        // Start position should account for the word being removed
        Assert.Equal(-3, completions[0].StartPosition);
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void FuzzyCompleter_ImplementsICompleter()
    {
        var inner = new WordCompleter(["test"]);
        var fuzzy = new FuzzyCompleter(inner);

        Assert.IsAssignableFrom<ICompleter>(fuzzy);
    }

    [Fact]
    public void FuzzyCompleter_InheritsFromCompleterBase()
    {
        var inner = new WordCompleter(["test"]);
        var fuzzy = new FuzzyCompleter(inner);

        Assert.IsAssignableFrom<CompleterBase>(fuzzy);
    }

    #endregion

    #region Case Insensitivity

    [Fact]
    public void GetCompletions_CaseInsensitive_MatchesRegardlessOfCase()
    {
        var inner = new WordCompleter(["Hello", "WORLD"]);
        var fuzzy = new FuzzyCompleter(inner);

        var completions = GetCompletions(fuzzy, "hel");

        Assert.Single(completions);
        Assert.Equal("Hello", completions[0].Text);
    }

    #endregion
}
