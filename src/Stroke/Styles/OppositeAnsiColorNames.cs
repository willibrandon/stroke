using System.Collections.Frozen;

namespace Stroke.Styles;

/// <summary>
/// Mapping of ANSI colors to their opposite (for light/dark swapping).
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>OPPOSITE_ANSI_COLOR_NAMES</c>
/// dictionary from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This is useful for turning color schemes that are optimized for a black
/// background usable for a white background.
/// </para>
/// <para>
/// This type is thread-safe. It is stateless with immutable data.
/// </para>
/// </remarks>
internal static class OppositeAnsiColorNames
{
    /// <summary>
    /// Dictionary mapping ANSI color names to their opposite.
    /// E.g., "ansiblack" → "ansiwhite"
    /// </summary>
    public static readonly FrozenDictionary<string, string> Opposites =
        new Dictionary<string, string>
        {
            ["ansidefault"] = "ansidefault",
            ["ansiblack"] = "ansiwhite",
            ["ansired"] = "ansibrightred",
            ["ansigreen"] = "ansibrightgreen",
            ["ansiyellow"] = "ansibrightyellow",
            ["ansiblue"] = "ansibrightblue",
            ["ansimagenta"] = "ansibrightmagenta",
            ["ansicyan"] = "ansibrightcyan",
            ["ansigray"] = "ansibrightblack",
            ["ansiwhite"] = "ansiblack",
            ["ansibrightred"] = "ansired",
            ["ansibrightgreen"] = "ansigreen",
            ["ansibrightyellow"] = "ansiyellow",
            ["ansibrightblue"] = "ansiblue",
            ["ansibrightmagenta"] = "ansimagenta",
            ["ansibrightcyan"] = "ansicyan",
            ["ansibrightblack"] = "ansigray",
        }.ToFrozenDictionary();
}
