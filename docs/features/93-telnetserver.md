# Feature 93: Telnet Server

## Overview

Implement a Telnet server that allows running prompt toolkit applications over the Telnet protocol. This enables building network-accessible REPLs, command-line interfaces, and interactive shells.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/contrib/telnet/`

## Public API

### TelnetServer

```csharp
namespace Stroke.Contrib.Telnet;

/// <summary>
/// Telnet server implementation.
/// Accepts incoming telnet connections and runs prompt toolkit applications.
/// </summary>
public sealed class TelnetServer
{
    /// <summary>
    /// Create a telnet server.
    /// </summary>
    /// <param name="host">Host address to bind to.</param>
    /// <param name="port">Port number (default 23).</param>
    /// <param name="interact">Async callback for each connection.</param>
    /// <param name="encoding">Character encoding (default UTF-8).</param>
    /// <param name="style">Style for formatted text output.</param>
    /// <param name="enableCpr">Enable cursor position requests.</param>
    public TelnetServer(
        string host = "127.0.0.1",
        int port = 23,
        Func<TelnetConnection, Task>? interact = null,
        string encoding = "utf-8",
        IStyle? style = null,
        bool enableCpr = true);

    /// <summary>
    /// Host address the server is bound to.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Port number the server is listening on.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// Character encoding for connections.
    /// </summary>
    public string Encoding { get; }

    /// <summary>
    /// Active connections.
    /// </summary>
    public IReadOnlySet<TelnetConnection> Connections { get; }

    /// <summary>
    /// Run the telnet server until cancelled.
    /// </summary>
    /// <param name="readyCallback">Called when server starts listening.</param>
    /// <param name="cancellationToken">Token to stop the server.</param>
    public Task RunAsync(
        Action? readyCallback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Start the server in the background.
    /// </summary>
    [Obsolete("Use RunAsync instead")]
    public void Start();

    /// <summary>
    /// Stop a server started with Start().
    /// </summary>
    [Obsolete("Use RunAsync with cancellation instead")]
    public Task StopAsync();
}
```

### TelnetConnection

```csharp
namespace Stroke.Contrib.Telnet;

/// <summary>
/// Represents a single telnet client connection.
/// </summary>
public sealed class TelnetConnection
{
    /// <summary>
    /// The underlying socket connection.
    /// </summary>
    public Socket Socket { get; }

    /// <summary>
    /// Remote endpoint address.
    /// </summary>
    public IPEndPoint RemoteEndPoint { get; }

    /// <summary>
    /// The parent server.
    /// </summary>
    public TelnetServer Server { get; }

    /// <summary>
    /// Character encoding for this connection.
    /// </summary>
    public string Encoding { get; }

    /// <summary>
    /// Current terminal size.
    /// </summary>
    public Size Size { get; }

    /// <summary>
    /// Whether the connection is closed.
    /// </summary>
    public bool IsClosed { get; }

    /// <summary>
    /// Send formatted text to the client.
    /// </summary>
    /// <param name="text">Text to send.</param>
    public void Send(IFormattedText text);

    /// <summary>
    /// Send formatted text above the current prompt.
    /// </summary>
    /// <param name="text">Text to send.</param>
    public void SendAbovePrompt(IFormattedText text);

    /// <summary>
    /// Erase the screen and move cursor to top.
    /// </summary>
    public void EraseScreen();

    /// <summary>
    /// Close the connection.
    /// </summary>
    public void Close();

    /// <summary>
    /// Feed incoming data to the parser.
    /// </summary>
    /// <param name="data">Raw bytes from client.</param>
    internal void Feed(byte[] data);

    /// <summary>
    /// Run the application for this connection.
    /// </summary>
    internal Task RunApplicationAsync();
}
```

### TelnetProtocolParser

```csharp
namespace Stroke.Contrib.Telnet;

/// <summary>
/// Parser for the Telnet protocol.
/// Handles IAC sequences, NAWS (window size), and terminal type negotiation.
/// </summary>
public sealed class TelnetProtocolParser
{
    /// <summary>
    /// Create a telnet protocol parser.
    /// </summary>
    /// <param name="dataReceived">Callback for user data.</param>
    /// <param name="sizeReceived">Callback for window size changes.</param>
    /// <param name="ttypeReceived">Callback for terminal type.</param>
    public TelnetProtocolParser(
        Action<byte[]> dataReceived,
        Action<int, int> sizeReceived,
        Action<string> ttypeReceived);

