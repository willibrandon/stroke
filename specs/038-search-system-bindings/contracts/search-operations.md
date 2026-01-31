# Contract: SearchOperations

**Namespace**: `Stroke.Application`
**Type**: `public static class`
**Python Source**: `prompt_toolkit/search.py`

## Public API

```csharp
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
/// State is accessed via AppContext.GetApp().
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
        SearchDirection direction = SearchDirection.Forward);

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
    public static void StopSearch(BufferControl? bufferControl = null);

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
        int count = 1);

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
    public static void AcceptSearch();
}
```

## Private API

```csharp
/// <summary>
/// Computes the reverse mapping of Layout.SearchLinks.
/// </summary>
/// <returns>Dictionary mapping BufferControl → SearchBufferControl.</returns>
private static Dictionary<BufferControl, SearchBufferControl> GetReverseSearchLinks(
    Layout.Layout layout);
```

## Python Reference Mapping

| Python Function | C# Method | Signature Match |
|----------------|-----------|-----------------|
| `start_search(buffer_control=None, direction=FORWARD)` | `StartSearch(bufferControl=null, direction=Forward)` | ✅ |
| `stop_search(buffer_control=None)` | `StopSearch(bufferControl=null)` | ✅ |
| `do_incremental_search(direction, count=1)` | `DoIncrementalSearch(direction, count=1)` | ✅ |
| `accept_search()` | `AcceptSearch()` | ✅ |
| `_get_reverse_search_links(layout)` | `GetReverseSearchLinks(layout)` | ✅ (private) |
