namespace Stroke.Input;

/// <summary>
/// Type of mouse event.
/// </summary>
/// <remarks>
/// Equivalent to Python Prompt Toolkit's <c>MouseEventType</c> enum in <c>prompt_toolkit.mouse_events</c>.
/// </remarks>
public enum MouseEventType
{
    /// <summary>
    /// Mouse button released. Fired for all buttons (left, right, middle).
    /// </summary>
    MouseUp,

    /// <summary>
    /// Left mouse button pressed. Not fired for middle or right buttons.
    /// </summary>
    MouseDown,

    /// <summary>
    /// Scroll wheel up.
    /// </summary>
    ScrollUp,

    /// <summary>
    /// Scroll wheel down.
    /// </summary>
    ScrollDown,

    /// <summary>
    /// Mouse moved while left button is held down.
    /// </summary>
    MouseMove
}
