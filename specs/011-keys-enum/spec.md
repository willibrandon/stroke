# Feature Specification: Keys Enum

**Feature Branch**: `011-keys-enum`
**Created**: 2026-01-25
**Status**: Draft
**Input**: User description: "Implement the Keys enum that defines all possible key press types for key bindings, ported from Python Prompt Toolkit's keys.py"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Register Key Bindings by Enum (Priority: P1)

A developer wants to register key bindings in their terminal application using strongly-typed enum values instead of magic strings, ensuring compile-time safety and IDE autocomplete support.

**Why this priority**: This is the primary use case for the Keys enum - developers need type-safe key binding registration to avoid runtime errors from typos in key strings.

**Independent Test**: Can be fully tested by creating a key binding with any Keys enum value and verifying the enum value correctly identifies the intended key.

**Acceptance Scenarios**:

1. **Given** a Keys enum with all key values defined, **When** a developer registers `Keys.ControlC` as a binding, **Then** the binding correctly identifies Ctrl+C key presses
2. **Given** a Keys enum value like `Keys.Enter`, **When** it is compared or converted to its string representation, **Then** it returns `"c-m"` (the canonical form)
3. **Given** the Keys enum, **When** a developer uses IDE autocomplete, **Then** all 151 key values are discoverable with their descriptions

---

### User Story 2 - Parse Key Strings to Enum Values (Priority: P2)

A developer loading key bindings from a configuration file needs to parse string representations (e.g., `"c-a"`, `"escape"`, `"enter"`) into strongly-typed Keys enum values.

**Why this priority**: Configuration files and user input commonly use string representations. Converting these to type-safe enums enables validation and consistent handling.

**Independent Test**: Can be tested by parsing various key strings (canonical and aliases) and verifying correct enum values are returned.

**Acceptance Scenarios**:

1. **Given** a canonical key string `"c-a"`, **When** parsed, **Then** returns `Keys.ControlA`
2. **Given** an alias key string `"enter"`, **When** parsed, **Then** returns `Keys.ControlM` (the canonical key for Enter)
3. **Given** an invalid key string `"invalid-key"`, **When** parsed, **Then** returns null (no match found)
4. **Given** a key string with alternate modifier order `"s-c-left"`, **When** parsed, **Then** returns `Keys.ControlShiftLeft` (normalizes to canonical `"c-s-left"`)

---

### User Story 3 - Use Key Aliases for Readability (Priority: P3)

A developer wants to use readable key names like `Tab`, `Enter`, `Backspace` in their code instead of control character equivalents like `ControlI`, `ControlM`, `ControlH`.

**Why this priority**: Code readability improves maintainability. Aliases allow developers to express intent clearly while maintaining compatibility with the underlying key system.

**Independent Test**: Can be tested by using alias constants and verifying they resolve to the correct underlying Keys values.

**Acceptance Scenarios**:

1. **Given** the `KeyAliases.Tab` constant, **When** used in a binding, **Then** it is equivalent to `Keys.ControlI`
2. **Given** the `KeyAliases.Enter` constant, **When** compared to `Keys.ControlM`, **Then** they are equal
3. **Given** the `KeyAliases.Backspace` constant, **When** its string representation is retrieved, **Then** it returns `"c-h"`
4. **Given** backwards-compatibility alias `KeyAliases.ShiftControlLeft`, **When** used, **Then** it is equivalent to `Keys.ControlShiftLeft`

---

### User Story 4 - Enumerate All Valid Keys (Priority: P4)

A developer building a key binding UI or documentation generator needs to enumerate all valid key string values for display or validation purposes.

**Why this priority**: Supporting tooling and validation requires a complete list of all valid key strings.

**Independent Test**: Can be tested by retrieving the collection and verifying it contains all expected canonical key strings.

**Acceptance Scenarios**:

1. **Given** the `AllKeys.Values` collection, **When** accessed, **Then** it contains all canonical key strings (e.g., `"escape"`, `"c-a"`, `"f1"`, etc.)
2. **Given** the `AllKeys.Values` collection, **When** counted, **Then** it contains exactly the same number of entries as the Keys enum (excluding aliases)
3. **Given** any Keys enum value, **When** its string representation is checked against `AllKeys.Values`, **Then** it is present in the collection

---

### Edge Cases

