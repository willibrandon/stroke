# Tasks: Utilities

**Input**: Design documents from `/specs/024-utilities/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Included per Constitution VIII (target 80% coverage) and spec.md success criteria SC-005, SC-007, SC-008.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

```text
src/Stroke/Core/           # Source files
tests/Stroke.Tests/Core/   # Test files
```

---

## Phase 1: Setup

**Purpose**: Verify project structure and dependencies are ready

- [x] T001 Verify Wcwidth NuGet package (v4.0.1) is referenced in src/Stroke/Stroke.csproj
- [x] T002 Verify src/Stroke/Core/ directory exists for utility classes
- [x] T003 Verify tests/Stroke.Tests/Core/ directory exists for test files

---

## Phase 2: Foundational

**Purpose**: No foundational tasks required. All utilities are independent classes with no shared infrastructure beyond the existing project structure.

**Checkpoint**: Ready to proceed with user stories.

---

## Phase 3: User Story 1 - Event-Based Component Communication (Priority: P1) MVP

**Goal**: Implement Event<TSender> class enabling pub/sub communication with += and -= operators.

**Independent Test**: Create an Event, attach handlers, fire the event, verify handlers are called in order with the correct sender.

### Tests for User Story 1

- [x] T004 [P] [US1] Create EventTests.cs in tests/Stroke.Tests/Core/EventTests.cs with tests for:
  - Constructor with sender only
  - Constructor with sender and initial handler
  - AddHandler adds handler to list
  - RemoveHandler removes handler from list
  - Fire() calls handlers in order added
  - Fire() passes sender to each handler
  - Operator += adds handler (returns same instance)
  - Operator -= removes handler (returns same instance)
  - Same handler added twice is called twice
  - RemoveHandler for non-existent handler is silently ignored
  - AddHandler with null throws ArgumentNullException
  - Operator += with null throws ArgumentNullException
  - Fire() with zero handlers completes successfully (no-op)
  - Handler throws exception: propagates, stops further handlers
  - Handler removes itself during Fire(): removal deferred to next Fire()
  - Handler adds new handler during Fire(): new handler called on next Fire()

### Implementation for User Story 1

- [x] T005 [US1] Implement Event<TSender> class in src/Stroke/Core/Event.cs per Event.md contract:
  - Private `_sender` field (TSender)
  - Private `_handlers` list (List<Action<TSender>>)
  - `Sender` property (readonly)
  - Constructor(TSender sender, Action<TSender>? handler = null)
  - `AddHandler(Action<TSender> handler)` - throws ArgumentNullException for null
  - `RemoveHandler(Action<TSender> handler)` - silently ignores non-existent
  - `Fire()` - iterates snapshot of handlers, passes sender
  - `operator +(Event<TSender> e, Action<TSender> handler)` - calls AddHandler, returns e
  - `operator -(Event<TSender> e, Action<TSender> handler)` - calls RemoveHandler, returns e
- [x] T006 [US1] Run EventTests and verify all tests pass (target: 100% coverage for Event.cs)

**Checkpoint**: Event class fully functional and tested.

---

## Phase 4: User Story 2 - Measure Display Width for CJK Characters (Priority: P1)

**Goal**: Implement UnicodeWidth utility for terminal display width calculation with caching.

**Independent Test**: Measure strings with ASCII, CJK, and control characters; verify widths and cache behavior.

### Tests for User Story 2

- [x] T007 [P] [US2] Create UnicodeWidthTests.cs in tests/Stroke.Tests/Core/UnicodeWidthTests.cs with tests for:
  - GetWidth(char) returns 1 for ASCII letter ('A')
  - GetWidth(char) returns 2 for CJK character ('中')
  - GetWidth(char) returns 0 for control character ('\x1b')
  - GetWidth(char) returns 0 for null character ('\0')
  - GetWidth(char) returns 0 for combining mark ('\u0301')
  - GetWidth(string) returns 0 for empty string
  - GetWidth(string) returns 0 for null string
  - GetWidth(string) returns string.Length for ASCII-only string
  - GetWidth(string) correctly sums mixed ASCII/CJK widths ("Hello世界" = 9)
  - GetWidth(string) caching: same string returns cached result
  - String of exactly 64 characters is cached indefinitely (short string)
  - String of 65 characters is classified as long string
  - 17th long string evicts the oldest long string (FIFO)
  - Thread safety: concurrent GetWidth calls succeed without errors

### Implementation for User Story 2

- [x] T008 [P] [US2] Implement StringWidthCache internal class in src/Stroke/Core/UnicodeWidth.cs:
  - Constants: LongStringMinLength = 64, MaxLongStrings = 16
  - Private Lock `_lock` for thread safety
  - Private Dictionary<string, int> `_cache`
  - Private Queue<string> `_longStrings` for FIFO eviction
  - `GetWidth(string text)` method with locking and eviction logic
- [x] T009 [US2] Implement UnicodeWidth static class in src/Stroke/Core/UnicodeWidth.cs:
  - Private static `_cache` (StringWidthCache instance)
  - `GetWidth(char c)` - uses Wcwidth, converts -1 to 0
  - `GetWidth(string? text)` - returns 0 for null/empty, delegates to cache
- [x] T010 [US2] Run UnicodeWidthTests and verify all tests pass (target: 100% coverage)

**Checkpoint**: UnicodeWidth fully functional with thread-safe caching.

---

## Phase 5: User Story 3 - Detect Runtime Platform (Priority: P2)

**Goal**: Implement PlatformUtils for cross-platform OS and environment detection.

**Independent Test**: Check platform properties and verify consistency (exactly one of IsWindows/IsMacOS/IsLinux is true on supported platforms).

### Tests for User Story 3

- [x] T011 [P] [US3] Create PlatformUtilsTests.cs in tests/Stroke.Tests/Core/PlatformUtilsTests.cs with tests for:
  - Exactly one of IsWindows/IsMacOS/IsLinux is true (on supported platforms)
  - SuspendToBackgroundSupported returns true on Unix, false on Windows
  - GetTermEnvironmentVariable() returns TERM value or empty string
  - IsDumbTerminal() returns true for "dumb" (case-insensitive)
  - IsDumbTerminal() returns true for "unknown" (case-insensitive)
  - IsDumbTerminal() returns false for other values
  - IsDumbTerminal() returns false when TERM is not set
  - IsDumbTerminal(term) parameter overrides environment variable
  - IsConEmuAnsi returns true only for "ON" (case-sensitive)
  - IsConEmuAnsi returns false for "on", "On", "OFF", etc.
  - IsConEmuAnsi returns false when ConEmuANSI is not set
  - InMainThread returns true when called from main thread
  - GetBellEnvironmentVariable() returns true by default (STROKE_BELL not set)
  - GetBellEnvironmentVariable() returns true for "true", "TRUE", "1"
  - GetBellEnvironmentVariable() returns false for other values

### Implementation for User Story 3

- [x] T012 [US3] Implement PlatformUtils static class in src/Stroke/Core/PlatformUtils.cs per PlatformUtils.md contract:
  - `IsWindows` property (RuntimeInformation.IsOSPlatform)
  - `IsMacOS` property (RuntimeInformation.IsOSPlatform)
  - `IsLinux` property (RuntimeInformation.IsOSPlatform)
  - `SuspendToBackgroundSupported` property (!IsWindows)
  - `GetTermEnvironmentVariable()` method (Environment.GetEnvironmentVariable)
  - `IsDumbTerminal(string? term = null)` method (case-insensitive "dumb"/"unknown")
  - `IsConEmuAnsi` property (case-sensitive "ON" check)
  - `InMainThread` property (Thread.CurrentThread.ManagedThreadId == 1)
  - `GetBellEnvironmentVariable()` method (STROKE_BELL check, defaults true)
- [x] T013 [US3] Run PlatformUtilsTests and verify all tests pass (target: >80% coverage)

**Checkpoint**: PlatformUtils fully functional for cross-platform detection.

---

## Phase 6: User Story 4 - Distribute Items by Weight (Priority: P3)

**Goal**: Implement TakeUsingWeights for proportional item distribution.

**Independent Test**: Take N items from generator and verify distribution matches weight proportions within tolerance.

### Tests for User Story 4

- [x] T014 [P] [US4] Create CollectionUtilsTests.cs in tests/Stroke.Tests/Core/CollectionUtilsTests.cs with tests for:
  - TakeUsingWeights with [1,2,4] weights distributes proportionally (within 5% for 100+ items)
  - Single item with positive weight yields that item infinitely
  - Equal weights yield round-robin distribution
  - Zero-weight items are filtered out
  - Negative weights are treated as zero (filtered out)
  - Empty items list throws ArgumentException
  - Null items throws ArgumentNullException
  - Null weights throws ArgumentNullException
  - Mismatched items/weights lengths throws ArgumentException
  - All zero weights throws ArgumentException
  - All negative weights throws ArgumentException (treated as all zero)
  - Mixed positive/zero/negative weights: only positive used
  - Generator is infinite (can take any number of items)
  - Thread safety: returns new iterator per call

### Implementation for User Story 4

- [x] T015 [US4] Implement CollectionUtils.TakeUsingWeights<T> in src/Stroke/Core/CollectionUtils.cs per CollectionUtils.md contract:
  - Validate inputs (ArgumentNullException, ArgumentException)
  - Filter items with weight > 0
  - Throw if no positive weights
  - Implement fill-based algorithm from research.md (R4)
  - Use yield return for lazy evaluation
- [x] T016 [US4] Run CollectionUtilsTests and verify all tests pass (target: >80% coverage)

**Checkpoint**: CollectionUtils fully functional for weighted distribution.

---

## Phase 7: User Story 5 - Convert Lazy Values (Priority: P3)

**Goal**: Implement ConversionUtils and AnyFloat for lazy value normalization.

**Independent Test**: Pass various value types (direct, callable, null) and verify correct conversion.

### Tests for User Story 5

- [x] T017 [P] [US5] Create ConversionUtilsTests.cs in tests/Stroke.Tests/Core/ConversionUtilsTests.cs with tests for:
  - ToStr(string) returns the string
  - ToStr(null string) returns ""
  - ToStr(Func<string>) invokes and returns result
  - ToStr(Func<Func<string>>) recursively unwraps
  - ToStr(object) calls ToString()
  - ToStr(object with null ToString()) returns ""
  - ToInt(int) returns the int
  - ToInt(Func<int>) invokes and returns result
  - ToInt(null Func<int>) returns 0
  - ToInt(object) uses Convert.ToInt32, returns 0 on failure
  - ToFloat(double) returns the double
  - ToFloat(Func<double>) invokes and returns result
  - ToFloat(null Func<double>) returns 0.0
  - ToFloat(AnyFloat) extracts value
  - ToFloat(object) uses Convert.ToDouble, returns 0.0 on failure
- [x] T018 [P] [US5] Create AnyFloatTests.cs in tests/Stroke.Tests/Core/AnyFloatTests.cs with tests for:
  - Implicit conversion from double
  - Implicit conversion from Func<double>
  - Explicit conversion to double
  - Value property returns concrete value
  - Value property invokes callable (each time, not cached)
  - HasValue is true for concrete value
  - HasValue is true for callable
  - HasValue is false for default(AnyFloat)
  - Equality: two concrete values compare by value
  - Equality: two callables compare by reference
  - Equality: concrete vs callable is not equal
  - GetHashCode consistency with Equals
  - Operators == and !=

### Implementation for User Story 5

- [x] T019 [P] [US5] Implement AnyFloat struct in src/Stroke/Core/ConversionUtils.cs per ConversionUtils.md contract:
  - Private `_value` (double?)
  - Private `_getter` (Func<double>?)
  - `Value` property (returns _value ?? _getter?.Invoke() ?? 0.0)
  - `HasValue` property
  - Implicit operators from double and Func<double>
  - Explicit operator to double
  - IEquatable<AnyFloat> implementation
  - GetHashCode, Equals, ==, != operators
- [x] T020 [US5] Implement ConversionUtils static class in src/Stroke/Core/ConversionUtils.cs per ConversionUtils.md contract:
  - ToStr overloads: string?, Func<string?>?, Func<Func<string?>?>?, object?
  - ToInt overloads: int, Func<int>?, object?
  - ToFloat overloads: double, Func<double>?, AnyFloat, object?
  - Recursive unwrapping for nested callables
- [x] T021 [US5] Run ConversionUtilsTests and AnyFloatTests, verify all tests pass (target: >80% coverage)

**Checkpoint**: ConversionUtils and AnyFloat fully functional.

---

## Phase 8: DummyContext (No User Story - Supporting Utility)

**Goal**: Implement DummyContext singleton for no-op disposable scenarios.

### Tests for DummyContext

- [x] T022 [P] Create DummyContextTests.cs in tests/Stroke.Tests/Core/DummyContextTests.cs with tests for:
  - Instance property returns singleton
  - Multiple accesses return same instance
  - Dispose() completes without error
  - Can be used in using statement

### Implementation for DummyContext

- [x] T023 Implement DummyContext in src/Stroke/Core/DummyContext.cs per DummyContext.md contract:
  - Private constructor
  - Static Instance property (singleton)
  - No-op Dispose() method
  - Implements IDisposable
- [x] T024 Run DummyContextTests and verify all tests pass (target: 100% coverage)

**Checkpoint**: DummyContext complete.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and documentation

- [x] T025 Run all utility tests together, verify >80% overall coverage
- [x] T026 Verify all 33 functional requirements (FR-001 through FR-033) have test coverage
- [x] T027 Verify edge cases from spec.md are covered by tests
- [x] T028 Run quickstart.md code examples to verify they work
- [x] T029 Verify thread safety for UnicodeWidth with concurrent test

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - verify prerequisites
- **Foundational (Phase 2)**: N/A (no foundational tasks)
- **User Stories (Phase 3-7)**: Can proceed in any order after Setup; no inter-story dependencies
- **DummyContext (Phase 8)**: Can run in parallel with any user story
- **Polish (Phase 9)**: Depends on all previous phases

### User Story Dependencies

All user stories are independent:

- **US1 (Event)**: No dependencies
- **US2 (UnicodeWidth)**: No dependencies (Wcwidth package already in project)
- **US3 (PlatformUtils)**: No dependencies
- **US4 (CollectionUtils)**: No dependencies
- **US5 (ConversionUtils)**: No dependencies
- **DummyContext**: No dependencies

### Within Each User Story

1. Tests written first (tasks marked [P] within story can run in parallel)
2. Implementation follows
3. Test verification confirms completion

### Parallel Opportunities

Setup tasks can all run in parallel:
- T001, T002, T003 (different verification targets)

All user story test files can be written in parallel:
- T004 (EventTests), T007 (UnicodeWidthTests), T011 (PlatformUtilsTests), T014 (CollectionUtilsTests), T017+T018 (ConversionUtils/AnyFloat tests), T022 (DummyContextTests)

Implementation files are independent (different source files):
- T005 (Event.cs), T008+T009 (UnicodeWidth.cs), T012 (PlatformUtils.cs), T015 (CollectionUtils.cs), T019+T020 (ConversionUtils.cs), T023 (DummyContext.cs)

---

## Parallel Example: All Test Files

```text
# Launch all test file creation in parallel:
T004: Create EventTests.cs
T007: Create UnicodeWidthTests.cs
T011: Create PlatformUtilsTests.cs
T014: Create CollectionUtilsTests.cs
T017: Create ConversionUtilsTests.cs
T018: Create AnyFloatTests.cs
T022: Create DummyContextTests.cs
```

## Parallel Example: All Implementation Files

```text
# After tests exist, launch all implementations in parallel:
T005: Implement Event.cs
T008+T009: Implement UnicodeWidth.cs (StringWidthCache + UnicodeWidth)
T012: Implement PlatformUtils.cs
T015: Implement CollectionUtils.cs
T019+T020: Implement ConversionUtils.cs (AnyFloat + ConversionUtils)
T023: Implement DummyContext.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup verification
2. Complete Phase 3: User Story 1 (Event class)
3. **STOP and VALIDATE**: Test Event independently
4. Event class is foundational for other Stroke components

### Incremental Delivery

1. Setup → Event (MVP) → Ready for component integration
2. Add UnicodeWidth → Ready for rendering
3. Add PlatformUtils → Ready for cross-platform code
4. Add CollectionUtils → Ready for scheduling
5. Add ConversionUtils → Ready for flexible APIs
6. Add DummyContext → Complete utility set

### Recommended Order (Single Developer)

Given no inter-story dependencies, work in priority order:
1. US1 (Event) - P1, foundational for other components
2. US2 (UnicodeWidth) - P1, required for rendering
3. US3 (PlatformUtils) - P2, enables cross-platform
4. US4 (CollectionUtils) - P3
5. US5 (ConversionUtils) - P3
6. DummyContext - Simple supporting utility

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story
- Each user story is independently completable and testable
- All utilities go in Stroke.Core namespace
- Target 80% test coverage per Constitution VIII and SC-005
- All 33 FRs must have test coverage per SC-007
- Edge cases from spec must have test coverage per SC-008
