namespace Stroke.Styles;

/// <summary>
/// Turn dark colors into light colors and the other way around.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SwapLightAndDarkStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This is meant to make color schemes that work on a dark background usable
/// on a light background (and the other way around).
/// </para>
/// <para>
/// Notice that this doesn't swap foreground and background like "reverse" does.
/// It turns light green into dark green and the other way around.
/// Foreground and background colors are considered individually.
/// </para>
/// <para>
/// Also notice that when reverse is used somewhere and no colors are given
/// in particular (like what is the default for the bottom toolbar), then this
/// doesn't change anything. This is what makes sense, because when the
/// 'default' color is chosen, it's what works best for the terminal, and
/// reverse works good with that.
/// </para>
/// <para>
/// This type is thread-safe. It is stateless.
/// </para>
/// </remarks>
public sealed class SwapLightAndDarkStyleTransformation : IStyleTransformation
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public SwapLightAndDarkStyleTransformation()
    {
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns the Attrs used when opposite luminosity should be used.
    /// </remarks>
    public Attrs TransformAttrs(Attrs attrs)
    {
        // Reverse colors (opposite luminosity)
        return attrs with
        {
            Color = ColorUtils.GetOppositeColor(attrs.Color),
            BgColor = ColorUtils.GetOppositeColor(attrs.BgColor)
        };
    }

    /// <inheritdoc/>
    public object InvalidationHash => $"{nameof(SwapLightAndDarkStyleTransformation)}-{GetHashCode()}";
}
