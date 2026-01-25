using Stroke.FormattedText;
using Xunit;

namespace Stroke.Tests.FormattedText;

/// <summary>
/// Tests for <see cref="FormattedText"/>.
/// </summary>
public sealed class FormattedTextTests
{
    [Fact]
    public void Empty_ReturnsSingleton()
    {
        var empty1 = Stroke.FormattedText.FormattedText.Empty;
        var empty2 = Stroke.FormattedText.FormattedText.Empty;

        Assert.Same(empty1, empty2);
        Assert.Empty(empty1);
    }

    [Fact]
    public void Constructor_WithEnumerable_CreatesFragments()
    {
        var fragments = new[]
        {
            new StyleAndTextTuple("bold", "hello"),
            new StyleAndTextTuple("", " "),
            new StyleAndTextTuple("italic", "world")
        };

        var ft = new Stroke.FormattedText.FormattedText(fragments);

        Assert.Equal(3, ft.Count);
        Assert.Equal("bold", ft[0].Style);
        Assert.Equal("hello", ft[0].Text);
        Assert.Equal("", ft[1].Style);
        Assert.Equal(" ", ft[1].Text);
        Assert.Equal("italic", ft[2].Style);
        Assert.Equal("world", ft[2].Text);
    }

    [Fact]
    public void Constructor_WithParams_CreatesFragments()
    {
        var ft = new Stroke.FormattedText.FormattedText(
            new StyleAndTextTuple("bold", "hello"),
            new StyleAndTextTuple("", "world")
        );

        Assert.Equal(2, ft.Count);
        Assert.Equal("bold", ft[0].Style);
        Assert.Equal("hello", ft[0].Text);
    }

    [Fact]
    public void Constructor_EmptyEnumerable_CreatesEmptyList()
    {
        var ft = new Stroke.FormattedText.FormattedText([]);

        Assert.Empty(ft);
    }

    [Fact]
    public void Indexer_ValidIndex_ReturnsFragment()
    {
        var ft = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("style1", "text1"),
            new StyleAndTextTuple("style2", "text2")
        ]);

        Assert.Equal("style1", ft[0].Style);
        Assert.Equal("style2", ft[1].Style);
    }

    [Fact]
    public void Indexer_InvalidIndex_ThrowsException()
    {
        var ft = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("style", "text")
        ]);

        // ImmutableArray throws IndexOutOfRangeException
        Assert.Throws<IndexOutOfRangeException>(() => ft[1]);
        Assert.Throws<IndexOutOfRangeException>(() => ft[-1]);
    }

    [Fact]
    public void GetEnumerator_IteratesAllFragments()
    {
        var ft = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("s1", "t1"),
            new StyleAndTextTuple("s2", "t2"),
            new StyleAndTextTuple("s3", "t3")
        ]);

        var styles = new List<string>();
        foreach (var fragment in ft)
        {
            styles.Add(fragment.Style);
        }

        Assert.Equal(["s1", "s2", "s3"], styles);
    }

    [Fact]
    public void IEnumerable_NonGeneric_Works()
    {
        var ft = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("s1", "t1")
        ]);

        var count = 0;
        System.Collections.IEnumerable enumerable = ft;
        foreach (var _ in enumerable)
        {
            count++;
        }

        Assert.Equal(1, count);
    }

    [Fact]
    public void Equality_SameFragments_AreEqual()
    {
        var ft1 = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("bold", "hello")
        ]);
        var ft2 = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("bold", "hello")
        ]);

        Assert.True(ft1.Equals(ft2));
        Assert.Equal(ft1.GetHashCode(), ft2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentFragments_AreNotEqual()
    {
        var ft1 = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("bold", "hello")
        ]);
        var ft2 = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("italic", "hello")
        ]);

        Assert.False(ft1.Equals(ft2));
    }

    [Fact]
    public void Equality_DifferentCount_AreNotEqual()
    {
        var ft1 = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("bold", "hello")
        ]);
        var ft2 = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("bold", "hello"),
            new StyleAndTextTuple("", "world")
        ]);

        Assert.False(ft1.Equals(ft2));
    }

    [Fact]
    public void Equality_WithNull_ReturnsFalse()
    {
        var ft = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("bold", "hello")
        ]);

        Assert.False(ft.Equals(null));
    }

    [Fact]
    public void ImplicitConversion_FromString_CreatesUnstyledFragment()
    {
        Stroke.FormattedText.FormattedText ft = "hello world";

        var fragment = Assert.Single(ft);
        Assert.Equal("", fragment.Style);
        Assert.Equal("hello world", fragment.Text);
    }

    [Fact]
    public void ImplicitConversion_FromEmptyString_ReturnsEmpty()
    {
        Stroke.FormattedText.FormattedText ft = "";

        Assert.Same(Stroke.FormattedText.FormattedText.Empty, ft);
    }

    [Fact]
    public void ImplicitConversion_FromNullString_ReturnsEmpty()
    {
        string? nullStr = null;
        Stroke.FormattedText.FormattedText ft = nullStr!;

        Assert.Same(Stroke.FormattedText.FormattedText.Empty, ft);
    }
}
