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
    public static Dimension SumLayoutDimensions(IReadOnlyList<Dimension> dimensions)
    {
        ArgumentNullException.ThrowIfNull(dimensions);

        if (dimensions.Count == 0)
        {
            return new Dimension(min: 0, max: 0, preferred: 0);
        }

        int sumMin = 0;
        int sumMax = 0;
        int sumPreferred = 0;

        foreach (var d in dimensions)
        {
            sumMin += d.Min;
            sumMax += d.Max;
            sumPreferred += d.Preferred;
        }

        return new Dimension(min: sumMin, max: sumMax, preferred: sumPreferred);
    }

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
    public static Dimension MaxLayoutDimensions(IReadOnlyList<Dimension> dimensions)
    {
        ArgumentNullException.ThrowIfNull(dimensions);

        // Step 1: If empty, return zero dimension
        if (dimensions.Count == 0)
        {
            return Dimension.Zero();
        }

        // Step 2: Check if all dimensions are zero (preferred=0 AND max=0)
        bool allZero = true;
        foreach (var d in dimensions)
        {
            if (d.Preferred != 0 || d.Max != 0)
            {
                allZero = false;
                break;
            }
        }

        if (allZero)
        {
            return Dimension.Zero();
        }

        // Step 3: Filter out "empty" dimensions (where preferred=0 AND max=0)
        var filtered = new List<Dimension>();
        foreach (var d in dimensions)
        {
            if (d.Preferred != 0 || d.Max != 0)
            {
                filtered.Add(d);
            }
        }

        // Step 4: If filtered list is empty, return default dimension
        if (filtered.Count == 0)
        {
            return new Dimension();
        }

        // Step 5: Calculate
        int highestMin = filtered[0].Min;
        int lowestMax = filtered[0].Max;
        int highestPreferred = filtered[0].Preferred;

        for (int i = 1; i < filtered.Count; i++)
        {
            var d = filtered[i];
            highestMin = Math.Max(highestMin, d.Min);
            lowestMax = Math.Min(lowestMax, d.Max);
            highestPreferred = Math.Max(highestPreferred, d.Preferred);
        }

        // Ensure max >= highest preferred
        int resultMax = Math.Max(lowestMax, highestPreferred);

        // Handle non-overlapping ranges: if min > max, set max = min
        if (highestMin > resultMax)
        {
            resultMax = highestMin;
        }

        return new Dimension(min: highestMin, max: resultMax, preferred: highestPreferred);
    }

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
    public static Dimension ToDimension(object? value)
    {
        // null -> default Dimension
        if (value is null)
        {
            return new Dimension();
        }

        // int -> Dimension.Exact(value)
        if (value is int intValue)
        {
            return Dimension.Exact(intValue);
        }

        // Dimension -> passthrough
        if (value is Dimension dimension)
        {
            return dimension;
        }

        // Func<object?> -> call and recursively convert
        if (value is Func<object?> callable)
        {
            var result = callable();
            return ToDimension(result);
        }

        // Unsupported type
        throw new ArgumentException("Not an integer or Dimension object.");
    }

    /// <summary>
    /// Tests whether the given value could be converted to a Dimension.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns>
    /// True if value is null, int, Dimension, or callable; false otherwise.
    /// Note: For callables, this returns true without invoking them,
    /// so it cannot guarantee the callable will produce a valid dimension.
    /// </returns>
    public static bool IsDimension(object? value)
    {
        if (value is null)
            return true;

        if (value is int)
            return true;

        if (value is Dimension)
            return true;

        if (value is Func<object?>)
            return true;

        return false;
    }
}

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
