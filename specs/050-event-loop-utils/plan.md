# Implementation Plan: Event Loop Utilities

**Branch**: `050-event-loop-utils` | **Date**: 2026-02-02 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/050-event-loop-utils/spec.md`

## Summary

Port the three utility functions from Python Prompt Toolkit's `prompt_toolkit/eventloop/utils.py` to C#: context-preserving background execution (`RunInExecutorWithContextAsync`), thread-safe callback scheduling with deadline coalescing (`CallSoonThreadSafe`), and exception traceback extraction (`GetTracebackFromContext`). Uses `ExecutionContext.Capture/Run` for context preservation and `SynchronizationContext.Post` for thread-safe scheduling.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: .NET BCL only — `System.Threading.ExecutionContext`, `System.Threading.SynchronizationContext`, `System.Threading.Tasks.Task`, `System.Diagnostics.StackTrace`
**Storage**: N/A (stateless utilities, in-memory only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform)
**Project Type**: Single project — adds files to existing `Stroke` project and `Stroke.Tests`
**Performance Goals**: Context capture/restore overhead < 1μs per call; callback scheduling < 1μs per post
**Constraints**: Thread-safe (Constitution XI); no global mutable state (Constitution VI)
**Scale/Scope**: 1 static class, 4 methods, ~120 LOC implementation + ~300 LOC tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | All 3 functions from `utils.py` ported. `loop` parameter removed (no asyncio equivalent), `*args` removed (use closures). `CancellationToken` added as platform adaptation. Documented in research.md R2, R4, R10. |
| II. Immutability | ✅ PASS | Static utility class with no mutable state. All parameters are values or immutable references. |
| III. Layered Architecture | ✅ PASS | `Stroke.EventLoop` has no dependencies on higher layers. Depends only on BCL types. Consumers (Application, AutoSuggest) are higher layers that depend downward. |
| IV. Cross-Platform | ✅ PASS | Uses only cross-platform BCL types. No platform-specific code. |
| V. Editing Mode Parity | ✅ N/A | Not editing-mode related. |
| VI. Performance-Conscious | ✅ PASS | `CallSoonThreadSafe` with deadline enables render coalescing (the primary performance use case). No global mutable state. |
| VII. Full Scope | ✅ PASS | All 3 utility functions implemented. Void overload added for `RunInExecutorWithContextAsync`. |
| VIII. Real-World Testing | ✅ PASS | Tests use real `ExecutionContext`, real `SynchronizationContext`, real thread pool. No mocks. |
| IX. Planning Documents | ✅ PASS | API mapping consulted — return types and naming adjusted per `docs/api-mapping.md` lines 764-766. See research.md R2-R4. |
| X. File Size | ✅ PASS | Implementation ~120 LOC, tests ~300 LOC. Well under 1,000 LOC limit. |
| XI. Thread Safety | ✅ PASS | All methods are stateless and thread-safe. `CallSoonThreadSafe` uses thread-safe `SynchronizationContext.Post`. |

**Post-Phase 1 Re-check**: All gates still pass. No complexity violations.

## Project Structure

### Documentation (this feature)

```text
specs/050-event-loop-utils/
├── plan.md                          # This file
├── spec.md                          # Feature specification
├── research.md                      # Phase 0 research decisions
├── data-model.md                    # Phase 1 data model (stateless)
├── quickstart.md                    # Phase 1 build sequence
├── contracts/
│   └── event-loop-utils.md          # Phase 1 API contract
├── checklists/
│   └── requirements.md              # Spec quality checklist
└── tasks.md                         # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/
└── EventLoop/
    └── EventLoopUtils.cs            # Static class with 4 methods (NEW)

tests/Stroke.Tests/
└── EventLoop/
    └── EventLoopUtilsTests.cs       # xUnit tests (NEW)
```

**Structure Decision**: Adds a new `EventLoop` directory under `src/Stroke/` matching the `Stroke.EventLoop` namespace from the API mapping. Tests go in a matching `EventLoop` directory under the existing `Stroke.Tests` project. No new projects needed — these files are added to the existing `Stroke` and `Stroke.Tests` projects.

## Complexity Tracking

> No violations — all Constitution gates pass without exception.

## API Mapping Compliance

Per `docs/api-mapping.md` lines 764-768, the following functions are mapped:

| Python | C# | Status |
|--------|-----|--------|
| `run_in_executor_with_context(func)` | `EventLoopUtils.RunInExecutorWithContextAsync<T>(func)` | In scope |
| `call_soon_threadsafe(func)` | `EventLoopUtils.CallSoonThreadSafe(action)` | In scope |
| `get_traceback_from_context(context)` | `EventLoopUtils.GetTracebackFromContext(context)` | In scope |
| `new_eventloop_with_inputhook(hook)` | `EventLoopUtils.NewEventLoopWithInputHook(hook)` | Out of scope (separate feature) |
| `set_eventloop_with_inputhook(hook)` | `EventLoopUtils.SetEventLoopWithInputHook(hook)` | Out of scope (separate feature) |

**Note**: The void overload `RunInExecutorWithContextAsync(Action)` is not explicitly in the API mapping but is required for C# API completeness since the Python function accepts any callable (both value-returning and void).

## Key Design Decisions

### 1. ExecutionContext Capture Pattern

```
Caller thread:  ExecutionContext.Capture() → ec
Thread pool:    ExecutionContext.Run(ec, func) → result
```

This mirrors Python's `contextvars.copy_context().run(func)`. The captured `ExecutionContext` carries `AsyncLocal<AppSession>` which is how `AppContext.GetApp()` works across threads.

### 2. CallSoonThreadSafe Deadline Algorithm

```
1. Compute deadline = Environment.TickCount64 + maxPostponeTime.TotalMilliseconds
2. Post Schedule() to SynchronizationContext
3. Schedule():
   a. If deadline expired → execute action
   b. Else re-post Schedule()
```

Simplification vs Python: Python checks `loop._ready` for idle detection. .NET has no equivalent for arbitrary `SynchronizationContext` implementations. We rely solely on deadline expiry — if the sync context is pumping messages, our re-posted callback will execute between other work items naturally. If idle, it executes immediately on the next pump.

### 3. Return Type Alignment

Per research.md R2-R3:
- `RunInExecutorWithContextAsync<T>` returns `Task<T>` (not `ValueTask<T>`) per API mapping
- `GetTracebackFromContext` returns `string?` (not `StackTrace?`) per API mapping
