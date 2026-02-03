# Data Model: Win32 Console Types

**Feature**: 051-win32-console-types
**Date**: 2026-02-02
**Status**: Complete

## Overview

This feature defines C# struct types that map directly to Windows Console API native structures. All types use explicit memory layout to ensure byte-for-byte compatibility with the Windows API for correct P/Invoke marshalling.

## Entities

### Structs (P/Invoke Interop)

#### COORD

| Field | Type | Size | Offset | Description |
|-------|------|------|--------|-------------|
| X | short | 2 | 0 | Column position (0-based) |
| Y | short | 2 | 2 | Row position (0-based) |

**Total Size**: 4 bytes
**Layout**: Sequential
**Relationships**: Used by CONSOLE_SCREEN_BUFFER_INFO, MOUSE_EVENT_RECORD, WINDOW_BUFFER_SIZE_RECORD, P/Invoke SetConsoleCursorPosition

---

#### SMALL_RECT

| Field | Type | Size | Offset | Description |
|-------|------|------|--------|-------------|
| Left | short | 2 | 0 | Left edge (inclusive) |
| Top | short | 2 | 2 | Top edge (inclusive) |
| Right | short | 2 | 4 | Right edge (inclusive) |
| Bottom | short | 2 | 6 | Bottom edge (inclusive) |

**Total Size**: 8 bytes
**Layout**: Sequential
**Relationships**: Used by CONSOLE_SCREEN_BUFFER_INFO, WriteConsoleOutput

---

#### KEY_EVENT_RECORD

| Field | Type | Size | Offset | Description |
|-------|------|------|--------|-------------|
| KeyDown | int | 4 | 0 | Non-zero if key pressed, zero if released |
| RepeatCount | ushort | 2 | 4 | Number of key repeats |
| VirtualKeyCode | ushort | 2 | 6 | Virtual-key code (Windows VK_*) |
| VirtualScanCode | ushort | 2 | 8 | Hardware scan code |
| UnicodeChar | char | 2 | 10 | Unicode character (replaces ANSI union) |
| ControlKeyState | uint | 4 | 12 | Modifier keys state (ControlKeyState flags) |

**Total Size**: 16 bytes
**Layout**: Sequential
**Relationships**: Part of INPUT_RECORD union; ControlKeyState links to ControlKeyState enum
**Validation**: KeyDown is BOOL (4-byte in Win32 API)

---

#### MOUSE_EVENT_RECORD

| Field | Type | Size | Offset | Description |
|-------|------|------|--------|-------------|
| MousePosition | COORD | 4 | 0 | Cursor position in screen buffer coordinates |
| ButtonState | uint | 4 | 4 | Button state (MouseButtonState flags) |
| ControlKeyState | uint | 4 | 8 | Modifier keys state (ControlKeyState flags) |
| EventFlags | uint | 4 | 12 | Event type (MouseEventFlags flags) |

**Total Size**: 16 bytes
**Layout**: Sequential
**Relationships**: Part of INPUT_RECORD union; references COORD, ControlKeyState, MouseEventFlags, MouseButtonState

---

#### WINDOW_BUFFER_SIZE_RECORD

| Field | Type | Size | Offset | Description |
|-------|------|------|--------|-------------|
| Size | COORD | 4 | 0 | New screen buffer size |

**Total Size**: 4 bytes
**Layout**: Sequential
**Relationships**: Part of INPUT_RECORD union; references COORD

---

#### MENU_EVENT_RECORD

| Field | Type | Size | Offset | Description |
|-------|------|------|--------|-------------|
| CommandId | uint | 4 | 0 | Menu command identifier |

**Total Size**: 4 bytes
**Layout**: Sequential
**Relationships**: Part of INPUT_RECORD union
**Notes**: Reserved by Windows; applications should ignore

---

#### FOCUS_EVENT_RECORD

| Field | Type | Size | Offset | Description |
|-------|------|------|--------|-------------|
| SetFocus | int | 4 | 0 | Non-zero if focus gained, zero if lost |

**Total Size**: 4 bytes
**Layout**: Sequential (BOOL is 4 bytes in Win32)
**Relationships**: Part of INPUT_RECORD union
**Notes**: Reserved by Windows; applications should ignore

---

#### INPUT_RECORD (Union)

| Field | Type | Size | Offset | Description |
|-------|------|------|--------|-------------|
| EventType | EventType (ushort) | 2 | 0 | Discriminator for union |
| *(padding)* | - | 2 | 2 | Alignment padding |
| KeyEvent | KEY_EVENT_RECORD | 16 | 4 | Keyboard event data |
| MouseEvent | MOUSE_EVENT_RECORD | 16 | 4 | Mouse event data |
| WindowBufferSizeEvent | WINDOW_BUFFER_SIZE_RECORD | 4 | 4 | Resize event data |
| MenuEvent | MENU_EVENT_RECORD | 4 | 4 | Menu event data |
| FocusEvent | FOCUS_EVENT_RECORD | 4 | 4 | Focus event data |

