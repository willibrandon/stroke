# Research: History System

**Feature**: History System
**Date**: 2026-01-24
**Status**: Complete

## Research Tasks

### 1. Python Prompt Toolkit History Implementation Analysis

**Task**: Analyze the exact semantics of Python Prompt Toolkit's history module.

**Decision**: Port all classes exactly as defined in `prompt_toolkit/history.py`.

**Rationale**: Constitution I requires 100% API fidelity. The Python implementation provides a clear template for caching behavior, loading order, and file format.

**Key Findings**:

1. **Base Class Pattern**: Python uses `History` as an abstract base class with:
   - `_loaded: bool` - Flag indicating if history has been loaded
   - `_loaded_strings: list[str]` - In-memory cache (newest-first order)
   - `load()` - Async generator that yields strings in newest-first order
   - `get_strings()` - Returns strings in oldest-first order (reverses `_loaded_strings`)
   - `append_string()` - Inserts at index 0 (newest first) and calls `store_string()`
   - `load_history_strings()` - Abstract method for backend-specific loading
   - `store_string()` - Abstract method for backend-specific persistence

2. **Loading Order Convention**:
   - `load_history_strings()` MUST yield newest-first (most recent items first)
   - `_loaded_strings` stores items in newest-first order
   - `get_strings()` reverses to return oldest-first
   - This allows partial loading while maintaining usability

3. **ThreadedHistory Pattern**:
   - Wraps another `History` instance
   - Spawns background thread on first `load()` call
   - Uses `threading.Event` for signaling between loader and consumers
   - Multiple concurrent `load()` calls share the same data
   - `append_string()` inserts at index 0 under lock, then calls `store_string()` on wrapped history

**Alternatives Considered**:
- Custom loading order: Rejected - would break API fidelity
- Simplified interface: Rejected - must match Python exactly

### 2. File Format Specification

**Task**: Document the exact file format used by FileHistory.

**Decision**: Match Python Prompt Toolkit's file format byte-for-byte.

**Rationale**: SC-002 requires byte-for-byte compatibility with Python Prompt Toolkit's format.

**File Format Specification**:

```text
# <timestamp>
+<line1>
+<line2>
...

# <timestamp>
+<single-line-command>
```

**Rules**:
1. **Comments**: Lines starting with `#` are comments (ignored when loading)
2. **Entry prefix**: Lines starting with `+` are entry content
3. **Multi-line entries**: Each line of a multi-line entry is prefixed with `+`
4. **Timestamps**: Written as `# YYYY-MM-DD HH:MM:SS.ffffff` (Python datetime format)
5. **Encoding**: UTF-8 with `errors="replace"` for invalid sequences
6. **Newlines**: Entries are separated by blank lines (timestamp comment starts new entry)
7. **Trailing newline**: Each `+` line includes the trailing newline character

**Example File**:
```text
# 2026-01-24 10:30:15.123456
+ls -la

# 2026-01-24 10:31:00.789012
+echo "hello
+world"
```

**Parsing Algorithm**:
1. Read file line by line
2. Decode each line as UTF-8 with replacement
3. If line starts with `+`, append `line[1:]` to current entry's lines
4. If line does NOT start with `+`, finalize current entry (if any) and reset
5. Finalize last entry after EOF
6. Reverse the list (file stores oldest-first, we return newest-first)

**Alternatives Considered**:
- JSON format: Rejected - not compatible with Python PTK
- Custom binary format: Rejected - not compatible, not human-readable

### 3. Thread Safety Implementation

**Task**: Determine thread safety approach for all mutable history implementations.

**Decision**: Use `System.Threading.Lock` (.NET 9+) with `EnterScope()` pattern.

**Rationale**: Constitution XI requires thread safety by default. The Lock type provides better performance than `lock` statement in .NET 9+.

**Implementation Pattern**:
```csharp
public sealed class SomeHistory : HistoryBase
{
    private readonly Lock _lock = new();

    protected override IEnumerable<string> LoadHistoryStrings()
    {
        using (_lock.EnterScope())
        {
            // Thread-safe loading
        }
    }
}
```

**Thread Safety by Class**:

| Class | Mutable State | Thread Safety Approach |
|-------|--------------|------------------------|
| `HistoryBase` | `_loaded`, `_loaded_strings` | Lock in `LoadAsync`, `AppendString` |
| `InMemoryHistory` | `_storage` | Lock in `LoadHistoryStrings`, `StoreString` |
| `DummyHistory` | None | Inherently thread-safe (stateless) |
| `FileHistory` | File I/O | Lock around file operations |
| `ThreadedHistory` | `_loadThread`, `_loadedStrings`, `_loaded`, `_stringLoadEvents` | Lock per Python pattern |

**Alternatives Considered**:
- `lock` statement: Works but `Lock` is preferred in .NET 9+
- `ReaderWriterLockSlim`: Over-engineered for this use case
- Immutable state: Not practical for mutable caching behavior

### 4. Async Pattern for LoadAsync

**Task**: Determine the C# async pattern for history loading.

**Decision**: Use `IAsyncEnumerable<string>` with `CancellationToken` support.

**Rationale**: Matches Python's async generator pattern while providing standard .NET async patterns.

**Implementation**:
```csharp
public interface IHistory
{
    IAsyncEnumerable<string> LoadAsync(CancellationToken cancellationToken = default);
    // ...
}
```

**Key Behaviors**:
1. `LoadAsync` yields items as they become available
2. First call triggers loading; subsequent calls use cache
3. Items yielded in newest-first order (most recent first)
4. `CancellationToken` allows early termination
5. ThreadedHistory yields items progressively as background thread loads them

**Alternatives Considered**:
- `Task<IReadOnlyList<string>>`: Doesn't support progressive yielding
- `IEnumerable<string>`: Doesn't support async I/O

### 5. Existing Code Analysis

**Task**: Analyze existing `IHistory` and `InMemoryHistory` implementations.

**Decision**: Update existing implementations to match Python Prompt Toolkit semantics.

**Findings**:

Current `IHistory.cs`:
- Missing: `LoadHistoryStrings()`, `StoreString()` abstract methods
- Has: `GetStrings()`, `AppendString()`, `LoadAsync()` (correct signatures)

Current `InMemoryHistory.cs`:
- Missing: `_storage` vs `_history` naming convention
- Missing: Pre-population constructor parameter
- Missing: Proper loading order (should yield newest-first)
- Has: Thread safety with `Lock` (correct pattern)

**Required Changes**:
1. Add `LoadHistoryStrings()` and `StoreString()` to `IHistory`
2. Create `HistoryBase` abstract class for shared caching logic
3. Update `InMemoryHistory` to use `_storage` list and add constructor with pre-population
4. Fix loading order in `InMemoryHistory.LoadHistoryStrings()` to yield newest-first

### 6. Test Strategy

**Task**: Define testing approach for history implementations.

**Decision**: Use xUnit with real file system and real threading.

**Rationale**: Constitution VIII forbids mocks/fakes/doubles.

**Test Categories**:

1. **Unit Tests** (per implementation):
   - InMemoryHistoryTests: Basic operations, pre-population, ordering
   - DummyHistoryTests: Verify no-op behavior
   - FileHistoryTests: File I/O, format compatibility, encoding
   - ThreadedHistoryTests: Background loading, progressive yielding

2. **Integration Tests**:
   - Cross-session persistence with FileHistory
   - ThreadedHistory wrapping FileHistory
   - Concurrent access stress tests (10+ threads, 1000+ operations)

3. **Format Compatibility Tests**:
   - Read files written by Python Prompt Toolkit
   - Write files readable by Python Prompt Toolkit

**Test Utilities Needed**:
- Temp file management for FileHistory tests
- Async test helpers for IAsyncEnumerable assertions
- Concurrent test helpers for thread safety verification

## Summary

All research tasks completed. No NEEDS CLARIFICATION items remain. Ready for Phase 1 design.

| Research Task | Status | Key Decision |
|---------------|--------|--------------|
| Python PTK Analysis | ✅ Complete | Port exact semantics including loading order |
| File Format | ✅ Complete | Match byte-for-byte with `+` prefix format |
| Thread Safety | ✅ Complete | Use `System.Threading.Lock` throughout |
| Async Pattern | ✅ Complete | Use `IAsyncEnumerable<string>` |
| Existing Code | ✅ Complete | Update to match PTK semantics |
| Test Strategy | ✅ Complete | Real file system, real threading, no mocks |
