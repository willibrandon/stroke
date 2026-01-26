# Tasks: Input System

**Input**: Design documents from `/specs/014-input-system/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Included per Constitution VIII (Real-World Testing). All tests use xUnit with PipeInput for testing without mocks.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Input/`
- **Tests**: `tests/Stroke.Tests/Input/`
- Platform-specific: `Vt100/`, `Posix/`, `Windows/`, `Pipe/`, `Typeahead/`

---

## Phase 1: Setup (Project Structure)

**Purpose**: Create directory structure and base files for Input System

- [ ] T001 Create directory structure under `src/Stroke/Input/` per plan.md (Vt100/, Posix/, Windows/, Pipe/, Typeahead/)
- [ ] T002 [P] Create `src/Stroke/Input/IInput.cs` with interface stub matching IInput.md contract
- [ ] T003 [P] Create `src/Stroke/Input/Pipe/IPipeInput.cs` with interface stub matching IPipeInput.md contract
- [ ] T004 [P] Create test directory structure under `tests/Stroke.Tests/Input/` per plan.md

---

## Phase 2: Foundational (Core Types & Parser)

**Purpose**: Core types and VT100 parser that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Core Types

- [ ] T005 Implement `KeyPress` readonly record struct in `src/Stroke/Input/KeyPress.cs` per KeyPress.md contract (Key, Data, equality, default data mapping)
- [ ] T006 [P] Write tests for `KeyPress` in `tests/Stroke.Tests/Input/KeyPressTests.cs` (construction, equality, default data for all key types)
- [ ] T007 [P] Implement `AnsiSequences` static class in `src/Stroke/Input/Vt100/AnsiSequences.cs` with FrozenDictionary mappings for all VT100 sequences
- [ ] T008 [P] Write tests for `AnsiSequences` in `tests/Stroke.Tests/Input/AnsiSequencesTests.cs` (lookup correctness, prefixes, reverse lookup)

### VT100 Parser

- [ ] T009 Implement `Vt100Parser` state machine in `src/Stroke/Input/Vt100/Vt100Parser.cs` per Vt100Parser.md contract (Ground, Escape, CsiEntry, CsiParam states)
- [ ] T010 Write tests for `Vt100Parser` basic parsing in `tests/Stroke.Tests/Input/Vt100ParserTests.cs` (single chars, escape sequences, arrow keys, function keys)
- [ ] T011 Add modifier key combination parsing to `Vt100Parser` in `src/Stroke/Input/Vt100/Vt100Parser.cs` (Ctrl+Arrow, Shift+F1, etc. per modifier table)
- [ ] T012 Write tests for `Vt100Parser` modifier combinations in `tests/Stroke.Tests/Input/Vt100ParserTests.cs`
- [ ] T013 Add bracketed paste mode to `Vt100Parser` in `src/Stroke/Input/Vt100/Vt100Parser.cs` (accumulate content between start/end sequences)
- [ ] T014 Write tests for `Vt100Parser` bracketed paste in `tests/Stroke.Tests/Input/Vt100ParserTests.cs` (normal paste, nested sequences, malformed)
- [ ] T015 Add mouse event sequence recognition to `Vt100Parser` in `src/Stroke/Input/Vt100/Vt100Parser.cs` (X10, SGR, urxvt protocols)
- [ ] T016 Write tests for `Vt100Parser` mouse events in `tests/Stroke.Tests/Input/Vt100ParserTests.cs`
- [ ] T017 Add buffer overflow handling to `Vt100Parser` (256-byte limit, emit and reset on overflow)
- [ ] T018 Write tests for `Vt100Parser` buffer limits and edge cases in `tests/Stroke.Tests/Input/Vt100ParserTests.cs`

### Factory and Dummy Input

- [ ] T019 Implement `DummyInput` in `src/Stroke/Input/DummyInput.cs` (always Closed, empty ReadKeys, no-op modes)
- [ ] T020 [P] Write tests for `DummyInput` in `tests/Stroke.Tests/Input/DummyInputTests.cs`
- [ ] T021 Implement `InputFactory` static class in `src/Stroke/Input/InputFactory.cs` per InputFactory.md (Create, CreatePipe with platform detection)
- [ ] T022 [P] Write tests for `InputFactory` in `tests/Stroke.Tests/Input/InputFactoryTests.cs` (platform detection logic, DummyInput fallback)

