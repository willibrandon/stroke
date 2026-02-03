namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Specifies which mouse buttons are pressed.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows MOUSE_EVENT_RECORD dwButtonState field values.
/// Multiple buttons can be pressed simultaneously.
/// </para>
/// </remarks>
[Flags]
public enum MouseButtonState : uint
{
    /// <summary>No buttons pressed.</summary>
    None = 0x0000,

    /// <summary>Left mouse button (primary button).</summary>
    FromLeft1stButtonPressed = 0x0001,

    /// <summary>Right mouse button.</summary>
    RightmostButtonPressed = 0x0002,

    /// <summary>Middle mouse button (button 2).</summary>
    FromLeft2ndButtonPressed = 0x0004,

    /// <summary>X1 button (button 3).</summary>
    FromLeft3rdButtonPressed = 0x0008,

    /// <summary>X2 button (button 4).</summary>
    FromLeft4thButtonPressed = 0x0010
}
