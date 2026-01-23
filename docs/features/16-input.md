# Feature 16: Input System

## Overview

Implement the input abstraction layer for reading keyboard and mouse input from terminals, with support for VT100 parsing, raw/cooked modes, and platform-specific backends.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/input/base.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/input/defaults.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/input/vt100.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/input/vt100_parser.py`

## Public API

### KeyPress Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Represents a key press event.
/// </summary>
public sealed class KeyPress : IEquatable<KeyPress>
{
    /// <summary>
    /// Creates a key press.
    /// </summary>
    /// <param name="key">A Keys enum value or a single character string.</param>
    /// <param name="data">The received string on stdin (often VT100 escape codes).</param>
    public KeyPress(Keys key, string? data = null);

    /// <summary>
    /// Creates a key press from a character.
    /// </summary>
    /// <param name="key">A single character.</param>
    /// <param name="data">The received string on stdin.</param>
    public KeyPress(string key, string? data = null);

    /// <summary>
    /// The key that was pressed.
    /// </summary>
    public Keys Key { get; }

    /// <summary>
    /// The raw data received from stdin.
    /// </summary>
    public string Data { get; }

    public bool Equals(KeyPress? other);
    public override bool Equals(object? obj);
    public override int GetHashCode();
    public override string ToString();
}
```

### IInput Interface (Abstract Base)

```csharp
namespace Stroke.Input;

/// <summary>
/// Abstraction for any input source.
/// </summary>
public interface IInput : IDisposable
{
    /// <summary>
    /// File descriptor for putting this in an event loop.
    /// </summary>
    int Fileno();

    /// <summary>
    /// Identifier for storing typeahead key presses.
    /// </summary>
    string TypeaheadHash { get; }

    /// <summary>
    /// Return a list of KeyPress objects which are read/parsed from the input.
    /// </summary>
    IReadOnlyList<KeyPress> ReadKeys();

    /// <summary>
    /// Flush the underlying parser and return pending keys.
    /// Used for VT100 input.
    /// </summary>
    IReadOnlyList<KeyPress> FlushKeys();

    /// <summary>
    /// The event loop can call this when the input has to be flushed.
    /// </summary>
    void Flush();

    /// <summary>
    /// True when the input stream is closed.
    /// </summary>
    bool Closed { get; }

    /// <summary>
    /// Context manager that turns the input into raw mode.
    /// </summary>
    IDisposable RawMode();

    /// <summary>
    /// Context manager that turns the input into cooked mode.
    /// </summary>
    IDisposable CookedMode();

    /// <summary>
    /// Return a context manager that makes this input active in the current event loop.
    /// </summary>
    IDisposable Attach(Action inputReadyCallback);

    /// <summary>
    /// Return a context manager that makes sure that this input is not active
    /// in the current event loop.
    /// </summary>
    IDisposable Detach();

    /// <summary>
    /// Close input.
    /// </summary>
    void Close();
}
```

### IPipeInput Interface

```csharp
namespace Stroke.Input;

/// <summary>
/// Abstraction for pipe input.
/// </summary>
public interface IPipeInput : IInput
{
    /// <summary>
    /// Feed byte string into the pipe.
    /// </summary>
    void SendBytes(byte[] data);

    /// <summary>
    /// Feed a text string into the pipe.
    /// </summary>
    void SendText(string data);
}
```

### DummyInput Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Input for use in a DummyApplication.
/// If used in an actual application, it will make the application render
/// itself once and exit immediately, due to an EOFError.
/// </summary>
public sealed class DummyInput : IInput
{
    public int Fileno() => throw new NotImplementedException();
    public string TypeaheadHash => $"dummy-{GetHashCode()}";
    public IReadOnlyList<KeyPress> ReadKeys() => Array.Empty<KeyPress>();
    public IReadOnlyList<KeyPress> FlushKeys() => Array.Empty<KeyPress>();
    public void Flush() { }
    public bool Closed => true;
    public IDisposable RawMode() => new NoopDisposable();
    public IDisposable CookedMode() => new NoopDisposable();
    public IDisposable Attach(Action inputReadyCallback)
    {
        inputReadyCallback();
        return new NoopDisposable();
    }
    public IDisposable Detach() => new NoopDisposable();
    public void Close() { }
    public void Dispose() { }
}
```

### Vt100Input Class

```csharp
namespace Stroke.Input;

/// <summary>
/// VT100 terminal input.
/// </summary>
public sealed class Vt100Input : IInput
{
    /// <summary>
    /// Creates a VT100 input.
    /// </summary>
    /// <param name="stdin">The stdin stream.</param>
    public Vt100Input(TextReader stdin);

