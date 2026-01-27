namespace Stroke.Filters;

/// <summary>
/// Represents either a boolean value or an <see cref="IFilter"/> instance.
/// </summary>
/// <remarks>
/// <para>
/// This union type allows APIs to accept both static boolean values and
/// dynamic filter conditions with a single parameter type.
/// </para>
/// <para>
/// Use implicit conversion from <see cref="bool"/> or <see cref="Filter"/>
/// to create instances.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>FilterOrBool</c> type
/// from <c>prompt_toolkit.filters.base</c>.
/// </para>
/// </remarks>
public readonly struct FilterOrBool : IEquatable<FilterOrBool>
{
    private readonly IFilter? _filter;
    private readonly bool _boolValue;
    private readonly bool _isFilter;

    /// <summary>
    /// Gets a value indicating whether this instance contains a filter.
    /// </summary>
    public bool IsFilter => _isFilter;

    /// <summary>
    /// Gets a value indicating whether this instance contains a boolean.
    /// </summary>
    public bool IsBool => !_isFilter;

    /// <summary>
    /// Gets the boolean value if <see cref="IsBool"/> is <c>true</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when this instance contains a filter instead of a boolean.
    /// </exception>
    public bool BoolValue
    {
        get
        {
            if (_isFilter)
            {
                throw new InvalidOperationException("Cannot get BoolValue when IsFilter is true.");
            }

            return _boolValue;
        }
    }

    /// <summary>
    /// Gets the filter if <see cref="IsFilter"/> is <c>true</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when this instance contains a boolean instead of a filter.
    /// </exception>
    public IFilter FilterValue
    {
        get
        {
            if (!_isFilter)
            {
                throw new InvalidOperationException("Cannot get FilterValue when IsBool is true.");
            }

            return _filter!;
        }
    }

    /// <summary>
    /// Creates a new instance containing a boolean value.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    public FilterOrBool(bool value)
    {
        _isFilter = false;
        _boolValue = value;
        _filter = null;
    }

    /// <summary>
    /// Creates a new instance containing a filter.
    /// </summary>
    /// <param name="filter">The filter. If <c>null</c>, treated as <see cref="Never"/>.</param>
    public FilterOrBool(IFilter? filter)
    {
        _isFilter = true;
        _boolValue = false;
        _filter = filter ?? Never.Instance;
    }

    /// <summary>
    /// Implicit conversion from boolean.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    public static implicit operator FilterOrBool(bool value) => new(value);

    /// <summary>
    /// Implicit conversion from filter.
    /// </summary>
    /// <param name="filter">The filter.</param>
    public static implicit operator FilterOrBool(Filter filter) => new(filter);

    /// <inheritdoc/>
    public bool Equals(FilterOrBool other)
    {
        if (_isFilter != other._isFilter)
        {
            return false;
        }

        if (_isFilter)
        {
            return ReferenceEquals(_filter, other._filter);
        }

        return _boolValue == other._boolValue;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is FilterOrBool other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (_isFilter)
        {
            return HashCode.Combine(true, _filter?.GetHashCode() ?? 0);
        }

        return HashCode.Combine(false, _boolValue);
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><c>true</c> if equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(FilterOrBool left, FilterOrBool right) => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><c>true</c> if not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(FilterOrBool left, FilterOrBool right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString()
    {
        if (_isFilter)
        {
            return _filter!.ToString() ?? string.Empty;
        }

        return _boolValue ? "true" : "false";
    }
}
