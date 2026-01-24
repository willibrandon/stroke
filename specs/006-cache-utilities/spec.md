# Feature Specification: Cache Utilities

**Feature Branch**: `006-cache-utilities`
**Created**: 2026-01-23
**Status**: Draft
**Input**: User description: "Implement the caching utilities used throughout Stroke for performance optimization. Port SimpleCache, FastDictCache, and memoization utilities from Python Prompt Toolkit cache.py module."

## Reference

**Source**: Python Prompt Toolkit `cache.py` (commit: HEAD of main branch)
**Location**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/cache.py`
**Target Namespace**: `Stroke.Core` (per `docs/api-mapping.md`)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Framework Developer Uses SimpleCache (Priority: P1)

A Stroke framework developer needs to cache expensive computations where the cache key is separate from the function arguments. They create a SimpleCache with a small maximum size to store computed values, retrieving cached results on subsequent calls and automatically evicting the oldest entries when the cache fills.

**Why this priority**: SimpleCache is the foundational caching primitive used internally by memoization and throughout the framework. It must work correctly before other cache types can be built.

**Independent Test**: Can be fully tested by creating a SimpleCache, adding entries, verifying retrieval, and confirming eviction behavior when max size is exceeded.

**Acceptance Scenarios**:

1. **Given** an empty SimpleCache with maxSize=3, **When** retrieving a key that doesn't exist with a getter function, **Then** the getter function is called and the result is cached and returned
2. **Given** a SimpleCache containing a cached value, **When** retrieving the same key, **Then** the cached value is returned without calling the getter function
3. **Given** a SimpleCache at maximum capacity, **When** adding a new entry, **Then** the oldest entry (first inserted) is evicted to make room
4. **Given** a SimpleCache with entries, **When** calling Clear(), **Then** all entries are removed

---

### User Story 2 - Framework Developer Uses FastDictCache for High-Frequency Lookups (Priority: P1)

A Stroke framework developer needs to cache lightweight immutable objects (like Char or Document instances) where dictionary lookup is faster than object instantiation. They use FastDictCache with a large default size (1,000,000 entries) that automatically creates missing values using the provided factory function.

**Why this priority**: FastDictCache is specifically designed for the most performance-critical caching scenarios in Stroke (Char and Document caching). It must be extremely lightweight and fast.

**Independent Test**: Can be fully tested by creating a FastDictCache, accessing keys via indexer, verifying factory invocation for missing keys, and confirming eviction at capacity.

**Acceptance Scenarios**:

1. **Given** an empty FastDictCache with a getValue factory, **When** accessing a key via indexer that doesn't exist, **Then** the getValue factory is called with the key, result is cached and returned
2. **Given** a FastDictCache containing a cached value, **When** accessing the same key via indexer, **Then** the cached value is returned without calling getValue
3. **Given** a FastDictCache at maximum size, **When** accessing a new key, **Then** the oldest entry (first inserted) is evicted before adding the new entry
4. **Given** a FastDictCache, **When** calling ContainsKey or TryGetValue, **Then** correct existence/value information is returned without triggering factory

---

### User Story 3 - Framework Developer Uses Memoization for Pure Functions (Priority: P2)

A Stroke framework developer wants to automatically cache results of pure functions or immutable class constructors. They use the Memoization.Memoize methods to wrap functions, ensuring repeated calls with the same arguments return cached results.

**Why this priority**: Memoization builds on SimpleCache and provides a convenient API for common caching patterns. It depends on SimpleCache being implemented first.

**Independent Test**: Can be fully tested by memoizing a function, calling it multiple times with same/different arguments, and verifying cache hits vs. factory calls.

**Acceptance Scenarios**:

1. **Given** a memoized function, **When** calling it with arguments for the first time, **Then** the original function executes and the result is cached
2. **Given** a memoized function that has been called, **When** calling it again with the same arguments, **Then** the cached result is returned without executing the original function
3. **Given** a memoized function, **When** calling it with different arguments, **Then** each unique argument combination is cached separately
4. **Given** a memoized function with maxSize reached, **When** calling with new arguments, **Then** oldest cached results are evicted

---

### Edge Cases

| Edge Case | Expected Behavior | Rationale |
|-----------|-------------------|-----------|
| maxSize/size set to 1 | Single-entry cache functions correctly; each new entry evicts the previous | Boundary condition must work |
| maxSize/size set to 0 or negative | Constructor MUST throw `ArgumentOutOfRangeException` | Invalid configuration |
| Same key retrieved multiple times | Returns cached value; key not duplicated in eviction tracking | Correctness |
| Null value returned by getter/factory | Null is cached as valid value; subsequent Gets return null without calling getter | Null is distinguishable from "not cached" |
| Factory throws exception | Exception propagates to caller; no entry added to cache; cache state unchanged | Atomicity |
| Null getter/factory passed to constructor or Get | MUST throw `ArgumentNullException` | Fail-fast on invalid input |
| Existing key accessed via Get/indexer | Returns cached value without invoking getter/factory | Cache hit behavior |

## Requirements *(mandatory)*

### Functional Requirements

#### SimpleCache

- **FR-001**: System MUST provide a `SimpleCache<TKey, TValue>` class with configurable maximum size (default: 8)
- **FR-002**: SimpleCache MUST evict the oldest entry when cache count exceeds maximum size (FIFO eviction, triggered when `Count > MaxSize`)
- **FR-003**: SimpleCache MUST provide a `Get(TKey key, Func<TValue> getter)` method that returns cached values or invokes getter for missing keys
- **FR-004**: SimpleCache MUST provide a `Clear()` method that removes all cached entries and resets eviction tracking
- **FR-005**: SimpleCache MUST expose `MaxSize` as a read-only `int` property
- **FR-017**: SimpleCache constructor MUST throw `ArgumentOutOfRangeException` if maxSize ≤ 0
- **FR-018**: SimpleCache.Get MUST throw `ArgumentNullException` if getter is null

#### FastDictCache

- **FR-006**: System MUST provide a `FastDictCache<TKey, TValue>` class with configurable size (default: 1,000,000)
- **FR-007**: FastDictCache MUST invoke the getValue factory when accessing a missing key via read-only indexer (`this[TKey key]`)
- **FR-008**: FastDictCache MUST evict the oldest entry before adding a new entry when cache count exceeds size (FIFO eviction, triggered when `Count > Size`)
- **FR-009**: FastDictCache MUST provide `ContainsKey(TKey key)` method that checks existence without invoking factory
- **FR-010**: FastDictCache MUST provide `TryGetValue(TKey key, out TValue value)` method that retrieves without invoking factory
- **FR-011**: FastDictCache MUST expose `Size` (maximum capacity) and `Count` (current entry count) as read-only `int` properties
- **FR-019**: FastDictCache constructor MUST throw `ArgumentOutOfRangeException` if size ≤ 0
- **FR-020**: FastDictCache constructor MUST throw `ArgumentNullException` if getValue factory is null

#### Memoization

- **FR-012**: System MUST provide a `Memoization` static class with exactly three `Memoize` method overloads for 1, 2, and 3 argument functions
- **FR-013**: Memoization MUST use argument values as cache keys, using `ValueTuple` for multi-argument functions to provide structural equality
- **FR-014**: Memoization MUST support configurable maximum cache size (default: 1024)
- **FR-021**: Memoization.Memoize MUST throw `ArgumentNullException` if func parameter is null
- **FR-022**: Memoization.Memoize MUST throw `ArgumentOutOfRangeException` if maxSize ≤ 0
- **FR-023**: Memoization argument types MUST have `notnull` constraint (`where T1 : notnull`, etc.)

#### Cross-Cutting

- **FR-015**: All cache implementations MUST require non-null keys via `where TKey : notnull` generic constraint
- **FR-016**: Cache implementations MUST be thread-safe using `System.Threading.Lock` with `EnterScope()` pattern (per Constitution Principle XI)
- **FR-024**: All cache classes MUST be `sealed` (not designed for inheritance)
- **FR-025**: Cache classes MUST NOT implement `IDisposable` (no unmanaged resources)
- **FR-026**: Thread safety MUST be verified with concurrent stress tests (10+ threads, 1000+ operations) per Constitution XI

### Behavioral Clarifications

| Behavior | Specification | Python PTK Parity |
|----------|---------------|-------------------|
| "Oldest entry" definition | First inserted entry (FIFO order), not least recently accessed (LRU) | ✓ Matches |
| Eviction trigger | When `Count > MaxSize/Size` after potential addition | ✓ Matches |
| FastDictCache eviction timing | Evicts oldest BEFORE adding new entry | ✓ Matches |
| Indexer semantics | FastDictCache indexer is read-only; equivalent to Python `__missing__` | ✓ Matches |
| Property naming | SimpleCache uses `MaxSize`, FastDictCache uses `Size` (intentional Python parity) | ✓ Matches |
| Memoization argument equality | Uses default equality for argument types (reference equality for reference types unless overridden) | ✓ Matches |
| Thread safety | All cache classes use `System.Threading.Lock` for thread-safe operations | ✗ Deviation per Constitution XI |

### Key Entities

- **SimpleCache<TKey, TValue>**: A basic FIFO cache where the cache key can differ from factory function arguments. Used when custom key generation is needed.
  - Internal implementation detail: Uses Dictionary + Queue for O(1) operations

- **FastDictCache<TKey, TValue>**: A high-performance cache optimized for scenarios where the key is the same as factory arguments. Provides dictionary-style indexer access with auto-population.
  - Internal implementation detail: Uses composition (not inheritance) with Dictionary + Queue
  - Default size of 1,000,000 chosen for Char/Document caching where memory is acceptable trade-off for lookup speed

- **Memoization**: A static utility class providing function wrappers that automatically cache results based on arguments.
  - Exactly 3 overloads (1, 2, 3 args) matching Python PTK; no variadic/params support
  - Uses SimpleCache internally for storage

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All cache types correctly evict oldest entries (FIFO) when capacity is exceeded (verified via unit tests with insertion order tracking)
- **SC-002**: Cached value retrieval completes without invoking factory (verified via call count tracking in tests)
- **SC-003**: FastDictCache indexer access for cache hits completes in O(1) time comparable to Dictionary<TKey,TValue> (verified via benchmark: <2x Dictionary lookup time)
- **SC-004**: Memoized functions return identical results to non-memoized versions for all input combinations (verified via equivalence tests)
- **SC-005**: Unit test line coverage reaches 80% or higher for all cache classes (measured via `dotnet test --collect:"XPlat Code Coverage"`)
- **SC-006**: All public APIs match Python Prompt Toolkit semantics as defined in cache.py (verified via API surface comparison and behavioral tests)

## Implementation Constraints

| Constraint | Requirement | Rationale |
|------------|-------------|-----------|
| Namespace | `Stroke.Core` | Per `docs/api-mapping.md` mapping |
| Thread Safety | Thread-safe via `System.Threading.Lock` | FR-016; Constitution Principle XI |
| Inheritance | All classes `sealed` | FR-024; not designed for extension |
| IDisposable | NOT implemented | FR-025; no unmanaged resources |
| XML Documentation | Required on all public types and members | Per Constitution Technical Standards |
| File Location | `src/Stroke/Core/` | Per project structure |

## Assumptions

- Cache classes are thread-safe for individual operations; compound operations (read-modify-write) require external coordination
- The getValue/getterFunc factory functions are deterministic for correct memoization behavior (non-deterministic factories will produce correct but potentially unexpected caching)
- Keys implement proper equality and hashing (`GetHashCode`/`Equals` for dictionary storage); incorrect implementations will cause cache misses or incorrect hits
- Null values are valid cache entries (distinguishable from "not cached")
- Reference type arguments to memoized functions use reference equality by default; callers requiring value equality must ensure arguments override `Equals`/`GetHashCode`

## API Summary

### SimpleCache<TKey, TValue>

```
Constructor: SimpleCache(int maxSize = 8)
Properties:  MaxSize (int, read-only)
Methods:     Get(TKey key, Func<TValue> getter) → TValue
             Clear() → void
Constraints: where TKey : notnull
```

### FastDictCache<TKey, TValue>

```
Constructor: FastDictCache(Func<TKey, TValue> getValue, int size = 1_000_000)
Properties:  Size (int, read-only), Count (int, read-only)
Indexer:     this[TKey key] { get; } → TValue (auto-populates)
Methods:     ContainsKey(TKey key) → bool
             TryGetValue(TKey key, out TValue value) → bool
Constraints: where TKey : notnull
```

### Memoization

```
Static Methods:
  Memoize<T1, TResult>(Func<T1, TResult> func, int maxSize = 1024) → Func<T1, TResult>
    where T1 : notnull
  Memoize<T1, T2, TResult>(Func<T1, T2, TResult> func, int maxSize = 1024) → Func<T1, T2, TResult>
    where T1 : notnull where T2 : notnull
  Memoize<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, int maxSize = 1024) → Func<T1, T2, T3, TResult>
    where T1 : notnull where T2 : notnull where T3 : notnull
```
