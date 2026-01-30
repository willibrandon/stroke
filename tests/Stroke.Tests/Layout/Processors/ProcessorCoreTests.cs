using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for TransformationInput and Transformation core types.
/// </summary>
public class TransformationInputTests
{
    private static BufferControl CreateBufferControl() => new();

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var bc = CreateBufferControl();
        var doc = new Document("hello");
        var fragments = new List<StyleAndTextTuple> { new("s", "hello") };
        Func<int, int> s2d = i => i + 1;
        Func<int, IReadOnlyList<StyleAndTextTuple>> getLine = _ => [];

        var ti = new TransformationInput(bc, doc, 0, s2d, fragments, 80, 24, getLine);

        Assert.Same(bc, ti.BufferControl);
        Assert.Same(doc, ti.Document);
        Assert.Equal(0, ti.LineNumber);
        Assert.Same(s2d, ti.SourceToDisplay);
        Assert.Same(fragments, ti.Fragments);
        Assert.Equal(80, ti.Width);
        Assert.Equal(24, ti.Height);
        Assert.Same(getLine, ti.GetLine);
    }

    [Fact]
    public void Constructor_GetLineDefaultsToNull()
    {
        var bc = CreateBufferControl();
        var doc = new Document("hello");
        var fragments = new List<StyleAndTextTuple>();
        var ti = new TransformationInput(bc, doc, 0, i => i, fragments, 80, 24);

        Assert.Null(ti.GetLine);
    }

    [Fact]
    public void Unpack_ReturnsTupleOfMainProperties()
    {
        var bc = CreateBufferControl();
        var doc = new Document("hello");
        var fragments = new List<StyleAndTextTuple> { new("s", "hello") };
        Func<int, int> s2d = i => i;

        var ti = new TransformationInput(bc, doc, 3, s2d, fragments, 80, 24);
        var (ubc, udoc, uln, us2d, ufrags, uw, uh) = ti.Unpack();

        Assert.Same(bc, ubc);
        Assert.Same(doc, udoc);
        Assert.Equal(3, uln);
        Assert.Same(s2d, us2d);
        Assert.Same(fragments, ufrags);
        Assert.Equal(80, uw);
        Assert.Equal(24, uh);
    }
}

public class TransformationTests
{
    [Fact]
    public void Constructor_SetsFragments()
    {
        var fragments = new List<StyleAndTextTuple> { new("s", "text") };
        var t = new Transformation(fragments);
        Assert.Same(fragments, t.Fragments);
    }

    [Fact]
    public void Constructor_DefaultMappingsAreIdentity()
    {
        var fragments = new List<StyleAndTextTuple>();
        var t = new Transformation(fragments);

        Assert.Equal(0, t.SourceToDisplay(0));
        Assert.Equal(5, t.SourceToDisplay(5));
        Assert.Equal(0, t.DisplayToSource(0));
        Assert.Equal(5, t.DisplayToSource(5));
    }

    [Fact]
    public void Constructor_CustomMappings()
    {
        var fragments = new List<StyleAndTextTuple>();
        var t = new Transformation(
            fragments,
            sourceToDisplay: i => i + 3,
            displayToSource: i => i - 3);

        Assert.Equal(3, t.SourceToDisplay(0));
        Assert.Equal(8, t.SourceToDisplay(5));
        Assert.Equal(-3, t.DisplayToSource(0));
        Assert.Equal(2, t.DisplayToSource(5));
    }

    [Fact]
    public void Constructor_NullMappingsFallBackToIdentity()
    {
        var fragments = new List<StyleAndTextTuple>();
        var t = new Transformation(fragments, null, null);

        Assert.Equal(42, t.SourceToDisplay(42));
        Assert.Equal(42, t.DisplayToSource(42));
    }
}
