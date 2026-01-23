# Feature 24: Layout Dimensions

## Overview

Implement the dimension system used to specify minimum, maximum, preferred, and weighted sizes for layout containers and controls.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/dimension.py`

## Public API

### Dimension Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Specified dimension (width/height) of a user control or window.
///
/// The layout engine tries to honor the preferred size. If that is not
/// possible, because the terminal is larger or smaller, it tries to keep in
/// between min and max.
/// </summary>
public sealed class Dimension
{
    /// <summary>
    /// Creates a dimension.
    /// </summary>
    /// <param name="min">Minimum size.</param>
    /// <param name="max">Maximum size.</param>
    /// <param name="weight">
    /// For a VSplit/HSplit, the actual size will be determined
    /// by taking the proportion of weights from all the children.
    /// </param>
    /// <param name="preferred">Preferred size.</param>
    public Dimension(
        int? min = null,
        int? max = null,
        int? weight = null,
        int? preferred = null);

    /// <summary>
    /// Minimum size.
    /// </summary>
    public int Min { get; }

    /// <summary>
    /// Maximum size.
    /// </summary>
    public int Max { get; }

    /// <summary>
    /// Preferred size.
    /// </summary>
    public int Preferred { get; }

    /// <summary>
    /// Weight for proportional sizing.
    /// </summary>
    public int Weight { get; }

    /// <summary>
    /// True if min was explicitly specified.
    /// </summary>
    public bool MinSpecified { get; }

    /// <summary>
    /// True if max was explicitly specified.
    /// </summary>
    public bool MaxSpecified { get; }

    /// <summary>
    /// True if preferred was explicitly specified.
    /// </summary>
    public bool PreferredSpecified { get; }

    /// <summary>
    /// True if weight was explicitly specified.
    /// </summary>
    public bool WeightSpecified { get; }

    /// <summary>
    /// Return a Dimension with an exact size (min, max, and preferred set to amount).
    /// </summary>
    public static Dimension Exact(int amount);

    /// <summary>
    /// Create a dimension that represents zero size (invisible).
    /// </summary>
    public static Dimension Zero();

    public override string ToString();
}

/// <summary>
/// Alias for Dimension.
/// </summary>
public static class D
{
    public static Dimension Create(
        int? min = null,
        int? max = null,
        int? weight = null,
        int? preferred = null) => new Dimension(min, max, weight, preferred);

    public static Dimension Exact(int amount) => Dimension.Exact(amount);
    public static Dimension Zero() => Dimension.Zero();
}
```

### Dimension Utilities

```csharp
namespace Stroke.Layout;

/// <summary>
/// Dimension utility functions.
/// </summary>
public static class DimensionUtils
{
    /// <summary>
    /// Sum a list of Dimension instances.
    /// </summary>
    public static Dimension SumLayoutDimensions(IReadOnlyList<Dimension> dimensions);

    /// <summary>
    /// Take the maximum of a list of Dimension instances.
    /// Used when we have a HSplit/VSplit to get the best width/height.
    /// </summary>
    public static Dimension MaxLayoutDimensions(IReadOnlyList<Dimension> dimensions);

    /// <summary>
    /// Turn the given object into a Dimension.
    /// </summary>
    /// <param name="value">int, Dimension, Func, or null.</param>
    public static Dimension ToDimension(object? value);

    /// <summary>
    /// Test whether the given value could be a valid dimension.
    /// </summary>
    public static bool IsDimension(object? value);
}
```

## Project Structure

```
src/Stroke/
└── Layout/
    ├── Dimension.cs
    └── DimensionUtils.cs
tests/Stroke.Tests/
└── Layout/
    ├── DimensionTests.cs
    └── DimensionUtilsTests.cs
```

## Implementation Notes

### Default Values

When dimension parameters are not specified:
- `min` defaults to 0
- `max` defaults to a very large number (10^10)
- `preferred` defaults to `min`
- `weight` defaults to 1

### Validation

The constructor validates:
- `max >= min` (throws if violated)
- `preferred` is clamped to `[min, max]`
- `weight >= 0`
- All values are non-negative

### Sum Algorithm

When summing dimensions:
```csharp
sumMin = sum(d.Min for d in dimensions)
sumMax = sum(d.Max for d in dimensions)
sumPreferred = sum(d.Preferred for d in dimensions)
```

### Max Algorithm

When taking the maximum:
1. If all dimensions are zero, return zero
2. Ignore empty dimensions (preferred=0, max=0)
3. Take the highest minimum
4. For max: prefer not to go larger than the smallest max unless other dimensions have bigger preferred
5. Ensure min <= max

### Callable Support

Dimensions can be wrapped in a `Func<Dimension>` for dynamic sizing. The `ToDimension` function handles this by calling the function and recursively converting the result.

## Dependencies

None (leaf feature).

## Implementation Tasks

1. Implement `Dimension` class
2. Implement `D` static class alias
3. Implement `DimensionUtils.SumLayoutDimensions`
4. Implement `DimensionUtils.MaxLayoutDimensions`
5. Implement `DimensionUtils.ToDimension`
6. Implement `DimensionUtils.IsDimension`
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Dimension class matches Python Prompt Toolkit semantics
- [ ] Sum and max algorithms work correctly
- [ ] Dynamic dimension (callable) support works
- [ ] Validation rules are enforced
- [ ] Unit tests achieve 80% coverage
