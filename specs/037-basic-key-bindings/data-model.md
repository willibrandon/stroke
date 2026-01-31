# Data Model: Basic Key Bindings

**Feature**: 037-basic-key-bindings
**Date**: 2026-01-30

## Entities

### BasicBindings (Static Class)

| Property | Type | Description |
|----------|------|-------------|
| N/A | Static class | No instance state — factory method only |

**Methods:**

| Method | Return Type | Parameters | Description |
|--------|-------------|------------|-------------|
| `LoadBasicBindings()` | `KeyBindings` | None | Creates and returns all basic key bindings |

**Private Members:**

| Member | Type | Description |
|--------|------|-------------|
| `InsertMode` | `IFilter` (static) | `ViInsertMode \| EmacsInsertMode` composite filter |
| `HasTextBeforeCursor` | `IFilter` (static) | `Condition(() => AppContext.GetApp().CurrentBuffer.Text.Length > 0)` |
| `InQuotedInsert` | `IFilter` (static) | `Condition(() => AppContext.GetApp().QuotedInsert)` |
| `IfNoRepeat` | `Func<KeyPressEvent, bool>` (static method) | `!@event.IsRepeat` |
| `Ignore` | `KeyHandlerCallable` (static) | No-op handler returning `null` |

### Binding Registration Groups

The `LoadBasicBindings` method registers bindings in the following order, matching Python source exactly:

| Group | Count | Filter | SaveBefore | Description |
|-------|-------|--------|------------|-------------|
| Ignored keys | 90 | None (Always) | Default | Control keys, F-keys, nav keys with modifiers, SIGINT, Ignore |
| Readline movement | 7 | None (Always) | Default | Home, End, Left, Right, Ctrl+Up, Ctrl+Down, Ctrl+L |
| Readline editing | 7 | `InsertMode` | Varies | Ctrl+K, Ctrl+U, Backspace, Delete, Ctrl+Delete, Ctrl+T, Ctrl+W |
| Self-insert | 1 | `InsertMode` | `IfNoRepeat` | Keys.Any → self-insert |
| Tab completion | 2 | `InsertMode` | Default | Tab, Shift+Tab |
| History nav | 2 | `~HasSelection` | Default | PageUp, PageDown |
| Ctrl+D | 1 | `HasTextBeforeCursor & InsertMode` | Default | delete-char |
| Enter multiline | 1 | `InsertMode & IsMultiline` | Default | Inline handler: Newline |
| Ctrl+J | 1 | None (Always) | Default | Inline handler: re-dispatch as Ctrl+M |
| Auto up/down | 2 | None (Always) | Default | Inline handlers: AutoUp/AutoDown |
| Delete selection | 1 | `HasSelection` | Default | Inline handler: CutSelection |
| Ctrl+Z | 1 | None (Always) | Default | Inline handler: InsertText |
| Bracketed paste | 1 | None (Always) | Default | Inline handler: normalize + InsertText |
| Quoted insert | 1 | `InQuotedInsert` | Default (eager) | Inline handler: literal InsertText |

### Dependencies (Existing Entities)

| Entity | Location | Usage |
|--------|----------|-------|
| `KeyBindings` | `Stroke.KeyBinding.KeyBindings` | Return type of `LoadBasicBindings()` |
| `Binding` | `Stroke.KeyBinding.Binding` | Named command lookup result |
| `NamedCommands` | `Stroke.KeyBinding.Bindings.NamedCommands` | 16 named commands referenced |
| `KeyPressEvent` | `Stroke.KeyBinding.KeyPressEvent` | Handler parameter |
| `KeyHandlerCallable` | `Stroke.KeyBinding.KeyHandlerCallable` | Handler delegate type |
| `KeyOrChar` | `Stroke.KeyBinding.KeyOrChar` | Key specification |
| `KeyPress` | `Stroke.KeyBinding.KeyPress` | Ctrl+J re-dispatch |
| `Keys` | `Stroke.Input.Keys` | Key enum values |
| `IFilter` | `Stroke.Filters.IFilter` | Filter conditions |
| `Condition` | `Stroke.Filters.Condition` | Dynamic filter creation |
| `Filter` | `Stroke.Filters.Filter` | Operator composition |
| `FilterOrBool` | `Stroke.Filters.FilterOrBool` | Add method parameter |
| `AppFilters` | `Stroke.Application.AppFilters` | HasSelection, IsMultiline, InPasteMode |
| `ViFilters` | `Stroke.Application.ViFilters` | ViInsertMode |
| `EmacsFilters` | `Stroke.Application.EmacsFilters` | EmacsInsertMode |
| `AppContext` | `Stroke.Application.AppContext` | Current app access |
| `Application<T>` | `Stroke.Application.Application<T>` | QuotedInsert, Clipboard |
| `KeyPressEventExtensions` | `Stroke.KeyBinding.Bindings.KeyPressEventExtensions` | GetApp() |
| `Buffer` | `Stroke.Core.Buffer` | AutoUp, AutoDown, CutSelection, Newline, InsertText |
| `KeyProcessor` | `Stroke.KeyBinding.KeyProcessor` | Feed() for Ctrl+J |
| `IClipboard` | `Stroke.Clipboard.IClipboard` | SetData() |
| `ClipboardData` | `Stroke.Clipboard.ClipboardData` | CutSelection return type |
| `NotImplementedOrNone` | `Stroke.KeyBinding.NotImplementedOrNone` | Handler return type |

### Named Commands Referenced (16 total)

| Command Name | Category | Used By |
|-------------|----------|---------|
| `beginning-of-line` | Movement | Home |
| `end-of-line` | Movement | End |
| `backward-char` | Movement | Left |
| `forward-char` | Movement | Right |
| `previous-history` | History | Ctrl+Up, PageUp |
| `next-history` | History | Ctrl+Down, PageDown |
| `clear-screen` | Misc | Ctrl+L |
| `kill-line` | Kill/Yank | Ctrl+K |
| `unix-line-discard` | Kill/Yank | Ctrl+U |
| `backward-delete-char` | Text Edit | Backspace |
| `delete-char` | Text Edit | Delete, Ctrl+Delete, Ctrl+D |
| `transpose-chars` | Text Edit | Ctrl+T |
| `unix-word-rubout` | Kill/Yank | Ctrl+W |
| `self-insert` | Text Edit | Keys.Any |
| `menu-complete` | Completion | Tab (Ctrl+I) |
| `menu-complete-backward` | Completion | Shift+Tab |

## State Transitions

None — `BasicBindings` is a stateless factory. The bindings it creates modify `Buffer` state through the existing `Buffer` API. Filter conditions query application state through the existing `AppContext`/`AppFilters` system.

## Validation Rules

- `LoadBasicBindings()` must return a non-null `KeyBindings` instance
- The returned `KeyBindings` must contain exactly the set of bindings defined in Python's `basic.py`
- All 16 named commands must resolve successfully via `NamedCommands.GetByName()`
- Filter compositions must short-circuit correctly (e.g., `InsertMode` must be `false` in Vi navigation mode)
