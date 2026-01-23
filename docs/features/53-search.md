# Feature 53: Search System

## Overview

Implement the search operations including SearchDirection, SearchState, and functions for starting, stopping, and applying incremental search through buffer contents.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/search.py`

## Public API

### SearchDirection Enum

```csharp
namespace Stroke.Search;

/// <summary>
/// Direction of search.
/// </summary>
public enum SearchDirection
{
    /// <summary>
    /// Search forward through the buffer.
    /// </summary>
    Forward,

    /// <summary>
    /// Search backward through the buffer.
    /// </summary>
    Backward
}
```

### SearchState Class

```csharp
namespace Stroke.Search;

/// <summary>
/// A search query associated with a search field.
///
/// Every searchable BufferControl points to a SearchBufferControl
/// which represents the search field. The SearchState attached to that
/// search field is used for storing the current search query.
///
/// It is possible to have one search field for multiple BufferControls.
/// In that case, they'll share the same SearchState.
/// </summary>
public sealed class SearchState
{
    /// <summary>
    /// Creates a SearchState.
    /// </summary>
    /// <param name="text">The search text.</param>
    /// <param name="direction">Search direction.</param>
    /// <param name="ignoreCase">Case insensitive search filter.</param>
    public SearchState(
        string text = "",
        SearchDirection direction = SearchDirection.Forward,
        IFilter? ignoreCase = null);

    /// <summary>
    /// The search text.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Search direction.
    /// </summary>
    public SearchDirection Direction { get; set; }

    /// <summary>
    /// Case insensitive search filter.
    /// </summary>
    public IFilter IgnoreCase { get; }

    /// <summary>
    /// Create a new SearchState with reversed direction.
    /// </summary>
    public static SearchState operator ~(SearchState state);
}
```

### Search Functions

```csharp
namespace Stroke.Search;

public static class SearchFunctions
{
    /// <summary>
    /// Start search through the given BufferControl.
    /// </summary>
    /// <param name="bufferControl">BufferControl to search (null for current).</param>
    /// <param name="direction">Search direction.</param>
    public static void StartSearch(
        BufferControl? bufferControl = null,
        SearchDirection direction = SearchDirection.Forward);

    /// <summary>
    /// Stop search through the given BufferControl.
    /// </summary>
    /// <param name="bufferControl">BufferControl to stop searching.</param>
    public static void StopSearch(BufferControl? bufferControl = null);

    /// <summary>
    /// Apply search but keep search buffer focused.
    /// </summary>
    /// <param name="direction">Search direction.</param>
    /// <param name="count">Number of matches to skip.</param>
    public static void DoIncrementalSearch(
        SearchDirection direction,
        int count = 1);

    /// <summary>
    /// Accept current search query and focus original BufferControl.
    /// </summary>
    public static void AcceptSearch();
}
```

## Project Structure

```
src/Stroke/
└── Search/
    ├── SearchDirection.cs
    ├── SearchState.cs
    └── SearchFunctions.cs
tests/Stroke.Tests/
└── Search/
    ├── SearchStateTests.cs
    └── SearchFunctionsTests.cs
```

## Implementation Notes

### SearchState Inversion

The `~` operator creates a reversed search state:

```csharp
public static SearchState operator ~(SearchState state)
{
    var newDirection = state.Direction == SearchDirection.Forward
        ? SearchDirection.Backward
        : SearchDirection.Forward;

    return new SearchState(state.Text, newDirection, state.IgnoreCase);
}
```

### StartSearch Flow

1. Get current `BufferControl` if none specified
2. Check if control is searchable (has `SearchBufferControl`)
3. Set search direction in `SearchState`
4. Focus the search `BufferControl`
5. Register in `layout.SearchLinks`
6. Set Vi mode to Insert (if in Vi mode)

```csharp
public static void StartSearch(
    BufferControl? bufferControl = null,
    SearchDirection direction = SearchDirection.Forward)
{
    var layout = GetApp().Layout;

    if (bufferControl == null)
    {
        if (layout.CurrentControl is not BufferControl bc)
            return;
        bufferControl = bc;
    }

    var searchControl = bufferControl.SearchBufferControl;
    if (searchControl == null)
        return;

    bufferControl.SearchState.Direction = direction;
    layout.Focus(searchControl);
    layout.SearchLinks[searchControl] = bufferControl;

    GetApp().ViState.InputMode = InputMode.Insert;
}
```

### StopSearch Flow

1. Get target `BufferControl` from search links
2. Focus the original `BufferControl`
3. Remove from `SearchLinks`
4. Reset search buffer
5. Set Vi mode to Navigation (if in Vi mode)

### DoIncrementalSearch

Updates search state and applies to buffer without leaving search mode:

```csharp
public static void DoIncrementalSearch(SearchDirection direction, int count = 1)
{
    var layout = GetApp().Layout;
    var searchControl = layout.CurrentControl as BufferControl;
    if (searchControl == null)
        return;

    var targetControl = layout.SearchTargetBufferControl;
    if (targetControl == null)
        return;

    var searchState = targetControl.SearchState;
    bool directionChanged = searchState.Direction != direction;

    searchState.Text = searchControl.Buffer.Text;
    searchState.Direction = direction;

    if (!directionChanged)
    {
        targetControl.Buffer.ApplySearch(
            searchState,
            includeCurrentPosition: false,
            count: count);
    }
}
```

### AcceptSearch

1. Update search state text from search buffer
2. Apply search to target buffer
3. Add query to search history
4. Call StopSearch

### SearchLinks

The `Layout.SearchLinks` dictionary maps:
- Key: `SearchBufferControl` (the search input field)
- Value: `BufferControl` (the buffer being searched)

This allows multiple buffers to share a search field, or vice versa.

### Integration with BufferControl

`BufferControl` has these search-related properties:

```csharp
public SearchBufferControl? SearchBufferControl { get; }
public SearchState SearchState { get; }
```

### Integration with Buffer

`Buffer` has the `ApplySearch` method:

```csharp
public void ApplySearch(
    SearchState searchState,
    bool includeCurrentPosition = true,
    int count = 1);
```

## Dependencies

- `Stroke.Application.Current` (Feature 49) - GetApp()
- `Stroke.Layout.Controls` (Feature 26) - BufferControl
- `Stroke.Layout.Layout` (Feature 29) - Layout class
- `Stroke.Filters` (Feature 12) - Filter system
- `Stroke.KeyBinding.ViState` (Feature 38) - Vi input mode

## Implementation Tasks

1. Implement `SearchDirection` enum
2. Implement `SearchState` class
3. Implement `SearchState` inversion operator
4. Implement `StartSearch` function
5. Implement `StopSearch` function
6. Implement `DoIncrementalSearch` function
7. Implement `AcceptSearch` function
8. Integrate with Layout.SearchLinks
9. Write comprehensive unit tests

## Acceptance Criteria

- [ ] SearchDirection enum has Forward and Backward
- [ ] SearchState stores text, direction, ignoreCase
- [ ] Inversion operator reverses direction
- [ ] StartSearch focuses search control
- [ ] StopSearch returns focus to original control
- [ ] DoIncrementalSearch updates without leaving search
- [ ] AcceptSearch applies and saves to history
- [ ] SearchLinks tracks search relationships
- [ ] Unit tests achieve 80% coverage
