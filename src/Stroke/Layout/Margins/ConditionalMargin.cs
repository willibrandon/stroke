using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;

namespace Stroke.Layout.Margins;

/// <summary>
/// Margin that conditionally shows another margin based on a filter.
/// </summary>
/// <remarks>
/// <para>
/// Wraps another margin and shows it only when the filter condition
/// evaluates to true. When false, the margin takes zero width.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ConditionalMargin</c> class from <c>layout/margins.py</c>.
/// </para>
/// </remarks>
public sealed class ConditionalMargin : IMargin
{
    /// <summary>
    /// Gets the wrapped margin.
    /// </summary>
    public IMargin Margin { get; }

    /// <summary>
    /// Gets the filter that determines visibility.
    /// </summary>
    public IFilter Filter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalMargin"/> class.
    /// </summary>
    /// <param name="margin">The margin to wrap.</param>
    /// <param name="filter">The filter condition.</param>
    /// <exception cref="ArgumentNullException"><paramref name="margin"/> is null.</exception>
    public ConditionalMargin(IMargin margin, FilterOrBool filter)
    {
        ArgumentNullException.ThrowIfNull(margin);

        Margin = margin;
        Filter = filter.HasValue ? FilterUtils.ToFilter(filter) : Always.Instance;
    }

    /// <inheritdoc/>
    public int GetWidth(Func<UIContent> getUIContent)
    {
        if (!Filter.Invoke())
            return 0;

        return Margin.GetWidth(getUIContent);
    }

    /// <inheritdoc/>
    public IReadOnlyList<StyleAndTextTuple> CreateMargin(
        WindowRenderInfo windowRenderInfo,
        int width,
        int height)
    {
        if (!Filter.Invoke())
            return Array.Empty<StyleAndTextTuple>();

        return Margin.CreateMargin(windowRenderInfo, width, height);
    }
}
