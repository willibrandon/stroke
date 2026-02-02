# Research: Dialog Shortcut Functions

**Feature**: 048-dialog-shortcuts
**Date**: 2026-02-02

## Research Summary

No NEEDS CLARIFICATION items were identified in the Technical Context. All design decisions are resolved by the Python source code (authoritative reference) and existing Stroke infrastructure. This document captures the research findings that inform implementation.

---

## R1: Python Source Fidelity Analysis

**Task**: Verify 1:1 mapping between Python `shortcuts/dialogs.py` and planned C# implementation.

**Decision**: All 9 Python-side symbols map directly to C# equivalents.

| Python Symbol | C# Equivalent | Notes |
|--------------|---------------|-------|
| `yes_no_dialog()` | `Dialogs.YesNoDialog()` | Returns `Application<bool>` |
| `button_dialog()` | `Dialogs.ButtonDialog<T>()` | Generic type for button values |
| `input_dialog()` | `Dialogs.InputDialog()` | Returns `Application<string?>` |
| `message_dialog()` | `Dialogs.MessageDialog()` | Returns `Application<object?>` |
| `radiolist_dialog()` | `Dialogs.RadioListDialog<T>()` | Generic for list values |
| `checkboxlist_dialog()` | `Dialogs.CheckboxListDialog<T>()` | Returns `Application<IReadOnlyList<T>?>` |
| `progress_dialog()` | `Dialogs.ProgressDialog()` | Returns `Application<object?>` |
| `_create_app()` | `Dialogs.CreateApp()` (private) | Shared Application factory |
| `_return_none()` | `Dialogs.ReturnNone()` (private) | Cancel handler |

**Alternatives considered**: None — faithful port requires exact mapping.

---

## R2: Application Exit Patterns

**Task**: Determine how each dialog communicates its result back.

**Decision**: Use `AppContext.GetApp().Exit(result: value)` for typed results and `AppContext.GetApp().Exit()` for cancel/void.

**Rationale**: This matches Python's `get_app().exit(result=value)` pattern. The `AppContext.GetApp()` returns `Application<object?>` — since Exit accepts `TResult? result = default`, calling it with the correct value type works through the generic covariance mechanism.

**Key detail**: Python's `get_app()` returns the app untyped. In C#, `AppContext.GetApp()` returns `Application<object?>`. Calling `Exit(result: someValue)` on this works because the value gets boxed. The `TaskCompletionSource<TResult>` in the actual `Application<TResult>` receives the value via `Unsafe.As` covariance internally.

---

## R3: Progress Dialog Thread Marshaling

**Task**: Determine how background thread callbacks safely update UI.

**Decision**: Two different mechanisms for two different operations:

1. **`setPercentage(int)`**: Sets `ProgressBar.Percentage` (Lock-protected) + calls `app.Invalidate()` (thread-safe via `Interlocked.Exchange`). Direct call from background thread is safe.

2. **`logText(string)`**: Must marshal `Buffer.InsertText()` to the async context because Buffer operations are not thread-safe. Use `_actionChannel.Writer.TryWrite(() => textArea.Buffer.InsertText(text))` followed by `app.Invalidate()`.

**Rationale**: This matches Python's pattern where `set_percentage` writes directly (ProgressBar has no thread concerns in Python's async model) while `log_text` uses `loop.call_soon_threadsafe(text_area.buffer.insert_text, text)`.

**Alternative considered**: Marshaling both operations through `_actionChannel`. Rejected because `setPercentage` only touches Lock-protected state and `Invalidate()`, both already thread-safe. Extra marshaling would add unnecessary latency.

---

## R4: Background Task Lifecycle

**Task**: Determine how the progress callback runs in background and triggers exit.

**Decision**: Use `PreRunCallables` to schedule a `CreateBackgroundTask` that runs the user callback, with `finally { app.Exit(); }`.

**Implementation pattern**:
```
app.PreRunCallables.Add(() =>
{
    _ = app.CreateBackgroundTask(async ct =>
    {
        try
        {
            await Task.Run(() => runCallback(setPercentage, logText), ct);
        }
        finally
        {
            app.Exit();
        }
    });
});
```

**Rationale**: `CreateBackgroundTask` is only valid during `RunAsync` (checked via `_backgroundTasksCts`). `PreRunCallables` execute after `Reset()` but before the first render — exactly when `_backgroundTasksCts` is initialized and `_actionChannel` is active. This matches Python's `pre_run_callables.append(lambda: run_in_executor_with_context(start))`.

---

## R5: Existing Widget Constructor Compatibility

**Task**: Verify all widget constructors match the parameter patterns used by Python dialog functions.

**Findings**:

| Widget | Python Usage | C# Constructor Match |
|--------|-------------|---------------------|
| `Label(text=text, dont_extend_height=True)` | ✅ `new Label(text, dontExtendHeight: true)` — `dontExtendHeight` defaults to `true` already |
| `Button(text=text, handler=handler)` | ✅ `new Button(text, handler: handler)` |
| `TextArea(text=default, multiline=False, password=password, completer=completer, validator=validator, accept_handler=accept)` | ✅ All parameters exist in C# constructor |
| `ValidationToolbar()` | ✅ Parameterless constructor available |
| `RadioList(values=values, default=default)` | ✅ `new RadioList<T>(values, @default: default)` |
| `CheckboxList(values=values, default_values=defaults)` | ✅ `new CheckboxList<T>(values, defaultValues: defaults)` |
| `ProgressBar()` | ✅ Parameterless constructor available |
| `Box(Label(text=text))` | ✅ `new Box(new AnyContainer(label))` |
| `Box(text_area, padding=D.exact(1))` | ✅ `new Box(new AnyContainer(textArea), padding: Dimension.Exact(1))` |
| `HSplit([...], padding=1)` | ✅ `new HSplit([...], padding: 1)` |
| `HSplit([...], padding=D(preferred=1, max=1))` | ✅ `new HSplit([...], padding: new Dimension(preferred: 1, max: 1))` |
| `Dialog(title=..., body=..., buttons=[...], with_background=True)` | ✅ `new Dialog(body, title: ..., buttons: [...], withBackground: true)` |

All constructor signatures are compatible. No adapter code needed.

---

## R6: `_actionChannel` Access Pattern

**Task**: Determine how to access `_actionChannel` from outside `Application` for `logText` marshaling.

**Decision**: `_actionChannel` is `internal` in `Application<TResult>`. Since `Dialogs.cs` is in `Stroke.Shortcuts` (same assembly as `Stroke.Application`), internal access is available. However, accessing via a typed `Application<TResult>` reference requires the `app` local variable — which we have in `ProgressDialog()`.

**Pattern**: Capture `app` in the `logText` closure. Access `app._actionChannel?.Writer.TryWrite(...)` directly.

**Note**: The Python code accesses `app.loop` (the event loop) which may be `None` if the app isn't running yet. The C# equivalent checks `_actionChannel is not null` before writing — the channel is `null` until `RunAsync()` starts. This null-check matches Python's `if loop is not None:` guard.
