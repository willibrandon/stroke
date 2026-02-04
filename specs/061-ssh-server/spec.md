# Feature Specification: SSH Server Integration

**Feature Branch**: `061-ssh-server`
**Created**: 2026-02-03
**Status**: Draft
**Input**: Implement SSH server integration allowing Stroke applications to run over SSH connections using FxSsh library, following Python Prompt Toolkit's ssh module patterns.

## Clarifications

### Session 2026-02-03

- Q: Should `PromptToolkitSshServer` be sealed or inheritable for custom authentication? → A: Inheritable with virtual `BeginAuth` and `CreateSession` methods (matches Python PTK design)

### Virtual Method Semantics

**BeginAuth Semantics**:
- Returns `false` (default): Skip authentication entirely; any username/password accepted
- Returns `true`: Authentication required; server must handle password validation via FxSsh's `UserAuth` event
- Override pattern: Subclass checks username against allowed list, returns `true` for known users

**Password Authentication Pattern** (when `BeginAuth` returns `true`):
```csharp
public class AuthenticatedSshServer : PromptToolkitSshServer
{
    private readonly Dictionary<string, string> _credentials;

    protected override bool BeginAuth(string username)
    {
        return _credentials.ContainsKey(username);  // Require auth for known users
    }

    // Password validation handled via FxSsh UserAuth event in constructor
}
```

**Public Key Authentication**: Not directly supported by this spec; FxSsh supports it but requires additional event handling beyond the scope of Python PTK parity.

### Protocol Details (NVT/SSH)

**Line Ending Conversion** (per RFC 854 NVT specification):
- **Output (LF→CRLF)**: All `\n` characters in output MUST be converted to `\r\n` before sending to SSH channel
- **Input handling**: Input is passed through unchanged; the Stroke input parser handles all escape sequences including CR, LF, and CRLF
- **Embedded CRLF**: If output already contains `\r\n`, it passes through unchanged (only bare `\n` is converted)

**Terminal Type Negotiation**:
- Terminal type is obtained via SSH PTY request (`PtyReceived` event in FxSsh)
- Common values: `xterm`, `xterm-256color`, `vt100`, `linux`, `screen`
- Default fallback if not provided: `vt100`
- Terminal type is passed to `Vt100Output` constructor for capability detection

**Terminal Size Negotiation**:
- **Initial size**: Received via `PtyReceived` event with `width` and `height` parameters
- **Default before negotiation**: `Size(columns=79, rows=20)` — matches Python PTK default
- **Size clamping**: Dimensions are clamped to range 1-500 to prevent memory issues with extreme values
- **Rationale for 79×20 default**: Matches Python PTK; 79 allows for 1-character margin in 80-column terminals

**Cursor Position Request (CPR)**:
- **Enable flag**: `EnableCpr` property on server and session controls whether CPR sequences are sent
- **Request sequence**: `ESC[6n` (sent by Vt100Output when determining cursor position)
- **Response format**: `ESC[{row};{col}R` (parsed by input system)
- **Timeout handling**: CPR responses are not guaranteed; applications must handle missing responses gracefully via Vt100Output's existing timeout mechanism (default: 1 second)
- **Terminal support**: Most modern terminals (xterm, iTerm2, Windows Terminal) support CPR; some (like `dumb` terminals) do not

**CreateSession Semantics**:
- Default: Returns new `PromptToolkitSshSession` with server's interact callback and enableCpr
- Override pattern: Return custom session subclass for per-session state or behavior

## API Mapping (Python PTK → C#)

### Class Name Mapping

