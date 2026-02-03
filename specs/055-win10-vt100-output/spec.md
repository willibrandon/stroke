# Feature Specification: Windows 10 VT100 Output

**Feature Branch**: `055-win10-vt100-output`
**Created**: 2026-02-03
**Status**: Draft
**Input**: User description: "Implement Windows10Output - a Windows-specific output class that enables VT100 escape sequences on Windows 10+ by setting the ENABLE_VIRTUAL_TERMINAL_PROCESSING console mode flag."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Terminal Application Rendering with Modern Colors (Priority: P1)

A developer building a terminal application on Windows 10+ wants to use ANSI escape sequences for rich text formatting (colors, cursor movement, text styles) without worrying about legacy Windows console limitations. The Windows10Output class transparently enables VT100 processing during flush operations.

**Why this priority**: This is the core value proposition - enabling modern terminal rendering on Windows 10+ using the same VT100 escape sequences that work on Linux/macOS, providing cross-platform consistency.

**Independent Test**: Can be fully tested by creating a Windows10Output instance, writing ANSI-colored text, flushing, and observing that colors render correctly in Windows Terminal or modern cmd.exe.

**Acceptance Scenarios**:

1. **Given** a Windows 10+ terminal environment, **When** the application writes ANSI escape sequences through Windows10Output and flushes, **Then** the text appears with correct colors and formatting.
2. **Given** a Windows10Output instance, **When** multiple flush operations occur, **Then** each flush temporarily enables VT100 mode and restores the original console mode afterward.
3. **Given** a Windows10Output instance with default settings, **When** querying the default color depth, **Then** true color (24-bit) is returned.

---

### User Story 2 - Console Size and Buffer Operations (Priority: P2)

A developer needs to query terminal dimensions and perform console-specific operations (scroll buffer, get cursor position) while still using VT100 for rendering. Windows10Output delegates console operations to Win32Output while delegating rendering to Vt100Output.

**Why this priority**: Console sizing and buffer operations are essential for layout calculations but secondary to the core rendering capability.

**Independent Test**: Can be tested by creating a Windows10Output instance and calling GetSize(), GetRowsBelowCursorPosition(), and ScrollBufferToPrompt() to verify Win32Output delegation works correctly.

**Acceptance Scenarios**:

1. **Given** a Windows10Output instance, **When** calling GetSize(), **Then** the terminal dimensions are returned using Win32 console APIs (same as Win32Output).
2. **Given** a Windows10Output instance, **When** calling GetRowsBelowCursorPosition(), **Then** the cursor position is calculated using Win32 console APIs.
3. **Given** a Windows10Output instance, **When** calling ScrollBufferToPrompt(), **Then** the scroll operation is performed via Win32 console APIs.

---

### User Story 3 - VT100 Support Detection (Priority: P3)

A developer or output factory needs to determine at runtime whether the current Windows environment supports VT100 escape sequences before choosing Windows10Output over legacy Win32Output.

**Why this priority**: Detection is important for output factory decisions but is a utility function rather than core functionality.

**Independent Test**: Can be tested by calling IsVt100Enabled() on various Windows versions/environments and verifying it correctly detects VT100 support.

**Acceptance Scenarios**:

1. **Given** a Windows 10+ environment with VT100 support, **When** calling IsVt100Enabled(), **Then** true is returned.
2. **Given** an older Windows environment without VT100 support, **When** calling IsVt100Enabled(), **Then** false is returned.
3. **Given** any Windows environment, **When** calling IsVt100Enabled(), **Then** the original console mode is restored after the test completes (no side effects).

---

### Edge Cases

#### Thread Safety Scenarios

| Scenario | Behavior |
|----------|----------|
| Flush called multiple times rapidly | Each flush independently acquires lock, enables VT100, flushes, restores mode |
| Concurrent Flush from multiple threads (same instance) | Serialized via per-instance `Lock`; second thread blocks until first completes |
| Concurrent Flush while another Flush is blocked | Second caller waits; no timeout or deadlock prevention (Lock provides fairness) |
| Cross-instance concurrent access (multiple Windows10Output instances) | No coordination; each instance has independent lock; interleaving possible at console level |
| Concurrent calls to non-Flush methods | Delegated to underlying Win32Output/Vt100Output; thread safety inherited from those implementations |

