# Feature Specification: Telnet Server

**Feature Branch**: `060-telnet-server`
**Created**: 2026-02-03
**Status**: Draft
**Input**: User description: "Implement a Telnet server that allows running prompt toolkit applications over the Telnet protocol. This enables building network-accessible REPLs, command-line interfaces, and interactive shells."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create Network-Accessible REPL (Priority: P1)

A developer wants to create a REPL (Read-Eval-Print Loop) that users can connect to remotely over telnet. This allows hosting interactive command-line tools on a server that multiple users can access simultaneously from any telnet-capable terminal.

**Why this priority**: This is the core value proposition - enabling network-accessible prompt toolkit applications. Without this, the feature has no purpose.

**Independent Test**: Can be fully tested by starting a telnet server, connecting with a standard telnet client, and verifying that prompt toolkit features (input, output, key bindings) work correctly over the network connection.

**Acceptance Scenarios**:

1. **Given** a TelnetServer configured with an interact callback, **When** the server starts listening, **Then** it accepts incoming telnet connections on the specified host and port
2. **Given** a telnet client connects to the server, **When** the connection is established, **Then** the server negotiates terminal capabilities (type, window size) and invokes the interact callback
3. **Given** an active telnet session with a PromptSession, **When** the user types input and presses Enter, **Then** the input is received by the application and responses are sent back to the client

---

### User Story 2 - Terminal Size Negotiation (Priority: P1)

A developer needs the telnet server to automatically detect and respond to the connected client's terminal dimensions. This ensures that multi-line prompts, completion menus, and full-screen layouts render correctly within the client's available screen space.

**Why this priority**: Without proper terminal size detection, prompt toolkit's layout system cannot function correctly, making the feature unusable for real applications.

**Independent Test**: Can be tested by connecting with a telnet client, resizing the terminal window, and verifying that the server receives and applies the new dimensions.

**Acceptance Scenarios**:

1. **Given** a telnet client connecting to the server, **When** the client sends NAWS (Negotiate About Window Size) data, **Then** the server parses and stores the terminal dimensions
2. **Given** an active telnet session, **When** the client resizes their terminal window, **Then** the server receives the updated dimensions and triggers a resize event in the running application
3. **Given** a client that does not support NAWS, **When** connecting to the server, **Then** the server uses sensible default dimensions (80x24)

---

### User Story 3 - Concurrent Connection Handling (Priority: P2)

An administrator wants to run a shared command-line tool that multiple users can access simultaneously. Each user should have an independent session with their own application state while sharing the same server process.

**Why this priority**: Multi-user support is essential for production use but builds on the single-connection foundation.

**Independent Test**: Can be tested by starting multiple telnet connections simultaneously and verifying each has independent state and input/output streams.

**Acceptance Scenarios**:

1. **Given** a running telnet server, **When** multiple clients connect simultaneously, **Then** each connection receives its own isolated application session
2. **Given** multiple active connections, **When** one connection sends input, **Then** only that connection's application receives and responds to it
3. **Given** an active connection, **When** another connection is established or closed, **Then** the first connection's session continues unaffected

---

### User Story 4 - Connection Lifecycle Management (Priority: P2)

A developer needs to gracefully handle client disconnections and server shutdown. When clients disconnect unexpectedly or the server stops, resources should be cleaned up properly without affecting other connections.

**Why this priority**: Proper resource cleanup is essential for long-running servers but depends on basic connection handling.

**Independent Test**: Can be tested by forcibly disconnecting a client and verifying resources are released and other connections continue working.

**Acceptance Scenarios**:

1. **Given** an active telnet connection, **When** the client disconnects (closes socket), **Then** the server detects the disconnection and cleans up the connection resources
2. **Given** multiple active connections, **When** the server is stopped via cancellation, **Then** all connections are closed gracefully and pending operations are cancelled
3. **Given** an interact callback that throws an exception, **When** an error occurs, **Then** the connection is closed and the error is logged without crashing the server

---

### User Story 5 - Send Messages to Clients (Priority: P3)

