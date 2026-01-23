# Feature 34: Search

## Overview

Implement the search system for incremental search through buffers, including search state management and search direction control.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/search.py`

## Public API

### SearchDirection Enum

```csharp
namespace Stroke.Search;

/// <summary>
/// Direction for search operations.
/// </summary>
public enum SearchDirection
{
    /// <summary>
    /// Search forward (toward end of document).
    /// </summary>
    Forward,

    /// <summary>
    /// Search backward (toward beginning of document).
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
/// Every searchable BufferControl points to a search_buffer_control
/// which represents the search field. The SearchState attached to
/// that search field stores the current search query.
/// </summary>
public sealed class SearchState
{
    /// <summary>
    /// Creates a SearchState.
    /// </summary>
    /// <param name="text">Initial search text.</param>
    /// <param name="direction">Search direction.</param>
    /// <param name="ignoreCase">Case-insensitive filter.</param>
    public SearchState(
        string text = "",
        SearchDirection direction = SearchDirection.Forward,
        object? ignoreCase = null);

    /// <summary>
    /// The search text.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// The search direction.
    /// </summary>
    public SearchDirection Direction { get; set; }

    /// <summary>
    /// Filter for case-insensitive search.
    /// </summary>
    public IFilter IgnoreCase { get; }

    /// <summary>
    /// Create an inverted SearchState (forward becomes backward).
    /// </summary>
    public static SearchState operator ~(SearchState state);
}
```

### Search Functions

```csharp
namespace Stroke.Search;

/// <summary>
/// Search operations for the application.
/// </summary>
public static class SearchOperations
{
    /// <summary>
    /// Start search through the given buffer control.
    /// </summary>
    /// <param name="bufferControl">BufferControl to search (null for current).</param>
    /// <param name="direction">Search direction.</param>
    public static void StartSearch(
        BufferControl? bufferControl = null,
        SearchDirection direction = SearchDirection.Forward);

    /// <summary>
    /// Stop search through the given buffer control.
    /// </summary>
    /// <param name="bufferControl">BufferControl to stop searching.</param>
    public static void StopSearch(BufferControl? bufferControl = null);

    /// <summary>
    /// Apply incremental search and keep search buffer focused.
    /// </summary>
    /// <param name="direction">Search direction.</param>
    /// <param name="count">Number of matches to skip.</param>
    public static void DoIncrementalSearch(
        SearchDirection direction,
        int count = 1);

    /// <summary>
    /// Accept current search query and focus original buffer.
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
    └── SearchOperations.cs
tests/Stroke.Tests/
└── Search/
    ├── SearchStateTests.cs
    └── SearchOperationsTests.cs
```

## Implementation Notes

### Search Flow

1. **Start Search**:
   - Get buffer control (current if not specified)
   - Get associated search buffer control
   - Set search direction on search state
   - Focus the search buffer control
   - Register search link in layout
   - Switch to Vi insert mode if in Vi mode

2. **During Search** (incremental):
   - Update search text from search buffer
   - Apply search to target buffer
   - Highlight matches via HighlightSearchProcessor

3. **Accept Search**:
   - Apply final search to target buffer
   - Add query to search history
   - Stop search and focus original buffer

4. **Stop Search**:
   - Focus original buffer control
   - Remove search link from layout
   - Reset search buffer
   - Switch to Vi navigation mode

### Search Links

Layout maintains `SearchLinks` dictionary:
- Key: `SearchBufferControl` (the search field)
- Value: `BufferControl` (the target being searched)

This allows multiple buffer controls to share one search field, or have independent search fields.

### Vi Mode Integration

When in Vi mode:
- `start_search`: Switch to INSERT mode
- `stop_search`: Switch to NAVIGATION mode

This allows typing in the search field naturally.

### Invert Operator

The `~` operator creates an inverted SearchState:
- Forward becomes Backward
- Backward becomes Forward
- Text and ignore_case preserved

Used for reverse search (like Vim's `N` vs `n`).

### Case Sensitivity

`IgnoreCase` is a filter that can be:
- A boolean value
- A filter that returns boolean dynamically
- Used by buffer search to determine match behavior

## Dependencies

- `Stroke.Layout.BufferControl` (Feature 26) - Buffer control
- `Stroke.Layout.SearchBufferControl` (Feature 26) - Search buffer
- `Stroke.Layout.Layout` (Feature 29) - Layout and focus
- `Stroke.KeyBinding.ViState` (Feature 38) - Vi mode state
- `Stroke.Filters` (Feature 12) - Filter system

## Implementation Tasks

1. Implement `SearchDirection` enum
2. Implement `SearchState` class with invert operator
3. Implement `StartSearch` function
4. Implement `StopSearch` function
5. Implement `DoIncrementalSearch` function
6. Implement `AcceptSearch` function
7. Integrate with Layout search links
8. Integrate with Vi mode state changes
9. Write comprehensive unit tests

## Acceptance Criteria

- [ ] SearchState matches Python Prompt Toolkit semantics
- [ ] Start/Stop search works correctly
- [ ] Incremental search updates as user types
- [ ] Accept search applies final result
- [ ] Search links are managed correctly
- [ ] Vi mode integration works
- [ ] Case sensitivity filter works
- [ ] Unit tests achieve 80% coverage
