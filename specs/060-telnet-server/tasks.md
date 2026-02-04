# Tasks: Telnet Server

**Input**: Design documents from `/specs/060-telnet-server/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/ ✓, quickstart.md ✓
**Branch**: `060-telnet-server`

**Tests**: Required per SC-008 (80% code coverage target)

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, etc.)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Contrib/Telnet/`
- **Tests**: `tests/Stroke.Tests/Contrib/Telnet/`
- **Examples**: `examples/Stroke.Examples.Telnet/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and directory structure

- [ ] T001 Create directory structure `src/Stroke/Contrib/Telnet/` and `tests/Stroke.Tests/Contrib/Telnet/`
- [ ] T002 [P] Verify existing dependencies exist: `Stroke.Application` (AppSession, AppContext), `Stroke.Output` (Vt100Output), `Stroke.Input` (IPipeInput, PipeInput)
- [ ] T003 [P] Verify `Stroke.Core.Size` struct is available for terminal dimensions

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T004 Implement `TelnetConstants` static class with all protocol byte constants in `src/Stroke/Contrib/Telnet/TelnetConstants.cs`
  - Commands: IAC (255), DO (253), DONT (254), WILL (251), WONT (252), SB (250), SE (240), NOP (0)
  - Simple commands: DM (242), BRK (243), IP (244), AO (245), AYT (246), EC (247), EL (248), GA (249)
  - Options: ECHO (1), SGA (3), TTYPE (24), NAWS (31), LINEMODE (34)
  - Subnegotiation: IS (0), SEND (1), MODE (1)
  - Reference: `contracts/TelnetConstants.md`

- [ ] T005 [P] Write unit tests for `TelnetConstants` in `tests/Stroke.Tests/Contrib/Telnet/TelnetConstantsTests.cs`
  - Verify all constant values match RFC 854 and Python PTK
  - Verify `ToByte()` helper method

- [ ] T006 Implement `TelnetProtocolParser` state machine in `src/Stroke/Contrib/Telnet/TelnetProtocolParser.cs`
  - States: Normal, Iac, IacCommand, Subnegotiation, SubnegotiationIac (per FR-003)
  - Callbacks: DataReceived, SizeReceived, TtypeReceived (per API-011)
  - Handle double-IAC escape (FR-016)
  - Handle NAWS parsing - 4 bytes, big-endian (FR-004)
  - Handle TTYPE parsing - ASCII string after IS marker (FR-005)
  - Handle DO/DONT/WILL/WONT - log and ignore (FR-003a)
  - Handle other commands - log and ignore (FR-003b)
  - Subnegotiation buffer limit 1024 bytes (EC-007)
  - NOT thread-safe - document constraint (TS-003, API-012)
  - Reference: `contracts/TelnetProtocolParser.md`, `data-model.md`

- [ ] T007 [P] Write unit tests for `TelnetProtocolParser` in `tests/Stroke.Tests/Contrib/Telnet/TelnetProtocolParserTests.cs`
  - Test normal data passthrough
  - Test IAC command handling (DO, DONT, WILL, WONT)
  - Test double-IAC escape (0xFF 0xFF → single 0xFF data)
  - Test NAWS parsing with valid 4-byte data
  - Test NAWS with invalid length (log warning, ignore)
  - Test TTYPE parsing with IS marker
  - Test TTYPE without IS marker (fallback)
  - Test partial IAC at buffer end (retain state)
  - Test subnegotiation buffer overflow (1024 limit)
  - Test malformed sequences (log and continue)
  - Test NOP bytes passthrough (FR-003c)

- [ ] T008 Implement `ConnectionStdout` TextWriter in `src/Stroke/Contrib/Telnet/ConnectionStdout.cs`
  - LF → CRLF conversion (FR-006)
  - Buffered writes with Flush()
  - Socket error handling - log but don't throw (ERR-002)
  - IsClosed state with idempotent Close()
  - Thread-safe via Lock (TS-002)
  - IsAtty returns true
  - Reference: `contracts/ConnectionStdout.md`

- [ ] T009 [P] Write unit tests for `ConnectionStdout` in `tests/Stroke.Tests/Contrib/Telnet/ConnectionStdoutTests.cs`
  - Test LF → CRLF conversion
  - Test CRLF → CRCRLF (matching Python PTK behavior)
  - Test Write() with null/empty strings
  - Test Flush() sends buffered data
  - Test Write() after Close() is no-op
  - Test thread safety (concurrent writes)

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Create Network-Accessible REPL (Priority: P1) 🎯 MVP

**Goal**: Developer can create a REPL that users connect to remotely over telnet

**Independent Test**: Start telnet server, connect with client, verify prompt toolkit features work

**Acceptance Scenarios**:
1. TelnetServer accepts incoming connections on specified host/port
2. Connection negotiates terminal capabilities and invokes interact callback
3. User input is received and responses sent back to client

### Tests for User Story 1

- [ ] T010 [P] [US1] Write integration tests in `tests/Stroke.Tests/Contrib/Telnet/TelnetServerIntegrationTests.cs`
  - Test server starts and accepts TCP connection
  - Test server sends initialization sequences (FR-002)
  - Test interact callback is invoked after negotiation
  - Test input/output roundtrip works
  - Test RunAsync with readyCallback timing (API-001, SC-005 <100ms)

### Implementation for User Story 1

- [ ] T011 [US1] Implement `TelnetConnection` class shell in `src/Stroke/Contrib/Telnet/TelnetConnection.cs`
  - Internal constructor with all parameters per contract
  - Properties: Socket, RemoteAddress, Server, Encoding, Style, Size, IsClosed, EnableCpr
  - Size defaults to 80x24 (EC-004)
  - Reference: `contracts/TelnetConnection.md`

- [ ] T012 [US1] Implement `TelnetConnection.Feed()` internal method
  - Create TelnetProtocolParser with callbacks
  - Wire DataReceived to PipeInput.SendBytes()
  - Wire SizeReceived to update Size property (with clamping 1-500)
  - Wire TtypeReceived to trigger Ready state
  - Handle 0x0 size as 1x1 (EC-009)

- [ ] T013 [US1] Implement `TelnetConnection.Send()` method
  - Use Vt100Output with ConnectionStdout
  - Render formatted text with Style
  - No-op if IsClosed (API-005, EC-005)
  - Thread-safe via Lock

- [ ] T014 [US1] Implement `TelnetConnection.EraseScreen()` method
  - Send ESC[2J + ESC[H sequences (API-007)
  - No-op if IsClosed

- [ ] T015 [US1] Implement `TelnetConnection.Close()` method
  - Idempotent (API-008)
  - Close socket, input, output
  - Set IsClosed = true
  - Thread-safe state transition (TS-005)

- [ ] T016 [US1] Implement `TelnetConnection.RunApplicationAsync()` internal method
  - Create isolated PipeInput (FR-007, ISO-001)
  - Create Vt100Output with ConnectionStdout
  - Set up AppSession context
  - Invoke interact callback
  - Handle callback exceptions (ERR-003)

- [ ] T017 [US1] Implement `TelnetServer` class in `src/Stroke/Contrib/Telnet/TelnetServer.cs`
  - Constructor with 6 parameters per contract
  - Port validation 1-65535 (ArgumentOutOfRangeException)
  - Default host "127.0.0.1", port 23
  - Default encoding UTF-8
  - Properties: Host, Port, Encoding, Style, EnableCpr
  - Connections property (thread-safe snapshot via ConcurrentDictionary)
  - Reference: `contracts/TelnetServer.md`

- [ ] T018 [US1] Implement `TelnetServer.RunAsync()` method - socket setup
  - Create Socket with TCP/IP
  - Bind to Host:Port
  - Handle AddressAlreadyInUse exception (EC-003)
  - Start listening
  - Invoke readyCallback after bind (API-001)
  - State transition Created → Running (TS-004)

- [ ] T019 [US1] Implement `TelnetServer.RunAsync()` method - accept loop
  - Accept connections in loop until cancelled
  - Create TelnetConnection for each
  - Send initialization sequences (FR-002, 7 exact sequences)
  - Start connection task
  - Add to Connections set

- [ ] T020 [US1] Implement `TelnetServer.RunAsync()` method - read loop per connection
  - Read from socket in loop
  - Feed data to connection's parser
  - Handle socket errors (ERR-001)
  - Clean up on disconnect (FR-011)
  - Timeout for NAWS/TTYPE negotiation 500ms (EC-011)

- [ ] T021 [US1] Implement deprecated `Start()` and `StopAsync()` methods
  - Mark with [Obsolete] attribute (API-004)
  - Start() launches RunAsync in background
  - StopAsync() cancels and awaits

- [ ] T022 [P] [US1] Write unit tests for `TelnetConnection` in `tests/Stroke.Tests/Contrib/Telnet/TelnetConnectionTests.cs`
  - Test Send() with formatted text
  - Test Send() on closed connection (no-op)
  - Test EraseScreen() sends correct sequences
  - Test Close() is idempotent
  - Test Size property with clamping

- [ ] T023 [P] [US1] Write unit tests for `TelnetServer` in `tests/Stroke.Tests/Contrib/Telnet/TelnetServerTests.cs`
  - Test constructor parameter validation
  - Test port out of range throws
  - Test default values
  - Test Connections property thread safety

**Checkpoint**: US1 complete - basic telnet REPL works. Can connect with client, type input, receive output.

---

## Phase 4: User Story 2 - Terminal Size Negotiation (Priority: P1)

**Goal**: Server automatically detects and responds to client terminal dimensions

**Independent Test**: Connect with telnet client, resize window, verify server receives new dimensions

**Acceptance Scenarios**:
1. Server parses NAWS data and stores terminal dimensions
2. Terminal resize triggers resize event in application
3. Clients without NAWS get default 80x24

### Tests for User Story 2

- [ ] T024 [P] [US2] Write NAWS tests in `tests/Stroke.Tests/Contrib/Telnet/TelnetServerNawsTests.cs`
  - Test NAWS parsing extracts correct dimensions
  - Test NAWS with large values (capped at 500)
  - Test NAWS with zero values (treated as 1)
  - Test client without NAWS uses 80x24 default
  - Test resize triggers Application._OnResize()

### Implementation for User Story 2

- [ ] T025 [US2] Enhance `TelnetConnection.Feed()` to handle NAWS resize events
  - Call SizeReceived callback on NAWS data
  - Clamp values to 1-500 range (EC-004)
  - Update Size property

- [ ] T026 [US2] Implement resize notification to running application (FR-017)
  - Store AppContext during RunApplicationAsync
  - On SizeReceived, invoke app resize handler via context
  - Match Python pattern: `context.run(lambda: get_app()._on_resize())`

- [ ] T027 [US2] Handle NAWS timeout in negotiation (EC-011)
  - If no NAWS within 500ms, use default 80x24
  - Proceed to Ready state anyway
  - Log at Debug level

**Checkpoint**: US2 complete - terminal size negotiation works. Resize events propagate to application.

---

## Phase 5: User Story 3 - Concurrent Connection Handling (Priority: P2)

**Goal**: Multiple users access server simultaneously with independent sessions

**Independent Test**: Start multiple telnet connections, verify each has independent state

**Acceptance Scenarios**:
1. Each connection receives isolated application session
2. Input on one connection doesn't affect others
3. Connections are independent of each other's lifecycle

### Tests for User Story 3

- [ ] T028 [P] [US3] Write concurrency tests in `tests/Stroke.Tests/Contrib/Telnet/TelnetServerConcurrencyTests.cs`
  - Test 50 concurrent connections (SC-001)
  - Test connection isolation (ISO-001/002/003)
  - Test Connections enumeration during add/remove (TS-008)
  - Test concurrent Send() to same connection (TS-002)

### Implementation for User Story 3

- [ ] T029 [US3] Ensure `TelnetServer.Connections` is thread-safe (TS-001)
  - Use ConcurrentDictionary<TelnetConnection, byte> internally
  - Return snapshot on enumeration
  - Atomic add/remove operations

- [ ] T030 [US3] Ensure connection isolation (ISO-001/002/003)
  - Each connection has own PipeInput
  - Each connection has own Vt100Output
  - Each connection has own AppSession
  - No shared mutable state between connections

- [ ] T031 [US3] Implement concurrent interact callback invocation (TS-006)
  - Each callback runs in own async context
  - Callbacks may execute concurrently
  - Isolated exception handling per connection

**Checkpoint**: US3 complete - multiple users work independently. 50+ concurrent connections supported.

---

## Phase 6: User Story 4 - Connection Lifecycle Management (Priority: P2)

**Goal**: Graceful handling of disconnections and server shutdown

**Independent Test**: Forcibly disconnect client, verify cleanup. Stop server, verify all connections close.

**Acceptance Scenarios**:
1. Client disconnect triggers cleanup
2. Server stop closes all connections gracefully
3. Callback exceptions don't crash server

### Tests for User Story 4

- [ ] T032 [P] [US4] Write lifecycle tests in `tests/Stroke.Tests/Contrib/Telnet/TelnetServerLifecycleTests.cs`
  - Test client disconnect triggers cleanup
  - Test cleanup completes within 1 second (SC-004)
  - Test server shutdown cancels all connections (TS-007)
  - Test interact callback exception handling (ERR-003)
  - Test rapid connect/disconnect cycles (EC-006)

### Implementation for User Story 4

- [ ] T033 [US4] Implement client disconnect detection and cleanup (FR-011)
  - Detect socket EOF or error
  - Remove from Connections set
  - Clean up PipeInput, Vt100Output, socket
  - Cleanup within 1 second (SC-004)

- [ ] T034 [US4] Implement server shutdown sequence (TS-007)
  - Stop accepting new connections immediately
  - Cancel in-progress negotiations
  - Signal all running connections to terminate
  - Wait for all cleanup to complete
  - Close listening socket

- [ ] T035 [US4] Implement interact callback exception handling (ERR-003)
  - Catch exceptions in connection run loop
  - Log at Error level with full details
  - Close connection and cleanup
  - Don't propagate to other connections
  - Don't crash server

- [ ] T036 [US4] Handle null interact callback (EC-008)
  - Accept connection
  - Complete negotiation
  - Close immediately (no application to run)

**Checkpoint**: US4 complete - lifecycle management robust. Clean shutdown, proper cleanup.

---

## Phase 7: User Story 5 - Send Messages to Clients (Priority: P3)

**Goal**: Push formatted text to clients outside normal prompt flow

**Independent Test**: Call Send/SendAbovePrompt, verify text appears on client

**Acceptance Scenarios**:
1. Send() transmits formatted text to client
2. SendAbovePrompt() appears above prompt without disrupting input
3. Styled text renders with ANSI escape sequences

### Tests for User Story 5

- [ ] T037 [P] [US5] Write messaging tests in `tests/Stroke.Tests/Contrib/Telnet/TelnetServerMessagingTests.cs`
  - Test Send() with plain text
  - Test Send() with Html formatted text
  - Test Send() with Ansi formatted text
  - Test SendAbovePrompt() with active prompt
  - Test SendAbovePrompt() without app throws InvalidOperationException (API-006)
  - Test styled text produces ANSI sequences (SC-007)

### Implementation for User Story 5

- [ ] T038 [US5] Implement `TelnetConnection.SendAbovePrompt()` method (FR-014)
  - Use RunInTerminal pattern
  - Require active AppSession context
  - Throw InvalidOperationException if no context (API-006)
  - Thread-safe

- [ ] T039 [US5] Ensure styled text rendering (FR-019)
  - Apply connection's Style to formatted text
  - Verify ANSI escape sequences in output
  - Test with Style.Default and custom styles

**Checkpoint**: US5 complete - server-push messaging works. Notifications, chat messages supported.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Finalization, documentation, coverage verification

- [ ] T040 [P] Add XML documentation to all public members
  - Thread safety guarantees documented
  - Exception conditions documented
  - Examples in key methods

- [ ] T041 [P] Add logging infrastructure (LOG-001/LOG-002)
  - Debug: protocol negotiation, state transitions
  - Information: connection established/closed
  - Warning: malformed sequences, encoding errors, socket errors
  - Error: interact callback exceptions

- [ ] T042 [P] Create example application `examples/Stroke.Examples.Telnet/BasicRepl.cs`
  - Simple REPL per quickstart.md
  - Demonstrate Send, SendAbovePrompt
  - Graceful shutdown with Ctrl+C

- [ ] T043 Verify test coverage ≥80% (SC-008)
  - Run `dotnet test --collect:"XPlat Code Coverage"`
  - Scope: all files in `src/Stroke/Contrib/Telnet/`
  - Add tests if below 80%

- [ ] T044 Run quickstart.md validation
  - Verify all code examples compile
  - Verify basic usage pattern works
  - Test with real telnet client

- [ ] T045 Performance verification
  - SC-001: 50 concurrent connections
  - SC-002: <500ms negotiation
  - SC-003: <50ms input latency
  - SC-004: <1s cleanup
  - SC-005: <100ms startup

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 - **BLOCKS all user stories**
- **User Stories (Phase 3-7)**: All depend on Phase 2 completion
  - US1 and US2 (both P1) can proceed in parallel after Phase 2
  - US3 and US4 (both P2) can proceed after US1 is testable
  - US5 (P3) can proceed after basic connection infrastructure exists
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 2 (Foundation)
    ├── US1 (P1): Network REPL ─────────────────────┐
    │                                                │
    ├── US2 (P1): Terminal Size ────────────────────┤ (can run in parallel)
    │                                                │
    ├── US3 (P2): Concurrent Connections ──────────→│ (builds on US1)
    │                                                │
    ├── US4 (P2): Lifecycle Management ────────────→│ (builds on US1)
    │                                                │
    └── US5 (P3): Send Messages ───────────────────→│ (builds on US1)
                                                     │
                                                     ▼
                                              Phase 8 (Polish)
```

