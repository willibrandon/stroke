# Research: Prompt Session

**Feature**: 047-prompt-session
**Date**: 2026-02-01

## Research Tasks

### R1: Application.RefreshInterval Mutability

**Question**: Python sets `self.app.refresh_interval = self.refresh_interval` per-prompt call. The current Stroke `Application.RefreshInterval` is read-only (`{ get; }`). How should we handle this?

**Decision**: Make `RefreshInterval` settable on `Application<TResult>` with Lock protection.

**Rationale**: The Python source explicitly updates `refresh_interval` on the application object each time `prompt()` or `prompt_async()` is called (line 1047). This enables per-prompt refresh behavior (e.g., a bottom toolbar that updates every 0.5 seconds for one prompt but not another). Making the property read-only would break this functionality.

**Implementation**: Change `public double? RefreshInterval { get; }` to a Lock-protected mutable property matching the pattern used for `EditingMode`, `Style`, etc. The auto-invalidation loop in `RunAsync` already reads this property reactively, so the change is safe.

**Alternatives Considered**:
- *Recreate Application per prompt*: Rejected — Python reuses the Application and only updates RefreshInterval. Recreating would lose state and violate API fidelity.
- *Pass via constructor only*: Rejected — breaks per-prompt override semantics in FR-010.

---

### R2: Custom Exception Types for Interrupt and EOF

**Question**: Python allows configurable `interrupt_exception` (default `KeyboardInterrupt`) and `eof_exception` (default `EOFError`) types. Stroke has no equivalent custom exception types.

**Decision**: Create `KeyboardInterruptException` and `EOFException` in `Stroke.Shortcuts` namespace as default exception types.

**Rationale**: The Python API accepts exception *types* as constructor parameters (`interrupt_exception: type[BaseException] = KeyboardInterrupt`). Users can provide custom exception types. We need default types that users can catch. Using `.NET`'s `OperationCanceledException` and `EndOfStreamException` as *defaults* would work functionally, but would deviate from the Python naming and make it harder for users porting code from Python Prompt Toolkit. The configurable type pattern itself maps well to C# using `Func<Exception>` factories or generic `Type` parameters.

**Implementation**:
- Define `KeyboardInterruptException : Exception` in `Stroke.Shortcuts`
- Define `EOFException : Exception` in `Stroke.Shortcuts`
- PromptSession constructor accepts `Type interruptException = typeof(KeyboardInterruptException)` and `Type eofException = typeof(EOFException)` — instantiated via `Activator.CreateInstance` when triggered
- Users who want `OperationCanceledException` can pass `typeof(OperationCanceledException)`

**Alternatives Considered**:
- *Use .NET built-in exceptions only*: Rejected — loses 1:1 API fidelity (Constitution I) and makes Python→C# porting harder.
- *Generic type parameters `PromptSession<TResult, TInterrupt, TEof>`*: Rejected — over-engineered, Python uses runtime types not generics for this.

---

### R3: PromptContinuationText Type Mapping

**Question**: Python defines `PromptContinuationText` as a union type: `str | MagicFormattedText | StyleAndTextTuples | Callable[[int, int, int], AnyFormattedText]`. How to represent in C#?

**Decision**: Use `object?` with runtime type checking, plus a convenience delegate type.

**Rationale**: C# lacks union types. The Python type accepts 4 distinct forms. We define a delegate `PromptContinuationCallable` for the callable form `Func<int, int, int, AnyFormattedText>` and accept `object?` for the property type. At render time, the `_GetContinuation` method checks: (1) is it a `PromptContinuationCallable`? Call it with width, lineNumber, wrapCount. (2) Otherwise, treat it as `AnyFormattedText` (which already handles string, FormattedText, etc.).

**Implementation**:
```
public delegate AnyFormattedText PromptContinuationCallable(int promptWidth, int lineNumber, int wrapCount);
```
The `PromptContinuationText` property type is `object?` — accepting `string`, `AnyFormattedText`, `PromptContinuationCallable`, or `null`.

**Alternatives Considered**:
- *Separate overloads*: Rejected — property-based API doesn't support overloads.
- *Wrapper record with implicit conversions*: Possible but over-engineered for a single property.

---

### R4: Python `_fields` Tuple → C# Per-Prompt Override Pattern

**Question**: Python uses `_fields` tuple + `setattr` to iterate and update session attributes dynamically. C# has no equivalent dynamic attribute access. How to port the per-prompt override logic?

**Decision**: Explicit property-by-property `if (param != null) this.Property = param` in `Prompt()` and `PromptAsync()` methods, matching the Python source's own explicit implementation.

**Rationale**: The Python source itself uses explicit `if message is not None: self.message = message` blocks (lines 966-1041), not `_fields` iteration, for the `prompt()` and `prompt_async()` methods. The `_fields` tuple is only used internally by the Python source (and ironically contains a duplicate `"is_password"` entry). The C# port should follow the explicit pattern that Python actually uses in its prompt methods.

**Implementation**: Both `Prompt()` and `PromptAsync()` contain ~36 `if (param is not null)` blocks, one per overridable property. This is verbose but type-safe, IDE-friendly, and matches the Python source exactly.

**Alternatives Considered**:
- *Reflection-based property iteration*: Rejected — fragile, no compile-time safety, poor IDE support.
- *Dictionary-based parameter passing*: Rejected — loses type safety entirely.

---

### R5: ExplodeTextFragments Usage in _SplitMultilinePrompt

**Question**: The `_split_multiline_prompt` helper uses `explode_text_fragments` from `layout.utils`. Does Stroke have this?

**Decision**: Use existing `LayoutUtils.ExplodeTextFragments()`.

**Rationale**: Already implemented at `Stroke.Layout.LayoutUtils.ExplodeTextFragments()` which returns an `ExplodedList`. The `_SplitMultilinePrompt` helper will use this directly to split prompt text at newline characters.

---

### R6: `to_str` Utility Usage

**Question**: Python uses `to_str(self.tempfile_suffix or "")` to convert callable-or-string to string. Does Stroke have this?

**Decision**: Inline the logic — use a simple ternary/pattern match.

**Rationale**: `to_str` in Python handles `str | Callable[[], str]`. In C#, this is `string | Func<string>`. We can inline: `tempfileSuffix is Func<string> f ? f() : (tempfileSuffix as string ?? "")`. No need for a separate utility for this one usage point.

---

### R7: Dumb Terminal Detection Integration

**Question**: How does dumb terminal detection work in the prompt flow?

**Decision**: Use existing `PlatformUtils.IsDumbTerminal()`.

**Rationale**: Already implemented at `Stroke.Core.PlatformUtils.IsDumbTerminal(string? term = null)`. The prompt method checks: if `_output` is null (no explicit output provided) AND `IsDumbTerminal()` returns true, then use `_DumbPrompt` instead of the full Application run loop.

---

### R8: Application.Run vs Application.RunAsync in Prompt Methods

**Question**: Python's `prompt()` calls `self.app.run()` (blocking) and `prompt_async()` calls `await self.app.run_async()`. How do these map to Stroke?

**Decision**: Direct mapping — `Prompt()` calls `App.Run()`, `PromptAsync()` calls `await App.RunAsync()`.

**Rationale**: Stroke's `Application<TResult>.Run()` returns `TResult` (blocking) and `Application<TResult>.RunAsync()` returns `Task<TResult>` (async). Parameters map directly:
- `set_exception_handler` → `setExceptionHandler`
- `handle_sigint` → `handleSigint`
- `in_thread` → `inThread` (Run only)
- `inputhook` → `inputHook` (Run only)

## Resolved — No Outstanding NEEDS CLARIFICATION
