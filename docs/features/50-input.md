# Feature 50: Input System

## Overview

Implement the input abstraction layer including the base Input class, platform-specific implementations (Vt100Input for POSIX, Win32Input for Windows), PipeInput for testing, and raw/cooked terminal mode management.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/input/base.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/input/defaults.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/input/vt100.py`

## Public API

### Input Abstract Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Abstraction for any input source.
/// </summary>
public abstract class Input
{
    /// <summary>
    /// File descriptor for use in event loop.
    /// </summary>
    public abstract int FileNo();

    /// <summary>
    /// Identifier for storing type ahead key presses.
    /// </summary>
    public abstract string TypeaheadHash();

    /// <summary>
    /// Read and parse keys from the input.
    /// </summary>
    /// <returns>List of KeyPress objects.</returns>
    public abstract IList<KeyPress> ReadKeys();

    /// <summary>
    /// Flush the underlying parser and return pending keys.
    /// </summary>
    public virtual IList<KeyPress> FlushKeys() => Array.Empty<KeyPress>();

    /// <summary>
    /// Flush the input.
    /// </summary>
    public virtual void Flush() { }

    /// <summary>
    /// Whether the input stream is closed.
    /// </summary>
    public abstract bool Closed { get; }

    /// <summary>
    /// Enter raw terminal mode.
    /// </summary>
    /// <returns>Disposable that restores previous mode.</returns>
    public abstract IDisposable RawMode();

    /// <summary>
    /// Enter cooked terminal mode.
    /// </summary>
    /// <returns>Disposable that restores previous mode.</returns>
    public abstract IDisposable CookedMode();

    /// <summary>
    /// Attach to event loop with callback.
    /// </summary>
    /// <param name="inputReadyCallback">Called when input is ready.</param>
    /// <returns>Disposable that detaches from event loop.</returns>
    public abstract IDisposable Attach(Action inputReadyCallback);

    /// <summary>
    /// Detach from event loop.
    /// </summary>
    /// <returns>Disposable that reattaches.</returns>
    public abstract IDisposable Detach();

    /// <summary>
    /// Close the input.
    /// </summary>
    public virtual void Close() { }
}
```

### PipeInput Abstract Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Abstraction for pipe input (useful for testing).
/// </summary>
public abstract class PipeInput : Input
{
    /// <summary>
    /// Feed raw bytes into the pipe.
    /// </summary>
    public abstract void SendBytes(byte[] data);

    /// <summary>
    /// Feed text into the pipe.
    /// </summary>
    public abstract void SendText(string data);
}
```

### DummyInput Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Input for use in DummyApplication.
/// Triggers EOFError immediately.
/// </summary>
public sealed class DummyInput : Input
{
    public override int FileNo() => throw new NotImplementedException();

    public override string TypeaheadHash() => $"dummy-{GetHashCode()}";

    public override IList<KeyPress> ReadKeys() => Array.Empty<KeyPress>();

    public override bool Closed => true;

    public override IDisposable RawMode() => Disposable.Empty;

    public override IDisposable CookedMode() => Disposable.Empty;

    public override IDisposable Attach(Action inputReadyCallback)
    {
        // Call callback immediately to trigger EOFError check
        inputReadyCallback();
        return Disposable.Empty;
    }

    public override IDisposable Detach() => Disposable.Empty;
}
```

### Vt100Input Class

```csharp
namespace Stroke.Input;

/// <summary>
/// VT100 input for POSIX systems.
/// </summary>
public sealed class Vt100Input : Input
{
    /// <summary>
    /// Creates a Vt100Input from a TextReader.
    /// </summary>
    /// <param name="stdin">The standard input stream.</param>
    public Vt100Input(TextReader stdin);

    /// <summary>
    /// The underlying stdin stream.
    /// </summary>
    public TextReader Stdin { get; }

    public override int FileNo();
    public override string TypeaheadHash();
    public override IList<KeyPress> ReadKeys();
    public override IList<KeyPress> FlushKeys();
    public override bool Closed { get; }
    public override IDisposable RawMode();
    public override IDisposable CookedMode();
    public override IDisposable Attach(Action inputReadyCallback);
    public override IDisposable Detach();
}
```

### Factory Functions

```csharp
namespace Stroke.Input;

public static class InputFactory
{
    /// <summary>
    /// Create the appropriate Input for the current OS/environment.
    /// </summary>
    /// <param name="stdin">Optional stdin override.</param>
    /// <param name="alwaysPreferTty">Use TTY even when stdin is piped.</param>
    public static Input CreateInput(
        TextReader? stdin = null,
        bool alwaysPreferTty = false);

    /// <summary>
    /// Create a pipe input for testing.
    /// </summary>
    public static PipeInputScope CreatePipeInput();
}
```

### PipeInputScope Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Scope for pipe input, used for testing.
/// </summary>
public sealed class PipeInputScope : IDisposable
{
    /// <summary>
    /// The pipe input instance.
    /// </summary>
    public PipeInput Input { get; }

    public void Dispose();
}
```

## Project Structure

```
src/Stroke/
└── Input/
    ├── Input.cs
    ├── PipeInput.cs
    ├── DummyInput.cs
    ├── InputFactory.cs
    ├── PipeInputScope.cs
    ├── Vt100Input.cs
    ├── Vt100Parser.cs
    ├── RawMode.cs
    ├── CookedMode.cs
    ├── PosixStdinReader.cs
    ├── PosixPipeInput.cs
    ├── Win32Input.cs
    └── Win32PipeInput.cs
tests/Stroke.Tests/
└── Input/
    ├── DummyInputTests.cs
    ├── Vt100InputTests.cs
    ├── RawModeTests.cs
    └── PipeInputTests.cs
