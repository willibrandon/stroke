# Feature Specification: Win32 Console Types

**Feature Branch**: `051-win32-console-types`
**Created**: 2026-02-02
**Status**: Draft
**Input**: User description: "Implement Win32 console API structures and constants for Windows console input/output operations. These types map directly to the Windows Console API structures."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Define Console Data Structures for P/Invoke (Priority: P1)

A developer building a terminal application on Windows needs to interact with the Win32 Console API via P/Invoke. They need correctly defined C# struct types that match the native Windows structures byte-for-byte, so that marshalling between managed and unmanaged code works correctly.

**Why this priority**: Without correct struct definitions, no Windows console operation can work. Every other Win32 console feature (input reading, output writing, cursor positioning) depends on these types being correct.

**Independent Test**: Can be fully tested by creating struct instances, verifying their sizes match the native equivalents, and confirming field offsets are correct. Delivers value as the foundational type layer for all Win32 console interop.

**Acceptance Scenarios**:

1. **Given** the COORD struct is defined, **When** a developer creates `new COORD(10, 20)`, **Then** the X field equals 10 and the Y field equals 20, and the struct size equals 4 bytes.
2. **Given** the CONSOLE_SCREEN_BUFFER_INFO struct is defined, **When** marshalled from native memory, **Then** all fields (dwSize, dwCursorPosition, wAttributes, srWindow, dwMaximumWindowSize) are populated correctly.
3. **Given** the INPUT_RECORD union struct is defined, **When** the EventType field is set to KeyEvent, **Then** the KeyEvent overlay field contains valid KEY_EVENT_RECORD data at the correct offset.

---

### User Story 2 - Use Flags Enums for Console State Interpretation (Priority: P2)

A developer reading console input events needs to interpret modifier key states (Ctrl, Alt, Shift) and mouse event types using well-defined flags enums. They combine and test flags using standard bitwise operations.

**Why this priority**: Flags enums provide the semantic layer that makes raw console data usable. Without them, developers would need to memorize magic numbers.

**Independent Test**: Can be fully tested by combining flag values with bitwise OR and verifying individual flags with bitwise AND. Delivers value as self-documenting constants for console state interpretation.

**Acceptance Scenarios**:

1. **Given** the ControlKeyState flags enum is defined, **When** a developer combines LeftCtrlPressed and ShiftPressed, **Then** the resulting value equals 0x0018 and both flags test true individually.
2. **Given** the MouseEventFlags enum is defined, **When** a developer checks for MouseMoved, **Then** the value equals 0x0001.
3. **Given** the ConsoleInputMode flags enum is defined, **When** a developer combines EnableProcessedInput and EnableMouseInput, **Then** the resulting value equals 0x0011.

---

### User Story 3 - Invoke Native Console Functions (Priority: P3)

A developer building Windows-specific console features needs P/Invoke declarations for kernel32.dll functions like GetStdHandle, GetConsoleScreenBufferInfo, ReadConsoleInput, and SetConsoleCursorPosition. These declarations must use the correct struct types and calling conventions.

**Why this priority**: P/Invoke declarations are the bridge between the managed struct types and actual Windows console operations. They depend on the structs being correctly defined first.

**Independent Test**: Can be tested on Windows by calling GetStdHandle with STD_OUTPUT_HANDLE and verifying a valid handle is returned. Delivers value as the callable interface to native console functions.

**Acceptance Scenarios**:

1. **Given** the NativeMethods class is defined with SupportedOSPlatform("windows"), **When** a developer calls GetStdHandle(STD_OUTPUT_HANDLE) on Windows, **Then** a non-null handle is returned.
2. **Given** the GetConsoleScreenBufferInfo P/Invoke is defined, **When** called with a valid output handle, **Then** a populated CONSOLE_SCREEN_BUFFER_INFO struct is returned with valid window dimensions.
3. **Given** the NativeMethods class is referenced on a non-Windows platform, **When** the code is compiled, **Then** the platform attribute provides a clear indication that Windows is required.

---

### User Story 4 - Access Standard Handle Constants (Priority: P2)

A developer needs well-named constants for the standard input, output, and error handles to pass to GetStdHandle, rather than remembering magic numbers like -10, -11, -12.

