# Feature 106: Cache and Utilities

## Overview

Implement core utility classes including caching primitives (SimpleCache, FastDictCache, memoized decorator), the Event class for event handling, platform detection utilities, and character width calculation.

## Python Prompt Toolkit Reference

**Source:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/cache.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/utils.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/token.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/log.py`

## Public API

### SimpleCache

```csharp
namespace Stroke.Utils;

/// <summary>
/// Simple LRU-like cache that discards the oldest item when size is exceeded.
/// Thread-safe implementation.
/// </summary>
/// <typeparam name="TKey">Key type (must be hashable).</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public sealed class SimpleCache<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// Create a simple cache.
    /// </summary>
    /// <param name="maxSize">Maximum number of items to cache.</param>
    public SimpleCache(int maxSize = 8);

    /// <summary>
    /// Maximum cache size.
    /// </summary>
    public int MaxSize { get; }

    /// <summary>
    /// Get an item from the cache, or compute and store it if not present.
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <param name="getterFunc">Function to compute the value if not cached.</param>
    /// <returns>The cached or computed value.</returns>
    public TValue Get(TKey key, Func<TValue> getterFunc);

    /// <summary>
    /// Clear the cache.
    /// </summary>
    public void Clear();

    /// <summary>
    /// Current number of items in the cache.
    /// </summary>
    public int Count { get; }
}
```

### FastDictCache

```csharp
namespace Stroke.Utils;

/// <summary>
/// Fast, lightweight cache optimized for tuple keys.
/// Designed for caching immutable objects like Char and Document
/// where dictionary lookup is faster than object instantiation.
/// </summary>
/// <typeparam name="TValue">Value type.</typeparam>
public sealed class FastDictCache<TValue>
{
    /// <summary>
    /// Create a fast dictionary cache.
    /// </summary>
    /// <param name="getValue">Function to compute values from key components.</param>
    /// <param name="size">Maximum cache size (default: 1,000,000).</param>
    public FastDictCache(Func<object[], TValue> getValue, int size = 1_000_000);

    /// <summary>
    /// Maximum cache size.
    /// </summary>
    public int MaxSize { get; }

    /// <summary>
    /// Get or create a value for the given key components.
    /// </summary>
    /// <param name="keys">Key components (must be hashable).</param>
    /// <returns>The cached or computed value.</returns>
    public TValue this[params object[] keys] { get; }

    /// <summary>
    /// Clear the cache.
    /// </summary>
    public void Clear();
}
```

### Memoized Attribute

```csharp
namespace Stroke.Utils;

/// <summary>
/// Memoization decorator for pure functions and immutable class factories.
/// Caches results based on argument values.
/// </summary>
/// <remarks>
/// Use with source generators or IL weaving for compile-time application.
/// For runtime use, use the Memoize helper methods.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
public sealed class MemoizedAttribute : Attribute
{
    /// <summary>
    /// Maximum cache size.
    /// </summary>
    public int MaxSize { get; set; } = 1024;
}

/// <summary>
/// Helper methods for memoization.
/// </summary>
public static class Memoize
{
    /// <summary>
    /// Create a memoized version of a function.
    /// </summary>
    public static Func<T, TResult> Create<T, TResult>(
        Func<T, TResult> func,
        int maxSize = 1024) where T : notnull;

    /// <summary>
    /// Create a memoized version of a function with two arguments.
    /// </summary>
    public static Func<T1, T2, TResult> Create<T1, T2, TResult>(
        Func<T1, T2, TResult> func,
        int maxSize = 1024) where T1 : notnull where T2 : notnull;
}
```

### Event

```csharp
namespace Stroke.Utils;

/// <summary>
/// Simple event to which handlers can be attached using += and -= operators.
/// </summary>
/// <typeparam name="TSender">Type of the sender object.</typeparam>
public sealed class Event<TSender>
{
    /// <summary>
    /// Create an event.
    /// </summary>
    /// <param name="sender">The sender object passed to handlers.</param>
    /// <param name="handler">Optional initial handler.</param>
    public Event(TSender sender, Action<TSender>? handler = null);

    /// <summary>
    /// The sender object.
    /// </summary>
    public TSender Sender { get; }

    /// <summary>
    /// Fire the event, calling all handlers.
    /// </summary>
    public void Fire();

    /// <summary>
    /// Add a handler using += operator.
    /// </summary>
    public static Event<TSender> operator +(Event<TSender> e, Action<TSender> handler);