A developer building a chat server or notification system wants to push formatted text to connected clients outside of the normal prompt flow. This enables sending alerts, messages from other users, or system notifications.

**Why this priority**: Server-push messaging extends the feature beyond simple REPL use cases but is not required for core functionality.

**Independent Test**: Can be tested by calling Send or SendAbovePrompt on an active connection and verifying the text appears on the client.

**Acceptance Scenarios**:

1. **Given** an active telnet connection, **When** the server calls Send with formatted text, **Then** the text is transmitted to the client and displayed
2. **Given** a connection with an active prompt, **When** the server calls SendAbovePrompt, **Then** the text appears above the prompt without disrupting user input
3. **Given** a connection with styles configured, **When** sending formatted text with style attributes, **Then** the text is rendered with appropriate ANSI escape sequences

---

### Edge Cases

- **EC-001**: What happens when the client sends malformed telnet sequences? **Resolution**: Server MUST log the malformed sequence and continue processing. Malformed sequences include:
  - IAC followed by unexpected byte (not DO/DONT/WILL/WONT/SB/SE/another IAC or command byte)
  - SB without matching SE (parser accumulates until buffer limit exceeded, then discards)
  - NAWS subnegotiation with incorrect length (not exactly 4 data bytes) - log warning, ignore
  - TTYPE subnegotiation missing IS marker (0x00) - fall back to default terminal type
  - Truncated IAC sequence at end of buffer (IAC as last byte) - retain state, continue on next Feed()
- **EC-002**: How does the system handle a client that connects but never sends terminal type? **Resolution**: Use default terminal type "VT100" (case-insensitive). If no TTYPE response is received within 500ms of sending the TTYPE SEND request (see EC-011), proceed with default. Vt100Output is created with "VT100" terminal type.
- **EC-011**: What are the timeout requirements for NAWS/TTYPE negotiation? **Resolution**: Negotiation phase has a combined timeout of 500ms (SC-002). If client does not respond with NAWS within this window, use default 80×24. If client does not respond with TTYPE, use "VT100". Timeout starts after initialization sequences are sent. After timeout, proceed to Ready state and invoke interact callback.
- **EC-003**: What happens if the server port is already in use? **Resolution**: `RunAsync()` MUST throw `SocketException` with error code `AddressAlreadyInUse` (EADDRINUSE).
- **EC-004**: How does the system handle very large terminal sizes from NAWS? **Resolution**: Terminal size values MUST be clamped to range 1-500 for both rows and columns. Values of 0 MUST be treated as 1. Values >500 MUST be capped at 500. Default dimensions are 80 columns × 24 rows.
- **EC-005**: What happens when Send is called after the connection is closed? **Resolution**: No-op (silently ignore), MUST NOT throw.
- **EC-006**: How does the system handle rapid connect/disconnect cycles? **Resolution**: Each connection MUST be fully cleaned up (socket closed, removed from Connections set) before resources are released. Cleanup MUST complete within SC-004 time limit (1 second).
- **EC-007**: What happens if subnegotiation data exceeds buffer size? **Resolution**: Subnegotiation buffer MUST be limited to 1024 bytes. If exceeded, parser MUST discard the partial subnegotiation and log a warning.
- **EC-008**: What happens if interact callback is null? **Resolution**: If no interact callback is provided, connection MUST be accepted but immediately closed after negotiation (no application to run).
- **EC-009**: What happens if terminal size is 0x0 from NAWS? **Resolution**: Treated as 1x1 (minimum valid size), as 0x0 is invalid for layout calculations.
- **EC-010**: What happens with partial/truncated subnegotiation (SB without SE)? **Resolution**: Parser retains state until SE is received or buffer limit exceeded. Connection remains functional.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST accept incoming TCP connections on a configurable host address and port
- **FR-002**: System MUST send telnet initialization sequences upon connection in the following exact order with these byte values:
  1. `IAC DO LINEMODE` (0xFF 0xFD 0x22) - Request linemode negotiation
  2. `IAC WILL SGA` (0xFF 0xFB 0x03) - Enable suppress go-ahead for full-duplex
  3. `IAC SB LINEMODE MODE 0 IAC SE` (0xFF 0xFA 0x22 0x01 0x00 0xFF 0xF0) - Disable line editing
  4. `IAC WILL ECHO` (0xFF 0xFB 0x01) - Server will handle echo
  5. `IAC DO NAWS` (0xFF 0xFD 0x1F) - Request window size negotiation
  6. `IAC DO TTYPE` (0xFF 0xFD 0x18) - Request terminal type
  7. `IAC SB TTYPE SEND IAC SE` (0xFF 0xFA 0x18 0x01 0xFF 0xF0) - Send terminal type request
