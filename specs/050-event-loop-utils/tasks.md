# Tasks: Event Loop Utilities

**Input**: Design documents from `/specs/050-event-loop-utils/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are included — the spec requires 80% coverage (SC-009) and the plan scopes ~300 LOC of tests.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Create project structure and namespace directory

- [ ] T001 Create `src/Stroke/EventLoop/` directory for the `Stroke.EventLoop` namespace
- [ ] T002 Create `tests/Stroke.Tests/EventLoop/` directory for test files

---

## Phase 2: User Story 1 — Context-Preserving Background Execution (Priority: P1) MVP

**Goal**: Offload CPU-bound or blocking operations to a background thread while preserving `ExecutionContext` (`AsyncLocal<T>` values like `AppContext.GetApp()`). Equivalent to Python's `run_in_executor_with_context`.

**Independent Test**: Set an `AsyncLocal<T>` value, dispatch to background via `RunInExecutorWithContextAsync`, assert the value is visible inside the background task. Also verify void overload, cancellation, exception propagation, and null validation.

### Tests for User Story 1

- [ ] T003 [P] [US1] Write tests for `RunInExecutorWithContextAsync<T>` context preservation (set `AsyncLocal<T>`, dispatch, assert visible) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T004 [P] [US1] Write tests for `RunInExecutorWithContextAsync<T>` result return (value-returning `Func<T>` returns correct `Task<T>`) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T005 [P] [US1] Write tests for `RunInExecutorWithContextAsync` void overload (`Action` completes `Task`) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T006 [P] [US1] Write tests for cancellation behavior (before dispatch and during execution) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T007 [P] [US1] Write tests for exception propagation through returned `Task` (FR-016) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T008 [P] [US1] Write tests for null argument validation (`ArgumentNullException` for null `func`/`action`) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T009 [P] [US1] Write test for suppressed `ExecutionContext` flow (capture returns null, function still executes) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`

### Implementation for User Story 1

- [ ] T010 [US1] Implement `RunInExecutorWithContextAsync<T>(Func<T>, CancellationToken)` in `src/Stroke/EventLoop/EventLoopUtils.cs` — capture `ExecutionContext`, dispatch via `Task.Run`, restore context via `ExecutionContext.Run`, handle null capture (FR-001, FR-002, FR-003, FR-012, FR-016)
- [ ] T011 [US1] Implement `RunInExecutorWithContextAsync(Action, CancellationToken)` void overload in `src/Stroke/EventLoop/EventLoopUtils.cs` — delegates to generic version or uses parallel implementation with `Action` (FR-002)

**Checkpoint**: `RunInExecutorWithContextAsync` tests pass. Context flows correctly to background threads.

---

## Phase 3: User Story 2 — Thread-Safe Callback Scheduling with Deadline (Priority: P2)

**Goal**: Schedule callbacks for thread-safe execution on the current `SynchronizationContext`, with optional deadline-based coalescing via re-posting. Equivalent to Python's `call_soon_threadsafe`.

**Independent Test**: Use a custom `SynchronizationContext` in tests. Schedule callbacks with and without deadlines; verify execution timing, coalescing behavior, fallback for null sync context, reentrancy, and edge cases.

### Tests for User Story 2

- [ ] T012 [P] [US2] Write test for no-deadline scheduling (posts via `SynchronizationContext.Post`, executes when queue processes) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T013 [P] [US2] Write test for null `SynchronizationContext` fallback (executes immediately on calling thread) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`. Note: this also covers FR-015 (disposed/stopped sync context) — in .NET, a disposed sync context manifests as `SynchronizationContext.Current` being null or `Post` throwing, both of which trigger the immediate-execution fallback
- [ ] T014 [P] [US2] Write test for deadline scheduling — idle sync context (callback runs on first re-post cycle) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T015 [P] [US2] Write test for deadline scheduling — busy sync context (callback defers until deadline expires, within 50ms tolerance) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`. Test approach: use a custom `SynchronizationContext` that queues posted callbacks and processes them on demand; post multiple no-op work items before the deadline callback to simulate contention, then pump the queue and verify the callback executes only after the deadline
- [ ] T016 [P] [US2] Write test for zero/negative `maxPostponeTime` (executes immediately, deadline already expired) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T017 [P] [US2] Write test for `TimeSpan.MaxValue` / overflow `maxPostponeTime` — verify deadline clamps to `long.MaxValue` (no overflow), callback still executes via re-post pattern in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T018 [P] [US2] Write test for null `action` argument (`ArgumentNullException`) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T019 [P] [US2] Write test for reentrancy — callback calls `CallSoonThreadSafe` without deadlock (FR-014) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T020 [P] [US2] Write test for exception propagation through sync context (FR-013) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T020b [P] [US2] Write test for multiple rapid `CallSoonThreadSafe` calls with the same deadline — verify each callback is independently scheduled with no deduplication (Edge Case 5) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`

### Implementation for User Story 2

- [ ] T021 [US2] Implement `CallSoonThreadSafe(Action, TimeSpan?)` in `src/Stroke/EventLoop/EventLoopUtils.cs` — null sync context fallback, no-deadline direct post, deadline re-post loop using `Environment.TickCount64`, zero/negative deadline handling, overflow clamping (FR-004 through FR-008, FR-012 through FR-015)

**Checkpoint**: `CallSoonThreadSafe` tests pass. Deadline coalescing works. Null sync context falls back correctly.

---

## Phase 4: User Story 3 — Exception Traceback Extraction (Priority: P3)

**Goal**: Extract a stack trace string from an exception context dictionary. Equivalent to Python's `get_traceback_from_context`.

**Independent Test**: Construct context dictionaries with various entries (exception with trace, no exception, non-exception value, null StackTrace) and verify correct return values.

### Tests for User Story 3

- [ ] T022 [P] [US3] Write test for exception with non-null `StackTrace` — returns stack trace string in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T023 [P] [US3] Write test for missing "exception" key — returns null in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T024 [P] [US3] Write test for non-`Exception` value under "exception" key — returns null in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T025 [P] [US3] Write test for exception with null `StackTrace` (never thrown) — returns null in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T026 [P] [US3] Write test for null `context` argument (`ArgumentNullException`) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`

