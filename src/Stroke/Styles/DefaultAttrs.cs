namespace Stroke.Styles;

/// <summary>
/// Default <see cref="Attrs"/> constants.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DEFAULT_ATTRS</c> constant
/// and the <c>_EMPTY_ATTRS</c> internal constant from <c>prompt_toolkit.styles</c>.
/// </para>
/// <para>
/// This type is thread-safe. It contains only readonly static fields.
/// </para>
/// </remarks>
public static class DefaultAttrs
{
    /// <summary>
    /// The default Attrs with all values set to their defaults (empty strings for colors, false for booleans).
    /// </summary>
    /// <remarks>
    /// This corresponds to Python's <c>DEFAULT_ATTRS</c> in <c>prompt_toolkit.styles.base</c>.
    /// </remarks>
    public static readonly Attrs Default = new(
        Color: "",
        BgColor: "",
        Bold: false,
        Underline: false,
        Strike: false,
        Italic: false,
        Blink: false,
        Reverse: false,
        Hidden: false,
        Dim: false);

    /// <summary>
    /// Empty Attrs with all values null (for inheritance from parent styles).
    /// </summary>
    /// <remarks>
    /// This corresponds to Python's <c>_EMPTY_ATTRS</c> in <c>prompt_toolkit.styles.style</c>.
    /// </remarks>
    public static readonly Attrs Empty = new(
        Color: null,
        BgColor: null,
        Bold: null,
        Underline: null,
        Strike: null,
        Italic: null,
        Blink: null,
        Reverse: null,
        Hidden: null,
        Dim: null);
}
