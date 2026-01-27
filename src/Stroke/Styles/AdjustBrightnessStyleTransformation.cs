namespace Stroke.Styles;

/// <summary>
/// Adjust the brightness to improve rendering on dark or light backgrounds.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>AdjustBrightnessStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// For dark backgrounds, increase minBrightness.
/// For light backgrounds, decrease maxBrightness.
/// Usually only one setting is adjusted.
/// </para>
/// <para>
/// This will only change brightness for text with a foreground color defined
/// but no background color. Works best for 256 or true color output.
/// </para>
/// <para>
/// Note: There is no universal way to detect whether the application is running
/// in a light or dark terminal. As a developer of a command line application,
/// you'll have to make this configurable for the user.
/// </para>
/// <para>
/// This type is thread-safe. The brightness callables may be invoked from multiple threads;
/// thread safety of the callables is the caller's responsibility.
/// </para>
/// </remarks>
public sealed class AdjustBrightnessStyleTransformation : IStyleTransformation
{
    private readonly Func<float> _minBrightness;
    private readonly Func<float> _maxBrightness;

    /// <summary>
    /// Creates a new instance with static brightness values.
    /// </summary>
    /// <param name="minBrightness">Minimum brightness (0.0-1.0). Default: 0.0.</param>
    /// <param name="maxBrightness">Maximum brightness (0.0-1.0). Default: 1.0.</param>
    public AdjustBrightnessStyleTransformation(float minBrightness = 0.0f, float maxBrightness = 1.0f)
    {
        _minBrightness = () => minBrightness;
        _maxBrightness = () => maxBrightness;
    }

    /// <summary>
    /// Creates a new instance with dynamic brightness callables.
    /// </summary>
    /// <param name="minBrightness">Callable that returns minimum brightness (0.0-1.0).</param>
    /// <param name="maxBrightness">Callable that returns maximum brightness (0.0-1.0).</param>
    /// <exception cref="ArgumentNullException">Thrown when minBrightness or maxBrightness is null.</exception>
    public AdjustBrightnessStyleTransformation(Func<float> minBrightness, Func<float> maxBrightness)
    {
        ArgumentNullException.ThrowIfNull(minBrightness);
        ArgumentNullException.ThrowIfNull(maxBrightness);
        _minBrightness = minBrightness;
        _maxBrightness = maxBrightness;
    }

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs)
    {
        var minBrightness = _minBrightness();
        var maxBrightness = _maxBrightness();

        // Validate ranges
        if (minBrightness < 0 || minBrightness > 1)
            throw new InvalidOperationException($"minBrightness must be between 0.0 and 1.0, got {minBrightness}");
        if (maxBrightness < 0 || maxBrightness > 1)
            throw new InvalidOperationException($"maxBrightness must be between 0.0 and 1.0, got {maxBrightness}");

        // Don't do anything if the whole brightness range is acceptable.
        // This also avoids turning ANSI colors into RGB sequences.
        if (minBrightness == 0.0f && maxBrightness == 1.0f)
        {
            return attrs;
        }

        // If a foreground color is given without a background color.
        var noBackground = string.IsNullOrEmpty(attrs.BgColor) || attrs.BgColor == "default";
        var hasFgColor = !string.IsNullOrEmpty(attrs.Color) && attrs.Color != "ansidefault";

        if (hasFgColor && noBackground)
        {
            // Calculate new RGB values
            var (r, g, b) = ColorToRgb(attrs.Color!);
            var (hue, brightness, saturation) = ColorUtils.RgbToHls(r, g, b);

            brightness = InterpolateBrightness(brightness, minBrightness, maxBrightness);

            var (r2, g2, b2) = ColorUtils.HlsToRgb(hue, brightness, saturation);
            var newColor = $"{(int)(r2 * 255):x2}{(int)(g2 * 255):x2}{(int)(b2 * 255):x2}";

            return attrs with { Color = newColor };
        }

        return attrs;
    }

    /// <summary>
    /// Parse Attrs color into RGB tuple.
    /// </summary>
    private static (double R, double G, double B) ColorToRgb(string color)
    {
        // Do RGB lookup for ANSI colors
        if (AnsiColorsToRgb.Colors.TryGetValue(color, out var rgb))
        {
            return (rgb.R / 255.0, rgb.G / 255.0, rgb.B / 255.0);
        }

        // Parse RRGGBB format
        return (
            int.Parse(color[..2], System.Globalization.NumberStyles.HexNumber) / 255.0,
            int.Parse(color[2..4], System.Globalization.NumberStyles.HexNumber) / 255.0,
            int.Parse(color[4..6], System.Globalization.NumberStyles.HexNumber) / 255.0
        );

        // NOTE: we don't have to support named colors here. They are already
        //       transformed into RGB values in StyleParser.ParseColor.
    }

    /// <summary>
    /// Map the brightness to the (minBrightness..maxBrightness) range.
    /// </summary>
    private static double InterpolateBrightness(double value, double minBrightness, double maxBrightness)
    {
        return minBrightness + (maxBrightness - minBrightness) * value;
    }

    /// <inheritdoc/>
    public object InvalidationHash => ("adjust-brightness", _minBrightness(), _maxBrightness());
}
