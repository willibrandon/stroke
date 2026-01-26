namespace Stroke.Input;

/// <summary>
/// No-op input implementation that immediately signals EOF.
/// </summary>
/// <remarks>
/// <para>
/// This class is used for non-terminal scenarios, unit tests that don't need input,
/// or as a placeholder when no real terminal is available. This is a faithful port
/// of Python Prompt Toolkit's <c>prompt_toolkit.input.DummyInput</c>.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. All operations are stateless or read-only.
/// </para>
/// </remarks>
public sealed class DummyInput : IInput
{
    private static int s_nextId;
    private readonly int _id;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DummyInput"/> class.
    /// </summary>
    public DummyInput()
    {
        _id = Interlocked.Increment(ref s_nextId);
    }

    /// <inheritdoc/>
    /// <remarks>Always returns true for DummyInput.</remarks>
    public bool Closed => true;

    /// <inheritdoc/>
    /// <remarks>Always returns an empty list for DummyInput.</remarks>
    public IReadOnlyList<KeyPress> ReadKeys()
    {
        ThrowIfDisposed();
        return [];
    }

    /// <inheritdoc/>
    /// <remarks>Always returns an empty list for DummyInput.</remarks>
    public IReadOnlyList<KeyPress> FlushKeys()
    {
        ThrowIfDisposed();
        return [];
    }

    /// <inheritdoc/>
    /// <remarks>Returns a no-op disposable for DummyInput.</remarks>
    public IDisposable RawMode() => NoOpDisposable.Instance;

    /// <inheritdoc/>
    /// <remarks>Returns a no-op disposable for DummyInput.</remarks>
    public IDisposable CookedMode() => NoOpDisposable.Instance;

    /// <inheritdoc/>
    /// <remarks>Returns a no-op disposable for DummyInput. The callback is never invoked.</remarks>
    public IDisposable Attach(Action inputReadyCallback)
    {
        ArgumentNullException.ThrowIfNull(inputReadyCallback);
        ThrowIfDisposed();
        return NoOpDisposable.Instance;
    }

    /// <inheritdoc/>
    /// <remarks>Returns a no-op disposable for DummyInput.</remarks>
    public IDisposable Detach() => NoOpDisposable.Instance;

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown; DummyInput has no file descriptor.</exception>
    public nint FileNo()
    {
        throw new NotSupportedException("DummyInput has no file descriptor.");
    }

    /// <inheritdoc/>
    public string TypeaheadHash() => $"DummyInput-{_id}";

    /// <inheritdoc/>
    public void Close()
    {
        // No-op; already always closed
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <summary>
    /// Singleton no-op disposable for mode contexts and attachments.
    /// </summary>
    private sealed class NoOpDisposable : IDisposable
    {
        public static readonly NoOpDisposable Instance = new();
        private NoOpDisposable() { }
        public void Dispose() { }
    }
}
