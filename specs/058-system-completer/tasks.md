# Tasks: System Completer

**Input**: Design documents from `/specs/058-system-completer/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Not explicitly requested in specification. Test tasks included per Constitution VIII (Real-World Testing, 80% coverage target).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/Stroke/`, `tests/Stroke.Tests/` at repository root
- Namespace: `Stroke.Contrib.Completers`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project structure verification - no new projects needed

- [ ] T001 Verify existing namespace structure includes `Stroke.Contrib.Completers` in `src/Stroke/Contrib/Completers/`
- [ ] T002 Verify test directory structure exists at `tests/Stroke.Tests/Contrib/Completers/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: No foundational tasks required - all dependencies already implemented

**Dependencies Verified (from research.md)**:
- ✅ `Grammar.Compile()` with escape/unescape functions (Grammar.cs)
- ✅ `GrammarCompleter` delegation pattern (GrammarCompleter.cs)
- ✅ `ExecutableCompleter` with platform detection (ExecutableCompleter.cs)
- ✅ `PathCompleter` with tilde expansion (PathCompleter.cs)

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Complete Executable Names (Priority: P1) 🎯 MVP

**Goal**: Users can complete any executable in PATH by typing partial command names

**Independent Test**: Type "gi" at command position, verify "git", "gist" appear as completions

### Implementation for User Story 1

- [ ] T003 [US1] Create SystemCompleter class with grammar constant in `src/Stroke/Contrib/Completers/SystemCompleter.cs`:
  - Define `GrammarPattern` constant with regex: `(?P<executable>[^\s]+)(\s+("[^"]*"|'[^']*'|[^'"]+))*\s+((?P<filename>[^\s]+)|"(?P<double_quoted_filename>[^\s]+)"|'(?P<single_quoted_filename>[^\s]+)')`
  - Create `sealed class SystemCompleter : GrammarCompleter`
  - Add XML documentation per Constitution (thread safety, usage examples)

- [ ] T004 [US1] Implement parameterless constructor in `src/Stroke/Contrib/Completers/SystemCompleter.cs`:
  - Compile grammar with `Grammar.Compile()` passing escape/unescape function dictionaries
  - Create completer dictionary: `executable` → `new ExecutableCompleter(minInputLen: 1, expandUser: true)`
  - Call base constructor with compiled grammar and completers

- [ ] T005 [US1] Create test file structure at `tests/Stroke.Tests/Contrib/Completers/SystemCompleterTests.cs`:
  - Create `SystemCompleterTests` class
  - Add test: executable completion at first word position (validates FR-001, FR-007)
  - Add test: no completions for non-existent executable prefix

**Checkpoint**: User Story 1 complete - executable completion works independently

---

## Phase 4: User Story 2 - Complete Unquoted File Paths (Priority: P2)

**Goal**: Users can complete file paths after a command and whitespace

**Independent Test**: Type "cat /ho", verify "/home" appears (if exists)

### Implementation for User Story 2

- [ ] T006 [US2] Add `filename` completer mapping in `src/Stroke/Contrib/Completers/SystemCompleter.cs`:
  - Map `filename` → `new PathCompleter(onlyDirectories: false, expandUser: true)`
  - Verify grammar pattern captures unquoted paths after whitespace

- [ ] T007 [US2] Add unquoted path completion tests in `tests/Stroke.Tests/Contrib/Completers/SystemCompleterTests.cs`:
  - Test: unquoted path completion after command (validates FR-002, FR-003)
  - Test: tilde expansion in unquoted paths (validates FR-006)
  - Test: relative paths `./` and `../` (validates FR-010)
  - Test: multiple argument positions (validates FR-009)

**Checkpoint**: User Stories 1 AND 2 work independently

---

## Phase 5: User Story 3 - Complete Double-Quoted File Paths (Priority: P3)

**Goal**: Users can complete file paths inside double quotes with proper escaping

**Independent Test**: Type `cat "/home/user/my fi`, verify paths with escaped internal quotes

### Implementation for User Story 3

- [ ] T008 [US3] Add `double_quoted_filename` completer mapping in `src/Stroke/Contrib/Completers/SystemCompleter.cs`:
  - Map `double_quoted_filename` → `new PathCompleter(onlyDirectories: false, expandUser: true)`
  - Add escape function: `"` → `\"`
  - Add unescape function: `\"` → `"`

- [ ] T009 [US3] Add double-quoted path completion tests in `tests/Stroke.Tests/Contrib/Completers/SystemCompleterTests.cs`:
  - Test: double-quoted path completion (validates FR-004)
  - Test: escape handling for internal double quotes
  - Test: tilde expansion inside double quotes (validates FR-006)

**Checkpoint**: User Stories 1, 2, AND 3 work independently

---

## Phase 6: User Story 4 - Complete Single-Quoted File Paths (Priority: P3)

**Goal**: Users can complete file paths inside single quotes with proper escaping

**Independent Test**: Type `cat '/home/user/my fi`, verify paths with escaped internal single quotes

### Implementation for User Story 4

