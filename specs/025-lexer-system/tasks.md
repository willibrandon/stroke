# Tasks: Lexer System

**Input**: Design documents from `/specs/025-lexer-system/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests ARE included as the spec mentions "xUnit (no mocks per Constitution VIII)" and targets 80% coverage per SC-007.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: Source in `src/Stroke/Lexers/`, tests in `tests/Stroke.Tests/Lexers/`
- Paths follow existing Stroke project structure per plan.md

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and namespace structure

- [ ] T001 Create Lexers directory at `src/Stroke/Lexers/`
- [ ] T002 Create test directory at `tests/Stroke.Tests/Lexers/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core interfaces and types that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T003 Implement `ILexer` interface in `src/Stroke/Lexers/ILexer.cs` with `LexDocument` returning `Func<int, IReadOnlyList<StyleAndTextTuple>>` and `InvalidationHash()` returning `object` (FR-001, FR-002)
- [ ] T004 [P] Create test file `tests/Stroke.Tests/Lexers/LexerBaseTests.cs` with ILexer contract verification tests

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Display Text Without Syntax Highlighting (Priority: P1) 🎯 MVP

**Goal**: SimpleLexer applies a single style to all text without tokenization. This is the foundational lexer used as fallback.

**Independent Test**: Create a SimpleLexer, pass a Document, verify each line returns expected style and text tuple.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T005 [P] [US1] Create `tests/Stroke.Tests/Lexers/SimpleLexerTests.cs` with tests for:
  - Default style returns empty style string (FR-003)
  - Custom style returns configured style (FR-003)
  - Null style treated as empty string (FR-003)
  - Negative line number returns empty list (FR-004, EC-005)
  - Line beyond bounds returns empty list (FR-004)
  - Empty document returns empty list for all lines (EC-006)
  - Whitespace-only lines processed normally (EC-009)
  - int.MaxValue line returns empty list (EC-008)
  - Multiple lines each get single token with style
  - InvalidationHash returns `this` instance

### Implementation for User Story 1

- [ ] T006 [US1] Implement `SimpleLexer` class in `src/Stroke/Lexers/SimpleLexer.cs`:
  - Sealed class implementing `ILexer`
  - Constructor with `string style = ""` parameter (null treated as "")
  - `Style` property (readonly, never null)
  - `LexDocument(Document)` returning function that maps line numbers to styled tokens
  - `InvalidationHash()` returning `this`
  - Thread-safe (immutable after construction per FR-022)
- [ ] T007 [US1] Add acceptance tests to SimpleLexerTests.cs:
  - `Given_SimpleLexerWithDefaultStyle_When_LexingMultiLineDocument_Then_EachLineHasEmptyStyle` (US1-AC1)
  - `Given_SimpleLexerWithCustomStyle_When_LexingDocument_Then_AllTextHasConfiguredStyle` (US1-AC2)
  - `Given_SimpleLexer_When_RequestingLineBeyondBounds_Then_EmptyListReturned` (US1-AC3)

**Checkpoint**: SimpleLexer fully functional and tested independently - MVP complete

---

## Phase 4: User Story 2 - Switch Between Lexers Dynamically (Priority: P2)

**Goal**: DynamicLexer delegates to a runtime-determined lexer via callback, enabling file type switching.

**Independent Test**: Create DynamicLexer with callback returning different lexers, verify output changes when callback returns different lexer.

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T008 [P] [US2] Create `tests/Stroke.Tests/Lexers/DynamicLexerTests.cs` with tests for:
  - Callback returns lexer, delegates to that lexer (FR-005)
  - Callback returns null, uses fallback SimpleLexer (FR-006)
  - InvalidationHash returns active lexer's hash (FR-007)
  - Null callback throws ArgumentNullException (EC-014)
  - Null document throws ArgumentNullException
  - Callback exception propagates to caller (EC-001)

### Implementation for User Story 2

