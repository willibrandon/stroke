# Feature 69: Utilities

## Overview

Implement utility functions and classes including the Event class for simple pub/sub, Unicode width calculation, platform detection, and various helper functions.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/utils.py`

## Public API

### Event Class

```csharp
namespace Stroke.Utils;

/// <summary>
/// Simple event with += and -= notation for handlers.
/// </summary>
/// <typeparam name="TSender">Type of the sender object.</typeparam>
public sealed class Event<TSender>
{
    /// <summary>
    /// Creates an Event.
    /// </summary>
    /// <param name="sender">The sender object.</param>
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
    /// Add a handler.
    /// </summary>
    public void AddHandler(Action<TSender> handler);

    /// <summary>
    /// Remove a handler.
    /// </summary>
    public void RemoveHandler(Action<TSender> handler);

    /// <summary>
    /// Add handler with += notation.
    /// </summary>
    public static Event<TSender> operator +(Event<TSender> e, Action<TSender> handler);

    /// <summary>
    /// Remove handler with -= notation.
    /// </summary>
    public static Event<TSender> operator -(Event<TSender> e, Action<TSender> handler);
}
```

### Unicode Width Functions

```csharp
namespace Stroke.Utils;

public static class UnicodeWidth
{
    /// <summary>
    /// Get display width of a string, accounting for wide characters.
    /// Uses wcwidth for character width calculation.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>Display width in columns.</returns>
    public static int GetWidth(string text);

    /// <summary>
    /// Get display width of a single character.
    /// </summary>
    /// <param name="c">The character to measure.</param>
    /// <returns>Display width (0, 1, or 2).</returns>
    public static int GetWidth(char c);
}
```

### Platform Detection

```csharp
namespace Stroke.Utils;

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
    /// True when suspend-to-background (SIGTSTP) is supported.
    /// Returns false on Windows.
    /// </summary>
    public static bool SuspendToBackgroundSupported { get; }

    /// <summary>
    /// True when ConEmu with ANSI is detected.
    /// </summary>
    public static bool IsConEmuAnsi { get; }

    /// <summary>
    /// True when running in the main thread.
    /// </summary>
    public static bool InMainThread { get; }

    /// <summary>
    /// Get the $TERM environment variable.
    /// </summary>
    public static string TermEnvironmentVariable { get; }

    /// <summary>
    /// True if terminal is considered "dumb".
    /// </summary>
    public static bool IsDumbTerminal { get; }

    /// <summary>
    /// True if bell is enabled via environment variable.
    /// </summary>
    public static bool BellEnabled { get; }
}
```

### Value Conversion Functions

```csharp
namespace Stroke.Utils;

public static class ValueHelpers
{
    /// <summary>
    /// Convert callable or string to string.
    /// </summary>
    public static string ToStr(Func<string>? value);
    public static string ToStr(string? value);
    public static string ToStr(object? value);

    /// <summary>
    /// Convert callable or int to int.
    /// </summary>
    public static int ToInt(Func<int>? value);
    public static int ToInt(int value);
    public static int ToInt(object? value, int defaultValue = 0);

    /// <summary>
    /// Convert callable or float to float.
    /// </summary>
    public static float ToFloat(Func<float>? value);
    public static float ToFloat(float value);
    public static float ToFloat(object? value, float defaultValue = 0f);
}
```

### Weight-Based Distribution

```csharp
namespace Stroke.Utils;

public static class WeightedDistribution
{
    /// <summary>
    /// Yield items in proportion to their weights.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="items">Items to yield.</param>
    /// <param name="weights">Weights for each item.</param>
    /// <returns>Infinite sequence of items.</returns>
    public static IEnumerable<T> TakeUsingWeights<T>(
        IReadOnlyList<T> items,
        IReadOnlyList<int> weights);
}
```

### DummyContext

```csharp
namespace Stroke.Utils;

/// <summary>
/// No-op disposable for cases where a context manager is optional.
/// </summary>
public sealed class DummyContext : IDisposable
{
    public static readonly DummyContext Instance = new();

    public void Dispose() { }
}
```

## Project Structure

```
src/Stroke/
└── Utils/
    ├── Event.cs
    ├── UnicodeWidth.cs
    ├── Platform.cs
    ├── ValueHelpers.cs
    ├── WeightedDistribution.cs
    └── DummyContext.cs
tests/Stroke.Tests/
└── Utils/
    ├── EventTests.cs
    ├── UnicodeWidthTests.cs
    ├── PlatformTests.cs
    └── WeightedDistributionTests.cs
```

## Implementation Notes

### Event Implementation

```csharp
public sealed class Event<TSender>
{
    private readonly List<Action<TSender>> _handlers = new();

