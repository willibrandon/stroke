using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace Stroke.Input.Posix;

/// <summary>
/// Non-blocking reader for POSIX stdin with EINTR retry support.
/// </summary>
/// <remarks>
/// <para>
/// This class provides low-level stdin reading with proper handling of:
/// <list type="bullet">
/// <item>Non-blocking I/O when configured</item>
/// <item>EINTR retry on interrupted system calls</item>
/// <item>Incremental UTF-8 decoding</item>
/// </list>
/// </para>
/// <para>
/// Thread safety: This class is not thread-safe. It should be used from a single
/// reader thread only. Use synchronization at a higher layer if needed.
/// </para>
/// </remarks>
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("freebsd")]
internal sealed unsafe partial class PosixStdinReader : IDisposable
{
    private const string LibC = "libc";

    // File descriptor flags
    private const int F_GETFL = 3;
    private const int F_SETFL = 4;
    private const int O_NONBLOCK = 0x0004; // macOS value, Linux is 0x800

    // Error codes
    private const int EAGAIN = 35;     // macOS
    private const int EWOULDBLOCK = 35; // Same as EAGAIN on macOS
    private const int EINTR = 4;

    [LibraryImport(LibC, EntryPoint = "read", SetLastError = true)]
    private static partial nint Read(int fd, byte* buf, nuint count);

    [LibraryImport(LibC, EntryPoint = "fcntl", SetLastError = true)]
    private static partial int Fcntl(int fd, int cmd, int arg);

    private readonly int _fd;
    private readonly Decoder _decoder;
    private readonly byte[] _readBuffer;
    private readonly char[] _charBuffer;
    private bool _nonBlocking;
    private bool _closed;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PosixStdinReader"/> class.
    /// </summary>
    /// <param name="fd">The file descriptor to read from (default: stdin = 0).</param>
    /// <param name="bufferSize">The read buffer size in bytes.</param>
    public PosixStdinReader(int fd = Termios.STDIN_FILENO, int bufferSize = 1024)
    {
        _fd = fd;
        _decoder = Encoding.UTF8.GetDecoder();
        _readBuffer = new byte[bufferSize];
        _charBuffer = new char[bufferSize]; // UTF-8 can produce at most same chars as bytes
    }

    /// <summary>
    /// Gets the file descriptor.
    /// </summary>
    public int FileDescriptor => _fd;

    /// <summary>
    /// Gets or sets whether the reader is in non-blocking mode.
    /// </summary>
    public bool NonBlocking
    {
        get => _nonBlocking;
        set
        {
            if (_nonBlocking == value)
                return;

            int flags = Fcntl(_fd, F_GETFL, 0);
            if (flags < 0)
                return;

            if (value)
                flags |= GetPlatformNonBlockFlag();
            else
                flags &= ~GetPlatformNonBlockFlag();

            if (Fcntl(_fd, F_SETFL, flags) == 0)
                _nonBlocking = value;
        }
    }

    /// <summary>
    /// Gets whether the stream has been closed.
    /// </summary>
    public bool Closed => _closed;

    /// <summary>
    /// Reads available data from stdin.
    /// </summary>
    /// <returns>
    /// A string containing decoded characters, or an empty string if no data is available
    /// (in non-blocking mode) or EOF is reached.
    /// </returns>
    public string Read()
    {
        ThrowIfDisposed();

        if (_closed)
            return string.Empty;

        nint bytesRead;
        fixed (byte* buf = _readBuffer)
        {
            while (true)
            {
                bytesRead = Read(_fd, buf, (nuint)_readBuffer.Length);

                if (bytesRead < 0)
                {
                    int errno = Marshal.GetLastPInvokeError();

                    // EINTR - retry the read
                    if (errno == EINTR)
                        continue;

                    // EAGAIN/EWOULDBLOCK - no data available (non-blocking)
                    if (errno == EAGAIN || errno == EWOULDBLOCK || errno == GetPlatformEagain())
                    {
                        return string.Empty;
                    }

                    // Other error - treat as closed
                    _closed = true;
                    return string.Empty;
                }

                break;
            }
        }

        // EOF
        if (bytesRead == 0)
        {
            _closed = true;
            return string.Empty;
        }

        // Decode UTF-8
        int charCount = _decoder.GetChars(_readBuffer, 0, (int)bytesRead, _charBuffer, 0);
        return new string(_charBuffer, 0, charCount);
    }

    /// <summary>
    /// Closes the reader.
    /// </summary>
    public void Close()
    {
        _closed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        // Restore blocking mode before disposing
        if (_nonBlocking)
            NonBlocking = false;

        _closed = true;
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <summary>
    /// Gets the platform-specific O_NONBLOCK flag value.
    /// </summary>
    private static int GetPlatformNonBlockFlag()
    {
        // Linux uses 0x800, macOS/FreeBSD use 0x0004
        if (OperatingSystem.IsLinux())
            return 0x800;
        return O_NONBLOCK;
    }

    /// <summary>
    /// Gets the platform-specific EAGAIN value.
    /// </summary>
    private static int GetPlatformEagain()
    {
        // Linux uses 11, macOS/FreeBSD use 35
        if (OperatingSystem.IsLinux())
            return 11;
        return EAGAIN;
    }
}