- **FR-003**: System MUST parse incoming telnet protocol data, separating user data from IAC (Interpret As Command, 0xFF) sequences. Parser state transitions:
  - **Normal**: Accumulate user data. On 0xFF → transition to **Iac** state
  - **Iac**: On DO/DONT/WILL/WONT → transition to **IacCommand**. On SB → transition to **Subnegotiation**. On 0xFF → emit 0xFF as user data, return to **Normal**
  - **IacCommand**: Read one argument byte, log and return to **Normal**
  - **Subnegotiation**: Accumulate bytes until 0xFF. On 0xFF → transition to **SubnegotiationIac**
  - **SubnegotiationIac**: On SE (0xF0) → process subnegotiation, return to **Normal**. On 0xFF → emit 0xFF into subnegotiation buffer, return to **Subnegotiation**
- **FR-003a**: System MUST handle client DONT/WONT responses by logging them and continuing (clients may refuse options)
- **FR-003b**: System MUST handle other telnet commands (DM=242, BRK=243, IP=244, AO=245, AYT=246, EC=247, EL=248, GA=249) by logging and ignoring them
- **FR-003c**: System MUST treat NOP bytes (0x00) in user data as regular data (pass through unchanged)
- **FR-004**: System MUST handle NAWS subnegotiation to receive terminal window dimensions. NAWS format:
  - Client sends: `IAC SB NAWS <width-high> <width-low> <height-high> <height-low> IAC SE`
  - Parser MUST interpret width and height as 16-bit big-endian unsigned integers
  - Parser MUST call SizeReceived callback with (rows=height, columns=width)
- **FR-005**: System MUST handle TTYPE subnegotiation to receive terminal type information. TTYPE flow:
  - Server sends: `IAC SB TTYPE SEND IAC SE` (request)
  - Client responds: `IAC SB TTYPE IS <terminal-type-ascii> IAC SE`
  - Parser MUST extract terminal type as ASCII string from bytes after IS (0x00) marker
  - Parser MUST call TtypeReceived callback with the terminal type string
- **FR-006**: System MUST convert line endings from LF (0x0A) to CRLF (0x0D 0x0A) when sending data to clients (per telnet NVT specification). This conversion:
  - MUST apply to all outbound text via ConnectionStdout.Write()
  - MUST NOT apply to raw byte sequences (e.g., ANSI escape codes)
  - MAY result in CRLF→CRCRLF for input already containing CRLF (matching Python PTK behavior)
- **FR-007**: System MUST create an isolated application session (PipeInput + Vt100Output) for each connection
- **FR-008**: System MUST invoke the user-provided interact callback for each new connection
- **FR-009**: System MUST support multiple concurrent connections, each with independent state
- **FR-010**: System MUST track active connections in a set that can be enumerated
- **FR-011**: System MUST clean up resources (socket, input, output) when a connection closes
- **FR-012**: System MUST support cancellation to stop the server gracefully
- **FR-013**: System MUST provide methods to send formatted text to a specific connection
- **FR-014**: System MUST provide a method to send text above an active prompt (run-in-terminal pattern)
- **FR-015**: System MUST provide a method to erase the screen and reset cursor position
- **FR-016**: System MUST handle double-IAC escape sequences: When the parser receives two consecutive IAC bytes (0xFF 0xFF), it MUST emit a single 0xFF byte as user data. This applies both in Normal state and within Subnegotiation buffers
- **FR-017**: System MUST notify the application when terminal size changes mid-session
- **FR-018**: System MUST support configurable character encoding (default UTF-8)
- **FR-019**: System MUST support configurable style for formatted text output
- **FR-020**: System MUST support enabling/disabling cursor position requests (CPR)