    /// <summary>
    /// Feed data to the parser.
    /// </summary>
    /// <param name="data">Raw bytes to parse.</param>
    public void Feed(byte[] data);
}
```

### Telnet Protocol Constants

```csharp
namespace Stroke.Contrib.Telnet;

/// <summary>
/// Telnet protocol constants.
/// </summary>
public static class TelnetConstants
{
    /// <summary>Interpret As Command</summary>
    public static readonly byte IAC = 255;

    /// <summary>Do option</summary>
    public static readonly byte DO = 253;

    /// <summary>Don't option</summary>
    public static readonly byte DONT = 254;

    /// <summary>Will option</summary>
    public static readonly byte WILL = 251;

    /// <summary>Won't option</summary>
    public static readonly byte WONT = 252;

    /// <summary>Subnegotiation Begin</summary>
    public static readonly byte SB = 250;

    /// <summary>Subnegotiation End</summary>
    public static readonly byte SE = 240;

    /// <summary>Echo option</summary>
    public static readonly byte ECHO = 1;

    /// <summary>Negotiate About Window Size</summary>
    public static readonly byte NAWS = 31;

    /// <summary>Linemode option</summary>
    public static readonly byte LINEMODE = 34;

    /// <summary>Suppress Go Ahead</summary>
    public static readonly byte SUPPRESS_GO_AHEAD = 3;

    /// <summary>Terminal Type option</summary>
    public static readonly byte TTYPE = 24;

    /// <summary>Send subcommand</summary>
    public static readonly byte SEND = 1;

    /// <summary>Is subcommand</summary>
    public static readonly byte IS = 0;

    /// <summary>No Operation</summary>
    public static readonly byte NOP = 0;

    /// <summary>Data Mark</summary>
    public static readonly byte DM = 242;

    /// <summary>Break</summary>
    public static readonly byte BRK = 243;

    /// <summary>Interrupt Process</summary>
    public static readonly byte IP = 244;

    /// <summary>Abort Output</summary>
    public static readonly byte AO = 245;

    /// <summary>Are You There</summary>
    public static readonly byte AYT = 246;

    /// <summary>Erase Character</summary>
    public static readonly byte EC = 247;

    /// <summary>Erase Line</summary>
    public static readonly byte EL = 248;

    /// <summary>Go Ahead</summary>
    public static readonly byte GA = 249;
}
```

## Project Structure

```
src/Stroke/
└── Contrib/
    └── Telnet/
        ├── TelnetServer.cs
        ├── TelnetConnection.cs
        ├── TelnetProtocolParser.cs
        └── TelnetConstants.cs
tests/Stroke.Tests/
└── Contrib/
    └── Telnet/
        └── TelnetServerTests.cs
```

## Implementation Notes

### Connection Initialization

```csharp
private static void InitializeTelnet(Socket connection)
{
    // Iac Do Linemode
    connection.Send(new byte[] { IAC, DO, LINEMODE });

    // Suppress Go Ahead (important for Putty echoing)
    connection.Send(new byte[] { IAC, WILL, SUPPRESS_GO_AHEAD });

    // Iac sb Linemode Mode 0
    connection.Send(new byte[] { IAC, SB, LINEMODE, MODE, 0, IAC, SE });

    // IAC Will Echo
    connection.Send(new byte[] { IAC, WILL, ECHO });

    // Negotiate window size
    connection.Send(new byte[] { IAC, DO, NAWS });

    // Negotiate terminal type
    connection.Send(new byte[] { IAC, DO, TTYPE });
    connection.Send(new byte[] { IAC, SB, TTYPE, SEND, IAC, SE });
}
```

### ConnectionStdout Wrapper

```csharp
internal sealed class ConnectionStdout : TextWriter
{
    private readonly Socket _connection;
    private readonly Encoding _encoding;
    private readonly List<byte> _buffer = new();
    private bool _closed;

    public ConnectionStdout(Socket connection, Encoding encoding)
    {
        _connection = connection;
        _encoding = encoding;
    }

    public override Encoding Encoding => _encoding;

