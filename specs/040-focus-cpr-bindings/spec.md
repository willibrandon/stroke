# Feature Specification: Focus & CPR Bindings

**Feature Branch**: `040-focus-cpr-bindings`
**Created**: 2026-01-31
**Status**: Draft
**Input**: User description: "Implement the focus navigation bindings and CPR (Cursor Position Request) response handling bindings, porting focus.py and cpr.py from Python Prompt Toolkit."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Focus Navigation Between Windows (Priority: P1)

A developer building a multi-pane terminal application (e.g., a layout with an editor pane, a file browser, and a status bar) needs users to navigate between focusable windows using Tab and Shift+Tab. When the user presses Tab, focus moves to the next visible focusable window in the layout. When the user presses Shift+Tab, focus moves to the previous visible focusable window.

**Why this priority**: Focus navigation is a fundamental interaction pattern for any multi-window terminal UI. Without it, users cannot move between panes, making multi-pane layouts unusable.

**Independent Test**: Can be fully tested by creating a layout with multiple focusable windows, invoking the focus-next and focus-previous functions, and verifying focus moves to the correct window each time.

**Acceptance Scenarios**:

1. **Given** a layout with three focusable windows (A, B, C) where A has focus, **When** focus-next is invoked, **Then** focus moves to window B.
2. **Given** a layout with three focusable windows (A, B, C) where C has focus, **When** focus-next is invoked, **Then** focus wraps around to window A.
3. **Given** a layout with three focusable windows (A, B, C) where A has focus, **When** focus-previous is invoked, **Then** focus wraps around to window C.
4. **Given** a layout with three focusable windows (A, B, C) where B has focus, **When** focus-previous is invoked, **Then** focus moves to window A.

---

### User Story 2 - CPR Response Handling (Priority: P1)

When Stroke queries the terminal for cursor position (via the DSR/CPR escape sequence), the terminal responds with an escape sequence encoding the cursor's row and column. The system must parse this response and report the absolute cursor row to the renderer so it can correctly position output on screen. This is critical for proper rendering when the terminal height or scroll position is unknown.

**Why this priority**: CPR response handling is essential for the renderer to determine cursor positioning. Without it, the renderer cannot accurately know where output appears on the terminal, leading to garbled display especially at startup or after terminal resizes.

**Independent Test**: Can be fully tested by simulating a CPR response key event with known row/column data and verifying the renderer receives the correct absolute row value.

**Acceptance Scenarios**:

1. **Given** a CPR response event with data encoding row 35 and column 1, **When** the CPR binding handler processes it, **Then** the renderer receives an absolute cursor row report of 35.
2. **Given** a CPR response event with data encoding row 1 and column 80, **When** the CPR binding handler processes it, **Then** the renderer receives an absolute cursor row report of 1.
3. **Given** a CPR response event, **When** the binding is registered, **Then** no undo point is created before handling (save-before is disabled).

---

### User Story 3 - Focus Navigation with Single Window (Priority: P2)

A developer creates an application with only one focusable window. When focus-next or focus-previous is invoked, focus should remain on the current window without errors, since there is nowhere else to navigate.

**Why this priority**: Single-window layouts are common (simple prompts, dialogs). The focus functions must handle this gracefully rather than crashing or producing unexpected behavior.

**Independent Test**: Can be fully tested by creating a layout with a single focusable window, invoking focus-next and focus-previous, and verifying focus stays on the same window.

**Acceptance Scenarios**:

1. **Given** a layout with one focusable window, **When** focus-next is invoked, **Then** focus remains on the same window and no exception is thrown.
2. **Given** a layout with one focusable window, **When** focus-previous is invoked, **Then** focus remains on the same window and no exception is thrown.

---

### User Story 4 - Focus Navigation with No Focusable Windows (Priority: P2)

When all windows in a layout are non-focusable or hidden, invoking focus-next or focus-previous should be a no-op without errors.

**Why this priority**: Edge case that must be handled gracefully to prevent runtime exceptions.

**Independent Test**: Can be fully tested by creating a layout with no visible focusable windows and invoking both focus functions, verifying no exception occurs.

**Acceptance Scenarios**:

1. **Given** a layout with no focusable windows, **When** focus-next is invoked, **Then** no exception occurs and layout state is unchanged.
2. **Given** a layout with no focusable windows, **When** focus-previous is invoked, **Then** no exception occurs and layout state is unchanged.

---

### Edge Cases

