# Research: Synchronized Output (DEC Mode 2026)

**Feature**: 067-synchronized-output
**Date**: 2026-02-07

## R-001: DEC Mode 2026 Specification

**Decision**: Use DEC Private Mode 2026 (Synchronized Output) with begin marker `\x1b[?2026h` and end marker `\x1b[?2026l`.

**Rationale**: This is the well-established standard documented at https://gist.github.com/christianparpart/d8a62cc1ab659194337d73e399004036. When enabled, the terminal buffers all output and commits it atomically when disabled. Unsupported terminals silently ignore DEC Private Mode sequences per the VT100 specification.

**Alternatives considered**:
- iTerm2 proprietary sync protocol — Not cross-terminal
- Double-buffering in application — Terminal still renders intermediate frames
- No sync, hide/show cursor only — Current Python PTK approach, still produces visible flicker on resize

## R-002: Python Prompt Toolkit Status

**Decision**: Implement as a documented enhancement over Python PTK — not a deviation from API compatibility.

**Rationale**: Python Prompt Toolkit has **no** synchronized output implementation. Searching the entire codebase for "synchronized", "2026", "BSU", and "ESU" yields zero results in source code. Python PTK's renderer only uses hide/show cursor around render cycles. No `BeginSynchronizedOutput`/`EndSynchronizedOutput` methods exist on the Python `Output` base class. This feature was not available when Python PTK's renderer was originally designed (~2015). Mode 2026 adoption grew after 2020.

**Alternatives considered**: None — this is clearly a new capability not present in the original.

## R-003: Reference Implementations

### Neovim (`src/nvim/tui/tui.c`)
- **Capability detection**: Queries terminal for Mode 2026 support via `tui_request_term_mode(kTermModeSynchronizedOutput)`. Sets `has_sync_mode = true` when terminal confirms.
- **User opt-in**: Controlled by `'termsync'` option (`sync_output` boolean).
- **Flush wrapping**: Three-buffer flush: `[pre][content][post]`. If `sync_output && has_sync_mode`, pre = `\x1b[?2026h`, post = `\x1b[?2026l`. Fallback: hide/show cursor.
- **No timeout**: Relies on capability detection to avoid sending unsupported sequences.

### Ghostty (`src/termio/Thread.zig`, `src/termio/Termio.zig`)
- **Timer-based safety**: Starts a 1-second timer (`sync_reset_ms = 1000`) when synchronized output begins. Auto-resets if the end marker isn't sent within 1 second.
- **Message queue**: `start_synchronized_output` message triggers timer start.
- **Resize handling**: Explicitly disables synchronized output on resize to show changes immediately.
- **Mode state**: `terminal.modes.set(.synchronized_output, value)` tracked via mutex-protected renderer state.

### Decision for Stroke
- **No capability detection**: Unlike Neovim, Stroke wraps per-render-cycle. Unsupported terminals ignore the sequences silently (DEC spec guarantee). Neovim's detection is for the `'termsync'` user option — Stroke always emits the sequences via VT100 output.
- **No timer-based safety**: Unlike Ghostty (which is a terminal emulator), Stroke is an application framework. Render cycles complete in milliseconds, well within Ghostty's 1-second timeout. The try/finally pattern ensures the end marker is always sent.
- **Approach**: Simple flag on Vt100Output, wrapping Flush() output in Mode 2026 markers when active. Begin/End methods on IOutput with no-ops on non-VT100 backends.

## R-004: Existing IOutput Interface

**Decision**: Add `BeginSynchronizedOutput()` and `EndSynchronizedOutput()` to the `IOutput` interface.

**Rationale**: The IOutput interface at `src/Stroke/Output/IOutput.cs` (399 lines) defines all terminal output operations. There are 7 implementations:

| Implementation | File | Strategy |
|---|---|---|
| `Vt100Output` | `src/Stroke/Output/Vt100Output.cs` (528 lines) | Set `_synchronizedOutput` flag; wrap Flush() output in Mode 2026 markers |
| `Win32Output` | `src/Stroke/Output/Windows/Win32Output.cs` | No-op (legacy Win32 API has no equivalent) |
| `Windows10Output` | `src/Stroke/Output/Windows/Windows10Output.cs` | Delegate to `_vt100Output` |
| `ConEmuOutput` | `src/Stroke/Output/Windows/ConEmuOutput.cs` | Delegate to `_vt100Output` |
| `PlainTextOutput` | `src/Stroke/Output/PlainTextOutput.cs` | No-op (escape codes would contaminate file/pipe output) |
| `DummyOutput` | `src/Stroke/Output/DummyOutput.cs` | No-op (testing output) |

**Alternatives considered**:
- Extension methods — Would not be polymorphic across implementations
- Separate interface — Adds unnecessary complexity; all IOutput implementations need the method

## R-005: Vt100Output Flush Mechanism

**Decision**: Add a `_synchronizedOutput` boolean flag to `Vt100Output`. When true, `Flush()` prepends `\x1b[?2026h` and appends `\x1b[?2026l` to the concatenated buffer before writing to stdout.

