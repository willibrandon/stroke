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

        foreach (var fragment in fragments)
        {
            var styleStr = fragment.Style;
            if (!string.IsNullOrEmpty(styleStr) && style is not null)
            {
                var attrs = style.GetAttrsForStyleStr(styleStr);
                if (styleTransformation is not null)
                {
                    attrs = styleTransformation.TransformAttrs(attrs);
                }
                output.SetAttributes(attrs, depth);
            }

            output.Write(fragment.Text);
        }

        output.ResetAttributes();
        output.Flush();
    }
}
