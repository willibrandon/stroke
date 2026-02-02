# Research: Shortcut Utilities

**Feature**: 046-shortcut-utils
**Date**: 2026-02-01

## Research Summary

All dependencies for this feature are **already implemented and verified**. No NEEDS CLARIFICATION items were identified in the Technical Context. This research document confirms the availability and exact signatures of each dependency.

---

## R-001: Renderer.PrintFormattedText Availability

**Decision**: Use `RendererUtils.PrintFormattedText` from `Stroke.Rendering`.

**Rationale**: The static method is already implemented at `src/Stroke/Rendering/RendererUtils.cs` with the exact signature needed:

```csharp
public static void PrintFormattedText(
    IOutput output,
    AnyFormattedText formattedText,
    IStyle? style = null,
    ColorDepth? colorDepth = null,
    IStyleTransformation? styleTransformation = null)
```

It converts `AnyFormattedText` → fragments, applies styles, writes to `IOutput`, resets attributes, and flushes. This maps directly to Python's `renderer_print_formatted_text`.

**Alternatives considered**: None — this is the canonical implementation.

---

## R-002: Application Context Access Pattern

**Decision**: Use `AppContext.GetAppOrNull()` to detect running applications and `AppContext.GetAppSession().Output` for default output.

**Rationale**: `AppContext` (at `src/Stroke/Application/AppContext.cs`) provides:
- `GetAppOrNull()` → returns `Application<object?>?` (maps to Python's `get_app_or_none()`)
- `GetAppSession()` → returns `AppSession` with lazy `.Output` (maps to Python's `get_app_session()`)

The `AsyncLocal<AppSession>` backing ensures correct context flow across async boundaries.

**Alternatives considered**: Direct thread-local access was rejected — `AsyncLocal` is the .NET-idiomatic approach and already implemented.

---

## R-003: Event Loop Dispatch for Running Application

**Decision**: Use `RunInTerminal.RunAsync(Action)` for dispatching print operations when an application is running.

**Rationale**: Python uses `loop.call_soon_threadsafe(lambda: run_in_terminal(render))`. In the C# port:
- There is no explicit event loop object — the Application manages its own rendering lifecycle
- `RunInTerminal.RunAsync(Action)` suspends the app UI, executes the action, then resumes — which achieves the same effect
- The `RunInTerminal.InTerminalContext` handles: erasing UI, detaching input, entering cooked mode, then restoring on dispose

**Key difference from Python**: Python schedules via `call_soon_threadsafe` which queues work on the event loop thread. The C# `RunInTerminal` operates synchronously within the caller's context but coordinates with the Application state. For thread safety when called from non-app threads, we use `Task.Run` to dispatch to the thread pool and await completion, similar to the Python approach.

**Implementation approach**: When `AppContext.GetAppOrNull()` returns a running app, call `RunInTerminal.RunAsync(render)` instead of calling `render()` directly. This matches the Python pattern where `run_in_terminal(render)` is called through the event loop.

**Alternatives considered**: Using `Application.PrintText()` directly was considered but rejected because `PrintText` doesn't support custom `sep`/`end`/`flush` parameters — it's a simpler API meant for application-internal use.

---

## R-004: Style Merging Infrastructure

**Decision**: Use `StyleMerger.MergeStyles(IEnumerable<IStyle?>)` with `DefaultStyles.DefaultUiStyle` and `DefaultStyles.DefaultPygmentsStyle`.

**Rationale**: Both are available and match the Python pattern exactly:
- `StyleMerger.MergeStyles` at `src/Stroke/Styles/StyleMerger.cs` — filters nulls, returns merged style with later-wins precedence
- `DefaultStyles.DefaultUiStyle` (Lazy, thread-safe) — 68 UI rules
- `DefaultStyles.DefaultPygmentsStyle` (Lazy, thread-safe) — 34 syntax highlighting rules

The `_create_merged_style` Python function maps to a private `CreateMergedStyle` method that builds `[DefaultUiStyle, DefaultPygmentsStyle?, userStyle?]` and calls `StyleMerger.MergeStyles`.

**Alternatives considered**: None — exact infrastructure match.

---

## R-005: Output Factory for TextWriter Redirection

**Decision**: Use `OutputFactory.Create(stdout: textWriter)` to create an `IOutput` from a `TextWriter`.

**Rationale**: `OutputFactory.Create` at `src/Stroke/Output/OutputFactory.cs` handles:
- `null` → uses `Console.Out`
- `TextWriter.Null` → returns `DummyOutput`
- Redirected → `PlainTextOutput`
- Interactive → `Vt100Output`

This maps directly to Python's `create_output(stdout=file)`.

**Alternatives considered**: None — exact infrastructure match.

---

## R-006: DummyInput for PrintContainer

**Decision**: Use `DummyInput` from `Stroke.Input` to create a non-interactive Application for container rendering.

**Rationale**: `DummyInput` at `src/Stroke/Input/DummyInput.cs` is stateless:
- `Closed` always returns `true`
- `ReadKeys()` returns empty list
- When Application tries to read input, the closed state triggers `EndOfStreamException`

This matches Python's `DummyInput()` usage in `print_container`. The Application is created with `input: new DummyInput()` and terminated by catching `EndOfStreamException`.

**Alternatives considered**: None — exact infrastructure match.

---

## R-007: Application.Run with inThread for PrintContainer

**Decision**: Use `Application<object?>.Run(inThread: true)` and catch `EndOfStreamException`.

**Rationale**: Python calls `app.run(in_thread=True)` which runs the app on a background thread and blocks the caller. The C# `Application<TResult>.Run(inThread: true)` at `Application.RunAsync.cs:313-340` does exactly this — creates a `Thread`, runs the app, and joins.

Python catches `EOFError`; the C# equivalent catches `EndOfStreamException` (the .NET standard exception for end-of-stream conditions).

**Alternatives considered**: Using `RunAsync` was considered but `Run(inThread: true)` is the faithful port of Python's `run(in_thread=True)`.

---

## R-008: FormattedText Value Conversion

**Decision**: Use `FormattedTextUtils.ToFormattedText(AnyFormattedText, autoConvert: true)` for converting individual values, with special handling for plain `IList` that is not `FormattedText`.

**Rationale**: Python's `to_text` helper does:
```python
if isinstance(val, list) and not isinstance(val, FormattedText):
    return to_formatted_text(f"{val}")
return to_formatted_text(val, auto_convert=True)
```

In C#, `FormattedText` does not extend `IList` (it's `IReadOnlyList<StyleAndTextTuple>`), so the check needs adaptation. We check if a value is `IList` (non-generic) but not `FormattedText`, then convert via `ToString()`. Otherwise, we convert via `AnyFormattedText` implicit conversion + `ToFormattedText`.

**Alternatives considered**: Relying solely on `AnyFormattedText` implicit conversions — rejected because it doesn't handle the "plain list → string" edge case from the Python source.

---

## R-009: `params` Array for Multiple Values (C# Adaptation)

**Decision**: Use `params object[] values` to match Python's `*values` parameter.

**Rationale**: Python uses `*values: Any` for variadic arguments. In C#, the equivalent is `params object[] values`. However, per api-mapping.md, the main overload is `Print(AnyFormattedText text, ...)` (single value). We need two overloads:

1. `Print(AnyFormattedText text, ...)` — single-value convenience (most common usage)
2. `Print(object[] values, ...)` — multi-value with sep/end (matches Python's `*values`)

The single-value overload wraps `text` in a one-element array and delegates to the multi-value implementation.

**Alternatives considered**: Single method with `params` — rejected because the api-mapping.md specifies `Print(AnyFormattedText text, ...)` as the primary signature, and `params object[]` would conflict with `AnyFormattedText` implicit conversions.
