# Feature Specification: Input System

**Feature Branch**: `014-input-system`
**Created**: 2026-01-25
**Status**: Draft
**Input**: User description: "Implement the input abstraction layer for reading keyboard and mouse input from terminals, with support for VT100 parsing, raw/cooked modes, and platform-specific backends"

## Clarifications

### Session 2026-01-25

- Q: When input is in raw mode, should Ctrl+C generate a key press event or terminate the process? → A: Ctrl+C produces a key press event (application handles it)
- Q: Should input reading operations be thread-safe? → A: Single-threaded access assumed at reader level; thread safety belongs at event distribution layer

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Read Keyboard Input (Priority: P1)

As a terminal application developer, I need to read keyboard input from users character-by-character so that I can build interactive command-line interfaces that respond immediately to key presses without waiting for Enter.

**Why this priority**: This is the fundamental capability - without reading keyboard input, no interactive terminal application can function. Every other feature depends on this.

**Independent Test**: Can be fully tested by creating an input source, sending key data, and verifying that parsed key presses are returned correctly.

**Acceptance Scenarios**:

1. **Given** a terminal input source is attached, **When** the user presses a regular character key (e.g., 'a'), **Then** the application receives a key press event identifying that character
2. **Given** a terminal input source is attached, **When** the user presses a special key (e.g., Enter, Tab, Backspace), **Then** the application receives a key press event identifying that special key
3. **Given** a terminal input source is attached, **When** the user presses a function key (e.g., F1-F12), **Then** the application receives a key press event identifying that function key
4. **Given** a terminal input source is attached, **When** the user presses an arrow key, **Then** the application receives a key press event identifying the navigation direction

---

### User Story 2 - Parse VT100 Escape Sequences (Priority: P1)

As a terminal application developer, I need the input system to automatically parse VT100/ANSI escape sequences into meaningful key press events so that I don't have to implement low-level terminal protocol handling myself.

**Why this priority**: VT100 sequences are the standard way terminals communicate special keys. Without parsing these, applications cannot detect arrow keys, function keys, or modifier combinations.

**Independent Test**: Can be fully tested by feeding raw escape sequence bytes and verifying the correct key press events are produced.

**Acceptance Scenarios**:

1. **Given** a VT100 input parser, **When** it receives the escape sequence for Up Arrow, **Then** it produces a key press event for the Up key
2. **Given** a VT100 input parser, **When** it receives a multi-character escape sequence, **Then** it buffers characters until the complete sequence is recognized
3. **Given** a VT100 input parser with a partial escape sequence buffered, **When** a timeout occurs (flush), **Then** it processes the buffered characters as individual keys (enabling standalone Escape key detection)
4. **Given** a VT100 input parser, **When** it receives the bracketed paste start sequence, **Then** it enters paste mode and captures all content until the end sequence

---

### User Story 3 - Raw Mode Terminal Control (Priority: P1)

As a terminal application developer, I need to put the terminal into raw mode so that the application receives each key press immediately without line buffering or echo, enabling real-time interactive behavior.

**Why this priority**: Interactive applications require raw mode to function. Without it, users must press Enter after each input, making features like incremental search or real-time completion impossible.

**Independent Test**: Can be fully tested by entering raw mode, typing characters, and verifying they are received immediately without echo.

**Acceptance Scenarios**:

1. **Given** a terminal in normal (cooked) mode, **When** raw mode is entered, **Then** terminal echo is disabled and characters are available immediately without line buffering
2. **Given** a terminal in raw mode, **When** raw mode is exited (context disposed), **Then** the terminal returns to its previous settings
3. **Given** raw mode entry fails (e.g., not a TTY), **When** the application attempts to enter raw mode, **Then** it gracefully handles the failure without crashing

---

### User Story 4 - Cooked Mode Restoration (Priority: P2)

As a terminal application developer, I need to temporarily restore cooked mode while in raw mode so that I can run subprocesses or prompt for input that expects normal terminal behavior.

**Why this priority**: Applications often need to shell out to external commands or use standard input functions that expect cooked mode. This enables seamless integration with the broader system.