**Checkpoint**: Foundation ready - VT100 parser, core types, factory complete. User story implementation can now begin.

---

## Phase 3: User Story 1 - Read Keyboard Input (Priority: P1) 🎯 MVP

**Goal**: Applications can receive individual key presses parsed from terminal input

**Independent Test**: Create PipeInput, send character data, verify KeyPress events returned correctly

### Tests for User Story 1

- [ ] T023 [P] [US1] Write tests for basic keyboard input in `tests/Stroke.Tests/Input/Vt100InputTests.cs` (regular chars, special keys via PipeInput)
- [ ] T024 [P] [US1] Write tests for function key input in `tests/Stroke.Tests/Input/Vt100InputTests.cs` (F1-F12 via PipeInput)
- [ ] T025 [P] [US1] Write tests for arrow key input in `tests/Stroke.Tests/Input/Vt100InputTests.cs` (Up, Down, Left, Right via PipeInput)

### Implementation for User Story 1

- [ ] T026 [US1] Implement base `PipeInputBase` in `src/Stroke/Input/Pipe/PipeInputBase.cs` (shared buffer logic, VT100 parser integration)
- [ ] T027 [US1] Implement `PosixPipeInput` in `src/Stroke/Input/Posix/PosixPipeInput.cs` (IPipeInput using OS pipe())
- [ ] T028 [US1] Implement `Win32PipeInput` in `src/Stroke/Input/Windows/Win32PipeInput.cs` (IPipeInput using Windows events)
- [ ] T029 [US1] Write tests for `PosixPipeInput` in `tests/Stroke.Tests/Input/PosixPipeInputTests.cs` (SendText, SendBytes, ReadKeys)
- [ ] T030 [US1] Write tests for `Win32PipeInput` in `tests/Stroke.Tests/Input/Win32PipeInputTests.cs` (SendText, SendBytes, ReadKeys)
- [ ] T031 [US1] Wire `InputFactory.CreatePipe()` to return platform-appropriate `IPipeInput` in `src/Stroke/Input/InputFactory.cs`
- [ ] T032 [US1] Verify all User Story 1 acceptance scenarios pass using PipeInput

**Checkpoint**: Basic keyboard input working. Applications can read characters, special keys, function keys, and arrow keys via PipeInput testing.

---

## Phase 4: User Story 2 - Parse VT100 Escape Sequences (Priority: P1)

**Goal**: Automatic parsing of VT100/ANSI escape sequences into meaningful key press events

**Independent Test**: Feed raw escape sequence bytes via PipeInput, verify correct KeyPress events produced

### Tests for User Story 2

- [ ] T033 [P] [US2] Write tests for escape sequence parsing in `tests/Stroke.Tests/Input/Vt100ParserTests.cs` (multi-char sequences, all navigation keys)
- [ ] T034 [P] [US2] Write tests for partial sequence buffering in `tests/Stroke.Tests/Input/Vt100ParserTests.cs` (incremental input)
- [ ] T035 [P] [US2] Write tests for standalone Escape detection in `tests/Stroke.Tests/Input/Vt100ParserTests.cs` (timeout/flush)
- [ ] T036 [P] [US2] Write tests for bracketed paste via PipeInput in `tests/Stroke.Tests/Input/PipeInputBracketedPasteTests.cs`

### Implementation for User Story 2

- [ ] T037 [US2] Add Flush() timeout logic to `PipeInputBase` in `src/Stroke/Input/Pipe/PipeInputBase.cs` (50-100ms timer for standalone Escape)
- [ ] T038 [US2] Add FlushKeys() implementation to pipe inputs for escape sequence timeout handling
- [ ] T039 [US2] Verify all User Story 2 acceptance scenarios pass (escape sequences, buffering, flush, bracketed paste)

**Checkpoint**: VT100 parsing working. All escape sequences correctly mapped to keys, standalone Escape detectable.

