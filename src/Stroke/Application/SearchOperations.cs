using Stroke.Core;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Controls;

namespace Stroke.Application;

/// <summary>
/// Static utility class providing search lifecycle methods.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's search functions from
/// <c>prompt_toolkit.search</c>: start_search, stop_search,
/// do_incremental_search, accept_search.
/// </para>
/// <para>
/// All methods in this class are stateless and thread-safe.
/// State is accessed via <see cref="AppContext.GetApp()"/>.
/// </para>
/// </remarks>
public static class SearchOperations
{
    /// <summary>
    /// Starts a search session by focusing the search field and setting the initial direction.
    /// </summary>
    /// <param name="bufferControl">
    /// The BufferControl to search through. If null, uses the currently focused BufferControl.
    /// Silently returns if the current control is not a BufferControl.
    /// </param>
    /// <param name="direction">The initial search direction.</param>
    /// <remarks>
    /// <para>Sets focus to the linked SearchBufferControl.</para>
    /// <para>Registers the search link in Layout.SearchLinks.</para>
    /// <para>Sets Vi input mode to Insert.</para>
    /// <para>Silently returns if the target has no SearchBufferControl.</para>
    /// </remarks>
    public static void StartSearch(
        BufferControl? bufferControl = null,
        SearchDirection direction = SearchDirection.Forward)
    {
        var app = AppContext.GetApp();
        var layout = app.Layout;

        // When no control is given, use the current control if that's a BufferControl.
        if (bufferControl is null)
        {
            if (layout.CurrentControl is not BufferControl currentBc)
                return;
            bufferControl = currentBc;
        }

        // Only if this control is searchable.
        var searchBufferControl = bufferControl.SearchBufferControl;
        if (searchBufferControl is null)
            return;

        bufferControl.SearchState.Direction = direction;

        // Make sure to focus the search BufferControl.
        layout.Focus(new FocusableElement(searchBufferControl));

        // Remember search link.
        layout.AddSearchLink(searchBufferControl, bufferControl);

        // If we're in Vi mode, make sure to go into insert mode.
        app.ViState.InputMode = InputMode.Insert;
    }

    /// <summary>
    /// Stops the current search session, returning focus to the original buffer.
    /// </summary>
    /// <param name="bufferControl">
    /// The target BufferControl to return focus to. If null, uses
    /// Layout.SearchTargetBufferControl. Silently returns if no active search exists.
    /// </param>
    /// <remarks>
    /// <para>Returns focus to the target BufferControl.</para>
    /// <para>Removes the search link from Layout.SearchLinks.</para>
    /// <para>Resets the search buffer content.</para>
    /// <para>Sets Vi input mode to Navigation.</para>
    /// </remarks>
    public static void StopSearch(BufferControl? bufferControl = null)
    {
        var app = AppContext.GetApp();
        var layout = app.Layout;

        SearchBufferControl? searchBufferControl;

        if (bufferControl is null)
        {
            bufferControl = layout.SearchTargetBufferControl;
            if (bufferControl is null)
            {
                // Not currently searching.
                return;
            }
            searchBufferControl = bufferControl.SearchBufferControl;
        }
        else
        {
            var reverseLinks = GetReverseSearchLinks(layout);
            if (!reverseLinks.TryGetValue(bufferControl, out searchBufferControl))
                return;
        }

        // Focus the original buffer again.
        layout.Focus(new FocusableElement(bufferControl));

        if (searchBufferControl is not null)
        {
            // Remove the search link.
            layout.RemoveSearchLink(searchBufferControl);

            // Reset content of search control.
            searchBufferControl.Buffer.Reset();
        }

        // If we're in Vi mode, go back to navigation mode.
        app.ViState.InputMode = InputMode.Navigation;
    }

    /// <summary>
    /// Performs an incremental search in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to search.</param>
    /// <param name="count">The number of matches to skip (default 1).</param>
    /// <remarks>
    /// <para>Updates SearchState text from the search buffer.</para>
    /// <para>Updates SearchState direction.</para>
    /// <para>Applies search to target buffer only when direction has NOT changed.</para>
    /// <para>Silently returns if current control is not a BufferControl or search target is null.</para>
    /// </remarks>
    public static void DoIncrementalSearch(
        SearchDirection direction,
        int count = 1)
    {
        var app = AppContext.GetApp();
        var layout = app.Layout;

        // Only search if the current control is a BufferControl.
        if (layout.CurrentControl is not BufferControl)
            return;

        var prevControl = layout.SearchTargetBufferControl;
        if (prevControl is null)
            return;

        var searchState = prevControl.SearchState;
        var searchBufferControl = layout.CurrentSearchBufferControl;
        if (searchBufferControl is null)
            return;

        // Update search_state.
        var directionChanged = searchState.Direction != direction;

        searchState.Text = searchBufferControl.Buffer.Text;
        searchState.Direction = direction;

        // Apply search to current buffer.
        if (!directionChanged)
        {
            prevControl.Buffer.ApplySearch(
                searchState, includeCurrentPosition: false, count: count);
        }
    }

    /// <summary>
    /// Accepts the current search result, keeping cursor at the found position.
    /// </summary>
    /// <remarks>
    /// <para>Updates SearchState text from search buffer (only if non-empty).</para>
    /// <para>Applies search including current position.</para>
    /// <para>Appends query to search history.</para>
    /// <para>Calls StopSearch to return focus.</para>
    /// <para>Silently returns if current control is not a BufferControl or search target is null.</para>
    /// </remarks>
    public static void AcceptSearch()
    {
        var app = AppContext.GetApp();
        var layout = app.Layout;

        var searchControl = layout.CurrentControl;
        var targetBufferControl = layout.SearchTargetBufferControl;

        if (searchControl is not BufferControl searchBc)
            return;
        if (targetBufferControl is null)
            return;

        var searchState = targetBufferControl.SearchState;

        // Update search state.
        if (!string.IsNullOrEmpty(searchBc.Buffer.Text))
        {
            searchState.Text = searchBc.Buffer.Text;
        }

        // Apply search.
        targetBufferControl.Buffer.ApplySearch(
            searchState, includeCurrentPosition: true);

        // Add query to history of search line.
        searchBc.Buffer.AppendToHistory();

        // Stop search and focus previous control again.
        StopSearch(targetBufferControl);
    }

    /// <summary>
    /// Computes the reverse mapping of Layout.SearchLinks.
    /// </summary>
    /// <param name="layout">The layout to get search links from.</param>
    /// <returns>Dictionary mapping BufferControl to SearchBufferControl.</returns>
    private static Dictionary<BufferControl, SearchBufferControl> GetReverseSearchLinks(
        Layout.Layout layout)
    {
        var result = new Dictionary<BufferControl, SearchBufferControl>();
        foreach (var (sbc, bc) in layout.SearchLinks)
        {
            result[bc] = sbc;
        }
        return result;
    }
}
