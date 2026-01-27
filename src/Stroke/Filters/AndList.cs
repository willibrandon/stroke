namespace Stroke.Filters;

/// <summary>
/// Internal filter representing the AND combination of multiple filters.
/// </summary>
/// <remarks>
/// <para>
/// This class is created by the <see cref="Filter.And"/> method and should not
/// be instantiated directly.
/// </para>
/// <para>
/// Evaluates to <c>true</c> only if all contained filters evaluate to <c>true</c>.
/// Uses short-circuit evaluation (left-to-right, stops at first <c>false</c>).
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>_AndList</c> class
/// from <c>prompt_toolkit.filters.base</c>.
/// </para>
/// </remarks>
internal sealed class AndList : Filter
{
    private readonly IReadOnlyList<IFilter> _filters;

    /// <summary>
    /// Gets the filters in this AND combination.
    /// </summary>
    public IReadOnlyList<IFilter> Filters => _filters;

    /// <summary>
    /// Initializes a new instance with the specified filters.
    /// </summary>
    /// <param name="filters">The filters to AND together.</param>
    private AndList(IReadOnlyList<IFilter> filters)
    {
        _filters = filters;
    }

    /// <summary>
    /// Creates a new AND filter from the given filters.
    /// </summary>
    /// <param name="filters">The filters to combine.</param>
    /// <returns>
    /// A single filter if only one unique filter remains after deduplication,
    /// otherwise a new <see cref="AndList"/> containing all unique filters.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the following optimizations:
    /// <list type="bullet">
    ///   <item>Flattens nested <see cref="AndList"/> instances</item>
    ///   <item>Removes duplicate filters using reference equality (preserving order)</item>
    ///   <item>Returns single filter directly if only one remains</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static IFilter Create(IEnumerable<IFilter> filters)
    {
        var flattened = new List<IFilter>();
        var seen = new HashSet<IFilter>(ReferenceEqualityComparer.Instance);

        foreach (var filter in filters)
        {
            if (filter is AndList andList)
            {
                // Flatten nested AndList
                foreach (var inner in andList._filters)
                {
                    if (seen.Add(inner))
                    {
                        flattened.Add(inner);
                    }
                }
            }
            else
            {
                if (seen.Add(filter))
                {
                    flattened.Add(filter);
                }
            }
        }

        return flattened.Count switch
        {
            0 => throw new ArgumentException("At least one filter is required after deduplication.", nameof(filters)),
            1 => flattened[0],
            _ => new AndList(flattened)
        };
    }

    /// <inheritdoc/>
    /// <returns><c>true</c> if all filters return <c>true</c>; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Uses short-circuit evaluation: returns <c>false</c> as soon as any filter returns <c>false</c>.
    /// </remarks>
    public override bool Invoke()
    {
        foreach (var filter in _filters)
        {
            if (!filter.Invoke())
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Join("&", _filters.Select(f => f.ToString()));
    }
}
