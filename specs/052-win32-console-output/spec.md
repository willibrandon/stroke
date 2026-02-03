# Feature Specification: Win32 Console Output

**Feature Branch**: `052-win32-console-output`
**Created**: 2026-02-02
**Status**: Draft
**Input**: User description: "Implement Windows Console output for legacy Windows terminals (cmd.exe) that don't fully support ANSI/VT100 escape sequences. Uses Win32 Console API for direct cursor control, color setting, and screen management."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Terminal Developer Uses Legacy Windows Console (Priority: P1)

A developer building a Stroke-based application needs to render terminal UI on a legacy Windows console (cmd.exe on older Windows versions) that does not support VT100 escape sequences. The application must display colored text, position the cursor correctly, and manage screen content using the native Win32 Console API.

**Why this priority**: This is the core functionality - without Win32Output, Stroke applications cannot work on legacy Windows terminals at all. This represents the fundamental capability the feature must deliver.

**Independent Test**: Can be fully tested by creating a Win32Output instance and verifying that Write, Flush, and basic cursor operations work on a Windows console without VT100 support.

**Acceptance Scenarios**:

1. **Given** a Windows console without VT100 support, **When** the application writes colored text, **Then** the text appears with the correct 16-color palette applied using Win32 text attributes.
2. **Given** a running Stroke application, **When** the application calls CursorGoto(5, 10), **Then** the cursor moves to row 5, column 10 using SetConsoleCursorPosition API.
3. **Given** a Win32Output instance, **When** Write is called with text, **Then** the text is buffered until Flush writes it to the console character-by-character.

---

### User Story 2 - Application Displays Styled Text with Color Mapping (Priority: P1)

A terminal application needs to display text with colors specified as ANSI color names (like "ansired") or RGB hex values (like "FF5544"). The system maps these colors to the nearest available Win32 console color from the 16-color palette.

**Why this priority**: Color rendering is essential for any meaningful terminal UI. Without accurate color mapping, applications appear incorrectly styled or unreadable.

**Independent Test**: Can be tested by setting various foreground and background colors and verifying the Win32 console attributes match expected values.

**Acceptance Scenarios**:

1. **Given** an ANSI color name "ansigreen", **When** SetAttributes is called, **Then** the Win32 foreground attribute is set to GREEN (0x0002).
2. **Given** an RGB hex color "FF0000", **When** SetAttributes is called, **Then** the closest Win32 color (RED with INTENSITY) is applied.
3. **Given** a color depth of 1-bit, **When** SetAttributes is called, **Then** colors are ignored and default attributes are used.
4. **Given** the reverse attribute is set, **When** SetAttributes is called, **Then** foreground and background colors are swapped.

---

### User Story 3 - Full-Screen Application Uses Alternate Buffer (Priority: P2)

A full-screen application (like a text editor or dialog) needs to enter an alternate screen buffer to preserve the original terminal content. When the application exits, the original content is restored.

**Why this priority**: Alternate screen support is important for full-screen applications but not required for basic prompts and simple UIs.

**Independent Test**: Can be tested by entering alternate screen, writing content, exiting, and verifying the original console content is preserved.

**Acceptance Scenarios**:

1. **Given** a normal console session, **When** EnterAlternateScreen is called, **Then** a new screen buffer is created and activated.
2. **Given** an active alternate screen, **When** QuitAlternateScreen is called, **Then** the original screen buffer is restored and the alternate buffer is closed.
3. **Given** already in alternate screen, **When** EnterAlternateScreen is called again, **Then** no action is taken (idempotent).

---

### User Story 4 - Application Clears and Erases Screen Content (Priority: P2)

An application needs to clear portions of the screen for UI updates - either the entire screen, from cursor to end of screen, or from cursor to end of the current line.

**Why this priority**: Screen manipulation is essential for dynamic UIs but applications can function with just write and cursor operations.

**Independent Test**: Can be tested by writing content, calling erase methods, and verifying the expected regions are cleared.

**Acceptance Scenarios**:

