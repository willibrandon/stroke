using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for BeforeInput and ShowArg processors.
/// </summary>
public class BeforeInputTests
{
    private static TransformationInput CreateInput(
        int lineNumber = 0,
        string text = "hello",
        IReadOnlyList<StyleAndTextTuple>? fragments = null)
    {
        var bc = new BufferControl();
        var doc = new Document(text);
        var frags = fragments ?? new List<StyleAndTextTuple> { new("", text.Split('\n')[Math.Min(lineNumber, text.Split('\n').Length - 1)]) };
        return new TransformationInput(bc, doc, lineNumber, i => i, frags, 80, 24);
    }

    [Fact]
    public void Line0_PrependsText()
    {
        var processor = new BeforeInput(">>> ");
        var input = CreateInput(0);

        var result = processor.ApplyTransformation(input);

        // Should have prefix + original
        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.StartsWith(">>> ", fullText);
    }

    [Fact]
    public void NonLine0_PassesThrough()
    {
        var processor = new BeforeInput(">>> ");
        var input = CreateInput(1, "line0\nline1");

        var result = processor.ApplyTransformation(input);

        // Should not have prefix
        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.DoesNotContain(">>>", fullText);
    }

    [Fact]
    public void Line0_PositionMappingShiftsByPrefixLength()
    {
        var processor = new BeforeInput(">> ");
        var input = CreateInput(0);

        var result = processor.ApplyTransformation(input);

        // Prefix ">> " has length 3
        Assert.Equal(3, result.SourceToDisplay(0));
        Assert.Equal(8, result.SourceToDisplay(5));
        Assert.Equal(-3, result.DisplayToSource(0));
        Assert.Equal(2, result.DisplayToSource(5));
    }

    [Fact]
    public void NonLine0_IdentityMapping()
    {
        var processor = new BeforeInput(">> ");
        var input = CreateInput(1, "a\nb");

        var result = processor.ApplyTransformation(input);

        Assert.Equal(0, result.SourceToDisplay(0));
        Assert.Equal(5, result.SourceToDisplay(5));
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var processor = new BeforeInput("test", "bold");
        var str = processor.ToString();
        Assert.StartsWith("BeforeInput(", str);
        Assert.Contains("style=bold", str);
    }

    [Fact]
    public void TextAndStyleProperties()
    {
        var processor = new BeforeInput("prompt", "class:prompt");
        Assert.Equal("class:prompt", processor.Style);
    }
}

public class ShowArgTests
{
    [Fact]
    public void ToString_ReturnsShowArg()
    {
        var processor = new ShowArg();
        Assert.Equal("ShowArg()", processor.ToString());
    }

    [Fact]
    public void ExtendsBeforeInput()
    {
        var processor = new ShowArg();
        Assert.IsAssignableFrom<BeforeInput>(processor);
    }
}