**Independent Test**: Can be fully tested by entering raw mode, then cooked mode, verifying terminal behavior matches cooked expectations, then exiting both.

**Acceptance Scenarios**:

1. **Given** a terminal in raw mode, **When** cooked mode is temporarily entered, **Then** terminal echo is re-enabled and line buffering is restored
2. **Given** cooked mode was entered from raw mode, **When** cooked mode is exited, **Then** raw mode settings are restored

---

### User Story 5 - Cross-Platform Input Support (Priority: P2)

As a terminal application developer, I need the input system to work correctly on Windows, macOS, and Linux so that my application is portable across operating systems.

**Why this priority**: Cross-platform support is essential for a library targeting .NET, which runs on all major operating systems. Without this, the library's utility is severely limited.

**Independent Test**: Can be tested on each platform by verifying key input is correctly received and parsed.

**Acceptance Scenarios**:

1. **Given** the application runs on a POSIX system (macOS/Linux), **When** input is created, **Then** VT100 input handling is used
2. **Given** the application runs on Windows, **When** input is created, **Then** Windows Console input handling is used
3. **Given** no valid terminal is available, **When** input is created, **Then** a dummy input is returned that signals EOF

---

### User Story 6 - Pipe Input for Testing (Priority: P2)

As a developer writing tests for a terminal application, I need to programmatically send input data to the application so that I can automate testing without requiring a real terminal.

**Why this priority**: Automated testing is critical for library quality. Without pipe input, tests would require manual interaction or complex terminal simulation.

**Independent Test**: Can be fully tested by creating a pipe input, sending text/bytes, and verifying the application receives the expected key presses.

**Acceptance Scenarios**:

1. **Given** a pipe input is created, **When** text is sent to the pipe, **Then** the application receives corresponding key press events
2. **Given** a pipe input is created, **When** raw bytes (including escape sequences) are sent, **Then** the application receives correctly parsed key press events
3. **Given** a pipe input is created, **When** it is closed, **Then** the application is notified of EOF

---

### User Story 7 - Event Loop Integration (Priority: P3)

As a terminal application developer, I need input sources to integrate with async event loops so that my application can efficiently wait for input while performing other async operations.

**Why this priority**: Modern .NET applications use async patterns extensively. Event loop integration enables efficient resource usage without busy-waiting.

**Independent Test**: Can be tested by attaching input to an event loop and verifying callbacks are invoked when input is available.

**Acceptance Scenarios**:

1. **Given** an input source, **When** it is attached to an event loop with a callback, **Then** the callback is invoked when input becomes available
2. **Given** an attached input source, **When** it is detached, **Then** it no longer triggers callbacks
3. **Given** an attached input source, **When** it is closed, **Then** it is automatically removed from the event loop

---

### User Story 8 - Mouse Event Detection (Priority: P3)

As a terminal application developer, I need to receive mouse events (clicks, scrolls) so that I can build applications with mouse-based interaction in terminals that support it.

**Why this priority**: Mouse support enhances usability for modern terminal emulators but is not required for basic functionality. Applications can be fully keyboard-driven.

**Independent Test**: Can be tested by sending mouse event escape sequences and verifying mouse events are detected.

**Acceptance Scenarios**:

1. **Given** a terminal that supports mouse reporting, **When** the user clicks, **Then** a mouse event key press is produced
2. **Given** mouse event escape sequences in different protocols (X10, SGR, urxvt), **When** they are received, **Then** they are recognized as mouse events

---

### Edge Cases

- What happens when stdin is redirected from a file or /dev/null?
  - The system should detect this and return a dummy input that signals EOF
- What happens when the Escape key is pressed alone (not part of a sequence)?
  - After a timeout (flush), the Escape should be recognized as a standalone key
  - Recommended flush timeout: 50-100ms after last input
- What happens when incomplete escape sequences are received?
  - Flush processing should emit what can be determined and pass through unrecognized characters
- What happens when the terminal is closed while input is attached?
  - The input should report as closed and callbacks should handle this gracefully