**Lock Scope**: Only `Flush()` is protected by the per-instance lock. All other operations delegate directly without locking because the underlying outputs handle their own thread safety.

**Lock Type**: `System.Threading.Lock` (.NET 9+) with `EnterScope()` pattern for automatic release via `using` statement. No timeout configuration; no explicit deadlock prevention beyond standard Lock semantics.

#### Error Handling Scenarios

| Scenario | Behavior |
|----------|----------|
| `stdout` parameter is null | Constructor throws `ArgumentNullException("stdout")` |
| Running on non-Windows platform | Constructor throws `PlatformNotSupportedException` |
| No console buffer (GUI app, redirected output) | Constructor propagates `NoConsoleScreenBufferError` from `Win32Output` constructor |
| `GetConsoleMode` fails during Flush | VT100 mode not enabled; Vt100Output.Flush() called directly; no exception thrown |
| `SetConsoleMode` fails during Flush (enable) | Flush proceeds without VT100 mode enabled (original mode retained); Vt100Output.Flush() called; original mode restore attempted |
| `SetConsoleMode` fails during Flush (restore) | Failure silently ignored; console may remain in VT100 mode |
| `Vt100Output.Flush()` throws exception | Exception propagates to caller; original mode still restored in finally block |
| Invalid console handle (`_hconsole`) | GetConsoleMode/SetConsoleMode fail; handled per above scenarios |
| Errors from delegated Win32Output/Vt100Output methods | Propagated directly to caller; no additional wrapping or handling |

#### WindowsVt100Support.IsVt100Enabled() Error Handling

| Scenario | Return Value |
|----------|--------------|
| GetStdHandle returns invalid handle | Returns false |
| GetConsoleMode fails (non-console, redirected) | Returns false |
| SetConsoleMode fails (VT100 not supported) | Returns false |
| SetConsoleMode succeeds | Returns true; original mode restored before return |

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide Windows10Output class implementing IOutput interface (38 members: 35 methods + 3 properties, per `Stroke.Output.IOutput` as defined in `src/Stroke/Output/IOutput.cs`)
- **FR-002**: System MUST enable ENABLE_VIRTUAL_TERMINAL_PROCESSING console mode flag before calling Vt100Output.Flush() and restore the original mode after Flush() returns (in a finally block)
- **FR-003**: System MUST restore the original console mode after each flush operation completes (in a finally block), including when exceptions occur
- **FR-004**: System MUST delegate console-specific operations (GetSize, GetRowsBelowCursorPosition, ScrollBufferToPrompt) to Win32Output
- **FR-005**: System MUST delegate all rendering operations (Write, WriteRaw, cursor movement, colors, screen clearing, etc.) to Vt100Output
- **FR-006**: System MUST return false for RespondsToCpr property (CPR not needed on Windows; Windows does not require terminal-based cursor position queries)
- **FR-007**: System MUST return ColorDepth.Depth24Bit (true color) as default color depth when no explicit depth is specified
- **FR-008**: System MUST provide WindowsVt100Support.IsVt100Enabled() static method for VT100 support detection
- **FR-009**: System MUST apply [SupportedOSPlatform("windows")] attribute to both Windows10Output and WindowsVt100Support classes
- **FR-010**: System MUST use P/Invoke constants ENABLE_PROCESSED_INPUT (0x0001) and ENABLE_VIRTUAL_TERMINAL_PROCESSING (0x0004) with combined value 0x0005 during VT100 mode enabling
- **FR-011**: System MUST use per-instance locking via `System.Threading.Lock` (.NET 9+) with `EnterScope()` pattern to serialize flush operations within each Windows10Output instance (per Constitution XI thread safety requirement)
- **FR-012**: System MUST throw ArgumentNullException when stdout parameter is null
- **FR-013**: System MUST throw PlatformNotSupportedException on non-Windows platforms
- **FR-014**: System MUST propagate NoConsoleScreenBufferError from Win32Output constructor when no console buffer exists
- **FR-015**: System MUST store console output handle during construction (not re-acquire during each Flush)
- **FR-016**: System MUST expose Win32Output and Vt100Output properties for access to underlying outputs
- **FR-017**: System MUST delegate Encoding and Stdout properties to Vt100Output