- [ ] T009 [US2] Implement `DynamicLexer` class in `src/Stroke/Lexers/DynamicLexer.cs`:
  - Sealed class implementing `ILexer`
  - Constructor with `Func<ILexer?>` parameter (throws ArgumentNullException if null)
  - Private `SimpleLexer("")` fallback instance
  - `LexDocument(Document)` invoking callback and delegating (or using fallback)
  - `InvalidationHash()` returning active lexer's hash (FR-007)
  - Thread-safe for returned function per FR-023
- [ ] T010 [US2] Add acceptance tests to DynamicLexerTests.cs:
  - `Given_DynamicLexerWithCallback_When_CallbackReturnsLexer_Then_DelegatesToThatLexer` (US2-AC1)
  - `Given_DynamicLexerWithCallback_When_CallbackReturnsNull_Then_FallbackUsed` (US2-AC2)
  - `Given_DynamicLexer_When_CallbackReturnsDifferentLexer_Then_HashChanges` (US2-AC3)
- [ ] T011 [P] [US2] Create `tests/Stroke.Tests/Lexers/DynamicLexerConcurrencyTests.cs` with:
  - `Concurrent_CallbackInvocation_Safe` (multiple threads calling LexDocument)
  - `Concurrent_ReturnedFunction_ThreadSafe`

**Checkpoint**: DynamicLexer fully functional - can switch lexers at runtime

---

## Phase 5: User Story 3 - Efficient Large Document Highlighting (Priority: P2)

**Goal**: ISyntaxSync interface with SyncFromStart and RegexSync strategies for finding safe lexing start positions in large documents.

**Independent Test**: Create RegexSync with pattern, provide large document, verify sync positions are within acceptable distance.

### Tests for User Story 3

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T012 [P] [US3] Create `tests/Stroke.Tests/Lexers/SyntaxSyncTests.cs` with tests for:
  - ISyntaxSync contract: GetSyncStartPosition returns (Row, Column) tuple (FR-008)
  - SyncFromStart always returns (0, 0) (FR-009)
  - SyncFromStart.Instance returns singleton
  - SyncFromStart null document throws ArgumentNullException
  - RegexSync scans backwards up to MaxBackwards (500) lines (FR-010)
  - RegexSync no match near start returns (0, 0) (FR-011)
  - RegexSync no match far from start returns (lineNo, 0) (FR-012)
  - RegexSync null pattern throws ArgumentNullException (EC-015)
  - RegexSync invalid regex throws ArgumentException (EC-003)
  - RegexSync empty pattern matches all lines (EC-016)
  - RegexSync null document throws ArgumentNullException
  - RegexSync.ForLanguage("Python") returns correct pattern (FR-013)
  - RegexSync.ForLanguage("Python 3") returns correct pattern
  - RegexSync.ForLanguage("HTML") returns correct pattern
  - RegexSync.ForLanguage("JavaScript") returns correct pattern
  - RegexSync.ForLanguage("Unknown") returns default pattern

### Implementation for User Story 3

- [ ] T013 [US3] Implement `ISyntaxSync` interface in `src/Stroke/Lexers/ISyntaxSync.cs`:
  - `GetSyncStartPosition(Document document, int lineNo)` returning `(int Row, int Column)`
- [ ] T014 [P] [US3] Implement `SyncFromStart` class in `src/Stroke/Lexers/SyncFromStart.cs`:
  - Sealed class implementing `ISyntaxSync`
  - Private constructor for singleton pattern
  - Static `Instance` property returning singleton
  - `GetSyncStartPosition` always returns (0, 0)
  - Throws ArgumentNullException if document is null
- [ ] T015 [P] [US3] Implement `RegexSync` class in `src/Stroke/Lexers/RegexSync.cs`:
  - Sealed class implementing `ISyntaxSync`
  - `MaxBackwards` constant = 500
  - `FromStartIfNoSyncPosFound` constant = 100
  - Constructor taking pattern string, compiling with RegexOptions.Compiled
  - `GetSyncStartPosition` scanning backwards to find pattern match
  - `ForLanguage(string language)` static factory method
