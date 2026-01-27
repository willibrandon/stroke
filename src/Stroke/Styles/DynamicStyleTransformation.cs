namespace Stroke.Styles;

/// <summary>
/// Style transformation that dynamically returns another transformation.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DynamicStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This type is thread-safe. The callable may be invoked from multiple threads;
/// thread safety of the callable is the caller's responsibility.
/// </para>
/// </remarks>
public sealed class DynamicStyleTransformation : IStyleTransformation
{
    private readonly Func<IStyleTransformation?> _getTransformation;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="getTransformation">Callable that returns a transformation, or null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getTransformation"/> is null.</exception>
    public DynamicStyleTransformation(Func<IStyleTransformation?> getTransformation)
    {
        ArgumentNullException.ThrowIfNull(getTransformation);
        _getTransformation = getTransformation;
    }

    /// <summary>
    /// Gets the current underlying transformation, or DummyStyleTransformation if null.
    /// </summary>
    private IStyleTransformation CurrentTransformation =>
        _getTransformation() ?? DummyStyleTransformation.Instance;

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs)
    {
        return CurrentTransformation.TransformAttrs(attrs);
    }

    /// <inheritdoc/>
    public object InvalidationHash => CurrentTransformation.InvalidationHash;
}
