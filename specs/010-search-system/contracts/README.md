# Contracts: Search System

**Feature**: 010-search-system
**Date**: 2026-01-25

## Overview

The Search System is a library feature with no external API surface (no REST/GraphQL endpoints). The "contracts" for this feature are the C# public API signatures.

## Public API Contract

### SearchDirection

```csharp
namespace Stroke.Core;

public enum SearchDirection
{
    Forward,
    Backward
}
```

### SearchState

```csharp
namespace Stroke.Core;

public sealed class SearchState
{
    // Constructor
    public SearchState(
        string text = "",
        SearchDirection direction = SearchDirection.Forward,
        Func<bool>? ignoreCase = null);

    // Properties
    public string Text { get; set; }
    public SearchDirection Direction { get; set; }
    public Func<bool>? IgnoreCaseFilter { get; set; }

    // Methods
    public bool IgnoreCase();
    public SearchState Invert();
    public override string ToString();
}
```

### SearchOperations (Stubs)

```csharp
namespace Stroke.Core;

public static class SearchOperations
{
    public static void StartSearch(SearchDirection direction = SearchDirection.Forward);
    public static void StopSearch();
    public static void DoIncrementalSearch(SearchDirection direction, int count = 1);
    public static void AcceptSearch();
}
```

## Compatibility Guarantees

1. **SearchDirection**: Enum values will not change
2. **SearchState**: Constructor signature is stable; optional parameters ensure backward compatibility
3. **SearchOperations**: Method signatures are stable; implementations will be added when dependencies are available

## Version Notes

- **v1.0 (Feature 10)**: Initial implementation with SearchState complete, SearchOperations stubs
- **Future**: SearchOperations will be fully implemented when Features 12, 20, 35 are complete
