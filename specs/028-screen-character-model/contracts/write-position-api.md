# API Contract: WritePosition

**Namespace**: `Stroke.Layout`
**Type**: `readonly record struct`
**Thread Safety**: Immutable (inherently thread-safe)

## Type Definition

```csharp
namespace Stroke.Layout;

/// <summary>
/// Represents a rectangular region with position and dimensions.
/// </summary>
/// <remarks>
/// <para>
/// Used for layout and fill operations. The position (XPos, YPos) may be negative
/// to represent partially visible floats, but Width and Height must be non-negative.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>WritePosition</c> class from <c>layout/screen.py</c>.
/// </para>
/// </remarks>
/// <param name="XPos">The X coordinate (column position). May be negative.</param>
/// <param name="YPos">The Y coordinate (row position). May be negative.</param>
/// <param name="Width">The region width. Must be non-negative.</param>
/// <param name="Height">The region height. Must be non-negative.</param>
/// <exception cref="ArgumentOutOfRangeException">
/// Thrown when <paramref name="Width"/> or <paramref name="Height"/> is negative.
/// </exception>
public readonly record struct WritePosition(int XPos, int YPos, int Width, int Height)
```

## Properties

```csharp
/// <summary>
/// Gets the X coordinate (column position).
/// </summary>
/// <remarks>
/// May be negative to represent a partially visible float that extends off the left edge.
/// </remarks>
public int XPos { get; init; }

/// <summary>
/// Gets the Y coordinate (row position).
/// </summary>
/// <remarks>
/// May be negative to represent a partially visible float that extends off the top edge.
/// </remarks>
public int YPos { get; init; }

/// <summary>
/// Gets the region width in columns.
/// </summary>
/// <remarks>
/// Always non-negative. A width of 0 represents an empty region.
/// </remarks>
public int Width { get; init; }

/// <summary>
/// Gets the region height in rows.
/// </summary>
/// <remarks>
/// Always non-negative. A height of 0 represents an empty region.
/// </remarks>
public int Height { get; init; }
```

## Validation

The primary constructor validates constraints:

```csharp
public WritePosition(int XPos, int YPos, int Width, int Height)
{
    if (Width < 0)
        throw new ArgumentOutOfRangeException(nameof(Width), Width, "Width must be non-negative.");
    if (Height < 0)
        throw new ArgumentOutOfRangeException(nameof(Height), Height, "Height must be non-negative.");

    this.XPos = XPos;
    this.YPos = YPos;
    this.Width = Width;
    this.Height = Height;
}
```

## Equality

Value equality is provided automatically by `record struct`:
- Two WritePosition values are equal if all four properties (XPos, YPos, Width, Height) are equal
- GetHashCode combines all four property values

## Usage Examples

```csharp
// Standard region
var region = new WritePosition(5, 10, 80, 24);
// region.XPos == 5, region.YPos == 10
// region.Width == 80, region.Height == 24

// Partially visible float (negative position)
var partialFloat = new WritePosition(-5, -3, 20, 10);
// Valid: position can be negative

// Empty region (zero dimensions)
var empty = new WritePosition(0, 0, 0, 0);
// Valid: width/height can be zero

// Value equality
var a = new WritePosition(1, 2, 3, 4);
var b = new WritePosition(1, 2, 3, 4);
// a == b → true
// a.Equals(b) → true

// Invalid: negative dimensions
try
{
    var invalid = new WritePosition(0, 0, -1, 5);
}
catch (ArgumentOutOfRangeException ex)
{
    // "Width must be non-negative."
}

// ToString
var pos = new WritePosition(5, 10, 80, 24);
// pos.ToString() → "WritePosition(x=5, y=10, width=80, height=24)"
```