### Key Entities

- **Windows10Output**: Hybrid IOutput implementation combining Win32Output (console operations) and Vt100Output (rendering) with VT100 mode switching during flush
- **WindowsVt100Support**: Static utility class for detecting VT100 escape sequence support on Windows
- **ConsoleMode**: Windows console mode flags that control input/output processing behavior

### IOutput Delegation Map (38 members)

All 38 IOutput interface members are delegated as follows:

| Member | Type | Delegate To | Rationale |
|--------|------|-------------|-----------|
| Write | Method | Vt100Output | VT100 escape sequence rendering |
| WriteRaw | Method | Vt100Output | VT100 escape sequence rendering |
| **Flush** | Method | **Custom** | VT100 mode enable → Vt100Output.Flush() → restore mode |
| EraseScreen | Method | Vt100Output | VT100 escape sequence (\x1b[2J) |
| EraseEndOfLine | Method | Vt100Output | VT100 escape sequence (\x1b[K) |
| EraseDown | Method | Vt100Output | VT100 escape sequence (\x1b[J) |
| EnterAlternateScreen | Method | Vt100Output | VT100 escape sequence (\x1b[?1049h) |
| QuitAlternateScreen | Method | Vt100Output | VT100 escape sequence (\x1b[?1049l) |
| CursorGoto | Method | Vt100Output | VT100 escape sequence (\x1b[row;colH) |
| CursorUp | Method | Vt100Output | VT100 escape sequence (\x1b[nA) |
| CursorDown | Method | Vt100Output | VT100 escape sequence (\x1b[nB) |
| CursorForward | Method | Vt100Output | VT100 escape sequence (\x1b[nC) |
| CursorBackward | Method | Vt100Output | VT100 escape sequence (\x1b[nD) |
| HideCursor | Method | Vt100Output | VT100 escape sequence (\x1b[?25l) |
| ShowCursor | Method | Vt100Output | VT100 escape sequence (\x1b[?25h) |
| SetCursorShape | Method | Vt100Output | VT100 escape sequence (DECSCUSR) |
| ResetCursorShape | Method | Vt100Output | VT100 escape sequence (DECSCUSR reset) |
| ResetAttributes | Method | Vt100Output | VT100 escape sequence (\x1b[0m) |
| SetAttributes | Method | Vt100Output | VT100 SGR escape sequences |
| DisableAutowrap | Method | Vt100Output | VT100 escape sequence (\x1b[?7l) |
| EnableAutowrap | Method | Vt100Output | VT100 escape sequence (\x1b[?7h) |
| EnableMouseSupport | Method | Win32Output | Windows console mouse event handling |
| DisableMouseSupport | Method | Win32Output | Windows console mouse event handling |
| EnableBracketedPaste | Method | Win32Output | Windows console input mode handling |
| DisableBracketedPaste | Method | Win32Output | Windows console input mode handling |
| SetTitle | Method | Vt100Output | VT100 escape sequence (OSC 2) |
| ClearTitle | Method | Vt100Output | VT100 escape sequence (OSC 2 empty) |
| Bell | Method | Vt100Output | VT100 BEL character (0x07) |
| AskForCpr | Method | Vt100Output | VT100 escape sequence (\x1b[6n) |
| ResetCursorKeyMode | Method | Vt100Output | VT100 escape sequence (\x1b[?1l) |
| GetSize | Method | Win32Output | Win32 GetConsoleScreenBufferInfo API |
| Fileno | Method | Vt100Output | File descriptor from TextWriter |
| **GetDefaultColorDepth** | Method | **Custom** | Returns ColorDepth.Depth24Bit by default |
| ScrollBufferToPrompt | Method | Win32Output | Win32 SetConsoleWindowInfo API |
| GetRowsBelowCursorPosition | Method | Win32Output | Win32 GetConsoleScreenBufferInfo API |
| **RespondsToCpr** | Property | **Constant** | Returns false (Windows does not need CPR) |
| Encoding | Property | Vt100Output | Encoding from underlying TextWriter |
| Stdout | Property | Vt100Output | TextWriter reference |

**Summary**: 27 methods → Vt100Output, 6 methods → Win32Output, 2 methods + 1 property → Custom/Constant, 2 properties → Vt100Output

### Custom Implementation Details

#### Constructor: Windows10Output(TextWriter stdout, ColorDepth? defaultColorDepth = null)

**Behavior**:
1. Validate `stdout` is not null → throw `ArgumentNullException` if null
2. Validate running on Windows platform → throw `PlatformNotSupportedException` if not Windows
3. Acquire console output handle via `ConsoleApi.GetStdHandle(STD_OUTPUT_HANDLE)` and store in `_hconsole` field
4. Create `Win32Output` instance wrapping `stdout` and `defaultColorDepth` → may throw `NoConsoleScreenBufferError`
5. Create `Vt100Output` instance via `Vt100Output.FromPty(stdout, defaultColorDepth)`
6. Store optional `defaultColorDepth` override in `_defaultColorDepth` field
7. Initialize per-instance `Lock` for flush synchronization

#### Flush()

**Behavior**:
1. Acquire per-instance lock via `_lock.EnterScope()`
2. Get current console mode via `ConsoleApi.GetConsoleMode(_hconsole, out originalMode)`
3. If GetConsoleMode fails: skip VT100 mode switching, delegate directly to `_vt100Output.Flush()`, return
4. Set console mode to `ENABLE_PROCESSED_INPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING` (0x0005) via `SetConsoleMode`
5. In try block: delegate to `_vt100Output.Flush()`
6. In finally block: restore `originalMode` via `SetConsoleMode` (always, even if step 5 throws)
7. Lock is automatically released via `using` pattern

#### RespondsToCpr (property)

**Behavior**: Always returns `false`. Windows does not require terminal-based Cursor Position Report queries because cursor position is available directly via Win32 API `GetConsoleScreenBufferInfo`.

#### GetDefaultColorDepth()

**Behavior**: Returns `_defaultColorDepth ?? ColorDepth.Depth24Bit`. Windows 10 has supported 24-bit true color since the Windows 10 Threshold 2 update (build 10586, November 2015). If caller provided an override via constructor, that value is returned instead.

### Public Properties

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| Win32Output | Win32Output | get | Underlying Win32 console output for advanced scenarios |
| Vt100Output | Vt100Output | get | Underlying VT100 terminal output for advanced scenarios |
| RespondsToCpr | bool | get | Always returns false (IOutput implementation) |
| Encoding | string | get | Delegates to Vt100Output.Encoding |
| Stdout | TextWriter? | get | Delegates to Vt100Output.Stdout |

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 38 IOutput interface members (35 methods + 3 properties) are implemented with delegation verified by: (a) code inspection confirming each member delegates to the target specified in the Delegation Map, (b) unit tests calling each member and verifying the expected underlying output receives the call
- **SC-002**: Console mode save/restore is verified by unit tests that: (a) confirm GetConsoleMode is called before SetConsoleMode, (b) confirm SetConsoleMode restores original mode in finally block even when Vt100Output.Flush() throws, (c) confirm mode is restored after normal completion
- **SC-003**: IsVt100Enabled() is verified by unit tests that: (a) confirm it returns true when SetConsoleMode succeeds, (b) confirm it returns false when GetConsoleMode fails, (c) confirm original console mode is restored after the check (no side effects)
- **SC-004**: Unit tests achieve 80% line coverage for Windows10Output and WindowsVt100Support classes, with coverage specifically including: constructor validation paths, Flush() lock acquisition, GetConsoleMode failure path, SetConsoleMode during Flush, finally block execution
- **SC-005**: Windows10Output passes IOutput interface compliance tests demonstrating interchangeability: (a) can be assigned to IOutput variable, (b) can be passed to methods accepting IOutput parameter, (c) behaves equivalently to other IOutput implementations for common operations

## Clarifications

### Session 2026-02-03

- Q: What thread safety guarantee for concurrent flush operations? → A: Per-instance lock - each Windows10Output instance serializes its own flush operations

## Platform Compatibility

### Minimum Windows Version

| Requirement | Value | Rationale |
|-------------|-------|-----------|
| Minimum Windows build | 10586 (Threshold 2) | First build supporting ENABLE_VIRTUAL_TERMINAL_PROCESSING |
| Release date | November 2015 | Over 10 years ago; safe baseline |
| Runtime check | `WindowsVt100Support.IsVt100Enabled()` | Dynamically detect support |

### Platform Attribute Requirements

- **Windows10Output**: `[SupportedOSPlatform("windows")]` required
- **WindowsVt100Support**: `[SupportedOSPlatform("windows")]` required
- Both classes will produce compiler warnings if referenced from non-Windows code

### Behavior on Unsupported Windows Versions (pre-10586)

| Scenario | Behavior |
|----------|----------|
| Constructor called | `NoConsoleScreenBufferError` may be thrown if console handle invalid |
| `IsVt100Enabled()` called | Returns false (SetConsoleMode fails) |
| Recommended pattern | Check `IsVt100Enabled()` before constructing Windows10Output |

### True Color (24-bit) Validation

True color support is assumed (not validated at runtime) because:
1. Windows 10 Threshold 2 (build 10586) introduced both VT100 and true color support simultaneously
2. If VT100 is supported, true color is also supported
3. `IsVt100Enabled()` returning true implies true color availability

### Terminal Application Differences

| Terminal | VT100 Support | True Color | Notes |
|----------|---------------|------------|-------|
| Windows Terminal | Full | Yes | Best experience; VT100 always enabled |
| cmd.exe (Win10+) | Full | Yes | Requires mode flag enabling (what this class does) |
| PowerShell (Win10+) | Full | Yes | Same as cmd.exe |
| ConEmu/Cmder | Full | Yes | Use ConEmuOutput instead; VT100 always enabled |
| Windows Console (pre-Win10) | None | No | Use Win32Output fallback |

## Integration Requirements

### OutputFactory Integration

The OutputFactory (or equivalent output selection logic) SHOULD select Windows10Output when:
1. `PlatformUtils.IsWindows` is true, AND
2. `PlatformUtils.IsConEmuAnsi` is false (not running in ConEmu), AND
3. `WindowsVt100Support.IsVt100Enabled()` returns true

**Selection Priority** (highest to lowest):
1. Non-Windows → `Vt100Output.FromPty()`
2. ConEmu detected → `ConEmuOutput`
3. VT100 supported → `Windows10Output`
4. Fallback → `Win32Output`

### WindowsVt100Support Relationship

`WindowsVt100Support.IsVt100Enabled()` delegates to `PlatformUtils.IsWindowsVt100Supported` for implementation consistency:

```csharp
public static bool IsVt100Enabled() => PlatformUtils.IsWindowsVt100Supported;
```

This ensures a single source of truth for VT100 detection logic.

### Constructor Parameter Forwarding

| Parameter | Forwarded To | Notes |
|-----------|--------------|-------|
| `stdout` | `Win32Output(stdout)`, `Vt100Output.FromPty(stdout)` | Same TextWriter for both |
| `defaultColorDepth` | `Win32Output(defaultColorDepth: ...)`, `Vt100Output.FromPty(defaultColorDepth: ...)` | Same value for both; also stored in `_defaultColorDepth` |

### P/Invoke Dependencies

The following P/Invoke methods from `Stroke.Input.Windows.ConsoleApi` are used:

| Method | Purpose |
|--------|---------|
| `GetStdHandle(int)` | Obtain console output handle (STD_OUTPUT_HANDLE = -11) |
| `GetConsoleMode(nint, out uint)` | Read current console mode flags |
| `SetConsoleMode(nint, uint)` | Set console mode flags (enable/restore VT100) |

Constants used from `Stroke.Input.Windows.ConsoleApi`:
- `ENABLE_PROCESSED_INPUT` (0x0001)
- `ENABLE_VIRTUAL_TERMINAL_PROCESSING` (0x0004)
- `STD_OUTPUT_HANDLE` (-11)

## Assumptions

- The existing Win32Output and Vt100Output classes are already implemented and tested
- The ConsoleApi P/Invoke methods (GetStdHandle, GetConsoleMode, SetConsoleMode) are already available
- STD_OUTPUT_HANDLE constant (-11) is defined in ConsoleApi
- PlatformUtils.IsWindowsVt100Supported property exists and functions correctly
