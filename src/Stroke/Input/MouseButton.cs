namespace Stroke.Input;

/// <summary>
/// Mouse button that was pressed.
/// </summary>
/// <remarks>
/// Equivalent to Python Prompt Toolkit's <c>MouseButton</c> enum in <c>prompt_toolkit.mouse_events</c>.
/// </remarks>
public enum MouseButton
{
    /// <summary>
    /// Left mouse button.
    /// </summary>
    Left,

    /// <summary>
    /// Middle mouse button.
    /// </summary>
    Middle,

    /// <summary>
    /// Right mouse button.
    /// </summary>
    Right,

    /// <summary>
    /// No button pressed (scrolling or just moving).
    /// </summary>
    None,

    /// <summary>
    /// A button was pressed but we don't know which one.
    /// </summary>
    Unknown
}
