namespace Stroke.Input;

/// <summary>
/// Modifier keys held during a mouse event.
/// </summary>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>MouseModifier</c> in <c>prompt_toolkit.mouse_events</c>.
/// </para>
/// <para>
/// The plural name <c>MouseModifiers</c> indicates this type represents a combination of modifiers
/// (flags), not a single value. This follows .NET conventions (e.g., <c>System.IO.FileAttributes</c>).
/// Python uses <c>frozenset[MouseModifier]</c> for modifier combinations; this <c>[Flags]</c> enum
/// is a C# language adaptation that is more efficient (no allocation) and supports natural bitwise operations.
/// </para>
/// </remarks>
[Flags]
public enum MouseModifiers
{
    /// <summary>
    /// No modifier keys held.
    /// </summary>
    None = 0,

    /// <summary>
    /// Shift key held.
    /// </summary>
    Shift = 1,

    /// <summary>
    /// Alt key held.
    /// </summary>
    Alt = 2,

    /// <summary>
    /// Control key held.
    /// </summary>
    Control = 4
}
