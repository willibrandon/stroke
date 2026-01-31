# Feature Specification: Mouse Bindings

**Feature Branch**: `036-mouse-bindings`
**Created**: 2026-01-30
**Status**: Draft
**Input**: User description: "Implement the mouse event handling bindings that process VT100 and Windows mouse events, including click, drag, and scroll events with modifier key support."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - VT100 Mouse Click Handling (Priority: P1)

A developer builds a terminal application with clickable UI elements (buttons, menus, text regions). When a user clicks within the terminal, the VT100 mouse protocol encodes the click as an escape sequence. The mouse bindings system parses this sequence, identifies the button pressed, the coordinates, and any modifier keys held, then dispatches the event to the appropriate mouse handler registered for that screen position.

**Why this priority**: Mouse click handling is the foundational interaction model. Without click detection, no mouse-driven UI element can function. This covers the most common mouse protocol format (XTerm SGR) used by modern terminals.

**Independent Test**: Can be fully tested by feeding raw XTerm SGR escape sequences into the key binding system and verifying the correct mouse event (button, position, modifiers) is dispatched to the handler at the expected coordinates.

**Acceptance Scenarios**:

1. **Given** a terminal application with mouse support enabled, **When** the user left-clicks at column 10, row 5 and the terminal sends an XTerm SGR sequence `ESC[<0;10;5M`, **Then** the system parses it as a left mouse-down event at position (9, 4) with no modifiers and invokes the registered mouse handler for that position.
2. **Given** a terminal application with mouse support enabled, **When** the user right-clicks while holding Shift and the terminal sends `ESC[<6;20;10M`, **Then** the system parses it as a right mouse-down with Shift modifier and dispatches accordingly.
3. **Given** a terminal application with mouse support enabled, **When** the user releases the left mouse button and the terminal sends `ESC[<0;10;5m` (lowercase 'm'), **Then** the system parses it as a left mouse-up event.

*Note: These scenarios are representative. Full modifier combination coverage (all 8 combos) is validated by the lookup table tests (SC-001), not repeated per acceptance scenario.*

---

### User Story 2 - Mouse Drag and Scroll Events (Priority: P2)

A developer implements a scrollable list or a draggable region in their terminal application. When the user holds a mouse button and moves the cursor, drag (mouse-move) events are generated. When the user scrolls the mouse wheel, scroll events are generated. The mouse bindings system correctly identifies these event types and dispatches them with proper button and modifier information.

**Why this priority**: Drag and scroll are the second most common mouse interactions after clicks. Scrollable content and draggable elements are core to interactive terminal UIs.

**Independent Test**: Can be fully tested by feeding drag escape sequences (button code + 32 offset) and scroll sequences (button codes 64/65) into the binding system and verifying correct event type, button, and modifier dispatch.

**Acceptance Scenarios**:

1. **Given** a terminal application with mouse support enabled, **When** the user holds the left button and moves the mouse, and the terminal sends `ESC[<32;15;8M`, **Then** the system parses it as a left-button mouse-move event at position (14, 7) with no modifiers.
2. **Given** a terminal application with mouse support enabled, **When** the user scrolls the mouse wheel up and the terminal sends `ESC[<64;10;5M`, **Then** the system parses it as a scroll-up event at position (9, 4) with no modifiers.
3. **Given** a terminal application with mouse support enabled, **When** the user scrolls down while holding Control and the terminal sends `ESC[<81;10;5M`, **Then** the system parses it as a scroll-down event with Control modifier.

*Note: These scenarios are representative. All 4 drag source buttons (Left, Middle, Right, None) and all scroll direction + modifier combinations are validated by the lookup table tests (SC-001), not repeated per acceptance scenario.*

---

### User Story 3 - Legacy Mouse Protocol Support (Priority: P3)

A developer's terminal application runs across a variety of terminal emulators, some of which use older mouse protocols. The Typical (X10) format encodes coordinates as single bytes, and the URXVT format uses decimal numbers without the SGR `<` prefix. The mouse bindings system correctly parses both legacy formats and dispatches events, ensuring broad terminal compatibility.