- What happens when parsing an empty string? Returns null (no match)
- What happens when parsing a key string with incorrect casing (e.g., `"C-A"`)? Parsing is case-insensitive; returns `Keys.ControlA`
- What happens when converting an undefined enum value to string? Throws `ArgumentOutOfRangeException`
- How does the system handle alias key strings that map to the same canonical key? All aliases resolve to the same Keys enum value

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `Keys` enum containing all key values from Python Prompt Toolkit's `keys.py` module (151 primary keys)
- **FR-002**: System MUST provide a `ToKeyString()` extension method that converts any `Keys` value to its canonical string representation (e.g., `Keys.ControlA` → `"c-a"`)
- **FR-003**: System MUST provide a `ParseKey()` method that converts a key string to its corresponding `Keys` value, returning null for invalid strings
- **FR-004**: System MUST support case-insensitive key string parsing
- **FR-005**: System MUST provide a `KeyAliases` static class with common key aliases (`Tab`, `Enter`, `Backspace`, `ControlSpace`) as readonly fields
- **FR-006**: System MUST provide backwards-compatibility aliases (`ShiftControlLeft`, `ShiftControlRight`, `ShiftControlHome`, `ShiftControlEnd`)
- **FR-007**: System MUST provide an `AllKeys.Values` collection containing all canonical key string values
- **FR-008**: System MUST provide a `KeyAliasMap.Aliases` dictionary mapping alias strings to canonical key strings
- **FR-009**: System MUST provide a `KeyAliasMap.GetCanonical()` method that returns the canonical key string for any alias
- **FR-010**: System MUST support alias resolution during parsing (e.g., `"enter"` → `Keys.ControlM`)
- **FR-011**: Key string representations MUST exactly match Python Prompt Toolkit (e.g., `"c-a"` not `"ctrl-a"`, `"s-tab"` not `"shift-tab"`)
- **FR-012**: Special event keys MUST use angle-bracket notation (e.g., `"<any>"`, `"<scroll-up>"`, `"<sigint>"`)

### Key Categories

The Keys enum MUST include the following categories matching Python Prompt Toolkit exactly:

- **Escape Keys**: `Escape`, `ShiftEscape`
- **Control Characters**: `ControlAt` through `ControlZ` (27 keys), plus `ControlBackslash`, `ControlSquareClose`, `ControlCircumflex`, `ControlUnderscore`
- **Control + Numbers**: `Control0` through `Control9` (10 keys)
- **Control + Shift + Numbers**: `ControlShift0` through `ControlShift9` (10 keys)
- **Navigation Keys**: `Left`, `Right`, `Up`, `Down`, `Home`, `End`, `Insert`, `Delete`, `PageUp`, `PageDown` (10 keys)
- **Control + Navigation**: `ControlLeft` through `ControlPageDown` (10 keys)
- **Shift + Navigation**: `ShiftLeft` through `ShiftPageDown` (10 keys)
- **Control + Shift + Navigation**: `ControlShiftLeft` through `ControlShiftPageDown` (10 keys)
- **Tab Keys**: `BackTab` (Shift+Tab)
- **Function Keys**: `F1` through `F24` (24 keys)
- **Control + Function Keys**: `ControlF1` through `ControlF24` (24 keys)
- **Special Keys**: `Any`, `ScrollUp`, `ScrollDown`, `CPRResponse`, `Vt100MouseEvent`, `WindowsMouseEvent`, `BracketedPaste`, `SIGINT`, `Ignore`

### Key Alias Mappings

The `KeyAliasMap.Aliases` dictionary MUST contain:

| Alias String  | Canonical String |
| ------------- | ---------------- |
| `"backspace"` | `"c-h"`          |
| `"c-space"`   | `"c-@"`          |
| `"enter"`     | `"c-m"`          |
| `"tab"`       | `"c-i"`          |
| `"s-c-left"`  | `"c-s-left"`     |
| `"s-c-right"` | `"c-s-right"`    |
| `"s-c-home"`  | `"c-s-home"`     |
| `"s-c-end"`   | `"c-s-end"`      |

### Key Entities

- **Keys Enum**: Enumeration of all key types; each value has a unique integer backing and maps to exactly one canonical string representation
- **Key String**: The canonical string representation of a key (e.g., `"c-a"`, `"escape"`, `"<any>"`); used for serialization and configuration
- **Key Alias**: An alternative string that maps to a canonical key string (e.g., `"enter"` → `"c-m"`)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 151 primary Keys enum values match Python Prompt Toolkit's keys.py exactly in name and string representation
- **SC-002**: Parsing any of the 151 canonical key strings returns the correct Keys enum value
- **SC-003**: Parsing any of the 8 alias strings returns the correct Keys enum value
- **SC-004**: Round-trip conversion (enum → string → enum) succeeds for all Keys values
- **SC-005**: Unit test coverage achieves at least 80% for all Keys-related types
- **SC-006**: All key string comparisons work correctly case-insensitively

## Assumptions

- The Keys enum uses standard C# enum semantics with integer backing values (not string-based like Python's StrEnum)
- Extension methods provide the string conversion functionality that Python's StrEnum provides inherently
- The `Stroke.Input` namespace is used per the project's namespace structure
- No external dependencies are required (base types only)
- Thread safety is achieved via immutability (enum values and readonly static data are inherently thread-safe per Constitution XI)