- What happens when raw mode is entered on a non-TTY file descriptor?
  - The operation should fail gracefully without crashing, or return a no-op context
- What happens when Ctrl+C is pressed in raw mode?
  - Ctrl+C produces a key press event (Keys.ControlC) that the application handles; it does not generate a process-terminating signal
- What happens when the terminal is resized during a read operation?
  - On POSIX: SIGWINCH may interrupt read (handled via EINTR retry)
  - Terminal resize does not affect input parsing; resize events are handled separately via output system
- What happens when terminal encoding changes mid-session?
  - The parser assumes UTF-8 throughout; encoding changes are not detected or handled
  - Applications should not change terminal encoding during input sessions
- What happens when stdin is a regular file?
  - File is read until EOF; `DummyInput` is NOT returned
  - However, raw mode operations return no-op contexts (files don't have terminal modes)
- What happens on memory exhaustion during paste accumulation?
  - Standard .NET `OutOfMemoryException` is thrown; no special handling
  - Callers expecting large pastes should consider memory constraints
- What happens when `Close()` is called during a blocked `ReadKeys()`?
  - POSIX: The blocked read is interrupted (via close or signal); returns available data
  - Windows: The blocked read returns immediately with empty list
  - In both cases, `Closed` becomes true after the call completes
- What happens when `Attach()` is called multiple times?
  - Multiple attachments are supported; they form a stack
  - Detaching restores the previous callback (LIFO order)
- What happens when `Detach()` is called when not attached?
  - Returns a no-op disposable; no error is thrown

### Non-Functional Requirements

- **NFR-001**: Raw mode entry/exit MUST complete within 10ms under normal system load
- **NFR-002**: Escape sequence lookup MUST be O(1) using FrozenDictionary
- **NFR-003**: Single character input MUST NOT allocate (steady-state zero allocation)
- **NFR-004**: Parser buffer reuse MUST minimize GC pressure during high-throughput input
- **NFR-005**: PipeInput MUST support sustained input rates of 10,000+ key presses per second

### Testing Strategy

**Unit Test Coverage**:
- Target: 80% code coverage minimum
- VT100 parser can achieve near-100% coverage via `FeedAndFlush` with crafted sequences
- Platform-specific code (P/Invoke) tested via integration tests on target platforms

**PipeInput Testing**:
- All acceptance scenarios MUST be testable using `PipeInput`
- `PipeInput` enables testing without real terminal access
- Platform-specific raw mode code requires actual terminal for full testing

**Platform-Specific Testing**:
- POSIX-specific tests run on Linux/macOS CI runners
- Windows-specific tests run on Windows CI runners
- Cross-platform tests use `PipeInput` and run on all platforms

**P/Invoke Testing Strategy**:
- P/Invoke wrappers (Termios, ConsoleApi) are thin and tested indirectly
- Integration tests verify end-to-end behavior (enter raw mode, send input, verify parsing)
- Mocking is NOT used (per Constitution VIII); real terminal APIs are exercised

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an abstraction for reading keyboard input from terminals
- **FR-002**: System MUST parse VT100/ANSI escape sequences into meaningful key press events
- **FR-003**: System MUST support entering raw mode to disable line buffering and echo
- **FR-004**: System MUST support restoring cooked mode for subprocess execution
- **FR-005**: System MUST provide a factory that creates the appropriate input type for the current platform
- **FR-006**: System MUST provide a dummy input implementation for non-terminal scenarios
- **FR-007**: System MUST provide pipe input for programmatic input during testing
- **FR-008**: System MUST support attaching input to async event loops with ready callbacks
- **FR-009**: System MUST support detaching input from event loops
- **FR-010**: System MUST flush pending escape sequences after timeout to enable standalone Escape detection
- **FR-011**: System MUST handle bracketed paste mode (capture pasted content as a single event)
- **FR-012**: System MUST recognize mouse event escape sequences (X10, SGR, urxvt protocols)
- **FR-013**: System MUST provide a typeahead hash for identifying input sources (hash is composed of input type name and file descriptor/handle, ensuring uniqueness per input instance)
- **FR-014**: System MUST preserve raw input data alongside parsed key identity
- **FR-015**: System MUST gracefully handle terminal errors without crashing
- **FR-016**: System MUST disable signal generation (ISIG) in raw mode so that Ctrl+C produces a key press event rather than terminating the process
- **FR-017**: System MUST support modifier key combinations (Ctrl+Arrow, Shift+Function keys, Alt+keys)
- **FR-018**: System MUST emit mouse event sequences as `Keys.Vt100MouseEvent` with raw data; parsing into `MouseEvent` record is caller responsibility (see Mouse Event Mapping section)
- **FR-019**: System MUST handle nested RawMode/CookedMode contexts using a reference-counting or stack-based approach
- **FR-020**: System MUST automatically retry read operations interrupted by signals (EINTR on POSIX)

### Key Entities

- **KeyPress**: Represents a single key press event, containing both the logical key identity and the raw input data received. Uses `Keys.Any` for regular character input where the character is stored in `Data`.
- **Input**: The abstract base representing any input source with methods for reading keys and managing terminal modes
- **PipeInput**: A specialized input that allows programmatic feeding of data for testing. Thread-safe: `SendBytes`/`SendText` may be called from any thread while `ReadKeys` is called from the reader thread.
- **DummyInput**: A no-op input that immediately signals EOF, used when no real terminal is available
- **Vt100Input**: The POSIX implementation using VT100 escape sequence parsing
- **Win32Input**: The Windows implementation using Console APIs with VT100 mode on Windows 10+ or legacy mode on older versions
- **Vt100Parser**: A state machine that converts raw VT100 input bytes into key press events
- **RawModeContext**: A disposable that manages raw terminal mode, restoring previous settings on disposal
- **CookedModeContext**: A disposable that manages cooked terminal mode, restoring previous settings on disposal

### Platform-Specific Requirements

#### POSIX (Linux/macOS)

**Raw Mode termios Flags**:
| Flag | Setting | Purpose |
|------|---------|---------|
| ECHO | OFF | Disable character echo |
| ICANON | OFF | Disable canonical (line) mode |
| ISIG | OFF | Disable signal generation (Ctrl+C, Ctrl+Z) |
| IEXTEN | OFF | Disable extended input processing |
| IXON | OFF | Disable XON/XOFF flow control |
| ICRNL | OFF | Disable CR-to-NL translation |
| INLCR | OFF | Disable NL-to-CR translation |
| IGNCR | OFF | Don't ignore CR |
| OPOST | OFF | Disable output processing |
| VMIN | 1 | Minimum characters for read |
| VTIME | 0 | No timeout |

**File Descriptor Requirements**:
- Event loop integration requires a valid file descriptor from `FileNo()`
- File descriptors must support `select()`, `poll()`, or `epoll()` for async notification
- Non-blocking mode is set when attaching to event loop

**EINTR Handling**:
- Read operations interrupted by signals (EINTR) MUST be automatically retried
- This is transparent to the caller

#### Windows

**Console Mode Flags for Raw Mode**:
| Flag | Setting | Purpose |
|------|---------|---------|
| ENABLE_ECHO_INPUT | OFF | Disable character echo |
| ENABLE_LINE_INPUT | OFF | Disable line buffering |
| ENABLE_PROCESSED_INPUT | OFF | Disable Ctrl+C signal |
| ENABLE_VIRTUAL_TERMINAL_INPUT | ON (Win10+) | Enable VT100 input sequences |

**VT100 Mode Selection** (Windows 10+):
- If `ENABLE_VIRTUAL_TERMINAL_INPUT` is supported, use VT100 parsing (same as POSIX)
- If not supported (older Windows), use legacy `ReadConsoleInput` with `KEY_EVENT` records
- Detection occurs at input creation time via `GetConsoleMode`/`SetConsoleMode`

**Legacy Windows Key Codes**:
- Legacy mode translates `KEY_EVENT` virtual key codes to `Keys` enum values
- Arrow keys: VK_UP (0x26), VK_DOWN (0x28), VK_LEFT (0x25), VK_RIGHT (0x27)
- Function keys: VK_F1-VK_F12 (0x70-0x7B)
- Modifiers detected via `dwControlKeyState` field

**Console API Unavailable**:
- If `GetStdHandle(STD_INPUT_HANDLE)` returns `INVALID_HANDLE_VALUE`, return `DummyInput`
- If `GetConsoleMode` fails (redirected stdin), return `DummyInput`

### Parser Constraints

**Buffer Limits**:
- Maximum incomplete escape sequence buffer: 256 bytes
- If buffer exceeds limit, flush as individual characters and reset parser state
- Maximum bracketed paste content: No limit (bounded only by available memory)
- Nested or malformed paste sequences: Inner `\x1b[200~` sequences are treated as literal content

**Memory Management**:
- Parser reuses internal buffer for sequence accumulation
- Completed sequences are emitted immediately; buffer is cleared
- No allocation during steady-state character input (single characters)

**Unrecognized Sequences**:
- Unrecognized CSI sequences emit `Keys.Escape` followed by literal characters
- OSC/DCS strings are consumed and discarded (no key event emitted)

### Mouse Event Mapping

Mouse escape sequences produce `KeyPress` with `Key = Keys.Vt100MouseEvent` and `Data` containing the raw sequence. The caller is responsible for parsing `Data` into a `MouseEvent` record (from Feature 013) using the appropriate protocol decoder:

| Protocol | Start Sequence | Coordinate Encoding |
|----------|---------------|---------------------|
| X10 | `\x1b[M` | 3 bytes: button, x+32, y+32 (max 223) |
| SGR | `\x1b[<` | Decimal params: `button;x;y` + `M` or `m` |
| urxvt | `\x1b[` | Decimal params: `button;x;y` + `M` |

**Coordinate Encoding**:
- X10: Limited to coordinates 1-223 (encoded as value+32 in single byte)
- SGR/urxvt: Unlimited coordinates (decimal encoding)
- All protocols use 1-based coordinates

### Thread Safety and Concurrency

**Single-Threaded Reader Contract**:
- `ReadKeys()` and `FlushKeys()` assume single-threaded access
- Concurrent calls to `ReadKeys()` result in undefined behavior
- This is documented but not enforced at runtime (no locking overhead)

**PipeInput Thread Safety**:
- `SendBytes()` and `SendText()` are thread-safe and may be called from any thread
- Data sent is immediately available to the next `ReadKeys()` call (no timing guarantees beyond visibility)
- High-volume input: PipeInput buffers all sent data; memory bounded only by available heap

**Mode Context Thread Safety**:
- `RawModeContext` and `CookedModeContext` may be disposed from any thread
- Disposal is idempotent (multiple dispose calls are safe)
- However, only one thread should manage mode transitions at a time (caller's responsibility)

**Close During ReadKeys**:
- If `Close()` is called while `ReadKeys()` is blocked, behavior is platform-dependent:
  - POSIX: Read returns with available data or empty list; `Closed` becomes true
  - Windows: Read returns immediately; `Closed` becomes true
- Applications should check `Closed` after each `ReadKeys()` call

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Applications can receive individual key presses without waiting for Enter
- **SC-002**: All standard VT100 escape sequences (arrow keys, function keys, modifiers) are correctly parsed
  - Arrow keys: Up, Down, Left, Right
  - Function keys: F1-F12
  - Navigation: Home, End, PageUp, PageDown, Insert, Delete
  - Modifiers: Ctrl+Arrow, Shift+Arrow, Alt+Arrow, Ctrl+Shift+Arrow combinations
- **SC-003**: The standalone Escape key is distinguishable from escape sequences within 100ms timeout
  - Measured via: Feed ESC, wait 100ms, Flush, verify Keys.Escape emitted
- **SC-004**: Raw mode entry and exit complete within 10ms
  - Measured via: Stopwatch around RawMode() and Dispose() calls, averaged over 100 iterations
- **SC-005**: Pipe input enables automated testing without real terminal access
  - Verified by: All acceptance scenarios pass using PipeInput only
- **SC-006**: Input works correctly on Windows, macOS, and Linux
  - Verified by: CI pipeline runs tests on all three platforms
- **SC-007**: Unit tests achieve 80% code coverage
  - Measured via: Coverlet with ReportGenerator in CI pipeline
- **SC-008**: Terminal settings are always restored after application exit (including crashes via disposal patterns)
  - Verified by: Dispose pattern tests, finalizer tests for emergency cleanup

## Assumptions

- The terminal emulator supports VT100/ANSI escape sequences (standard for all modern terminals)
- On POSIX systems, termios APIs are available for terminal mode control
- On Windows, Console APIs are available for input handling
- The .NET runtime provides access to file descriptors on POSIX systems via `SafeHandle.DangerousGetHandle()`
- Applications will use the disposable pattern correctly to ensure terminal restoration
- Input reading (ReadKeys, FlushKeys) assumes single-threaded access; thread safety is the responsibility of higher-level event distribution layers (channels, queues)
- Terminal encoding is UTF-8 and does not change during a session
- Mouse tracking is enabled/disabled by the output system; the input system only parses received mouse sequences

## Exception Handling

### IInput Exceptions

| Method | Exception | Condition |
|--------|-----------|-----------|
| `ReadKeys()` | `ObjectDisposedException` | Called after `Close()` or `Dispose()` |
| `FlushKeys()` | `ObjectDisposedException` | Called after `Close()` or `Dispose()` |
| `RawMode()` | None | Returns no-op disposable if not a TTY |
| `CookedMode()` | None | Returns no-op disposable if not a TTY |
| `Attach()` | `ArgumentNullException` | `inputReadyCallback` is null |
| `Attach()` | `ObjectDisposedException` | Called after `Close()` or `Dispose()` |
| `Detach()` | None | Returns no-op disposable if not attached |
| `FileNo()` | `NotSupportedException` | `DummyInput` (has no file descriptor) |
| `Close()` | None | Idempotent; multiple calls are safe |

### IPipeInput Exceptions

| Method | Exception | Condition |
|--------|-----------|-----------|
| `SendBytes()` | `ObjectDisposedException` | Called after `Close()` |
| `SendText()` | `ObjectDisposedException` | Called after `Close()` |
| `SendText()` | `ArgumentNullException` | `data` is null |

### InputFactory Exceptions

| Method | Exception | Condition |
|--------|-----------|-----------|
| `Create()` | None | Never throws; returns `DummyInput` on failure |
| `CreatePipe()` | `PlatformNotSupportedException` | Platform lacks pipe support (hypothetical) |

### Vt100Parser Exceptions

| Method | Exception | Condition |
|--------|-----------|-----------|
| Constructor | `ArgumentNullException` | `feedKeyCallback` is null |
| `Feed()` | None | Never throws; invalid sequences handled gracefully |
| `Flush()` | None | Never throws |
| `Reset()` | None | Never throws |

## Keys.Any and Character Input

Regular printable characters (letters, digits, punctuation, Unicode) are represented as:
- `Key = Keys.Any`
- `Data = the character string (e.g., "a", "Z", "5", "@", "日")`

This differs from control characters and special keys which have dedicated `Keys` enum values:
- `Keys.ControlA` through `Keys.ControlZ` for Ctrl+letter combinations
- `Keys.Tab`, `Keys.Enter`, `Keys.Escape` for common special keys
- `Keys.Up`, `Keys.Down`, etc. for navigation keys

**Pattern Matching Example**:
```csharp
switch (keyPress.Key)
{
    case Keys.Any:
        // Regular character - check keyPress.Data
        ProcessCharacter(keyPress.Data);
        break;
    case Keys.ControlC:
        // Ctrl+C pressed
        HandleInterrupt();
        break;
    case Keys.Up:
        // Arrow key
        MoveUp();
        break;
}
```
