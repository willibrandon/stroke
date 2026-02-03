# Data Model: Event Loop Utilities

**Feature**: 050-event-loop-utils
**Date**: 2026-02-02

## Overview

This feature is a stateless utility module. There are no persistent entities, no state transitions, and no data storage. All functions are static and pure (or side-effecting only through their scheduling behavior).

## Key Concepts

### ExecutionContext (BCL Type — Not Created)

The .NET `System.Threading.ExecutionContext` captures the ambient state of `AsyncLocal<T>` values at the point of capture. It is used by `RunInExecutorWithContextAsync` to preserve context across thread boundaries.

- **Lifecycle**: Captured once at call time via `ExecutionContext.Capture()`, then passed to `ExecutionContext.Run()` on the thread pool thread. The captured context is not consumed or invalidated by `Run()` — it remains valid but is used only once per dispatch in this feature.
- **Relationship**: Carries `AsyncLocal<AppSession>` from `Stroke.Application.AppContext`, which is the primary consumer of context preservation.

### SynchronizationContext (BCL Type — Not Created)

The .NET `System.Threading.SynchronizationContext` is the target for `CallSoonThreadSafe` callback scheduling. It represents an event loop or message pump.

- **Lifecycle**: Read from `SynchronizationContext.Current` at call time. Not owned or managed by EventLoop utilities.
- **Relationship**: Used by `CallSoonThreadSafe` to post callbacks. If null, callbacks execute synchronously.

### Deadline (Computed Value — Not Stored)

A wall-clock time computed as `Environment.TickCount64 + maxPostponeTime.TotalMilliseconds`. Used internally by `CallSoonThreadSafe` to determine when a deferred callback must execute.

- **Lifecycle**: Computed at call time, compared at each reschedule iteration, discarded when callback executes.
- **No persistence**: Exists only as a local variable within the scheduling closure.

## Type Dependencies

```
Stroke.EventLoop.EventLoopUtils
  ├── uses System.Threading.ExecutionContext (BCL)
  ├── uses System.Threading.SynchronizationContext (BCL)
  ├── uses System.Threading.Tasks.Task (BCL)
  ├── uses System.Threading.CancellationToken (BCL)
  └── uses System.Diagnostics.StackTrace (BCL)

Consumers:
  ├── Stroke.EventLoop.AsyncGeneratorUtils (future feature)
  ├── Stroke.Application.Application<T>.Invalidate()
  └── Stroke.AutoSuggest.ThreadedAutoSuggest
```
