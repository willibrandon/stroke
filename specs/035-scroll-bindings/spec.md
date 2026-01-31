# Feature Specification: Scroll Bindings

**Feature Branch**: `035-scroll-bindings`
**Created**: 2026-01-30
**Status**: Draft
**Input**: User description: "Implement scroll key bindings for navigating through long multiline buffers, including page up/down, half-page scrolling, and single-line scrolling."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Full Page Scrolling (Priority: P1)

A developer is editing a long multiline buffer (e.g., a configuration file or script) and needs to quickly navigate through large sections of content. They press Page Down to jump forward one full page, or Page Up to jump backward one full page. The viewport scrolls and the cursor repositions to the beginning of the newly visible content.

**Why this priority**: Full page scrolling is the most common scroll operation and the primary way users navigate through long buffers. Without it, users must arrow-key line by line through hundreds of lines.

**Independent Test**: Can be fully tested by loading a buffer with many lines of text, pressing Page Down/Up, and verifying the cursor moves by a full window height and the viewport adjusts accordingly.

**Acceptance Scenarios**:

1. **Given** a buffer with 100 uniform single-row lines and a window height of 20 lines with the cursor on line 1, **When** the user presses Page Down, **Then** the vertical scroll offset is set to the last visible line index (line 20) and the cursor moves to line 20 at the first non-whitespace character.
2. **Given** a buffer with 100 uniform single-row lines and a window height of 20 lines with the cursor on line 40, **When** the user presses Page Up, **Then** the cursor moves to the first visible line (or one line above the current row, whichever is lower) at the first non-whitespace character, and the vertical scroll resets to 0.
3. **Given** a buffer with 100 lines and the cursor already on the last visible line, **When** the user presses Page Down, **Then** the viewport scrolls forward by one page and the cursor repositions at the top of the new view.
4. **Given** a buffer with the cursor at line 5, **When** the user presses Page Up, **Then** the cursor moves up (at least one line) and the viewport scroll offset resets to show the cursor.

---

### User Story 2 - Full Window Forward/Backward Scrolling (Priority: P1)

A Vi mode user navigates through a long buffer using Ctrl-F (scroll forward one full window) and Ctrl-B (scroll backward one full window). These functions calculate scroll distance based on the actual rendered line heights, accounting for wrapped lines that occupy more than one row of screen space.

**Why this priority**: These are fundamental Vi navigation commands and the core scroll engine that half-page scrolling delegates to. They must correctly handle variable line heights from wrapped content.

**Independent Test**: Can be fully tested by creating a buffer with lines of varying lengths (some wrapping), pressing Ctrl-F/Ctrl-B in Vi mode, and verifying the cursor advances or retreats by the correct number of logical lines.

**Acceptance Scenarios**:

1. **Given** a buffer with uniform single-row lines and a window height of 20 rows, **When** the user invokes scroll forward, **Then** the cursor moves down exactly 20 logical lines (since each line occupies 1 rendered row, the accumulated height of 20 lines equals the window height).
2. **Given** a buffer where some lines wrap to 2-3 rows, **When** the user invokes scroll forward, **Then** the cursor moves down the number of logical lines whose accumulated rendered heights (via `GetHeightForLine()`) fill the window height. For example, with a 20-row window, if lines alternate between 1 and 2 rows, the cursor moves down ~13 logical lines (accumulated height: 1+2+1+2+...=20).
3. **Given** a buffer with the cursor near the end, **When** the user invokes scroll forward, **Then** the cursor stops at the last available line without going past the buffer boundary.
4. **Given** a buffer with the cursor near the beginning, **When** the user invokes scroll backward, **Then** the cursor stops at line 0 without going negative.

---

### User Story 3 - Half Page Scrolling (Priority: P2)

A Vi mode user wants finer-grained scrolling than a full page. They press Ctrl-D to scroll down half a page or Ctrl-U to scroll up half a page. The scroll distance is half the window height, providing a middle ground between full page and single line scrolling.

**Why this priority**: Half-page scroll is the second most common Vi scroll navigation after full page. It provides better context retention since half the previous content remains visible.