    public override void Write(string? value)
    {
        if (value == null || _closed) return;

        // Convert LF to CRLF for telnet
        value = value.Replace("\n", "\r\n");
        _buffer.AddRange(_encoding.GetBytes(value));
        Flush();
    }

    public override void Flush()
    {
        if (_closed || _buffer.Count == 0) return;

        try
        {
            _connection.Send(_buffer.ToArray());
        }
        catch (SocketException)
        {
            // Connection lost
        }

        _buffer.Clear();
    }

    public void Close() => _closed = true;
}
```

### Protocol Parser State Machine

```csharp
public sealed class TelnetProtocolParser
{
    private enum State { Data, Iac, IacCommand, Subnegotiation }

    private State _state = State.Data;
    private byte _command;
    private readonly List<byte> _subData = new();

    public void Feed(byte[] data)
    {
        foreach (var b in data)
        {
            switch (_state)
            {
                case State.Data:
                    if (b == IAC)
                        _state = State.Iac;
                    else if (b != 0)
                        _dataReceived(new[] { b });
                    break;

                case State.Iac:
                    if (b == IAC)
                    {
                        _dataReceived(new[] { b });
                        _state = State.Data;
                    }
                    else if (b == SB)
                    {
                        _subData.Clear();
                        _state = State.Subnegotiation;
                    }
                    else if (b is DO or DONT or WILL or WONT)
                    {
                        _command = b;
                        _state = State.IacCommand;
                    }
                    else
                    {
                        CommandReceived(b, Array.Empty<byte>());
                        _state = State.Data;
                    }
                    break;

                case State.IacCommand:
                    CommandReceived(_command, new[] { b });
                    _state = State.Data;
                    break;

                case State.Subnegotiation:
                    if (b == IAC)
                        _state = State.SubnegotiationIac;
                    else
                        _subData.Add(b);
                    break;

                case State.SubnegotiationIac:
                    if (b == SE)
                    {
                        Negotiate(_subData.ToArray());
                        _state = State.Data;
                    }
                    else
                    {
                        _subData.Add(b);
                        _state = State.Subnegotiation;
                    }
                    break;
            }
        }
    }

    private void Negotiate(byte[] data)
    {
        if (data.Length == 0) return;

        var command = data[0];
        var payload = data.AsSpan(1).ToArray();

        if (command == NAWS && payload.Length == 4)
        {
            var columns = (payload[0] << 8) | payload[1];
            var rows = (payload[2] << 8) | payload[3];
            _sizeReceived(rows, columns);
        }
        else if (command == TTYPE && payload.Length > 0 && payload[0] == IS)
        {
            var ttype = Encoding.ASCII.GetString(payload, 1, payload.Length - 1);
            _ttypeReceived(ttype);
        }
    }
}
```

### Usage Example

```csharp
async Task Interact(TelnetConnection connection)
{
    connection.Send("Welcome to the telnet server!\n");

    var session = new PromptSession();
    while (true)
    {
        var result = await session.PromptAsync(">> ");
        if (result == "exit")
            break;

        connection.Send($"You typed: {result}\n");
    }
}

var server = new TelnetServer(
    host: "0.0.0.0",
    port: 2323,
    interact: Interact
);

await server.RunAsync();
```

## Dependencies

- Feature 1: Document model
- Feature 3: Application
- Feature 5: Input abstraction (PipeInput)
- Feature 6: VT100 Output
- Feature 85: Formatted text utilities
- System.Net.Sockets

## Implementation Tasks

1. Implement TelnetConstants
2. Implement TelnetProtocolParser state machine
3. Implement ConnectionStdout wrapper
4. Implement TelnetConnection class
5. Implement TelnetServer with async accept loop
6. Integrate with AppSession
7. Handle terminal size changes
8. Handle terminal type negotiation
9. Write unit tests

## Acceptance Criteria

- [ ] Server accepts telnet connections
- [ ] Negotiates terminal type and size
- [ ] Converts LF to CRLF for output
- [ ] Parses IAC sequences correctly
- [ ] Updates Size on NAWS
- [ ] Creates proper AppSession per connection
- [ ] Handles multiple concurrent connections
- [ ] Cleans up on disconnect
- [ ] Unit tests achieve 80% coverage
