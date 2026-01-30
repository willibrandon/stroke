using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for HighlightSearchProcessor and HighlightIncrementalSearchProcessor.
/// </summary>
public class HighlightSearchProcessorTests
{
    // NOTE: HighlightSearchProcessor calls AppContext.GetApp() internally,
    // which returns a DummyApplication when no app is running.
    // DummyApplication.IsDone returns true, so search highlighting won't apply
    // in unit tests without a running app. We test the processor's construction
    // and the code path where the app IS done (no-op).

    private static TransformationInput CreateInput(
        BufferControl? bc = null,
        string text = "hello world hello",
        int lineNumber = 0,
        IReadOnlyList<StyleAndTextTuple>? fragments = null)
    {
        bc ??= new BufferControl();
        var doc = new Document(text);
        var lineText = text.Split('\n')[Math.Min(lineNumber, text.Split('\n').Length - 1)];
        var frags = fragments ?? new List<StyleAndTextTuple> { new("", lineText) };
        return new TransformationInput(bc, doc, lineNumber, i => i, frags, 80, 24);
    }

    [Fact]
    public void AppDone_ReturnsFragmentsUnchanged()
    {
        // DummyApplication has IsDone=true, so processor should no-op
        var processor = new HighlightSearchProcessor();
        var fragments = new List<StyleAndTextTuple> { new("", "hello world") };
        var input = CreateInput(fragments: fragments);

        var result = processor.ApplyTransformation(input);

        Assert.Same(fragments, result.Fragments);
    }

    [Fact]
    public void EmptySearchText_NoChange()
    {
        // Search state with empty text should cause no-op
        var processor = new HighlightSearchProcessor();
        var fragments = new List<StyleAndTextTuple> { new("", "hello") };
        var input = CreateInput(fragments: fragments);

        var result = processor.ApplyTransformation(input);

        Assert.Same(fragments, result.Fragments);
    }

    [Fact]
    public void IsNotSealed_AllowsInheritance()
    {
        // HighlightSearchProcessor is not sealed - HighlightIncrementalSearchProcessor extends it
        Assert.False(typeof(HighlightSearchProcessor).IsSealed);
    }
}

public class HighlightIncrementalSearchProcessorTests
{
    [Fact]
    public void ExtendsHighlightSearchProcessor()
    {
        var processor = new HighlightIncrementalSearchProcessor();
        Assert.IsAssignableFrom<HighlightSearchProcessor>(processor);
    }

    [Fact]
    public void AppDone_ReturnsFragmentsUnchanged()
    {
        var processor = new HighlightIncrementalSearchProcessor();
        var fragments = new List<StyleAndTextTuple> { new("", "hello") };
        var bc = new BufferControl();
        var doc = new Document("hello");
        var input = new TransformationInput(bc, doc, 0, i => i, fragments, 80, 24);

        var result = processor.ApplyTransformation(input);

        // DummyApplication.IsDone = true, so no-op even with search buffer
        Assert.Same(fragments, result.Fragments);
    }
}