- What happens when focus-next/focus-previous is called on a layout with only non-visible windows? Focus functions delegate to `Layout.FocusNext()`/`FocusPrevious()`, which only consider visible, focusable windows and are a no-op if none exist (guarded by `windows.Count > 0`).
- What happens when a CPR response contains malformed data (e.g., missing semicolon, non-numeric values)? The handler follows the same parsing behavior as Python Prompt Toolkit, which trusts the terminal to send well-formed data. Malformed data will result in a parsing exception (undefined behavior), matching the Python source which performs no defensive validation.
- What happens when focus wraps around from the last to the first window (and vice versa)? The navigation cycles seamlessly via modular arithmetic in the layout's focus methods.
- What happens when `KeyPressEvent.Data` is null or empty for a CPR response event? This cannot occur in practice because the input parser only generates `Keys.CPRResponse` events when a valid CPR escape sequence is detected. No defensive guard is added, matching Python behavior.
- What happens when CPR response contains extreme row/column values (e.g., row=0, row=99999)? The parsed value is passed directly to `Renderer.ReportAbsoluteCursorRow()` without bounds checking, matching Python behavior. The renderer handles downstream calculations.
- What happens when focus functions are called before the application is fully initialized? `GetApp()` throws `InvalidOperationException` if `KeyPressEvent.App` is null, which is the standard behavior for all binding handlers and cannot occur during normal key processing.
- What happens when windows change visibility during focus traversal? The layout's focus methods capture the visible focusable window list at invocation time (within a lock scope), so visibility changes during traversal do not affect the current operation.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `FocusNext` handler function (not a binding loader) that advances focus to the next visible focusable window in the layout, delegating to the layout's `FocusNext()` method. A "visible focusable window" is a window returned by the layout's visible-focusable-windows traversal (as defined in Feature 29's Layout system).
- **FR-002**: System MUST provide a `FocusPrevious` handler function (not a binding loader) that moves focus to the previous visible focusable window in the layout, delegating to the layout's `FocusPrevious()` method.
- **FR-003**: Focus navigation MUST wrap around: advancing past the last focusable window returns to the first, and moving before the first returns to the last. This wrapping is implemented by the layout's focus methods via modular arithmetic on the visible focusable window list.
- **FR-004**: System MUST provide a binding loader method (`LoadCprBindings`) that returns a `KeyBindings` instance containing a single binding for `Keys.CPRResponse` events.
- **FR-005**: The CPR response handler MUST parse the row and column values from the CPR escape sequence data. The data format is `\x1b[<row>;<col>R` (ESC `[` row `;` col `R`). Parsing MUST strip the 2-character prefix (`\x1b[`) and 1-character suffix (`R`), then split on `;` to extract row and column as integers.
- **FR-006**: The CPR response handler MUST report the parsed row value to the renderer via `Renderer.ReportAbsoluteCursorRow(row)`. The column value is parsed but not used (matching Python Prompt Toolkit behavior).
- **FR-007**: The CPR response binding MUST be registered with save-before disabled (`saveBefore` callback returns `false` for all events), since CPR responses are terminal state reports, not user actions. This prevents undo point creation before the handler executes.
- **FR-008**: Both focus functions MUST accept a `KeyPressEvent` parameter and access the application's layout through the event's application reference (via `GetApp().Layout`).

### Key Entities

- **FocusFunctions**: Static class containing focus-next and focus-previous methods for focus traversal between visible windows.
- **CprBindings**: Static class containing a binding loader method that produces key bindings for CPR response handling.
- **KeyPressEvent**: Event object providing access to the application context, event data, and key information.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Focus navigation correctly cycles through all visible focusable windows in both forward and backward directions, verified by automated tests covering 3-window layouts as specified in User Story 1 acceptance scenarios (A→B, C→A wrap, A→C wrap, B→A).
- **SC-002**: CPR response data is correctly parsed and the row value is reported to the renderer, verified by automated tests with at least 3 row/column combinations covering typical (35,1), boundary (1,80), and mid-range (100,40) values.
- **SC-003**: Focus functions handle edge cases (zero windows, one window) without throwing exceptions: zero windows results in a no-op with unchanged layout state; one window results in focus remaining on the same window.
- **SC-004**: All 3 public APIs from Python Prompt Toolkit are ported with 100% fidelity: `focus_next` → `FocusNext`, `focus_previous` → `FocusPrevious`, `load_cpr_bindings` → `LoadCprBindings` (adjusted for C# PascalCase naming conventions).
- **SC-005**: Unit test coverage for this feature's new source files (`FocusFunctions.cs`, `CprBindings.cs`) reaches at least 80%, measured per-feature.

## Assumptions

- The layout system already provides `FocusNext()` and `FocusPrevious()` traversal methods (from Feature 29). **Validated**: `Layout.cs:379` and `Layout.cs:362` — both methods exist, are thread-safe (use `_lock.EnterScope()`), and handle zero-window (no-op via `windows.Count > 0` guard) and single-window (wraps to same index) cases internally.
- The renderer already provides `ReportAbsoluteCursorRow(int row)` to receive absolute cursor row reports (from Feature 30). **Validated**: `Renderer.cs:471` — method exists and is thread-safe (uses `_cprLock.EnterScope()`).
- The keys enumeration already includes `Keys.CPRResponse` (from Feature 11). **Validated**: `Keys.cs:794`.
- The key bindings system supports a `saveBefore` parameter (`Func<KeyPressEvent, bool>?`) for controlling undo point creation (from Feature 22). **Validated**: `KeyBindings.cs:81` and `Binding.cs:69`.
- CPR response data from the terminal is well-formed; no defensive parsing is needed (matching Python Prompt Toolkit behavior, which performs `event.data[2:-1].split(";")` without try/catch).

## Dependencies

- Key bindings registry (Feature 22) - For binding registration (`KeyBindings` class) and the `saveBefore` parameter on `KeyBindings.Add<T>()`.
- Key press event (Feature 22) - `KeyPressEvent` object for key press handlers, providing `Data` property and `GetApp()` extension method.
- Layout system (Feature 29) - `Layout.FocusNext()` and `Layout.FocusPrevious()` methods for focus traversal.
- Renderer (Feature 30) - `Renderer.ReportAbsoluteCursorRow(int row)` for CPR response reporting.
- Keys enumeration (Feature 11) - `Keys.CPRResponse` enum constant for CPR response binding registration.
