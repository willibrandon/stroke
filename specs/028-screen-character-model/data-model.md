# Data Model: Screen and Character Model

**Feature**: 028-screen-character-model
**Date**: 2026-01-29

## Entity Definitions

### Char

Represents a single styled character cell in a Screen buffer.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Character | string | The displayed character string | Not null; may be multi-char for caret notation (e.g., "^A") |
| Style | string | Style classes and formatting | Not null; may be empty |
| Width | int | Display width in terminal cells | 0, 1, or 2 (computed via UnicodeWidth) |

**Invariants**:
- Immutable after construction
- Control characters (0x00-0x1F, 0x7F) transformed to caret notation with "class:control-character" style
- High-byte characters (0x80-0x9F) transformed to hex notation (e.g., "<80>")
- Non-breaking space (0xA0) displayed as space with "class:nbsp" style
- Value equality based on (Character, Style) tuple

**Relationships**:
- Used by: Screen.DataBuffer
- Cached by: CharCache (FastDictCache)

---

### CharacterDisplayMappings

Static utility providing control character to display representation mappings.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Mappings | FrozenDictionary<char, string> | Char → display string | 66 entries (0x00-0x1F, 0x7F, 0x80-0x9F, 0xA0) |

**Mapping Categories**:
1. Control characters (0x00-0x1F): Caret notation (^@, ^A, ..., ^_)
2. Delete (0x7F): ^?
3. High-byte (0x80-0x9F): Hex notation (<80>, <81>, ..., <9f>)
4. Non-breaking space (0xA0): Single space

**Invariants**:
- Static and immutable
- O(1) lookup via FrozenDictionary

---

### WritePosition

Represents a rectangular region for layout and fill operations.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| XPos | int | X coordinate (column) | May be negative (partial visibility) |
| YPos | int | Y coordinate (row) | May be negative (partial visibility) |
| Width | int | Region width | Must be >= 0 |
| Height | int | Region height | Must be >= 0 |

**Invariants**:
- Immutable (readonly record struct)
- Value equality based on all four properties
- Constructor validates Width >= 0 and Height >= 0

**Relationships**:
- Used by: Screen.FillArea, Screen.VisibleWindowsToWritePositions

---

### IWindow

Marker interface for window types used as dictionary keys.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| (none) | - | Marker interface | Must have proper equality semantics |

**Invariants**:
- Serves as key type for cursor/menu position dictionaries
- Actual Window class will implement this interface (future feature)

**Relationships**:
- Used by: Screen.CursorPositions, Screen.MenuPositions, Screen.VisibleWindowsToWritePositions

---

### Screen

Two-dimensional buffer of Char instances with cursor tracking and deferred drawing.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| DefaultChar | Char | Default character for empty cells | Not null; typically space with transparent style |
| Width | int | Current screen width | >= 0; grows with writes |
| Height | int | Current screen height | >= 0; grows with writes |
| ShowCursor | bool | Cursor visibility flag | Default: true |
| DataBuffer | Dictionary<int, Dictionary<int, Char>> | Sparse character storage | Row → (Col → Char) |
| ZeroWidthEscapes | Dictionary<int, Dictionary<int, string>> | Escape sequences | Row → (Col → concatenated escapes) |
| CursorPositions | Dictionary<IWindow, Point> | Per-window cursor | Window → Point |
| MenuPositions | Dictionary<IWindow, Point> | Per-window menu anchor | Window → Point |
| VisibleWindowsToWritePositions | Dictionary<IWindow, WritePosition> | Drawn windows | Window → WritePosition |
| DrawFloatFunctions | List<(int ZIndex, Action DrawFunc)> | Deferred draws | Sorted by ZIndex when executed |

**Invariants**:
- Thread-safe (Lock synchronization per Constitution XI)
- Sparse storage: only accessed cells stored in DataBuffer
- ZeroWidthEscapes at position are concatenated strings
- GetMenuPosition falls back to GetCursorPosition, then Point(0, 0)
- DrawAllFloats executes in ascending z-index order

**State Transitions**:
- Empty → Populated: Writing characters via indexer
- Drawing queued → Drawing executed: DrawAllFloats processes queue
- Draw function may queue additional draws (iterative processing)

**Relationships**:
- Contains: Char (in DataBuffer)
- Uses: IWindow (as dictionary key)
- Uses: Point (for positions)
- Uses: WritePosition (for visible windows and fill areas)

---

## Entity Relationship Diagram

```
┌──────────────────────┐
│  CharacterDisplay    │
│     Mappings         │
│  (static utility)    │
└──────────┬───────────┘
           │ references
           ▼
┌──────────────────────┐       ┌──────────────────────┐
│        Char          │◄──────│      CharCache       │
│  (immutable cell)    │ caches│  (FastDictCache)     │
│  - Character         │       │  - size: 1,000,000   │
│  - Style             │       └──────────────────────┘
│  - Width             │
└──────────┬───────────┘
           │ stored in
           ▼
┌──────────────────────────────────────────────────────┐
│                      Screen                          │
│  (mutable, thread-safe 2D buffer)                    │
│                                                      │
│  DataBuffer: Dict<row, Dict<col, Char>>              │
│  ZeroWidthEscapes: Dict<row, Dict<col, string>>      │
│                                                      │
│  CursorPositions: Dict<IWindow, Point>    ◄──────┐   │
│  MenuPositions: Dict<IWindow, Point>      ◄──────┤   │
│  VisibleWindowsToWritePositions ◄────────────────┤   │
│                                                  │   │
│  DrawFloatFunctions: List<(zindex, Action)>      │   │
└──────────────────────────────────────────────────┼───┘
                                                   │
                                    ┌──────────────┴──────┐
                                    │                     │
                              ┌─────▼─────┐    ┌──────────▼────────┐
                              │  IWindow  │    │   WritePosition   │
                              │ (marker)  │    │ (x, y, w, h)      │
                              └───────────┘    │   record struct   │
                                               └───────────────────┘
```

## Validation Rules

### Char Validation
- Character transformation applied at construction (not deferred)
- Width calculated via UnicodeWidth.GetWidth(Character)
- Empty string character allowed (width = 0)

### WritePosition Validation
- Width must be >= 0 (ArgumentOutOfRangeException if negative)
- Height must be >= 0 (ArgumentOutOfRangeException if negative)
- XPos and YPos may be any int (including negative)

### Screen Validation
- No coordinate validation on access (any int valid)
- FillArea with empty/whitespace style is no-op
- DrawAllFloats processes until queue empty (handles nested draws)
