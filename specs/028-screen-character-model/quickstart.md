# Quickstart: Screen and Character Model

**Feature**: 028-screen-character-model
**Date**: 2026-01-29

## Overview

This feature provides the Screen buffer system for storing styled characters in a 2D grid. It's the foundation for terminal rendering in Stroke.

## Key Types

| Type | Purpose |
|------|---------|
| `Char` | Immutable styled character cell |
| `CharacterDisplayMappings` | Control character → display mappings |
| `WritePosition` | Rectangular region (x, y, width, height) |
| `Screen` | 2D character buffer with cursor tracking |
| `IWindow` | Marker interface for window dictionary keys |

## Quick Examples

### Creating Characters

```csharp
using Stroke.Layout;

// Cached character (preferred for memory efficiency)
var space = Char.Create(" ", Char.Transparent);
var keyword = Char.Create("if", "class:keyword");

// Direct construction
var ch = new Char("A", "class:text");

// Control characters auto-transform
var ctrl = new Char("\x01", "");
// ctrl.Character == "^A"
// ctrl.Style contains "class:control-character"

// Wide characters report correct width
var cjk = new Char("中", "");
// cjk.Width == 2
```

### Working with Screen

```csharp
using Stroke.Layout;
using Stroke.Core.Primitives;

// Create screen
var screen = new Screen(initialWidth: 80, initialHeight: 24);

// Write characters using indexer
screen[0, 0] = Char.Create("H", "class:title");
screen[0, 1] = Char.Create("i", "class:title");

// Read characters (unset cells return default)
var ch = screen[10, 10]; // Default: space with transparent style

// Access dimensions
Console.WriteLine($"Screen: {screen.Width}x{screen.Height}");
```

### Cursor and Menu Positions

```csharp
// Assuming window implements IWindow
IWindow myWindow = GetWindow();

// Track cursor per window
screen.SetCursorPosition(myWindow, new Point(10, 5));
var cursorPos = screen.GetCursorPosition(myWindow);

// Menu position (falls back to cursor if not set)
screen.SetMenuPosition(myWindow, new Point(10, 7));
var menuPos = screen.GetMenuPosition(myWindow);

// Cursor visibility
screen.ShowCursor = true;
```

### Zero-Width Escape Sequences

```csharp
// Add invisible escape sequences (hyperlinks, etc.)
screen.AddZeroWidthEscape(0, 0, "\x1b]8;;https://example.com\x1b\\");

// Multiple escapes at same position are concatenated
screen.AddZeroWidthEscape(0, 0, "\x1b]8;;\x1b\\"); // Close hyperlink

// Retrieve escapes
var escapes = screen.GetZeroWidthEscapes(0, 0);
```

### Z-Index Float Drawing

```csharp
// Queue draw functions with z-index
screen.DrawWithZIndex(10, () => DrawDialog());
screen.DrawWithZIndex(5, () => DrawTooltip());
screen.DrawWithZIndex(20, () => DrawMenu());

// Execute all in z-index order (5, 10, 20)
screen.DrawAllFloats();
```

### Filling Regions

```csharp
// Define a region
var region = new WritePosition(xpos: 5, ypos: 3, width: 20, height: 10);

// Fill with background style (prepended to existing)
screen.FillArea(region, "class:background");

// Fill with style appended after existing
screen.FillArea(region, "class:highlight", after: true);

// Apply style to all existing content
screen.AppendStyleToContent("class:dim");
```

### Resetting Screen

```csharp
// Clear screen for reuse (avoids allocating a new Screen)
screen.Clear();
// All content, positions, and escapes are cleared
// Width/Height reset to initial constructor values
// DefaultChar and ShowCursor are preserved
```

## Dependencies

This feature requires:
- `Stroke.Core.Primitives.Point` - for cursor/menu positions
- `Stroke.Core.FastDictCache` - for character interning
- `Stroke.Core.UnicodeWidth` - for character width calculation

## Namespace

All types are in `Stroke.Layout`:

```csharp
using Stroke.Layout;
using Stroke.Core.Primitives; // For Point
```

## Thread Safety

- `Char` - immutable, inherently thread-safe
- `CharacterDisplayMappings` - static immutable, thread-safe
- `WritePosition` - immutable record struct, thread-safe
- `Screen` - thread-safe via Lock synchronization

## Next Steps

After implementing this feature, you can:
1. Build a `Renderer` that reads Screen content and outputs to terminal
2. Implement `Window` class (implements IWindow) for layout management
3. Create layout containers that write to Screen
