# Feature 80: Selection

## Overview

Implement selection data structures for tracking text selection state, selection types, and paste modes.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/selection.py`

## Public API

### SelectionType Enum

```csharp
namespace Stroke;

/// <summary>
/// Type of text selection.
/// </summary>
public enum SelectionType
{
    /// <summary>
    /// Character-wise selection (Visual in Vi).
    /// </summary>
    Characters,

    /// <summary>
    /// Line-wise selection (Visual-Line in Vi).
    /// </summary>
    Lines,

    /// <summary>
    /// Block/rectangular selection (Visual-Block in Vi).
    /// </summary>
    Block
}
```

### PasteMode Enum

```csharp
namespace Stroke;

/// <summary>
/// Mode for pasting clipboard content.
/// </summary>
public enum PasteMode
{
    /// <summary>
    /// Yank like Emacs (insert at cursor).
    /// </summary>
    Emacs,

    /// <summary>
    /// Paste after cursor (Vi 'p').
    /// </summary>
    ViAfter,

    /// <summary>
    /// Paste before cursor (Vi 'P').
    /// </summary>
    ViBefore
}
```

### SelectionState

```csharp
namespace Stroke;

/// <summary>
/// State of the current selection.
/// </summary>
public sealed class SelectionState
{
    /// <summary>
    /// The cursor position where selection started.
    /// </summary>
    public int OriginalCursorPosition { get; }

    /// <summary>
    /// The type of selection.
    /// </summary>
    public SelectionType Type { get; }

    /// <summary>
    /// Whether shift-selection mode is active.
    /// </summary>
    public bool ShiftMode { get; private set; }

    /// <summary>
    /// Create a selection state.
    /// </summary>
    /// <param name="originalCursorPosition">Starting cursor position.</param>
    /// <param name="type">Type of selection.</param>
    public SelectionState(
        int originalCursorPosition = 0,
        SelectionType type = SelectionType.Characters);

    /// <summary>
    /// Enter shift-selection mode.
    /// </summary>
    public void EnterShiftMode();
}
```

## Project Structure

```
src/Stroke/
├── SelectionType.cs
├── PasteMode.cs
└── SelectionState.cs
tests/Stroke.Tests/
└── SelectionTests.cs
```

## Implementation Notes

### SelectionState Implementation

```csharp
public sealed class SelectionState
{
    public int OriginalCursorPosition { get; }
    public SelectionType Type { get; }
    public bool ShiftMode { get; private set; }

    public SelectionState(
        int originalCursorPosition = 0,
        SelectionType type = SelectionType.Characters)
    {
        OriginalCursorPosition = originalCursorPosition;
        Type = type;
        ShiftMode = false;
    }

    public void EnterShiftMode()
    {
        ShiftMode = true;
    }

    public override string ToString() =>
        $"SelectionState(OriginalCursorPosition={OriginalCursorPosition}, " +
        $"Type={Type})";
}
```

### Integration with Document

The Document class provides methods to work with selections:

```csharp
// In Document
public (int Start, int End) SelectionRange(SelectionState selection)
{
    var start = Math.Min(CursorPosition, selection.OriginalCursorPosition);
    var end = Math.Max(CursorPosition, selection.OriginalCursorPosition);

    if (selection.Type == SelectionType.Lines)
    {
        // Expand to full lines
        start = Text.LastIndexOf('\n', start - 1) + 1;
        var lineEnd = Text.IndexOf('\n', end);
        end = lineEnd >= 0 ? lineEnd + 1 : Text.Length;
    }

    return (start, end);
}

public string SelectedText(SelectionState selection)
{
    var (start, end) = SelectionRange(selection);
    return Text.Substring(start, end - start);
}
```

### Block Selection

Block (rectangular) selection requires special handling for multi-line text:

```csharp
public IEnumerable<(int Start, int End)> BlockSelectionRanges(SelectionState selection)
{
    var (startRow, startCol) = GetRowColumnFromPosition(selection.OriginalCursorPosition);
    var (endRow, endCol) = GetRowColumnFromPosition(CursorPosition);

    var minRow = Math.Min(startRow, endRow);
    var maxRow = Math.Max(startRow, endRow);
    var minCol = Math.Min(startCol, endCol);
    var maxCol = Math.Max(startCol, endCol);

    for (var row = minRow; row <= maxRow; row++)
    {
        var line = Lines[row];
        var lineStart = GetPositionFromRowColumn(row, minCol);
        var lineEnd = GetPositionFromRowColumn(row, Math.Min(maxCol, line.Length));
        yield return (lineStart, lineEnd);
    }
}
```

## Dependencies

- Feature 2: Document (for selection range calculations)

## Implementation Tasks

1. Implement `SelectionType` enum
2. Implement `PasteMode` enum
3. Implement `SelectionState` class
4. Add selection range methods to Document
5. Add block selection support
6. Integrate with Buffer for selection operations
7. Write unit tests

## Acceptance Criteria

- [ ] SelectionType has Characters, Lines, Block values
- [ ] PasteMode has Emacs, ViAfter, ViBefore values
- [ ] SelectionState tracks original position and type
- [ ] ShiftMode can be entered for shift-selection
- [ ] Document.SelectionRange returns correct bounds
- [ ] Line selection expands to full lines
- [ ] Block selection returns per-line ranges
- [ ] Unit tests achieve 80% coverage
