# Feature Specification: ConEmu Output

**Feature Branch**: `053-conemu-output`
**Created**: 2026-02-02
**Status**: Draft
**Input**: User description: "ConEmu Output - Windows-specific hybrid output combining Win32Output and Vt100Output for enhanced color support in ConEmu/Cmder terminals"
**Python Source**: `prompt_toolkit/output/conemu.py` (lines 1-66)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Terminal Application in ConEmu (Priority: P1)

A developer runs their Stroke-based terminal application inside ConEmu or Cmder on Windows. The application automatically detects the ConEmu environment and uses the hybrid output mode, enabling 256-color and true-color rendering while maintaining proper console sizing and scrolling behavior.

**Why this priority**: This is the core value proposition - enabling rich color output in ConEmu/Cmder terminals that would otherwise be limited to 16 colors with pure Win32 output.

**Independent Test**: Can be fully tested by launching a Stroke application in ConEmu with `ConEmuANSI=ON` and verifying that colors render correctly while console operations (resize, scroll) function properly.

**Acceptance Scenarios**:

1. **Given** a Stroke application running in ConEmu with `ConEmuANSI=ON`, **When** the application renders colored text using 256-color palette, **Then** the colors display correctly using VT100 escape sequences
2. **Given** a Stroke application running in ConEmu, **When** the application queries terminal size, **Then** the size is obtained via Win32 console APIs returning accurate dimensions
3. **Given** a Stroke application running in ConEmu, **When** the user resizes the terminal window, **Then** the application receives the updated size correctly

**Traceability**: FR-001, FR-002, FR-003, FR-007

---

### User Story 2 - Mouse Support in ConEmu (Priority: P2)

A developer's interactive terminal application running in ConEmu needs mouse support for menus, selection, or drag operations. The hybrid output mode delegates mouse operations to Win32Output, ensuring reliable mouse tracking that works consistently with ConEmu's Windows console integration.

**Why this priority**: Mouse support is essential for interactive applications but builds on the foundation of P1 (basic output functionality).

**Independent Test**: Can be tested by enabling mouse support in a Stroke application within ConEmu and verifying click, drag, and scroll events are captured correctly.

**Acceptance Scenarios**:

1. **Given** a Stroke application in ConEmu, **When** mouse support is enabled, **Then** click events are captured via Win32 APIs
2. **Given** a Stroke application in ConEmu with mouse enabled, **When** the user scrolls with the mouse wheel, **Then** scroll events are properly received

**Traceability**: FR-004

---

### User Story 3 - Bracketed Paste in ConEmu (Priority: P3)

A user pastes multi-line text into a Stroke application running in ConEmu. The bracketed paste mode ensures the pasted text is handled as a single paste operation rather than being interpreted as multiple command entries.

**Why this priority**: Bracketed paste is a convenience feature that improves user experience but is not essential for basic functionality.

**Independent Test**: Can be tested by enabling bracketed paste and pasting multi-line text, verifying it arrives as a single bracketed sequence.

**Acceptance Scenarios**:

1. **Given** a Stroke application in ConEmu with bracketed paste enabled, **When** the user pastes multi-line text, **Then** the text is received with bracketed paste delimiters
2. **Given** a Stroke application in ConEmu, **When** bracketed paste is disabled, **Then** pasted text is processed normally without bracketed delimiters

**Traceability**: FR-006

---

### Edge Cases

| Edge Case | Expected Behavior | Traceability |
|-----------|-------------------|--------------|
| Application runs outside ConEmu (no `ConEmuANSI` environment variable) | System MUST NOT use ConEmuOutput; fall back to appropriate output type (Win32Output or Vt100Output based on platform detection) | FR-001 |
| ConEmu with `ConEmuANSI=OFF` | System treats this as non-ConEmu environment; case-sensitive exact match required | FR-001 |
| ConEmu with `ConEmuANSI` set to lowercase "on", "1", or "true" | System treats as non-ConEmu environment; only exact "ON" value triggers ConEmu mode | FR-001 |
| Win32Output operations fail (no console attached) | `NoConsoleScreenBufferError` propagates from Win32Output constructor | FR-013 |
| Both outputs share the same TextWriter | Safe: VT100 writes escape sequences while Win32 uses console APIs - they operate on different layers | FR-002 |
| TextWriter becomes invalid after construction | Underlying output exceptions propagate unchanged; no special handling | FR-014 |
| Environment variable modified after construction | No effect; ConEmu detection occurs only at construction time | FR-001 |
| Concurrent access from multiple threads | Thread-safe; delegates to thread-safe Win32Output and Vt100Output | FR-015 |
| Rapid interleaving of Win32 and Vt100 operations | Safe; each output manages its own state independently | FR-015 |
| Non-Windows platform instantiation | `PlatformNotSupportedException` thrown before any output creation | FR-010 |

