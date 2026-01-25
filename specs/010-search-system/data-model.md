# Data Model: Search System

**Feature**: 010-search-system
**Date**: 2026-01-25
**Status**: Complete

## Overview

The Search System defines two primary entities: `SearchState` (mutable query state) and `SearchDirection` (enum). This document specifies the complete data model for these types.

## Entity: SearchDirection

**Type**: Enumeration
**Namespace**: `Stroke.Core`
**Status**: Already implemented (complete)

### Values

| Value | Integer | Description |
|-------|---------|-------------|
| `Forward` | 0 | Search from cursor position toward end of buffer |
| `Backward` | 1 | Search from cursor position toward beginning of buffer |

### C# Definition

```csharp
namespace Stroke.Core;

/// <summary>
/// Search direction for text search operations.
/// </summary>
public enum SearchDirection
{
    /// <summary>Search forward from cursor position.</summary>
    Forward,

    /// <summary>Search backward from cursor position.</summary>
    Backward
}
```

---

## Entity: SearchState

**Type**: Mutable class (intentionally mutable per Python design)
**Namespace**: `Stroke.Core`
**Status**: Enhancement required

### Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `_text` | `string` | `""` | Private backing field for search text |
| `_direction` | `SearchDirection` | `Forward` | Private backing field for search direction |
| `_ignoreCaseFilter` | `Func<bool>?` | `null` | Private backing field for case-insensitivity filter |
| `_lock` | `Lock` | `new()` | Synchronization primitive (readonly) |

### Properties

| Property | Type | Access | Thread-Safe | Description |
|----------|------|--------|-------------|-------------|
| `Text` | `string` | get/set | Yes | The search query text |
| `Direction` | `SearchDirection` | get/set | Yes | Forward or backward search |
| `IgnoreCaseFilter` | `Func<bool>?` | get/set | Yes | Filter determining case sensitivity |

### Methods

| Method | Signature | Thread-Safe | Description |
|--------|-----------|-------------|-------------|
| Constructor | `SearchState(string text = "", SearchDirection direction = Forward, Func<bool>? ignoreCase = null)` | N/A | Initializes a new instance |
| `IgnoreCase` | `bool IgnoreCase()` | Yes | Evaluates the ignore case filter |
| `Invert` | `SearchState Invert()` | Yes | Returns new state with reversed direction |
| `ToString` | `string ToString()` | Yes | Returns debug representation |

### State Transitions

```
┌─────────────────────────────────────────────┐
│              SearchState                     │
├─────────────────────────────────────────────┤
│ Text: ""                                     │
│ Direction: Forward                           │
│ IgnoreCaseFilter: null                       │
└─────────────────────────────────────────────┘
                    │
                    │ Set Text = "hello"
                    ▼
┌─────────────────────────────────────────────┐
│              SearchState                     │
├─────────────────────────────────────────────┤
│ Text: "hello"                                │
│ Direction: Forward                           │
│ IgnoreCaseFilter: null                       │
└─────────────────────────────────────────────┘
                    │
                    │ Invert()
                    ▼
┌─────────────────────────────────────────────┐
│          NEW SearchState                     │
├─────────────────────────────────────────────┤
│ Text: "hello"        (preserved)             │
│ Direction: Backward  (reversed)              │
│ IgnoreCaseFilter: null (preserved)           │
└─────────────────────────────────────────────┘
```

### Validation Rules

| Rule | Condition | Behavior |
|------|-----------|----------|
| Text null → empty | `text == null` | Convert to `""` |
| IgnoreCase null → false | `IgnoreCaseFilter == null` | `IgnoreCase()` returns `false` |

### C# Definition (Enhanced)

