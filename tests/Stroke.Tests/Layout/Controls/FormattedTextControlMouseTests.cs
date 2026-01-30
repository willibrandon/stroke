using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;
using Xunit;

namespace Stroke.Tests.Layout.Controls;

/// <summary>
/// Tests for FormattedTextControl mouse handler functionality.
/// </summary>
public sealed class FormattedTextControlMouseTests
{
    #region Basic Mouse Handler Tests

    [Fact]
    public void MouseHandler_NoFragments_ReturnsNotImplemented()
    {
        var control = new FormattedTextControl(Array.Empty<StyleAndTextTuple>());

        var result = control.MouseHandler(new MouseEvent(
            new Point(0, 0),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None));

        Assert.Equal(NotImplementedOrNone.NotImplemented, result);
    }

    [Fact]
    public void MouseHandler_NoHandlerInFragments_ReturnsNotImplemented()
    {
        var control = new FormattedTextControl("Hello World");

        var result = control.MouseHandler(new MouseEvent(
            new Point(3, 0),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None));

        Assert.Equal(NotImplementedOrNone.NotImplemented, result);
    }

    [Fact]
    public void MouseHandler_WithHandler_CallsHandler()
    {
        var handlerCalled = false;
        MouseEvent? receivedEvent = null;

        var fragments = new[]
        {
            new StyleAndTextTuple("", "Click me",
                MouseHandler: e =>
                {
                    handlerCalled = true;
                    receivedEvent = e;
                    return NotImplementedOrNone.None;
                })
        };

        var control = new FormattedTextControl(fragments);

        var mouseEvent = new MouseEvent(
            new Point(3, 0),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None);

        var result = control.MouseHandler(mouseEvent);

        Assert.True(handlerCalled);
        Assert.NotNull(receivedEvent);
        Assert.Equal(mouseEvent, receivedEvent);
        Assert.Equal(NotImplementedOrNone.None, result);
    }

    [Fact]
    public void MouseHandler_HandlerReturnsNotImplemented_PropagatesResult()
    {
        var fragments = new[]
        {
            new StyleAndTextTuple("", "Click me",
                MouseHandler: _ => NotImplementedOrNone.NotImplemented)
        };

        var control = new FormattedTextControl(fragments);

        var result = control.MouseHandler(new MouseEvent(
            new Point(3, 0),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None));

        Assert.Equal(NotImplementedOrNone.NotImplemented, result);
    }

    #endregion

    #region Fragment Position Resolution Tests

    [Fact]
    public void MouseHandler_MultipleFragments_FindsCorrectHandler()
    {
        var handler1Called = false;
        var handler2Called = false;

        var fragments = new[]
        {
            new StyleAndTextTuple("class:a", "AAA",
                MouseHandler: _ => { handler1Called = true; return NotImplementedOrNone.None; }),
            new StyleAndTextTuple("class:b", "BBB",
                MouseHandler: _ => { handler2Called = true; return NotImplementedOrNone.None; })
        };

        var control = new FormattedTextControl(fragments);

        // Click on position 4 should hit second fragment (offset 3 = 'B')
        control.MouseHandler(new MouseEvent(
            new Point(4, 0),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None));

        Assert.False(handler1Called);
        Assert.True(handler2Called);
    }

    [Fact]
    public void MouseHandler_ClickOnFirstFragment_CallsFirstHandler()
    {
        var handler1Called = false;
        var handler2Called = false;

        var fragments = new[]
        {
            new StyleAndTextTuple("", "First",
                MouseHandler: _ => { handler1Called = true; return NotImplementedOrNone.None; }),
            new StyleAndTextTuple("", "Second",
                MouseHandler: _ => { handler2Called = true; return NotImplementedOrNone.None; })
        };

        var control = new FormattedTextControl(fragments);

        // Click on position 2 = 'r' in "First"
        control.MouseHandler(new MouseEvent(
            new Point(2, 0),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None));

        Assert.True(handler1Called);
        Assert.False(handler2Called);
    }

    [Fact]
    public void MouseHandler_FragmentWithoutHandler_SkipsToBreak()
    {
        var handlerCalled = false;

        var fragments = new[]
        {
            new StyleAndTextTuple("", "No handler"),
            new StyleAndTextTuple("", "Has handler",
                MouseHandler: _ => { handlerCalled = true; return NotImplementedOrNone.None; })
        };

        var control = new FormattedTextControl(fragments);

        // Click on position 3 = first fragment (no handler)
        var result = control.MouseHandler(new MouseEvent(
            new Point(3, 0),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None));

        Assert.False(handlerCalled);
        Assert.Equal(NotImplementedOrNone.NotImplemented, result);
    }

