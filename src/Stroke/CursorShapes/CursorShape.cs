namespace Stroke.CursorShapes;

/// <summary>
/// Represents the visual appearance of the terminal cursor.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>CursorShape</c> enum
/// from <c>prompt_toolkit.cursor_shapes</c>.
/// </para>
/// <para>
/// Cursor shapes are implemented using DECSCUSR (DEC Select Cursor Style) escape sequences.
/// Not all terminals support all cursor shapes; unsupported shapes are silently ignored.
/// </para>
/// </remarks>
public enum CursorShape
{
    /// <summary>
    /// Never send cursor shape escape sequences. This is the default.
    /// </summary>
    /// <remarks>
    /// Use this value when the application should preserve the user's terminal cursor setting.
    /// </remarks>
    NeverChange = 0,

    /// <summary>
    /// Solid block cursor. DECSCUSR code: 2.
    /// </summary>
    Block = 1,

    /// <summary>
    /// Vertical bar cursor (beam). DECSCUSR code: 6.
    /// </summary>
    Beam = 2,

    /// <summary>
    /// Underline cursor. DECSCUSR code: 4.
    /// </summary>
    Underline = 3,

    /// <summary>
    /// Blinking block cursor. DECSCUSR code: 1.
    /// </summary>
    BlinkingBlock = 4,

    /// <summary>
    /// Blinking vertical bar cursor (beam). DECSCUSR code: 5.
    /// </summary>
    BlinkingBeam = 5,

    /// <summary>
    /// Blinking underline cursor. DECSCUSR code: 3.
    /// </summary>
    BlinkingUnderline = 6
}

/// <summary>
/// Extension methods for <see cref="CursorShape"/>.
/// </summary>
public static class CursorShapeExtensions
{
    /// <summary>
    /// Gets the DECSCUSR (DEC Select Cursor Style) code for the cursor shape.
    /// </summary>
    /// <param name="shape">The cursor shape.</param>
    /// <returns>
    /// The DECSCUSR code, or <c>null</c> for <see cref="CursorShape.NeverChange"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// DECSCUSR codes:
    /// <list type="bullet">
    ///   <item><description>0: Reset to terminal default</description></item>
    ///   <item><description>1: Blinking block</description></item>
    ///   <item><description>2: Steady block</description></item>
    ///   <item><description>3: Blinking underline</description></item>
    ///   <item><description>4: Steady underline</description></item>
    ///   <item><description>5: Blinking bar (beam)</description></item>
    ///   <item><description>6: Steady bar (beam)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static int? GetDecscusrCode(this CursorShape shape)
    {
        return shape switch
        {
            CursorShape.NeverChange => null,
            CursorShape.Block => 2,
            CursorShape.Beam => 6,
            CursorShape.Underline => 4,
            CursorShape.BlinkingBlock => 1,
            CursorShape.BlinkingBeam => 5,
            CursorShape.BlinkingUnderline => 3,
            _ => null
        };
    }

    /// <summary>
    /// Gets the DECSCUSR escape sequence for the cursor shape.
    /// </summary>
    /// <param name="shape">The cursor shape.</param>
    /// <returns>
    /// The escape sequence string, or <c>null</c> for <see cref="CursorShape.NeverChange"/>.
    /// </returns>
    public static string? GetEscapeSequence(this CursorShape shape)
    {
        var code = shape.GetDecscusrCode();
        return code.HasValue ? $"\x1b[{code.Value} q" : null;
    }
}
