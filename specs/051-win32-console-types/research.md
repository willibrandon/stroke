# Research: Win32 Console Types

**Feature**: 051-win32-console-types
**Date**: 2026-02-02
**Status**: Complete

## Research Questions Resolved

### 1. Existing Infrastructure Analysis

**Question**: What Win32-related code already exists in Stroke?

**Decision**: Extend existing `Stroke.Input.Windows` namespace with new `Win32Types` sub-namespace.

**Rationale**:
- `ConsoleApi.cs` (236 lines) already contains P/Invoke declarations and mode flag constants
- `Win32Input.cs` (413 lines) uses `ConsoleApi` for input handling
- Adding struct types as a sub-namespace maintains logical grouping
- Existing code uses `LibraryImport` pattern (modern .NET 5+) and `[SupportedOSPlatform("windows")]`

**Alternatives Considered**:
- Create new `Stroke.Interop.Win32` namespace: Rejected because it would create a new top-level namespace when existing `Input.Windows` already handles Win32 code
- Add all types to `ConsoleApi.cs`: Rejected because it would exceed 1,000 LOC limit

### 2. P/Invoke Attribute Pattern

**Question**: Should we use `[DllImport]` or `[LibraryImport]`?

**Decision**: Use `[LibraryImport]` with source generators (modern .NET 5+ approach).

**Rationale**:
- Existing `ConsoleApi.cs` already uses `[LibraryImport]` pattern
- Source generators provide compile-time marshalling code generation
- Better performance than reflection-based `[DllImport]`
- Requires `partial` methods and classes

**Alternatives Considered**:
- `[DllImport]`: Rejected as legacy pattern; existing code uses `[LibraryImport]`

### 3. Native Struct Sizes (Verified)

**Question**: What are the exact struct sizes for correct P/Invoke marshalling?

**Decision**: Use verified sizes from Microsoft documentation:

| Struct | Size (bytes) | Layout Notes |
|--------|--------------|--------------|
| COORD | 4 | 2 shorts, sequential |
| SMALL_RECT | 8 | 4 shorts, sequential |
| KEY_EVENT_RECORD | 16 | Includes 2-byte char union |
| MOUSE_EVENT_RECORD | 16 | COORD + 3 DWORDs |
| WINDOW_BUFFER_SIZE_RECORD | 4 | Single COORD |
| MENU_EVENT_RECORD | 4 | Single UINT |
| FOCUS_EVENT_RECORD | 4 | Single BOOL (4 bytes in Win32) |
| INPUT_RECORD | 20 | 2-byte EventType + 2-byte padding + 16-byte union |
| CONSOLE_SCREEN_BUFFER_INFO | 22 | COORD + COORD + WORD + SMALL_RECT + COORD |
| CHAR_INFO | 4 | 2-byte char + 2-byte attributes |
| SECURITY_ATTRIBUTES | 12/24 | Platform-dependent (pointer size) |

**Rationale**: Sizes verified against official Microsoft documentation at learn.microsoft.com.

### 4. INPUT_RECORD Union Layout

**Question**: How to implement the C union in C# for INPUT_RECORD?

**Decision**: Use `[StructLayout(LayoutKind.Explicit)]` with overlapping `[FieldOffset(4)]` for all event record fields.

**Rationale**:
- C# doesn't have native unions; explicit layout simulates them
- EventType at offset 0 (2 bytes) + 2 bytes padding = offset 4 for union
- All 5 event record types share offset 4
- This matches Windows native layout exactly

**Code Pattern**:
```csharp
[StructLayout(LayoutKind.Explicit, Size = 20)]
public struct InputRecord
{
    [FieldOffset(0)] public EventType EventType;  // 2 bytes
    // 2 bytes padding implicit
    [FieldOffset(4)] public KeyEventRecord KeyEvent;
    [FieldOffset(4)] public MouseEventRecord MouseEvent;
    [FieldOffset(4)] public WindowBufferSizeRecord WindowBufferSizeEvent;
    [FieldOffset(4)] public MenuEventRecord MenuEvent;
    [FieldOffset(4)] public FocusEventRecord FocusEvent;
}
```

### 5. UNICODE_OR_ASCII Simplification

**Question**: How to handle the Python `UNICODE_OR_ASCII` union in C#?

**Decision**: Use `char` (UTF-16) directly, eliminating the union.

**Rationale**:
- .NET is natively Unicode (UTF-16)
- All modern Windows APIs use Unicode variants (W-suffix functions)
- Python version maintains union for legacy ANSI compatibility
- Spec assumption documents this deviation

**Alternatives Considered**:
- Explicit union with byte and char: Rejected as unnecessary complexity

### 6. Handle Type Convention

**Question**: Should handles use `IntPtr` or `nint`?

**Decision**: Use `nint` (native integer) for all handle types.

