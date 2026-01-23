# Feature 05: Cache Utilities

## Overview

Implement the caching utilities used throughout Stroke for performance optimization.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/cache.py`

## Public API

### SimpleCache Class

```csharp
namespace Stroke.Core.Cache;

/// <summary>
/// Very simple cache that discards the oldest item when the cache size is exceeded.
/// </summary>
/// <typeparam name="TKey">The key type (must be hashable).</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed class SimpleCache<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// Creates a simple cache.
    /// </summary>
    /// <param name="maxSize">Maximum size of the cache. Don't make it too big.</param>
    public SimpleCache(int maxSize = 8);

    /// <summary>
    /// Maximum size of the cache.
    /// </summary>
    public int MaxSize { get; }

    /// <summary>
    /// Get object from the cache. If not found, call getterFunc to resolve it,
    /// and put that on the top of the cache instead.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="getterFunc">Function to create the value if not cached.</param>
    /// <returns>The cached or newly created value.</returns>
    public TValue Get(TKey key, Func<TValue> getterFunc);

    /// <summary>
    /// Clear the cache.
    /// </summary>
    public void Clear();
}
```

### FastDictCache Class

```csharp
namespace Stroke.Core.Cache;

/// <summary>
/// Fast, lightweight cache which keeps at most `size` items.
/// It will discard the oldest items in the cache first.
///
/// The cache is a dictionary, which doesn't keep track of access counts.
/// It is perfect to cache little immutable objects which are not expensive to
/// create, but where a dictionary lookup is still much faster than an object
/// instantiation.
/// </summary>
/// <typeparam name="TKey">The key type (tuple of hashable items).</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed class FastDictCache<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// Creates a fast dictionary cache.
    /// </summary>
    /// <param name="getValue">Callable that's called in case of a missing key.</param>
    /// <param name="size">Maximum cache size (default: 1000000).</param>
    public FastDictCache(Func<TKey, TValue> getValue, int size = 1000000);

    /// <summary>
    /// Maximum cache size.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets or creates the value for the specified key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached or newly created value.</returns>
    public TValue this[TKey key] { get; }

    /// <summary>
    /// Checks if the key exists in the cache.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key exists.</returns>
    public bool ContainsKey(TKey key);

    /// <summary>
    /// Tries to get the value for the specified key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value if found.</param>
    /// <returns>True if found.</returns>
    public bool TryGetValue(TKey key, out TValue value);

    /// <summary>
    /// Gets the current count of items in the cache.
    /// </summary>
    public int Count { get; }
}
```

### Memoized Attribute/Decorator

```csharp
namespace Stroke.Core.Cache;

/// <summary>
/// Provides memoization for pure functions and immutable class constructors.
/// </summary>
public static class Memoization
{
    /// <summary>
    /// Creates a memoized version of a function.
    /// </summary>
    /// <typeparam name="TArg">The argument type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="func">The function to memoize.</param>
    /// <param name="maxSize">Maximum cache size.</param>
    /// <returns>A memoized function.</returns>
    public static Func<TArg, TResult> Memoize<TArg, TResult>(
        Func<TArg, TResult> func,
        int maxSize = 1024) where TArg : notnull;

    /// <summary>
    /// Creates a memoized version of a function with two arguments.
    /// </summary>
    public static Func<TArg1, TArg2, TResult> Memoize<TArg1, TArg2, TResult>(
        Func<TArg1, TArg2, TResult> func,
        int maxSize = 1024) where TArg1 : notnull where TArg2 : notnull;

    /// <summary>
    /// Creates a memoized version of a function with three arguments.
    /// </summary>
    public static Func<TArg1, TArg2, TArg3, TResult> Memoize<TArg1, TArg2, TArg3, TResult>(
        Func<TArg1, TArg2, TArg3, TResult> func,
        int maxSize = 1024) where TArg1 : notnull where TArg2 : notnull where TArg3 : notnull;
}
```

## Project Structure

```
src/Stroke/
└── Core/
    └── Cache/
        ├── SimpleCache.cs
        ├── FastDictCache.cs
        └── Memoization.cs
tests/Stroke.Tests/
└── Core/
    └── Cache/
        ├── SimpleCacheTests.cs
        ├── FastDictCacheTests.cs
        └── MemoizationTests.cs
```

## Implementation Notes

### Performance

These caches are designed for high performance. `FastDictCache` in particular is used to cache `Char` and `Document` objects. The implementation should:

- Use efficient dictionary lookups
- Minimize allocations
- Handle eviction efficiently using a deque for key ordering

### Thread Safety

The Python implementation is not thread-safe. The C# implementation should match this behavior (no locking), as these caches are typically used within a single-threaded context.

## Dependencies

- None (base types only)

## Implementation Tasks

1. Implement `SimpleCache<TKey, TValue>` class
2. Implement `FastDictCache<TKey, TValue>` class
3. Implement `Memoization` static class
4. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All cache types match Python Prompt Toolkit semantics
- [ ] Eviction works correctly when size is exceeded
- [ ] Performance is acceptable for high-frequency usage
- [ ] Unit tests achieve 80% coverage
