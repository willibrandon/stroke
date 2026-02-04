using System.Text;
using Stroke.Input.Vt100;

namespace Stroke.Input.Pipe;

/// <summary>
/// Shared base logic for pipe input implementations.
/// </summary>
/// <remarks>
/// <para>
/// This class provides the common implementation for pipe-based input, including
/// VT100 parser integration, thread-safe send operations, and UTF-8 encoding.
/// </para>
/// <para>
/// Thread safety: <see cref="SendBytes"/> and <see cref="SendText"/> are thread-safe
/// and may be called from any thread. <see cref="ReadKeys"/> should be called from
/// a single reader thread.
/// </para>
/// </remarks>
public abstract class PipeInputBase : IPipeInput
{
    private static int s_nextId;

    private readonly int _id;
    private readonly Lock _lock = new();
    private readonly Queue<byte> _inputBuffer = new();
    private readonly List<KeyPress> _parsedKeys = new();
    private readonly Vt100Parser _parser;
    private Action? _inputReadyCallback;
    private bool _closed;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeInputBase"/> class.
    /// </summary>
    protected PipeInputBase()
    {
        _id = Interlocked.Increment(ref s_nextId);
        _parser = new Vt100Parser(key => _parsedKeys.Add(key));
    }

    /// <inheritdoc/>
    public bool Closed
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _closed;
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<KeyPress> ReadKeys()
    {
        ThrowIfDisposed();

        List<KeyPress> result;

        using (_lock.EnterScope())
        {
            if (_inputBuffer.Count == 0)
            {
                return [];
            }

            // Decode buffered bytes as UTF-8
            var bytes = new byte[_inputBuffer.Count];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = _inputBuffer.Dequeue();
            }

            var text = Encoding.UTF8.GetString(bytes);

            // Parse through VT100 parser
            _parsedKeys.Clear();
            _parser.Feed(text);

            result = [.. _parsedKeys];
            _parsedKeys.Clear();
        }

        return result;
    }

    /// <inheritdoc/>
    public IReadOnlyList<KeyPress> FlushKeys()
    {
        ThrowIfDisposed();

        List<KeyPress> result;

        using (_lock.EnterScope())
        {
            // First drain any buffered input and feed to parser
            if (_inputBuffer.Count > 0)
            {
                var bytes = new byte[_inputBuffer.Count];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = _inputBuffer.Dequeue();
                }

                var text = Encoding.UTF8.GetString(bytes);
                _parser.Feed(text);
            }

            // Now flush any partial sequences from the parser
            _parsedKeys.Clear();
            _parser.Flush();
            result = [.. _parsedKeys];
            _parsedKeys.Clear();
        }

        return result;
    }

    /// <inheritdoc/>
    public void SendBytes(ReadOnlySpan<byte> data)
    {
        ThrowIfDisposed();

        Action? callback;
        using (_lock.EnterScope())
        {
            if (_closed)
            {
                throw new ObjectDisposedException(GetType().Name, "Pipe has been closed.");
            }

            foreach (var b in data)
            {
                _inputBuffer.Enqueue(b);
            }

            callback = _inputReadyCallback;
        }

        // Notify outside the lock to avoid deadlocks
        callback?.Invoke();
    }

    /// <inheritdoc/>
    public void SendText(string data)
    {
        ArgumentNullException.ThrowIfNull(data);
        SendBytes(Encoding.UTF8.GetBytes(data));
    }

    /// <inheritdoc/>
    public virtual IDisposable RawMode() => NoOpDisposable.Instance;

    /// <inheritdoc/>
    public virtual IDisposable CookedMode() => NoOpDisposable.Instance;

    /// <inheritdoc/>
    public virtual IDisposable Attach(Action inputReadyCallback)
    {
        ArgumentNullException.ThrowIfNull(inputReadyCallback);
        ThrowIfDisposed();

        using (_lock.EnterScope())
        {
            _inputReadyCallback = inputReadyCallback;
        }

        return new DetachDisposable(this);
    }

    /// <inheritdoc/>
    public virtual IDisposable Detach()
    {
        using (_lock.EnterScope())
        {
            _inputReadyCallback = null;
        }
        return NoOpDisposable.Instance;
    }

    /// <summary>
    /// Disposable that detaches the input callback when disposed.
    /// </summary>
    private sealed class DetachDisposable(PipeInputBase owner) : IDisposable
    {
        public void Dispose() => owner.Detach();
    }

    /// <inheritdoc/>
    public virtual nint FileNo()
    {
        // Override in platform-specific implementations
        throw new NotSupportedException("PipeInputBase does not have a file descriptor.");
    }

    /// <inheritdoc/>
    public string TypeaheadHash() => $"{GetType().Name}-{_id}";

    /// <inheritdoc/>
    public virtual void Close()
    {
        using (_lock.EnterScope())
        {
            _closed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases resources used by this instance.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(); false if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Close();
        }

        _disposed = true;
    }

    /// <summary>
    /// Throws <see cref="ObjectDisposedException"/> if this instance has been disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
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

/// <summary>
/// Simple cross-platform pipe input implementation for testing.
/// </summary>
/// <remarks>
/// This class provides a fully in-memory pipe input that works on all platforms.
/// For platform-specific implementations with OS pipe support, use
/// <c>PosixPipeInput</c> or <c>Win32PipeInput</c>.
/// </remarks>
public sealed class SimplePipeInput : PipeInputBase
{
}
