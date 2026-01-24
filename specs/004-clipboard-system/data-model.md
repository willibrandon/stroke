# Data Model: Clipboard System

**Feature**: 004-clipboard-system
**Date**: 2026-01-23

## Entities

### ClipboardData (Existing)

**Description**: Immutable value object representing text content with selection type information.

**Location**: `src/Stroke/Core/ClipboardData.cs`

**Properties**:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Text` | `string` | `""` | The clipboard text content |
| `Type` | `SelectionType` | `Characters` | Selection type (Characters, Lines, Block) |

**Validation**:
- None (accepts any string including empty)

**Relationships**:
- Used by: IClipboard implementations (stored in kill ring)
- Depends on: SelectionType enum

### IClipboard (New)

**Description**: Interface defining clipboard operations for storing and retrieving text with selection type.

**Location**: `src/Stroke/Core/IClipboard.cs`

**Methods**:

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `SetData` | `ClipboardData data` | `void` | Store data on clipboard |
| `GetData` | none | `ClipboardData` | Retrieve current data |
| `SetText` | `string text` | `void` | Shortcut for plain text (default impl) |
| `Rotate` | none | `void` | Rotate kill ring (default impl: no-op) |

**Implementations**:
- `DummyClipboard`: No-op storage
- `InMemoryClipboard`: Kill ring storage
- `DynamicClipboard`: Delegate wrapper

### DummyClipboard (New)

**Description**: Clipboard implementation that stores nothing and returns empty data.

**Location**: `src/Stroke/Core/DummyClipboard.cs`

**Behavior**:

| Method | Behavior |
|--------|----------|
| `SetData` | No-op |
| `SetText` | No-op |
| `Rotate` | No-op |
| `GetData` | Returns `new ClipboardData()` |

**State**: None (stateless)

### InMemoryClipboard (New)

**Description**: Default clipboard implementation with kill ring for Emacs-style yank-pop.

**Location**: `src/Stroke/Core/InMemoryClipboard.cs`

**Constructor Parameters**:

| Parameter | Type | Default | Validation |
|-----------|------|---------|------------|
| `data` | `ClipboardData?` | `null` | None |
| `maxSize` | `int` | `60` | Must be >= 1 |

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `MaxSize` | `int` | Maximum kill ring capacity (read-only) |

**Internal State**:

| Field | Type | Description |
|-------|------|-------------|
| `_ring` | `LinkedList<ClipboardData>` | Kill ring storage |

**Behavior**:

| Method | Behavior |
|--------|----------|
| `SetData` | Add to front; trim if > maxSize |
| `GetData` | Return first item or empty ClipboardData |
| `Rotate` | Move first item to end (if ring non-empty) |

**Kill Ring Semantics**:

```
Initial: [A, B, C] (A = most recent)
SetData(D): [D, A, B, C] → trim if needed
GetData(): returns D
Rotate(): [A, B, C, D] (D moved to end)
GetData(): returns A
```

### DynamicClipboard (New)

**Description**: Clipboard wrapper that delegates to dynamically selected clipboard.

**Location**: `src/Stroke/Core/DynamicClipboard.cs`

**Constructor Parameters**:

| Parameter | Type | Description |
|-----------|------|-------------|
| `getClipboard` | `Func<IClipboard?>` | Callback returning clipboard to use |

**Internal State**:

| Field | Type | Description |
|-------|------|-------------|
| `_getClipboard` | `Func<IClipboard?>` | Stored callback |

**Behavior**:

| Method | Behavior |
|--------|----------|
| `SetData` | Call `_getClipboard()?.SetData(data)` or dummy |
| `SetText` | Call `_getClipboard()?.SetText(text)` or dummy |
| `Rotate` | Call `_getClipboard()?.Rotate()` or dummy |
| `GetData` | Return `_getClipboard()?.GetData()` or empty |

**Fallback**: When `_getClipboard()` returns `null`, use `new DummyClipboard()`.

## Entity Relationships

```
                    ┌─────────────────┐
                    │   IClipboard    │
                    │   (interface)   │
                    └────────┬────────┘
                             │
           ┌─────────────────┼─────────────────┐
           │                 │                 │
           ▼                 ▼                 ▼
   ┌───────────────┐ ┌───────────────┐ ┌───────────────┐
   │DummyClipboard │ │InMemoryClip.  │ │DynamicClip.   │
   │  (sealed)     │ │  (sealed)     │ │  (sealed)     │
   └───────────────┘ └───────┬───────┘ └───────┬───────┘
                             │                 │
                             │ uses            │ delegates to
                             ▼                 ▼
                     ┌───────────────┐ ┌───────────────┐
                     │ClipboardData  │ │  IClipboard?  │
                     │  (immutable)  │ │  (via Func)   │
                     └───────┬───────┘ └───────────────┘
                             │
                             │ contains
                             ▼
                     ┌───────────────┐
                     │SelectionType  │
                     │   (enum)      │
                     └───────────────┘
```

## State Transitions

### InMemoryClipboard Kill Ring

```
State: Empty
  │
  │ SetData(A)
  ▼
State: [A]
  │
  │ SetData(B)
  ▼
State: [B, A]
  │
  │ Rotate()
  ▼
State: [A, B]
  │
  │ GetData() → returns A
  │
  │ SetData(C)
  ▼
State: [C, A, B]
  │
  │ (repeat until maxSize reached)
  │
  │ SetData(X) when size == maxSize
  ▼
State: [X, ...] (oldest removed from end)
```

## Validation Rules

| Entity | Rule | Error |
|--------|------|-------|
| InMemoryClipboard | maxSize >= 1 | `ArgumentOutOfRangeException` |
| DynamicClipboard | getClipboard != null | `ArgumentNullException` |
| ClipboardData | text != null | `ArgumentNullException` (TBD - Python allows None→"") |

## Notes

1. **ClipboardData already exists** - Implementation was part of earlier work
2. **ClipboardDataTests already exist** - 17 tests covering constructor and properties
3. **Namespace**: All types in `Stroke.Core` per api-mapping.md
4. **Thread Safety**: Required per Constitution XI - InMemoryClipboard uses System.Threading.Lock