- [ ] T016 [US3] Add acceptance tests to SyntaxSyncTests.cs:
  - `Given_RegexSyncWithPattern_When_RequestingLine1000_Then_PositionWithin500Lines` (US3-AC1)
  - `Given_RegexSync_When_NoMatchNearStart_Then_ReturnsZeroZero` (US3-AC2)
  - `Given_RegexSync_When_NoMatchFarFromStart_Then_ReturnsRequestedLine` (US3-AC3)
  - `Given_SyncFromStart_When_RequestingAnyLine_Then_ReturnsZeroZero` (US3-AC4)

**Checkpoint**: Syntax synchronization infrastructure complete - ready for PygmentsLexer

---

## Phase 6: User Story 4 - Apply Pygments-Style Syntax Highlighting (Priority: P3)

**Goal**: PygmentsLexer adapts IPygmentsLexer implementations to Stroke with token-to-style conversion.

**Independent Test**: Create PygmentsLexer with TestPythonLexer, lex document, verify tokens converted to "class:pygments.tokentype" format.

### Tests for User Story 4

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T017 [P] [US4] Create `tests/Stroke.Tests/Lexers/TestPythonLexer.cs` - real IPygmentsLexer implementation for testing (Constitution VIII - no mocks):
  - Name property returns "Python"
  - GetTokensUnprocessed tokenizes with keywords, names, text, punctuation
- [ ] T018 [P] [US4] Create `tests/Stroke.Tests/Lexers/PygmentsLexerTests.cs` with tests for:
  - Token conversion single level: ["Keyword"] → "class:pygments.keyword" (FR-014)
  - Token conversion nested: ["Name", "Function"] → "class:pygments.name.function" (FR-014)
  - Token conversion deep: ["Name", "Function", "Magic"] → "class:pygments.name.function.magic"
  - syncFromStart default (HasValue=false) treated as true (FR-015)
  - syncFromStart=true lexes from beginning (FR-015)
  - syncFromStart=false uses syntaxSync (FR-015)
  - syncFromStart with IFilter evaluates filter (FR-015)
  - Null pygmentsLexer throws ArgumentNullException (EC-017)
  - Null document throws ArgumentNullException
  - FromFilename with unknown extension returns SimpleLexer (FR-019)
  - FromFilename with null throws ArgumentNullException (EC-019)
  - FromFilename with empty returns SimpleLexer (EC-019)
  - IPygmentsLexer.Name returns lexer name (FR-020)
  - IPygmentsLexer.GetTokensUnprocessed returns tokens in order (FR-020)
  - Negative line number returns empty list (EC-005)
  - Empty document returns empty list (EC-006)
  - Unicode content processed without error (EC-011)

### Implementation for User Story 4

- [ ] T019 [US4] Implement `IPygmentsLexer` interface in `src/Stroke/Lexers/IPygmentsLexer.cs`:
  - `Name` property returning string
  - `GetTokensUnprocessed(string text)` returning `IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)>`
- [ ] T020 [US4] Implement internal `TokenCache` class in `src/Stroke/Lexers/TokenCache.cs`:
  - ConcurrentDictionary for thread-safe caching
  - `GetStyleClass(IReadOnlyList<string> tokenType)` converting to "class:pygments.x.y.z" format
- [ ] T021 [US4] Implement `PygmentsLexer` class in `src/Stroke/Lexers/PygmentsLexer.cs`:
  - Sealed class implementing `ILexer`
  - `MinLinesBackwards` constant = 50
  - `ReuseGeneratorMaxDistance` constant = 100
  - Constructor with `IPygmentsLexer`, `FilterOrBool syncFromStart`, `ISyntaxSync? syntaxSync`
  - `FromFilename(string filename, FilterOrBool syncFromStart)` static factory
  - `LexDocument(Document)` creating isolated state with line cache and generator tracking
  - `InvalidationHash()` returning `this`
  - Token conversion using TokenCache
