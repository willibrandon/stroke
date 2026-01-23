# Feature 107: Win32 Console Types

## Overview

Implement Win32 console API structures and constants for Windows console input/output operations. These types map directly to the Windows Console API structures.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/win32_types.py`

## Public API

### Standard Handle Constants

```csharp
namespace Stroke.Interop.Win32;

/// <summary>
/// Standard handle constants for GetStdHandle.
/// </summary>
public static class StdHandles
{
    /// <summary>
    /// Standard input handle.
    /// </summary>
    public const int STD_INPUT_HANDLE = -10;

    /// <summary>
    /// Standard output handle.
    /// </summary>
    public const int STD_OUTPUT_HANDLE = -11;

    /// <summary>
    /// Standard error handle.
    /// </summary>
    public const int STD_ERROR_HANDLE = -12;
}
```

### COORD Structure

```csharp
namespace Stroke.Interop.Win32;

/// <summary>
/// Represents a coordinate in the console screen buffer.
/// </summary>
/// <remarks>
/// See: https://docs.microsoft.com/en-us/windows/console/coord-str
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct COORD
{
    /// <summary>
    /// X coordinate (column).
    /// </summary>
    public short X;

    /// <summary>
    /// Y coordinate (row).
    /// </summary>
    public short Y;

    public COORD(short x, short y)
    {
        X = x;
        Y = y;
    }
}
```

### SMALL_RECT Structure

```csharp
namespace Stroke.Interop.Win32;

/// <summary>
/// Represents a rectangle in the console screen buffer.
/// </summary>
/// <remarks>
/// See: https://docs.microsoft.com/en-us/windows/console/small-rect-str
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct SMALL_RECT
{
    public short Left;
    public short Top;
    public short Right;
    public short Bottom;
}
```

### CONSOLE_SCREEN_BUFFER_INFO Structure

```csharp
namespace Stroke.Interop.Win32;

/// <summary>
/// Contains information about a console screen buffer.
/// </summary>
/// <remarks>
/// See: https://docs.microsoft.com/en-us/windows/console/console-screen-buffer-info-str
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct CONSOLE_SCREEN_BUFFER_INFO
{
    /// <summary>
    /// Size of the screen buffer in character columns and rows.
    /// </summary>
    public COORD dwSize;

    /// <summary>
    /// Position of the cursor in the screen buffer.
    /// </summary>
    public COORD dwCursorPosition;

    /// <summary>
    /// Character attributes (foreground/background colors).
    /// </summary>
    public ushort wAttributes;

    /// <summary>
    /// Coordinates of the upper-left and lower-right corners of the
    /// display window within the screen buffer.
    /// </summary>
    public SMALL_RECT srWindow;

    /// <summary>
    /// Maximum size of the console window.
    /// </summary>
    public COORD dwMaximumWindowSize;
}
```

### KEY_EVENT_RECORD Structure

```csharp
namespace Stroke.Interop.Win32;

/// <summary>
/// Describes a keyboard input event.
/// </summary>
/// <remarks>
/// See: https://docs.microsoft.com/en-us/windows/console/key-event-record-str
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct KEY_EVENT_RECORD
{
    /// <summary>
    /// True for key down, false for key up.
    /// </summary>
    public int KeyDown;

    /// <summary>
    /// Number of times the key was repeated.
    /// </summary>
    public ushort RepeatCount;

    /// <summary>
    /// Virtual key code.
    /// </summary>
    public ushort VirtualKeyCode;

    /// <summary>
    /// Virtual scan code.
    /// </summary>
    public ushort VirtualScanCode;

    /// <summary>
    /// Character value (union with AsciiChar).
    /// </summary>
    public char UnicodeChar;

    /// <summary>
    /// Control key state (modifiers).
    /// </summary>
    public uint ControlKeyState;
}
```

### MOUSE_EVENT_RECORD Structure

```csharp
namespace Stroke.Interop.Win32;

