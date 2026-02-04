# Data Model: Async Generator Utilities

**Feature**: 059-async-generator-utils
**Date**: 2026-02-03

## Entity Overview

This feature introduces minimal data types focused on async generator management:

| Entity | Purpose | Mutability |
|--------|---------|------------|
| `IAsyncDisposableValue<T>` | Interface combining disposal with value access | Immutable (interface) |
| `AsyncDisposableValue<T>` | Internal wrapper holding async generator | Mutable (tracks enumerator) |
| `Done` | Sentinel marking producer completion | Immutable (stateless) |

---

## IAsyncDisposableValue<T>

**Purpose**: Public interface returned by `Aclosing<T>()` that combines `IAsyncDisposable` with access to the wrapped value.

**Fields**:
| Name | Type | Description |
|------|------|-------------|
| `Value` | `T` (read-only) | The wrapped async generator |

**Relationships**:
- Extends `IAsyncDisposable`
- Implemented by internal `AsyncDisposableValue<T>`

**Validation Rules**:
- None (interface contract only)

---

## AsyncDisposableValue<T>

**Purpose**: Internal implementation that tracks the async enumerator and ensures proper disposal.

**Fields**:
| Name | Type | Description |
|------|------|-------------|
| `Value` | `IAsyncEnumerable<T>` | The wrapped async generator |
| `_enumerator` | `IAsyncEnumerator<T>?` | Cached enumerator for disposal |

**State Transitions**:
```
Created → Enumerating → Disposed
           ↓
         (Exception during enumeration)
           ↓
         Disposed
```

**Relationships**:
- Implements `IAsyncDisposableValue<IAsyncEnumerable<T>>`
- Holds reference to user-provided `IAsyncEnumerable<T>`

**Validation Rules**:
- `Value` must not be null (validated at construction)

---

## Done (Sentinel)

**Purpose**: Internal marker type signaling that the producer has finished.

**Fields**: None (stateless marker type)

**Relationships**:
- Used only within `GeneratorToAsyncGenerator` producer-consumer flow
- Placed in `BlockingCollection<object>` to signal completion

**Validation Rules**: None

---

## GeneratorToAsyncGenerator Internal State

**Purpose**: The async generator returned by `GeneratorToAsyncGenerator<T>()` maintains internal producer-consumer state.

**Internal Fields** (not a separate type, but documented for clarity):
| Name | Type | Description |
|------|------|-------------|
| `_quitting` | `volatile bool` | Cancellation flag for producer thread |
| `_queue` | `BlockingCollection<object>` | Bounded buffer for items + Done sentinel |
| `_producerTask` | `Task` | Background thread running the producer |
| `_producerException` | `Exception?` | Stored exception from producer |

**State Transitions**:
```
Idle (not enumerated) → Running (producer active) → Completed (Done received)
                                ↓
                         Disposed (quitting=true, await producer)
```

**Buffer Behavior**:
- Default capacity: 1000 (matches Python `DEFAULT_BUFFER_SIZE`)
- Producer blocks on `TryAdd` when buffer is full
- Consumer receives items via `Task.Run(() => queue.Take())`

---

## Relationships Diagram

```
┌─────────────────────────────────────┐
│   User Code                         │
│   await using (Aclosing(asyncGen))  │
└──────────────┬──────────────────────┘
               │ returns
               ▼
┌─────────────────────────────────────┐
│ IAsyncDisposableValue<IAsyncEnum>   │
│   .Value → IAsyncEnumerable<T>      │
│   .DisposeAsync() → cleanup         │
└──────────────┬──────────────────────┘
               │ implemented by
               ▼
┌─────────────────────────────────────┐
│ AsyncDisposableValue<T> (internal)  │
│   tracks enumerator for disposal    │
└─────────────────────────────────────┘


┌─────────────────────────────────────┐
│   User Code                         │
│   await foreach (GeneratorToAsync)  │
└──────────────┬──────────────────────┘
               │ iterates
               ▼
┌─────────────────────────────────────────────────────┐
│ IAsyncEnumerable<T> (returned by GeneratorToAsync)  │
│   ┌─────────────┐        ┌─────────────────────┐   │
│   │ Producer    │ ─────► │ BlockingCollection  │   │
│   │ (bg thread) │ items  │ (bounded buffer)    │   │
│   └─────────────┘        └──────────┬──────────┘   │
│         ▲                           │              │
│         │ quitting                  ▼              │
│         │ flag        ┌─────────────────────┐      │
│         └──────────── │ Consumer            │      │
│                       │ (async MoveNext)    │      │
│                       └─────────────────────┘      │
└─────────────────────────────────────────────────────┘
```

---

## Constants

| Name | Value | Description |
|------|-------|-------------|
| `DefaultBufferSize` | `1000` | Default bounded buffer capacity |
| `ProducerTimeout` | `1 second` | Timeout for `TryAdd` to check quitting flag |
