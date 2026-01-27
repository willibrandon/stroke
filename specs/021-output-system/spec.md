# Feature Specification: Output System

**Feature Branch**: `021-output-system`
**Created**: 2026-01-27
**Status**: Draft
**Input**: User description: "Feature 15: Output System - Implement the output abstraction layer for writing to terminals with support for VT100 escape sequences, cursor control, colors, and platform-specific backends."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Terminal Output with VT100 Escape Sequences (Priority: P1)

A terminal application developer needs to write formatted text to a terminal with proper cursor control, color output, and screen management using standard VT100/ANSI escape sequences.

**Why this priority**: This is the core functionality of the output system. Without VT100 output, no terminal rendering can occur on POSIX systems (Linux, macOS). This forms the foundation for all terminal UI rendering.

**Independent Test**: Can be fully tested by creating a VT100Output instance with a StringWriter and verifying that all escape sequences are correctly generated for cursor movement, colors, screen clearing, and other terminal operations.

**Acceptance Scenarios**:

1. **Given** a VT100 output connected to a terminal, **When** I call CursorGoto(5, 10), **Then** the escape sequence `\x1b[5;10H` is written to the output stream.
2. **Given** a VT100 output, **When** I call SetAttributes with red foreground and bold, **Then** the appropriate ANSI color codes are written based on the current color depth.
3. **Given** a VT100 output, **When** I call EraseScreen(), **Then** the escape sequence `\x1b[2J` is written to clear the screen.
4. **Given** a VT100 output, **When** I call Write("Hello\x1bWorld"), **Then** the text "Hello?World" is written with the escape character replaced.
5. **Given** a VT100 output, **When** I call WriteRaw("Hello\x1b[31m"), **Then** the text is written verbatim without modification.

---

### User Story 2 - Color Depth Management (Priority: P1)

A terminal application needs to detect and adapt to the color capabilities of the terminal (monochrome, 16 colors, 256 colors, or true color) to render appropriately formatted output.

**Why this priority**: Color depth detection is essential for proper rendering. Applications must adapt their color output based on terminal capabilities to avoid garbled output or missing colors.

**Independent Test**: Can be fully tested by setting environment variables (STROKE_COLOR_DEPTH, NO_COLOR) and verifying the color depth is correctly detected, and by verifying color code generation at each depth level.

**Acceptance Scenarios**:

1. **Given** the NO_COLOR environment variable is set, **When** I query color depth from environment, **Then** Depth1Bit (monochrome) is returned.
2. **Given** STROKE_COLOR_DEPTH is set to "DEPTH_24_BIT", **When** I query color depth from environment, **Then** Depth24Bit is returned.
3. **Given** a 256-color terminal, **When** I set an RGB color #FF5733, **Then** the closest 256-color palette entry is used.
4. **Given** a 16-color terminal with foreground #FF0000, **When** I set background color, **Then** the background is excluded from matching to prevent identical fg/bg.
5. **Given** a true color terminal, **When** I set an RGB color #FF5733, **Then** the exact RGB values are sent using 24-bit color escape sequences.

---

### User Story 3 - Cursor Shape Control (Priority: P2)

A terminal application needs to change the cursor appearance (block, beam, underline, with or without blinking) to provide visual feedback about the current editing mode or state.

**Why this priority**: Cursor shape control enhances user experience by providing visual cues (e.g., block cursor in Vi normal mode, beam cursor in insert mode). While valuable, the application can function without this feature.

**Independent Test**: Can be fully tested by calling SetCursorShape with various shapes and verifying the correct DECSCUSR escape sequences are generated.

**Acceptance Scenarios**:

1. **Given** a VT100 output, **When** I set cursor shape to Block, **Then** the escape sequence `\x1b[2 q` is written.
2. **Given** a VT100 output, **When** I set cursor shape to Beam, **Then** the escape sequence `\x1b[6 q` is written.
3. **Given** a VT100 output, **When** I set cursor shape to BlinkingBlock, **Then** the escape sequence `\x1b[1 q` is written.
4. **Given** cursor shape was changed, **When** I call ResetCursorShape, **Then** the escape sequence `\x1b[0 q` is written to restore defaults.
5. **Given** cursor shape was never changed, **When** I call ResetCursorShape, **Then** no escape sequence is written.

---

### User Story 4 - Terminal Feature Toggling (Priority: P2)

