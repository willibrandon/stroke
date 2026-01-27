# Contract: Never Filter

**Namespace**: `Stroke.Filters`
**Port of**: `prompt_toolkit.filters.base.Never`

## Class Definition

```csharp
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
/// </remarks>
public sealed class Never : Filter
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="Never"/> filter.
    /// </summary>
    public static Never Instance { get; }

    /// <summary>
    /// Initializes the static singleton instance.
    /// </summary>
    static Never();

    // Private constructor prevents external instantiation
    private Never();

    /// <inheritdoc/>
    /// <returns>Always returns <c>false</c>.</returns>
    public override bool Invoke();

    /// <inheritdoc/>
    /// <returns>Returns this <see cref="Never"/> instance (annihilation property).</returns>
    public override IFilter And(IFilter other);

    /// <inheritdoc/>
    /// <returns>Returns <paramref name="other"/> (identity property).</returns>
    public override IFilter Or(IFilter other);

    /// <inheritdoc/>
    /// <returns>Returns <see cref="Always.Instance"/>.</returns>
    public override IFilter Invert();

    /// <inheritdoc/>
    public override string ToString();
}
```

## Behavioral Contract

### Instance

- MUST be a singleton (same instance for all accesses)
- MUST be initialized before first use (static constructor)

### Invoke()

- MUST always return `false`
- MUST NOT throw exceptions
- MUST NOT have side effects

### And(other)

- `Never & x` MUST return `Never` (annihilation property for AND)
- MUST throw `ArgumentNullException` if `other` is `null`

### Or(other)

- `Never | x` MUST return `x` (identity property for OR)
- MUST throw `ArgumentNullException` if `other` is `null`

### Invert()

- `~Never` MUST return `Always.Instance`
- MUST always return the same `Always` instance

### ToString()

- MUST return `"Never"`

## Algebraic Properties

```
Never & x = Never       (annihilator for AND)
Never | x = x           (identity for OR)
~Never = Always
```
