using Stroke.Core.Primitives;

namespace Stroke.Input;

/// <summary>
/// Mouse event, sent to UIControl.MouseHandler.
/// </summary>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>MouseEvent</c> class in <c>prompt_toolkit.mouse_events</c>.
/// </para>
/// <para>
/// This is an immutable value type (record struct) capturing a complete mouse event with position,
/// type, button, and modifiers. Stack allocation avoids heap pressure for frequent events.
/// </para>
/// </remarks>
/// <param name="Position">The position in the terminal (X=column, Y=row).</param>
/// <param name="EventType">The type of mouse event.</param>
/// <param name="Button">The mouse button.</param>
/// <param name="Modifiers">The modifier keys held.</param>
public readonly record struct MouseEvent(
    Point Position,
    MouseEventType EventType,
    MouseButton Button,
    MouseModifiers Modifiers)
{
    /// <summary>
    /// Returns a string representation of the mouse event.
    /// </summary>
    /// <remarks>
    /// Format matches Python Prompt Toolkit's <c>MouseEvent.__repr__</c> output:
    /// <c>MouseEvent({Position}, {EventType}, {Button}, {Modifiers})</c>
    /// </remarks>
    /// <returns>A string representation of this mouse event.</returns>
    public override string ToString() =>
        $"MouseEvent({Position}, {EventType}, {Button}, {Modifiers})";
}
