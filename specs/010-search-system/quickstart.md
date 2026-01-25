# Quickstart: Search System

**Feature**: 010-search-system
**Date**: 2026-01-25

## Overview

The Search System provides text search functionality for Stroke applications. This quickstart demonstrates using `SearchState` with `Buffer` to implement search operations.

## Prerequisites

- .NET 10 SDK
- Stroke library (this project)

## Basic Usage

### Creating a SearchState

```csharp
using Stroke.Core;

// Default search state (empty text, forward direction)
var searchState = new SearchState();

// Search state with initial text
var searchState = new SearchState("hello");

// Search state with direction
var searchState = new SearchState("hello", SearchDirection.Backward);

// Search state with case-insensitivity
var searchState = new SearchState("hello", SearchDirection.Forward, () => true);
```

### Modifying SearchState

SearchState is mutable - properties can be updated during search:

```csharp
var searchState = new SearchState();

// User types each character
searchState.Text = "h";      // Incremental search for "h"
searchState.Text = "he";     // Incremental search for "he"
searchState.Text = "hel";    // Incremental search for "hel"
searchState.Text = "hello";  // Incremental search for "hello"

// Change direction
searchState.Direction = SearchDirection.Backward;

// Toggle case sensitivity
bool ignoreCase = false;
searchState.IgnoreCaseFilter = () => ignoreCase;
ignoreCase = true;  // Future IgnoreCase() calls return true
```

### Inverting Search Direction

```csharp
var forward = new SearchState("hello", SearchDirection.Forward);
var backward = forward.Invert();

Console.WriteLine(forward.Direction);   // Forward
Console.WriteLine(backward.Direction);  // Backward
Console.WriteLine(backward.Text);       // "hello" (preserved)
```

### Using SearchState with Buffer

```csharp
using Stroke.Core;

// Create a buffer with some content
var buffer = new Buffer();
buffer.SetDocument(new Document("The quick brown fox jumps over the lazy dog"));

// Create search state
var searchState = new SearchState("fox");

// Get preview document showing search result
var previewDoc = buffer.DocumentForSearch(searchState);
Console.WriteLine($"Found at position: {previewDoc.CursorPosition}");

// Get search position without modifying buffer
int position = buffer.GetSearchPosition(searchState);
Console.WriteLine($"Search position: {position}");

// Apply search - moves cursor to found position
buffer.ApplySearch(searchState);
Console.WriteLine($"Cursor now at: {buffer.CursorPosition}");

// Search backward from current position
searchState.Direction = SearchDirection.Backward;
buffer.ApplySearch(searchState);
```

### Case-Insensitive Search

```csharp
var buffer = new Buffer();
buffer.SetDocument(new Document("Hello HELLO HeLLo"));

// Case-sensitive search (default)
var searchState = new SearchState("hello");
var position = buffer.GetSearchPosition(searchState);
// Returns -1 or cursor position if no match

// Case-insensitive search
searchState.IgnoreCaseFilter = () => true;
buffer.ApplySearch(searchState);
// Finds "Hello" at position 0
```

### Repeated Search (count parameter)

```csharp
var buffer = new Buffer();
buffer.SetDocument(new Document("aaa bbb aaa ccc aaa"));

var searchState = new SearchState("aaa");

// Find first occurrence
buffer.ApplySearch(searchState, count: 1);  // Position: 0

// Find second occurrence
buffer.ApplySearch(searchState, count: 2);  // Position: 8

// Find third occurrence
buffer.ApplySearch(searchState, count: 3);  // Position: 16
```

### Debugging SearchState

```csharp
var searchState = new SearchState("test", SearchDirection.Forward, () => true);
Console.WriteLine(searchState.ToString());
// Output: SearchState("test", direction=Forward, ignoreCase=True)

var inverted = searchState.Invert();
Console.WriteLine(inverted.ToString());
// Output: SearchState("test", direction=Backward, ignoreCase=True)
```

## Thread Safety

SearchState is thread-safe. All property access is synchronized:

```csharp
var searchState = new SearchState();

// Safe to access from multiple threads
Parallel.For(0, 100, i =>
{
    searchState.Text = $"search{i}";
    var text = searchState.Text;
    var direction = searchState.Direction;
    var inverted = searchState.Invert();
});
```

## SearchOperations (Coming Soon)

The `SearchOperations` static class provides UI-level search lifecycle methods. These require Features 12 (Filters), 20 (Layout), and 35 (Application):

```csharp
// NOT YET AVAILABLE - throws NotImplementedException
// SearchOperations.StartSearch(SearchDirection.Forward);
// SearchOperations.StopSearch();
// SearchOperations.DoIncrementalSearch(SearchDirection.Forward);
// SearchOperations.AcceptSearch();
```

## Common Patterns

### Incremental Search Implementation

```csharp
public void OnSearchTextChanged(string newText)
{
    _searchState.Text = newText;

    // Get preview without modifying buffer state
    var previewDoc = _buffer.DocumentForSearch(_searchState);

    // Update UI to show preview position
    UpdateSearchPreview(previewDoc);
}

public void OnSearchAccepted()
{
    // Apply the search, moving cursor to found position
    _buffer.ApplySearch(_searchState);
}

public void OnSearchCancelled()
{
    // Clear search state
    _searchState.Text = "";
}
```

### Bidirectional Search

```csharp
public void SearchForward()
{
    _searchState.Direction = SearchDirection.Forward;
    _buffer.ApplySearch(_searchState, includeCurrentPosition: false);
}

public void SearchBackward()
{
    _searchState.Direction = SearchDirection.Backward;
    _buffer.ApplySearch(_searchState, includeCurrentPosition: false);
}

public void ReverseSearchDirection()
{
    _searchState = _searchState.Invert();
    _buffer.ApplySearch(_searchState, includeCurrentPosition: false);
}
```

### Search with History Wrapping

Buffer search automatically wraps around history:

```csharp
var buffer = new Buffer();
buffer.History = new InMemoryHistory(["line1: foo", "line2: bar", "line3: foo"]);
buffer.LoadHistoryIfNotYetLoaded();

// Set current content
buffer.SetDocument(new Document("current: baz"));

// Search for "foo" - will wrap to history entries
var searchState = new SearchState("foo");
buffer.ApplySearch(searchState);  // Finds "foo" in history
```

## Error Handling

```csharp
var buffer = new Buffer();
buffer.SetDocument(new Document("Hello World"));

// Empty search text - no operation
var empty = new SearchState("");
buffer.ApplySearch(empty);  // No-op, cursor unchanged

// Text not found - cursor unchanged
var notFound = new SearchState("xyz");
int originalPosition = buffer.CursorPosition;
buffer.ApplySearch(notFound);
Assert.Equal(originalPosition, buffer.CursorPosition);
```

## Best Practices

1. **Reuse SearchState**: Create one SearchState per search session, update Text property
2. **Use DocumentForSearch for preview**: Don't modify buffer state until user accepts
3. **Handle not found gracefully**: Check if position changed after ApplySearch
4. **Use IgnoreCaseFilter delegate**: Allow runtime case sensitivity toggle
5. **Thread safety**: SearchState is safe for concurrent access