### Within Each User Story

- Tests SHOULD be written first (TDD encouraged but not required)
- Models/parsers before services
- Internal methods before public API
- Core implementation before edge cases

### Parallel Opportunities

**Phase 2 (Foundation)**:
- T004 → T005 (TelnetConstants impl → tests)
- T006 → T007 (Parser impl → tests)
- T008 → T009 (Stdout impl → tests)
- T005, T007, T009 are parallel *with each other* (once their prereqs complete)
- T004 → T006 → T008 are sequential (each builds on prior)

**Phase 3 (US1)**:
- T010, T022, T023 (all tests) can run in parallel
- T011-T21 are largely sequential (building on each other)

**Phases 3-7 (User Stories)**:
- US1 and US2 can be developed in parallel after Phase 2
- US3/US4/US5 can start once US1 basic infrastructure exists

---

## Parallel Example: Phase 2 Foundation

```bash
# After T004 (TelnetConstants) completes, launch tests in parallel:
Task: "T005 - Write unit tests for TelnetConstants"
Task: "T007 - Write unit tests for TelnetProtocolParser"
Task: "T009 - Write unit tests for ConnectionStdout"

# These can run while T006 and T008 are being implemented
```

## Parallel Example: User Story 1

```bash
# After implementation tasks complete, launch all US1 tests:
Task: "T010 - Write integration tests for TelnetServer"
Task: "T022 - Write unit tests for TelnetConnection"
Task: "T023 - Write unit tests for TelnetServer"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundation (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test with real telnet client
5. Deploy/demo if ready - basic REPL works!

### Incremental Delivery

1. Setup + Foundation → Core infrastructure ready
2. Add US1 → Test independently → Basic REPL ✓
3. Add US2 → Test independently → Resize works ✓
4. Add US3 → Test independently → Multi-user ✓
5. Add US4 → Test independently → Robust lifecycle ✓
6. Add US5 → Test independently → Push messaging ✓
7. Polish → Production ready

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundation together
2. Once Foundation is done:
   - Developer A: User Story 1 + User Story 2 (both P1)
   - Developer B: Tests for Foundation classes (parallel)
3. After US1 core exists:
   - Developer A: User Story 3 + 4 (P2)
   - Developer B: User Story 5 (P3) + polish
4. All stories complete and integrate

---

## Summary

| Phase | Tasks | Parallel Tasks | Files Created |
|-------|-------|----------------|---------------|
| Setup | 3 | 2 | (directories only) |
| Foundation | 6 | 3 | TelnetConstants.cs, TelnetProtocolParser.cs, ConnectionStdout.cs + tests |
| US1 (REPL) | 14 | 3 | TelnetConnection.cs, TelnetServer.cs + tests |
| US2 (Size) | 4 | 1 | (enhancements to existing) |
| US3 (Concurrent) | 4 | 1 | (enhancements to existing) |
| US4 (Lifecycle) | 5 | 1 | (enhancements to existing) |
| US5 (Messaging) | 3 | 1 | (enhancements to existing) |
| Polish | 6 | 3 | BasicRepl.cs example |
| **Total** | **45** | **15** | **5 source + 5 test + 1 example** |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Constitution VIII: Real tests only (no mocks, no FluentAssertions)
- Constitution XI: Thread safety required for all mutable state
