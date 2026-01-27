namespace Stroke.Filters;

/// <summary>
/// Filter that always returns <c>true</c>.
/// </summary>
/// <remarks>
/// <para>
/// This filter serves as the identity element for AND operations and the
/// annihilator for OR operations in filter algebra.
/// </para>
/// <para>
/// Use <see cref="Instance"/> to get the singleton instance.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Always</c> class
/// from <c>prompt_toolkit.filters.base</c>.
/// </para>
/// </remarks>
public sealed class Always : Filter
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="Always"/> filter.
    /// </summary>
    /// <remarks>
    /// This instance is lazily initialized in a thread-safe manner.
    /// </remarks>
    public static Always Instance { get; } = new();

    /// <summary>
    /// Private constructor prevents external instantiation.
    /// </summary>
    private Always()
    {
    }

    /// <inheritdoc/>
    /// <returns>Always returns <c>true</c>.</returns>
    public override bool Invoke()
    {
        return true;
    }

    /// <inheritdoc/>
    /// <returns>Returns <paramref name="other"/> (identity property: Always &amp; x = x).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is <c>null</c>.</exception>
    public override IFilter And(IFilter other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return other;
    }

    /// <inheritdoc/>
    /// <returns>Returns this <see cref="Always"/> instance (annihilation property: Always | x = Always).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is <c>null</c>.</exception>
    public override IFilter Or(IFilter other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return this;
    }

    /// <inheritdoc/>
    /// <returns>Returns <see cref="Never.Instance"/>.</returns>
    public override IFilter Invert()
    {
        return Never.Instance;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Always";
    }
}
