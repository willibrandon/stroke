using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for HighlightSelectionProcessor.
/// </summary>
public class HighlightSelectionProcessorTests
{
    private static TransformationInput CreateInput(
        string text = "hello world",
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
    public void NoSelection_ReturnsFragmentsUnchanged()
    {
        var processor = new HighlightSelectionProcessor();
        var fragments = new List<StyleAndTextTuple> { new("", "hello") };
        var input = CreateInput(fragments: fragments);

        var result = processor.ApplyTransformation(input);

        // No selection active on Document, so no change
        Assert.Same(fragments, result.Fragments);
    }

    [Fact]
    public void IsSealed()
    {
        Assert.True(typeof(HighlightSelectionProcessor).IsSealed);
    }

    [Fact]
    public void ImplementsIProcessor()
    {
        var processor = new HighlightSelectionProcessor();
        Assert.IsAssignableFrom<IProcessor>(processor);
    }
}
