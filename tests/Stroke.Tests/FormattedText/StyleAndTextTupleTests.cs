using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
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

        var (style, text, handler) = tuple;

        Assert.Equal("bold", style);
        Assert.Equal("hello", text);
        Assert.Null(handler);
    }

    [Fact]
    public void ToString_ReturnsReadableFormat()
    {
        var tuple = new StyleAndTextTuple("bold", "hello");

        var str = tuple.ToString();

        Assert.Contains("bold", str);
        Assert.Contains("hello", str);
    }

    [Fact]
    public void Constructor_WithMouseHandler_SetsAllProperties()
    {
        static NotImplementedOrNone Handler(MouseEvent e) => NotImplementedOrNone.None;
        var tuple = new StyleAndTextTuple("class:link", "Click me", Handler);

        Assert.Equal("class:link", tuple.Style);
        Assert.Equal("Click me", tuple.Text);
        Assert.NotNull(tuple.MouseHandler);
    }

    [Fact]
    public void Constructor_WithoutMouseHandler_HasNullHandler()
    {
        var tuple = new StyleAndTextTuple("bold", "hello");

        Assert.Null(tuple.MouseHandler);
    }

    [Fact]
    public void Constructor_WithExplicitNullHandler_HasNullHandler()
    {
        var tuple = new StyleAndTextTuple("bold", "hello", null);

        Assert.Null(tuple.MouseHandler);
    }

    [Fact]
    public void ImplicitConversion_FromThreeTuple_Works()
    {
        static NotImplementedOrNone Handler(MouseEvent e) => NotImplementedOrNone.None;
        StyleAndTextTuple tuple = ("class:link", "Click", (Func<MouseEvent, NotImplementedOrNone>?)Handler);

        Assert.Equal("class:link", tuple.Style);
        Assert.Equal("Click", tuple.Text);
        Assert.NotNull(tuple.MouseHandler);
    }

    [Fact]
    public void MouseHandler_CanBeInvoked()
    {
        bool handlerCalled = false;
        NotImplementedOrNone Handler(MouseEvent e)
        {
            handlerCalled = true;
            return NotImplementedOrNone.None;
        }

        var tuple = new StyleAndTextTuple("class:button", "Submit", Handler);
        var mouseEvent = new MouseEvent(
            new Point(0, 0),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None);

        var result = tuple.MouseHandler!(mouseEvent);

        Assert.True(handlerCalled);
        Assert.Same(NotImplementedOrNone.None, result);
    }

    [Fact]
    public void MouseHandler_CanReturnNotImplemented()
    {
        static NotImplementedOrNone Handler(MouseEvent e) => NotImplementedOrNone.NotImplemented;

        var tuple = new StyleAndTextTuple("class:area", "Text", Handler);
        var mouseEvent = new MouseEvent(
            new Point(0, 0),
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);

        var result = tuple.MouseHandler!(mouseEvent);

        Assert.Same(NotImplementedOrNone.NotImplemented, result);
    }

    [Fact]
    public void Equality_WithSameHandler_AreEqual()
    {
        static NotImplementedOrNone Handler(MouseEvent e) => NotImplementedOrNone.None;
        var tuple1 = new StyleAndTextTuple("style", "text", Handler);
        var tuple2 = new StyleAndTextTuple("style", "text", Handler);

        Assert.Equal(tuple1, tuple2);
    }

    [Fact]
    public void Equality_WithDifferentHandlers_AreNotEqual()
    {
        static NotImplementedOrNone Handler1(MouseEvent e) => NotImplementedOrNone.None;
        static NotImplementedOrNone Handler2(MouseEvent e) => NotImplementedOrNone.NotImplemented;

        var tuple1 = new StyleAndTextTuple("style", "text", Handler1);
        var tuple2 = new StyleAndTextTuple("style", "text", Handler2);

        Assert.NotEqual(tuple1, tuple2);
    }

    [Fact]
    public void Equality_WithNullVsNonNullHandler_AreNotEqual()
    {
        static NotImplementedOrNone Handler(MouseEvent e) => NotImplementedOrNone.None;

        var tuple1 = new StyleAndTextTuple("style", "text");
        var tuple2 = new StyleAndTextTuple("style", "text", Handler);

        Assert.NotEqual(tuple1, tuple2);
    }
}
