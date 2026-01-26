# IInput Interface Contract

**Namespace**: `Stroke.Input`
**Type**: Interface
**Python Equivalent**: `prompt_toolkit.input.Input`

## Summary

Abstraction for any terminal input source. An instance of this interface can be passed to an `Application` and will be used for reading keyboard and mouse input.

## Thread Safety

`ReadKeys` and `FlushKeys` assume single-threaded access. Thread safety at the event distribution layer (channels, queues) is the caller's responsibility.

---

## Members

### Properties

#### Closed

```csharp
bool Closed { get; }
```

Gets whether the input stream is closed.

**Remarks**: When true, the application should handle this as an EOF condition.

---

### Methods

#### ReadKeys

```csharp
IReadOnlyList<KeyPress> ReadKeys();
```

Reads and parses available key presses from the input.

**Returns**: A list of `KeyPress` objects parsed from the input stream. Returns an empty list if no input is available.

**Exceptions**: `ObjectDisposedException` thrown if called after `Close()` or `Dispose()`.

**Blocking Behavior**:
- When attached to event loop: Non-blocking; returns immediately with available data or empty list
- When not attached: May block until input is available (platform-dependent)
- POSIX: Uses non-blocking I/O when attached; blocking read otherwise
- Windows: Uses `WaitForSingleObject` with timeout when attached; blocking `ReadConsoleInput` otherwise

**Remarks**:
- Reads raw input data and parses it into key press events
- For VT100 input, this includes parsing escape sequences
- Assumes single-threaded access; concurrent calls result in undefined behavior

---

#### FlushKeys

```csharp
IReadOnlyList<KeyPress> FlushKeys();
```

Flushes pending partial escape sequences and returns any resulting key presses.

**Returns**: A list of `KeyPress` objects from flushed partial sequences.

**Default Implementation**: Returns an empty list.

**Remarks**:
- Critical for detecting standalone Escape key presses
- When Escape is pressed alone (not as part of a sequence), the parser waits briefly for additional characters
- If none arrive, flush should be called to emit the Escape as a key press

---

#### RawMode

```csharp
IDisposable RawMode();
```

Enters raw terminal mode.

**Returns**: A disposable that restores the previous terminal mode when disposed.

**Raw Mode Settings**:
| Setting | Value |
|---------|-------|
| Echo | Disabled |
| Canonical mode | Disabled (characters available immediately) |
| Signal generation | Disabled (Ctrl+C produces a key press) |
| Flow control | Disabled (Ctrl+S/Ctrl+Q produce key presses) |

**Remarks**: If the input is not connected to a TTY, this may return a no-op disposable.

---

#### CookedMode

```csharp
IDisposable CookedMode();
```

Enters cooked (canonical) terminal mode.

**Returns**: A disposable that restores the previous terminal mode when disposed.

**Use Case**: Temporarily needing normal terminal behavior while in raw mode, for example when running a subprocess or prompting for line-buffered input.

**Cooked Mode Settings**:
| Setting | Value |
|---------|-------|
| Echo | Enabled |
| Canonical mode | Enabled (line buffering) |
| Signal generation | Enabled |

---

#### Attach

```csharp
IDisposable Attach(Action inputReadyCallback);
```

Attaches this input to the current event loop with a callback.

**Parameters**:
- `inputReadyCallback`: A callback invoked when input is available to read.

**Returns**: A disposable that detaches the input when disposed.

**Exceptions**:
- `ArgumentNullException` thrown if `inputReadyCallback` is null.
- `ObjectDisposedException` thrown if called after `Close()` or `Dispose()`.

**Multiple Attach Semantics**:
- Multiple `Attach()` calls are supported and form a stack (LIFO)
- Each `Attach()` returns a unique disposable
- Disposing the most recent attachment restores the previous callback
- Disposing an older attachment removes it from the stack (callbacks below it shift up)

**Callback Invocation Contract**:
- Callback is invoked on the event loop thread (platform-dependent)
- Callback MUST NOT block; it should call `ReadKeys()` and return promptly
- Callback may be invoked spuriously (check `ReadKeys()` result for actual data)
- Callback is NOT invoked during or after `Close()`

**Remarks**:
- The callback is invoked when data is available on the input stream
- The callback should then call `ReadKeys()` to retrieve the available key presses

---

#### Detach

```csharp
IDisposable Detach();
```

Temporarily detaches this input from the event loop.

**Returns**: A disposable that reattaches the input when disposed.

**Behavior When Not Attached**: Returns a no-op disposable; no error is thrown.

**Use Case**: Temporarily suspending input processing, for example when running an external command that needs direct terminal access.

---

#### FileNo

```csharp
nint FileNo();
```

Gets the native file descriptor or handle for this input.

**Returns**: The file descriptor (POSIX) or handle (Windows) for event loop registration.

**Exceptions**: `NotImplementedException` thrown by `DummyInput` which has no file descriptor.

---

#### TypeaheadHash

```csharp
string TypeaheadHash();
```

Gets a unique identifier for storing typeahead key presses.

**Returns**: A string that uniquely identifies this input source.

**Remarks**: Typeahead handling stores excess key presses from one prompt for the next prompt. This hash ensures different inputs don't interfere.

---

#### Close

```csharp
void Close();
```

Closes the input.

**Remarks**: After closing, `Closed` returns true and `ReadKeys()` returns an empty list.

---

## Implementations

| Class | Platform | Description |
|-------|----------|-------------|
| `DummyInput` | Any | No-op input for non-terminal scenarios |
| `Vt100Input` | POSIX | VT100 escape sequence parsing |
| `Win32Input` | Windows | Windows Console API input |
