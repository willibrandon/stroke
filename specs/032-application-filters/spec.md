# Feature Specification: Application Filters

**Feature Branch**: `032-application-filters`
**Created**: 2026-01-30
**Status**: Draft
**Input**: User description: "Feature 121: Application Filters - Implement application-specific filters that query runtime application state, building on the core filter infrastructure and requiring access to the Application, ViState, and other runtime components."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Application State Filters for Key Bindings (Priority: P1)

As a framework developer building key binding configurations, I need filters that query the current application state (e.g., whether the buffer has a selection, completions are available, or the buffer is read-only) so that key bindings activate only when the relevant application conditions are met.

**Why this priority**: Application state filters are the foundation for conditional key bindings. Without them, all key bindings fire unconditionally, making editing modes and context-sensitive behavior impossible.

**Independent Test**: Can be fully tested by creating an application with a known buffer state (e.g., selection active, completions present) and verifying that each filter returns the correct boolean result. Delivers conditional key binding activation based on runtime state.

**Acceptance Scenarios**:

1. **Given** an application with a buffer that has a selection, **When** the `HasSelection` filter is evaluated, **Then** it returns true.
2. **Given** an application with a buffer that has no selection, **When** the `HasSelection` filter is evaluated, **Then** it returns false.
3. **Given** no active application context, **When** any application state filter is evaluated, **Then** it returns false gracefully without throwing an exception.
4. **Given** an application with completions showing and one selected, **When** `CompletionIsSelected` is evaluated, **Then** it returns true.
5. **Given** an application with active completions showing, **When** `HasCompletions` is evaluated, **Then** it returns true.
6. **Given** a buffer marked as read-only, **When** `IsReadOnly` is evaluated, **Then** it returns true.
7. **Given** a buffer with a validation error, **When** `HasValidationError` is evaluated, **Then** it returns true.
8. **Given** an application where the key processor has a numeric arg, **When** `HasArg` is evaluated, **Then** it returns true.
9. **Given** an application that is done (returning/aborting), **When** `IsDone` is evaluated, **Then** it returns true.
10. **Given** a renderer with known height, **When** `RendererHeightIsKnown` is evaluated, **Then** it returns true.
11. **Given** a multiline buffer, **When** `IsMultiline` is evaluated, **Then** it returns true.
12. **Given** an application with paste mode enabled, **When** `InPasteMode` is evaluated, **Then** it returns true.
13. **Given** a buffer with a non-empty suggestion, **When** `HasSuggestion` is evaluated, **Then** it returns true.
14. **Given** a buffer with an empty suggestion, **When** `HasSuggestion` is evaluated, **Then** it returns false.
15. **Given** two application filters composed with `&` (AND), `|` (OR), or `~` (NOT), **When** the composed filter is evaluated, **Then** it produces the expected boolean result matching the operator semantics.

---

### User Story 2 - Focus Filters for Layout Management (Priority: P1)

As a framework developer building complex layouts with multiple panes, I need filters that check whether a specific buffer, control, or container currently has focus so that UI components respond to focus changes (e.g., showing/hiding toolbars, enabling context menus).

**Why this priority**: Focus filters are critical for multi-pane layouts where different key bindings and UI behaviors depend on which element is active. They enable the `has_focus` pattern from Python Prompt Toolkit.

**Independent Test**: Can be fully tested by creating a layout with multiple buffers/controls, setting focus to each one in turn, and verifying the corresponding `HasFocus` filter returns true while others return false.

**Acceptance Scenarios**:

1. **Given** a layout with buffers named "default" and "search", and the "default" buffer has focus, **When** `HasFocus("default")` is evaluated, **Then** it returns true.
2. **Given** the same layout, **When** `HasFocus("search")` is evaluated, **Then** it returns false.
3. **Given** a specific `Buffer` instance with focus, **When** `HasFocus(buffer)` is evaluated with that instance, **Then** it returns true.
4. **Given** a specific `UIControl` with focus, **When** `HasFocus(control)` is evaluated, **Then** it returns true.
5. **Given** a container that contains the currently focused window, **When** `HasFocus(container)` is evaluated, **Then** it returns true.
6. **Given** a `BufferControl` with focus, **When** `BufferHasFocus` is evaluated, **Then** it returns true.
7. **Given** a non-`BufferControl` with focus (e.g., `FormattedTextControl`), **When** `BufferHasFocus` is evaluated, **Then** it returns false.
8. **Given** `HasFocus` called twice with the same buffer name, **When** both calls return, **Then** they return distinct filter instances (no memoization for string overloads to avoid memory leaks from retaining application references).

