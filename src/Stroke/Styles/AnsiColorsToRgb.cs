using System.Collections.Frozen;

namespace Stroke.Styles;

/// <summary>
/// Mapping of ANSI color names to RGB values for color transformations.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>ANSI_COLORS_TO_RGB</c>
/// dictionary from <c>prompt_toolkit.output.vt100</c>.
/// </para>
/// <para>
/// This type is thread-safe. It is stateless with immutable data.
/// </para>
/// </remarks>
internal static class AnsiColorsToRgb
{
    /// <summary>
    /// Dictionary mapping ANSI color names to RGB tuples.
    /// </summary>
    public static readonly FrozenDictionary<string, (int R, int G, int B)> Colors =
        new Dictionary<string, (int R, int G, int B)>
        {
            // Don't use, 'default' doesn't really have a value.
            ["ansidefault"] = (0x00, 0x00, 0x00),
            ["ansiblack"] = (0x00, 0x00, 0x00),
            ["ansigray"] = (0xE5, 0xE5, 0xE5),
            ["ansibrightblack"] = (0x7F, 0x7F, 0x7F),
            ["ansiwhite"] = (0xFF, 0xFF, 0xFF),
            // Low intensity
            ["ansired"] = (0xCD, 0x00, 0x00),
            ["ansigreen"] = (0x00, 0xCD, 0x00),
            ["ansiyellow"] = (0xCD, 0xCD, 0x00),
            ["ansiblue"] = (0x00, 0x00, 0xCD),
            ["ansimagenta"] = (0xCD, 0x00, 0xCD),
            ["ansicyan"] = (0x00, 0xCD, 0xCD),
            // High intensity
            ["ansibrightred"] = (0xFF, 0x00, 0x00),
            ["ansibrightgreen"] = (0x00, 0xFF, 0x00),
            ["ansibrightyellow"] = (0xFF, 0xFF, 0x00),
            ["ansibrightblue"] = (0x00, 0x00, 0xFF),
            ["ansibrightmagenta"] = (0xFF, 0x00, 0xFF),
            ["ansibrightcyan"] = (0x00, 0xFF, 0xFF),
        }.ToFrozenDictionary();
}
