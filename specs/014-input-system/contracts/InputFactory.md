# InputFactory Class Contract

**Namespace**: `Stroke.Input`
**Type**: Static class
**Python Equivalent**: `prompt_toolkit.input.defaults` module

## Summary

Factory for creating platform-appropriate input instances. This factory automatically selects the correct input implementation based on the current platform and environment.

---

## Methods

### Create

```csharp
public static IInput Create(Stream? stdin = null, bool alwaysPreferTty = false);
```

Creates an input instance appropriate for the current platform.

**Parameters**:
- `stdin`: Optional stdin stream. If null, uses the system's standard input.
- `alwaysPreferTty`: If true and stdin is not a TTY but stdout or stderr is, use the TTY for input instead.

**Returns**: An `IInput` instance appropriate for the current environment.

**Selection Logic**:

| Condition | Result |
|-----------|--------|
| stdin is null and environment cannot provide input | `DummyInput` |
| Windows platform | `Win32Input` |
| POSIX platform (Linux/macOS) | `Vt100Input` |
| stdin doesn't support fileno() | `DummyInput` |

**Remarks**: The `alwaysPreferTty` flag is useful when stdin is piped but you still want to read interactive input from the terminal.

**Example**:
```csharp
// Default: use system stdin
using var input = InputFactory.Create();

// Use specific stream
using var input = InputFactory.Create(myStream);

// Prefer TTY even if stdin is piped
using var input = InputFactory.Create(alwaysPreferTty: true);
```

---

### CreatePipe

```csharp
public static IPipeInput CreatePipe();
```

Creates a pipe input for testing.

**Returns**: An `IPipeInput` instance that allows programmatic input feeding.

**Exceptions**: `PlatformNotSupportedException` thrown if the platform does not support pipe creation (hypothetical; all supported platforms have pipes).

**Platform Implementation**:

| Platform | Implementation |
|----------|----------------|
| POSIX (Linux/macOS) | `PosixPipeInput` using OS pipes |
| Windows | `Win32PipeInput` using Windows events |

**Remarks**: This method always succeeds on supported platforms. Unlike `Create()`, it does not fall back to `DummyInput`.

**Example**:
```csharp
using var pipeInput = InputFactory.CreatePipe();
pipeInput.SendText("hello\r");
var keys = pipeInput.ReadKeys();
```

---

## Platform Detection

The factory uses the following runtime checks:

```csharp
OperatingSystem.IsWindows()   // Windows platform
OperatingSystem.IsLinux()     // Linux platform
OperatingSystem.IsMacOS()     // macOS platform
```

---

## Usage Patterns

### Basic Interactive Application

```csharp
using var input = InputFactory.Create();
using (input.RawMode())
{
    while (!input.Closed)
    {
        var keys = input.ReadKeys();
        ProcessKeys(keys);
    }
}
```

### Unit Testing

```csharp
[Fact]
public void ProcessesInput_Correctly()
{
    using var input = InputFactory.CreatePipe();
    input.SendText("test\r");

    var keys = input.ReadKeys();
    Assert.Equal(5, keys.Count); // 't', 'e', 's', 't', Enter
}
```

### Piped Input with TTY Fallback

```csharp
// Useful for: echo "data" | myapp
// Still allows interactive prompts via TTY
using var input = InputFactory.Create(alwaysPreferTty: true);
```
