using System.Text;
using Microsoft.Extensions.Logging;

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
    /// <summary>Maximum subnegotiation buffer size (per EC-007).</summary>
    private const int MaxSubnegotiationBufferSize = 1024;

    private static readonly ILogger _logger = StrokeLogger.CreateLogger("Stroke.Telnet.Protocol");

    private ParserState _state = ParserState.Normal;
    private byte _pendingCommand;
    private readonly List<byte> _subnegotiationBuffer = new();
    private readonly List<byte> _dataBuffer = new();

    /// <summary>
    /// Callback invoked when user data is received (non-protocol bytes).
    /// </summary>
    public DataReceivedCallback DataReceived { get; }

    /// <summary>
    /// Callback invoked when terminal size is received via NAWS.
    /// </summary>
    public SizeReceivedCallback SizeReceived { get; }

    /// <summary>
    /// Callback invoked when terminal type is received via TTYPE.
    /// </summary>
    public TtypeReceivedCallback TtypeReceived { get; }

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
        DataReceivedCallback dataReceived,
        SizeReceivedCallback sizeReceived,
        TtypeReceivedCallback ttypeReceived)
    {
        DataReceived = dataReceived ?? throw new ArgumentNullException(nameof(dataReceived));
        SizeReceived = sizeReceived ?? throw new ArgumentNullException(nameof(sizeReceived));
        TtypeReceived = ttypeReceived ?? throw new ArgumentNullException(nameof(ttypeReceived));
    }

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
    public void Feed(ReadOnlySpan<byte> data)
    {
        foreach (var b in data)
        {
            ProcessByte(b);
        }

        // Flush any accumulated data
        FlushDataBuffer();
    }

    private void ProcessByte(byte b)
    {
        switch (_state)
        {
            case ParserState.Normal:
                ProcessNormalState(b);
                break;

            case ParserState.Iac:
                ProcessIacState(b);
                break;

            case ParserState.IacCommand:
                ProcessIacCommandState(b);
                break;

            case ParserState.Subnegotiation:
                ProcessSubnegotiationState(b);
                break;

            case ParserState.SubnegotiationIac:
                ProcessSubnegotiationIacState(b);
                break;
        }
    }

    private void ProcessNormalState(byte b)
    {
        if (b == TelnetConstants.NOP)
        {
            // NOP bytes are passed through as data (FR-003c)
            _dataBuffer.Add(b);
        }
        else if (b == TelnetConstants.IAC)
        {
            // Flush data before processing IAC
            FlushDataBuffer();
            _state = ParserState.Iac;
        }
        else
        {
            // Regular user data
            _dataBuffer.Add(b);
        }
    }

    private void ProcessIacState(byte b)
    {
        if (b == TelnetConstants.IAC)
        {
            // Double IAC → single 0xFF as user data (FR-016)
            _dataBuffer.Add(TelnetConstants.IAC);
            _state = ParserState.Normal;
        }
        else if (b == TelnetConstants.DO || b == TelnetConstants.DONT ||
                 b == TelnetConstants.WILL || b == TelnetConstants.WONT)
        {
            // IAC DO/DONT/WILL/WONT → need one more byte
            _pendingCommand = b;
            _state = ParserState.IacCommand;
        }
        else if (b == TelnetConstants.SB)
        {
            // Start subnegotiation
            _subnegotiationBuffer.Clear();
            _state = ParserState.Subnegotiation;
        }
        else if (IsSimpleCommand(b))
        {
            // Simple commands: NOP, DM, BRK, IP, AO, AYT, EC, EL, GA
            // Log and ignore (FR-003b)
            LogCommand(b, []);
            _state = ParserState.Normal;
        }
        else
        {
            // Unknown IAC sequence - log and return to normal
            LogMalformed($"Unknown IAC command: {b}");
            _state = ParserState.Normal;
        }
    }

    private void ProcessIacCommandState(byte b)
    {
        // Got the argument for DO/DONT/WILL/WONT
        LogCommand(_pendingCommand, [b]);
        _state = ParserState.Normal;
    }

    private void ProcessSubnegotiationState(byte b)
    {
        if (b == TelnetConstants.IAC)
        {
            _state = ParserState.SubnegotiationIac;
        }
        else if (_subnegotiationBuffer.Count < MaxSubnegotiationBufferSize)
        {
            _subnegotiationBuffer.Add(b);
        }
        else
        {
            // Buffer overflow - log warning but continue collecting
            // until we see IAC SE (EC-007)
            LogMalformed($"Subnegotiation buffer overflow (>{MaxSubnegotiationBufferSize} bytes)");
        }
    }

    private void ProcessSubnegotiationIacState(byte b)
    {
        if (b == TelnetConstants.SE)
        {
            // End of subnegotiation
            ProcessSubnegotiation();
            _state = ParserState.Normal;
        }
        else if (b == TelnetConstants.IAC)
        {
            // Escaped IAC within subnegotiation
            if (_subnegotiationBuffer.Count < MaxSubnegotiationBufferSize)
            {
                _subnegotiationBuffer.Add(TelnetConstants.IAC);
            }
            _state = ParserState.Subnegotiation;
        }
        else
        {
            // Unexpected byte after IAC in subnegotiation - add both and continue
            if (_subnegotiationBuffer.Count < MaxSubnegotiationBufferSize - 1)
            {
                _subnegotiationBuffer.Add(b);
            }
            _state = ParserState.Subnegotiation;
        }
    }

    private void ProcessSubnegotiation()
    {
        if (_subnegotiationBuffer.Count == 0)
        {
            LogMalformed("Empty subnegotiation");
            return;
        }

        var command = _subnegotiationBuffer[0];
        var payload = _subnegotiationBuffer.Skip(1).ToArray();

        if (command == TelnetConstants.NAWS)
        {
            ProcessNaws(payload);
        }
        else if (command == TelnetConstants.TTYPE)
        {
            ProcessTtype(payload);
        }
        else
        {
            // Unknown subnegotiation - log and ignore
            LogCommand(command, payload);
        }
    }

    private void ProcessNaws(byte[] data)
    {
        // NAWS: 4 bytes - columns (2 bytes big-endian) + rows (2 bytes big-endian)
        if (data.Length == 4)
        {
            var columns = (data[0] << 8) | data[1];
            var rows = (data[2] << 8) | data[3];
            SizeReceived(rows, columns);
        }
        else
        {
            LogMalformed($"Invalid NAWS data length: {data.Length} (expected 4)");
        }
    }

    private void ProcessTtype(byte[] data)
    {
        if (data.Length == 0)
        {
            LogMalformed("Empty TTYPE data");
            return;
        }

        var subcmd = data[0];
        if (subcmd == TelnetConstants.IS)
        {
            // Terminal type follows as ASCII
            try
            {
                var ttype = Encoding.ASCII.GetString(data, 1, data.Length - 1);
                TtypeReceived(ttype);
            }
            catch (Exception ex)
            {
                LogMalformed($"Invalid TTYPE encoding: {ex.Message}");
                TtypeReceived("VT100"); // Fallback
            }
        }
        else
        {
            LogMalformed($"Non-IS TTYPE subnegotiation: {subcmd}");
        }
    }

    private void FlushDataBuffer()
    {
        if (_dataBuffer.Count > 0)
        {
            DataReceived(_dataBuffer.ToArray());
            _dataBuffer.Clear();
        }
    }

    private static bool IsSimpleCommand(byte b)
    {
        return b == TelnetConstants.NOP ||
               b == TelnetConstants.DM ||
               b == TelnetConstants.BRK ||
               b == TelnetConstants.IP ||
               b == TelnetConstants.AO ||
               b == TelnetConstants.AYT ||
               b == TelnetConstants.EC ||
               b == TelnetConstants.EL ||
               b == TelnetConstants.GA;
    }

    private static void LogCommand(byte command, byte[] data)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Telnet command: {Command:X2} with {DataLength} bytes", command, data.Length);
        }
    }

    private static void LogMalformed(string message)
    {
        _logger.LogWarning("Malformed telnet data: {Message}", message);
    }

    /// <summary>
    /// Delegate for receiving user data.
    /// </summary>
    /// <param name="data">The user data bytes.</param>
    public delegate void DataReceivedCallback(ReadOnlySpan<byte> data);

    /// <summary>
    /// Delegate for receiving terminal size.
    /// </summary>
    /// <param name="rows">Number of rows.</param>
    /// <param name="columns">Number of columns.</param>
    public delegate void SizeReceivedCallback(int rows, int columns);

    /// <summary>
    /// Delegate for receiving terminal type.
    /// </summary>
    /// <param name="ttype">Terminal type string.</param>
    public delegate void TtypeReceivedCallback(string ttype);
}

/// <summary>
/// Parser state machine states.
/// </summary>
internal enum ParserState
{
    /// <summary>Normal state - reading user data.</summary>
    Normal,

    /// <summary>Just received IAC, waiting for command.</summary>
    Iac,

    /// <summary>Received IAC + command (DO/DONT/WILL/WONT), waiting for argument.</summary>
    IacCommand,

    /// <summary>In subnegotiation (SB), collecting until IAC SE.</summary>
    Subnegotiation,

    /// <summary>In subnegotiation, just received IAC.</summary>
    SubnegotiationIac
}