/// <summary>
/// Describes a mouse input event.
/// </summary>
/// <remarks>
/// See: https://docs.microsoft.com/en-us/windows/console/mouse-event-record-str
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct MOUSE_EVENT_RECORD
{
    /// <summary>
    /// Position of the mouse pointer in character cell coordinates.
    /// </summary>
    public COORD MousePosition;

    /// <summary>
    /// State of the mouse buttons.
    /// </summary>
    public uint ButtonState;

    /// <summary>
    /// Control key state (modifiers).
    /// </summary>
    public uint ControlKeyState;

    /// <summary>
    /// Type of mouse event.
    /// </summary>
    public uint EventFlags;
}
```

### WINDOW_BUFFER_SIZE_RECORD Structure

```csharp
namespace Stroke.Interop.Win32;

/// <summary>
/// Describes a console screen buffer resize event.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct WINDOW_BUFFER_SIZE_RECORD
{
    /// <summary>
    /// New size of the screen buffer.
    /// </summary>
    public COORD Size;
}
```

### INPUT_RECORD Structure

```csharp
namespace Stroke.Interop.Win32;

/// <summary>
/// Event types for INPUT_RECORD.
/// </summary>
public enum EventType : ushort
{
    KeyEvent = 1,
    MouseEvent = 2,
    WindowBufferSizeEvent = 4,
    MenuEvent = 8,
    FocusEvent = 16
}

/// <summary>
/// Describes an input event in the console input buffer.
/// </summary>
/// <remarks>
/// See: https://docs.microsoft.com/en-us/windows/console/input-record-str
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public struct INPUT_RECORD
{
    /// <summary>
    /// Type of input event.
    /// </summary>
    [FieldOffset(0)]
    public EventType EventType;

    /// <summary>
    /// Key event data.
    /// </summary>
    [FieldOffset(4)]
    public KEY_EVENT_RECORD KeyEvent;

    /// <summary>
    /// Mouse event data.
    /// </summary>
    [FieldOffset(4)]
    public MOUSE_EVENT_RECORD MouseEvent;

    /// <summary>
    /// Window resize event data.
    /// </summary>
    [FieldOffset(4)]
    public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
}
```

### Control Key State Flags

```csharp
namespace Stroke.Interop.Win32;

/// <summary>
/// Control key state flags for keyboard and mouse events.
/// </summary>
[Flags]
public enum ControlKeyState : uint
{
    None = 0,
    RightAltPressed = 0x0001,
    LeftAltPressed = 0x0002,
    RightCtrlPressed = 0x0004,
    LeftCtrlPressed = 0x0008,
    ShiftPressed = 0x0010,
    NumLockOn = 0x0020,
    ScrollLockOn = 0x0040,
    CapsLockOn = 0x0080,
    EnhancedKey = 0x0100
}
```

### Mouse Event Flags

```csharp
namespace Stroke.Interop.Win32;

/// <summary>
/// Mouse event flags.
/// </summary>
[Flags]
public enum MouseEventFlags : uint
{
    None = 0,
    MouseMoved = 0x0001,
    DoubleClick = 0x0002,
    MouseWheeled = 0x0004,
    MouseHwheeled = 0x0008
}

/// <summary>
/// Mouse button state flags.
/// </summary>
[Flags]
public enum MouseButtonState : uint
{
    None = 0,
    LeftButtonPressed = 0x0001,
    RightButtonPressed = 0x0002,
    FromLeft2ndButtonPressed = 0x0004,
    FromLeft3rdButtonPressed = 0x0008,
    FromLeft4thButtonPressed = 0x0010
}
```

### SECURITY_ATTRIBUTES Structure

```csharp
namespace Stroke.Interop.Win32;