A terminal application needs to enable/disable terminal features like mouse support, alternate screen buffer, and bracketed paste mode.

**Why this priority**: These features enable full-screen applications and proper paste handling. They are essential for interactive applications but the core text output works without them.

**Independent Test**: Can be fully tested by calling feature toggle methods and verifying correct escape sequences are generated for each feature.

**Acceptance Scenarios**:

1. **Given** a VT100 output, **When** I call EnterAlternateScreen, **Then** the escape sequence `\x1b[?1049h\x1b[H` is written.
2. **Given** a VT100 output, **When** I call EnableMouseSupport, **Then** escape sequences for basic, drag, urxvt, and SGR mouse modes are written.
3. **Given** a VT100 output, **When** I call EnableBracketedPaste, **Then** the escape sequence `\x1b[?2004h` is written.
4. **Given** a VT100 output with alternate screen active, **When** I call QuitAlternateScreen, **Then** the escape sequence `\x1b[?1049l` is written.

---

### User Story 5 - Platform-Agnostic Output Factory (Priority: P2)

A terminal application needs to automatically get the appropriate output implementation for the current platform without knowing platform-specific details.

**Why this priority**: This simplifies cross-platform development by abstracting away platform detection. Applications can request output and receive the correct implementation automatically.

**Independent Test**: Can be fully tested by testing with various Console.IsOutputRedirected states and verifying appropriate output type is returned on different platforms.

**Acceptance Scenarios**:

1. **Given** stdout is a TTY on POSIX, **When** I call CreateOutput(), **Then** a Vt100Output instance is returned.
2. **Given** stdout is redirected to a file on POSIX, **When** I call CreateOutput(), **Then** a PlainTextOutput instance is returned.
3. **Given** stdout is null, **When** I call CreateOutput(), **Then** a DummyOutput instance is returned.
4. **Given** alwaysPreferTty is true and stdout is not a TTY but stderr is, **When** I call CreateOutput(), **Then** output is created for stderr.

---

### User Story 6 - Plain Text Output for Redirected Streams (Priority: P3)

A terminal application needs to write plain text without escape sequences when output is redirected to a file or pipe.

**Why this priority**: This enables tools to be used in pipelines and have their output redirected cleanly. Lower priority as most interactive applications require a real terminal.

**Independent Test**: Can be fully tested by creating PlainTextOutput with a StringWriter and verifying no escape sequences are written for any operation.

**Acceptance Scenarios**:

1. **Given** a PlainTextOutput, **When** I call Write("Hello"), **Then** "Hello" is written without modification.
2. **Given** a PlainTextOutput, **When** I call SetAttributes with colors and bold, **Then** nothing is written (no escape sequences).
3. **Given** a PlainTextOutput, **When** I call CursorForward(5), **Then** 5 spaces are written.
4. **Given** a PlainTextOutput, **When** I call CursorDown(1), **Then** a newline is written.

---

### User Story 7 - Testing with DummyOutput (Priority: P3)

A developer writing unit tests for terminal applications needs an output implementation that accepts all calls without producing any output.

**Why this priority**: Essential for testing but not for production use. Enables testing of application logic without terminal dependencies.

**Independent Test**: Can be fully tested by creating DummyOutput and verifying all methods execute without error and produce no output.

**Acceptance Scenarios**:

1. **Given** a DummyOutput, **When** I call any output method, **Then** the method completes without error.
2. **Given** a DummyOutput, **When** I call GetSize(), **Then** a default size (40 rows, 80 columns) is returned.
3. **Given** a DummyOutput, **When** I call Fileno(), **Then** NotImplementedException is thrown.
4. **Given** a DummyOutput, **When** I call GetDefaultColorDepth(), **Then** Depth1Bit is returned.

---

### User Story 8 - Thread-Safe Concurrent Output (Priority: P2)

A multi-threaded application needs to safely write to terminal output from multiple threads without data corruption or race conditions.

**Why this priority**: .NET applications commonly use async/await and parallel processing. Thread safety prevents subtle bugs and ensures reliable output in concurrent scenarios.

**Independent Test**: Can be fully tested by spawning multiple threads that call Write/Flush concurrently and verifying no exceptions or corrupted output.

**Acceptance Scenarios**:

1. **Given** a Vt100Output instance, **When** multiple threads call Write() concurrently, **Then** all writes are added to the buffer without corruption.
2. **Given** a Vt100Output instance, **When** one thread calls Flush() while another calls Write(), **Then** both operations complete without exception.
3. **Given** a Vt100Output instance, **When** concurrent HideCursor() and ShowCursor() calls occur, **Then** internal state remains consistent.
4. **Given** an EscapeCodeCache, **When** multiple threads request the same Attrs simultaneously, **Then** only one escape sequence is computed and all receive the cached result.

---

### User Story 9 - Cursor Shape Configuration (Priority: P3)

A terminal application needs to configure cursor shape based on editing mode (Vi navigation vs. insert) or other application state.

**Why this priority**: Cursor shape configuration enhances user experience but is optional for basic functionality.

**Independent Test**: Can be fully tested by creating cursor shape configs and verifying correct shapes are returned for different application states.

**Acceptance Scenarios**:

1. **Given** a SimpleCursorShapeConfig with Block, **When** GetCursorShape is called, **Then** Block is always returned.
2. **Given** a ModalCursorShapeConfig, **When** application is in Vi Navigation mode, **Then** Block is returned.
3. **Given** a ModalCursorShapeConfig, **When** application is in Vi Insert mode, **Then** Beam is returned.
4. **Given** a DynamicCursorShapeConfig with a custom function, **When** GetCursorShape is called, **Then** the function is invoked and its result is returned.

---

### Edge Cases

#### Cursor Movement
- **Amount = 0**: No escape sequence is written (no-op).
- **Amount = 1**: Optimized single-character sequence is used (e.g., `\x1b[A` instead of `\x1b[1A`).
- **Amount > 1**: Parameterized sequence is used (e.g., `\x1b[5A` for amount=5).
- **Negative amount**: Behavior is undefined; implementations MAY treat as 0 or use absolute value.

#### CursorGoto Edge Cases
- **Row/column = 0**: VT100 uses 1-based indexing; 0 is treated as 1.
- **Row/column exceeding terminal size**: Escape sequence is sent; terminal clips to boundaries.

#### Color Mapping
- **RGB values at boundaries (0 or 255)**: Mapped using standard Euclidean distance algorithm.
- **Grayscale RGB (r=g=b)**: In 16-color mode, mapped to white/gray/black based on luminosity.
- **Same fg/bg after mapping**: When foreground and background map to the same 16-color, the background mapping excludes that color to find the next closest match.
- **Invalid RGB values (outside 0-255)**: Clamped to valid range [0, 255].

#### Buffer and Flush
- **Flush with empty buffer**: No I/O operation occurs (no write call to stdout).
- **Multiple concurrent Flush calls**: Thread-safe; each flush is atomic but ordering is non-deterministic.

#### Terminal Title
- **Title contains \x1b (ESC)**: ESC characters are removed (not replaced).
- **Title contains \x07 (BEL)**: BEL characters are removed (not replaced).
- **Title exceeds terminal limit**: Title is sent as-is; terminal truncates if necessary.
- **Empty title**: ClearTitle() sets title to empty string using OSC sequence.

#### Screen and Cursor State
- **Repeated EnterAlternateScreen calls**: Each call writes the escape sequence; no state tracking prevents duplicates.
- **ResetCursorShape without prior SetCursorShape**: No escape sequence is written (flag tracks if shape was ever changed).
- **HideCursor when already hidden**: No escape sequence is written (state tracking prevents duplicates).
- **ShowCursor when already visible**: No escape sequence is written (state tracking prevents duplicates).

#### Input/Output Edge Cases
- **Write() with null**: Throws ArgumentNullException.
- **WriteRaw() with null**: Throws ArgumentNullException.
- **Write() with empty string**: Adds empty string to buffer (effectively no-op).
- **Write() with embedded null bytes (0x00)**: Null bytes are passed through without modification.
- **Fileno() on DummyOutput**: Throws NotImplementedException.
- **Fileno() on PlainTextOutput with non-file stream**: Throws NotImplementedException.

#### Bell
- **Bell() when bell is disabled**: No-op (no BEL character written).
- **Bell() with enableBell=true**: Writes BEL character (\x07) to output.

