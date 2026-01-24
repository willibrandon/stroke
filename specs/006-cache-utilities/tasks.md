# Tasks: Cache Utilities

**Input**: Design documents from `/specs/006-cache-utilities/`
**Prerequisites**: plan.md, spec.md, data-model.md, research.md, quickstart.md

**Tests**: Included per Constitution VIII (80% coverage target) and SC-005.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Per plan.md project structure:
- **Source**: `src/Stroke/Core/`
- **Tests**: `tests/Stroke.Tests/Core/`

---

## Phase 1: Setup

**Purpose**: Verify project structure and namespace placement

- [ ] T001 Verify `src/Stroke/Core/` directory exists for cache implementations
- [ ] T002 Verify `tests/Stroke.Tests/Core/` directory exists for cache tests
- [ ] T003 Confirm `Stroke.Core` namespace is used per api-mapping.md

---

## Phase 2: Foundational

**Purpose**: No foundational infrastructure required - cache utilities are self-contained with zero external dependencies

**⚠️ Note**: This feature has no blocking prerequisites. User stories can begin immediately after Setup.

**Checkpoint**: Setup verified - user story implementation can begin

---

## Phase 3: User Story 1 - SimpleCache (Priority: P1) 🎯 MVP

**Goal**: Provide a basic FIFO cache where cache key can differ from factory function arguments

**Independent Test**: Create a SimpleCache, add entries via Get(), verify retrieval returns cached values, confirm eviction removes oldest entry when maxSize exceeded, verify Clear() removes all entries

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T004 [P] [US1] Create test file `tests/Stroke.Tests/Core/SimpleCacheTests.cs` with test class structure
- [ ] T005 [P] [US1] Add constructor tests: default maxSize=8, custom maxSize, ArgumentOutOfRangeException for maxSize≤0
- [ ] T006 [P] [US1] Add Get() tests: getter invoked for missing key, cached value returned for existing key, null getter throws ArgumentNullException
- [ ] T007 [P] [US1] Add FIFO eviction tests: oldest entry (first inserted) evicted when Count > MaxSize, eviction order verified
- [ ] T008 [P] [US1] Add Clear() tests: all entries removed, cache can be reused after clear
- [ ] T009 [P] [US1] Add edge case tests: maxSize=1 single-entry cache, null value caching, exception propagation from getter, duplicate key retrieval
- [ ] T010 [P] [US1] Add concurrent stress tests: 10+ threads, 1000+ Get operations, verify no exceptions or data corruption (per Constitution XI, FR-026)

### Implementation for User Story 1

- [ ] T011 [US1] Create `src/Stroke/Core/SimpleCache.cs` with class skeleton, XML documentation, and `private readonly Lock _lock = new()`
- [ ] T012 [US1] Implement constructor with maxSize validation (ArgumentOutOfRangeException if ≤0), initialize Dictionary and Queue
- [ ] T013 [US1] Implement MaxSize read-only property
- [ ] T014 [US1] Implement Get(TKey key, Func<TValue> getter) method with null getter validation, cache lookup, getter invocation, and caching - wrap in `using (_lock.EnterScope())`
- [ ] T015 [US1] Implement FIFO eviction logic in Get() - when Count > MaxSize, dequeue oldest key and remove from dictionary (within lock scope)
- [ ] T016 [US1] Implement Clear() method to reset both dictionary and queue - wrap in `using (_lock.EnterScope())`
- [ ] T017 [US1] Add `where TKey : notnull` generic constraint and `sealed` class modifier

**Checkpoint**: SimpleCache fully functional, thread-safe, and independently testable. Run `dotnet test --filter "FullyQualifiedName~SimpleCacheTests"` to verify (includes concurrent stress tests).

---

## Phase 4: User Story 2 - FastDictCache (Priority: P1)

**Goal**: Provide a high-performance cache optimized for scenarios where the key IS the factory argument, with dictionary-style indexer access

**Independent Test**: Create a FastDictCache with factory function, access key via indexer to trigger factory, verify subsequent access returns cached value, confirm eviction at capacity, verify ContainsKey and TryGetValue work without invoking factory

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T018 [P] [US2] Create test file `tests/Stroke.Tests/Core/FastDictCacheTests.cs` with test class structure
- [ ] T019 [P] [US2] Add constructor tests: default size=1,000,000, custom size, ArgumentOutOfRangeException for size≤0, ArgumentNullException for null getValue
- [ ] T020 [P] [US2] Add indexer tests: factory invoked with key for missing entry, cached value returned for existing entry, factory receives correct key
- [ ] T021 [P] [US2] Add FIFO eviction tests: oldest entry evicted BEFORE adding new entry when Count > Size, eviction order verified
- [ ] T022 [P] [US2] Add ContainsKey tests: returns true for cached keys, returns false for missing keys, does NOT invoke factory
- [ ] T023 [P] [US2] Add TryGetValue tests: returns true and value for cached keys, returns false for missing keys, does NOT invoke factory
- [ ] T024 [P] [US2] Add property tests: Size returns configured maximum, Count returns actual entry count
- [ ] T025 [P] [US2] Add edge case tests: size=1 single-entry cache, null value from factory, exception propagation from factory
- [ ] T026 [P] [US2] Add concurrent stress tests: 10+ threads, 1000+ indexer accesses, verify no exceptions or data corruption (per Constitution XI, FR-026)