1. **Given** a screen with content, **When** EraseScreen is called, **Then** the entire screen is filled with spaces and the cursor moves to home position.
2. **Given** cursor at row 5 column 10, **When** EraseEndOfLine is called, **Then** characters from column 10 to end of line 5 are cleared.
3. **Given** cursor at row 5, **When** EraseDown is called, **Then** all content from row 5 to the bottom of the screen is cleared.

---

### User Story 5 - Application Enables Mouse Input (Priority: P3)

An interactive application needs to receive mouse click and scroll events for UI interaction. Mouse support is enabled via the Win32 console input mode flags.

**Why this priority**: Mouse support is an enhancement for interactive applications but not required for basic terminal functionality.

**Independent Test**: Can be tested by enabling mouse support and verifying the console input mode flags are correctly set.

**Acceptance Scenarios**:

1. **Given** mouse support is disabled, **When** EnableMouseSupport is called, **Then** ENABLE_MOUSE_INPUT flag is set and ENABLE_QUICK_EDIT_MODE is cleared.
2. **Given** mouse support is enabled, **When** DisableMouseSupport is called, **Then** ENABLE_MOUSE_INPUT flag is cleared.

---

### User Story 6 - Platform Detection and Graceful Failure (Priority: P1)

When an application attempts to use Win32Output on a non-Windows platform, or on Windows outside a console context, clear error messages guide the developer to the correct solution.

**Why this priority**: Clear error handling prevents confusion and helps developers quickly identify platform issues.

**Independent Test**: Can be tested by attempting to create Win32Output in various contexts and verifying appropriate errors are raised.

**Acceptance Scenarios**:

1. **Given** running on Linux or macOS, **When** Win32Output is instantiated, **Then** a PlatformNotSupportedException is thrown.
2. **Given** running on Windows outside a console (e.g., in a GUI app), **When** Win32Output is instantiated, **Then** NoConsoleScreenBufferError is thrown with a helpful message.
3. **Given** running in Git Bash or similar terminal emulator, **When** Win32Output is instantiated, **Then** the error message suggests using winpty or cmd.exe.

---

### Edge Cases

**Terminal State:**
- What happens when terminal is resized during operation? GetSize returns updated dimensions on next call.
- What happens if console handle becomes invalid mid-operation? P/Invoke calls fail; errors propagated or logged.

**Text Output:**
- How does the system handle hidden text (password input)? Write outputs spaces matching the character width via UnicodeWidth.
- What happens when Write is called with null? No-op (no error, no output).
- What happens when Write is called with empty string? No-op (no error, no output).
- How does WriteConsole handle wide (CJK) characters? Unicode characters are written correctly; width calculation uses UnicodeWidth.

**Cursor Movement:**
- What happens when cursor movement would go outside screen bounds? Positions are clamped to valid ranges (0 to buffer dimension - 1).
- What happens when CursorUp/Down/Forward/Backward is called with zero? No-op.
- What happens when CursorUp/Down/Forward/Backward is called with negative amount? No-op (treated as zero).

**Color Handling:**
- What happens when an invalid color string is provided? Falls back to black (0x0000) for foreground.
- What happens when RGB string has # prefix? Strip prefix and parse; if still invalid, fall back to black.
- What happens when RGB string is wrong length (not 6 chars)? Fall back to black.
- What happens when RGB string contains non-hex characters? Fall back to black.

**Attribute Handling:**
- What happens when bold/underline/italic/blink attributes are set? Ignored (not supported by Win32 console).
- What happens when reverse attribute is set? Foreground and background color bits are swapped.

**Alternate Screen:**
- What happens when EnterAlternateScreen is called while already in alternate screen? No-op (idempotent).
- What happens when QuitAlternateScreen is called while not in alternate screen? No-op (idempotent).
- What happens if alternate screen buffer creation fails? NoConsoleScreenBufferError or Win32 error propagated.

