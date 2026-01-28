using Wcwidth;

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
    private static readonly StringWidthCache _cache = new();

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
    public static int GetWidth(char c)
    {
        var width = UnicodeCalculator.GetWidth(c);
        // Convert -1 (control characters) to 0 for safe arithmetic
        return width < 0 ? 0 : width;
    }

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
    public static int GetWidth(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        return _cache.GetWidth(text);
    }
}

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
    /// Strings with length > this threshold are subject to eviction.
    /// </summary>
    public const int LongStringMinLength = 64;

    /// <summary>
    /// Maximum number of long strings to keep in cache.
    /// </summary>
    public const int MaxLongStrings = 16;

    private readonly Lock _lock = new();
    private readonly Dictionary<string, int> _cache = [];
    private readonly Queue<string> _longStrings = new();

    /// <summary>
    /// Gets the width of a string, using cache if available.
    /// </summary>
    /// <param name="text">The string to measure.</param>
    /// <returns>The display width.</returns>
    public int GetWidth(string text)
    {
        using (_lock.EnterScope())
        {
            if (_cache.TryGetValue(text, out var cachedWidth))
            {
                return cachedWidth;
            }

            // Calculate width
            var width = CalculateWidth(text);

            // Determine if this is a long string
            bool isLong = text.Length > LongStringMinLength;

            if (isLong)
            {
                // Evict oldest long string if we're at capacity
                if (_longStrings.Count >= MaxLongStrings)
                {
                    var oldest = _longStrings.Dequeue();
                    _cache.Remove(oldest);
                }

                _longStrings.Enqueue(text);
            }

            _cache[text] = width;
            return width;
        }
    }

    private static int CalculateWidth(string text)
    {
        var width = 0;
        foreach (var c in text)
        {
            var charWidth = UnicodeCalculator.GetWidth(c);
            if (charWidth > 0)
            {
                width += charWidth;
            }
        }
        return width;
    }
}
