# Data Model: Telnet Server

**Feature**: 060-telnet-server
**Date**: 2026-02-03

## Entities

### TelnetServer

The main server class that listens for connections and manages the server lifecycle.

| Field | Type | Description |
|-------|------|-------------|
| Host | `string` | Bind address (default "127.0.0.1") |
| Port | `int` | Listen port (default 23) |
| Encoding | `Encoding` | Character encoding (default UTF-8) |
| Style | `IStyle?` | Optional style for formatted text |
| EnableCpr | `bool` | Enable cursor position requests (default true) |
| Connections | `IReadOnlySet<TelnetConnection>` | Active connections (thread-safe) |

**Relationships**:
- Contains 0..* `TelnetConnection` instances
- References interact callback `Func<TelnetConnection, Task>`

**State Transitions**:
```
Created → Running → Stopped
           ↑
           └─ (via cancellation)
```

### TelnetConnection

Represents a single client connection with isolated input/output.

| Field | Type | Description |
|-------|------|-------------|
| Socket | `Socket` | The underlying TCP socket |
| Address | `IPEndPoint` | Remote client address |
| Server | `TelnetServer` | Parent server reference |
| Encoding | `Encoding` | Character encoding |
| Style | `IStyle?` | Formatting style |
| Size | `Size` | Current terminal dimensions (rows, columns) |
| EnableCpr | `bool` | Cursor position requests enabled |
| IsClosed | `bool` | Connection state |

**Relationships**:
- Belongs to exactly 1 `TelnetServer`
- Contains 1 `TelnetProtocolParser`
- Contains 1 `ConnectionStdout`
- Creates 1 `IPipeInput` (during run)
- Creates 1 `Vt100Output` (after TTYPE negotiation)

**State Transitions**:
```
Created → Negotiating → Ready → Running → Closed
                                   ↑
                                   └─ (via disconnect/error)
```

### TelnetProtocolParser

Stateful parser that processes raw telnet byte streams.

| Field | Type | Description |
|-------|------|-------------|
| State | `ParserState` | Current state machine state |
| SubnegotiationBuffer | `List<byte>` | Buffer for SB...SE data |
| PendingCommand | `byte` | Command byte awaiting argument |

**Callbacks** (provided at construction):
- `DataReceived(byte[] data)` - User data received
- `SizeReceived(int rows, int columns)` - NAWS data received
- `TtypeReceived(string ttype)` - Terminal type received

**Parser States**:
```
Normal ──IAC──→ Iac ──DO/DONT/WILL/WONT──→ IacCommand ──arg──→ Normal
                 │
                 ├──SB──→ Subnegotiation ──IAC SE──→ Normal
                 │
                 └──IAC──→ Normal (emit 0xFF as data)
```

### ConnectionStdout

TextWriter wrapper for telnet-compatible output.

| Field | Type | Description |
|-------|------|-------------|
| Socket | `Socket` | Output socket |
| Encoding | `Encoding` | Character encoding |
| Buffer | `List<byte>` | Write buffer |
| IsClosed | `bool` | Closed state |

**Behavior**:
- `Write(string)` - Converts LF→CRLF, encodes, buffers, flushes
- `Flush()` - Sends buffered bytes over socket
- `Close()` - Marks as closed, prevents further writes

## Protocol Constants

### TelnetConstants

| Constant | Value | Purpose |
|----------|-------|---------|
| IAC | 255 (0xFF) | Interpret As Command |
| DONT | 254 (0xFE) | Refuse to perform option |
| DO | 253 (0xFD) | Request option |
| WONT | 252 (0xFC) | Refuse to perform option |
| WILL | 251 (0xFB) | Agree to perform option |
| SB | 250 (0xFA) | Subnegotiation Begin |
| SE | 240 (0xF0) | Subnegotiation End |
| NOP | 0 (0x00) | No Operation |
| LINEMODE | 34 (0x22) | Linemode option |
| NAWS | 31 (0x1F) | Negotiate About Window Size |
| TTYPE | 24 (0x18) | Terminal Type |
| ECHO | 1 (0x01) | Echo option |
| SGA | 3 (0x03) | Suppress Go Ahead |
| MODE | 1 (0x01) | Linemode mode |
| SEND | 1 (0x01) | TTYPE subneg: send |
| IS | 0 (0x00) | TTYPE subneg: is |

## Validation Rules

### TelnetServer

- `Host`: Non-null, valid IP address or hostname
- `Port`: 1-65535 range
- `Encoding`: Non-null

### TelnetConnection

- `Size.Rows`: 1-500 (cap large values from NAWS)
- `Size.Columns`: 1-500 (cap large values from NAWS)
- Default size: 80×24

### Input Validation

- Malformed IAC sequences: Ignore and continue
- Invalid NAWS data (not 4 bytes): Log warning, ignore
- Invalid TTYPE encoding: Fall back to "VT100"

## Sequence Diagrams

### Connection Establishment

```
Client                    Server
   |                         |
   |──────TCP Connect───────→|
   |                         |
   |←──IAC DO LINEMODE───────|
   |←──IAC WILL SGA──────────|
   |←──IAC SB LINEMODE MODE 0|
   |←──IAC WILL ECHO─────────|
   |←──IAC DO NAWS───────────|
   |←──IAC DO TTYPE──────────|
   |←──IAC SB TTYPE SEND─────|
   |                         |
   |──IAC SB NAWS w h───────→|
   |──IAC SB TTYPE IS vt100─→|
   |                         |
   |                    [Create Vt100Output]
   |                    [Invoke interact callback]
   |                         |
   |←────Prompt Output───────|
   |                         |
```

### Data Flow

```
Client                    Parser              Connection           Application
   |                         |                     |                     |
   |───raw bytes────────────→|                     |                     |
   |                         |──DataReceived──────→|                     |
   |                         |                     |──PipeInput.Send────→|
   |                         |                     |                     |──ReadKeys
   |                         |                     |                     |
   |                         |                     |←────────output──────|
   |←─────ANSI escapes───────|←────Vt100Output────|                     |
   |                         |                     |                     |
```
