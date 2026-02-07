# Proposal: Fix Terminal Resize Flicker via Synchronized Output (DEC Mode 2026)

## Problem Statement

Terminal resize causes visible flicker in Stroke and Python Prompt Toolkit alike. This is a [long-standing issue](https://github.com/prompt-toolkit/python-prompt-toolkit/issues/29) that affects both projects because Stroke faithfully ported the same rendering architecture.

**Root cause:** `OnResize()` calls `Renderer.Erase()` which sends `CSI J` (erase down) and flushes to the terminal **immediately**, clearing the screen before the new layout is computed and rendered. The user sees a blank screen for one or more frames between the erase and the redraw.

```
Current resize flow:
1. SIGWINCH received
2. Renderer.Erase() → cursor home → erase down → FLUSH     ← screen goes blank
3. RequestAbsoluteCursorPosition()
4. Invalidate() → schedules Redraw()
   ... time passes ...
5. Redraw() → Renderer.Render() → ScreenDiff → FLUSH       ← content reappears
```

The gap between step 2 and step 5 is visible to the user as flicker.

## Solution: Two-Part Fix

### Part 1: Synchronized Output (Mode 2026)

Wrap every render flush in DEC Private Mode 2026 escape sequences. The terminal buffers all output between the begin and end markers and commits atomically in a single frame.

| Sequence | Meaning |
|----------|---------|
| `CSI ? 2026 h` | Begin synchronized update (terminal buffers output) |
| `CSI ? 2026 l` | End synchronized update (terminal commits atomically) |

**Terminal support:** Windows Terminal (v1.23+), iTerm2, Kitty, Alacritty, Warp, Ghostty, WezTerm, foot, Contour, mintty. Unsupporting terminals silently ignore the sequences — graceful degradation with zero risk.

### Part 2: Deferred Erase on Resize

Remove the immediate `Erase()` call from `OnResize()`. Instead, reset renderer state and let the next `Render()` handle the erase inside its synchronized output block. This eliminates the blank-frame gap entirely.

```
Fixed resize flow:
1. SIGWINCH received
2. ResetForResize() → reset _cursorPos, _lastScreen, _lastSize (no I/O)
3. RequestAbsoluteCursorPosition()
4. Invalidate() → schedules Redraw()
5. Redraw() → Renderer.Render():
   a. BeginSynchronizedOutput → writes CSI ? 2026 h to buffer
   b. OutputScreenDiff → cursor home (absolute) → erase → render content
   c. EndSynchronizedOutput → writes CSI ? 2026 l to buffer
   d. Flush() → all output sent atomically
```

The terminal never shows a blank frame because the erase and redraw are in the same synchronized block.

## Reference Implementations

### Neovim (application-side pattern — what we follow)

Neovim wraps every render flush in synchronized output markers using a 3-part buffer:

```c
// neovim/src/nvim/tui/tui.c — flush_buf()
static void flush_buf(TUIData *tui)
{
    uv_buf_t bufs[3];
    char pre[32], post[32];

    bufs[0].len = flush_buf_start(tui, pre, sizeof(pre));   // CSI ? 2026 h
    bufs[1] = { tui->buf, tui->bufpos };                    // render data
    bufs[2].len = flush_buf_end(tui, post, sizeof(post));   // CSI ? 2026 l

    uv_write(&req, &tui->output_handle, bufs, 3, NULL);    // atomic write
}
```

Key design decisions:
- **Detection:** Queries terminal via `DECRQM` (`CSI ? 2026 $ p`) at startup
- **Fallback:** Hides cursor instead when Mode 2026 is unsupported
- **User control:** `termsync` option to enable/disable
- **Resize:** Goes through the same flush path — automatically wrapped

### Ghostty (terminal-emulator-side — how the terminal handles it)

Ghostty implements Mode 2026 on the receiving end:

```zig
// ghostty/src/renderer/generic.zig
if (state.terminal.modes.get(.synchronized_output)) {
    log.debug("synchronized output started, skipping render", .{});
    return;  // Pause rendering — no frames emitted
}
```

Key design decisions:
- **Pause, not buffer:** Renderer skips frames entirely during synchronized region
- **1-second timeout:** Auto-disables Mode 2026 if end marker never arrives (prevents hung terminals)
- **Force-disable on resize:** `self.terminal.modes.set(.synchronized_output, false)` on terminal resize (spec-allowed)
- **Diff-based rendering:** After sync ends, diff algorithm detects all accumulated changes and renders in one frame

## Detailed Implementation Plan

### Change 1: Add synchronized output methods to `IOutput`

**File:** `src/Stroke/Output/IOutput.cs`

Add two new methods to the interface:

```csharp
/// <summary>
/// Begin a synchronized output region (DEC Mode 2026).
/// Terminal emulators that support this mode will buffer all output
/// until <see cref="EndSynchronizedOutput"/> and commit atomically,
/// preventing flicker during rendering.
/// </summary>
void BeginSynchronizedOutput();

/// <summary>
/// End a synchronized output region (DEC Mode 2026).
/// Terminal emulators commit all buffered output atomically.
/// </summary>
void EndSynchronizedOutput();
```

### Change 2: Implement in `Vt100Output`

**File:** `src/Stroke/Output/Vt100Output.cs`

Add a `_synchronizedOutput` flag and implement the methods:

```csharp
private bool _synchronizedOutput;

public void BeginSynchronizedOutput()
{
    using (_lock.EnterScope())
    {
        _synchronizedOutput = true;
    }
}

public void EndSynchronizedOutput()
{
    using (_lock.EnterScope())
    {
        _synchronizedOutput = false;
    }
}
```

Modify `Flush()` to wrap buffer content in Mode 2026 markers when the flag is set:

```csharp
public void Flush()
{
    using (_lock.EnterScope())
    {
        if (_buffer.Count == 0)
        {
            // Even with empty buffer, need to end sync if active
            if (_synchronizedOutput)
            {
                _stdout.Write("\x1b[?2026h\x1b[?2026l");
                _stdout.Flush();
            }
            return;
        }

        var output = string.Concat(_buffer);
        _buffer.Clear();

        if (_synchronizedOutput)
        {
            _stdout.Write("\x1b[?2026h");
            _stdout.Write(output);
            _stdout.Write("\x1b[?2026l");
        }
        else
        {
            _stdout.Write(output);
        }

        _stdout.Flush();
    }
}
```

### Change 3: Implement no-ops in other outputs

**Files:** `Win32Output.cs`, `PlainTextOutput.cs`, `DummyOutput.cs`, `ConEmuOutput.cs`

All non-VT100 outputs get empty implementations:

```csharp
public void BeginSynchronizedOutput() { }
public void EndSynchronizedOutput() { }
```

**Rationale:**
- `Win32Output` — Legacy Windows Console API has no equivalent
- `PlainTextOutput` — Would contaminate file/pipe output with escape codes
- `DummyOutput` — No-op by design
- `ConEmuOutput` — Delegates rendering to its `Vt100Output`, which handles sync

### Change 4: Implement in `Windows10Output`

**File:** `src/Stroke/Output/Windows/Windows10Output.cs`

Delegate to the internal `Vt100Output`:

```csharp
public void BeginSynchronizedOutput() => _vt100Output.BeginSynchronizedOutput();
public void EndSynchronizedOutput() => _vt100Output.EndSynchronizedOutput();
```

### Change 5: Implement in `ConEmuOutput`

**File:** `src/Stroke/Output/Windows/ConEmuOutput.cs`

Delegate to the internal `Vt100Output`:

```csharp
public void BeginSynchronizedOutput() => _vt100Output.BeginSynchronizedOutput();
public void EndSynchronizedOutput() => _vt100Output.EndSynchronizedOutput();
```

### Change 6: Wrap `Renderer.Render()` in synchronized output

**File:** `src/Stroke/Rendering/Renderer.cs`

In the `Render()` method, wrap the rendering and flush in begin/end:

```csharp
public void Render(Application<object?> app, Layout layout, bool isDone = false)
{
    var output = _output;

    // ... setup code (mouse, cursor key mode) ...

    // Begin synchronized output — terminal buffers until end marker
    output.BeginSynchronizedOutput();

    // ... screen creation, height calculation, layout write ...

    // ScreenDiff (the actual rendering — writes to output buffer)
    var (newCursorPos, newLastStyle) = ScreenDiff.OutputScreenDiff(...);

    // ... cursor shape ...

    // Flush all buffered output (wrapped in sync markers by Vt100Output)
    output.Flush();

    // End synchronized output
    output.EndSynchronizedOutput();

    // ... update state (_cursorPos, _lastScreen, _lastSize, etc.) ...
}
```

### Change 7: Use absolute cursor positioning in `OutputScreenDiff`

**File:** `src/Stroke/Rendering/Renderer.Diff.cs`

When `previousScreen` is null or width changed (the full-redraw path), replace the relative `MoveCursor(0,0)` with an absolute cursor-home escape:

```csharp
// Current code (line 155-161):
if (isDone || previousScreen is null || previousWidth != width)
{
    currentPos = MoveCursor(new Point(0, 0));  // Relative — needs accurate starting pos
    ResetAttributes();
    output.EraseDown();
    previousScreen = new Screen();
}

// Fixed code:
if (isDone || previousScreen is null || previousWidth != width)
{
    output.WriteRaw("\x1b[H");                 // Absolute cursor home — works from any position
    currentPos = new Point(0, 0);
    ResetAttributes();
    output.EraseDown();
    previousScreen = new Screen();
}
```

**Why this matters:** After a resize, `_cursorPos` may be stale (pointing to coordinates that no longer exist in the resized terminal). Relative cursor movements from a stale position produce incorrect results. The absolute `CSI H` (cursor home) command positions the cursor at row 1, column 1 regardless of current position.

**Win32Output consideration:** `Win32Output` doesn't use `OutputScreenDiff` VT100 paths — it uses Win32 API calls. The `\x1b[H` sequence is only emitted through `Vt100Output` and `Windows10Output` (which enables VT100 processing).

### Change 8: Add `ResetForResize()` to Renderer

**File:** `src/Stroke/Rendering/Renderer.cs`

Add a new method that resets state without performing any I/O:

```csharp
/// <summary>
/// Reset renderer state for a terminal resize without performing any I/O.
/// The actual erase and redraw happen during the next <see cref="Render"/> call,
/// wrapped in synchronized output to prevent flicker.
/// </summary>
public void ResetForResize()
{
    _cursorPos = new Point(0, 0);
    _lastScreen = null;
    _lastSize = null;
    _lastStyle = null;
    _lastCursorShape = null;
    MouseHandlers = new MouseHandlers();
    _minAvailableHeight = 0;
    _cursorKeyModeReset = false;
    _mouseSupportEnabled = false;
}
```

### Change 9: Update `OnResize()` to use deferred erase

**File:** `src/Stroke/Application/Application.RunAsync.cs`

Replace the immediate `Erase()` with the state-only reset:

```csharp
// Current code (line 713-718):
private void OnResize()
{
    Renderer.Erase(leaveAlternateScreen: false);      // Immediate I/O — causes flicker
    Renderer.RequestAbsoluteCursorPosition();
    Invalidate();
}

// Fixed code:
private void OnResize()
{
    Renderer.ResetForResize();                         // State-only reset — no I/O
    Renderer.RequestAbsoluteCursorPosition();
    Invalidate();                                      // Next Render() handles erase + redraw
}
```

### Change 10: Wrap `Renderer.Erase()` in synchronized output

**File:** `src/Stroke/Rendering/Renderer.cs`

`Erase()` is still called from other paths (e.g., `EraseWhenDone`, application exit). Wrap it:

```csharp
public void Erase(bool leaveAlternateScreen = true)
{
    var output = _output;

    output.BeginSynchronizedOutput();

    output.CursorBackward(_cursorPos.X);
    output.CursorUp(_cursorPos.Y);
    output.EraseDown();
    output.ResetAttributes();
    output.EnableAutowrap();

    output.Flush();

    output.EndSynchronizedOutput();

    Reset(leaveAlternateScreen: leaveAlternateScreen);
}
```

### Change 11: Wrap `Renderer.Clear()` in synchronized output

**File:** `src/Stroke/Rendering/Renderer.cs`

```csharp
public void Clear()
{
    var output = _output;

    output.BeginSynchronizedOutput();

    output.EraseScreen();
    output.CursorGoto(0, 0);

    output.Flush();

    output.EndSynchronizedOutput();

    Reset();
}
```

## Files Changed

| File | Change |
|------|--------|
| `src/Stroke/Output/IOutput.cs` | Add `BeginSynchronizedOutput()`, `EndSynchronizedOutput()` |
| `src/Stroke/Output/Vt100Output.cs` | Implement sync methods + wrap `Flush()` in Mode 2026 markers |
| `src/Stroke/Output/Windows/Windows10Output.cs` | Delegate sync methods to `_vt100Output` |
| `src/Stroke/Output/Windows/ConEmuOutput.cs` | Delegate sync methods to `_vt100Output` |
| `src/Stroke/Output/Windows/Win32Output.cs` | No-op sync methods |
| `src/Stroke/Output/PlainTextOutput.cs` | No-op sync methods |
| `src/Stroke/Output/DummyOutput.cs` | No-op sync methods |
| `src/Stroke/Rendering/Renderer.cs` | Wrap `Render()`, `Erase()`, `Clear()` in sync; add `ResetForResize()` |
| `src/Stroke/Rendering/Renderer.Diff.cs` | Replace relative cursor home with absolute `CSI H` |
| `src/Stroke/Application/Application.RunAsync.cs` | Replace `Erase()` in `OnResize()` with `ResetForResize()` |

## Testing Strategy

### Automated Tests

1. **Vt100Output sync markers** — Verify `Flush()` prepends `\x1b[?2026h` and appends `\x1b[?2026l` when `BeginSynchronizedOutput()` is active
2. **Vt100Output without sync** — Verify `Flush()` writes raw content when sync is not active
3. **No-op implementations** — Verify `Win32Output`, `PlainTextOutput`, `DummyOutput` don't emit sync sequences
4. **Renderer state reset** — Verify `ResetForResize()` clears all expected state without I/O
5. **Absolute cursor home** — Verify `OutputScreenDiff` emits `\x1b[H` on full redraw path

### Manual Verification (TUI Driver)

1. Launch `fancy-zsh-prompt` example
2. Resize terminal window — verify no visible flicker
3. Verify prompt re-renders correctly at new size
4. Verify live clock continues updating after resize
5. Test on: macOS Terminal.app, iTerm2, Windows Terminal, Ghostty

### Regression Testing

1. Run full test suite (9,311 tests) — verify no failures
2. Run all 102 examples — verify rendering unchanged
3. Test on all CI platforms (macOS, Ubuntu, Windows)

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Terminal ignores Mode 2026 | Expected for older terminals | None — sequences silently ignored | Graceful degradation by design |
| Stale cursor position after resize | Low — absolute `CSI H` handles this | Medium — rendering glitch | `CSI H` is the fix |
| Synchronized output timeout | Very low — renders complete in milliseconds | Low — terminal auto-resets | Ghostty: 1s timeout; most terminals similar |
| Win32Output (legacy Windows) | N/A — no VT100 sequences | None | No-op implementation |
| Concurrent resize + render race | Low — Invalidate coalesces | Low — extra redraw at worst | Atomic flag + channel coalescing |

## Relationship to Python Prompt Toolkit

This is a **documented enhancement** over the shared rendering architecture, not a deviation from the API. The [synchronized output spec](https://gist.github.com/christianparpart/d8a62cc1ab659194337d73e399004036) was formalized around 2020 and terminal adoption has grown steadily since — it was not available when Python PTK's renderer was originally designed. Now that Mode 2026 enjoys broad support across modern terminals, Stroke can take advantage of it.

This fix maintains 100% API compatibility while improving rendering behavior. No public APIs are changed. The enhancement is entirely internal to the rendering pipeline.