```csharp
namespace Stroke.Core;

/// <summary>
/// A search 'query', associated with a search field (like a SearchToolbar).
/// </summary>
/// <remarks>
/// <para>
/// Every searchable <see cref="BufferControl"/> points to a <c>search_buffer_control</c>
/// (another <see cref="BufferControl"/>) which represents the search field. The
/// <see cref="SearchState"/> attached to that search field is used for storing the current
/// search query.
/// </para>
/// <para>
/// It is possible to have one search field for multiple <see cref="BufferControl"/>s. In
/// that case, they'll share the same <see cref="SearchState"/>.
/// If there are multiple <see cref="BufferControl"/>s that display the same <see cref="Buffer"/>,
/// then they can have a different <see cref="SearchState"/> each (if they have a different
/// search control).
/// </para>
/// <para>
/// This class is thread-safe. All property access is synchronized.
/// </para>
/// </remarks>
public sealed class SearchState
{
    private readonly Lock _lock = new();
    private string _text;
    private SearchDirection _direction;
    private Func<bool>? _ignoreCaseFilter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchState"/> class.
    /// </summary>
    /// <param name="text">The search text.</param>
    /// <param name="direction">The search direction.</param>
    /// <param name="ignoreCase">The filter function for case-insensitive search.</param>
    public SearchState(
        string text = "",
        SearchDirection direction = SearchDirection.Forward,
        Func<bool>? ignoreCase = null)
    {
        _text = text ?? "";
        _direction = direction;
        _ignoreCaseFilter = ignoreCase;
    }

    /// <summary>
    /// Gets or sets the search text.
    /// </summary>
    public string Text
    {
        get { using (_lock.EnterScope()) return _text; }
        set { using (_lock.EnterScope()) _text = value ?? ""; }
    }

    /// <summary>
    /// Gets or sets the search direction.
    /// </summary>
    public SearchDirection Direction
    {
        get { using (_lock.EnterScope()) return _direction; }
        set { using (_lock.EnterScope()) _direction = value; }
    }

    /// <summary>
    /// Gets or sets the filter function for case-insensitive search.
    /// </summary>
    public Func<bool>? IgnoreCaseFilter
    {
        get { using (_lock.EnterScope()) return _ignoreCaseFilter; }
        set { using (_lock.EnterScope()) _ignoreCaseFilter = value; }
    }

    /// <summary>
    /// Gets a value indicating whether to ignore case during search.
    /// </summary>
    /// <returns>True if the search should ignore case; otherwise, false.</returns>
    public bool IgnoreCase()
    {
        Func<bool>? filter;
        using (_lock.EnterScope())
        {
            filter = _ignoreCaseFilter;
        }
        return filter?.Invoke() ?? false;
    }

    /// <summary>
    /// Creates a new SearchState where backwards becomes forwards and vice versa.
    /// </summary>
    /// <returns>A new <see cref="SearchState"/> with reversed direction.</returns>
    public SearchState Invert()
    {
        using (_lock.EnterScope())
        {
            var newDirection = _direction == SearchDirection.Backward
                ? SearchDirection.Forward
                : SearchDirection.Backward;

            return new SearchState(_text, newDirection, _ignoreCaseFilter);
        }
    }

    /// <summary>
    /// Returns a string representation of the search state for debugging.
    /// </summary>
    /// <returns>A string showing the text, direction, and ignore case value.</returns>
    public override string ToString()
    {
        using (_lock.EnterScope())
        {
            var ignoreCase = _ignoreCaseFilter?.Invoke() ?? false;
            return $"SearchState(\"{_text}\", direction={_direction}, ignoreCase={ignoreCase})";
        }
    }
}
```

---

## Entity: SearchOperations

**Type**: Static utility class
**Namespace**: `Stroke.Core`
**Status**: New (stub implementation)

### Methods

| Method | Signature | Dependencies | Status |
|--------|-----------|--------------|--------|
| `StartSearch` | `void StartSearch(SearchDirection direction = Forward)` | Layout, Application | Stub |
| `StopSearch` | `void StopSearch()` | Layout, Application | Stub |
| `DoIncrementalSearch` | `void DoIncrementalSearch(SearchDirection direction, int count = 1)` | Layout, Filters | Stub |
| `AcceptSearch` | `void AcceptSearch()` | Layout | Stub |

### Stub Implementation

All methods throw `NotImplementedException` with message indicating required dependencies:
- Features 12 (Filters), 20 (Layout), 35 (Application) must be implemented first

---

## Relationships

```
┌─────────────────┐        uses          ┌─────────────────┐
│  SearchState    │ ─────────────────────│ SearchDirection │
│                 │                       │    (enum)       │
│  - Text         │                       │                 │
│  - Direction ◄──┼───────────────────────┤  - Forward      │
│  - IgnoreCase   │                       │  - Backward     │
└────────┬────────┘                       └─────────────────┘
         │
         │ used by
         ▼
┌─────────────────┐        uses          ┌─────────────────┐
│     Buffer      │ ─────────────────────│  SearchState    │
│                 │                       │                 │
│ - Search()      │                       │                 │
│ - ApplySearch() │                       │                 │
│ - GetSearchPos()│                       │                 │
└─────────────────┘                       └─────────────────┘
         │
         │ used by (future)
         ▼
┌─────────────────┐        uses          ┌─────────────────┐
│SearchOperations │ ─────────────────────│  SearchState    │
│   (static)      │                       │                 │
│                 │      requires         │                 │
│ - StartSearch() ├──────────────────────►│ Layout (F20)   │
│ - StopSearch()  │                       │ Application(35)│
│ - DoIncremental │                       │ Filters (F12)  │
│ - AcceptSearch()│                       │                 │
└─────────────────┘                       └─────────────────┘
```

## Thread Safety Guarantees

| Type | Thread Safety | Mechanism |
|------|---------------|-----------|
| `SearchDirection` | Inherently safe | Enum (immutable) |
| `SearchState` | Thread-safe | Lock synchronization on all property access |
| `SearchOperations` | Thread-safe | Stateless static methods (stubs throw) |

## Migration Notes

### From Existing Stub

The existing `SearchState.cs` is a minimal stub. Changes required:

1. Add `private readonly Lock _lock = new();`
2. Add private backing fields for all properties
3. Wrap property getters/setters with `using (_lock.EnterScope())`
4. Add constructor parameter `Func<bool>? ignoreCase`
5. Add `Invert()` method
6. Add `ToString()` override

### Breaking Changes

None. All changes are additive:
- Constructor gains optional `ignoreCase` parameter (default null = backward compatible)
- New `Invert()` method
- New `ToString()` override
- Thread safety is internal implementation detail
