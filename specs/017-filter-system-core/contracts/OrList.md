# Contract: OrList Filter (Internal)

**Namespace**: `Stroke.Filters`
**Port of**: `prompt_toolkit.filters.base._OrList`

## Class Definition

```csharp
/// <summary>
/// Internal filter representing the OR combination of multiple filters.
/// </summary>
/// <remarks>
/// <para>
/// This class is created by the <see cref="Filter.Or"/> method and should not
/// be instantiated directly.
/// </para>
/// <para>
/// Evaluates to <c>true</c> if any contained filter evaluates to <c>true</c>.
/// Uses short-circuit evaluation.
/// </para>
/// </remarks>
internal sealed class OrList : Filter
{
    private readonly IReadOnlyList<IFilter> _filters;

    /// <summary>
    /// Gets the filters in this OR combination.
    /// </summary>
    public IReadOnlyList<IFilter> Filters { get; }

    /// <summary>
    /// Initializes a new instance with the specified filters.
    /// </summary>
    /// <param name="filters">The filters to OR together.</param>
    private OrList(IReadOnlyList<IFilter> filters);

    /// <summary>
    /// Creates a new OR filter from the given filters.
    /// </summary>
    /// <param name="filters">The filters to combine.</param>
    /// <returns>
    /// A single filter if only one unique filter remains after deduplication,
    /// otherwise a new <see cref="OrList"/> containing all unique filters.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the following optimizations:
    /// <list type="bullet">
    ///   <item>Flattens nested <see cref="OrList"/> instances</item>
    ///   <item>Removes duplicate filters (preserving order)</item>
    ///   <item>Returns single filter directly if only one remains</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static IFilter Create(IEnumerable<IFilter> filters);

    /// <inheritdoc/>
    /// <returns><c>true</c> if any filter returns <c>true</c>; otherwise, <c>false</c>.</returns>
    public override bool Invoke();

    /// <inheritdoc/>
    public override string ToString();
}
```

## Behavioral Contract

### Create(filters)

- MUST flatten nested `OrList` instances into a single list
- MUST remove duplicate filters while preserving order of first occurrence
- MUST return the single filter directly if only one unique filter remains
- MUST return a new `OrList` if multiple unique filters remain
- MUST NOT modify the input enumerable

### Invoke()

- MUST evaluate filters in order using short-circuit evaluation
- MUST return `true` as soon as any filter returns `true`
- MUST return `false` only if all filters return `false`
- MUST propagate exceptions from individual filter evaluations

### ToString()

- MUST return filters joined with `"|"` separator
- Example: `"Condition|Condition|Never"`

## Flattening Example

```
Input: (a | b) | c
       Where (a | b) is OrList([a, b])

Flattened: OrList([a, b, c])
```

## Deduplication Example

```
Input: a | b | a | c | b

After deduplication: OrList([a, b, c])
```

## Short-Circuit Behavior

```csharp
// If first filter returns true, remaining filters are NOT evaluated
var result = new OrList([
    new Condition(() => true),     // Returns true
    new Condition(() => throw new Exception())  // Never called
]).Invoke();  // Returns true without exception
```
