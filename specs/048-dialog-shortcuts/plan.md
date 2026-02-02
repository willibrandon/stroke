# Implementation Plan: Dialog Shortcut Functions

**Branch**: `048-dialog-shortcuts` | **Date**: 2026-02-02 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/048-dialog-shortcuts/spec.md`

## Summary

Implement 7 dialog shortcut functions (`YesNoDialog`, `MessageDialog`, `InputDialog`, `ButtonDialog`, `RadioListDialog`, `CheckboxListDialog`, `ProgressDialog`) as static methods on a `Dialogs` class in `Stroke.Shortcuts`, plus a shared `CreateApp` private helper and `ReturnNone` cancel handler. This is a faithful 1:1 port of Python Prompt Toolkit's `shortcuts/dialogs.py` (331 lines). Each function composes existing widgets (Dialog, Label, Button, TextArea, RadioList, CheckboxList, ProgressBar, ValidationToolbar) into an `Application<T>` ready to run. The api-mapping.md defines an additional `Async` wrapper layer (`YesNoDialogAsync`, etc.) that creates the application and runs it. Both layers are implemented: the factory functions (returning `Application<T>`) and the async convenience methods (returning `Task<T>`).

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Application, Stroke.Widgets (Dialog, Label, Button, TextArea, RadioList, CheckboxList, ProgressBar, ValidationToolbar, Box), Stroke.Layout (HSplit, Layout), Stroke.KeyBinding (KeyBindings, MergedKeyBindings, DefaultKeyBindings), Stroke.Filters (FilterOrBool), Stroke.Completion (ICompleter), Stroke.Validation (IValidator), Stroke.FormattedText (AnyFormattedText), Stroke.Styles (IStyle)
**Storage**: N/A (stateless factory functions)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Cross-platform (.NET 10 — Linux, macOS, Windows 10+)
**Project Type**: Single project (library)
**Performance Goals**: Immediate dialog construction; no hot paths beyond Application.RunAsync
**Constraints**: ≤1,000 LOC per file; thread-safe progress dialog updates
**Scale/Scope**: 1 source file (~250 LOC), 1 test file (~400 LOC), 7 public methods + 7 async wrappers + 2 private helpers

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Direct 1:1 port of `shortcuts/dialogs.py` — 7 functions + `_create_app` + `_return_none` |
| II. Immutability | ✅ PASS | Dialogs class is stateless (static methods only). No mutable state introduced. |
| III. Layered Architecture | ✅ PASS | `Stroke.Shortcuts` (layer 8) depends on `Stroke.Application` (layer 7), widgets (layer 5+), key bindings (layer 4) — all downward dependencies. |
| IV. Cross-Platform | ✅ PASS | No platform-specific code. Relies on existing cross-platform widget infrastructure. |
| V. Editing Mode Parity | ✅ N/A | Dialog shortcuts don't introduce editing modes. |
| VI. Performance | ✅ PASS | Stateless factory functions; no caching needed. Progress dialog uses existing Channel-based marshaling. |
| VII. Full Scope | ✅ PASS | All 7 dialog types implemented. No scope reduction. |
| VIII. Real-World Testing | ✅ PASS | Tests exercise real Application, Dialog, Button, etc. instances. No mocks. |
| IX. Planning Documents | ✅ PASS | api-mapping.md consulted — `Dialogs.MessageDialogAsync`, `Dialogs.YesNoDialogAsync`, etc. with Async suffix convention. |
| X. File Size | ✅ PASS | Estimated ~250 LOC source, ~400 LOC tests — both well under 1,000. |
| XI. Thread Safety | ✅ PASS | Dialogs class is stateless. Progress dialog uses `_actionChannel.Writer.TryWrite` for thread-safe UI updates. |

## Project Structure

### Documentation (this feature)

```text
specs/048-dialog-shortcuts/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── dialogs-api.md   # API contracts in markdown
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/Shortcuts/
├── Dialogs.cs           # NEW — 7 dialog factory methods + async wrappers + CreateApp + ReturnNone

