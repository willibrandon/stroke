# Data Model: Keys Enum

**Feature**: 011-keys-enum
**Date**: 2026-01-25
**Purpose**: Define entities, relationships, and validation rules for Phase 1 design

## Entities

### E1: Keys (Enum)

**Definition**: Enumeration of all key press types for key bindings, providing compile-time safety.

**Values**: 143 primary key values organized by category:

| Category | Values | Count |
|----------|--------|-------|
| Escape | `Escape`, `ShiftEscape` | 2 |
| Control Characters | `ControlAt`..`ControlZ`, `ControlBackslash`, `ControlSquareClose`, `ControlCircumflex`, `ControlUnderscore` | 31 |
| Control + Numbers | `Control0`..`Control9` | 10 |
| Control + Shift + Numbers | `ControlShift0`..`ControlShift9` | 10 |
| Navigation | `Left`, `Right`, `Up`, `Down`, `Home`, `End`, `Insert`, `Delete`, `PageUp`, `PageDown` | 10 |
| Control + Navigation | `ControlLeft`..`ControlPageDown` | 10 |
| Shift + Navigation | `ShiftLeft`..`ShiftPageDown` | 10 |
| Control + Shift + Navigation | `ControlShiftLeft`..`ControlShiftPageDown` | 10 |
| Tab | `BackTab` | 1 |
| Function Keys | `F1`..`F24` | 24 |
| Control + Function Keys | `ControlF1`..`ControlF24` | 24 |
| Special | `Any`, `ScrollUp`, `ScrollDown`, `CPRResponse`, `Vt100MouseEvent`, `WindowsMouseEvent`, `BracketedPaste`, `SIGINT`, `Ignore` | 9 |

**Backing Type**: `int` (default, implicit values 0-142)

**String Representation**: Each value maps to exactly one canonical string (via `ToKeyString()`):

```text
Keys.Escape        → "escape"
Keys.ShiftEscape   → "s-escape"
Keys.ControlAt     → "c-@"
Keys.ControlA      → "c-a"
Keys.Left          → "left"
Keys.F1            → "f1"
Keys.Any           → "<any>"
Keys.ScrollUp      → "<scroll-up>"
Keys.SIGINT        → "<sigint>"
```

**Validation Rules**:
- V1: All 143 values MUST have unique integer backing values
- V2: All 143 values MUST have unique canonical string representations
- V3: String representations MUST match Python Prompt Toolkit exactly

---

### E2: Key String (Value Object)

**Definition**: The canonical string representation of a key, used for serialization and configuration.

**Format Patterns**:

| Pattern | Examples | Description |
|---------|----------|-------------|
| `escape`, `s-escape` | Escape keys | Standalone or with shift modifier |
| `c-{char}` | `c-a`, `c-@`, `c-1` | Control + character |
| `c-s-{char}` | `c-s-1`, `c-s-left` | Control + Shift + character |
| `s-{key}` | `s-left`, `s-tab` | Shift + key |
| `{direction}` | `left`, `right`, `up`, `down` | Arrow keys |
| `{nav}` | `home`, `end`, `insert`, `delete`, `pageup`, `pagedown` | Navigation keys |
| `f{n}` | `f1`..`f24` | Function keys |
| `c-f{n}` | `c-f1`..`c-f24` | Control + function keys |
| `<{name}>` | `<any>`, `<scroll-up>`, `<sigint>` | Special event keys |

**Validation Rules**:
- V4: Modifier order MUST be `c-` (Control) before `s-` (Shift)
- V5: Special keys MUST use angle bracket notation `<name>`
- V6: Key names MUST be lowercase

---

### E3: Key Alias (Value Object)

**Definition**: An alternative string that maps to a canonical key string for usability.

**Alias Mappings** (8 total):

