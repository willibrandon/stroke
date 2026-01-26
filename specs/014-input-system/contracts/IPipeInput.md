# IPipeInput Interface Contract

**Namespace**: `Stroke.Input`
**Type**: Interface (extends `IInput`)
**Python Equivalent**: `prompt_toolkit.input.PipeInput`

## Summary

Extended input interface for pipe-based input, primarily used for testing. Pipe input allows programmatic feeding of data into the input system, enabling automated testing without requiring a real terminal.

## Thread Safety

`SendBytes` and `SendText` are thread-safe and may be called from any thread while `ReadKeys` is called from the reader thread. This enables test scenarios where input is fed asynchronously.

## Inheritance

```
IInput
  └── IPipeInput
```

---

## Members

### Methods

#### SendBytes

```csharp
void SendBytes(ReadOnlySpan<byte> data);
```

Feeds raw bytes into the pipe.

**Parameters**:
- `data`: The bytes to feed into the input stream.

**Exceptions**: `ObjectDisposedException` thrown if the pipe has been closed.

**Encoding**: Bytes are interpreted as UTF-8. Invalid UTF-8 sequences are replaced with U+FFFD.

**Timing**: Data is immediately available to the next `ReadKeys()` call. There is no buffering delay, but the reader thread must call `ReadKeys()` to receive the data.

**Remarks**:
- Bytes are decoded using UTF-8 and processed through the VT100 parser
- Allows sending raw escape sequences for testing

**Example**:
```csharp
// Send arrow up key
pipeInput.SendBytes("\x1b[A"u8.ToArray());
```

---

#### SendText

```csharp
void SendText(string data);
```

Feeds a text string into the pipe.

**Parameters**:
- `data`: The text to feed into the input stream.

**Exceptions**:
- `ObjectDisposedException` thrown if the pipe has been closed.
- `ArgumentNullException` thrown if `data` is null.

**Encoding**: Text is encoded as UTF-8 before being fed into the parser.

**Timing**: Data is immediately available to the next `ReadKeys()` call.

**Remarks**:
- Text is encoded as UTF-8 and processed through the VT100 parser
- Special characters (like escape) in the string will be interpreted as escape sequences

**Example**:
```csharp
// Send text as if typed
pipeInput.SendText("hello world\r");
```

---

## High-Volume Input

PipeInput buffers all sent data in memory. There is no backpressure mechanism. For test scenarios with very large input volumes:
- Memory usage is bounded only by available heap
- Consider chunking large inputs and calling `ReadKeys()` between chunks
- Sustained rates of 10,000+ key presses per second are supported

---

## Implementations

| Class | Platform | Description |
|-------|----------|-------------|
| `PosixPipeInput` | POSIX | Uses OS pipe() system call |
| `Win32PipeInput` | Windows | Uses Windows event objects |

---

## Usage Example

```csharp
using var pipeInput = InputFactory.CreatePipe();

// Send simulated input
pipeInput.SendText("hello");
pipeInput.SendBytes("\x1b[A"u8.ToArray()); // Up arrow

// Read parsed keys
var keys = pipeInput.ReadKeys();
// keys contains: 'h', 'e', 'l', 'l', 'o', Keys.Up
```
