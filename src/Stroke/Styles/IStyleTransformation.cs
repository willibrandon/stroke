namespace Stroke.Styles;

/// <summary>
/// Abstract base interface for style transformations.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>StyleTransformation</c>
/// abstract class from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// Style transformations are applied after styles are computed, allowing
/// post-processing like dark mode, contrast adjustment, or color inversion.
/// </para>
/// </remarks>
public interface IStyleTransformation
{
    /// <summary>
    /// Transform the given <see cref="Attrs"/> and return a new <see cref="Attrs"/>.
    /// </summary>
    /// <param name="attrs">The attributes to transform.</param>
    /// <returns>Transformed attributes.</returns>
    /// <remarks>
    /// Color formats can be either "ansi..." names or 6-digit lowercase hex (without '#' prefix).
    /// </remarks>
    Attrs TransformAttrs(Attrs attrs);

    /// <summary>
    /// Invalidation hash for the transformation. When this changes, cached styles should be invalidated.
    /// </summary>
    object InvalidationHash { get; }
}
