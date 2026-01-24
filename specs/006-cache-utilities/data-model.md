# Data Model: Cache Utilities

**Feature**: 006-cache-utilities
**Date**: 2026-01-23

## Entities

### SimpleCache<TKey, TValue>

A basic FIFO cache where the cache key can differ from factory function arguments.

```csharp
namespace Stroke.Core;

/// <summary>
/// Very simple cache that discards the oldest item when the cache size is exceeded.
/// </summary>
/// <remarks>
/// <para>This type is thread-safe. Individual operations are atomic.</para>
/// <para>Port of Python Prompt Toolkit's <c>SimpleCache</c> class from <c>cache.py</c>.</para>
/// </remarks>
/// <typeparam name="TKey">The type of cache keys. Must be non-null and implement proper equality.</typeparam>
/// <typeparam name="TValue">The type of cached values.</typeparam>
public sealed class SimpleCache<TKey, TValue> where TKey : notnull
{
    // Internal storage
    private readonly Lock _lock = new();
    private readonly Dictionary<TKey, TValue> _data;
    private readonly Queue<TKey> _keys;

    // Properties
    public int MaxSize { get; }  // Read-only, set at construction

    // Constructor
    public SimpleCache(int maxSize = 8);

    // Methods
    public TValue Get(TKey key, Func<TValue> getter);
    public void Clear();
}
```

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `_lock` | `Lock` | Synchronization primitive for thread safety |
| `_data` | `Dictionary<TKey, TValue>` | Stores cached key-value pairs |
| `_keys` | `Queue<TKey>` | Tracks insertion order for FIFO eviction |

**Properties**:
| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `MaxSize` | `int` | Read-only | Maximum cache capacity |

**Methods**:
| Method | Signature | Description |
|--------|-----------|-------------|
| `Get` | `TValue Get(TKey key, Func<TValue> getter)` | Returns cached value or invokes getter and caches result |
| `Clear` | `void Clear()` | Removes all cached entries |

**Validation Rules**:
- `maxSize` must be > 0 (enforced in constructor)
- `key` must not be null (enforced by `where TKey : notnull`)
- `getter` must not be null (enforced in Get method)

**State Transitions**:
1. Empty → Populated: First `Get` call adds entry
2. Populated → At Capacity: Additional `Get` calls until `Count == MaxSize`
3. At Capacity → Eviction: Next `Get` with new key evicts oldest entry

---

### FastDictCache<TKey, TValue>

A high-performance cache optimized for scenarios where the key IS the factory arguments.

```csharp
namespace Stroke.Core;

/// <summary>
/// Fast, lightweight cache which keeps at most <see cref="Size"/> items.
/// It will discard the oldest items in the cache first (FIFO eviction).
/// </summary>
/// <remarks>
/// <para>
/// The cache provides dictionary-style indexer access. Accessing a missing key
/// automatically invokes the factory function to create and cache the value.
/// </para>
/// <para>This type is thread-safe. Individual operations are atomic.</para>
/// <para>Port of Python Prompt Toolkit's <c>FastDictCache</c> class from <c>cache.py</c>.</para>
/// </remarks>
/// <typeparam name="TKey">The type of cache keys. Must be non-null and implement proper equality.</typeparam>
/// <typeparam name="TValue">The type of cached values.</typeparam>
public sealed class FastDictCache<TKey, TValue> where TKey : notnull
{
    // Internal storage
    private readonly Lock _lock = new();
    private readonly Dictionary<TKey, TValue> _data;
    private readonly Queue<TKey> _keys;
    private readonly Func<TKey, TValue> _getValue;

    // Properties
    public int Size { get; }   // Maximum capacity
    public int Count { get; }  // Current entry count

    // Constructor
    public FastDictCache(Func<TKey, TValue> getValue, int size = 1_000_000);

    // Indexer
    public TValue this[TKey key] { get; }  // Read-only indexer, auto-populates

    // Methods
    public bool ContainsKey(TKey key);
    public bool TryGetValue(TKey key, out TValue value);
}
```

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `_lock` | `Lock` | Synchronization primitive for thread safety |
| `_data` | `Dictionary<TKey, TValue>` | Stores cached key-value pairs |
| `_keys` | `Queue<TKey>` | Tracks insertion order for FIFO eviction |
| `_getValue` | `Func<TKey, TValue>` | Factory function for missing keys |

**Properties**:
| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `Size` | `int` | Read-only | Maximum cache capacity |
| `Count` | `int` | Read-only | Current number of cached entries |

**Indexer**:
| Indexer | Signature | Description |
|---------|-----------|-------------|
| `this[]` | `TValue this[TKey key] { get; }` | Returns cached value, auto-creates if missing |

