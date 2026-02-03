# Tasks: Windows 10 VT100 Output

**Input**: Design documents from `/specs/055-win10-vt100-output/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/Windows10Output.md ✅, quickstart.md ✅

**Tests**: Included per Constitution VIII (real-world testing) and spec.md Success Criteria SC-004 (80% coverage requirement).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Output/Windows/`
- **Tests**: `tests/Stroke.Tests/Output/Windows/`

---

## Phase 1: Setup

**Purpose**: Verify prerequisites and project structure

- [x] T001 Verify existing classes are available: Win32Output, Vt100Output, ConsoleApi, PlatformUtils in respective namespaces
- [x] T002 Verify P/Invoke constants exist: ENABLE_PROCESSED_INPUT (0x0001), ENABLE_VIRTUAL_TERMINAL_PROCESSING (0x0004), STD_OUTPUT_HANDLE (-11) in src/Stroke/Input/Windows/ConsoleApi.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure required before user stories

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 [P] Create WindowsVt100Support static class skeleton in src/Stroke/Output/Windows/WindowsVt100Support.cs
- [x] T004 [P] Create Windows10Output class skeleton in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T005 Implement WindowsVt100Support.IsVt100Enabled() delegating to PlatformUtils.IsWindowsVt100Supported in src/Stroke/Output/Windows/WindowsVt100Support.cs
- [x] T006 Implement Windows10Output constructor with validation (null check, platform check, handle acquisition) in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T007 [P] Create Windows10OutputTests test class skeleton in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T008 [P] Create WindowsVt100SupportTests test class skeleton in tests/Stroke.Tests/Output/Windows/WindowsVt100SupportTests.cs

**Checkpoint**: Foundation ready - class skeletons exist with constructor validation

---

## Phase 3: User Story 1 - Terminal Application Rendering with Modern Colors (Priority: P1) 🎯 MVP

**Goal**: Enable ANSI escape sequences for rich text formatting on Windows 10+ with VT100 mode switching during flush

**Independent Test**: Create Windows10Output instance, write ANSI-colored text, flush, verify VT100 mode was enabled/restored

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T009 [P] [US1] Test constructor throws ArgumentNullException for null stdout in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T010 [P] [US1] Test constructor throws PlatformNotSupportedException on non-Windows in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T011 [P] [US1] Test constructor propagates NoConsoleScreenBufferError from Win32Output in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T011a [P] [US1] Test constructor stores console handle once (FR-015: verify handle is not re-acquired during each Flush) in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T012 [P] [US1] Test Flush() acquires lock and calls Vt100Output.Flush() in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T013 [P] [US1] Test Flush() restores console mode in finally block in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T014 [P] [US1] Test concurrent Flush() calls are serialized via per-instance lock (10+ threads, 1000+ iterations per Constitution XI) in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T015 [P] [US1] Test Write() delegates to Vt100Output in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T016 [P] [US1] Test WriteRaw() delegates to Vt100Output in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T017 [P] [US1] Test GetDefaultColorDepth() returns Depth24Bit by default in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T018 [P] [US1] Test GetDefaultColorDepth() returns override when provided in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T019 [P] [US1] Test RespondsToCpr returns false in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs

### Implementation for User Story 1

- [x] T020 [US1] Implement Flush() with VT100 mode switching (lock, GetConsoleMode, SetConsoleMode, delegate, restore) in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T021 [P] [US1] Implement Write() delegation to Vt100Output in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T022 [P] [US1] Implement WriteRaw() delegation to Vt100Output in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T023 [P] [US1] Implement GetDefaultColorDepth() returning Depth24Bit or override in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T024 [P] [US1] Implement RespondsToCpr property returning false in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T025 [US1] Implement all VT100 rendering delegations (EraseScreen, EraseEndOfLine, EraseDown, etc.) to Vt100Output in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T026 [US1] Implement cursor delegations (CursorGoto, CursorUp, CursorDown, CursorForward, CursorBackward, HideCursor, ShowCursor, SetCursorShape, ResetCursorShape) to Vt100Output in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T027 [US1] Implement attribute delegations (ResetAttributes, SetAttributes, DisableAutowrap, EnableAutowrap) to Vt100Output in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T028 [US1] Implement title and bell delegations (SetTitle, ClearTitle, Bell, AskForCpr, ResetCursorKeyMode) to Vt100Output in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T029 [US1] Implement alternate screen delegations (EnterAlternateScreen, QuitAlternateScreen) to Vt100Output in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T030 [US1] Implement property delegations (Encoding, Stdout, Fileno) to Vt100Output in src/Stroke/Output/Windows/Windows10Output.cs

**Checkpoint**: User Story 1 complete - VT100 rendering works with mode switching during flush

---

## Phase 4: User Story 2 - Console Size and Buffer Operations (Priority: P2)

**Goal**: Support terminal dimension queries and console-specific operations via Win32Output delegation

**Independent Test**: Create Windows10Output instance, call GetSize(), GetRowsBelowCursorPosition(), ScrollBufferToPrompt() and verify results

### Tests for User Story 2

