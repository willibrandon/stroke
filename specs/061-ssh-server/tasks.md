# Tasks: SSH Server Integration

**Input**: Design documents from `/specs/061-ssh-server/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/ ✓, quickstart.md ✓

**Tests**: Included (required per Constitution VIII - 80% coverage, real SSH connections, no mocks)

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Contrib/Ssh/`
- **Tests**: `tests/Stroke.Tests/Contrib/Ssh/`
- **Examples**: `examples/Stroke.Examples.Ssh/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, dependencies, and base abstractions

- [ ] T001 Add FxSsh NuGet package reference (v1.3.0) to `src/Stroke/Stroke.csproj`
- [ ] T002 Add SSH.NET NuGet package reference to `tests/Stroke.Tests/Stroke.Tests.csproj` (for integration test client)
- [ ] T003 [P] Create `src/Stroke/Contrib/Ssh/` directory structure
- [ ] T004 [P] Create `tests/Stroke.Tests/Contrib/Ssh/` directory structure

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core abstractions and infrastructure that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 Create `ISshChannel` interface in `src/Stroke/Contrib/Ssh/ISshChannel.cs` with methods: `Write(string)`, `Close()`, `GetTerminalType()`, `GetTerminalSize()`, `GetEncoding()`, `SetLineMode(bool)` per FR-014, FR-015
- [ ] T006 Create `SshChannel` FxSsh adapter class in `src/Stroke/Contrib/Ssh/SshChannel.cs` implementing `ISshChannel`, wrapping FxSsh channel with cached terminal info
- [ ] T007 [P] Create `SshChannelStdout` TextWriter in `src/Stroke/Contrib/Ssh/SshChannelStdout.cs` with LF→CRLF conversion, `IsAtty=true`, delegating to `ISshChannel` per FR-006, FR-007

**Checkpoint**: Foundation ready - channel abstraction enables all session/server work

---

## Phase 3: User Story 1 - Run Interactive Application Over SSH (Priority: P1) 🎯 MVP

**Goal**: Enable developers to expose Stroke applications over SSH with session isolation

**Independent Test**: Start SSH server with interact callback, connect via SSH client, type input, verify output rendered correctly

### Tests for User Story 1

- [ ] T008 [P] [US1] Create `tests/Stroke.Tests/Contrib/Ssh/SshServerTests.cs` with constructor validation tests (port range, null interact, host key validation) and `BeginAuth_ReturnsFalseByDefault` test per FR-010
- [ ] T009 [P] [US1] Create `tests/Stroke.Tests/Contrib/Ssh/SshSessionTests.cs` with session property tests (Interact, EnableCpr, GetSize defaults) and `SessionStart_CallsSetLineModeFalse` test per FR-011
- [ ] T010 [P] [US1] Create `tests/Stroke.Tests/Contrib/Ssh/SshChannelStdoutTests.cs` with Write/Encoding/IsAtty/Flush tests
- [ ] T011 [P] [US1] Create `tests/Stroke.Tests/Contrib/Ssh/SshServerIntegrationTests.cs` with SSH.NET client→server tests for: basic connect, prompt interaction, session isolation with 3 concurrent clients

### Implementation for User Story 1

- [ ] T012 [US1] Create `PromptToolkitSshSession` class in `src/Stroke/Contrib/Ssh/PromptToolkitSshSession.cs` with:
  - Constructor accepting `ISshChannel`, `Func<PromptToolkitSshSession, Task>` interact, `bool enableCpr`
  - Properties: `Interact`, `EnableCpr`, `AppSession?`, `InteractTask?`
  - `GetSize()` returning current size or default 79×20 per FR-004
  - `DataReceived(string data)` routing to PipeInput per FR-005
  - Internal state: `_size`, `_pipeInput`, `_vt100Output`, `_closed` with `Lock` synchronization per Constitution XI
- [ ] T013 [US1] Create `PromptToolkitSshServer` class in `src/Stroke/Contrib/Ssh/PromptToolkitSshServer.cs` with:
  - Constructor: `host`, `port`, `interact`, `hostKeyPath`, `encoding`, `style`, `enableCpr` per contracts
  - Properties: `Host`, `Port`, `Encoding`, `Style`, `EnableCpr`, `Connections`
  - `RunAsync(Action? readyCallback, CancellationToken)` starting FxSsh server, wiring events per FR-002
  - `ConcurrentDictionary` for thread-safe session tracking per Constitution XI
- [ ] T014 [US1] Implement FxSsh event wiring in `PromptToolkitSshServer`:
  - `ConnectionAccepted` → create session, add to Connections
  - `CommandOpened` → create PipeInput, Vt100Output, AppSession, start interact task per FR-003
  - `DataReceived` → route to session's `DataReceived()` per FR-005
  - Channel close → dispose resources, remove from Connections per FR-012, FR-013
- [ ] T015 [US1] Add virtual `BeginAuth(string username)` method returning `false` by default per FR-010
- [ ] T016 [US1] Add virtual `CreateSession(ISshChannel channel)` factory method per FR-009
- [ ] T017 [US1] Implement session lifecycle: `SetLineMode(false)` call on start per FR-011, resource disposal order (PipeInput→Vt100Output→AppSession→Channel) per spec

**Checkpoint**: User Story 1 complete - can run interactive Stroke applications over SSH with multiple isolated sessions

---

## Phase 4: User Story 2 - Terminal Size Tracking (Priority: P1)

**Goal**: Detect terminal resize and trigger application re-render

**Independent Test**: Connect via SSH, resize terminal, verify application receives resize events and re-renders

### Tests for User Story 2

- [ ] T018 [P] [US2] Add `TerminalSizeChanged_UpdatesSize_TriggersInvalidation` test to `SshSessionTests.cs`
- [ ] T019 [P] [US2] Add `WindowChange_ReflectedWithin100ms` latency test to `SshServerIntegrationTests.cs` per SC-003

### Implementation for User Story 2

- [ ] T020 [US2] Implement `TerminalSizeChanged(int width, int height)` in `PromptToolkitSshSession` per FR-008:
  - Clamp dimensions to 1-500 per spec edge cases
  - Update `_size` under lock
  - Trigger app invalidation via `AppSession.Invalidate()` if running
- [ ] T021 [US2] Wire FxSsh `WindowChange` event in `PromptToolkitSshServer` to call session's `TerminalSizeChanged()`
- [ ] T022 [US2] Wire FxSsh `PtyReceived` event to store initial terminal size and type in `SshChannel`

**Checkpoint**: User Story 2 complete - terminal resize events trigger re-render within 100ms

---

## Phase 5: User Story 3 - Line Ending Conversion (Priority: P2)

**Goal**: Transparent LF→CRLF conversion per NVT specification

**Independent Test**: Send text with newlines through SSH channel, verify CRLF sequences sent to client

### Tests for User Story 3

- [ ] T023 [P] [US3] Add `Write_ConvertsLfToCrlf` test to `SshChannelStdoutTests.cs`
- [ ] T024 [P] [US3] Add `Write_PreservesExistingCrlf` test to `SshChannelStdoutTests.cs`
- [ ] T025 [P] [US3] Add integration test verifying newlines display correctly on SSH client in `SshServerIntegrationTests.cs`

### Implementation for User Story 3

> Note: Core implementation already in T007. Tests T023-T025 verify behavior. No additional implementation tasks needed—this phase validates via tests only.

**Checkpoint**: User Story 3 complete - output renders correctly on SSH clients

---

## Phase 6: User Story 4 - Cursor Position Request Support (Priority: P2)

**Goal**: Optional CPR escape sequence support for precise cursor positioning

**Independent Test**: Run application with CPR enabled, trigger cursor position query, verify response received

### Tests for User Story 4

- [ ] T028 [P] [US4] Add `EnableCpr_True_AllowsCprSequences` test to `SshSessionTests.cs`
- [ ] T029 [P] [US4] Add `EnableCpr_False_NoCprSequencesSent` test to `SshSessionTests.cs`

### Implementation for User Story 4

- [ ] T030 [US4] Verify `EnableCpr` property flows from server to session to `Vt100Output` constructor
- [ ] T031 [US4] Ensure `Vt100Output` respects `enableCpr` flag when sending `ESC[6n` sequences

**Checkpoint**: User Story 4 complete - CPR works over SSH when enabled

---

## Phase 7: User Story 5 - Session Cleanup on Disconnect (Priority: P2)

**Goal**: Proper resource cleanup on graceful and abrupt disconnect

**Independent Test**: Connect, run application, disconnect abruptly, verify no resource leaks

### Tests for User Story 5

- [ ] T032 [P] [US5] Create `tests/Stroke.Tests/Contrib/Ssh/SshServerLifecycleTests.cs` with `GracefulDisconnect_CleansUpResources` test
- [ ] T033 [P] [US5] Add `AbruptDisconnect_CleansUpResources` test to `SshServerLifecycleTests.cs`
- [ ] T034 [P] [US5] Add `Cleanup_CompletesWithin5Seconds` timing test per SC-004 to `SshServerLifecycleTests.cs`

### Implementation for User Story 5

- [ ] T035 [US5] Implement cleanup sequence on channel close: dispose PipeInput → Vt100Output → AppSession → remove from Connections per FR-012
- [ ] T036 [US5] Add exception logging when interact callback throws, then close channel per spec error handling
- [ ] T037 [US5] Add broken pipe exception handling (catch, log, mark closed) per spec edge cases
- [ ] T038 [US5] Implement graceful shutdown in `RunAsync`: stop accepting → cancel sessions → wait 5s → force-close per spec

**Checkpoint**: User Story 5 complete - sessions clean up properly on any disconnect type

---

## Phase 8: Concurrency & Stress Testing

**Purpose**: Thread safety verification per Constitution XI

### Tests

- [ ] T039 [P] Create `tests/Stroke.Tests/Contrib/Ssh/SshServerConcurrencyTests.cs` with:
  - `ConcurrentConnections_100Sessions_NoFailures` per SC-002
  - `RapidConnectDisconnect_1000Cycles_NoLeaks`
  - `MixedOperations_ConcurrentSendResizeDisconnect_ThreadSafe`
- [ ] T040 [P] Add memory leak test: 100 connect/disconnect cycles, verify no heap growth per spec

### Implementation

- [ ] T041 Verify `Connections` uses `ConcurrentDictionary<PromptToolkitSshSession, byte>` for O(1) add/remove
- [ ] T042 Verify all session state mutations use `Lock.EnterScope()` pattern
- [ ] T043 Add data buffering for pre-session data (64KB limit per spec edge cases)

**Checkpoint**: Concurrency requirements met - 100 concurrent sessions without resource exhaustion

---

## Phase 9: Example Application

**Purpose**: Port of Python PTK's asyncssh-server.py example

- [ ] T044 Create `examples/Stroke.Examples.Ssh/Stroke.Examples.Ssh.csproj` with Stroke reference
- [ ] T045 Create `examples/Stroke.Examples.Ssh/Program.cs` with command-line router for examples
- [ ] T046 Create `examples/Stroke.Examples.Ssh/Examples/AsyncsshServer.cs` porting asyncssh-server.py:
  - Progress bar demo
  - Normal prompt with input
  - Autocompletion (animal names)
  - HTML syntax highlighting
  - Yes/no dialog
  - Input dialog
- [ ] T047 Add README with usage instructions for running examples

**Checkpoint**: Example demonstrates all SSH server capabilities

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, coverage, validation

- [ ] T048 Verify 80% code coverage for `Stroke.Contrib.Ssh` namespace per SC-005
- [ ] T049 Add XML documentation comments to all public types per Constitution standards
- [ ] T050 Run quickstart.md validation - manually test minimal example works
- [ ] T051 Verify API mapping completeness per SC-006 - all Python PTK public APIs ported
- [ ] T052 Update CLAUDE.md with SSH server as completed feature

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies - start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 - BLOCKS all user stories
- **Phases 3-7 (User Stories)**: All depend on Phase 2 completion
  - US1 (P1): Core functionality - **implement first**
  - US2 (P1): Can proceed after US1 core classes exist
  - US3 (P2): Can proceed after T007 (SshChannelStdout)
  - US4 (P2): Can proceed after US1 session implementation
  - US5 (P2): Can proceed after US1 server implementation
- **Phase 8 (Concurrency)**: Depends on US1, US2, US5 complete
- **Phase 9 (Examples)**: Depends on US1, US2 complete
- **Phase 10 (Polish)**: Depends on all user stories complete

### User Story Dependencies

| Story | Depends On | Notes |
|-------|------------|-------|
| US1 | Phase 2 only | Core MVP - no dependencies |
| US2 | US1 (session class) | Needs session to add resize |
| US3 | T007 (SshChannelStdout) | Already implemented in foundational |
| US4 | US1 (session+server) | Needs I/O pipeline |
| US5 | US1 (server+session) | Needs lifecycle to test cleanup |

### Within Each User Story

- Tests written FIRST, must FAIL before implementation
- Interface/abstraction before implementation
- Core classes before event wiring
- Story complete before moving to next priority

### Parallel Opportunities

**Phase 2 (Foundational)**:
```
Parallel: T005 (ISshChannel) | T007 (SshChannelStdout)
Then: T006 (SshChannel) depends on T005
```

**Phase 3 (US1) Tests**:
```
Parallel: T008 | T009 | T010 | T011
```

**Phase 3 (US1) Implementation**:
```
T012 (Session) → T013 (Server) → T014 (Events) → T015+T016+T017
```

**Cross-Story Parallelism** (after US1):
```
Team A: US2 (resize)
Team B: US3 (line endings) - mostly verification
Team C: US4 (CPR)
Team D: US5 (cleanup)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T004)
2. Complete Phase 2: Foundational (T005-T007)
3. Complete Phase 3: User Story 1 (T008-T017)
4. **STOP and VALIDATE**: Test with SSH client, verify prompts work
5. Can deploy/demo SSH server with basic functionality