```

## Implementation Notes

### Raw Mode (POSIX)

Raw mode disables terminal features for character-at-a-time input:

```csharp
public sealed class RawMode : IDisposable
{
    private readonly int _fileno;
    private readonly TermiosSettings? _previousSettings;

    public RawMode(int fileno)
    {
        _fileno = fileno;
        try
        {
            _previousSettings = Termios.GetAttributes(fileno);
            var newSettings = _previousSettings.Clone();

            // Disable echo, canonical mode, extended functions, signal generation
            newSettings.LocalFlags &= ~(LocalFlags.ECHO | LocalFlags.ICANON |
                                         LocalFlags.IEXTEN | LocalFlags.ISIG);

            // Disable XON/XOFF flow control, carriage return translation
            newSettings.InputFlags &= ~(InputFlags.IXON | InputFlags.IXOFF |
                                         InputFlags.ICRNL | InputFlags.INLCR |
                                         InputFlags.IGNCR);

            // Set VMIN to 1 (read at least 1 character)
            newSettings.ControlCharacters[ControlCharacter.VMIN] = 1;

            Termios.SetAttributes(fileno, newSettings, When.NOW);
        }
        catch
        {
            _previousSettings = null;
        }
    }

    public void Dispose()
    {
        if (_previousSettings != null)
        {
            try { Termios.SetAttributes(_fileno, _previousSettings, When.NOW); }
            catch { }
        }
    }
}
```

### Cooked Mode

Cooked mode re-enables terminal features (opposite of raw mode):

```csharp
public sealed class CookedMode : IDisposable
{
    private readonly int _fileno;
    private readonly TermiosSettings? _previousSettings;

    public CookedMode(int fileno)
    {
        _fileno = fileno;
        try
        {
            _previousSettings = Termios.GetAttributes(fileno);
            var newSettings = _previousSettings.Clone();

            // Enable echo, canonical mode, extended functions, signal generation
            newSettings.LocalFlags |= LocalFlags.ECHO | LocalFlags.ICANON |
                                      LocalFlags.IEXTEN | LocalFlags.ISIG;

            // Enable carriage return to newline translation
            newSettings.InputFlags |= InputFlags.ICRNL;

            Termios.SetAttributes(fileno, newSettings, When.NOW);
        }
        catch
        {
            _previousSettings = null;
        }
    }

    public void Dispose()
    {
        if (_previousSettings != null)
        {
            try { Termios.SetAttributes(_fileno, _previousSettings, When.NOW); }
            catch { }
        }
    }
}
```

### CreateInput Platform Selection

```csharp
public static Input CreateInput(TextReader? stdin = null, bool alwaysPreferTty = false)
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        if (stdin == null && Console.In == null)
            return new DummyInput();
        return new Win32Input(stdin ?? Console.In);
    }
    else
    {
        stdin ??= Console.In;

        if (alwaysPreferTty)
        {
            // Prefer TTY device even when stdin is piped
            foreach (var stream in new[] { Console.In, Console.Out, Console.Error })
            {
                if (IsAtty(stream))
                {
                    stdin = stream;
                    break;
                }
            }
        }

        // Check if we can get file descriptor
        try
        {
            GetFileNo(stdin);
        }
        catch
        {
            return new DummyInput();
        }

        return new Vt100Input(stdin);
    }
}
```

### Vt100Parser Integration

Vt100Input uses Vt100Parser to parse escape sequences:

```csharp
public override IList<KeyPress> ReadKeys()
{
    // Read available data
    string data = _stdinReader.Read();

    // Feed to parser
    _vt100Parser.Feed(data);

    // Return accumulated key presses
    var result = _buffer.ToList();
    _buffer.Clear();
    return result;
}

public override IList<KeyPress> FlushKeys()
{
    // Flush pending escape sequences (important for standalone Escape key)
    _vt100Parser.Flush();

    var result = _buffer.ToList();
    _buffer.Clear();
    return result;
}
```

### Attach/Detach with Event Loop

```csharp
public override IDisposable Attach(Action inputReadyCallback)
{
    var loop = EventLoop.Current;
    int fd = FileNo();

    // Add file descriptor to event loop
    loop.AddReader(fd, () =>
    {
        if (Closed)
            loop.RemoveReader(fd);
        inputReadyCallback();
    });

    return Disposable.Create(() => loop.RemoveReader(fd));
}
```

## Dependencies

- `Stroke.Input.VtParser` (Feature 03) - VT100 parser
- `Stroke.KeyBinding.KeyPress` (Feature 19) - KeyPress class
- Platform-specific terminal APIs (termios, Windows Console API)

## Implementation Tasks

1. Implement `Input` abstract base class
2. Implement `PipeInput` abstract class
3. Implement `DummyInput` class
4. Implement `Vt100Input` with parser
5. Implement `RawMode` context manager
6. Implement `CookedMode` context manager
7. Implement `PosixStdinReader` for non-blocking read
8. Implement `PosixPipeInput` for testing
9. Implement `Win32Input` for Windows
10. Implement `Win32PipeInput` for Windows testing
11. Implement `InputFactory.CreateInput()`
12. Implement `InputFactory.CreatePipeInput()`
13. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Input abstract class defines full interface
- [ ] DummyInput triggers EOF immediately
- [ ] Vt100Input reads and parses VT100 sequences
- [ ] RawMode disables terminal echo/canonical mode
- [ ] CookedMode restores terminal settings
- [ ] FlushKeys handles standalone Escape key
- [ ] CreateInput detects platform correctly
- [ ] PipeInput allows programmatic input for testing
- [ ] Attach/Detach integrates with event loop
- [ ] Unit tests achieve 80% coverage
