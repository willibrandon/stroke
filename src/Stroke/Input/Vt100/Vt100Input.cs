using System.Runtime.InteropServices;
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
public sealed partial class Vt100Input : IInput
{
    private static int s_nextId;

    private readonly int _id;
    private readonly int _fd;
    private readonly PosixStdinReader _stdinReader;
    private readonly Vt100Parser _parser;
    private readonly List<KeyPress> _keyBuffer = new();
    private readonly Lock _callbackLock = new();
    private readonly Stack<Action> _callbackStack = new();
    private readonly ManualResetEventSlim _inputProcessed = new(true); // Starts signaled
    private Thread? _inputMonitorThread;
    private volatile bool _monitorRunning;
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
        {
            // Signal that input processing is complete even if we're closed
            _inputProcessed.Set();
            return [];
        }

        // Read available data from stdin
        var data = _stdinReader.Read();

        // Signal that input has been processed (consumed from stdin buffer).
        // This allows the monitor thread to poll again.
        _inputProcessed.Set();

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

        bool startMonitor = false;

        using (_callbackLock.EnterScope())
        {
            _callbackStack.Push(inputReadyCallback);

            // Enable non-blocking mode when attached
            _stdinReader.NonBlocking = true;

            // Start the input monitor thread if this is the first callback
            if (_inputMonitorThread is null || !_inputMonitorThread.IsAlive)
            {
                startMonitor = true;
            }
        }

        if (startMonitor)
        {
            StartInputMonitor();
        }

        return new AttachDisposable(this, inputReadyCallback);
    }

    /// <summary>
    /// Starts the background thread that monitors stdin for input availability.
    /// </summary>
    private void StartInputMonitor()
    {
        _monitorRunning = true;
        _inputMonitorThread = new Thread(InputMonitorLoop)
        {
            Name = "Vt100Input-Monitor",
            IsBackground = true
        };
        _inputMonitorThread.Start();
    }

    /// <summary>
    /// Stops the input monitor thread and waits for it to exit.
    /// </summary>
    private void StopInputMonitor()
    {
        _monitorRunning = false;

        // Wait for the thread to exit (up to 500ms) to prevent race conditions
        // where a new Attach() call checks IsAlive before the thread terminates.
        var thread = _inputMonitorThread;
        if (thread is not null && thread.IsAlive)
        {
            thread.Join(500);
        }
        _inputMonitorThread = null;
    }

    /// <summary>
    /// The input monitor loop that polls stdin and invokes callbacks when input is available.
    /// </summary>
    private void InputMonitorLoop()
    {
        Span<PollFd> pollFds = stackalloc PollFd[1];

        while (_monitorRunning && !_closed && !_disposed)
        {
            pollFds[0] = new PollFd { Fd = _fd, Events = POLLIN, REvents = 0 };

            // Poll with 100ms timeout to allow checking _monitorRunning flag
            var pollResult = Poll(pollFds, 1, 100);

            if (pollResult > 0 && (pollFds[0].REvents & POLLIN) != 0)
            {
                // Input is available, invoke the top callback
                Action? callback = null;
                using (_callbackLock.EnterScope())
                {
                    if (_callbackStack.Count > 0)
                    {
                        callback = _callbackStack.Peek();
                    }
                }

                if (callback != null)
                {
                    // Reset the event before invoking callback.
                    // This ensures we wait until ReadKeys() consumes the input
                    // before polling again, preventing a busy loop.
                    _inputProcessed.Reset();

                    callback.Invoke();

                    // Wait for ReadKeys() to consume the input (with timeout to allow shutdown).
                    // This prevents busy-looping when poll() keeps returning immediately.
                    _inputProcessed.Wait(200);
                }
            }
            else if (pollResult < 0)
            {
                // Error occurred (ignore EINTR)
                var errno = Marshal.GetLastPInvokeError();
                if (errno != EINTR)
                {
                    break;
                }
            }
        }
    }

    // Poll constants
    private const short POLLIN = 0x0001;
    private const int EINTR = 4;

    [StructLayout(LayoutKind.Sequential)]
    private struct PollFd
    {
        public int Fd;
        public short Events;
        public short REvents;
    }

    [LibraryImport("libc", EntryPoint = "poll", SetLastError = true)]
    private static partial int Poll(Span<PollFd> fds, int nfds, int timeout);

    /// <inheritdoc/>
    public IDisposable Detach()
    {
        Action? currentCallback;
        bool stopMonitor = false;

        using (_callbackLock.EnterScope())
        {
            if (_callbackStack.Count == 0)
                return NoOpDisposable.Instance;

            currentCallback = _callbackStack.Pop();

            // Disable non-blocking mode and stop monitor if no more callbacks
            if (_callbackStack.Count == 0)
            {
                _stdinReader.NonBlocking = false;
                stopMonitor = true;
            }
        }

        if (stopMonitor)
        {
            StopInputMonitor();
        }

        return new ReattachDisposable(this, currentCallback);
    }

    /// <summary>
    /// Reads a line from the terminal file descriptor using direct POSIX read(),
    /// bypassing .NET's Console class which manages its own terminal state.
    /// Used by RunSystemCommandAsync to wait for Enter after running
    /// a system command.
    /// </summary>
    public unsafe void ReadLineFromFd()
    {
        // Read one byte at a time until we get a newline.
        // We're in cooked mode here, so the kernel line-buffers and we'll
        // get the full line when Enter is pressed. But we read byte-by-byte
        // to be safe with partial reads.
        var buf = new byte[1];

        fixed (byte* ptr = buf)
        {
            while (true)
            {
                nint bytesRead = PosixStdinReader.PosixRead(_fd, ptr, 1);
                int errno = Marshal.GetLastPInvokeError();

                if (bytesRead <= 0)
                {
                    if (bytesRead < 0)
                    {
                        if (errno == EINTR)
                            continue; // Retry on EINTR
                    }
                    break; // EOF or error
                }

                if (buf[0] == (byte)'\n' || buf[0] == (byte)'\r')
                    break;
            }
        }
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
        StopInputMonitor();
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
        bool stopMonitor = false;

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
            {
                _stdinReader.NonBlocking = false;
                stopMonitor = true;
            }
        }

        if (stopMonitor)
        {
            StopInputMonitor();
        }
    }

    private void ReattachCallback(Action callback)
    {
        bool startMonitor = false;

        using (_callbackLock.EnterScope())
        {
            _callbackStack.Push(callback);
            _stdinReader.NonBlocking = true;

            // Restart the monitor thread if it was stopped during Detach
            if (_inputMonitorThread is null || !_inputMonitorThread.IsAlive)
            {
                startMonitor = true;
            }
        }

        if (startMonitor)
        {
            StartInputMonitor();
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
