namespace Stroke.Output;

/// <summary>
/// Represents the color capability level of a terminal.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>ColorDepth</c> enum
/// from <c>prompt_toolkit.output.color_depth</c>.
/// </para>
/// <para>
/// Color depth determines how colors are rendered:
/// <list type="bullet">
///   <item><description><see cref="Depth1Bit"/>: Monochrome (no colors)</description></item>
///   <item><description><see cref="Depth4Bit"/>: 16 ANSI colors</description></item>
///   <item><description><see cref="Depth8Bit"/>: 256 colors (default)</description></item>
///   <item><description><see cref="Depth24Bit"/>: True color (24-bit RGB)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ColorDepth
{
    /// <summary>
    /// Monochrome output (no colors).
    /// </summary>
    Depth1Bit = 0,

    /// <summary>
    /// 16 ANSI colors (standard terminal colors).
    /// </summary>
    Depth4Bit = 1,

    /// <summary>
    /// 256 colors (extended palette). This is the default.
    /// </summary>
    Depth8Bit = 2,

    /// <summary>
    /// True color (24-bit RGB, 16 million colors).
    /// </summary>
    Depth24Bit = 3
}

/// <summary>
/// Extension methods and utilities for <see cref="ColorDepth"/>.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's color depth detection logic
/// from <c>prompt_toolkit.output.color_depth</c>.
/// </para>
/// </remarks>
public static class ColorDepthExtensions
{
    /// <summary>
    /// The default color depth (256 colors).
    /// </summary>
    public static ColorDepth Default => ColorDepth.Depth8Bit;

    /// <summary>
    /// Alias for <see cref="ColorDepth.Depth1Bit"/>.
    /// </summary>
    public static ColorDepth Monochrome => ColorDepth.Depth1Bit;

    /// <summary>
    /// Alias for <see cref="ColorDepth.Depth4Bit"/>.
    /// </summary>
    public static ColorDepth AnsiColorsOnly => ColorDepth.Depth4Bit;

    /// <summary>
    /// Alias for <see cref="ColorDepth.Depth24Bit"/>.
    /// </summary>
    public static ColorDepth TrueColor => ColorDepth.Depth24Bit;

    /// <summary>
    /// Attempts to detect color depth from environment variables.
    /// </summary>
    /// <returns>
    /// The detected color depth, or <c>null</c> if no environment variable specifies it.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Environment variables are checked in the following priority order:
    /// <list type="number">
    ///   <item><description><c>NO_COLOR</c>: If set (any value), returns <see cref="ColorDepth.Depth1Bit"/>.</description></item>
    ///   <item><description><c>STROKE_COLOR_DEPTH</c>: If set, returns the specified depth.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Valid values for <c>STROKE_COLOR_DEPTH</c>:
    /// <c>DEPTH_1_BIT</c>, <c>DEPTH_4_BIT</c>, <c>DEPTH_8_BIT</c>, <c>DEPTH_24_BIT</c>.
    /// </para>
    /// </remarks>
    public static ColorDepth? FromEnvironment()
    {
        // Check NO_COLOR first (https://no-color.org/)
        var noColor = Environment.GetEnvironmentVariable("NO_COLOR");
        if (noColor is not null)
        {
            return ColorDepth.Depth1Bit;
        }

        // Check STROKE_COLOR_DEPTH (matches PROMPT_TOOLKIT_COLOR_DEPTH pattern)
        var strokeColorDepth = Environment.GetEnvironmentVariable("STROKE_COLOR_DEPTH");
        if (strokeColorDepth is not null)
        {
            return strokeColorDepth switch
            {
                "DEPTH_1_BIT" => ColorDepth.Depth1Bit,
                "DEPTH_4_BIT" => ColorDepth.Depth4Bit,
                "DEPTH_8_BIT" => ColorDepth.Depth8Bit,
                "DEPTH_24_BIT" => ColorDepth.Depth24Bit,
                _ => null // Invalid value
            };
        }

        return null;
    }

    /// <summary>
    /// Gets the default color depth based on the terminal type.
    /// </summary>
    /// <param name="term">The TERM environment variable value, or <c>null</c> if not set.</param>
    /// <returns>The appropriate color depth for the terminal type.</returns>
    /// <remarks>
    /// <para>
    /// Terminal type detection:
    /// <list type="bullet">
    ///   <item><description><c>dumb</c> or starts with <c>dumb</c>: <see cref="ColorDepth.Depth1Bit"/></description></item>
    ///   <item><description><c>linux</c>: <see cref="ColorDepth.Depth4Bit"/> (Linux console)</description></item>
    ///   <item><description><c>eterm-color</c>: <see cref="ColorDepth.Depth4Bit"/> (Emacs terminal)</description></item>
    ///   <item><description>All others: <see cref="ColorDepth.Depth8Bit"/> (default)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static ColorDepth GetDefaultForTerm(string? term)
    {
        if (term is null)
        {
            return Default;
        }

        if (term == "dumb" || term.StartsWith("dumb", StringComparison.Ordinal))
        {
            return ColorDepth.Depth1Bit;
        }

        if (term is "linux" or "eterm-color")
        {
            return ColorDepth.Depth4Bit;
        }

        return Default;
    }
}
