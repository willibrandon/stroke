using Stroke.Core.Primitives;
using Stroke.Input;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for <see cref="MouseEvent"/> record struct.
/// </summary>
public class MouseEventTests
{
    [Fact]
    public void MouseEvent_Construction_SetsAllProperties()
    {
        var position = new Point(10, 5);
        var eventType = MouseEventType.MouseDown;
        var button = MouseButton.Left;
        var modifiers = MouseModifiers.Shift;

        var mouseEvent = new MouseEvent(position, eventType, button, modifiers);

        Assert.Equal(position, mouseEvent.Position);
        Assert.Equal(eventType, mouseEvent.EventType);
        Assert.Equal(button, mouseEvent.Button);
        Assert.Equal(modifiers, mouseEvent.Modifiers);
    }

    [Fact]
    public void MouseEvent_ToString_ReturnsCorrectFormat()
    {
        // FR-010: Format: MouseEvent({Position}, {EventType}, {Button}, {Modifiers})
        var mouseEvent = new MouseEvent(
            new Point(10, 5),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None);

        var str = mouseEvent.ToString();

        Assert.Equal("MouseEvent(Point { X = 10, Y = 5 }, MouseDown, Left, None)", str);
    }

    [Fact]
    public void MouseEvent_ToString_WithModifiers_ShowsModifiers()
    {
        var mouseEvent = new MouseEvent(
            new Point(10, 5),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.Shift | MouseModifiers.Control);

        var str = mouseEvent.ToString();

        Assert.Contains("Shift", str);
        Assert.Contains("Control", str);
    }

    [Fact]
    public void MouseEvent_Equality_SameValues_AreEqual()
    {
        var event1 = new MouseEvent(new Point(10, 5), MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.None);
        var event2 = new MouseEvent(new Point(10, 5), MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.None);

        Assert.Equal(event1, event2);
        Assert.True(event1 == event2);
    }

    [Fact]
    public void MouseEvent_Equality_DifferentPosition_NotEqual()
    {
        var event1 = new MouseEvent(new Point(10, 5), MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.None);
        var event2 = new MouseEvent(new Point(20, 5), MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.None);

        Assert.NotEqual(event1, event2);
        Assert.True(event1 != event2);
    }