| Python PTK Class | C# Stroke Class | Notes |
|------------------|-----------------|-------|
| `PromptToolkitSSHSession` | `PromptToolkitSshSession` | Case convention: SSH → Ssh (C# PascalCase) |
| `PromptToolkitSSHServer` | `PromptToolkitSshServer` | Case convention: SSH → Ssh |
| Nested `Stdout` class | `SshChannelStdout` | Promoted to top-level internal class for clarity |

### Method Mapping

| Python PTK Method | C# Stroke Method | Notes |
|-------------------|------------------|-------|
| `session_requested()` | `CreateSession()` | Virtual factory method for custom session types |
| `begin_auth(username)` | `BeginAuth(username)` | Virtual; returns `false` (no auth) by default |
| `data_received(data, datatype)` | `DataReceived(data)` | `datatype` parameter not used in implementation |
| `terminal_size_changed(w, h, pw, ph)` | `TerminalSizeChanged(width, height)` | Pixel dimensions not used |
| `_get_size()` | `GetSize()` | Public in C# (no underscore convention) |
| `connection_made(chan)` | Internal via constructor | Channel passed at construction |
| `shell_requested()` | Internal | Always returns true |
| `session_started()` | Internal | Triggers interact callback |

### Constructor Parameter Mapping

**PromptToolkitSshServer**:
| Python Parameter | C# Parameter | Notes |
|------------------|--------------|-------|
| `interact` | `interact` | `Func<PromptToolkitSshSession, Task>` |
| `enable_cpr` | `enableCpr` | Default: `true` |
| (N/A - asyncssh handles) | `host` | Default: `"127.0.0.1"` |
| (N/A - asyncssh handles) | `port` | Default: `2222` |
| (N/A - asyncssh handles) | `hostKeyPath` | Required for FxSsh |
| (N/A - asyncssh handles) | `encoding` | Default: UTF-8 |
| (N/A - asyncssh handles) | `style` | Optional style |

**PromptToolkitSshSession**:
| Python Parameter | C# Parameter | Notes |
|------------------|--------------|-------|
| `interact` | `Interact` | Property (readonly) |
| `enable_cpr` | `EnableCpr` | Property (readonly) |

### Nested Stdout Class → SshChannelStdout

The Python nested `Stdout` class is ported as `SshChannelStdout`:

| Python Stdout Method | C# SshChannelStdout | Notes |
|---------------------|---------------------|-------|
| `write(data)` | `Write(string)` | With LF→CRLF conversion |
| `isatty()` | `IsAtty` property | Always returns `true` |
| `flush()` | `Flush()` | No-op for SSH |
| `encoding` property | `Encoding` property | From channel |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Run Interactive Application Over SSH (Priority: P1)

A developer wants to expose their Stroke-based REPL or CLI tool over SSH so that remote users can connect and interact with the application securely. Each SSH connection should get its own isolated session with independent UI state.

**Why this priority**: This is the core value proposition - enabling network-accessible Stroke applications with SSH protocol security. Without this, no other SSH functionality is meaningful.

**Independent Test**: Can be fully tested by starting an SSH server with a simple interact callback, connecting via SSH client, typing input, and verifying output is rendered correctly. Delivers remote access capability for Stroke applications.

**Acceptance Scenarios**:

1. **Given** a running SSH server with an interact callback, **When** a user connects via SSH client, **Then** the interact callback is invoked with access to the SSH session
2. **Given** an SSH session running a PromptSession, **When** the user types input and presses Enter, **Then** the input is captured and the prompt result is returned
3. **Given** multiple SSH clients connecting simultaneously, **When** each client interacts with the application, **Then** each has an isolated session with independent state

---

### User Story 2 - Terminal Size Tracking (Priority: P1)

When a user resizes their terminal window while connected over SSH, the Stroke application should detect this change and re-render the UI appropriately. This enables responsive layouts and proper menu positioning.

**Why this priority**: Essential for usability - without terminal size tracking, completion menus and layouts would render incorrectly, making the application unusable.

**Independent Test**: Can be tested by connecting via SSH, resizing the terminal, and verifying the application receives resize events and re-renders correctly.

**Acceptance Scenarios**:

1. **Given** an SSH session with a running application, **When** the user resizes their terminal, **Then** the application receives the new dimensions and triggers a re-render
2. **Given** a completion menu is visible, **When** the terminal is resized smaller, **Then** the menu repositions correctly within the new bounds

---

### User Story 3 - Line Ending Conversion (Priority: P2)

SSH connections using NVT (Network Virtual Terminal) require LF to be converted to CRLF for proper display. The SSH server should handle this conversion transparently so application code doesn't need to worry about it.

**Why this priority**: Required for proper display but simpler than core session management.

**Independent Test**: Can be tested by sending text with newlines through the SSH channel and verifying CRLF sequences are sent to the client.

**Acceptance Scenarios**:

1. **Given** an SSH session, **When** the application writes text containing LF characters, **Then** the SSH channel receives the text with LF converted to CRLF
2. **Given** an SSH session outputting formatted text, **When** the output includes embedded newlines, **Then** each newline displays correctly on the SSH client

---

### User Story 4 - Cursor Position Request Support (Priority: P2)

For proper positioning of dropdown menus and completion popups, the Stroke application may need to query the actual cursor position from the terminal. This optional feature should work over SSH when enabled.

**Why this priority**: Important for advanced UI features but applications can function without it.

**Independent Test**: Can be tested by running an application with CPR enabled, triggering a cursor position query, and verifying the response is received and parsed.

**Acceptance Scenarios**:

1. **Given** an SSH server with CPR enabled, **When** the application queries cursor position, **Then** the terminal responds with correct row/column values
2. **Given** an SSH server with CPR disabled, **When** the application would query cursor position, **Then** no CPR escape sequences are sent

---

### User Story 5 - Session Cleanup on Disconnect (Priority: P2)

When an SSH client disconnects (gracefully or due to network failure), the server should properly clean up resources including the PipeInput, Vt100Output, and AppSession associated with that connection.

**Why this priority**: Required for production use to prevent resource leaks, but core functionality works without it during development.

**Independent Test**: Can be tested by connecting, running an application, disconnecting abruptly, and verifying no resource leaks occur.

**Acceptance Scenarios**:

1. **Given** an active SSH session, **When** the client gracefully closes the connection, **Then** the session resources are disposed and the connection is removed from the server's connection list
2. **Given** an active SSH session, **When** the client disconnects abruptly (network failure), **Then** the server detects the disconnection and cleans up resources

---

### Edge Cases

- What happens when a client sends data before the session is fully initialized? Data should be buffered until PipeInput is ready, then delivered in order. Buffer size limit: 64KB; data beyond limit is discarded with warning logged.
- How does the system handle SSH negotiation timeout? FxSsh handles this with standard timeouts (30 seconds default); failed negotiations result in connection closure.
- What happens when the interact callback throws an exception? The exception is logged via Stroke's logging system and the SSH channel is closed cleanly.
- How does the system handle very rapid connect/disconnect cycles? Concurrent session management uses thread-safe collections to prevent state corruption.
- What happens on mid-escape-sequence disconnect? Partial data in PipeInput buffer is discarded; no special handling needed as session is closing.
- What happens when channel write fails (broken pipe)? Write exceptions are caught, logged, and session is closed gracefully (matching Python PTK's `except BrokenPipeError: pass` pattern).

## Concurrency & Thread Safety Requirements

### Thread Safety Mandates

Per Constitution XI, all mutable classes MUST be thread-safe:

| Class | Thread Safety Mechanism | Rationale |
|-------|-------------------------|-----------|
| `PromptToolkitSshServer` | `ConcurrentDictionary<PromptToolkitSshSession, byte>` for connections; `Lock` for task list | Multiple sessions connect/disconnect concurrently |
| `PromptToolkitSshSession` | `Lock` for mutable state (`_size`, `_closed`); `volatile` for boolean flags | DataReceived may be called from network thread while interact runs |
| `SshChannel` | Delegates to FxSsh channel (inherently thread-safe) | FxSsh handles internal synchronization |
| `SshChannelStdout` | Delegates to channel; atomic per-write | Multiple writes may occur concurrently |

### Concurrent Access Guarantees

- **Accept while processing**: Server MUST continue accepting new connections while existing sessions are active
- **DataReceived calls**: Multiple `DataReceived` calls from the same session are serialized by FxSsh; calls across sessions are concurrent
- **Session state mutations**: Each operation on session state (`_size`, `_closed`) is atomic via `Lock`; compound read-modify-write requires caller synchronization
- **Connections property**: Returns a snapshot; safe to enumerate while sessions connect/disconnect

### Concurrency Limits

- **Maximum concurrent sessions**: 100 sessions without resource exhaustion (per SC-002)
- **Measurement method**: Memory usage stays under 500MB with 100 active sessions
- **No hard limit enforced**: Beyond 100, behavior degrades gracefully (increased latency, not crashes)

### Shutdown Sequence

Graceful shutdown MUST follow this order:
1. Stop accepting new connections (close listener socket)
2. Cancel all active sessions via cancellation token propagation
3. Wait for sessions to complete with 5-second timeout
4. Force-close remaining sessions after timeout
5. Dispose server resources (host key, listener)

### Cancellation Token Propagation

- `RunAsync(cancellationToken)` receives the master cancellation token
- Cancellation triggers: (1) stop accepting, (2) propagate to all sessions
- Sessions receive cancellation via `AppSession` context
- Running `interact` callbacks receive `OperationCanceledException` on their next `await`

## FxSsh Integration Details

### Event Mapping

| FxSsh Event | Stroke Handler | Description |
|-------------|----------------|-------------|
| `SshServer.ConnectionAccepted` | Create `PromptToolkitSshSession` | New client connected; session added to Connections |
| `UserAuthService.UserAuth` | Call `BeginAuth(username)` virtual | Set `UserAuthArgs.Result` based on return value |
| `ConnectionService.PtyReceived` | Store terminal type, width, height | Initial terminal negotiation complete |
| `ConnectionService.CommandOpened` | Start interact callback | Shell/exec request received; create I/O infrastructure |
| `Channel.DataReceived` | Call `DataReceived(string)` | Route keyboard data to PipeInput |
| `ConnectionService.WindowChange` | Call `TerminalSizeChanged(w, h)` | Terminal resized; trigger app invalidation |
| `Channel.Closed` / disconnect | Cleanup session resources | Remove from Connections, dispose PipeInput/AppSession |

### Host Key Management

**Supported Algorithms**:
- **RSA** (rsa-sha2-256, rsa-sha2-512): 2048-bit minimum, 4096-bit recommended
- **ECDSA** (ecdsa-sha2-nistp256, nistp384, nistp521): Modern elliptic curve
- **Ed25519** (ssh-ed25519): Most modern, smallest key size, recommended

**Host Key Configuration**:
- `hostKeyPath` parameter accepts path to PEM-format private key file
- Key file is validated at constructor time; `ArgumentException` thrown for invalid/missing files
- Key generation guidance in quickstart.md: `ssh-keygen -t ed25519 -f ssh_host_key -N ""`

**Invalid Key Handling**:
- Missing file: `FileNotFoundException` with descriptive message
- Invalid PEM format: `ArgumentException` with "Invalid host key format" message
- Unsupported algorithm: `ArgumentException` listing supported algorithms

### Authentication Flow

**When `BeginAuth` returns `false`** (default):
1. FxSsh receives any password/key from client
2. `UserAuthArgs.Result` is set to `AuthResult.Success`
3. Session proceeds to shell request
4. No password validation occurs

**When `BeginAuth` returns `true`**:
1. FxSsh prompts client for password
2. Server must hook `UserAuthService.UserAuth` to validate
3. Set `UserAuthArgs.Result = AuthResult.Success` or `AuthResult.Failure`
4. On failure, client may retry (up to 3 attempts default)
5. After max failures, connection is closed by FxSsh

**Authentication Timeout**:
- FxSsh default: 30 seconds for complete authentication
- Timeout results in connection closure (no session created)

**Authentication Failure Logging**:
- Failed attempts logged via Stroke's logging system at Warning level
- Log message includes: username, source IP, failure reason

## Resource Management

### Disposable Resource Lifecycle

| Resource | Creation | Disposal Trigger | Disposal Order |
|----------|----------|------------------|----------------|
| `PipeInput` | Session start (CommandOpened) | Session end or disconnect | 1st (stop input) |
| `Vt100Output` | Session start | Session end | 2nd (stop output) |
| `AppSession` | Session start | Session end | 3rd (release context) |
| `SshChannel` | Connection accepted | Session end or disconnect | 4th (close channel) |

**Disposal Order Rationale**: Input stopped first to prevent new data processing; output stopped before context release to flush pending writes; channel closed last to ensure graceful SSH disconnect.

### Memory Leak Prevention

- All disposable resources wrapped in `using` statements or try/finally blocks
- Session removed from `Connections` dictionary in finally block (even on exception)
- PipeInput explicitly closed to prevent pending read waits
- **Testability**: Memory leak prevention verified by running 100 connect/disconnect cycles and checking no growth in managed heap

### Buffer Size Limits

- **Pre-session data buffer**: 64KB maximum; excess data discarded with warning
- **SSH channel buffer**: Managed by FxSsh (128KB default)
- **PipeInput buffer**: Unbounded (existing Stroke behavior)

## Error Handling

### Exception Handling Strategy

| Exception Source | Handling | User Impact |
|------------------|----------|-------------|
| Interact callback throws | Log exception, close channel gracefully | Session ends, other sessions unaffected |
| Channel write fails (BrokenPipeError) | Catch, log, mark session as closed | Silent cleanup |
| FxSsh protocol error | Logged by FxSsh, connection closed | Session never starts |
| Invalid host key | `ArgumentException` at constructor | Server fails to start |
| Encoding error (invalid UTF-8 bytes) | Replace with U+FFFD replacement character | Garbled character displayed |

### FxSsh Exception Types

The following FxSsh exception types should be caught and handled:
- `SshConnectionException`: Network or protocol failure
- `SshAuthenticationException`: Auth mechanism failure (when auth required)

Other exceptions should propagate to surface bugs.

### Network Failure Detection

- FxSsh detects TCP disconnect via socket read/write failures
- `Channel.Closed` event fired on disconnect detection
- Detection latency: Immediate for clean disconnect; up to TCP timeout (typically 2 minutes) for network partition
- **Keepalive**: FxSsh supports SSH keepalive messages (disabled by default)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `PromptToolkitSshSession` class representing a single SSH connection with access to the interact callback, CPR setting, AppSession, and current InteractTask
- **FR-002**: System MUST provide a `PromptToolkitSshServer` class with an interact callback parameter and EnableCpr configuration
- **FR-003**: System MUST create isolated sessions for each SSH connection with independent PipeInput, Vt100Output, and AppSession instances
- **FR-004**: System MUST expose a `GetSize()` method that returns current terminal size as `Size(columns=79, rows=20)` (width×height) if not yet negotiated
- **FR-005**: System MUST route incoming SSH data to the session's PipeInput for keyboard handling via `DataReceived(string data)` method
- **FR-006**: System MUST route Vt100Output rendering to the SSH channel via SshChannelStdout wrapper
- **FR-007**: System MUST convert LF to CRLF in output per NVT specification
- **FR-008**: System MUST notify the running application when terminal size changes via `TerminalSizeChanged(int width, int height)` method
- **FR-009**: System MUST provide a virtual `CreateSession()` method that creates a new `PromptToolkitSshSession` instance (overridable for custom session types)
- **FR-010**: System MUST provide a virtual `BeginAuth(string username)` method that returns false by default (overridable for custom authentication)
- **FR-011**: System MUST call `SetLineMode(false)` when the session starts to disable library line editing; for FxSsh this is a no-op since SSH protocol has no built-in line mode (unlike Telnet), but the call maintains API consistency with ISshChannel interface
- **FR-012**: System MUST properly dispose resources (PipeInput, AppSession) when the session ends
- **FR-013**: System MUST close the SSH channel when the interact callback completes or throws
- **FR-014**: System MUST provide an `ISshChannel` interface abstracting SSH channel operations for testability
- **FR-015**: ISshChannel MUST provide methods: `Write(string)`, `Close()`, `GetTerminalType()`, `GetTerminalSize()`, `GetEncoding()`, and `SetLineMode(bool)`

### Key Entities

- **PromptToolkitSshServer**: Inheritable server class that creates sessions for incoming SSH connections. Holds the interact callback and CPR configuration. Provides virtual authentication hook (`BeginAuth`) and virtual session factory (`CreateSession`) for subclass customization, matching Python PTK's extensibility pattern.
- **PromptToolkitSshSession**: Represents a single SSH session. Manages the lifecycle of PipeInput, Vt100Output, and AppSession for one connection. Handles data routing between SSH channel and Stroke input/output system. Exposes `Interact`, `EnableCpr`, `AppSession`, `InteractTask`, and `GetSize()`.
- **ISshChannel**: Abstraction over the SSH channel for writing data, querying terminal info, and lifecycle management. Enables testing without actual SSH connections.
- **SshChannelStdout**: Internal TextWriter wrapper that routes writes to the SSH channel with LF-to-CRLF conversion. Reports `IsAtty = true` for proper terminal detection.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can connect to a Stroke application over SSH and complete interactive workflows (prompts, dialogs, menus)
  - **Measurement**: Integration test runs asyncssh-server.py equivalent, verifies progress bar completes, prompt accepts input, dialog returns result
  - **Pass criteria**: All 3 workflow types complete successfully via SSH.NET test client

- **SC-002**: System supports 100 concurrent SSH sessions without resource exhaustion
  - **Measurement**: Stress test creates 100 simultaneous SSH connections, each runs simple interact callback
  - **Pass criteria**: Memory usage < 500MB, no session failures, all sessions complete within 60 seconds

- **SC-003**: Terminal resize events are reflected in the application within 100ms
  - **Measurement**: Test sends WindowChange event, measures time until `_on_resize()` callback fires
  - **Pass criteria**: 95th percentile latency < 100ms across 100 resize events

- **SC-004**: Session cleanup completes within 5 seconds of client disconnect (graceful wait with force-close fallback)
  - **Start point**: Client sends disconnect or TCP connection closes
  - **End point**: Session removed from Connections, all resources disposed
  - **Measurement**: Timer from disconnect detection to Connections.Count decrement

- **SC-005**: Unit tests achieve 80% code coverage for all SSH server components
  - **Scope**: All files in `src/Stroke/Contrib/Ssh/` directory
  - **Measurement**: Coverlet coverage report for `Stroke.Contrib.Ssh` namespace
  - **Exclusions**: None (all SSH code counted)

- **SC-006**: All Python Prompt Toolkit SSH module public APIs are faithfully ported with equivalent functionality
  - **Checklist**: See "API Mapping (Python PTK → C#)" section above
  - **Pass criteria**: All public classes, methods, and properties from `__all__` have C# equivalents

## Traceability Matrix

### User Stories → Functional Requirements

| User Story | Related FRs | Notes |
|------------|-------------|-------|
| US-1 (Interactive Application) | FR-001, FR-002, FR-003, FR-005, FR-006, FR-009 | Core session lifecycle |
| US-2 (Terminal Size Tracking) | FR-004, FR-008 | Size negotiation and change events |
| US-3 (Line Ending Conversion) | FR-007 | NVT compliance |
| US-4 (CPR Support) | FR-002 (enableCpr) | Optional cursor position requests |
| US-5 (Session Cleanup) | FR-012, FR-013 | Resource disposal on disconnect |

### Functional Requirements → Acceptance Scenarios

| FR | Tested By | Notes |
|----|-----------|-------|
| FR-001 | US-1 Scenario 1 | Session class with required properties |
| FR-002 | US-1 Scenario 1, US-4 Scenario 1-2 | Server with interact and enableCpr |
| FR-003 | US-1 Scenario 3 | Multiple isolated sessions |
| FR-004 | US-2 Scenario 2 | GetSize() with default and negotiated values |
| FR-005 | US-1 Scenario 2 | DataReceived routes to PipeInput |
| FR-006 | US-1 Scenario 2 | Vt100Output via SshChannelStdout |
| FR-007 | US-3 Scenario 1-2 | LF→CRLF conversion |
| FR-008 | US-2 Scenario 1 | TerminalSizeChanged triggers re-render |
| FR-009 | US-1 Scenario 1 | Virtual CreateSession |
| FR-010 | (Custom auth tests) | Virtual BeginAuth |
| FR-011 | (Unit test) | SetLineMode called on start |
| FR-012 | US-5 Scenario 1-2 | Resource disposal |
| FR-013 | US-5 Scenario 1 | Channel close on callback complete |
| FR-014 | (Unit tests) | ISshChannel interface exists |
| FR-015 | (Unit tests) | ISshChannel method signatures |

### Success Criteria → Testable Requirements

| SC | Verifying Tests | Notes |
|----|-----------------|-------|
| SC-001 | SshServerIntegrationTests | Full workflow tests |
| SC-002 | SshServerConcurrencyTests | 100-session stress test |
| SC-003 | SshSessionTests.TerminalResize | Latency measurement |
| SC-004 | SshServerLifecycleTests | Cleanup timing |
| SC-005 | Coverlet report | Coverage gate in CI |
| SC-006 | API review | Manual verification against mapping |

## Adaptation Notes (asyncssh → FxSsh)

### Why FxSsh Instead of asyncssh?

Python PTK uses **asyncssh** for SSH server functionality. In .NET:
- **SSH.NET** (the obvious choice) is client-only; no server support
- **Rebex SSH** has server support but is commercial ($899+)
- **FxSsh** is the only MIT-licensed, actively maintained SSH server for .NET

### Key Differences from Python asyncssh

| asyncssh Pattern | FxSsh Equivalent | Impact |
|------------------|------------------|--------|
| Async context managers | Event handlers | Different flow, same result |
| `set_line_mode()` | No equivalent | Documented as no-op in spec |
| Subclassing `SSHServer` | Event hooks | Composition over inheritance |
| `create_server()` coroutine | `SshServer.Start()` | Synchronous start |

### Future Maintainer Guidance

If FxSsh is ever replaced, the `ISshChannel` abstraction layer means only `SshChannel.cs` needs modification. The session, server, and stdout classes should remain unchanged.

## Testing Requirements

### Integration Test Requirements

- **Real SSH connections required**: All integration tests use actual TCP connections
- **Test client**: SSH.NET (MIT license) for SSH client functionality
- **Port allocation**: Tests use port 0 (auto-assign) to avoid conflicts
- **Cleanup**: Each test disposes server and awaits full shutdown

### Concurrency Test Requirements

Per Constitution XI, stress tests MUST verify thread safety:
- **Concurrent connections**: 10+ threads, 100+ total connections
- **Rapid connect/disconnect**: 1000 cycles in 60 seconds
- **Mixed operations**: Concurrent connect, send, resize, disconnect
- **No failures allowed**: Zero exceptions, zero resource leaks

### Test Isolation Requirements

- **Port isolation**: Each test gets unique port via port 0
- **State isolation**: No shared mutable state between tests
- **Cleanup verification**: Connections.Count == 0 after each test
- **Host key isolation**: Each test generates ephemeral key pair
