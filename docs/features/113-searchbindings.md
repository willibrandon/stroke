# Feature 113: Search Bindings

## Overview

Implement search-related key binding functions for incremental search operations including starting, aborting, accepting search, and navigating through search results.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/search.py`

## Public API

### Search Binding Functions

```csharp
namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Search-related key binding functions.
/// </summary>
public static class SearchBindings
{
    /// <summary>
    /// Abort an incremental search and restore the original line.
    /// Usually bound to Ctrl+G or Ctrl+C.
    /// </summary>
    /// <remarks>
    /// Filter: IsSearching
    /// </remarks>
    [KeyBinding(Filter = nameof(Filters.IsSearching))]
    public static void AbortSearch(KeyPressEvent e);

    /// <summary>
    /// Accept the search and exit search mode.
    /// Usually bound to Enter.
    /// </summary>
    /// <remarks>
    /// Filter: IsSearching
    /// </remarks>
    [KeyBinding(Filter = nameof(Filters.IsSearching))]
    public static void AcceptSearch(KeyPressEvent e);

    /// <summary>
    /// Enter reverse incremental search.
    /// Usually bound to Ctrl+R.
    /// </summary>
    /// <remarks>
    /// Filter: ControlIsSearchable
    /// </remarks>
    [KeyBinding(Filter = nameof(Filters.ControlIsSearchable))]
    public static void StartReverseIncrementalSearch(KeyPressEvent e);

    /// <summary>
    /// Enter forward incremental search.
    /// Usually bound to Ctrl+S.
    /// </summary>
    /// <remarks>
    /// Filter: ControlIsSearchable
    /// </remarks>
    [KeyBinding(Filter = nameof(Filters.ControlIsSearchable))]
    public static void StartForwardIncrementalSearch(KeyPressEvent e);

    /// <summary>
    /// Apply reverse incremental search, keeping search buffer focused.
    /// </summary>
    /// <remarks>
    /// Filter: IsSearching
    /// </remarks>
    [KeyBinding(Filter = nameof(Filters.IsSearching))]
    public static void ReverseIncrementalSearch(KeyPressEvent e);

    /// <summary>
    /// Apply forward incremental search, keeping search buffer focused.
    /// </summary>
    /// <remarks>
    /// Filter: IsSearching
    /// </remarks>
    [KeyBinding(Filter = nameof(Filters.IsSearching))]
    public static void ForwardIncrementalSearch(KeyPressEvent e);

    /// <summary>
    /// Accept the search operation, then accept the input.
    /// </summary>
    /// <remarks>
    /// Filter: IsSearching and previous buffer is returnable
    /// </remarks>
    [KeyBinding(Filter = "IsSearching & PreviousBufferIsReturnable")]
    public static void AcceptSearchAndAcceptInput(KeyPressEvent e);
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Bindings/
        └── SearchBindings.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        └── SearchBindingsTests.cs
```

## Implementation Notes

### SearchBindings Implementation

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class SearchBindings
{
    [KeyBinding(Filter = nameof(Filters.IsSearching))]
    public static void AbortSearch(KeyPressEvent e)
    {
        // Abort search and restore original line
        Search.StopSearch();
    }

    [KeyBinding(Filter = nameof(Filters.IsSearching))]
    public static void AcceptSearch(KeyPressEvent e)
    {
        // Accept search and exit search mode
        Search.AcceptSearch();
    }

    [KeyBinding(Filter = nameof(Filters.ControlIsSearchable))]
    public static void StartReverseIncrementalSearch(KeyPressEvent e)
    {
        // Enter reverse incremental search
        Search.StartSearch(SearchDirection.Backward);
    }

    [KeyBinding(Filter = nameof(Filters.ControlIsSearchable))]
    public static void StartForwardIncrementalSearch(KeyPressEvent e)
    {
        // Enter forward incremental search
        Search.StartSearch(SearchDirection.Forward);
    }

    [KeyBinding(Filter = nameof(Filters.IsSearching))]
    public static void ReverseIncrementalSearch(KeyPressEvent e)
    {
        // Apply reverse search with repeat count
        Search.DoIncrementalSearch(SearchDirection.Backward, count: e.Arg);
    }

    [KeyBinding(Filter = nameof(Filters.IsSearching))]
    public static void ForwardIncrementalSearch(KeyPressEvent e)
    {
        // Apply forward search with repeat count
        Search.DoIncrementalSearch(SearchDirection.Forward, count: e.Arg);
    }

    private static Filter PreviousBufferIsReturnable =>
        Filters.Condition(() =>
        {
            var prevControl = GetApp().Layout.SearchTargetBufferControl;
            return prevControl?.Buffer.IsReturnable == true;
        });

    [KeyBinding]
    public static void AcceptSearchAndAcceptInput(KeyPressEvent e)
    {
        // Only callable when IsSearching & PreviousBufferIsReturnable
        Search.AcceptSearch();
        e.CurrentBuffer.ValidateAndHandle();
    }
}
```

### Search Module Reference

These functions use the Search module from Feature 34/53:

```csharp
namespace Stroke;

public static class Search
{
    /// <summary>
    /// Start incremental search.
    /// </summary>
    public static void StartSearch(SearchDirection direction = SearchDirection.Forward);

    /// <summary>
    /// Stop search and restore original state.
    /// </summary>
    public static void StopSearch();

    /// <summary>
    /// Accept search results.
    /// </summary>
    public static void AcceptSearch();

    /// <summary>
    /// Perform incremental search step.
    /// </summary>
    public static void DoIncrementalSearch(SearchDirection direction, int count = 1);
}
```

### Usage in Emacs Bindings

```csharp
public static class EmacsBindings
{
    public static IKeyBindings LoadSearchBindings()
    {
        var kb = new KeyBindings();

        // Start search
        kb.Add(Keys.ControlR, SearchBindings.StartReverseIncrementalSearch);
        kb.Add(Keys.ControlS, SearchBindings.StartForwardIncrementalSearch);

        // While searching
        kb.Add(Keys.ControlG, SearchBindings.AbortSearch);
        kb.Add(Keys.ControlC, SearchBindings.AbortSearch);
        kb.Add(Keys.Enter, SearchBindings.AcceptSearch);

        // Navigate results
        kb.Add(Keys.ControlR, filter: Filters.IsSearching,
            handler: SearchBindings.ReverseIncrementalSearch);
        kb.Add(Keys.ControlS, filter: Filters.IsSearching,
            handler: SearchBindings.ForwardIncrementalSearch);

        return kb;
    }
}
```

## Dependencies

- Feature 9: Search State
- Feature 12: Filters (IsSearching, ControlIsSearchable)
- Feature 19: Key Bindings
- Feature 53: Search Operations

## Implementation Tasks

1. Implement AbortSearch function
2. Implement AcceptSearch function
3. Implement StartReverseIncrementalSearch
4. Implement StartForwardIncrementalSearch
5. Implement ReverseIncrementalSearch
6. Implement ForwardIncrementalSearch
7. Implement AcceptSearchAndAcceptInput
8. Add KeyBinding attributes with filters
9. Write unit tests

## Acceptance Criteria

- [ ] Ctrl+R starts reverse search
- [ ] Ctrl+S starts forward search
- [ ] Ctrl+G/Ctrl+C aborts search
- [ ] Enter accepts search
- [ ] Repeated Ctrl+R/S navigates results
- [ ] Accept and handle works when returnable
- [ ] Arg count affects search navigation
- [ ] Unit tests achieve 80% coverage
