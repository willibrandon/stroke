using Stroke.FormattedText;
using Xunit;

namespace Stroke.Tests.FormattedText;

/// <summary>
/// Tests for <see cref="StyleAndTextTuple"/>.
/// </summary>
public sealed class StyleAndTextTupleTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var tuple = new StyleAndTextTuple("bold", "hello");

        Assert.Equal("bold", tuple.Style);
        Assert.Equal("hello", tuple.Text);
    }

    [Fact]
    public void Constructor_EmptyStyle_IsValid()
    {
        var tuple = new StyleAndTextTuple("", "text");

        Assert.Equal("", tuple.Style);
        Assert.Equal("text", tuple.Text);
    }

    [Fact]
    public void Constructor_EmptyText_IsValid()
    {
        var tuple = new StyleAndTextTuple("style", "");

        Assert.Equal("style", tuple.Style);
        Assert.Equal("", tuple.Text);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var tuple1 = new StyleAndTextTuple("bold", "hello");
        var tuple2 = new StyleAndTextTuple("bold", "hello");

        Assert.Equal(tuple1, tuple2);
        Assert.True(tuple1 == tuple2);
        Assert.False(tuple1 != tuple2);
        Assert.Equal(tuple1.GetHashCode(), tuple2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentStyle_AreNotEqual()
    {
        var tuple1 = new StyleAndTextTuple("bold", "hello");
        var tuple2 = new StyleAndTextTuple("italic", "hello");

        Assert.NotEqual(tuple1, tuple2);
        Assert.False(tuple1 == tuple2);
        Assert.True(tuple1 != tuple2);
    }

    [Fact]
    public void Equality_DifferentText_AreNotEqual()
    {
        var tuple1 = new StyleAndTextTuple("bold", "hello");
        var tuple2 = new StyleAndTextTuple("bold", "world");

        Assert.NotEqual(tuple1, tuple2);
    }

    [Fact]
    public void ImplicitConversion_FromValueTuple_Works()
    {
        StyleAndTextTuple tuple = ("bold", "hello");

        Assert.Equal("bold", tuple.Style);
        Assert.Equal("hello", tuple.Text);
    }

    [Fact]
    public void Deconstruction_ReturnsComponents()
    {
        var tuple = new StyleAndTextTuple("bold", "hello");

        var (style, text) = tuple;

        Assert.Equal("bold", style);
        Assert.Equal("hello", text);
    }

    [Fact]
    public void ToString_ReturnsReadableFormat()
    {
        var tuple = new StyleAndTextTuple("bold", "hello");

        var str = tuple.ToString();

        Assert.Contains("bold", str);
        Assert.Contains("hello", str);
    }
}
