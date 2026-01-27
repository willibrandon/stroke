using System.Collections.Frozen;

namespace Stroke.Input;

/// <summary>
/// Provides alias string to canonical key string mappings.
/// </summary>
/// <remarks>
/// <para>
/// This class contains the alias mappings from Python Prompt Toolkit's KEY_ALIASES dictionary.
/// Aliases allow alternative string representations to be resolved to canonical key strings.
/// </para>
/// <para>
/// The alias mappings are:
/// <list type="bullet">
/// <item><description>"backspace" → "c-h"</description></item>
/// <item><description>"c-space" → "c-@"</description></item>
/// <item><description>"enter" → "c-m"</description></item>
/// <item><description>"tab" → "c-i"</description></item>
/// <item><description>"s-c-left" → "c-s-left" (modifier order normalization)</description></item>
/// <item><description>"s-c-right" → "c-s-right" (modifier order normalization)</description></item>
/// <item><description>"s-c-home" → "c-s-home" (modifier order normalization)</description></item>
/// <item><description>"s-c-end" → "c-s-end" (modifier order normalization)</description></item>
/// </list>
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. All static fields are readonly
/// and initialized at class load time.
/// </para>
/// </remarks>
public static class KeyAliasMap
{
    /// <summary>
    /// Dictionary mapping alias strings to canonical key strings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This dictionary contains all key aliases from Python Prompt Toolkit.
    /// Keys are case-insensitive for lookup purposes.
    /// </para>
    /// <para>
    /// Note: This is the raw alias dictionary. To resolve aliases including
    /// case-insensitivity, use <see cref="GetCanonical"/>.
    /// </para>
    /// </remarks>
    public static IReadOnlyDictionary<string, string> Aliases { get; } = new Dictionary<string, string>
    {
        ["backspace"] = "c-h",
        ["c-space"] = "c-@",
        ["enter"] = "c-m",
        ["tab"] = "c-i",
        // ShiftControl was renamed to ControlShift
        ["s-c-left"] = "c-s-left",
        ["s-c-right"] = "c-s-right",
        ["s-c-home"] = "c-s-home",
        ["s-c-end"] = "c-s-end",
    };

    /// <summary>
    /// Case-insensitive alias lookup dictionary for O(1) resolution.
    /// </summary>
    private static readonly FrozenDictionary<string, string> CaseInsensitiveAliases =
        new Dictionary<string, string>(Aliases, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the canonical key string for a given input string.
    /// </summary>
    /// <param name="keyString">The key string to resolve (may be an alias or canonical string).</param>
    /// <returns>
    /// The canonical key string if the input is an alias, or the input unchanged if not an alias.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs case-insensitive alias resolution. If the input matches
    /// an alias (ignoring case), the canonical key string is returned. Otherwise,
    /// the input is returned unchanged.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// KeyAliasMap.GetCanonical("enter");     // Returns "c-m"
    /// KeyAliasMap.GetCanonical("ENTER");     // Returns "c-m" (case-insensitive)
    /// KeyAliasMap.GetCanonical("c-a");       // Returns "c-a" (not an alias)
    /// KeyAliasMap.GetCanonical("s-c-left");  // Returns "c-s-left" (modifier order normalized)
    /// </code>
    /// </example>
    public static string GetCanonical(string keyString)
    {
        if (CaseInsensitiveAliases.TryGetValue(keyString, out var canonical))
        {
            return canonical;
        }

        return keyString;
    }
}
