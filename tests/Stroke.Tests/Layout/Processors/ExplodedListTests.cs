using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for ExplodedList and LayoutUtils.ExplodeTextFragments.
/// </summary>
public class ExplodedListTests
{
    // --- ExplodedList tests ---

    [Fact]
    public void Exploded_AlwaysReturnsTrue()
    {
        var list = new ExplodedList(new List<StyleAndTextTuple>());
        Assert.True(list.Exploded);
    }

    [Fact]
    public void Add_SingleCharFragment_AddsAsIs()
    {
        var list = new ExplodedList(new List<StyleAndTextTuple>());
        list.Add(new StyleAndTextTuple("style", "a"));
        Assert.Single(list);
        Assert.Equal("a", list[0].Text);
        Assert.Equal("style", list[0].Style);
    }

    [Fact]
    public void Add_MultiCharFragment_ExplodesIntoSingleChars()
    {
        var list = new ExplodedList(new List<StyleAndTextTuple>());
        list.Add(new StyleAndTextTuple("style", "abc"));
        Assert.Equal(3, list.Count);
        Assert.Equal("a", list[0].Text);
        Assert.Equal("b", list[1].Text);
        Assert.Equal("c", list[2].Text);
        // All should have same style
        Assert.All(list, f => Assert.Equal("style", f.Style));
    }

    [Fact]
    public void Add_PreservesMouseHandler()
    {
        Func<MouseEvent, NotImplementedOrNone> handler = _ => NotImplementedOrNone.None;
        var list = new ExplodedList(new List<StyleAndTextTuple>());
        list.Add(new StyleAndTextTuple("s", "ab", handler));
        Assert.Equal(2, list.Count);
        Assert.Same(handler, list[0].MouseHandler);
        Assert.Same(handler, list[1].MouseHandler);
    }

    [Fact]
    public void SetItem_SingleChar_ReplacesInPlace()
    {
        var list = new ExplodedList(new List<StyleAndTextTuple>
        {
            new("s1", "a"),
            new("s2", "b"),
        });
        list[0] = new StyleAndTextTuple("s3", "x");
        Assert.Equal(2, list.Count);
        Assert.Equal("x", list[0].Text);
        Assert.Equal("s3", list[0].Style);
    }

    [Fact]
    public void SetItem_MultiChar_ExplodesAndChangesLength()
    {
        var list = new ExplodedList(new List<StyleAndTextTuple>
        {
            new("s1", "a"),
            new("s2", "b"),
        });
        list[0] = new StyleAndTextTuple("s3", "xyz");
        // Should now be: x, y, z, b
        Assert.Equal(4, list.Count);
        Assert.Equal("x", list[0].Text);
        Assert.Equal("y", list[1].Text);
        Assert.Equal("z", list[2].Text);
        Assert.Equal("b", list[3].Text);
    }

    [Fact]
    public void InsertItem_MultiChar_ExplodesAtIndex()
    {
        var list = new ExplodedList(new List<StyleAndTextTuple>
        {
            new("s1", "a"),
        });
        list.Insert(0, new StyleAndTextTuple("s2", "xy"));
        // Should be: x, y, a
        Assert.Equal(3, list.Count);
        Assert.Equal("x", list[0].Text);
        Assert.Equal("y", list[1].Text);
        Assert.Equal("a", list[2].Text);
    }

    [Fact]
    public void AddRange_ExplodesEachItem()
    {
        var list = new ExplodedList(new List<StyleAndTextTuple>());
        list.AddRange(new[]
        {
            new StyleAndTextTuple("s1", "ab"),
            new StyleAndTextTuple("s2", "c"),
        });
        Assert.Equal(3, list.Count);
        Assert.Equal("a", list[0].Text);
        Assert.Equal("s1", list[0].Style);
        Assert.Equal("b", list[1].Text);
        Assert.Equal("s1", list[1].Style);
        Assert.Equal("c", list[2].Text);
        Assert.Equal("s2", list[2].Style);
    }

