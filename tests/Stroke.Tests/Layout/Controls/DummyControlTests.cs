using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;
using Xunit;

namespace Stroke.Tests.Layout.Controls;

/// <summary>
/// Tests for DummyControl class.
/// </summary>
public sealed class DummyControlTests
{
    [Fact]
    public void IsFocusable_ReturnsFalse()
    {
        var control = new DummyControl();

        Assert.False(control.IsFocusable);
    }

    [Fact]
    public void CreateContent_ReturnsValidUIContent()
    {
        var control = new DummyControl();

        var content = control.CreateContent(80, 24);

        Assert.NotNull(content);
        Assert.Equal(1, content.LineCount);
    }

    [Fact]
    public void CreateContent_SameContentForDifferentSizes()
    {
        var control = new DummyControl();

        var content1 = control.CreateContent(80, 24);
        var content2 = control.CreateContent(40, 10);

        // DummyControl returns static content
        Assert.Equal(content1.LineCount, content2.LineCount);
    }

    [Fact]
    public void Reset_DoesNothing()
    {
        var control = new DummyControl();

        // Should not throw
        control.Reset();
    }

    [Fact]
    public void PreferredWidth_ReturnsNull()
    {
        var control = new DummyControl();

        var result = control.PreferredWidth(80);

        Assert.Null(result);
    }

    [Fact]
    public void PreferredHeight_ReturnsNull()
    {
        var control = new DummyControl();

        var result = control.PreferredHeight(80, 24, false, null);

        Assert.Null(result);
    }

    [Fact]
    public void MouseHandler_ReturnsNotImplemented()
    {
        var control = new DummyControl();
        var mouseEvent = new MouseEvent(
            new Stroke.Core.Primitives.Point(0, 0),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None);

        var result = control.MouseHandler(mouseEvent);

        Assert.Same(NotImplementedOrNone.NotImplemented, result);
    }

    [Fact]
    public void MoveCursorDown_DoesNothing()
    {
        var control = new DummyControl();

        // Should not throw
        control.MoveCursorDown();
    }

    [Fact]
    public void MoveCursorUp_DoesNothing()
    {
        var control = new DummyControl();

        // Should not throw
        control.MoveCursorUp();
    }

    [Fact]
    public void GetKeyBindings_ReturnsNull()
    {
        var control = new DummyControl();

        var result = control.GetKeyBindings();

        Assert.Null(result);
    }

    [Fact]
    public void GetInvalidateEvents_ReturnsEmpty()
    {
        var control = new DummyControl();

        var result = control.GetInvalidateEvents();

        Assert.Empty(result);
    }
}
