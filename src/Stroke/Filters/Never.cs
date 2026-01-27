namespace Stroke.Filters;

/// <summary>
/// Filter that always returns <c>false</c>.
/// </summary>
/// <remarks>
/// <para>
/// This filter serves as the annihilator for AND operations and the
/// identity element for OR operations in filter algebra.
/// </para>
/// <para>
/// Use <see cref="Instance"/> to get the singleton instance.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Never</c> class
/// from <c>prompt_toolkit.filters.base</c>.
/// </para>
/// </remarks>
public sealed class Never : Filter
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="Never"/> filter.
    /// </summary>
    /// <remarks>
    /// This instance is lazily initialized in a thread-safe manner.
    /// </remarks>
    public static Never Instance { get; } = new();

    /// <summary>
    /// Private constructor prevents external instantiation.
    /// </summary>
    private Never()
    {
    }

    /// <inheritdoc/>
    /// <returns>Always returns <c>false</c>.</returns>
    public override bool Invoke()
    {
        return false;
    }

    /// <inheritdoc/>
    /// <returns>Returns this <see cref="Never"/> instance (annihilation property: Never &amp; x = Never).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is <c>null</c>.</exception>
    public override IFilter And(IFilter other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return this;
    }

    /// <inheritdoc/>
    /// <returns>Returns <paramref name="other"/> (identity property: Never | x = x).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is <c>null</c>.</exception>
    public override IFilter Or(IFilter other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return other;
    }

    /// <inheritdoc/>
    /// <returns>Returns <see cref="Always.Instance"/>.</returns>
    public override IFilter Invert()
    {
        return Always.Instance;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Never";
    }
}
