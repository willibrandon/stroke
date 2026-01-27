# Feature 121: Application Filters

## Overview

Implement application-specific filters that query runtime application state. These filters build on the core filter infrastructure (Feature 12) and require access to the Application, ViState, and other runtime components.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/filters/app.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/filters/cli.py`

## Public API

### Application Filters

```csharp
namespace Stroke.Filters;

/// <summary>
/// Built-in filters that check application state.
/// </summary>
public static class AppFilters
{
    /// <summary>
    /// Enable when a buffer with the given name has focus.
    /// </summary>
    public static IFilter HasFocus(string bufferName);

    /// <summary>
    /// Enable when the given buffer has focus.
    /// </summary>
    public static IFilter HasFocus(IBuffer buffer);

    /// <summary>
    /// Enable when the given control has focus.
    /// </summary>
    public static IFilter HasFocus(IUIControl control);

    /// <summary>
    /// Enable when the given container has focus.
    /// </summary>
    public static IFilter HasFocus(IContainer container);

    /// <summary>
    /// Enabled when the currently focused control is a BufferControl.
    /// </summary>
    public static readonly IFilter BufferHasFocus;

    /// <summary>
    /// Enable when the current buffer has a selection.
    /// </summary>
    public static readonly IFilter HasSelection;

    /// <summary>
    /// Enable when the current buffer has a suggestion.
    /// </summary>
    public static readonly IFilter HasSuggestion;

    /// <summary>
    /// Enable when the current buffer has completions.
    /// </summary>
    public static readonly IFilter HasCompletions;

    /// <summary>
    /// True when the user selected a completion.
    /// </summary>
    public static readonly IFilter CompletionIsSelected;

    /// <summary>
    /// True when the current buffer is read only.
    /// </summary>
    public static readonly IFilter IsReadOnly;

    /// <summary>
    /// True when the current buffer has been marked as multiline.
    /// </summary>
    public static readonly IFilter IsMultiline;

    /// <summary>
    /// Current buffer has validation error.
    /// </summary>
    public static readonly IFilter HasValidationError;

    /// <summary>
    /// Enable when the input processor has an 'arg'.
    /// </summary>
    public static readonly IFilter HasArg;

    /// <summary>
    /// True when the CLI is returning, aborting or exiting.
    /// </summary>
    public static readonly IFilter IsDone;

    /// <summary>
    /// Only true when the renderer knows its real height.
    /// </summary>
    public static readonly IFilter RendererHeightIsKnown;

    /// <summary>
    /// Check whether a given editing mode is active.
    /// </summary>
    public static IFilter InEditingMode(EditingMode editingMode);

    /// <summary>
    /// True when paste mode is enabled.
    /// </summary>
    public static readonly IFilter InPasteMode;
}
```

### Vi Mode Filters

```csharp
namespace Stroke.Filters;

/// <summary>
/// Vi-specific filters.
/// </summary>
public static class ViFilters
{
    /// <summary>
    /// True when Vi mode is active.
    /// </summary>
    public static readonly IFilter ViMode;

    /// <summary>
    /// Active when Vi navigation key bindings are active.
    /// </summary>
    public static readonly IFilter ViNavigationMode;

    /// <summary>
    /// Active when Vi insert mode is active.
    /// </summary>
    public static readonly IFilter ViInsertMode;

    /// <summary>
    /// Active when Vi insert-multiple mode is active.
    /// </summary>
    public static readonly IFilter ViInsertMultipleMode;

    /// <summary>
    /// Active when Vi replace mode is active.
    /// </summary>
    public static readonly IFilter ViReplaceMode;

    /// <summary>
    /// Active when Vi replace-single mode is active.
    /// </summary>
    public static readonly IFilter ViReplaceSingleMode;

    /// <summary>
    /// Active when Vi selection mode is active.
    /// </summary>
    public static readonly IFilter ViSelectionMode;