    public int Fileno();
    public string TypeaheadHash { get; }
    public IReadOnlyList<KeyPress> ReadKeys();
    public IReadOnlyList<KeyPress> FlushKeys();
    public void Flush();
    public bool Closed { get; }
    public IDisposable RawMode();
    public IDisposable CookedMode();
    public IDisposable Attach(Action inputReadyCallback);
    public IDisposable Detach();
    public void Close();
    public void Dispose();
}
```

### Vt100Parser Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Parser for VT100 input escape sequences.
/// </summary>
public sealed class Vt100Parser
{
    /// <summary>
    /// Creates a VT100 parser.
    /// </summary>
    /// <param name="feedKeyCallback">Callback to receive parsed key presses.</param>
    public Vt100Parser(Action<KeyPress> feedKeyCallback);

    /// <summary>
    /// Feed input data into the parser.
    /// </summary>
    public void Feed(string data);

    /// <summary>
    /// Flush any pending input.
    /// </summary>
    public void Flush();

    /// <summary>
    /// Reset parser state.
    /// </summary>
    public void Reset();
}
```

### PosixStdinReader Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Wrapper around stdin for POSIX systems.
/// </summary>
public sealed class PosixStdinReader : IDisposable
{
    public PosixStdinReader(TextReader stdin);

    /// <summary>
    /// Read available data from stdin.
    /// </summary>
    public string Read();

    /// <summary>
    /// Close the reader.
    /// </summary>
    public void Close();

    public void Dispose();
}
```

### RawModeContext Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Context for putting terminal in raw mode.
/// </summary>
public sealed class RawModeContext : IDisposable
{
    public RawModeContext(int fileno);
    public void Dispose();
}
```

### CreateInput Factory Functions

```csharp
namespace Stroke.Input;

/// <summary>
/// Input factory methods.
/// </summary>
public static class InputFactory
{
    /// <summary>
    /// Create the appropriate Input class for the current platform.
    /// </summary>
    /// <param name="stdin">The stdin stream (or null for default).</param>
    /// <param name="alwaysPreferTty">When set, look for a TTY if stdin is not one.</param>
    public static IInput CreateInput(
        TextReader? stdin = null,
        bool alwaysPreferTty = true);

    /// <summary>
    /// Create a pipe input for testing.
    /// </summary>
    public static IPipeInput CreatePipeInput();
}
```

### PipeInput Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Input class that reads from a pipe.
/// Useful for testing and automation.
/// </summary>
public sealed class PipeInput : IPipeInput
{
    public PipeInput();

    public void SendBytes(byte[] data);
    public void SendText(string data);

    public int Fileno();
    public string TypeaheadHash { get; }
    public IReadOnlyList<KeyPress> ReadKeys();
    public IReadOnlyList<KeyPress> FlushKeys();
    public void Flush();
    public bool Closed { get; }
    public IDisposable RawMode();
    public IDisposable CookedMode();
    public IDisposable Attach(Action inputReadyCallback);
    public IDisposable Detach();
    public void Close();
    public void Dispose();
}
```

## Project Structure

```
src/Stroke/
└── Input/
    ├── KeyPress.cs
    ├── IInput.cs
    ├── IPipeInput.cs
    ├── DummyInput.cs
    ├── Vt100Input.cs
    ├── Vt100Parser.cs
    ├── PosixStdinReader.cs
    ├── RawModeContext.cs
    ├── PipeInput.cs
    ├── InputFactory.cs
    └── NoopDisposable.cs (internal)
tests/Stroke.Tests/
└── Input/
    ├── KeyPressTests.cs
    ├── DummyInputTests.cs
    ├── Vt100ParserTests.cs
    ├── PipeInputTests.cs
    └── InputFactoryTests.cs
```

## Implementation Notes

### VT100 Parser State Machine

The parser handles:
- CSI sequences (`\x1b[...`) for navigation keys, function keys
- SGR mouse events (`\x1b[<...M`)
- Cursor position reports (`\x1b[row;colR`)
- Bracketed paste (`\x1b[200~` ... `\x1b[201~`)
- Special keys (Escape, Tab, Enter, Backspace)
- UTF-8 multi-byte characters

### Raw Mode

Raw mode disables:
- Line buffering
- Echo
- Canonical mode
- Signal generation (Ctrl+C)

### Cooked Mode

Cooked mode enables:
- Line buffering
- Echo
- Canonical mode

### Mouse Event Parsing

VT100 mouse events are parsed from:
- X10 mouse protocol
- SGR extended mouse protocol
- urxvt mouse protocol

### Typeahead Hash

The typeahead hash is used to identify the input source for storing typeahead key presses across application sessions.

### Input Attachment

When input is attached:
- Register file descriptor with event loop
- Install signal handlers (if applicable)
- Configure terminal for input

## Dependencies

- `Stroke.Input.Keys` (Feature 10) - Key enum values
- `Stroke.Input.MouseEvent` (Feature 17) - Mouse event parsing

## Implementation Tasks

1. Implement `KeyPress` class with equality
2. Implement `IInput` interface
3. Implement `IPipeInput` interface
4. Implement `DummyInput` class
5. Implement `Vt100Parser` state machine
6. Implement `PosixStdinReader` class
7. Implement `RawModeContext` class
8. Implement `Vt100Input` class
9. Implement `PipeInput` class
10. Implement `InputFactory` static class
11. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All input types match Python Prompt Toolkit semantics
- [ ] VT100 parser handles all escape sequences correctly
- [ ] Raw/cooked mode transitions work correctly
- [ ] Mouse event parsing is correct
- [ ] Unit tests achieve 80% coverage
