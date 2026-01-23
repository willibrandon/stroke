# Feature 101: Pipe Inputs

## Overview

Implement pipe input classes for programmatic input injection, primarily used for unit testing. Includes PosixPipeInput for Unix systems and Win32PipeInput for Windows.

## Python Prompt Toolkit Reference

**Source:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/input/posix_pipe.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/input/win32_pipe.py`

## Public API

### PipeInput Base

```csharp
namespace Stroke.Input;

/// <summary>
/// Abstract base for pipe inputs. Pipe inputs allow sending text or key
/// presses programmatically, useful for unit testing.
/// </summary>
public interface IPipeInput : IInput
{
    /// <summary>
    /// Send raw bytes to the input.
    /// </summary>
    /// <param name="data">Bytes to send.</param>
    void SendBytes(byte[] data);

    /// <summary>
    /// Send text to the input.
    /// </summary>
    /// <param name="text">Text to send.</param>
    void SendText(string text);

    /// <summary>
    /// Close the pipe (signals EOF to reader).
    /// </summary>
    void Close();
}
```

### PosixPipeInput

```csharp
namespace Stroke.Input;

/// <summary>
/// Input that is sent through a POSIX pipe.
/// Useful for sending input programmatically, primarily for unit testing.
/// </summary>
/// <remarks>
/// Uses VT100 escape sequences over the pipe for key input.
/// </remarks>
/// <example>
/// using var input = PosixPipeInput.Create();
/// input.SendText("hello\n");
/// // Read keys from input...
/// </example>
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class PosixPipeInput : Vt100Input, IPipeInput
{
    /// <summary>
    /// Create a new POSIX pipe input.
    /// </summary>
    /// <param name="initialText">Optional initial text to send.</param>
    /// <returns>A disposable pipe input.</returns>
    public static PosixPipeInput Create(string initialText = "");

    /// <summary>
    /// The underlying pipe file descriptors.
    /// </summary>
    internal (int ReadFd, int WriteFd) Pipe { get; }

    /// <inheritdoc/>
    public void SendBytes(byte[] data);

    /// <inheritdoc/>
    public void SendText(string text);

    /// <inheritdoc/>
    public void Close();

    /// <inheritdoc/>
    public override string TypeaheadHash();

    /// <inheritdoc/>
    public override IDisposable RawMode();

    /// <inheritdoc/>
    public override IDisposable CookedMode();
}
```

### Win32PipeInput

```csharp
namespace Stroke.Input;

/// <summary>
/// Input pipe that works on Windows.
/// Text or bytes can be fed into the pipe, and key strokes can be read.
/// Uses VT100 escape sequences over the pipe even though it's Windows.
/// </summary>
/// <example>
/// using var input = Win32PipeInput.Create();
/// input.SendText("hello\n");
/// // Read keys from input...
/// </example>
[SupportedOSPlatform("windows")]
public sealed class Win32PipeInput : IPipeInput
{
    /// <summary>
    /// Create a new Windows pipe input.
    /// </summary>
    /// <returns>A disposable pipe input.</returns>
    public static Win32PipeInput Create();

    /// <summary>
    /// Whether the pipe has been closed.
    /// </summary>
    public bool Closed { get; }

    /// <summary>
    /// The Windows event handle for event loop registration.
    /// </summary>
    public nint Handle { get; }

    /// <inheritdoc/>
    public IReadOnlyList<KeyPress> ReadKeys();

    /// <inheritdoc/>
    public IReadOnlyList<KeyPress> FlushKeys();

    /// <inheritdoc/>
    public void SendBytes(byte[] data);

    /// <inheritdoc/>
    public void SendText(string text);

    /// <inheritdoc/>
    public void Close();

    /// <inheritdoc/>
    public string TypeaheadHash();

    /// <inheritdoc/>
    public IDisposable Attach(Action inputReadyCallback);

    /// <inheritdoc/>
    public IDisposable Detach();

    /// <inheritdoc/>
    public IDisposable RawMode();

    /// <inheritdoc/>
    public IDisposable CookedMode();
}
```

## Project Structure

```
src/Stroke/
└── Input/
    ├── IPipeInput.cs
    ├── PosixPipeInput.cs  (Unix-only)
    └── Win32PipeInput.cs  (Windows-only)
tests/Stroke.Tests/
└── Input/
    ├── PosixPipeInputTests.cs
    └── Win32PipeInputTests.cs