**Why this priority**: These constants are referenced throughout all console operations and are a prerequisite for any P/Invoke call that needs a handle.

**Independent Test**: Can be tested by verifying constant values match the Windows API definitions (-10, -11, -12).

**Acceptance Scenarios**:

1. **Given** the StdHandles class is defined, **When** a developer references STD_INPUT_HANDLE, **Then** the value equals -10.
2. **Given** the StdHandles class is defined, **When** a developer references STD_OUTPUT_HANDLE, **Then** the value equals -11.

---

### Edge Cases

#### Unknown EventType Values (CHK050)
- **Question**: What happens when an INPUT_RECORD has an EventType that doesn't match any defined enum value (e.g., future Windows versions add new event types)?
- **Handling**: The EventType field is a ushort that may contain values not in the EventType enum. Code SHOULD check `Enum.IsDefined(typeof(EventType), record.EventType)` before processing. Unknown event types SHOULD be ignored (skipped) rather than causing exceptions.

#### Cross-Platform Marshalling (CHK051)
- **Question**: How does the system handle struct marshalling on non-Windows platforms where these types exist but cannot be used with actual console APIs?
- **Handling**: Struct types compile and can be instantiated on all platforms. Calling P/Invoke methods on non-Windows throws `PlatformNotSupportedException`. Code MUST use `OperatingSystem.IsWindows()` guards before any P/Invoke call.

#### Mismatched Union Field Access (CHK052)
- **Question**: What happens when reading the union field of INPUT_RECORD that doesn't correspond to the EventType?
- **Handling**: Reading the wrong union field yields undefined/garbage data (standard C union behavior). This is not an error condition—the caller is responsible for checking EventType before accessing union fields. No validation is performed.

#### UnicodeChar Simplification (CHK053)
- **Question**: How does KEY_EVENT_RECORD handle the UnicodeChar/AsciiChar union?
- **Handling**: The C# port uses `char` (2-byte Unicode) directly, collapsing the Python UNICODE_OR_ASCII union since .NET is natively Unicode. ASCII characters are a subset of Unicode and work correctly.

#### Invalid Handle Values (CHK054)
- **Question**: What happens when GetStdHandle is called with invalid values?
- **Handling**: GetStdHandle returns `INVALID_HANDLE_VALUE` (-1 cast to nint) for invalid input. Callers SHOULD check `handle != (nint)(-1)` and `handle != IntPtr.Zero`. Failed P/Invoke calls set Win32 error code retrievable via `Marshal.GetLastWin32Error()`.

#### Console Not Attached (CHK055)
- **Question**: What happens when P/Invoke calls are made without an attached console?
- **Handling**: GetStdHandle returns `IntPtr.Zero` when no console is attached (e.g., Windows GUI application). Subsequent console operations fail with error code. Callers MUST check handle validity before use.

#### INPUT_RECORD Array Sizing (CHK056)
- **Question**: How should INPUT_RECORD arrays be sized for ReadConsoleInput?
- **Handling**: ReadConsoleInput reads up to `nLength` records into the provided array. The actual count is returned in `lpNumberOfEventsRead`. Recommended buffer size: 128 records (2,560 bytes) for typical input scenarios. Array MUST be pre-allocated; P/Invoke does not allocate.

#### SMALL_RECT Edge Values (CHK057)
- **Question**: What are valid SMALL_RECT coordinate ranges?
- **Handling**: SMALL_RECT uses short (Int16) for coordinates, allowing -32768 to 32767. Console buffers typically use non-negative coordinates. Width/Height properties return `(short)(Right - Left + 1)` which may overflow for extreme values. Callers SHOULD validate coordinates are within console buffer bounds.

#### COORD Negative Values (CHK058)
- **Question**: Are negative COORD values valid?
- **Handling**: COORD uses short (Int16), allowing negative values. Console coordinates are typically non-negative, but relative positioning may use negative offsets. The Windows API accepts negative coordinates; behavior is context-dependent. No validation is performed at the struct level.

## Requirements *(mandatory)*

### Functional Requirements

#### Struct Definitions (with exact sizes and layouts)

- **FR-001**: System MUST define a Coord struct (4 bytes) with X (short, 2 bytes) and Y (short, 2 bytes) fields using `[StructLayout(LayoutKind.Sequential)]`. MUST include a constructor `Coord(short x, short y)` and implement `IEquatable<Coord>`.

