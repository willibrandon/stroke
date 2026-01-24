# Data Model: Project Setup and Primitives

**Date**: 2026-01-23
**Feature**: 001-project-setup-primitives

## Overview

This document defines the data model for the core primitive types. Both types are simple value types with no persistence, relationships, or state transitions.

## Entities

### Point

**Purpose**: Represents a position in 2D screen space.

**Python Reference**:
```python
class Point(NamedTuple):
    x: int
    y: int
```

**C# Definition**:
```csharp
namespace Stroke.Core.Primitives;

/// <summary>
/// Represents a point in 2D screen coordinates.
/// </summary>
/// <param name="X">The X coordinate (column position).</param>
/// <param name="Y">The Y coordinate (row position).</param>
public readonly record struct Point(int X, int Y)
{
    /// <summary>
    /// Gets a point at the origin (0, 0).
    /// </summary>
    public static Point Zero { get; } = new(0, 0);

    /// <summary>
    /// Returns a new point offset by the specified deltas.
    /// </summary>
    /// <param name="dx">The X offset.</param>
    /// <param name="dy">The Y offset.</param>
    /// <returns>A new Point offset from this point.</returns>
    public Point Offset(int dx, int dy) => new(X + dx, Y + dy);

    /// <summary>
    /// Adds two points together.
    /// </summary>
    public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);

    /// <summary>
    /// Subtracts one point from another.
    /// </summary>
    public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
}
```

**Fields**:

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| X | int | Column position (horizontal) | Any integer (negative allowed for off-screen) |
| Y | int | Row position (vertical) | Any integer (negative allowed for off-screen) |

**Computed Properties**:

| Property | Type | Description |
|----------|------|-------------|
| Zero | Point (static) | Returns Point(0, 0) |

**Operations**:

| Method/Operator | Input | Output | Description |
|-----------------|-------|--------|-------------|
| Offset(dx, dy) | int, int | Point | Returns new Point with added offsets |
| + (Point, Point) | Point, Point | Point | Component-wise addition |
| - (Point, Point) | Point, Point | Point | Component-wise subtraction |

---

### Size

**Purpose**: Represents terminal dimensions (rows and columns).

**Python Reference**:
```python
class Size(NamedTuple):
    rows: int
    columns: int
```

**C# Definition**:
```csharp
namespace Stroke.Core.Primitives;

/// <summary>
/// Represents a size with rows (height) and columns (width).
/// </summary>
/// <param name="Rows">The number of rows (height).</param>
/// <param name="Columns">The number of columns (width).</param>
public readonly record struct Size(int Rows, int Columns)
{
    /// <summary>
    /// Gets a zero-sized Size (0, 0).
    /// </summary>
    public static Size Zero { get; } = new(0, 0);

    /// <summary>
    /// Gets the height (alias for Rows).
    /// </summary>
    public int Height => Rows;

    /// <summary>
    /// Gets the width (alias for Columns).
    /// </summary>
    public int Width => Columns;

    /// <summary>
    /// Gets a value indicating whether this size is empty (zero or negative dimensions).
    /// </summary>
    public bool IsEmpty => Rows <= 0 || Columns <= 0;
}
```

**Fields**:

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Rows | int | Number of rows (height) | Any integer |
| Columns | int | Number of columns (width) | Any integer |

**Computed Properties**:

| Property | Type | Description |
|----------|------|-------------|
| Zero | Size (static) | Returns Size(0, 0) |
| Height | int | Alias for Rows |
| Width | int | Alias for Columns |
| IsEmpty | bool | True if Rows <= 0 OR Columns <= 0 |

---

## Relationships

None. Point and Size are independent value types with no relationships to each other or external entities.

## State Transitions

None. Both types are immutable value types with no lifecycle or state management.

## Validation Rules

| Type | Rule | Behavior |
|------|------|----------|
| Point | Negative coordinates | Allowed (off-screen positions are valid) |
| Size | Negative dimensions | Allowed but IsEmpty returns true |
| Size | Zero dimensions | Allowed but IsEmpty returns true |
| Both | Integer overflow | Standard .NET overflow behavior (wraps in unchecked context) |

## Value Semantics

Both types inherit the following from `readonly record struct`:

- **Equality**: Component-wise value equality (X==X && Y==Y for Point; Rows==Rows && Columns==Columns for Size)
- **Hashing**: Derived from component values
- **ToString**: Automatic formatting (e.g., `Point { X = 5, Y = 10 }`)
- **Deconstruction**: Pattern matching support via positional parameters
- **With-expressions**: Immutable copy with modifications (e.g., `point with { X = 10 }`)