    /// <summary>
    /// Remove a handler using -= operator.
    /// </summary>
    public static Event<TSender> operator -(Event<TSender> e, Action<TSender> handler);

    /// <summary>
    /// Add a handler.
    /// </summary>
    public void AddHandler(Action<TSender> handler);

    /// <summary>
    /// Remove a handler.
    /// </summary>
    public void RemoveHandler(Action<TSender> handler);
}
```

### DummyContext

```csharp
namespace Stroke.Utils;

/// <summary>
/// A no-op IDisposable for use as a context manager placeholder.
/// </summary>
public sealed class NullDisposable : IDisposable
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static readonly NullDisposable Instance = new();

    private NullDisposable() { }

    /// <inheritdoc/>
    public void Dispose() { }
}
```

### Platform Utilities

```csharp
namespace Stroke.Utils;

/// <summary>
/// Platform detection and utility functions.
/// </summary>
public static class Platform
{
    /// <summary>
    /// True when running on Windows.
    /// </summary>
    public static bool IsWindows { get; }

    /// <summary>
    /// True when running on macOS.
    /// </summary>
    public static bool IsMacOS { get; }

    /// <summary>
    /// True when running on Linux.
    /// </summary>
    public static bool IsLinux { get; }

    /// <summary>
    /// True when VT100 escape sequences are supported on Windows.
    /// </summary>
    public static bool IsWindowsVt100Supported { get; }

    /// <summary>
    /// True when running in ConEmu with ANSI support.
    /// </summary>
    public static bool IsConEmuAnsi { get; }

    /// <summary>
    /// True when suspend-to-background (Ctrl+Z) is supported.
    /// Typically false on Windows.
    /// </summary>
    public static bool SuspendToBackgroundSupported { get; }

    /// <summary>
    /// True when running in the main thread.
    /// </summary>
    public static bool InMainThread { get; }

    /// <summary>
    /// True when the terminal bell is enabled (PROMPT_TOOLKIT_BELL env var).
    /// </summary>
    public static bool BellEnabled { get; }

    /// <summary>
    /// The $TERM environment variable value.
    /// </summary>
    public static string TermEnvironmentVariable { get; }

    /// <summary>
    /// True if the terminal is considered "dumb" (no cursor/color support).
    /// </summary>
    public static bool IsDumbTerminal { get; }

    /// <summary>
    /// Check if a specific terminal type is "dumb".
    /// </summary>
    public static bool IsDumbTerminal(string term);
}
```

### Character Width

```csharp
namespace Stroke.Utils;

/// <summary>
/// Unicode character width utilities using wcwidth.
/// </summary>
public static class CharacterWidth
{
    /// <summary>
    /// Get the display width of a character or string.
    /// </summary>
    /// <param name="text">Character or string to measure.</param>
    /// <returns>Display width (0 for non-printable, 2 for wide characters).</returns>
    /// <remarks>
    /// Uses wcwidth internally with caching for performance.
    /// Non-printable control characters return 0 instead of -1.
    /// </remarks>
    public static int GetWidth(string text);

    /// <summary>
    /// Get the display width of a single character.
    /// </summary>
    public static int GetWidth(char c);
}
```

### Token Constants

```csharp
namespace Stroke;

/// <summary>
/// Token constants for special formatting.
/// </summary>
public static class Token
{
    /// <summary>
    /// Special token indicating zero-width escape sequence.
    /// Used for embedding escape codes that don't take display space.
    /// </summary>
    public const string ZeroWidthEscape = "[ZeroWidthEscape]";
}
```

### Weight-Based Distribution

```csharp
namespace Stroke.Utils;

/// <summary>
/// Weight-based distribution utilities.
/// </summary>
public static class WeightedDistribution
{
    /// <summary>
    /// Yield items in proportion to their weights.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="items">Items to yield.</param>
    /// <param name="weights">Weight for each item (positive integers).</param>
    /// <returns>Infinite enumerable yielding items proportionally.</returns>
    /// <example>
    /// // Getting 70 items yields ~10 A's, ~20 B's, ~40 C's
    /// var items = TakeUsingWeights(new[] { "A", "B", "C" }, new[] { 5, 10, 20 });
    /// </example>
    public static IEnumerable<T> TakeUsingWeights<T>(
        IReadOnlyList<T> items,
        IReadOnlyList<int> weights);
}
```

### Type Conversion Helpers

```csharp
namespace Stroke.Utils;

/// <summary>
/// Helpers for handling callable or literal values.
/// </summary>
public static class Convert
{
    /// <summary>
    /// Convert callable or string to string.
    /// </summary>
    public static string ToString(Func<string>? callable);
    public static string ToString(string value);
    public static string ToString(object value);

