namespace Stroke.Styles;

/// <summary>
/// Internal color conversion utilities.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's color conversion functions
/// from <c>prompt_toolkit.styles.style_transformation</c>, which uses Python's
/// <c>colorsys</c> module for RGB/HLS conversion.
/// </para>
/// <para>
/// This type is thread-safe. It is stateless.
/// </para>
/// </remarks>
internal static class ColorUtils
{
    /// <summary>
    /// Convert RGB to HLS (Hue, Lightness, Saturation).
    /// </summary>
    /// <param name="r">Red component (0.0-1.0).</param>
    /// <param name="g">Green component (0.0-1.0).</param>
    /// <param name="b">Blue component (0.0-1.0).</param>
    /// <returns>Tuple of (Hue 0.0-1.0, Lightness 0.0-1.0, Saturation 0.0-1.0).</returns>
    /// <remarks>
    /// This is a faithful port of Python's <c>colorsys.rgb_to_hls</c> function.
    /// </remarks>
    public static (double H, double L, double S) RgbToHls(double r, double g, double b)
    {
        var maxc = Math.Max(r, Math.Max(g, b));
        var minc = Math.Min(r, Math.Min(g, b));
        var sumc = maxc + minc;

        // Lightness
        var l = sumc / 2.0;

        if (minc == maxc)
        {
            return (0.0, l, 0.0);
        }

        var rangec = maxc - minc;

        // Saturation
        double s;
        if (l <= 0.5)
        {
            s = rangec / sumc;
        }
        else
        {
            s = rangec / (2.0 - sumc);
        }

        // Hue
        var rc = (maxc - r) / rangec;
        var gc = (maxc - g) / rangec;
        var bc = (maxc - b) / rangec;

        double h;
        if (r == maxc)
        {
            h = bc - gc;
        }
        else if (g == maxc)
        {
            h = 2.0 + rc - bc;
        }
        else
        {
            h = 4.0 + gc - rc;
        }

        h = (h / 6.0) % 1.0;
        if (h < 0)
        {
            h += 1.0;
        }

        return (h, l, s);
    }

    /// <summary>
    /// Convert HLS to RGB.
    /// </summary>
    /// <param name="h">Hue (0.0-1.0).</param>
    /// <param name="l">Lightness (0.0-1.0).</param>
    /// <param name="s">Saturation (0.0-1.0).</param>
    /// <returns>Tuple of (Red 0.0-1.0, Green 0.0-1.0, Blue 0.0-1.0).</returns>
    /// <remarks>
    /// This is a faithful port of Python's <c>colorsys.hls_to_rgb</c> function.
    /// </remarks>
    public static (double R, double G, double B) HlsToRgb(double h, double l, double s)
    {
        if (s == 0.0)
        {
            return (l, l, l);
        }

        double m2;
        if (l <= 0.5)
        {
            m2 = l * (1.0 + s);
        }
        else
        {
            m2 = l + s - (l * s);
        }

        var m1 = 2.0 * l - m2;

        return (
            HueToRgbComponent(m1, m2, h + 1.0 / 3.0),
            HueToRgbComponent(m1, m2, h),
            HueToRgbComponent(m1, m2, h - 1.0 / 3.0)
        );
    }

    /// <summary>
    /// Helper function for HLS to RGB conversion.
    /// </summary>
    private static double HueToRgbComponent(double m1, double m2, double hue)
    {
        // Normalize hue to [0, 1)
        hue = hue % 1.0;
        if (hue < 0)
        {
            hue += 1.0;
        }

        if (hue < 1.0 / 6.0)
        {
            return m1 + (m2 - m1) * hue * 6.0;
        }

        if (hue < 0.5)
        {
            return m2;
        }

        if (hue < 2.0 / 3.0)
        {
            return m1 + (m2 - m1) * (2.0 / 3.0 - hue) * 6.0;
        }

        return m1;
    }

    /// <summary>
    /// Get the opposite color (inverted luminosity).
    /// </summary>
    /// <param name="colorName">Color as ANSI name or 6-digit hex.</param>
    /// <returns>Opposite color, or null/empty if input was null/empty.</returns>
    /// <remarks>
    /// <para>
    /// This is a faithful port of Python Prompt Toolkit's <c>get_opposite_color</c>
    /// function from <c>prompt_toolkit.styles.style_transformation</c>.
    /// </para>
    /// <para>
    /// Takes a color name in either 'ansi...' format or 6 digit RGB, returns the
    /// color of opposite luminosity (same hue/saturation).
    /// </para>
    /// </remarks>
    public static string? GetOppositeColor(string? colorName)
    {
        // Because color/bgcolor can be null in Attrs
        if (colorName is null)
        {
            return null;
        }

        // Special values
        if (colorName is "" or "default")
        {
            return colorName;
        }

        // Try ANSI color names
        if (OppositeAnsiColorNames.Opposites.TryGetValue(colorName, out var opposite))
        {
            return opposite;
        }

        // Try 6 digit RGB colors
        var r = int.Parse(colorName[..2], System.Globalization.NumberStyles.HexNumber) / 255.0;
        var g = int.Parse(colorName[2..4], System.Globalization.NumberStyles.HexNumber) / 255.0;
        var b = int.Parse(colorName[4..6], System.Globalization.NumberStyles.HexNumber) / 255.0;

        var (h, l, s) = RgbToHls(r, g, b);

        // Invert lightness
        l = 1.0 - l;

        var (r2, g2, b2) = HlsToRgb(h, l, s);

        var ri = (int)(r2 * 255);
        var gi = (int)(g2 * 255);
        var bi = (int)(b2 * 255);

        return $"{ri:x2}{gi:x2}{bi:x2}";
    }
}
