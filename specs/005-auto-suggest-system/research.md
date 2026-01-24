# Research: Auto Suggest System

**Feature**: 005-auto-suggest-system
**Date**: 2026-01-23

## Overview

Research findings for implementing the auto-suggestion system. All technical context items were clear from the specification; this document captures decisions and patterns from the Python source analysis.

## Python Source Analysis

**Source**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/auto_suggest.py`

### Key Implementation Details

1. **Suggestion Class** (lines 39-51)
   - Simple container with `text` property
   - Has `__repr__` for debugging
   - No validation on text (empty string is valid)

2. **AutoSuggest Base Class** (lines 53-84)
   - Abstract base with `get_suggestion(buffer, document)` method
   - Default async implementation just calls sync method
   - Buffer and document passed separately for async safety

3. **ThreadedAutoSuggest** (lines 86-109)
   - Wraps another AutoSuggest
   - Sync method delegates directly
   - Async method uses `run_in_executor_with_context`
   - Preserves execution context when running in thread pool

4. **DummyAutoSuggest** (lines 112-118)
   - Returns `None` (no suggestion)
   - Stateless

5. **AutoSuggestFromHistory** (lines 121-140)
   - Core algorithm:
     1. Get current line (text after last newline via `rsplit("\n", 1)[-1]`)
     2. Skip if empty or whitespace-only
     3. Iterate history in reverse (most recent first)
     4. For each history entry, check each line in reverse
     5. Return first match where line starts with current text
     6. Suggestion is the suffix (after matched prefix)

6. **ConditionalAutoSuggest** (lines 143-156)
   - Takes underlying auto-suggest and filter
   - Filter is converted via `to_filter()` (supports bool or Filter type)
   - Calls underlying only if filter evaluates to true

7. **DynamicAutoSuggest** (lines 159-177)
   - Takes callable that returns AutoSuggest or None
   - Falls back to DummyAutoSuggest if callable returns None
   - Both sync and async methods evaluate callable each time

## Design Decisions

### Decision 1: Suggestion as Record Type

**Decision**: Use C# `record` type for `Suggestion`
**Rationale**: Records provide immutability, value equality, and automatic `ToString()` implementation
**Alternatives Considered**:
- `sealed class` - more verbose, requires manual equality
- `readonly struct` - value type semantics might cause boxing in interface contexts

### Decision 2: Interface vs Abstract Class

**Decision**: Use `IAutoSuggest` interface (not abstract class)
**Rationale**:
- Follows existing Stroke pattern (IClipboard, IHistory from api-mapping.md)
- Allows multiple inheritance scenarios
- Python's ABC is conceptually an interface
**Alternatives Considered**:
- Abstract class with default async implementation - would require inheriting from base

### Decision 3: Async Pattern

**Decision**: Use `ValueTask<Suggestion?>` for async method (not `Task<Suggestion?>`)
**Rationale**:
- Most implementations return synchronously (just wrap sync result)
- ValueTask avoids allocation for sync-completing operations
- Matches spec requirement for performance-conscious design
**Alternatives Considered**:
- `Task<Suggestion?>` - always allocates, even for sync completion

### Decision 4: Filter Type

**Decision**: Use `Func<bool>` for ConditionalAutoSuggest filter parameter
**Rationale**:
- Simpler than Python's Filter class
- Stroke.Filters namespace not yet implemented
- Can be upgraded to Filter interface later without breaking API
**Alternatives Considered**:
- Python-style Filter class - would require implementing Filter system first

### Decision 5: Thread Pool Execution

**Decision**: Use `Task.Run()` for ThreadedAutoSuggest async execution
**Rationale**:
- Standard .NET pattern for offloading to thread pool
- Matches Python's `run_in_executor_with_context` semantics
- Captures current SynchronizationContext for proper async flow
**Alternatives Considered**:
- Custom thread management - unnecessary complexity
- `ThreadPool.QueueUserWorkItem` - less integrated with async/await

### Decision 6: History Interface Dependency

**Decision**: Define minimal `IHistory` interface stub in this feature
**Rationale**:
- AutoSuggestFromHistory needs `GetStrings()` method
- Full History feature (with FileHistory, etc.) is future work
- Interface-first design allows compilation and testing now
**Alternatives Considered**:
- Wait for Buffer/History features - would block this feature
- Pass history strings directly - deviates from Python API

### Decision 7: Buffer Interface Dependency

**Decision**: Define minimal `IBuffer` interface stub with `History` property
**Rationale**:
- Python API passes buffer to `get_suggestion(buffer, document)`
- Buffer provides access to history
- Minimal interface allows testing without full Buffer implementation
**Alternatives Considered**:
- Change API to pass history directly - breaks faithful porting
- Create temporary IBuffer mock - violates Constitution VIII

## Thread Safety Analysis

| Type | Mutable State | Thread Safety Strategy |
|------|---------------|----------------------|
| `Suggestion` | None (immutable record) | Inherently thread-safe |
| `IAutoSuggest` | N/A (interface) | N/A |
| `DummyAutoSuggest` | None (stateless) | Inherently thread-safe |
| `AutoSuggestFromHistory` | None (stateless) | Inherently thread-safe |
| `ConditionalAutoSuggest` | None (stores references only) | Inherently thread-safe |
| `DynamicAutoSuggest` | None (stores reference only) | Inherently thread-safe |
| `ThreadedAutoSuggest` | None (stores reference only) | Inherently thread-safe |

**Conclusion**: All types are stateless or immutable; no synchronization required per Constitution XI.

## Performance Considerations

### AutoSuggestFromHistory Algorithm

- **Time Complexity**: O(H × L) where H = history entries, L = avg lines per entry
- **Space Complexity**: O(1) - no additional allocations during search
- **Early Termination**: Returns on first match (most common case is quick)
- **Target**: 1ms for 10,000 entries (SC-001)

**Optimization Strategy**:
1. Use `string.StartsWith()` with `StringComparison.Ordinal` for fast prefix matching
2. Iterate history in reverse (most recent first) for likely quick matches
3. Use `ReadOnlySpan<char>` where possible to avoid substring allocations

## Test Implementation Notes

### Testing Without Full Buffer Implementation

For `AutoSuggestFromHistory` tests, create a minimal test implementation:

```csharp
// Test implementation for AutoSuggestFromHistory
internal sealed class TestBuffer : IBuffer
{
    public IHistory History { get; }
    public Document Document { get; set; } = new();

    public TestBuffer(IHistory history) => History = history;
}

internal sealed class TestHistory : IHistory
{
    private readonly List<string> _strings = new();

    public IReadOnlyList<string> GetStrings() => _strings;
    public void AppendString(string value) => _strings.Add(value);
    // Other methods throw NotImplementedException
}
```

This provides real implementations (not mocks) that satisfy Constitution VIII while enabling testing before the full Buffer/History features are complete.

## No Outstanding Research Items

All NEEDS CLARIFICATION items from Technical Context have been resolved:
- Language/Version: C# 13 / .NET 10 ✓
- Dependencies: None (Core layer) ✓
- Testing: xUnit with standard assertions ✓
- Performance: 1ms target confirmed ✓
- Constraints: Thread safety confirmed (all stateless) ✓
