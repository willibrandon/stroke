using System.Runtime.Versioning;
using Stroke.Input.Vt100;

namespace Stroke.Input.Windows;

/// <summary>
/// Windows console input implementation.
/// </summary>
/// <remarks>
/// <para>
/// This class provides keyboard and mouse input from Windows Console.
/// On Windows 10 version 1511 and later, it uses VT100 mode for escape sequence
/// processing. On earlier versions, it uses legacy Console API input.
/// </para>
/// <para>
/// Thread safety: <see cref="ReadKeys"/> and <see cref="FlushKeys"/> should be called
/// from a single reader thread. The event loop callback mechanism handles synchronization.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class Win32Input : IInput
{
    private static int s_nextId;

    private readonly int _id;
    private readonly nint _handle;
    private readonly bool _useVt100Mode;
    private readonly Vt100Parser? _parser;
    private readonly List<KeyPress> _keyBuffer = new();
    private readonly Lock _callbackLock = new();
    private readonly Stack<Action> _callbackStack = new();
    private readonly ManualResetEventSlim _inputProcessed = new(true); // Starts signaled
    private Thread? _inputMonitorThread;
    private volatile bool _monitorRunning;
    private bool _closed;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Win32Input"/> class.
    /// </summary>
    /// <param name="alwaysPreferTty">If true, prefer TTY input even when stdin is redirected.</param>
    public Win32Input(bool alwaysPreferTty = false)
    {
        _id = Interlocked.Increment(ref s_nextId);
        _handle = ConsoleApi.GetStdHandle(ConsoleApi.STD_INPUT_HANDLE);

        if (_handle == ConsoleApi.INVALID_HANDLE_VALUE)
        {
            _closed = true;
            return;
        }

        // Check if VT100 mode is available (Windows 10 1511+)
        _useVt100Mode = TryEnableVt100Mode();

        if (_useVt100Mode)
        {
            _parser = new Vt100Parser(key => _keyBuffer.Add(key));
        }
    }

    /// <inheritdoc/>
    public bool Closed => _closed;

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

        _keyBuffer.Clear();

        if (_useVt100Mode && _parser != null)
        {
            // VT100 mode: read escape sequences
            var data = ReadVt100Data();
            if (!string.IsNullOrEmpty(data))
            {
                _parser.Feed(data);
            }
        }
        else
        {
            // Legacy mode: read console input events
            ReadLegacyInput();
        }

        // Signal that input has been processed (consumed from input buffer).
        // This allows the monitor thread to wait again without busy-looping.
        _inputProcessed.Set();

        var result = _keyBuffer.ToList();
        _keyBuffer.Clear();

        return result;
    }

    /// <inheritdoc/>
    public IReadOnlyList<KeyPress> FlushKeys()
    {
        ThrowIfDisposed();

        if (_parser == null)
            return [];

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
        return new Win32RawMode();
    }

    /// <inheritdoc/>
    public IDisposable CookedMode()
    {
        ThrowIfDisposed();
        return new Win32CookedMode();
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

            // Start the input monitor thread if no active monitor exists.
            // Also restart if _monitorRunning is false — this handles the race where
            // StopInputMonitor() set the flag but the thread hasn't exited yet,
            // which would cause the new callback to never be invoked.
            if (_inputMonitorThread is null || !_inputMonitorThread.IsAlive || !_monitorRunning)
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

    /// <inheritdoc/>
    public IDisposable Detach()
    {
        Action? currentCallback;

        using (_callbackLock.EnterScope())
        {
            if (_callbackStack.Count == 0)
                return NoOpDisposable.Instance;

            currentCallback = _callbackStack.Pop();
        }

        return new ReattachDisposable(this, currentCallback);
    }

    /// <inheritdoc/>
    public nint FileNo()
    {
        ThrowIfDisposed();
        return _handle;
    }

    /// <inheritdoc/>
    public string TypeaheadHash() => $"Win32Input-{_id}-{_handle}";

    /// <summary>
    /// Waits for input to become available with an optional timeout.
    /// </summary>
    /// <param name="timeoutMs">
    /// Timeout in milliseconds. Use <see cref="ConsoleApi.INFINITE"/> for no timeout.
    /// </param>
    /// <returns>
    /// <c>true</c> if input is available; <c>false</c> if the wait timed out or the input is closed.
    /// </returns>
    /// <remarks>
    /// This method is used for event loop integration. It allows the event loop to wait
    /// efficiently on the console input handle using <c>WaitForSingleObject</c>.
    /// </remarks>
    public bool WaitForInput(uint timeoutMs = ConsoleApi.INFINITE)
    {
        ThrowIfDisposed();

        if (_closed)
            return false;

        var result = ConsoleApi.WaitForSingleObject(_handle, timeoutMs);
        return result == ConsoleApi.WAIT_OBJECT_0;
    }

    /// <inheritdoc/>
    public void Close()
    {
        _closed = true;
        StopInputMonitor();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        Close();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <summary>
    /// Attempts to enable VT100 input mode on Windows Console.
    /// </summary>
    /// <returns>True if VT100 mode was enabled; false otherwise.</returns>
    private bool TryEnableVt100Mode()
    {
        if (!ConsoleApi.GetConsoleMode(_handle, out uint mode))
            return false;

        // Try to enable virtual terminal input
        uint newMode = mode | ConsoleApi.ENABLE_VIRTUAL_TERMINAL_INPUT;

        return ConsoleApi.SetConsoleMode(_handle, newMode);
    }

    /// <summary>
    /// Starts the background thread that monitors the console input handle for available input.
    /// </summary>
    private void StartInputMonitor()
    {
        // Wait for any previous monitor thread to finish before starting a new one.
        // This handles the race where StopInputMonitor() set _monitorRunning = false
        // but the thread hasn't exited yet due to WaitForSingleObject/Wait timeouts.
        if (_inputMonitorThread is not null && _inputMonitorThread.IsAlive)
        {
            _inputMonitorThread.Join(500);
        }

        _monitorRunning = true;
        _inputMonitorThread = new Thread(InputMonitorLoop)
        {
            Name = "Win32Input-Monitor",
            IsBackground = true
        };
        _inputMonitorThread.Start();
    }

    /// <summary>
    /// Stops the input monitor thread.
    /// </summary>
    private void StopInputMonitor()
    {
        _monitorRunning = false;
        // The thread will exit on the next WaitForSingleObject timeout
    }

    /// <summary>
    /// The input monitor loop that waits on the console input handle and invokes
    /// callbacks when input is available.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the Windows equivalent of <see cref="Vt100.Vt100Input"/>'s
    /// <c>InputMonitorLoop</c> which uses POSIX <c>poll()</c>.
    /// On Windows, we use <c>WaitForSingleObject</c> on the console input handle
    /// with a 100ms timeout to allow periodic checks of the <c>_monitorRunning</c> flag.
    /// </para>
    /// <para>
    /// After invoking the callback, the loop waits for <see cref="ReadKeys"/> to signal
    /// <see cref="_inputProcessed"/> before polling again, preventing busy-looping
    /// when the input handle remains signaled (e.g., due to mouse or window events
    /// that <see cref="Console.KeyAvailable"/> filters out).
    /// </para>
    /// </remarks>
    private void InputMonitorLoop()
    {
        while (_monitorRunning && !_closed && !_disposed)
        {
            // Wait on the console input handle with 100ms timeout
            var waitResult = ConsoleApi.WaitForSingleObject(_handle, 100);

            if (waitResult == ConsoleApi.WAIT_OBJECT_0)
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
                    _inputProcessed.Wait(200);
                }
            }
            // WAIT_TIMEOUT: loop back and check _monitorRunning
            // WAIT_FAILED: unexpected, but loop back and retry
        }
    }

    /// <summary>
    /// Reads VT100 escape sequence data from the console.
    /// </summary>
    private string ReadVt100Data()
    {
        // Use Console.In for VT100 mode input
        // This is a simplified implementation; production would use ReadConsole with VT100 processing
        if (!Console.KeyAvailable)
            return string.Empty;

        var chars = new char[256];
        int count = 0;

        while (Console.KeyAvailable && count < chars.Length)
        {
            var key = Console.ReadKey(intercept: true);
            chars[count++] = key.KeyChar;
        }

        return new string(chars, 0, count);
    }

    /// <summary>
    /// Reads legacy console input events.
    /// </summary>
    private void ReadLegacyInput()
    {
        // Simplified legacy input: use Console.ReadKey
        if (!Console.KeyAvailable)
            return;

        var keyInfo = Console.ReadKey(intercept: true);
        var key = MapConsoleKeyToKeys(keyInfo);
        var data = keyInfo.KeyChar != '\0' ? keyInfo.KeyChar.ToString() : null;

        _keyBuffer.Add(new KeyPress(key, data));
    }

    /// <summary>
    /// Maps a <see cref="ConsoleKeyInfo"/> to a <see cref="Keys"/> value.
    /// </summary>
    /// <remarks>
    /// Following Python Prompt Toolkit conventions:
    /// - Backspace = ControlH
    /// - Tab = ControlI
    /// - Enter = ControlM
    /// </remarks>
    private static Keys MapConsoleKeyToKeys(ConsoleKeyInfo keyInfo)
    {
        // Map common keys
        // Note: Following Python PTK, Backspace=ControlH, Tab=ControlI, Enter=ControlM
        return keyInfo.Key switch
        {
            ConsoleKey.UpArrow => Keys.Up,
            ConsoleKey.DownArrow => Keys.Down,
            ConsoleKey.LeftArrow => Keys.Left,
            ConsoleKey.RightArrow => Keys.Right,
            ConsoleKey.Home => Keys.Home,
            ConsoleKey.End => Keys.End,
            ConsoleKey.PageUp => Keys.PageUp,
            ConsoleKey.PageDown => Keys.PageDown,
            ConsoleKey.Insert => Keys.Insert,
            ConsoleKey.Delete => Keys.Delete,
            ConsoleKey.Backspace => Keys.ControlH,  // Backspace = ControlH in PTK
            ConsoleKey.Tab => Keys.ControlI,        // Tab = ControlI in PTK
            ConsoleKey.Enter => Keys.ControlM,      // Enter = ControlM in PTK
            ConsoleKey.Escape => Keys.Escape,
            ConsoleKey.F1 => Keys.F1,
            ConsoleKey.F2 => Keys.F2,
            ConsoleKey.F3 => Keys.F3,
            ConsoleKey.F4 => Keys.F4,
            ConsoleKey.F5 => Keys.F5,
            ConsoleKey.F6 => Keys.F6,
            ConsoleKey.F7 => Keys.F7,
            ConsoleKey.F8 => Keys.F8,
            ConsoleKey.F9 => Keys.F9,
            ConsoleKey.F10 => Keys.F10,
            ConsoleKey.F11 => Keys.F11,
            ConsoleKey.F12 => Keys.F12,
            ConsoleKey.Spacebar when keyInfo.KeyChar == ' ' => Keys.Any, // Space has character data
            _ when keyInfo.KeyChar != '\0' => Keys.Any,
            _ => Keys.Any
        };
    }

    private void RemoveCallback(Action callback)
    {
        bool stopMonitor = false;

        using (_callbackLock.EnterScope())
        {
            var temp = _callbackStack.ToList();
            _callbackStack.Clear();
            foreach (var cb in temp.Where(c => c != callback).Reverse())
            {
                _callbackStack.Push(cb);
            }

            if (_callbackStack.Count == 0)
            {
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
        using (_callbackLock.EnterScope())
        {
            _callbackStack.Push(callback);
        }
    }

    /// <summary>
    /// Disposable that removes a callback when disposed.
    /// </summary>
    private sealed class AttachDisposable(Win32Input input, Action callback) : IDisposable
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
    private sealed class ReattachDisposable(Win32Input input, Action callback) : IDisposable
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

    /// <summary>
    /// Waits for multiple handles until one becomes signaled or the timeout expires.
    /// </summary>
    /// <param name="handles">Array of handles to wait on.</param>
    /// <param name="timeoutMs">
    /// Timeout in milliseconds. Use <see cref="ConsoleApi.INFINITE"/> for no timeout.
    /// </param>
    /// <returns>
    /// The index of the signaled handle, or <c>-1</c> if the wait timed out or failed.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a port of Python Prompt Toolkit's <c>wait_for_handles</c> function.
    /// It allows the event loop to wait efficiently on multiple handles simultaneously.
    /// </para>
    /// <para>
    /// Thread safety: This method is thread-safe and can be called from any thread.
    /// </para>
    /// </remarks>
    public static int WaitForHandles(nint[] handles, uint timeoutMs = ConsoleApi.INFINITE)
    {
        ArgumentNullException.ThrowIfNull(handles);

        if (handles.Length == 0)
            return -1;

        var result = ConsoleApi.WaitForMultipleObjects(
            (uint)handles.Length,
            handles,
            false,  // Wait for any
            timeoutMs);

        if (result >= ConsoleApi.WAIT_OBJECT_0 && result < ConsoleApi.WAIT_OBJECT_0 + handles.Length)
        {
            return (int)(result - ConsoleApi.WAIT_OBJECT_0);
        }

        // WAIT_TIMEOUT or WAIT_FAILED
        return -1;
    }
}
