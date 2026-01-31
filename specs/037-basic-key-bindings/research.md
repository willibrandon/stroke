# Research: Basic Key Bindings

**Feature**: 037-basic-key-bindings
**Date**: 2026-01-30

## R-001: KeyBindings Registration Pattern for Named Commands

**Decision**: Use `kb.Add<Binding>([keys], filter, saveBefore)(NamedCommands.GetByName("name"))` for named commands, and `kb.Add<KeyHandlerCallable>([keys], filter)(handler)` for inline lambda handlers.

**Rationale**: The `KeyBindings.Add<T>()` method (line 76-149 of `KeyBindings.cs`) accepts both `Binding` and `KeyHandlerCallable` as generic type parameters. When `T` is `Binding`, the decorator extracts the handler from the existing binding and composes filters (AND for filter, OR for eager/isGlobal). When `T` is `KeyHandlerCallable`, it creates a new `Binding` directly. Using `Add<Binding>` with `NamedCommands.GetByName()` is the most faithful port of Python's `handle("key")(get_by_name("name"))` pattern.

**Alternatives Considered**:
- `kb.Add<KeyHandlerCallable>([keys])(NamedCommands.GetByName("name").Handler)` — extracts handler manually, losing the Binding's own filter/eager composition. Less faithful.
- Direct `Binding` construction and manual insertion — bypasses the thread-safe Add mechanism.

## R-002: Filter Composition for Insert Mode

**Decision**: Create the insert mode filter as `((Filter)ViFilters.ViInsertMode) | EmacsFilters.EmacsInsertMode`.

**Rationale**: The `|` operator is defined on the `Filter` base class (`Filter.cs:156`). Since `ViFilters.ViInsertMode` and `EmacsFilters.EmacsInsertMode` are typed as `IFilter` (but are actually `Condition : Filter` instances), one side must be cast to `Filter` to resolve the operator. The existing codebase uses this pattern — e.g., `CompletionsMenu.cs:65` uses `((Filter)filter).And(...)`.

**Alternatives Considered**:
- `.Or()` extension method chain — works but less readable than the `|` operator that mirrors Python's `vi_insert_mode | emacs_insert_mode`.

## R-003: Dynamic Filter Conditions (HasTextBeforeCursor, InQuotedInsert)

**Decision**: Define these as private static `IFilter` properties within `BasicBindings` using `new Condition(() => ...)`, following the same pattern as `AppFilters`.

**Rationale**: Python defines `has_text_before_cursor` and `in_quoted_insert` as module-level `@Condition`-decorated functions in `basic.py` (lines 32-39). These are local to the `basic` module and not shared with other binding modules. Defining them as private static properties within `BasicBindings` is the faithful C# equivalent — private scope matches the module-private Python scope.

**Alternatives Considered**:
- Adding to `AppFilters` as public properties — would expose implementation details beyond the basic bindings module; not present in the Python source as public API.
- Creating a separate `BasicFilters` static class — over-engineering for two private filters.

## R-004: Ignored Keys Registration Strategy

**Decision**: Register each ignored key with its own individual `kb.Add<KeyHandlerCallable>([key])(_ignore)` call, sharing a single static no-op handler. Use `Keys` enum values directly (not string parsing) for all keys that have enum values, and `new KeyOrChar(Keys.SIGINT)` for `<sigint>`.

**Rationale**: Python uses the `@handle` decorator stacking pattern where the same handler is decorated 80+ times. In C#, each `Add` call is a separate statement. Using a shared static handler (`_ignore`) avoids lambda allocation per binding. Using `Keys` enum values directly is more type-safe and avoids runtime string parsing overhead.

**Alternatives Considered**:
- Loop over string array with `KeyBindingUtils.ParseKey()` — adds runtime parsing cost, harder to verify at compile time. The features doc (59-basicbindings.md) shows this approach but it's pseudocode, not the actual registration API.
- Individual lambdas per key — wasteful allocation of identical delegates.

## R-005: KeyProcessor.Feed for Ctrl+J Re-dispatch

**Decision**: Cast `@event.KeyProcessor` to `KeyProcessor` and call `Feed(new KeyPress(Keys.ControlM, "\r"), first: true)`.

**Rationale**: `KeyPressEvent.KeyProcessor` returns `object` (due to early implementation typing). The actual type is `KeyProcessor` (from `Stroke.KeyBinding`). The Python source (`basic.py:202`) calls `event.key_processor.feed(KeyPress(Keys.ControlM, "\r"), first=True)`. The C# `KeyProcessor.Feed(KeyPress keyPress, bool first = false)` method at `KeyProcessor.cs:85` has the exact same signature.

