# Feature 90: Platform Utilities

## Overview

Implement platform detection and environment utilities for cross-platform terminal handling.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/utils.py`

## Public API

### Platform Detection

```csharp
namespace Stroke.Utils;

/// <summary>
/// Platform detection and environment utilities.
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
    /// True when the ConEmu Windows console is used.
    /// </summary>
    public static bool IsConEmuAnsi { get; }

    /// <summary>
    /// True when suspend-to-background (Ctrl+Z) is supported.
    /// </summary>
    public static bool SuspendToBackgroundSupported { get; }

    /// <summary>
    /// True when running in the main thread.
    /// </summary>
    public static bool InMainThread { get; }

    /// <summary>
    /// Get the TERM environment variable.
    /// </summary>
    public static string TermEnvironmentVariable { get; }

    /// <summary>
    /// True if the terminal is a "dumb" terminal.
    /// </summary>
    public static bool IsDumbTerminal { get; }

    /// <summary>
    /// True if the bell should be enabled (from env var).
    /// </summary>
    public static bool BellEnabled { get; }
}
```

### Unicode Width

```csharp
namespace Stroke.Utils;

/// <summary>
/// Unicode character width calculations.
/// </summary>
public static class UnicodeWidth
{
    /// <summary>
    /// Get the display width of a character.
    /// Wide characters (CJK) return 2, most others return 1.
    /// </summary>
    /// <param name="c">Character to measure.</param>
    /// <returns>Display width (0, 1, or 2).</returns>
    public static int GetWidth(char c);

    /// <summary>
    /// Get the display width of a string.
    /// </summary>
    /// <param name="s">String to measure.</param>
    /// <returns>Total display width.</returns>
    public static int GetWidth(string s);
}
```

### Value Coercion

```csharp
namespace Stroke.Utils;

/// <summary>
/// Value coercion utilities for dynamic values.
/// </summary>
public static class Coerce
{
    /// <summary>
    /// Convert a string or string-returning delegate to string.
    /// </summary>
    public static string ToString(Func<string>? value);
    public static string ToString(string? value);

    /// <summary>
    /// Convert an int or int-returning delegate to int.
    /// </summary>
    public static int ToInt(Func<int>? value);
    public static int ToInt(int value);

    /// <summary>
    /// Convert a float or float-returning delegate to float.
    /// </summary>
    public static double ToDouble(Func<double>? value);
    public static double ToDouble(double value);
}
```

### Weighted Distribution

```csharp
namespace Stroke.Utils;

/// <summary>
/// Utility for distributing items proportionally by weight.
/// </summary>
public static class WeightedDistribution
{
    /// <summary>
    /// Yield items in proportion to their weights.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="items">Items to distribute.</param>
    /// <param name="weights">Weight for each item.</param>
    /// <returns>Infinite sequence of items in proportion.</returns>
    public static IEnumerable<T> TakeUsingWeights<T>(
        IReadOnlyList<T> items,
        IReadOnlyList<int> weights);
}
```

## Project Structure

```
src/Stroke/
└── Utils/
    ├── Platform.cs
    ├── UnicodeWidth.cs
    ├── Coerce.cs
    └── WeightedDistribution.cs
tests/Stroke.Tests/
└── Utils/
    └── PlatformUtilsTests.cs
```

## Implementation Notes

### Platform Detection Implementation

```csharp
public static class Platform
{
    public static bool IsWindows { get; } =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsMacOS { get; } =
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsLinux { get; } =
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static bool IsWindowsVt100Supported { get; } =
        IsWindows && CheckWindowsVt100Support();

    public static bool IsConEmuAnsi { get; } =
        IsWindows &&
        Environment.GetEnvironmentVariable("ConEmuANSI") == "ON";

    public static bool SuspendToBackgroundSupported { get; } = !IsWindows;

    public static bool InMainThread =>
        Thread.CurrentThread == Thread.CurrentThread.GetType()
            .GetProperty("MainThread")?.GetValue(null);

    public static string TermEnvironmentVariable =>
        Environment.GetEnvironmentVariable("TERM") ?? "";

