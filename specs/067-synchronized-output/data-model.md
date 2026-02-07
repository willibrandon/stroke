# Data Model: Synchronized Output (DEC Mode 2026)

**Feature**: 067-synchronized-output
**Date**: 2026-02-07

## Entities

### Synchronized Output Region

A logical bracket around terminal output where the terminal buffers all received data and commits it atomically as a single frame.

**Attributes**:
- **Active** (boolean): Whether synchronized output is currently active
- **Begin Marker** (constant string): `\x1b[?2026h` — CSI ? 2026 h (Set Mode)
- **End Marker** (constant string): `\x1b[?2026l` — CSI ? 2026 l (Reset Mode)

**State Transitions**:
- Inactive → Active: `BeginSynchronizedOutput()` called (sets flag)
- Active → Inactive: `EndSynchronizedOutput()` called (clears flag)
- Active + `Flush()`: `Flush()` reads the flag and wraps output in Mode 2026 markers, but does NOT change the flag state. The flag remains active until `EndSynchronizedOutput()` is called.

**Constraints**:
- Begin/End calls must be paired via try/finally to guarantee the end marker is sent even on exception (FR-017)
- The flag defaults to false (inactive) on construction
- The flag is protected by the existing `_lock` (System.Threading.Lock) in Vt100Output (FR-015)
- Begin and End are idempotent: multiple Begin calls without End keep the flag true; multiple End calls without Begin keep the flag false (FR-016)
- Flush() reads but does not mutate the flag

### Renderer State (extended)

The collection of cached state the renderer uses to compute differential screen updates. Extended with the `ResetForResize()` method that clears all state without I/O.

**Attributes** (all reset by `ResetForResize()`):
- **CursorPos** (Point): Current cursor position in rendered output
- **LastScreen** (Screen?): Previous rendered screen for diffing
- **LastSize** (Size?): Previous terminal size
- **LastStyle** (string?): Last style string applied
- **LastCursorShape** (CursorShape?): Last cursor shape set
- **MouseHandlers** (MouseHandlers): Mouse handlers from last render
- **MinAvailableHeight** (int): Minimum rows available for layout
- **CursorKeyModeReset** (bool): Whether cursor key mode has been reset
- **MouseSupportEnabled** (bool): Whether mouse tracking is active

## Relationships

```
IOutput  ──1:1──  Synchronized Output Flag (only in Vt100Output)
Renderer ──1:1──  IOutput (uses for all terminal I/O)
Renderer ──1:*──  Renderer State (manages rendering state)
Application ──1:1──  Renderer (owns the renderer)
```

## Data Flow: Render Cycle

```
Application.Invalidate()
  → Renderer.Render()
    → output.BeginSynchronizedOutput()  [sets flag]
    → ScreenDiff.OutputScreenDiff()     [writes to buffer]
    → output.Flush()                    [wraps buffer in Mode 2026 markers, writes to terminal]
    → output.EndSynchronizedOutput()    [clears flag]
```

## Data Flow: Resize

```
SIGWINCH signal
  → Application.OnResize()
    → Renderer.ResetForResize()         [resets all state, NO I/O]
    → Renderer.RequestAbsoluteCursorPosition()
    → Application.Invalidate()          [schedules redraw]
  → (next render cycle)
    → Renderer.Render()
      → _lastScreen is null → full-redraw path
      → output.WriteRaw("\x1b[H")       [absolute cursor home]
      → output.EraseDown()              [clear screen]
      → (render new content)            [all inside synchronized output block]
```
