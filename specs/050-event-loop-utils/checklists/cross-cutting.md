# Cross-Cutting Requirements Quality Checklist: Event Loop Utilities

**Purpose**: Full cross-artifact consistency and requirements quality validation (concurrency semantics, API contracts, edge cases)
**Created**: 2026-02-02
**Feature**: [spec.md](../spec.md)
**Depth**: Standard | **Audience**: Reviewer | **Scope**: All artifacts (spec, plan, research, contracts, data-model, quickstart)

## Requirement Completeness

- [x] CHK001 - Are null-argument validation requirements specified for all public methods? Contracts define `ArgumentNullException` behavior, but spec has no corresponding functional requirement. [Gap, Contracts §signatures] — **Resolved**: Added FR-012 requiring `ArgumentNullException` for null `func`, `action`, `context` parameters.
- [x] CHK002 - Is exception propagation from `CallSoonThreadSafe` callbacks defined? Spec FR-004 through FR-008 specify scheduling behavior but not what happens if the scheduled callback itself throws. [Gap, Spec §FR-004] — **Resolved**: Added FR-013 specifying exceptions propagate through the sync context's normal exception handling.
- [x] CHK003 - Are performance requirements documented? Plan §Technical Context sets "<1μs" targets for context capture and callback posting, but spec has no non-functional performance requirements. [Gap, Plan §Performance Goals] — **Resolved**: Added SC-010 with <1μs targets for context capture/restore and callback posting.
- [x] CHK004 - Is the "ambient context" concept in FR-001 defined precisely enough to be testable — specifically, does it encompass only `AsyncLocal<T>` values or broader `ExecutionContext` state? [Clarity, Spec §FR-001] — **Resolved**: FR-001 now explicitly says "`ExecutionContext` — specifically, `AsyncLocal<T>` values such as `AppContext.GetApp()`."
- [x] CHK005 - Are requirements for the void overload (`RunInExecutorWithContextAsync(Action)`) explicitly stated? FR-002 says "both value-returning and void" but no separate FR exists. Plan §API Mapping notes the void overload is "not explicitly in the API mapping." [Gap, Spec §FR-002, Plan §API Mapping Compliance] — **Resolved**: FR-002 now explicitly states "The void overload MUST be provided as a separate method signature accepting an `Action`."

## Requirement Clarity

- [x] CHK006 - Is "one synchronization context pump cycle" in SC-004 quantified with a measurable time bound or verifiable condition? [Measurability, Spec §SC-004] — **Resolved**: SC-004 reworded to "posted via a single `Post` call and execute when the context processes its queue" — testable by verifying the Post call is made.
- [x] CHK007 - Is "idle (no pending work)" in FR-006 defined with testable criteria? Research R6 concludes idle detection is not possible for arbitrary `SynchronizationContext` implementations, yet the spec requires it. [Ambiguity, Spec §FR-006, Research §R6] — **Resolved**: FR-006 rewritten to describe the re-post pattern that provides *natural* idle coalescing without explicit idle detection, referencing Research R6.
- [x] CHK008 - Is the dictionary key name for `GetTracebackFromContext` specified in the spec? The contracts define "exception" as the lookup key, but the spec only says "exception context dictionary." [Clarity, Spec §FR-009, Contracts §GetTracebackFromContext] — **Resolved**: FR-009 now explicitly states `look up the key "exception"`.
- [x] CHK009 - Is the return type for background execution specified precisely? Spec says "awaitable value" while contracts specify `Task<T>`. Is the spec intentionally abstract or missing precision? [Clarity, Spec §FR-002, Contracts §RunInExecutorWithContextAsync] — **Resolved**: FR-002 now explicitly says "`Task<T>` for value-returning functions and a `Task` for void operations."
- [x] CHK010 - Is the parameter type for `GetTracebackFromContext` specified in the spec? Research R7 chooses `IDictionary<string, object?>` over non-generic `IDictionary`, but the spec doesn't specify either. [Gap, Spec §FR-009, Research §R7] — **Resolved**: FR-009 now specifies `IDictionary<string, object?>`.