#### Terminal Resize During Output
- **GetSize() during resize**: Returns current size at time of call; may be stale if resize occurs during buffered output.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a ColorDepth enum with values for 1-bit (monochrome), 4-bit (16 colors), 8-bit (256 colors), and 24-bit (true color) depths.
- **FR-002**: System MUST provide a CursorShape enum with values for NeverChange, Block, Beam, Underline, BlinkingBlock, BlinkingBeam, and BlinkingUnderline.
- **FR-003**: System MUST detect color depth from environment variables (STROKE_COLOR_DEPTH, NO_COLOR).
- **FR-004**: System MUST provide an IOutput interface defining all terminal output operations.
- **FR-005**: System MUST provide a Vt100Output class that implements IOutput with VT100/ANSI escape sequences.
- **FR-006**: System MUST provide a PlainTextOutput class that writes text without escape sequences.
- **FR-007**: System MUST provide a DummyOutput class for testing that performs no actual output.
- **FR-008**: System MUST escape VT100 sequences in Write() method (replace \x1b with ?).
- **FR-009**: System MUST pass through text verbatim in WriteRaw() method.
- **FR-010**: System MUST cache escape code sequences for Attrs to avoid repeated string building.
- **FR-011**: System MUST map RGB colors to nearest 16-color ANSI color when in 4-bit mode.
- **FR-012**: System MUST map RGB colors to nearest 256-color palette entry when in 8-bit mode.
- **FR-013**: System MUST output exact RGB values when in 24-bit true color mode.
- **FR-014**: System MUST avoid matching foreground and background to the same ANSI color (when they were originally different).
- **FR-015**: System MUST track cursor visibility state to avoid redundant hide/show escape sequences.
- **FR-016**: System MUST track cursor shape change state to avoid resetting shape that was never changed.
- **FR-017**: System MUST support terminal title setting with proper escaping of control characters.
- **FR-018**: System MUST support alternate screen buffer enter/exit.
- **FR-019**: System MUST support mouse mode enable/disable (basic, drag, urxvt, SGR).
- **FR-020**: System MUST support bracketed paste mode enable/disable.
- **FR-021**: System MUST support cursor position report (CPR) request.
- **FR-022**: System MUST provide factory method to create appropriate output based on platform and terminal detection.
- **FR-023**: System MUST optimize cursor movement sequences (use single character for amount=1).
- **FR-024**: System MUST buffer output and flush to stdout only when Flush() is called.
- **FR-025**: System MUST provide thread-safe operation for all mutable output implementations.

### Key Entities

- **ColorDepth**: Represents the color capability level of a terminal (1-bit, 4-bit, 8-bit, 24-bit).
- **CursorShape**: Represents the visual appearance of the terminal cursor (block, beam, underline, with blinking variants).
- **IOutput**: Contract defining all terminal output operations (writing, cursor control, colors, screen management).
- **Vt100Output**: Terminal output using VT100/ANSI escape sequences for POSIX terminals.
- **PlainTextOutput**: Plain text output without escape sequences for redirected streams.
- **DummyOutput**: No-op output implementation for testing purposes.
- **AnsiColorCodes**: Mappings between ANSI color names and their escape code values.
- **EscapeCodeCache**: Cache that maps style attributes to pre-computed escape sequences.
- **SixteenColorCache**: Cache that maps RGB values to the nearest 16-color ANSI color.
- **TwoFiftySixColorCache**: Cache that maps RGB values to the nearest 256-color palette entry.
- **FlushStdout**: Helper class providing write-and-flush behavior for immediate output.
- **ICursorShapeConfig**: Interface for determining cursor shape based on application state.
- **SimpleCursorShapeConfig**: Returns a fixed cursor shape.
- **ModalCursorShapeConfig**: Returns cursor shape based on editing mode (Vi/Emacs).
- **DynamicCursorShapeConfig**: Wraps a function that returns cursor shape configuration.

### VT100 Escape Sequence Reference