    #endregion

    #region Multi-line Mouse Handler Tests

    [Fact]
    public void MouseHandler_MultilineText_FindsCorrectLine()
    {
        var line0Called = false;
        var line1Called = false;

        var fragments = new[]
        {
            new StyleAndTextTuple("", "Line 0\n"),
            new StyleAndTextTuple("", "Line 1",
                MouseHandler: _ => { line1Called = true; return NotImplementedOrNone.None; })
        };

        var control = new FormattedTextControl(fragments);

        // Click on line 1
        control.MouseHandler(new MouseEvent(
            new Point(2, 1),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None));

        Assert.False(line0Called);
        Assert.True(line1Called);
    }

    [Fact]
    public void MouseHandler_YBeyondLines_ReturnsNotImplemented()
    {
        var control = new FormattedTextControl("Single line");

        var result = control.MouseHandler(new MouseEvent(
            new Point(0, 5),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None));

        Assert.Equal(NotImplementedOrNone.NotImplemented, result);
    }

    [Fact]
    public void MouseHandler_NegativeY_ReturnsNotImplemented()
    {
        var control = new FormattedTextControl("Hello");

        var result = control.MouseHandler(new MouseEvent(
            new Point(0, -1),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None));

        Assert.Equal(NotImplementedOrNone.NotImplemented, result);
    }

    #endregion

    #region Event Type Tests

    [Fact]
    public void MouseHandler_MouseUp_PassesEventType()
    {
        MouseEventType? receivedType = null;

        var fragments = new[]
        {
            new StyleAndTextTuple("", "Click",
                MouseHandler: e => { receivedType = e.EventType; return NotImplementedOrNone.None; })
        };

        var control = new FormattedTextControl(fragments);

        control.MouseHandler(new MouseEvent(
            new Point(0, 0),
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None));

        Assert.Equal(MouseEventType.MouseUp, receivedType);
    }

    [Fact]
    public void MouseHandler_ScrollUp_PassesEventType()
    {
        MouseEventType? receivedType = null;

        var fragments = new[]
        {
            new StyleAndTextTuple("", "Scrollable",
                MouseHandler: e => { receivedType = e.EventType; return NotImplementedOrNone.None; })
        };

        var control = new FormattedTextControl(fragments);

        control.MouseHandler(new MouseEvent(
            new Point(0, 0),
            MouseEventType.ScrollUp,
            MouseButton.None,
            MouseModifiers.None));

        Assert.Equal(MouseEventType.ScrollUp, receivedType);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void MouseHandler_XAtEndOfLine_ReturnsNotImplemented()
    {
        var handlerCalled = false;
        var fragments = new[]
        {
            new StyleAndTextTuple("", "Short",
                MouseHandler: _ => { handlerCalled = true; return NotImplementedOrNone.None; })
        };

        var control = new FormattedTextControl(fragments);

        // Click beyond end of text (x=100 for 5-char fragment)
        var result = control.MouseHandler(new MouseEvent(
            new Point(100, 0),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None));

        Assert.False(handlerCalled);
        Assert.Equal(NotImplementedOrNone.NotImplemented, result);
    }

    [Fact]
    public void MouseHandler_XAtExactBoundary_HitsCorrectFragment()
    {
        var handler1Called = false;
        var handler2Called = false;

        var fragments = new[]
        {
            new StyleAndTextTuple("", "ABC",
                MouseHandler: _ => { handler1Called = true; return NotImplementedOrNone.None; }),
            new StyleAndTextTuple("", "DEF",
                MouseHandler: _ => { handler2Called = true; return NotImplementedOrNone.None; })
        };

        var control = new FormattedTextControl(fragments);

        // Click at x=2 (last char of "ABC")
        control.MouseHandler(new MouseEvent(
            new Point(2, 0),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None));

        Assert.True(handler1Called);
        Assert.False(handler2Called);
    }

    [Fact]
    public void MouseHandler_XAtFirstCharOfSecondFragment_HitsSecond()
    {
        var handler1Called = false;
        var handler2Called = false;

        var fragments = new[]
        {
            new StyleAndTextTuple("", "ABC",
                MouseHandler: _ => { handler1Called = true; return NotImplementedOrNone.None; }),
            new StyleAndTextTuple("", "DEF",
                MouseHandler: _ => { handler2Called = true; return NotImplementedOrNone.None; })
        };

        var control = new FormattedTextControl(fragments);

        // Click at x=3 (first char of "DEF")
        control.MouseHandler(new MouseEvent(
            new Point(3, 0),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None));

        Assert.False(handler1Called);
        Assert.True(handler2Called);
    }

    #endregion
}
