namespace Stroke.Styles;

/// <summary>
/// Style parsing utilities.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>parse_color</c> function
/// from <c>prompt_toolkit.styles.style</c>.
/// </para>
/// <para>
/// This type is thread-safe. All methods are pure functions with no shared state.
/// </para>
/// </remarks>
public static class StyleParser
{
    /// <summary>
    /// Parse and validate a color format.
    /// </summary>
    /// <remarks>
    /// Supports:
    /// <list type="bullet">
    ///   <item>ANSI color names: "ansiblue", "ansired", etc.</item>
    ///   <item>Named colors: "red", "blue", "AliceBlue", etc.</item>
    ///   <item>Hex codes: "#RGB", "#RRGGBB", "RRGGBB"</item>
    ///   <item>Empty string or "default" for default color</item>
    /// </list>
    /// </remarks>
    /// <param name="text">The color string to parse.</param>
    /// <returns>
    /// Normalized color string:
    /// <list type="bullet">
    ///   <item>ANSI names are preserved (e.g., "ansiblue")</item>
    ///   <item>Named colors are converted to lowercase hex (e.g., "f0f8ff")</item>
    ///   <item>Hex codes are normalized to lowercase 6-digit (e.g., "ff0000")</item>
    ///   <item>Empty string or "default" returned as-is</item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the color format is invalid.</exception>
    public static string ParseColor(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        // ANSI color names.
        if (AnsiColorNames.IsAnsiColor(text))
        {
            return text;
        }

        // ANSI color aliases.
        var aliasResolved = AnsiColorNames.ResolveAlias(text);
        if (aliasResolved is not null)
        {
            return aliasResolved;
        }

        // 148 named colors (case-insensitive lookup).
        if (NamedColors.TryGetHexValue(text, out var hexValue))
        {
            return hexValue;
        }

        // Hex codes starting with '#'.
        if (text.Length > 0 && text[0] == '#')
        {
            var col = text[1..];

            // Keep this for backwards-compatibility (Pygments does it).
            // I don't like the '#' prefix for named colors.
            if (AnsiColorNames.IsAnsiColor(col))
            {
                return col;
            }

            var aliasResolvedAfterHash = AnsiColorNames.ResolveAlias(col);
            if (aliasResolvedAfterHash is not null)
            {
                return aliasResolvedAfterHash;
            }

            // 6 digit hex color.
            if (col.Length == 6 && IsValidHex(col))
            {
                return col.ToLowerInvariant();
            }

            // 3 digit hex color (expand to 6 digits).
            if (col.Length == 3 && IsValidHex(col))
            {
                return $"{col[0]}{col[0]}{col[1]}{col[1]}{col[2]}{col[2]}".ToLowerInvariant();
            }
        }

        // Default values.
        if (text is "" or "default")
        {
            return text;
        }

        throw new ArgumentException($"Wrong color format '{text}'", nameof(text));
    }

    /// <summary>
    /// Validates that a string contains only hexadecimal characters.
    /// </summary>
    private static bool IsValidHex(string value)
    {
        foreach (var c in value)
        {
            if (!char.IsAsciiHexDigit(c))
            {
                return false;
            }
        }
        return true;
    }
}
