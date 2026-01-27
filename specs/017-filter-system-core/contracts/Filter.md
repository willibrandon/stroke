# Contract: Filter Abstract Base Class

**Namespace**: `Stroke.Filters`
**Port of**: `prompt_toolkit.filters.base.Filter`

## Class Definition

```csharp
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
/// </remarks>
public abstract class Filter : IFilter
{
    private readonly Lock _lock;
    private readonly Dictionary<IFilter, IFilter> _andCache;
    private readonly Dictionary<IFilter, IFilter> _orCache;
    private IFilter? _invertResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="Filter"/> class.
    /// </summary>
    protected Filter();

    /// <inheritdoc/>
    public abstract bool Invoke();

    /// <inheritdoc/>
    public virtual IFilter And(IFilter other);

    /// <inheritdoc/>
    public virtual IFilter Or(IFilter other);

    /// <inheritdoc/>
    public virtual IFilter Invert();

    /// <summary>
    /// AND operator for combining filters.
    /// </summary>
    public static IFilter operator &(Filter left, IFilter right);

    /// <summary>
    /// OR operator for combining filters.
    /// </summary>
    public static IFilter operator |(Filter left, IFilter right);

    /// <summary>
    /// NOT operator for negating a filter.
    /// </summary>
    public static IFilter operator ~(Filter filter);
}
```

## Behavioral Contract

### Constructor

- MUST initialize empty `_andCache` and `_orCache` dictionaries
- MUST initialize `_invertResult` to `null`
- MUST create a new `Lock` instance

### And(other)

- MUST throw `ArgumentNullException` if `other` is `null`
- MUST check if `other` is `Always` and return `this`
- MUST check if `other` is `Never` and return `other`
- MUST check cache before creating new combination
- MUST cache new combinations
- MUST be thread-safe

### Or(other)

- MUST throw `ArgumentNullException` if `other` is `null`
- MUST check if `other` is `Always` and return `other`
- MUST check if `other` is `Never` and return `this`
- MUST check cache before creating new combination
- MUST cache new combinations
- MUST be thread-safe

### Invert()

- MUST check cache before creating new inversion
- MUST cache new inversions
- MUST be thread-safe

### Thread Safety

- All cache reads and writes MUST be protected by `_lock`
- Lock scope MUST use `EnterScope()` pattern for automatic release
- Factory method calls (AndList.Create, OrList.Create) MAY occur inside lock

## Implementation Notes

```csharp
// Example implementation of And():
public virtual IFilter And(IFilter other)
{
    ArgumentNullException.ThrowIfNull(other);

    if (other is Always)
        return this;
    if (other is Never)
        return other;

    using (_lock.EnterScope())
    {
        if (_andCache.TryGetValue(other, out var cached))
            return cached;

        var result = AndList.Create([this, other]);
        _andCache[other] = result;
        return result;
    }
}
```