## Cross-Artifact Consistency

- [x] CHK011 - Is the method name `CallSoonThreadSafe` consistent across all artifacts? Spec edge cases (lines 62, 65) use `CallSoonThreadsafe` (lowercase 's'), while plan, contracts, and research use `CallSoonThreadSafe` (capital 'S'). [Conflict, Spec §Edge Cases vs. Research §R4] — **Resolved**: Spec edge cases corrected to `CallSoonThreadSafe` (capital S) throughout.
- [x] CHK012 - Does Spec Assumption 3 ("may require a custom synchronization context") align with Research R6's decision to NOT use a custom sync context? The assumption appears invalidated but not updated. [Conflict, Spec §Assumptions, Research §R6] — **Resolved**: Assumption 3 rewritten to state "has no direct .NET equivalent" and describes the deadline-based re-posting solution, referencing Research R6.
- [x] CHK013 - Does FR-006 ("idle detection") align with the behavioral contract's deadline-only approach? Contracts §CallSoonThreadSafe step 3c says "If deadline expired or no pending work, executes action" but Research R6 says idle detection isn't possible. [Conflict, Spec §FR-006, Contracts §Behavioral Contract, Research §R6] — **Resolved**: FR-006 rewritten to describe re-post coalescing pattern. Contracts §CallSoonThreadSafe step 3c-3d updated to remove "no pending work" check and describe the re-post mechanism.
- [x] CHK014 - Does the data model's claim that ExecutionContext is "consumed once at execution time" accurately describe `ExecutionContext.Run()` behavior? .NET's `ExecutionContext.Run` does not consume/invalidate the captured context. [Accuracy, Data-Model §ExecutionContext] — **Resolved**: Data model lifecycle updated to clarify that `Run()` does not consume/invalidate the context.
- [x] CHK015 - Are the return types consistent between spec and contracts? Spec says "awaitable value" and "stack trace," while contracts specify `Task<T>` and `string?` per API mapping. Are both intentionally at different abstraction levels? [Consistency, Spec §FR-002/FR-009, Research §R2/R3] — **Resolved**: FR-002 now says `Task<T>`/`Task` explicitly; FR-009 now says "stack trace string"; SC-002 updated to reference `Task<T>` and `Task`.

## Concurrency Semantics

- [x] CHK016 - Are reentrancy requirements specified for `CallSoonThreadSafe`? What happens if a callback scheduled via `CallSoonThreadSafe` itself calls `CallSoonThreadSafe`? [Gap, Spec §FR-004] — **Resolved**: Added FR-014 requiring reentrancy support without deadlock or undefined behavior.
- [x] CHK017 - Are ordering guarantees for `CallSoonThreadSafe` defined? If multiple callbacks are posted without deadlines, must they execute in FIFO order? Python's `call_soon_threadsafe` guarantees FIFO via the event loop. [Gap, Spec §FR-004] — **Resolved**: FR-004 now states ordering follows `SynchronizationContext.Post` semantics (typically FIFO, but implementation-dependent).
- [x] CHK018 - Is the thread on which the `CancellationToken` is observed specified? FR-003 says "standard cancellation mechanism" — is cancellation checked before dispatch, during execution, or both? Contracts §Behavioral Contract step 6 says "observed via CancellationToken" without specifying when. [Clarity, Spec §FR-003, Contracts §RunInExecutorWithContextAsync] — **Resolved**: FR-003 now specifies "both before dispatch to the thread pool (early exit) and during execution (cooperative cancellation via the token passed to `Task.Run`)."
- [x] CHK019 - Is thread safety in FR-010 defined precisely enough? "Thread-safe and safe to call from any thread" — does this mean concurrent calls to the same method are safe, or that calls from non-default threads are safe? [Clarity, Spec §FR-010] — **Resolved**: FR-010 now says "concurrent calls to any method from multiple threads MUST be safe without external synchronization."

## Edge Case & Failure Coverage