    /// <summary>
    /// Active when waiting for a text object in Vi.
    /// </summary>
    public static readonly IFilter ViWaitingForTextObjectMode;

    /// <summary>
    /// Active when Vi digraph mode is active.
    /// </summary>
    public static readonly IFilter ViDigraphMode;

    /// <summary>
    /// Active when recording a Vi macro.
    /// </summary>
    public static readonly IFilter ViRecordingMacro;

    /// <summary>
    /// When the '/' and '?' bindings for Vi search are reversed.
    /// </summary>
    public static readonly IFilter ViSearchDirectionReversed;
}
```

### Emacs Mode Filters

```csharp
namespace Stroke.Filters;

/// <summary>
/// Emacs-specific filters.
/// </summary>
public static class EmacsFilters
{
    /// <summary>
    /// When the Emacs bindings are active.
    /// </summary>
    public static readonly IFilter EmacsMode;

    /// <summary>
    /// When Emacs insert mode is active.
    /// </summary>
    public static readonly IFilter EmacsInsertMode;

    /// <summary>
    /// When Emacs selection mode is active.
    /// </summary>
    public static readonly IFilter EmacsSelectionMode;
}
```

### Search Filters

```csharp
namespace Stroke.Filters;

/// <summary>
/// Search-related filters.
/// </summary>
public static class SearchFilters
{
    /// <summary>
    /// When we are searching.
    /// </summary>
    public static readonly IFilter IsSearching;

    /// <summary>
    /// When the current UIControl is searchable.
    /// </summary>
    public static readonly IFilter ControlIsSearchable;

    /// <summary>
    /// When shift selection mode is active.
    /// </summary>
    public static readonly IFilter ShiftSelectionMode;
}
```

## Project Structure

```
src/Stroke/
└── Filters/
    ├── AppFilters.cs
    ├── ViFilters.cs
    ├── EmacsFilters.cs
    └── SearchFilters.cs
tests/Stroke.Tests/
└── Filters/
    ├── AppFiltersTests.cs
    ├── ViFiltersTests.cs
    ├── EmacsFiltersTests.cs
    └── SearchFiltersTests.cs
```

## Implementation Notes

### Accessing Application State

All app filters use `GetApp()` to access the current application context:

```csharp
public static readonly IFilter HasSelection = new Condition(() =>
{
    var app = AppContext.CurrentApp;
    return app?.CurrentBuffer?.SelectionState != null;
});
```

### Memoization

Frequently accessed filters should use memoization to avoid repeated allocations:

```csharp
private static readonly Dictionary<string, IFilter> _hasFocusCache = new();

public static IFilter HasFocus(string bufferName)
{
    if (!_hasFocusCache.TryGetValue(bufferName, out var filter))
    {
        filter = new Condition(() =>
            AppContext.CurrentApp?.Layout?.HasFocus(bufferName) ?? false);
        _hasFocusCache[bufferName] = filter;
    }
    return filter;
}
```

## Dependencies

- `Stroke.Filters` (Feature 12) - Core filter infrastructure (IFilter, Condition, etc.)
- `Stroke.Application` (Feature 31) - For accessing current app state
- `Stroke.Core.Buffer` (Feature 06) - For buffer-related filters
- `Stroke.KeyBinding.ViState` (Feature 38) - For Vi mode filters
- `Stroke.KeyBinding.EmacsState` (Feature 39) - For Emacs mode filters

## Implementation Tasks

1. Implement `AppFilters` static class
2. Implement `ViFilters` static class
3. Implement `EmacsFilters` static class
4. Implement `SearchFilters` static class
5. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All application filters correctly query app state
- [ ] Filters return false gracefully when no app is active
- [ ] Filter caching/memoization works correctly
- [ ] Unit tests achieve 80% coverage

## Related Features

- Feature 12: Filter System (core infrastructure this builds on)
- Feature 31: Application (provides runtime state)
- Feature 38: Vi State (provides Vi mode state)
- Feature 39: Emacs State (provides Emacs mode state)
