namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Specifies the state of control keys and toggle keys.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows dwControlKeyState field values. Multiple flags can be combined.
/// </para>
/// </remarks>
[Flags]
public enum ControlKeyState : uint
{
    /// <summary>No control keys pressed.</summary>
    None = 0x0000,

    /// <summary>Right Alt key is pressed.</summary>
    RightAltPressed = 0x0001,

    /// <summary>Left Alt key is pressed.</summary>
    LeftAltPressed = 0x0002,

    /// <summary>Right Ctrl key is pressed.</summary>
    RightCtrlPressed = 0x0004,

    /// <summary>Left Ctrl key is pressed.</summary>
    LeftCtrlPressed = 0x0008,

    /// <summary>Shift key is pressed.</summary>
    ShiftPressed = 0x0010,

    /// <summary>NumLock is toggled on.</summary>
    NumLockOn = 0x0020,

    /// <summary>ScrollLock is toggled on.</summary>
    ScrollLockOn = 0x0040,

    /// <summary>CapsLock is toggled on.</summary>
    CapsLockOn = 0x0080,

    /// <summary>The key is an enhanced key (extended keyboard).</summary>
    EnhancedKey = 0x0100
}
