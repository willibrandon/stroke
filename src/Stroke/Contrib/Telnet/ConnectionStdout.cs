using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Stroke.Contrib.Telnet;

/// <summary>
/// TextWriter wrapper for telnet socket output.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>_ConnectionStdout</c> class.
/// It wraps a socket and provides a TextWriter interface compatible with
/// <see cref="Output.Vt100Output"/>.
/// </para>
/// <para>
/// Key behaviors:
/// <list type="bullet">
/// <item>LF → CRLF conversion per telnet NVT specification (FR-006)</item>
/// <item>Buffered writes with explicit flush</item>
/// <item>Silent no-op after close (won't throw on closed socket)</item>
/// </list>
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. Write and Flush operations are
/// synchronized.
/// </para>
/// </remarks>
internal sealed class ConnectionStdout : TextWriter
{
    private static readonly ILogger _logger = StrokeLogger.CreateLogger("Stroke.Telnet.Connection");

    private readonly Socket _socket;
    private readonly Encoding _encoding;
    private readonly Lock _lock = new();
    private readonly List<byte> _buffer = new();
    private volatile bool _closed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStdout"/> class.
    /// </summary>
    /// <param name="socket">The socket to write to.</param>
    /// <param name="encoding">The character encoding.</param>
    public ConnectionStdout(Socket socket, Encoding encoding)
    {
        _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    /// <summary>
    /// Gets the character encoding for this writer.
    /// </summary>
    public override Encoding Encoding => _encoding;

    /// <summary>
    /// Gets whether this writer has been closed.
    /// </summary>
    public bool IsClosed => _closed;

    /// <summary>
    /// Returns whether the underlying stream supports TTY features.
    /// Always returns true for telnet (we pretend to be a TTY).
    /// </summary>
    internal bool IsAtty => true;

    /// <summary>
    /// Writes a string to the buffer.
    /// </summary>
    /// <param name="value">The string to write.</param>
    /// <remarks>
    /// <para>
    /// Line endings are converted: LF (0x0A) becomes CRLF (0x0D 0x0A).
    /// This is per the telnet Network Virtual Terminal specification.
    /// </para>
    /// <para>
    /// Note: This may double-convert existing CRLF sequences to CR-CR-LF.
    /// This matches Python Prompt Toolkit's behavior.
    /// </para>
    /// <para>
    /// After writing, the buffer is automatically flushed.
    /// </para>
    /// <para>
    /// If the connection is closed, this is a no-op.
    /// </para>
    /// </remarks>
    public override void Write(string? value)
    {
        if (value == null || _closed)
        {
            return;
        }

        // LF → CRLF conversion per telnet NVT spec (FR-006)
        var converted = value.Replace("\n", "\r\n");

        using (_lock.EnterScope())
        {
            if (_closed)
            {
                return;
            }

            _buffer.AddRange(_encoding.GetBytes(converted));
            FlushUnsafe();
        }
    }

    /// <summary>
    /// Writes a character to the buffer.
    /// </summary>
    /// <param name="value">The character to write.</param>
    public override void Write(char value)
    {
        Write(value.ToString());
    }

    /// <summary>
    /// Writes characters to the buffer.
    /// </summary>
    /// <param name="buffer">The character buffer.</param>
    /// <param name="index">Start index.</param>
    /// <param name="count">Number of characters.</param>
    public override void Write(char[] buffer, int index, int count)
    {
        Write(new string(buffer, index, count));
    }

    /// <summary>
    /// Flushes the buffer to the socket.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sends all buffered bytes over the socket. After flushing, the buffer
    /// is cleared.
    /// </para>
    /// <para>
    /// Socket errors (e.g., connection reset) are logged but not thrown.
    /// This allows graceful handling of client disconnections.
    /// </para>
    /// </remarks>
    public override void Flush()
    {
        using (_lock.EnterScope())
        {
            FlushUnsafe();
        }
    }

    /// <summary>
    /// Marks this writer as closed.
    /// </summary>
    /// <remarks>
    /// After closing, all write operations become no-ops. The underlying
    /// socket is NOT closed by this method (managed by TelnetConnection).
    /// </remarks>
    public new void Close()
    {
        _closed = true;
    }

    /// <summary>
    /// Internal flush without locking - caller must hold the lock.
    /// </summary>
    private void FlushUnsafe()
    {
        if (_buffer.Count == 0 || _closed)
        {
            return;
        }

        try
        {
            _socket.Send(_buffer.ToArray());
        }
        catch (SocketException ex)
        {
            // Connection may have been closed (ERR-002)
            _logger.LogDebug(ex, "Socket error during flush, connection may be closed");
        }
        catch (ObjectDisposedException)
        {
            // Socket was disposed - mark as closed
            _logger.LogDebug("Socket disposed during flush");
            _closed = true;
        }

        _buffer.Clear();
    }
}
