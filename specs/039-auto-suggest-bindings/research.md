# Research: Auto Suggest Bindings

**Feature**: 039-auto-suggest-bindings
**Date**: 2026-01-31

## Research Tasks

### R1: Namespace Placement

**Question**: Should `AutoSuggestBindings` go in `Stroke.KeyBinding.Bindings` or `Stroke.Application.Bindings`?

**Decision**: `Stroke.Application.Bindings`

**Rationale**: The implementation requires `AppContext.GetApp()` (Application layer) for the `SuggestionAvailable` filter and `EmacsFilters.EmacsMode` (Application layer) for the partial accept binding. Per Constitution III (Layered Architecture), lower layers must not reference higher layers. Since the KeyBinding layer (layer 4) cannot depend on the Application layer (layer 7), the bindings must be placed in the Application layer. This is consistent with `SearchBindings`, `BasicBindings`, `ScrollBindings`, and `PageNavigationBindings` — all of which live in `Stroke.Application.Bindings` because they depend on Application-layer types.

**Alternatives considered**:
- `Stroke.KeyBinding.Bindings`: Rejected — would create a circular dependency (KeyBinding → Application for AppContext/EmacsFilters)

### R2: Filter Composition Pattern

**Question**: How should the `SuggestionAvailable` filter be implemented?

**Decision**: Private static `IFilter` field using `Condition` constructor with lambda

**Rationale**: The existing codebase uses `new Condition(() => ...)` for dynamic state filters (see `AppFilters.HasSuggestion`, `BasicBindings.HasTextBeforeCursor`). The `SuggestionAvailable` filter is more specific than `AppFilters.HasSuggestion` because it also checks `Document.IsCursorAtTheEnd`. Creating a dedicated filter within the class follows the pattern in `BasicBindings` and `SearchBindings`.

**Alternatives considered**:
- Reuse `AppFilters.HasSuggestion` composed with a cursor-at-end check: Possible but adds unnecessary indirection. The Python source defines the filter inline as a local function, so a private static field in the class is the closest port.
- Public filter on `AppFilters`: Rejected — the Python source defines it as a local function inside `load_auto_suggest_bindings()`, not as a module-level export.

### R3: Handler Function Signatures

**Question**: Should handlers return `NotImplementedOrNone?` (like `KeyHandlerCallable`) or `void`?

**Decision**: `NotImplementedOrNone?` returning `null` on success, matching `KeyHandlerCallable` delegate

**Rationale**: All binding handler functions in the codebase that are registered via `kb.Add<KeyHandlerCallable>(...)` must match the `KeyHandlerCallable` signature: `NotImplementedOrNone? Handler(KeyPressEvent @event)`. The Python `_accept` and `_fill` functions have no return value, which maps to returning `null` (success) in the C# delegate. This matches the pattern in `SearchBindings` and `ScrollBindings`.

**Alternatives considered**:
- `void` handlers: Not compatible with `KeyHandlerCallable` delegate registration pattern

### R4: Regex Pattern for Word Boundary Splitting

**Question**: How should the Python regex `r"([^\s/]+(?:\s+|/))"` be ported to C#?

**Decision**: Use `System.Text.RegularExpressions.Regex.Split()` with the same pattern `@"([^\s/]+(?:\s+|/))"`.

**Rationale**: The C# `Regex.Split()` method with capturing groups behaves identically to Python's `re.split()` with capturing groups — both include the captured group text in the result array. The pattern itself uses standard regex syntax that works identically in both languages. The Python `next(x for x in t if x)` maps to LINQ `t.First(x => !string.IsNullOrEmpty(x))` or a simple loop.

**Alternatives considered**:
- Pre-compiled `Regex` field: The suggestion text is typically short (single command), so the compilation cost is negligible. However, for correctness, a `GeneratedRegex` source generator attribute could be used. Given the simplicity and the Python source not caching it either, a local `Regex` instance per call is acceptable.
- Manual string scanning: Rejected — faithfulness to Python source requires using regex split with the same pattern

### R5: Buffer.InsertText for Suggestion Acceptance

**Question**: How does suggestion acceptance interact with the buffer?

**Decision**: Call `buffer.InsertText(text)` which inserts text at the current cursor position and clears the existing suggestion.

**Rationale**: The Python source calls `b.insert_text(suggestion.text)`. The Stroke `Buffer.InsertText` method already exists and handles:
- Text insertion at cursor position
- Suggestion clearing (sets `Suggestion = null`)
- Undo history management
- Thread-safe access via Lock

No additional work is needed for buffer integration.

### R6: Existing Dependencies Verification

**Question**: Are all required dependencies already implemented?

**Findings**:
- `AppContext.GetApp()`: EXISTS (`src/Stroke/Application/AppContext.cs`) — thread-safe app access
- `EmacsFilters.EmacsMode`: EXISTS (`src/Stroke/Application/EmacsFilters.cs`) — `Condition` checking `EditingMode.Emacs`
- `Buffer.Suggestion` property: EXISTS (`src/Stroke/Core/Buffer.cs`) — thread-safe get/set
- `Buffer.InsertText()`: EXISTS (`src/Stroke/Core/Buffer.cs`) — inserts text at cursor, clears suggestion
- `Document.IsCursorAtTheEnd`: EXISTS (`src/Stroke/Core/Document.cs:249`) — `_cursorPosition == _text.Length`
- `Suggestion` record: EXISTS (`src/Stroke/AutoSuggest/Suggestion.cs`) — immutable record with `Text` property
- `KeyBindings`: EXISTS (`src/Stroke/KeyBinding/KeyBindings.cs`) — binding registry
- `KeyBindings.Add<T>()`: EXISTS (`src/Stroke/KeyBinding/KeyBindings.cs`) — binding registration
- `KeyHandlerCallable`: EXISTS (`src/Stroke/KeyBinding/KeyHandlerCallable.cs`) — handler delegate type
- `KeyPressEvent`: EXISTS (`src/Stroke/KeyBinding/KeyPressEvent.cs`) — handler parameter type
- `KeyOrChar`: EXISTS (`src/Stroke/KeyBinding/KeyOrChar.cs`) — key specification for binding registration
- `NotImplementedOrNone`: EXISTS (`src/Stroke/KeyBinding/NotImplementedOrNone.cs`) — handler return type
- `FilterOrBool`: EXISTS (`src/Stroke/Filters/FilterOrBool.cs`) — filter wrapper for binding registration
- `Condition`: EXISTS (`src/Stroke/Filters/Condition.cs`) — dynamic filter creation
- `Filter.And()`: EXISTS (`src/Stroke/Filters/Filter.cs`) — filter composition
- `Keys`: EXISTS (`src/Stroke/Input/Keys.cs`) — key constants (ControlF, ControlE, Right, Escape)

**Decision**: All 16 dependencies are available. No new infrastructure needed.
