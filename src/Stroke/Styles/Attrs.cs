namespace Stroke.Styles;

/// <summary>
/// Style attributes for terminal text formatting.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Attrs</c> NamedTuple
/// from <c>prompt_toolkit.styles.base</c>.
/// </para>
/// <para>
/// Nullable fields allow inheritance from parent styles. When merging multiple
/// Attrs, the last non-null value for each field is used.
/// </para>
/// <para>
/// This type is thread-safe. It is immutable.
/// </para>
/// </remarks>
/// <param name="Color">Foreground color: hex "rrggbb" (no #), ANSI name, or empty for default.</param>
/// <param name="BgColor">Background color: hex "rrggbb" (no #), ANSI name, or empty for default.</param>
/// <param name="Bold">Bold text attribute.</param>
/// <param name="Underline">Underlined text attribute.</param>
/// <param name="Strike">Strikethrough text attribute.</param>
/// <param name="Italic">Italic text attribute.</param>
/// <param name="Blink">Blinking text attribute.</param>
/// <param name="Reverse">Reversed foreground/background colors.</param>
/// <param name="Hidden">Hidden text attribute.</param>
/// <param name="Dim">Dim/faint text attribute.</param>
public readonly record struct Attrs(
    string? Color = null,
    string? BgColor = null,
    bool? Bold = null,
    bool? Underline = null,
    bool? Strike = null,
    bool? Italic = null,
    bool? Blink = null,
    bool? Reverse = null,
    bool? Hidden = null,
    bool? Dim = null);
