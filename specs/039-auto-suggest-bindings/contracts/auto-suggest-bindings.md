# API Contract: AutoSuggestBindings

**Module**: `Stroke.Application.Bindings`
**Python Source**: `prompt_toolkit.key_binding.bindings.auto_suggest`
**Date**: 2026-01-31

## Public API

### AutoSuggestBindings Static Class

**Visibility deviation from Python**: Python's `__all__` exports only `load_auto_suggest_bindings`. The handlers `_accept` and `_fill` are module-private (underscore-prefixed). In C#, `AcceptSuggestion` and `AcceptPartialSuggestion` are made `public` for two reasons: (1) the `kb.Add<KeyHandlerCallable>()` registration pattern in the codebase uses method group references to public static methods, and (2) public handlers enable direct unit testing without reflection, consistent with `SearchBindings`, `BasicBindings`, and `ScrollBindings` which all expose their handlers publicly.

```csharp
namespace Stroke.Application.Bindings;

/// <summary>
/// Key binding loader for auto suggestion acceptance bindings.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.auto_suggest</c> module.
/// Provides a single factory method that creates and returns a <see cref="KeyBindings"/>
/// instance containing all auto suggest key bindings: full suggestion acceptance
/// (Ctrl-F, Ctrl-E, Right arrow) and partial word-segment acceptance (Escape+F, Emacs only).
/// </para>
/// <para>
/// These bindings must be loaded after Vi bindings so that suggestion acceptance
/// takes priority over Vi cursor movement when a suggestion is available.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. The factory method creates a
/// new <see cref="KeyBindings"/> instance on each call.
/// </para>
/// </remarks>
public static class AutoSuggestBindings
{
    /// <summary>
    /// Accept the full suggestion text. Reads <c>Buffer.Suggestion</c>, guards against
    /// null (race condition between filter evaluation and handler execution), then
    /// inserts the entire <c>Suggestion.Text</c> into the buffer via <c>Buffer.InsertText()</c>.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success; guard returns <c>null</c> if suggestion is null.</returns>
    /// <remarks>
    /// Maps to Python's <c>_accept</c> handler (private). Made public in C# for testability
    /// and consistency with the <c>kb.Add&lt;KeyHandlerCallable&gt;</c> registration pattern
    /// used throughout <c>Stroke.Application.Bindings</c>.
    /// </remarks>
    public static NotImplementedOrNone? AcceptSuggestion(KeyPressEvent @event);

    /// <summary>
    /// Accept the next word segment of the suggestion. Splits the suggestion text
    /// using <c>Regex.Split(@"([^\s/]+(?:\s+|/))")</c> and inserts the first non-empty
    /// segment into the buffer. This respects both whitespace boundaries and path
    /// separator (/) boundaries, matching Python's <c>next(x for x in t if x)</c> logic.
    /// Active only in Emacs editing mode.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success; guard returns <c>null</c> if suggestion is null.</returns>
    /// <remarks>
    /// Maps to Python's <c>_fill</c> handler (private). Made public in C# for testability
    /// and consistency with the <c>kb.Add&lt;KeyHandlerCallable&gt;</c> registration pattern
    /// used throughout <c>Stroke.Application.Bindings</c>.
    /// </remarks>
    public static NotImplementedOrNone? AcceptPartialSuggestion(KeyPressEvent @event);

    /// <summary>
    /// Load key bindings for accepting auto suggestion text.
    /// </summary>
    /// <returns>
    /// A new <see cref="KeyBindings"/> instance containing all 4 auto suggest key bindings.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Registers the following bindings:
    /// </para>
    /// <list type="bullet">
    ///   <item>Ctrl-F → <see cref="AcceptSuggestion"/> (filter: SuggestionAvailable)</item>
    ///   <item>Ctrl-E → <see cref="AcceptSuggestion"/> (filter: SuggestionAvailable)</item>
    ///   <item>Right arrow → <see cref="AcceptSuggestion"/> (filter: SuggestionAvailable)</item>
    ///   <item>Escape+F → <see cref="AcceptPartialSuggestion"/> (filter: SuggestionAvailable AND EmacsMode)</item>
    /// </list>
    /// </remarks>
    public static KeyBindings LoadAutoSuggestBindings();
}
```

## Internal Members

### SuggestionAvailable Filter

