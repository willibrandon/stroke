using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for TabsProcessor.
/// </summary>
public class TabsProcessorTests
{
    private static TransformationInput CreateInput(
        string text,
        int lineNumber = 0,
        IReadOnlyList<StyleAndTextTuple>? fragments = null)
    {
        var bc = new BufferControl();
        var doc = new Document(text);
        var lineText = text.Split('\n')[Math.Min(lineNumber, text.Split('\n').Length - 1)];
        var frags = fragments ?? new List<StyleAndTextTuple> { new("", lineText) };
        return new TransformationInput(bc, doc, lineNumber, i => i, frags, 80, 24);
    }

    [Fact]
    public void TabAtColumn0_ExpandsToFullWidth()
    {
        var processor = new TabsProcessor();
        var input = CreateInput("\thello",
            fragments: new List<StyleAndTextTuple> { new("", "\thello") });

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        // Tab at col 0: tabstop=4, so 4 chars: "|" + "┈┈┈"
        Assert.StartsWith("|", fullText);
        Assert.Equal(9, fullText.Length); // 4 tab chars + 5 "hello" chars
    }

    [Fact]
    public void TabAtColumn2_ExpandsToRemainder()
    {
        var processor = new TabsProcessor();
        // "ab\t" — tab at column 2, tabstop=4, so 4-2=2 chars
        var input = CreateInput("ab\t",
            fragments: new List<StyleAndTextTuple> { new("", "ab\t") });

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        // "a" + "b" + "|" + "┈" = 4 chars total
        Assert.Equal(4, fullText.Length);
    }

    [Fact]
    public void TabAtExactBoundary_ExpandsToFullTabWidth()
    {
        var processor = new TabsProcessor();
        // "abcd\t" — tab at column 4 (exact boundary), expands to 4
        var input = CreateInput("abcd\t",
            fragments: new List<StyleAndTextTuple> { new("", "abcd\t") });

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        // 4 chars "abcd" + 4 tab chars = 8
        Assert.Equal(8, fullText.Length);
    }

    [Fact]
    public void TabWidth1_ExpandsToSingleChar()
    {
        var processor = new TabsProcessor(tabstop: 1);
        var input = CreateInput("\thello",
            fragments: new List<StyleAndTextTuple> { new("", "\thello") });

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        // Tab at col 0 with tabstop=1: 1 - (0 % 1) = 1; but 0%1=0 so count=1-0=1
        // Just 1 separator char + "hello" = 6 chars
        Assert.Equal(6, fullText.Length);
    }

    [Fact]
    public void PositionMapping_Bidirectional()
    {
        var processor = new TabsProcessor();
        // "\thello"
        var input = CreateInput("\thello",
            fragments: new List<StyleAndTextTuple> { new("", "\thello") });

        var result = processor.ApplyTransformation(input);

        // Source position 0 (the tab char) → display position 0
        Assert.Equal(0, result.SourceToDisplay(0));
        // Source position 1 (first char of "hello") → display position 4 (after 4 tab expansion chars)
        Assert.Equal(4, result.SourceToDisplay(1));
        // Source position 6 → display position 9 (past end)
        Assert.Equal(9, result.SourceToDisplay(6));

        // Display position 0 → source position 0
        Assert.Equal(0, result.DisplayToSource(0));
        // Display position 4 → source position 1
        Assert.Equal(1, result.DisplayToSource(4));
    }

    [Fact]
    public void CustomCharsAndWidth()
    {
        var processor = new TabsProcessor(tabstop: 2, char1: ">", char2: "-");
        var input = CreateInput("\tx",
            fragments: new List<StyleAndTextTuple> { new("", "\tx") });

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        // Tab at col 0, tabstop=2: 2 chars: ">" + "-"
        Assert.Equal(">-x", fullText);
    }

    [Fact]
    public void CallableParameters()
    {
        var processor = new TabsProcessor(
            tabstop: (Func<int>)(() => 3),
            char1: (Func<string>)(() => "#"),
            char2: (Func<string>)(() => "="));
        var input = CreateInput("\tx",
            fragments: new List<StyleAndTextTuple> { new("", "\tx") });

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        // Tab at col 0, tabstop=3: 3 chars: "#" + "==" + "x"
        Assert.Equal("#==x", fullText);
    }

    [Fact]
    public void DefaultProperties()
    {
        var processor = new TabsProcessor();
        Assert.Equal(4, processor.TabStop);
        Assert.Equal("|", processor.Char1);
        Assert.Equal("\u2508", processor.Char2);
        Assert.Equal("class:tab", processor.Style);
    }

    [Fact]
    public void TabStyle_Applied()
    {
        var processor = new TabsProcessor(style: "class:my-tab");
        var input = CreateInput("\tx",
            fragments: new List<StyleAndTextTuple> { new("", "\tx") });

        var result = processor.ApplyTransformation(input);

        // First fragment should have the tab style
        Assert.Equal("class:my-tab", result.Fragments[0].Style);
    }

    [Fact]
    public void NoTabs_PassesThrough()
    {
        var processor = new TabsProcessor();
        var input = CreateInput("hello",
            fragments: new List<StyleAndTextTuple> { new("", "hello") });

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.Equal("hello", fullText);
    }

    [Fact]
    public void IsSealed()
    {
        Assert.True(typeof(TabsProcessor).IsSealed);
    }
}