**Total Size**: 20 bytes (2 + 2 padding + 16 max union)
**Layout**: Explicit (LayoutKind.Explicit)
**Relationships**: Contains all event record types at overlapping offset; discriminated by EventType

---

#### CONSOLE_SCREEN_BUFFER_INFO

| Field | Type | Size | Offset | Description |
|-------|------|------|--------|-------------|
| dwSize | COORD | 4 | 0 | Screen buffer dimensions |
| dwCursorPosition | COORD | 4 | 4 | Current cursor location |
| wAttributes | ushort | 2 | 8 | Current text attributes |
| srWindow | SMALL_RECT | 8 | 10 | Visible window rectangle |
| dwMaximumWindowSize | COORD | 4 | 18 | Maximum window size |

**Total Size**: 22 bytes
**Layout**: Sequential
**Relationships**: References COORD, SMALL_RECT; returned by GetConsoleScreenBufferInfo

---

#### CHAR_INFO

| Field | Type | Size | Offset | Description |
|-------|------|------|--------|-------------|
| UnicodeChar | char | 2 | 0 | Character to display |
| Attributes | ushort | 2 | 2 | Text/background colors |

**Total Size**: 4 bytes
**Layout**: Sequential
**Relationships**: Used by WriteConsoleOutput
**Notes**: Not in Python win32_types.py; added for WriteConsoleOutput support

---

#### SECURITY_ATTRIBUTES

| Field | Type | Size | Offset | Description |
|-------|------|------|--------|-------------|
| nLength | uint | 4 | 0 | Size of this struct |
| lpSecurityDescriptor | nint | 4/8 | 4/8 | Pointer to security descriptor |
| bInheritHandle | int | 4 | 8/16 | Handle inheritance flag |

**Total Size**: 12 bytes (x86) / 24 bytes (x64) due to pointer alignment
**Layout**: Sequential with Pack=0 (natural alignment)
**Relationships**: Used by CreateEvent and other handle-creation functions

---

### Enums

#### EventType (ushort)

| Value | Name | Description |
|-------|------|-------------|
| 0x0001 | KeyEvent | Keyboard input event |
| 0x0002 | MouseEvent | Mouse input event |
| 0x0004 | WindowBufferSizeEvent | Screen buffer resize event |
| 0x0008 | MenuEvent | Menu selection event (reserved) |
| 0x0010 | FocusEvent | Focus change event (reserved) |

**Backing Type**: ushort (2 bytes, matches WORD)
**Flags**: No (single value)

---

#### ControlKeyState (uint) [Flags]

| Value | Name | Description |
|-------|------|-------------|
| 0x0001 | RightAltPressed | Right Alt key is pressed |
| 0x0002 | LeftAltPressed | Left Alt key is pressed |
| 0x0004 | RightCtrlPressed | Right Ctrl key is pressed |
| 0x0008 | LeftCtrlPressed | Left Ctrl key is pressed |
| 0x0010 | ShiftPressed | Shift key is pressed |
| 0x0020 | NumLockOn | NumLock is toggled on |
| 0x0040 | ScrollLockOn | ScrollLock is toggled on |
| 0x0080 | CapsLockOn | CapsLock is toggled on |
| 0x0100 | EnhancedKey | Key has enhanced key code |

**Backing Type**: uint (4 bytes, matches DWORD)
**Flags**: Yes (combinable)

---

#### MouseEventFlags (uint) [Flags]

| Value | Name | Description |
|-------|------|-------------|
| 0x0000 | None | Button press/release |
| 0x0001 | MouseMoved | Mouse position changed |
| 0x0002 | DoubleClick | Double-click occurred |
| 0x0004 | MouseWheeled | Vertical scroll wheel rotated |
| 0x0008 | MouseHWheeled | Horizontal scroll wheel rotated |

**Backing Type**: uint (4 bytes, matches DWORD)
**Flags**: Yes (combinable)

---

#### MouseButtonState (uint) [Flags]

| Value | Name | Description |
|-------|------|-------------|
| 0x0001 | FromLeft1stButtonPressed | Left button (button 1) |
| 0x0002 | RightmostButtonPressed | Right button |
| 0x0004 | FromLeft2ndButtonPressed | Middle button (button 2) |
| 0x0008 | FromLeft3rdButtonPressed | Button 3 (X1) |
| 0x0010 | FromLeft4thButtonPressed | Button 4 (X2) |