**Alternatives Considered**:
- Using a typed `KeyProcessor` property on `KeyPressEvent` — would require changing the existing class API, which is outside the scope of this feature.

## R-006: Application Access for QuotedInsert and Clipboard

**Decision**: Use the existing `KeyPressEventExtensions.GetApp()` extension method to get `Application<object>`, then access `.QuotedInsert` and `.Clipboard.SetData()`.

**Rationale**: `KeyPressEventExtensions.GetApp()` (at `KeyPressEventExtensions.cs:24`) is the established pattern for typed Application access from key handlers. `Application<TResult>.QuotedInsert` is a thread-safe `bool` property (`Application.cs:366-371`). `Application<TResult>.Clipboard` returns `IClipboard` which has `SetData(ClipboardData data)`.

**Alternatives Considered**: None — this is the only established access pattern.

## R-007: File Placement and Namespace

**Decision**: Place `BasicBindings.cs` in `src/Stroke/Application/Bindings/BasicBindings.cs` under namespace `Stroke.Application.Bindings`.

**Rationale**: The existing binding loader classes (`ScrollBindings.cs`, `PageNavigationBindings.cs`, `MouseBindings.cs`) are all in `Stroke.Application.Bindings`. Following Constitution III (Layered Architecture), binding loaders that reference `AppFilters`, `ViFilters`, `EmacsFilters`, and `Application<T>` belong in the Application layer. The features doc (`59-basicbindings.md`) suggests `Stroke.KeyBinding.Bindings` but since this class uses `AppFilters`, `ViFilters`, `EmacsFilters` from `Stroke.Application`, it must be in the Application layer to avoid circular dependencies.

**Alternatives Considered**:
- `Stroke.KeyBinding.Bindings` — would create a circular dependency: KeyBinding layer would depend on Application layer (for AppFilters), violating Constitution III.

## R-008: Test File Organization

**Decision**: Place tests in `tests/Stroke.Tests/Application/Bindings/BasicBindingsTests.cs`, split into multiple files if approaching 1,000 LOC: `BasicBindingsIgnoredKeysTests.cs`, `BasicBindingsReadlineTests.cs`, `BasicBindingsHandlerTests.cs`.

**Rationale**: Constitution X limits files to 1,000 LOC. With 18 functional requirements and ~7 user stories generating 30+ test cases, plus test infrastructure, tests will approach or exceed the limit. The existing pattern (`ScrollBindingsTests.cs` at 656 lines for 8 scroll functions) suggests ~50 lines per test case average. For ~40 test cases, we'd need ~2,000 lines — requiring a split.

**Alternatives Considered**:
- Single test file — would exceed 1,000 LOC limit.
- Per-user-story files — would create 7 small files, excessive fragmentation.

## R-009: Save-Before Callback (IfNoRepeat)

**Decision**: Define `IfNoRepeat` as a private static method: `private static bool IfNoRepeat(KeyPressEvent @event) => !@event.IsRepeat;`

**Rationale**: Python defines `if_no_repeat` as a module-level function (line 26-29). It's used as the `save_before` parameter for Backspace, Delete, Ctrl+Delete, and self-insert bindings (FR-016). The C# `saveBefore` parameter has type `Func<KeyPressEvent, bool>?`, and a static method reference is the cleanest C# equivalent.

**Alternatives Considered**:
- Lambda `e => !e.IsRepeat` — allocates a delegate per usage site instead of sharing one.
- Separate utility class — over-engineering for a single private helper.

## R-010: InPasteMode Filter Access for Enter Handler

**Decision**: Use `AppFilters.InPasteMode` and call its filter evaluation within the Enter handler: `@event.CurrentBuffer!.Newline(copyMargin: !((Filter)AppFilters.InPasteMode).Invoke())`.

**Rationale**: Python calls `in_paste_mode()` as a function (line 193). `AppFilters.InPasteMode` is an `IFilter` with an `Invoke()` method that evaluates the condition at runtime. The `Filter` base class has an `Invoke()` method. Since `AppFilters.InPasteMode` is a `Condition : Filter`, casting and calling `Invoke()` evaluates the filter dynamically in the handler.

**Alternatives Considered**:
- Directly accessing `AppContext.GetApp().PasteMode.Invoke()` — duplicates the filter logic; the AppFilters abstraction exists for this purpose.