### Thread Safety Requirements

This section documents concurrency guarantees per Constitution XI (Thread Safety by Default).

- **TS-001**: `TelnetServer.Connections` property MUST return a thread-safe snapshot that can be safely enumerated while connections are being added or removed. Implementation MUST use `ConcurrentDictionary` with snapshot copy on enumeration.
- **TS-002**: Concurrent `Send()` calls to the same `TelnetConnection` from different threads MUST be serialized internally. Write operations MUST be atomic at the connection level using `Lock`.
- **TS-003**: `TelnetProtocolParser.Feed()` is NOT thread-safe. It MUST be called from a single reader thread per connection. Documentation MUST explicitly state this constraint.
- **TS-004**: `TelnetServer` state machine transitions (Created→Running→Stopped) MUST be atomic and thread-safe.
- **TS-005**: `TelnetConnection` state machine transitions (Created→Negotiating→Ready→Running→Closed) MUST be atomic using interlocked operations.
- **TS-006**: The `interact` callback MAY be invoked concurrently for different connections. Each invocation runs in its own async context with isolated state.
- **TS-007**: Server shutdown (`CancellationToken` triggered) while connections are being established MUST:
  1. Stop accepting new TCP connections immediately
  2. Cancel in-progress negotiation for pending connections
  3. Signal all running connections to terminate via their cancellation tokens
  4. Wait for all connection cleanup to complete (bounded by SC-004)
- **TS-008**: Connection enumeration via `foreach (var c in server.Connections)` MUST NOT throw if connections are added/removed during iteration.

### Connection Isolation

- **ISO-001**: "Isolated" in FR-007/FR-009 means each connection MUST have:
  - Its own `IPipeInput` instance (not shared with other connections)
  - Its own `Vt100Output` instance wrapping `ConnectionStdout`
  - Its own `AppSession` context with independent `Buffer`, `History`, and `Clipboard`
  - Its own `TelnetProtocolParser` instance with dedicated callbacks
- **ISO-002**: Input received on one connection MUST NOT be delivered to another connection's application.
- **ISO-003**: Output written by one connection's application MUST NOT appear on another connection's terminal.

### State Machines

**TelnetServer Lifecycle**:
```
Created ─[RunAsync()]─→ Running ─[CancellationToken]─→ Stopped
```

**TelnetConnection Lifecycle**:
```
Created ─[socket accept]─→ Negotiating ─[TTYPE received]─→ Ready ─[interact invoked]─→ Running ─[close/error]─→ Closed
```

### Key Entities

- **TelnetServer**: The main server class that listens for connections and manages the server lifecycle. Contains host, port, encoding, style, active connections set, and interact callback.
- **TelnetConnection**: Represents a single client connection. Contains socket reference, remote endpoint, encoding, current terminal size, parent server reference, and connection state (open/closed).
- **TelnetProtocolParser**: Stateful parser that processes raw telnet byte streams, separating user data from protocol commands and handling subnegotiation sequences.
- **ConnectionStdout**: TextWriter wrapper that converts socket output to telnet-compatible format (LF to CRLF conversion) and handles buffered writes.

### API Contracts

#### TelnetServer Constructor Parameters

| Parameter | Type | Default | Constraints | Description |
|-----------|------|---------|-------------|-------------|
| `host` | `string` | `"127.0.0.1"` | Non-null, valid IP or hostname | Bind address |
| `port` | `int` | `23` | 1-65535 inclusive | Listen port. Values outside range MUST throw `ArgumentOutOfRangeException` |
| `interact` | `Func<TelnetConnection, Task>?` | `null` | None | Session handler. If null, connections close after negotiation (see EC-008) |
| `encoding` | `Encoding?` | `Encoding.UTF8` | None (null → UTF-8) | Character encoding |
| `style` | `IStyle?` | `null` | None | Formatted text style |
| `enableCpr` | `bool` | `true` | None | Enable cursor position requests |

