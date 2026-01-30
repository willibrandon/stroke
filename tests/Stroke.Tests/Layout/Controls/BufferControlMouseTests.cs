using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;
using Xunit;

using Buffer = Stroke.Core.Buffer;

namespace Stroke.Tests.Layout.Controls;

/// <summary>
/// Tests for BufferControl mouse handler functionality.
/// </summary>
public sealed class BufferControlMouseTests
{
    #region Single Click Tests

    [Fact]
    public void MouseHandler_SingleClick_MovesCursor()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello World";
        buffer.CursorPosition = 0;
        var control = new BufferControl(buffer: buffer);

        var mouseEvent = new MouseEvent(
            new Point(5, 0),
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);

        var result = control.MouseHandler(mouseEvent);

        Assert.Equal(NotImplementedOrNone.None, result);
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void MouseHandler_ClickBeyondLineEnd_ClampsToCursor()
    {
        var buffer = new Buffer();
        buffer.Text = "Hi";
        buffer.CursorPosition = 0;
        var control = new BufferControl(buffer: buffer);

        var mouseEvent = new MouseEvent(
            new Point(10, 0), // Beyond "Hi"
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);

        control.MouseHandler(mouseEvent);

        // Should clamp to line length
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void MouseHandler_ClickOnSecondLine_MovesCursorToLine()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2";
        buffer.CursorPosition = 0;
        var control = new BufferControl(buffer: buffer);

        var mouseEvent = new MouseEvent(
            new Point(2, 1), // Line 2, column 2
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);

        control.MouseHandler(mouseEvent);

        var doc = buffer.Document;
        Assert.Equal(1, doc.CursorPositionRow);
        Assert.Equal(2, doc.CursorPositionCol);
    }

    [Fact]
    public void MouseHandler_ClickBeyondLastLine_ClampsToLastLine()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2";
        buffer.CursorPosition = 0;
        var control = new BufferControl(buffer: buffer);

        var mouseEvent = new MouseEvent(
            new Point(0, 10), // Way beyond last line
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);

        control.MouseHandler(mouseEvent);

