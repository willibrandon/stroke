# Research: Telnet Server

**Feature**: 060-telnet-server
**Date**: 2026-02-03

## Overview

This document captures research findings for porting Python Prompt Toolkit's telnet server to Stroke. The Python implementation serves as the authoritative reference.

## Python Source Analysis

### Module Structure

Python Prompt Toolkit's telnet module at `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/contrib/telnet/`:

| File | Purpose | Key Classes/Functions |
|------|---------|----------------------|
| `__init__.py` | Public exports | `TelnetServer` |
| `server.py` | Server and connection | `TelnetServer`, `TelnetConnection`, `_ConnectionStdout` |
| `protocol.py` | Protocol parsing | `TelnetProtocolParser`, protocol constants |
| `log.py` | Logging | `logger` |

### Key Design Decisions from Python

#### 1. Connection Lifecycle

**Python Pattern** (from `server.py:389-428`):
```python
async def run() -> None:
    try:
        with create_pipe_input() as vt100_input:
            connection = TelnetConnection(...)
            self.connections.add(connection)
            try:
                await connection.run_application()
            finally:
                self.connections.remove(connection)
    except EOFError:
        # Client disconnected
    except KeyboardInterrupt:
        # Ctrl+C
    except BaseException as e:
        print(f"Got {type(e).__name__}", e)
```

**Decision**: Each connection gets its own `PipeInput` via context manager. Connection is added to server's set before `run_application()` and removed in `finally`.

**C# Adaptation**: Use `using` statement with `PipeInput.Create()`. Same try/finally pattern for connection set management.

#### 2. Protocol Parser State Machine

**Python Pattern** (from `protocol.py:156-209`):
```python
def _parse_coroutine(self) -> Generator[None, bytes, None]:
    while True:
        d = yield
        if d == int2byte(0):
            pass  # NOP
        elif d == IAC:
            d2 = yield
            if d2 == IAC:
                self.received_data(d2)  # Escaped IAC
            elif d2 in (DO, DONT, WILL, WONT):
                d3 = yield
                self.command_received(d2, d3)
            elif d2 == SB:
                # Subnegotiation
                data = []
                while True:
                    d3 = yield
                    if d3 == IAC:
                        d4 = yield
                        if d4 == SE:
                            break
                        else:
                            data.append(d4)
                    else:
                        data.append(d3)
                self.negotiate(b"".join(data))
        else:
            self.received_data(d)
```

**Decision**: Python uses a generator-based coroutine as a state machine. Each `yield` suspends until the next byte arrives.

**C# Adaptation**: Since C# doesn't have Python's generator-based coroutines, implement as explicit state machine with enum states:
- `Normal` - Default state, reading user data
- `Iac` - Just received IAC, waiting for command
- `IacCommand` - Received IAC + command byte, waiting for argument
- `Subnegotiation` - In SB sequence, collecting until IAC SE

#### 3. Output LF→CRLF Conversion

**Python Pattern** (from `server.py:90-93`):
```python
def write(self, data: str) -> None:
    data = data.replace("\n", "\r\n")
    self._buffer.append(data.encode(self._encoding, errors=self._errors))
    self.flush()
```

**Decision**: All output converts LF to CRLF per telnet NVT specification.

**C# Adaptation**: Same approach in `ConnectionStdout.Write()`.

#### 4. Terminal Type Negotiation

**Python Pattern** (from `server.py:68-74`):
```python
# Negotiate terminal type
connection.send(IAC + DO + TTYPE)
# Request the terminal type
connection.send(IAC + SB + TTYPE + SEND + IAC + SE)
```

**Decision**: Server requests terminal type via DO TTYPE, then sends subnegotiation to get the actual value. The `Vt100Output` is created only after TTYPE is received.

**C# Adaptation**: Same negotiation sequence. Use `TaskCompletionSource` or `AsyncAutoResetEvent` to signal when TTYPE is received before proceeding.

#### 5. Window Size Updates

**Python Pattern** (from `server.py:163-167`):
```python
def size_received(rows: int, columns: int) -> None:
    self.size = Size(rows=rows, columns=columns)
    if self.vt100_output is not None and self.context:
        self.context.run(lambda: get_app()._on_resize())
```

**Decision**: When NAWS data arrives, update size and notify the running application via context.