---

## Phase 5: User Story 3 - Raw Mode Terminal Control (Priority: P1)

**Goal**: Put terminal into raw mode for immediate character-by-character input without echo

**Independent Test**: Enter raw mode, type characters, verify received immediately without echo

### Tests for User Story 3

- [ ] T040 [P] [US3] Write tests for `RawModeContext` in `tests/Stroke.Tests/Input/RawModeContextTests.cs` (enter/exit, dispose pattern)
- [ ] T041 [P] [US3] Write tests for raw mode on non-TTY in `tests/Stroke.Tests/Input/RawModeContextTests.cs` (graceful no-op)

### Implementation for User Story 3 (POSIX)

- [ ] T042 [US3] Implement `Termios` P/Invoke wrapper in `src/Stroke/Input/Posix/Termios.cs` (tcgetattr, tcsetattr, termios struct)
- [ ] T043 [US3] Implement `RawModeContext` for POSIX in `src/Stroke/Input/Vt100/RawModeContext.cs` (termios flags per spec table)
- [ ] T044 [US3] Add EINTR retry logic to POSIX read operations in `src/Stroke/Input/Posix/PosixStdinReader.cs`

### Implementation for User Story 3 (Windows)

- [ ] T045 [US3] Implement `ConsoleApi` P/Invoke wrapper in `src/Stroke/Input/Windows/ConsoleApi.cs` (GetConsoleMode, SetConsoleMode)
- [ ] T046 [US3] Implement `Win32RawMode` in `src/Stroke/Input/Windows/Win32RawMode.cs` (console mode flags per spec table)

### Integration for User Story 3

- [ ] T047 [US3] Wire RawMode() in `Vt100Input` to create `RawModeContext`
- [ ] T048 [US3] Wire RawMode() in `Win32Input` to create `Win32RawMode`
- [ ] T049 [US3] Write integration tests for raw mode in `tests/Stroke.Tests/Input/RawModeIntegrationTests.cs` (requires real terminal)
- [ ] T050 [US3] Verify all User Story 3 acceptance scenarios pass

**Checkpoint**: Raw mode working on both platforms. Applications can enter/exit raw mode, terminal settings restored correctly.

---

## Phase 6: User Story 4 - Cooked Mode Restoration (Priority: P2)

**Goal**: Temporarily restore cooked mode while in raw mode for subprocess execution

**Independent Test**: Enter raw mode, then cooked mode, verify line buffering and echo restored, exit both

### Tests for User Story 4

- [ ] T051 [P] [US4] Write tests for `CookedModeContext` in `tests/Stroke.Tests/Input/CookedModeContextTests.cs` (enter/exit, nesting)

### Implementation for User Story 4

- [ ] T052 [US4] Implement `CookedModeContext` for POSIX in `src/Stroke/Input/Vt100/CookedModeContext.cs` (restore terminal flags)
- [ ] T053 [US4] Implement cooked mode for Windows in `src/Stroke/Input/Windows/Win32CookedMode.cs`
- [ ] T054 [US4] Wire CookedMode() in `Vt100Input` and `Win32Input`
- [ ] T055 [US4] Add reference counting for nested mode contexts per FR-019
- [ ] T056 [US4] Verify all User Story 4 acceptance scenarios pass

**Checkpoint**: Cooked mode restoration working. Applications can temporarily exit raw mode for subprocesses.

---

## Phase 7: User Story 5 - Cross-Platform Input Support (Priority: P2)

**Goal**: Input system works correctly on Windows, macOS, and Linux

**Independent Test**: Run key input tests on each platform in CI

### Tests for User Story 5

- [ ] T057 [P] [US5] Write cross-platform tests in `tests/Stroke.Tests/Input/CrossPlatformInputTests.cs` (factory creates correct type)

### Implementation for User Story 5 (POSIX)

- [ ] T058 [US5] Implement `PosixStdinReader` in `src/Stroke/Input/Posix/PosixStdinReader.cs` (non-blocking I/O, select/poll integration)
- [ ] T059 [US5] Implement `Vt100Input` in `src/Stroke/Input/Vt100/Vt100Input.cs` (IInput using PosixStdinReader + Vt100Parser)
- [ ] T060 [US5] Write tests for `Vt100Input` in `tests/Stroke.Tests/Input/Vt100InputTests.cs`