#### TelnetServer.RunAsync Semantics

- **API-001**: `readyCallback` MUST be invoked on the server's async context after the listening socket is bound but before accepting the first connection. It indicates the server is ready to receive connections.
- **API-002**: `cancellationToken` MUST trigger orderly shutdown as specified in TS-007.
- **API-003**: `RunAsync` MUST NOT return until all cleanup is complete.

#### TelnetServer Deprecated API

- **API-004**: `Start()` and `StopAsync()` are deprecated (marked `[Obsolete]`). Rationale: `RunAsync` with `CancellationToken` provides cleaner lifecycle management matching .NET async patterns. Legacy methods exist for Python PTK API compatibility.

#### TelnetConnection Method Contracts

- **API-005**: `Send(formattedText)` on a closed connection MUST be a no-op (no exception thrown).
- **API-006**: `SendAbovePrompt(formattedText)` MUST throw `InvalidOperationException` if called outside an active `AppSession` context (no running application).
- **API-007**: `EraseScreen()` MUST send ANSI escape sequence `ESC[2J` (erase entire screen) followed by `ESC[H` (cursor to home position 1,1). On closed connection, MUST be a no-op.
- **API-008**: `Close()` MUST be idempotent. Multiple calls have no additional effect after the first.
- **API-009**: `Feed(data)` is `internal` visibility. Only called by TelnetServer's socket reader loop.
- **API-010**: `Size` property is updated internally by the parser's `SizeReceived` callback. External code SHOULD treat it as read-only (no public setter).

#### TelnetProtocolParser Contracts

- **API-011**: All callback parameters (`dataReceived`, `sizeReceived`, `ttypeReceived`) MUST be non-null. Constructor MUST throw `ArgumentNullException` if any callback is null.
- **API-012**: Parser is NOT thread-safe (see TS-003). Single-threaded access is caller's responsibility.
- **API-013**: Parser instances are NOT reusable across connections. Create a new parser per connection.
- **API-014**: Callback invocation order within a single `Feed()` call follows byte stream order. If NAWS data precedes user data in the byte stream, `SizeReceived` is called before `DataReceived` for that chunk.

### Error Handling

#### Exception Types

| Scenario | Exception Type | When Thrown |
|----------|---------------|-------------|
| Port out of range (1-65535) | `ArgumentOutOfRangeException` | `TelnetServer` constructor |
| Port already in use | `SocketException` (AddressAlreadyInUse) | `RunAsync()` when binding |
| Network unreachable | `SocketException` | `RunAsync()` when binding |
| Null callback parameter | `ArgumentNullException` | `TelnetProtocolParser` constructor |
| SendAbovePrompt without app | `InvalidOperationException` | `SendAbovePrompt()` |

#### Socket Error Handling

- **ERR-001**: Socket read failures during `Feed()` (e.g., connection reset by peer) MUST:
  1. Log the error at Warning level
  2. Transition connection to Closed state
  3. Remove connection from server's Connections set
  4. Clean up all connection resources (input, output, socket)
  5. NOT propagate exception to caller or crash the server

- **ERR-002**: Socket write failures in `ConnectionStdout.Flush()` MUST:
  1. Log the error at Warning level
  2. Mark the ConnectionStdout as closed
  3. NOT throw exception (silent failure for graceful disconnect handling)

#### Interact Callback Error Handling

- **ERR-003**: Exceptions thrown by the `interact` callback MUST:
  1. Be caught by the connection's run loop
  2. Be logged at Error level with full exception details
  3. Trigger connection close and cleanup
  4. NOT propagate to other connections
  5. NOT crash the server

#### Encoding Error Handling

- **ERR-004**: Encoding errors when decoding TTYPE string MUST:
  1. Log a warning with the raw bytes
  2. Fall back to default terminal type "VT100"
  3. Continue connection normally

- **ERR-005**: Encoding errors when writing output MUST use the configured Encoding's error handling (typically replacement character fallback).

