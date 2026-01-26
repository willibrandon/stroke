# Feature Specification: Mouse Events

**Feature Branch**: `013-mouse-events`
**Created**: 2026-01-25
**Status**: Draft
**Input**: User description: "Feature 17: Mouse Events - Implement the mouse event system for handling mouse clicks, drags, scrolls, and movements in the terminal."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Handle Mouse Clicks (Priority: P1)

As a terminal application developer, I need to detect when users click in specific regions of the terminal so that I can make UI elements interactive and responsive to mouse input.

**Why this priority**: Mouse clicks are the most fundamental mouse interaction. Without click handling, no other mouse functionality provides value. This is the core building block for all mouse-driven UI.

**Independent Test**: Can be fully tested by creating a mouse handler for a region, simulating a click event at those coordinates, and verifying the handler receives the event with correct position, button, and type.

**Acceptance Scenarios**:

1. **Given** a registered mouse handler for a rectangular region, **When** a left mouse button down event occurs within that region, **Then** the handler receives a MouseEvent with EventType=MouseDown, Button=Left, and correct Position coordinates
2. **Given** a registered mouse handler for a rectangular region, **When** a mouse button up event occurs within that region, **Then** the handler receives a MouseEvent with EventType=MouseUp and correct Position
3. **Given** a registered mouse handler for a rectangular region, **When** a right or middle button click occurs, **Then** the handler receives the event with the appropriate Button value (Right or Middle)
4. **Given** no handler registered for a position, **When** a mouse event occurs at that position, **Then** no handler is invoked (no error occurs)

---

### User Story 2 - Handle Mouse Scrolling (Priority: P2)

As a terminal application developer, I need to detect scroll wheel events so that I can implement scrollable content areas in my terminal UI.

**Why this priority**: Scroll events are essential for content-heavy applications but build upon the basic click infrastructure. Scrolling enables navigation through lists, documents, and long content.

**Independent Test**: Can be fully tested by simulating scroll up/down events at specific coordinates and verifying handlers receive the correct ScrollUp or ScrollDown event types.

**Acceptance Scenarios**:

1. **Given** a registered mouse handler for a region, **When** a scroll up event occurs within that region, **Then** the handler receives a MouseEvent with EventType=ScrollUp
2. **Given** a registered mouse handler for a region, **When** a scroll down event occurs within that region, **Then** the handler receives a MouseEvent with EventType=ScrollDown
3. **Given** a scroll event, **When** the event includes modifier keys (Shift, Ctrl, Alt), **Then** the handler receives the event with correct Modifiers set

---

### User Story 3 - Handle Mouse Drag/Movement (Priority: P3)

As a terminal application developer, I need to track mouse movement while a button is held so that I can implement drag-and-drop, selection, and resize interactions.

**Why this priority**: Drag/movement tracking enables advanced interactions but is less commonly needed than clicks and scrolls. It builds on the foundation of button state tracking.

**Independent Test**: Can be fully tested by simulating a mouse down event followed by movement events, verifying each movement event is received with EventType=MouseMove and updated Position.

**Acceptance Scenarios**:

1. **Given** the left mouse button is held down, **When** the mouse moves to a new position, **Then** the handler receives a MouseEvent with EventType=MouseMove and the new Position
2. **Given** a mouse move event, **When** modifier keys are held, **Then** the handler receives the event with correct Modifiers (Shift, Alt, Control)

---

### User Story 4 - Handler Registration for UI Regions (Priority: P1)

As a layout system developer, I need to register and retrieve mouse handlers for specific rectangular regions so that the rendering system can route mouse events to the appropriate UI elements.

**Why this priority**: Handler registration is fundamental infrastructure that must work before any mouse events can be processed. It's co-equal with click handling in importance.

**Independent Test**: Can be fully tested by registering handlers for various regions, querying handlers at different positions, and verifying correct handler retrieval or null for unhandled positions.

**Acceptance Scenarios**:

1. **Given** a MouseHandlers grid, **When** I set a handler for a rectangular region, **Then** querying any position within that region returns the registered handler
2. **Given** a MouseHandlers grid with handlers set, **When** I query a position outside all registered regions, **Then** null is returned
3. **Given** a MouseHandlers grid with handlers, **When** I call Clear, **Then** all handlers are removed and queries return null
4. **Given** overlapping handler regions, **When** I set a new handler for a region, **Then** the newer handler replaces the previous one for overlapping positions

---

### User Story 5 - Event Bubbling Support (Priority: P2)

As a container/layout developer, I need handlers to signal whether they consumed an event so that unhandled events can bubble up to parent containers.

**Why this priority**: Event bubbling enables nested UI components to work together. It's important for complex UIs but requires basic handling to work first.

**Independent Test**: Can be fully tested by having a handler return NotImplemented and verifying the caller can detect this return value to continue event propagation.

**Acceptance Scenarios**:

1. **Given** a mouse handler, **When** it returns NotImplementedOrNone.NotImplemented, **Then** the caller knows the event was not handled and should bubble up
2. **Given** a mouse handler, **When** it returns NotImplementedOrNone.None, **Then** the caller knows the event was handled and should not bubble

---

### Edge Cases

- What happens when a mouse event occurs at position (0, 0)?
  - The handler at position (0, 0) should be invoked normally
