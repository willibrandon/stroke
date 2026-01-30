# Contract: AppFilters

**Feature**: 032-application-filters
**Python Source**: `prompt_toolkit/filters/app.py` (lines 58-217)

## Static Class: AppFilters

**Namespace**: `Stroke.Application`
**Dependencies**: `Stroke.Filters` (IFilter, Condition), `Stroke.Core` (Buffer, SimpleCache), `Stroke.Layout` (Layout, LayoutUtils, Window, IContainer, BufferControl, IUIControl), `Stroke.KeyBinding` (EditingMode, KeyProcessor)

### Properties (IFilter)

```csharp
/// <summary>True when the current buffer has a selection.</summary>
public static IFilter HasSelection { get; }
// Logic: AppContext.GetApp().CurrentBuffer.SelectionState is not null

/// <summary>True when the current buffer has a non-empty suggestion.</summary>
public static IFilter HasSuggestion { get; }
// Logic: buffer.Suggestion is not null && buffer.Suggestion.Text != ""

/// <summary>True when the current buffer has active completions.</summary>
public static IFilter HasCompletions { get; }
// Logic: buffer.CompleteState is not null && buffer.CompleteState.Completions.Count > 0

/// <summary>True when the user has selected a completion.</summary>
public static IFilter CompletionIsSelected { get; }
// Logic: buffer.CompleteState is not null && buffer.CompleteState.CurrentCompletion is not null

/// <summary>True when the current buffer is read-only.</summary>
public static IFilter IsReadOnly { get; }
// Logic: AppContext.GetApp().CurrentBuffer.ReadOnly

/// <summary>True when the current buffer is multiline.</summary>
public static IFilter IsMultiline { get; }
// Logic: AppContext.GetApp().CurrentBuffer.Multiline

/// <summary>True when the current buffer has a validation error.</summary>
public static IFilter HasValidationError { get; }
// Logic: AppContext.GetApp().CurrentBuffer.ValidationError is not null

/// <summary>True when the key processor has an 'arg' (numeric prefix).</summary>
public static IFilter HasArg { get; }
// Logic: AppContext.GetApp().KeyProcessor.Arg is not null

/// <summary>True when the application is done (returning/aborting).</summary>
public static IFilter IsDone { get; }
// Logic: AppContext.GetApp().IsDone

/// <summary>True when the renderer knows its real terminal height.</summary>
public static IFilter RendererHeightIsKnown { get; }
// Logic: AppContext.GetApp().Renderer.HeightIsKnown

/// <summary>True when paste mode is active.</summary>
public static IFilter InPasteMode { get; }
// Logic: AppContext.GetApp().PasteMode.Invoke()

/// <summary>True when a BufferControl has focus.</summary>
public static IFilter BufferHasFocus { get; }
// Logic: AppContext.GetApp().Layout.BufferHasFocus
```

### Methods

```csharp
/// <summary>
/// Create a filter that checks if a specific buffer has focus by name.
/// Each call returns a new instance (no memoization).
/// </summary>
public static IFilter HasFocus(string bufferName)
// Logic: AppContext.GetApp().CurrentBuffer.Name == bufferName

/// <summary>
/// Create a filter that checks if a specific Buffer instance has focus.
/// Each call returns a new instance (no memoization).
/// </summary>
public static IFilter HasFocus(Buffer buffer)
// Logic: AppContext.GetApp().CurrentBuffer == buffer (reference equality)

/// <summary>
/// Create a filter that checks if a specific UIControl has focus.
/// Each call returns a new instance (no memoization).
/// </summary>
public static IFilter HasFocus(IUIControl control)
// Logic: AppContext.GetApp().Layout.CurrentControl == control

/// <summary>
/// Create a filter that checks if a container (or any of its descendant windows) has focus.
/// For Window instances, checks direct equality. For other containers, walks descendants.
/// Each call returns a new instance (no memoization).
/// </summary>
public static IFilter HasFocus(IContainer container)
// Logic: Window → direct check; other → walk descendants via LayoutUtils.Walk()

/// <summary>
/// Create a cached filter for a given editing mode.
/// Returns the same instance for the same EditingMode value (memoized).
/// </summary>
public static IFilter InEditingMode(EditingMode editingMode)
// Logic: AppContext.GetApp().EditingMode == editingMode
// Caching: SimpleCache<EditingMode, IFilter> with capacity 2
```

### Behavioral Notes

- All properties are static readonly `IFilter` backed by `new Condition(() => ...)`.
- `HasFocus` overloads return a **new** `Condition` on every call (FR-013).
- `InEditingMode` uses a `SimpleCache<EditingMode, IFilter>` for memoization (FR-012).
- All filters return `false` when no application is running (DummyApplication fallback).
