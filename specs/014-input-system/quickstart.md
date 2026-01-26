# Quickstart: Input System

**Feature**: 014-input-system
**Date**: 2026-01-25

## Overview

The Input System provides terminal input abstraction for reading keyboard and mouse events with platform-specific backends and VT100 escape sequence parsing.

---

## Quick Examples

### Reading Keyboard Input

```csharp
using Stroke.Input;

// Create platform-appropriate input
using var input = InputFactory.Create();

// Enter raw mode (no echo, immediate character access)
using (input.RawMode())
{
    Console.WriteLine("Press any key (Ctrl+C to exit)...");

    while (!input.Closed)
    {
        var keys = input.ReadKeys();
        foreach (var keyPress in keys)
        {
            Console.WriteLine($"Key: {keyPress.Key}, Data: {Escape(keyPress.Data)}");

            if (keyPress.Key == Keys.ControlC)
                return;
        }

        // Flush for standalone Escape detection
        var flushed = input.FlushKeys();
        foreach (var keyPress in flushed)
        {
            Console.WriteLine($"Flushed: {keyPress.Key}");
        }

        Thread.Sleep(10); // Small delay between polls
    }
}

static string Escape(string s) =>
    string.Join("", s.Select(c => c < 32 ? $"\\x{(int)c:x2}" : c.ToString()));
```

### Testing with Pipe Input

```csharp
using Stroke.Input;

// Create pipe input for testing
using var pipeInput = InputFactory.CreatePipe();

// Send simulated keystrokes
pipeInput.SendText("hello");
pipeInput.SendBytes("\x1b[A"u8.ToArray()); // Up arrow

// Read the parsed keys
var keys = pipeInput.ReadKeys();
// keys[0..4] = 'h', 'e', 'l', 'l', 'o'
// keys[5] = Keys.Up

pipeInput.FlushKeys(); // Ensure all pending input is parsed
```

### Event Loop Integration

```csharp
using Stroke.Input;

using var input = InputFactory.Create();

// Attach callback for input-ready notification
using (input.Attach(OnInputReady))
using (input.RawMode())
{
    // Event loop runs here
    await RunEventLoopAsync();
}

void OnInputReady()
{
    var keys = input.ReadKeys();
    foreach (var key in keys)
    {
        ProcessKey(key);
    }
}
```

### Running Subprocesses (Cooked Mode)

```csharp
using Stroke.Input;

using var input = InputFactory.Create();

using (input.RawMode())
{
    // Interactive application running...

    // Need to run external command
    using (input.CookedMode())
    {
        // Terminal is back to normal line-buffered mode
        var process = Process.Start("vim", "file.txt");
        process.WaitForExit();
    }

    // Back to raw mode
}
```

---

## Core Concepts

### KeyPress Structure

A `KeyPress` contains:
- **Key**: The logical key identity (`Keys` enum)
- **Data**: The raw input data (escape sequences, characters)

```csharp
var keyPress = new KeyPress(Keys.Up, "\x1b[A");
Console.WriteLine(keyPress.Key);  // Up
Console.WriteLine(keyPress.Data); // \x1b[A
```

### Terminal Modes

| Mode | Echo | Line Buffering | Signals |
|------|------|----------------|---------|
| Raw | Off | Off (immediate) | Off (Ctrl+C is key) |
| Cooked | On | On (wait for Enter) | On |

### Escape Sequence Handling

The VT100 parser handles partial sequences:

```
Byte received: ESC
  → Parser waits (could be start of sequence)

Byte received: [
  → Parser waits (CSI sequence starting)

Byte received: A
  → Parser emits Keys.Up
```

If only ESC is received and no more bytes arrive, call `FlushKeys()` to emit it as `Keys.Escape`.

---

## API Reference

### InputFactory

| Method | Returns | Description |
|--------|---------|-------------|
| `Create(stdin?, alwaysPreferTty?)` | `IInput` | Create platform-appropriate input |
| `CreatePipe()` | `IPipeInput` | Create pipe input for testing |

### IInput

| Member | Type | Description |
|--------|------|-------------|
| `Closed` | `bool` | Whether input is closed |
| `ReadKeys()` | `IReadOnlyList<KeyPress>` | Read available key presses |
| `FlushKeys()` | `IReadOnlyList<KeyPress>` | Flush pending partial sequences |
| `RawMode()` | `IDisposable` | Enter raw terminal mode |
| `CookedMode()` | `IDisposable` | Enter cooked terminal mode |
| `Attach(callback)` | `IDisposable` | Attach to event loop |
| `Detach()` | `IDisposable` | Detach from event loop |
| `FileNo()` | `nint` | Native file descriptor/handle |
| `TypeaheadHash()` | `string` | Unique input identifier |
| `Close()` | `void` | Close the input |

### IPipeInput (extends IInput)

| Method | Description |
|--------|-------------|
| `SendBytes(data)` | Feed raw bytes into pipe |
| `SendText(data)` | Feed text into pipe |

---

## Platform-Specific Behavior

### POSIX (Linux/macOS)

- Uses termios for mode control
- File descriptor-based event loop integration
- Vt100Parser for escape sequence handling

### Windows

- Uses Console APIs for mode control
- Handle-based event loop integration
- Win10+ supports VT100 mode (shares Vt100Parser)
- Legacy Windows uses ConsoleInputReader

### DummyInput

- Used when stdin is not a terminal
- `Closed` always returns `true`
- `ReadKeys()` returns empty list
- Mode methods return no-op disposables

---

## Testing Patterns

### Unit Test Example

```csharp
[Fact]
public void ArrowKeys_AreParsedCorrectly()
{
    var keys = new List<KeyPress>();
    var parser = new Vt100Parser(keys.Add);

    parser.FeedAndFlush("\x1b[A\x1b[B\x1b[C\x1b[D");

    Assert.Equal(4, keys.Count);
    Assert.Equal(Keys.Up, keys[0].Key);
    Assert.Equal(Keys.Down, keys[1].Key);
    Assert.Equal(Keys.Right, keys[2].Key);
    Assert.Equal(Keys.Left, keys[3].Key);
}
```

### Integration Test with PipeInput

```csharp
[Fact]
public void PipeInput_SendsTextThroughParser()
{
    using var input = InputFactory.CreatePipe();

    input.SendText("abc");
    var keys = input.ReadKeys();

    Assert.Equal(3, keys.Count);
    Assert.All(keys, k => Assert.Equal(k.Data, k.Key.ToString().ToLower()));
}
```

---

## Common Escape Sequences

| Sequence | Key |
|----------|-----|
| `\x1b[A` | Up |
| `\x1b[B` | Down |
| `\x1b[C` | Right |
| `\x1b[D` | Left |
| `\x1b[H` | Home |
| `\x1b[F` | End |
| `\x1b[2~` | Insert |
| `\x1b[3~` | Delete |
| `\x1b[5~` | PageUp |
| `\x1b[6~` | PageDown |
| `\x1bOP` | F1 |
| `\x1bOQ` | F2 |
| `\x1bOR` | F3 |
| `\x1bOS` | F4 |
| `\x1b[15~` | F5 |
| `\x1b[200~`...`\x1b[201~` | BracketedPaste |
