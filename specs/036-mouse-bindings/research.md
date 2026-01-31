# Research: Mouse Bindings

**Feature**: 036-mouse-bindings
**Date**: 2026-01-30

## R1: MouseModifiers Representation — `FrozenSet<MouseModifier>` vs `[Flags]` enum

**Decision**: Use existing `MouseModifiers` flags enum (already implemented in Feature 013)

**Rationale**: The Python source uses `frozenset[MouseModifier]` for modifier combinations. The Stroke codebase already adapted this to a `[Flags] public enum MouseModifiers` with values `None = 0`, `Shift = 1`, `Alt = 2`, `Control = 4` in `Stroke.Input.MouseModifiers`. This is a documented C# language adaptation that is more efficient (no allocation, O(1) bitwise operations) and already established across the codebase. The feature spec document at `docs/features/62-mousebindings.md` references `FrozenSet<MouseModifier>`, but this does not match the actual implementation. The lookup tables will use `MouseModifiers` enum values directly.

**Alternatives considered**:
- `FrozenSet<MouseModifier>`: Would match Python exactly but contradicts the existing `MouseModifiers` flags enum and `MouseEvent` record struct which uses `MouseModifiers Modifiers` property. Would require a breaking change to Feature 013.

## R2: Lookup Table Data Structure

**Decision**: Use `FrozenDictionary<TKey, TValue>` for all three lookup tables

**Rationale**: `FrozenDictionary` (from `System.Collections.Frozen`) provides O(1) immutable lookup with zero runtime allocation after initialization. The tables are static, read-only, and queried on every mouse event — `FrozenDictionary` is the optimal data structure. XTerm SGR uses `FrozenDictionary<(int Code, char Suffix), (MouseButton, MouseEventType, MouseModifiers)>` (tuple key). Typical and URXVT use `FrozenDictionary<int, (MouseButton, MouseEventType, MouseModifiers)>`.

**Alternatives considered**:
- `Dictionary<K,V>`: Mutable, slower for read-heavy patterns, not inherently thread-safe
- `ImmutableDictionary<K,V>`: O(log n) lookup — slower than FrozenDictionary for this use case
- `ReadOnlyDictionary<K,V>`: Wrapper over mutable dictionary, no optimization for frozen content

## R3: Renderer Private Field Access (`_cursorPos`)

**Decision**: Add `internal Point CursorPos` property to `Renderer` for Windows mouse event coordinate adjustment

**Rationale**: The Python code accesses `event.app.renderer._cursor_pos.y` in the Windows mouse handler (line 328). In Python, the underscore prefix is a naming convention (not enforced access control). In C#, `_cursorPos` is a truly private field on `Renderer`. The Windows handler needs this value to compute `rowsAboveCursor = screenBufferInfo.CursorPosition.Y - renderer.CursorPos.Y`. Adding an `internal` property follows the principle of least privilege — accessible within the Stroke assembly but not to external consumers. This is necessary for the faithful port of the Windows handler logic.

**Alternatives considered**:
- Make `_cursorPos` public: Over-exposes implementation detail
- Pass cursor pos through Application: Changes Application API not present in Python
- Skip Windows handler: Violates Constitution VII (Full Scope)

## R4: Win32Output Dependency

**Decision**: Windows handler uses type-check pattern (`output is Win32Output`) but `Win32Output` class does not yet exist. Use `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)` as the outer guard, and a dynamic type check for the actual output implementation. If no `Win32Output` exists at compile time, the handler returns `NotImplemented` on all platforms — functionally correct since no Win32 output can be active without the class existing.

**Rationale**: The Python source imports `Win32Output` and `Windows10_Output` lazily (inside the handler function) and checks `isinstance(output, (Win32Output, Windows10_Output))`. Since these classes are not yet ported (they are part of the Output system Feature 21/57), the C# handler should be structured to allow future Win32Output integration without modification. The handler body is gated by `RuntimeInformation.IsOSPlatform` first, then checks output type. On non-Windows platforms, it short-circuits immediately. On Windows, if no compatible output type is found, it returns `NotImplemented`.

