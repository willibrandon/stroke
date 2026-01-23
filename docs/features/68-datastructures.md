# Feature 68: Data Structures

## Overview

Implement the core data structures used throughout the library: `Point` for 2D coordinates and `Size` for terminal dimensions.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/data_structures.py`

## Public API

### Point Structure

```csharp
namespace Stroke.Data;

/// <summary>
/// Represents a 2D point with X and Y coordinates.
/// Used for cursor positions and screen coordinates.
/// </summary>
public readonly record struct Point(int X, int Y)
{
    /// <summary>
    /// Origin point (0, 0).
    /// </summary>
    public static readonly Point Zero = new(0, 0);

    /// <summary>
    /// Add two points.
    /// </summary>
    public static Point operator +(Point a, Point b) =>
        new(a.X + b.X, a.Y + b.Y);

    /// <summary>
    /// Subtract two points.
    /// </summary>
    public static Point operator -(Point a, Point b) =>
        new(a.X - b.X, a.Y - b.Y);

    /// <summary>
    /// Create a new point with offset.
    /// </summary>
    public Point Offset(int x, int y) => new(X + x, Y + y);

    /// <summary>
    /// Deconstruct into tuple.
    /// </summary>
    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}
```

### Size Structure

```csharp
namespace Stroke.Data;

/// <summary>
/// Represents a 2D size with rows and columns.
/// Used for terminal and window dimensions.
/// </summary>
public readonly record struct Size(int Rows, int Columns)
{
    /// <summary>
    /// Empty size (0, 0).
    /// </summary>
    public static readonly Size Empty = new(0, 0);

    /// <summary>
    /// Total number of cells (rows * columns).
    /// </summary>
    public int Area => Rows * Columns;

    /// <summary>
    /// Check if size has zero area.
    /// </summary>
    public bool IsEmpty => Rows <= 0 || Columns <= 0;

    /// <summary>
    /// Deconstruct into tuple.
    /// </summary>
    public void Deconstruct(out int rows, out int columns)
    {
        rows = Rows;
        columns = Columns;
    }
}
```

## Project Structure

```
src/Stroke/
└── Data/
    ├── Point.cs
    └── Size.cs
tests/Stroke.Tests/
└── Data/
    ├── PointTests.cs
    └── SizeTests.cs
```

## Implementation Notes

### Point Usage Examples

```csharp
// Cursor positioning
var cursorPos = new Point(10, 5);
var newPos = cursorPos.Offset(1, 0); // Move right

// Screen coordinates
var (x, y) = cursorPos;

// Mouse events
var mousePos = new Point(event.X, event.Y);
```

### Size Usage Examples

```csharp
// Terminal size
var termSize = output.GetSize();
Console.WriteLine($"Terminal is {termSize.Columns}x{termSize.Rows}");

// Window dimensions
var windowSize = new Size(rows: 24, columns: 80);

// Deconstruction
var (rows, columns) = termSize;
```

### Record Struct Benefits

Using `record struct` provides:
- Value semantics (no heap allocation)
- Built-in equality comparison
- Built-in ToString()
- Immutability by default
- Deconstruction support

### Comparison with Python

Python uses `NamedTuple`:
```python
class Point(NamedTuple):
    x: int
    y: int

class Size(NamedTuple):
    rows: int
    columns: int
```

C# equivalent with `record struct`:
```csharp
public readonly record struct Point(int X, int Y);
public readonly record struct Size(int Rows, int Columns);
```

### Coordinate System

- **Point.X**: Column (0 = leftmost)
- **Point.Y**: Row (0 = topmost)
- **Size.Columns**: Width
- **Size.Rows**: Height

Note: The coordinate system is typical terminal-style where Y increases downward.

### Integration with WritePosition

```csharp
// WritePosition uses Point-like semantics
public readonly record struct WritePosition(
    int XPos,
    int YPos,
    int Width,
    int Height);

// Convert
var topLeft = new Point(writePos.XPos, writePos.YPos);
var size = new Size(writePos.Height, writePos.Width);
```

## Dependencies

None - these are foundational types.

## Implementation Tasks

1. Implement `Point` record struct
2. Implement `Size` record struct
3. Add operator overloads for Point
4. Add helper properties (Zero, Empty, Area, IsEmpty)
5. Add Offset method to Point
6. Add XML documentation
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Point stores X and Y coordinates
- [ ] Size stores Rows and Columns
- [ ] Point addition and subtraction work
- [ ] Point.Zero is (0, 0)
- [ ] Size.Empty is (0, 0)
- [ ] Size.Area calculates correctly
- [ ] Size.IsEmpty returns true for zero/negative
- [ ] Deconstruction works for both types
- [ ] Record equality comparison works
- [ ] ToString() produces readable output
- [ ] Unit tests achieve 80% coverage