**Rationale**: Vt100Output.Flush() (lines 154-177) already:
1. Acquires `_lock` via `EnterScope()`
2. Concatenates all buffered strings
3. Writes concatenated output to `_stdout`
4. Flushes `_stdout`

The Mode 2026 markers are emitted by `Flush()` around the actual write to the underlying stream. `BeginSynchronizedOutput()` and `EndSynchronizedOutput()` set and clear the flag respectively — they do not themselves write to the stream. `Flush()` checks the flag and, when true, prepends/appends the markers to the concatenated buffer content in a single write operation. This ensures the terminal receives the markers as part of a single write, making the atomic commit reliable.

**Alternatives considered**:
- Write markers as separate WriteRaw calls — Would not guarantee they're part of the same write operation to stdout
- Buffer markers in the buffer list — Adds unnecessary string concatenation overhead

## R-006: Renderer Render Flow

**Decision**: Wrap the render+flush sequence in `BeginSynchronizedOutput()`/`EndSynchronizedOutput()` calls. Use try/finally to guarantee the end marker.

**Rationale**: `Renderer.Render()` (lines 182-327) follows this flow:
1. Setup: alternate screen, bracketed paste, cursor key mode, mouse support
2. Create screen, compute height, write layout
3. `ScreenDiff.OutputScreenDiff()` — writes diff to output buffer
4. Set cursor shape
5. `output.Flush()` — commits all buffered output to terminal
6. Update state

The synchronized output begin must come before step 3 (the first output writes) and end must come after step 5 (the flush). Setup operations (step 1) write to the buffer but don't flush — they'll be committed in the same flush.

## R-007: Resize Handler

**Decision**: Replace `Renderer.Erase(leaveAlternateScreen: false)` in `OnResize()` with a new `ResetForResize()` method that only resets in-memory state.

**Rationale**: `OnResize()` (line 713-718 of Application.RunAsync.cs) currently:
1. `Renderer.Erase(leaveAlternateScreen: false)` — Sends cursor movement + EraseDown + Flush **immediately**
2. `Renderer.RequestAbsoluteCursorPosition()` — Requests CPR
3. `Invalidate()` — Schedules a redraw

Step 1 causes the blank-frame flicker because it immediately erases the screen. The next Render() call then redraws from scratch. The gap between erase and redraw is visible as a flash.

The fix: `ResetForResize()` resets all renderer state (cursor position, last screen, last size, etc.) **without any I/O**. Since `_lastScreen` is null after reset, the next `Render()` call enters the full-redraw path in OutputScreenDiff (line 155-161) which erases and redraws — now inside a synchronized output block.

**Alternatives considered**:
- Debounce resize events — Already done via Invalidate() coalescing, but doesn't fix the immediate erase
- Only defer the flush — Would still buffer stale output

## R-008: Absolute Cursor Positioning on Full Redraw

**Decision**: Replace `MoveCursor(new Point(0, 0))` with `output.WriteRaw("\x1b[H")` (absolute cursor home) on the full-redraw path in OutputScreenDiff.

**Rationale**: ScreenDiff.OutputScreenDiff line 155-161:
```csharp
if (isDone || previousScreen is null || previousWidth != width)
{
    currentPos = MoveCursor(new Point(0, 0));
    ResetAttributes();
    output.EraseDown();
    previousScreen = new Screen();
}
```

`MoveCursor()` uses **relative** cursor movement (CursorUp/CursorDown/CursorForward/CursorBackward) based on `currentPos`. After a resize, `_cursorPos` may be stale — pointing to coordinates that no longer exist in the new terminal dimensions. Relative movement from a stale position produces incorrect results.

`\x1b[H` (CSI H) is "Cursor Home" — it moves the cursor to row 1, column 1 regardless of current position. This is safe from any state.

**Alternatives considered**:
- `CursorGoto(1, 1)` — Same effect but through the method call; `\x1b[H` is more direct and avoids the `Math.Max(1, row)` logic

## R-009: Test Infrastructure

**Decision**: Add new test files in existing test directories:
- `tests/Stroke.Tests/Output/Vt100OutputSynchronizedOutputTests.cs`
- `tests/Stroke.Tests/Rendering/RendererSynchronizedOutputTests.cs`

**Rationale**: The test project structure at `tests/Stroke.Tests/` has directories matching source namespaces:
- `tests/Stroke.Tests/Output/` — 12 existing test files covering Vt100Output, PlainTextOutput, DummyOutput, ColorLookupTable, etc.
- `tests/Stroke.Tests/Rendering/` — Existing rendering tests
- `tests/Stroke.Tests/Layout/` — Screen, Char, etc. tests

Tests use xUnit with standard assertions (no mocks per Constitution VIII). Vt100Output tests capture output by writing to a `StringWriter`, then checking the string for expected escape sequences. The `DummyOutput` class is used for testing non-output behaviors.

**Key patterns observed**:
- `StringWriter` as stdout for output capture
- Direct `Vt100Output.FromPty(writer)` construction
- Escape sequence string matching via `Assert.Contains`
- `TestWindow` (implements IWindow) for screen cursor tests
