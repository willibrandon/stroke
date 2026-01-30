using Stroke.Application;
using Stroke.Filters;
using Stroke.Layout.Containers;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Layout.Menus;

/// <summary>
/// Container that displays completions in several columns.
/// When the <c>showMeta</c> filter evaluates to true, it shows the meta information
/// at the bottom.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>MultiColumnCompletionsMenu</c> class from
/// <c>layout/menus.py</c>.
/// </para>
/// <para>
/// This class is immutable after construction and inherently thread-safe.
/// </para>
/// </remarks>
public class MultiColumnCompletionsMenu : HSplit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiColumnCompletionsMenu"/> class.
    /// </summary>
    /// <param name="minRows">Minimum rows for multi-column layout. Default: 3.</param>
    /// <param name="suggestedMaxColumnWidth">Suggested maximum column width. Default: 30.</param>
    /// <param name="showMeta">Filter controlling meta row visibility. Default: true.</param>
    /// <param name="extraFilter">Additional visibility filter. Default: true.</param>
    /// <param name="zIndex">Z-index for overlay positioning. Default: 10^8.</param>
    public MultiColumnCompletionsMenu(
        int minRows = 3,
        int suggestedMaxColumnWidth = 30,
        FilterOrBool showMeta = default,
        FilterOrBool extraFilter = default,
        int zIndex = 100_000_000)
        : base(
            children: BuildChildren(minRows, suggestedMaxColumnWidth, showMeta, extraFilter),
            zIndex: zIndex)
    {
    }

    private static IReadOnlyList<IContainer> BuildChildren(
        int minRows,
        int suggestedMaxColumnWidth,
        FilterOrBool showMeta,
        FilterOrBool extraFilter)
    {
        // Python lines 641-680
        var showMetaFilter = showMeta.HasValue
            ? FilterUtils.ToFilter(showMeta)
            : Filters.Always.Instance;
        var extraFilterResolved = extraFilter.HasValue
            ? FilterUtils.ToFilter(extraFilter)
            : Filters.Always.Instance;

        // Display filter: show when there are completions but not at the point
        // we are returning the input.
        var fullFilter = ((Filter)extraFilterResolved)
            .And(AppFilters.HasCompletions)
            .And(((Filter)AppFilters.IsDone).Invert());

        // Condition checking whether any completion has DisplayMeta (formatted, not plain text).
        var anyCompletionHasMeta = new Condition(() =>
        {
            var completeState = AppContext.GetApp().CurrentBuffer.CompleteState;
            if (completeState is null) return false;
            foreach (var c in completeState.Completions)
            {
                if (c.DisplayMeta is not null && !c.DisplayMeta.Value.IsEmpty)
                    return true;
            }
            return false;
        });

        // NOTE: We don't set style='class:completion-menu' to the
        // MultiColumnCompletionMenuControl, because this is used in a
        // Float that is made transparent, and the size of the control
        // doesn't always correspond exactly with the size of the
        // generated content.
        var completionsWindow = new ConditionalContainer(
            content: new AnyContainer(
                new Window(
                    content: new MultiColumnCompletionMenuControl(
                        minRows: minRows,
                        suggestedMaxColumnWidth: suggestedMaxColumnWidth),
                    width: new Dimension(min: 8),
                    height: new Dimension(min: 1))),
            filter: new FilterOrBool(fullFilter));

        var metaWindow = new ConditionalContainer(
            content: new AnyContainer(
                new Window(
                    content: new SelectedCompletionMetaControl())),
            filter: new FilterOrBool(fullFilter.And(showMetaFilter).And(anyCompletionHasMeta)));

        return [completionsWindow, metaWindow];
    }
}