### Implementation for User Story 2

- [ ] T027 [US2] Create `src/Stroke/Core/FastDictCache.cs` with class skeleton, XML documentation, and `private readonly Lock _lock = new()`
- [ ] T028 [US2] Implement constructor with size validation (ArgumentOutOfRangeException if ≤0), getValue validation (ArgumentNullException if null), initialize Dictionary, Queue, and store factory
- [ ] T029 [US2] Implement Size read-only property (maximum capacity)
- [ ] T030 [US2] Implement Count read-only property (current entry count from dictionary) - wrap in `using (_lock.EnterScope())`
- [ ] T031 [US2] Implement read-only indexer `this[TKey key]` with cache lookup, FIFO eviction when Count > Size, factory invocation, and caching - wrap in `using (_lock.EnterScope())`
- [ ] T032 [US2] Implement ContainsKey(TKey key) delegating to internal dictionary - wrap in `using (_lock.EnterScope())`
- [ ] T033 [US2] Implement TryGetValue(TKey key, out TValue value) delegating to internal dictionary - wrap in `using (_lock.EnterScope())`
- [ ] T034 [US2] Add `where TKey : notnull` generic constraint and `sealed` class modifier

**Checkpoint**: FastDictCache fully functional, thread-safe, and independently testable. Run `dotnet test --filter "FullyQualifiedName~FastDictCacheTests"` to verify (includes concurrent stress tests).

---

## Phase 5: User Story 3 - Memoization (Priority: P2)

**Goal**: Provide static utility methods to wrap functions with automatic result caching based on arguments

**Depends On**: User Story 1 (SimpleCache) - Memoization uses SimpleCache internally

**Independent Test**: Memoize functions with 1, 2, and 3 arguments, call with same arguments multiple times and verify original function called only once, verify different arguments are cached separately, verify eviction at maxSize

### Tests for User Story 3

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T035 [P] [US3] Create test file `tests/Stroke.Tests/Core/MemoizationTests.cs` with test class structure
- [ ] T036 [P] [US3] Add validation tests: ArgumentNullException for null func, ArgumentOutOfRangeException for maxSize≤0 (all overloads)
- [ ] T037 [P] [US3] Add single-arg Memoize tests: function called once per unique arg, cached result returned on repeat calls, default maxSize=1024
- [ ] T038 [P] [US3] Add two-arg Memoize tests: function called once per unique (arg1, arg2) combination, ValueTuple key equality verified
- [ ] T039 [P] [US3] Add three-arg Memoize tests: function called once per unique (arg1, arg2, arg3) combination, ValueTuple key equality verified
- [ ] T040 [P] [US3] Add eviction tests: oldest cached result evicted when maxSize exceeded, verified for all overloads
- [ ] T041 [P] [US3] Add equivalence tests: memoized function returns identical results to original function (SC-004)
- [ ] T042 [P] [US3] Add edge case tests: null return value cached, reference type argument equality behavior
- [ ] T043 [P] [US3] Add concurrent stress tests: 10+ threads, 1000+ memoized function calls, verify no exceptions or data corruption (per Constitution XI, FR-026)

### Implementation for User Story 3

- [ ] T044 [US3] Create `src/Stroke/Core/Memoization.cs` with static class skeleton and XML documentation
- [ ] T045 [US3] Implement `Memoize<T1, TResult>(Func<T1, TResult> func, int maxSize = 1024)` with null validation, create SimpleCache<T1, TResult>, return wrapper Func (thread-safe via SimpleCache)
- [ ] T046 [US3] Implement `Memoize<T1, T2, TResult>(Func<T1, T2, TResult> func, int maxSize = 1024)` with null validation, create SimpleCache<(T1, T2), TResult> using ValueTuple key, return wrapper Func
- [ ] T047 [US3] Implement `Memoize<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, int maxSize = 1024)` with null validation, create SimpleCache<(T1, T2, T3), TResult> using ValueTuple key, return wrapper Func
- [ ] T048 [US3] Add `where T1 : notnull`, `where T2 : notnull`, `where T3 : notnull` generic constraints on respective overloads

