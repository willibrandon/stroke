# Research: Key Bindings System

**Feature**: 022-key-bindings-system
**Date**: 2026-01-27
**Status**: Complete

## Research Summary

All technical questions have been resolved through analysis of Python Prompt Toolkit source code and existing Stroke infrastructure.

---

## Research Item 1: Cache Implementation Strategy

**Question**: What caching approach should be used for binding lookups?

**Decision**: Use existing `Stroke.Core.SimpleCache<TKey, TValue>` class

**Rationale**:
- SimpleCache already exists and is thread-safe
- Supports configurable maxSize (10,000 for GetBindingsForKeys, 1,000 for GetBindingsStartingWithKeys)
- Uses LRU-style eviction (oldest key removed when size exceeded)
- Matches Python Prompt Toolkit's SimpleCache usage exactly

**Alternatives Considered**:
- `System.Runtime.Caching.MemoryCache`: Too heavyweight, requires additional dependencies
- `ConcurrentDictionary`: No size limit, could grow unbounded
- Custom implementation: Unnecessary duplication of existing code

**Source Reference**: `/Users/brandon/src/stroke/src/Stroke/Core/SimpleCache.cs`

---

## Research Item 2: Filter Integration Pattern

**Question**: How should filters be integrated with the binding system?

**Decision**: Use existing `IFilter` interface and `FilterUtils.ToFilter()` conversion

**Rationale**:
- `IFilter` interface already provides `Invoke()`, `And()`, `Or()`, `Invert()` methods
- `FilterOrBool` allows accepting both `bool` and `IFilter` in API parameters
- `FilterUtils.ToFilter()` converts `FilterOrBool` to `IFilter` (true → Always, false → Never)
- Matches Python's `to_filter()` function exactly

**Implementation Pattern**:
```csharp
// In Binding constructor:
public Binding(
    IReadOnlyList<KeyOrChar> keys,
    KeyHandlerCallable handler,
    FilterOrBool filter = default,  // default is false in struct
    FilterOrBool eager = default,
    FilterOrBool isGlobal = default,
    ...)
{
    Filter = FilterUtils.ToFilter(filter.IsFilter || filter.BoolValue ? filter : true);
    Eager = FilterUtils.ToFilter(eager);
    IsGlobal = FilterUtils.ToFilter(isGlobal);
}
```

**Source Reference**:
- `/Users/brandon/src/stroke/src/Stroke/Filters/IFilter.cs`
- `/Users/brandon/src/stroke/src/Stroke/Filters/FilterUtils.cs`

---

## Research Item 3: Key Type Representation

**Question**: How should keys be represented in binding sequences?

**Decision**: Use union type `KeyOrChar` that can hold either `Keys` enum or single `char`

**Rationale**:
- Python uses `Keys | str` union where str is a single character
- C# needs explicit union representation
- Readonly record struct provides value semantics and immutability
- Pattern matching enables clean handling

**Implementation**:
```csharp
/// <summary>
/// Represents either a Keys enum value or a single character.
/// </summary>
public readonly record struct KeyOrChar
{
    private readonly Keys? _key;
    private readonly char? _char;

    public bool IsKey => _key.HasValue;
    public bool IsChar => _char.HasValue;
    public Keys Key => _key ?? throw new InvalidOperationException();
    public char Char => _char ?? throw new InvalidOperationException();

    public static implicit operator KeyOrChar(Keys key) => new(key);
    public static implicit operator KeyOrChar(char c) => new(c);
}
```

**Alternatives Considered**:
- `object`: Type-unsafe, requires casting
- Separate overloads: Combinatorial explosion with key sequences
- String-only: Loses type safety of Keys enum

---

## Research Item 4: Never Filter Optimization

**Question**: How should bindings with `Never` filter be handled?

**Decision**: Skip storing bindings with `Never` filter (optimization from Python)

**Rationale**:
- Python code explicitly checks `if isinstance(filter, Never)` and returns identity decorator
- Bindings with Never filter can never be active, so storing them wastes memory
- This is a documented optimization in Python Prompt Toolkit

**Implementation**:
```csharp
public Func<T, T> Add<T>(...) where T : class
{
    if (filter.IsFilter && filter.FilterValue is Never)
    {
        // When filter is Never, binding will never be active
        // Don't store it - just return identity decorator
        return func => func;
    }
    // ... normal binding logic
}
```

