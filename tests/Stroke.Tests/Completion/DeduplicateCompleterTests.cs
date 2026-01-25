using Stroke.Completion;
using Stroke.Core;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="DeduplicateCompleter"/>.
/// </summary>
public sealed class DeduplicateCompleterTests
{
    private static List<CompletionItem> GetCompletions(ICompleter completer, string text) =>
        completer.GetCompletions(new Document(text), new CompleteEvent()).ToList();

    #region Duplicate Removal by Document Text

    [Fact]
    public void GetCompletions_DuplicatesByDocumentText_Removed()
    {
        // Two completions that would result in the same document text
        var inner = new ManualCompleter([
            new CompletionItem("hello"),
            new CompletionItem("hello") // Duplicate
        ]);
        var dedup = new DeduplicateCompleter(inner);

        var completions = GetCompletions(dedup, "");

        Assert.Single(completions);
        Assert.Equal("hello", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_DifferentTextSameResult_Removed()
    {
        // Different completion text and start positions but same resulting document.
        // Document is "hel" with cursor at end (position 3).
        // Completion 1: startPosition=-3 replaces "hel" with "hello" -> result: "hello"
        // Completion 2: startPosition=-2 replaces "el" with "ello" -> result: "hello" (same result!)
        var inner = new ManualCompleter([
            new CompletionItem("hello", startPosition: -3), // "hel"[..0] + "hello" = "hello"
            new CompletionItem("ello", startPosition: -2)   // "hel"[..1] + "ello" = "h" + "ello" = "hello"
        ]);
        var dedup = new DeduplicateCompleter(inner);

        var document = new Document("hel");
        var completions = dedup.GetCompletions(document, new CompleteEvent()).ToList();

        Assert.Single(completions);
    }

    #endregion

    #region First Occurrence Kept

    [Fact]
    public void GetCompletions_FirstOccurrenceKept()
    {
        var inner = new ManualCompleter([
            new CompletionItem("first", display: "First Display"),
            new CompletionItem("first", display: "Second Display") // Same text, different display
        ]);
        var dedup = new DeduplicateCompleter(inner);

        var completions = GetCompletions(dedup, "");

        Assert.Single(completions);
        // The first occurrence should be kept
        Assert.Contains("First", completions[0].DisplayText);
    }

    #endregion

    #region Order Preserved

    [Fact]
    public void GetCompletions_OrderPreserved()
    {
        var inner = new ManualCompleter([
            new CompletionItem("alpha"),
            new CompletionItem("beta"),
            new CompletionItem("alpha"), // Duplicate, should be removed
            new CompletionItem("gamma"),
            new CompletionItem("beta") // Duplicate, should be removed
        ]);
        var dedup = new DeduplicateCompleter(inner);

        var completions = GetCompletions(dedup, "");

        Assert.Equal(3, completions.Count);
        Assert.Equal("alpha", completions[0].Text);
        Assert.Equal("beta", completions[1].Text);
        Assert.Equal("gamma", completions[2].Text);
    }

    #endregion

    #region No-Change Completions Skipped

    [Fact]
    public void GetCompletions_NoChangeCompletions_Skipped()
    {
        // A completion that doesn't change the document
        var inner = new ManualCompleter([
            new CompletionItem("hello", startPosition: -5), // Would just re-insert "hello"
            new CompletionItem("world")
        ]);
        var dedup = new DeduplicateCompleter(inner);

        var document = new Document("hello");
        var completions = dedup.GetCompletions(document, new CompleteEvent()).ToList();

        Assert.Single(completions);
        Assert.Equal("world", completions[0].Text);
    }

    #endregion

    #region Unique Completions Pass Through

    [Fact]
    public void GetCompletions_UniqueCompletions_PassThrough()
    {
        var inner = new ManualCompleter([
            new CompletionItem("one"),
            new CompletionItem("two"),
            new CompletionItem("three")
        ]);
        var dedup = new DeduplicateCompleter(inner);

        var completions = GetCompletions(dedup, "");

        Assert.Equal(3, completions.Count);
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void DeduplicateCompleter_ImplementsICompleter()
    {
        var inner = new WordCompleter(["test"]);
        var dedup = new DeduplicateCompleter(inner);

        Assert.IsAssignableFrom<ICompleter>(dedup);
    }

    #endregion

    #region Helper Classes

    private sealed class ManualCompleter : CompleterBase
    {
        private readonly List<CompletionItem> _completions;

        public ManualCompleter(IEnumerable<CompletionItem> completions)
        {
            _completions = completions.ToList();
        }

        public override IEnumerable<CompletionItem> GetCompletions(Document document, CompleteEvent completeEvent)
        {
            return _completions;
        }
    }

    #endregion
}