## Requirements *(mandatory)*

### Functional Requirements

#### ConEmu Detection

- **FR-001**: System MUST detect ConEmu environment by checking for `ConEmuANSI` environment variable with exact value `"ON"` (case-sensitive). Values such as `"on"`, `"1"`, `"true"`, or `"OFF"` MUST NOT trigger ConEmu mode.

#### Output Creation and Sharing

- **FR-002**: System MUST create both Win32Output and Vt100Output instances sharing the same underlying TextWriter passed to the constructor. Win32Output MUST be instantiated first, followed by Vt100Output.

#### Delegation to Win32Output

The following operations MUST delegate to Win32Output because they require Windows Console API access for accurate results:

- **FR-003**: Console sizing operations (`GetSize`, `GetRowsBelowCursorPosition`) MUST delegate to Win32Output. *Rationale*: VT100 cannot accurately determine Windows console buffer dimensions; Win32 APIs provide authoritative size information.
- **FR-004**: Mouse support operations (`EnableMouseSupport`, `DisableMouseSupport`) MUST delegate to Win32Output. *Rationale*: Windows console mouse tracking requires Win32 input mode configuration; VT100 mouse sequences are processed but mode setup needs Win32 APIs.
- **FR-005**: Scroll operations (`ScrollBufferToPrompt`) MUST delegate to Win32Output. *Rationale*: Windows-specific scroll buffer manipulation requires direct console API access.
- **FR-006**: Bracketed paste operations (`EnableBracketedPaste`, `DisableBracketedPaste`) MUST delegate to Win32Output. *Rationale*: Matches Python Prompt Toolkit behavior; Win32Output handles these as no-ops but maintains API consistency.

#### Delegation to Vt100Output

The following operations MUST delegate to Vt100Output because they use ANSI escape sequences for rendering:

- **FR-007**: Text output operations MUST delegate to Vt100Output:
  - `Write(string data)` - Write text with escape sequence sanitization
  - `WriteRaw(string data)` - Write raw text/escape sequences
  - `Flush()` - Flush output buffer to stream

- **FR-007a**: Cursor movement operations MUST delegate to Vt100Output:
  - `CursorGoto(int row, int column)`
  - `CursorUp(int amount)`
  - `CursorDown(int amount)`
  - `CursorForward(int amount)`
  - `CursorBackward(int amount)`

- **FR-007b**: Cursor visibility operations MUST delegate to Vt100Output:
  - `HideCursor()`
  - `ShowCursor()`
  - `SetCursorShape(CursorShape shape)`
  - `ResetCursorShape()`

- **FR-007c**: Screen control operations MUST delegate to Vt100Output:
  - `EraseScreen()`
  - `EraseEndOfLine()`
  - `EraseDown()`
  - `EnterAlternateScreen()`
  - `QuitAlternateScreen()`

- **FR-007d**: Attribute operations MUST delegate to Vt100Output:
  - `ResetAttributes()`
  - `SetAttributes(Attrs attrs, ColorDepth colorDepth)`
  - `DisableAutowrap()`
  - `EnableAutowrap()`

- **FR-007e**: Title and bell operations MUST delegate to Vt100Output:
  - `SetTitle(string title)`
  - `ClearTitle()`
  - `Bell()`

- **FR-007f**: Cursor position report operations MUST delegate to Vt100Output:
  - `AskForCpr()`
  - `ResetCursorKeyMode()`

- **FR-007g**: Terminal information operations MUST delegate to Vt100Output:
  - `Fileno()` - Returns file descriptor (delegates to Vt100Output)
  - `GetDefaultColorDepth()` - Returns color depth (delegates to Vt100Output)

- **FR-007h**: Properties MUST delegate to Vt100Output:
  - `Encoding` - Returns "utf-8" from Vt100Output
  - `Stdout` - Returns TextWriter from Vt100Output

#### Other Requirements

- **FR-008**: System MUST report `RespondsToCpr` as `false` since cursor position reporting is not needed on Windows. This is a direct property, not a delegation.
- **FR-009**: System MUST support optional `defaultColorDepth` parameter (type: `ColorDepth?`) propagated to both Win32Output and Vt100Output constructors.
- **FR-010**: System MUST be marked with `[SupportedOSPlatform("windows")]` attribute for platform safety. Instantiation on non-Windows platforms MUST throw `PlatformNotSupportedException`.
- **FR-011**: System MUST expose underlying outputs as public readonly properties:
  - `Win32Output Win32Output { get; }` - readonly, initialized at construction
  - `Vt100Output Vt100Output { get; }` - readonly, initialized at construction

  These properties expose the underlying instances but do not allow reassignment. Consumers MAY call methods on the underlying outputs directly but SHOULD use ConEmuOutput methods for standard operations.