    [Fact]
    public void Add_EmptyString_AddsEmptyFragment()
    {
        var list = new ExplodedList(new List<StyleAndTextTuple>());
        list.Add(new StyleAndTextTuple("s", ""));
        // Empty text has length 0, so single-char path (length <= 1)
        Assert.Single(list);
        Assert.Equal("", list[0].Text);
    }

    [Fact]
    public void SetItem_PreservesMouseHandler()
    {
        Func<MouseEvent, NotImplementedOrNone> handler = _ => NotImplementedOrNone.None;
        var list = new ExplodedList(new List<StyleAndTextTuple>
        {
            new("s1", "a"),
        });
        list[0] = new StyleAndTextTuple("s2", "xy", handler);
        Assert.Equal(2, list.Count);
        Assert.Same(handler, list[0].MouseHandler);
        Assert.Same(handler, list[1].MouseHandler);
    }

    // --- ExplodeTextFragments tests ---

    [Fact]
    public void ExplodeTextFragments_SplitsMultiCharFragments()
    {
        var fragments = new List<StyleAndTextTuple>
        {
            new("s1", "abc"),
            new("s2", "de"),
        };
        var result = LayoutUtils.ExplodeTextFragments(fragments);
        Assert.Equal(5, result.Count);
        Assert.Equal("a", result[0].Text);
        Assert.Equal("s1", result[0].Style);
        Assert.Equal("b", result[1].Text);
        Assert.Equal("c", result[2].Text);
        Assert.Equal("d", result[3].Text);
        Assert.Equal("s2", result[3].Style);
        Assert.Equal("e", result[4].Text);
    }

    [Fact]
    public void ExplodeTextFragments_Idempotent_ReturnsSameList()
    {
        var fragments = new List<StyleAndTextTuple>
        {
            new("s1", "ab"),
        };
        var first = LayoutUtils.ExplodeTextFragments(fragments);
        var second = LayoutUtils.ExplodeTextFragments(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void ExplodeTextFragments_PreservesStyleAndMouseHandler()
    {
        Func<MouseEvent, NotImplementedOrNone> handler = _ => NotImplementedOrNone.None;
        var fragments = new List<StyleAndTextTuple>
        {
            new("bold", "hi", handler),
        };
        var result = LayoutUtils.ExplodeTextFragments(fragments);
        Assert.Equal(2, result.Count);
        Assert.Equal("bold", result[0].Style);
        Assert.Equal("h", result[0].Text);
        Assert.Same(handler, result[0].MouseHandler);
        Assert.Equal("bold", result[1].Style);
        Assert.Equal("i", result[1].Text);
        Assert.Same(handler, result[1].MouseHandler);
    }

    [Fact]
    public void ExplodeTextFragments_EmptyList_ReturnsEmptyExplodedList()
    {
        var fragments = new List<StyleAndTextTuple>();
        var result = LayoutUtils.ExplodeTextFragments(fragments);
        Assert.Empty(result);
        Assert.True(result.Exploded);
    }

    [Fact]
    public void ExplodeTextFragments_SingleCharFragments_AlreadyExploded()
    {
        var fragments = new List<StyleAndTextTuple>
        {
            new("s", "a"),
            new("s", "b"),
        };
        var result = LayoutUtils.ExplodeTextFragments(fragments);
        Assert.Equal(2, result.Count);
        Assert.Equal("a", result[0].Text);
        Assert.Equal("b", result[1].Text);
    }

    [Fact]
    public void ExplodeTextFragments_MultiByteUnicode_ExplodesPerCharacter()
    {
        // CJK characters - each is one char in C# but multi-byte in UTF-8
        var fragments = new List<StyleAndTextTuple>
        {
            new("s", "你好"),
        };
        var result = LayoutUtils.ExplodeTextFragments(fragments);
        Assert.Equal(2, result.Count);
        Assert.Equal("你", result[0].Text);
        Assert.Equal("好", result[1].Text);
    }
}