- [ ] T022 [US4] Add acceptance tests to PygmentsLexerTests.cs:
  - `Given_PygmentsLexerWithLexer_When_LexingDocument_Then_TokensConvertedToClassPygmentsFormat` (US4-AC1)
  - `Given_PygmentsLexerWithSyncFromStartEnabled_When_LexingAnyLine_Then_LexesFromBeginning` (US4-AC2)
  - `Given_PygmentsLexerWithSyncFromStartDisabled_When_LexingFarLine_Then_UsesSyntaxSync` (US4-AC3)
  - `Given_Filename_When_CreatingLexer_Then_AppropriateLexerReturned` (US4-AC4)

**Checkpoint**: PygmentsLexer can adapt external lexers with proper token conversion

---

## Phase 7: User Story 5 - Cache Lexed Lines for Performance (Priority: P3)

**Goal**: PygmentsLexer caches lexed lines and reuses generators for efficient scrolling in large documents.

**Independent Test**: Lex same line multiple times, verify cached result returned without re-lexing; lex sequential lines, verify generator reuse.

### Tests for User Story 5

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T023 [P] [US5] Add caching tests to `tests/Stroke.Tests/Lexers/PygmentsLexerTests.cs`:
  - Cache hit returns cached result without re-lexing (FR-016)
  - Cache miss lexes and caches result (FR-016)
  - Generator reuse within ReuseGeneratorMaxDistance (FR-017)
  - Generator not reused beyond ReuseGeneratorMaxDistance (FR-017)
  - New generator goes back at least MinLinesBackwards (FR-018)
- [ ] T024 [P] [US5] Create `tests/Stroke.Tests/Lexers/PygmentsLexerConcurrencyTests.cs`:
  - `Concurrent_LexDocument_NoExceptions` (10 threads, 100 calls each)
  - `Concurrent_LineAccess_ConsistentResults` (1000 concurrent line requests)
  - `Concurrent_CacheAccess_NoCorruption` (verify consistent cached values)

### Implementation for User Story 5

- [ ] T025 [US5] Enhance `PygmentsLexer.LexDocument` with caching infrastructure:
  - Line cache: `Dictionary<int, IReadOnlyList<StyleAndTextTuple>>` per returned function
  - Cache lookup before lexing
  - Cache population after lexing
- [ ] T026 [US5] Implement generator reuse logic in `PygmentsLexer`:
  - Track active generators with current line position
  - Reuse when `generatorLine < requestedLine AND requestedLine - generatorLine < 100`
  - Create new generator at `max(0, requestedLine - MinLinesBackwards)` when not reusing
- [ ] T027 [US5] Add thread safety with Lock in `PygmentsLexer` (FR-021, FR-024):
  - `Lock` instance per returned function
  - `EnterScope()` pattern for cache and generator access
  - Satisfies FR-021 (thread safety for mutable state) and FR-024 (Lock pattern)
- [ ] T028 [US5] Add acceptance tests to PygmentsLexerTests.cs:
  - `Given_LexedLine_When_RequestingSameLine_Then_CachedResultReturned` (US5-AC1)
  - `Given_GeneratorAtLineN_When_RequestingLineNPlus10_Then_GeneratorReused` (US5-AC2)
  - `Given_GeneratorAtLineN_When_RequestingLineBeyondReuseDistance_Then_NewGeneratorCreated` (US5-AC3)

**Checkpoint**: Full caching and generator reuse implemented - performance optimized

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Performance validation, edge case coverage, documentation

- [ ] T029 [P] Create `tests/Stroke.Tests/Lexers/EdgeCaseTests.cs` with comprehensive edge case coverage:
  - EC-002: Mixed line endings (Document handles normalization)
  - EC-004: Malformed tokens from IPygmentsLexer (processed without validation)
  - EC-007: Concurrent access thread safety verification
  - EC-010: Very long lines (>64KB) processed without truncation
  - EC-012: Catastrophic regex backtracking (no timeout enforced - document behavior)
  - EC-018: Null/empty text in GetTokensUnprocessed
  - EC-020: Generator disposed mid-iteration