- **FR-012**: System MUST implement the `IOutput` interface completely with no missing methods.

#### Constructor Requirements

- **FR-013**: Constructor MUST accept the following parameters:
  - `TextWriter stdout` (required) - The output stream; MUST NOT be null
  - `ColorDepth? defaultColorDepth` (optional, default: null) - Color depth override

  Constructor MUST:
  1. Validate `stdout` is not null (throw `ArgumentNullException`)
  2. Validate platform is Windows (throw `PlatformNotSupportedException` if not)
  3. Create Win32Output first (may throw `NoConsoleScreenBufferError`)
  4. Create Vt100Output second (pass `() => Size.Empty` for size callback)
  5. Store both outputs in readonly properties

- **FR-014**: Exceptions from underlying outputs MUST propagate unchanged. ConEmuOutput MUST NOT catch or wrap exceptions from Win32Output or Vt100Output operations.

#### Thread Safety

- **FR-015**: ConEmuOutput MUST be thread-safe. Thread safety is achieved by delegation to Win32Output and Vt100Output, which are both thread-safe implementations. ConEmuOutput itself has no mutable state beyond the readonly output references.

### Delegation Summary Table

| IOutput Method | Delegates To | Rationale |
|----------------|--------------|-----------|
| `Write` | Vt100Output | Text rendering via escape sequences |
| `WriteRaw` | Vt100Output | Raw escape sequence output |
| `Flush` | Vt100Output | Buffer flush to stream |
| `EraseScreen` | Vt100Output | ANSI escape sequence |
| `EraseEndOfLine` | Vt100Output | ANSI escape sequence |
| `EraseDown` | Vt100Output | ANSI escape sequence |
| `EnterAlternateScreen` | Vt100Output | ANSI escape sequence |
| `QuitAlternateScreen` | Vt100Output | ANSI escape sequence |
| `CursorGoto` | Vt100Output | ANSI escape sequence |
| `CursorUp` | Vt100Output | ANSI escape sequence |
| `CursorDown` | Vt100Output | ANSI escape sequence |
| `CursorForward` | Vt100Output | ANSI escape sequence |
| `CursorBackward` | Vt100Output | ANSI escape sequence |
| `HideCursor` | Vt100Output | ANSI escape sequence |
| `ShowCursor` | Vt100Output | ANSI escape sequence |
| `SetCursorShape` | Vt100Output | ANSI escape sequence |
| `ResetCursorShape` | Vt100Output | ANSI escape sequence |
| `ResetAttributes` | Vt100Output | ANSI escape sequence |
| `SetAttributes` | Vt100Output | ANSI escape sequence |
| `DisableAutowrap` | Vt100Output | ANSI escape sequence |
| `EnableAutowrap` | Vt100Output | ANSI escape sequence |
| `EnableMouseSupport` | Win32Output | Windows console input mode |
| `DisableMouseSupport` | Win32Output | Windows console input mode |
| `EnableBracketedPaste` | Win32Output | Matches Python PTK behavior |
| `DisableBracketedPaste` | Win32Output | Matches Python PTK behavior |
| `SetTitle` | Vt100Output | ANSI escape sequence |
| `ClearTitle` | Vt100Output | ANSI escape sequence |
| `Bell` | Vt100Output | ANSI BEL character |
| `AskForCpr` | Vt100Output | ANSI escape sequence |
| `ResetCursorKeyMode` | Vt100Output | ANSI escape sequence |
| `GetSize` | Win32Output | Accurate Windows console dimensions |
| `Fileno` | Vt100Output | File descriptor from stream |
| `GetDefaultColorDepth` | Vt100Output | Color depth detection |
| `ScrollBufferToPrompt` | Win32Output | Windows console buffer API |
| `GetRowsBelowCursorPosition` | Win32Output | Windows console cursor info |
| `Encoding` (property) | Vt100Output | Returns "utf-8" |
| `Stdout` (property) | Vt100Output | Returns TextWriter |
| `RespondsToCpr` (property) | Direct: `false` | No CPR needed on Windows |

### Key Entities

