using System.Runtime.InteropServices;

namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Describes a mouse input event in a console input record.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows MOUSE_EVENT_RECORD structure.
/// </para>
/// <para>
/// Thread safety: Immutable value type; inherently thread-safe.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly struct MouseEventRecord
{
    /// <summary>
    /// Mouse position in screen buffer coordinates.
    /// </summary>
    public readonly Coord MousePosition;

    /// <summary>
    /// State of mouse buttons.
    /// </summary>
    public readonly MouseButtonState ButtonState;

    /// <summary>
    /// State of control keys.
    /// </summary>
    public readonly ControlKeyState ControlKeyState;

    /// <summary>
    /// Type of mouse event.
    /// </summary>
    public readonly MouseEventFlags EventFlags;
}
