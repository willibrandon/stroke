# Research: Clipboard System

**Feature**: 004-clipboard-system
**Date**: 2026-01-23

## Overview

This research documents design decisions for the Clipboard System implementation based on Python Prompt Toolkit reference analysis and existing Stroke codebase patterns.

## Research Topics

### 1. IClipboard Interface Design

**Decision**: Use C# interface (`IClipboard`) instead of abstract class

**Rationale**:
- `docs/api-mapping.md` explicitly maps Python `Clipboard` ABC to C# `IClipboard`
- Interfaces provide cleaner composition and testability in C#
- All method signatures are well-defined with no shared implementation state
- Default interface methods (C# 8+) can provide `SetText` and `Rotate` defaults

**Alternatives Considered**:
- Abstract class (rejected: api-mapping.md specifies interface)
- Record-based approach (rejected: clipboard has mutable state in implementations)

### 2. Kill Ring Data Structure

**Decision**: Use `LinkedList<ClipboardData>` for kill ring storage

**Rationale**:
- O(1) insertion at front (`AddFirst`)
- O(1) removal from back (`RemoveLast`)
- O(1) rotation (move first to last)
- Python uses `deque` which has same characteristics

**Alternatives Considered**:
- `List<T>` (rejected: O(n) insertion at front)
- `Queue<T>` (rejected: doesn't support efficient front access)
- Circular array (rejected: more complex, no benefit for typical sizes)

### 3. Thread Safety

**Decision**: No thread safety required

**Rationale**:
- Python Prompt Toolkit does not implement thread-safe clipboard
- Spec assumption states single-threaded UI context
- Adding thread safety would deviate from faithful port
- If needed later, can wrap with thread-safe decorator

**Alternatives Considered**:
- `ConcurrentQueue<T>` (rejected: unnecessary complexity, different semantics)
- Lock-based synchronization (rejected: not in Python original)

### 4. Namespace Placement

**Decision**: Place all clipboard types in `Stroke.Core` namespace

**Rationale**:
- `ClipboardData` already exists in `Stroke.Core`
- Maintains layered architecture (Core has no external dependencies)
- `SelectionType` dependency is in same namespace
- Consistent with existing codebase structure

**Alternatives Considered**:
- `Stroke.Clipboard` namespace (rejected: conflicts with api-mapping.md showing types in same module as selection)
- Separate Clipboard subfolder (rejected: adds unnecessary complexity for 4 small files)

### 5. Default Interface Methods vs Abstract Class

**Decision**: Use default interface methods for `SetText` and `Rotate`

**Rationale**:
- C# 8+ supports default interface implementations
- `SetText` default: `SetData(new ClipboardData(text))`
- `Rotate` default: no-op (matches Python where `rotate()` has empty body)
- Implementations can override if needed (InMemoryClipboard overrides Rotate)

**Implementation**:
```csharp
public interface IClipboard
{
    void SetData(ClipboardData data);
    ClipboardData GetData();

    void SetText(string text) => SetData(new ClipboardData(text));
    void Rotate() { } // No-op default
}
```

### 6. DynamicClipboard Fallback Behavior

**Decision**: Fallback to `new DummyClipboard()` when delegate returns null

**Rationale**:
- Matches Python Prompt Toolkit behavior exactly
- `_clipboard()` method returns `self.get_clipboard() or DummyClipboard()`
- Creates new DummyClipboard instance on each null return
- Simple and faithful to original

**Alternatives Considered**:
- Cache single DummyClipboard instance (rejected: minor optimization, deviates from Python)
- Throw exception on null (rejected: Python returns DummyClipboard)

### 7. Constructor Parameter Validation

**Decision**: Validate `maxSize >= 1` in InMemoryClipboard constructor

**Rationale**:
- Python uses `assert max_size >= 1`
- C# equivalent: `ArgumentOutOfRangeException` for invalid values
- Consistent with .NET conventions

**Implementation**:
```csharp
public InMemoryClipboard(ClipboardData? data = null, int maxSize = 60)
{
    if (maxSize < 1)
        throw new ArgumentOutOfRangeException(nameof(maxSize), "Must be at least 1.");
    // ...
}
```

## Python Reference Analysis

### base.py Analysis

| Python API | C# Mapping | Notes |
|------------|------------|-------|
| `ClipboardData` | `ClipboardData` | Already implemented |
| `ClipboardData.text` | `ClipboardData.Text` | Property |
| `ClipboardData.type` | `ClipboardData.Type` | Property |
| `Clipboard` (ABC) | `IClipboard` | Interface |
| `Clipboard.set_data` | `IClipboard.SetData` | Abstract in Python |
| `Clipboard.get_data` | `IClipboard.GetData` | Abstract in Python |
| `Clipboard.set_text` | `IClipboard.SetText` | Default impl |
| `Clipboard.rotate` | `IClipboard.Rotate` | Default impl (no-op) |
| `DummyClipboard` | `DummyClipboard` | All methods no-op |
| `DynamicClipboard` | `DynamicClipboard` | Delegate wrapper |

### in_memory.py Analysis

| Python API | C# Mapping | Notes |
|------------|------------|-------|
| `InMemoryClipboard.__init__` | Constructor | data + maxSize params |
| `InMemoryClipboard.max_size` | `MaxSize` property | Read-only |
| `InMemoryClipboard._ring` | `_ring` field | LinkedList |
| `InMemoryClipboard.set_data` | `SetData` | AddFirst + trim |
| `InMemoryClipboard.get_data` | `GetData` | First or empty |
| `InMemoryClipboard.rotate` | `Rotate` | Move first to last |

## Conclusion

All technical decisions align with:
1. Python Prompt Toolkit semantics (faithful port)
2. docs/api-mapping.md specifications
3. Existing Stroke.Core codebase patterns
4. Constitution principles

No NEEDS CLARIFICATION items remain. Ready for Phase 1 design.
