# Design: Resize Rebase — Reflow-Aware Cursor Repositioning

**Date:** 2026-02-10
**Bug:** `docs/bugs/resize-garble.md`
**Status:** Proposed

---

## Problem

Resizing the terminal while a non-fullscreen prompt is displayed produces garbled output. The root cause is a **cursor position desynchronization**: `ResetForResize()` (`Renderer.cs:449-461`) zeroes `_cursorPos` to `(0,0)` without emitting any terminal I/O. When the full-redraw path in `OutputScreenDiff` (`Renderer.Diff.cs:155-161`) calls `MoveCursor(new Point(0, 0))`, the target matches the tracked position, so **zero escape sequences are emitted**. The real terminal cursor — wherever reflow left it — stays put, and all subsequent `EraseDown` + character writes land at the wrong position.

A prior fix used `\x1b[H` (CSI H, absolute cursor home) which correctly repositioned the cursor but broke prompt stacking: `\x1b[H` jumps to terminal row 1, overwriting all previous prompt/response output above the current prompt region. That fix was intentionally reverted in commit `0e43013`.

## What the code confirms

From `Renderer.cs` and `Renderer.Diff.cs`:

* Full redraw does: `currentPos = MoveCursor(new Point(0, 0)); ResetAttributes(); EraseDown();`
* `MoveCursor` is purely **relative**, driven by `currentPos` (the tracked `_cursorPos`).

The current bug is worst-case because `ResetForResize()` does:

```csharp
_cursorPos = new Point(0, 0);  // <- lies about where the real cursor is
_lastScreen = null;            // <- forces full redraw
```

So the full redraw path starts with **a no-op cursor move** and erases/draws from wherever the terminal cursor actually is after the resize.

## Why a naive CPR fix is insufficient

The obvious next step is to request a Cursor Position Report after resize, then compute:

```
promptRow = cprRow - _cursorPos.Y
```

This assumes `_cursorPos.Y` (the logical screen row) equals the physical terminal row offset from prompt origin to cursor. **After resize, that assumption is false.** Terminal text reflow can turn a single logical screen row into multiple physical rows (if the terminal narrowed) or collapse multiple physical rows into fewer (if it widened). The correct relationship is:

```
promptRow = cprRow - physicalCursorOffsetY_after_reflow
```

Where `physicalCursorOffsetY_after_reflow` is derived from the **old screen contents** reflowed at the **new terminal width** — not from `_cursorPos.Y`.

Additionally, computing the prompt row inside `ReportAbsoluteCursorRow` (the CPR callback) is too early: at that point the new terminal width may not yet be known to the renderer.

## Design: Treat resize as a "rebase" problem

### Principle

On resize, do **not** try to guess prompt origin by "absolute home" (`\x1b[H`) or by "old logical row count".

Instead:

1. **Snapshot what was last rendered** (screen + cursor position) *once* when resize begins.
2. On the next render, inside the synchronized output block:
   * Compute how the old screen would occupy physical lines under the new width (**reflow model**).
   * Use that to compute how many **physical terminal lines** the cursor is below the prompt origin.
   * Move the real cursor back to the prompt origin (`\r` + `CursorUp(dy)`), set tracked `_cursorPos=(0,0)`.
3. Then do the normal full redraw (`EraseDown` + repaint).

This keeps prompt stacking intact because we **never jump to terminal row 1** — we only move within the current viewport relative to the prompt origin.

### Why this matches fish / reedline / rustyline patterns

Those editors all effectively do this:

* Track the last rendered content geometry.
* On resize, recompute how many terminal rows the prompt+buffer occupies.
* Reposition and redraw.

The `Screen` abstraction is already the perfect source-of-truth for that geometry.

---

## How terminals actually reflow: insights from XtermSharp

