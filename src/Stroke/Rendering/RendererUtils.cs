using Stroke.FormattedText;
using Stroke.Output;
using Stroke.Styles;

namespace Stroke.Rendering;

/// <summary>
/// Utility methods for formatted text output.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>print_formatted_text</c> from
/// <c>prompt_toolkit.renderer</c>.
/// </para>
/// </remarks>
public static class RendererUtils
{
    /// <summary>
    /// Print formatted text directly to an output device.
    /// </summary>
    /// <param name="output">The output device.</param>
    /// <param name="formattedText">The formatted text to print.</param>
    /// <param name="style">The style to use for rendering.</param>
    /// <param name="colorDepth">The color depth for rendering.</param>
    /// <param name="styleTransformation">Optional style transformation.</param>
    public static void PrintFormattedText(
        IOutput output,
        AnyFormattedText formattedText,
        IStyle? style = null,
        ColorDepth? colorDepth = null,
        IStyleTransformation? styleTransformation = null)
    {
        ArgumentNullException.ThrowIfNull(output);

        var fragments = formattedText.ToFormattedText();
        var depth = colorDepth ?? output.GetDefaultColorDepth();

        // Reset first (matches Python's initial reset_attributes()).
        output.ResetAttributes();
        Attrs? lastAttrs = null;

        foreach (var fragment in fragments)
        {
            var styleStr = fragment.Style;

            // Always resolve attrs, even for empty style strings, so that
            // attribute resets (e.g. \x1b[0m) properly clear previous state.
            var attrs = style is not null
                ? style.GetAttrsForStyleStr(styleStr)
                : DefaultAttrs.Default;

            if (styleTransformation is not null)
            {
                attrs = styleTransformation.TransformAttrs(attrs);
            }

            // Set style attributes only if something changed.
            if (attrs != lastAttrs)
            {
                if (attrs != DefaultAttrs.Default)
                {
                    output.SetAttributes(attrs, depth);
                }
                else
                {
                    output.ResetAttributes();
                }
            }
            lastAttrs = attrs;

            // Print escape sequences as raw output.
            if (styleStr.Contains("[ZeroWidthEscape]"))
            {
                output.WriteRaw(fragment.Text);
            }
            else
            {
                // Eliminate carriage returns and insert CR before every newline
                // (important when the front-end is a telnet client).
                var text = fragment.Text.Replace("\r", "").Replace("\n", "\r\n");
                output.Write(text);
            }
        }

        // Reset again.
        output.ResetAttributes();
        output.Flush();
    }
}
