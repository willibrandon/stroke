# Research: Event Loop Utilities

**Feature**: 050-event-loop-utils
**Date**: 2026-02-02

## R1: Scope — Feature Spec vs API Mapping

**Decision**: This feature implements the `utils.py` subset of `prompt_toolkit.eventloop` — the three utility functions `RunInExecutorWithContextAsync`, `CallSoonThreadSafe`, and `GetTracebackFromContext`. The broader `eventloop` module (async generators, input hook selector, win32 helpers) are separate features.

**Rationale**: The feature spec explicitly scopes to `eventloop/utils.py`. The API mapping shows additional items (`AsyncGeneratorUtils`, `InputHookSelector`, `NewEventLoopWithInputHook`, `SetEventLoopWithInputHook`) that are not in the spec's scope. `InputHook` and `InputHookContext` already exist in `Stroke.Application`.

**Alternatives considered**: Implementing the full `eventloop` module at once. Rejected — the spec is scoped to utils only, and the other components have different dependencies (async generators depend on `RunInExecutorWithContextAsync`; input hook selector has platform-specific concerns).

## R2: Return Type for RunInExecutorWithContextAsync

**Decision**: Use `Task<T>` (not `ValueTask<T>`) per the API mapping.

**Rationale**: The API mapping (`docs/api-mapping.md` line 764) specifies `Task<T> RunInExecutorWithContextAsync<T>(Func<T> func)`. The spec proposed `ValueTask<T>` but the API mapping takes precedence per Constitution IX. Additionally, since this always dispatches to `Task.Run`, it will always allocate a `Task` anyway — `ValueTask<T>` would just wrap it with no benefit.

**Alternatives considered**: `ValueTask<T>` (from spec) — rejected because it contradicts the API mapping and provides no performance benefit since the underlying operation always produces a `Task`.

## R3: Return Type for GetTracebackFromContext

**Decision**: Return `string?` (not `StackTrace?`) per the API mapping.

**Rationale**: The API mapping (`docs/api-mapping.md` line 766) specifies `string? GetTracebackFromContext(IDictionary context)`. The Python version returns a `TracebackType` which is essentially a formatted stack trace string. Returning `string?` is more portable and matches how consumers use it (for logging/display).

**Alternatives considered**: `StackTrace?` (from spec) — rejected because it contradicts the API mapping. Consumers want the string representation for diagnostics.

**Python fallback omitted**: The Python implementation has a fallback path (`sys.exc_info()[2]`) for exceptions that lack a `__traceback__` attribute. In .NET, all `Exception` instances always have a `StackTrace` property (though it may be `null` if never thrown). The `sys.exc_info()` fallback has no .NET equivalent and is intentionally dropped. When `StackTrace` is `null`, the C# version returns `null`.

## R4: Method Naming — CallSoonThreadSafe

**Decision**: Use `CallSoonThreadSafe` (capital S in "Safe") per the API mapping.

**Rationale**: The API mapping uses `CallSoonThreadSafe` consistently. The spec used `CallSoonThreadsafe` (lowercase 's'). C# PascalCase convention treats "ThreadSafe" as two words, each capitalized.

## R5: Context Preservation Mechanism

**Decision**: Use `ExecutionContext.Capture()` + `ExecutionContext.Run()` to preserve `AsyncLocal<T>` values across thread pool dispatch.

**Rationale**: Python's `contextvars.copy_context().run(func)` copies context variables and runs a function within that copied context. The .NET equivalent is `ExecutionContext`, which automatically flows `AsyncLocal<T>` values. However, `Task.Run` already flows `ExecutionContext` by default in .NET. The explicit capture/run pattern is needed to match Python's semantics of capturing at call time (not at execution time) and to handle edge cases where `ExecutionContext.SuppressFlow()` may have been called.

**Key insight**: In standard .NET, `Task.Run` already flows `ExecutionContext`. The explicit capture is a defensive pattern for cases where flow is suppressed, and it makes the intent explicit — matching the Python pattern where `copy_context()` is an explicit operation.

## R6: Idle Detection for CallSoonThreadSafe

**Decision**: Use `SynchronizationContext.Post` for scheduling. For idle detection, check whether the posted callback runs immediately vs needs rescheduling using a timer-based approach with `Environment.TickCount64` for deadline enforcement.

**Rationale**: Python checks `loop._ready` (a private attribute) to detect if the event loop has pending callbacks. There is no .NET equivalent for arbitrary `SynchronizationContext` implementations. Instead, we use a simpler pattern: post the callback, and if the deadline hasn't expired, allow the callback to be deferred by re-posting. When deadline expires, execute immediately. This matches the behavioral contract without relying on implementation details.

**Alternatives considered**:
1. Custom `SynchronizationContext` with work queue inspection — rejected as over-engineering for a utility function.
2. `TaskScheduler.Current` inspection — rejected as it exposes different semantics than `SynchronizationContext`.
3. Always execute immediately when posted (ignoring idle detection) — rejected as it doesn't match the Python deadline coalescing behavior.

## R7: Parameter Types for GetTracebackFromContext

**Decision**: Use `IDictionary<string, object?>` as the parameter type.

**Rationale**: The API mapping specifies `IDictionary context` (non-generic). However, the Python source uses `dict[str, Any]` which maps to `IDictionary<string, object?>`. The generic version provides type safety while maintaining the same interface. This is a type-system adaptation per Constitution I.

## R8: Void Overload for RunInExecutorWithContextAsync

**Decision**: Provide a `Task RunInExecutorWithContextAsync(Action action)` overload in addition to the generic version.

**Rationale**: The Python function accepts any callable. C# distinguishes between `Func<T>` (returns value) and `Action` (void). The spec correctly identifies both overloads. The API mapping only shows the generic version, but the void overload is necessary for API completeness since many callsites pass void actions (e.g., the `runner` function in `generator_to_async_generator`).

## R9: Namespace Location

**Decision**: Place `EventLoopUtils` in `Stroke.EventLoop` namespace, in file `src/Stroke/EventLoop/EventLoopUtils.cs`.

**Rationale**: The API mapping maps `prompt_toolkit.eventloop` → `Stroke.EventLoop`. The existing `InputHook` and `InputHookContext` are currently in `Stroke.Application` (separate concern — they were ported as part of the Application feature). EventLoop utilities are a distinct concern and should live in their own namespace directory.

## R10: Cancellation Token Parameter

**Decision**: Add an optional `CancellationToken` parameter to `RunInExecutorWithContextAsync`.

**Rationale**: The Python version doesn't have cancellation (Python asyncio handles this differently via task cancellation). However, `CancellationToken` is the standard .NET cancellation pattern and is essential for responsive cancellation of background work. This is a type-system/platform adaptation per Constitution I.

## R11: win32.py Functions

**Decision**: Out of scope for this feature. `wait_for_handles` and `create_win32_event` are Windows-specific helpers used by `InputHookSelector` and will be ported alongside it.

**Rationale**: These functions are not part of `utils.py` and are not referenced by any of the three utility functions being ported.
