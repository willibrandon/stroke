# Contract: TelnetProtocolParser

**Namespace**: `Stroke.Contrib.Telnet`
**Python Source**: `prompt_toolkit.contrib.telnet.protocol.TelnetProtocolParser`

## Class Signature

```csharp
namespace Stroke.Contrib.Telnet;

/// <summary>
/// Stateful parser for the telnet protocol.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>TelnetProtocolParser</c> class.
/// It processes raw telnet byte streams, separating user data from protocol commands
/// (IAC sequences) and handling subnegotiation.
/// </para>
/// <para>
/// The parser implements a state machine that processes bytes one at a time:
/// <list type="bullet">
/// <item>User data is forwarded to the <see cref="DataReceived"/> callback</item>
/// <item>NAWS subnegotiation triggers <see cref="SizeReceived"/> callback</item>
/// <item>TTYPE subnegotiation triggers <see cref="TtypeReceived"/> callback</item>
/// <item>Other protocol commands are logged but otherwise ignored</item>
/// </list>
/// </para>
/// <para>
/// Thread safety: This class is NOT thread-safe. It should be called from a single
/// reader thread per connection.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var parser = new TelnetProtocolParser(
///     dataReceived: data => pipeInput.SendBytes(data),
///     sizeReceived: (rows, cols) => connection.Size = new Size(cols, rows),
///     ttypeReceived: ttype => CreateOutput(ttype));
///
/// // Feed data from socket
/// parser.Feed(receivedBytes);
/// </code>
/// </example>
public sealed class TelnetProtocolParser
{
    /// <summary>
    /// Callback invoked when user data is received (non-protocol bytes).
    /// </summary>
    public Action<ReadOnlySpan<byte>> DataReceived { get; }

    /// <summary>
    /// Callback invoked when terminal size is received via NAWS.
    /// </summary>
    public Action<int, int> SizeReceived { get; }

    /// <summary>
    /// Callback invoked when terminal type is received via TTYPE.
    /// </summary>
    public Action<string> TtypeReceived { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TelnetProtocolParser"/> class.
    /// </summary>
    /// <param name="dataReceived">
    /// Callback for user data. Called with spans of non-protocol bytes.
    /// </param>
    /// <param name="sizeReceived">
    /// Callback for NAWS data. Called with (rows, columns).
    /// </param>
    /// <param name="ttypeReceived">
    /// Callback for TTYPE data. Called with the terminal type string.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if any callback is null.
    /// </exception>
    public TelnetProtocolParser(
        Action<ReadOnlySpan<byte>> dataReceived,
        Action<int, int> sizeReceived,
        Action<string> ttypeReceived);

    /// <summary>
    /// Feeds raw bytes to the parser.
    /// </summary>
    /// <param name="data">Raw bytes from the socket.</param>
    /// <remarks>
    /// <para>
    /// Bytes are processed one at a time through the state machine. User data
    /// is accumulated and delivered via <see cref="DataReceived"/>. Protocol
    /// sequences are handled internally.
    /// </para>
    /// <para>
    /// Special handling:
    /// <list type="bullet">
    /// <item>IAC IAC (255 255) → single 255 byte as user data</item>
    /// <item>IAC DO/DONT/WILL/WONT arg → logged, ignored</item>
    /// <item>IAC SB NAWS ... IAC SE → triggers <see cref="SizeReceived"/></item>
    /// <item>IAC SB TTYPE IS ... IAC SE → triggers <see cref="TtypeReceived"/></item>
    /// <item>Malformed sequences → logged, ignored</item>
    /// </list>
    /// </para>
    /// </remarks>
    public void Feed(ReadOnlySpan<byte> data);
}
```

## Internal State Machine

```csharp
internal enum ParserState
{
    /// <summary>Normal state - reading user data.</summary>
    Normal,

    /// <summary>Just received IAC, waiting for command.</summary>
    Iac,

    /// <summary>Received IAC + command, waiting for argument.</summary>
    IacCommand,

    /// <summary>In subnegotiation (SB), collecting until IAC SE.</summary>
    Subnegotiation,

    /// <summary>In subnegotiation, just received IAC.</summary>
    SubnegotiationIac
}
```

## Functional Requirements Coverage

| Requirement | Method |
|-------------|--------|
| FR-003: Parse telnet data | `Feed()` |
| FR-004: Handle NAWS | `Feed()` + `SizeReceived` callback |
| FR-005: Handle TTYPE | `Feed()` + `TtypeReceived` callback |
| FR-016: Handle double-IAC | `Feed()` (IAC IAC → 0xFF data) |

## Python API Mapping

| Python | C# |
|--------|-----|
| `data_received_callback` | `DataReceived` |
| `size_received_callback` | `SizeReceived` |
| `ttype_received_callback` | `TtypeReceived` |
| `feed(data)` | `Feed(data)` |
| `received_data(data)` | (internal) calls `DataReceived` |
| `naws(data)` | (internal) calls `SizeReceived` |
| `ttype(data)` | (internal) calls `TtypeReceived` |
| `negotiate(data)` | (internal) routes to naws/ttype |
| `command_received(cmd, data)` | (internal) logs commands |

## Protocol Sequences Handled

### NAWS (Negotiate About Window Size)

```
Server: IAC DO NAWS
Client: IAC SB NAWS <width-high> <width-low> <height-high> <height-low> IAC SE
```

Parser extracts width (columns) and height (rows) as 16-bit big-endian values.

### TTYPE (Terminal Type)

```
Server: IAC DO TTYPE
Server: IAC SB TTYPE SEND IAC SE
Client: IAC SB TTYPE IS <terminal-type-string> IAC SE
```

Parser extracts terminal type as ASCII string (e.g., "xterm-256color").