**Methods**:
| Method | Signature | Description |
|--------|-----------|-------------|
| `ContainsKey` | `bool ContainsKey(TKey key)` | Checks if key exists without invoking factory |
| `TryGetValue` | `bool TryGetValue(TKey key, out TValue value)` | Gets value if exists without invoking factory |

**Validation Rules**:
- `size` must be > 0 (enforced in constructor)
- `getValue` must not be null (enforced in constructor)
- `key` must not be null (enforced by `where TKey : notnull`)

**State Transitions**:
1. Empty → Populated: First indexer access adds entry
2. Populated → At Capacity: Additional accesses until `Count == Size`
3. At Capacity → Eviction: Next access with new key evicts oldest BEFORE adding new

---

### Memoization

Static utility class providing function wrappers that automatically cache results.

```csharp
namespace Stroke.Core;

/// <summary>
/// Provides memoization utilities for caching function results.
/// </summary>
/// <remarks>
/// <para>Memoized functions cache results based on argument values.</para>
/// <para>The returned wrapper is thread-safe (uses SimpleCache which is thread-safe).</para>
/// <para>Port of Python Prompt Toolkit's <c>memoized</c> decorator from <c>cache.py</c>.</para>
/// </remarks>
public static class Memoization
{
    // Factory methods for 1, 2, and 3 argument functions
    public static Func<T1, TResult> Memoize<T1, TResult>(
        Func<T1, TResult> func,
        int maxSize = 1024) where T1 : notnull;

    public static Func<T1, T2, TResult> Memoize<T1, T2, TResult>(
        Func<T1, T2, TResult> func,
        int maxSize = 1024) where T1 : notnull where T2 : notnull;

    public static Func<T1, T2, T3, TResult> Memoize<T1, T2, T3, TResult>(
        Func<T1, T2, T3, TResult> func,
        int maxSize = 1024) where T1 : notnull where T2 : notnull where T3 : notnull;
}
```

**Methods**:
| Method | Signature | Description |
|--------|-----------|-------------|
| `Memoize<T1, TResult>` | `Func<T1, TResult> Memoize(Func<T1, TResult> func, int maxSize = 1024)` | Wraps single-arg function |
| `Memoize<T1, T2, TResult>` | `Func<T1, T2, TResult> Memoize(Func<T1, T2, TResult> func, int maxSize = 1024)` | Wraps two-arg function |
| `Memoize<T1, T2, T3, TResult>` | `Func<T1, T2, T3, TResult> Memoize(Func<T1, T2, T3, TResult> func, int maxSize = 1024)` | Wraps three-arg function |

**Validation Rules**:
- `func` must not be null (enforced in each method)
- `maxSize` must be > 0 (enforced via SimpleCache constructor)

**Internal Implementation**:
- Uses `SimpleCache<TKey, TResult>` internally
- Cache key for single arg: `T1` directly
- Cache key for multi args: `ValueTuple<T1, T2>` or `ValueTuple<T1, T2, T3>`

---

## Relationships

```text
┌─────────────────────────────────────────────────────────────┐
│                      Memoization                             │
│                   (static utility)                           │
│                                                              │
│  Memoize<T1, TResult>() ─────────────────────────────────┐  │
│  Memoize<T1, T2, TResult>() ─────────────────────────────┼──┼─► uses
│  Memoize<T1, T2, T3, TResult>() ─────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   SimpleCache<TKey, TValue>                  │
│                                                              │
│  MaxSize: int                                                │
│  _data: Dictionary<TKey, TValue>                            │
│  _keys: Queue<TKey>                                         │
│                                                              │
│  Get(key, getter) → TValue                                  │
│  Clear()                                                     │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                 FastDictCache<TKey, TValue>                  │
│                     (independent)                            │
│                                                              │
│  Size: int                                                   │
│  Count: int                                                  │
│  _data: Dictionary<TKey, TValue>                            │
│  _keys: Queue<TKey>                                         │
│  _getValue: Func<TKey, TValue>                              │
│                                                              │
│  this[key] → TValue (auto-populates)                        │
│  ContainsKey(key) → bool                                    │
│  TryGetValue(key, out value) → bool                         │
└─────────────────────────────────────────────────────────────┘
```

- `Memoization` **uses** `SimpleCache` for all memoized functions
- `FastDictCache` is **independent** (does not use SimpleCache)
- Both `SimpleCache` and `FastDictCache` share the same FIFO eviction pattern

## Null Handling

| Type | Null Keys | Null Values |
|------|-----------|-------------|
| SimpleCache | Not allowed (`where TKey : notnull`) | Allowed (valid cache entry) |
| FastDictCache | Not allowed (`where TKey : notnull`) | Allowed (valid cache entry) |
| Memoization | Not allowed (generic constraints) | Allowed (valid return value) |

Per spec edge case: "How does the cache handle null values returned by getter functions? (Should cache null as a valid value)"
