namespace Stroke.Filters;

/// <summary>
/// Internal filter representing the negation of another filter.
/// </summary>
/// <remarks>
/// <para>
/// This class is created by the <see cref="Filter.Invert"/> method and should not
/// be instantiated directly.
/// </para>
/// <para>
/// Returns the opposite boolean value of the wrapped filter.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>_Invert</c> class
/// from <c>prompt_toolkit.filters.base</c>.
/// </para>
/// </remarks>
internal sealed class InvertFilter : Filter
{
    private readonly IFilter _filter;

    /// <summary>
    /// Gets the filter being negated.
    /// </summary>
    public IFilter InnerFilter => _filter;

    /// <summary>
    /// Initializes a new instance wrapping the specified filter.
    /// </summary>
    /// <param name="filter">The filter to negate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is <c>null</c>.</exception>
    internal InvertFilter(IFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        _filter = filter;
    }

    /// <inheritdoc/>
    /// <returns>The negation of the wrapped filter's result.</returns>
    public override bool Invoke()
    {
        return !_filter.Invoke();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"~{_filter}";
    }
}