        var doc = buffer.Document;
        Assert.Equal(1, doc.CursorPositionRow); // Last line
    }

    #endregion

    #region Double Click Tests

    [Fact]
    public void MouseHandler_DoubleClick_SelectsWord()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello World";
        buffer.CursorPosition = 0;
        var control = new BufferControl(buffer: buffer);

        // First click
        var click1 = new MouseEvent(
            new Point(2, 0), // In "Hello"
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);
        control.MouseHandler(click1);

        // Second click (double-click) - within 500ms
        var click2 = new MouseEvent(
            new Point(2, 0),
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);
        control.MouseHandler(click2);

        // Cursor should be at word start "Hello"
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void MouseHandler_DoubleClickOnWord_FindsWordBoundaries()
    {
        var buffer = new Buffer();
        buffer.Text = "one two three";
        buffer.CursorPosition = 0;
        var control = new BufferControl(buffer: buffer);

        // Double-click on "two"
        var click1 = new MouseEvent(
            new Point(5, 0), // In "two"
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);
        control.MouseHandler(click1);

        var click2 = new MouseEvent(
            new Point(5, 0),
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);
        control.MouseHandler(click2);

        // Cursor should be at start of "two"
        Assert.Equal(4, buffer.CursorPosition);
    }

    [Fact]
    public void MouseHandler_DoubleClickWithUnderscore_IncludesUnderscore()
    {
        var buffer = new Buffer();
        buffer.Text = "hello_world test";
        buffer.CursorPosition = 0;
        var control = new BufferControl(buffer: buffer);

        // Double-click on "hello_world"
        var click1 = new MouseEvent(
            new Point(7, 0), // In "hello_world"
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);
        control.MouseHandler(click1);

        var click2 = new MouseEvent(
            new Point(7, 0),
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);
        control.MouseHandler(click2);

        // Word with underscore should be selected as one unit
        Assert.Equal(0, buffer.CursorPosition); // Start of "hello_world"
    }

    #endregion

    #region Triple Click Tests

    [Fact]
    public void MouseHandler_TripleClick_SelectsLine()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1 with text\nLine 2";
        buffer.CursorPosition = 5;
        var control = new BufferControl(buffer: buffer);

        // Three rapid clicks at same position
        var position = new Point(8, 0);

        for (int i = 0; i < 3; i++)
        {
            var click = new MouseEvent(
                position,
                MouseEventType.MouseUp,
                MouseButton.Left,
                MouseModifiers.None);
            control.MouseHandler(click);
        }

        // After triple-click, cursor should be at line start
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void MouseHandler_TripleClickOnSecondLine_SelectsSecondLine()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2 content\nLine 3";
        buffer.CursorPosition = 0;
        var control = new BufferControl(buffer: buffer);

        // Triple-click on line 2
        var position = new Point(5, 1);

        for (int i = 0; i < 3; i++)
        {
            var click = new MouseEvent(
                position,
                MouseEventType.MouseUp,
                MouseButton.Left,
                MouseModifiers.None);
            control.MouseHandler(click);
        }

        // Cursor should be at start of line 2
        Assert.Equal(7, buffer.CursorPosition); // "Line 1\n" = 7
    }

    #endregion

    #region Click Timing Tests

    [Fact]
    public void MouseHandler_SlowDoubleClick_TreatedAsTwoSingleClicks()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello World";
        buffer.CursorPosition = 0;
        var control = new BufferControl(buffer: buffer);

        // First click
        var click1 = new MouseEvent(
            new Point(2, 0),
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);
        control.MouseHandler(click1);
        Assert.Equal(2, buffer.CursorPosition);

        // Wait longer than click timeout (simulated by clicking at different position)
        // In real scenario, this would be >500ms apart
        // Here we just click at a different position which resets the count
        var click2 = new MouseEvent(
            new Point(8, 0), // Different position
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);
        control.MouseHandler(click2);

        // Should just move cursor, not select word
        Assert.Equal(8, buffer.CursorPosition);
    }

    [Fact]
    public void MouseHandler_ClickAtDifferentPosition_ResetClickCount()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello World";
        buffer.CursorPosition = 0;
        var control = new BufferControl(buffer: buffer);

        // Click at position A
        var click1 = new MouseEvent(
            new Point(2, 0),
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);
        control.MouseHandler(click1);

        // Click at different position B - not a double-click
        var click2 = new MouseEvent(
            new Point(8, 0),
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);
        control.MouseHandler(click2);

        // Click at B again - this is first click at B, not double-click
        var click3 = new MouseEvent(
            new Point(8, 0),
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);
        control.MouseHandler(click3);

        // Now should be double-click at B
        Assert.Equal(6, buffer.CursorPosition); // Start of "World"
    }

    #endregion

    #region Mouse Event Type Tests

    [Fact]
    public void MouseHandler_MouseDown_NotHandled()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello";
        buffer.CursorPosition = 0;
        var control = new BufferControl(buffer: buffer);

        var mouseEvent = new MouseEvent(
            new Point(3, 0),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None);

        var result = control.MouseHandler(mouseEvent);

        Assert.Equal(NotImplementedOrNone.NotImplemented, result);
        Assert.Equal(0, buffer.CursorPosition); // Unchanged
    }

    [Fact]
    public void MouseHandler_MouseMove_NotHandled()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello";
        buffer.CursorPosition = 0;
        var control = new BufferControl(buffer: buffer);

        var mouseEvent = new MouseEvent(
            new Point(3, 0),
            MouseEventType.MouseMove,
            MouseButton.Left,
            MouseModifiers.None);

        var result = control.MouseHandler(mouseEvent);

        Assert.Equal(NotImplementedOrNone.NotImplemented, result);
    }

    [Fact]
    public void MouseHandler_ScrollUp_NotHandled()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello";
        var control = new BufferControl(buffer: buffer);

        var mouseEvent = new MouseEvent(
            new Point(0, 0),
            MouseEventType.ScrollUp,
            MouseButton.None,
            MouseModifiers.None);

        var result = control.MouseHandler(mouseEvent);

        Assert.Equal(NotImplementedOrNone.NotImplemented, result);
    }

    [Fact]
    public void MouseHandler_ScrollDown_NotHandled()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello";
        var control = new BufferControl(buffer: buffer);

        var mouseEvent = new MouseEvent(
            new Point(0, 0),
            MouseEventType.ScrollDown,
            MouseButton.None,
            MouseModifiers.None);

        var result = control.MouseHandler(mouseEvent);

        Assert.Equal(NotImplementedOrNone.NotImplemented, result);
    }

    #endregion

    #region Empty Buffer Tests

    [Fact]
    public void MouseHandler_EmptyBuffer_HandlesClick()
    {
        var buffer = new Buffer();
        buffer.Text = "";
        var control = new BufferControl(buffer: buffer);

        var mouseEvent = new MouseEvent(
            new Point(5, 0),
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);

        var result = control.MouseHandler(mouseEvent);

        Assert.Equal(NotImplementedOrNone.None, result);
        Assert.Equal(0, buffer.CursorPosition);
    }

    #endregion
}