**Source Reference**: Python `key_bindings.py` lines 282-287

---

## Research Item 5: Handler Delegate Type

**Question**: What delegate type should be used for key binding handlers?

**Decision**: Define `KeyHandlerCallable` as delegate returning `NotImplementedOrNone` or `Task<NotImplementedOrNone>`

**Rationale**:
- Python handlers return `NotImplementedOrNone | Coroutine[..., NotImplementedOrNone]`
- C# equivalent uses Task for async handlers
- `NotImplementedOrNone` already exists in Stroke.KeyBinding namespace

**Implementation**:
```csharp
/// <summary>
/// Delegate for synchronous key binding handlers.
/// </summary>
public delegate NotImplementedOrNone? KeyHandlerCallable(KeyPressEvent @event);

/// <summary>
/// Delegate for asynchronous key binding handlers.
/// </summary>
public delegate Task<NotImplementedOrNone?> AsyncKeyHandlerCallable(KeyPressEvent @event);
```

**Note**: The `Binding.Call()` method checks if result is awaitable and creates background task.

---

## Research Item 6: Version Tracking for Cache Invalidation

**Question**: How should version tracking work for cache invalidation?

**Decision**: Use incrementing integer version with composite versions for merged registries

**Rationale**:
- Python uses `Hashable` type for version (int for KeyBindings)
- Merged registries use tuple of child versions
- Version changes trigger cache rebuild in proxy classes

**Implementation**:
```csharp
// KeyBindings
private int _version = 0;
public object Version => _version;

// MergedKeyBindings
public object Version => (_lastVersion, _registries.Select(r => r.Version).ToArray());
```

**Source Reference**: Python `key_bindings.py` lines 158-165, 597-611

---

## Research Item 7: Thread Safety Pattern

**Question**: What thread safety pattern should be used?

**Decision**: Use `System.Threading.Lock` with `EnterScope()` per Constitution XI

**Rationale**:
- Constitution XI mandates thread safety for mutable classes
- `Lock.EnterScope()` provides automatic release via `using`
- Matches existing Stroke patterns (SimpleCache, Buffer, etc.)

**Implementation**:
```csharp
public sealed class KeyBindings : IKeyBindingsBase
{
    private readonly Lock _lock = new();
    private readonly List<Binding> _bindings = [];

    public void Add(...)
    {
        using (_lock.EnterScope())
        {
            _bindings.Add(binding);
            _version++;
            ClearCache();
        }
    }
}
```

---

## Research Item 8: KeyPress Record Structure

**Question**: How should KeyPress be implemented?

**Decision**: Use `readonly record struct` with optional data parameter

**Rationale**:
- Python `KeyPress` is a simple data class with key and data
- `data` defaults to key's value representation if not provided
- Record struct provides value semantics, immutability, equality

**Implementation**:
```csharp
/// <summary>
/// Represents a key press with key value and optional raw data.
/// </summary>
public readonly record struct KeyPress
{
    public KeyOrChar Key { get; }
    public string Data { get; }

    public KeyPress(KeyOrChar key, string? data = null)
    {
        Key = key;
        Data = data ?? GetDefaultData(key);
    }

    private static string GetDefaultData(KeyOrChar key) =>
        key.IsChar ? key.Char.ToString() : key.Key.ToString();
}
```

**Source Reference**: Python `key_processor.py` lines 36-61

---

## Resolved Clarifications

| Item | Resolution |
|------|------------|
| Cache implementation | Use existing SimpleCache |
| Filter integration | Use IFilter + FilterUtils.ToFilter |
| Key representation | KeyOrChar union struct |
| Never optimization | Skip storing Never-filtered bindings |
| Handler delegate | KeyHandlerCallable with NotImplementedOrNone return |
| Version tracking | Integer version, composite for merged |
| Thread safety | Lock with EnterScope |
| KeyPress structure | Readonly record struct |

---

## Next Steps

1. Proceed to Phase 1: Design & Contracts
2. Create `data-model.md` with entity definitions
3. Create API contracts in `contracts/` directory
4. Create `quickstart.md` with usage examples