    /// <summary>
    /// Convert callable or int to int.
    /// </summary>
    public static int ToInt(Func<int>? callable);
    public static int ToInt(int value);

    /// <summary>
    /// Convert callable or float to float.
    /// </summary>
    public static float ToFloat(Func<float>? callable);
    public static float ToFloat(float value);
}
```

## Project Structure

```
src/Stroke/
├── Utils/
│   ├── SimpleCache.cs
│   ├── FastDictCache.cs
│   ├── Memoize.cs
│   ├── Event.cs
│   ├── NullDisposable.cs
│   ├── Platform.cs
│   ├── CharacterWidth.cs
│   ├── WeightedDistribution.cs
│   └── Convert.cs
├── Token.cs
└── Logging.cs
tests/Stroke.Tests/
└── Utils/
    ├── SimpleCacheTests.cs
    ├── FastDictCacheTests.cs
    ├── EventTests.cs
    ├── PlatformTests.cs
    └── CharacterWidthTests.cs
```

## Implementation Notes

### SimpleCache Implementation

```csharp
public sealed class SimpleCache<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _data = new();
    private readonly Queue<TKey> _keys = new();
    private readonly object _lock = new();

    public SimpleCache(int maxSize = 8)
    {
        if (maxSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSize));
        MaxSize = maxSize;
    }

    public int MaxSize { get; }
    public int Count => _data.Count;

    public TValue Get(TKey key, Func<TValue> getterFunc)
    {
        lock (_lock)
        {
            if (_data.TryGetValue(key, out var value))
                return value;

            value = getterFunc();
            _data[key] = value;
            _keys.Enqueue(key);

            // Remove oldest when size exceeded
            while (_data.Count > MaxSize && _keys.Count > 0)
            {
                var keyToRemove = _keys.Dequeue();
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

### Event Implementation

```csharp
public sealed class Event<TSender>
{
    private readonly List<Action<TSender>> _handlers = new();

    public Event(TSender sender, Action<TSender>? handler = null)
    {
        Sender = sender;
        if (handler != null)
            AddHandler(handler);
    }

    public TSender Sender { get; }

    public void Fire()
    {
        foreach (var handler in _handlers.ToList())
            handler(Sender);
    }

    public void AddHandler(Action<TSender> handler)
        => _handlers.Add(handler);

    public void RemoveHandler(Action<TSender> handler)
        => _handlers.Remove(handler);

    public static Event<TSender> operator +(Event<TSender> e, Action<TSender> handler)
    {
        e.AddHandler(handler);
        return e;
    }

    public static Event<TSender> operator -(Event<TSender> e, Action<TSender> handler)
    {
        e.RemoveHandler(handler);
        return e;
    }
}
```

### CharacterWidth Implementation

```csharp
public static class CharacterWidth
{
    private static readonly FastDictCache<int> _cache = new(
        keys => ComputeWidth((string)keys[0]),
        size: 1_000_000);

    public static int GetWidth(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
        return _cache[text];
    }

    public static int GetWidth(char c)
        => GetWidth(c.ToString());

    private static int ComputeWidth(string text)
    {
        if (text.Length == 1)
        {
            // Use wcwidth for single character
            var width = UnicodeWidth.GetWidth(text[0]);
            return Math.Max(0, width);  // -1 becomes 0 for non-printable
        }

        // Sum widths for strings
        var total = 0;
        foreach (var c in text)
            total += GetWidth(c);
        return total;
    }
}
```

## Dependencies

- External: wcwidth library for Unicode character width

## Implementation Tasks

1. Implement SimpleCache with thread safety
2. Implement FastDictCache with tuple key support
3. Implement Memoize helper methods
4. Implement Event class with operator overloads
5. Implement Platform static class
6. Implement CharacterWidth with caching
7. Implement WeightedDistribution.TakeUsingWeights
8. Implement Convert helper methods
9. Write unit tests

## Acceptance Criteria

- [ ] SimpleCache evicts oldest items on overflow
- [ ] FastDictCache handles tuple keys efficiently
- [ ] Event supports += and -= operators
- [ ] Platform correctly detects OS and features
- [ ] CharacterWidth caches wcwidth results
- [ ] CharacterWidth handles wide characters (CJK)
- [ ] WeightedDistribution distributes proportionally
- [ ] Thread-safe cache implementations
- [ ] Unit tests achieve 80% coverage
