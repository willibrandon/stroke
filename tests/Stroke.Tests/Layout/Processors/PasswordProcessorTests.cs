using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for PasswordProcessor.
/// </summary>
public class PasswordProcessorTests
{
    private static TransformationInput CreateInput(
        IReadOnlyList<StyleAndTextTuple> fragments,
        int lineNumber = 0)
    {
        var bc = new BufferControl();
        var doc = new Document("hello");
        return new TransformationInput(bc, doc, lineNumber, i => i, fragments, 80, 24);
    }

    [Fact]
    public void DefaultMask_ReplacesWithAsterisks()
    {
        var processor = new PasswordProcessor();
        var fragments = new List<StyleAndTextTuple>
        {
            new("", "hello"),
        };
        var input = CreateInput(fragments);

        var result = processor.ApplyTransformation(input);

        Assert.Single(result.Fragments);
        Assert.Equal("*****", result.Fragments[0].Text);
    }

    [Fact]
    public void CustomMask_ReplacesWithCustomChar()
    {
        var processor = new PasswordProcessor(".");
        var fragments = new List<StyleAndTextTuple>
        {
            new("", "abc"),
        };
        var input = CreateInput(fragments);

        var result = processor.ApplyTransformation(input);

        Assert.Single(result.Fragments);
        Assert.Equal("...", result.Fragments[0].Text);
    }

    [Fact]
    public void PreservesStyle()
    {
        var processor = new PasswordProcessor();
        var fragments = new List<StyleAndTextTuple>
        {
            new("bold", "hi"),
        };
        var input = CreateInput(fragments);

        var result = processor.ApplyTransformation(input);

        Assert.Equal("bold", result.Fragments[0].Style);
        Assert.Equal("**", result.Fragments[0].Text);
    }

    [Fact]
    public void PreservesMouseHandler()
    {
        Func<MouseEvent, NotImplementedOrNone> handler = _ => NotImplementedOrNone.None;
        var processor = new PasswordProcessor();
        var fragments = new List<StyleAndTextTuple>
        {
            new("", "ab", handler),
        };
        var input = CreateInput(fragments);

        var result = processor.ApplyTransformation(input);

        Assert.Same(handler, result.Fragments[0].MouseHandler);
    }

    [Fact]
    public void MultiByteUnicode_ReplacesPerCharacter()
    {
        var processor = new PasswordProcessor();
        // CJK characters - each is one C# char
        var fragments = new List<StyleAndTextTuple>
        {
            new("", "你好世"),
        };
        var input = CreateInput(fragments);

        var result = processor.ApplyTransformation(input);

        Assert.Equal("***", result.Fragments[0].Text);
    }

    [Fact]
    public void MultipleFragments_EachMasked()
    {
        var processor = new PasswordProcessor();
        var fragments = new List<StyleAndTextTuple>
        {
            new("s1", "ab"),
            new("s2", "cd"),
        };
        var input = CreateInput(fragments);

        var result = processor.ApplyTransformation(input);

        Assert.Equal(2, result.Fragments.Count);
        Assert.Equal("**", result.Fragments[0].Text);
        Assert.Equal("s1", result.Fragments[0].Style);
        Assert.Equal("**", result.Fragments[1].Text);
        Assert.Equal("s2", result.Fragments[1].Style);
    }

    [Fact]
    public void CharProperty_ReturnsConfiguredValue()
    {
        var processor = new PasswordProcessor(".");
        Assert.Equal(".", processor.Char);
    }
}
