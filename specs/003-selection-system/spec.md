# Feature Specification: Selection System

**Feature Branch**: `003-selection-system`
**Created**: 2026-01-23
**Status**: Draft
**Input**: User description: "Implement the selection data structures that represent text selection state, types, and paste modes."

## Reference

**Python Source**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/selection.py`

**Downstream Dependencies**: These types will be consumed by:
- `Buffer` class (to track active selection state)
- `Document.GetSelectionTuples()` method (to compute selected ranges)
- Vi visual mode bindings (future)
- Clipboard operations (future)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create Selection State (Priority: P1)

A developer using Stroke needs to track where a text selection began and what type of selection is active (character, line, or block). When the user starts selecting text, the system creates a SelectionState that remembers the original cursor position and selection type.

**Why this priority**: This is the core functionality - without tracking selection state, no selection features (copy, cut, visual mode) can work.

**Independent Test**: Can be fully tested by creating a SelectionState with various cursor positions and selection types, then verifying the stored values are correct.

**Acceptance Scenarios**:

1. **Given** no active selection, **When** a developer creates a SelectionState with cursor position 5 and Characters type, **Then** OriginalCursorPosition returns 5 and Type returns Characters
2. **Given** no active selection, **When** a developer creates a SelectionState with default parameters, **Then** OriginalCursorPosition returns 0 and Type returns Characters
3. **Given** a SelectionState exists, **When** the Type property is accessed, **Then** it returns the selection type specified at construction

---

### User Story 2 - Use Selection Types (Priority: P1)

A developer needs to distinguish between different selection modes that correspond to Vi visual modes: character selection (Visual), line selection (Visual-Line), and block selection (Visual-Block).

**Why this priority**: Selection types are fundamental to implementing Vi visual modes and determining how selected content is processed.

**Independent Test**: Can be fully tested by using each SelectionType enum value and verifying correct string representation and value.

**Acceptance Scenarios**:

1. **Given** a need to track character-based selection, **When** SelectionType.Characters is used, **Then** it correctly identifies character selection mode
2. **Given** a need to track line-based selection, **When** SelectionType.Lines is used, **Then** it correctly identifies line selection mode
3. **Given** a need to track block selection, **When** SelectionType.Block is used, **Then** it correctly identifies block selection mode

---

### User Story 3 - Use Paste Modes (Priority: P2)

A developer needs to distinguish between different paste behaviors: Emacs-style yank, Vi paste-after (p), and Vi paste-before (P). Each mode determines where pasted content is inserted relative to the cursor.

**Why this priority**: Paste modes are essential for implementing editor-specific paste behaviors but depend on having selection/clipboard content first.

**Independent Test**: Can be fully tested by using each PasteMode enum value and verifying correct identification of the paste behavior.

**Acceptance Scenarios**:

1. **Given** a need for Emacs-style paste, **When** PasteMode.Emacs is used, **Then** it correctly identifies Emacs yank behavior
2. **Given** a need for Vi paste-after, **When** PasteMode.ViAfter is used, **Then** it correctly identifies Vi 'p' command behavior
3. **Given** a need for Vi paste-before, **When** PasteMode.ViBefore is used, **Then** it correctly identifies Vi 'P' command behavior

---

### User Story 4 - Enter Shift Mode (Priority: P2)

A developer needs to track when shift-selection mode is active. When the user holds Shift and moves the cursor, the selection should extend. The SelectionState tracks this via a ShiftMode flag that can be activated.

**Why this priority**: Shift-selection is a common editing pattern but is secondary to basic selection state tracking.

**Independent Test**: Can be fully tested by creating a SelectionState, calling EnterShiftMode(), and verifying ShiftMode returns true.

**Acceptance Scenarios**:

1. **Given** a newly created SelectionState, **When** ShiftMode is accessed, **Then** it returns false
2. **Given** a SelectionState with ShiftMode inactive, **When** EnterShiftMode() is called, **Then** ShiftMode returns true
3. **Given** a SelectionState already in shift mode, **When** EnterShiftMode() is called again, **Then** ShiftMode remains true (idempotent)

---

### User Story 5 - Display Selection State (Priority: P3)

A developer debugging selection behavior needs a human-readable representation of the SelectionState. The ToString() method provides a clear representation showing the original cursor position and selection type.

**Why this priority**: Debugging support is helpful but not required for core functionality.

**Independent Test**: Can be fully tested by creating a SelectionState and verifying ToString() returns the expected format.

**Acceptance Scenarios**:

1. **Given** a SelectionState with position 10 and Lines type, **When** ToString() is called, **Then** it returns `SelectionState(OriginalCursorPosition=10, Type=Lines)`

---

### User Story 6 - Sealed Class Constraint (Priority: P3)

The SelectionState class must be sealed to prevent inheritance, ensuring consistent behavior and matching Python's design intent.

**Why this priority**: Architectural constraint that doesn't affect functionality.

**Independent Test**: Can be verified by attempting to inherit from SelectionState and confirming a compile error.

**Acceptance Scenarios**:

1. **Given** SelectionState class definition, **When** a developer attempts to create a subclass, **Then** the compiler produces an error

---

### Edge Cases

- **Negative cursor positions**: The system accepts any int value including negatives (matching Python behavior)
- **Boundary values**: int.MinValue and int.MaxValue are valid cursor positions
- **Default parameters**: Uses position 0 and Characters type when no arguments provided
- **ShiftMode in ToString()**: Not included in output (matching Python's __repr__ implementation)
- **Multiple EnterShiftMode() calls**: Idempotent - calling multiple times has no additional effect
- **One-way ShiftMode**: There is intentionally no ExitShiftMode() method (matches Python design)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a SelectionType enum with three values: Characters, Lines, and Block
- **FR-002**: System MUST provide a PasteMode enum with three values: Emacs, ViAfter, and ViBefore
- **FR-003**: System MUST provide a SelectionState class that stores the original cursor position where selection started (read-only after construction)
- **FR-004**: SelectionState MUST store the type of selection (read-only after construction)
- **FR-005**: SelectionState MUST track whether shift mode is active via a ShiftMode property (mutable via EnterShiftMode only)
- **FR-006**: SelectionState MUST provide an EnterShiftMode() method that activates shift mode (idempotent)
- **FR-007**: SelectionState MUST provide a ToString() method with format: `SelectionState(OriginalCursorPosition={value}, Type={enumValue})`
- **FR-008**: SelectionState constructor MUST accept optional originalCursorPosition (default 0) and type (default Characters) parameters
- **FR-009**: SelectionState MUST be a sealed class (not designed for inheritance, per Constitution II)
- **FR-010**: Enums MUST NOT have explicit underlying values (use C# default int values)
- **FR-011**: SelectionState type parameter MUST be non-nullable (SelectionType, not SelectionType?)

### Key Entities

- **SelectionType**: Enumeration representing the type of text selection (character-based, line-based, or block-based). Maps to Vi visual modes.
- **PasteMode**: Enumeration representing how clipboard content should be pasted (Emacs yank, Vi paste-after, Vi paste-before).
- **SelectionState**: Tracks the state of an active text selection including where it started, what type it is, and whether shift-selection mode is active.

### Property Mutability

| Property | Mutable | Notes |
|----------|---------|-------|
| OriginalCursorPosition | No | Set at construction, read-only thereafter |
| Type | No | Set at construction, read-only thereafter |
| ShiftMode | Yes* | Can only transition false→true via EnterShiftMode() |

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All three SelectionType enum values are accessible and distinguishable
- **SC-002**: All three PasteMode enum values are accessible and distinguishable
- **SC-003**: SelectionState correctly stores and returns the original cursor position
- **SC-004**: SelectionState correctly stores and returns the selection type
- **SC-005**: ShiftMode defaults to false and becomes true after EnterShiftMode() is called
- **SC-006**: ToString() output matches format `SelectionState(OriginalCursorPosition={n}, Type={t})`
- **SC-007**: All public APIs match Python Prompt Toolkit's selection.py (see API Fidelity section)
- **SC-008**: Constructor defaults work correctly (position=0, type=Characters)
- **SC-009**: SelectionState cannot be subclassed (sealed)

## API Fidelity

### Verification Criteria for 100% API Fidelity (SC-007)

1. All items in Python's `__all__` list have C# equivalents: SelectionType, PasteMode, SelectionState
2. All enum values present with equivalent semantics
3. All class properties present with matching types and defaults
4. All class methods present with matching signatures
5. Constructor signature matches (optional params with same defaults)

### Python → C# Mapping

| Python | C# | Deviation |
|--------|-----|-----------|
| `snake_case` names | `PascalCase` names | C# naming convention |
| `__repr__()` | `ToString()` | C# convention |
| `SelectionType.CHARACTERS` | `SelectionType.Characters` | C# enum naming |
| `PasteMode.VI_AFTER` | `PasteMode.ViAfter` | C# enum naming |
| Class is not sealed | Class is sealed | Constitution II requirement |

### Acceptable C# Convention Deviations

1. **Naming**: `snake_case` → `PascalCase` (e.g., `original_cursor_position` → `OriginalCursorPosition`)
2. **Method names**: `__repr__` → `ToString()`, `enter_shift_mode` → `EnterShiftMode`
3. **Enum values**: `SCREAMING_SNAKE_CASE` → `PascalCase` (e.g., `CHARACTERS` → `Characters`)
4. **Sealed class**: Added per Constitution II (Python doesn't have this concept)
5. **Enum ToString()**: Returns `"Characters"` not `"<SelectionType.CHARACTERS: 'CHARACTERS'>"` (C# default behavior)

## Assumptions

- Negative cursor positions are valid (Python does not validate this)
- ShiftMode is intentionally not included in ToString() output, matching Python behavior
- The SelectionState class is mutable only via EnterShiftMode() - this is an intentional deviation from Constitution II's "Immutability by Default" to match Python's design
- Enum member names use PascalCase (C# convention), and enum.ToString() returns PascalCase (C# default behavior)
- Thread-safety is out of scope (matching Python which is not thread-safe)

## Out of Scope

- Thread-safety guarantees
- Serialization support
- Equality comparison overrides (use reference equality)
- GetHashCode override
