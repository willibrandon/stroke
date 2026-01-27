# Feature Specification: ANSI Formatted Text - % Operator

**Feature Branch**: `020-ansi-formatted-text`
**Created**: 2026-01-26
**Status**: Draft
**Input**: User description: "Feature 78: ANSI Formatted Text - Implement ANSI escape sequence parsing for converting terminal-styled strings into FormattedText"

## Context

The `Ansi` class was implemented in Feature 015 (Formatted Text System) with comprehensive support for:
- SGR (Select Graphic Rendition) parsing for colors and text attributes
- 16, 256, and true color (24-bit) modes
- Zero-width escape sequences (between `\x01` and `\x02`)
- Cursor forward sequences (for spacing)
- `Format()` method for safe string interpolation

**Missing API**: The Python Prompt Toolkit `ANSI` class includes a `__mod__` method (`%` operator) for Python-style string formatting that was not ported.

This specification addresses adding the `%` operator to achieve 100% API parity with Python Prompt Toolkit per Constitutional Principle I.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Python-style % Interpolation with Single Value (Priority: P1)

A developer wants to insert a single value into an ANSI-styled template using the familiar Python `%` syntax, with automatic ANSI escape sequence neutralization to prevent injection.

**Why this priority**: Core API parity with Python Prompt Toolkit. The `%` operator is a documented public API in Python PTK that developers may expect to use.

**Independent Test**: Can be fully tested by creating an `Ansi` template with `%s` placeholder and applying `%` operator with a value, verifying the value is inserted and any ANSI sequences in the value are escaped.

**Acceptance Scenarios**:

1. **Given** an Ansi template `new Ansi("\x1b[1mHello %s\x1b[0m")`, **When** applying `% "World"`, **Then** the result contains "Hello World" with bold styling applied only to template portions
2. **Given** an Ansi template with `%s` placeholder, **When** applying `%` with a value containing `\x1b[31m`, **Then** the escape sequence in the value is replaced with `?` to prevent style injection

---

### User Story 2 - Python-style % Interpolation with Multiple Values (Priority: P1)

A developer wants to insert multiple values into an ANSI-styled template using tuple-style `%` syntax.

**Why this priority**: Complete the API parity. Python's `%` operator supports both single values and tuples of values.

**Independent Test**: Can be fully tested by creating an Ansi template with multiple `%s` placeholders and applying `%` operator with an array of values.

**Acceptance Scenarios**:

1. **Given** an Ansi template `new Ansi("%s said: %s")`, **When** applying `% new object[] { "Alice", "Hello" }`, **Then** the result contains "Alice said: Hello"
2. **Given** an Ansi template with three `%s` placeholders, **When** applying `%` with an array of three values, **Then** all three values are substituted in order

---

### Edge Cases

| Scenario | Input | Expected Output | Rationale |
|----------|-------|-----------------|-----------|
| Null single value | `% (object?)null` | Empty string substituted | `Escape()` returns `""` for null |
| Null array parameter | `% (object[]?)null` | ArgumentNullException | Defensive; cannot iterate null |
| Empty array | `% new object[] { }` | Template unchanged (no substitutions) | No values to substitute |
| More placeholders than values | `"%s and %s" % "one"` | `"one and %s"` | Python parity |
| More values than placeholders | `"%s" % new[] { "a", "b" }` | `"a"` (extra ignored) | Python parity |
| No placeholders | `"Hello" % "ignored"` | `"Hello"` (unchanged) | No `%s` to replace |
| Non-string value | `"%s" % 42` | `"42"` | ToString() called |
| Value with `\x1b` | `"%s" % "\x1b[31m"` | `"?[31m"` | Escape neutralizes |
| Value with `\b` | `"%s" % "a\bb"` | `"a?b"` | Escape neutralizes |
| Value with both `\x1b` and `\b` | `"%s" % "\x1b\b"` | `"??"` | Both neutralized |

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `%` operator on the `Ansi` class that accepts a single object value
- **FR-002**: System MUST provide a `%` operator on the `Ansi` class that accepts an array of objects
- **FR-003**: The `%` operator MUST replace `%s` placeholders with provided values in order
- **FR-004**: The `%` operator MUST escape ANSI control characters (`\x1b`) in values before substitution
- **FR-005**: The `%` operator MUST escape backspace characters (`\b`) in values before substitution
- **FR-006**: The `%` operator MUST return a new `Ansi` instance (immutability preserved)
- **FR-007**: The `%` operator MUST match the behavior of Python Prompt Toolkit's `ANSI.__mod__` method (source: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/formatted_text/ansi.py` lines 268-276)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All existing Ansi tests continue to pass (regression prevention)
- **SC-002**: The `%` operator with a single value correctly substitutes into `%s` placeholder
- **SC-003**: The `%` operator with an array substitutes multiple `%s` placeholders in sequence
- **SC-004**: Values containing ANSI escape sequences are sanitized (escape character replaced with `?`)
- **SC-005**: Test coverage for the Ansi class maintains 80% or higher coverage
- **SC-006**: API matches Python Prompt Toolkit's ANSI class signature per api-mapping.md