| Operation | Escape Sequence | Notes |
|-----------|-----------------|-------|
| **Screen Control** | | |
| EraseScreen | `\x1b[2J` | Erase entire screen |
| EraseEndOfLine | `\x1b[K` | Erase to end of current line |
| EraseDown | `\x1b[J` | Erase from cursor to bottom |
| EnterAlternateScreen | `\x1b[?1049h\x1b[H` | Enter alternate + home cursor |
| QuitAlternateScreen | `\x1b[?1049l` | Exit alternate screen |
| **Cursor Movement** | | |
| CursorGoto(row, col) | `\x1b[{row};{col}H` | 1-based row/column |
| CursorUp(1) | `\x1b[A` | Optimized for n=1 |
| CursorUp(n) | `\x1b[{n}A` | n > 1 |
| CursorDown(1) | `\x1b[B` | Optimized for n=1 |
| CursorDown(n) | `\x1b[{n}B` | n > 1 |
| CursorForward(1) | `\x1b[C` | Optimized for n=1 |
| CursorForward(n) | `\x1b[{n}C` | n > 1 |
| CursorBackward(1) | `\b` | Backspace for n=1 |
| CursorBackward(n) | `\x1b[{n}D` | n > 1 |
| **Cursor Visibility** | | |
| HideCursor | `\x1b[?25l` | |
| ShowCursor | `\x1b[?12l\x1b[?25h` | Stop blink + show |
| **Cursor Shape (DECSCUSR)** | | |
| Block | `\x1b[2 q` | Steady block |
| BlinkingBlock | `\x1b[1 q` | Blinking block |
| Underline | `\x1b[4 q` | Steady underline |
| BlinkingUnderline | `\x1b[3 q` | Blinking underline |
| Beam | `\x1b[6 q` | Steady bar |
| BlinkingBeam | `\x1b[5 q` | Blinking bar |
| ResetCursorShape | `\x1b[0 q` | Terminal default |
| **Attributes** | | |
| ResetAttributes | `\x1b[0m` | Reset all attributes |
| SetAttributes | `\x1b[0;{codes}m` | Reset + set new |
| **Text Styles** | | |
| Bold | `1` | SGR code |
| Dim | `2` | SGR code |
| Italic | `3` | SGR code |
| Underline | `4` | SGR code |
| Blink | `5` | SGR code |
| Reverse | `7` | SGR code |
| Hidden | `8` | SGR code |
| Strike | `9` | SGR code |
| **Colors (4-bit)** | | |
| FG Black | `30` | |
| FG Red | `31` | |
| FG Green | `32` | |
| FG Yellow | `33` | |
| FG Blue | `34` | |
| FG Magenta | `35` | |
| FG Cyan | `36` | |
| FG White | `37` | |
| FG Bright Black | `90` | |
| FG Bright Red | `91` | |
| FG Bright Green | `92` | |
| FG Bright Yellow | `93` | |
| FG Bright Blue | `94` | |
| FG Bright Magenta | `95` | |
| FG Bright Cyan | `96` | |
| FG Bright White | `97` | |
| BG codes | `40-47`, `100-107` | Same pattern + 10 |
| **Colors (8-bit)** | | |
| FG 256-color | `38;5;{n}` | n = 0-255 |
| BG 256-color | `48;5;{n}` | n = 0-255 |
| **Colors (24-bit)** | | |
| FG True color | `38;2;{r};{g};{b}` | RGB values 0-255 |
| BG True color | `48;2;{r};{g};{b}` | RGB values 0-255 |
| **Autowrap** | | |
| DisableAutowrap | `\x1b[?7l` | |
| EnableAutowrap | `\x1b[?7h` | |
| **Mouse Modes** | | |
| EnableBasicMouse | `\x1b[?1000h` | Click tracking |
| EnableDragMouse | `\x1b[?1003h` | Button+drag tracking |
| EnableUrxvtMouse | `\x1b[?1015h` | Extended coords |
| EnableSgrMouse | `\x1b[?1006h` | SGR extended mode |
| DisableMouse | `\x1b[?1000l\x1b[?1003l\x1b[?1015l\x1b[?1006l` | All modes off |
| **Bracketed Paste** | | |
| EnableBracketedPaste | `\x1b[?2004h` | |
| DisableBracketedPaste | `\x1b[?2004l` | |
| **Title** | | |
| SetTitle | `\x1b]2;{title}\x07` | OSC sequence |
| ClearTitle | `\x1b]2;\x07` | Empty title |
| **Cursor Key Mode** | | |
| ResetCursorKeyMode | `\x1b[?1l` | Normal mode |
| **Cursor Position Report** | | |
| AskForCpr | `\x1b[6n` | Response: `\x1b[{row};{col}R` |
| **Bell** | | |
| Bell | `\x07` | BEL character |

### Color Mapping Algorithms

#### RGB to 16-Color Mapping (FR-011)

Uses **squared Euclidean distance** in RGB color space:

```
distance = (r1 - r2)² + (g1 - g2)² + (b1 - b2)²
```

**Saturation check for gray exclusion**:
```
saturation = |r - g| + |g - b| + |b - r|
if saturation > 30, exclude gray-like colors from candidates
```

**Foreground/background collision avoidance**:
When mapping background color, exclude the foreground color result from candidates to prevent identical fg/bg.

#### RGB to 256-Color Mapping (FR-012)

Uses **squared Euclidean distance** in RGB color space against the 256-color palette:

**Palette Structure**:
- Indices 0-15: 16 ANSI colors (SKIPPED during mapping to avoid theme-dependent colors)
- Indices 16-231: 6×6×6 color cube (216 colors)
  - Formula: `index = 16 + (36 × r_level) + (6 × g_level) + b_level`
  - Levels: 0, 95, 135, 175, 215, 255 (indices 0-5)
- Indices 232-255: 24 grayscale levels (8-238 in steps of 10)

### Environment Variables

| Variable | Valid Values | Behavior |
|----------|--------------|----------|
| `NO_COLOR` | Any value (presence checked) | Force Depth1Bit (monochrome) |
| `STROKE_COLOR_DEPTH` | `DEPTH_1_BIT` | Return Depth1Bit |
| `STROKE_COLOR_DEPTH` | `DEPTH_4_BIT` | Return Depth4Bit |
| `STROKE_COLOR_DEPTH` | `DEPTH_8_BIT` | Return Depth8Bit |
| `STROKE_COLOR_DEPTH` | `DEPTH_24_BIT` | Return Depth24Bit |
| `TERM` | `dumb` (or starts with `dumb`) | Return Depth1Bit |
| `TERM` | `linux` | Return Depth4Bit |
| `TERM` | `eterm-color` | Return Depth4Bit |
| `TERM` | Other values | Return Depth8Bit (default) |

**Priority**: NO_COLOR > STROKE_COLOR_DEPTH > TERM

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 35 IOutput interface methods and 2 properties (37 total members) are implemented in Vt100Output, PlainTextOutput, and DummyOutput classes. [Verified by: interface compilation + reflection-based member count assertion]
- **SC-002**: VT100 escape sequences match Python Prompt Toolkit output exactly for all operations. [Verified by: comparing StringWriter output against reference strings from VT100 Escape Sequence Reference table]
- **SC-003**: Color depth detection correctly identifies terminal capabilities from environment variables. [Verified by: parameterized tests for each (NO_COLOR, STROKE_COLOR_DEPTH, TERM) combination listed in Environment Variables table]
- **SC-004**: RGB to 16-color mapping produces the same results as Python Prompt Toolkit for any RGB input. [Verified by: testing reference RGB values: (255,0,0)→red, (0,255,0)→green, (128,128,128)→gray, (255,128,0)→yellow, plus Python PTK test corpus]
- **SC-005**: RGB to 256-color mapping produces the same results as Python Prompt Toolkit for any RGB input. [Verified by: testing all 6×6×6 cube corner points, grayscale boundaries, and Python PTK test corpus]
- **SC-006**: Escape code caching eliminates redundant string building (same Attrs produces cached result). [Verified by: calling SetAttributes twice with same Attrs and asserting reference equality of returned strings]
- **SC-007**: Unit test coverage achieves 80% or higher for all output classes. [Verified by: `dotnet test --collect:"XPlat Code Coverage"` with Coverlet reporting ≥80% line coverage]
- **SC-008**: All ANSI color name mappings match Python Prompt Toolkit values exactly. [Verified by: exhaustive comparison of 16 ANSI color RGB values against Python PTK constants]
- **SC-009**: Cross-platform output factory correctly selects appropriate output type on all supported platforms. [Verified by: testing with mocked Console.IsOutputRedirected values returning expected IOutput subtypes]
- **SC-010**: Thread safety is verified through concurrent access tests. [Verified by: spawning 100 threads performing 1000 Write+Flush cycles each with no exceptions or data corruption]

### Traceability Matrix