- **FR-002**: System MUST define a SmallRect struct (8 bytes) with Left, Top, Right, Bottom (each short, 2 bytes) fields using `[StructLayout(LayoutKind.Sequential)]`. MUST include:
  - Constructor `SmallRect(short left, short top, short right, short bottom)`
  - Computed property `Width` returning `(short)(Right - Left + 1)`
  - Computed property `Height` returning `(short)(Bottom - Top + 1)`
  - `IEquatable<SmallRect>` implementation

- **FR-003**: System MUST define a ConsoleScreenBufferInfo struct (22 bytes) with fields in sequential order:
  - Size (Coord, 4 bytes)
  - CursorPosition (Coord, 4 bytes)
  - Attributes (ushort, 2 bytes)
  - Window (SmallRect, 8 bytes)
  - MaximumWindowSize (Coord, 4 bytes)

- **FR-004**: System MUST define a KeyEventRecord struct (16 bytes) with fields using `[StructLayout(LayoutKind.Sequential)]`:
  - KeyDown (int, 4 bytes) - non-zero if key pressed, zero if released
  - RepeatCount (ushort, 2 bytes)
  - VirtualKeyCode (ushort, 2 bytes)
  - VirtualScanCode (ushort, 2 bytes)
  - UnicodeChar (char, 2 bytes)
  - ControlKeyState (ControlKeyState enum, 4 bytes)
  - MUST include computed property `IsKeyDown` returning `KeyDown != 0`

- **FR-005**: System MUST define a MouseEventRecord struct (16 bytes) with fields using `[StructLayout(LayoutKind.Sequential)]`:
  - MousePosition (Coord, 4 bytes)
  - ButtonState (MouseButtonState enum, 4 bytes)
  - ControlKeyState (ControlKeyState enum, 4 bytes)
  - EventFlags (MouseEventFlags enum, 4 bytes)

- **FR-006**: System MUST define a WindowBufferSizeRecord struct (4 bytes) with Size (Coord) field

- **FR-007**: System MUST define a MenuEventRecord struct (4 bytes) with CommandId (uint) field, faithfully porting the Python MENU_EVENT_RECORD (reserved by Windows, included for API completeness)

- **FR-008**: System MUST define a FocusEventRecord struct (4 bytes) with SetFocus (int) field, faithfully porting the Python FOCUS_EVENT_RECORD (reserved by Windows, included for API completeness). MUST include computed property `HasFocus` returning `SetFocus != 0`

- **FR-009**: System MUST define an InputRecord struct (20 bytes) using `[StructLayout(LayoutKind.Explicit, Size = 20)]` with:
  - `[FieldOffset(0)]` EventType (EventType enum, 2 bytes) - note: 2 bytes padding follows
  - `[FieldOffset(4)]` KeyEvent (KeyEventRecord) - union overlay
  - `[FieldOffset(4)]` MouseEvent (MouseEventRecord) - union overlay
  - `[FieldOffset(4)]` WindowBufferSizeEvent (WindowBufferSizeRecord) - union overlay
  - `[FieldOffset(4)]` MenuEvent (MenuEventRecord) - union overlay
  - `[FieldOffset(4)]` FocusEvent (FocusEventRecord) - union overlay
  - The union payload occupies 16 bytes (largest member: KeyEventRecord/MouseEventRecord)
  - **EventType-to-field mapping**: The EventType discriminant determines which union field contains valid data:
    | EventType Value | Valid Union Field | Notes |
    |-----------------|-------------------|-------|
    | `KeyEvent` (0x0001) | `KeyEvent` | Keyboard input |
    | `MouseEvent` (0x0002) | `MouseEvent` | Mouse input |
    | `WindowBufferSizeEvent` (0x0004) | `WindowBufferSizeEvent` | Resize event |
    | `MenuEvent` (0x0008) | `MenuEvent` | Reserved by Windows |
    | `FocusEvent` (0x0010) | `FocusEvent` | Reserved by Windows |
  - Reading a union field when EventType does not match yields undefined/garbage data (standard C union behavior)

