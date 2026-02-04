# Data Model: SSH Server Integration

**Feature**: 061-ssh-server
**Created**: 2026-02-03

## Entities

### PromptToolkitSshServer

Main server class that accepts SSH connections and creates sessions.

| Field | Type | Description |
|-------|------|-------------|
| Host | `string` | Host address to bind (default: "127.0.0.1") |
| Port | `int` | Port number to listen on (default: 22, or 2222 for non-root) |
| Encoding | `Encoding` | Character encoding (default: UTF-8) |
| Style | `IStyle?` | Optional style for formatted text output |
| EnableCpr | `bool` | Enable cursor position requests (default: true) |
| Connections | `IReadOnlySet<PromptToolkitSshSession>` | Active session snapshot |

**Relationships**:
- One-to-many with `PromptToolkitSshSession` (server manages multiple sessions)

**State Transitions**:
- Created → Running (via `RunAsync()`)
- Running → Stopped (via cancellation token or `StopAsync()`)

**Validation Rules**:
- Port must be 0-65535 (0 = auto-assign)
- Host key must be provided (RSA, ECDSA, or Ed25519)

---

### PromptToolkitSshSession

Represents a single SSH connection with isolated I/O.

| Field | Type | Description |
|-------|------|-------------|
| Interact | `Func<PromptToolkitSshSession, Task>` | Callback for session interaction |
| EnableCpr | `bool` | Cursor position requests enabled |
| AppSession | `AppSession?` | Current application session (null until interaction starts) |
| InteractTask | `Task?` | The running interact task |

**Internal State**:
| Field | Type | Description |
|-------|------|-------------|
| _channel | `ISshChannel` | Abstracted SSH channel |
| _pipeInput | `IPipeInput` | Input for keyboard data routing |
| _vt100Output | `Vt100Output?` | Terminal output renderer |
| _size | `Size` | Current terminal dimensions |
| _closed | `bool` | Connection closed flag |

**Relationships**:
- Many-to-one with `PromptToolkitSshServer`
- One-to-one with `ISshChannel`
- One-to-one with `IPipeInput`
- One-to-one with `Vt100Output`
- One-to-one with `AppSession`

**State Transitions**:
- Created → Interacting (when `_interact()` starts)
- Interacting → Closed (when callback completes or connection drops)

---

### ISshChannel

Abstraction interface for SSH channel operations.

| Method | Returns | Description |
|--------|---------|-------------|
| Write(string) | void | Send data to client |
| Close() | void | Close the channel |
| GetTerminalType() | string | Get negotiated terminal type (e.g., "xterm") |
| GetTerminalSize() | (int Width, int Height) | Get current terminal dimensions |
| GetEncoding() | Encoding | Get channel encoding |
| SetLineMode(bool) | void | Enable/disable line mode (no-op for SSH) |

**Rationale**: Enables testability without actual SSH infrastructure (FR-014).

---

### SshChannel

FxSsh-specific implementation of `ISshChannel`.

| Field | Type | Description |
|-------|------|-------------|
| _channel | FxSsh Channel | Underlying FxSsh channel object |
| _terminalType | `string` | Cached terminal type from PtyReceived |
| _width | `int` | Cached terminal width |
| _height | `int` | Cached terminal height |
| _encoding | `Encoding` | Channel encoding |

**Responsibilities**:
- Adapter between `ISshChannel` interface and FxSsh-specific API
- Caches terminal info from FxSsh events

---

### SshChannelStdout

TextWriter wrapper for SSH channel output with LF→CRLF conversion.

| Field | Type | Description |
|-------|------|-------------|
| _channel | `ISshChannel` | Target channel for output |

| Property | Type | Description |
|----------|------|-------------|
| IsAtty | bool | Always returns `true` (terminal detection) |
| Encoding | Encoding | Channel encoding |

**Behavior**:
- All `Write()` methods convert `\n` to `\r\n` per NVT specification (FR-007)
- Reports as TTY for proper terminal detection in `Vt100Output`

---

## Concurrency Model

### Thread Safety Requirements

| Entity | Thread Safety | Mechanism |
|--------|---------------|-----------|
| PromptToolkitSshServer | Thread-safe | `ConcurrentDictionary<PromptToolkitSshSession, byte>` for connections, `Lock` for task list |
| PromptToolkitSshSession | Thread-safe | `Lock` for mutable state, `volatile` for boolean flags |
| SshChannel | Thread-safe | Delegates to FxSsh channel (thread-safe) |
| SshChannelStdout | Thread-safe | Delegates to channel |

### Concurrent Access Patterns

```
Main Thread          Accept Loop           Session N
     │                    │                    │
     │ RunAsync()         │                    │
     │───────────────────►│                    │
     │                    │ Accept connection  │
     │                    │───────────────────►│
     │                    │                    │ _interact()
     │                    │                    │ ───────────►
     │                    │  DataReceived      │
     │                    │◄───────────────────│
     │                    │  route to PipeInput│
     │                    │───────────────────►│
     │ CancellationToken  │                    │
     │───────────────────►│                    │
     │                    │ Close all sessions │
     │                    │───────────────────►│ Dispose
```

---

## Event Flow

### Connection Lifecycle

```
1. SshServer.ConnectionAccepted
   └─► Create PromptToolkitSshSession

2. UserAuthService.UserAuth
   └─► Call BeginAuth(username) virtual method
   └─► Set UserAuthArgs.Result

3. ConnectionService.PtyReceived
   └─► Store terminal type, width, height

4. ConnectionService.CommandOpened
   └─► Create SshChannel adapter
   └─► Create PipeInput, Vt100Output
   └─► Create AppSession
   └─► Start interact callback

5. Channel.DataReceived (ongoing)
   └─► Route to PipeInput.SendBytes()

6. ConnectionService.WindowChange (ongoing)
   └─► Update session size
   └─► Trigger app invalidation

7. Session ends (callback completes or disconnect)
   └─► Channel.Close()
   └─► Dispose PipeInput, AppSession
   └─► Remove from Connections
```

---

## Data Validation

### Constructor Validation

| Parameter | Validation | Error |
|-----------|------------|-------|
| port | 0 ≤ port ≤ 65535 | `ArgumentOutOfRangeException` |
| hostKey | Non-null, valid PEM | `ArgumentException` |
| interact | Non-null | `ArgumentNullException` |

### Runtime Validation

| Condition | Handling |
|-----------|----------|
| Terminal size 0x0 | Clamp to 1x1 minimum |
| Terminal size overflow | Clamp to 500x500 maximum |
| Data before session ready | Buffer until PipeInput ready |
| Auth timeout | Close connection |