**Why this priority**: While modern terminals predominantly use XTerm SGR, supporting legacy protocols ensures the framework works in older terminal emulators (rxvt-unicode, older xterm configurations) and edge-case environments.

**Independent Test**: Can be fully tested by feeding Typical-format escape sequences (`ESC[M` + 3 raw bytes) and URXVT-format sequences (`ESC[code;x;yM` without `<`) and verifying correct parsing and dispatch.

**Acceptance Scenarios**:

1. **Given** a terminal emulator using the Typical (X10) mouse protocol, **When** the user left-clicks at column 10, row 5 and the terminal sends `ESC[M` followed by bytes (32, 42, 37), **Then** the system parses it as a left mouse-down event at position (9, 4) with unknown modifiers.
2. **Given** a terminal emulator using the URXVT protocol, **When** the user scrolls up and the terminal sends `ESC[96;14;13M`, **Then** the system parses it as a scroll-up event at position (13, 12) with unknown modifiers.
3. **Given** a terminal emulator using the Typical protocol, **When** coordinates contain surrogate escape values (>= 0xDC00), **Then** the system correctly subtracts 0xDC00 before applying the standard offset, producing valid coordinates.

---

### User Story 4 - Scroll Events Without Position (Priority: P3)

A developer's terminal application receives scroll events that do not include cursor position information (raw ScrollUp/ScrollDown key events rather than VT100 mouse packets). The system converts these into equivalent Up/Down arrow key presses so that scrollable content still responds.

**Why this priority**: Some terminal configurations send scroll events without position data. Converting them to arrow keys provides a graceful fallback that keeps scrollable content functional.

**Independent Test**: Can be fully tested by feeding ScrollUp and ScrollDown key events into the binding system and verifying that Up and Down key presses are injected into the key processor.

**Acceptance Scenarios**:

1. **Given** a terminal that sends scroll events without position data, **When** a ScrollUp event arrives, **Then** the system feeds an Up arrow key press into the key processor as the first event in the queue.
2. **Given** a terminal that sends scroll events without position data, **When** a ScrollDown event arrives, **Then** the system feeds a Down arrow key press into the key processor as the first event in the queue.

---

### User Story 5 - Windows Mouse Event Handling (Priority: P3)

A developer runs their terminal application on Windows using the Win32 console API. Mouse events arrive in a different format (`button;eventType;x;y` as semicolon-separated values). The system parses this format, adjusts coordinates relative to the application's layout position within the console buffer, and dispatches the event to the correct handler.

**Why this priority**: Windows support is required for cross-platform compatibility, but the Win32 console path is less commonly used than VT100 on modern Windows Terminal. It serves as a fallback for legacy Windows console environments.

**Independent Test**: Can be fully tested by feeding Windows-format mouse event strings into the binding system and verifying correct parsing, coordinate adjustment, and handler dispatch.

**Acceptance Scenarios**:

1. **Given** a terminal application running on Windows with Win32 console output, **When** a mouse click event arrives as `Left;MouseDown;10;5`, **Then** the system parses the button, event type, and coordinates, adjusts y for rows above the cursor in the console buffer, and dispatches to the handler.
2. **Given** a terminal application running on a non-Windows platform, **When** a WindowsMouseEvent key arrives, **Then** the system returns NotImplemented without attempting to parse or dispatch.

---

### Edge Cases

