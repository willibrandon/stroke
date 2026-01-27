# Quickstart: Filter System (Core Infrastructure)

**Feature**: 017-filter-system-core
**Date**: 2026-01-26

## Overview

The Filter System provides composable boolean conditions for controlling feature activation. Filters can be combined using boolean operators (`&`, `|`, `~`) to create complex conditional expressions.

## Quick Examples

### Basic Filter Usage

```csharp
using Stroke.Filters;

// Create a dynamic condition
var isActive = new Condition(() => _state.IsActive);

// Evaluate the filter
bool active = isActive.Invoke();  // true or false based on _state.IsActive

// Use constant filters
bool always = Always.Instance.Invoke();  // Always true
bool never = Never.Instance.Invoke();    // Always false
```

### Combining Filters

```csharp
// AND combination - both must be true
var canEdit = new Condition(() => _hasFocus) & new Condition(() => !_isReadOnly);

// OR combination - either can be true
var showHelp = new Condition(() => _isNewUser) | new Condition(() => _helpRequested);

// Negation
var notSearching = ~new Condition(() => _isSearching);

// Complex combinations (cast to Filter when chaining)
var shouldActivate = (Filter)((Filter)canEdit | showHelp) & notSearching;
```

### Using FilterOrBool in APIs

```csharp
// API that accepts both booleans and filters
public void SetVisibility(FilterOrBool visible)
{
    var filter = FilterUtils.ToFilter(visible);
    // Use filter...
}

// Call with boolean
SetVisibility(true);
SetVisibility(false);

// Call with filter
SetVisibility(new Condition(() => _isVisible));
SetVisibility(Always.Instance);

// Quick evaluation
bool shouldShow = FilterUtils.IsTrue(visible);
```

## Key Types

| Type | Description |
|------|-------------|
| `IFilter` | Interface for all filters with `Invoke()`, `And()`, `Or()`, `Invert()` |
| `Filter` | Abstract base class with caching and operators |
| `Always` | Singleton that always returns `true` |
| `Never` | Singleton that always returns `false` |
| `Condition` | Wraps a `Func<bool>` for dynamic evaluation |
| `FilterOrBool` | Union type accepting `bool` or `IFilter` |
| `FilterUtils` | Static utilities: `ToFilter()`, `IsTrue()` |

## Operators

| Operator | Method | Description |
|----------|--------|-------------|
| `a & b` | `a.And(b)` | Returns `true` only if both are `true` |
| `a \| b` | `a.Or(b)` | Returns `true` if either is `true` |
| `~a` | `a.Invert()` | Returns opposite of `a` |

> **Note**: Operators are defined on the `Filter` class, not the `IFilter` interface (C# doesn't support operators on interfaces). When chaining operators, the result is `IFilter`, so you may need to cast to `Filter` for subsequent operations: `(Filter)(a & b) & c`.

## Algebraic Properties

```csharp
// Always is identity for AND
Always.Instance & x  ==  x

// Never is identity for OR
Never.Instance | x  ==  x

// Always is annihilator for OR
Always.Instance | x  ==  Always.Instance

// Never is annihilator for AND
Never.Instance & x  ==  Never.Instance

// Double negation
~~x.Invoke()  ==  x.Invoke()

// Negation of constants
~Always.Instance  ==  Never.Instance
~Never.Instance   ==  Always.Instance
```

## Performance Characteristics

- **Caching**: Repeated combinations return cached instances
- **Lazy Evaluation**: Filters evaluate only when `Invoke()` is called
- **Short-Circuit**: AND stops at first `false`, OR stops at first `true`
- **Flattening**: Nested AND/OR combinations flatten into single lists
- **Deduplication**: Duplicate filters are removed from combinations

## Thread Safety

All filter types are thread-safe:
- Filter instances are immutable after construction
- Caches use `Lock` for synchronization
- `Condition` wrapper is thread-safe, but the wrapped `Func<bool>` must be thread-safe if it accesses shared state

## Common Patterns

### Feature Flags

```csharp
var featureEnabled = new Condition(() => Config.EnableNewFeature);
if (featureEnabled.Invoke())
{
    // Use new feature
}
```

### Conditional Key Bindings (Preview)

```csharp
// This pattern will be used in keybinding system
keyBindings.Add(
    keys: [Keys.ControlC],
    handler: HandleCopy,
    filter: new Condition(() => HasSelection)  // Only active when text selected
);
```

### Combining Application State

```csharp
// Cast to Filter when chaining multiple operators
var canSave =
    (Filter)(new Condition(() => _document.IsDirty) &
    new Condition(() => !_document.IsReadOnly)) &
    new Condition(() => _hasWritePermission);
```

## Related Documentation

- [IFilter Contract](contracts/IFilter.md)
- [Data Model](data-model.md)
- [Full Specification](spec.md)