- **FR-014**: System MUST define a SecurityAttributes struct with:
  - Length (uint, 4 bytes)
  - SecurityDescriptor (nint, 4/8 bytes depending on platform)
  - InheritHandle (int, 4 bytes) - BOOL as int for correct marshalling
  - Static factory method `Create()` that initializes Length to the struct size
  - Total size: 12 bytes (x86) or 24 bytes (x64)

- **FR-018**: System MUST define a CharInfo struct (4 bytes) with:
  - UnicodeChar (char, 2 bytes)
  - Attributes (ushort, 2 bytes)
  - Constructor `CharInfo(char unicodeChar, ushort attributes)`
  - `IEquatable<CharInfo>` implementation

#### Enum Definitions (with exact hex values)

- **FR-010**: System MUST define an EventType enum (underlying type: ushort) with values:
  - `KeyEvent = 0x0001`
  - `MouseEvent = 0x0002`
  - `WindowBufferSizeEvent = 0x0004`
  - `MenuEvent = 0x0008`
  - `FocusEvent = 0x0010`

- **FR-011**: System MUST define a ControlKeyState flags enum (underlying type: uint) with `[Flags]` attribute and values:
  - `None = 0x0000`
  - `RightAltPressed = 0x0001`
  - `LeftAltPressed = 0x0002`
  - `RightCtrlPressed = 0x0004`
  - `LeftCtrlPressed = 0x0008`
  - `ShiftPressed = 0x0010`
  - `NumLockOn = 0x0020`
  - `ScrollLockOn = 0x0040`
  - `CapsLockOn = 0x0080`
  - `EnhancedKey = 0x0100`

- **FR-012**: System MUST define flags enums with `[Flags]` attribute:
  - **MouseEventFlags** (underlying type: uint):
    - `None = 0x0000` (button press/release)
    - `MouseMoved = 0x0001`
    - `DoubleClick = 0x0002`
    - `MouseWheeled = 0x0004`
    - `MouseHWheeled = 0x0008`
  - **MouseButtonState** (underlying type: uint):
    - `None = 0x0000`
    - `FromLeft1stButtonPressed = 0x0001` (left button)
    - `RightmostButtonPressed = 0x0002` (right button)
    - `FromLeft2ndButtonPressed = 0x0004` (middle button)
    - `FromLeft3rdButtonPressed = 0x0008` (X1 button)
    - `FromLeft4thButtonPressed = 0x0010` (X2 button)

- **FR-013**: System MUST define flags enums with `[Flags]` attribute:
  - **ConsoleInputMode** (underlying type: uint):
    - `None = 0x0000`
    - `EnableProcessedInput = 0x0001`
    - `EnableLineInput = 0x0002`
    - `EnableEchoInput = 0x0004`
    - `EnableWindowInput = 0x0008`
    - `EnableMouseInput = 0x0010`
    - `EnableInsertMode = 0x0020`
    - `EnableQuickEditMode = 0x0040`
    - `EnableExtendedFlags = 0x0080`
    - `EnableVirtualTerminalInput = 0x0200`
    - **Usage example**: Combining flags with bitwise OR:
      ```csharp
      var mode = ConsoleInputMode.EnableProcessedInput
               | ConsoleInputMode.EnableMouseInput
               | ConsoleInputMode.EnableWindowInput;
      // mode == 0x0019
      ```
  - **ConsoleOutputMode** (underlying type: uint):
    - `None = 0x0000`
    - `EnableProcessedOutput = 0x0001`
    - `EnableWrapAtEolOutput = 0x0002`
    - `EnableVirtualTerminalProcessing = 0x0004`
    - `DisableNewlineAutoReturn = 0x0008`
    - `EnableLvbGridWorldwide = 0x0010`

#### Constants

- **FR-015**: System MUST define a StdHandles static class with constants:
  - `STD_INPUT_HANDLE = -10`
  - `STD_OUTPUT_HANDLE = -11`
  - `STD_ERROR_HANDLE = -12`

#### P/Invoke Declarations