### Implementation for User Story 5 (Windows)

- [ ] T061 [US5] Implement `ConsoleInputReader` (legacy) in `src/Stroke/Input/Windows/ConsoleInputReader.cs` (KEY_EVENT translation)
- [ ] T062 [US5] Implement `Vt100ConsoleInputReader` (Win10+) in `src/Stroke/Input/Windows/Vt100ConsoleInputReader.cs` (VT100 mode)
- [ ] T063 [US5] Implement `Win32Input` in `src/Stroke/Input/Windows/Win32Input.cs` (IInput with mode detection)
- [ ] T064 [US5] Write tests for `Win32Input` in `tests/Stroke.Tests/Input/Win32InputTests.cs`

### Factory Integration for User Story 5

- [ ] T065 [US5] Wire `InputFactory.Create()` to return `Vt100Input` (POSIX) or `Win32Input` (Windows) or `DummyInput` (fallback)
- [ ] T066 [US5] Add stdin/TTY detection logic to `InputFactory.Create()` per selection table
- [ ] T067 [US5] Verify all User Story 5 acceptance scenarios pass on all platforms

**Checkpoint**: Cross-platform input working. InputFactory correctly creates platform-appropriate input.

---

## Phase 8: User Story 6 - Pipe Input for Testing (Priority: P2)

**Goal**: Programmatically send input data for automated testing without real terminal

**Independent Test**: Create PipeInput, send text/bytes, verify key presses received

### Tests for User Story 6

- [ ] T068 [P] [US6] Write tests for PipeInput SendText in `tests/Stroke.Tests/Input/PipeInputTests.cs`
- [ ] T069 [P] [US6] Write tests for PipeInput SendBytes in `tests/Stroke.Tests/Input/PipeInputTests.cs`
- [ ] T070 [P] [US6] Write tests for PipeInput Close/EOF in `tests/Stroke.Tests/Input/PipeInputTests.cs`
- [ ] T071 [P] [US6] Write tests for PipeInput thread safety in `tests/Stroke.Tests/Input/PipeInputThreadSafetyTests.cs`

### Implementation for User Story 6

- [ ] T072 [US6] Add thread-safe SendBytes/SendText to `PipeInputBase` with Lock synchronization
- [ ] T073 [US6] Add encoding handling (UTF-8) to SendText in `PipeInputBase`
- [ ] T074 [US6] Add Close() and Closed property to pipe inputs
- [ ] T075 [US6] Add exception handling (ObjectDisposedException) for operations after Close()
- [ ] T076 [US6] Verify all User Story 6 acceptance scenarios pass

**Checkpoint**: PipeInput fully working. All acceptance tests can run without real terminal.

---

## Phase 9: User Story 7 - Event Loop Integration (Priority: P3)

**Goal**: Input sources integrate with async event loops for efficient waiting

**Independent Test**: Attach input to callback, send data, verify callback invoked

### Tests for User Story 7

- [ ] T077 [P] [US7] Write tests for Attach/Detach in `tests/Stroke.Tests/Input/EventLoopIntegrationTests.cs`
- [ ] T078 [P] [US7] Write tests for multiple Attach calls (stack semantics) in `tests/Stroke.Tests/Input/EventLoopIntegrationTests.cs`
- [ ] T079 [P] [US7] Write tests for Close during Attach in `tests/Stroke.Tests/Input/EventLoopIntegrationTests.cs`

### Implementation for User Story 7

- [ ] T080 [US7] Implement Attach() with callback stack in `InputBase` or per-implementation
- [ ] T081 [US7] Implement Detach() returning reattach disposable
- [ ] T082 [US7] Add non-blocking mode support to `PosixStdinReader` for event loop integration
- [ ] T083 [US7] Add WaitForSingleObject with timeout to `Win32Input` for event loop integration
- [ ] T084 [US7] Wire FileNo() to return file descriptor/handle for external event loop registration
- [ ] T085 [US7] Verify all User Story 7 acceptance scenarios pass

