namespace Stroke.Filters;

/// <summary>
/// Utility methods for working with filters and boolean values.
/// </summary>
/// <remarks>
/// <para>
/// Provides conversion between <see cref="FilterOrBool"/> values and
/// <see cref="IFilter"/> instances.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>to_filter</c> and
/// <c>is_true</c> functions from <c>prompt_toolkit.filters.utils</c>.
/// </para>
/// </remarks>
public static class FilterUtils
{
    /// <summary>
    /// Converts a <see cref="FilterOrBool"/> value to an <see cref="IFilter"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>
    /// <see cref="Always.Instance"/> if <paramref name="value"/> is <c>true</c>,
    /// <see cref="Never.Instance"/> if <paramref name="value"/> is <c>false</c>,
    /// or the filter itself if <paramref name="value"/> contains a filter.
    /// </returns>
    public static IFilter ToFilter(FilterOrBool value)
    {
        if (value.IsFilter)
        {
            return value.FilterValue;
        }

        return value.BoolValue ? Always.Instance : Never.Instance;
    }

    /// <summary>
    /// Evaluates a <see cref="FilterOrBool"/> value to a boolean.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <returns>
    /// The boolean result of evaluating the value.
    /// If <paramref name="value"/> is a boolean, returns it directly.
    /// If <paramref name="value"/> is a filter, returns <c>filter.Invoke()</c>.
    /// </returns>
    /// <remarks>
    /// This method is equivalent to <c>ToFilter(value).Invoke()</c> but may be
    /// more efficient for boolean values since it avoids the filter lookup.
    /// Any exception thrown by the filter's <see cref="IFilter.Invoke"/> method
    /// will propagate to the caller.
    /// </remarks>
    public static bool IsTrue(FilterOrBool value)
    {
        if (value.IsFilter)
        {
            return value.FilterValue.Invoke();
        }

        return value.BoolValue;
    }
}
