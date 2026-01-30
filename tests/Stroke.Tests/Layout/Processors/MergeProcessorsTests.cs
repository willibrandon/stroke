using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for DummyProcessor, ProcessorUtils.MergeProcessors, and MergedProcessor.
/// </summary>
public class DummyProcessorTests
{
    private static TransformationInput CreateInput(
        IReadOnlyList<StyleAndTextTuple>? fragments = null,
        int lineNumber = 0)
    {
        var bc = new BufferControl();
        var doc = new Document("hello");
        var frags = fragments ?? new List<StyleAndTextTuple> { new("s", "hello") };
        return new TransformationInput(bc, doc, lineNumber, i => i, frags, 80, 24);
    }

    [Fact]
    public void ApplyTransformation_ReturnsFragmentsUnchanged()
    {
        var fragments = new List<StyleAndTextTuple>
        {
            new("bold", "hello"),
            new("", " world"),
        };
        var input = CreateInput(fragments);
        var processor = new DummyProcessor();

        var result = processor.ApplyTransformation(input);

        Assert.Same(fragments, result.Fragments);
    }

    [Fact]
    public void ApplyTransformation_IdentityMappings()
    {
        var input = CreateInput();
        var processor = new DummyProcessor();

        var result = processor.ApplyTransformation(input);

        Assert.Equal(0, result.SourceToDisplay(0));
        Assert.Equal(5, result.SourceToDisplay(5));
        Assert.Equal(0, result.DisplayToSource(0));
        Assert.Equal(5, result.DisplayToSource(5));
    }

    [Fact]
    public void ApplyTransformation_EmptyFragments()
    {
        var fragments = new List<StyleAndTextTuple>();
        var input = CreateInput(fragments);
        var processor = new DummyProcessor();

        var result = processor.ApplyTransformation(input);

        Assert.Empty(result.Fragments);
    }
}

public class MergeProcessorsTests
{
    private static TransformationInput CreateInput(
        IReadOnlyList<StyleAndTextTuple>? fragments = null)
    {
        var bc = new BufferControl();
        var doc = new Document("hello");
        var frags = fragments ?? new List<StyleAndTextTuple> { new("s", "hello") };
        return new TransformationInput(bc, doc, 0, i => i, frags, 80, 24);
    }

    [Fact]
    public void MergeProcessors_EmptyList_ReturnsDummyProcessor()
    {
        var result = ProcessorUtils.MergeProcessors([]);
        Assert.IsType<DummyProcessor>(result);
    }

    [Fact]
    public void MergeProcessors_SingleProcessor_ReturnsItDirectly()
    {
        var processor = new DummyProcessor();
        var result = ProcessorUtils.MergeProcessors([processor]);
        Assert.Same(processor, result);
    }

    [Fact]
    public void MergeProcessors_MultipleProcessors_ReturnsMergedProcessor()
    {
        var p1 = new DummyProcessor();
        var p2 = new DummyProcessor();
        var result = ProcessorUtils.MergeProcessors([p1, p2]);

        // MergedProcessor is internal, so just check it's not DummyProcessor
        Assert.IsNotType<DummyProcessor>(result);
    }

    [Fact]
    public void MergedProcessor_ChainsProcessorsInOrder()
    {
        // Create two processors that add different prefixes
        var p1 = new TestOffsetProcessor(2); // shifts by +2
        var p2 = new TestOffsetProcessor(3); // shifts by +3

        var merged = ProcessorUtils.MergeProcessors([p1, p2]);
        var input = CreateInput();
        var result = merged.ApplyTransformation(input);

        // Source-to-display: 0 -> +2 -> +3 = 5
        Assert.Equal(5, result.SourceToDisplay(0));
        Assert.Equal(15, result.SourceToDisplay(10));

        // Display-to-source: reverse chain: 5 -> -3 -> -2 = 0
        Assert.Equal(0, result.DisplayToSource(5));
        Assert.Equal(10, result.DisplayToSource(15));
    }

    [Fact]
    public void MergedProcessor_BidirectionalMapping()
    {
        var p1 = new TestOffsetProcessor(5);
        var p2 = new TestOffsetProcessor(10);
        var merged = ProcessorUtils.MergeProcessors([p1, p2]);
        var input = CreateInput();
        var result = merged.ApplyTransformation(input);

        // Round-trip: source -> display -> source should be identity
        for (int i = 0; i < 20; i++)
        {
            var display = result.SourceToDisplay(i);
            var source = result.DisplayToSource(display);
            Assert.Equal(i, source);
        }
    }

    [Fact]
    public void MergedProcessor_EmptyFragmentList()
    {
        var p1 = new DummyProcessor();
        var merged = ProcessorUtils.MergeProcessors([p1, new DummyProcessor()]);
        var input = CreateInput(new List<StyleAndTextTuple>());
        var result = merged.ApplyTransformation(input);
        Assert.Empty(result.Fragments);
    }

    [Fact]
    public void MergedProcessor_BoundaryPositionValues()
    {
        var p1 = new TestOffsetProcessor(1);
        var merged = ProcessorUtils.MergeProcessors([p1]);
        var input = CreateInput();
        var result = merged.ApplyTransformation(input);

        // Position 0 maps correctly
        Assert.Equal(1, result.SourceToDisplay(0));
        Assert.Equal(0, result.DisplayToSource(1));
    }

    [Fact]
    public void MergedProcessor_InitialFunctionRemoval()
    {
        // When the input TransformationInput already has a non-identity
        // source_to_display, the merged result should NOT include it.
        var p1 = new TestOffsetProcessor(3);
        var bc = new BufferControl();
        var doc = new Document("hello");
        var frags = new List<StyleAndTextTuple> { new("s", "hello") };

        // Input already shifts by +10
        var input = new TransformationInput(bc, doc, 0, i => i + 10, frags, 80, 24);
        var merged = ProcessorUtils.MergeProcessors([p1]);
        var result = merged.ApplyTransformation(input);

        // The merged result should only include p1's shift (+3),
        // not the initial +10
        Assert.Equal(3, result.SourceToDisplay(0));
        Assert.Equal(13, result.SourceToDisplay(10));
    }

    /// <summary>
    /// Simple test processor that shifts positions by a fixed amount.
    /// </summary>
    private sealed class TestOffsetProcessor : IProcessor
    {
        private readonly int _offset;
        public TestOffsetProcessor(int offset) => _offset = offset;

        public Transformation ApplyTransformation(TransformationInput ti)
        {
            return new Transformation(
                ti.Fragments,
                sourceToDisplay: i => i + _offset,
                displayToSource: i => i - _offset);
        }
    }
}
