# Data Model: Vi Digraphs

**Feature**: 026-vi-digraphs
**Date**: 2026-01-28

## Entities

### Digraph Mapping

A digraph mapping associates a two-character sequence with a Unicode code point.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Char1 | `char` | First character of digraph | Any printable ASCII character |
| Char2 | `char` | Second character of digraph | Any printable ASCII character |
| CodePoint | `int` | Unicode code point | 0x0000 to 0x10FFFF (practical range 0x00 to 0xFB06) |

**Key**: `(Char1, Char2)` - composite key of both characters

**Notes**:
- Case-sensitive: `('A', '*')` and `('a', '*')` map to different Greek letters
- Order-sensitive: Only canonical ordering from RFC1345 is valid; `('u', 'E')` is not valid even though `('E', 'u')` maps to Euro sign

### Data Distribution

| Category | Count | Code Point Range | Example |
|----------|-------|------------------|---------|
| Control characters | 32 | 0x00-0x1F | ('N','U') → 0x00 (NUL) |
| ASCII printable | ~30 | 0x20-0x7F | ('S','P') → 0x20 (Space) |
| Latin-1 Supplement | ~100 | 0x80-0xFF | ('c',',') → 0xE7 (ç) |
| Latin Extended-A | ~100 | 0x100-0x17F | ('A','-') → 0x100 (Ā) |
| Latin Extended-B | ~30 | 0x180-0x24F | ('O','9') → 0x1A0 (Ơ) |
| Greek and Coptic | ~80 | 0x370-0x3FF | ('p','*') → 0x3C0 (π) |
| Cyrillic | ~100 | 0x400-0x4FF | ('A','=') → 0x410 (А) |
| Hebrew | ~30 | 0x5D0-0x5EA | ('A','+') → 0x5D0 (א) |
| Arabic | ~60 | 0x600-0x6FF | ('a','+') → 0x627 (ا) |
| Currency symbols | ~10 | 0x20A0-0x20BF | ('E','u') → 0x20AC (€) |
| Mathematical operators | ~50 | 0x2200-0x22FF | ('R','T') → 0x221A (√) |
| Box drawing | ~60 | 0x2500-0x257F | ('h','h') → 0x2500 (─) |
| Hiragana | ~90 | 0x3040-0x309F | ('k','a') → 0x304B (か) |
| Katakana | ~90 | 0x30A0-0x30FF | ('K','a') → 0x30AB (カ) |
| Bopomofo | ~40 | 0x3100-0x312F | ('b','4') → 0x3105 (ㄅ) |
| Latin ligatures | 6 | 0xFB00-0xFB06 | ('f','i') → 0xFB01 (ﬁ) |

**Total**: 1,300+ unique digraph mappings

### Storage Structure

```text
FrozenDictionary<(char, char), int>
│
├── Key: (char, char) value tuple
│   └── Hashable, value equality, case-sensitive
│
└── Value: int (Unicode code point)
    └── Range: 0x00 to 0xFB06 (no supplementary plane characters in source)
```

**Memory Estimate**:
- Key: 4 bytes per entry (2 chars × 2 bytes)
- Value: 4 bytes per entry (int)
- Overhead: ~8-16 bytes per entry (hash table metadata)
- Total: ~20KB for 1,300 entries

## Relationships

```text
┌─────────────────┐
│   ViState       │
│ (already exists)│
├─────────────────┤
│ WaitingForDigraph: bool      │◄─── Set true when Ctrl+K pressed
│ DigraphSymbol1: string?      │◄─── First char stored here
└─────────────────┘
         │
         │ Uses for lookup
         ▼
┌─────────────────┐
│   Digraphs      │
│ (new static)    │
├─────────────────┤
│ Map: IReadOnlyDictionary     │
│ Lookup(c1, c2): int?         │
│ GetString(c1, c2): string?   │
└─────────────────┘
```

## State Transitions

This feature has no state transitions - it is a pure lookup utility. State management for digraph input is handled by `ViState` (already implemented):

1. **Ctrl+K pressed**: `ViState.WaitingForDigraph = true`
2. **First char typed**: `ViState.DigraphSymbol1 = char.ToString()`
3. **Second char typed**: Call `Digraphs.GetString(sym1, sym2)`, insert result, reset state
4. **Escape pressed**: Clear `WaitingForDigraph` and `DigraphSymbol1`

## Validation Rules

| Rule | Validation | Error Handling |
|------|------------|----------------|
| VR-001 | Key must exist in dictionary | Return `null` (no exception) |
| VR-002 | Characters must be provided | N/A (method signature ensures this) |
| VR-003 | Order matters | Only canonical order returns result |

## Data Integrity

- **Immutability**: Dictionary is populated at static initialization and never modified
- **Thread Safety**: Guaranteed by immutability (no synchronization needed)
- **Consistency**: 1:1 mapping from Python source; no runtime modifications
