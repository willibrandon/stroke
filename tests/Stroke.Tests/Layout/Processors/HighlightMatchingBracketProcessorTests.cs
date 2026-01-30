using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for HighlightMatchingBracketProcessor.
/// </summary>
public class HighlightMatchingBracketProcessorTests
{
    private static TransformationInput CreateInput(
        string text = "(hello)",
        int lineNumber = 0,
        int cursorPosition = 0,
        IReadOnlyList<StyleAndTextTuple>? fragments = null)
    {
        var bc = new BufferControl();
        var doc = new Document(text, cursorPosition);
        var lineText = text.Split('\n')[Math.Min(lineNumber, text.Split('\n').Length - 1)];
        var frags = fragments ?? new List<StyleAndTextTuple> { new("", lineText) };
        return new TransformationInput(bc, doc, lineNumber, i => i, frags, 80, 24);
    }

    [Fact]
    public void CursorOnBracket_HighlightsMatchingPair()
    {
        // DummyApplication.IsDone is false (no future set), so highlighting proceeds
        var processor = new HighlightMatchingBracketProcessor();
        var input = CreateInput("(hello)", cursorPosition: 0);

        var result = processor.ApplyTransformation(input);

        // Fragments get exploded into 7 single-char fragments
        Assert.Equal(7, result.Fragments.Count);
        // First char '(' gets cursor highlight
        Assert.Contains("class:matching-bracket.cursor", result.Fragments[0].Style);
    }

    [Fact]
    public void DefaultChars_ContainsAllBracketTypes()
    {
        var processor = new HighlightMatchingBracketProcessor();
        Assert.Equal("[](){}<>", processor.Chars);
    }

    [Fact]
    public void DefaultMaxCursorDistance()
    {
        var processor = new HighlightMatchingBracketProcessor();
        Assert.Equal(1000, processor.MaxCursorDistance);
    }

    [Fact]
    public void CustomChars()
    {
        var processor = new HighlightMatchingBracketProcessor(chars: "()");
        Assert.Equal("()", processor.Chars);
    }

    [Fact]
    public void CustomMaxCursorDistance()
    {
        var processor = new HighlightMatchingBracketProcessor(maxCursorDistance: 500);
        Assert.Equal(500, processor.MaxCursorDistance);
    }

    [Fact]
    public void IsSealed()
    {
        Assert.True(typeof(HighlightMatchingBracketProcessor).IsSealed);
    }

    [Fact]
    public async Task ConcurrentCacheAccess_ThreadSafe()
    {
        // SimpleCache is internally thread-safe, so this should not throw
        var processor = new HighlightMatchingBracketProcessor();

        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var input = CreateInput("(abc)", cursorPosition: 0);
                // Should not throw due to concurrent cache access
                processor.ApplyTransformation(input);
            }, TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(tasks);
    }
}