- What happens when position coordinates exceed the handler grid bounds?
  - Return null handler (no crash or exception)
- How does the system handle negative coordinates?
  - Return null handler (invalid positions are ignored)
- What happens when a handler is set for a zero-width or zero-height region?
  - No positions are affected (empty region)
- How are modifier combinations handled (e.g., Shift+Ctrl)?
  - The Modifiers flags value contains all active modifiers combined with bitwise OR (e.g., Shift | Control)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a MouseEventType enumeration with values: MouseUp, MouseDown, ScrollUp, ScrollDown, MouseMove
- **FR-002**: System MUST provide a MouseButton enumeration with values: Left, Middle, Right, None, Unknown
- **FR-003**: System MUST provide a MouseModifiers [Flags] enumeration with values: None=0, Shift=1, Alt=2, Control=4
- **FR-004**: System MUST provide a MouseEvent class that encapsulates Position, EventType, Button, and Modifiers
- **FR-005**: System MUST provide a MouseHandlers class that maintains a 2D grid mapping positions to handler callbacks
- **FR-006**: MouseHandlers.SetMouseHandlerForRange MUST accept bounds where xMin/yMin are inclusive and xMax/yMax are exclusive
- **FR-007**: MouseHandlers MUST support retrieving the handler at a specific position (or null if none registered)
- **FR-008**: MouseHandlers MUST support clearing all registered handlers
- **FR-009**: System MUST provide a NotImplementedOrNone return type with NotImplemented and None singleton values
- **FR-010**: MouseEvent.ToString MUST return format: `MouseEvent({Position}, {EventType}, {Button}, {Modifiers})`
- **FR-011**: MouseEvent MUST accept a MouseModifiers flags value for modifier keys
- **FR-012**: MouseEvent.Position MUST use Point from Stroke.Core where X=column, Y=row
- **FR-013**: MouseHandlers.SetMouseHandlerForRange handler parameter MUST be non-null
- **FR-014**: If a registered handler throws an exception, the exception MUST propagate to the caller

### Key Entities

- **MouseEventType**: Enumeration representing the type of mouse interaction (up, down, scroll, move). Namespace: `Stroke.Input`
- **MouseButton**: Enumeration representing which mouse button was involved. Namespace: `Stroke.Input`
- **MouseModifiers**: [Flags] enumeration representing modifier keys held during the event (None=0, Shift=1, Alt=2, Control=4). Namespace: `Stroke.Input`. Named plural to indicate flag combination capability.
- **MouseEvent**: Immutable value type capturing a complete mouse event with position, type, button, and modifiers. Namespace: `Stroke.Input`
- **MouseHandlers**: Mutable container managing a 2D grid of handler callbacks, used by the renderer during layout. Namespace: `Stroke.Layout`. Lifecycle: created empty → populated via SetMouseHandlerForRange → queried via GetHandler → cleared before next layout pass.
- **NotImplementedOrNone**: Abstract type with two singleton instances signaling whether an event was handled. Namespace: `Stroke.Layout`

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 5 MouseEventType values and 5 MouseButton values from Python `mouse_events.py` are represented with matching semantics
- **SC-002**: Mouse handlers can be registered and retrieved for any valid terminal coordinate (non-negative integers)
- **SC-003**: Handler retrieval achieves O(1) time complexity via dictionary lookup
- **SC-004**: Unit tests achieve at least 80% code coverage
- **SC-005**: All public APIs have XML documentation comments
- **SC-006**: MouseEvent correctly represents all combinations of event types, buttons, and modifiers
- **SC-007**: MouseHandlers memory usage scales with registered regions, not grid dimensions (sparse storage)

## Clarifications

### Session 2026-01-25

- Q: Should MouseModifiers be a separate enum with IReadOnlySet or a [Flags] enum? → A: [Flags] enum per api-mapping.md specification
- Q: Why is MouseModifiers plural when Python uses singular MouseModifier? → A: The plural name indicates the type represents a combination of modifiers (flags), not a single value. This follows .NET conventions (e.g., `System.IO.FileAttributes`).
- Q: What is the deviation from Python's frozenset for modifiers? → A: Python uses `frozenset[MouseModifier]` for immutable modifier collections. C# uses `[Flags] enum MouseModifiers` which is more idiomatic, efficient (no allocation), and supports natural bitwise operations. Per Constitution Principle I, this deviation is permitted as a C# language adaptation.
- Q: What is the expected ToString format? → A: `MouseEvent({Position}, {EventType}, {Button}, {Modifiers})` matching Python's `__repr__` output format (e.g., `MouseEvent(Point(10, 5), MouseDown, Left, None)`)

## Assumptions

- The existing Point type from Stroke.Core (`src/Stroke/Core/Primitives/Point.cs`) is available and represents terminal coordinates where X=column, Y=row
- Handler callbacks follow the delegate signature `Func<MouseEvent, NotImplementedOrNone>` (non-nullable delegate)
- This feature does not include VT100 mouse protocol parsing (that's a separate input layer concern)
- MouseHandlers MUST be thread-safe per Constitution XI. All mutable operations (SetMouseHandlerForRange, GetHandler, Clear) MUST use Lock synchronization.