---

### User Story 3 - Vi Mode Filters for Vi Key Bindings (Priority: P1)

As a framework developer implementing Vi editing mode, I need filters that detect the current Vi sub-mode (navigation, insert, replace, visual, digraph, macro recording, text object waiting) so that Vi key bindings activate only in their designated modes.

**Why this priority**: Vi mode has multiple sub-modes with overlapping key bindings. Without mode-aware filters, keys like `d`, `i`, `w` cannot distinguish between navigation commands, insert text, and operator-pending states.

**Independent Test**: Can be fully tested by setting the application to Vi editing mode, cycling through each Vi sub-mode, and verifying the corresponding filter returns true while all others return false.

**Acceptance Scenarios**:

1. **Given** an application in Vi editing mode, **When** `ViMode` is evaluated, **Then** it returns true.
2. **Given** an application in Vi editing mode with navigation input mode, **When** `ViNavigationMode` is evaluated, **Then** it returns true.
3. **Given** an application in Vi editing mode with insert input mode, **When** `ViInsertMode` is evaluated, **Then** it returns true.
4. **Given** an application in Vi editing mode with insert-multiple input mode, **When** `ViInsertMultipleMode` is evaluated, **Then** it returns true.
5. **Given** an application in Vi editing mode with replace input mode, **When** `ViReplaceMode` is evaluated, **Then** it returns true.
6. **Given** an application in Vi editing mode with replace-single input mode, **When** `ViReplaceSingleMode` is evaluated, **Then** it returns true.
7. **Given** an application in Vi editing mode with a selection, **When** `ViSelectionMode` is evaluated, **Then** it returns true.
8. **Given** Vi mode with a pending operator (e.g., `d` pressed, waiting for motion), **When** `ViWaitingForTextObjectMode` is evaluated, **Then** it returns true.
9. **Given** Vi mode with digraph input active, **When** `ViDigraphMode` is evaluated, **Then** it returns true.
10. **Given** Vi mode recording a macro, **When** `ViRecordingMacro` is evaluated, **Then** it returns true.
11. **Given** an application NOT in Vi editing mode, **When** any Vi filter is evaluated, **Then** it returns false.
12. **Given** Vi navigation mode but with a pending operator, **When** `ViNavigationMode` is evaluated, **Then** it returns false (operator-pending overrides navigation).
13. **Given** Vi navigation mode but with digraph wait active, **When** `ViNavigationMode` is evaluated, **Then** it returns false (digraph wait overrides navigation).
14. **Given** Vi navigation mode but with a selection active, **When** `ViNavigationMode` is evaluated, **Then** it returns false (selection overrides navigation).
15. **Given** Vi insert mode but with temporary navigation mode active, **When** `ViInsertMode` is evaluated, **Then** it returns false.
16. **Given** Vi insert mode but the buffer is read-only, **When** `ViInsertMode` is evaluated, **Then** it returns false (read-only forces navigation behavior).
17. **Given** Vi editing mode with a read-only buffer (not in navigation input mode), **When** `ViNavigationMode` is evaluated, **Then** it returns true (read-only forces navigation behavior).
18. **Given** Vi search direction is reversed, **When** `ViSearchDirectionReversed` is evaluated, **Then** it returns true.

---

### User Story 4 - Emacs Mode Filters for Emacs Key Bindings (Priority: P2)

As a framework developer implementing Emacs editing mode, I need filters that detect Emacs mode, Emacs insert mode, and Emacs selection mode so that Emacs key bindings activate correctly.

**Why this priority**: Emacs mode has fewer sub-modes than Vi but still requires mode-aware binding. This is secondary to Vi because Emacs has a simpler mode model.

**Independent Test**: Can be fully tested by setting the application to Emacs editing mode, toggling selection state, and verifying each Emacs filter returns the correct value.

**Acceptance Scenarios**:

