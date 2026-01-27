namespace Stroke.Filters;

/// <summary>
/// Abstract base class for filters providing caching and operator overloads.
/// </summary>
/// <remarks>
/// <para>
/// This class provides the common implementation for caching AND, OR, and inversion
/// results to avoid repeated allocations.
/// </para>
/// <para>
/// Derived classes only need to implement <see cref="Invoke"/>.
/// </para>
/// <para>
/// This type is thread-safe. Cache operations are synchronized using
/// <see cref="System.Threading.Lock"/>.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Filter</c> abstract base class
/// from <c>prompt_toolkit.filters.base</c>.
/// </para>
/// </remarks>
public abstract class Filter : IFilter
{
    private readonly Lock _lock = new();
    private readonly Dictionary<IFilter, IFilter> _andCache = new();
    private readonly Dictionary<IFilter, IFilter> _orCache = new();
    private IFilter? _invertResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="Filter"/> class.
    /// </summary>
    protected Filter()
    {
    }

    /// <inheritdoc/>
    public abstract bool Invoke();

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// This method caches results for repeated calls with the same <paramref name="other"/> filter.
    /// </para>
    /// <para>
    /// If <paramref name="other"/> is <see cref="Always"/>, returns this filter (identity property).
    /// If <paramref name="other"/> is <see cref="Never"/>, returns <see cref="Never"/> (annihilation property).
    /// </para>
    /// </remarks>
    public virtual IFilter And(IFilter other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // Identity: x & Always = x
        if (other is Always)
        {
            return this;
        }

        // Annihilation: x & Never = Never
        if (other is Never)
        {
            return other;
        }

        using (_lock.EnterScope())
        {
            if (_andCache.TryGetValue(other, out var cached))
            {
                return cached;
            }

            var result = AndList.Create([this, other]);
            _andCache[other] = result;
            return result;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// This method caches results for repeated calls with the same <paramref name="other"/> filter.
    /// </para>
    /// <para>
    /// If <paramref name="other"/> is <see cref="Always"/>, returns <see cref="Always"/> (annihilation property).
    /// If <paramref name="other"/> is <see cref="Never"/>, returns this filter (identity property).
    /// </para>
    /// </remarks>
    public virtual IFilter Or(IFilter other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // Annihilation: x | Always = Always
        if (other is Always)
        {
            return other;
        }

        // Identity: x | Never = x
        if (other is Never)
        {
            return this;
        }

        using (_lock.EnterScope())
        {
            if (_orCache.TryGetValue(other, out var cached))
            {
                return cached;
            }

            var result = OrList.Create([this, other]);
            _orCache[other] = result;
            return result;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This method caches the result for repeated calls.
    /// </remarks>
    public virtual IFilter Invert()
    {
        using (_lock.EnterScope())
        {
            if (_invertResult is not null)
            {
                return _invertResult;
            }

            _invertResult = new InvertFilter(this);
            return _invertResult;
        }
    }

    /// <summary>
    /// AND operator for combining filters.
    /// </summary>
    /// <param name="left">The left filter operand.</param>
    /// <param name="right">The right filter operand.</param>
    /// <returns>A filter that returns <c>true</c> only if both filters return <c>true</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
    public static IFilter operator &(Filter left, IFilter right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.And(right);
    }

    /// <summary>
    /// OR operator for combining filters.
    /// </summary>
    /// <param name="left">The left filter operand.</param>
    /// <param name="right">The right filter operand.</param>
    /// <returns>A filter that returns <c>true</c> if either filter returns <c>true</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
    public static IFilter operator |(Filter left, IFilter right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.Or(right);
    }

    /// <summary>
    /// NOT operator for negating a filter.
    /// </summary>
    /// <param name="filter">The filter to negate.</param>
    /// <returns>A filter that returns the opposite of the input filter's result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is <c>null</c>.</exception>
    public static IFilter operator ~(Filter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return filter.Invert();
    }
}
