using Stroke.Application;
using Stroke.Filters;
using Stroke.Layout.Containers;
using Stroke.Layout.Margins;
using Stroke.Layout.Windows;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Layout.Menus;

/// <summary>
/// Completion menu container that displays completions in a single-column popup
/// with optional scrollbar.
/// </summary>
/// <remarks>
/// <para>
/// Wraps a <see cref="CompletionsMenuControl"/> in a <see cref="Window"/> with
/// scrollbar margin, conditional visibility (shown only when completions exist
/// and input is not done), and a high z-index for overlay positioning.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>CompletionsMenu</c> class from <c>layout/menus.py</c>.
/// </para>
/// <para>
/// This class is immutable after construction and inherently thread-safe.
/// </para>
/// </remarks>
public class CompletionsMenu : ConditionalContainer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompletionsMenu"/> class.
    /// </summary>
    /// <param name="maxHeight">Maximum number of visible completion rows. Null for unlimited.</param>
    /// <param name="scrollOffset">Number of completions to keep visible above/below selection.</param>
    /// <param name="extraFilter">Additional filter for visibility. Combined with has_completions and ~is_done.</param>
    /// <param name="displayArrows">Whether to display scrollbar arrows.</param>
    /// <param name="zIndex">Z-index for overlay positioning. Default: 10^8.</param>
    public CompletionsMenu(
        int? maxHeight = null,
        int scrollOffset = 0,
        FilterOrBool extraFilter = default,
        FilterOrBool displayArrows = default,
        int zIndex = 100_000_000)
        : base(
            content: new AnyContainer(
                new Window(
                    content: new CompletionsMenuControl(),
                    width: new Dimension(min: 8),
                    height: new Dimension(min: 1, max: maxHeight),
                    scrollOffsets: new ScrollOffsets(top: scrollOffset, bottom: scrollOffset),
                    rightMargins: [new ScrollbarMargin(displayArrows: displayArrows.HasValue ? displayArrows : false)],
                    dontExtendWidth: true,
                    style: "class:completion-menu",
                    zIndex: zIndex)),
            filter: CombineFilter(extraFilter))
    {
    }

    private static FilterOrBool CombineFilter(FilterOrBool extraFilter)
    {
        var filter = extraFilter.HasValue
            ? FilterUtils.ToFilter(extraFilter)
            : Filters.Always.Instance;
        return new FilterOrBool(
            ((Filter)filter).And(AppFilters.HasCompletions).And(((Filter)AppFilters.IsDone).Invert()));
    }
}