    public Event(TSender sender, Action<TSender>? handler = null)
    {
        Sender = sender;
        if (handler != null) AddHandler(handler);
    }

    public TSender Sender { get; }

    public void Fire()
    {
        foreach (var handler in _handlers)
        {
            handler(Sender);
        }
    }

    public void AddHandler(Action<TSender> handler)
    {
        _handlers.Add(handler);
    }

    public void RemoveHandler(Action<TSender> handler)
    {
        _handlers.Remove(handler);
    }

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

### Unicode Width with Caching

```csharp
public static class UnicodeWidth
{
    private static readonly ConcurrentDictionary<string, int> _cache = new();
    private static readonly Queue<string> _longStrings = new();
    private const int LongStringMinLen = 64;
    private const int MaxLongStrings = 16;

    public static int GetWidth(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        if (_cache.TryGetValue(text, out var cached))
            return cached;

        var width = text.Sum(c => GetWidth(c));
        _cache[text] = width;

        // Rotate long strings to limit cache size
        if (text.Length > LongStringMinLen)
        {
            lock (_longStrings)
            {
                _longStrings.Enqueue(text);
                while (_longStrings.Count > MaxLongStrings)
                {
                    var old = _longStrings.Dequeue();
                    _cache.TryRemove(old, out _);
                }
            }
        }

        return width;
    }

    public static int GetWidth(char c)
    {
        // Use wcwidth equivalent
        // CJK characters are width 2
        // Control characters are width 0
        // Most others are width 1
        var width = Wcwidth.UnicodeCalculator.GetWidth(c);
        return Math.Max(0, width);
    }
}
```

### Platform Detection

```csharp
public static class Platform
{
    public static bool IsWindows =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsMacOS =>
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsLinux =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static bool SuspendToBackgroundSupported =>
        !IsWindows; // SIGTSTP only on Unix

    public static bool IsConEmuAnsi =>
        IsWindows &&
        Environment.GetEnvironmentVariable("ConEmuANSI") == "ON";

    public static bool InMainThread =>
        Thread.CurrentThread.IsBackground == false &&
        Thread.CurrentThread.ManagedThreadId == 1;

    public static string TermEnvironmentVariable =>
        Environment.GetEnvironmentVariable("TERM") ?? "";

    public static bool IsDumbTerminal
    {
        get
        {
            var term = TermEnvironmentVariable.ToLowerInvariant();
            return term is "dumb" or "unknown";
        }
    }

    public static bool BellEnabled
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("PROMPT_TOOLKIT_BELL") ?? "true";
            return value.ToLowerInvariant() is "1" or "true";
        }
    }
}
```

### TakeUsingWeights

```csharp
public static IEnumerable<T> TakeUsingWeights<T>(
    IReadOnlyList<T> items,
    IReadOnlyList<int> weights)
{
    if (items.Count != weights.Count)
        throw new ArgumentException("Items and weights must have same length");

    // Filter zero weights
    var filtered = items.Zip(weights, (item, weight) => (item, weight))
        .Where(x => x.weight > 0)
        .ToList();

    if (filtered.Count == 0)
        throw new ArgumentException("No items with positive weight");

    var taken = new int[filtered.Count];
    var maxWeight = filtered.Max(x => x.weight);

    for (var i = 1; ; i++)
    {
        var added = false;
        for (var j = 0; j < filtered.Count; j++)
        {
            while (taken[j] < i * filtered[j].weight / (double)maxWeight)
            {
                yield return filtered[j].item;
                taken[j]++;
                added = true;
            }
        }
        if (!added && i > 1) i = 1; // Reset if nothing added
    }
}
```

## Dependencies

- `System.Runtime.InteropServices` - Platform detection
- `Wcwidth` NuGet package or implementation - Character width

## Implementation Tasks

1. Implement `Event<TSender>` class
2. Implement `UnicodeWidth` with caching
3. Implement `Platform` detection properties
4. Implement `ValueHelpers` conversion functions
5. Implement `TakeUsingWeights` generator
6. Implement `DummyContext`
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Event fires all handlers in order
- [ ] Event += and -= work correctly
- [ ] UnicodeWidth measures CJK as width 2
- [ ] UnicodeWidth caches results
- [ ] Platform correctly detects Windows/macOS/Linux
- [ ] SuspendToBackgroundSupported is false on Windows
- [ ] IsDumbTerminal checks $TERM
- [ ] ToStr/ToInt/ToFloat handle callables
- [ ] TakeUsingWeights distributes proportionally
- [ ] Unit tests achieve 80% coverage
