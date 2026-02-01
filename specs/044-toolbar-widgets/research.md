# Research: Toolbar Widgets

**Feature**: 044-toolbar-widgets
**Date**: 2026-02-01

## Research Tasks

### RT-01: FormattedTextToolbar — Window Subclass Pattern

**Question**: How should FormattedTextToolbar extend Window given C#'s FormattedTextControl accepts typed constructors, not AnyFormattedText?

**Finding**: Python's `FormattedTextToolbar.__init__` passes `text` (AnyFormattedText) directly to `FormattedTextControl(text, **kw)`, where `FormattedTextControl` stores it and uses `to_formatted_text()` at render time. In C#, `FormattedTextControl` has three constructors:
- `FormattedTextControl(IReadOnlyList<StyleAndTextTuple> text, ...)`
- `FormattedTextControl(Func<IReadOnlyList<StyleAndTextTuple>> textGetter, ...)`
- `FormattedTextControl(string text, ...)`

**Decision**: Use the `Func<IReadOnlyList<StyleAndTextTuple>>` constructor, wrapping the `AnyFormattedText` parameter with a lambda that calls `FormattedTextUtils.ToFormattedText(text)` at render time. This preserves the dynamic evaluation behavior of the Python version.

**Rationale**: The Func constructor matches Python's lazy evaluation behavior. Calling `ToFormattedText()` each time the func is invoked matches how Python's `_get_formatted_text_cached` method calls `to_formatted_text(self.text)` on each render cycle (with caching handled by `FormattedTextControl`'s internal `_fragmentCache`).

**Alternatives Considered**:
- Direct `IReadOnlyList<StyleAndTextTuple>` constructor: Would lose dynamic text support (functions returning text).
- Adding an `AnyFormattedText` overload to `FormattedTextControl`: Violates Constitution I (no API invention).

### RT-02: IMagicContainer Implementation Pattern

**Question**: What is the correct pattern for implementing `IMagicContainer` since no existing implementations exist in the codebase?

**Finding**: The `IMagicContainer` interface has a single method: `IContainer PtContainer()`. Python's `__pt_container__` protocol returns the inner container (typically a `ConditionalContainer`). The `AnyContainer` union type already accepts `IMagicContainer` via its constructor.

**Decision**: Each toolbar class (except FormattedTextToolbar which extends Window) implements `IMagicContainer` and returns its `ConditionalContainer` from `PtContainer()`. Fields are initialized in the constructor and stored as readonly properties.

**Rationale**: Direct match to Python pattern. The `IMagicContainer` interface is already designed for this purpose per its XML docs.

**Alternatives Considered**: None — the pattern is dictated by the Python source.

### RT-03: SystemToolbar Key Bindings Architecture

**Question**: How to port Python's nested key binding construction with decorators (`@handle`) to C#?

**Finding**: Python uses `KeyBindings()` instances with `@handle` decorators. The Stroke codebase uses `KeyBindings.Add()` returning a builder, or direct `Binding` construction. Existing bindings in `ViBindings`, `EmacsBindings`, etc. use `kb.Add(keys, handler, filter, isGlobal)` pattern.

**Decision**: Build three `KeyBindings` instances (emacs, vi, global) using the `Add()` method. Wrap each with `ConditionalKeyBindings` (emacs → EmacsMode, vi → ViMode, global → enableGlobalBindings). Merge via `MergedKeyBindings`. Store result as `IKeyBindingsBase` field.

**Rationale**: Exact structural match to Python's `_build_key_bindings()`. The existing pattern from `ViBindings`/`EmacsBindings` confirms this approach.

**Alternatives Considered**: Single `KeyBindings` with per-binding filters: Would work but diverges from Python's structure unnecessarily.

### RT-04: CompletionsToolbarControl Pagination Algorithm

**Question**: How does the Python pagination algorithm work and what types does it need?

**Finding**: The Python algorithm:
1. Gets `complete_state` from `get_app().current_buffer.complete_state`
2. Iterates completions, building fragments with `to_formatted_text(c.display_text, style=...)`
3. When accumulated `fragment_list_len(fragments) + len(c.display_text) >= content_width`:
   - If current item not yet displayed (i <= index), clears fragments and sets `cut_left = True`
   - Otherwise sets `cut_right = True` and breaks
4. Pads/trims to content_width
5. Wraps with arrow indicators in 6-char margins (space + arrow + space on each side)

**Decision**: Port the algorithm directly. Use `FormattedTextUtils.FragmentListLen()` for width calculation. Use `Completion.DisplayText` property. Access completions via `AppContext.GetApp().CurrentBuffer.CompleteState`. The C# `Completion.DisplayText` returns plain text (either `Display` formatted text converted to plain text, or `Text`), which needs to be measured by character length (matching Python's `len(c.display_text)`).

**Rationale**: Faithful port of the exact algorithm. The existing `CompletionState.Completions` returns `IReadOnlyList<CompletionItem>` where `CompletionItem` is `Stroke.Completion.Completion`.

**Alternatives Considered**: Using `UnicodeWidth.GetWidth()` for display text measurement: The Python code uses `len(c.display_text)` not `fragment_list_len`, so character count is correct for the comparison. The `fragment_list_len` is used for the accumulated fragments (which may have zero-width escape styles).

### RT-05: SearchToolbar is_searching Condition

**Question**: How does the SearchToolbar determine if it's actively searching?

**Finding**: Python creates a `@Condition` closure: `return self.control in get_app().layout.search_links`. In Stroke, `Layout.SearchLinks` is `Dictionary<SearchBufferControl, BufferControl>`. The condition checks if the toolbar's `SearchBufferControl` is a key in the search links dictionary.

**Decision**: Create a `Condition` with a lambda: `() => AppContext.GetApp().Layout.SearchLinks.ContainsKey(control)` where `control` is the `SearchBufferControl` instance.

**Rationale**: Direct port of the Python pattern using existing `Condition` class and `Layout.SearchLinks` dictionary.

**Alternatives Considered**: None — the pattern is dictated by the Python source.

### RT-06: SearchToolbar BeforeInput Dynamic Prompt

**Question**: How does the SearchToolbar dynamically select the search prompt?

**Finding**: Python's `get_before_input()` function returns different `AnyFormattedText` based on:
- `not is_searching()` → `text_if_not_searching`
- `control.searcher_search_state.direction == SearchDirection.BACKWARD` → `"?"` (vi) or `backward_search_prompt`
- else → `"/"` (vi) or `forward_search_prompt`

This function is passed to `BeforeInput(get_before_input, style="class:search-toolbar.prompt")`.

**Decision**: Create a `Func<AnyFormattedText>` that implements the same logic, using `SearcherSearchState.Direction` property on the `SearchBufferControl`. Pass it to `BeforeInput` constructor which accepts `AnyFormattedText` (which has implicit conversion from `Func<AnyFormattedText>`).

**Rationale**: The `BeforeInput` constructor accepts `AnyFormattedText`, and `AnyFormattedText` has an implicit conversion from `Func<AnyFormattedText>`. The function will be evaluated lazily at render time.

**Alternatives Considered**: None — direct port.

### RT-07: Namespace Placement for Widgets

**Question**: Where should toolbar widgets be placed in the namespace hierarchy?

**Finding**: CLAUDE.md specifies `Stroke.Widgets.Base/Text/Controls/Lists/Containers/Toolbars/Dialogs` as the namespace structure. The API mapping maps `prompt_toolkit.widgets` → `Stroke.Widgets`. Python's toolbar classes are in `prompt_toolkit.widgets.toolbars`.

**Decision**: Use namespace `Stroke.Widgets.Toolbars`. Place files in `src/Stroke/Widgets/Toolbars/`. This is the first Widgets namespace in the project — no separate csproj needed since all code is in the single `Stroke` project.

**Rationale**: Follows the namespace structure defined in CLAUDE.md and mirrors Python's module hierarchy.

**Alternatives Considered**:
- `Stroke.Widgets` (flat): Would conflict with future base widgets (TextArea, Label, etc.).
- Separate `Stroke.Widgets` project: Over-engineering; the existing single-project pattern handles this.

### RT-08: ValidationToolbar Position Calculation

**Question**: How does Python calculate line/column for the validation error display?

**Finding**: Python uses `buff.document.translate_index_to_position(buff.validation_error.cursor_position)` which returns `(row, col)` (0-indexed). Then formats: `f"{message} (line={row + 1} column={column + 1})"` (1-indexed display).

**Decision**: Use `Document.TranslateIndexToPosition(validationError.CursorPosition)` which returns `(int Row, int Col)` (0-indexed). Display with `$"{message} (line={row + 1} column={col + 1})"`.

**Rationale**: Direct port. The C# `TranslateIndexToPosition` returns a named tuple with 0-indexed values, matching Python's behavior.

**Alternatives Considered**: None — direct port.

### RT-09: SearchBufferControl inputProcessors Integration Gap

**Question**: Can the SearchToolbar attach a `BeforeInput` processor to `SearchBufferControl` given the current C# constructor?

**Finding**: Python's `SearchToolbar` passes `input_processors=[BeforeInput(get_before_input, style="class:search-toolbar.prompt")]` to the `SearchBufferControl` constructor. However, the C# `SearchBufferControl` constructor signature is:
```csharp
public SearchBufferControl(
    Buffer? buffer = null,
    FilterOrBool ignoreCase = default,
    SearchState? searcherSearchState = null,
    ILexer? lexer = null,
    FilterOrBool focusable = default,
    IKeyBindingsBase? keyBindings = null)
```
It does NOT accept `inputProcessors` and does NOT forward it to the `BufferControl` base constructor. The `base()` call passes only `buffer`, `lexer`, `focusable`, and `keyBindings`.

**Decision**: Extend `SearchBufferControl`'s constructor to accept an `IReadOnlyList<IProcessor>? inputProcessors = null` parameter and forward it to `BufferControl.base()`. This is a minor extension of an existing class, exposing an already-existing `BufferControl` parameter — not a new API invention.

**Rationale**: This is the minimal change required to support the Python API faithfully. `BufferControl` already has an `inputProcessors` parameter; `SearchBufferControl` simply needs to pass it through. The change is backward-compatible since the new parameter has a default value of null.

**Alternatives Considered**:
- Setting `inputProcessors` after construction: Not possible — `BufferControl` stores it in a readonly field at construction time.
- Using a wrapper or decorator pattern: Over-engineering for a simple parameter forwarding.
- Making `inputProcessors` a separate property: Would diverge from `BufferControl`'s constructor-only pattern.

## Summary

All 9 research tasks resolved with no NEEDS CLARIFICATION remaining. Key decisions:
- `FormattedTextToolbar` bridges `AnyFormattedText` to `FormattedTextControl` via `Func<>` constructor
- `IMagicContainer` implementations follow the Python `__pt_container__` protocol directly
- `SystemToolbar` key bindings use three-group merge pattern matching Python exactly
- `CompletionsToolbarControl` pagination algorithm is a direct port
- Namespace is `Stroke.Widgets.Toolbars` under the existing Stroke project
- `SearchBufferControl` needs a minor extension to accept `inputProcessors` (RT-09)