**Alternatives considered**:
- Defer Windows handler entirely: Violates Constitution VII
- Stub Win32Output: Violates Constitution VIII (no fakes)
- Compile-time #if WINDOWS: Removes the handler from non-Windows builds, diverging from Python behavior

## R5: `KeyPressEvent.App` Access Pattern

**Decision**: Use existing `KeyPressEventExtensions.GetApp()` extension method to access the typed `Application<object>` from `KeyPressEvent`

**Rationale**: The Python code accesses `event.app` which is a typed `Application` reference. In Stroke, `KeyPressEvent.App` is `object?`. The existing internal extension method `GetApp()` (in `Stroke.KeyBinding.Bindings.KeyPressEventExtensions`) casts to `Application<object>` and throws `InvalidOperationException` if null or wrong type. This pattern is already used by `ScrollBindings`, `NamedCommands`, `CompletionBindings`, and all other binding implementations.

**Alternatives considered**: None — this is the established pattern.

## R6: Namespace Placement

**Decision**: Place `MouseBindings` in `Stroke.KeyBinding.Bindings` namespace, file at `src/Stroke/KeyBinding/Bindings/MouseBindings.cs`

**Rationale**: The api-mapping.md maps `prompt_toolkit.key_binding.bindings` → `Stroke.KeyBinding.Bindings`. The feature document `docs/features/62-mousebindings.md` explicitly specifies this namespace. Existing binding classes (`CompletionBindings`, `NamedCommands`) already reside here. The `KeyPressEventExtensions` helper is also in this namespace, providing `GetApp()` access.

Note: `ScrollBindings` and `PageNavigationBindings` are in `Stroke.Application.Bindings` rather than `Stroke.KeyBinding.Bindings`. This is because they depend on `Stroke.Application` types (AppFilters, EmacsFilters, ViFilters) for conditional key binding wrappers. `MouseBindings` does NOT use conditional wrappers or mode-specific filters, so it belongs in the lower-layer `Stroke.KeyBinding.Bindings` namespace. However, the VT100 handler accesses `@event.GetApp().Renderer` which is an `Application` type — this is a runtime-only dependency through the `KeyPressEvent.App` property, not a compile-time project reference issue, since `Application` is defined in the same assembly.

**Alternatives considered**: `Stroke.Application.Bindings` — rejected because mouse bindings have no modal filtering and the feature doc specifies `Stroke.KeyBinding.Bindings`.

## R7: HeightIsUnknownException Handling

**Decision**: Catch `HeightIsUnknownException` (from `Stroke.Rendering`) in the VT100 handler when accessing `Renderer.RowsAboveLayout`

**Rationale**: The Python code catches `HeightIsUnknownError` (line 265). In Stroke, `Renderer.RowsAboveLayout` throws `HeightIsUnknownException` when `_minAvailableHeight` is 0 and not in alternate screen. The exception type is defined in the same file as `Renderer` at `Stroke.Rendering.HeightIsUnknownException`. The handler catches this and returns `NotImplementedOrNone.NotImplemented`.

**Alternatives considered**: None — direct 1:1 port of the error handling pattern.

## R8: Python `mouse_handlers.mouse_handlers[y][x]` vs Stroke `MouseHandlers.GetHandler(x, y)`

**Decision**: Use `MouseHandlers.GetHandler(x, y)` — note the parameter order difference

**Rationale**: The Python code accesses `event.app.renderer.mouse_handlers.mouse_handlers[y][x]` where the outer dict is keyed by y (row) and inner by x (column). The Stroke `MouseHandlers.GetHandler(int x, int y)` method takes x first, then y (matching conventional x,y coordinate ordering). The implementation internally maps y→row dictionary, x→column entry. This parameter order difference is intentional in the C# adaptation.

**Alternatives considered**: None — use the existing API as designed.