- What happens when an unknown XTerm SGR event code is received (code not in the lookup table)? The system returns NotImplemented.
- What happens when an unknown URXVT event code is received? The system falls back to an Unknown button with MouseMove event type.
- What happens when an unknown Typical event code is received? The system attempts lookup and may throw `KeyNotFoundException` if the code is not present. This is intentional: the Python reference does not guard this path, and faithfully porting this behavior (Constitution Principle I) means not adding defensive checks that the original lacks.
- What happens when the renderer's window height is not yet known? The system returns NotImplemented without dispatching.
- What happens when the renderer cannot determine rows above the layout (`HeightIsUnknownException`)? The system catches the exception and returns NotImplemented.
- What happens when mouse coordinates fall outside the visible layout area? The handler at that position is invoked; it may return NotImplemented if no interactive element exists there.
- What happens when Typical-format coordinates contain surrogate escape values from PosixStdinReader? The system detects values >= 0xDC00 and subtracts 0xDC00 before applying the standard -32 offset.
- What happens when modifier key combinations are used with scroll events? The system correctly identifies Shift, Alt, Control, and combinations thereof through the bit-field encoding in XTerm SGR format.
- What happens when mouse coordinates become negative after y-adjustment (e.g., click above the application layout)? The system passes the negative coordinate to the mouse handler registry; the handler at that position (typically the DummyHandler) returns NotImplemented. This matches the Python reference behavior.
- What happens when a malformed escape sequence reaches the mouse handler (truncated, non-numeric values)? Malformed sequences are filtered by the VT100 input parser layer before reaching mouse bindings. The mouse handler assumes well-formed `Data` from the input system as a precondition.
- What happens when `KeyPressEvent.Data` is null or empty? The `Data` property is populated by the input system and is a precondition for all mouse handlers. If null, the handler will fail at array access. This is not guarded because it indicates an input system bug, not a user-facing scenario.
- What happens when concurrent mouse events arrive simultaneously? The `MouseBindings` class is entirely stateless (static methods, immutable lookup tables). Each handler invocation operates on its own `KeyPressEvent` data. No synchronization is needed at the handler level; runtime dependencies (Renderer, MouseHandlers, KeyProcessor) handle their own thread safety.
- What happens if the Application is not yet initialized when a mouse event arrives (`GetApp()` returns null)? The `GetApp()` extension method throws `InvalidOperationException` if `KeyPressEvent.App` is null or not the expected type. This is a precondition: mouse bindings are only active when an Application is running.
- What happens when mouse coordinates are at position (0, 0) after transformation? This is a valid position representing the top-left corner of the layout. The handler registered at (0, 0) is invoked normally.
- What happens when modifier keys are held during Typical or URXVT scroll events? The Typical and URXVT protocols cannot encode modifiers (FR-007). All events from these protocols, including scroll events, report Unknown modifier regardless of which keys the user holds.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST parse XTerm SGR mouse sequences where uppercase `ESC[<code;x;yM` indicates press/move/scroll events and lowercase `ESC[<code;x;ym` indicates button release events. The suffix character ('M' or 'm') combined with the numeric event code forms the lookup table key. The system extracts button, event type, coordinates, and modifier keys.
- **FR-002**: System MUST parse Typical (X10) mouse sequences (`ESC[M` + 3 raw bytes) and extract button, event type, and coordinates
- **FR-003**: System MUST parse URXVT mouse sequences (`ESC[code;x;yM` without `<` prefix) and extract button, event type, and coordinates
- **FR-004**: System MUST distinguish between Left, Middle, Right, None, and Unknown mouse buttons
- **FR-005**: System MUST distinguish between MouseDown, MouseUp, MouseMove, ScrollUp, and ScrollDown event types
- **FR-006**: System MUST detect Shift, Alt, and Control modifier keys individually and in all combinations (7 non-empty combinations plus no-modifier) for XTerm SGR events. Modifiers are encoded as a bit-field within the event code: bit 2 (value 4) = Shift, bit 3 (value 8) = Alt, bit 4 (value 16) = Control. Button identity uses bits 0-1, and bit 5 (value 32) indicates drag/motion events. Values are mapped to the `MouseModifiers` flags enum.
- **FR-007**: System MUST report Unknown modifier for Typical and URXVT protocol events since those formats cannot encode modifiers. "Unknown modifier" uses the same underlying value as "no modifier" (`MouseModifiers.None` = 0) but carries different semantic intent: it indicates the protocol cannot determine whether modifiers were held, rather than asserting no modifiers were pressed.
- **FR-008**: System MUST convert XTerm SGR 1-based coordinates to 0-based coordinates (subtract 1 from both x and y)
- **FR-008a**: System MUST convert URXVT 1-based coordinates to 0-based coordinates (subtract 1 from both x and y), using the same transform as XTerm SGR
- **FR-009**: System MUST convert Typical format byte-encoded coordinates by subtracting 32, then subtracting 1 to get 0-based coordinates
- **FR-010**: System MUST handle surrogate escape values (>= 0xDC00) in Typical format by subtracting 0xDC00 before applying the standard offset
- **FR-011**: System MUST adjust y-coordinates by subtracting the renderer's rows-above-layout value for all three VT100 protocol formats (XTerm SGR, Typical, URXVT) to account for terminal content above the application layout
- **FR-012**: System MUST dispatch parsed mouse events to the mouse handler registered at the computed (x, y) position via the renderer's mouse handler registry
- **FR-013**: System MUST return NotImplemented (a return value, not an exception) when the renderer's `HeightIsKnown` property returns false. This is an early-exit guard checked before attempting any coordinate adjustment.
- **FR-014**: System MUST catch `HeightIsUnknownException` (thrown by `Renderer.RowsAboveLayout` when terminal height cannot be determined) and return NotImplemented. This handles the race condition where height status changes between the FR-013 check and the actual layout metric access.
- **FR-015**: System MUST return NotImplemented when an XTerm SGR event code is not found in the lookup table
- **FR-016**: System MUST fall back to Unknown button with MouseMove event type when a URXVT event code is not found in the lookup table
- **FR-017**: System MUST convert ScrollUp key events (without position data) into Up arrow key presses fed to the key processor with `first: true` (inserted at the front of the event queue)
- **FR-018**: System MUST convert ScrollDown key events (without position data) into Down arrow key presses fed to the key processor with `first: true` (inserted at the front of the event queue)
- **FR-019**: System MUST parse Windows mouse event format (`button;eventType;x;y` as semicolon-separated string values, e.g. `Left;MouseDown;10;5`) on Windows platforms where a Win32-compatible output is active. If the platform is Windows but the output is not Win32-compatible, the system MUST return NotImplemented.
- **FR-020**: System MUST adjust Windows mouse event y-coordinates by subtracting rows above the cursor position from the Win32 screen buffer info
- **FR-021**: System MUST return NotImplemented for WindowsMouseEvent on non-Windows platforms
- **FR-022**: System MUST provide a single public method that returns a KeyBindings instance containing all mouse-related bindings
- **FR-023**: System MUST register exactly 4 key bindings: Vt100MouseEvent, ScrollUp, ScrollDown, and WindowsMouseEvent
- **FR-024**: System MUST contain the complete XTerm SGR event lookup table with all 108 entries (3 buttons x 2 up/down x 8 modifier combos + 4 drag buttons x 8 modifier combos + 2 scroll directions x 8 modifier combos)
- **FR-025**: System MUST contain the complete Typical event lookup table with all 10 entries matching the Python reference
- **FR-026**: System MUST contain the complete URXVT event lookup table with all 4 entries matching the Python reference

