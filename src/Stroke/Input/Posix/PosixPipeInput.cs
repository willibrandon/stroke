// Copyright (c) 2025 Brandon Pugh. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Stroke.Input.Pipe;
using Stroke.Input.Vt100;

namespace Stroke.Input.Posix;

/// <summary>
/// POSIX-specific pipe input implementation using OS pipes.
/// </summary>
/// <remarks>
/// <para>
/// This implementation uses real OS pipes (via pipe() system call) for
/// sending input data. The read-end of the pipe is consumed by a
/// VT100 parser for escape sequence handling.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's
/// <c>prompt_toolkit.input.posix_pipe.PosixPipeInput</c>.
/// </para>
/// <para>
/// Thread safety: <see cref="SendBytes"/> and <see cref="SendText"/> are
/// thread-safe. <see cref="ReadKeys"/> and <see cref="FlushKeys"/> assume
/// single-threaded access.
/// </para>
/// </remarks>
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("freebsd")]
public sealed partial class PosixPipeInput : IPipeInput
{
    private static int _nextId;

    private readonly int _id;
    private readonly int _readFd;
    private readonly int _writeFd;
    private readonly Vt100Parser _parser;
    private readonly List<KeyPress> _buffer = new();
    private readonly Lock _writeLock = new();
    private readonly Stack<Action> _callbackStack = new();

    private bool _readClosed;
    private bool _writeClosed;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PosixPipeInput"/> class.
    /// </summary>
    /// <param name="initialText">Optional initial text to send to the pipe.</param>
    public PosixPipeInput(string? initialText = null)
    {
        _id = Interlocked.Increment(ref _nextId);

        // Create the pipe
        Span<int> fds = stackalloc int[2];
        var result = Pipe(fds);
        if (result != 0)
        {
            throw new InvalidOperationException($"Failed to create pipe: {Marshal.GetLastWin32Error()}");
        }

        _readFd = fds[0];
        _writeFd = fds[1];

        _parser = new Vt100Parser(kp => _buffer.Add(kp));

        if (!string.IsNullOrEmpty(initialText))
        {
            SendText(initialText);
        }
    }

    /// <inheritdoc/>
    public bool Closed => _writeClosed;

    /// <inheritdoc/>
    public IReadOnlyList<KeyPress> ReadKeys()
    {
        ThrowIfDisposed();

        // Check if data is available using poll() to avoid blocking
        Span<PollFd> pollFds = stackalloc PollFd[1];
        pollFds[0] = new PollFd { Fd = _readFd, Events = POLLIN, REvents = 0 };

        var pollResult = Poll(pollFds, 1, 0); // 0 timeout = non-blocking check
        if (pollResult <= 0 || (pollFds[0].REvents & POLLIN) == 0)
        {
            // No data available, return empty
            return _buffer.ToList().Count > 0 ? [.. _buffer] : [];
        }

        // Read available data from the pipe
        var buffer = new byte[4096];
        int bytesRead;

        try
        {
            bytesRead = Read(_readFd, buffer, buffer.Length);
            if (bytesRead < 0)
            {
                bytesRead = 0;
            }
        }
        catch
        {
            bytesRead = 0;
        }

        if (bytesRead > 0)
        {
            var text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            _parser.Feed(text);
        }

        var result = _buffer.ToList();
        _buffer.Clear();
        return result;
    }

    /// <inheritdoc/>
    public IReadOnlyList<KeyPress> FlushKeys()
    {
        ThrowIfDisposed();

        // Check if data is available using poll() to avoid blocking
        Span<PollFd> pollFds = stackalloc PollFd[1];
        pollFds[0] = new PollFd { Fd = _readFd, Events = POLLIN, REvents = 0 };

        var pollResult = Poll(pollFds, 1, 0); // 0 timeout = non-blocking check
        if (pollResult > 0 && (pollFds[0].REvents & POLLIN) != 0)
        {
            // Data available, read it
            var buffer = new byte[4096];
            int bytesRead;

            try
            {
                bytesRead = Read(_readFd, buffer, buffer.Length);
                if (bytesRead < 0)
                {
                    bytesRead = 0;
                }
            }
            catch
            {
                bytesRead = 0;
            }

            if (bytesRead > 0)
            {
                var text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                _parser.Feed(text);
            }
        }

        // Now flush any partial sequences from the parser
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

        using (_writeLock.EnterScope())
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    var written = Write(_writeFd, ptr, data.Length);
                    if (written < 0)
                    {
                        throw new InvalidOperationException($"Failed to write to pipe: {Marshal.GetLastWin32Error()}");
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public void SendText(string data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ThrowIfDisposed();
        ThrowIfClosed();

        var bytes = Encoding.UTF8.GetBytes(data);
        SendBytes(bytes);
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
        return _readFd;
    }

    /// <inheritdoc/>
    public string TypeaheadHash() => $"posix-pipe-input-{_id}";

    /// <inheritdoc/>
    public void Close()
    {
        // Only close the write-end of the pipe. This will unblock any reader
        // with EOF, while keeping the read-end open for event loop registration.
        if (!_writeClosed)
        {
            CloseWriteEnd();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        CloseWriteEnd();
        CloseReadEnd();
    }

    private void CloseReadEnd()
    {
        if (_readClosed) return;

        try
        {
            CloseFd(_readFd);
        }
        catch
        {
            // Ignore close errors
        }

        _readClosed = true;
    }

    private void CloseWriteEnd()
    {
        if (_writeClosed) return;

        try
        {
            CloseFd(_writeFd);
        }
        catch
        {
            // Ignore close errors
        }

        _writeClosed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private void ThrowIfClosed()
    {
        if (_writeClosed)
        {
            throw new ObjectDisposedException(GetType().Name, "Pipe is closed.");
        }
    }

    #region P/Invoke

    private const short POLLIN = 0x0001;

    [StructLayout(LayoutKind.Sequential)]
    private struct PollFd
    {
        public int Fd;
        public short Events;
        public short REvents;
    }

    [LibraryImport("libc", EntryPoint = "pipe", SetLastError = true)]
    private static partial int Pipe(Span<int> pipefd);

    [LibraryImport("libc", EntryPoint = "read", SetLastError = true)]
    private static partial int Read(int fd, byte[] buf, int count);

    [LibraryImport("libc", EntryPoint = "write", SetLastError = true)]
    private static unsafe partial int Write(int fd, byte* buf, int count);

    [LibraryImport("libc", EntryPoint = "close", SetLastError = true)]
    private static partial int CloseFd(int fd);

    [LibraryImport("libc", EntryPoint = "poll", SetLastError = true)]
    private static partial int Poll(Span<PollFd> fds, int nfds, int timeout);

    #endregion

    #region Helper Classes

    private sealed class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }

    private sealed class DetachDisposable(PosixPipeInput input) : IDisposable
    {
        public void Dispose()
        {
            if (input._callbackStack.Count > 0)
            {
                input._callbackStack.Pop();
            }
        }
    }

    private sealed class ReattachDisposable(PosixPipeInput input, Action callback) : IDisposable
    {
        public void Dispose()
        {
            input._callbackStack.Push(callback);
        }
    }

    #endregion
}
