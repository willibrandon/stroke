using System.Runtime.Versioning;
using Stroke.Input.Posix;

namespace Stroke.Input.Vt100;

/// <summary>
/// POSIX VT100 terminal input implementation.
/// </summary>
/// <remarks>
/// <para>
/// This class provides keyboard and mouse input from POSIX terminals (Linux, macOS, FreeBSD)
/// using VT100/ANSI escape sequence parsing. It integrates with:
/// <list type="bullet">
/// <item><see cref="PosixStdinReader"/> for low-level stdin reading with EINTR handling</item>
/// <item><see cref="Vt100Parser"/> for escape sequence parsing</item>
/// <item><see cref="RawModeContext"/> for terminal mode control</item>
/// </list>
/// </para>
/// <para>
/// Thread safety: <see cref="ReadKeys"/> and <see cref="FlushKeys"/> should be called
/// from a single reader thread. The event loop callback mechanism handles synchronization.
/// </para>
/// </remarks>
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("freebsd")]
public sealed class Vt100Input : IInput
{
    private static int s_nextId;

    private readonly int _id;
    private readonly int _fd;
    private readonly PosixStdinReader _stdinReader;
    private readonly Vt100Parser _parser;
    private readonly List<KeyPress> _keyBuffer = new();
    private readonly Lock _callbackLock = new();
    private readonly Stack<Action> _callbackStack = new();
    private bool _closed;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Vt100Input"/> class.
    /// </summary>
    /// <param name="fd">The file descriptor for input (default: stdin = 0).</param>
    public Vt100Input(int fd = Termios.STDIN_FILENO)
    {
        _id = Interlocked.Increment(ref s_nextId);
        _fd = fd;
        _stdinReader = new PosixStdinReader(fd);
        _parser = new Vt100Parser(key => _keyBuffer.Add(key));
    }

    /// <inheritdoc/>
    public bool Closed => _closed || _stdinReader.Closed;

    /// <inheritdoc/>
    public IReadOnlyList<KeyPress> ReadKeys()
    {
        ThrowIfDisposed();

        if (_closed)
            return [];

        // Read available data from stdin
        var data = _stdinReader.Read();

        if (string.IsNullOrEmpty(data))
        {
            if (_stdinReader.Closed)
                _closed = true;
            return [];
        }

        // Parse through VT100 parser
        _keyBuffer.Clear();
        _parser.Feed(data);

        var result = _keyBuffer.ToList();
        _keyBuffer.Clear();

        return result;
    }

    /// <inheritdoc/>
    public IReadOnlyList<KeyPress> FlushKeys()
    {
        ThrowIfDisposed();

        _keyBuffer.Clear();
        _parser.Flush();

        var result = _keyBuffer.ToList();
        _keyBuffer.Clear();

        return result;
    }

    /// <inheritdoc/>
    public IDisposable RawMode()
    {
        ThrowIfDisposed();
        return new RawModeContext(_fd);
    }

    /// <inheritdoc/>
    public IDisposable CookedMode()
    {
        ThrowIfDisposed();
        // CookedModeContext will be implemented in Phase 6
        return new CookedModeContext(_fd);
    }

    /// <inheritdoc/>
    public IDisposable Attach(Action inputReadyCallback)
    {
        ArgumentNullException.ThrowIfNull(inputReadyCallback);
        ThrowIfDisposed();

        using (_callbackLock.EnterScope())
        {
            _callbackStack.Push(inputReadyCallback);

            // Enable non-blocking mode when attached
            _stdinReader.NonBlocking = true;
        }

        return new AttachDisposable(this, inputReadyCallback);
    }

    /// <inheritdoc/>
    public IDisposable Detach()
    {
        Action? currentCallback;

        using (_callbackLock.EnterScope())
        {
            if (_callbackStack.Count == 0)
                return NoOpDisposable.Instance;

            currentCallback = _callbackStack.Pop();

            // Disable non-blocking mode if no more callbacks
            if (_callbackStack.Count == 0)
                _stdinReader.NonBlocking = false;
        }

        return new ReattachDisposable(this, currentCallback);
    }

    /// <inheritdoc/>
    public nint FileNo()
    {
        ThrowIfDisposed();
        return _fd;
    }

    /// <inheritdoc/>
    public string TypeaheadHash() => $"Vt100Input-{_id}-{_fd}";

    /// <inheritdoc/>
    public void Close()
    {
        _closed = true;
        _stdinReader.Close();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        Close();
        _stdinReader.Dispose();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private void RemoveCallback(Action callback)
    {
        using (_callbackLock.EnterScope())
        {
            // Rebuild stack without the specified callback
            var temp = _callbackStack.ToList();
            _callbackStack.Clear();
            foreach (var cb in temp.Where(c => c != callback).Reverse())
            {
                _callbackStack.Push(cb);
            }

            // Disable non-blocking mode if no more callbacks
            if (_callbackStack.Count == 0)
                _stdinReader.NonBlocking = false;
        }
    }

    private void ReattachCallback(Action callback)
    {
        using (_callbackLock.EnterScope())
        {
            _callbackStack.Push(callback);
            _stdinReader.NonBlocking = true;
        }
    }

    /// <summary>
    /// Disposable that removes a callback when disposed.
    /// </summary>
    private sealed class AttachDisposable(Vt100Input input, Action callback) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            input.RemoveCallback(callback);
        }
    }

    /// <summary>
    /// Disposable that reattaches a callback when disposed.
    /// </summary>
    private sealed class ReattachDisposable(Vt100Input input, Action callback) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            input.ReattachCallback(callback);
        }
    }

    /// <summary>
    /// Singleton no-op disposable.
    /// </summary>
    private sealed class NoOpDisposable : IDisposable
    {
        public static readonly NoOpDisposable Instance = new();
        private NoOpDisposable() { }
        public void Dispose() { }
    }
}