### Implementation for User Story 3

- [ ] T027 [US3] Implement `GetTracebackFromContext(IDictionary<string, object?>)` in `src/Stroke/EventLoop/EventLoopUtils.cs` — look up "exception" key, check `Exception` type with non-null `StackTrace`, return `string?` (FR-009, FR-012)

**Checkpoint**: `GetTracebackFromContext` tests pass. All dictionary scenarios handled correctly.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Thread safety stress tests, XML documentation, and final validation

- [ ] T028 [P] Write concurrency stress test — 10 threads × 1000 iterations per method, call `RunInExecutorWithContextAsync`, `CallSoonThreadSafe`, and `GetTracebackFromContext` concurrently, assert no deadlocks, data corruption, or unhandled exceptions (SC-008) in `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs`
- [ ] T029 [P] Add XML documentation comments to all public types and members in `src/Stroke/EventLoop/EventLoopUtils.cs` (FR-011) — per contracts/event-loop-utils.md
- [ ] T030 Run `dotnet build src/Stroke/` — verify clean compilation with no warnings
- [ ] T031 Run `dotnet test tests/Stroke.Tests/ --filter "FullyQualifiedName~EventLoop"` — verify all tests pass
- [ ] T032 Run full regression `dotnet test` — verify no existing tests broken
- [ ] T033 Verify test coverage meets 80% threshold for `EventLoopUtils.cs` (SC-009)
- [ ] T034 [P] Write performance smoke test — verify `ExecutionContext.Capture`/`Run` and `SynchronizationContext.Post` overhead is within expected range (SC-010). Use `Stopwatch` to measure 10,000 iterations and assert mean time per call is under 10μs (generous bound to avoid flaky tests; the <1μs target is an implementation-level goal validated by inspection, not a hard test gate)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **US1 (Phase 2)**: Depends on Setup — creates the static class file and first methods
- **US2 (Phase 3)**: Depends on Setup — uses the same file but implements independent method
- **US3 (Phase 4)**: Depends on Setup — uses the same file but implements independent method
- **Polish (Phase 5)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Setup — no dependencies on other stories
- **User Story 2 (P2)**: Can start after Setup — no dependencies on US1 (different method, same file)
- **User Story 3 (P3)**: Can start after Setup — no dependencies on US1 or US2

**Note**: Since all methods live in a single file (`EventLoopUtils.cs`), the user stories are best implemented sequentially (US1 → US2 → US3) to avoid merge conflicts. However, test tasks within each story are fully parallelizable.

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Null validation tests before core logic tests
- Implementation follows test structure
- Story complete before moving to next priority

### Parallel Opportunities

- All test tasks within a story marked [P] can run in parallel (they're in the same file but independent test methods)
- US1 tests (T003-T009) can all run in parallel
- US2 tests (T012-T020b) can all run in parallel
- US3 tests (T022-T026) can all run in parallel
- Polish tasks T028, T029, and T034 can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: T003 "Context preservation test"
Task: T004 "Result return test"
Task: T005 "Void overload test"
Task: T006 "Cancellation test"
Task: T007 "Exception propagation test"
Task: T008 "Null validation test"
Task: T009 "Suppressed flow test"

# Then implement sequentially:
Task: T010 "Implement RunInExecutorWithContextAsync<T>"
Task: T011 "Implement void overload"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 2: User Story 1 tests + implementation (T003-T011)
3. **STOP and VALIDATE**: Run `dotnet test --filter "FullyQualifiedName~EventLoop"` — US1 tests pass
4. Context preservation across threads is proven

### Incremental Delivery

1. Setup → Ready
2. Add User Story 1 (RunInExecutorWithContextAsync) → Test → Validate (MVP!)
3. Add User Story 2 (CallSoonThreadSafe) → Test → Validate
4. Add User Story 3 (GetTracebackFromContext) → Test → Validate
5. Polish (stress tests, docs, coverage) → Final validation
6. Each story adds independent value without breaking previous stories

### Build Sequence (from quickstart.md)

Within implementation, follow the quickstart build order:
1. `GetTracebackFromContext` — simplest, no dependencies (US3 could go first by complexity, but P1 priority drives order)
2. `RunInExecutorWithContextAsync<T>` — core context preservation
3. `RunInExecutorWithContextAsync` void overload — delegates to generic
4. `CallSoonThreadSafe` — most complex (deadline scheduling)

**Recommended**: Follow priority order (US1 → US2 → US3) per spec, but within US1 implementation, the quickstart's build order is already followed.

---

## Notes

- [P] tasks = different files or independent test methods, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Single implementation file: `src/Stroke/EventLoop/EventLoopUtils.cs` (~120 LOC)
- Single test file: `tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs` (~300 LOC)
- No new NuGet packages required — BCL only
- No changes to existing files required
- All tests use real `ExecutionContext`, real `SynchronizationContext`, real thread pool — no mocks (Constitution VIII)
