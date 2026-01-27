namespace Stroke.Filters;

/// <summary>
/// Internal filter representing the OR combination of multiple filters.
/// </summary>
/// <remarks>
/// <para>
/// This class is created by the <see cref="Filter.Or"/> method and should not
/// be instantiated directly.
/// </para>
/// <para>
/// Evaluates to <c>true</c> if any contained filter evaluates to <c>true</c>.
/// Uses short-circuit evaluation (left-to-right, stops at first <c>true</c>).
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>_OrList</c> class
/// from <c>prompt_toolkit.filters.base</c>.
/// </para>
/// </remarks>
internal sealed class OrList : Filter
{
    private readonly IReadOnlyList<IFilter> _filters;

    /// <summary>
    /// Gets the filters in this OR combination.
    /// </summary>
    public IReadOnlyList<IFilter> Filters => _filters;

    /// <summary>
    /// Initializes a new instance with the specified filters.
    /// </summary>
    /// <param name="filters">The filters to OR together.</param>
    private OrList(IReadOnlyList<IFilter> filters)
    {
        _filters = filters;
    }

    /// <summary>
    /// Creates a new OR filter from the given filters.
    /// </summary>
    /// <param name="filters">The filters to combine.</param>
    /// <returns>
    /// A single filter if only one unique filter remains after deduplication,
    /// otherwise a new <see cref="OrList"/> containing all unique filters.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the following optimizations:
    /// <list type="bullet">
    ///   <item>Flattens nested <see cref="OrList"/> instances</item>
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
            if (filter is OrList orList)
            {
                // Flatten nested OrList
                foreach (var inner in orList._filters)
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
            _ => new OrList(flattened)
        };
    }

    /// <inheritdoc/>
    /// <returns><c>true</c> if any filter returns <c>true</c>; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Uses short-circuit evaluation: returns <c>true</c> as soon as any filter returns <c>true</c>.
    /// </remarks>
    public override bool Invoke()
    {
        foreach (var filter in _filters)
        {
            if (filter.Invoke())
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Join("|", _filters.Select(f => f.ToString()));
    }
}
