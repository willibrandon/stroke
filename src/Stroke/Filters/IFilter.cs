namespace Stroke.Filters;

/// <summary>
/// Base interface for all filters to activate/deactivate a feature depending on a condition.
/// </summary>
/// <remarks>
/// <para>
/// The return value of <see cref="Invoke"/> determines if the feature should be active.
/// </para>
/// <para>
/// Filters can be combined using boolean operators:
/// <list type="bullet">
///   <item><c>&amp;</c> - AND combination</item>
///   <item><c>|</c> - OR combination</item>
///   <item><c>~</c> - Negation</item>
/// </list>
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Filter</c> abstract base class
/// from <c>prompt_toolkit.filters.base</c>.
/// </para>
/// </remarks>
public interface IFilter
{
    /// <summary>
    /// Evaluates the filter condition.
    /// </summary>
    /// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
    bool Invoke();

    /// <summary>
    /// Creates a new filter that is the AND combination of this filter and another.
    /// </summary>
    /// <param name="other">The other filter to combine with.</param>
    /// <returns>A filter that returns <c>true</c> only if both filters return <c>true</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is <c>null</c>.</exception>
    IFilter And(IFilter other);

    /// <summary>
    /// Creates a new filter that is the OR combination of this filter and another.
    /// </summary>
    /// <param name="other">The other filter to combine with.</param>
    /// <returns>A filter that returns <c>true</c> if either filter returns <c>true</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is <c>null</c>.</exception>
    IFilter Or(IFilter other);

    /// <summary>
    /// Creates a new filter that is the negation of this filter.
    /// </summary>
    /// <returns>A filter that returns the opposite of this filter's result.</returns>
    IFilter Invert();
}