### Test Requirements

Each functional requirement MUST have corresponding test coverage:

| Requirement | Test Scenario(s) |
|-------------|-----------------|
| FR-001 (single value operator) | `PercentOperator_WithSingleValue_*` tests |
| FR-002 (array operator) | `PercentOperator_WithArray_*` tests |
| FR-003 (%s substitution) | All positive substitution tests |
| FR-004 (escape \x1b) | `PercentOperator_WithAnsiInValue_EscapesControlChars` |
| FR-005 (escape \b) | `PercentOperator_WithBackspaceInValue_EscapesBackspace` |
| FR-006 (immutability) | `PercentOperator_ReturnsNewInstance` |
| FR-007 (Python parity) | All edge case tests matching Python behavior |

### Security Test Requirements

The following **negative/malicious input** test cases are REQUIRED:

- **SEC-T001**: Value containing raw ANSI escape (`\x1b[31mred\x1b[0m`) - must be neutralized
- **SEC-T002**: Value containing backspace sequences (`password\b\b\b\b****`) - must be neutralized
- **SEC-T003**: Value containing combined injection attempt (`\x1b[31m\b\b\b`) - all control chars neutralized
- **SEC-T004**: Value attempting to reset terminal state (`\x1b[0m\x1b[H\x1b[J`) - escape chars neutralized

## Python API Translation

This section documents how the Python implementation maps to C#.

### Python Source Reference

**Location**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/formatted_text/ansi.py`

**Lines 268-276** (`__mod__` method):
```python
def __mod__(self, value: object) -> ANSI:
    if not isinstance(value, tuple):
        value = (value,)
    value = tuple(ansi_escape(i) for i in value)
    return ANSI(self.value % value)
```

**Lines 290-294** (`ansi_escape` function):
```python
def ansi_escape(text: object) -> str:
    return str(text).replace("\x1b", "?").replace("\b", "?")
```

### Translation Mapping

| Python Construct | C# Equivalent | Rationale |
|------------------|---------------|-----------|
| `def __mod__(self, value)` | Two operator overloads: `%(Ansi, object)` and `%(Ansi, object[])` | C# static typing requires separate signatures; provides cleaner API than runtime type checking |
| `if not isinstance(value, tuple)` | Single-value overload wraps to array internally | C# type system handles this via overload resolution |
| `tuple` argument | `object[]` argument | C# arrays are the idiomatic equivalent of Python tuples for this use case |
| `ansi_escape(i)` | `AnsiFormatter.Escape(args[i])` | Identical behavior: replaces `\x1b` and `\b` with `?` |
| `ANSI(self.value % value)` | `new Ansi(AnsiFormatter.FormatPercent(ansi.Value, ...))` | Both create new instance with formatted string |

### Intentional Deviation

**Deviation**: C# uses two operator overloads instead of Python's single method with `isinstance` check.

**Rationale**:
1. C# is statically typed; runtime type checking is non-idiomatic
2. Two overloads provide better IntelliSense/autocomplete experience
3. Follows established `Html` class pattern in the codebase
4. Behaviorally equivalent to Python: single values are processed as if they were single-element arrays

## Security Considerations

### Characters Escaped

Only two character sequences are escaped (replaced with `?`):
- `\x1b` (ESC, 0x1B) - ANSI escape sequence initiator
- `\b` (BS, 0x08) - Backspace character

### Characters NOT Escaped (Explicit Exclusions)

The following control characters are **intentionally NOT escaped** to maintain exact parity with Python Prompt Toolkit's `ansi_escape()` function:
- `\x9b` (CSI, 0x9B) - 8-bit CSI introducer
- `\x07` (BEL, 0x07) - Bell character
- `\x0d` (CR, 0x0D) - Carriage return
- Other C0/C1 control characters

**Rationale**: Python PTK only escapes `\x1b` and `\b`. Escaping additional characters would deviate from the Python behavior. If security requirements change in Python PTK, this implementation should be updated to match.

## Assumptions

- The existing `AnsiFormatter.FormatPercent()` method correctly implements `%s` substitution logic (already used by `Html` class).
- The existing `AnsiFormatter.Escape()` method correctly escapes ANSI control characters.
- C# operator overloading for `%` will be used (same pattern as `Html` class).
