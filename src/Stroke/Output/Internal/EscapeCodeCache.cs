using System.Collections.Concurrent;
using System.Text;
using Stroke.Styles;

namespace Stroke.Output.Internal;

/// <summary>
/// Cache for mapping <see cref="Attrs"/> to VT100 escape sequences.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>_EscapeCodeCache</c> class
/// from <c>prompt_toolkit.output.vt100</c>.
/// </para>
/// <para>
/// This class is thread-safe.
/// </para>
/// </remarks>
internal sealed class EscapeCodeCache
{
    private readonly ColorDepth _colorDepth;
    private readonly ConcurrentDictionary<Attrs, string> _cache = new();
    private readonly SixteenColorCache _fgCache16 = new(isBg: false);
    private readonly SixteenColorCache _bgCache16 = new(isBg: true);
    private readonly TwoFiftySixColorCache _cache256 = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EscapeCodeCache"/> class.
    /// </summary>
    /// <param name="colorDepth">The color depth for escape sequence generation.</param>
    public EscapeCodeCache(ColorDepth colorDepth)
    {
        _colorDepth = colorDepth;
    }

    /// <summary>
    /// Gets the escape sequence for the given attributes.
    /// </summary>
    /// <param name="attrs">The attributes to convert.</param>
    /// <returns>The VT100 escape sequence.</returns>
    public string GetEscapeSequence(Attrs attrs)
    {
        return _cache.GetOrAdd(attrs, ComputeEscapeSequence);
    }

    private string ComputeEscapeSequence(Attrs attrs)
    {
        var codes = new List<int>();

        // Always start with reset
        codes.Add(0);

        // Process foreground color
        string? fgName = null;
        if (!string.IsNullOrEmpty(attrs.Color))
        {
            fgName = ProcessColor(attrs.Color, isBg: false, codes, null);
        }

        // Process background color (with fg collision avoidance for 16-color mode)
        if (!string.IsNullOrEmpty(attrs.BgColor))
        {
            ProcessColor(attrs.BgColor, isBg: true, codes, fgName);
        }

        // Process style attributes
        if (attrs.Bold == true) codes.Add(1);
        if (attrs.Dim == true) codes.Add(2);
        if (attrs.Italic == true) codes.Add(3);
        if (attrs.Underline == true) codes.Add(4);
        if (attrs.Blink == true) codes.Add(5);
        if (attrs.Reverse == true) codes.Add(7);
        if (attrs.Hidden == true) codes.Add(8);
        if (attrs.Strike == true) codes.Add(9);

        // Build escape sequence
        if (codes.Count == 1)
        {
            // Just reset
            return "\x1b[0m";
        }

        return $"\x1b[{string.Join(";", codes)}m";
    }

    private string? ProcessColor(string color, bool isBg, List<int> codes, string? excludeFg)
    {
        if (_colorDepth == ColorDepth.Depth1Bit)
        {
            // Monochrome - no colors
            return null;
        }

        // Check if it's a named ANSI color
        if (color.StartsWith("ansi", StringComparison.OrdinalIgnoreCase))
        {
            return ProcessAnsiColor(color, isBg, codes, excludeFg);
        }

        // Check if it's a hex color
        if (TryParseHexColor(color, out var r, out var g, out var b))
        {
            return ProcessRgbColor(r, g, b, isBg, codes, excludeFg);
        }

        // Unknown color format - ignore
        return null;
    }

    private string? ProcessAnsiColor(string color, bool isBg, List<int> codes, string? excludeFg)
    {
        var cache = isBg ? _bgCache16 : _fgCache16;
        var code = cache.GetCodeForName(color);

        if (code.HasValue)
        {
            codes.Add(code.Value);
            return color;
        }

        return null;
    }

    private string? ProcessRgbColor(int r, int g, int b, bool isBg, List<int> codes, string? excludeFg)
    {
        switch (_colorDepth)
        {
            case ColorDepth.Depth4Bit:
                // 16-color mode - use nearest ANSI color
                var cache16 = isBg ? _bgCache16 : _fgCache16;
                var (code16, name) = cache16.GetCode(r, g, b, excludeFg);
                codes.Add(code16);
                return name;

            case ColorDepth.Depth8Bit:
                // 256-color mode
                var index = _cache256.GetCode(r, g, b);
                codes.Add(isBg ? 48 : 38);
                codes.Add(5);
                codes.Add(index);
                return null;

            case ColorDepth.Depth24Bit:
                // True color mode
                codes.Add(isBg ? 48 : 38);
                codes.Add(2);
                codes.Add(r);
                codes.Add(g);
                codes.Add(b);
                return null;

            default:
                return null;
        }
    }

    private static bool TryParseHexColor(string color, out int r, out int g, out int b)
    {
        r = g = b = 0;

        // Remove # prefix if present
        if (color.StartsWith('#'))
        {
            color = color[1..];
        }

        if (color.Length != 6)
        {
            return false;
        }

        try
        {
            r = Convert.ToInt32(color[..2], 16);
            g = Convert.ToInt32(color[2..4], 16);
            b = Convert.ToInt32(color[4..6], 16);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
