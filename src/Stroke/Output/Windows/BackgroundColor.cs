namespace Stroke.Output.Windows;

/// <summary>
/// Win32 console background color constants.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>BACKGROUND_COLOR</c> class
/// from <c>prompt_toolkit.output.win32</c>.
/// </para>
/// <para>
/// These constants represent the background color attributes for the
/// Windows Console API. Values are foreground colors shifted left by 4 bits.
/// Combine colors with <see cref="Intensity"/> to create bright variants.
/// </para>
/// </remarks>
public static class BackgroundColor
{
    /// <summary>Black background (0x00).</summary>
    public const int Black = 0x0000;

    /// <summary>Dark blue background (0x10).</summary>
    public const int Blue = 0x0010;

    /// <summary>Dark green background (0x20).</summary>
    public const int Green = 0x0020;

    /// <summary>Dark cyan background (0x30).</summary>
    public const int Cyan = 0x0030;

    /// <summary>Dark red background (0x40).</summary>
    public const int Red = 0x0040;

    /// <summary>Dark magenta background (0x50).</summary>
    public const int Magenta = 0x0050;

    /// <summary>Dark yellow (brown) background (0x60).</summary>
    public const int Yellow = 0x0060;

    /// <summary>Light gray background (0x70).</summary>
    public const int Gray = 0x0070;

    /// <summary>
    /// Intensity flag (0x80). Combine with a color to create bright variants.
    /// </summary>
    /// <example>
    /// <code>
    /// // Bright blue background
    /// int brightBlue = BackgroundColor.Blue | BackgroundColor.Intensity;
    /// </code>
    /// </example>
    public const int Intensity = 0x0080;
}