- **FR-016**: System MUST provide P/Invoke declarations using `[LibraryImport]` source generator (not legacy `[DllImport]`) for kernel32.dll functions. The containing class MUST be `partial` and use the constant `private const string Kernel32 = "kernel32.dll"`. Each method MUST specify:
  - `SetLastError = true` for methods that set Win32 error codes
  - `[MarshalAs(UnmanagedType.Bool)]` for BOOL return types
  - `[In]` attribute for input array parameters
  - `[Out]` attribute for output array parameters
  - `out` parameter modifier for output structs
  - `ref` parameter modifier for in/out structs
  - Unicode entry points (W-suffix) for string-related functions

  Required P/Invoke methods:
  - `GetStdHandle(int nStdHandle)` → returns nint
  - `GetConsoleScreenBufferInfo(nint hConsoleOutput, out ConsoleScreenBufferInfo lpConsoleScreenBufferInfo)` → returns bool
  - `GetConsoleMode(nint hConsoleHandle, out uint lpMode)` → returns bool
  - `SetConsoleMode(nint hConsoleHandle, uint dwMode)` → returns bool
  - `ReadConsoleInput(nint hConsoleInput, [Out] InputRecord[] lpBuffer, uint nLength, out uint lpNumberOfEventsRead)` → returns bool (entry point: `ReadConsoleInputW`)
  - `WriteConsoleOutput(nint hConsoleOutput, [In] CharInfo[] lpBuffer, Coord dwBufferSize, Coord dwBufferCoord, ref SmallRect lpWriteRegion)` → returns bool (entry point: `WriteConsoleOutputW`)
  - `SetConsoleCursorPosition(nint hConsoleOutput, Coord dwCursorPosition)` → returns bool
  - `CreateEvent(ref SecurityAttributes lpEventAttributes, [MarshalAs(UnmanagedType.Bool)] bool bManualReset, [MarshalAs(UnmanagedType.Bool)] bool bInitialState, string? lpName)` → returns nint (entry point: `CreateEventW`)
  - `SetEvent(nint hEvent)` → returns bool
  - `ResetEvent(nint hEvent)` → returns bool
  - `CloseHandle(nint hObject)` → returns bool
  - `WaitForMultipleObjects(uint nCount, nint[] lpHandles, [MarshalAs(UnmanagedType.Bool)] bool bWaitAll, uint dwMilliseconds)` → returns uint

- **FR-017**: System MUST annotate P/Invoke declarations with `[SupportedOSPlatform("windows")]` attribute on the containing class. This attribute is from `System.Runtime.Versioning` namespace and triggers CA1416 analyzer warnings when P/Invoke methods are called without platform guards

### Non-Functional Requirements

#### Naming Conventions

- **NFR-001**: All C# type and member names MUST follow PascalCase convention, translating from Python Prompt Toolkit's snake_case:
  | Python Name | C# Name |
  |-------------|---------|
  | `COORD` | `Coord` |
  | `SMALL_RECT` | `SmallRect` |
  | `KEY_EVENT_RECORD` | `KeyEventRecord` |
  | `MOUSE_EVENT_RECORD` | `MouseEventRecord` |
  | `WINDOW_BUFFER_SIZE_RECORD` | `WindowBufferSizeRecord` |
  | `MENU_EVENT_RECORD` | `MenuEventRecord` |
  | `FOCUS_EVENT_RECORD` | `FocusEventRecord` |
  | `INPUT_RECORD` | `InputRecord` |
  | `CONSOLE_SCREEN_BUFFER_INFO` | `ConsoleScreenBufferInfo` |
  | `SECURITY_ATTRIBUTES` | `SecurityAttributes` |
  | `CHAR_INFO` | `CharInfo` |

#### Immutability

- **NFR-002**: All struct types MUST be declared as `readonly struct` to ensure immutability and enable compiler optimizations. All fields MUST be declared as `readonly`. Exception: SecurityAttributes requires mutable Length field for initialization via factory method.

#### Interface Implementations

- **NFR-003**: Value-semantic structs (Coord, SmallRect, CharInfo) MUST implement:
  - `IEquatable<T>` with value-based equality
  - `Equals(object?)` override
  - `GetHashCode()` override
  - `operator ==` and `operator !=`
  - `ToString()` override returning a human-readable representation

#### Thread Safety

- **NFR-004**: All struct types are inherently thread-safe due to their immutable, value-type nature. P/Invoke calls to kernel32.dll are thread-safe as the Windows Console API handles concurrent access internally.

#### Memory Allocation

