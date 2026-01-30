# Contract: CombinedRegistry and Default Bindings

**Namespace**: `Stroke.Application` (CombinedRegistry), `Stroke.KeyBinding` (defaults)
**Source**: `prompt_toolkit.application.application._CombinedRegistry`, `prompt_toolkit.key_binding.defaults`

## CombinedRegistry (Internal)

```csharp
/// <summary>
/// Internal key bindings aggregator for an Application. Merges key bindings from
/// the focused control hierarchy, global-only bindings, application bindings,
/// page navigation bindings, and default bindings.
/// </summary>
/// <remarks>
/// This class caches merged bindings keyed by (current_window, controls_set) to avoid
/// recomputation on every key press. Not exposed publicly.
/// </remarks>
/// <para>
/// <b>Visibility rationale:</b> Internal because this is an implementation detail of
/// Application. Users configure key bindings via Application.KeyBindings and per-control
/// bindings. The merge algorithm is not part of the public API contract, allowing it to
/// evolve without breaking changes.
/// </para>
internal sealed class CombinedRegistry : IKeyBindingsBase
{
    internal CombinedRegistry(Application<object?> app);

    /// <summary>Not implemented — this object is not wrapped in another KeyBindings.</summary>
    public object Version { get; }

    /// <summary>Not implemented — this object is not wrapped in another KeyBindings.</summary>
    public IReadOnlyList<Binding> Bindings { get; }

    /// <summary>Get bindings matching the exact key sequence.</summary>
    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<KeyOrChar> keys);

    /// <summary>Get bindings that start with the given key sequence prefix.</summary>
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<KeyOrChar> keys);
}
```

### Merge Algorithm

The `CombinedRegistry` creates merged bindings with this priority order (highest to lowest):

1. **Focused control's key bindings** — from `currentWindow.Content.GetKeyBindings()`
2. **Parent container bindings** — walking up from the focused window to the root (or first modal container)
3. **Global-only bindings** — from containers NOT in the focused hierarchy, wrapped in `GlobalOnlyKeyBindings`
4. **Application key bindings** — from `Application.KeyBindings`
5. **Page navigation bindings** — from `LoadPageNavigationBindings()`, wrapped in `ConditionalKeyBindings` with `Application.EnablePageNavigationBindings`
6. **Default bindings** — from `LoadKeyBindings()` (basic + emacs + vi + mouse + CPR, conditional on `buffer_has_focus`)

The result list is **reversed** so that the focused control's bindings have highest priority in the merged result.

## Default Bindings

```csharp
/// <summary>
/// Provides the default key bindings for Stroke applications.
/// </summary>
public static class DefaultKeyBindings
{
    /// <summary>
    /// Load the default key bindings that merge basic, Emacs, Vi, mouse, and CPR bindings.
    /// The editing-mode-specific bindings are conditional on buffer_has_focus.
    /// </summary>
    /// <returns>Merged default key bindings.</returns>
    public static IKeyBindingsBase Load();

    /// <summary>
    /// Load page navigation bindings (Emacs and Vi page up/down, scroll).
    /// Conditional on buffer_has_focus.
    /// </summary>
    /// <returns>Page navigation key bindings.</returns>
    public static IKeyBindingsBase LoadPageNavigation();
}
```

## App-Aware Filters

```csharp
/// <summary>
/// Filter functions that query the current application state.
/// Used by key bindings, containers, and other components.
/// </summary>
public static class AppFilters
{
    /// <summary>True when a BufferControl has focus in the current application.
    /// Queries: <c>AppContext.GetApp().Layout.CurrentControl is BufferControl</c>.</summary>
    public static IFilter BufferHasFocus { get; }

    /// <summary>True when the current application is in Vi navigation mode.
    /// Queries: <c>App.ViState.InputMode == InputMode.Navigation</c>.</summary>
    public static IFilter ViNavigationMode { get; }

    /// <summary>True when the current application is in Vi insert mode.
    /// Queries: <c>App.ViState.InputMode == InputMode.Insert</c>.</summary>
    public static IFilter ViInsertMode { get; }

    /// <summary>True when the current application uses Emacs editing mode.
    /// Queries: <c>App.EditingMode == EditingMode.Emacs</c>.</summary>
    public static IFilter EmacsMode { get; }

    /// <summary>True when the current application uses Vi editing mode.
    /// Queries: <c>App.EditingMode == EditingMode.Vi</c>.</summary>
    public static IFilter ViMode { get; }

    /// <summary>True when the current application is searching.
    /// Queries: <c>App.Layout.IsSearching</c>.</summary>
    public static IFilter IsSearching { get; }

    /// <summary>True when the current application has a selection active.
    /// Queries: <c>App.CurrentBuffer.SelectionState != null</c>.</summary>
    public static IFilter HasSelection { get; }

    /// <summary>True when the current buffer has a completer assigned.
    /// Queries: <c>App.CurrentBuffer.Completer != null</c>.</summary>
    public static IFilter HasCompletions { get; }

    /// <summary>True when a completion menu is currently showing.
    /// Queries: <c>App.CurrentBuffer.CompletionState != null &amp;&amp; CompletionState.CurrentCompletion != null</c>.</summary>
    public static IFilter CompletionIsSelected { get; }

    /// <summary>True when there is a validation error on the current buffer.
    /// Queries: <c>App.CurrentBuffer.ValidationError != null</c>.</summary>
    public static IFilter HasValidationError { get; }

    /// <summary>True when the current buffer has an argument typed (e.g., Vi numeric prefix).
    /// Queries: <c>App.KeyProcessor.Arg != null</c>.</summary>
    public static IFilter HasArg { get; }

    /// <summary>True when the current buffer is read-only.
    /// Queries: <c>App.CurrentBuffer.ReadOnly</c> (filter evaluation).</summary>
    public static IFilter IsReadOnly { get; }

    /// <summary>True when the current buffer is a multiline buffer.
    /// Queries: <c>App.CurrentBuffer.Multiline</c> (filter evaluation).</summary>
    public static IFilter IsMultiline { get; }

    /// <summary>True when the current application has an active buffer focused.
    /// Queries: <c>App.Layout.CurrentBuffer != null</c>.</summary>
    public static IFilter HasFocus { get; }

    /// <summary>Create a filter that checks if a specific buffer has focus.
    /// Queries: <c>App.Layout.CurrentBuffer?.Name == bufferName</c>.</summary>
    public static IFilter CreateHasFocus(string bufferName);
}
```
