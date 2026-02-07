using Stroke.Completion;
using Stroke.Core;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="CompletionUtils"/>.
/// </summary>
public sealed class CompletionUtilsTests
{
    private static List<CompletionItem> GetCompletions(ICompleter completer, string text) =>
        completer.GetCompletions(new Document(text), new CompleteEvent()).ToList();

    #region Merge Combines All Completers

    [Fact]
    public void Merge_CombinesAllCompleters()
    {
        var completer1 = new WordCompleter(["alpha", "beta"]);
        var completer2 = new WordCompleter(["gamma", "delta"]);

        var merged = CompletionUtils.Merge([completer1, completer2]);
        var completions = GetCompletions(merged, "");

        Assert.Equal(4, completions.Count);
        Assert.Contains(completions, c => c.Text == "alpha");
        Assert.Contains(completions, c => c.Text == "gamma");
    }

    [Fact]
    public void Merge_PreservesOrder()
    {
        var completer1 = new WordCompleter(["first"]);
        var completer2 = new WordCompleter(["second"]);
        var completer3 = new WordCompleter(["third"]);

        var merged = CompletionUtils.Merge([completer1, completer2, completer3]);
        var completions = GetCompletions(merged, "");

        Assert.Equal(3, completions.Count);
        Assert.Equal("first", completions[0].Text);
        Assert.Equal("second", completions[1].Text);
        Assert.Equal("third", completions[2].Text);
    }

    #endregion

    #region Merge with Deduplicate

    [Fact]
    public void Merge_WithDeduplicate_RemovesDuplicates()
    {
        var completer1 = new WordCompleter(["hello", "world"]);
        var completer2 = new WordCompleter(["hello", "there"]); // "hello" is duplicate

        var merged = CompletionUtils.Merge([completer1, completer2], deduplicate: true);
        var completions = GetCompletions(merged, "");

        Assert.Equal(3, completions.Count);
        Assert.Single(completions, c => c.Text == "hello");
    }

    [Fact]
    public void Merge_WithoutDeduplicate_KeepsDuplicates()
    {
        var completer1 = new WordCompleter(["hello"]);
        var completer2 = new WordCompleter(["hello"]);

        var merged = CompletionUtils.Merge([completer1, completer2], deduplicate: false);
        var completions = GetCompletions(merged, "");

        Assert.Equal(2, completions.Count);
    }

    #endregion

    #region Merge Empty Returns DummyCompleter

    [Fact]
    public void Merge_EmptyList_ReturnsDummyCompleter()
    {
        var merged = CompletionUtils.Merge([]);
        var completions = GetCompletions(merged, "test");

        Assert.Empty(completions);
    }

    [Fact]
    public void Merge_SingleCompleter_ReturnsWrapped()
    {
        var completer = new WordCompleter(["single"]);
        var merged = CompletionUtils.Merge([completer]);
        var completions = GetCompletions(merged, "si");

        Assert.Single(completions);
        Assert.Equal("single", completions[0].Text);
    }

    #endregion

    #region GetCommonSuffix Algorithm

    [Fact]
    public void GetCommonSuffix_CommonPrefix_ReturnsCommon()
    {
        var document = new Document("hel");
        var completions = new[]
        {
            new CompletionItem("lo", startPosition: 0),    // "hel" + "lo" = "hello"
            new CompletionItem("licopter", startPosition: 0) // "hel" + "licopter" = "helicopter"
        };

        var suffix = CompletionUtils.GetCommonCompleteSuffix(document, completions);

        Assert.Equal("l", suffix); // Common prefix of "lo" and "licopter"
    }

    [Fact]
    public void GetCommonSuffix_ExactMatch_ReturnsFullSuffix()
    {
        var document = new Document("hel");
        var completions = new[]
        {
            new CompletionItem("lo", startPosition: 0),
            new CompletionItem("lo", startPosition: 0)
        };

        var suffix = CompletionUtils.GetCommonCompleteSuffix(document, completions);

        Assert.Equal("lo", suffix);
    }

    #endregion

    #region GetCommonSuffix Empty Input

    [Fact]
    public void GetCommonSuffix_EmptyCompletions_ReturnsEmpty()
    {
        var document = new Document("test");
        var completions = Array.Empty<CompletionItem>();

        var suffix = CompletionUtils.GetCommonCompleteSuffix(document, completions);

        Assert.Equal("", suffix);
    }

    [Fact]
    public void GetCommonSuffix_SingleCompletion_ReturnsFullSuffix()
    {
        var document = new Document("hel");
        var completions = new[]
        {
            new CompletionItem("lo", startPosition: 0)
        };

        var suffix = CompletionUtils.GetCommonCompleteSuffix(document, completions);

        Assert.Equal("lo", suffix);
    }

    #endregion

    #region GetCommonSuffix With Negative StartPosition

    [Fact]
    public void GetCommonSuffix_NegativeStartPosition_MatchingPrefix_ReturnsSuffix()
    {
        // Document is "hel", completions replace "hel" (-3) with full words.
        // "hel".EndsWith("hel") is true, so both pass the filter.
        // GetSuffix extracts: "hello"[3..] = "lo", "hellos"[3..] = "los"
        // Common prefix of "lo" and "los" = "lo"
        var document = new Document("hel");
        var completions = new[]
        {
            new CompletionItem("hello", startPosition: -3),
            new CompletionItem("hellos", startPosition: -3)
        };

        var suffix = CompletionUtils.GetCommonCompleteSuffix(document, completions);

        Assert.Equal("lo", suffix);
    }

    [Fact]
    public void GetCommonSuffix_NegativeStartPosition_SingleCompletion_ReturnsFullSuffix()
    {
        var document = new Document("hel");
        var completions = new[]
        {
            new CompletionItem("hello", startPosition: -3) // suffix = "lo"
        };

        var suffix = CompletionUtils.GetCommonCompleteSuffix(document, completions);

        Assert.Equal("lo", suffix);
    }

    #endregion

    #region GetCommonSuffix No Common

    [Fact]
    public void GetCommonSuffix_NoCommonPrefix_ReturnsEmpty()
    {
        var document = new Document("");
        var completions = new[]
        {
            new CompletionItem("apple"),
            new CompletionItem("banana")
        };

        var suffix = CompletionUtils.GetCommonCompleteSuffix(document, completions);

        Assert.Equal("", suffix);
    }

    [Fact]
    public void GetCommonSuffix_CompletionChangesBeforeCursor_ReturnsEmpty()
    {
        var document = new Document("xyz");
        var completions = new[]
        {
            new CompletionItem("hello", startPosition: -3), // Would replace "xyz" with "hello"
            new CompletionItem("world", startPosition: 0)  // Normal completion
        };

        var suffix = CompletionUtils.GetCommonCompleteSuffix(document, completions);

        // When any completion changes text before cursor differently, return empty
        Assert.Equal("", suffix);
    }

    #endregion
}
