# Contract: Always Filter

**Namespace**: `Stroke.Filters`
**Port of**: `prompt_toolkit.filters.base.Always`

## Class Definition

```csharp
/// <summary>
/// Filter that always returns <c>true</c>.
/// </summary>
/// <remarks>
/// <para>
/// This filter serves as the identity element for AND operations and the
/// annihilator for OR operations in filter algebra.
/// </para>
/// <para>
/// Use <see cref="Instance"/> to get the singleton instance.
/// </para>
/// </remarks>
public sealed class Always : Filter
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="Always"/> filter.
    /// </summary>
    public static Always Instance { get; }

    /// <summary>
    /// Initializes the static singleton instance.
    /// </summary>
    static Always();

    // Private constructor prevents external instantiation
    private Always();

    /// <inheritdoc/>
    /// <returns>Always returns <c>true</c>.</returns>
    public override bool Invoke();

    /// <inheritdoc/>
    /// <returns>Returns <paramref name="other"/> (identity property).</returns>
    public override IFilter And(IFilter other);

    /// <inheritdoc/>
    /// <returns>Returns this <see cref="Always"/> instance (annihilation property).</returns>
    public override IFilter Or(IFilter other);

    /// <inheritdoc/>
    /// <returns>Returns <see cref="Never.Instance"/>.</returns>
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

- MUST always return `true`
- MUST NOT throw exceptions
- MUST NOT have side effects

### And(other)

- `Always & x` MUST return `x` (identity property for AND)
- MUST throw `ArgumentNullException` if `other` is `null`

### Or(other)

- `Always | x` MUST return `Always` (annihilation property for OR)
- MUST throw `ArgumentNullException` if `other` is `null`

### Invert()

- `~Always` MUST return `Never.Instance`
- MUST always return the same `Never` instance

### ToString()

- MUST return `"Always"`

## Algebraic Properties

```
Always & x = x          (identity for AND)
Always | x = Always     (annihilator for OR)
~Always = Never
```