**Note on NotImplemented**: Throughout these requirements, "return NotImplemented" refers to returning the `NotImplementedOrNone.NotImplemented` singleton value from the handler function. It is a normal return value indicating the event was not handled, not an exception. The VT100 and Windows handlers return `NotImplementedOrNone?` (NotImplemented or the handler's result); the scroll handlers return void (always handled).

### Key Entities

- **MouseEvent**: Represents a fully parsed mouse event with position (x, y), event type, button, and modifier set. Dispatched to handlers.
- **Mouse Protocol Format**: The encoding scheme used by the terminal emulator (XTerm SGR, Typical/X10, URXVT, Windows). Determines how raw escape sequences are parsed.
- **Modifier Set**: A `MouseModifiers` flags enum value combining Shift, Alt, and/or Control modifiers active during a mouse event. XTerm SGR encodes these as bit flags in the event code; Typical and URXVT protocols report Unknown (value `MouseModifiers.None`).
- **Mouse Handler**: A callback registered at a specific screen position that processes dispatched mouse events and returns a result or NotImplemented.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 108 XTerm SGR event table entries produce correct (button, event type, modifier) tuples when looked up — "correct" means matching the Python reference source values exactly. Tests MUST validate specific representative entries (e.g., code 0/'M' → Left/MouseDown/None, code 2/'m' → Right/MouseUp/None, code 36/'M' → Shift+Left/MouseMove/Shift, code 64/'M' → None/ScrollUp/None) in addition to total count.
- **SC-002**: All 10 Typical event table entries produce correct (button, event type, modifier) tuples when looked up — verified against the data model table (codes 32-35, 64-67, 96-97)
- **SC-003**: All 4 URXVT event table entries produce correct (button, event type, modifier) tuples when looked up — verified against the data model table (codes 32, 35, 96, 97)
- **SC-004**: Mouse coordinates are correctly transformed from protocol-specific encoding to 0-based positions for all three VT100 protocol formats. Representative test vectors: XTerm SGR (10,5) → (9,4); Typical bytes (42,37) → (9,4); URXVT (14,13) → (13,12); Typical with surrogate escape (0xDC00+42, 0xDC00+37) → (9,4).
- **SC-005**: Scroll events without position data successfully convert to arrow key presses (ScrollUp → Up, ScrollDown → Down) with `first: true`
- **SC-006**: The system gracefully returns NotImplemented for each of these scenarios: (a) unknown XTerm SGR event code not in lookup table, (b) renderer `HeightIsKnown` returns false, (c) `HeightIsUnknownException` thrown by `RowsAboveLayout`, (d) WindowsMouseEvent on non-Windows platform, (e) Windows platform with non-Win32-compatible output
- **SC-007**: Unit test coverage reaches at least 80% of the mouse bindings module. Lookup table validation and coordinate transform logic are fully testable. Handler dispatch paths that require a running Application context may be excluded from coverage calculation if untestable in isolation.
- **SC-008**: The mouse bindings load method produces a KeyBindings instance with exactly 4 registered bindings

### Non-Functional Requirements

- **NFR-001**: Lookup table access MUST be O(1) using `FrozenDictionary` for all three protocol tables. No per-event memory allocation is permitted in the lookup path.
- **NFR-002**: The three static lookup tables (122 total entries across 3 `FrozenDictionary` instances) are allocated once at class load time and impose no ongoing memory overhead beyond their static footprint.
- **NFR-003**: The `MouseBindings` class MUST be a static, stateless class with no mutable fields. All lookup tables are `static readonly`. This design is inherently thread-safe per Constitution Principle XI — no synchronization is required.
- **NFR-004**: The class MUST be safe to call from any thread without external synchronization. Thread safety for runtime dependencies (Renderer, MouseHandlers, KeyProcessor) is the responsibility of those classes, not MouseBindings.

## Assumptions

- The renderer's `HeightIsKnown` property and `RowsAboveLayout` property/method are available from the existing Renderer implementation (Feature 57).
- The `MouseHandlers` registry on the renderer provides a `GetHandler(x, y)` method (x first, then y) that returns a callable handler for the given coordinates.
- The `Keys` enum already contains `Vt100MouseEvent`, `ScrollUp`, `ScrollDown`, and `WindowsMouseEvent` values (Feature 03).
- The `MouseEvent`, `MouseButton`, `MouseEventType`, and `MouseModifiers` (flags enum) types are already defined (Feature 013).
- The `KeyProcessor.Feed` method accepts a `KeyPress` and a `first` parameter to insert at the front of the queue.
- The `Win32Output` class is not yet implemented (part of Feature 21/57). The Windows mouse handler MUST be structured to return NotImplemented when no Win32-compatible output type is available, allowing future integration without modification. When `Win32Output` is available, it provides `GetWin32ScreenBufferInfo()` for coordinate adjustment.
- The event handler's `Data` property contains the raw escape sequence string for mouse events.
- The `HeightIsUnknownException` is defined in `Stroke.Rendering` (in the same file as `Renderer`).