#### Logging Mechanism

- **LOG-001**: All logging MUST use `System.Diagnostics.Debug.WriteLine()` for debug builds and be a no-op in release builds, OR use the application's configured logging infrastructure if available via `ILogger` injection. Implementation MAY use `Microsoft.Extensions.Logging.ILogger<TelnetServer>` if the dependency is available.
- **LOG-002**: Log levels:
  - **Debug**: Protocol negotiation details, state transitions
  - **Information**: Connection established, connection closed
  - **Warning**: Malformed protocol sequences, encoding errors, socket errors
  - **Error**: Interact callback exceptions

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Server MUST support at least 50 concurrent telnet connections simultaneously (minimum target, not a hard cap). Implementation MAY support more; no artificial upper limit should be imposed.
  - **Measurement**: Test creates 50 TCP connections, verifies all receive initialization sequences and can send/receive data concurrently.

- **SC-002**: Terminal size negotiation completes within 500ms of connection establishment for clients that support NAWS.
  - **Measurement**: Timer starts when TCP connection is accepted, ends when interact callback is invoked. Measured per-connection.

- **SC-003**: Input latency from keypress to application receipt is under 50ms on local network connections.
  - **Measurement**: Test sends timestamped byte via socket, measures time until DataReceived callback invoked. Uses `Stopwatch.GetTimestamp()`. Localhost (127.0.0.1) only.

- **SC-004**: Connection cleanup completes within 1 second of client disconnection.
  - **Measurement**: Timer starts when socket read returns 0 bytes (EOF) or throws, ends when connection is removed from Connections set. Verified via `Assert.DoesNotContain`.

- **SC-005**: Server startup (from constructor to ready callback) completes within 100ms.
  - **Measurement**: Timer starts at `RunAsync()` call, ends when `readyCallback` is invoked. Uses `Stopwatch`.

- **SC-006**: All prompt toolkit features (input editing, key bindings, completion, history) function correctly over telnet connections.
  - **Measurement**: Integration test creates PromptSession over telnet, verifies: (1) text input captured, (2) Ctrl+A moves cursor to start, (3) Tab triggers completion if configured, (4) Up arrow retrieves history if available. Each verifiable via Send/Feed roundtrip.

- **SC-007**: Formatted text with styles renders correctly with appropriate ANSI escape sequences on connected clients.
  - **Measurement**: Test sends Html-formatted text via Send(), captures raw bytes written to socket, verifies ANSI escape sequences present (e.g., `\x1b[31m` for red).

- **SC-008**: Unit tests achieve 80% code coverage for the telnet module.
  - **Scope**: All files in `src/Stroke/Contrib/Telnet/` directory: TelnetServer.cs, TelnetConnection.cs, TelnetProtocolParser.cs, ConnectionStdout.cs, TelnetConstants.cs
  - **Measurement**: `dotnet test --collect:"XPlat Code Coverage"` with Coverlet. Line coverage ≥80%.

## Non-Functional Requirements

### Security

- **NFR-001**: Telnet is an UNENCRYPTED protocol. All data (including passwords) is transmitted in plaintext. Documentation MUST include security warnings about this limitation.
- **NFR-002**: For secure connections, users SHOULD use SSH instead. This module does NOT provide encryption.
- **NFR-003**: Server SHOULD NOT be bound to `0.0.0.0` (all interfaces) in production without firewall protection.

### Resource Limits

- **NFR-004**: Each connection SHOULD consume no more than 64KB of memory for buffers (subnegotiation: 1KB, receive: 4KB, send: 4KB, parser state: minimal).
- **NFR-005**: No artificial upper limit on concurrent connections (limited only by OS resources).

### Observability

- **NFR-006**: Logging as specified in LOG-001/LOG-002.
- **NFR-007**: Health checks: Server health can be determined by `Connections.Count` and whether `RunAsync` task is still running.
- **NFR-008**: No built-in metrics. Applications MAY instrument via interact callback timing.

### Python PTK Compatibility

