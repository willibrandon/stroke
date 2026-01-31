# Research: Named Commands

**Feature**: 034-named-commands
**Date**: 2026-01-30

## R-001: Registry Pattern — Static Dictionary vs Dependency Injection

**Decision**: Use a static `partial class NamedCommands` with a static `ConcurrentDictionary<string, Binding>` initialized via a static constructor that registers all built-in commands.

**Rationale**: The Python source uses a module-level `_readline_commands: dict[str, Binding]` dictionary with a `register(name)` decorator. The C# equivalent is a static class with a static dictionary. `ConcurrentDictionary` provides thread-safe reads and writes without explicit locking, which is simpler and more performant than `Dictionary` + `Lock` for this use case.

**Alternatives considered**:
- Instance-based registry injected via DI: Rejected because Python uses module-level global state, and the constitution requires faithful porting. Named commands are global by nature (Readline command names are a fixed vocabulary).
- `Dictionary<string, Binding>` with `Lock`: Rejected because `ConcurrentDictionary` is the idiomatic .NET choice for a thread-safe dictionary with simple read/write patterns and avoids lock contention on reads.
- `ImmutableDictionary<string, Binding>` with atomic swap: Rejected because `Register` mutates in place (matching Python's `_readline_commands[name] = binding`); immutable swap adds complexity without benefit.

## R-002: Handler Signature — KeyHandlerCallable Delegate

**Decision**: Named command handlers will be static methods with the signature `static NotImplementedOrNone? HandlerName(KeyPressEvent @event)` matching the `KeyHandlerCallable` delegate. Each handler method is registered as a `Binding` by wrapping it via the `Binding` constructor with `Keys = [Keys.Any]` (a placeholder key sequence, matching how Python's `key_binding()` decorator creates a Binding without specifying keys).

**Rationale**: Python's `named_commands.py` uses `@register(name)` which internally calls `key_binding()(handler)` to wrap plain handler functions into `Binding` objects. The `key_binding()` factory creates a `Binding` with default filter/eager/global settings and `Keys.Any` as the key sequence. In C#, we create `Binding` objects directly via the constructor with `[Keys.Any]` as the key sequence, matching the Python pattern.

**Alternatives considered**:
- Store raw `KeyHandlerCallable` delegates instead of `Binding` objects: Rejected because Python stores `Binding` objects in the registry, and consumers (e.g., key binding configurations) expect `Binding` instances with their full metadata (RecordInMacro, Filter, etc.).
- Use `Action<KeyPressEvent>` instead of `KeyHandlerCallable`: Rejected because `KeyHandlerCallable` is the established delegate type in Stroke's key binding system and returns `NotImplementedOrNone?`.

## R-003: Completion Commands — Dependency on Missing CompletionBindings Module

**Decision**: The three completion commands (`complete`, `menu-complete`, `menu-complete-backward`) will be registered with handlers that delegate to `DisplayCompletionsLikeReadline` and `GenerateCompletions` helper functions. These helper functions will be implemented as part of this feature in a separate `CompletionBindings` static class in the `Stroke.KeyBinding.Bindings` namespace, porting Python's `prompt_toolkit.key_binding.bindings.completion` module's two public functions.

**Rationale**: Python's `named_commands.py` imports `display_completions_like_readline` and `generate_completions` from `.completion`. These are standalone functions, not part of a complex module. The `generate_completions` function is ~8 lines; `display_completions_like_readline` is ~30 lines (the `_display_completions_like_readline` internal async function is longer but is a separate helper). These are small enough to implement alongside the named commands.

**Alternatives considered**:
- Stub the completion commands with no-ops: Rejected per Constitution VII (Full Scope Commitment — no deferral).
- Create a full CompletionBindings feature: Overkill. Only the two functions imported by `named_commands.py` are needed. The full completion key bindings (tab, shift-tab, etc.) are a separate feature.

## R-004: Application Access Pattern — KeyPressEvent.App Typing

**Decision**: The `KeyPressEvent.App` property is currently typed as `object?`. Named command handlers will cast `event.App` to `Application<object>` (or the appropriate generic type) to access `Renderer`, `Clipboard`, `EditingMode`, `EmacsState`, `KeyProcessor`, `Layout`, `Output`, `QuotedInsert`, `PreRunCallables`, and `Exit()`. A helper property or extension method `GetApp()` will provide the typed cast.

**Rationale**: The existing `KeyPressEvent.App` returns `object?` because it was created before the Application class was fully implemented. The Application class is now available. Many named command handlers need typed access to Application properties (clipboard, renderer, editing mode, emacs state, etc.).

**Alternatives considered**:
- Change `KeyPressEvent.App` to `Application<object>`: This would be a breaking change to an existing public API and affect all existing consumers. Better to add a helper.
- Use `dynamic` for App access: Rejected for lack of type safety and performance.

## R-005: `call-last-kbd-macro` — RecordInMacro=false Binding

**Decision**: The `call-last-kbd-macro` command is the only named command that requires `recordInMacro: false` on its `Binding`. In Python, this is achieved by applying `@key_binding(record_in_macro=False)` before the `@register` decorator. In C#, the `Binding` constructor will be called with `recordInMacro: new FilterOrBool(false)` for this specific command.

**Rationale**: Direct translation of the Python pattern. The `record_in_macro=False` prevents the macro replay key sequence from being recorded into the macro itself, which would cause infinite recursion.

**Alternatives considered**: None — this is a direct faithful port.

## R-006: `insert-comment` — Numeric Argument Semantics

**Decision**: In Python, `insert-comment` checks `event.arg != 1` to determine comment vs uncomment behavior. When no arg is provided, `event.arg` defaults to 1, so the default behavior is to comment. When any explicit arg is provided (including 1 via explicit input), it still evaluates as `!= 1` only if arg is not 1. However, the Readline spec says "without numeric argument" = comment, "with numeric argument" = uncomment. Python's implementation uses `event.arg != 1` as a proxy: the default arg is 1 (no explicit arg), and any explicit arg triggers uncomment. This is a known imperfect mapping but is the faithful port behavior.

**Rationale**: Constitution I requires faithful porting. The Python implementation's behavior is the authoritative reference.

**Alternatives considered**: Using `event.ArgPresent` to distinguish "no argument" from "argument of 1" would be more semantically correct per Readline docs, but would deviate from the Python source behavior. The faithful port takes precedence.

## R-007: `print-last-kbd-macro` — RunInTerminal Usage

**Decision**: The `print-last-kbd-macro` handler will use `Stroke.Application.RunInTerminal.RunAsync()` to temporarily suspend the UI and print the macro to the terminal, matching the Python implementation which uses `run_in_terminal(print_macro)`.

**Rationale**: Direct port of `run_in_terminal(print_macro)` from the Python source. The `RunInTerminal` static class already exists in `Stroke.Application`.

**Alternatives considered**: None — direct faithful port.

## R-008: `edit-and-execute-command` — OpenInEditor Async

**Decision**: The `edit-and-execute-command` handler will call `buff.OpenInEditorAsync(validateAndHandle: true)`. Since `KeyHandlerCallable` returns `NotImplementedOrNone?` (synchronous), the handler will fire-and-forget the async operation, consistent with how the Python version calls `buff.open_in_editor(validate_and_handle=True)` without awaiting.

**Rationale**: The Python source does not await the `open_in_editor` call. The `Buffer.OpenInEditorAsync` method handles its own lifecycle. Fire-and-forget matches the Python semantics.

**Alternatives considered**:
- Making the handler async using `AsyncKeyHandlerCallable`: Would change the binding type and doesn't match the Python pattern where this is a regular synchronous handler.

## R-009: `reverse-search-history` — Layout/Control Access

**Decision**: The handler accesses `event.App` → cast to Application → `Layout.CurrentControl` → check if `BufferControl` → access `SearchBufferControl` → set as current control. Also sets `CurrentSearchState.Direction = SearchDirection.Backward`.

**Rationale**: Direct port of the Python implementation which accesses `event.app.layout.current_control` and checks `isinstance(control, BufferControl)`.

**Alternatives considered**: None — direct faithful port.

## R-010: Test Strategy — Real Buffer/Application Instances

**Decision**: Tests will create real `Buffer` instances with pre-set `Document` text and cursor positions, and real `Application<object>` instances (or lightweight test harnesses) to provide clipboard, emacs state, etc. `KeyPressEvent` instances will be constructed with the necessary state. No mocks.

**Rationale**: Constitution VIII forbids mocks, fakes, doubles, and simulations. The existing test infrastructure creates real `Buffer` and `Application` instances.

**Alternatives considered**: None — Constitution VIII is non-negotiable.

## R-011: File Organization — Partial Class Split

**Decision**: Use C# `partial class` to split `NamedCommands` across 8 files:
- `NamedCommands.cs` — static constructor (registers all commands), `GetByName`, `Register`, dictionary field
- `NamedCommands.Movement.cs` — 10 movement handler methods
- `NamedCommands.History.cs` — 6 history handler methods
- `NamedCommands.TextEdit.cs` — 9 text editing handler methods
- `NamedCommands.KillYank.cs` — 10 kill/yank handler methods
- `NamedCommands.Completion.cs` — 3 completion handler methods + CompletionBindings helpers
- `NamedCommands.Macro.cs` — 4 macro handler methods
- `NamedCommands.Misc.cs` — 7 miscellaneous handler methods

**Rationale**: Constitution X requires files under 1,000 LOC. The Python source is 692 lines in a single file. With C# verbosity (XML docs, namespace, using directives, explicit types), the total would exceed 1,000 LOC in a single file. Partial class provides clean logical separation while maintaining a unified API.

**Alternatives considered**:
- Separate static classes per category (e.g., `MovementCommands`, `HistoryCommands`): Rejected because Python has a single registry; a single `NamedCommands` class is the faithful port. Multiple classes would fragment the API.
- No file splitting: Would violate Constitution X.