    public static bool IsDumbTerminal =>
        TermEnvironmentVariable.ToLowerInvariant() is "dumb" or "unknown";

    public static bool BellEnabled
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("PROMPT_TOOLKIT_BELL") ?? "true";
            return value.ToLowerInvariant() is "1" or "true";
        }
    }

    private static bool CheckWindowsVt100Support()
    {
        // Check if Windows 10 version 1607+ with VT100 support
        if (!IsWindows) return false;

        try
        {
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            return GetConsoleMode(handle, out var mode) &&
                   SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }
        catch
        {
            return false;
        }
    }
}
```

### Unicode Width Implementation

```csharp
public static class UnicodeWidth
{
    private static readonly Dictionary<char, int> _cache = new();

    public static int GetWidth(char c)
    {
        if (_cache.TryGetValue(c, out var width))
            return width;

        width = CalculateWidth(c);
        _cache[c] = width;
        return width;
    }

    public static int GetWidth(string s)
    {
        var total = 0;
        foreach (var c in s)
            total += GetWidth(c);
        return total;
    }

    private static int CalculateWidth(char c)
    {
        // Control characters
        if (c < 32 || (c >= 0x7f && c < 0xa0))
            return 0;

        // Wide characters (CJK, etc.)
        if (IsWideCharacter(c))
            return 2;

        return 1;
    }

    private static bool IsWideCharacter(char c)
    {
        // CJK Unified Ideographs and related ranges
        return c >= 0x1100 && (
            c <= 0x115F ||  // Hangul Jamo
            c == 0x2329 || c == 0x232A ||  // Angle brackets
            (c >= 0x2E80 && c <= 0x9FFF) ||  // CJK
            (c >= 0xAC00 && c <= 0xD7A3) ||  // Hangul Syllables
            (c >= 0xF900 && c <= 0xFAFF) ||  // CJK Compatibility
            (c >= 0xFE10 && c <= 0xFE6F) ||  // CJK Compatibility Forms
            (c >= 0xFF00 && c <= 0xFF60) ||  // Fullwidth Forms
            (c >= 0xFFE0 && c <= 0xFFE6)     // Fullwidth Forms
        );
    }
}
```

### TakeUsingWeights Implementation

```csharp
public static IEnumerable<T> TakeUsingWeights<T>(
    IReadOnlyList<T> items,
    IReadOnlyList<int> weights)
{
    if (items.Count != weights.Count)
        throw new ArgumentException("Items and weights must have same length");
    if (items.Count == 0)
        throw new ArgumentException("Must have at least one item");

    // Filter out zero-weight items
    var filtered = items.Zip(weights, (item, weight) => (item, weight))
        .Where(x => x.weight > 0)
        .ToList();

    if (filtered.Count == 0)
        throw new ArgumentException("No items with positive weight");

    var maxWeight = filtered.Max(x => x.weight);
    var taken = new int[filtered.Count];
    var iteration = 0;

    while (true)
    {
        var added = false;
        for (var i = 0; i < filtered.Count; i++)
        {
            var (item, weight) = filtered[i];
            if (taken[i] < iteration * weight / (double)maxWeight)
            {
                yield return item;
                taken[i]++;
                added = true;
            }
        }

        if (!added)
            iteration++;
    }
}
```

## Dependencies

- System.Runtime.InteropServices (for platform detection)

## Implementation Tasks

1. Implement `Platform` static class
2. Implement Windows VT100 detection
3. Implement `UnicodeWidth` with caching
4. Implement `Coerce` utilities
5. Implement `TakeUsingWeights`
6. Add CJK wide character detection
7. Write unit tests

## Acceptance Criteria

- [ ] Platform detection works on all OSes
- [ ] IsWindowsVt100Supported correctly detects support
- [ ] UnicodeWidth handles wide characters
- [ ] Width caching improves performance
- [ ] Coerce handles callables and values
- [ ] TakeUsingWeights distributes proportionally
- [ ] Unit tests achieve 80% coverage