**Checkpoint**: Memoization fully functional, thread-safe (via SimpleCache), and independently testable. Run `dotnet test --filter "FullyQualifiedName~MemoizationTests"` to verify (includes concurrent stress tests).

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, documentation, and coverage validation

- [ ] T049 Run full test suite: `dotnet test tests/Stroke.Tests/` - all cache tests must pass (including concurrent stress tests)
- [ ] T050 Verify test coverage meets 80% threshold (SC-005): `dotnet test --collect:"XPlat Code Coverage"`
- [ ] T051 [P] Validate quickstart.md examples compile and work correctly
- [ ] T052 [P] Verify XML documentation complete on all public types and members per Constitution Technical Standards
- [ ] T053 [P] Run benchmark to verify FastDictCache indexer <2x Dictionary lookup time (SC-003): 1000 iterations, 100 warm-up, measure p50/p99 latency
- [ ] T054 Final code review: verify all FR-001 through FR-026 requirements implemented, all classes sealed (FR-024), no IDisposable (FR-025)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - No blocking infrastructure for this feature
- **User Story 1 (Phase 3)**: Can start after Setup verification
- **User Story 2 (Phase 4)**: Can start after Setup verification - **PARALLEL with US1**
- **User Story 3 (Phase 5)**: Depends on User Story 1 completion (uses SimpleCache internally)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

```
Setup (Phase 1)
     │
     ├─────────────────────┐
     ▼                     ▼
User Story 1 (P1)    User Story 2 (P1)
SimpleCache          FastDictCache
     │                     │
     ▼                     │
User Story 3 (P2)          │
Memoization               │
     │                     │
     └─────────┬───────────┘
               ▼
        Polish (Phase 6)
```

### Within Each User Story

1. Tests MUST be written and FAIL before implementation (including concurrent stress tests per FR-026)
2. Implementation proceeds: skeleton with Lock → constructor → properties → methods (with lock scopes) → constraints
3. Run story-specific tests to verify completion (including concurrent stress tests)
4. Commit after each task or logical group

### Parallel Opportunities

**Phase 3 + Phase 4 (US1 + US2) are FULLY PARALLEL:**
- SimpleCache and FastDictCache have no dependencies on each other
- Tests (including concurrent stress tests) can be written simultaneously
- Implementation can proceed in parallel

**Within User Story 1:**
- T004-T010 (all tests including concurrent stress) can run in parallel

**Within User Story 2:**
- T018-T026 (all tests including concurrent stress) can run in parallel

**Within User Story 3:**
- T035-T043 (all tests including concurrent stress) can run in parallel

**Phase 6 (Polish):**
- T051, T052, T053 can run in parallel

---

## Parallel Example: User Stories 1 and 2

```bash
# These two user stories can be implemented in parallel:

# Stream A: User Story 1 (SimpleCache)
Task: T004-T010 Write all SimpleCache tests including concurrent stress tests (parallel)
Task: T011-T017 Implement SimpleCache with thread safety (sequential)

# Stream B: User Story 2 (FastDictCache) - CONCURRENT with Stream A
Task: T018-T026 Write all FastDictCache tests including concurrent stress tests (parallel)
Task: T027-T034 Implement FastDictCache with thread safety (sequential)

# After both complete:
# Stream C: User Story 3 (Memoization) - depends on US1
Task: T035-T043 Write all Memoization tests including concurrent stress tests (parallel)
Task: T044-T048 Implement Memoization (sequential, thread-safe via SimpleCache)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (verify directories)
2. Skip Phase 2: No foundational tasks needed
3. Complete Phase 3: User Story 1 (SimpleCache)
4. **STOP and VALIDATE**: Run SimpleCacheTests
5. SimpleCache is usable as standalone component

### Incremental Delivery

1. Complete Setup → Ready to implement
2. Add User Story 1 (SimpleCache) → Test independently → Checkpoint (usable cache)
3. Add User Story 2 (FastDictCache) → Test independently → Checkpoint (high-performance cache)
4. Add User Story 3 (Memoization) → Test independently → Checkpoint (function caching)
5. Polish → Final validation → Feature complete

### Parallel Team Strategy

With two developers:

1. Both verify Setup (Phase 1)
2. Developer A: User Story 1 (SimpleCache)
3. Developer B: User Story 2 (FastDictCache)
4. When US1 complete: Developer A starts User Story 3 (Memoization)
5. When US2 complete: Developer B starts Polish tasks (T048-T050)
6. Both complete Polish review (T046, T047, T051)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- US1 and US2 are both P1 priority but can proceed in parallel
- US3 depends on US1 (Memoization uses SimpleCache internally)
- All cache classes MUST be thread-safe using `System.Threading.Lock` with `EnterScope()` pattern (Constitution XI)
- Concurrent stress tests (10+ threads, 1000+ operations) are REQUIRED for each cache class (FR-026)