- [ ] T010 [US4] Add `single_quoted_filename` completer mapping in `src/Stroke/Contrib/Completers/SystemCompleter.cs`:
  - Map `single_quoted_filename` → `new PathCompleter(onlyDirectories: false, expandUser: true)`
  - Add escape function: `'` → `\'`
  - Add unescape function: `\'` → `'`

- [ ] T011 [US4] Add single-quoted path completion tests in `tests/Stroke.Tests/Contrib/Completers/SystemCompleterTests.cs`:
  - Test: single-quoted path completion (validates FR-005)
  - Test: escape handling for internal single quotes

**Checkpoint**: All user stories (US1-US4) work independently

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Edge cases, documentation, and validation

- [ ] T012 [P] Add edge case tests in `tests/Stroke.Tests/Contrib/Completers/SystemCompleterTests.cs`:
  - Test: empty PATH handling (no completions shown)
  - Test: non-existent PATH directories (silently skipped)
  - Test: empty input (executable completion attempted)
  - Test: whitespace-only after command (file path completion activates)

- [ ] T013 [P] Add cross-platform and NFR tests in `tests/Stroke.Tests/Contrib/Completers/SystemCompleterTests.cs`:
  - Test: thread safety with concurrent GetCompletions calls (validates NFR-001)
  - Test: verify SystemCompleter has no mutable instance fields (validates NFR-002)
  - Test: platform-appropriate behavior (conditional on runtime OS)

- [ ] T014 Run quickstart.md validation scenarios:
  - Verify basic usage example works
  - Verify all table examples produce expected completions

- [ ] T015 Run full test suite and verify 80%+ coverage target

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - verification only
- **Foundational (Phase 2)**: N/A - all dependencies pre-exist
- **User Stories (Phases 3-6)**: Can proceed sequentially (shared file)
- **Polish (Phase 7)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 1 (P1)**: MVP - can start immediately, no dependencies
- **User Story 2 (P2)**: Builds on US1 (same file, adds `filename` mapping)
- **User Story 3 (P3)**: Builds on US1-US2 (same file, adds `double_quoted_filename` mapping)
- **User Story 4 (P3)**: Builds on US1-US3 (same file, adds `single_quoted_filename` mapping)

**Note**: All user stories modify the same file (`SystemCompleter.cs`), so they must be implemented sequentially. However, the implementation is designed so each story adds incremental functionality that can be tested independently.

### Within Each User Story

- Implementation task(s) → Test task(s)
- Each story adds one completer mapping + corresponding tests
- Story complete before moving to next priority

### Parallel Opportunities

- T001, T002 can run in parallel (different directories)
- T012, T013 can run in parallel (different test categories in same file)
- Test validation (T014, T015) can run in parallel after implementation

---

## Parallel Example: Setup Phase

```bash
# Launch setup verification tasks together:
Task: "Verify existing namespace structure includes `Stroke.Contrib.Completers` in src/Stroke/Contrib/Completers/"
Task: "Verify test directory structure exists at tests/Stroke.Tests/Contrib/Completers/"
```

## Parallel Example: Polish Phase

```bash
# Launch polish tasks together (after all stories complete):
Task: "Add edge case tests in tests/Stroke.Tests/Contrib/Completers/SystemCompleterTests.cs"
Task: "Add cross-platform tests in tests/Stroke.Tests/Contrib/Completers/SystemCompleterTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Skip Phase 2: Foundational (no tasks)
3. Complete Phase 3: User Story 1 (T003-T005)
4. **STOP and VALIDATE**: Test executable completion independently
5. Deploy/demo if ready - SystemCompleter is usable for basic command completion

### Incremental Delivery

1. Setup → Ready (verification only)
2. Add User Story 1 → Test → **MVP!** (executable completion)
3. Add User Story 2 → Test → Unquoted paths work
4. Add User Story 3 → Test → Double-quoted paths work
5. Add User Story 4 → Test → Single-quoted paths work
6. Polish → Production ready

### Single Developer Strategy

With this ~50 LOC feature:

1. Complete all setup verification
2. Implement US1 fully (class + constructor + tests)
3. Incrementally add US2, US3, US4 mappings and tests
4. Polish with edge cases
5. **Total estimated tasks**: 15 tasks, ~1-2 hours implementation

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- All user stories share `SystemCompleter.cs` - sequential implementation required
- Each story adds testable functionality incrementally
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: modifying Grammar/GrammarCompleter (use existing API), adding extensibility hooks (NFR-003 forbids this)

---

## Task Summary

| Phase | Tasks | Parallel | Description |
|-------|-------|----------|-------------|
| Setup | T001-T002 | 2 | Directory verification |
| Foundational | — | 0 | No tasks (deps pre-exist) |
| US1 (P1) MVP | T003-T005 | 0 | Executable completion |
| US2 (P2) | T006-T007 | 0 | Unquoted file paths |
| US3 (P3) | T008-T009 | 0 | Double-quoted paths |
| US4 (P3) | T010-T011 | 0 | Single-quoted paths |
| Polish | T012-T015 | 2 | Edge cases, validation |

**Total**: 15 tasks
**Parallel opportunities**: 4 tasks (Setup: 2, Polish: 2)
**MVP scope**: T001-T005 (5 tasks)
