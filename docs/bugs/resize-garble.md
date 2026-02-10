# Bug Report: Terminal Output Garbled on Resize (fancy-zsh-prompt)

**Reported:** 2026-02-10
**Severity:** High
**Affects:** All non-fullscreen prompts with multi-line or dynamic-width content
**Reproducible:** 100% of the time

---

## Summary

Resizing the terminal while the `fancy-zsh-prompt` example is waiting for input
produces severely garbled output. The prompt line duplicates dozens of times,
fragments overlap, timestamps get truncated mid-string, and the display becomes
unusable. Normal prompts partially recover on the next Enter, but the garble
persists through resize events.

## Steps to Reproduce

```bash
dotnet run --project examples/Stroke.Examples.Prompts -- fancy-zsh-prompt
```

1. Type `hello`, press Enter. Observe normal "You said: hello" output.
2. Type `world`, press Enter. Observe normal stacked prompts.
3. Grab the terminal window edge and resize (wider, narrower, or both).
4. Observe garbled output: duplicated prompt lines, truncated timestamps,
   overlapping fragments.

## Expected Behavior

On resize, the current prompt should be erased and cleanly redrawn at the
correct position, with previous prompt/response pairs remaining intact above.

## Actual Behavior

The prompt redraws at wrong positions, producing output like:

```
#  root  abc ~/.oh-my-zsh/themes  master!    py36   2026-02-10T02:11:26.1398
 root  abc ~/.oh-my-zsh/themes  master!    py36   2026-02-
 root  abc ~/.oh-my-zsh/th root  abc ~/.oh root  abc ~/.oh-my-zsh
 root  abc ~/.oh-my-zsh/themes  master!  root  abc ~/.oh
```

The garble worsens with rapid or large resizes. Each resize event stacks
another broken redraw on top.

## Regression History

| Commit | State | What changed |
|--------|-------|-------------|
| `6a33984` (PR #71) | Resize worked | Introduced `\x1b[H` absolute cursor home in full-redraw path + `ResetForResize()` + synchronized output |
| `0e43013` (ProgressBar) | **Resize broken** | Reverted `\x1b[H` back to `MoveCursor(new Point(0, 0))` because absolute home broke prompt stacking — each new prompt would jump to terminal row 1 instead of stacking below previous prompts |
| `28684b5` (IApplication) | Still broken | Only changed type signatures (`Application<object?>` to `IApplication`), no logic changes |

The regression was introduced in commit `0e43013` with this exact diff in
`Renderer.Diff.cs`:

```diff
-            output.WriteRaw("\x1b[H");
-            currentPos = new Point(0, 0);
+            currentPos = MoveCursor(new Point(0, 0));
```

The revert was intentional and necessary — `\x1b[H]` (CSI H, Cursor Home)
moves to **absolute** terminal row 1, column 1. In non-fullscreen mode, this
jumps above all previous prompt/response output, destroying the stacking
behavior that REPL-style prompts require. The commit message documents this:
"Use relative cursor movement instead of absolute home in screen diff (fixes
prompt stacking)".

## Root Cause Analysis

### The Desynchronization Problem

`ResetForResize()` sets `_cursorPos = (0, 0)` **without performing any terminal
I/O**:

```csharp
// Renderer.cs:449-461
public void ResetForResize()
{
    _cursorPos = new Point(0, 0);   // <-- tracked position says "origin"
    _lastScreen = null;              // <-- forces full redraw
    _lastSize = null;
    _lastStyle = null;
    _lastCursorShape = null;
    MouseHandlers = new MouseHandlers();
    _minAvailableHeight = 0;
    _cursorKeyModeReset = false;
    _mouseSupportEnabled = false;
    _bracketedPasteEnabled = false;
}
```

After this call:

- **Tracked cursor** (`_cursorPos`): `(0, 0)`
- **Real terminal cursor**: unchanged — still wherever the prompt was rendered

On the next `Render()` call, the full-redraw path triggers because
`_lastScreen` is null:

```csharp
// Renderer.Diff.cs:155-161
if (isDone || previousScreen is null || previousWidth != width)
{
    currentPos = MoveCursor(new Point(0, 0));   // target == current == (0,0)
    ResetAttributes();
    output.EraseDown();
    previousScreen = new Screen();
}
```

`MoveCursor(new Point(0, 0))` compares the target `(0, 0)` against
`currentPos` which is also `(0, 0)` (from the reset). Since they match,
**it emits zero cursor movement escape sequences**. The `EraseDown()` and
subsequent character writes happen at whatever position the real terminal
cursor occupies — which after a resize could be anywhere.

### Why `\x1b[H` Fixed Resize But Broke Stacking

