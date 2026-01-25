# Research: Keys Enum

**Feature**: 011-keys-enum
**Date**: 2026-01-25
**Purpose**: Resolve unknowns from Technical Context before Phase 1 design

## Research Summary

This feature has **no unknowns requiring research**. The Keys enum is a direct 1:1 port of Python Prompt Toolkit's `keys.py` module with:

- **Explicit source of truth**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/keys.py`
- **Exact value mapping**: All 143 enum values and 8 aliases are explicitly defined in the source
- **Deterministic string representations**: Each key's canonical string is hardcoded in Python source
- **No external dependencies**: Pure enum + static data structures
- **No architectural decisions**: Namespace mapping predetermined by `docs/api-mapping.md`

## Decisions

### D1: Enum Backing Type

**Decision**: Use default `int` backing type for Keys enum (implicit integer values starting at 0)

**Rationale**:
- Python's Keys enum uses string values (StrEnum), but C# enums cannot be string-backed
- Integer backing is standard C# practice and sufficient for our needs
- String conversion handled via extension methods and cached dictionaries
- Matches how other Stroke enums are implemented

**Alternatives Rejected**:
- String-based enum patterns (like Java enum with string field) - overcomplicated for this use case
- Explicit integer assignments - unnecessary; default incrementing is sufficient

### D2: String Conversion Strategy

**Decision**: Use extension method `ToKeyString()` with internal dictionary lookup

**Rationale**:
- Python's StrEnum provides implicit `.value` string property
- C# requires explicit conversion; extension method provides similar ergonomics
- Dictionary lookup gives O(1) performance
- Lazy-initialized static dictionary (thread-safe via static constructor)

**Alternatives Rejected**:
- `ToString()` override - cannot override for enums, and default returns enum name not key string
- Attribute-based approach - adds reflection overhead; dictionary is simpler and faster

### D3: Alias Implementation Strategy

**Decision**: Separate `KeyAliases` static class with readonly fields referencing existing Keys values

**Rationale**:
- Python uses enum value aliasing (`Tab = ControlI`)
- C# enums don't support aliasing directly
- Static readonly fields provide compile-time alias resolution
- Keeps Keys enum clean with only the 143 primary values

**Alternatives Rejected**:
- Duplicate enum values with same integer - causes ambiguity in switch statements
- Extension methods on Keys - aliases should be standalone constants, not methods

### D4: Collection of All Keys

**Decision**: `AllKeys.Values` as `IReadOnlyList<string>` containing all canonical key strings

**Rationale**:
- Matches Python's `ALL_KEYS` (list of string values)
- `IReadOnlyList<string>` is immutable and indexable
- Lazy-initialized static field (built once on first access)
- Excludes aliases per Python behavior

**Alternatives Rejected**:
- `IEnumerable<Keys>` - Python returns strings, not enum values
- `IReadOnlySet<string>` - ordering may matter for iteration; list preserves enum order

### D5: Parsing Strategy

**Decision**: `ParseKey()` method using dictionary for canonical keys + `KeyAliasMap.GetCanonical()` for alias resolution

**Rationale**:
- Case-insensitive parsing via `StringComparer.OrdinalIgnoreCase`
- Two-phase lookup: try canonical first, then try alias resolution
- Returns `Keys?` (nullable) for invalid strings instead of throwing

**Alternatives Rejected**:
- Throwing on invalid input - caller should decide error handling
- Single merged dictionary - keeps alias logic separate and explicit

## Source Analysis

### Python Keys Enum Structure (keys.py:11-208)

```python
class Keys(str, Enum):
    value: str
    Escape = "escape"
    ShiftEscape = "s-escape"
    ControlAt = "c-@"  # Also Control-Space
    ControlA = "c-a"
    # ... 143 total primary values

    # Aliases (references to other enum values)
    ControlSpace = ControlAt
    Tab = ControlI
    Enter = ControlM
    Backspace = ControlH
    ShiftControlLeft = ControlShiftLeft
    ShiftControlRight = ControlShiftRight
    ShiftControlHome = ControlShiftHome
    ShiftControlEnd = ControlShiftEnd

ALL_KEYS: list[str] = [k.value for k in Keys]

KEY_ALIASES: dict[str, str] = {
    "backspace": "c-h",
    "c-space": "c-@",
    "enter": "c-m",
    "tab": "c-i",
    "s-c-left": "c-s-left",
    "s-c-right": "c-s-right",
    "s-c-home": "c-s-home",
    "s-c-end": "c-s-end",
}
```

### Verified Key Count

From Python source analysis:
- **143 primary enum values** (excludes aliases which are just references)
- **8 alias mappings** in KEY_ALIASES dictionary
- **5 enum aliases** (ControlSpace, Tab, Enter, Backspace, ShiftControl*)

## C# Implementation Mapping

| Python | C# Type | C# API |
|--------|---------|--------|
| `Keys` enum | `enum Keys` | `Stroke.Input.Keys` |
| `Keys.value` property | Extension method | `Keys.ToKeyString()` |
| `Keys(string)` constructor | Static method | `KeysExtensions.ParseKey(string)` |
| `ALL_KEYS` list | Static property | `AllKeys.Values` |
| `KEY_ALIASES` dict | Static property | `KeyAliasMap.Aliases` |
| Enum aliases | Static class fields | `KeyAliases.Tab`, etc. |

## No Further Research Needed

All implementation details are deterministic based on:
1. Python source code (exact values and semantics)
2. api-mapping.md (namespace assignment)
3. Constitution principles (immutability, thread safety)

**Ready for Phase 1: Design & Contracts**
