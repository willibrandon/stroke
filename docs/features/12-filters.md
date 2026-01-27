# Feature 12: Filter System (Core Infrastructure)

## Overview

Implement the core filter infrastructure for conditional enabling/disabling of features. This provides the base classes and combinators; application-specific filters are in Feature 121.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/filters/base.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/filters/utils.py`

## Public API

### IFilter Interface (Abstract Base)

```csharp
namespace Stroke.Filters;

/// <summary>
/// Base interface for any filter to activate/deactivate a feature, depending on a condition.
/// The return value of Invoke() tells if the feature should be active.
/// </summary>
public interface IFilter
{
    /// <summary>
    /// Evaluate the filter.
    /// </summary>
    bool Invoke();

    /// <summary>
    /// Chaining of filters using the & operator.
    /// </summary>
    IFilter And(IFilter other);

    /// <summary>
    /// Chaining of filters using the | operator.
    /// </summary>
    IFilter Or(IFilter other);

    /// <summary>
    /// Inverting of filter using the ~ operator.
    /// </summary>
    IFilter Invert();
}
```

### Filter Abstract Class

```csharp
namespace Stroke.Filters;

/// <summary>
/// Base class for filters with caching for combined filters.
/// </summary>
public abstract class Filter : IFilter
{
    protected Filter();

    public abstract bool Invoke();

    /// <summary>
    /// Chaining of filters using the & operator.
    /// </summary>
    public IFilter And(IFilter other);

    /// <summary>
    /// Chaining of filters using the | operator.
    /// </summary>
    public IFilter Or(IFilter other);

    /// <summary>
    /// Inverting of filter.
    /// </summary>
    public IFilter Invert();

    // Operator overloads for C# convenience
    public static IFilter operator &(Filter left, Filter right);
    public static IFilter operator |(Filter left, Filter right);
    public static IFilter operator ~(Filter filter);

    // Prevent direct bool conversion (ambiguous meaning)
    // Users must call Invoke() explicitly
}
```

### Always Class

```csharp
namespace Stroke.Filters;

/// <summary>
/// Filter that is always true.
/// </summary>
public sealed class Always : Filter
{
    public static readonly Always Instance = new();

    public override bool Invoke() => true;

    public new IFilter And(IFilter other) => other;
    public new IFilter Or(IFilter other) => this;
    public new IFilter Invert() => Never.Instance;
}
```

### Never Class

```csharp
namespace Stroke.Filters;

/// <summary>
/// Filter that is always false.
/// </summary>
public sealed class Never : Filter
{
    public static readonly Never Instance = new();

    public override bool Invoke() => false;

    public new IFilter And(IFilter other) => this;
    public new IFilter Or(IFilter other) => other;
    public new IFilter Invert() => Always.Instance;
}
```

### Condition Class

```csharp
namespace Stroke.Filters;

/// <summary>
/// Turn any callable into a Filter.
/// </summary>
public sealed class Condition : Filter
{
    /// <summary>
    /// Creates a condition from a callable.
    /// </summary>
    /// <param name="func">Callable which takes no inputs and returns a boolean.</param>
    public Condition(Func<bool> func);

    /// <summary>
    /// The underlying function.
    /// </summary>
    public Func<bool> Func { get; }

    public override bool Invoke();
    public override string ToString();
}
```

### Filter Utility Functions

```csharp
namespace Stroke.Filters;

/// <summary>
/// Filter utility functions.
/// </summary>
public static class FilterUtils
{
    /// <summary>
    /// Accept both booleans and Filters and turn it into a Filter.
    /// </summary>
    public static IFilter ToFilter(bool value);

    /// <summary>
    /// Accept both booleans and Filters and turn it into a Filter.
    /// </summary>
    public static IFilter ToFilter(IFilter filter);

    /// <summary>
    /// Accept both booleans and Filters and turn it into a Filter.
    /// </summary>
    public static IFilter ToFilter(Func<bool> func);

    /// <summary>
    /// Test whether value is true. In case of a Filter, call it.
    /// </summary>
    public static bool IsTrue(bool value);

    /// <summary>
    /// Test whether value is true. In case of a Filter, call it.
    /// </summary>
    public static bool IsTrue(IFilter filter);
}
```

## Project Structure

```
src/Stroke/
└── Filters/
    ├── IFilter.cs
    ├── Filter.cs
    ├── Always.cs
    ├── Never.cs
    ├── Condition.cs
    ├── AndList.cs (internal)
    ├── OrList.cs (internal)
    ├── Invert.cs (internal)
    └── FilterUtils.cs
tests/Stroke.Tests/
└── Filters/
    ├── FilterTests.cs
    ├── AlwaysNeverTests.cs
    ├── ConditionTests.cs
    ├── FilterCombinationTests.cs
    └── FilterUtilsTests.cs
```

## Implementation Notes

### Filter Caching

The `Filter` base class caches combined filters to avoid repeated allocations:
- `_andCache`: Dictionary mapping other filter to combined AND result
- `_orCache`: Dictionary mapping other filter to combined OR result
- `_invertResult`: Cached inverted filter

### Short-Circuit Optimization

The `Always` and `Never` classes override combination methods for short-circuit optimization:
- `Always & x` → `x`
- `Always | x` → `Always`
- `Never & x` → `Never`
- `Never | x` → `x`

### AndList and OrList Flattening

When combining multiple filters, nested `_AndList` and `_OrList` instances are flattened:
- `(a & b) & c` → `_AndList([a, b, c])` (not nested)
- Duplicate filters are removed for efficiency

### Bool Conversion Prevention

The Python implementation raises an error on bool conversion to prevent ambiguous usage. In C#, we achieve this by not implementing implicit conversion to bool and requiring explicit `Invoke()` calls.

### Lazy Evaluation

Filters are lazy - they only compute their value when `Invoke()` is called. Combined filters evaluate their component filters on each call.

## Dependencies

None. This is the core filter infrastructure with no external dependencies.

## Implementation Tasks

1. Implement `IFilter` interface
2. Implement `Filter` base class with caching
3. Implement `Always` and `Never` singletons
4. Implement `Condition` class
5. Implement internal `_AndList`, `_OrList`, `_Invert` classes
6. Implement `FilterUtils` static class
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All filter types match Python Prompt Toolkit semantics
- [ ] Filter combination caching works correctly
- [ ] Short-circuit optimization works correctly
- [ ] Unit tests achieve 80% coverage

## Related Features

- Feature 121: Application Filters (app-specific filters that use this infrastructure)