1. **Given** an application in Emacs editing mode, **When** `EmacsMode` is evaluated, **Then** it returns true.
2. **Given** Emacs mode with no selection and a writable buffer, **When** `EmacsInsertMode` is evaluated, **Then** it returns true.
3. **Given** Emacs mode with an active selection, **When** `EmacsSelectionMode` is evaluated, **Then** it returns true.
4. **Given** Emacs mode with a selection active, **When** `EmacsInsertMode` is evaluated, **Then** it returns false (selection mode overrides insert).
5. **Given** Emacs mode with a read-only buffer, **When** `EmacsInsertMode` is evaluated, **Then** it returns false.
6. **Given** an application NOT in Emacs editing mode, **When** any Emacs filter is evaluated, **Then** it returns false.
7. **Given** no active application context (DummyApplication), **When** `EmacsMode` is evaluated, **Then** it returns true (DummyApplication defaults to Emacs editing mode).

---

### User Story 5 - Search Filters for Search UI (Priority: P2)

As a framework developer building search functionality, I need filters that detect whether the application is currently searching, whether the focused control supports search, and whether shift-selection mode is active so that search-related key bindings and UI elements activate appropriately.

**Why this priority**: Search is important but built on top of the core editing experience. These filters enable search bar visibility and search-specific key bindings.

**Independent Test**: Can be fully tested by activating search mode in an application with a searchable control and verifying each search filter returns the correct value.

**Acceptance Scenarios**:

1. **Given** an application in search mode, **When** `IsSearching` is evaluated, **Then** it returns true.
2. **Given** a `BufferControl` with a search buffer control assigned, **When** `ControlIsSearchable` is evaluated, **Then** it returns true.
3. **Given** a `BufferControl` without a search buffer control, **When** `ControlIsSearchable` is evaluated, **Then** it returns false.
4. **Given** a buffer with a shift-mode selection, **When** `ShiftSelectionMode` is evaluated, **Then** it returns true.
5. **Given** a buffer with a non-shift selection (e.g., Vi visual mode), **When** `ShiftSelectionMode` is evaluated, **Then** it returns false.
6. **Given** a non-`BufferControl` with focus (e.g., `FormattedTextControl`), **When** `ControlIsSearchable` is evaluated, **Then** it returns false.

---

### User Story 6 - Editing Mode Factory Filter (Priority: P2)

As a framework developer, I need a factory method that creates filters for checking the active editing mode so that I can build key binding configurations that respond to mode switches between Vi and Emacs.

**Why this priority**: The `InEditingMode` factory enables dynamic editing mode detection. It complements the specific Vi/Emacs filters by providing a general-purpose mode check with caching.

**Independent Test**: Can be fully tested by creating filters for Vi and Emacs editing modes and verifying they return correct values when the application's editing mode changes.

**Acceptance Scenarios**:

1. **Given** an application in Vi editing mode, **When** `InEditingMode(EditingMode.Vi)` is evaluated, **Then** it returns true.
2. **Given** an application in Vi editing mode, **When** `InEditingMode(EditingMode.Emacs)` is evaluated, **Then** it returns false.
3. **Given** `InEditingMode` called twice with the same `EditingMode` value, **When** both calls return, **Then** both return the same cached filter instance.

---

### Edge Cases

