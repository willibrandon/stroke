# Contract: FilterOrBool Union Type

**Namespace**: `Stroke.Filters`
**Port of**: `prompt_toolkit.filters.base.FilterOrBool`

## Struct Definition

```csharp
/// <summary>
/// Represents either a boolean value or an <see cref="IFilter"/> instance.
/// </summary>
/// <remarks>
/// <para>
/// This union type allows APIs to accept both static boolean values and
/// dynamic filter conditions with a single parameter type.
/// </para>
/// <para>
/// Use implicit conversion from <see cref="bool"/> or <see cref="IFilter"/>
/// to create instances.
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
    public bool IsFilter { get; }

    /// <summary>
    /// Gets a value indicating whether this instance contains a boolean.
    /// </summary>
    public bool IsBool { get; }

    /// <summary>
    /// Gets the boolean value if <see cref="IsBool"/> is <c>true</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when this instance contains a filter instead of a boolean.
    /// </exception>
    public bool BoolValue { get; }

    /// <summary>
    /// Gets the filter if <see cref="IsFilter"/> is <c>true</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when this instance contains a boolean instead of a filter.
    /// </exception>
    public IFilter FilterValue { get; }

    /// <summary>
    /// Creates a new instance containing a boolean value.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    public FilterOrBool(bool value);

    /// <summary>
    /// Creates a new instance containing a filter.
    /// </summary>
    /// <param name="filter">The filter. If <c>null</c>, treated as <see cref="Never"/>.</param>
    public FilterOrBool(IFilter? filter);

    /// <summary>
    /// Implicit conversion from boolean.
    /// </summary>
    public static implicit operator FilterOrBool(bool value);

    /// <summary>
    /// Implicit conversion from filter.
    /// </summary>
    public static implicit operator FilterOrBool(Filter filter);

    /// <inheritdoc/>
    public bool Equals(FilterOrBool other);

    /// <inheritdoc/>
    public override bool Equals(object? obj);

    /// <inheritdoc/>
    public override int GetHashCode();

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(FilterOrBool left, FilterOrBool right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(FilterOrBool left, FilterOrBool right);

    /// <inheritdoc/>
    public override string ToString();
}
```

## Behavioral Contract

### Constructor (bool)

- MUST set `_isFilter` to `false`
- MUST set `_boolValue` to the provided value
- MUST set `_filter` to `null`

### Constructor (IFilter?)

- MUST set `_isFilter` to `true`
- MUST handle `null` filter as `Never.Instance`
- MUST set `_filter` to the provided filter (or `Never.Instance` if null)

### Properties

- `IsFilter` MUST return `true` only if instance contains a filter
- `IsBool` MUST return `true` only if instance contains a boolean
- `BoolValue` MUST throw `InvalidOperationException` if `IsFilter` is `true`
- `FilterValue` MUST throw `InvalidOperationException` if `IsBool` is `true`

### Implicit Conversions

- `bool` to `FilterOrBool` MUST work without explicit cast
- `Filter` (concrete) to `FilterOrBool` MUST work without explicit cast
- `IFilter` interface conversion requires explicit handling in consuming APIs

### Equality

- Two `FilterOrBool` instances are equal if:
  - Both are booleans with the same value, OR
  - Both are filters with reference equality

### ToString()

- MUST return `"true"` or `"false"` for boolean values
- MUST return the filter's `ToString()` for filter values

## Usage Examples

```csharp
// Implicit conversion from bool
FilterOrBool always = true;
FilterOrBool never = false;

// Implicit conversion from Filter
FilterOrBool dynamic = new Condition(() => IsActive);

// Use in method parameter
void Configure(FilterOrBool enabled = true)
{
    var filter = FilterUtils.ToFilter(enabled);
    // ...
}

// All these work:
Configure();                                    // Uses default (true)
Configure(true);                                // Static enable
Configure(false);                               // Static disable
Configure(new Condition(() => SomeState));      // Dynamic
Configure(Always.Instance);                     // Always filter
```

## Thread Safety

- `FilterOrBool` is a readonly struct and is inherently thread-safe
- Value semantics ensure no shared mutable state
