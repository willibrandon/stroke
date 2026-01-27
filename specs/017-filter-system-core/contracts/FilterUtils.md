# Contract: FilterUtils Static Class

**Namespace**: `Stroke.Filters`
**Port of**: `prompt_toolkit.filters.utils`

## Class Definition

```csharp
/// <summary>
/// Utility methods for working with filters and boolean values.
/// </summary>
/// <remarks>
/// <para>
/// Provides conversion between <see cref="FilterOrBool"/> values and
/// <see cref="IFilter"/> instances.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>to_filter</c> and
/// <c>is_true</c> functions.
/// </para>
/// </remarks>
public static class FilterUtils
{
    /// <summary>
    /// Converts a <see cref="FilterOrBool"/> value to an <see cref="IFilter"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>
    /// <see cref="Always.Instance"/> if <paramref name="value"/> is <c>true</c>,
    /// <see cref="Never.Instance"/> if <paramref name="value"/> is <c>false</c>,
    /// or the filter itself if <paramref name="value"/> contains a filter.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> contains an invalid type.
    /// </exception>
    public static IFilter ToFilter(FilterOrBool value);

    /// <summary>
    /// Evaluates a <see cref="FilterOrBool"/> value to a boolean.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <returns>
    /// The boolean result of evaluating the value.
    /// If <paramref name="value"/> is a boolean, returns it directly.
    /// If <paramref name="value"/> is a filter, returns <c>filter.Invoke()</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> contains an invalid type.
    /// </exception>
    public static bool IsTrue(FilterOrBool value);
}
```

## Behavioral Contract

### ToFilter(value)

| Input | Output |
|-------|--------|
| `true` | `Always.Instance` |
| `false` | `Never.Instance` |
| `IFilter instance` | Same instance (unchanged) |

- MUST return singleton instances for booleans (no new allocations)
- MUST return the same filter instance if a filter is provided
- MUST NOT invoke the filter (just return it)

### IsTrue(value)

| Input | Output |
|-------|--------|
| `true` | `true` |
| `false` | `false` |
| `Always.Instance` | `true` |
| `Never.Instance` | `false` |
| Any `IFilter` | `filter.Invoke()` |

- MUST be equivalent to `ToFilter(value).Invoke()`
- MUST propagate exceptions from filter evaluation

## Usage Examples

```csharp
// Accept bool or filter in API
public void EnableFeature(FilterOrBool condition = true)
{
    var filter = FilterUtils.ToFilter(condition);
    // Use filter...
}

// Evaluate condition immediately
public bool ShouldShow(FilterOrBool visibility)
{
    return FilterUtils.IsTrue(visibility);
}

// Method accepts either
EnableFeature(true);                           // Static enable
EnableFeature(false);                          // Static disable
EnableFeature(new Condition(() => IsActive));  // Dynamic
```

## Thread Safety

- `FilterUtils` is stateless and thread-safe
- Methods delegate to `Always.Instance`, `Never.Instance`, and `IFilter.Invoke()`
- Thread safety of filter evaluation depends on the specific filter implementation
