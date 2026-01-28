# Contract: UnicodeWidth

**Namespace**: `Stroke.Core`
**File**: `src/Stroke/Core/UnicodeWidth.cs`

## API Contract

```csharp
namespace Stroke.Core;

/// <summary>
/// Unicode character width calculations for terminal display.
/// </summary>
/// <remarks>
/// <para>
/// Wraps the Wcwidth library for POSIX-compatible character width determination.
/// Results are cached for performance using an LRU strategy.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>get_cwidth</c> function and <c>_CharSizesCache</c>
/// class from <c>utils.py</c>.
/// </para>
/// <para>
/// This type is thread-safe. The internal cache uses synchronization for concurrent access.
/// </para>
/// </remarks>
public static class UnicodeWidth
{
    /// <summary>
    /// Gets the display width of a character in terminal cells.
    /// </summary>
    /// <param name="c">The character to measure.</param>
    /// <returns>
    /// 0 for zero-width or control characters, 1 for standard width, 2 for wide (CJK) characters.
    /// Never returns a negative value.
    /// </returns>
    /// <remarks>
    /// Control characters that wcwidth would report as -1 are returned as 0 for safe arithmetic.
    /// </remarks>
    public static int GetWidth(char c);

    /// <summary>
    /// Gets the display width of a string in terminal cells.
    /// </summary>
    /// <param name="text">The string to measure.</param>
    /// <returns>
    /// The total display width of all characters. Returns 0 for null or empty strings.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Results are cached to avoid redundant computation. Short strings (≤64 characters)
    /// are cached indefinitely. Long strings (>64 characters) are cached with LRU eviction
    /// when more than 16 long strings are cached.
    /// </para>
    /// <para>
    /// If the string was previously measured, the cached result is returned without
    /// recalculation.
    /// </para>
    /// </remarks>
    public static int GetWidth(string? text);
}
```

## Internal Types

```csharp
/// <summary>
/// LRU cache for string width calculations.
/// </summary>
/// <remarks>
/// Thread-safe. Matches Python Prompt Toolkit's _CharSizesCache behavior.
/// </remarks>
internal sealed class StringWidthCache
{
    /// <summary>
    /// Minimum string length for considering it "long".
    /// </summary>
    public const int LongStringMinLength = 64;

    /// <summary>
    /// Maximum number of long strings to keep in cache.
    /// </summary>
    public const int MaxLongStrings = 16;

    /// <summary>
    /// Gets the width of a string, using cache if available.
    /// </summary>
    /// <param name="text">The string to measure.</param>
    /// <returns>The display width.</returns>
    public int GetWidth(string text);
}
```

## Functional Requirements Coverage

| Requirement | Method |
|-------------|--------|
| FR-006 | `GetWidth(string)` |
| FR-007 | `GetWidth(char)` returns 2 for CJK, 1 for standard |
| FR-008 | `GetWidth(char)` returns 0 for control (converts -1 to 0) |
| FR-009 | `StringWidthCache` caches results |
| FR-010 | `StringWidthCache` evicts long strings when > 16 |
| FR-030 | `StringWidthCache` uses FIFO eviction (oldest first) |
| FR-031 | Strings of exactly 64 chars are short strings (cached indefinitely) |

## Edge Cases

| Scenario | Behavior |
|----------|----------|
| Empty string | Returns 0 |
| Null string | Returns 0 |
| Control character (e.g., \x1b) | Returns 0 (not -1) |
| CJK character (e.g., '中') | Returns 2 |
| Combining mark (e.g., '\u0301') | Returns 0 |
| Cached string | Returns cached value without recalculation |
| 17th long string | Evicts oldest long string from cache (FIFO) |
| String of exactly 64 characters | Treated as short string, cached indefinitely |
| String of 65 characters | Treated as long string, subject to eviction |
| Surrogate pairs (emoji) | Handled by Wcwidth; most emoji are width 2 |

