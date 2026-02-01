using Stroke.Core.Primitives;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;
using Stroke.Widgets.Toolbars;
using Xunit;

namespace Stroke.Tests.Widgets.Toolbars;

/// <summary>
/// Tests for CompletionsToolbarControl (internal UIControl that renders completions horizontally).
/// </summary>
/// <remarks>
/// <para>
/// CompletionsToolbarControl is internal but accessible via InternalsVisibleTo.
/// CreateContent requires AppContext.GetApp() which needs a running Application,
/// so only non-Application-dependent behavior is tested directly here.
/// </para>
/// </remarks>
public sealed class CompletionsToolbarControlTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_CreatesInstance()
    {
        var control = new CompletionsToolbarControl();

        Assert.NotNull(control);
    }

    #endregion

    #region IsFocusable Tests

    [Fact]
    public void IsFocusable_IsFalse()
    {
        var control = new CompletionsToolbarControl();

        Assert.False(control.IsFocusable);
    }

    [Fact]
    public void IsFocusable_AlwaysReturnsFalse()
    {
        // Verify it is consistently false across multiple accesses
        var control = new CompletionsToolbarControl();

        Assert.False(control.IsFocusable);
        Assert.False(control.IsFocusable);
    }

    #endregion

    #region IUIControl Implementation Tests

    [Fact]
    public void CompletionsToolbarControl_ImplementsIUIControl()
    {
        var control = new CompletionsToolbarControl();

        Assert.IsAssignableFrom<IUIControl>(control);
    }

    #endregion

    #region MouseHandler Tests

    [Fact]
    public void MouseHandler_ReturnsNotImplemented()
    {
        var control = new CompletionsToolbarControl();
        var mouseEvent = new MouseEvent(
            new Point(0, 0),
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);

        var result = control.MouseHandler(mouseEvent);

        Assert.Same(NotImplementedOrNone.NotImplemented, result);
    }

    [Fact]
    public void MouseHandler_WithMouseDown_ReturnsNotImplemented()
    {
        var control = new CompletionsToolbarControl();
        var mouseEvent = new MouseEvent(
            new Point(5, 3),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None);

        var result = control.MouseHandler(mouseEvent);

        Assert.Same(NotImplementedOrNone.NotImplemented, result);
    }

    [Fact]
    public void MouseHandler_WithScrollUp_ReturnsNotImplemented()
    {
        var control = new CompletionsToolbarControl();
        var mouseEvent = new MouseEvent(
            new Point(0, 0),
            MouseEventType.ScrollUp,
            MouseButton.None,
            MouseModifiers.None);

        var result = control.MouseHandler(mouseEvent);

        Assert.Same(NotImplementedOrNone.NotImplemented, result);
    }

    [Fact]
    public void MouseHandler_WithScrollDown_ReturnsNotImplemented()
    {
        var control = new CompletionsToolbarControl();
        var mouseEvent = new MouseEvent(
            new Point(0, 0),
            MouseEventType.ScrollDown,
            MouseButton.None,
            MouseModifiers.None);

        var result = control.MouseHandler(mouseEvent);

        Assert.Same(NotImplementedOrNone.NotImplemented, result);
    }

    [Fact]
    public void MouseHandler_DoesNotReturnNone()
    {
        var control = new CompletionsToolbarControl();
        var mouseEvent = new MouseEvent(
            new Point(0, 0),
            MouseEventType.MouseUp,
            MouseButton.Left,
            MouseModifiers.None);

        var result = control.MouseHandler(mouseEvent);

        Assert.NotSame(NotImplementedOrNone.None, result);
    }

    #endregion

    #region Multiple Instance Tests

    [Fact]
    public void MultipleInstances_AllHaveIsFocusableFalse()
    {
        var control1 = new CompletionsToolbarControl();
        var control2 = new CompletionsToolbarControl();

        Assert.False(control1.IsFocusable);
        Assert.False(control2.IsFocusable);
    }

    [Fact]
    public void MultipleInstances_AreDistinct()
    {
        var control1 = new CompletionsToolbarControl();
        var control2 = new CompletionsToolbarControl();

        Assert.NotSame(control1, control2);
    }

    #endregion
}
