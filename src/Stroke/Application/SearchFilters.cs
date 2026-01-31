using Stroke.Filters;
using Stroke.Layout.Controls;

namespace Stroke.Application;

/// <summary>
/// Filter functions for search-related state.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's search-specific filter functions from
/// <c>prompt_toolkit.filters.app</c>.
/// </para>
/// </remarks>
public static class SearchFilters
{
    /// <summary>True when the application is in search mode.</summary>
    public static IFilter IsSearching { get; } = new Condition(() =>
        AppContext.GetApp().Layout.IsSearching);

    /// <summary>True when the currently focused control is a searchable BufferControl.</summary>
    public static IFilter ControlIsSearchable { get; } = new Condition(() =>
    {
        var control = AppContext.GetApp().Layout.CurrentControl;
        return control is BufferControl bc && bc.SearchBufferControl is not null;
    });

    /// <summary>True when the current buffer has empty text.</summary>
    /// <remarks>
    /// Typically used during search to detect when the search buffer is empty.
    /// Port of Python Prompt Toolkit's <c>search_buffer_is_empty</c> condition
    /// from <c>prompt_toolkit.key_binding.bindings.vi</c>.
    /// </remarks>
    public static IFilter SearchBufferIsEmpty { get; } = new Condition(() =>
        AppContext.GetApp().CurrentBuffer.Text == "");

    /// <summary>True when the current buffer has a shift-mode selection.</summary>
    public static IFilter ShiftSelectionMode { get; } = new Condition(() =>
    {
        var selectionState = AppContext.GetApp().CurrentBuffer.SelectionState;
        return selectionState is not null && selectionState.ShiftMode;
    });
}
