namespace Stroke.Output.Windows;

/// <summary>
/// Maps colors to Win32 console color attributes.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>ColorLookupTable</c> class
/// from <c>prompt_toolkit.output.win32</c>.
/// </para>
/// <para>
/// This type is thread-safe. The color cache is protected by a lock.
/// </para>
/// <para>
/// Supports two types of color specifications:
/// <list type="bullet">
///   <item><description>ANSI color names (e.g., "ansired", "ansibrightblue") - 17 predefined colors.</description></item>
///   <item><description>RGB hex strings (e.g., "ff0000") - mapped to closest 16-color palette entry.</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class ColorLookupTable
{
    private readonly Lock _lock = new();
    private readonly Dictionary<string, (int Fg, int Bg)> _cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The 17 ANSI color name to Win32 attribute mappings.
    /// </summary>
    private static readonly Dictionary<string, (int Fg, int Bg)> AnsiColors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ansidefault"] = (0x0000, 0x0000),
        ["ansiblack"] = (ForegroundColor.Black, BackgroundColor.Black),
        ["ansigray"] = (ForegroundColor.Gray, BackgroundColor.Gray),
        ["ansibrightblack"] = (ForegroundColor.Black | ForegroundColor.Intensity, BackgroundColor.Black | BackgroundColor.Intensity),
        ["ansiwhite"] = (ForegroundColor.Gray | ForegroundColor.Intensity, BackgroundColor.Gray | BackgroundColor.Intensity),
        ["ansired"] = (ForegroundColor.Red, BackgroundColor.Red),
        ["ansigreen"] = (ForegroundColor.Green, BackgroundColor.Green),
        ["ansiyellow"] = (ForegroundColor.Yellow, BackgroundColor.Yellow),
        ["ansiblue"] = (ForegroundColor.Blue, BackgroundColor.Blue),
        ["ansimagenta"] = (ForegroundColor.Magenta, BackgroundColor.Magenta),
        ["ansicyan"] = (ForegroundColor.Cyan, BackgroundColor.Cyan),
        ["ansibrightred"] = (ForegroundColor.Red | ForegroundColor.Intensity, BackgroundColor.Red | BackgroundColor.Intensity),
        ["ansibrightgreen"] = (ForegroundColor.Green | ForegroundColor.Intensity, BackgroundColor.Green | BackgroundColor.Intensity),
        ["ansibrightyellow"] = (ForegroundColor.Yellow | ForegroundColor.Intensity, BackgroundColor.Yellow | BackgroundColor.Intensity),
        ["ansibrightblue"] = (ForegroundColor.Blue | ForegroundColor.Intensity, BackgroundColor.Blue | BackgroundColor.Intensity),
        ["ansibrightmagenta"] = (ForegroundColor.Magenta | ForegroundColor.Intensity, BackgroundColor.Magenta | BackgroundColor.Intensity),
        ["ansibrightcyan"] = (ForegroundColor.Cyan | ForegroundColor.Intensity, BackgroundColor.Cyan | BackgroundColor.Intensity),
    };

    /// <summary>
    /// The 16 RGB reference colors for closest-match lookup.
    /// Each entry: (R, G, B, ForegroundAttribute, BackgroundAttribute).
    /// </summary>
    private static readonly (int R, int G, int B, int Fg, int Bg)[] RgbTable =
    [
        (0, 0, 0, ForegroundColor.Black, BackgroundColor.Black),
        (0, 0, 170, ForegroundColor.Blue, BackgroundColor.Blue),
        (0, 170, 0, ForegroundColor.Green, BackgroundColor.Green),
        (0, 170, 170, ForegroundColor.Cyan, BackgroundColor.Cyan),
        (170, 0, 0, ForegroundColor.Red, BackgroundColor.Red),
        (170, 0, 170, ForegroundColor.Magenta, BackgroundColor.Magenta),
        (170, 170, 0, ForegroundColor.Yellow, BackgroundColor.Yellow),
        (136, 136, 136, ForegroundColor.Gray, BackgroundColor.Gray),
        (68, 68, 255, ForegroundColor.Blue | ForegroundColor.Intensity, BackgroundColor.Blue | BackgroundColor.Intensity),
        (68, 255, 68, ForegroundColor.Green | ForegroundColor.Intensity, BackgroundColor.Green | BackgroundColor.Intensity),
        (68, 255, 255, ForegroundColor.Cyan | ForegroundColor.Intensity, BackgroundColor.Cyan | BackgroundColor.Intensity),
        (255, 68, 68, ForegroundColor.Red | ForegroundColor.Intensity, BackgroundColor.Red | BackgroundColor.Intensity),
        (255, 68, 255, ForegroundColor.Magenta | ForegroundColor.Intensity, BackgroundColor.Magenta | BackgroundColor.Intensity),
        (255, 255, 68, ForegroundColor.Yellow | ForegroundColor.Intensity, BackgroundColor.Yellow | BackgroundColor.Intensity),
        (68, 68, 68, ForegroundColor.Black | ForegroundColor.Intensity, BackgroundColor.Black | BackgroundColor.Intensity),
        (255, 255, 255, ForegroundColor.Gray | ForegroundColor.Intensity, BackgroundColor.Gray | BackgroundColor.Intensity),
    ];

    /// <summary>
    /// Initializes a new <see cref="ColorLookupTable"/> with the standard 16-color Win32 palette.
    /// </summary>
    public ColorLookupTable()
    {
    }

    /// <summary>
    /// Looks up the Win32 foreground color attribute for the given color.
    /// </summary>
    /// <param name="color">ANSI color name (e.g., "ansired") or RGB hex (e.g., "ff0000").</param>
    /// <returns>The Win32 foreground color attribute value.</returns>
    /// <remarks>
    /// <para>
    /// Invalid or unrecognized colors return black (0x0000).
    /// </para>
    /// </remarks>
    public int LookupFgColor(string color)
    {
        var (fg, _) = LookupColor(color);
        return fg;
    }

    /// <summary>
    /// Looks up the Win32 background color attribute for the given color.
    /// </summary>
    /// <param name="color">ANSI color name (e.g., "ansired") or RGB hex (e.g., "ff0000").</param>
    /// <returns>The Win32 background color attribute value.</returns>
    /// <remarks>
    /// <para>
    /// Invalid or unrecognized colors return black (0x0000).
    /// </para>
    /// </remarks>
    public int LookupBgColor(string color)
    {
        var (_, bg) = LookupColor(color);
        return bg;
    }

    /// <summary>
    /// Looks up both foreground and background Win32 color attributes.
    /// </summary>
    private (int Fg, int Bg) LookupColor(string color)
    {
        if (string.IsNullOrEmpty(color))
        {
            return (ForegroundColor.Black, BackgroundColor.Black);
        }

        // Normalize: strip # prefix if present
        if (color.StartsWith('#'))
        {
            color = color[1..];
        }

        using (_lock.EnterScope())
        {
            // Check cache first
            if (_cache.TryGetValue(color, out var cached))
            {
                return cached;
            }

            // Try ANSI color lookup
            if (AnsiColors.TryGetValue(color, out var ansiResult))
            {
                _cache[color] = ansiResult;
                return ansiResult;
            }

            // Try RGB hex lookup
            var rgbResult = LookupRgbColor(color);
            _cache[color] = rgbResult;
            return rgbResult;
        }
    }

    /// <summary>
    /// Converts an RGB hex string to the closest Win32 color.
    /// </summary>
    private static (int Fg, int Bg) LookupRgbColor(string rgbHex)
    {
        // Must be exactly 6 hex characters
        if (rgbHex.Length != 6)
        {
            return (ForegroundColor.Black, BackgroundColor.Black);
        }

        // Parse RGB components
        if (!TryParseHexByte(rgbHex.AsSpan(0, 2), out var r) ||
            !TryParseHexByte(rgbHex.AsSpan(2, 2), out var g) ||
            !TryParseHexByte(rgbHex.AsSpan(4, 2), out var b))
        {
            return (ForegroundColor.Black, BackgroundColor.Black);
        }

        // Find closest color using squared Euclidean distance
        int minDistance = int.MaxValue;
        (int Fg, int Bg) closest = (ForegroundColor.Black, BackgroundColor.Black);

        foreach (var (tr, tg, tb, fg, bg) in RgbTable)
        {
            int dr = r - tr;
            int dg = g - tg;
            int db = b - tb;
            int distance = dr * dr + dg * dg + db * db;

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = (fg, bg);
            }
        }

        return closest;
    }

    /// <summary>
    /// Tries to parse a 2-character hex string as a byte.
    /// </summary>
    private static bool TryParseHexByte(ReadOnlySpan<char> hex, out int value)
    {
        value = 0;

        if (hex.Length != 2)
        {
            return false;
        }

        if (!TryParseHexDigit(hex[0], out var high) ||
            !TryParseHexDigit(hex[1], out var low))
        {
            return false;
        }

        value = (high << 4) | low;
        return true;
    }

    /// <summary>
    /// Tries to parse a single hex digit.
    /// </summary>
    private static bool TryParseHexDigit(char c, out int value)
    {
        value = c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'a' and <= 'f' => c - 'a' + 10,
            >= 'A' and <= 'F' => c - 'A' + 10,
            _ => -1
        };
        return value >= 0;
    }
}
