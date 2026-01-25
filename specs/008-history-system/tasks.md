# Tasks: History System

**Input**: Design documents from `/specs/008-history-system/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md
**Branch**: `008-history-system`

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/History/`
- **Tests**: `tests/Stroke.Tests/History/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and interface/base class foundation

- [ ] T001 Update IHistory interface with LoadHistoryStrings and StoreString methods in src/Stroke/History/IHistory.cs
- [ ] T002 Create HistoryBase abstract class with caching (_loaded, _loadedStrings) in src/Stroke/History/HistoryBase.cs
- [ ] T003 Create test project directory structure at tests/Stroke.Tests/History/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core base class implementation that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T004 Implement HistoryBase.LoadAsync with caching logic (yields newest-first) in src/Stroke/History/HistoryBase.cs
- [ ] T005 Implement HistoryBase.GetStrings (returns oldest-first by reversing cache) in src/Stroke/History/HistoryBase.cs
- [ ] T006 Implement HistoryBase.AppendString (inserts at index 0, calls StoreString) in src/Stroke/History/HistoryBase.cs
- [ ] T007 Add thread safety with Lock to HistoryBase._loaded and _loadedStrings in src/Stroke/History/HistoryBase.cs
- [ ] T008 [P] Create HistoryBaseTests validating caching and ordering behavior in tests/Stroke.Tests/History/HistoryBaseTests.cs

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - In-Memory Command Recall (Priority: P1) 🎯 MVP

**Goal**: Session-only history storage with oldest-first GetStrings and newest-first LoadAsync

**Independent Test**: Create InMemoryHistory, append strings, verify retrieval order via GetStrings (oldest-first) and LoadAsync (newest-first)

### Tests for User Story 1

- [ ] T009 [P] [US1] Create InMemoryHistoryTests with basic append/get tests in tests/Stroke.Tests/History/InMemoryHistoryTests.cs
- [ ] T010 [P] [US1] Add test for pre-populated constructor (history_strings parameter) in tests/Stroke.Tests/History/InMemoryHistoryTests.cs
- [ ] T011 [P] [US1] Add test for loading order (LoadAsync yields newest-first) in tests/Stroke.Tests/History/InMemoryHistoryTests.cs
- [ ] T012 [P] [US1] Add thread safety concurrent access test (10+ threads, 1000+ operations) in tests/Stroke.Tests/History/InMemoryHistoryTests.cs

### Implementation for User Story 1

- [ ] T013 [US1] Refactor InMemoryHistory to extend HistoryBase in src/Stroke/History/InMemoryHistory.cs
- [ ] T014 [US1] Rename _history to _storage and change to oldest-first storage order in src/Stroke/History/InMemoryHistory.cs
- [ ] T015 [US1] Add pre-population constructor InMemoryHistory(IEnumerable<string>? historyStrings) in src/Stroke/History/InMemoryHistory.cs
- [ ] T016 [US1] Implement LoadHistoryStrings to yield _storage in reverse (newest-first) in src/Stroke/History/InMemoryHistory.cs
- [ ] T017 [US1] Implement StoreString to add to _storage (oldest-first) in src/Stroke/History/InMemoryHistory.cs
- [ ] T018 [US1] Remove redundant GetStrings/AppendString/LoadAsync (use HistoryBase implementations) in src/Stroke/History/InMemoryHistory.cs
- [ ] T019 [US1] Add XML documentation matching Python PTK semantics in src/Stroke/History/InMemoryHistory.cs

**Checkpoint**: InMemoryHistory fully functional and independently testable

---

## Phase 4: User Story 2 - Persistent Command History (Priority: P1)

**Goal**: File-backed persistent history with Python PTK-compatible format

**Independent Test**: Create FileHistory with temp file, add entries, create new instance, verify persistence

### Tests for User Story 2

- [ ] T020 [P] [US2] Create FileHistoryTests with basic file operations in tests/Stroke.Tests/History/FileHistoryTests.cs
- [ ] T021 [P] [US2] Add test for file format (# timestamp, + prefix) in tests/Stroke.Tests/History/FileHistoryTests.cs
- [ ] T022 [P] [US2] Add test for multi-line entries (each line gets + prefix) in tests/Stroke.Tests/History/FileHistoryTests.cs
- [ ] T023 [P] [US2] Add test for UTF-8 encoding with replacement for invalid sequences in tests/Stroke.Tests/History/FileHistoryTests.cs
- [ ] T024 [P] [US2] Add test for non-existent file (creates on first write) in tests/Stroke.Tests/History/FileHistoryTests.cs
- [ ] T025 [P] [US2] Add test for corrupted/malformed entries (ignores bad lines) in tests/Stroke.Tests/History/FileHistoryTests.cs
- [ ] T026 [P] [US2] Add test for cross-session persistence (reload yields previous entries) in tests/Stroke.Tests/History/FileHistoryTests.cs
- [ ] T027 [P] [US2] Add thread safety concurrent file access test in tests/Stroke.Tests/History/FileHistoryTests.cs
- [ ] T080 [P] [US2] Add test for DirectoryNotFoundException when parent directory missing in tests/Stroke.Tests/History/FileHistoryTests.cs
- [ ] T081 [P] [US2] Add test for IOException propagation on read-only file system in tests/Stroke.Tests/History/FileHistoryTests.cs
- [ ] T086 [P] [US2] Add test for timestamp format matches Python datetime exactly (YYYY-MM-DD HH:MM:SS.ffffff) in tests/Stroke.Tests/History/FileHistoryTests.cs

### Implementation for User Story 2

- [ ] T028 [US2] Create FileHistory class extending HistoryBase in src/Stroke/History/FileHistory.cs
- [ ] T029 [US2] Add _filename field and Filename property in src/Stroke/History/FileHistory.cs
- [ ] T030 [US2] Add constructor FileHistory(string filename) with null validation in src/Stroke/History/FileHistory.cs
- [ ] T031 [US2] Implement LoadHistoryStrings with file parsing (+ prefix, # comments) in src/Stroke/History/FileHistory.cs
- [ ] T032 [US2] Implement StoreString with timestamp comment and + prefix format in src/Stroke/History/FileHistory.cs
- [ ] T033 [US2] Handle multi-line entries (split by newline, prefix each line with +) in src/Stroke/History/FileHistory.cs
- [ ] T034 [US2] Add UTF-8 encoding with DecoderFallback.ReplacementFallback in src/Stroke/History/FileHistory.cs
- [ ] T035 [US2] Add thread safety Lock around file operations in src/Stroke/History/FileHistory.cs
- [ ] T036 [US2] Add XML documentation in src/Stroke/History/FileHistory.cs

**Checkpoint**: FileHistory fully functional with byte-for-byte PTK format compatibility

---

## Phase 5: User Story 3 - Fast Application Startup (Priority: P2)

**Goal**: Background loading wrapper for non-blocking startup

**Independent Test**: Wrap slow-loading history in ThreadedHistory, verify first items available within 100ms

### Tests for User Story 3

- [ ] T037 [P] [US3] Create ThreadedHistoryTests with basic wrapper tests in tests/Stroke.Tests/History/ThreadedHistoryTests.cs
- [ ] T038 [P] [US3] Add test for background thread creation on first LoadAsync in tests/Stroke.Tests/History/ThreadedHistoryTests.cs
- [ ] T039 [P] [US3] Add test for progressive streaming (items yielded as they load) in tests/Stroke.Tests/History/ThreadedHistoryTests.cs
- [ ] T040 [P] [US3] Add test for 100ms first-item availability (SC-003) using Stopwatch and artificial delay in wrapped history in tests/Stroke.Tests/History/ThreadedHistoryTests.cs
- [ ] T041 [P] [US3] Add test for AppendString before load completes in tests/Stroke.Tests/History/ThreadedHistoryTests.cs
- [ ] T042 [P] [US3] Add test for multiple concurrent LoadAsync calls in tests/Stroke.Tests/History/ThreadedHistoryTests.cs
- [ ] T043 [P] [US3] Add test for delegation to wrapped history (LoadHistoryStrings, StoreString) in tests/Stroke.Tests/History/ThreadedHistoryTests.cs
- [ ] T082 [P] [US3] Add test for daemon thread (IsBackground = true) in tests/Stroke.Tests/History/ThreadedHistoryTests.cs
- [ ] T083 [P] [US3] Add test for cache reset when load starts (AppendString before any LoadAsync, then LoadAsync reloads from backend) in tests/Stroke.Tests/History/ThreadedHistoryTests.cs

### Implementation for User Story 3

- [ ] T044 [US3] Create ThreadedHistory class implementing IHistory in src/Stroke/History/ThreadedHistory.cs
- [ ] T045 [US3] Add _history, _loadThread, _lock, _stringLoadEvents fields in src/Stroke/History/ThreadedHistory.cs
- [ ] T046 [US3] Add _loaded and _loadedStrings fields for caching (matches HistoryBase pattern) in src/Stroke/History/ThreadedHistory.cs
- [ ] T047 [US3] Add constructor ThreadedHistory(IHistory history) with null validation in src/Stroke/History/ThreadedHistory.cs
- [ ] T048 [US3] Add History property to access wrapped instance in src/Stroke/History/ThreadedHistory.cs
- [ ] T049 [US3] Implement LoadHistoryStrings delegating to _history.LoadHistoryStrings() in src/Stroke/History/ThreadedHistory.cs
- [ ] T050 [US3] Implement StoreString delegating to _history.StoreString() in src/Stroke/History/ThreadedHistory.cs
- [ ] T051 [US3] Implement background thread loader (spawns on first LoadAsync) in src/Stroke/History/ThreadedHistory.cs
- [ ] T052 [US3] Implement progressive streaming via ManualResetEventSlim signaling in src/Stroke/History/ThreadedHistory.cs
- [ ] T053 [US3] Implement LoadAsync with event-based waiting for new items in src/Stroke/History/ThreadedHistory.cs
- [ ] T054 [US3] Implement GetStrings returning cache (oldest-first) in src/Stroke/History/ThreadedHistory.cs
- [ ] T055 [US3] Implement AppendString (insert at index 0, call StoreString) in src/Stroke/History/ThreadedHistory.cs
- [ ] T056 [US3] Add thread safety with Lock for all mutable state in src/Stroke/History/ThreadedHistory.cs
- [ ] T057 [US3] Add XML documentation in src/Stroke/History/ThreadedHistory.cs

**Checkpoint**: ThreadedHistory provides non-blocking background loading

---

## Phase 6: User Story 4 - Privacy-Sensitive Contexts (Priority: P3)

**Goal**: No-op history implementation that discards all operations

**Independent Test**: Create DummyHistory, append strings, verify GetStrings returns empty, LoadAsync yields nothing

### Tests for User Story 4

- [ ] T058 [P] [US4] Create DummyHistoryTests with no-op verification in tests/Stroke.Tests/History/DummyHistoryTests.cs
- [ ] T059 [P] [US4] Add test for AppendString no-op behavior in tests/Stroke.Tests/History/DummyHistoryTests.cs
- [ ] T060 [P] [US4] Add test for GetStrings returns empty list in tests/Stroke.Tests/History/DummyHistoryTests.cs
- [ ] T061 [P] [US4] Add test for LoadAsync yields nothing in tests/Stroke.Tests/History/DummyHistoryTests.cs
- [ ] T062 [P] [US4] Add test for StoreString no-op behavior in tests/Stroke.Tests/History/DummyHistoryTests.cs

### Implementation for User Story 4

- [ ] T063 [US4] Create DummyHistory class implementing IHistory directly in src/Stroke/History/DummyHistory.cs
- [ ] T064 [US4] Implement LoadHistoryStrings returning empty enumerable in src/Stroke/History/DummyHistory.cs
- [ ] T065 [US4] Implement StoreString as no-op in src/Stroke/History/DummyHistory.cs
- [ ] T066 [US4] Implement AppendString as no-op (direct IHistory implementation, independent of HistoryBase) in src/Stroke/History/DummyHistory.cs
- [ ] T067 [US4] Implement GetStrings returning empty list in src/Stroke/History/DummyHistory.cs
- [ ] T068 [US4] Implement LoadAsync yielding nothing in src/Stroke/History/DummyHistory.cs
- [ ] T069 [US4] Add XML documentation noting stateless/thread-safe nature in src/Stroke/History/DummyHistory.cs

**Checkpoint**: DummyHistory provides privacy-mode history

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Integration tests, edge cases, and final validation

### Integration Tests

- [ ] T070 [P] Create HistoryIntegrationTests for cross-implementation scenarios in tests/Stroke.Tests/History/HistoryIntegrationTests.cs
- [ ] T071 [P] Add test for ThreadedHistory wrapping FileHistory in tests/Stroke.Tests/History/HistoryIntegrationTests.cs
- [ ] T072 [P] Add test for ThreadedHistory wrapping InMemoryHistory in tests/Stroke.Tests/History/HistoryIntegrationTests.cs
- [ ] T073 [P] Add format compatibility test (read Python PTK-written file) in tests/Stroke.Tests/History/FileHistoryFormatTests.cs

### Edge Cases

- [ ] T074 Add FileHistory test for empty file behavior in tests/Stroke.Tests/History/FileHistoryTests.cs
- [ ] T075 Add FileHistory test for file with only comments (no entries) in tests/Stroke.Tests/History/FileHistoryTests.cs
- [ ] T076 Add InMemoryHistory test for Empty singleton behavior in tests/Stroke.Tests/History/InMemoryHistoryTests.cs
- [ ] T076b Add test for multiple LoadAsync calls (verify caching, no backend re-read) in tests/Stroke.Tests/History/HistoryBaseTests.cs

### Validation (Null Handling)

- [ ] T084 [P] Add test for AppendString(null) throws ArgumentNullException in tests/Stroke.Tests/History/HistoryBaseTests.cs
- [ ] T085 [P] Add test for StoreString(null) throws ArgumentNullException (all implementations) in tests/Stroke.Tests/History/HistoryBaseTests.cs

### Validation

- [ ] T077 Run quickstart.md examples as integration tests in tests/Stroke.Tests/History/QuickstartValidationTests.cs
- [ ] T078 Verify 80% test coverage target (SC-005) using dotnet test --collect:"XPlat Code Coverage"
- [ ] T079 Verify all XML documentation complete and accurate across all History classes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phases 3-6)**: All depend on Foundational phase completion
  - US1 (InMemory) and US4 (Dummy) can run in parallel
  - US2 (FileHistory) can run in parallel with US1/US4
  - US3 (ThreadedHistory) depends on at least one other implementation existing for testing
- **Polish (Phase 7)**: Depends on all user story phases being complete

### User Story Dependencies

- **User Story 1 (P1 - InMemory)**: After Phase 2 - No dependencies on other stories
- **User Story 2 (P1 - FileHistory)**: After Phase 2 - No dependencies on other stories
- **User Story 3 (P2 - ThreadedHistory)**: After Phase 2 - Needs US1 or US2 for wrapping tests
- **User Story 4 (P3 - DummyHistory)**: After Phase 2 - No dependencies on other stories

### Within Each User Story

- Tests written FIRST, ensure they FAIL before implementation
- Implementation follows test completion
- Story complete before moving to next priority (or parallel if capacity allows)

### Parallel Opportunities

- T009-T012 (US1 tests) can run in parallel
- T020-T027 (US2 tests) can run in parallel
- T037-T043 (US3 tests) can run in parallel
- T058-T062 (US4 tests) can run in parallel
- T070-T073 (integration tests) can run in parallel
- US1 and US4 can be implemented in parallel (no shared dependencies)
- US1 and US2 can be implemented in parallel (no shared dependencies)

---

## Parallel Example: User Story 1 Tests

```bash
# Launch all US1 tests together:
Task: "T009 [P] [US1] Create InMemoryHistoryTests with basic append/get tests"
Task: "T010 [P] [US1] Add test for pre-populated constructor"
Task: "T011 [P] [US1] Add test for loading order (newest-first)"
Task: "T012 [P] [US1] Add thread safety concurrent access test"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: Foundational (T004-T008)
3. Complete Phase 3: User Story 1 (T009-T019)
4. **STOP and VALIDATE**: Test InMemoryHistory independently
5. Can deploy/use with just InMemoryHistory

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add User Story 1 (InMemory) → Test → MVP with session-only history
3. Add User Story 2 (FileHistory) → Test → Persistent history available
4. Add User Story 3 (Threaded) → Test → Fast startup for large histories
5. Add User Story 4 (Dummy) → Test → Privacy mode available
6. Polish → Full feature complete

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (InMemory)
   - Developer B: User Story 2 (FileHistory)
   - Developer C: User Story 4 (Dummy)
3. After US1 or US2 complete:
   - Developer D: User Story 3 (Threaded) - needs something to wrap

---

## Summary

| Metric | Value |
|--------|-------|
| **Total Tasks** | 87 |
| **Phase 1 (Setup)** | 3 tasks |
| **Phase 2 (Foundational)** | 5 tasks |
| **Phase 3 (US1 - InMemory)** | 11 tasks (4 tests, 7 impl) |
| **Phase 4 (US2 - FileHistory)** | 20 tasks (11 tests, 9 impl) |
| **Phase 5 (US3 - Threaded)** | 23 tasks (9 tests, 14 impl) |
| **Phase 6 (US4 - Dummy)** | 12 tasks (5 tests, 7 impl) |
| **Phase 7 (Polish)** | 13 tasks |
| **Parallel Opportunities** | 49 tasks marked [P] |
| **MVP Scope** | Phases 1-3 (19 tasks) |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Tests use xUnit with real file system (no mocks per Constitution VIII)
- Thread safety uses System.Threading.Lock (.NET 9+) per Constitution XI
- File format must be byte-for-byte compatible with Python Prompt Toolkit
- Stop at any checkpoint to validate story independently
