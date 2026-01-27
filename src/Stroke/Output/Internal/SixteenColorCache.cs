using System.Collections.Concurrent;

namespace Stroke.Output.Internal;

/// <summary>
/// Cache for mapping RGB colors to 16 ANSI colors using Euclidean distance.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>_16ColorCache</c> class
/// from <c>prompt_toolkit.output.vt100</c>.
/// </para>
/// <para>
/// This class is thread-safe.
/// </para>
/// </remarks>
internal sealed class SixteenColorCache
{
    private readonly bool _isBg;
    private readonly ConcurrentDictionary<(int R, int G, int B, string? Exclude), (int Code, string Name)> _cache = new();

    /// <summary>
    /// The 16 ANSI color palette with RGB values.
    /// </summary>
    /// <remarks>
    /// Values match Python Prompt Toolkit's FG_ANSI_COLORS.
    /// </remarks>
    private static readonly (string Name, int R, int G, int B, int FgCode, int BgCode)[] AnsiColors =
    [
        ("ansiblack", 0, 0, 0, 30, 40),
        ("ansired", 205, 0, 0, 31, 41),
        ("ansigreen", 0, 205, 0, 32, 42),
        ("ansiyellow", 205, 205, 0, 33, 43),
        ("ansiblue", 0, 0, 238, 34, 44),
        ("ansimagenta", 205, 0, 205, 35, 45),
        ("ansicyan", 0, 205, 205, 36, 46),
        ("ansigray", 229, 229, 229, 37, 47),         // Light gray (ansiwhite)
        ("ansibrightblack", 127, 127, 127, 90, 100), // Dark gray
        ("ansibrightred", 255, 0, 0, 91, 101),
        ("ansibrightgreen", 0, 255, 0, 92, 102),
        ("ansibrightyellow", 255, 255, 0, 93, 103),
        ("ansibrightblue", 92, 92, 255, 94, 104),
        ("ansibrightmagenta", 255, 0, 255, 95, 105),
        ("ansibrightcyan", 0, 255, 255, 96, 106),
        ("ansiwhite", 255, 255, 255, 97, 107)        // Bright white
    ];

    /// <summary>
    /// Gray-like color names to exclude when saturation is high.
    /// </summary>
    private static readonly HashSet<string> GrayLikeColors =
    [
        "ansiblack",
        "ansigray",
        "ansibrightblack",
        "ansiwhite"
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="SixteenColorCache"/> class.
    /// </summary>
    /// <param name="isBg">True if this cache is for background colors.</param>
    public SixteenColorCache(bool isBg)
    {
        _isBg = isBg;
    }

    /// <summary>
    /// Gets the ANSI color code and name for an RGB color.
    /// </summary>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <param name="exclude">Color name to exclude (for fg/bg collision avoidance).</param>
    /// <returns>The ANSI code and color name.</returns>
    public (int Code, string Name) GetCode(int r, int g, int b, string? exclude = null)
    {
        // Clamp to valid range
        r = Math.Clamp(r, 0, 255);
        g = Math.Clamp(g, 0, 255);
        b = Math.Clamp(b, 0, 255);

        var key = (r, g, b, exclude);

        return _cache.GetOrAdd(key, k => ComputeNearestColor(k.R, k.G, k.B, k.Exclude));
    }

    private (int Code, string Name) ComputeNearestColor(int r, int g, int b, string? exclude)
    {
        // Calculate saturation: |r-g| + |g-b| + |b-r|
        var saturation = Math.Abs(r - g) + Math.Abs(g - b) + Math.Abs(b - r);
        var excludeGrays = saturation > 30;

        var bestDistance = int.MaxValue;
        var bestCode = 0;
        var bestName = "";

        foreach (var color in AnsiColors)
        {
            // Skip excluded color (for fg/bg collision avoidance)
            if (color.Name == exclude)
            {
                continue;
            }

            // Skip gray-like colors if saturation is high
            if (excludeGrays && GrayLikeColors.Contains(color.Name))
            {
                continue;
            }

            // Calculate squared Euclidean distance
            var dr = r - color.R;
            var dg = g - color.G;
            var db = b - color.B;
            var distance = dr * dr + dg * dg + db * db;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCode = _isBg ? color.BgCode : color.FgCode;
                bestName = color.Name;
            }
        }

        return (bestCode, bestName);
    }

    /// <summary>
    /// Gets the ANSI color code for a named ANSI color.
    /// </summary>
    /// <param name="name">The color name (e.g., "ansiblack", "ansired").</param>
    /// <returns>The ANSI code, or null if not found.</returns>
    public int? GetCodeForName(string name)
    {
        foreach (var color in AnsiColors)
        {
            if (string.Equals(color.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return _isBg ? color.BgCode : color.FgCode;
            }
        }

        return null;
    }
}