**Rationale**:
- Existing `ConsoleApi.cs` uses `nint` for handles
- `nint` is C# 9+ alias for `IntPtr` with better arithmetic support
- Modern .NET convention

### 7. Enum Backing Types

**Question**: What backing types for each enum?

**Decision**: Match the native Windows types:

| Enum | Backing Type | Windows Type |
|------|--------------|--------------|
| EventType | ushort | WORD (2 bytes) |
| ControlKeyState | uint | DWORD (4 bytes) |
| MouseEventFlags | uint | DWORD (4 bytes) |
| MouseButtonState | uint | DWORD (4 bytes) |
| ConsoleInputMode | uint | DWORD (4 bytes) |
| ConsoleOutputMode | uint | DWORD (4 bytes) |

### 8. Enum/Flag Values (Verified)

All values verified against Microsoft documentation:

**EventType**:
- KeyEvent = 0x0001
- MouseEvent = 0x0002
- WindowBufferSizeEvent = 0x0004
- MenuEvent = 0x0008
- FocusEvent = 0x0010

**ControlKeyState**:
- RightAltPressed = 0x0001
- LeftAltPressed = 0x0002
- RightCtrlPressed = 0x0004
- LeftCtrlPressed = 0x0008
- ShiftPressed = 0x0010
- NumLockOn = 0x0020
- ScrollLockOn = 0x0040
- CapsLockOn = 0x0080
- EnhancedKey = 0x0100

**MouseEventFlags**:
- MouseMoved = 0x0001
- DoubleClick = 0x0002
- MouseWheeled = 0x0004
- MouseHWheeled = 0x0008

**MouseButtonState**:
- FromLeft1stButtonPressed = 0x0001
- RightmostButtonPressed = 0x0002
- FromLeft2ndButtonPressed = 0x0004
- FromLeft3rdButtonPressed = 0x0008
- FromLeft4thButtonPressed = 0x0010

**ConsoleInputMode**:
- EnableProcessedInput = 0x0001
- EnableLineInput = 0x0002
- EnableEchoInput = 0x0004
- EnableWindowInput = 0x0008
- EnableMouseInput = 0x0010
- EnableInsertMode = 0x0020
- EnableQuickEditMode = 0x0040
- EnableExtendedFlags = 0x0080
- EnableVirtualTerminalInput = 0x0200

**ConsoleOutputMode**:
- EnableProcessedOutput = 0x0001
- EnableWrapAtEolOutput = 0x0002
- EnableVirtualTerminalProcessing = 0x0004
- DisableNewlineAutoReturn = 0x0008
- EnableLvbGridWorldwide = 0x0010

### 9. Existing Constants Consolidation

**Question**: Should StdHandles duplicate constants from ConsoleApi?

**Decision**: Create `StdHandles` as a separate public-facing class; keep `ConsoleApi` constants for internal use.

**Rationale**:
- `ConsoleApi` is a low-level P/Invoke wrapper
- `StdHandles` provides a clean public API matching Python `win32_types.py`
- Values (-10, -11, -12) are already in `ConsoleApi`; `StdHandles` provides semantic wrapper
- Follows Python Prompt Toolkit's module organization

### 10. Additional P/Invoke Methods Needed

**Question**: Which P/Invoke methods are missing from `ConsoleApi.cs`?

**Decision**: Add the following methods (FR-016):

| Method | Status | Notes |
|--------|--------|-------|
| GetStdHandle | ✅ EXISTS | In ConsoleApi |
| GetConsoleScreenBufferInfo | ❌ ADD | Needs CONSOLE_SCREEN_BUFFER_INFO struct |
| GetConsoleMode | ✅ EXISTS | In ConsoleApi |
| SetConsoleMode | ✅ EXISTS | In ConsoleApi |
| ReadConsoleInput | ❌ ADD | Needs INPUT_RECORD struct |
| WriteConsoleOutput | ❌ ADD | Needs CHAR_INFO struct |
| SetConsoleCursorPosition | ❌ ADD | Needs COORD struct |
| CreateEvent | ✅ EXISTS | In ConsoleApi |
| SetEvent | ✅ EXISTS | In ConsoleApi |
| ResetEvent | ✅ EXISTS | In ConsoleApi |
| CloseHandle | ✅ EXISTS | In ConsoleApi |
| WaitForMultipleObjects | ✅ EXISTS | In ConsoleApi |

Methods to add: `GetConsoleScreenBufferInfo`, `ReadConsoleInput`, `WriteConsoleOutput`, `SetConsoleCursorPosition`

## Sources

- Microsoft Learn: Console API Reference (https://learn.microsoft.com/en-us/windows/console/)
- Python Prompt Toolkit: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/win32_types.py`
- Existing Stroke code: `/Users/brandon/src/stroke/src/Stroke/Input/Windows/ConsoleApi.cs`
