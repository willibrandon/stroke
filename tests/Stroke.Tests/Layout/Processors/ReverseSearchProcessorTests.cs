using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for ReverseSearchProcessor.
/// </summary>
public class ReverseSearchProcessorTests
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
    public void NoMainBuffer_PassesThrough()
    {
        // When GetMainBuffer returns null (no active search target),
        // the processor passes through unchanged
        var processor = new ReverseSearchProcessor();
        var fragments = new List<StyleAndTextTuple> { new("", "search text") };
        var input = CreateInput("search text", fragments: fragments);

        var result = processor.ApplyTransformation(input);

        // No main buffer found, so passthrough
        Assert.Same(fragments, result.Fragments);
    }

    [Fact]
    public void NonLine0_PassesThrough()
    {
        // Even with a main buffer, non-line-0 passes through
        var processor = new ReverseSearchProcessor();
        var input = CreateInput("line0\nline1", lineNumber: 1);

        var result = processor.ApplyTransformation(input);

        // lineNumber != 0, so passthrough regardless of mainControl
        Assert.Equal(input.Fragments.Count, result.Fragments.Count);
    }

    [Fact]
    public void IsSealed()
    {
        Assert.True(typeof(ReverseSearchProcessor).IsSealed);
    }

    [Fact]
    public void ImplementsIProcessor()
    {
        var processor = new ReverseSearchProcessor();
        Assert.IsAssignableFrom<IProcessor>(processor);
    }
}
