namespace Stroke.Styles;

/// <summary>
/// Style transformation that swaps the 'reverse' attribute.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>ReverseStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This is still experimental.
/// </para>
/// <para>
/// This type is thread-safe. It is stateless.
/// </para>
/// </remarks>
public sealed class ReverseStyleTransformation : IStyleTransformation
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public ReverseStyleTransformation()
    {
    }

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs)
    {
        return attrs with { Reverse = !attrs.Reverse };
    }

    /// <inheritdoc/>
    public object InvalidationHash => $"{nameof(ReverseStyleTransformation)}-{GetHashCode()}";
}
