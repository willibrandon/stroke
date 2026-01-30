using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for DisplayMultipleCursors processor.
/// </summary>
public class DisplayMultipleCursorsTests
{
    private static TransformationInput CreateInput(
        string text = "hello",
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
    public void ViInsertMultipleNotActive_PassesThrough()
    {
        // When ViInsertMultipleMode filter returns false (which it does
        // with DummyApplication since EditingMode != Vi), pass through
        var processor = new DisplayMultipleCursors();
        var input = CreateInput("hello");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.Equal("hello", fullText);
    }

    [Fact]
    public void IsSealed()
    {
        Assert.True(typeof(DisplayMultipleCursors).IsSealed);
    }

    [Fact]
    public void ImplementsIProcessor()
    {
        var processor = new DisplayMultipleCursors();
        Assert.IsAssignableFrom<IProcessor>(processor);
    }
}
