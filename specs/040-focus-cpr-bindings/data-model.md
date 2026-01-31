# Data Model: Focus & CPR Bindings

**Feature**: 040-focus-cpr-bindings
**Date**: 2026-01-31

## Entities

This feature introduces no new data entities. Both modules are stateless functions that operate on existing entities:

### Existing Entities Referenced

| Entity | Location | Role in Feature |
|--------|----------|-----------------|
| `KeyPressEvent` | `Stroke.KeyBinding` | Input to all handler functions; provides `GetApp()` and `Data` |
| `KeyBindings` | `Stroke.KeyBinding` | Registry returned by `LoadCprBindings()` factory |
| `Application<TResult>` | `Stroke.Application` | Accessed via `@event.GetApp()`; provides `Layout` and `Renderer` |
| `Layout` | `Stroke.Layout` | Target of `FocusNext()` / `FocusPrevious()` delegation |
| `Renderer` | `Stroke.Rendering` | Target of `ReportAbsoluteCursorRow(row)` call |
| `Keys` | `Stroke.Input` | Enum providing `CPRResponse` constant |

### Data Flow

```
Focus Flow:
  KeyPressEvent → GetApp() → Application.Layout → FocusNext() / FocusPrevious()

CPR Flow:
  KeyPressEvent.Data → parse "\x1b[row;colR" → (row, col) → Renderer.ReportAbsoluteCursorRow(row)
```

## State Transitions

No new state machines or transitions. Focus navigation modifies the existing `Layout._stack` (focus stack) which is already thread-safe. CPR handling modifies the existing `Renderer._cprSupport` state which is already thread-safe.

## Validation Rules

- CPR data parsing trusts terminal to send well-formed data (matching Python behavior — no defensive parsing)
- Focus functions are no-ops when `Layout` has zero visible focusable windows (handled by `Layout.FocusNext()`/`FocusPrevious()` internally)
