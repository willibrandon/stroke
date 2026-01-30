using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Pipeline composition acceptance tests for User Story 1.
/// Tests chained processor execution and position mapping composition.
/// </summary>
public class ProcessorPipelineTests
{
    private static TransformationInput CreateInput(
        IReadOnlyList<StyleAndTextTuple>? fragments = null,
        int lineNumber = 0,
        Func<int, int>? sourceToDisplay = null)
    {
        var bc = new BufferControl();
        var doc = new Document("hello");
        var frags = fragments ?? new List<StyleAndTextTuple> { new("s", "hello") };
        return new TransformationInput(bc, doc, lineNumber, sourceToDisplay ?? (i => i), frags, 80, 24);
    }

    [Fact]
    public void Scenario1_IdentityProcessor_PassThrough()
    {
        var processor = new DummyProcessor();
        var fragments = new List<StyleAndTextTuple>
        {
            new("bold", "hello"),
            new("", " world"),
        };
        var input = CreateInput(fragments);

        var result = processor.ApplyTransformation(input);

        Assert.Same(fragments, result.Fragments);
        Assert.Equal(0, result.SourceToDisplay(0));
        Assert.Equal(5, result.SourceToDisplay(5));
    }

    [Fact]
    public void Scenario2_TwoOffsetComposition()
    {
        // Two test processors that shift by fixed amounts
        var p1 = new OffsetProcessor(2);
        var p2 = new OffsetProcessor(3);

        var merged = ProcessorUtils.MergeProcessors([p1, p2]);
        var input = CreateInput();
        var result = merged.ApplyTransformation(input);

        // Combined shift: +2 + +3 = +5
        Assert.Equal(5, result.SourceToDisplay(0));
        Assert.Equal(10, result.SourceToDisplay(5));
    }

    [Fact]
    public void Scenario3_TransformationInputFieldAccess()
    {
        var bc = new BufferControl();
        var doc = new Document("test\nline2");
        var fragments = new List<StyleAndTextTuple> { new("s", "test") };
        Func<int, int> s2d = i => i + 1;
        Func<int, IReadOnlyList<StyleAndTextTuple>> getLine = _ => [];

        var ti = new TransformationInput(bc, doc, 1, s2d, fragments, 80, 24, getLine);

        Assert.Same(bc, ti.BufferControl);
        Assert.Same(doc, ti.Document);
        Assert.Equal(1, ti.LineNumber);
        Assert.Equal(2, ti.SourceToDisplay(1));
        Assert.Same(fragments, ti.Fragments);
        Assert.Equal(80, ti.Width);
        Assert.Equal(24, ti.Height);
        Assert.NotNull(ti.GetLine);
    }

    [Fact]
    public void Scenario4_MergeProcessorsEmpty()
    {
        var result = ProcessorUtils.MergeProcessors([]);
        Assert.IsType<DummyProcessor>(result);
    }

    [Fact]
    public void Scenario5_MergeProcessorsSingle()
    {
        var p = new OffsetProcessor(5);
        var result = ProcessorUtils.MergeProcessors([p]);
        Assert.Same(p, result);
    }

    [Fact]
    public void BidirectionalMappingVerification()
    {
        var p1 = new OffsetProcessor(3);
        var p2 = new OffsetProcessor(7);

        var merged = ProcessorUtils.MergeProcessors([p1, p2]);
        var input = CreateInput();
        var result = merged.ApplyTransformation(input);

        // Round-trip verification
        for (int i = 0; i <= 20; i++)
        {
            var display = result.SourceToDisplay(i);
            var source = result.DisplayToSource(display);
            Assert.Equal(i, source);
        }
    }

    [Fact]
    public void NestedMergedProcessor()
    {
        // Create a merged processor and merge it with another
        var p1 = new OffsetProcessor(2);
        var p2 = new OffsetProcessor(3);
        var inner = ProcessorUtils.MergeProcessors([p1, p2]);

        var p3 = new OffsetProcessor(5);
        var outer = ProcessorUtils.MergeProcessors([inner, p3]);

        var input = CreateInput();
        var result = outer.ApplyTransformation(input);

        // Total shift: 2 + 3 + 5 = 10
        Assert.Equal(10, result.SourceToDisplay(0));
        Assert.Equal(0, result.DisplayToSource(10));
    }

    [Fact]
    public void OutOfRange_BoundaryPositionMapping()
    {
        var p1 = new OffsetProcessor(5);
        var merged = ProcessorUtils.MergeProcessors([p1, new DummyProcessor()]);
        var input = CreateInput();
        var result = merged.ApplyTransformation(input);

        // Boundary: position 0
        Assert.Equal(5, result.SourceToDisplay(0));
        Assert.Equal(0, result.DisplayToSource(5));

        // Large values still work
        Assert.Equal(1005, result.SourceToDisplay(1000));
    }

    private sealed class OffsetProcessor : IProcessor
    {
        private readonly int _offset;
        public OffsetProcessor(int offset) => _offset = offset;

        public Transformation ApplyTransformation(TransformationInput ti)
        {
            return new Transformation(
                ti.Fragments,
                sourceToDisplay: i => i + _offset,
                displayToSource: i => i - _offset);
        }
    }
}
