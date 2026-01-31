# Contract: SearchBindings

**Namespace**: `Stroke.Application.Bindings`
**Type**: `public static class`
**Python Source**: `prompt_toolkit/key_binding/bindings/search.py`

## Public API

```csharp
namespace Stroke.Application.Bindings;

/// <summary>
/// Search-related key binding handler functions.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.search</c> module.
/// All functions match the <see cref="KeyHandlerCallable"/> delegate signature.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe.
/// </para>
/// </remarks>
public static class SearchBindings
{
    /// <summary>
    /// Abort an incremental search and restore the original line.
    /// Usually bound to Ctrl+G / Ctrl+C.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="SearchFilters.IsSearching"/></remarks>
    public static NotImplementedOrNone? AbortSearch(KeyPressEvent @event);

    /// <summary>
    /// Accept current search result. When Enter is pressed in isearch, quit isearch mode.
    /// Usually bound to Enter.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="SearchFilters.IsSearching"/></remarks>
    public static NotImplementedOrNone? AcceptSearch(KeyPressEvent @event);

    /// <summary>
    /// Enter reverse incremental search. Usually bound to Ctrl+R.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="SearchFilters.ControlIsSearchable"/></remarks>
    public static NotImplementedOrNone? StartReverseIncrementalSearch(KeyPressEvent @event);

    /// <summary>
    /// Enter forward incremental search. Usually bound to Ctrl+S.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="SearchFilters.ControlIsSearchable"/></remarks>
    public static NotImplementedOrNone? StartForwardIncrementalSearch(KeyPressEvent @event);

    /// <summary>
    /// Apply reverse incremental search, keeping search buffer focused.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="SearchFilters.IsSearching"/></remarks>
    public static NotImplementedOrNone? ReverseIncrementalSearch(KeyPressEvent @event);

    /// <summary>
    /// Apply forward incremental search, keeping search buffer focused.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="SearchFilters.IsSearching"/></remarks>
    public static NotImplementedOrNone? ForwardIncrementalSearch(KeyPressEvent @event);

    /// <summary>
    /// Accept the search operation first, then accept the input.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>
    /// Filter: <see cref="SearchFilters.IsSearching"/> AND PreviousBufferIsReturnable.
    /// Calls AcceptSearch then ValidateAndHandle on the current buffer.
    /// </remarks>
    public static NotImplementedOrNone? AcceptSearchAndAcceptInput(KeyPressEvent @event);
}
```

## Filter Requirements

Each binding function has an associated filter condition:

| Function | Filter | Python Equivalent |
|----------|--------|-------------------|
| AbortSearch | `SearchFilters.IsSearching` | `is_searching` |
| AcceptSearch | `SearchFilters.IsSearching` | `is_searching` |
| StartReverseIncrementalSearch | `SearchFilters.ControlIsSearchable` | `control_is_searchable` |
| StartForwardIncrementalSearch | `SearchFilters.ControlIsSearchable` | `control_is_searchable` |
| ReverseIncrementalSearch | `SearchFilters.IsSearching` | `is_searching` |
| ForwardIncrementalSearch | `SearchFilters.IsSearching` | `is_searching` |
| AcceptSearchAndAcceptInput | `SearchFilters.IsSearching & PreviousBufferIsReturnable` | `is_searching & _previous_buffer_is_returnable` |

### PreviousBufferIsReturnable

A private `Condition` within SearchBindings (or alongside it):

```csharp
/// <summary>
/// True if the previously focused buffer has a return handler.
/// </summary>
private static readonly IFilter PreviousBufferIsReturnable = new Condition(() =>
{
    var prevControl = AppContext.GetApp().Layout.SearchTargetBufferControl;
    return prevControl is not null && prevControl.Buffer.IsReturnable;
});
```

Python equivalent:
```python
@Condition
def _previous_buffer_is_returnable() -> bool:
    prev_control = get_app().layout.search_target_buffer_control
    return bool(prev_control and prev_control.buffer.is_returnable)
```

## Python Reference Mapping

| Python Function | C# Method | Filter Match |
|----------------|-----------|--------------|
| `abort_search(event)` | `AbortSearch(@event)` | ✅ `is_searching` |
| `accept_search(event)` | `AcceptSearch(@event)` | ✅ `is_searching` |
| `start_reverse_incremental_search(event)` | `StartReverseIncrementalSearch(@event)` | ✅ `control_is_searchable` |
| `start_forward_incremental_search(event)` | `StartForwardIncrementalSearch(@event)` | ✅ `control_is_searchable` |
| `reverse_incremental_search(event)` | `ReverseIncrementalSearch(@event)` | ✅ `is_searching` |
| `forward_incremental_search(event)` | `ForwardIncrementalSearch(@event)` | ✅ `is_searching` |
| `accept_search_and_accept_input(event)` | `AcceptSearchAndAcceptInput(@event)` | ✅ `is_searching & _previous_buffer_is_returnable` |