**Independent Test**: Can be fully tested by loading a buffer, pressing Ctrl-D/Ctrl-U in Vi mode, and verifying the cursor moves by approximately half the window height.

**Acceptance Scenarios**:

1. **Given** a buffer with uniform single-row lines and a window height of 20 rows, **When** the user invokes half-page down, **Then** the cursor moves down exactly 10 logical lines (target scroll height = 20 // 2 = 10; each line occupies 1 rendered row).
2. **Given** a buffer with uniform single-row lines and a window height of 20 rows, **When** the user invokes half-page up, **Then** the cursor moves up exactly 10 logical lines.
3. **Given** a window height of 21 rows (odd number) and uniform single-row lines, **When** the user invokes half-page down, **Then** the cursor moves down exactly 10 logical lines (target scroll height = 21 // 2 = 10, integer division truncates toward zero).

---

### User Story 4 - Single Line Scrolling (Priority: P2)

A Vi mode user wants to scroll the viewport by exactly one line without moving the cursor, unless the cursor would scroll out of view. They press Ctrl-E to scroll the viewport down one line or Ctrl-Y to scroll the viewport up one line. If scrolling would move the cursor out of the visible area, the cursor is adjusted to remain visible.

**Why this priority**: Single-line scroll is essential for precise viewport positioning in Vi mode. It allows users to peek at nearby content without losing their cursor position.

**Independent Test**: Can be fully tested by loading a buffer, pressing Ctrl-E/Ctrl-Y in Vi mode, and verifying the viewport offset changes by one while the cursor adjusts only when necessary.

**Acceptance Scenarios**:

1. **Given** a buffer with 50 lines and the cursor in the middle of the viewport, **When** the user invokes scroll one line down, **Then** the viewport vertical scroll increases by 1 and the cursor position remains unchanged.
2. **Given** a buffer with 50 lines and the cursor at the top scroll offset boundary, **When** the user invokes scroll one line down, **Then** the viewport scrolls down by 1 and the cursor moves down by 1 line to stay visible.
3. **Given** a buffer with vertical scroll at 0, **When** the user invokes scroll one line up, **Then** nothing happens (already at top).
4. **Given** a buffer with vertical scroll > 0 and the cursor near the bottom of the viewport, **When** the user invokes scroll one line up, **Then** the viewport scrolls up by 1 and the cursor moves up if it would fall below the visible area.

---

### User Story 5 - Emacs Page Navigation Bindings (Priority: P3)

An Emacs mode user navigates through long buffers using Ctrl-V (page down) and Escape-V (page up), plus the standard Page Down/Page Up keys. These bindings are only active when in Emacs editing mode.

**Why this priority**: Emacs bindings complete the editing mode parity requirement. The underlying scroll functions are shared with Vi mode, so this is primarily a binding registration task.

**Independent Test**: Can be fully tested by switching to Emacs mode, pressing Ctrl-V and Escape-V, and verifying page scrolling occurs.

**Acceptance Scenarios**:

1. **Given** the editor in Emacs mode, **When** the user presses Ctrl-V, **Then** the page scrolls down (same as Page Down).
2. **Given** the editor in Emacs mode, **When** the user presses Escape then V, **Then** the page scrolls up (same as Page Up).
3. **Given** the editor in Vi mode, **When** the user presses Ctrl-V, **Then** the Emacs page navigation binding does not activate.

---

### User Story 6 - Vi Page Navigation Bindings (Priority: P3)

A Vi mode user has access to all Vi-standard scroll bindings: Ctrl-F (forward page), Ctrl-B (backward page), Ctrl-D (half page down), Ctrl-U (half page up), Ctrl-E (one line down), Ctrl-Y (one line up), plus Page Down/Page Up. These bindings are only active when in Vi editing mode.

**Why this priority**: Vi bindings complete the editing mode parity requirement alongside Emacs bindings.

**Independent Test**: Can be fully tested by switching to Vi mode, pressing each Vi scroll key combination, and verifying the correct scroll function is invoked.

**Acceptance Scenarios**:

1. **Given** the editor in Vi mode, **When** the user presses Ctrl-F, **Then** the window scrolls forward one full page.
2. **Given** the editor in Vi mode, **When** the user presses Ctrl-B, **Then** the window scrolls backward one full page.
3. **Given** the editor in Vi mode, **When** the user presses Ctrl-D, **Then** the window scrolls down half a page.
4. **Given** the editor in Vi mode, **When** the user presses Ctrl-U, **Then** the window scrolls up half a page.
5. **Given** the editor in Vi mode, **When** the user presses Ctrl-E, **Then** the viewport scrolls down one line.
6. **Given** the editor in Vi mode, **When** the user presses Ctrl-Y, **Then** the viewport scrolls up one line.
7. **Given** the editor in Emacs mode, **When** the user presses Ctrl-F, **Then** the Vi page navigation binding does not activate.

---

### User Story 7 - Combined Navigation with Buffer Focus Guard (Priority: P3)

All page navigation bindings (both Vi and Emacs) are only active when a Buffer control is focused. If a different widget (such as a dialog, menu, or terminal pane) is focused, the scroll bindings must not intercept keystrokes.

**Why this priority**: Focus-guarding prevents key conflicts with other widgets that may use the same key combinations for different purposes.

**Independent Test**: Can be fully tested by verifying that the combined bindings are wrapped in a BufferHasFocus conditional filter.

**Acceptance Scenarios**:

1. **Given** a Buffer is focused, **When** the user presses any scroll key, **Then** the scroll action is performed.
2. **Given** a non-Buffer widget is focused, **When** the user presses a scroll key like Ctrl-D, **Then** the scroll binding does not activate and the key is passed through.

---

### Edge Cases

- **EC-001**: What happens when the buffer has fewer lines than the window height? Scroll functions MUST handle this gracefully without error, clamping movement to available lines. The line height accumulation loop in ScrollForward/ScrollBackward will naturally terminate early when `y` reaches the last/first line.
- **EC-002**: What happens when the cursor is already at the first line and scroll backward is invoked? The cursor MUST remain at line 0 (the `max(0, cursorRow - 1)` start position and the accumulation loop naturally produce row 0).
- **EC-003**: What happens when the cursor is already at the last line and scroll forward is invoked? The cursor MUST remain at the last line (the accumulation loop starting at `cursorRow + 1` immediately finds `y >= lineCount` and sets cursor to the boundary).
- **EC-004**: What happens when the window has no render info (not yet rendered)? All scroll functions MUST return immediately without error (null check, no-op). This includes the case where `window` itself is null.
- **EC-005**: What happens with a single-line buffer? All scroll functions that modify cursor position MUST be effectively no-ops (the accumulation loop produces the same row, and single-line scroll boundary checks prevent viewport changes). No errors or exceptions.
- **EC-006**: What happens when line heights vary due to long wrapped lines? Scroll calculations MUST use `GetHeightForLine()` to accumulate actual rendered heights, not logical line counts. A line wrapping to 3 rows consumes 3 rows of the scroll budget.
- **EC-007**: What happens with an empty buffer (zero content)? ScrollForward/ScrollBackward MUST handle `lineCount == 0` or `lineCount == 1` without error. The accumulation loop terminates immediately and `TranslateRowColToIndex(0, 0)` produces position 0.
- **EC-008**: What happens when ScrollPageDown is invoked and the viewport is already showing the last page of content? The `max(lastVisibleLine, verticalScroll + 1)` formula ensures forward progress of at least 1 line. If `lastVisibleLine` is the last line in the buffer, the cursor repositions to that final line.
- **EC-009**: What happens when ScrollPageUp is invoked and VerticalScroll is already 0? The cursor MUST still reposition to `max(0, min(firstVisibleLine, cursorRow - 1))`, which may move the cursor up even when the viewport cannot scroll further. The vertical scroll remains at 0.
- **EC-010**: What happens when a single wrapped line occupies the entire window height (or more)? ScrollForward accumulates that line's height, which meets or exceeds the target, so the cursor advances by exactly one logical line. ScrollBackward behaves symmetrically. This is correct per the Python implementation's accumulation logic.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a scroll forward function that moves the cursor down by the number of logical lines whose accumulated rendered heights (via `GetHeightForLine()`) fill the window height. Starting from `cursorRow + 1`, the function accumulates line heights until adding the next line would meet or exceed the target scroll height, then sets the cursor to column 0 of the resulting row via `TranslateRowColToIndex(targetRow, 0)`. The target scroll height is `WindowHeight` for full scroll, or `WindowHeight // 2` (integer division, floor) for half scroll.
- **FR-002**: System MUST provide a scroll backward function that moves the cursor up by the number of logical lines whose accumulated rendered heights fill the window height. Starting from `max(0, cursorRow - 1)`, the function accumulates line heights downward (decrementing row) until adding the next line would meet or exceed the target scroll height, then sets the cursor to column 0 of the resulting row via `TranslateRowColToIndex(targetRow, 0)`. The target scroll height is `WindowHeight` for full scroll, or `WindowHeight // 2` (integer division, floor) for half scroll.
- **FR-003**: System MUST provide half-page scroll functions (down and up) that delegate to the scroll forward/backward internal implementation with `half=true`, resulting in a target scroll height of `WindowHeight // 2` (integer division, floor). For a window height of 21 rows, the half-page target is 10 rows.
- **FR-004**: System MUST provide single-line scroll functions (down and up) that adjust the viewport vertical scroll offset by exactly one line. The cursor MUST be adjusted only when it would exit the visible area: for scroll-one-line-down, the cursor moves down one line only when `cursorPosition.Y <= configuredScrollOffsets.Top` (cursor is at or above the top scroll offset boundary); for scroll-one-line-up, the cursor moves up by `max(0, cursorPosition.Y - (windowHeight - 1 - firstLineHeight - configuredScrollOffsets.Bottom))` steps (each step via `GetCursorUpPosition()`), where `firstLineHeight` is the rendered height of the first visible line.
- **FR-005**: System MUST provide page down and page up functions that scroll the viewport and reposition the cursor. Page down sets the cursor at the target line using `TranslateRowColToIndex(targetRow, 0)` then adjusts to the first non-whitespace character via `GetStartOfLinePosition(afterWhitespace: true)`. Page up uses the same cursor positioning approach (`TranslateRowColToIndex` then `GetStartOfLinePosition(afterWhitespace: true)`).
- **FR-006**: System MUST provide Emacs page navigation bindings: Ctrl-V (page down), Escape-V (page up), Page Down, Page Up — active only in Emacs mode.
- **FR-007**: System MUST provide Vi page navigation bindings: Ctrl-F (forward), Ctrl-B (backward), Ctrl-D (half down), Ctrl-U (half up), Ctrl-E (one line down), Ctrl-Y (one line up), Page Down, Page Up — active only in Vi mode.
- **FR-008**: System MUST provide a combined page navigation loader that merges Emacs and Vi bindings, guarded by a buffer-has-focus condition.
- **FR-009**: All scroll functions MUST gracefully handle the case where the current window or its render info is null (not yet rendered), returning without error.
- **FR-010**: Scroll forward MUST clamp the cursor to within `[0, UIContent.LineCount - 1]` rows. The accumulation loop condition `y < lineCount` naturally enforces this upper bound.
- **FR-011**: Scroll backward MUST clamp the cursor to within `[0, UIContent.LineCount - 1]` rows. The starting position `max(0, cursorRow - 1)` and the loop condition `y > 0` naturally enforce the lower bound of row 0.
- **FR-012**: Single-line scroll down MUST check that `VerticalScroll < ContentHeight - WindowHeight` before adjusting the offset. If this condition is false, the function MUST be a no-op.
- **FR-013**: Single-line scroll up MUST check that `VerticalScroll > 0` before decrementing. If this condition is false, the function MUST be a no-op.
- **FR-014**: Page down MUST set the vertical scroll offset to `max(lastVisibleLine, verticalScroll + 1)` and set the cursor to that line index at column 0 via `TranslateRowColToIndex`, then adjust to the first non-whitespace character via `GetStartOfLinePosition(afterWhitespace: true)`. The `max` ensures forward progress even when `lastVisibleLine` equals the current scroll position.
- **FR-015**: Page up MUST position the cursor at row `max(0, min(firstVisibleLine, cursorRow - 1))` at column 0 via `TranslateRowColToIndex`, then adjust to the first non-whitespace character via `GetStartOfLinePosition(afterWhitespace: true)`. The `min(firstVisibleLine, cursorRow - 1)` ensures at least one line of upward movement when the cursor is below the first visible line. After cursor repositioning, the vertical scroll offset MUST be reset to 0 (the Window's own scroll logic will adjust to ensure the cursor is visible).

### Key Entities

- **ScrollFunctions**: A collection of 8 static scroll functions (forward, backward, half-page down/up, one-line down/up, page down/up) that operate on a KeyPressEvent to navigate the buffer. All scroll functions accept a `KeyPressEvent` parameter and return `NotImplementedOrNone?` (returning `null` on success), matching the `KeyHandlerCallable` delegate signature.
- **PageNavigationBindings**: A collection of 3 static binding loaders (Emacs, Vi, combined) that register scroll functions to mode-specific key combinations with appropriate filter conditions.
- **Window RenderInfo**: Provides the rendering context needed by scroll functions — window height, content height, line heights, cursor position, scroll offsets, visible line ranges.

### Function Mapping (Python → C#)

All 8 Python `scroll.py` functions are mapped 1:1 to C# equivalents:

| Python Function | C# Method | Category |
|----------------|-----------|----------|
| `scroll_forward(event, half=False)` | `ScrollForward(KeyPressEvent)` | Cursor-only (no viewport change) |
| `scroll_backward(event, half=False)` | `ScrollBackward(KeyPressEvent)` | Cursor-only (no viewport change) |
| `scroll_half_page_down(event)` | `ScrollHalfPageDown(KeyPressEvent)` | Cursor-only (delegates to ScrollForward with half=true) |
| `scroll_half_page_up(event)` | `ScrollHalfPageUp(KeyPressEvent)` | Cursor-only (delegates to ScrollBackward with half=true) |
| `scroll_one_line_down(event)` | `ScrollOneLineDown(KeyPressEvent)` | Viewport primary (cursor adjusts conditionally) |
| `scroll_one_line_up(event)` | `ScrollOneLineUp(KeyPressEvent)` | Viewport primary (cursor adjusts conditionally) |
| `scroll_page_down(event)` | `ScrollPageDown(KeyPressEvent)` | Both viewport and cursor |
| `scroll_page_up(event)` | `ScrollPageUp(KeyPressEvent)` | Both viewport and cursor |

**Delegation pattern**: The Python `scroll_forward`/`scroll_backward` functions accept a `half` boolean parameter. In C#, `ScrollHalfPageDown` and `ScrollHalfPageUp` delegate to a shared internal method (`ScrollForwardInternal`/`ScrollBackwardInternal`) with `half=true`, while `ScrollForward` and `ScrollBackward` call it with `half=false`. This avoids exposing the `half` parameter in the public `KeyHandlerCallable`-compatible signature.

**Structural distinction**: `ScrollForward`/`ScrollBackward` (and their half-page variants) modify only `Buffer.CursorPosition` — they do not change `Window.VerticalScroll`. In contrast, `ScrollOneLineDown`/`ScrollOneLineUp` primarily modify `Window.VerticalScroll` and only adjust the cursor conditionally. `ScrollPageDown`/`ScrollPageUp` modify both.

All 3 Python `page_navigation.py` loader functions are mapped 1:1:

| Python Function | C# Method |
|----------------|-----------|
| `load_page_navigation_bindings()` | `LoadPageNavigationBindings()` |
| `load_emacs_page_navigation_bindings()` | `LoadEmacsPageNavigationBindings()` |
| `load_vi_page_navigation_bindings()` | `LoadViPageNavigationBindings()` |

### Key Binding Mapping Tables

**Emacs mode bindings** (from `LoadEmacsPageNavigationBindings`, filtered by `EmacsMode`):

| Key | Scroll Function |
|-----|----------------|
| Ctrl-V | ScrollPageDown |
| PageDown | ScrollPageDown |
| Escape, V (two-key sequence, not a chord) | ScrollPageUp |
| PageUp | ScrollPageUp |

**Vi mode bindings** (from `LoadViPageNavigationBindings`, filtered by `ViMode`):

| Key | Scroll Function |
|-----|----------------|
| Ctrl-F | ScrollForward |
| Ctrl-B | ScrollBackward |
| Ctrl-D | ScrollHalfPageDown |
| Ctrl-U | ScrollHalfPageUp |
| Ctrl-E | ScrollOneLineDown |
| Ctrl-Y | ScrollOneLineUp |
| PageDown | ScrollPageDown |
| PageUp | ScrollPageUp |

Both Vi and Emacs modes share the same `ScrollPageDown`/`ScrollPageUp` functions for their PageDown/PageUp bindings.

### State Modification Matrix

Each scroll function modifies one or both of `Buffer.CursorPosition` and `Window.VerticalScroll`. The table below specifies which state each function modifies, the positioning mode (absolute set vs relative delta), and the order of operations.

| Function | CursorPosition | VerticalScroll | Operation Order |
|----------|---------------|----------------|----------------|
| ScrollForward | Yes — absolute set via `TranslateRowColToIndex` | No | cursor only |
| ScrollBackward | Yes — absolute set via `TranslateRowColToIndex` | No | cursor only |
| ScrollHalfPageDown | Yes — absolute set (delegates to ScrollForward) | No | cursor only |
| ScrollHalfPageUp | Yes — absolute set (delegates to ScrollBackward) | No | cursor only |
| ScrollOneLineDown | Conditionally — relative delta via `GetCursorDownPosition` | Yes — increment by 1 | 1. adjust cursor (if needed), 2. increment scroll |
| ScrollOneLineUp | Conditionally — relative delta via `GetCursorUpPosition` (repeated) | Yes — decrement by 1 | 1. adjust cursor (if needed), 2. decrement scroll |
| ScrollPageDown | Yes — absolute set via `TranslateRowColToIndex`, then relative delta via `GetStartOfLinePosition` | Yes — absolute set to `max(lastVisibleLine, verticalScroll + 1)` | 1. set scroll, 2. set cursor absolute, 3. adjust cursor relative |
| ScrollPageUp | Yes — absolute set via `TranslateRowColToIndex`, then relative delta via `GetStartOfLinePosition` | Yes — absolute set to 0 | 1. set cursor absolute, 2. adjust cursor relative, 3. set scroll to 0 |

**Absolute vs relative**: Absolute positioning uses `Buffer.CursorPosition = index` (complete replacement). Relative positioning uses `Buffer.CursorPosition += delta` (additive adjustment). Some functions use both in sequence (absolute set followed by relative adjustment for whitespace skipping).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 8 scroll functions faithfully replicate the behavior defined in FR-001 through FR-005, FR-009 through FR-015, and the State Modification Matrix. For uniform single-row lines, scroll forward/backward move the cursor by exactly `WindowHeight` (or `WindowHeight // 2` for half-page) logical lines. For variable-height lines, the cursor position matches the line height accumulation algorithm specified in FR-001/FR-002.
- **SC-002**: All 3 binding loaders register exactly the key-to-function mappings specified in the Key Binding Mapping Tables, with the correct mode filters (EmacsMode, ViMode) and the BufferHasFocus guard on the combined loader.
- **SC-003**: Scroll functions correctly handle variable line heights. Given a buffer with known line heights and a known window height, the resulting cursor position after a scroll operation is deterministic and matches the accumulation algorithm in FR-001/FR-002.
- **SC-004**: All scroll bindings are mode-conditional — Vi bindings only fire in Vi mode, Emacs bindings only fire in Emacs mode. This is enforced by `ConditionalKeyBindings` with the appropriate mode filter.
- **SC-005**: Combined navigation bindings are guarded by buffer-has-focus (`AppFilters.BufferHasFocus`), preventing key conflicts with other focused widgets.
- **SC-006**: Unit test coverage reaches at least 80% for all new code, measured by `dotnet test` with coverage collection enabled (e.g., `--collect:"XPlat Code Coverage"`).
- **SC-007**: All edge cases defined in EC-001 through EC-010 are handled without exceptions.

### Non-Functional Requirements

- **NFR-001**: All scroll functions MUST be synchronous, single-pass operations with O(n) time complexity where n is the number of lines in one page of content. No heap allocations beyond the initial method call frame.
- **NFR-002**: Both `ScrollBindings` and `PageNavigationBindings` MUST be stateless static classes with no instance fields or static mutable state. This makes them inherently thread-safe with no locking required (per Constitution XI).
- **NFR-003**: Source files MUST remain under 1,000 lines of code per file (per Constitution X). `ScrollBindings.cs` is estimated at ~180 LOC and `PageNavigationBindings.cs` at ~70 LOC.

## Assumptions

### Layout & Rendering Dependencies (Feature 029)

- `Window` (namespace: `Stroke.Layout.Containers`): Provides `VerticalScroll` (mutable `int`, get/set, thread-safe via Lock) and `RenderInfo` (nullable `WindowRenderInfo?`).
- `WindowRenderInfo` (namespace: `Stroke.Layout.Windows`): Provides the following properties and methods used by scroll functions:
  - `WindowHeight` (int) — rendered height of the window in rows
  - `ContentHeight` (int) — total content height in rows
  - `CursorPosition` (Point) — cursor screen position with `.Y` for row
  - `ConfiguredScrollOffsets` (ScrollOffsets) — with `.Top` and `.Bottom` margin values
  - `UIContent.LineCount` (int) — total number of logical lines
  - `GetHeightForLine(int lineNo)` → int — rendered height of a specific logical line
  - `FirstVisibleLine(bool afterScrollOffset = false)` → int — first visible logical line index
  - `LastVisibleLine(bool beforeScrollOffset = false)` → int — last visible logical line index

### Buffer & Document Dependencies (Features 002, 007)

- `Buffer.CursorPosition` (int, get/set, thread-safe via Lock) — text index position of cursor
- `Buffer.Document` (Document) — the immutable document model, providing:
  - `CursorPositionRow` (int) — zero-based row of cursor position
  - `TranslateRowColToIndex(int row, int col)` → int — converts (row, col) to text index
  - `GetCursorDownPosition(int count = 1)` → int — relative delta for moving cursor down
  - `GetCursorUpPosition(int count = 1)` → int — relative delta for moving cursor up
  - `GetStartOfLinePosition(bool afterWhitespace = false)` → int — relative delta to start of line

### KeyBinding Infrastructure (Feature 022)

- `KeyBindings` — mutable registry for key-to-handler mappings
- `ConditionalKeyBindings(IKeyBindingsBase keyBindings, IFilter filter)` — wraps bindings with a filter condition
- `MergedKeyBindings(params IKeyBindingsBase[] registries)` — merges multiple binding registries; bindings from all registries are flattened in order

### Filter Dependencies (Feature 032)

- `AppFilters.BufferHasFocus` — returns true when any `BufferControl` currently has focus in the layout (not specific to a particular buffer instance).
- `ViFilters.ViMode` — returns true when `Application.EditingMode == EditingMode.Vi`.
- `EmacsFilters.EmacsMode` — returns true when `Application.EditingMode == EditingMode.Emacs`.
- **Mutual exclusivity**: `ViMode` and `EmacsMode` are mutually exclusive by design — `EditingMode` is a single enum value, so exactly one mode is active at any time. This means overlapping keys (PageDown/PageUp in both Vi and Emacs bindings) will only match bindings for the active mode.
- **Merge order**: `LoadPageNavigationBindings` merges Emacs bindings first, then Vi bindings. However, since the mode filters are mutually exclusive, merge order has no observable effect on behavior — only bindings for the active mode will ever match.

### General

- `Window.VerticalScroll` is directly readable and writable by scroll functions without intermediate API.
- Scroll bindings do not affect mode state — they only consume the current mode via filters. A mode switch between scroll operations naturally deactivates one set of bindings and activates the other; no special handling is required.
