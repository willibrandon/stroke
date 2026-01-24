# Quickstart: Selection System

**Feature**: 003-selection-system
**Date**: 2026-01-23

## Overview

The Selection System provides data structures for tracking text selection state in terminal applications. It supports character, line, and block selection modes (matching Vi visual modes) and paste modes for Emacs and Vi editors.

## Installation

The selection types are part of `Stroke.Core` namespace:

```csharp
using Stroke.Core;
```

## Basic Usage

### Creating a Selection

```csharp
// Default: character selection at position 0
var selection = new SelectionState();

// Character selection at specific position
var charSel = new SelectionState(originalCursorPosition: 42);

// Line selection
var lineSel = new SelectionState(type: SelectionType.Lines);

// Block selection at position
var blockSel = new SelectionState(
    originalCursorPosition: 10,
    type: SelectionType.Block);
```

### Using Selection Types

```csharp
// Check selection type
if (selection.Type == SelectionType.Characters)
{
    // Handle character selection
}
else if (selection.Type == SelectionType.Lines)
{
    // Handle line selection (select entire lines)
}
else if (selection.Type == SelectionType.Block)
{
    // Handle block/rectangular selection
}
```

### Shift Selection Mode

```csharp
// Create selection
var selection = new SelectionState(originalCursorPosition: 5);

// Check if shift mode
Console.WriteLine(selection.ShiftMode); // false

// Enter shift mode (e.g., user held Shift while moving cursor)
selection.EnterShiftMode();

Console.WriteLine(selection.ShiftMode); // true
```

### Using Paste Modes

```csharp
// Determine paste behavior
PasteMode mode = GetPasteModeFromEditor();

switch (mode)
{
    case PasteMode.Emacs:
        // Insert at cursor position
        break;
    case PasteMode.ViAfter:
        // Insert after cursor (Vi 'p')
        break;
    case PasteMode.ViBefore:
        // Insert before cursor (Vi 'P')
        break;
}
```

### Debugging

```csharp
var selection = new SelectionState(
    originalCursorPosition: 10,
    type: SelectionType.Lines);

Console.WriteLine(selection.ToString());
// Output: SelectionState(OriginalCursorPosition=10, Type=Lines)
```

## Integration with Document

The selection system integrates with `Document.GetSelectionTuples()`:

```csharp
var document = new Document("Hello\nWorld\nTest");
var selection = new SelectionState(
    originalCursorPosition: 0,
    type: SelectionType.Lines);

// Get selected line ranges
var ranges = document.GetSelectionTuples(selection, cursorPosition: 12);
// Returns tuples of (from, to) for selected lines
```

## Vi Mode Mapping

| SelectionType | Vi Mode | Key |
|---------------|---------|-----|
| `Characters` | Visual | `v` |
| `Lines` | Visual-Line | `V` |
| `Block` | Visual-Block | `Ctrl+v` |

| PasteMode | Vi Command | Key |
|-----------|------------|-----|
| `ViAfter` | Paste after | `p` |
| `ViBefore` | Paste before | `P` |

## API Reference

### SelectionType Enum

- `Characters` - Character-by-character selection
- `Lines` - Whole line selection
- `Block` - Rectangular block selection

### PasteMode Enum

- `Emacs` - Paste at cursor (Emacs yank)
- `ViAfter` - Paste after cursor (Vi `p`)
- `ViBefore` - Paste before cursor (Vi `P`)

### SelectionState Class

| Member | Type | Description |
|--------|------|-------------|
| `OriginalCursorPosition` | `int` | Position where selection started |
| `Type` | `SelectionType` | Type of selection |
| `ShiftMode` | `bool` | Whether shift key initiated selection |
| `EnterShiftMode()` | `void` | Activate shift selection mode |
| `ToString()` | `string` | Debug representation |
