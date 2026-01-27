# Feature 09: Search System

## Overview

Implement the search operations for searching through buffer content and history.

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
    Forward,
    Backward
}
```

### SearchState Class

```csharp
namespace Stroke.Search;

/// <summary>
/// A search 'query', associated with a search field (like a SearchToolbar).
///
/// Every searchable BufferControl points to a search_buffer_control
/// (another BufferControl) which represents the search field. The
/// SearchState attached to that search field is used for storing the current
/// search query.
/// </summary>
public sealed class SearchState
{
    /// <summary>
    /// Creates a search state.
    /// </summary>
    /// <param name="text">The search text.</param>
    /// <param name="direction">The search direction.</param>
    /// <param name="ignoreCase">Whether to ignore case.</param>
    public SearchState(
        string text = "",
        SearchDirection direction = SearchDirection.Forward,
        Func<bool>? ignoreCase = null);

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
    public Func<bool> IgnoreCase { get; }

    /// <summary>
    /// Create a new SearchState with reversed direction.
    /// </summary>
    public SearchState Invert();

    public override string ToString();
}
```

### Search Functions

```csharp
namespace Stroke.Search;

/// <summary>
/// Search operations for BufferControl.
/// </summary>
public static class SearchOperations
{
    /// <summary>
    /// Start search through the given buffer control using the search buffer control.
    /// </summary>
    /// <param name="bufferControl">Start search for this BufferControl. If null, search through the current control.</param>
    /// <param name="direction">The search direction.</param>
    public static void StartSearch(
        IBufferControl? bufferControl = null,
        SearchDirection direction = SearchDirection.Forward);

    /// <summary>
    /// Stop search through the given buffer control.
    /// </summary>
    /// <param name="bufferControl">The buffer control to stop searching.</param>
    public static void StopSearch(IBufferControl? bufferControl = null);

    /// <summary>
    /// Apply search, but keep search buffer focused.
    /// </summary>
    /// <param name="direction">The search direction.</param>
    /// <param name="count">Number of matches to advance.</param>
    public static void DoIncrementalSearch(SearchDirection direction, int count = 1);

    /// <summary>
    /// Accept current search query. Focus original BufferControl again.
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
    ├── SearchDirectionTests.cs
    ├── SearchStateTests.cs
    └── SearchOperationsTests.cs
```

## Implementation Notes

### SearchState Inversion

The `Invert()` method returns a new `SearchState` with the opposite direction:
- `Forward` → `Backward`
- `Backward` → `Forward`

The text and ignoreCase filter are preserved.

### Integration with Layout

The search functions interact with the application layout:
- `StartSearch` focuses the search buffer control
- `StopSearch` returns focus to the original buffer
- `DoIncrementalSearch` updates the buffer's search position without changing focus
- `AcceptSearch` applies the search and returns focus

### Vi Mode Integration

When starting/stopping search in Vi mode, the input mode is updated:
- Start search → Insert mode
- Stop search → Navigation mode

## Dependencies

- `Stroke.Core.Buffer` (Feature 06)
- `Stroke.Filters` (Feature 12)
- `Stroke.Layout.Controls` (Feature 20)
- `Stroke.Application` (Feature 31)

## Implementation Tasks

1. Implement `SearchDirection` enum
2. Implement `SearchState` class with inversion
3. Implement `SearchOperations` static class
4. Write comprehensive unit tests

## Acceptance Criteria

- [ ] SearchState matches Python Prompt Toolkit semantics
- [ ] Search direction inversion works correctly
- [ ] Search operations integrate with layout correctly
- [ ] Vi mode integration works correctly
- [ ] Unit tests achieve 80% coverage
