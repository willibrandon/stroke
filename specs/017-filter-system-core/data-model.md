# Data Model: Filter System (Core Infrastructure)

**Feature**: 017-filter-system-core
**Date**: 2026-01-26

## Entity Overview

The filter system consists of immutable filter types that evaluate boolean conditions at runtime. Filters can be combined using AND, OR, and NOT operations to create complex conditional expressions.

## Core Entities

### IFilter (Interface)

The contract for all filter implementations.

| Member | Type | Description |
|--------|------|-------------|
| `Invoke()` | Method → `bool` | Evaluates the filter and returns result |
| `And(other)` | Method → `IFilter` | Returns a filter representing `this AND other` |
| `Or(other)` | Method → `IFilter` | Returns a filter representing `this OR other` |
| `Invert()` | Method → `IFilter` | Returns a filter representing `NOT this` |

**Operators**:
- `operator &` - AND combination
- `operator |` - OR combination
- `operator ~` - Negation

**Validation Rules**:
- `other` parameter must not be null (ArgumentNullException)
- Operators delegate to instance methods

### Filter (Abstract Base Class)

Implements caching and common behavior for all filters.

| Field | Type | Description |
|-------|------|-------------|
| `_lock` | `Lock` | Thread synchronization for cache access |
| `_andCache` | `Dictionary<IFilter, IFilter>` | Cached AND combinations |
| `_orCache` | `Dictionary<IFilter, IFilter>` | Cached OR combinations |
| `_invertResult` | `IFilter?` | Cached inversion result |

**State**:
- Caches start empty and grow as combinations are created
- Caches are never cleared (filters are long-lived)

### Always (Singleton)

Filter that always evaluates to `true`.

| Member | Type | Description |
|--------|------|-------------|
| `Instance` | Static property | Singleton instance |
| `Invoke()` | Method | Always returns `true` |

**Algebraic Properties**:
- `Always & x` → `x` (identity for AND)
- `Always | x` → `Always` (annihilator for OR)
- `~Always` → `Never`

### Never (Singleton)

Filter that always evaluates to `false`.

| Member | Type | Description |
|--------|------|-------------|
| `Instance` | Static property | Singleton instance |
| `Invoke()` | Method | Always returns `false` |

**Algebraic Properties**:
- `Never & x` → `Never` (annihilator for AND)
- `Never | x` → `x` (identity for OR)
- `~Never` → `Always`

### Condition

Filter wrapping a `Func<bool>` callable.

| Field | Type | Description |
|-------|------|-------------|
| `_func` | `Func<bool>` | The callable to evaluate |

**Validation Rules**:
- `func` must not be null (ArgumentNullException in constructor)
- Exceptions from `func` propagate to caller (not swallowed)

### AndList (Internal)

Represents AND combination of multiple filters.

| Field | Type | Description |
|-------|------|-------------|
| `_filters` | `IReadOnlyList<IFilter>` | Filters to AND together |

**Factory Behavior** (`Create` method):
1. Flatten nested `AndList` instances
2. Remove duplicate filters (preserve order)
3. If single filter remains, return it directly
4. Otherwise, create new `AndList`

**Evaluation**: `all(f.Invoke() for f in _filters)` with short-circuit

### OrList (Internal)

Represents OR combination of multiple filters.

| Field | Type | Description |
|-------|------|-------------|
| `_filters` | `IReadOnlyList<IFilter>` | Filters to OR together |

**Factory Behavior** (`Create` method):
1. Flatten nested `OrList` instances
2. Remove duplicate filters (preserve order)
3. If single filter remains, return it directly
4. Otherwise, create new `OrList`

**Evaluation**: `any(f.Invoke() for f in _filters)` with short-circuit

### InvertFilter (Internal)

Represents negation of another filter.

| Field | Type | Description |
|-------|------|-------------|
| `_filter` | `IFilter` | The filter to negate |

**Evaluation**: `!_filter.Invoke()`

### FilterOrBool (Union Type Struct)

Represents either a filter or a boolean value.

| Field | Type | Description |
|-------|------|-------------|
| `_filter` | `IFilter?` | Filter value (if not boolean) |
| `_boolValue` | `bool` | Boolean value (if not filter) |
| `_isFilter` | `bool` | True if holds a filter |

**Implicit Conversions**:
- `bool` → `FilterOrBool`
- `IFilter` → `FilterOrBool`

**Validation Rules**:
- Null filters converted to `Never`

## Utility Functions

### FilterUtils (Static Class)

| Method | Signature | Description |
|--------|-----------|-------------|
| `ToFilter` | `IFilter ToFilter(FilterOrBool value)` | Converts bool/filter to IFilter |
| `IsTrue` | `bool IsTrue(FilterOrBool value)` | Evaluates filter-or-bool to boolean |

**ToFilter Behavior**:
- `true` → `Always.Instance`
- `false` → `Never.Instance`
- `IFilter` → same instance

**IsTrue Behavior**:
- Calls `ToFilter(value).Invoke()`

## Relationships

```
IFilter (interface)
    │
    ├── Filter (abstract base)
    │       │
    │       ├── Always (singleton)
    │       ├── Never (singleton)
    │       ├── Condition (Func<bool> wrapper)
    │       ├── AndList (internal, combines multiple)
    │       ├── OrList (internal, combines multiple)
    │       └── InvertFilter (internal, negates one)
    │
    └── FilterOrBool (union struct, converts to IFilter)

FilterUtils ──uses──▶ IFilter, Always, Never, FilterOrBool
```

## Thread Safety

All filter types are thread-safe:

| Type | Thread Safety Mechanism |
|------|------------------------|
| `IFilter` | Interface (no state) |
| `Filter` | Lock for cache access |
| `Always` | Immutable singleton |
| `Never` | Immutable singleton |
| `Condition` | Immutable (func is readonly) |
| `AndList` | Immutable (list is readonly) |
| `OrList` | Immutable (list is readonly) |
| `InvertFilter` | Immutable (filter is readonly) |
| `FilterOrBool` | Immutable struct |
| `FilterUtils` | Stateless static methods |

**Note**: The `Func<bool>` in `Condition` may access mutable state. Thread safety of that state is the caller's responsibility.

## State Transitions

Filters are immutable after construction. No state transitions occur.

Cache state grows monotonically as combinations are created:
1. Empty cache → First combination creates cache entry
2. Cache lookup hit → Return cached instance
3. Cache lookup miss → Create new filter, cache it, return it

Caches are never cleared during filter lifetime.
