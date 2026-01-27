# Contract: DimensionUtils Static Class

**Namespace**: `Stroke.Layout`
**File**: `src/Stroke/Layout/DimensionUtils.cs`

## Class Definition

```csharp
namespace Stroke.Layout;

/// <summary>
/// Utility functions for working with Dimension instances.
/// </summary>
public static class DimensionUtils
{
    /// <summary>
    /// Sums a list of Dimension instances.
    /// </summary>
    /// <param name="dimensions">The dimensions to sum.</param>
    /// <returns>
    /// A new Dimension where min, max, and preferred are the sums of the
    /// corresponding values from all input dimensions.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when dimensions is null.
    /// </exception>
    public static Dimension SumLayoutDimensions(IReadOnlyList<Dimension> dimensions);

    /// <summary>
    /// Takes the maximum of a list of Dimension instances.
    /// Used when we have a HSplit/VSplit to get the best width/height.
    /// </summary>
    /// <param name="dimensions">The dimensions to compare.</param>
    /// <returns>
    /// A Dimension representing the maximum constraints from all inputs.
    /// Returns Dimension.Zero() if the list is empty or all dimensions are zero.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when dimensions is null.
    /// </exception>
    public static Dimension MaxLayoutDimensions(IReadOnlyList<Dimension> dimensions);

    /// <summary>
    /// Converts a value to a Dimension.
    /// </summary>
    /// <param name="value">
    /// The value to convert. Accepts:
    /// - null: Returns a default Dimension (no constraints)
    /// - int: Returns Dimension.Exact(value)
    /// - Dimension: Returns the dimension unchanged
    /// - Func&lt;object?&gt;: Calls the function and recursively converts the result
    /// </param>
    /// <returns>A Dimension instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when value is not a supported type.
    /// </exception>
    public static Dimension ToDimension(object? value);

    /// <summary>
    /// Tests whether the given value could be converted to a Dimension.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns>
    /// True if value is null, int, Dimension, or callable; false otherwise.
    /// Note: For callables, this returns true without invoking them,
    /// so it cannot guarantee the callable will produce a valid dimension.
    /// </returns>
    public static bool IsDimension(object? value);
}
```

## Behavior Specifications

### SumLayoutDimensions

1. Sum `min` values from all dimensions
2. Sum `max` values from all dimensions
3. Sum `preferred` values from all dimensions
4. Return `new Dimension(min: sumMin, max: sumMax, preferred: sumPreferred)`
5. If list is empty, return `new Dimension(min: 0, max: 0, preferred: 0)`

**Note**: Weight is not summed (not meaningful for aggregation).

### MaxLayoutDimensions

Algorithm (faithful to Python):

```
1. If dimensions is empty:
   return Dimension.Zero()

2. If all dimensions have preferred=0 AND max=0:
   return Dimension.Zero()

3. Filter out "empty" dimensions (where preferred=0 AND max=0)

4. If filtered list is empty:
   return new Dimension()  // Default dimension

5. Calculate:
   - min = max(d.Min for d in filtered)
   - max = min(d.Max for d in filtered)
   - max = max(max, max(d.Preferred for d in filtered))
   - If min > max: max = min  // Handle non-overlapping ranges
   - preferred = max(d.Preferred for d in filtered)

6. Return new Dimension(min: min, max: max, preferred: preferred)
```

**Edge Cases**:
- Single-element list: Returns that dimension's constraints (after zero-filtering if applicable)
- All-identical dimensions: Returns those same constraints
- Non-overlapping ranges (e.g., [1-5] and [8-9]): Adjusts max upward to equal min

### ToDimension

Type dispatch:
1. `null` → `new Dimension()`
2. `int value` → `Dimension.Exact(value)`
3. `Dimension d` → `d` (passthrough)
4. `Func<object?>` → Call function, recursively call `ToDimension` on result
5. Other types → throw `ArgumentException("Not an integer or Dimension object.")`

### IsDimension

Returns `true` for:
- `null`
- `int` (or boxed int)
- `Dimension` instance
- Any callable (delegate that can be invoked with no arguments)

Returns `false` for all other types.

## D Static Class (Alias)

```csharp
namespace Stroke.Layout;

/// <summary>
/// Convenient alias for Dimension with shorter syntax.
/// </summary>
public static class D
{
    /// <summary>
    /// Creates a dimension with the specified constraints.
    /// </summary>
    public static Dimension Create(
        int? min = null,
        int? max = null,
        int? weight = null,
        int? preferred = null)
        => new Dimension(min, max, weight, preferred);

    /// <summary>
    /// Creates an exact-size dimension.
    /// </summary>
    public static Dimension Exact(int amount)
        => Dimension.Exact(amount);

    /// <summary>
    /// Creates a zero-size dimension.
    /// </summary>
    public static Dimension Zero()
        => Dimension.Zero();
}
```
