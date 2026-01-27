using System.Collections.Frozen;

namespace Stroke.Styles;

/// <summary>
/// ANSI color name constants.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>ANSI_COLOR_NAMES</c>
/// and <c>ANSI_COLOR_NAMES_ALIASES</c> from <c>prompt_toolkit.styles.base</c>.
/// </para>
/// <para>
/// This type is thread-safe. It contains only readonly static data.
/// </para>
/// </remarks>
public static class AnsiColorNames
{
    /// <summary>
    /// List of standard ANSI color names (17 total).
    /// </summary>
    /// <remarks>
    /// <para>
    /// ISO 6429 colors. Usually, the terminal application allows configuring
    /// the RGB values for these names.
    /// </para>
    /// <para>
    /// Low intensity, dark: ansiblack, ansired, ansigreen, ansiyellow,
    /// ansiblue, ansimagenta, ansicyan, ansigray.
    /// </para>
    /// <para>
    /// High intensity, bright: ansibrightblack, ansibrightred, ansibrightgreen,
    /// ansibrightyellow, ansibrightblue, ansibrightmagenta, ansibrightcyan, ansiwhite.
    /// </para>
    /// </remarks>
    public static readonly IReadOnlyList<string> Names =
    [
        "ansidefault",
        // Low intensity, dark. (One or two components 0x80, the other 0x00.)
        "ansiblack",
        "ansired",
        "ansigreen",
        "ansiyellow",
        "ansiblue",
        "ansimagenta",
        "ansicyan",
        "ansigray",
        // High intensity, bright. (One or two components 0xff, the other 0x00. Not supported everywhere.)
        "ansibrightblack",
        "ansibrightred",
        "ansibrightgreen",
        "ansibrightyellow",
        "ansibrightblue",
        "ansibrightmagenta",
        "ansibrightcyan",
        "ansiwhite",
    ];

    private static readonly FrozenSet<string> _namesSet = Names.ToFrozenSet();

    /// <summary>
    /// Aliases for backwards compatibility (10 total).
    /// Maps old names to current canonical names.
    /// </summary>
    /// <remarks>
    /// People don't use the same ANSI color names everywhere. In prompt_toolkit 1.0
    /// some unconventional names were used (contributed to Pygments). This is fixed
    /// now, but the old names are still supported.
    /// </remarks>
    public static readonly IReadOnlyDictionary<string, string> Aliases = new Dictionary<string, string>
    {
        ["ansidarkgray"] = "ansibrightblack",
        ["ansiteal"] = "ansicyan",
        ["ansiturquoise"] = "ansibrightcyan",
        ["ansibrown"] = "ansiyellow",
        ["ansipurple"] = "ansimagenta",
        ["ansifuchsia"] = "ansibrightmagenta",
        ["ansilightgray"] = "ansigray",
        ["ansidarkred"] = "ansired",
        ["ansidarkgreen"] = "ansigreen",
        ["ansidarkblue"] = "ansiblue",
    }.ToFrozenDictionary();

    /// <summary>
    /// Checks if the given name is a canonical ANSI color name (not an alias).
    /// </summary>
    /// <param name="name">The color name to check.</param>
    /// <returns><c>true</c> if canonical ANSI color name; otherwise, <c>false</c>.</returns>
    public static bool IsAnsiColor(string name)
    {
        return _namesSet.Contains(name);
    }

    /// <summary>
    /// Checks if the given name is a valid ANSI color name or alias.
    /// </summary>
    /// <param name="name">The color name to check.</param>
    /// <returns><c>true</c> if valid ANSI color or alias; otherwise, <c>false</c>.</returns>
    public static bool IsAnsiColorOrAlias(string name)
    {
        return _namesSet.Contains(name) || Aliases.ContainsKey(name);
    }

    /// <summary>
    /// Resolves an alias to its canonical ANSI color name.
    /// Returns <c>null</c> if not an alias.
    /// </summary>
    /// <param name="name">The color name or alias.</param>
    /// <returns>The canonical ANSI color name, or <c>null</c> if not an alias.</returns>
    public static string? ResolveAlias(string name)
    {
        return Aliases.TryGetValue(name, out var canonical) ? canonical : null;
    }
}
