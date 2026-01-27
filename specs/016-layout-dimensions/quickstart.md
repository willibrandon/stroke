# Quickstart: Layout Dimensions

**Feature**: 016-layout-dimensions

## Overview

The Layout Dimensions system provides size constraints for terminal UI elements. Use `Dimension` to specify minimum, maximum, preferred, and weighted sizes for controls and containers.

## Basic Usage

### Creating Dimensions

```csharp
using Stroke.Layout;

// Default dimension (no constraints)
var d1 = new Dimension();
// Min=0, Max=1000000000, Preferred=0, Weight=1

// With specific constraints
var d2 = new Dimension(min: 10, max: 50, preferred: 30);

// Exact size (fixed, won't grow or shrink)
var d3 = Dimension.Exact(20);
// Min=20, Max=20, Preferred=20

// Zero size (invisible element)
var d4 = Dimension.Zero();
// Min=0, Max=0, Preferred=0

// Using the D alias for shorter syntax
var d5 = D.Create(min: 5, max: 100, weight: 2);
```

### Proportional Sizing with Weights

```csharp
// Panel A gets 1 unit of space
var panelA = new Dimension(weight: 1);

// Panel B gets 2 units (twice as much as A)
var panelB = new Dimension(weight: 2);

// In an HSplit, B would be twice as wide as A
// (subject to min/max constraints)
```

### Checking What Was Specified

```csharp
var d = new Dimension(min: 10, preferred: 20);

Console.WriteLine(d.MinSpecified);       // True
Console.WriteLine(d.MaxSpecified);       // False (used default)
Console.WriteLine(d.PreferredSpecified); // True
Console.WriteLine(d.WeightSpecified);    // False (used default)
```

## Dimension Aggregation

### Summing Dimensions (Side-by-Side Layout)

```csharp
var dimensions = new List<Dimension>
{
    new Dimension(min: 10, max: 30, preferred: 20),
    new Dimension(min: 5, max: 15, preferred: 10)
};

var total = DimensionUtils.SumLayoutDimensions(dimensions);
// Min=15, Max=45, Preferred=30
```

### Max Dimensions (Stacked Layout)

```csharp
var dimensions = new List<Dimension>
{
    new Dimension(min: 10, max: 50, preferred: 30),
    new Dimension(min: 20, max: 40, preferred: 25)
};

var max = DimensionUtils.MaxLayoutDimensions(dimensions);
// Takes highest min, considers max and preferred values
```

## Dynamic Dimensions

For sizes that change at runtime based on application state:

```csharp
// Function that returns a dimension
Func<object?> dynamicSize = () =>
{
    int contentLength = GetCurrentContentLength();
    return new Dimension(min: contentLength, preferred: contentLength);
};

// Convert when needed
var actualDimension = DimensionUtils.ToDimension(dynamicSize);

// Can also use simpler forms
Func<object?> exactSize = () => 42;  // Returns int
var d = DimensionUtils.ToDimension(exactSize);
// Equivalent to Dimension.Exact(42)
```

## Type Conversion

```csharp
// From null (default dimension)
var d1 = DimensionUtils.ToDimension(null);

// From int (exact dimension)
var d2 = DimensionUtils.ToDimension(25);

// From Dimension (passthrough)
var d3 = DimensionUtils.ToDimension(new Dimension(min: 10));

// Check if a value can be converted
bool canConvert = DimensionUtils.IsDimension(someValue);
```

## Common Patterns

### Fixed Toolbar Height

```csharp
var toolbarHeight = Dimension.Exact(3);  // Always 3 rows
```

### Flexible Content Area

```csharp
var contentHeight = new Dimension(min: 10);  // At least 10 rows, can grow
```

### Proportional Split

```csharp
var sidebar = new Dimension(min: 20, max: 40, weight: 1);
var main = new Dimension(min: 40, weight: 3);  // 3x the weight of sidebar
```

### Status Bar (Zero Height When Hidden)

```csharp
bool showStatus = GetStatusBarVisibility();
var statusHeight = showStatus ? Dimension.Exact(1) : Dimension.Zero();
```

## Error Handling

```csharp
// These throw ArgumentOutOfRangeException:
new Dimension(min: -1);     // Negative min
new Dimension(weight: -1);  // Negative weight

// This throws ArgumentException:
new Dimension(min: 50, max: 10);  // max < min

// Preferred is auto-clamped (no error):
var d = new Dimension(min: 10, max: 20, preferred: 100);
// preferred becomes 20 (clamped to max)
```

## Debug Output

```csharp
var d = new Dimension(min: 10, max: 50, weight: 2);
Console.WriteLine(d);
// Output: Dimension(min=10, max=50, weight=2)
// Note: preferred not shown because it wasn't explicitly specified
```