```

## Implementation Notes

### PosixPipeInput Implementation

```csharp
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class PosixPipeInput : Vt100Input, IPipeInput, IDisposable
{
    private static int _idCounter;
    private readonly int _id;
    private readonly int _readFd;
    private readonly int _writeFd;
    private bool _readClosed;
    private bool _writeClosed;

    private PosixPipeInput(int readFd, int writeFd, string initialText)
        : base(CreatePseudoStdin(readFd))
    {
        _readFd = readFd;
        _writeFd = writeFd;
        _id = Interlocked.Increment(ref _idCounter);

        if (!string.IsNullOrEmpty(initialText))
            SendText(initialText);
    }

    public static PosixPipeInput Create(string initialText = "")
    {
        var (readFd, writeFd) = PosixInterop.Pipe();
        return new PosixPipeInput(readFd, writeFd, initialText);
    }

    private static TextReader CreatePseudoStdin(int readFd)
    {
        // Create a TextReader wrapper around the read file descriptor
        return new PosixFdTextReader(readFd);
    }

    public void SendBytes(byte[] data)
    {
        if (_writeClosed)
            throw new ObjectDisposedException("Pipe write end is closed");

        PosixInterop.Write(_writeFd, data, data.Length);
    }

    public void SendText(string text)
    {
        SendBytes(Encoding.UTF8.GetBytes(text));
    }

    public void Close()
    {
        // Close only the write end - this unblocks the reader
        // which will see EOF and eventually raise EOFError
        if (!_writeClosed)
        {
            PosixInterop.Close(_writeFd);
            _writeClosed = true;
        }
    }

    public override string TypeaheadHash() => $"pipe-input-{_id}";

    public override IDisposable RawMode() => NullDisposable.Instance;

    public override IDisposable CookedMode() => NullDisposable.Instance;

    public void Dispose()
    {
        if (!_writeClosed)
        {
            PosixInterop.Close(_writeFd);
            _writeClosed = true;
        }
        if (!_readClosed)
        {
            PosixInterop.Close(_readFd);
            _readClosed = true;
        }
    }
}
```

### Win32PipeInput Implementation

```csharp
[SupportedOSPlatform("windows")]
public sealed class Win32PipeInput : IPipeInput, IDisposable
{
    private static int _idCounter;
    private readonly int _id;
    private readonly nint _event;
    private readonly Vt100Parser _parser;
    private readonly List<KeyPress> _buffer = new();
    private bool _closed;

    private Win32PipeInput(nint eventHandle)
    {
        _event = eventHandle;
        _id = Interlocked.Increment(ref _idCounter);
        _parser = new Vt100Parser(key => _buffer.Add(key));
    }

    public static Win32PipeInput Create()
    {
        var eventHandle = Win32.CreateEvent(
            IntPtr.Zero, // No security attributes
            true,        // Manual reset
            false,       // Initial state not signaled
            null);       // No name

        if (eventHandle == IntPtr.Zero)
            throw new Win32Exception();

        return new Win32PipeInput(eventHandle);
    }

    public bool Closed => _closed;
    public nint Handle => _event;

    public IReadOnlyList<KeyPress> ReadKeys()
    {
        var result = _buffer.ToList();
        _buffer.Clear();

        // Reset event (if not closed)
        if (!_closed)
            Win32.ResetEvent(_event);

        return result;
    }

    public IReadOnlyList<KeyPress> FlushKeys()
    {
        _parser.Flush();
        return ReadKeys();
    }

    public void SendBytes(byte[] data)
    {
        SendText(Encoding.UTF8.GetString(data));
    }

    public void SendText(string text)
    {
        if (_closed)
            throw new ObjectDisposedException("Pipe is closed");

        // Parse through VT100 parser
        _parser.Feed(text);

        // Signal that data is available
        Win32.SetEvent(_event);
    }

    public void Close()
    {
        _closed = true;
        Win32.SetEvent(_event);  // Wake up any waiters
    }

    public string TypeaheadHash() => $"pipe-input-{_id}";

    public IDisposable Attach(Action inputReadyCallback)
    {
        return Win32Input.AttachInput(this, inputReadyCallback);
    }

    public IDisposable Detach()
    {
        return Win32Input.DetachInput(this);
    }

    public IDisposable RawMode() => NullDisposable.Instance;
    public IDisposable CookedMode() => NullDisposable.Instance;

    public void Dispose()
    {
        if (_event != IntPtr.Zero)
            Win32.CloseHandle(_event);
    }
}
```

### Usage Examples

```csharp
// Unit testing with pipe input
[Fact]
public async Task TestPromptInput()
{
    using var input = PosixPipeInput.Create();
    using var output = new StringWriter();

    var session = new PromptSession(input: input, output: output);

    // Send test input
    input.SendText("hello world\n");

    // Run prompt
    var result = await session.PromptAsync(">>> ");

    Assert.Equal("hello world", result);
}

// Testing key sequences
[Fact]
public void TestEscapeSequence()
{
    using var input = Win32PipeInput.Create();

    // Send arrow key
    input.SendText("\x1b[A");  // Up arrow

    var keys = input.FlushKeys();
    Assert.Single(keys);
    Assert.Equal(Keys.Up, keys[0].Key);
}

// Testing with initial text
[Fact]
public void TestInitialText()
{
    using var input = PosixPipeInput.Create("initial\n");

    var keys = input.ReadKeys();
    // Keys for 'i', 'n', 'i', 't', 'i', 'a', 'l', Enter
}
```

## Dependencies

- Feature 7: Input abstraction (IInput)
- Feature 8: VT100 input parsing (Vt100Input, Vt100Parser)
- Feature 75: Win32 input (for Windows event loop integration)

## Implementation Tasks

1. Implement IPipeInput interface
2. Implement PosixPipeInput with os.pipe wrapper
3. Implement Win32PipeInput with Win32 events
4. Integrate VT100 parser for key parsing
5. Implement TypeaheadHash for unique identification
6. Add factory methods with context management
7. Handle proper resource cleanup
8. Write unit tests

## Acceptance Criteria

- [ ] PosixPipeInput sends text through pipe
- [ ] PosixPipeInput parses VT100 sequences
- [ ] Win32PipeInput sends text through events
- [ ] Win32PipeInput parses VT100 sequences
- [ ] Close() signals EOF to reader
- [ ] TypeaheadHash returns unique values
- [ ] RawMode/CookedMode are no-ops
- [ ] Proper resource disposal
- [ ] Unit tests achieve 80% coverage
