# Contract: Dimension Class

**Namespace**: `Stroke.Layout`
**File**: `src/Stroke/Layout/Dimension.cs`

## Class Definition

```csharp
namespace Stroke.Layout;

/// <summary>
/// Specified dimension (width/height) of a user control or window.
/// The layout engine tries to honor the preferred size. If that is not
/// possible, because the terminal is larger or smaller, it tries to keep
/// in between min and max.
/// </summary>
/// <remarks>
/// This class is immutable and thread-safe.
/// </remarks>
public sealed class Dimension
{
    /// <summary>
    /// Default maximum value used when max is not specified.
    /// Effectively unlimited for terminal layout purposes.
    /// </summary>
    public const int MaxDimensionValue = 1_000_000_000;

    /// <summary>
    /// Default weight value used when weight is not specified.
    /// </summary>
    public const int DefaultWeight = 1;

    /// <summary>
    /// Creates a dimension with the specified constraints.
    /// </summary>
    /// <param name="min">Minimum size. Must be >= 0. Defaults to 0.</param>
    /// <param name="max">Maximum size. Must be >= min. Defaults to MaxDimensionValue.</param>
    /// <param name="weight">
    /// Weight for proportional sizing in VSplit/HSplit containers.
    /// Must be >= 0. Defaults to 1.
    /// </param>
    /// <param name="preferred">
    /// Preferred size. Automatically clamped to [min, max].
    /// Defaults to min if not specified.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when min, max, preferred, or weight is negative.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when max is less than min.
    /// </exception>
    public Dimension(
        int? min = null,
        int? max = null,
        int? weight = null,
        int? preferred = null);

    /// <summary>
    /// Gets the minimum size.
    /// </summary>
    public int Min { get; }

    /// <summary>
    /// Gets the maximum size.
    /// </summary>
    public int Max { get; }

    /// <summary>
    /// Gets the preferred size, always within [Min, Max].
    /// </summary>
    public int Preferred { get; }

    /// <summary>
    /// Gets the weight for proportional sizing.
    /// </summary>
    public int Weight { get; }

    /// <summary>
    /// Gets whether min was explicitly specified in the constructor.
    /// </summary>
    public bool MinSpecified { get; }

    /// <summary>
    /// Gets whether max was explicitly specified in the constructor.
    /// </summary>
    public bool MaxSpecified { get; }

    /// <summary>
    /// Gets whether preferred was explicitly specified in the constructor.
    /// </summary>
    public bool PreferredSpecified { get; }

    /// <summary>
    /// Gets whether weight was explicitly specified in the constructor.
    /// </summary>
    public bool WeightSpecified { get; }

    /// <summary>
    /// Creates a dimension with an exact size (min, max, and preferred all set to amount).
    /// </summary>
    /// <param name="amount">The exact size. Must be >= 0.</param>
    /// <returns>A dimension with min = max = preferred = amount.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when amount is negative.
    /// </exception>
    public static Dimension Exact(int amount);

    /// <summary>
    /// Creates a zero-size dimension representing an invisible element.
    /// </summary>
    /// <returns>A dimension with min = max = preferred = 0.</returns>
    public static Dimension Zero();

    /// <summary>
    /// Returns a string representation showing only explicitly specified parameters.
    /// </summary>
    /// <returns>A string in the format "Dimension(param=value, ...)".</returns>
    public override string ToString();
}
```

## Behavior Specifications

### Constructor Behavior

1. **Parameter Validation**:
   - If `min < 0`: throw `ArgumentOutOfRangeException`
   - If `max < 0`: throw `ArgumentOutOfRangeException`
   - If `preferred < 0`: throw `ArgumentOutOfRangeException`
   - If `weight < 0`: throw `ArgumentOutOfRangeException`

2. **Default Application**:
   - `min ?? 0`
   - `max ?? MaxDimensionValue`
   - `preferred ?? min` (after min default applied)
   - `weight ?? DefaultWeight`

3. **Cross-Parameter Validation**:
   - If `max < min`: throw `ArgumentException("Invalid Dimension: max < min.")`

4. **Preferred Clamping**:
   - If `preferred < min`: set `preferred = min`
   - If `preferred > max`: set `preferred = max`

### ToString Behavior

Returns format: `Dimension(param=value, ...)` where only explicitly specified parameters are included.

Examples:
- `new Dimension()` → `"Dimension()"`
- `new Dimension(min: 5)` → `"Dimension(min=5)"`
- `new Dimension(min: 5, max: 10)` → `"Dimension(min=5, max=10)"`
- `new Dimension(min: 5, max: 10, preferred: 7, weight: 2)` → `"Dimension(min=5, max=10, preferred=7, weight=2)"`