- [ ] T030 [P] Add benchmark test for SC-001: SimpleLexer ≤1ms/line verification
- [ ] T031 [P] Add benchmark test for SC-004: O(1) cached line retrieval verification
- [ ] T032 Run quickstart.md validation - verify all code examples compile and work
- [ ] T033 Verify code coverage ≥80% per class (SC-007) using `dotnet test --collect:"XPlat Code Coverage"`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational - can start after T003/T004
- **User Story 2 (Phase 4)**: Depends on US1 (needs SimpleLexer as fallback)
- **User Story 3 (Phase 5)**: Depends on Foundational only - can run parallel with US1/US2
- **User Story 4 (Phase 6)**: Depends on US3 (needs ISyntaxSync) and US1 (SimpleLexer for FromFilename fallback)
- **User Story 5 (Phase 7)**: Depends on US4 (enhances PygmentsLexer)
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 1: Setup
    │
    ▼
Phase 2: Foundational (ILexer interface)
    │
    ├────────────────┬────────────────┐
    ▼                ▼                │
Phase 3: US1      Phase 5: US3       │
(SimpleLexer)     (ISyntaxSync)      │
    │                │                │
    ├────────────────┤                │
    ▼                ▼                │
Phase 4: US2 ◄───────┘                │
(DynamicLexer)                        │
    │                                 │
    └────────────────┬────────────────┘
                     ▼
              Phase 6: US4
              (PygmentsLexer)
                     │
                     ▼
              Phase 7: US5
              (Caching)
                     │
                     ▼
              Phase 8: Polish
```

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Interfaces before implementations
- Core implementation before thread safety enhancements
- Story complete before moving to next priority

### Parallel Opportunities

- T005 and T012 can run in parallel (different user stories after Foundational)
- T014 and T015 can run in parallel (both implement ISyntaxSync)
- T017 and T018 can run in parallel (test helpers and test file)
- T023 and T024 can run in parallel (different test files)
- T029, T030, T031 can run in parallel (different test files)

---

## Parallel Example: Foundational + Multiple Stories

```bash
# After Foundational complete, these can run in parallel:
# Team Member A:
Task: T005 - SimpleLexer tests
Task: T006 - SimpleLexer implementation

# Team Member B:
Task: T012 - SyntaxSync tests
Task: T013-T15 - ISyntaxSync, SyncFromStart, RegexSync
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 2: Foundational (T003-T004)
3. Complete Phase 3: User Story 1 (T005-T007)
4. **STOP and VALIDATE**: SimpleLexer works independently
5. Deploy/demo if ready - basic text display without highlighting

### Incremental Delivery

1. Setup + Foundational → ILexer interface defined
2. Add User Story 1 → SimpleLexer functional (MVP!)
3. Add User Story 2 → DynamicLexer enables runtime switching
4. Add User Story 3 → Syntax synchronization for large documents
5. Add User Story 4 → Pygments-style syntax highlighting
6. Add User Story 5 → Performance optimization with caching
7. Polish → Edge cases, benchmarks, documentation validation

### Suggested MVP Scope

**MVP = User Story 1 (SimpleLexer)** - This provides:
- Basic ILexer interface contract
- Single-style text display
- Foundation for all other lexers
- Fallback for when no syntax highlighting is available

---

## Summary

| Phase | Tasks | Description |
|-------|-------|-------------|
| 1: Setup | T001-T002 | Create directory structure |
| 2: Foundational | T003-T004 | ILexer interface and contract tests |
| 3: US1 (P1) | T005-T007 | SimpleLexer - MVP |
| 4: US2 (P2) | T008-T011 | DynamicLexer - runtime switching |
| 5: US3 (P2) | T012-T016 | ISyntaxSync - large document support |
| 6: US4 (P3) | T017-T022 | PygmentsLexer - syntax highlighting |
| 7: US5 (P3) | T023-T028 | Caching and performance |
| 8: Polish | T029-T033 | Edge cases, benchmarks, validation |

**Total Tasks**: 33
**Tasks per User Story**: US1=3, US2=4, US3=5, US4=6, US5=6
**Parallel Opportunities**: 15 tasks marked [P]
**MVP Scope**: Phases 1-3 (7 tasks)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All tasks follow Constitution requirements (VIII: real tests, XI: thread safety)
