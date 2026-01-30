using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for AfterInput processor.
/// </summary>
public class AfterInputTests
{
    private static TransformationInput CreateInput(
        int lineNumber,
        string text,
        IReadOnlyList<StyleAndTextTuple>? fragments = null)
    {
        var bc = new BufferControl();
        var doc = new Document(text);
        var lines = text.Split('\n');
        var lineText = lineNumber < lines.Length ? lines[lineNumber] : "";
        var frags = fragments ?? new List<StyleAndTextTuple> { new("", lineText) };
        return new TransformationInput(bc, doc, lineNumber, i => i, frags, 80, 24);
    }

    [Fact]
    public void LastLine_AppendsText()
    {
        var processor = new AfterInput(" [END]");
        var input = CreateInput(0, "hello");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.EndsWith(" [END]", fullText);
    }

    [Fact]
    public void NonLastLine_PassesThrough()
    {
        var processor = new AfterInput(" [END]");
        var input = CreateInput(0, "line0\nline1");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.DoesNotContain("[END]", fullText);
    }

    [Fact]
    public void LastLineOfMultiline_AppendsText()
    {
        var processor = new AfterInput("!");
        // Document has 2 lines, line 1 is the last
        var input = CreateInput(1, "line0\nline1");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.EndsWith("!", fullText);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var processor = new AfterInput("text", "style");
        var str = processor.ToString();
        Assert.StartsWith("AfterInput(", str);
        Assert.Contains("style=style", str);
    }

    [Fact]
    public void Properties_ReturnConstructorValues()
    {
        var processor = new AfterInput("suffix", "class:suffix");
        Assert.Equal("class:suffix", processor.Style);
    }
}
