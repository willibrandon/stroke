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
        int? preferred = null)
    {
        // Track what was specified
        MinSpecified = min.HasValue;
        MaxSpecified = max.HasValue;
        WeightSpecified = weight.HasValue;
        PreferredSpecified = preferred.HasValue;

        // Validate negative values before applying defaults
        if (min.HasValue && min.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(min));
        if (max.HasValue && max.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(max));
        if (preferred.HasValue && preferred.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(preferred));
        if (weight.HasValue && weight.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(weight));

        // Apply defaults
        Min = min ?? 0;
        Max = max ?? MaxDimensionValue;
        Weight = weight ?? DefaultWeight;

        // Cross-parameter validation
        if (Max < Min)
            throw new ArgumentException("Invalid Dimension: max < min.");

        // Preferred defaults to min, then is clamped to [min, max]
        var rawPreferred = preferred ?? Min;
        Preferred = Math.Clamp(rawPreferred, Min, Max);
    }

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
    public static Dimension Exact(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        return new Dimension(min: amount, max: amount, preferred: amount);
    }

    /// <summary>
    /// Creates a zero-size dimension representing an invisible element.
    /// </summary>
    /// <returns>A dimension with min = max = preferred = 0.</returns>
    public static Dimension Zero() => Exact(0);

    /// <summary>
    /// Returns a string representation showing only explicitly specified parameters.
    /// </summary>
    /// <returns>A string in the format "Dimension(param=value, ...)".</returns>
    public override string ToString()
    {
        var parts = new List<string>();

        if (MinSpecified)
            parts.Add($"min={Min}");
        if (MaxSpecified)
            parts.Add($"max={Max}");
        if (PreferredSpecified)
            parts.Add($"preferred={Preferred}");
        if (WeightSpecified)
            parts.Add($"weight={Weight}");

        return $"Dimension({string.Join(", ", parts)})";
    }
}
