# Feature 12: Filter System

## Overview

Implement the filter system for conditional enabling/disabling of features based on application state.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/filters/base.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/filters/app.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/filters/utils.py`

## Public API

### IFilter Interface (Abstract Base)

```csharp
namespace Stroke.Filters;

/// <summary>
/// Base interface for any filter to activate/deactivate a feature, depending on a condition.
/// The return value of Invoke() tells if the feature should be active.
/// </summary>
public interface IFilter
{
    /// <summary>
    /// Evaluate the filter.
    /// </summary>
    bool Invoke();

    /// <summary>
    /// Chaining of filters using the & operator.
    /// </summary>
    IFilter And(IFilter other);

    /// <summary>
    /// Chaining of filters using the | operator.
    /// </summary>
    IFilter Or(IFilter other);

    /// <summary>
    /// Inverting of filter using the ~ operator.
    /// </summary>
    IFilter Invert();
}
```

### Filter Abstract Class

```csharp
namespace Stroke.Filters;

/// <summary>
/// Base class for filters with caching for combined filters.
/// </summary>
public abstract class Filter : IFilter
{
    protected Filter();

    public abstract bool Invoke();

    /// <summary>
    /// Chaining of filters using the & operator.
    /// </summary>
    public IFilter And(IFilter other);

    /// <summary>
    /// Chaining of filters using the | operator.
    /// </summary>
    public IFilter Or(IFilter other);

    /// <summary>
    /// Inverting of filter.
    /// </summary>
    public IFilter Invert();

    // Operator overloads for C# convenience
    public static IFilter operator &(Filter left, Filter right);
    public static IFilter operator |(Filter left, Filter right);
    public static IFilter operator ~(Filter filter);

    // Prevent direct bool conversion (ambiguous meaning)
    // Users must call Invoke() explicitly
}
```

### Always Class

```csharp
namespace Stroke.Filters;

/// <summary>
/// Filter that is always true.
/// </summary>
public sealed class Always : Filter
{
    public static readonly Always Instance = new();

    public override bool Invoke() => true;

    public new IFilter And(IFilter other) => other;
    public new IFilter Or(IFilter other) => this;
    public new IFilter Invert() => Never.Instance;
}
```

### Never Class

```csharp
namespace Stroke.Filters;

/// <summary>
/// Filter that is always false.
/// </summary>
public sealed class Never : Filter
{
    public static readonly Never Instance = new();

    public override bool Invoke() => false;

    public new IFilter And(IFilter other) => this;
    public new IFilter Or(IFilter other) => other;
    public new IFilter Invert() => Always.Instance;
}
```

### Condition Class

```csharp
namespace Stroke.Filters;

/// <summary>
/// Turn any callable into a Filter.
/// </summary>
public sealed class Condition : Filter
{
    /// <summary>
    /// Creates a condition from a callable.
    /// </summary>
    /// <param name="func">Callable which takes no inputs and returns a boolean.</param>
    public Condition(Func<bool> func);

    /// <summary>
    /// The underlying function.
    /// </summary>
    public Func<bool> Func { get; }

    public override bool Invoke();
    public override string ToString();
}
```

### Filter Utility Functions

```csharp
namespace Stroke.Filters;

/// <summary>
/// Filter utility functions.
/// </summary>
public static class FilterUtils
{
    /// <summary>
    /// Accept both booleans and Filters and turn it into a Filter.
    /// </summary>
    public static IFilter ToFilter(bool value);

    /// <summary>
    /// Accept both booleans and Filters and turn it into a Filter.
    /// </summary>
    public static IFilter ToFilter(IFilter filter);

    /// <summary>
    /// Accept both booleans and Filters and turn it into a Filter.
    /// </summary>
    public static IFilter ToFilter(Func<bool> func);

    /// <summary>
    /// Test whether value is true. In case of a Filter, call it.
    /// </summary>
    public static bool IsTrue(bool value);

    /// <summary>
    /// Test whether value is true. In case of a Filter, call it.
    /// </summary>
    public static bool IsTrue(IFilter filter);
}
```

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
    ├── IFilter.cs
    ├── Filter.cs
    ├── Always.cs
    ├── Never.cs
    ├── Condition.cs
    ├── AndList.cs (internal)
    ├── OrList.cs (internal)
    ├── Invert.cs (internal)
    ├── FilterUtils.cs
    ├── AppFilters.cs
    ├── ViFilters.cs
    ├── EmacsFilters.cs
    └── SearchFilters.cs
tests/Stroke.Tests/
└── Filters/
    ├── FilterTests.cs
    ├── AlwaysNeverTests.cs
    ├── ConditionTests.cs
    ├── FilterCombinationTests.cs
    └── FilterUtilsTests.cs
```

## Implementation Notes

### Filter Caching

The `Filter` base class caches combined filters to avoid repeated allocations:
- `_andCache`: Dictionary mapping other filter to combined AND result
- `_orCache`: Dictionary mapping other filter to combined OR result
- `_invertResult`: Cached inverted filter

### Short-Circuit Optimization

The `Always` and `Never` classes override combination methods for short-circuit optimization:
- `Always & x` → `x`
- `Always | x` → `Always`
- `Never & x` → `Never`
- `Never | x` → `x`

### AndList and OrList Flattening

When combining multiple filters, nested `_AndList` and `_OrList` instances are flattened:
- `(a & b) & c` → `_AndList([a, b, c])` (not nested)
- Duplicate filters are removed for efficiency

### Bool Conversion Prevention

The Python implementation raises an error on bool conversion to prevent ambiguous usage. In C#, we achieve this by not implementing implicit conversion to bool and requiring explicit `Invoke()` calls.

### Lazy Evaluation

Filters are lazy - they only compute their value when `Invoke()` is called. Combined filters evaluate their component filters on each call.

## Dependencies

- `Stroke.Application` (Feature 35) - For accessing current app state
- `Stroke.Core.Buffer` (Feature 06) - For buffer-related filters
- `Stroke.KeyBinding.ViState` (Feature 28) - For Vi mode filters

## Implementation Tasks

1. Implement `IFilter` interface
2. Implement `Filter` base class with caching
3. Implement `Always` and `Never` singletons
4. Implement `Condition` class
5. Implement internal `_AndList`, `_OrList`, `_Invert` classes
6. Implement `FilterUtils` static class
7. Implement `AppFilters` static class
8. Implement `ViFilters` static class
9. Implement `EmacsFilters` static class
10. Implement `SearchFilters` static class
11. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All filter types match Python Prompt Toolkit semantics
- [ ] Filter combination caching works correctly
- [ ] Short-circuit optimization works correctly
- [ ] All application filters correctly query app state
- [ ] Unit tests achieve 80% coverage