XtermSharp (Miguel de Icaza's .NET terminal emulator, a port of xterm.js) provides a ground-truth reference for how terminal emulators handle text reflow on resize. Understanding the terminal side of the equation validates our reflow model and reveals important simplifications.

### Soft wraps vs hard wraps

Terminal emulators distinguish two kinds of line breaks:

- **Soft wrap** (`BufferLine.IsWrapped = true`): The line continued from the previous line because a character was written past the right margin with autowrap enabled. These are created by the terminal's character-writing logic (`InputHandler.cs:1296-1308`): when a character's width would exceed the right margin, the terminal either calls `Scroll(isWrapped: true)` (at scroll bottom) or sets `nextLine.IsWrapped = true` on the existing next line.

- **Hard wrap** (`BufferLine.IsWrapped = false`): An explicit newline (`\n`). The terminal's `LineFeedBasic()` method (`Terminal.cs:858-878`) calls `Scroll(isWrapped: false)`, creating a new buffer line with `IsWrapped = false`. Carriage return (`\r`) only moves the cursor to column 0 — it does not affect `IsWrapped`.

This distinction is the entire basis for reflow:

- **When narrowing**: Only soft-wrapped line groups and hard-wrapped lines whose content exceeds the new width are candidates for reflow. Hard-wrapped lines that fit within the new width are skipped entirely (`ReflowNarrower.cs`).
- **When widening**: Only soft-wrapped lines are merged back together. Hard-wrapped lines are never merged — they remain separate buffer lines (`ReflowWider.cs`).

**Note:** Cursor movement commands (CursorUp, CursorDown, CursorForward, CursorBackward, SetCursor) do **not** modify `IsWrapped`. Only character writing at the right margin and linefeed set this flag.

### Why Stroke's output is all hard wraps

Stroke's `ScreenDiff.MoveCursor()` (`Renderer.Diff.cs:68-103`) moves downward between logical screen rows using `\r\n`:

```csharp
// Renderer.Diff.cs:75-81
ResetAttributes();
output.Write(string.Concat(Enumerable.Repeat("\r\n", target.Y - currentY)));
```

The `\r` triggers `CarriageReturn()` (moves X to 0, no `IsWrapped` change). The `\n` triggers `LineFeedBasic()` → `Scroll(isWrapped: false)`. Every new line created by Stroke's renderer has `IsWrapped = false`.

Additionally, `ScreenDiff` **disables autowrap** before rendering (`Renderer.Diff.cs:149-152`):

```csharp
if (previousScreen is null || !fullScreen)
{
    output.DisableAutowrap();
}
```

And it clamps content to `width - 1` columns (`Renderer.Diff.cs:177`):

```csharp
int newMaxLineLen = Math.Min(width - 1, GetMaxColumnIndex(newRow));
```

With autowrap disabled and content clamped to `width - 1`, no character write can trigger a soft wrap. **Every row boundary in Stroke's output is a hard newline.** The terminal will never set `IsWrapped = true` for any line produced by Stroke.

### Implications for the reflow model

Because Stroke produces only hard wraps, the terminal's buffer reflow engine (as implemented in XtermSharp) will:

1. **Never merge rows when widening.** Soft-wrap merging only applies to lines with `IsWrapped = true`. Since all Stroke lines are hard-wrapped, widening the terminal does not reduce the number of buffer lines. Each logical screen row remains one buffer line. `PhysicalLines(len, newWidth)` correctly returns 1 for any row where `len <= newWidth`.

2. **Never split hard-wrapped rows into new buffer lines when narrowing.** The reflow engine's narrowing logic (`ReflowNarrower.cs`) does restructure the buffer for soft-wrapped groups and for hard-wrapped lines that exceed the new width. But the key insight is that even without buffer restructuring, a hard-wrapped line whose content exceeds `newWidth` still **visually wraps** on screen — the terminal displays it across multiple physical rows. Our `PhysicalLines(len, newWidth)` formula (`ceil(len / newWidth)`) correctly counts these physical display rows.

3. **The cursor position is minimally adjusted.** XtermSharp clamps the cursor (`X = Math.Min(X, newCols - 1)`, `Y = Math.Min(Y, newRows - 1)`) but does **not** move the cursor to follow reflowed content. Both `ReflowWider` and `ReflowNarrower` explicitly **skip line groups containing the cursor** (`ReflowNarrower.cs:34-40`, `ReflowWider.cs:52-57`), leaving cursor fixup to the application.

This validates the core assumption of our reflow model: **after resize, the cursor remains at approximately the same absolute terminal position, and we can compute the physical row distance to the prompt origin from how many physical display rows the old content occupies at the new width.**

### Wide character boundary edge case

When the terminal visually wraps a hard-wrapped line (i.e., the content exceeds `newWidth`), wide characters (CJK, width=2) at wrap boundaries introduce a subtlety.

**The problem:** If a width-2 character would straddle a physical line boundary — starting at column `newWidth - 1` and needing column `newWidth` which doesn't exist — the terminal cannot split the character. Instead, it:

1. Leaves column `newWidth - 1` blank (or shows a placeholder).
2. Places the wide character at column 0 of the next physical row.

This means the physical row holds only `newWidth - 1` effective cells instead of `newWidth`. The naive `ceil(len / newWidth)` formula can **undercount** by 1 or more physical rows when wide characters happen to fall at wrap boundaries.

**Example:** A row with 10 cells of content at `newWidth = 5`:
- All narrow chars: `ABCDE` | `FGHIJ` → 2 physical rows. `ceil(10/5) = 2` ✓
- Wide char at position 4: `ABCD` | `__WW` | `EFGH` → the wide char (W, width=2) at index 4 would start at column 4 and need column 5. Terminal pads column 4, pushes W to next row. Result: 3 physical rows, but `ceil(10/5) = 2` ✗

XtermSharp handles this in `ReflowNarrower.GetNewLineLengths()` (`ReflowNarrower.cs:213-252`) by checking each wrap boundary:

```csharp
bool endsWithWide = wrappedLines[srcLine].GetWidth(srcCol - 1) == 2;
if (endsWithWide) {
    srcCol--;  // Back up to avoid splitting wide char
}
int lineLength = endsWithWide ? newCols - 1 : newCols;
```

**Impact on our model:** The `PhysicalLines` formula is an approximation. For rows containing only narrow (width=1) characters, it is exact. For rows with wide characters, it may undercount, but only when a wide character happens to straddle a wrap boundary — a relatively uncommon scenario for typical prompt content.

**Our safety net:** The CPR column confidence check catches this discrepancy. If wide characters cause the actual physical line count to differ from our model, the cursor's real column (reported by CPR) won't match our expected column, triggering the safe fallback (`\r\n` to start fresh). This is by design — the confidence check exists precisely for cases where the model and terminal disagree.

**Possible future improvement:** An exact `PhysicalLines` could be computed by scanning the row's cell data and simulating the wrap with wide-character boundary handling. This would require iterating through cells rather than using a simple `ceil(len/width)`. Whether this is worth the complexity depends on how often the approximation causes visible artifacts in practice. The safe fallback makes this a quality-of-life improvement rather than a correctness requirement.

---

## Concrete integration

### A) Fix both resize pathways in `Application.RunAsync.cs`

#### 1. `OnResize()` calls `PrepareForResize()` instead of `ResetForResize()`

Current (`Application.RunAsync.cs:715-720`):

```csharp
private void OnResize()
{
    Renderer.ResetForResize();
    RequestAbsoluteCursorPosition();
    Invalidate();
}
```

Becomes:

```csharp
private void OnResize()
{
    Renderer.PrepareForResize();
    RequestAbsoluteCursorPosition();
    Invalidate();
}
```

#### 2. Polling path marshals `OnResize()` instead of `Invalidate()`

Right now, the polling path (`Application.RunAsync.cs:217-233`) bypasses resize logic entirely:

```csharp
if (newSize != lastSize)
{
    lastSize = newSize;
    Invalidate();
}
```

That means on platforms where polling is the resize signal (Windows, or SIGWINCH not available), the renderer never gets `PrepareForResize()` — the cursor desync happens silently.

Fix — marshal through the same `OnResize()` as SIGWINCH:

```csharp
if (newSize != lastSize)
{
    lastSize = newSize;
    _actionChannel?.Writer.TryWrite(OnResize);
}
```

This single change removes an entire class of "resize detected but renderer not prepared" bugs, and ensures one coherent resize pipeline everywhere.

---

### B) Replace `ResetForResize()` with a reflow-aware "resize rebase" mechanism in `Renderer.cs`

#### 1. Add resize snapshot fields

Add near the existing rendering state fields (`Renderer.cs`, after line 67):

```csharp
// Resize rebase state (async context only; CPR fields guarded by _cprLock)
private bool _resizePending;
private Screen? _resizeScreenSnapshot;
private Size? _resizeSizeSnapshot;
private Point _resizeCursorSnapshot;

// CPR captured after resize (for safety/validation)
private int? _resizeCprRow;
private int? _resizeCprCol;
```

The snapshot fields (`_resizePending`, `_resizeScreenSnapshot`, `_resizeSizeSnapshot`, `_resizeCursorSnapshot`) live on the async context thread — no lock required. The CPR fields (`_resizeCprRow`, `_resizeCprCol`) are written by the CPR callback thread and read by `Render()`, so they use the existing `_cprLock`.

#### 2. Replace `ResetForResize()` with `PrepareForResize()`

Key requirements:

* Do **not** mutate `_cursorPos` — that's the whole bug.
* Capture snapshots only once if multiple resizes happen before the next render (burst coalescing).
* Still invalidate diff state to force a full redraw.

Replaces `Renderer.cs:449-461`:

```csharp
/// <summary>
/// Prepare renderer state for a terminal resize without performing any terminal I/O.
/// Snapshots the current screen and cursor position for reflow-aware repositioning
/// in the next <see cref="Render"/> call.
/// </summary>
public void PrepareForResize()
{
    // Snapshot once per resize-burst, before we nuke diff state.
    if (!_resizePending)
    {
        _resizePending = true;
        _resizeScreenSnapshot = _lastScreen;
        _resizeSizeSnapshot = _lastSize;
        _resizeCursorSnapshot = _cursorPos;
    }

    // Clear CPR resize data (thread-safe since CPR callback writes these)
    using (_cprLock.EnterScope())
    {
        _resizeCprRow = null;
        _resizeCprCol = null;
    }

    // Force full redraw next Render(), but DO NOT lie about _cursorPos.
    _lastScreen = null;
    _lastSize = null;
    _lastStyle = null;
    _lastCursorShape = null;
    MouseHandlers = new MouseHandlers();
    _minAvailableHeight = 0;

    // Re-acquire terminal modes next render
    _cursorKeyModeReset = false;
    _mouseSupportEnabled = false;
    _bracketedPasteEnabled = false;
}

/// <summary>
/// Backward compatibility alias for <see cref="PrepareForResize"/>.
/// </summary>
public void ResetForResize() => PrepareForResize();
```

#### 3. Clear resize fields in `Reset()`

In `Reset()` (`Renderer.cs:400-436`), add cleanup for resize state alongside the existing field resets:

```csharp
_resizePending = false;
_resizeScreenSnapshot = null;
_resizeSizeSnapshot = null;
_resizeCursorSnapshot = default;
```

#### 4. Make `Render()` treat "size changed" as resize even if no explicit event

The existing check in `Render()` (`Renderer.cs:248-251`) just nulls `_lastScreen`:

```csharp
if (_lastSize != size)
{
    _lastScreen = null;
}
```

This covers "polling detected a change" but never calls `PrepareForResize()`, so the cursor desync still happens.

Replace with:

```csharp
if (_lastSize is { } prevSize && prevSize != size)
{
    // Covers: polling-based resizes, outputs that change size without SIGWINCH, etc.
    PrepareForResize();
}
else if (_lastSize != size)
{
    // First render: no previous screen.
    _lastScreen = null;
}
```

This is belt-and-suspenders: even if the polling path or SIGWINCH already called `PrepareForResize()`, hitting it again is safe because the `if (!_resizePending)` guard prevents re-snapshotting.

#### 5. Rebase cursor inside synchronized output before `ScreenDiff`

In the synchronized output block (`Renderer.cs:288`), **before** the `OutputScreenDiff` call at line 292, insert:

```csharp
output.BeginSynchronizedOutput();
try
{
    // --- Resize rebase: reposition cursor to prompt origin ---
    if (_resizePending)
    {
        RebaseCursorAfterResize(output, size);
    }
    // --- End resize rebase ---

    var (newCursorPos, newLastStyle) = ScreenDiff.OutputScreenDiff(
        ...
    );
```

After this block, `_cursorPos` is `(0,0)` and the real cursor is at the prompt origin. `OutputScreenDiff`'s full-redraw path (`MoveCursor(0,0)` at `Renderer.Diff.cs:157`) correctly sees target==current and emits no movement. `EraseDown` clears from the prompt origin down. Character writes fill in the new content.

#### 6. Implement `RebaseCursorAfterResize` and helpers

```csharp
private void RebaseCursorAfterResize(IOutput output, Size newSize)
{
    int newWidth = Math.Max(1, newSize.Columns);

    // Snapshot CPR info (thread-safe)
    int? cprRow;
    int? cprCol;
    using (_cprLock.EnterScope())
    {
        cprRow = _resizeCprRow;
        cprCol = _resizeCprCol;
    }

    // Compute how many *physical* lines we are below the prompt origin after reflow.
    int dy;
    if (_resizeScreenSnapshot is not null)
    {
        dy = ComputePhysicalCursorOffsetAfterReflow(
            _resizeScreenSnapshot, _resizeCursorSnapshot, newWidth);
    }
    else
    {
        // Best effort if we don't have a snapshot.
        dy = _resizeCursorSnapshot.Y;
    }

    // Safety gating:
    // - If CPR row exists, ensure we don't try to move above the top of the viewport.
    // - If CPR col exists, ensure the terminal's reflow matches our model.
    bool safe = true;

    if (cprRow is int row1Based)
    {
        safe = dy <= (row1Based - 1);
    }

    if (safe && cprCol is int col1Based)
    {
        int expectedCol = (_resizeCursorSnapshot.X % newWidth) + 1;
        if (col1Based != expectedCol)
        {
            safe = false;
        }
    }

    // Also ensure dy isn't clearly insane.
    if (dy < 0 || dy >= newSize.Rows)
    {
        safe = false;
    }

    if (safe)
    {
        // Move to column 1 on the current physical line, then move up dy lines.
        output.Write("\r");
        if (dy > 0)
            output.CursorUp(dy);
    }
    else
    {
        // Non-destructive fallback:
        // Start a fresh prompt region rather than erasing user history from a wrong origin.
        output.Write("\r\n");
    }

    // Now force ScreenDiff's full-redraw path to be aligned with reality.
    _cursorPos = new Point(0, 0);

    // Clear resize state
    _resizePending = false;
    _resizeScreenSnapshot = null;
    _resizeSizeSnapshot = null;
}
```

### C) The keystone algorithm: `ComputePhysicalCursorOffsetAfterReflow`

This models how the terminal reflowed the old screen content at the new width, computing the **physical** row distance from prompt origin to cursor.

For each old screen row 0 through `cursorY-1`:
- Compute the effective row width in cells (excluding trailing unstyled whitespace, accounting for wide characters).
- Compute how many physical lines that row occupies at `newWidth`: `ceil(len / newWidth)`, minimum 1.
- Sum these physical line counts.

For the cursor's own row, add the cursor's physical sub-row within that row: `floor(cursorX / newWidth)`.

**Note:** assumes each logical screen row ends with a hard newline in the terminal buffer. This is verified true — the renderer uses `\r\n` to move down (`Renderer.Diff.cs:78`) and disables autowrap (`Renderer.Diff.cs:151`), so every row boundary is a hard wrap (`IsWrapped = false` in terminal emulator terms). See "How terminals actually reflow" section above.

**Wide character limitation:** The `PhysicalLines` helper uses `ceil(len / width)`, which is exact for rows containing only narrow (width=1) characters. For rows with CJK or other width-2 characters, this can undercount when a wide character straddles a physical line boundary (the terminal pads the boundary, adding an extra physical row). The CPR column confidence check catches this discrepancy and triggers the safe fallback. See "Wide character boundary edge case" above for details.

```csharp
private int ComputePhysicalCursorOffsetAfterReflow(
    Screen oldScreen, Point oldCursor, int newWidth)
{
    int y = Math.Max(0, oldCursor.Y);
    int x = Math.Max(0, oldCursor.X);

    int offsetY = 0;

    // Rows above the cursor row
    for (int row = 0; row < y; row++)
    {
        int len = GetRowUsedWidth(oldScreen, row);
        offsetY += PhysicalLines(len, newWidth);
    }

    // Cursor row: cursor might be beyond last drawn cell (due to cursor motion).
    // Only matters for within-row wraps.
    offsetY += x / newWidth;

    return offsetY;
}

private static int PhysicalLines(int cellLen, int width)
{
    if (width <= 0) return 1;
    if (cellLen <= 0) return 1;
    return (cellLen + width - 1) / width;
}
```

`GetRowUsedWidth` mirrors the logic in `ScreenDiff.GetMaxColumnIndex` (`Renderer.Diff.cs:125-140`): find the maximum column index where the cell has content or a styled space, but also account for wide characters (where a single cell occupies `Width` > 1 columns):

```csharp
// Add this alias at top of Renderer.cs to avoid ambiguity with System.Char
using Char = Stroke.Layout.Char;

private int GetRowUsedWidth(Screen screen, int row)
{
    if (screen.DataBuffer is not IDictionary<int, Dictionary<int, Char>> buffer)
        return 0;

    if (!buffer.TryGetValue(row, out var rowDict) || rowDict is null || rowDict.Count == 0)
        return 0;

    int maxExclusive = 0;
    bool hasStyleCache = _styleStringHasStyle is not null;

    foreach (var (index, cell) in rowDict)
    {
        bool significant =
            cell.Character != " " ||
            (hasStyleCache && _styleStringHasStyle![cell.Style]);

        if (!significant)
            continue;

        int w = cell.Width > 0 ? cell.Width : 1;
        int end = index + w; // exclusive end position in cells
        if (end > maxExclusive)
            maxExclusive = end;
    }

    return maxExclusive;
}
```

The expected CPR column for the confidence check:

```csharp
int expectedCol = (_resizeCursorSnapshot.X % newWidth) + 1;  // 1-based
```

---

### D) Upgrade CPR reporting to include column

`CprBindings.cs:46-58` already parses **both** row and col from the `\x1b[row;colR` response but discards `col`:

```csharp
var row = int.Parse(parts[0]);
var col = int.Parse(parts[1]);
@event.GetApp().Renderer.ReportAbsoluteCursorRow(row);
```

For a high-quality fix, pass both so the renderer can:
- Validate that the terminal's post-resize cursor column matches the model.
- Clamp "move up" safely (`dy <= cprRow - 1`).

#### 1. Add `ReportAbsoluteCursorPosition(int row, int col)` to `Renderer.cs`

```csharp
/// <summary>
/// Report the absolute cursor position. Called when a CPR response is received.
/// </summary>
/// <param name="row">The absolute cursor row (1-based).</param>
/// <param name="col">The absolute cursor column (1-based).</param>
public void ReportAbsoluteCursorPosition(int row, int col)
{
    using (_cprLock.EnterScope())
    {
        _cprSupport = CprSupportState.Supported;

        int totalRows = _output.GetSize().Rows;
        int rowsBelowCursor = totalRows - row + 1;
        _minAvailableHeight = rowsBelowCursor;

        if (_waitingForCprFutures.Count > 0)
        {
            var tcs = _waitingForCprFutures.Dequeue();
            tcs.TrySetResult(true);
        }

        // Store CPR position for resize safety validation
        if (_resizePending)
        {
            _resizeCprRow = row;
            _resizeCprCol = col;
        }
    }
}
```

#### 2. Keep the old method for compatibility

```csharp
public void ReportAbsoluteCursorRow(int row)
{
    ReportAbsoluteCursorPosition(row, 1);
}
```

#### 3. Update `CprBindings.cs:55`

```diff
-@event.GetApp().Renderer.ReportAbsoluteCursorRow(row);
+@event.GetApp().Renderer.ReportAbsoluteCursorPosition(row, col);
```

---

### E) Repositioning strategy

**Relative positioning** (`\r` + `CursorUp(dy)`) is the primary strategy:

- `\r` moves to column 0 of the current physical line.
- `CursorUp(dy)` moves up by `dy` **physical lines** (not logical screen rows).
- This works because `dy` is computed from the reflow model in physical lines.

CPR is used as **validation, not as the primary positioning mechanism**:

- If `cprCol != expectedCol`, the terminal reflowed differently than modeled — use safe fallback.
- If `dy > cprRow - 1`, the prompt origin would be above the viewport — use safe fallback.

**Safe fallback** — "do no harm" principle: never erase user history above the prompt when unsure. Write `\r\n` to start a fresh prompt region. This yields a duplicated prompt line *at worst*, but avoids destructive erases and garble. A much better failure mode for production tooling.

---

## Thread safety

- `_resizePending`, `_resizeScreenSnapshot`, `_resizeSizeSnapshot`, `_resizeCursorSnapshot` are only accessed on the async context thread (`PrepareForResize()` is called from `OnResize()` which is marshaled via `_actionChannel`, and `RebaseCursorAfterResize()` runs in `Render()`). No lock required.
- `_resizeCprRow` and `_resizeCprCol` are written by the CPR callback thread (`ReportAbsoluteCursorPosition`) and read by `Render()`. Both access paths are guarded by the existing `_cprLock`.
- The `if (!_resizePending)` guard in `PrepareForResize()` is safe because it only runs on the async context thread.

---

## Files changed

| File | What changes |
|------|-------------|
| `src/Stroke/Rendering/Renderer.cs` | Add resize snapshot fields. Replace `ResetForResize()` with `PrepareForResize()` (keep old as alias). Add `ReportAbsoluteCursorPosition(int, int)`. Add `ComputePhysicalCursorOffsetAfterReflow`, `PhysicalLines`, `GetRowUsedWidth`. Add rebase block in `Render()`. Clear resize fields in `Reset()`. Upgrade `_lastSize` check in `Render()` to call `PrepareForResize()`. |
| `src/Stroke/Application/Bindings/CprBindings.cs` | Pass `col` to `ReportAbsoluteCursorPosition(row, col)`. |
| `src/Stroke/Application/Application.RunAsync.cs` | `OnResize()` calls `PrepareForResize()` instead of `ResetForResize()`. Polling path marshals `OnResize()` via `_actionChannel` instead of calling `Invalidate()` directly. |
| `src/Stroke/Rendering/Renderer.Diff.cs` | No changes. |

---

## Concrete patch plan

### Patch 1: `CprBindings.cs` — report row+col

```diff
-        @event.GetApp().Renderer.ReportAbsoluteCursorRow(row);
+        @event.GetApp().Renderer.ReportAbsoluteCursorPosition(row, col);
```

### Patch 2: `Application.RunAsync.cs` — unify resize handling

#### A) OnResize calls `PrepareForResize()`

```diff
 private void OnResize()
 {
-    Renderer.ResetForResize();
+    Renderer.PrepareForResize();
     RequestAbsoluteCursorPosition();
     Invalidate();
 }
```

#### B) Polling path marshals `OnResize()` instead of `Invalidate()`

```diff
 if (newSize != lastSize)
 {
     lastSize = newSize;
-    Invalidate();
+    _actionChannel?.Writer.TryWrite(OnResize);
 }
```

### Patch 3: `Renderer.cs` — snapshot-on-resize + reflow-aware cursor rebase

#### A) Add `using` alias and fields

At the top:

```csharp
using Char = Stroke.Layout.Char;
```

Add fields near rendering state:

```csharp
// Resize rebase state (async context)
private bool _resizePending;
private Screen? _resizeScreenSnapshot;
private Size? _resizeSizeSnapshot;
private Point _resizeCursorSnapshot;

// CPR captured after resize (for safety/validation; guarded by _cprLock)
private int? _resizeCprRow;
private int? _resizeCprCol;
```

#### B) Replace `ResetForResize()` with `PrepareForResize()`

```diff
-public void ResetForResize()
-{
-    _cursorPos = new Point(0, 0);
-    _lastScreen = null;
-    _lastSize = null;
-    _lastStyle = null;
-    _lastCursorShape = null;
-    MouseHandlers = new MouseHandlers();
-    _minAvailableHeight = 0;
-    _cursorKeyModeReset = false;
-    _mouseSupportEnabled = false;
-    _bracketedPasteEnabled = false;
-}
+public void PrepareForResize()
+{
+    // Snapshot once per resize-burst, before we nuke diff state.
+    if (!_resizePending)
+    {
+        _resizePending = true;
+        _resizeScreenSnapshot = _lastScreen;
+        _resizeSizeSnapshot = _lastSize;
+        _resizeCursorSnapshot = _cursorPos;
+    }
+
+    using (_cprLock.EnterScope())
+    {
+        _resizeCprRow = null;
+        _resizeCprCol = null;
+    }
+
+    // Force full redraw next Render(), but DO NOT lie about _cursorPos.
+    _lastScreen = null;
+    _lastSize = null;
+    _lastStyle = null;
+    _lastCursorShape = null;
+    MouseHandlers = new MouseHandlers();
+    _minAvailableHeight = 0;
+    _cursorKeyModeReset = false;
+    _mouseSupportEnabled = false;
+    _bracketedPasteEnabled = false;
+}
+
+public void ResetForResize() => PrepareForResize();
```

#### C) `Render()` — upgrade `_lastSize` check to trigger `PrepareForResize()`

```diff
-if (_lastSize != size)
-{
-    _lastScreen = null;
-}
+if (_lastSize is { } prevSize && prevSize != size)
+{
+    // Covers: polling-based resizes, outputs that change size without SIGWINCH, etc.
+    PrepareForResize();
+}
+else if (_lastSize != size)
+{
+    // First render: no previous screen.
+    _lastScreen = null;
+}
```

#### D) `Render()` — rebase cursor inside synchronized output

```diff
 output.BeginSynchronizedOutput();
 try
 {
+    if (_resizePending)
+    {
+        RebaseCursorAfterResize(output, size);
+    }
+
     var (newCursorPos, newLastStyle) = ScreenDiff.OutputScreenDiff(
```

#### E) `Reset()` — clear resize fields

Add alongside the existing field resets in `Reset()`:

```csharp
_resizePending = false;
_resizeScreenSnapshot = null;
_resizeSizeSnapshot = null;
_resizeCursorSnapshot = default;
```

#### F) Add `ReportAbsoluteCursorPosition(int row, int col)`

```csharp
public void ReportAbsoluteCursorPosition(int row, int col)
{
    using (_cprLock.EnterScope())
    {
        _cprSupport = CprSupportState.Supported;

        int totalRows = _output.GetSize().Rows;
        int rowsBelowCursor = totalRows - row + 1;
        _minAvailableHeight = rowsBelowCursor;

        if (_waitingForCprFutures.Count > 0)
        {
            var tcs = _waitingForCprFutures.Dequeue();
            tcs.TrySetResult(true);
        }

        if (_resizePending)
        {
            _resizeCprRow = row;
            _resizeCprCol = col;
        }
    }
}
```

Rewrite existing method:

```diff
 public void ReportAbsoluteCursorRow(int row)
 {
-    using (_cprLock.EnterScope())
-    {
-        _cprSupport = CprSupportState.Supported;
-        ...
-    }
+    ReportAbsoluteCursorPosition(row, 1);
 }
```

#### G) Add private helper methods

`RebaseCursorAfterResize`, `ComputePhysicalCursorOffsetAfterReflow`, `PhysicalLines`, and `GetRowUsedWidth` as defined in sections B.6 and C above.

---

## Why this is the "no compromises" version

- **Fixes the root cause**: tracked cursor no longer lies on resize.
- **Does not use `\x1b[H`**: prompt stacking is safe.
- **Correct under reflow**: the key missing piece — computes physical offset from the last rendered screen instead of using stale `_cursorPos.Y`.
- **Works regardless of how resize is detected**: SIGWINCH and polling share the same `OnResize()` pipeline.
- **Uses CPR the right way**: validation (row/col match check), not "row − logicalY".
- **Has a safe fallback** that avoids destructive erases if the model and terminal disagree.
- **No flicker**: all reposition + erase + redraw happen in the existing synchronized output block.
- **Graceful failure mode**: if the prompt origin is off-screen or the model is unreliable, a fresh prompt line is started without corrupting history.

---

## Testing strategy

Three layers, all exercising real implementations per Constitution VIII (no mocks, fakes, doubles, or simulations).

### 1. Pure unit tests for `ComputePhysicalCursorOffsetAfterReflow`

Build real `Screen` objects (using the same `DataBuffer` structure the renderer uses) and assert `dy` across widths:

- Single row, shrink width.
- Multiple rows, different lengths.
- Wide characters (`Width=2`) — verify correct cell accounting.
- Styled trailing spaces (should count toward row width).
- Empty rows (should count as 1 physical line each).
- Right-aligned text (holes in the row — sparse `DataBuffer` with content only at high column indices).

These are standard xUnit tests against the real `ComputePhysicalCursorOffsetAfterReflow` and `GetRowUsedWidth` methods. No mocking required — `Screen` is a concrete class with a public `DataBuffer`.

### 2. Escape sequence output verification (real Renderer + real Vt100Output)

Exercise the full resize code path through the real `Renderer` writing to a real `Vt100Output` backed by a `StringWriter`. This is **not** a simulation — it uses the actual production code writing actual escape sequences to an output stream. This is the same pattern used by Terminal.Gui (real `ConsoleDriver` writing to a captured buffer) and Spectre.Console (real `IAnsiConsole` writing to a `StringWriter`).

**What this tests:**
- `PrepareForResize()` snapshots correctly (screen, cursor, size).
- `RebaseCursorAfterResize()` emits the correct escape sequences (`\r`, `CursorUp(dy)`, or `\r\n` fallback).
- The full `Render()` → `OutputScreenDiff` pipeline produces correct output after resize.
- CPR column confidence check triggers safe fallback when expected.

**Test pattern** (inspired by Terminal.Gui's `TestContext` and Spectre.Console's `TestConsole`):

```csharp
[Fact]
public void Resize_Narrower_EmitsCursorUpToPromptOrigin()
{
    // Arrange: real Vt100Output writing to StringWriter
    var writer = new StringWriter();
    var output = new Vt100Output(writer, () => new Size(80, 24));

    // Create real Renderer with real output
    var renderer = new Renderer(output, fullScreen: false);

    // First render at width 80 — establishes _lastScreen, _cursorPos
    renderer.Render(app, layout, isDone: false);
    writer.GetStringBuilder().Clear();

    // Act: resize to width 40
    output.SetSizeCallback(() => new Size(40, 24));
    renderer.PrepareForResize();
    renderer.Render(app, layout, isDone: false);

    // Assert: captured escape sequences contain CursorUp
    var sequences = writer.ToString();
    Assert.Contains("\x1b[", sequences); // CSI present
    // Verify \r followed by CursorUp(N) where N = expected dy
    Assert.Matches(@"\r\x1b\[\d+A", sequences);
}
```

**Test cases:**
- Narrow: 80→40, multi-line prompt content that wraps. Assert `CursorUp(dy)` where `dy > _cursorPos.Y`.
- Widen: 40→80, rows that no longer wrap. Assert `CursorUp(dy)` where `dy == _cursorPos.Y` (no reflow effect).
- Rapid resize burst: multiple `PrepareForResize()` calls before one `Render()`. Assert snapshot is from the *first* resize (burst coalescing).
- CPR safety: set `_resizeCprCol` to a mismatched value. Assert output contains `\r\n` (safe fallback), not `CursorUp`.
- No old screen: resize before first render. Assert relative fallback using `_cursorPos`.
- Wide characters: row with CJK content at wrap boundary. Verify `dy` computation.

### 3. Real PTY integration tests (gold standard)

Launch Stroke examples in a real PTY using the TUI driver, resize the PTY, and verify actual screen state. This exercises the full stack: real terminal, real escape sequences, real reflow, real cursor positioning. This is the same approach used by fish-shell (pexpect + real PTY) and tui-test (Playwright-style terminal automation).

**Pattern** (using TUI driver MCP tools):

```
1. tui_launch: Start fancy-zsh-prompt example in a real PTY (cols=80, rows=24)
2. tui_wait_for_text: Wait for prompt to appear
3. tui_send_text: Type "hello" + Enter, wait for response
4. tui_send_text: Type "world" + Enter, wait for stacked prompt
5. tui_resize: Narrow to cols=40
6. tui_wait_for_idle: Wait for redraw to complete
7. tui_text: Capture screen content
8. Assert: no duplicated prompt fragments in captured text
9. Assert: prompt content is intact (not garbled)
10. tui_resize: Widen to cols=120
11. tui_wait_for_idle + tui_text: Verify clean redraw
12. tui_resize: Rapid narrow→wide→narrow sequence
13. tui_wait_for_idle + tui_text: Verify no accumulated garble
14. tui_close: Clean up
```

**Test cases:**
- Narrow resize (80→40): long prompt lines wrap, verify no duplication.
- Wide resize (40→120): wrapped content un-wraps visually, verify clean redraw.
- Rapid resize burst: multiple resizes before redraw completes, verify final state is clean.
- Extreme narrow (80→20): heavy wrapping, verify prompt is readable.
- Same width, different height (80×24→80×12): vertical shrink, verify no garble.
- CJK content: prompt with wide characters, resize across wrap boundaries.

### What each layer catches

| Layer | Catches | Approach |
|-------|---------|----------|
| **Tier 1** (unit) | Logic errors in reflow computation | Real `Screen` objects, assert `dy` values |
| **Tier 2** (escape sequence) | Integration errors in Renderer pipeline | Real `Renderer` + real `Vt100Output` → `StringWriter`, assert escape sequences |
| **Tier 3** (PTY) | Real-terminal weirdness | Actual PTY, actual terminal reflow, actual cursor behavior |