### Incremental Delivery

1. **MVP**: Setup + Foundational + US1 → Basic SSH server works
2. **+US2**: Terminal resize → Responsive layouts
3. **+US3**: Line endings → Correct display (already mostly done)
4. **+US4**: CPR support → Advanced positioning
5. **+US5**: Cleanup → Production-ready reliability
6. **+Concurrency**: Stress tests → Verified thread safety
7. **+Example**: Demo app → Showcase capabilities

### Task Count Summary

| Phase | Task Range | Count |
|-------|------------|-------|
| Setup | T001-T004 | 4 |
| Foundational | T005-T007 | 3 |
| US1 | T008-T017 | 10 |
| US2 | T018-T022 | 5 |
| US3 | T023-T025 | 3 |
| US4 | T028-T031 | 4 |
| US5 | T032-T038 | 7 |
| Concurrency | T039-T043 | 5 |
| Examples | T044-T047 | 4 |
| Polish | T048-T052 | 5 |
| **Total** | | **50** |

---

## Notes

- All tests use real SSH connections per Constitution VIII (no mocks)
- SSH.NET is the test client (MIT license, well-maintained)
- FxSsh is the server library (MIT license, cross-platform)
- Lock + EnterScope() pattern required for all mutable state per Constitution XI
- Port 0 (auto-assign) used in tests to avoid conflicts
- Each test generates ephemeral host key for isolation