**Backing Type**: uint (4 bytes, matches DWORD)
**Flags**: Yes (combinable)

---

#### ConsoleInputMode (uint) [Flags]

| Value | Name | Description |
|-------|------|-------------|
| 0x0001 | EnableProcessedInput | Ctrl+C processed by system |
| 0x0002 | EnableLineInput | Line-at-a-time input |
| 0x0004 | EnableEchoInput | Echo typed characters |
| 0x0008 | EnableWindowInput | Window resize events |
| 0x0010 | EnableMouseInput | Mouse events |
| 0x0020 | EnableInsertMode | Insert mode enabled |
| 0x0040 | EnableQuickEditMode | Quick edit (mouse select) |
| 0x0080 | EnableExtendedFlags | Required for quick edit |
| 0x0200 | EnableVirtualTerminalInput | VT100 escape sequences |

**Backing Type**: uint (4 bytes, matches DWORD)
**Flags**: Yes (combinable)
**Notes**: Not in Python win32_types.py; added for console mode control

---

#### ConsoleOutputMode (uint) [Flags]

| Value | Name | Description |
|-------|------|-------------|
| 0x0001 | EnableProcessedOutput | Process control chars |
| 0x0002 | EnableWrapAtEolOutput | Wrap at end of line |
| 0x0004 | EnableVirtualTerminalProcessing | VT100 escape sequences |
| 0x0008 | DisableNewlineAutoReturn | No auto CR on LF |
| 0x0010 | EnableLvbGridWorldwide | Grid attributes |

**Backing Type**: uint (4 bytes, matches DWORD)
**Flags**: Yes (combinable)
**Notes**: Not in Python win32_types.py; added for console mode control

---

### Static Classes

#### StdHandles

| Constant | Value | Description |
|----------|-------|-------------|
| STD_INPUT_HANDLE | -10 | Standard input handle ID |
| STD_OUTPUT_HANDLE | -11 | Standard output handle ID |
| STD_ERROR_HANDLE | -12 | Standard error handle ID |

**Notes**: These are handle IDs passed to GetStdHandle, not actual handles.

## Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         INPUT_RECORD                                │
│  EventType ─────────────────────────────────────────────────────┐   │
│      │                                                          │   │
│      ▼                                                          │   │
│  ┌─────────┬──────────────┬───────────────┬──────────┬────────┐│   │
│  │KeyEvent │ MouseEvent   │WindowBuffer   │MenuEvent │Focus   ││   │
│  │         │              │SizeEvent      │          │Event   ││   │
│  └────┬────┴──────┬───────┴───────┬───────┴──────────┴────────┘│   │
│       │           │               │                             │   │
└───────┼───────────┼───────────────┼─────────────────────────────┘   │
        │           │               │
        ▼           ▼               ▼
   ┌─────────┐ ┌─────────┐    ┌─────────┐
   │COORD    │ │Control  │    │COORD    │
   │(mouse   │ │KeyState │    │(buffer  │
   │position)│ │(flags)  │    │size)    │
   └─────────┘ └─────────┘    └─────────┘
        │           │
        │           ▼
        │    ┌───────────┐
        │    │MouseEvent │
        │    │Flags      │
        │    └───────────┘
        │           │
        │           ▼
        │    ┌───────────┐
        │    │MouseButton│
        │    │State      │
        │    └───────────┘
        │
        ▼
┌──────────────────────────────┐
│  CONSOLE_SCREEN_BUFFER_INFO  │
│  ┌───────┐ ┌───────┐        │
│  │dwSize │ │cursor │        │
│  │(COORD)│ │(COORD)│        │
│  └───────┘ └───────┘        │
│  ┌──────────────────┐       │
│  │srWindow          │       │
│  │(SMALL_RECT)      │       │
│  └──────────────────┘       │
└──────────────────────────────┘
```

## Validation Rules

1. **Struct Size Invariants**: All struct sizes MUST match native Windows sizes exactly (verified by unit tests using `Marshal.SizeOf<T>()`)

2. **EventType Discriminator**: When accessing INPUT_RECORD union fields, only the field corresponding to EventType should be read. Reading other fields produces undefined (garbage) data.

3. **ControlKeyState Combination**: Multiple flags can be combined (e.g., `LeftCtrlPressed | ShiftPressed`). Individual flags can be tested with bitwise AND.

4. **Handle Values**: StdHandles constants are NOT handles themselves; they're IDs that must be passed to GetStdHandle to obtain actual handles.

5. **Platform Restriction**: All P/Invoke calls require Windows. Types compile on all platforms, but P/Invoke methods throw `PlatformNotSupportedException` on non-Windows.