tests/Stroke.Tests/Shortcuts/
├── DialogsTests.cs      # NEW — unit tests for all 7 dialog types + edge cases
```

**Structure Decision**: Single new file `Dialogs.cs` in the existing `Stroke.Shortcuts` namespace, alongside `Prompt.cs`, `FormattedTextOutput.cs`, `TerminalUtils.cs`. Tests go in the existing `tests/Stroke.Tests/Shortcuts/` directory. No new projects or directories needed beyond the file itself.

## Complexity Tracking

> No violations. All principles pass.

## Design Decisions

### D1: API Surface — Factory vs Async

**Python pattern**: Each function (e.g., `yes_no_dialog(...)`) returns `Application[bool]`. The caller then calls `app.run()` or `await app.run_async()`.

**C# mapping** (per api-mapping.md): Two layers:
1. **Factory method**: `YesNoDialog(...)` returns `Application<bool>` — matches Python directly
2. **Async convenience**: `YesNoDialogAsync(...)` returns `Task<bool>` — calls factory + `RunAsync()`

Both are implemented as static methods on the `Dialogs` class.

### D2: ReturnNone → Cancel Handler

Python's `_return_none()` calls `get_app().exit()` with no result (returns None). In C#:
- For `Application<object?>`: call `app.Exit()` — exits with `default(object?)` = `null`
- The existing `Application<T>.Exit()` method accepts `result: default` which works for reference types (null) and value types (default)
- We use `AppContext.GetApp().Exit()` matching Python's `get_app().exit()`

### D3: Progress Dialog Background Execution

Python uses `run_in_executor_with_context(start)` in a `pre_run` callable. In C#:
- Use `Application.CreateBackgroundTask()` which manages `CancellationToken` and task tracking
- The `logText` callback marshals via `_actionChannel.Writer.TryWrite()` (equivalent to `loop.call_soon_threadsafe`)
- The `setPercentage` callback sets the property directly (thread-safe via Lock in ProgressBar) + calls `app.Invalidate()` (thread-safe)
- A `finally` block ensures `app.Exit()` is called even on exception

### D4: Generic Type Parameter for ButtonDialog, RadioListDialog, CheckboxListDialog

Python uses `_T = TypeVar("_T")` for generic dialog functions. In C#:
- `ButtonDialog<T>`, `RadioListDialog<T>`, `CheckboxListDialog<T>` use generic type parameter `T`
- `CheckboxListDialog<T>` returns `Application<IReadOnlyList<T>?>` (nullable for cancel)
- `RadioListDialog<T>` returns `Application<T?>` (nullable for cancel, per api-mapping: `Task<T>`)

### D5: Key Binding Assembly in CreateApp

Python's `_create_app` creates tab/shift-tab bindings and merges with `load_key_bindings()`. In C#:
- Create `KeyBindings` with tab/s-tab handlers calling `FocusFunctions.FocusNext`/`FocusPrevious`
- Note: Tab/Shift-Tab are already in Dialog's own key bindings (with `~HasCompletions` filter). The `_create_app` adds ADDITIONAL tab/s-tab without the filter. This matches Python where both exist.
- Merge via `new MergedKeyBindings(DefaultKeyBindings.Load(), bindings)`
- Pass to `Application` constructor's `keyBindings` parameter

### D6: Input Dialog's Accept Handler and Focus Transfer

Python's `input_dialog` defines `accept(buf: Buffer) -> bool` which focuses the OK button and returns `True` (keep text). In C#:
- `TextArea.AcceptHandler` takes `Func<Buffer, bool>`
- The handler calls `AppContext.GetApp().Layout.Focus(okButton.Window)` to move focus to OK button
- Returns `true` to keep the text in the buffer

## Post-Design Constitution Re-Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Contracts match Python source exactly. All 9 symbols ported. Parameter names/defaults verified against Python source line-by-line. |
| II. Immutability | ✅ PASS | No new mutable state. `Dialogs` is a pure static class. |
| III. Layered Architecture | ✅ PASS | All dependencies are downward: Shortcuts → Application → Widgets → Layout → KeyBinding → Core. |
| IV. Cross-Platform | ✅ PASS | No OS-specific code. |
| V. Editing Mode Parity | ✅ N/A | No editing modes involved. |
| VI. Performance | ✅ PASS | Factory methods are O(1) widget composition. No unnecessary allocations. |
| VII. Full Scope | ✅ PASS | 7/7 dialog types + 7 async wrappers + 2 helpers = full coverage. |
| VIII. Real-World Testing | ✅ PASS | Tests use real widgets and Application instances. Zero mocks. |
| IX. Planning Documents | ✅ PASS | api-mapping.md signatures matched in contracts. |
| X. File Size | ✅ PASS | Dialogs.cs estimated ~250 LOC, DialogsTests.cs estimated ~400 LOC. |
| XI. Thread Safety | ✅ PASS | ProgressDialog logText uses `_actionChannel` (thread-safe Channel). setPercentage uses Lock-protected ProgressBar. |

All gates pass. Ready for `/speckit.tasks`.