| Requirement | User Stories | Acceptance Scenarios |
|-------------|--------------|---------------------|
| FR-001 ColorDepth enum | US-2 | US-2.1, US-2.2 |
| FR-002 CursorShape enum | US-3 | US-3.1-3.5 |
| FR-003 Color depth detection | US-2 | US-2.1, US-2.2 |
| FR-004 IOutput interface | US-1, US-5, US-6, US-7 | All scenarios |
| FR-005 Vt100Output | US-1, US-4 | US-1.1-1.5, US-4.1-4.4 |
| FR-006 PlainTextOutput | US-6 | US-6.1-6.4 |
| FR-007 DummyOutput | US-7 | US-7.1-7.4 |
| FR-008 Write escape escaping | US-1 | US-1.4 |
| FR-009 WriteRaw verbatim | US-1 | US-1.5 |
| FR-010 Escape code caching | US-1 | SC-006 |
| FR-011 RGB to 16-color | US-2 | US-2.4 |
| FR-012 RGB to 256-color | US-2 | US-2.3 |
| FR-013 24-bit true color | US-2 | US-2.5 |
| FR-014 Avoid fg/bg collision | US-2 | US-2.4 |
| FR-015 Cursor visibility tracking | US-3, US-8 | US-3.5, US-8.3 |
| FR-016 Cursor shape tracking | US-3 | US-3.4, US-3.5 |
| FR-017 Terminal title | US-4 | Edge Cases §Title |
| FR-018 Alternate screen | US-4 | US-4.1, US-4.4 |
| FR-019 Mouse modes | US-4 | US-4.2 |
| FR-020 Bracketed paste | US-4 | US-4.3 |
| FR-021 CPR request | US-1 | VT100 §CPR |
| FR-022 Factory method | US-5 | US-5.1-5.4 |
| FR-023 Cursor movement optimization | US-1 | Edge Cases §Cursor |
| FR-024 Buffered output | US-1 | Edge Cases §Buffer |
| FR-025 Thread safety | US-8 | US-8.1-8.4 |

## Assumptions

- Terminal supports standard VT100/ANSI escape sequences (modern terminals on Linux, macOS, and Windows 10+).
- UTF-8 encoding is the default for all output streams.
- Console.Out/stderr are used as the default stdout streams when not explicitly provided.
- Environment variable STROKE_COLOR_DEPTH follows the same naming convention as Python's PROMPT_TOOLKIT_COLOR_DEPTH.
- Default terminal size of 80 columns × 24 rows is used when actual size cannot be determined or reports 0×0.
- Bell sound is enabled by default but can be disabled via configuration.
- Cursor position reporting (CPR) is supported by most modern terminals but may be disabled in some contexts.
- Multi-byte UTF-8 characters in Write() and WriteRaw() are passed through without modification; only the escape byte (0x1b) is replaced in Write().
- Embedded null characters (0x00) in Write() and WriteRaw() are passed through without modification.

## Dependencies

- **Stroke.Core.Primitives.Size**: Used by IOutput.GetSize() to return terminal dimensions.
- **Stroke.Styles.Attrs**: Used by IOutput.SetAttributes() to specify text styling (colors, bold, italic, etc.).
- **Stroke.CursorShapes.CursorShape**: Used by IOutput.SetCursorShape() to specify cursor appearance.
- **System.IO.TextWriter**: Used as the underlying output stream for all implementations.
- **System.Threading.Lock**: Used for thread synchronization in mutable implementations (.NET 9+).

## Non-Functional Requirements

### Performance

- **NFR-001**: EscapeCodeCache MUST achieve O(1) lookup time for cached Attrs-to-escape-sequence mappings.
- **NFR-002**: SixteenColorCache and TwoFiftySixColorCache MUST use lazy initialization (only compute mappings on first access).
- **NFR-003**: Color cache memory usage SHOULD NOT exceed 10KB for typical applications (common colors only).
- **NFR-004**: Flush() with an empty buffer MUST NOT perform any I/O operations.
- **NFR-005**: Repeated identical SetAttributes() calls SHOULD reuse cached escape sequences.

### Reliability

- **NFR-006**: All IOutput implementations MUST be resilient to I/O exceptions during Flush() (log and continue).
- **NFR-007**: Terminal size detection failure MUST fall back to 80×24 rather than throwing.

### Thread Safety

- **NFR-008**: All mutable state in Vt100Output and PlainTextOutput MUST be protected by System.Threading.Lock using EnterScope() pattern.
- **NFR-009**: EscapeCodeCache, SixteenColorCache, and TwoFiftySixColorCache MUST use ConcurrentDictionary or equivalent thread-safe collections.
- **NFR-010**: Individual IOutput method calls MUST be atomic; callers are responsible for synchronizing compound operations (e.g., CursorGoto + Write + Flush).

