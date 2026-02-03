namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Specifies the type of mouse event.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows MOUSE_EVENT_RECORD dwEventFlags field values.
/// </para>
/// </remarks>
[Flags]
public enum MouseEventFlags : uint
{
    /// <summary>A mouse button was pressed or released.</summary>
    None = 0x0000,

    /// <summary>The mouse position changed.</summary>
    MouseMoved = 0x0001,

    /// <summary>A mouse button was double-clicked.</summary>
    DoubleClick = 0x0002,

    /// <summary>The vertical scroll wheel was rotated.</summary>
    MouseWheeled = 0x0004,

    /// <summary>The horizontal scroll wheel was rotated.</summary>
    MouseHWheeled = 0x0008
}