**Bell:**
- What happens when Bell() is called? Emits console beep via '\a' character write or MessageBeep API.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST implement the `IOutput` interface for Windows console output using Win32 Console API.
- **FR-002**: System MUST support writing text to the console buffer with character-by-character output to avoid rendering artifacts.
- **FR-003**: System MUST map 16 ANSI named colors to corresponding Win32 console color attributes.
- **FR-004**: System MUST map arbitrary RGB hex colors to the closest Win32 16-color palette entry using squared-distance matching.
- **FR-005**: System MUST support cursor positioning using SetConsoleCursorPosition.
- **FR-006**: System MUST support relative cursor movement (up, down, forward, backward).
- **FR-007**: System MUST support screen erase operations (full screen, to end of screen, to end of line).
- **FR-008**: System MUST support alternate screen buffer via CreateConsoleScreenBuffer and SetConsoleActiveScreenBuffer.
- **FR-009**: System MUST support setting terminal title via SetConsoleTitleW.
- **FR-010**: System MUST support mouse input enable/disable via console input mode flags.
- **FR-011**: System MUST cache RGB-to-Win32-color mappings for performance.
- **FR-012**: System MUST throw NoConsoleScreenBufferError when not running in a Windows console.
- **FR-013**: System MUST throw PlatformNotSupportedException when running on non-Windows platforms.
- **FR-014**: System MUST report terminal size based on the visible window region (or complete buffer width when configured).
- **FR-015**: System MUST support the hidden attribute by outputting spaces instead of actual characters.
- **FR-016**: System MUST support the reverse attribute by swapping foreground and background color bits.
- **FR-017**: System MUST return 4-bit color depth as the default for Win32 console.
- **FR-018**: System MUST provide a static Win32RefreshWindow method to force console repaint (workaround for console bugs).
- **FR-019**: System MUST be thread-safe per Constitution XI; all mutable state protected by `System.Threading.Lock`.
- **FR-020**: System MUST implement all IOutput interface methods, with unsupported operations implemented as documented no-ops.

### Key Entities

- **Win32Output**: The main IOutput implementation that wraps Win32 Console API calls for rendering.
- **ColorLookupTable**: Internal color mapper that converts ANSI color names and RGB hex values to Win32 console color attributes.
- **NoConsoleScreenBufferError**: Exception thrown when Win32Output cannot access a valid console screen buffer.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All IOutput interface methods are implemented and functional on Windows. *Verified by: Interface implementation compiles; all methods callable.*
- **SC-002**: Text with any of the 16 ANSI color names renders with the correct Win32 color attribute. *Verified by: Unit tests comparing expected vs actual attribute values.*
- **SC-003**: Applications display correctly on Windows cmd.exe without VT100 support enabled. *Verified by: Manual testing on legacy cmd.exe.*
- **SC-004**: Alternate screen buffer preserves and restores original console content. *Verified by: TUI Driver capture before/after test.*
- **SC-005**: Color lookup for repeated RGB values uses cached results (no redundant calculations). *Verified by: Debug logging or cache hit counter.*
- **SC-006**: Cursor operations position the cursor within 1ms response time. *Verified by: Stopwatch measurement in performance test (optional benchmark).*
- **SC-007**: Unit tests achieve at least 80% code coverage for Win32Output and ColorLookupTable. *Verified by: `dotnet test --collect:"XPlat Code Coverage"` with coverlet.*
- **SC-008**: Applications receive clear, actionable error messages when run outside a Windows console. *Verified by: Exception message contains "winpty" or "cmd.exe" suggestion.*

## Assumptions

- The existing `IOutput` interface in `Stroke.Output` is the target interface for implementation.
- Win32Types (Coord, SmallRect, ConsoleScreenBufferInfo, etc.) are already available in `Stroke.Input.Windows.Win32Types`.
- Some ConsoleApi P/Invoke methods (GetStdHandle, GetConsoleMode, SetConsoleMode, GetConsoleScreenBufferInfo, SetConsoleCursorPosition) are already available in `Stroke.Input.Windows.ConsoleApi`.
- Additional P/Invoke methods (SetConsoleTextAttribute, FillConsoleOutputCharacter, FillConsoleOutputAttribute, WriteConsoleW, SetConsoleTitleW, CreateConsoleScreenBuffer, SetConsoleActiveScreenBuffer, SetConsoleWindowInfo, GetConsoleWindow, RedrawWindow) will need to be added to ConsoleApi.
- The `UnicodeWidth` utility for character width calculation is already available.
- The `Attrs` record type for text attributes is already available in `Stroke.Styles`.
- The `ColorDepth` enum is already available in `Stroke.Output`.
