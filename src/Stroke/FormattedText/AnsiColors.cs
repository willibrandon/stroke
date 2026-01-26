using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Stroke.FormattedText;

/// <summary>
/// ANSI color code mappings for parsing SGR escape sequences.
/// </summary>
/// <remarks>
/// <para>
/// This class provides mappings between ANSI SGR color codes and their
/// corresponding color names, as well as the 256-color palette.
/// </para>
/// <para>
/// Equivalent to the color tables in Python Prompt Toolkit's <c>vt100.py</c>.
/// </para>
/// </remarks>
internal static class AnsiColors
{
    /// <summary>
    /// Maps foreground SGR codes to color names.
    /// </summary>
    public static readonly FrozenDictionary<int, string> ForegroundColors = new Dictionary<int, string>
    {
        // Default
        [39] = "ansidefault",
        // Low intensity (30-37)
        [30] = "ansiblack",
        [31] = "ansired",
        [32] = "ansigreen",
        [33] = "ansiyellow",
        [34] = "ansiblue",
        [35] = "ansimagenta",
        [36] = "ansicyan",
        [37] = "ansigray",
        // High intensity (90-97)
        [90] = "ansibrightblack",
        [91] = "ansibrightred",
        [92] = "ansibrightgreen",
        [93] = "ansibrightyellow",
        [94] = "ansibrightblue",
        [95] = "ansibrightmagenta",
        [96] = "ansibrightcyan",
        [97] = "ansiwhite"
    }.ToFrozenDictionary();

    /// <summary>
    /// Maps background SGR codes to color names.
    /// </summary>
    public static readonly FrozenDictionary<int, string> BackgroundColors = new Dictionary<int, string>
    {
        // Default
        [49] = "ansidefault",
        // Low intensity (40-47)
        [40] = "ansiblack",
        [41] = "ansired",
        [42] = "ansigreen",
        [43] = "ansiyellow",
        [44] = "ansiblue",
        [45] = "ansimagenta",
        [46] = "ansicyan",
        [47] = "ansigray",
        // High intensity (100-107)
        [100] = "ansibrightblack",
        [101] = "ansibrightred",
        [102] = "ansibrightgreen",
        [103] = "ansibrightyellow",
        [104] = "ansibrightblue",
        [105] = "ansibrightmagenta",
        [106] = "ansibrightcyan",
        [107] = "ansiwhite"
    }.ToFrozenDictionary();

    /// <summary>
    /// The 256-color palette mapping color index to hex color string.
    /// </summary>
    /// <remarks>
    /// Colors 0-15: 16 basic colors
    /// Colors 16-231: 6x6x6 color cube
    /// Colors 232-255: grayscale
    /// </remarks>
    public static readonly ImmutableArray<string> Palette256 = BuildPalette256();

    /// <summary>
    /// Gets the hex color string for a 256-color index.
    /// </summary>
    /// <param name="index">The color index (0-255).</param>
    /// <returns>The hex color string (e.g., "#ff0000"), or null if index is out of range.</returns>
    public static string? Get256Color(int index)
    {
        if (index < 0 || index >= 256)
            return null;
        return Palette256[index];
    }

    private static ImmutableArray<string> BuildPalette256()
    {
        var colors = new string[256];

        // Colors 0-15: 16 basic colors
        var basicColors = new (int R, int G, int B)[]
        {
            (0x00, 0x00, 0x00), // 0
            (0xCD, 0x00, 0x00), // 1
            (0x00, 0xCD, 0x00), // 2
            (0xCD, 0xCD, 0x00), // 3
            (0x00, 0x00, 0xEE), // 4
            (0xCD, 0x00, 0xCD), // 5
            (0x00, 0xCD, 0xCD), // 6
            (0xE5, 0xE5, 0xE5), // 7
            (0x7F, 0x7F, 0x7F), // 8
            (0xFF, 0x00, 0x00), // 9
            (0x00, 0xFF, 0x00), // 10
            (0xFF, 0xFF, 0x00), // 11
            (0x5C, 0x5C, 0xFF), // 12
            (0xFF, 0x00, 0xFF), // 13
            (0x00, 0xFF, 0xFF), // 14
            (0xFF, 0xFF, 0xFF)  // 15
        };

        for (int i = 0; i < 16; i++)
        {
            var (r, g, b) = basicColors[i];
            colors[i] = $"#{r:x2}{g:x2}{b:x2}";
        }

        // Colors 16-231: 6x6x6 color cube
        int[] valueRange = [0x00, 0x5F, 0x87, 0xAF, 0xD7, 0xFF];

        for (int i = 0; i < 216; i++)
        {
            int r = valueRange[i / 36 % 6];
            int g = valueRange[i / 6 % 6];
            int b = valueRange[i % 6];
            colors[16 + i] = $"#{r:x2}{g:x2}{b:x2}";
        }

        // Colors 232-255: grayscale
        for (int i = 0; i < 24; i++)
        {
            int v = 8 + i * 10;
            colors[232 + i] = $"#{v:x2}{v:x2}{v:x2}";
        }

        return [.. colors];
    }
}
