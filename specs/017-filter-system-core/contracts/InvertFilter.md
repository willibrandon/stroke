# Contract: InvertFilter (Internal)

**Namespace**: `Stroke.Filters`
**Port of**: `prompt_toolkit.filters.base._Invert`

## Class Definition

```csharp
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
/// </remarks>
internal sealed class InvertFilter : Filter
{
    private readonly IFilter _filter;

    /// <summary>
    /// Gets the filter being negated.
    /// </summary>
    public IFilter InnerFilter { get; }

    /// <summary>
    /// Initializes a new instance wrapping the specified filter.
    /// </summary>
    /// <param name="filter">The filter to negate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is <c>null</c>.</exception>
    internal InvertFilter(IFilter filter);

    /// <inheritdoc/>
    /// <returns>The negation of the wrapped filter's result.</returns>
    public override bool Invoke();

    /// <inheritdoc/>
    public override string ToString();
}
```

## Behavioral Contract

### Constructor

- MUST throw `ArgumentNullException` if `filter` is `null`
- MUST store the filter reference

### Invoke()

- MUST return `!_filter.Invoke()`
- MUST NOT catch or suppress exceptions from the wrapped filter

### ToString()

- MUST return `"~"` followed by the wrapped filter's string representation
- Example: `"~Condition"` or `"~Always"`

## Double Negation Handling

Double negation is handled through caching in the base `Filter` class:

```csharp
// First inversion
var inverted = filter.Invert();  // Creates InvertFilter, caches it

// Second inversion of the inverted filter
var doubleInverted = inverted.Invert();  // Creates InvertFilter wrapping inverted

// When evaluated:
// doubleInverted.Invoke()
// = !inverted.Invoke()
// = !(!filter.Invoke())
// = filter.Invoke()
```

The behavior produces the original result, though not the same instance. This matches Python Prompt Toolkit behavior.

## Special Cases

- `Always.Invert()` returns `Never.Instance` directly (not InvertFilter)
- `Never.Invert()` returns `Always.Instance` directly (not InvertFilter)
- These optimizations are handled in the `Always` and `Never` classes
