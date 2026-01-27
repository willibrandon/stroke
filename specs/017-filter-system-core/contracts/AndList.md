# Contract: AndList Filter (Internal)

**Namespace**: `Stroke.Filters`
**Port of**: `prompt_toolkit.filters.base._AndList`

## Class Definition

```csharp
/// <summary>
/// Internal filter representing the AND combination of multiple filters.
/// </summary>
/// <remarks>
/// <para>
/// This class is created by the <see cref="Filter.And"/> method and should not
/// be instantiated directly.
/// </para>
/// <para>
/// Evaluates to <c>true</c> only if all contained filters evaluate to <c>true</c>.
/// Uses short-circuit evaluation.
/// </para>
/// </remarks>
internal sealed class AndList : Filter
{
    private readonly IReadOnlyList<IFilter> _filters;

    /// <summary>
    /// Gets the filters in this AND combination.
    /// </summary>
    public IReadOnlyList<IFilter> Filters { get; }

    /// <summary>
    /// Initializes a new instance with the specified filters.
    /// </summary>
    /// <param name="filters">The filters to AND together.</param>
    private AndList(IReadOnlyList<IFilter> filters);

    /// <summary>
    /// Creates a new AND filter from the given filters.
    /// </summary>
    /// <param name="filters">The filters to combine.</param>
    /// <returns>
    /// A single filter if only one unique filter remains after deduplication,
    /// otherwise a new <see cref="AndList"/> containing all unique filters.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the following optimizations:
    /// <list type="bullet">
    ///   <item>Flattens nested <see cref="AndList"/> instances</item>
    ///   <item>Removes duplicate filters (preserving order)</item>
    ///   <item>Returns single filter directly if only one remains</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static IFilter Create(IEnumerable<IFilter> filters);

    /// <inheritdoc/>
    /// <returns><c>true</c> if all filters return <c>true</c>; otherwise, <c>false</c>.</returns>
    public override bool Invoke();

    /// <inheritdoc/>
    public override string ToString();
}
```

## Behavioral Contract

### Create(filters)

- MUST flatten nested `AndList` instances into a single list
- MUST remove duplicate filters while preserving order of first occurrence
- MUST return the single filter directly if only one unique filter remains
- MUST return a new `AndList` if multiple unique filters remain
- MUST NOT modify the input enumerable

### Invoke()

- MUST evaluate filters in order using short-circuit evaluation
- MUST return `false` as soon as any filter returns `false`
- MUST return `true` only if all filters return `true`
- MUST propagate exceptions from individual filter evaluations

### ToString()

- MUST return filters joined with `"&"` separator
- Example: `"Condition&Condition&Always"`

## Flattening Example

```
Input: (a & b) & c
       Where (a & b) is AndList([a, b])

Flattened: AndList([a, b, c])
```

## Deduplication Example

```
Input: a & b & a & c & b

After deduplication: AndList([a, b, c])
```

## Short-Circuit Behavior

```csharp
// If first filter returns false, remaining filters are NOT evaluated
var result = new AndList([
    new Condition(() => false),    // Returns false
    new Condition(() => throw new Exception())  // Never called
]).Invoke();  // Returns false without exception
```