**C# Adaptation**: Use `Vt100Output.GetSize` callback pattern. Invoke `Application._OnResize()` via stored context.

## Technology Decisions

### 1. PipeInput Implementation

**Research**: Python Prompt Toolkit has `PipeInput` as an abstract base with `PosixPipeInput` and `Win32PipeInput` implementations.

**Decision**: Implement `PipeInput` in `Stroke.Input` namespace (if not already present) as a cross-platform pipe-based input that:
- Creates an OS pipe (read/write file descriptors)
- Implements `IInput` interface
- Provides `SendBytes(byte[] data)` and `SendText(string data)` methods
- Inherits VT100 parsing from `Vt100Input` base

**Alternatives Considered**:
- `MemoryStream` - Rejected: doesn't support async wait for data arrival
- Named pipes - Rejected: unnecessary complexity for in-process communication

### 2. Socket Management

**Research**: Python uses raw sockets with `socket.socket()` and event loop readers.

**Decision**: Use `System.Net.Sockets.Socket` with `SocketAsyncEventArgs` for efficient async I/O:
- `AcceptAsync` for new connections
- `ReceiveAsync` for reading data
- Direct `Send` for writing (telnet is latency-sensitive)

**Alternatives Considered**:
- `TcpListener`/`TcpClient` - Rejected: less control over socket options
- Kestrel - Rejected: massive overkill for raw telnet

### 3. Thread Safety Model

**Research**: Python assumes single-threaded asyncio event loop. Stroke requires thread safety per Constitution XI.

**Decision**:
- `TelnetServer.Connections`: Use `ConcurrentDictionary<TelnetConnection, byte>` as thread-safe set
- `TelnetConnection` methods: Use `Lock` for state mutations
- `TelnetProtocolParser`: Stateful but single-threaded per connection (no lock needed if called from one reader)

### 4. Logging

**Research**: Python uses `logging.getLogger(__package__)`.

**Decision**: Use existing `Stroke.Logging.StrokeLogger` pattern (if present) or create `Microsoft.Extensions.Logging.ILogger` instance for telnet namespace.

**Alternatives Considered**:
- Console.WriteLine - Rejected: not configurable, noisy
- No logging - Rejected: debugging network issues requires visibility

### 5. Encoding

**Research**: Python defaults to UTF-8 but supports configurable encoding.

**Decision**: Default to `Encoding.UTF8`. Accept `System.Text.Encoding` in constructor for configurability.

## Dependencies

### Required Existing Components

| Component | Namespace | Purpose |
|-----------|-----------|---------|
| `IInput` | `Stroke.Input` | Input abstraction |
| `IOutput` | `Stroke.Output` | Output abstraction |
| `Vt100Output` | `Stroke.Output` | VT100 terminal output |
| `AppSession` | `Stroke.Application` | Session management |
| `AppContext` | `Stroke.Application` | Context management |
| `Size` | `Stroke.Core.Primitives` | Terminal dimensions |
| `IStyle` | `Stroke.Styles` | Formatted text styling |

### Required New Components

| Component | Reason |
|-----------|--------|
| `PipeInput` | Needed for telnet input injection - may already exist |

## Test Strategy

### Unit Tests (TelnetProtocolParserTests)

- Parse IAC sequences correctly
- Handle double-IAC escape
- Parse NAWS subnegotiation
- Parse TTYPE subnegotiation
- Handle malformed sequences gracefully

### Integration Tests (TelnetServerTests)

- Accept TCP connections
- Complete negotiation handshake
- Echo input back to client
- Handle concurrent connections
- Graceful shutdown

### End-to-End Tests (manual/TUI driver)

- Connect with real telnet client
- Verify prompt rendering
- Test key bindings work
- Test window resize

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| PipeInput missing | Medium | High | Implement before telnet server |
| Socket API differences | Low | Medium | Use cross-platform .NET BCL only |
| Thread safety bugs | Medium | High | Add concurrent stress tests |
| Protocol edge cases | Low | Low | Port Python's test cases |

## Conclusion

The port is straightforward due to excellent Python source availability. Main adaptation work is:
1. Converting generator-based parser to explicit state machine
2. Implementing PipeInput if missing
3. Adding thread safety per Constitution XI

All unknowns are resolved. Proceeding to Phase 1.
