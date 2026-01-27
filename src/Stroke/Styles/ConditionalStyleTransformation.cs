using Stroke.Filters;

namespace Stroke.Styles;

/// <summary>
/// Apply the style transformation depending on a condition.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>ConditionalStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This type is thread-safe. The filter may be invoked from multiple threads;
/// thread safety of the filter is the caller's responsibility.
/// </para>
/// </remarks>
public sealed class ConditionalStyleTransformation : IStyleTransformation
{
    private readonly IStyleTransformation _transformation;
    private readonly IFilter _filter;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="transformation">The transformation to apply when filter is true.</param>
    /// <param name="filter">The filter condition.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transformation"/> is null.</exception>
    public ConditionalStyleTransformation(
        IStyleTransformation transformation,
        FilterOrBool filter)
    {
        ArgumentNullException.ThrowIfNull(transformation);
        _transformation = transformation;
        _filter = FilterUtils.ToFilter(filter);
    }

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs)
    {
        if (_filter.Invoke())
        {
            return _transformation.TransformAttrs(attrs);
        }
        return attrs;
    }

    /// <inheritdoc/>
    public object InvalidationHash => (_filter.Invoke(), _transformation.InvalidationHash);
}
