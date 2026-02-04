using System.Text;
using FxSsh.Services;

namespace Stroke.Contrib.Ssh;

/// <summary>
/// FxSsh-specific implementation of <see cref="ISshChannel"/>.
/// </summary>
/// <remarks>
/// <para>
/// This class adapts the FxSsh <see cref="SessionChannel"/> to the
/// <see cref="ISshChannel"/> interface, enabling testability and abstraction
/// from the underlying SSH library.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. All read operations are atomic,
/// and the underlying FxSsh channel handles write synchronization.
/// </para>
/// </remarks>
internal sealed class SshChannel : ISshChannel
{
    private readonly SessionChannel _channel;
    private readonly Encoding _encoding;
    private readonly Lock _lock = new();
    private string _terminalType = "vt100";
    private int _width = 79;
    private int _height = 20;
    private volatile bool _closed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SshChannel"/> class.
    /// </summary>
    /// <param name="channel">The underlying FxSsh channel.</param>
    /// <param name="encoding">The character encoding for the channel.</param>
    public SshChannel(SessionChannel channel, Encoding encoding)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    /// <summary>
    /// Gets whether this channel has been closed.
    /// </summary>
    public bool IsClosed => _closed;

    /// <inheritdoc/>
    public void Write(string data)
    {
        if (_closed || string.IsNullOrEmpty(data))
        {
            return;
        }

        try
        {
            var bytes = _encoding.GetBytes(data);
            _channel.SendData(bytes);
        }
        catch (ObjectDisposedException)
        {
            _closed = true;
        }
        catch (Exception)
        {
            // Swallow write errors - connection may be closed
        }
    }

    /// <inheritdoc/>
    public void Close()
    {
        if (_closed)
        {
            return;
        }

        _closed = true;

        try
        {
            // Small delay before closing to allow client to finish shell setup.
            // Without this, if the interact callback returns immediately (e.g., in tests),
            // the channel closes before SSH.NET finishes its ShellStream handshake,
            // causing null waitHandle errors on the client side.
            Thread.Sleep(50);
            _channel.SendClose();
        }
        catch
        {
            // Ignore close errors
        }
    }

    /// <inheritdoc/>
    public string GetTerminalType()
    {
        using (_lock.EnterScope())
        {
            return _terminalType;
        }
    }

    /// <inheritdoc/>
    public (int Width, int Height) GetTerminalSize()
    {
        using (_lock.EnterScope())
        {
            return (_width, _height);
        }
    }

    /// <inheritdoc/>
    public Encoding GetEncoding()
    {
        return _encoding;
    }

    /// <inheritdoc/>
    public void SetLineMode(bool enabled)
    {
        // No-op for SSH - SSH doesn't have built-in line mode like Telnet.
        // The Stroke application handles all line editing.
        // This method exists for API consistency with Python Prompt Toolkit.
    }

    /// <summary>
    /// Updates the terminal type from PTY request.
    /// </summary>
    /// <param name="terminalType">The terminal type (e.g., "xterm", "xterm-256color").</param>
    internal void SetTerminalType(string terminalType)
    {
        using (_lock.EnterScope())
        {
            _terminalType = terminalType ?? "vt100";
        }
    }

    /// <summary>
    /// Updates the terminal size from PTY request or window change.
    /// </summary>
    /// <param name="width">Terminal width in columns.</param>
    /// <param name="height">Terminal height in rows.</param>
    internal void SetTerminalSize(int width, int height)
    {
        // Clamp to valid range [1, 500] per spec edge cases
        width = Math.Clamp(width, 1, 500);
        height = Math.Clamp(height, 1, 500);

        using (_lock.EnterScope())
        {
            _width = width;
            _height = height;
        }
    }
}