/// <summary>
/// Security attributes for handles.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SECURITY_ATTRIBUTES
{
    public uint nLength;
    public nint lpSecurityDescriptor;
    public bool bInheritHandle;
}
```

## Project Structure

```
src/Stroke/
└── Interop/
    └── Win32/
        ├── StdHandles.cs
        ├── COORD.cs
        ├── SMALL_RECT.cs
        ├── CONSOLE_SCREEN_BUFFER_INFO.cs
        ├── KEY_EVENT_RECORD.cs
        ├── MOUSE_EVENT_RECORD.cs
        ├── WINDOW_BUFFER_SIZE_RECORD.cs
        ├── INPUT_RECORD.cs
        ├── ControlKeyState.cs
        ├── MouseEventFlags.cs
        └── SECURITY_ATTRIBUTES.cs
tests/Stroke.Tests/
└── Interop/
    └── Win32/
        └── Win32TypesTests.cs
```

## Implementation Notes

### P/Invoke Declarations

```csharp
namespace Stroke.Interop.Win32;

[SupportedOSPlatform("windows")]
internal static class NativeMethods
{
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern nint GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetConsoleScreenBufferInfo(
        nint hConsoleOutput,
        out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadConsoleInput(
        nint hConsoleInput,
        [Out] INPUT_RECORD[] lpBuffer,
        uint nLength,
        out uint lpNumberOfEventsRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteConsoleOutput(
        nint hConsoleOutput,
        CHAR_INFO[] lpBuffer,
        COORD dwBufferSize,
        COORD dwBufferCoord,
        ref SMALL_RECT lpWriteRegion);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetConsoleCursorPosition(
        nint hConsoleOutput,
        COORD dwCursorPosition);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern nint CreateEvent(
        nint lpEventAttributes,
        bool bManualReset,
        bool bInitialState,
        string? lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetEvent(nint hEvent);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ResetEvent(nint hEvent);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(nint hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint WaitForMultipleObjects(
        uint nCount,
        nint[] lpHandles,
        bool bWaitAll,
        uint dwMilliseconds);
}
```

### Console Mode Flags

```csharp
namespace Stroke.Interop.Win32;

/// <summary>
/// Console input mode flags.
/// </summary>
[Flags]
public enum ConsoleInputMode : uint
{
    None = 0,
    EnableProcessedInput = 0x0001,
    EnableLineInput = 0x0002,
    EnableEchoInput = 0x0004,
    EnableWindowInput = 0x0008,
    EnableMouseInput = 0x0010,
    EnableInsertMode = 0x0020,
    EnableQuickEditMode = 0x0040,
    EnableExtendedFlags = 0x0080,
    EnableAutoPosition = 0x0100,
    EnableVirtualTerminalInput = 0x0200
}

/// <summary>
/// Console output mode flags.
/// </summary>
[Flags]
public enum ConsoleOutputMode : uint
{
    None = 0,
    EnableProcessedOutput = 0x0001,
    EnableWrapAtEolOutput = 0x0002,
    EnableVirtualTerminalProcessing = 0x0004,
    DisableNewlineAutoReturn = 0x0008,
    EnableLvbGridWorldwide = 0x0010
}
```

## Dependencies

- Windows SDK (kernel32.dll)

## Implementation Tasks

1. Define COORD struct with LayoutKind.Sequential
2. Define SMALL_RECT struct
3. Define CONSOLE_SCREEN_BUFFER_INFO struct
4. Define KEY_EVENT_RECORD struct
5. Define MOUSE_EVENT_RECORD struct
6. Define INPUT_RECORD union struct
7. Define control key state flags enum
8. Define mouse event flags enums
9. Add P/Invoke declarations
10. Write unit tests

## Acceptance Criteria

- [ ] All structures match Windows Console API definitions
- [ ] LayoutKind attributes correctly specified
- [ ] Union types use FieldOffset correctly
- [ ] P/Invoke declarations work on Windows
- [ ] Flags enums have correct values
- [ ] SupportedOSPlatform attributes present
- [ ] Unit tests verify structure sizes
- [ ] Unit tests verify P/Invoke calls (on Windows)
