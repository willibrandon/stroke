namespace Stroke.Styles;

/// <summary>
/// Style transformation that doesn't transform anything.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DummyStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This type is thread-safe. It is stateless.
/// </para>
/// </remarks>
public sealed class DummyStyleTransformation : IStyleTransformation
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static readonly DummyStyleTransformation Instance = new();

    private DummyStyleTransformation()
    {
    }

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs) => attrs;

    /// <inheritdoc/>
    /// <remarks>
    /// Always returns the same hash for all dummy instances.
    /// </remarks>
    public object InvalidationHash => "dummy-style-transformation";
}
