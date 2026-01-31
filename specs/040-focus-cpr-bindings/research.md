# Research: Focus & CPR Bindings

**Feature**: 040-focus-cpr-bindings
**Date**: 2026-01-31

## Research Tasks

### RT-01: FocusFunctions handler pattern

**Question**: What is the correct handler function signature and access pattern for focus functions?

**Decision**: Use the standard `NotImplementedOrNone?` return type with `KeyPressEvent` parameter, accessing the application layout via `@event.GetApp().Layout`.

**Rationale**: This matches the established pattern used by all other handler functions in `Stroke.Application.Bindings` (ScrollBindings, SearchBindings, AutoSuggestBindings). The `GetApp()` extension method provides typed access to the `Application<object>` instance.

**Alternatives considered**:
- Direct `@event.App` property access — Rejected because it returns `object?` and requires manual casting. `GetApp()` handles this safely.
- Returning `void` — Rejected because all binding handlers in Stroke use `NotImplementedOrNone?` for consistency with the `KeyHandlerCallable` delegate.

**Source evidence**:
- `ScrollBindings.cs`: `@event.GetApp().Layout.CurrentWindow`
- `SearchBindings.cs`: `@event.GetApp()` pattern throughout
- `AutoSuggestBindings.cs`: `@event.CurrentBuffer` for buffer access
- Python `focus.py`: `event.app.layout.focus_next()` → C# `@event.GetApp().Layout.FocusNext()`

### RT-02: CPR data parsing pattern

**Question**: How should CPR response data be parsed from `KeyPressEvent.Data`?

**Decision**: Parse `@event.Data` by stripping the leading `\x1b[` (2 chars) and trailing `R` (1 char), then splitting on `;` to extract row and column as integers.

**Rationale**: This exactly mirrors the Python implementation: `event.data[2:-1].split(";")`. In C#, `@event.Data[2..^1].Split(';')` achieves the same result using range syntax.

**Alternatives considered**:
- Regex parsing — Rejected as unnecessary complexity for a well-defined format. The Python source uses simple string slicing.
- Span-based parsing — Rejected as premature optimization for a non-hot-path handler.

**Source evidence**:
- Python `cpr.py` line 25: `row, col = map(int, event.data[2:-1].split(";"))`
- `MouseBindings.cs` uses similar `@event.Data` string indexing patterns
- CPR response format is standardized: `\x1b[<row>;<col>R`

### RT-03: saveBefore parameter for CPR binding

**Question**: How to register a binding with `saveBefore` disabled?

**Decision**: Pass `saveBefore: _ => false` to `KeyBindings.Add<T>()`. The `saveBefore` parameter accepts `Func<KeyPressEvent, bool>?` with default `_ => true`.

**Rationale**: The Python source uses `save_before=lambda e: False`. The C# `Binding` class already supports this via the `saveBefore` constructor parameter (confirmed in `Binding.cs:69` and `KeyBindings.cs:81`).

**Alternatives considered**: None — this is the only correct approach, matching both Python semantics and the existing C# infrastructure.

**Source evidence**:
- Python `cpr.py` line 18: `@key_bindings.add(Keys.CPRResponse, save_before=lambda e: False)`
- `Binding.cs:69`: `Func<KeyPressEvent, bool>? saveBefore = null`
- `Binding.cs:93`: `SaveBefore = saveBefore ?? (_ => true);`

### RT-04: Keys.CPRResponse enum value

**Question**: What is the exact enum member name for CPR response in the Keys enum?

**Decision**: Use `Keys.CPRResponse` (all caps "CPR", PascalCase "Response").

**Rationale**: The actual enum in `Keys.cs:794` is `CPRResponse`. The api-mapping.md shows `Keys.CprResponse` but the implementation uses `CPRResponse`. Implementation takes precedence over mapping documentation.

**Source evidence**:
- `Keys.cs:794`: `CPRResponse,`
- `api-mapping.md:1220`: `Keys.CPRResponse → Keys.CprResponse` (naming discrepancy; actual code uses `CPRResponse`)

### RT-05: Test infrastructure for Application.Bindings

**Question**: What test patterns are used for testing binding handlers in `Stroke.Tests/Application/Bindings/`?

**Decision**: Follow the established pattern from existing test files: create a real `Layout` with focusable windows, a real `Renderer`, and a real `Application`, then construct `KeyPressEvent` objects and invoke handlers directly.

**Rationale**: Constitution VIII forbids mocks. All existing binding tests (ScrollBindingsTests, SearchBindingsTests, AutoSuggestBindingsTests) use real object graphs.

**Alternatives considered**: None — Constitution VIII is non-negotiable on this point.

**Source evidence**:
- `AutoSuggestBindingsTests.cs`, `ScrollBindingsTests.cs`, `SearchBindingsTests.cs` — all use real object construction
- Test files use helper patterns like `CreateEnvironment()` and `CreateEvent()`

## Summary

All research tasks resolved. No NEEDS CLARIFICATION items remain. The implementation is straightforward:
- FocusFunctions: 2 thin delegation methods following established handler patterns
- CprBindings: 1 factory method with 1 binding registration and CPR data parsing
- Tests: Real objects, direct handler invocation, standard xUnit assertions
