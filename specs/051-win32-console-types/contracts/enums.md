# API Contract: Win32 Console Enums

**Feature**: 051-win32-console-types
**Namespace**: `Stroke.Input.Windows.Win32Types`
**Date**: 2026-02-02

## EventType

```csharp
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
```

---

## ControlKeyState

```csharp
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
```

---

## MouseEventFlags

```csharp
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
```

---

## MouseButtonState

```csharp
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
```

---

## ConsoleInputMode

```csharp
/// <summary>
/// Specifies console input mode flags for SetConsoleMode.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows console input mode flags.
/// Multiple flags can be combined.
/// </para>
/// <para>
/// Note: This enum is not present in Python Prompt Toolkit's win32_types.py
/// but is needed for console mode control.
/// </para>
/// </remarks>
[Flags]
public enum ConsoleInputMode : uint
{
    /// <summary>No input mode flags.</summary>
    None = 0x0000,

    /// <summary>
    /// Ctrl+C is processed by the system and not placed in the input buffer.
    /// </summary>
    EnableProcessedInput = 0x0001,

    /// <summary>
    /// ReadFile or ReadConsole returns when a carriage return is received.
    /// </summary>
    EnableLineInput = 0x0002,

    /// <summary>
    /// Characters read are written to the active screen buffer as they are read.
    /// </summary>
    EnableEchoInput = 0x0004,

    /// <summary>
    /// Window resize events are placed in the input buffer.
    /// </summary>
    EnableWindowInput = 0x0008,

    /// <summary>
    /// Mouse events are placed in the input buffer.
    /// </summary>
    EnableMouseInput = 0x0010,

    /// <summary>
    /// Insert mode is enabled.
    /// </summary>
    EnableInsertMode = 0x0020,

    /// <summary>
    /// Quick edit mode is enabled (select text with mouse).
    /// </summary>
    EnableQuickEditMode = 0x0040,

    /// <summary>
    /// Required when setting ENABLE_QUICK_EDIT_MODE.
    /// </summary>
    EnableExtendedFlags = 0x0080,

    /// <summary>
    /// VT100 escape sequences are enabled for input.
    /// </summary>
    EnableVirtualTerminalInput = 0x0200
}
```

---

## ConsoleOutputMode

```csharp
/// <summary>
/// Specifies console output mode flags for SetConsoleMode.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows console output mode flags.
/// Multiple flags can be combined.
/// </para>
/// <para>
/// Note: This enum is not present in Python Prompt Toolkit's win32_types.py
/// but is needed for console mode control.
/// </para>
/// </remarks>
[Flags]
public enum ConsoleOutputMode : uint
{
    /// <summary>No output mode flags.</summary>
    None = 0x0000,

    /// <summary>
    /// Characters written are parsed for ASCII control sequences.
    /// </summary>
    EnableProcessedOutput = 0x0001,

    /// <summary>
    /// Cursor moves to the beginning of the next line when reaching end of line.
    /// </summary>
    EnableWrapAtEolOutput = 0x0002,

    /// <summary>
    /// VT100 escape sequences are processed for output.
    /// </summary>
    EnableVirtualTerminalProcessing = 0x0004,

    /// <summary>
    /// When writing with LF, cursor moves down without returning to column 0.
    /// </summary>
    DisableNewlineAutoReturn = 0x0008,

    /// <summary>
    /// Enables grid attribute support for worldwide character sets.
    /// </summary>
    EnableLvbGridWorldwide = 0x0010
}
```