| Alias String | Canonical String | Rationale |
|--------------|------------------|-----------|
| `"backspace"` | `"c-h"` | Readable name for ASCII DEL |
| `"c-space"` | `"c-@"` | Alternative notation |
| `"enter"` | `"c-m"` | Readable name for carriage return |
| `"tab"` | `"c-i"` | Readable name for horizontal tab |
| `"s-c-left"` | `"c-s-left"` | Modifier order normalization |
| `"s-c-right"` | `"c-s-right"` | Modifier order normalization |
| `"s-c-home"` | `"c-s-home"` | Modifier order normalization |
| `"s-c-end"` | `"c-s-end"` | Modifier order normalization |

**Validation Rules**:
- V7: Alias resolution MUST be case-insensitive
- V8: Canonical string returned by alias lookup MUST match an existing Keys value

---

## Relationships

```
┌──────────────────────────────────────────────────────────────────┐
│                          Keys Enum                                │
│  ┌─────────────┐  ┌─────────────┐        ┌─────────────┐         │
│  │ ControlA    │  │ Enter       │  ...   │ F24         │         │
│  │ (int: 2)    │  │ (int: 14)   │        │ (int: 141)  │         │
│  └──────┬──────┘  └──────┬──────┘        └──────┬──────┘         │
│         │                │                      │                 │
└─────────┼────────────────┼──────────────────────┼─────────────────┘
          │ 1:1            │ 1:1                  │ 1:1
          ▼                ▼                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Canonical Key Strings                         │
│  ┌─────────────┐  ┌─────────────┐        ┌─────────────┐        │
│  │ "c-a"       │  │ "c-m"       │  ...   │ "f24"       │        │
│  └──────┬──────┘  └──────┬──────┘        └─────────────┘        │
│         │                │                                       │
└─────────┼────────────────┼───────────────────────────────────────┘
          │                │
          │                │ N:1 (aliases point to canonical)
          │                │
┌─────────┼────────────────┼───────────────────────────────────────┐
│         │  Key Aliases   │                                       │
│         │  ┌─────────────┴───────┐                               │
│         │  │ "enter" → "c-m"     │                               │
│         │  │ "tab" → "c-i"       │                               │
│         │  │ "backspace" → "c-h" │                               │
│         │  │ "c-space" → "c-@"   │                               │
│         │  │ "s-c-left" → ...    │                               │
│         │  └─────────────────────┘                               │
└──────────────────────────────────────────────────────────────────┘
```

**Cardinalities**:
- Keys → Key String: **1:1** (each enum value has exactly one canonical string)
- Key String → Keys: **1:1** (each canonical string maps to exactly one enum value)
- Alias → Key String: **N:1** (multiple aliases can map to same canonical string)
- Keys ↔ Alias: **1:N** (one Keys value may have multiple aliases, e.g., `ControlM` has `"enter"` alias)

---

## Static Data Structures

### S1: ToKeyString Lookup Dictionary

**Type**: `IReadOnlyDictionary<Keys, string>`
**Purpose**: O(1) enum-to-string conversion
**Initialization**: Static constructor, populated from hardcoded mappings
**Size**: 143 entries

### S2: ParseKey Lookup Dictionary

**Type**: `Dictionary<string, Keys>` with `StringComparer.OrdinalIgnoreCase`
**Purpose**: O(1) case-insensitive string-to-enum conversion
**Initialization**: Static constructor, inverted from S1
**Size**: 143 entries

### S3: Alias Map Dictionary

**Type**: `IReadOnlyDictionary<string, string>` with `StringComparer.OrdinalIgnoreCase`
**Purpose**: Alias string to canonical string resolution
**Initialization**: Static field initializer
**Size**: 8 entries

### S4: AllKeys Collection

**Type**: `IReadOnlyList<string>`
**Purpose**: Enumerate all canonical key strings (matches Python `ALL_KEYS`)
**Initialization**: Static constructor, built from S1 values
**Size**: 143 entries (excludes aliases)

---

## State Transitions

**N/A** - Keys enum and associated data structures are stateless and immutable.

---

## Thread Safety

All entities are inherently thread-safe:
- `Keys` enum: Immutable value type
- Static dictionaries: Initialized once via static constructor (thread-safe by CLR guarantee)
- `KeyAliases` class: Static readonly fields
- No mutable state exists anywhere in this feature
