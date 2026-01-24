# Quickstart: Cache Utilities

**Feature**: 006-cache-utilities
**Date**: 2026-01-23

## Overview

Cache utilities provide high-performance caching for Stroke's internal operations. These are low-level utilities typically used by framework developers, not end users.

## SimpleCache

Use `SimpleCache<TKey, TValue>` when the cache key differs from the factory function arguments.

```csharp
using Stroke.Core;

// Create a cache with max 100 entries
var cache = new SimpleCache<string, ExpensiveResult>(maxSize: 100);

// Get or compute a value
var result = cache.Get("my-key", () => ComputeExpensiveResult());

// Subsequent calls with same key return cached value
var cachedResult = cache.Get("my-key", () => throw new Exception("Not called"));

// Clear all entries when needed
cache.Clear();
```

## FastDictCache

Use `FastDictCache<TKey, TValue>` for high-frequency lookups where the key IS the factory argument.

```csharp
using Stroke.Core;

// Create a cache with factory function
var charCache = new FastDictCache<(char Char, string Style), CharInfo>(
    key => new CharInfo(key.Char, key.Style),
    size: 1_000_000);

// Access via indexer - auto-creates missing entries
var info = charCache[('A', "bold")];

// Check existence without triggering factory
if (charCache.ContainsKey(('B', "normal")))
{
    // Key already cached
}

// Try-get pattern
if (charCache.TryGetValue(('C', "italic"), out var existing))
{
    // Use existing value
}
```

## Memoization

Use `Memoization.Memoize` to wrap pure functions for automatic result caching.

```csharp
using Stroke.Core;

// Single argument function
Func<int, long> factorial = n => n <= 1 ? 1 : n * factorial(n - 1);
var memoizedFactorial = Memoization.Memoize(factorial);

// Two argument function
Func<int, int, int> add = (a, b) => a + b;
var memoizedAdd = Memoization.Memoize(add);

// Three argument function with custom cache size
Func<string, int, bool, string> format = (s, n, upper) =>
    upper ? s.PadLeft(n).ToUpper() : s.PadLeft(n);
var memoizedFormat = Memoization.Memoize(format, maxSize: 512);

// Use like normal functions
var result = memoizedFactorial(10);  // Computed
var cached = memoizedFactorial(10);  // From cache
```

## Key Points

1. **FIFO Eviction**: All caches evict the oldest entry when capacity is exceeded
2. **Thread-Safe**: All cache operations are atomic; safe for concurrent access
3. **Null Values**: Null is a valid cached value (distinguishable from "not cached")
4. **Non-Null Keys**: All cache types require non-null keys

## Common Use Cases in Stroke

| Component | Cache Type | Purpose |
|-----------|------------|---------|
| `Char` interning | `FastDictCache` | Cache common character objects |
| `Document` caching | `FastDictCache` | Cache immutable Document instances |
| Line computations | `SimpleCache` | Cache expensive line calculations |
| Width calculations | Memoization | Cache Unicode width lookups |