- What happens when no application is active (null application context)? All filters return false without throwing exceptions, except `EmacsFilters.EmacsMode` and `EmacsFilters.EmacsInsertMode` which return true because DummyApplication defaults to Emacs editing mode with no selection and a writable buffer.
- What happens when `HasFocus` is called with the same buffer name many times? Each call creates a new filter instance (no memoization for string overloads, matching Python Prompt Toolkit's explicit decision to avoid memory leaks from cached references to user controls).
- What happens when a Vi filter is evaluated but the application is in Emacs mode? All Vi filters return false.
- What happens when `HasCompletions` is checked but the completion state has zero completions? It returns false (checks that completions exist and the count is greater than zero).
- What happens when `HasSuggestion` is checked with a suggestion that has empty text? It returns false (checks that the suggestion text is non-empty).
- What happens when Vi insert mode filter is evaluated with a read-only buffer? Returns false (read-only buffers force navigation-like behavior in Vi).
- What happens when `InEditingMode` is called with the same mode from multiple threads? Memoization is thread-safe and returns the same instance.
- What happens when Vi navigation mode is checked while temporary navigation mode is active? Returns true (temporary navigation counts as navigation).
- What happens when `HasFocus(container)` is evaluated with a container that has nested sub-containers? It walks all descendant windows and returns true if any is the current focused window.
- What happens when `ViSearchDirectionReversed` is evaluated with no application or default state? Returns false because DummyApplication's `ReverseViSearchDirection` is `Never.Instance`.
- What happens when `BufferHasFocus` is evaluated with an empty layout or when a non-`BufferControl` has focus? Returns false because the current control is not a `BufferControl`.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an `AppFilters` static class containing filters that query the current application's buffer state: `HasSelection`, `HasSuggestion`, `HasCompletions`, `CompletionIsSelected`, `IsReadOnly`, `IsMultiline`, `HasValidationError`, `HasArg`, `IsDone`, `RendererHeightIsKnown`, `InPasteMode`
- **FR-002**: System MUST provide focus filters via `HasFocus` overloads accepting a buffer name (string), a Buffer instance, a UIControl instance, or a Container instance
- **FR-003**: System MUST provide a `BufferHasFocus` filter that returns true when the currently focused control is a `BufferControl`
- **FR-004**: System MUST provide an `InEditingMode` factory method that returns a cached filter for a given editing mode value
- **FR-005**: System MUST provide a `ViFilters` static class with filters for all Vi sub-modes: `ViMode`, `ViNavigationMode`, `ViInsertMode`, `ViInsertMultipleMode`, `ViReplaceMode`, `ViReplaceSingleMode`, `ViSelectionMode`, `ViWaitingForTextObjectMode`, `ViDigraphMode`, `ViRecordingMacro`, `ViSearchDirectionReversed`
- **FR-006**: System MUST provide an `EmacsFilters` static class with filters: `EmacsMode`, `EmacsInsertMode`, `EmacsSelectionMode`
- **FR-007**: System MUST provide a `SearchFilters` static class with filters: `IsSearching`, `ControlIsSearchable`, `ShiftSelectionMode`
- **FR-008**: All Vi input-mode filters (`ViNavigationMode`, `ViInsertMode`, `ViInsertMultipleMode`, `ViReplaceMode`, `ViReplaceSingleMode`) MUST return false when a pending operator, digraph wait, or active selection is present; additionally `ViInsertMode`, `ViInsertMultipleMode`, `ViReplaceMode`, `ViReplaceSingleMode` MUST return false when temporary navigation mode or read-only buffer is active. `ViNavigationMode` MUST return true when in Vi mode and the input mode is Navigation, OR when temporary navigation mode is active, OR when the buffer is read-only (read-only forces navigation behavior)
- **FR-009**: All filters MUST return false gracefully when no active application context exists
- **FR-010**: `HasFocus` for container types MUST walk the container's child windows and return true if any child window is the currently focused window
- **FR-011**: All filter instances MUST be composable using the existing filter infrastructure's boolean operators (`&`, `|`, `~`)
- **FR-012**: The `InEditingMode` factory MUST use memoization to return the same filter instance for the same `EditingMode` input
- **FR-013**: `HasFocus` with any argument type MUST NOT be globally memoized (to avoid retaining references to disposed application/control instances)
- **FR-014**: All filters MUST faithfully port the logic from Python Prompt Toolkit's `prompt_toolkit/filters/app.py` with no invented behavior

### Key Entities

- **Application Filter**: A boolean condition that queries runtime application state (buffer, layout, editing mode, renderer) and returns true/false for use in conditional key bindings and UI visibility
- **Vi State**: Runtime state tracking Vi editing sub-mode (navigation, insert, replace, visual), pending operators, digraph input, macro recording, and temporary navigation mode
- **Emacs State**: Runtime state tracking Emacs editing mode and macro recording
- **Focus Target**: Any focusable element in the layout hierarchy (buffer name, Buffer instance, UIControl, Container, Window)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every filter defined in Python Prompt Toolkit's `filters/app.py` has a corresponding filter in Stroke with matching boolean semantics across all tested scenarios
- **SC-002**: All filters return false without exceptions when evaluated outside an active application context, except `EmacsFilters.EmacsMode` and `EmacsFilters.EmacsInsertMode` which return true because the DummyApplication defaults to Emacs editing mode with no selection and a writable buffer
- **SC-003**: Filter composition works correctly - combining application filters with `&`, `|`, `~` operators produces expected boolean results in all tested combinations
- **SC-004**: Unit test coverage for all filter classes reaches 80% or higher
- **SC-005**: `InEditingMode` memoization returns identical instances when called with the same input, verified by reference equality
- **SC-006**: All Vi mode guard conditions (operator pending, digraph wait, selection, temporary navigation, read-only) correctly suppress mode filters as defined in Python Prompt Toolkit's logic
