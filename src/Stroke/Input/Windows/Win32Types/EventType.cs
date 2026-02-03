namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Specifies the type of input event in an <see cref="InputRecord"/>.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows INPUT_RECORD EventType field values.
/// </para>
/// </remarks>
public enum EventType : ushort
{
    /// <summary>Keyboard input event.</summary>
    KeyEvent = 0x0001,

    /// <summary>Mouse input event.</summary>
    MouseEvent = 0x0002,

    /// <summary>Console screen buffer resize event.</summary>
    WindowBufferSizeEvent = 0x0004,

    /// <summary>Menu event (reserved by Windows).</summary>
    MenuEvent = 0x0008,

    /// <summary>Focus change event (reserved by Windows).</summary>
    FocusEvent = 0x0010
}