**Checkpoint**: Event loop integration working. Inputs can be attached to async loops with callbacks.

---

## Phase 10: User Story 8 - Mouse Event Detection (Priority: P3)

**Goal**: Receive mouse events (clicks, scrolls) from terminals that support mouse reporting

**Independent Test**: Send mouse escape sequence via PipeInput, verify mouse event detected

### Tests for User Story 8

- [ ] T086 [P] [US8] Write tests for X10 mouse protocol in `tests/Stroke.Tests/Input/MouseEventParserTests.cs`
- [ ] T087 [P] [US8] Write tests for SGR mouse protocol in `tests/Stroke.Tests/Input/MouseEventParserTests.cs`
- [ ] T088 [P] [US8] Write tests for urxvt mouse protocol in `tests/Stroke.Tests/Input/MouseEventParserTests.cs`

### Implementation for User Story 8

- [ ] T089 [US8] Add mouse protocol detection to `Vt100Parser` state machine
- [ ] T090 [US8] Implement X10 coordinate decoding (button, x+32, y+32)
- [ ] T091 [US8] Implement SGR coordinate decoding (decimal params)
- [ ] T092 [US8] Implement urxvt coordinate decoding (decimal params)
- [ ] T093 [US8] Ensure `Keys.Vt100MouseEvent` KeyPress has raw sequence in Data for caller parsing
- [ ] T094 [US8] Verify all User Story 8 acceptance scenarios pass

**Checkpoint**: Mouse event detection working. All three protocols recognized.

---

## Phase 11: Typeahead Buffer

**Purpose**: Store excess key presses for next prompt (supports typeahead)

- [ ] T095 Implement `TypeaheadBuffer` in `src/Stroke/Input/Typeahead/TypeaheadBuffer.cs` (thread-safe ConcurrentDictionary)
- [ ] T096 [P] Write tests for `TypeaheadBuffer` in `tests/Stroke.Tests/Input/TypeaheadBufferTests.cs` (Store, Get, Clear, thread safety)
- [ ] T097 Wire TypeaheadHash() in all IInput implementations
- [ ] T098 Add typeahead integration to ReadKeys() flow (check buffer first, store excess)

---

## Phase 12: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T099 Add XML documentation comments to all public types in `src/Stroke/Input/`
- [ ] T100 [P] Verify NFR-001: Raw mode entry/exit <10ms (add benchmark if needed)
- [ ] T101 [P] Verify NFR-002: FrozenDictionary O(1) lookups in AnsiSequences
- [ ] T102 [P] Verify NFR-003: Zero allocation for single character input (add benchmark using BenchmarkDotNet [MemoryDiagnoser])
- [ ] T102.5 [P] Verify NFR-004: Parser buffer reuse minimizes GC pressure (inspect Vt100Parser implementation, add benchmark if needed)
- [ ] T103 [P] Verify NFR-005: PipeInput 10,000+ keys/second (add benchmark)
- [ ] T104 Run `tests/Stroke.Tests/Input/` suite and verify 80% coverage per SC-007
- [ ] T105 Run `quickstart.md` validation scenarios
- [ ] T106 Verify all exception handling per spec Exception Handling tables

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-10)**: All depend on Foundational phase completion
  - US1-US3 (P1): Should be done first as they are P1 priority
  - US4-US6 (P2): Can start after Foundational, ideally after P1 stories
  - US7-US8 (P3): Can start after Foundational, ideally after P2 stories
- **Typeahead (Phase 11)**: Depends on basic IInput implementations (after US5)
- **Polish (Phase 12)**: Depends on all user stories being complete

### User Story Dependencies

| Story | Priority | Depends On | Can Run With |
|-------|----------|------------|--------------|
| US1 - Read Keyboard | P1 | Foundation | US2 (shares parser) |
| US2 - VT100 Parsing | P1 | Foundation | US1 (same parser) |
| US3 - Raw Mode | P1 | Foundation | Independent |
| US4 - Cooked Mode | P2 | US3 (uses same contexts) | Independent after US3 |
| US5 - Cross-Platform | P2 | US1, US2, US3 | Integrates all P1 work |
| US6 - Pipe Testing | P2 | US1 (PipeInput) | Independent |
| US7 - Event Loop | P3 | US5 (needs full input) | US8 |
| US8 - Mouse Events | P3 | US2 (parser) | US7 |

