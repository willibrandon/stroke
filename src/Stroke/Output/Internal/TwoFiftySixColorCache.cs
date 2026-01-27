using System.Collections.Concurrent;

namespace Stroke.Output.Internal;

/// <summary>
/// Cache for mapping RGB colors to the 256-color palette.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>_256ColorCache</c> class
/// from <c>prompt_toolkit.output.vt100</c>.
/// </para>
/// <para>
/// The 256-color palette consists of:
/// <list type="bullet">
///   <item><description>Indices 0-15: 16 ANSI colors (skipped during RGB mapping)</description></item>
///   <item><description>Indices 16-231: 6x6x6 color cube (216 colors)</description></item>
///   <item><description>Indices 232-255: 24 grayscale levels</description></item>
/// </list>
/// </para>
/// <para>
/// This class is thread-safe.
/// </para>
/// </remarks>
internal sealed class TwoFiftySixColorCache
{
    private readonly ConcurrentDictionary<(int R, int G, int B), int> _cache = new();

    /// <summary>
    /// Color cube levels (6 values for each of R, G, B).
    /// </summary>
    private static readonly int[] CubeLevels = [0, 95, 135, 175, 215, 255];

    /// <summary>
    /// Grayscale RGB values (24 levels from index 232-255).
    /// </summary>
    private static readonly int[] GrayscaleLevels;

    /// <summary>
    /// Pre-computed 256-color palette RGB values.
    /// </summary>
    private static readonly (int R, int G, int B)[] Palette;

    static TwoFiftySixColorCache()
    {
        // Build grayscale levels: 8, 18, 28, 38, ..., 238 (24 levels)
        GrayscaleLevels = new int[24];
        for (var i = 0; i < 24; i++)
        {
            GrayscaleLevels[i] = 8 + i * 10;
        }

        // Build full 256-color palette
        Palette = new (int R, int G, int B)[256];

        // Indices 0-15: ANSI colors (not used for RGB mapping, but filled for completeness)
        // These are theme-dependent, so we use approximate values
        Palette[0] = (0, 0, 0);       // Black
        Palette[1] = (205, 0, 0);     // Red
        Palette[2] = (0, 205, 0);     // Green
        Palette[3] = (205, 205, 0);   // Yellow
        Palette[4] = (0, 0, 238);     // Blue
        Palette[5] = (205, 0, 205);   // Magenta
        Palette[6] = (0, 205, 205);   // Cyan
        Palette[7] = (229, 229, 229); // White (light gray)
        Palette[8] = (127, 127, 127); // Bright black (dark gray)
        Palette[9] = (255, 0, 0);     // Bright red
        Palette[10] = (0, 255, 0);    // Bright green
        Palette[11] = (255, 255, 0);  // Bright yellow
        Palette[12] = (92, 92, 255);  // Bright blue
        Palette[13] = (255, 0, 255);  // Bright magenta
        Palette[14] = (0, 255, 255);  // Bright cyan
        Palette[15] = (255, 255, 255); // Bright white

        // Indices 16-231: 6x6x6 color cube
        for (var r = 0; r < 6; r++)
        {
            for (var g = 0; g < 6; g++)
            {
                for (var b = 0; b < 6; b++)
                {
                    var index = 16 + 36 * r + 6 * g + b;
                    Palette[index] = (CubeLevels[r], CubeLevels[g], CubeLevels[b]);
                }
            }
        }

        // Indices 232-255: Grayscale
        for (var i = 0; i < 24; i++)
        {
            var level = GrayscaleLevels[i];
            Palette[232 + i] = (level, level, level);
        }
    }

    /// <summary>
    /// Gets the 256-color palette index for an RGB color.
    /// </summary>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <returns>The palette index (16-255).</returns>
    /// <remarks>
    /// Indices 0-15 are skipped as they are theme-dependent ANSI colors.
    /// </remarks>
    public int GetCode(int r, int g, int b)
    {
        // Clamp to valid range
        r = Math.Clamp(r, 0, 255);
        g = Math.Clamp(g, 0, 255);
        b = Math.Clamp(b, 0, 255);

        return _cache.GetOrAdd((r, g, b), k => ComputeNearestColor(k.R, k.G, k.B));
    }

    private static int ComputeNearestColor(int r, int g, int b)
    {
        var bestDistance = int.MaxValue;
        var bestIndex = 16;

        // Only check indices 16-255 (skip ANSI colors 0-15)
        for (var i = 16; i < 256; i++)
        {
            var color = Palette[i];
            var dr = r - color.R;
            var dg = g - color.G;
            var db = b - color.B;
            var distance = dr * dr + dg * dg + db * db;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;

                // Perfect match
                if (distance == 0)
                {
                    break;
                }
            }
        }

        return bestIndex;
    }

    /// <summary>
    /// Gets the RGB values for a palette index.
    /// </summary>
    /// <param name="index">The palette index (0-255).</param>
    /// <returns>The RGB values.</returns>
    public static (int R, int G, int B) GetRgbForIndex(int index)
    {
        if (index < 0 || index > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be 0-255");
        }

        return Palette[index];
    }
}