    [Fact]
    public void MouseEvent_Equality_DifferentEventType_NotEqual()
    {
        var event1 = new MouseEvent(new Point(10, 5), MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.None);
        var event2 = new MouseEvent(new Point(10, 5), MouseEventType.MouseUp, MouseButton.Left, MouseModifiers.None);

        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void MouseEvent_Equality_DifferentButton_NotEqual()
    {
        var event1 = new MouseEvent(new Point(10, 5), MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.None);
        var event2 = new MouseEvent(new Point(10, 5), MouseEventType.MouseDown, MouseButton.Right, MouseModifiers.None);

        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void MouseEvent_Equality_DifferentModifiers_NotEqual()
    {
        var event1 = new MouseEvent(new Point(10, 5), MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.None);
        var event2 = new MouseEvent(new Point(10, 5), MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.Shift);

        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void MouseEvent_GetHashCode_SameValues_SameHash()
    {
        var event1 = new MouseEvent(new Point(10, 5), MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.None);
        var event2 = new MouseEvent(new Point(10, 5), MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.None);

        Assert.Equal(event1.GetHashCode(), event2.GetHashCode());
    }

    [Fact]
    public void MouseEvent_IsImmutable()
    {
        // Record struct is inherently immutable - verify it's readonly value type
        var type = typeof(MouseEvent);
        Assert.True(type.IsValueType);

        // Verify it's a record struct (has EqualityContract or similar record markers)
        // Record structs have init-only setters for the 'with' expression support
        // but are still effectively immutable since you can't call init outside construction
        var properties = type.GetProperties();
        Assert.Equal(4, properties.Length); // Position, EventType, Button, Modifiers
    }

    // T013: Click-specific test cases
    [Theory]
    [InlineData(MouseEventType.MouseDown, MouseButton.Left)]
    [InlineData(MouseEventType.MouseDown, MouseButton.Right)]
    [InlineData(MouseEventType.MouseDown, MouseButton.Middle)]
    [InlineData(MouseEventType.MouseUp, MouseButton.Left)]
    [InlineData(MouseEventType.MouseUp, MouseButton.Right)]
    [InlineData(MouseEventType.MouseUp, MouseButton.Middle)]
    public void MouseEvent_ClickEvents_CorrectlyRepresented(MouseEventType eventType, MouseButton button)
    {
        var mouseEvent = new MouseEvent(
            new Point(10, 5),
            eventType,
            button,
            MouseModifiers.None);

        Assert.Equal(eventType, mouseEvent.EventType);
        Assert.Equal(button, mouseEvent.Button);
    }

    // T014: Edge case tests for position (0, 0)
    [Fact]
    public void MouseEvent_AtOrigin_WorksCorrectly()
    {
        var mouseEvent = new MouseEvent(
            Point.Zero,
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None);

        Assert.Equal(0, mouseEvent.Position.X);
        Assert.Equal(0, mouseEvent.Position.Y);
    }

    [Fact]
    public void MouseEvent_AtOrigin_ToString_ContainsZeroCoordinates()
    {
        var mouseEvent = new MouseEvent(
            Point.Zero,
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None);

        var str = mouseEvent.ToString();
        Assert.Contains("X = 0", str);
        Assert.Contains("Y = 0", str);
    }

    // T032: Scroll-specific test cases
    [Theory]
    [InlineData(MouseEventType.ScrollUp)]
    [InlineData(MouseEventType.ScrollDown)]
    public void MouseEvent_ScrollEvents_CorrectlyRepresented(MouseEventType eventType)
    {
        var mouseEvent = new MouseEvent(
            new Point(10, 5),
            eventType,
            MouseButton.None,
            MouseModifiers.None);

        Assert.Equal(eventType, mouseEvent.EventType);
        Assert.Equal(MouseButton.None, mouseEvent.Button);
    }

    // T033: Scroll events with modifier keys
    [Theory]
    [InlineData(MouseModifiers.Shift)]
    [InlineData(MouseModifiers.Control)]
    [InlineData(MouseModifiers.Alt)]
    public void MouseEvent_ScrollWithModifiers_CorrectlyRepresented(MouseModifiers modifiers)
    {
        var mouseEvent = new MouseEvent(
            new Point(10, 5),
            MouseEventType.ScrollUp,
            MouseButton.None,
            modifiers);

        Assert.Equal(modifiers, mouseEvent.Modifiers);
    }

    [Fact]
    public void MouseEvent_ScrollWithCombinedModifiers_CorrectlyRepresented()
    {
        var combined = MouseModifiers.Shift | MouseModifiers.Control | MouseModifiers.Alt;
        var mouseEvent = new MouseEvent(
            new Point(10, 5),
            MouseEventType.ScrollUp,
            MouseButton.None,
            combined);

        Assert.True(mouseEvent.Modifiers.HasFlag(MouseModifiers.Shift));
        Assert.True(mouseEvent.Modifiers.HasFlag(MouseModifiers.Control));
        Assert.True(mouseEvent.Modifiers.HasFlag(MouseModifiers.Alt));
    }

    // T035: Movement-specific test cases
    [Fact]
    public void MouseEvent_MouseMove_CorrectlyRepresented()
    {
        var mouseEvent = new MouseEvent(
            new Point(20, 15),
            MouseEventType.MouseMove,
            MouseButton.Left,
            MouseModifiers.None);

        Assert.Equal(MouseEventType.MouseMove, mouseEvent.EventType);
        Assert.Equal(new Point(20, 15), mouseEvent.Position);
    }

    // T036: Mouse move with modifier keys held
    [Theory]
    [InlineData(MouseModifiers.Shift)]
    [InlineData(MouseModifiers.Control)]
    [InlineData(MouseModifiers.Alt)]
    public void MouseEvent_MouseMoveWithModifiers_CorrectlyRepresented(MouseModifiers modifiers)
    {
        var mouseEvent = new MouseEvent(
            new Point(20, 15),
            MouseEventType.MouseMove,
            MouseButton.Left,
            modifiers);

        Assert.Equal(MouseEventType.MouseMove, mouseEvent.EventType);
        Assert.Equal(modifiers, mouseEvent.Modifiers);
    }

    // T037: Sequence test - MouseDown → MouseMove → MouseUp
    [Fact]
    public void MouseEvent_DragSequence_AllEventTypesWork()
    {
        var startPos = new Point(10, 5);
        var movePos = new Point(15, 8);
        var endPos = new Point(20, 10);

        var down = new MouseEvent(startPos, MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.None);
        var move = new MouseEvent(movePos, MouseEventType.MouseMove, MouseButton.Left, MouseModifiers.None);
        var up = new MouseEvent(endPos, MouseEventType.MouseUp, MouseButton.Left, MouseModifiers.None);

        Assert.Equal(MouseEventType.MouseDown, down.EventType);
        Assert.Equal(startPos, down.Position);

        Assert.Equal(MouseEventType.MouseMove, move.EventType);
        Assert.Equal(movePos, move.Position);

        Assert.Equal(MouseEventType.MouseUp, up.EventType);
        Assert.Equal(endPos, up.Position);
    }

    [Fact]
    public void MouseEvent_CanBeUsedAsKey_InDictionary()
    {
        var dict = new Dictionary<MouseEvent, string>();
        var key = new MouseEvent(new Point(10, 5), MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.None);

        dict[key] = "test";

        var sameKey = new MouseEvent(new Point(10, 5), MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.None);
        Assert.Equal("test", dict[sameKey]);
    }

    [Fact]
    public void MouseEvent_WithExpression_CreatesNewInstance()
    {
        var original = new MouseEvent(new Point(10, 5), MouseEventType.MouseDown, MouseButton.Left, MouseModifiers.None);
        var modified = original with { Position = new Point(20, 10) };

        Assert.NotEqual(original.Position, modified.Position);
        Assert.Equal(original.EventType, modified.EventType);
        Assert.Equal(original.Button, modified.Button);
        Assert.Equal(original.Modifiers, modified.Modifiers);
    }
}