- [x] CHK020 - Is the fallback behavior for disposed/stopped `SynchronizationContext` specified as a functional requirement? The spec edge case (line 62) describes it, but no FR covers this scenario. [Gap, Spec §Edge Cases] — **Resolved**: Added FR-015 requiring fallback to immediate invocation on the calling thread.
- [x] CHK021 - Is the behavior when `func` throws in `RunInExecutorWithContextAsync` covered by a functional requirement? The spec edge case (line 61) defines it, but no FR explicitly requires exception propagation through the returned task. [Gap, Spec §Edge Cases vs. Spec §FR-001/FR-002] — **Resolved**: Added FR-016 requiring exception propagation through the returned `Task`.
- [x] CHK022 - Are requirements defined for `GetTracebackFromContext` when the exception has a null `StackTrace` property (exception created but never thrown)? Spec says "trace information is present" — is a null stack trace on a real Exception considered "no trace information"? [Edge Case, Spec §FR-009] — **Resolved**: FR-009 now explicitly says "non-null `StackTrace` property." Edge case added for null StackTrace scenario.
- [x] CHK023 - Is the behavior when `maxPostponeTime` is `TimeSpan.MaxValue` or extremely large values addressed? Only zero/negative are covered in edge cases. [Gap, Spec §Edge Cases] — **Resolved**: Edge case added for `TimeSpan.MaxValue` / overflow behavior.

## Acceptance Criteria Quality

- [x] CHK024 - Can SC-005 ("execute no later than the deadline, even under event loop contention") be reliably tested given timing-dependent behavior? Are tolerance bounds defined? [Measurability, Spec §SC-005] — **Resolved**: SC-005 now specifies "plus a 50ms tolerance to account for thread scheduling jitter under contention."
- [x] CHK025 - Can SC-006 ("execute immediately" when idle) be reliably tested? What defines "immediately" — within 1ms, within one pump cycle, within the same call? [Measurability, Spec §SC-006] — **Resolved**: SC-006 now says "executes on the first re-post cycle (within one message-pump iteration)" — testable via a custom sync context in tests.
- [x] CHK026 - Is SC-008 ("function correctly when called concurrently") specific enough to define a test? Does "concurrently" mean 2 threads, 100 threads, or stress testing? [Measurability, Spec §SC-008] — **Resolved**: SC-008 now specifies "at least 10 threads in stress tests with no deadlocks, data corruption, or unhandled exceptions."
- [x] CHK027 - Does SC-009 (80% coverage) apply to the implementation file only, or also to the behavioral contract branches? [Clarity, Spec §SC-009] — **Resolved**: SC-009 now explicitly scopes to "all lines in the `EventLoopUtils.cs` implementation file."

## Dependencies & Assumptions

- [x] CHK028 - Is the relationship between this feature and the existing `InputHook`/`InputHookContext` in `Stroke.Application` documented in the spec? Plan and research reference them but the spec does not. [Gap, Spec §Assumptions, Research §R1] — **Resolved**: Added assumption noting InputHook/InputHookContext were ported with Application and are a separate concern.
- [x] CHK029 - Are the downstream consumer dependencies (Application.Invalidate, ThreadedAutoSuggest, AsyncGeneratorUtils) documented in the spec so reviewers understand why these utilities exist? Data model lists them but spec does not. [Gap, Spec §Assumptions, Data-Model §Type Dependencies] — **Resolved**: Added "Downstream Consumers" section to spec listing Application.Invalidate, ThreadedAutoSuggest, and AsyncGeneratorUtils with usage context.

## Notes

- **Focus**: Cross-cutting (concurrency semantics, API contracts, edge cases, cross-artifact consistency)
- **Depth**: Standard (29 items)
- **All 29 items resolved.** Key changes: FR-006 reconciled with Research R6, 5 new FRs added (FR-012 through FR-016), SC-010 added for performance, naming inconsistency fixed, data model corrected, contracts behavioral contract updated.
- Items reference: Spec §, Contracts §, Research §, Plan §, Data-Model § for traceability.
