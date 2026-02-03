namespace Stroke.Output.Windows;

/// <summary>
/// Win32 console foreground color constants.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>FOREGROUND_COLOR</c> class
/// from <c>prompt_toolkit.output.win32</c>.
/// </para>
/// <para>
/// These constants represent the foreground (text) color attributes for the
/// Windows Console API. Combine colors with <see cref="Intensity"/> to create
/// bright variants.
/// </para>
/// </remarks>
public static class ForegroundColor
{
    /// <summary>Black foreground (0x0).</summary>
    public const int Black = 0x0000;

    /// <summary>Dark blue foreground (0x1).</summary>
    public const int Blue = 0x0001;

    /// <summary>Dark green foreground (0x2).</summary>
    public const int Green = 0x0002;

    /// <summary>Dark cyan foreground (0x3).</summary>
    public const int Cyan = 0x0003;

    /// <summary>Dark red foreground (0x4).</summary>
    public const int Red = 0x0004;

    /// <summary>Dark magenta foreground (0x5).</summary>
    public const int Magenta = 0x0005;

    /// <summary>Dark yellow (brown) foreground (0x6).</summary>
    public const int Yellow = 0x0006;

    /// <summary>Light gray foreground (0x7).</summary>
    public const int Gray = 0x0007;

    /// <summary>
    /// Intensity flag (0x8). Combine with a color to create bright variants.
    /// </summary>
    /// <example>
    /// <code>
    /// // Bright red foreground
    /// int brightRed = ForegroundColor.Red | ForegroundColor.Intensity;
    /// </code>
    /// </example>
    public const int Intensity = 0x0008;
}