- **NFR-009**: All struct types are value types allocated on the stack by default. Array parameters to P/Invoke methods (InputRecord[], CharInfo[]) are heap-allocated. Structs SHOULD be passed by `ref` or `in` to avoid unnecessary copying for larger structs (InputRecord at 20 bytes, ConsoleScreenBufferInfo at 22 bytes).

#### Performance

- **NFR-010**: P/Invoke marshalling uses blittable types where possible (structs with explicit layouts and primitive fields) to enable direct memory pinning without copying. All struct fields MUST be blittable types (int, uint, short, char, nint). The `[StructLayout]` attributes ensure zero-copy marshalling.

#### Namespace and File Organization

- **NFR-005**: All types MUST be placed in the `Stroke.Input.Windows.Win32Types` namespace, except:
  - `StdHandles` in `Stroke.Input.Windows`
  - P/Invoke declarations extend existing `ConsoleApi` class in `Stroke.Input.Windows`

- **NFR-006**: Each struct and enum type MUST be in its own file (one type per file). No file MUST exceed 1,000 lines of code.

#### Documentation

- **NFR-007**: All public types and members MUST have XML documentation comments (`///`) including:
  - `<summary>` for the type/member purpose
  - `<remarks>` explaining relationship to Windows API and Python Prompt Toolkit
  - Thread safety statement in remarks

#### Cross-Platform Compilation

- **NFR-008**: All struct and enum types MUST compile on all platforms (Windows, Linux, macOS). P/Invoke methods are callable only on Windows at runtime. Code consuming P/Invoke MUST use `OperatingSystem.IsWindows()` guards:
  ```csharp
  if (OperatingSystem.IsWindows())
  {
      var handle = ConsoleApi.GetStdHandle(StdHandles.STD_OUTPUT_HANDLE);
      // ... Windows-specific code
  }
  ```

### Key Entities

- **COORD**: Represents a 2D coordinate (column, row) in the console screen buffer. Used extensively by other structures.
- **SMALL_RECT**: Represents a rectangular region in the console screen buffer, defined by its four edges.
- **INPUT_RECORD**: A discriminated union that wraps all possible console input events (keyboard, mouse, resize, menu, focus).
- **KEY_EVENT_RECORD**: Describes a keyboard input event including key state, virtual key codes, and modifier state.
- **MOUSE_EVENT_RECORD**: Describes a mouse input event including position, button state, and event type.
- **WINDOW_BUFFER_SIZE_RECORD**: Describes a console screen buffer resize event with the new buffer size.
- **MENU_EVENT_RECORD**: Describes a menu event with a command identifier.
- **FOCUS_EVENT_RECORD**: Describes a focus change event with a set-focus indicator.
- **CONSOLE_SCREEN_BUFFER_INFO**: Snapshot of console buffer state including size, cursor position, and visible window.
- **SECURITY_ATTRIBUTES**: Security descriptor structure used when creating handles.
- **CHAR_INFO**: Character and attribute pair for direct screen buffer writing.
- **ConsoleApi**: Existing P/Invoke wrapper class extended with console function declarations for kernel32.dll.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All struct types have correct sizes verified against their native Windows counterparts. Expected sizes (verifiable via `Marshal.SizeOf<T>()` or `sizeof(T)` in unsafe context):
  | Struct | Expected Size | Verification |
  |--------|---------------|--------------|
  | Coord | 4 bytes | `Marshal.SizeOf<Coord>() == 4` |
  | SmallRect | 8 bytes | `Marshal.SizeOf<SmallRect>() == 8` |
  | KeyEventRecord | 16 bytes | `Marshal.SizeOf<KeyEventRecord>() == 16` |
  | MouseEventRecord | 16 bytes | `Marshal.SizeOf<MouseEventRecord>() == 16` |
  | WindowBufferSizeRecord | 4 bytes | `Marshal.SizeOf<WindowBufferSizeRecord>() == 4` |
  | MenuEventRecord | 4 bytes | `Marshal.SizeOf<MenuEventRecord>() == 4` |
  | FocusEventRecord | 4 bytes | `Marshal.SizeOf<FocusEventRecord>() == 4` |
  | InputRecord | 20 bytes | `Marshal.SizeOf<InputRecord>() == 20` |
  | ConsoleScreenBufferInfo | 22 bytes | `Marshal.SizeOf<ConsoleScreenBufferInfo>() == 22` |
  | CharInfo | 4 bytes | `Marshal.SizeOf<CharInfo>() == 4` |
  | SecurityAttributes | 12/24 bytes | Platform-dependent (x86/x64) |

