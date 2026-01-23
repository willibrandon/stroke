# Feature 81: Search

## Overview

Implement search operations for incremental search, search state management, and search navigation in buffers.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/search.py`

## Public API

### SearchDirection Enum

```csharp
namespace Stroke;

/// <summary>
/// Direction of search.
/// </summary>
public enum SearchDirection
{
    /// <summary>
    /// Search forward from cursor.
    /// </summary>
    Forward,

    /// <summary>
    /// Search backward from cursor.
    /// </summary>
    Backward
}
```

### SearchState

```csharp
namespace Stroke;

/// <summary>
/// A search query with direction and options.
/// Associated with a search field (like SearchToolbar).
/// </summary>
public sealed class SearchState
{
    /// <summary>
    /// The search text pattern.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// The search direction.
    /// </summary>
    public SearchDirection Direction { get; set; }

    /// <summary>
    /// Whether to ignore case when searching.
    /// </summary>
    public IFilter IgnoreCase { get; }

    /// <summary>
    /// Create a search state.
    /// </summary>
    /// <param name="text">Initial search text.</param>
    /// <param name="direction">Search direction.</param>
    /// <param name="ignoreCase">Case sensitivity filter.</param>
    public SearchState(
        string text = "",
        SearchDirection direction = SearchDirection.Forward,
        IFilter? ignoreCase = null);

    /// <summary>
    /// Create an inverted search state (opposite direction).
    /// </summary>
    public static SearchState operator ~(SearchState state);
}
```

### Search Functions

```csharp
namespace Stroke;

/// <summary>
/// Search operations for buffers.
/// </summary>
public static class Search
{
    /// <summary>
    /// Start searching in the given buffer control.
    /// </summary>
    /// <param name="bufferControl">The buffer control to search in.</param>
    /// <param name="direction">Initial search direction.</param>
    public static void StartSearch(
        BufferControl? bufferControl = null,
        SearchDirection direction = SearchDirection.Forward);

    /// <summary>
    /// Stop the current search.
    /// </summary>
    /// <param name="bufferControl">The buffer control to stop searching.</param>
    public static void StopSearch(BufferControl? bufferControl = null);

    /// <summary>
    /// Perform incremental search step.
    /// </summary>
    /// <param name="direction">Direction to search.</param>
    /// <param name="count">Number of matches to skip.</param>
    public static void DoIncrementalSearch(
        SearchDirection direction,
        int count = 1);

    /// <summary>
    /// Accept the current search query and return to the buffer.
    /// </summary>
    public static void AcceptSearch();
}
```

## Project Structure

```
src/Stroke/
├── SearchDirection.cs
├── SearchState.cs
└── Search.cs
tests/Stroke.Tests/
└── SearchTests.cs
```

## Implementation Notes

### SearchState Implementation

```csharp
public sealed class SearchState
{
    public string Text { get; set; }
    public SearchDirection Direction { get; set; }
    public IFilter IgnoreCase { get; }

    public SearchState(
        string text = "",
        SearchDirection direction = SearchDirection.Forward,
        IFilter? ignoreCase = null)
    {
        Text = text;
        Direction = direction;
        IgnoreCase = ignoreCase ?? Filters.Never;
    }

    public static SearchState operator ~(SearchState state)
    {
        var newDirection = state.Direction == SearchDirection.Forward
            ? SearchDirection.Backward
            : SearchDirection.Forward;

        return new SearchState(state.Text, newDirection, state.IgnoreCase);
    }

    public override string ToString() =>
        $"SearchState({Text}, direction={Direction}, ignoreCase={IgnoreCase})";
}
```

### Start Search Operation

```csharp
public static void StartSearch(
    BufferControl? bufferControl = null,
    SearchDirection direction = SearchDirection.Forward)
{
    var app = Application.Current;
    var layout = app.Layout;

    // Get current buffer control if not specified
    if (bufferControl == null)
    {
        if (layout.CurrentControl is not BufferControl bc)
            return;
        bufferControl = bc;
    }

    // Get the search buffer control
    var searchBufferControl = bufferControl.SearchBufferControl;
    if (searchBufferControl == null)
        return;

    // Set search direction
    bufferControl.SearchState.Direction = direction;

    // Focus search control
    layout.Focus(searchBufferControl);

    // Remember search link
    layout.SearchLinks[searchBufferControl] = bufferControl;

    // Enter insert mode for Vi
    if (app.ViState != null)
        app.ViState.InputMode = InputMode.Insert;
}
```

### Incremental Search

```csharp
public static void DoIncrementalSearch(SearchDirection direction, int count = 1)
{
    var app = Application.Current;
    var layout = app.Layout;

    var searchControl = layout.CurrentControl as BufferControl;
    if (searchControl == null)
        return;

    var targetControl = layout.SearchTargetBufferControl;
    if (targetControl == null)
        return;

    var searchState = targetControl.SearchState;

    // Check if direction changed
    var directionChanged = searchState.Direction != direction;

    // Update search state
    searchState.Text = searchControl.Buffer.Text;
    searchState.Direction = direction;

    // Apply search if direction didn't change
    if (!directionChanged)
    {
        targetControl.Buffer.ApplySearch(
            searchState,
            includeCurrentPosition: false,
            count: count);
    }
}
```

### Buffer Search Integration

```csharp
// In Buffer
public void ApplySearch(
    SearchState searchState,
    bool includeCurrentPosition = true,
    int count = 1)
{
    if (string.IsNullOrEmpty(searchState.Text))
        return;

    var comparison = searchState.IgnoreCase()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    for (var i = 0; i < count; i++)
    {
        int foundPos = searchState.Direction == SearchDirection.Forward
            ? FindNext(searchState.Text, includeCurrentPosition, comparison)
            : FindPrevious(searchState.Text, includeCurrentPosition, comparison);

        if (foundPos >= 0)
        {
            CursorPosition = foundPos;
            includeCurrentPosition = false;
        }
    }
}
```

## Dependencies

- Feature 26: Filters (IFilter)
- Feature 3: Buffer (search operations)
- Feature 50: BufferControl (search integration)
- Feature 33: Vi state (input mode)

## Implementation Tasks

1. Implement `SearchDirection` enum
2. Implement `SearchState` class with inversion operator
3. Implement `StartSearch` function
4. Implement `StopSearch` function
5. Implement `DoIncrementalSearch` function
6. Implement `AcceptSearch` function
7. Add search methods to Buffer
8. Integrate with Layout for search links
9. Write unit tests

## Acceptance Criteria

- [ ] SearchDirection has Forward and Backward values
- [ ] SearchState stores text, direction, and case option
- [ ] Inversion operator (~) reverses direction
- [ ] StartSearch focuses the search control
- [ ] StopSearch returns focus to buffer
- [ ] DoIncrementalSearch updates as user types
- [ ] AcceptSearch commits the search
- [ ] Buffer.ApplySearch finds matches correctly
- [ ] Unit tests achieve 80% coverage
