# Research: Filter System (Core Infrastructure)

**Feature**: 017-filter-system-core
**Date**: 2026-01-26

## Overview

This research document consolidates findings for the Filter System Core Infrastructure. Since this feature has zero external dependencies and follows established patterns in both Python Prompt Toolkit and the existing Stroke codebase, no unknowns required clarification.

## Research Items

### 1. Python Prompt Toolkit Filter Implementation

**Decision**: Port `prompt_toolkit.filters.base` and `prompt_toolkit.filters.utils` exactly

**Rationale**: Constitution I requires 100% faithful port. The Python implementation is mature, well-tested, and provides the exact semantics needed.

**Alternatives considered**:
- Custom filter design → Rejected (violates Constitution I)
- Using .NET interfaces only (no abstract base) → Rejected (Python uses base class for caching)

**Key findings from Python source**:
- `Filter` is an abstract class with `__call__()` abstract method
- Caching is per-instance using `_and_cache`, `_or_cache`, `_invert_result` dictionaries
- `_AndList` and `_OrList` flatten nested combinations via `create()` classmethod
- `_remove_duplicates()` preserves order while deduplicating
- `Always` and `Never` override operators for short-circuit behavior
- `__bool__` raises `ValueError` to prevent implicit boolean conversion

### 2. Thread Safety Strategy

**Decision**: Use `System.Threading.Lock` with `EnterScope()` pattern for filter caches

**Rationale**: Constitution XI requires thread safety. The Lock pattern is already used in `FastDictCache`, `InMemoryClipboard`, and other Stroke classes.

**Alternatives considered**:
- `ConcurrentDictionary` → Rejected (doesn't match Python's simple dict semantics, adds complexity)
- No locking (immutable types) → Insufficient (cache mutation requires synchronization)
- `ReaderWriterLockSlim` → Rejected (overkill for simple cache operations)

**Key findings**:
- Each `Filter` instance has its own caches (no global state)
- Lock contention is minimal since caches are per-instance
- Cache reads and writes are fast operations
- Follow same pattern as `FastDictCache.cs`

### 3. Operator Overloading in C#

**Decision**: Use static interface members for operator overloads on `IFilter`

**Rationale**: C# 11+ supports static abstract interface members, allowing `&`, `|`, `~` operators on the interface type directly.

**Alternatives considered**:
- Extension methods → Rejected (cannot define operators)
- Only instance methods (And/Or/Invert) → Insufficient (operators are required per api-mapping.md)
- Concrete class operators only → Rejected (need operators on IFilter type per api-mapping.md)

**Key findings from api-mapping.md**:
```csharp
public interface IFilter
{
    bool Evaluate();
    IFilter And(IFilter other);
    IFilter Or(IFilter other);
    IFilter Not();

    static IFilter operator &(IFilter left, IFilter right);
    static IFilter operator |(IFilter left, IFilter right);
    static IFilter operator ~(IFilter filter);
}
```

### 4. FilterOrBool Union Type

**Decision**: Implement as a readonly struct with implicit conversion operators

**Rationale**: Follows established pattern in Stroke (see `AnyFormattedText` in FormattedText namespace)

**Alternatives considered**:
- Discriminated union record → More complex, less idiomatic C#
- Object with runtime checks → Loses type safety
- Two separate overloads everywhere → Rejected (not ergonomic)

**Key findings**:
- Python uses `FilterOrBool = Union[Filter, bool]` type alias
- C# struct with implicit conversions provides similar ergonomics
- `ToFilter()` and `IsTrue()` methods handle conversion

### 5. Caching Strategy

**Decision**: Per-instance cache dictionaries with Lock synchronization

**Rationale**: Matches Python implementation exactly. Each `Filter` instance maintains its own `_andCache`, `_orCache`, and `_invertResult`.

**Alternatives considered**:
- Global cache (static) → Rejected (violates Constitution VI - no global mutable state)
- WeakReference caching → Rejected (added complexity, Python doesn't use it)
- No caching → Rejected (FR-008 requires caching)

**Key findings**:
- Cache is Dictionary<IFilter, IFilter> keyed by the "other" filter
- Invert cache is a single nullable field (only one inversion per filter)
- Cache never expires (filters are long-lived)

### 6. Naming Conventions

**Decision**: Follow api-mapping.md exactly

**Rationale**: Constitution IX requires adherence to planning documents

**Key mappings**:
| Python | C# |
|--------|-----|
| `Filter` | `IFilter` (interface) |
| `Filter` (base) | `Filter` (abstract class) |
| `__call__()` | `Invoke()` |
| `__and__()` | `And()` / `operator &` |
| `__or__()` | `Or()` / `operator |` |
| `__invert__()` | `Invert()` / `operator ~` |
| `__bool__()` | N/A (prevent via no implicit conversion) |
| `_AndList` | `AndList` (internal) |
| `_OrList` | `OrList` (internal) |
| `_Invert` | `InvertFilter` (internal) |
| `to_filter()` | `FilterUtils.ToFilter()` |
| `is_true()` | `FilterUtils.IsTrue()` |

## Conclusion

All technical decisions align with Constitution principles and api-mapping.md. No blockers or unknowns remain. Ready for Phase 1 design.
