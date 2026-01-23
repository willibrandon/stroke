# Feature 02: Selection System

## Overview

Implement the selection data structures that represent text selection state, types, and paste modes.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/selection.py`

## Public API

### SelectionType Enum

```csharp
namespace Stroke.Core;

/// <summary>
/// Type of selection.
/// </summary>
public enum SelectionType
{
    /// <summary>
    /// Characters. (Visual in Vi.)
    /// </summary>
    Characters,

    /// <summary>
    /// Whole lines. (Visual-Line in Vi.)
    /// </summary>
    Lines,

    /// <summary>
    /// A block selection. (Visual-Block in Vi.)
    /// </summary>
    Block
}
```

### PasteMode Enum

```csharp
namespace Stroke.Core;

/// <summary>
/// Mode for pasting clipboard data.
/// </summary>
public enum PasteMode
{
    /// <summary>
    /// Yank like Emacs.
    /// </summary>
    Emacs,

    /// <summary>
    /// When pressing 'p' in Vi.
    /// </summary>
    ViAfter,

    /// <summary>
    /// When pressing 'P' in Vi.
    /// </summary>
    ViBefore
}
```

### SelectionState Class

```csharp
namespace Stroke.Core;

/// <summary>
/// State of the current selection.
/// </summary>
public sealed class SelectionState
{
    /// <summary>
    /// Creates a new SelectionState.
    /// </summary>
    /// <param name="originalCursorPosition">The cursor position where selection started.</param>
    /// <param name="type">The type of selection.</param>
    public SelectionState(int originalCursorPosition = 0, SelectionType type = SelectionType.Characters);

    /// <summary>
    /// The cursor position where the selection started.
    /// </summary>
    public int OriginalCursorPosition { get; }

    /// <summary>
    /// The type of selection.
    /// </summary>
    public SelectionType Type { get; }

    /// <summary>
    /// Whether shift mode is active.
    /// </summary>
    public bool ShiftMode { get; private set; }

    /// <summary>
    /// Enter shift mode.
    /// </summary>
    public void EnterShiftMode();

    public override string ToString();
}
```

## Project Structure

```
src/Stroke/
└── Core/
    ├── SelectionType.cs
    ├── PasteMode.cs
    └── SelectionState.cs
tests/Stroke.Tests/
└── Core/
    ├── SelectionTypeTests.cs
    ├── PasteModeTests.cs
    └── SelectionStateTests.cs
```

## Implementation Notes

### Enum Values

The enum string values in Python are preserved in C# as the enum member names. The string representation should match:
- `SelectionType.CHARACTERS` → `SelectionType.Characters` (string: "CHARACTERS")
- `SelectionType.LINES` → `SelectionType.Lines` (string: "LINES")
- `SelectionType.BLOCK` → `SelectionType.Block` (string: "BLOCK")

### SelectionState Mutability

Note that `SelectionState` has mutable state (`ShiftMode`). This matches the Python implementation where `enter_shift_mode()` modifies the instance.

## Dependencies

- None (base types only)

## Implementation Tasks

1. Implement `SelectionType` enum
2. Implement `PasteMode` enum
3. Implement `SelectionState` class
4. Write unit tests for all types

## Acceptance Criteria

- [ ] All enum values match Python Prompt Toolkit exactly
- [ ] SelectionState has correct constructor and properties
- [ ] ShiftMode functionality works correctly
- [ ] ToString implementations match Python repr