`\x1b[H` (CSI H) is an **absolute** cursor positioning command — it moves to
row 1, column 1 of the terminal window, regardless of where the cursor
currently is. This correctly repositions the cursor after a resize (fixing the
garble), but in non-fullscreen mode the prompt output starts at line N of the
terminal (not line 1). Previous prompt/response pairs sit above that region.
`\x1b[H` jumps above them all, causing each redraw to overwrite historical
output.

### How Python Handles It

Python PTK's `_on_resize` (application.py:590-600) takes a different approach:

```python
def _on_resize(self) -> None:
    self.renderer.erase(leave_alternate_screen=False)
    self._request_absolute_cursor_position()
    self._redraw()
```

Python calls `erase()` first, which uses the **current** `_cursor_pos` to
move the cursor to the prompt origin via relative movement **before** resetting
state:

```python
def erase(self, leave_alternate_screen=True):
    output.cursor_backward(self._cursor_pos.x)   # uses tracked position
    output.cursor_up(self._cursor_pos.y)           # uses tracked position
    output.erase_down()
    output.reset_attributes()
    output.enable_autowrap()
    output.flush()
    self.reset(leave_alternate_screen=leave_alternate_screen)  # NOW reset
```

The critical difference: Python moves the cursor to the origin **using the
still-valid tracked position**, then clears the screen, and only then resets
`_cursor_pos` to `(0, 0)`. After this sequence, the tracked position and real
cursor position are synchronized at `(0, 0)` — the prompt origin.

**Python has the same garbling problem** — it is just as bad. After a resize,
the terminal may have reflowed text, changed line wrapping, or shifted the
scroll buffer. The relative cursor movement in `erase()` uses `_cursor_pos`
values that were computed before the resize, so the cursor does not reach the
true prompt origin. The erase wipes the wrong region, the redraw writes at
the wrong position, and the result is the same garbled mess seen in Stroke.

### Synchronized Output Does Not Help Here

PR #71 added DEC Mode 2026 synchronized output wrapping
(`BeginSynchronizedOutput` / `EndSynchronizedOutput`) around `Render()`,
`Erase()`, and `Clear()`. This prevents the terminal from painting
intermediate states during a render cycle. However, synchronized output only
solves **flicker** (partial frames becoming visible). It does not solve the
garble because the garble is caused by the cursor being at the wrong position
when the render *starts* — the atomically painted frame is itself wrong.

## Affected Code Paths

| File | Lines | Role |
|------|-------|------|
| `src/Stroke/Rendering/Renderer.cs` | 449-461 | `ResetForResize()` — zeroes `_cursorPos` without I/O |
| `src/Stroke/Rendering/Renderer.Diff.cs` | 155-161 | Full-redraw path — `MoveCursor(0,0)` is no-op after reset |
| `src/Stroke/Application/Application.RunAsync.cs` | 715-720 | `OnResize()` — calls `ResetForResize()` + `Invalidate()` |
| `src/Stroke/Rendering/Renderer.Diff.cs` | 68-103 | `MoveCursor()` — relative movement based on tracked `currentPos` |

## Constraints on the Fix

Any fix must satisfy both requirements simultaneously:

1. **Prompt stacking**: In non-fullscreen mode, successive prompts must stack
   below each other. The renderer must not jump to terminal row 1.

2. **Cursor synchronization on resize**: After a resize, the tracked
   `_cursorPos` must match the real terminal cursor position before the
   full-redraw path begins writing characters.

The `\x1b[H` approach satisfies (2) but violates (1).
The current `MoveCursor` approach satisfies (1) but violates (2).

## Relation to Python PTK

Python PTK has the same garbling on resize. Its `erase()` approach — relative
cursor move using pre-resize tracked position, then reset — does not prevent
the garble because the tracked position is stale after terminal reflow.

Python has not fixed this. However, that does not mean it is unfixable.
Terminals do provide mechanisms that could be used to solve this:

- **CPR (Cursor Position Report)**: After resize, send `\x1b[6n` and wait for
  the terminal to respond with the actual cursor row/column. This gives the
  true cursor position post-reflow. Stroke already has CPR infrastructure
  (`RequestAbsoluteCursorPosition`, `ReportAbsoluteCursorRow`).
- **Alternate screen buffer**: Fullscreen apps using the alternate screen
  buffer (`\x1b[?1049h`) don't have this problem because the alternate screen
  is a fixed grid that doesn't reflow.
- **Save/restore cursor**: `\x1b[s` / `\x1b[u` (DECSC/DECRC) could bracket
  the prompt region if the terminal preserves saved position across resize.
- **Absolute positioning with offset**: Instead of `\x1b[H` (row 1), use
  `\x1b[{row};1H` with a row computed from CPR or from RowsAboveLayout to
  target the prompt origin specifically.
