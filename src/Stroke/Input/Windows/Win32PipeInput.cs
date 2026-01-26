// Copyright (c) 2025 Brandon Pugh. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Stroke.Input.Pipe;
using Stroke.Input.Vt100;

namespace Stroke.Input.Windows;

/// <summary>
/// Windows-specific pipe input implementation using Windows Events.
/// </summary>
/// <remarks>
/// <para>
/// This implementation uses Windows Event handles for signaling data availability
/// rather than real OS pipes. Data is fed directly into a VT100 parser and stored
/// in an internal buffer. The event is signaled when data is available.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's
/// <c>prompt_toolkit.input.win32_pipe.Win32PipeInput</c>.
/// </para>
/// <para>
/// Thread safety: <see cref="SendBytes"/> and <see cref="SendText"/> are
/// thread-safe. <see cref="ReadKeys"/> and <see cref="FlushKeys"/> assume
/// single-threaded access.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed partial class Win32PipeInput : IPipeInput
{
    private static int _nextId;

    private readonly int _id;
    private readonly nint _event;
    private readonly Vt100Parser _parser;
    private readonly List<KeyPress> _buffer = new();
    private readonly Lock _writeLock = new();
    private readonly Stack<Action> _callbackStack = new();

    private bool _closed;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Win32PipeInput"/> class.
    /// </summary>
    /// <param name="initialText">Optional initial text to send to the pipe.</param>
    public Win32PipeInput(string? initialText = null)
    {
        _id = Interlocked.Increment(ref _nextId);

        // Create a manual-reset event for signaling data availability
        _event = CreateEventW(nint.Zero, true, false, null);
        if (_event == nint.Zero)
        {
            throw new InvalidOperationException($"Failed to create event: {Marshal.GetLastWin32Error()}");
        }

        _parser = new Vt100Parser(kp => _buffer.Add(kp));

        if (!string.IsNullOrEmpty(initialText))
        {
            SendText(initialText);
        }
    }

    /// <inheritdoc/>
    public bool Closed => _closed;

    /// <summary>
    /// Gets the Windows event handle used for event loop registration.
    /// </summary>
    public nint Handle => _event;

    /// <inheritdoc/>
    public IReadOnlyList<KeyPress> ReadKeys()
    {
        ThrowIfDisposed();

        var result = _buffer.ToList();
        _buffer.Clear();

        // Reset event if not closed
        if (!_closed)
        {
            ResetEvent(_event);
        }

        return result;
    }

    /// <inheritdoc/>
    public IReadOnlyList<KeyPress> FlushKeys()
    {
        ThrowIfDisposed();

        _parser.Flush();
        var result = _buffer.ToList();
        _buffer.Clear();
        return result;
    }

    /// <inheritdoc/>
    public void SendBytes(ReadOnlySpan<byte> data)
    {
        ThrowIfDisposed();
        ThrowIfClosed();

        var text = Encoding.UTF8.GetString(data);
        SendTextInternal(text);
    }

    /// <inheritdoc/>
    public void SendText(string data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ThrowIfDisposed();
        ThrowIfClosed();

        SendTextInternal(data);
    }

    private void SendTextInternal(string text)
    {
        using (_writeLock.EnterScope())
        {
            // Feed text through the VT100 parser
            _parser.Feed(text);

            // Signal that data is available
            SetEvent(_event);
        }
    }

    /// <inheritdoc/>
    public IDisposable RawMode() => new NoOpDisposable();

    /// <inheritdoc/>
    public IDisposable CookedMode() => new NoOpDisposable();

    /// <inheritdoc/>
    public IDisposable Attach(Action inputReadyCallback)
    {
        ArgumentNullException.ThrowIfNull(inputReadyCallback);
        ThrowIfDisposed();

        _callbackStack.Push(inputReadyCallback);
        return new DetachDisposable(this);
    }

    /// <inheritdoc/>
    public IDisposable Detach()
    {
        if (_callbackStack.Count > 0)
        {
            var previous = _callbackStack.Pop();
            return new ReattachDisposable(this, previous);
        }

        return new NoOpDisposable();
    }

    /// <inheritdoc/>
    public nint FileNo()
    {
        ThrowIfDisposed();
        // Return the event handle (can be used with WaitForSingleObject)
        return _event;
    }

    /// <inheritdoc/>
    public string TypeaheadHash() => $"win32-pipe-input-{_id}";

    /// <inheritdoc/>
    public void Close()
    {
        if (_closed) return;

        _closed = true;
        // Signal the event to wake up any waiters
        SetEvent(_event);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _closed = true;

        if (_event != nint.Zero)
        {
            CloseHandle(_event);
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private void ThrowIfClosed()
    {
        if (_closed)
        {
            throw new ObjectDisposedException(GetType().Name, "Pipe is closed.");
        }
    }

    #region P/Invoke

    [LibraryImport("kernel32.dll", EntryPoint = "CreateEventW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint CreateEventW(nint lpEventAttributes, [MarshalAs(UnmanagedType.Bool)] bool bManualReset, [MarshalAs(UnmanagedType.Bool)] bool bInitialState, string? lpName);

    [LibraryImport("kernel32.dll", EntryPoint = "SetEvent", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetEvent(nint hEvent);

    [LibraryImport("kernel32.dll", EntryPoint = "ResetEvent", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ResetEvent(nint hEvent);

    [LibraryImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(nint hObject);

    #endregion

    #region Helper Classes

    private sealed class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }

    private sealed class DetachDisposable(Win32PipeInput input) : IDisposable
    {
        public void Dispose()
        {
            if (input._callbackStack.Count > 0)
            {
                input._callbackStack.Pop();
            }
        }
    }

    private sealed class ReattachDisposable(Win32PipeInput input, Action callback) : IDisposable
    {
        public void Dispose()
        {
            input._callbackStack.Push(callback);
        }
    }

    #endregion
}
