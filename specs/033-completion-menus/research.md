# Research: Completion Menus

**Feature Branch**: `033-completion-menus`
**Date**: 2026-01-30

## Research Questions

No NEEDS CLARIFICATION items existed in the Technical Context. All dependencies and patterns are well-understood from the existing codebase and Python reference. The research below documents key decisions and patterns.

## Decisions

### R-001: Column Width Caching Strategy

**Decision**: Use `Dictionary<CompletionState, (int Count, int Width)>` with manual staleness check on completion count, matching Python's `WeakKeyDictionary` pattern.

**Rationale**: Python uses `WeakKeyDictionary` to avoid retaining references to dead `CompletionState` objects. In C#, `CompletionState` is a reference type managed by `Buffer.CompleteState`. When completions change, `Buffer` creates a new `CompletionState` instance, so old entries become unreachable. A `ConditionalWeakTable<CompletionState, StrongBox<(int, int)>>` provides equivalent weak-reference semantics in .NET. However, since the cache only holds one entry at a time in practice (the current completion state), a simple dictionary with explicit clearing on state change is simpler and equally correct. We use `ConditionalWeakTable` for faithful weak-reference behavior.

**Alternatives Considered**:
- `ConcurrentDictionary` — overhead not justified for single-entry cache
- Manual cleanup — error-prone, misses edge cases
- `ConditionalWeakTable<CompletionState, StrongBox<(int, int)>>` — closest .NET equivalent to `WeakKeyDictionary`, provides automatic GC cleanup (**selected**)

### R-002: CompletionsMenu Inheritance vs Composition

**Decision**: `CompletionsMenu` inherits from `ConditionalContainer` (constructor delegation). `MultiColumnCompletionsMenu` inherits from `HSplit`.

**Rationale**: Python PTK uses class inheritance (`class CompletionsMenu(ConditionalContainer)` and `class MultiColumnCompletionsMenu(HSplit)`). However, in the Stroke codebase, `ConditionalContainer` is `sealed` and `HSplit` is `sealed`. This requires a deviation: use composition (wrap + delegate) instead of inheritance, or unseal the classes.

**Resolution**: Unseal `ConditionalContainer` and `HSplit` to allow subclassing, matching Python PTK's inheritance model. This is the minimal change needed for API fidelity. Both classes need to be unsealed because:
- `CompletionsMenu(ConditionalContainer)` — Python line 261
- `MultiColumnCompletionsMenu(HSplit)` — Python line 627

**Alternatives Considered**:
- Composition with delegation — violates Constitution I (API fidelity), changes the type hierarchy
- Unsealing both base classes — minimal change, enables faithful port (**selected**)

### R-003: ScrollbarMargin DisplayArrows Parameter Type

**Decision**: The existing `ScrollbarMargin` takes `bool displayArrows`. Python PTK's `CompletionsMenu` passes a `FilterOrBool` value for `display_arrows`. The Stroke `ScrollbarMargin` already accepts `bool`, so we convert `FilterOrBool` to `bool` at construction time using `filter.Invoke()`, or we pass the `FilterOrBool` parameter through as the Python code does. Since the Python `ScrollbarMargin` constructor accepts `display_arrows: FilterOrBool`, the Stroke `ScrollbarMargin` should also accept `FilterOrBool`. However, the existing implementation already takes `bool`. For this feature, we simply evaluate the filter at construction time since the `CompletionsMenu` constructor receives `display_arrows` as a parameter and passes it straight through. The Python code also constructs `ScrollbarMargin(display_arrows=display_arrows)` where `display_arrows` is already a `Filter`. We will update `ScrollbarMargin` to accept `FilterOrBool` to maintain fidelity.

**Rationale**: Faithful port of Python PTK requires `ScrollbarMargin` to accept filter-like values.

**Alternatives Considered**:
- Keep `bool` parameter, evaluate filter at `CompletionsMenu` construction — loses dynamic filter behavior
- Accept `FilterOrBool` in `ScrollbarMargin` — faithful to Python PTK (**selected**)

### R-004: Mouse Handler Return Type

**Decision**: Mouse handlers return `NotImplementedOrNone`. Both menu controls return `NotImplementedOrNone.None` (event consumed) from `MouseHandler`.

**Rationale**: Matches Python returning `None` from mouse handlers (event consumed, no bubbling).

### R-005: Thread Safety Scope

**Decision**: `MultiColumnCompletionMenuControl` requires `Lock` for its mutable fields (`scroll`, `_renderedRows`, `_renderedColumns`, `_totalColumns`, `_renderPosToCompletion`, `_renderLeftArrow`, `_renderRightArrow`, `_renderWidth`, `_columnWidthForCompletionState`). `CompletionsMenuControl` is stateless (no fields) and requires no synchronization. `SelectedCompletionMetaControl` is stateless and requires no synchronization. `MenuUtils` is a static utility class with no state.

**Rationale**: Constitution XI requires thread safety for mutable state. Only `MultiColumnCompletionMenuControl` has mutable fields.

### R-006: Grouper Implementation

