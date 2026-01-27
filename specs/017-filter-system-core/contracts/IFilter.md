# Contract: IFilter Interface

**Namespace**: `Stroke.Filters`
**Port of**: `prompt_toolkit.filters.base.Filter`

## Interface Definition

```csharp
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
/// This is a faithful port of Python Prompt Toolkit's <c>Filter</c> abstract base class.
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

    /// <summary>
    /// AND operator for combining filters.
    /// </summary>
    static abstract IFilter operator &(IFilter left, IFilter right);

    /// <summary>
    /// OR operator for combining filters.
    /// </summary>
    static abstract IFilter operator |(IFilter left, IFilter right);

    /// <summary>
    /// NOT operator for negating a filter.
    /// </summary>
    static abstract IFilter operator ~(IFilter filter);
}
```

## Behavioral Contract

### Invoke()

- MUST return a boolean value
- MUST NOT throw exceptions (except from user-provided callables in `Condition`)
- MAY be called multiple times with potentially different results (dynamic evaluation)

### And(other)

- MUST return `other` when `this` is `Always`
- MUST return `Never` when `this` is `Never`
- MUST cache results for repeated calls with same `other`
- MUST flatten nested AND combinations
- MUST remove duplicate filters

### Or(other)

- MUST return `Always` when `this` is `Always`
- MUST return `other` when `this` is `Never`
- MUST cache results for repeated calls with same `other`
- MUST flatten nested OR combinations
- MUST remove duplicate filters

### Invert()

- MUST cache result for repeated calls
- Double inversion MUST produce original filter behavior
- `~Always` MUST return `Never`
- `~Never` MUST return `Always`

### Operators

- `a & b` MUST be equivalent to `a.And(b)`
- `a | b` MUST be equivalent to `a.Or(b)`
- `~a` MUST be equivalent to `a.Invert()`
