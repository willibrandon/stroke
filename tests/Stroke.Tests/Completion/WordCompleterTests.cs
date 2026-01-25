using Stroke.Completion;
using Stroke.Core;
using Stroke.FormattedText;
using System.Text.RegularExpressions;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="WordCompleter"/>.
/// </summary>
public sealed class WordCompleterTests
{
    private static List<CompletionItem> GetCompletions(ICompleter completer, string text) =>
        completer.GetCompletions(new Document(text), new CompleteEvent()).ToList();

    #region Basic Prefix Matching

    [Fact]
    public void GetCompletions_BasicPrefix_MatchesStartOfWord()
    {
        var completer = new WordCompleter(["hello", "help", "world"]);

        var completions = GetCompletions(completer, "hel");

        Assert.Equal(2, completions.Count);
        Assert.Contains(completions, c => c.Text == "hello");
        Assert.Contains(completions, c => c.Text == "help");
    }

    [Fact]
    public void GetCompletions_NoMatches_ReturnsEmpty()
    {
        var completer = new WordCompleter(["hello", "help", "world"]);

        var completions = GetCompletions(completer, "xyz");

        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_EmptyInput_ReturnsAllWords()
    {
        var completer = new WordCompleter(["hello", "world"]);

        var completions = GetCompletions(completer, "");

        Assert.Equal(2, completions.Count);
    }

    [Fact]
    public void GetCompletions_ExactMatch_ReturnsWord()
    {
        var completer = new WordCompleter(["hello", "help"]);

        var completions = GetCompletions(completer, "hello");

        Assert.Single(completions);
        Assert.Equal("hello", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_StartPosition_IsNegativeWordLength()
    {
        var completer = new WordCompleter(["hello"]);

        var completions = GetCompletions(completer, "hel");

        Assert.Single(completions);
        Assert.Equal(-3, completions[0].StartPosition);
    }

    #endregion

    #region Case Insensitive Matching

    [Fact]
    public void GetCompletions_IgnoreCase_MatchesRegardlessOfCase()
    {
        var completer = new WordCompleter(["Hello", "HELP", "World"], ignoreCase: true);

        var completions = GetCompletions(completer, "hel");

        Assert.Equal(2, completions.Count);
        Assert.Contains(completions, c => c.Text == "Hello");
        Assert.Contains(completions, c => c.Text == "HELP");
    }

    [Fact]
    public void GetCompletions_CaseSensitive_MatchesExactCase()
    {
        var completer = new WordCompleter(["Hello", "hello", "HELLO"]);

        var completions = GetCompletions(completer, "Hel");

        Assert.Single(completions);
        Assert.Equal("Hello", completions[0].Text);
    }

    #endregion

    #region MatchMiddle

    [Fact]
    public void GetCompletions_MatchMiddle_MatchesAnywhereInWord()
    {
        var completer = new WordCompleter(["hello", "shell", "help"], matchMiddle: true);

        var completions = GetCompletions(completer, "ell");

        Assert.Equal(2, completions.Count);
        Assert.Contains(completions, c => c.Text == "hello");
        Assert.Contains(completions, c => c.Text == "shell");
    }

    [Fact]
    public void GetCompletions_MatchMiddlePlusIgnoreCase_CombinesBehaviors()
    {
        var completer = new WordCompleter(
            ["HelloWorld", "SHELL", "helper"],
            matchMiddle: true,
            ignoreCase: true);

        var completions = GetCompletions(completer, "LL");

        Assert.Equal(2, completions.Count);
        Assert.Contains(completions, c => c.Text == "HelloWorld");
        Assert.Contains(completions, c => c.Text == "SHELL");
    }

    #endregion

    #region WORD Mode

    [Fact]
    public void GetCompletions_WordMode_UsesWhitespaceDelimitedToken()
    {
        var completer = new WordCompleter(["test-word", "another-word"], WORD: true);

        // "test-wo" - with WORD mode, extracts "test-wo" (whitespace-delimited)
        var completions = GetCompletions(completer, "test-wo");

        Assert.Single(completions);
        Assert.Equal("test-word", completions[0].Text);
        Assert.Equal(-7, completions[0].StartPosition); // -len("test-wo")
    }

    [Fact]
    public void GetCompletions_WordMode_AfterSpace()
    {
        var completer = new WordCompleter(["command", "commit"], WORD: true);

        var completions = GetCompletions(completer, "git com");

        Assert.Equal(2, completions.Count);
        Assert.Equal(-3, completions[0].StartPosition); // -len("com")
    }

    #endregion

    #region Sentence Mode

    [Fact]
    public void GetCompletions_Sentence_MatchesEntireTextBeforeCursor()
    {
        var completer = new WordCompleter(
            ["hello world", "hello there", "goodbye world"],
            sentence: true);

        var completions = GetCompletions(completer, "hello ");

        Assert.Equal(2, completions.Count);
        Assert.Contains(completions, c => c.Text == "hello world");
        Assert.Contains(completions, c => c.Text == "hello there");
    }

    [Fact]
    public void GetCompletions_Sentence_StartPositionIsFullLength()
    {
        var completer = new WordCompleter(["hello world"], sentence: true);

        var completions = GetCompletions(completer, "hello ");

        Assert.Single(completions);
        Assert.Equal(-6, completions[0].StartPosition); // -len("hello ")
    }

    [Fact]
    public void WordCompleter_WordAndSentence_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new WordCompleter(["test"], WORD: true, sentence: true));
    }

    #endregion

    #region Pattern Override

    [Fact]
    public void GetCompletions_Pattern_OverridesDefaultWordExtraction()
    {
        // Custom pattern: words (reversed text is searched, so pattern matches beginning)
        // Using a pattern that matches word characters
        var pattern = new Regex(@"(\w+)");
        var completer = new WordCompleter(["user", "admin", "guest"], pattern: pattern);

        // "hello us" - the pattern finds "su" in reversed text, returns "us"
        var completions = GetCompletions(completer, "hello us");

        Assert.Single(completions);
        Assert.Equal("user", completions[0].Text);
        Assert.Equal(-2, completions[0].StartPosition); // -len("us")
    }

    [Fact]
    public void GetCompletions_Pattern_NoMatchReturnsAll()
    {
        var pattern = new Regex(@"#\w*$");
        var completer = new WordCompleter(["#tag1", "#tag2"], pattern: pattern);

        // No # in input, pattern doesn't match → word_before_cursor is empty → all match
        var completions = GetCompletions(completer, "test");

        Assert.Equal(2, completions.Count);
    }

    #endregion

    #region Display and Meta Dictionaries

    [Fact]
    public void GetCompletions_DisplayDict_SetsDisplayText()
    {
        var displayDict = new Dictionary<string, AnyFormattedText>
        {
            ["git"] = "Git VCS",
            ["svn"] = "Subversion"
        };
        var completer = new WordCompleter(["git", "svn"], displayDict: displayDict);

        var completions = GetCompletions(completer, "g");

        Assert.Single(completions);
        Assert.Equal("git", completions[0].Text);
        Assert.Equal("Git VCS", completions[0].DisplayText.ToPlainText());
    }

    [Fact]
    public void GetCompletions_DisplayDict_MissingEntry_UsesText()
    {
        var displayDict = new Dictionary<string, AnyFormattedText> { ["git"] = "Git VCS" };
        var completer = new WordCompleter(["git", "svn"], displayDict: displayDict);

        var completions = GetCompletions(completer, "s");

        Assert.Single(completions);
        Assert.Equal("svn", completions[0].Text);
        Assert.Equal("svn", completions[0].DisplayText.ToPlainText()); // Falls back to text
    }

    [Fact]
    public void GetCompletions_MetaDict_SetsDisplayMeta()
    {
        var metaDict = new Dictionary<string, AnyFormattedText>
        {
            ["git"] = "version control",
            ["svn"] = "legacy vcs"
        };
        var completer = new WordCompleter(["git", "svn"], metaDict: metaDict);

        var completions = GetCompletions(completer, "g");

        Assert.Single(completions);
        Assert.Equal("version control", completions[0].DisplayMetaText.ToPlainText());
    }

    [Fact]
    public void GetCompletions_MetaDict_MissingEntry_ReturnsEmpty()
    {
        var metaDict = new Dictionary<string, AnyFormattedText> { ["git"] = "vcs" };
        var completer = new WordCompleter(["git", "svn"], metaDict: metaDict);

        var completions = GetCompletions(completer, "s");

        Assert.Single(completions);
        Assert.True(completions[0].DisplayMetaText.IsEmpty);
    }

    #endregion

    #region Dynamic Word List

    [Fact]
    public void GetCompletions_DynamicWords_InvokesFunc()
    {
        var callCount = 0;
        Func<IEnumerable<string>> wordsFunc = () =>
        {
            callCount++;
            return ["dynamic1", "dynamic2"];
        };

        var completer = new WordCompleter(wordsFunc);

        var completions1 = GetCompletions(completer, "dyn");
        var completions2 = GetCompletions(completer, "dyn");

        Assert.Equal(2, callCount); // Called twice
        Assert.Equal(2, completions1.Count);
        Assert.Equal(2, completions2.Count);
    }

    [Fact]
    public void GetCompletions_DynamicWords_ReflectsChanges()
    {
        var words = new List<string> { "initial" };
        var completer = new WordCompleter(() => words);

        var completions1 = GetCompletions(completer, "");
        Assert.Single(completions1);

        words.Add("added");
        var completions2 = GetCompletions(completer, "");
        Assert.Equal(2, completions2.Count);
    }

    #endregion

    #region Async Behavior

    [Fact]
    public async Task GetCompletionsAsync_YieldsResults()
    {
        var completer = new WordCompleter(["hello", "help"]);

        var completions = new List<CompletionItem>();
        await foreach (var c in completer.GetCompletionsAsync(
            new Document("hel"), new CompleteEvent()))
        {
            completions.Add(c);
        }

        Assert.Equal(2, completions.Count);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetCompletions_EmptyWordList_ReturnsEmpty()
    {
        var completer = new WordCompleter([]);

        var completions = GetCompletions(completer, "test");

        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_WordAfterSpace_MatchesLastWord()
    {
        var completer = new WordCompleter(["command", "commit"]);

        // Without WORD mode, word is "com" (alphanumeric word boundary)
        var completions = GetCompletions(completer, "git com");

        Assert.Equal(2, completions.Count);
        Assert.Equal(-3, completions[0].StartPosition);
    }

    [Fact]
    public void GetCompletions_CursorInMiddleOfWord()
    {
        var completer = new WordCompleter(["hello", "help"]);

        // Cursor at position 3: "hel|lo" - word before cursor is "hel"
        var doc = new Document("hello", cursorPosition: 3);
        var completions = completer.GetCompletions(doc, new CompleteEvent()).ToList();

        Assert.Equal(2, completions.Count);
        Assert.Equal(-3, completions[0].StartPosition);
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void WordCompleter_ImplementsICompleter()
    {
        var completer = new WordCompleter(["test"]);

        Assert.IsAssignableFrom<ICompleter>(completer);
    }

    [Fact]
    public void WordCompleter_InheritsFromCompleterBase()
    {
        var completer = new WordCompleter(["test"]);

        Assert.IsAssignableFrom<CompleterBase>(completer);
    }

    #endregion
}