```csharp
/// <summary>
/// True when a suggestion is available for acceptance: the current buffer has
/// a non-null suggestion with non-empty text and the cursor is at the end of the document.
/// </summary>
private static readonly IFilter SuggestionAvailable = new Condition(() =>
{
    var app = AppContext.GetApp();
    var buffer = app.CurrentBuffer;
    return buffer.Suggestion is not null
        && buffer.Suggestion.Text.Length > 0
        && buffer.Document.IsCursorAtTheEnd;
});
```

## Key Binding Registration

| Key Sequence | Handler | Filter | eager | saveBefore | Notes |
|-------------|---------|--------|-------|------------|-------|
| Ctrl-F | `AcceptSuggestion` | `SuggestionAvailable` | default (`Never`) | default (`null` → always) | Full accept |
| Ctrl-E | `AcceptSuggestion` | `SuggestionAvailable` | default (`Never`) | default (`null` → always) | Full accept |
| Right | `AcceptSuggestion` | `SuggestionAvailable` | default (`Never`) | default (`null` → always) | Full accept; overrides Vi right arrow |
| Escape, F | `AcceptPartialSuggestion` | `SuggestionAvailable & EmacsMode` | default (`Never`) | default (`null` → always) | Partial accept; Emacs only |

**Parameter notes**: The Python source does not set `eager` or `save_before` for any auto suggest binding. All four bindings use default values: `eager` defaults to `Never` (non-greedy matching), and `saveBefore` defaults to `null` (always save buffer state before handler execution). This matches the Python behavior where no special eager or save parameters are passed to the `@handle` decorator.

## Word Boundary Pattern

The partial acceptance handler uses `Regex.Split` with pattern `@"([^\s/]+(?:\s+|/))"`, then selects the first non-empty element via `First(x => !string.IsNullOrEmpty(x))` (equivalent to Python's `next(x for x in t if x)`):

| Input | Full Split Result | First Non-Empty Segment |
|-------|-------------------|-------------------------|
| `"commit -m 'message'"` | `["", "commit ", "", "-m ", "'message'"]` | `"commit "` |
| `"/home/user/documents/"` | `["/", "home/", "", "user/", "", "documents/", ""]` | `"/"` |
| `"abc"` | `["abc"]` | `"abc"` |
| `" commit -m 'fix'"` | `[" ", "commit ", "", "-m ", "'fix'"]` | `" "` |

**Note**: When the suggestion text starts with a path separator (e.g., `"/home/..."`), the first non-empty segment is just `"/"` — the leading separator before the first word. When the suggestion text starts with whitespace (e.g., `" commit ..."`), the first non-empty segment is just `" "` — the leading whitespace. This is faithful to Python's behavior where each Escape-F press advances by exactly one segment boundary.

## Dependencies

| Dependency | Package | Used For |
|-----------|---------|----------|
| `AppContext.GetApp()` | `Stroke.Application` | Access current buffer in filter |
| `EmacsFilters.EmacsMode` | `Stroke.Application` | Compose filter for partial accept binding |
| `Buffer.Suggestion` | `Stroke.Core` | Read suggestion in filter and handlers |
| `Buffer.InsertText()` | `Stroke.Core` | Insert accepted text into buffer |
| `Document.IsCursorAtTheEnd` | `Stroke.Core` | Filter condition (cursor at end) |
| `Suggestion` | `Stroke.AutoSuggest` | Immutable suggestion record with `Text` property |
| `KeyBindings` | `Stroke.KeyBinding` | Binding registry created by factory method |
| `KeyHandlerCallable` | `Stroke.KeyBinding` | Handler delegate type for `Add<T>()` |
| `KeyPressEvent` | `Stroke.KeyBinding` | Handler parameter providing buffer access |
| `KeyOrChar` | `Stroke.KeyBinding` | Key specification for binding registration |
| `NotImplementedOrNone` | `Stroke.KeyBinding` | Handler return type (`null` on success) |
| `FilterOrBool` | `Stroke.Filters` | Filter wrapper for binding registration |
| `Condition` | `Stroke.Filters` | Dynamic filter creation for `SuggestionAvailable` |
| `Filter.And()` | `Stroke.Filters` | Filter composition (SuggestionAvailable AND EmacsMode) |
| `Keys` | `Stroke.Input` | Key constants (ControlF, ControlE, Right, Escape) |
