# Feature 87: Cache Utilities

## Overview

Implement caching utilities for performance optimization, including simple LRU cache, fast dictionary cache, and memoization decorator.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/cache.py`

## Public API

### SimpleCache

```csharp
namespace Stroke.Utils;

/// <summary>
/// Simple cache that discards the oldest item when size is exceeded.
/// Thread-safe for concurrent access.
/// </summary>
/// <typeparam name="TKey">Key type (must be hashable).</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public sealed class SimpleCache<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// Maximum number of items in the cache.
    /// </summary>
    public int MaxSize { get; }

    /// <summary>
    /// Create a simple cache.
    /// </summary>
    /// <param name="maxSize">Maximum size (default: 8).</param>
    public SimpleCache(int maxSize = 8);

    /// <summary>
    /// Get an item from the cache, or create it if not found.
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <param name="getter">Factory function if not cached.</param>
    /// <returns>The cached or newly created value.</returns>
    public TValue Get(TKey key, Func<TValue> getter);

    /// <summary>
    /// Clear all cached items.
    /// </summary>
    public void Clear();
}
```

### FastDictCache

```csharp
namespace Stroke.Utils;

/// <summary>
/// Fast, lightweight dictionary-based cache.
/// Optimized for small immutable objects where dictionary lookup
/// is faster than object instantiation.
/// </summary>
/// <typeparam name="TKey">Tuple key type.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
/// <remarks>
/// Used to cache Char and Document instances for performance.
/// </remarks>
public sealed class FastDictCache<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// Maximum cache size.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Create a fast dictionary cache.
    /// </summary>
    /// <param name="getValue">Factory function for missing keys.</param>
    /// <param name="size">Maximum size (default: 1,000,000).</param>
    public FastDictCache(Func<TKey, TValue> getValue, int size = 1000000);

    /// <summary>
    /// Get value for key, creating if not cached.
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <returns>The cached or newly created value.</returns>
    public TValue this[TKey key] { get; }

    /// <summary>
    /// Check if key is in cache.
    /// </summary>
    public bool ContainsKey(TKey key);

    /// <summary>
    /// Clear all cached items.
    /// </summary>
    public void Clear();
}
```

### Memoize Attribute

```csharp
namespace Stroke.Utils;

/// <summary>
/// Memoization helper for pure functions and immutable types.
/// </summary>
public static class Memoize
{
    /// <summary>
    /// Create a memoized version of a function.
    /// </summary>
    /// <typeparam name="TArg">Argument type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <param name="func">Function to memoize.</param>
    /// <param name="maxSize">Cache size (default: 1024).</param>
    /// <returns>Memoized function.</returns>
    public static Func<TArg, TResult> Create<TArg, TResult>(
        Func<TArg, TResult> func,
        int maxSize = 1024) where TArg : notnull;

    /// <summary>
    /// Create a memoized version of a function with two arguments.
    /// </summary>
    public static Func<TArg1, TArg2, TResult> Create<TArg1, TArg2, TResult>(
        Func<TArg1, TArg2, TResult> func,
        int maxSize = 1024)
        where TArg1 : notnull
        where TArg2 : notnull;
}
```

## Project Structure

```
src/Stroke/
└── Utils/
    ├── SimpleCache.cs
    ├── FastDictCache.cs
    └── Memoize.cs
tests/Stroke.Tests/
└── Utils/
    └── CacheTests.cs
```

## Implementation Notes

### SimpleCache Implementation

```csharp
public sealed class SimpleCache<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _data = new();
    private readonly LinkedList<TKey> _keys = new();
    private readonly object _lock = new();

    public int MaxSize { get; }

    public SimpleCache(int maxSize = 8)
    {
        if (maxSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSize));
        MaxSize = maxSize;
    }

    public TValue Get(TKey key, Func<TValue> getter)
    {
        lock (_lock)
        {
            if (_data.TryGetValue(key, out var value))
                return value;

            value = getter();
            _data[key] = value;
            _keys.AddLast(key);

            // Evict oldest when size exceeded
            while (_data.Count > MaxSize)
            {
                var keyToRemove = _keys.First!.Value;
                _keys.RemoveFirst();
                _data.Remove(keyToRemove);
            }

            return value;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _data.Clear();
            _keys.Clear();
        }
    }
}
```

### FastDictCache Implementation

```csharp
public sealed class FastDictCache<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _cache = new();
    private readonly LinkedList<TKey> _keys = new();
    private readonly Func<TKey, TValue> _getValue;
    private readonly object _lock = new();

    public int Size { get; }

    public FastDictCache(Func<TKey, TValue> getValue, int size = 1000000)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        _getValue = getValue;
        Size = size;
    }

    public TValue this[TKey key]
    {
        get
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var value))
                    return value;

                // Evict oldest if over size
                if (_cache.Count > Size)
                {
                    var keyToRemove = _keys.First!.Value;
                    _keys.RemoveFirst();
                    _cache.Remove(keyToRemove);
                }

                value = _getValue(key);
                _cache[key] = value;
                _keys.AddLast(key);
                return value;
            }
        }
    }

    public bool ContainsKey(TKey key)
    {
        lock (_lock)
            return _cache.ContainsKey(key);
    }

    public void Clear()
    {
        lock (_lock)
        {
            _cache.Clear();
            _keys.Clear();
        }
    }
}
```

### Memoize Implementation

```csharp
public static class Memoize
{
    public static Func<TArg, TResult> Create<TArg, TResult>(
        Func<TArg, TResult> func,
        int maxSize = 1024) where TArg : notnull
    {
        var cache = new SimpleCache<TArg, TResult>(maxSize);
        return arg => cache.Get(arg, () => func(arg));
    }

    public static Func<TArg1, TArg2, TResult> Create<TArg1, TArg2, TResult>(
        Func<TArg1, TArg2, TResult> func,
        int maxSize = 1024)
        where TArg1 : notnull
        where TArg2 : notnull
    {
        var cache = new SimpleCache<(TArg1, TArg2), TResult>(maxSize);
        return (a1, a2) => cache.Get((a1, a2), () => func(a1, a2));
    }
}
```

### Usage in Library

```csharp
// Char caching for screen rendering
private static readonly FastDictCache<(char, string), Char> _charCache =
    new(key => new Char(key.Item1, key.Item2));

public static Char GetChar(char c, string style) => _charCache[(c, style)];

// Document caching
private static readonly FastDictCache<(string, int), Document> _docCache =
    new(key => new Document(key.Item1, key.Item2));
```

## Dependencies

None (utility module).

## Implementation Tasks

1. Implement `SimpleCache<TKey, TValue>`
2. Implement `FastDictCache<TKey, TValue>`
3. Implement `Memoize.Create` methods
4. Add thread safety
5. Integrate with Char caching
6. Integrate with Document caching
7. Write unit tests

## Acceptance Criteria

- [ ] SimpleCache evicts oldest items
- [ ] FastDictCache provides fast lookups
- [ ] Memoize creates cached function wrappers
- [ ] Thread-safe for concurrent access
- [ ] MaxSize limits are respected
- [ ] Clear() removes all cached items
- [ ] Unit tests achieve 80% coverage