## Platform-Specific Behavior

### Windows

- **Windows 10+**: VT100 escape sequences are supported natively. Vt100Output is used.
- **Windows-Specific Methods**: ScrollBufferToPrompt() and GetRowsBelowCursorPosition() are available for Windows Console scrollback manipulation.
- **ConEmu/MSYS2/WSL Detection**: Environment variable detection (TERM, COLORTERM, WT_SESSION) determines color depth and VT100 support.

### POSIX (Linux, macOS)

- **TTY Detection**: Console.IsOutputRedirected determines whether to use Vt100Output or PlainTextOutput.
- **TERM Variable Processing**:
  - `TERM=dumb` or starts with `dumb` → Depth1Bit
  - `TERM=linux` → Depth4Bit (Linux console has limited color support)
  - `TERM=eterm-color` → Depth4Bit (Emacs terminal)
  - Other values → Depth8Bit (default)
- **Title Setting**: Not supported on `linux` or `eterm-color` terminals (no escape sequence sent).

### Redirected Output

- **Console.IsOutputRedirected=true**: PlainTextOutput is used automatically.
- **alwaysPreferTty=true**: If stdout is redirected but stderr is a TTY, output goes to stderr.

## Clarifications & Disambiguation

### DummyOutput Depth1Bit vs "No-Op" (CHK068)

DummyOutput returns `Depth1Bit` from `GetDefaultColorDepth()` because:
- It represents the minimal color capability (monochrome)
- Callers that ask for color depth receive a valid enum value
- The "no-op" aspect refers to output operations (Write, Flush), not queries (GetSize, GetDefaultColorDepth)

**Behavior Summary**:
- Output operations (Write, WriteRaw, Flush, etc.): No-op (silent, no side effects)
- Query operations (GetSize, GetDefaultColorDepth, Encoding): Return sensible defaults

### _enableCpr Flag vs RespondsToCpr Property (CHK069)

The `_enableCpr` constructor parameter controls whether CPR is enabled for the output instance:
- `_enableCpr = true` AND output is a TTY → `RespondsToCpr = true`
- `_enableCpr = true` AND output is NOT a TTY → `RespondsToCpr = false`
- `_enableCpr = false` (regardless of TTY) → `RespondsToCpr = false`

The property combines two conditions: feature enablement AND terminal capability.

### "linux" Terminal Type vs Linux Operating System (CHK070)

The string `"linux"` in TERM environment variable refers to the **Linux console** (the raw VT on Linux servers), NOT the Linux operating system:
- `TERM=linux` → Running on raw Linux console → Depth4Bit (limited colors)
- Running Linux with `TERM=xterm-256color` → Depth8Bit (modern terminal emulator)
- Running Linux with `TERM=xterm-truecolor` → Depth24Bit (modern terminal)

The OS is detected separately via `RuntimeInformation.IsOSPlatform()`.

### PlainTextOutput "Always Return" vs "No-Op" (CHK071)

PlainTextOutput methods are categorized as:
- **No-op methods**: Methods that normally produce escape sequences do nothing (SetAttributes, HideCursor, etc.)
- **Text-equivalent methods**: Methods that simulate cursor movement with text:
  - `CursorForward(n)` → writes n spaces
  - `CursorDown(n)` → writes n newlines
- **Direct output methods**: Write, WriteRaw → write text directly (no escaping needed since no terminal)

This ensures redirected output is human-readable while maintaining interface compliance.

### CursorShape.NeverChange vs ResetCursorShape (CHK072)

- **CursorShape.NeverChange**: Instructs `SetCursorShape()` to NOT send any escape sequence. Used when the application should preserve the user's terminal cursor setting. This is the DEFAULT for applications that don't need cursor shape control.

- **ResetCursorShape()**: Sends `\x1b[0 q` to reset cursor to terminal's default shape, but ONLY if cursor shape was previously changed. Used at application exit to restore terminal state.

**Use Cases**:
- Application doesn't care about cursor shape: Use `NeverChange` (default), never call `ResetCursorShape()`
- Vi-style editor: Call `SetCursorShape(Block)` in normal mode, `SetCursorShape(Beam)` in insert mode, call `ResetCursorShape()` on exit