- **NFR-009**: API MUST be a faithful port of Python Prompt Toolkit's `prompt_toolkit.contrib.telnet` module per Constitution I.
- **NFR-010**: Behavioral compatibility: Applications ported from Python PTK telnet SHOULD work with minimal changes (only C# syntax adjustments).

## Dependencies

### Required Stroke Modules

| Module | Classes Used | Purpose |
|--------|-------------|---------|
| `Stroke.Application` | `AppSession`, `AppContext`, `RunInTerminal` | Session lifecycle, context management |
| `Stroke.Input` | `IPipeInput`, `PipeInput` | Per-connection input isolation |
| `Stroke.Output` | `IOutput`, `Vt100Output` | Terminal output rendering |
| `Stroke.Styles` | `IStyle`, `BaseStyle` | Formatted text styling |
| `Stroke.FormattedText` | `AnyFormattedText`, `Html`, `Ansi` | Rich text content |
| `Stroke.Core` | `Size` | Terminal dimensions |

### BCL Dependencies

- `System.Net.Sockets` (Socket, TcpListener)
- `System.Text` (Encoding)
- `System.Threading` (Lock, CancellationToken)
- `System.Threading.Tasks` (Task, async/await)

## Assumptions

- **ASM-001**: Connected clients support VT100 escape sequences for cursor positioning, colors, and screen clearing. Clients without VT100 support will see raw escape codes.
- **ASM-002**: UTF-8 is a safe default encoding for modern telnet clients. Legacy clients may require explicit encoding configuration.
- **ASM-003**: Network latency is acceptable for interactive use (<100ms typical).
- **ASM-004**: Clients will respond to NAWS/TTYPE requests within reasonable time (500ms timeout).

## Requirements Traceability

### User Story → Functional Requirements

| User Story | Functional Requirements |
|------------|------------------------|
| US-1: Network REPL | FR-001, FR-002, FR-003, FR-007, FR-008 |
| US-2: Terminal Size | FR-004, FR-017 |
| US-3: Concurrent Connections | FR-009, FR-010 |
| US-4: Lifecycle Management | FR-011, FR-012 |
| US-5: Send Messages | FR-013, FR-014, FR-015, FR-019 |

### Functional Requirements → Acceptance Scenarios

| Requirement | Acceptance Scenario |
|-------------|---------------------|
| FR-001 | US-1 Scenario 1 |
| FR-002 | US-1 Scenario 2 |
| FR-004 | US-2 Scenario 1, US-2 Scenario 2 |
| FR-007 | US-3 Scenario 1 |
| FR-009 | US-3 Scenario 2, US-3 Scenario 3 |
| FR-011 | US-4 Scenario 1 |
| FR-012 | US-4 Scenario 2 |
| FR-013 | US-5 Scenario 1 |
| FR-014 | US-5 Scenario 2 |
| FR-019 | US-5 Scenario 3 |

### Edge Cases → Requirements

| Edge Case | Handling Requirement |
|-----------|---------------------|
| EC-001 (malformed sequences) | FR-003, API-014 |
| EC-002 (no TTYPE) | EC-011, FR-005 |
| EC-003 (port in use) | Exception Types table |
| EC-004 (large NAWS) | FR-004, data-model validation |
| EC-005 (Send after close) | API-005 |
| EC-006 (rapid connect/disconnect) | FR-011, SC-004 |
| EC-007 (buffer overflow) | FR-003, Subnegotiation state |
| EC-008 (null interact) | API Contracts table |
| EC-009 (zero size) | FR-004 clamping |
| EC-010 (partial SB) | FR-003 state machine |

### Success Criteria → Requirements

| Success Criteria | Traced Requirements |
|-----------------|---------------------|
| SC-001 | FR-009, TS-001 |
| SC-002 | FR-002, FR-004, FR-005, EC-011 |
| SC-003 | FR-003, TS-003 |
| SC-004 | FR-011, ERR-001 |
| SC-005 | FR-001, API-001 |
| SC-006 | FR-007, ISO-001/002/003 |
| SC-007 | FR-013, FR-019 |
| SC-008 | All FR-* |