### Within Each User Story

- Tests written first, verify they FAIL
- Core types/models before services
- Platform-specific implementations after shared logic
- Story complete when all acceptance scenarios pass

### Parallel Opportunities

**Phase 1 (Setup)**:
- T002, T003, T004 can all run in parallel

**Phase 2 (Foundational)**:
- T006, T007, T008 can run in parallel (after T005)
- T020, T022 can run in parallel (after T019, T021)

**User Stories**:
- US1, US2 share the parser but test different aspects - can overlap
- US3 (Raw Mode) is independent once Foundation complete
- US6 (Pipe Testing) can run in parallel with US4, US5
- US7, US8 can run in parallel (different features)

---

## Parallel Example: Foundational Phase

```bash
# After T005 (KeyPress) is done:
Task: T006 - Write tests for KeyPress
Task: T007 - Implement AnsiSequences
Task: T008 - Write tests for AnsiSequences
# All can run in parallel (different files)
```

## Parallel Example: User Story Tests

```bash
# User Story 1 tests (can all run in parallel):
Task: T023 - Write tests for basic keyboard input
Task: T024 - Write tests for function key input
Task: T025 - Write tests for arrow key input

# User Story 6 tests (can all run in parallel):
Task: T068 - Write tests for PipeInput SendText
Task: T069 - Write tests for PipeInput SendBytes
Task: T070 - Write tests for PipeInput Close/EOF
Task: T071 - Write tests for PipeInput thread safety
```

---

## Implementation Strategy

### MVP First (User Stories 1-3)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (VT100 parser, core types)
3. Complete Phase 3-5: User Stories 1, 2, 3 (P1 priority)
4. **STOP and VALIDATE**: All P1 functionality working with PipeInput
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Parser and core types ready
2. Add US1 (Read Keyboard) → Test with PipeInput → MVP!
3. Add US2 (VT100 Parsing) → Full escape sequence support
4. Add US3 (Raw Mode) → Real terminal support
5. Add US4-6 (P2) → Cooked mode, cross-platform, pipe testing
6. Add US7-8 (P3) → Event loop, mouse events
7. Polish → Documentation, benchmarks, coverage

### Parallel Team Strategy

With multiple developers after Foundational:

- **Developer A**: US1 + US2 (keyboard input + parsing - tightly coupled)
- **Developer B**: US3 + US4 (terminal modes - tightly coupled)
- **Developer C**: US5 (cross-platform - integration work)
- **Developer D**: US6 + US7 + US8 (testing + advanced features)

---

## Summary

**Total Tasks**: 107
**Tasks by Phase**:
- Phase 1 (Setup): 4 tasks
- Phase 2 (Foundational): 18 tasks
- Phase 3 (US1 - Read Keyboard): 10 tasks
- Phase 4 (US2 - VT100 Parsing): 7 tasks
- Phase 5 (US3 - Raw Mode): 11 tasks
- Phase 6 (US4 - Cooked Mode): 6 tasks
- Phase 7 (US5 - Cross-Platform): 11 tasks
- Phase 8 (US6 - Pipe Testing): 9 tasks
- Phase 9 (US7 - Event Loop): 9 tasks
- Phase 10 (US8 - Mouse Events): 9 tasks
- Phase 11 (Typeahead): 4 tasks
- Phase 12 (Polish): 9 tasks

**MVP Scope**: Phases 1-5 (US1, US2, US3) = 50 tasks
**Parallel Opportunities**: 40+ tasks marked [P]
**Independent Test Criteria**: Each user story has explicit "Independent Test" defined

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- Tests use PipeInput (no mocks per Constitution VIII)
- Platform-specific code (P/Invoke) tested via integration tests on target platforms
- Stop at any checkpoint to validate story independently
- Commit after each task or logical group