**Decision**: Implement the `grouper` function (Python's `itertools` recipe) as a local helper within `CreateContent`. It groups completions into columns of `height` items each, with `null` fill for incomplete final column.

**Rationale**: Faithful port of the `grouper(n, iterable, fillvalue)` pattern used in Python PTK lines 407-412.

**Implementation**: Use LINQ `Chunk` (.NET 8+) to split completions into groups of `height`, then transpose using nested iteration.

### R-007: `_get_menu_item_fragments` and `_trim_formatted_text` Placement

**Decision**: Port as static methods in the `MenuUtils` internal static class. Python defines these as module-level functions.

**Rationale**: C# has no module-level functions. A static utility class is the idiomatic equivalent. Named `MenuUtils` to be descriptive and avoid collision with other utilities.

## Dependencies Verified

| Dependency | Location | Verified |
|-----------|----------|----------|
| `IUIControl` interface | `src/Stroke/Layout/Controls/IUIControl.cs` | Yes |
| `UIContent` class | `src/Stroke/Layout/Controls/UIContent.cs` | Yes |
| `ConditionalContainer` class | `src/Stroke/Layout/Containers/ConditionalContainer.cs` | Yes (sealed — needs unsealing) |
| `HSplit` class | `src/Stroke/Layout/Containers/HSplit.cs` | Yes (sealed — needs unsealing) |
| `Window` class | `src/Stroke/Layout/Containers/Window.cs` | Yes |
| `ScrollbarMargin` class | `src/Stroke/Layout/Margins/ScrollbarMargin.cs` | Yes (needs FilterOrBool for displayArrows) |
| `ScrollOffsets` class | `src/Stroke/Layout/Windows/ScrollOffsets.cs` | Yes |
| `Dimension` class | `src/Stroke/Layout/Dimension.cs` | Yes |
| `CompletionState` class | `src/Stroke/Core/CompletionState.cs` | Yes |
| `Completion` record | `src/Stroke/Completion/Completion.cs` | Yes |
| `Buffer` class | `src/Stroke/Core/Buffer.cs` + `Buffer.Completion.cs` | Yes |
| `AppContext.GetApp()` | `src/Stroke/Application/AppContext.cs` | Yes |
| `AppFilters.HasCompletions` | `src/Stroke/Application/AppFilters.cs` | Yes |
| `AppFilters.IsDone` | `src/Stroke/Application/AppFilters.cs` | Yes |
| `StyleAndTextTuple` | `src/Stroke/FormattedText/StyleAndTextTuple.cs` | Yes |
| `FormattedTextUtils.FragmentListWidth` | `src/Stroke/FormattedText/FormattedTextUtils.cs` | Yes |
| `FormattedTextUtils.ToFormattedText` | `src/Stroke/FormattedText/FormattedTextUtils.cs` | Yes (requires `style` parameter overload: `ToFormattedText(fragments, style: string)` to apply a style class to all fragments) |
| `ExplodedList` | `src/Stroke/Layout/ExplodedList.cs` | Yes |
| `UnicodeWidth.GetWidth` | `src/Stroke/Core/UnicodeWidth.cs` | Yes |
| `Point` | `src/Stroke/Core/Primitives/Point.cs` | Yes |
| `MouseEvent` / `MouseEventType` | `src/Stroke/Input/MouseEvent.cs` / `MouseEventType.cs` | Yes |
| `NotImplementedOrNone` | `src/Stroke/KeyBinding/NotImplementedOrNone.cs` | Yes |
| `KeyBindings` | `src/Stroke/KeyBinding/KeyBindings.cs` | Yes |
| `IKeyBindingsBase` | `src/Stroke/KeyBinding/IKeyBindingsBase.cs` | Yes |
| `KeyPressEvent` | `src/Stroke/KeyBinding/KeyPressEvent.cs` | Yes |
| `FilterOrBool` | `src/Stroke/Filters/FilterOrBool.cs` | Yes |
| `Condition` | `src/Stroke/Filters/Condition.cs` | Yes |
| `FilterUtils.ToFilter` | `src/Stroke/Filters/FilterUtils.cs` | Yes |
| `Layout.VisibleWindows` | `src/Stroke/Layout/Layout.cs` | Yes |
| `GetLinePrefixCallable` | `src/Stroke/Layout/Windows/GetLinePrefixCallable.cs` | Yes |

## Prerequisite Changes Required

1. **Unseal `ConditionalContainer`**: Change `public sealed class ConditionalContainer` to `public class ConditionalContainer` to allow `CompletionsMenu` to inherit.
   - **Impact assessment**: `ConditionalContainer` has no virtual methods beyond its base class. Removing `sealed` allows subclassing but does not change behavior for existing callers. No downstream types rely on the sealed guarantee (checked via codebase search). Risk: minimal — only adds extensibility.

2. **Unseal `HSplit`**: Change `public sealed class HSplit` to `public class HSplit` to allow `MultiColumnCompletionsMenu` to inherit.
   - **Impact assessment**: `HSplit` inherits from a split container base. Removing `sealed` enables subclassing. No existing code depends on the `sealed` constraint (no pattern matching on `HSplit` as a sealed type, no assumptions about non-extensibility). Risk: minimal — only adds extensibility.

3. **Update `ScrollbarMargin`**: Accept `FilterOrBool` for `displayArrows` parameter instead of `bool`, to match Python PTK's API.
   - **Backward compatibility**: `FilterOrBool` has an implicit conversion operator from `bool`, so all existing callers passing `true`/`false` will continue to compile and behave identically. The change is source-compatible and binary-compatible for typical usage. The internal storage changes from `bool` to `IFilter`, with evaluation at render time instead of construction time — this is the desired dynamic behavior.