- **SC-002**: All union struct field offsets are correctly defined, allowing safe access to the appropriate event record based on EventType. Verifiable via `Marshal.OffsetOf<InputRecord>("KeyEvent") == 4` etc.

- **SC-003**: All flags enum values match their corresponding Windows API constant definitions. **Source of truth**: [Microsoft Learn - Console Virtual Terminal Sequences](https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences) and [Console Structures](https://learn.microsoft.com/en-us/windows/console/console-structures)

- **SC-004**: P/Invoke declarations are callable on Windows and correctly marshal data between managed and native code. Verified by:
  - Calling `GetStdHandle(STD_OUTPUT_HANDLE)` returns handle != `IntPtr.Zero`
  - Calling `GetConsoleScreenBufferInfo` populates all struct fields with non-default values
  - Calling `ReadConsoleInput` returns keyboard events when keys are pressed

- **SC-005**: All types compile and are usable on non-Windows platforms (even though P/Invoke calls require Windows at runtime). Verified by building on Linux/macOS without errors.

- **SC-006**: 100% of public types from Python Prompt Toolkit's `win32_types.py` have corresponding C# equivalents, including MENU_EVENT_RECORD and FOCUS_EVENT_RECORD. **Source file**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/win32_types.py`

- **SC-007**: All defined structures have comprehensive unit tests verifying construction, field assignment, and size correctness. **Test file count**: 14 test files covering all 11 structs (CoordTests, SmallRectTests, KeyEventRecordTests, MouseEventRecordTests, WindowBufferSizeRecordTests, MenuEventRecordTests, FocusEventRecordTests, InputRecordTests, ConsoleScreenBufferInfoTests, CharInfoTests, SecurityAttributesTests), 6 enums (EnumTests), StdHandles (StdHandlesTests), and P/Invoke methods (NativeMethodsTests)

- **SC-008**: Calling P/Invoke methods without `OperatingSystem.IsWindows()` guard produces CA1416 analyzer warning "This call site is reachable on all platforms"

## Dependencies

### Internal Dependencies

- **ConsoleApi.cs** (`src/Stroke/Input/Windows/ConsoleApi.cs`): Existing P/Invoke wrapper class that will be extended with new methods (GetConsoleScreenBufferInfo, ReadConsoleInput, WriteConsoleOutput, SetConsoleCursorPosition). Already contains: GetStdHandle, GetConsoleMode, SetConsoleMode, CreateEvent, SetEvent, ResetEvent, CloseHandle, WaitForMultipleObjects.

- **Win32Input.cs** (`src/Stroke/Input/Windows/Win32Input.cs`): Existing Windows input handler that will consume the new struct types (InputRecord, KeyEventRecord, MouseEventRecord) once they are available. Currently uses simplified Console.ReadKey approach.

### External References

- **Python Prompt Toolkit Source**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/win32_types.py` — The authoritative source for faithful API porting. All struct types in this feature MUST have corresponding implementations in the Python source.

- **Microsoft Documentation**:
  - [Console Structures](https://learn.microsoft.com/en-us/windows/console/console-structures)
  - [Console Functions](https://learn.microsoft.com/en-us/windows/console/console-functions)
  - [Virtual Terminal Sequences](https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences)

## Assumptions

- The C# port uses `char` (2-byte Unicode) for KEY_EVENT_RECORD.UnicodeChar, collapsing the Python UNICODE_OR_ASCII union since .NET is natively Unicode
- Console mode flags (ConsoleInputMode, ConsoleOutputMode) are included even though they don't appear in the Python `win32_types.py`, because they are needed by the broader Win32 console subsystem and are referenced in other Python Prompt Toolkit modules
- The CHAR_INFO struct is included for WriteConsoleOutput support even though it doesn't appear in `win32_types.py`
- P/Invoke declarations use native integer types for handle types, following modern .NET conventions
- All P/Invoke declarations are internal since they are implementation details consumed by higher-level Stroke APIs
