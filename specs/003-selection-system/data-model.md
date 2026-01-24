# Data Model: Selection System

**Feature**: 003-selection-system
**Date**: 2026-01-23

## Entities

### SelectionType (Enum)

Represents the type of text selection, corresponding to Vi visual modes.

| Value | Description | Vi Mode |
|-------|-------------|---------|
| `Characters` | Character-based selection | Visual (`v`) |
| `Lines` | Whole line selection | Visual-Line (`V`) |
| `Block` | Rectangular block selection | Visual-Block (`Ctrl+v`) |

**C# Definition**:
```csharp
public enum SelectionType
{
    Characters,  // Visual in Vi
    Lines,       // Visual-Line in Vi
    Block        // Visual-Block in Vi
}
```

### PasteMode (Enum)

Represents how clipboard content should be pasted relative to the cursor.

| Value | Description | Editor Command |
|-------|-------------|----------------|
| `Emacs` | Paste at cursor position | Emacs yank (`C-y`) |
| `ViAfter` | Paste after cursor | Vi `p` |
| `ViBefore` | Paste before cursor | Vi `P` |

**C# Definition**:
```csharp
public enum PasteMode
{
    Emacs,    // Yank at cursor
    ViAfter,  // Vi 'p' command
    ViBefore  // Vi 'P' command
}
```

### SelectionState (Class)

Tracks the state of an active text selection.

| Property | Type | Mutable | Default | Description |
|----------|------|---------|---------|-------------|
| `OriginalCursorPosition` | `int` | No | 0 | Cursor position when selection started |
| `Type` | `SelectionType` | No | `Characters` | Type of selection |
| `ShiftMode` | `bool` | Yes* | `false` | Whether selection was initiated with Shift key |

*ShiftMode can only transition from `false` to `true` via `EnterShiftMode()`.

**Methods**:
| Method | Return | Description |
|--------|--------|-------------|
| `EnterShiftMode()` | `void` | Sets ShiftMode to true |
| `ToString()` | `string` | Returns debug representation |

**C# Definition**:
```csharp
public sealed class SelectionState
{
    public SelectionState(
        int originalCursorPosition = 0,
        SelectionType type = SelectionType.Characters);

    public int OriginalCursorPosition { get; }
    public SelectionType Type { get; }
    public bool ShiftMode { get; private set; }

    public void EnterShiftMode();
    public override string ToString();
}
```

## Relationships

```text
SelectionState
├── OriginalCursorPosition: int
├── Type: SelectionType ────────► SelectionType enum
└── ShiftMode: bool

(PasteMode is independent - used by clipboard/paste operations)
```

## State Transitions

### SelectionState.ShiftMode

```text
┌─────────────┐    EnterShiftMode()    ┌─────────────┐
│ ShiftMode = │ ────────────────────► │ ShiftMode = │
│    false    │                        │    true     │
└─────────────┘                        └─────────────┘
                                             │
                                             │ (no way back)
                                             ▼
                                       [terminal state]
```

Note: ShiftMode is a one-way flag. Once set to `true`, it cannot be reset. To start a new selection without shift mode, create a new `SelectionState` instance.

## Validation Rules

1. **OriginalCursorPosition**: Any integer value is valid (including negative, matching Python behavior)
2. **Type**: Must be a valid `SelectionType` enum value
3. **ShiftMode**: Boolean, defaults to `false`

## Usage Examples

```csharp
// Character selection starting at position 10
var charSelection = new SelectionState(originalCursorPosition: 10);

// Line selection starting at position 0
var lineSelection = new SelectionState(type: SelectionType.Lines);

// Block selection with shift mode
var blockSelection = new SelectionState(
    originalCursorPosition: 5,
    type: SelectionType.Block);
blockSelection.EnterShiftMode();
```