- **ConEmuOutput**: The hybrid output class that proxies operations to Win32Output or Vt100Output based on operation type. Implements IOutput interface. Thread-safe through delegation.
- **Win32Output**: Existing Windows console output implementation using kernel32.dll P/Invoke. Handles: console sizing (`GetSize`, `GetRowsBelowCursorPosition`), mouse support, scroll buffer, bracketed paste. Located at `Stroke.Output.Windows.Win32Output`.
- **Vt100Output**: Existing VT100 terminal output implementation using ANSI escape sequences. Handles: all rendering operations (text, cursor, colors, screen control). Located at `Stroke.Output.Vt100Output`.
- **IOutput**: The output abstraction interface that ConEmuOutput must implement. Defines 30+ methods for terminal output operations. Located at `Stroke.Output.IOutput`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Applications running in ConEmu display 256-color output correctly. *Measurement*: Render test pattern with ANSI color codes 0-255; verify each renders as distinct color via screenshot comparison against reference image with <5% pixel variance tolerance.
- **SC-002**: Console resize events are detected within 100ms of window resize. *Measurement*: Programmatically resize window, measure time between resize API call and `GetSize()` returning new dimensions; average of 10 trials must be <100ms.
- **SC-003**: All IOutput interface methods function correctly without throwing unexpected exceptions. *Expected exceptions*: `ArgumentNullException` (null parameters), `NoConsoleScreenBufferError` (no console), `PlatformNotSupportedException` (non-Windows), `IOException` (stream errors). Any other exception type is unexpected.
- **SC-004**: Unit test coverage reaches 80% line coverage for the ConEmuOutput class. *Measurement*: Use `dotnet test --collect:"XPlat Code Coverage"` with Coverlet; verify ConEmuOutput.cs has ≥80% line coverage.
- **SC-005**: ConEmu detection correctly identifies ConEmu environment in under 1ms. *Measurement*: Call `PlatformUtils.IsConEmuAnsi` 1000 times; average time per call must be <1ms (total <1 second).
- **SC-006**: The hybrid approach renders text at least as fast as pure VT100 output. *Measurement*: Benchmark 10,000 `Write()` + `Flush()` cycles; ConEmuOutput time must be ≤110% of Vt100Output time (allowing 10% overhead tolerance for delegation).

## Assumptions

| Assumption | Validation |
|------------|------------|
| ConEmu/Cmder sets the `ConEmuANSI` environment variable to `ON` when ANSI support is enabled | Verified against [ConEmu ANSI documentation](http://conemu.github.io/en/AnsiEscapeCodes.html) |
| The same TextWriter can be safely used by both Win32Output and Vt100Output | Safe: Win32 uses console APIs (kernel32.dll), VT100 writes to TextWriter; no conflict because Win32Output's `Write`/`Flush` also use the console handle, not the TextWriter for actual output |
| Win32Output already exists and implements all required Win32 console operations | Verified: `src/Stroke/Output/Windows/Win32Output.cs` (723 lines) |
| Vt100Output already exists and implements all VT100 escape sequence rendering | Verified: `src/Stroke/Output/Vt100Output.cs` (491 lines) |
| PlatformUtils class exists with `IsConEmuAnsi` detection property | Verified: `src/Stroke/Core/PlatformUtils.cs` lines 69-70 |

## Deviations from Python Implementation

| Aspect | Python Implementation | C# Implementation | Rationale |
|--------|----------------------|-------------------|-----------|
| Delegation mechanism | `__getattr__` dynamic dispatch | Explicit method implementations | C# lacks `__getattr__`; explicit methods provide compile-time safety and better IDE support |
| Output registration | `Output.register(ConEmuOutput)` | N/A (interface implementation) | C# uses interface implementation, not ABC registration |
| Thread safety | Single-threaded assumed | Thread-safe via delegation | .NET applications commonly use multiple threads; Constitution XI requires thread safety |

## Python Source Reference

```python
# prompt_toolkit/output/conemu.py (lines 21-65)

class ConEmuOutput:
    """
    ConEmu (Windows) output abstraction.

    ConEmu is a Windows console application, but it also supports ANSI escape
    sequences. This output class is actually a proxy to both `Win32Output` and
    `Vt100_Output`. It uses `Win32Output` for console sizing and scrolling, but
    all cursor movements and scrolling happens through the `Vt100_Output`.

    This way, we can have 256 colors in ConEmu and Cmder. Rendering will be
    even a little faster as well.

    http://conemu.github.io/
    http://gooseberrycreative.com/cmder/
    """

    def __init__(
        self, stdout: TextIO, default_color_depth: ColorDepth | None = None
    ) -> None:
        self.win32_output = Win32Output(stdout, default_color_depth=default_color_depth)
        self.vt100_output = Vt100_Output(
            stdout, lambda: Size(0, 0), default_color_depth=default_color_depth
        )

    @property
    def responds_to_cpr(self) -> bool:
        return False  # We don't need this on Windows.

    def __getattr__(self, name: str) -> Any:
        if name in (
            "get_size",
            "get_rows_below_cursor_position",
            "enable_mouse_support",
            "disable_mouse_support",
            "scroll_buffer_to_prompt",
            "get_win32_screen_buffer_info",
            "enable_bracketed_paste",
            "disable_bracketed_paste",
        ):
            return getattr(self.win32_output, name)
        else:
            return getattr(self.vt100_output, name)


Output.register(ConEmuOutput)
```
