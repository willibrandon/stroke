# Research: Scroll Bindings

**Feature**: 035-scroll-bindings
**Date**: 2026-01-30

## Research Summary

All items from Technical Context resolved. No NEEDS CLARIFICATION markers remain.

---

## R-001: Application Access Pattern from KeyPressEvent

**Decision**: Use `@event.GetApp()` extension method from `KeyPressEventExtensions` to get typed `Application<object>`, then access `app.Layout.CurrentWindow` for the Window and `app.CurrentBuffer` for the Buffer.

**Rationale**: This is the established pattern used by `NamedCommands.Movement.cs` (line 110: `@event.GetApp().Renderer.Clear()`) and `CompletionBindings.cs`. The Python code accesses `event.app.layout.current_window` and `event.app.current_buffer`.

**Alternatives considered**: Direct `@event.CurrentBuffer!` (only provides Buffer, not Window/Layout).

---

## R-002: Window and RenderInfo Access

**Decision**: Access Window via `app.Layout.CurrentWindow` (type: `Window`, namespace: `Stroke.Layout.Containers`). Access RenderInfo via `window.RenderInfo` (type: `WindowRenderInfo?`, namespace: `Stroke.Layout.Windows`).

**Rationale**: The Python code checks `w and w.render_info` before proceeding. The C# equivalent is null-checking `window.RenderInfo`. `WindowRenderInfo` provides all required properties: `WindowHeight`, `ContentHeight`, `CursorPosition`, `ConfiguredScrollOffsets`, `GetHeightForLine()`, `FirstVisibleLine()`, `LastVisibleLine()`, `UIContent.LineCount`.

**Key properties confirmed**:
- `window.VerticalScroll` (int, get/set, thread-safe via Lock) — mutable scroll offset
- `info.WindowHeight` (int) — rendered height in rows
- `info.ContentHeight` (int) — total content lines
- `info.CursorPosition` (Point) — cursor screen position
- `info.ConfiguredScrollOffsets` (ScrollOffsets) — scroll margin config with `.Top`, `.Bottom`
- `info.GetHeightForLine(int lineNo)` (int) — rendered height of a line
- `info.FirstVisibleLine(bool afterScrollOffset = false)` (int)
- `info.LastVisibleLine(bool beforeScrollOffset = false)` (int)
- `info.UIContent.LineCount` (int) — total logical lines

---

## R-003: Buffer and Document Cursor Manipulation

**Decision**: Use `Buffer.CursorPosition` (get/set) for absolute positioning and `Buffer.Document` methods for relative calculations. Thread-safe operations on Buffer are handled internally.

**Rationale**: The Python code uses `b.cursor_position = ...` for absolute and `b.cursor_position += ...` for relative. The C# Buffer class supports the same patterns.

**Key methods confirmed** (from `Document.LineNavigation.cs`):
- `TranslateRowColToIndex(int row, int col)` — converts (row, col) to text index
- `GetCursorDownPosition(int count = 1)` — relative position delta for moving cursor down
- `GetCursorUpPosition(int count = 1)` — relative position delta for moving cursor up
- `GetStartOfLinePosition(bool afterWhitespace = false)` — relative position delta to start of current line
- `CursorPositionRow` — current row of cursor (zero-based)

---

## R-004: Key Binding Registration Pattern

**Decision**: Use `KeyBindings` class with `Add<KeyHandlerCallable>()` to register key-to-handler mappings. Wrap with `ConditionalKeyBindings` for mode filters. Use `MergedKeyBindings` for combining Vi + Emacs bindings.

**Rationale**: The Python code uses `key_bindings = KeyBindings()`, `handle = key_bindings.add`, `handle("c-f")(scroll_forward)`, then wraps with `ConditionalKeyBindings(key_bindings, vi_mode)`. The C# equivalent uses `keyBindings.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlF)])(handler)`.

**Key classes confirmed**:
- `KeyBindings` (namespace: `Stroke.KeyBinding`) — mutable registry
- `ConditionalKeyBindings` (namespace: `Stroke.KeyBinding`) — wraps with IFilter
- `MergedKeyBindings` (namespace: `Stroke.KeyBinding`) — merges multiple registries
- `IKeyBindingsBase` (namespace: `Stroke.KeyBinding`) — common interface

**Key representation**: `new KeyOrChar(Keys.ControlF)` for special keys, `KeyOrChar.FromKey(Keys.Escape)` pattern.

---

## R-005: Filter Availability

**Decision**: Use `EmacsFilters.EmacsMode`, `ViFilters.ViMode`, and `AppFilters.BufferHasFocus` from `Stroke.Application` namespace.

**Rationale**: These are confirmed to exist as static `IFilter` properties:
- `ViFilters.ViMode` — `new Condition(() => AppContext.GetApp().EditingMode == EditingMode.Vi)`
- `EmacsFilters.EmacsMode` — `new Condition(() => AppContext.GetApp().EditingMode == EditingMode.Emacs)`
- `AppFilters.BufferHasFocus` — checks if a buffer control currently has focus

---

## R-006: File Organization

**Decision**: Create two new files in `src/Stroke/KeyBinding/Bindings/`:
1. `ScrollBindings.cs` — 8 static scroll functions
2. `PageNavigationBindings.cs` — 3 static binding loaders

**Rationale**: This mirrors the Python source organization (`scroll.py` + `page_navigation.py`) and is consistent with how other bindings files are organized in the Stroke codebase (e.g., `CompletionBindings.cs`, `NamedCommands*.cs`). Both files will be well under the 1,000 LOC limit.

**Alternatives considered**: Putting everything in one file (rejected — Python has two separate modules). Using partial classes (rejected — these are independent static classes, not partials of a single entity).

---

## R-007: Handler Signature Compatibility

**Decision**: Scroll functions must match `KeyHandlerCallable` delegate: `NotImplementedOrNone? handler(KeyPressEvent @event)`. The Python functions return `None` (void); the C# equivalents will return `null`.

**Rationale**: The `KeyHandlerCallable` delegate is `public delegate NotImplementedOrNone? KeyHandlerCallable(KeyPressEvent @event)`. All existing handlers (NamedCommands, CompletionBindings) return `null` from successful operations. The `scroll_forward(event, half=False)` Python function with a `half` parameter requires wrapper lambdas for the half-page variants.

---

## R-008: Test Organization

**Decision**: Create test files in `tests/Stroke.Tests/KeyBinding/Bindings/`:
1. `ScrollBindingsTests.cs` — tests for 8 scroll functions
2. `PageNavigationBindingsTests.cs` — tests for 3 binding loaders

**Rationale**: Follows the existing test directory structure where tests mirror source paths (e.g., `NamedCommandsMovementTests.cs`, `NamedCommandsRegistryTests.cs`).

---

## R-009: Thread Safety Classification

**Decision**: Both `ScrollBindings` and `PageNavigationBindings` are stateless static classes requiring no synchronization.

**Rationale**: Per Constitution XI, stateless types are inherently thread-safe. The scroll functions read/write through `Window.VerticalScroll` (which has its own Lock) and `Buffer.CursorPosition` (which has its own Lock). The binding loaders return new `IKeyBindingsBase` instances. No shared mutable state exists in the scroll/navigation classes themselves.
