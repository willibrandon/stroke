using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for ShowLeadingWhiteSpaceProcessor and ShowTrailingWhiteSpaceProcessor.
/// </summary>
public class ShowLeadingWhiteSpaceProcessorTests
{
    private static TransformationInput CreateInput(
        string lineText,
        IReadOnlyList<StyleAndTextTuple>? fragments = null)
    {
        var bc = new BufferControl();
        var doc = new Document(lineText);
        var frags = fragments ?? new List<StyleAndTextTuple> { new("", lineText) };
        return new TransformationInput(bc, doc, 0, i => i, frags, 80, 24);
    }

    [Fact]
    public void LeadingSpaces_Replaced()
    {
        var processor = new ShowLeadingWhiteSpaceProcessor(getChar: () => ".");
        var input = CreateInput("  hello");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.StartsWith("..", fullText);
        Assert.EndsWith("hello", fullText);
    }

    [Fact]
    public void NoLeadingSpaces_Unchanged()
    {
        var processor = new ShowLeadingWhiteSpaceProcessor(getChar: () => ".");
        var input = CreateInput("hello world");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.Equal("hello world", fullText);
    }

    [Fact]
    public void AllWhitespace_AllReplaced()
    {
        var processor = new ShowLeadingWhiteSpaceProcessor(getChar: () => "·");
        var input = CreateInput("   ");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.Equal("···", fullText);
    }

    [Fact]
    public void CustomChar()
    {
        var processor = new ShowLeadingWhiteSpaceProcessor(getChar: () => "→");
        var input = CreateInput(" x");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.Equal("→x", fullText);
    }

    [Fact]
    public void StyleApplied()
    {
        var processor = new ShowLeadingWhiteSpaceProcessor(
            getChar: () => ".", style: "class:my-ws");
        var input = CreateInput(" x");

        var result = processor.ApplyTransformation(input);

        // First fragment should have the custom style
        Assert.Equal("class:my-ws", result.Fragments[0].Style);
    }

    [Fact]
    public void DefaultProperties()
    {
        var processor = new ShowLeadingWhiteSpaceProcessor();
        Assert.Equal("class:leading-whitespace", processor.Style);
        Assert.NotNull(processor.GetChar);
    }

    [Fact]
    public void IsSealed()
    {
        Assert.True(typeof(ShowLeadingWhiteSpaceProcessor).IsSealed);
    }
}

public class ShowTrailingWhiteSpaceProcessorTests
{
    private static TransformationInput CreateInput(
        string lineText,
        IReadOnlyList<StyleAndTextTuple>? fragments = null)
    {
        var bc = new BufferControl();
        var doc = new Document(lineText);
        var frags = fragments ?? new List<StyleAndTextTuple> { new("", lineText) };
        return new TransformationInput(bc, doc, 0, i => i, frags, 80, 24);
    }

    [Fact]
    public void TrailingSpaces_Replaced()
    {
        var processor = new ShowTrailingWhiteSpaceProcessor(getChar: () => ".");
        var input = CreateInput("hello  ");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.StartsWith("hello", fullText);
        Assert.EndsWith("..", fullText);
    }

    [Fact]
    public void NoTrailingSpaces_Unchanged()
    {
        var processor = new ShowTrailingWhiteSpaceProcessor(getChar: () => ".");
        var input = CreateInput("hello world");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.Equal("hello world", fullText);
    }

    [Fact]
    public void AllWhitespace_AllReplaced()
    {
        // All whitespace is both leading AND trailing; trailing processor replaces from end
        var processor = new ShowTrailingWhiteSpaceProcessor(getChar: () => "·");
        var input = CreateInput("   ");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.Equal("···", fullText);
    }

    [Fact]
    public void CustomChar()
    {
        var processor = new ShowTrailingWhiteSpaceProcessor(getChar: () => "←");
        var input = CreateInput("x ");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.Equal("x←", fullText);
    }

    [Fact]
    public void StyleApplied()
    {
        var processor = new ShowTrailingWhiteSpaceProcessor(
            getChar: () => ".", style: "class:my-trail");
        var input = CreateInput("x ");

        var result = processor.ApplyTransformation(input);

        // Last fragment should have the custom style
        Assert.Equal("class:my-trail", result.Fragments[^1].Style);
    }

    [Fact]
    public void DefaultProperties()
    {
        var processor = new ShowTrailingWhiteSpaceProcessor();
        Assert.Equal("class:trailing-whitespace", processor.Style);
        Assert.NotNull(processor.GetChar);
    }

    [Fact]
    public void CorrectedFromPythonTypo()
    {
        // Python uses "class:training-whitespace" (typo)
        // C# uses "class:trailing-whitespace" (corrected)
        var processor = new ShowTrailingWhiteSpaceProcessor();
        Assert.DoesNotContain("training", processor.Style);
        Assert.Contains("trailing", processor.Style);
    }

    [Fact]
    public void IsSealed()
    {
        Assert.True(typeof(ShowTrailingWhiteSpaceProcessor).IsSealed);
    }
}