- [x] T031 [P] [US2] Test GetSize() delegates to Win32Output in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T032 [P] [US2] Test GetRowsBelowCursorPosition() delegates to Win32Output in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T033 [P] [US2] Test ScrollBufferToPrompt() delegates to Win32Output in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T034 [P] [US2] Test EnableMouseSupport() delegates to Win32Output in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T035 [P] [US2] Test DisableMouseSupport() delegates to Win32Output in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T036 [P] [US2] Test EnableBracketedPaste() delegates to Win32Output in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs
- [x] T037 [P] [US2] Test DisableBracketedPaste() delegates to Win32Output in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs

### Implementation for User Story 2

- [x] T038 [P] [US2] Implement GetSize() delegation to Win32Output in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T039 [P] [US2] Implement GetRowsBelowCursorPosition() delegation to Win32Output in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T040 [P] [US2] Implement ScrollBufferToPrompt() delegation to Win32Output in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T041 [P] [US2] Implement mouse support delegations (EnableMouseSupport, DisableMouseSupport) to Win32Output in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T042 [P] [US2] Implement bracketed paste delegations (EnableBracketedPaste, DisableBracketedPaste) to Win32Output in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T043 [US2] Expose Win32Output and Vt100Output public properties in src/Stroke/Output/Windows/Windows10Output.cs

**Checkpoint**: User Story 2 complete - console operations delegate correctly to Win32Output

---

## Phase 5: User Story 3 - VT100 Support Detection (Priority: P3)

**Goal**: Provide runtime detection of VT100 support for output factory decisions

**Independent Test**: Call WindowsVt100Support.IsVt100Enabled() and verify correct detection on various Windows environments

### Tests for User Story 3

- [x] T044 [P] [US3] Test IsVt100Enabled() returns true when SetConsoleMode succeeds in tests/Stroke.Tests/Output/Windows/WindowsVt100SupportTests.cs
- [x] T045 [P] [US3] Test IsVt100Enabled() returns false when GetConsoleMode fails in tests/Stroke.Tests/Output/Windows/WindowsVt100SupportTests.cs
- [x] T046 [P] [US3] Test IsVt100Enabled() restores original console mode (no side effects) in tests/Stroke.Tests/Output/Windows/WindowsVt100SupportTests.cs

### Implementation for User Story 3

- [x] T047 [US3] Verify WindowsVt100Support.IsVt100Enabled() implementation delegates correctly in src/Stroke/Output/Windows/WindowsVt100Support.cs

**Checkpoint**: User Story 3 complete - VT100 detection works correctly for output factory

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T048 [P] Add XML documentation comments to all public members of Windows10Output in src/Stroke/Output/Windows/Windows10Output.cs
- [x] T049 [P] Add XML documentation comments to all public members of WindowsVt100Support in src/Stroke/Output/Windows/WindowsVt100Support.cs
- [x] T050 Verify 80% test coverage for Windows10Output and WindowsVt100Support per SC-004
- [x] T051 Run quickstart.md validation scenarios: (1) basic ANSI color output, (2) detection pattern with IsVt100Enabled(), (3) true color default, (4) console operations GetSize/ScrollBufferToPrompt, (5) thread-safe flush
- [x] T052 [P] Test IOutput interface compliance (can assign to IOutput, can pass as IOutput parameter) per SC-005 in tests/Stroke.Tests/Output/Windows/Windows10OutputTests.cs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - User stories can proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Shares Windows10Output class with US1 but different methods
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Completely independent (WindowsVt100Support class only)

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Implementation follows test order
- Story complete before moving to next priority (recommended)

### Parallel Opportunities

- All Foundational tasks T003-T008 marked [P] can run in parallel
- All tests within a user story marked [P] can run in parallel
- All implementation tasks marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members
- User Story 3 is completely independent (different class)

---

## Parallel Example: User Story 1 Tests

```bash
# Launch all US1 tests together:
Task: "T009 Test constructor throws ArgumentNullException"
Task: "T010 Test constructor throws PlatformNotSupportedException"
Task: "T011 Test constructor propagates NoConsoleScreenBufferError"
Task: "T012 Test Flush() acquires lock"
Task: "T013 Test Flush() restores console mode in finally"
Task: "T014 Test concurrent Flush() serialization"
Task: "T015 Test Write() delegates to Vt100Output"
Task: "T016 Test WriteRaw() delegates to Vt100Output"
Task: "T017 Test GetDefaultColorDepth() returns Depth24Bit"
Task: "T018 Test GetDefaultColorDepth() with override"
Task: "T019 Test RespondsToCpr returns false"
```

## Parallel Example: User Story 1 Implementation (after tests)

```bash
# Launch all parallelizable US1 implementation tasks:
Task: "T021 Implement Write() delegation"
Task: "T022 Implement WriteRaw() delegation"
Task: "T023 Implement GetDefaultColorDepth()"
Task: "T024 Implement RespondsToCpr property"

# Then sequential (depends on above):
Task: "T020 Implement Flush() with VT100 mode switching"
Task: "T025-T030 Implement remaining delegations"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (class skeletons, constructor)
3. Complete Phase 3: User Story 1 (VT100 rendering)
4. **STOP and VALIDATE**: Test VT100 mode switching independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → VT100 rendering works (MVP!)
3. Add User Story 2 → Test independently → Console operations work
4. Add User Story 3 → Test independently → Detection utility available
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (core rendering)
   - Developer B: User Story 3 (detection - fully independent)
3. Developer A continues to User Story 2 after US1 complete
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files or different methods, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Per Constitution VIII: No mocks - tests use real Windows console APIs
