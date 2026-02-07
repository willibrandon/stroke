# Contract: IOutput Synchronized Output Extension

**Feature**: 067-synchronized-output
**Requirement**: FR-001

## Interface Extension

```csharp
public interface IOutput
{
    // ... existing members ...

    /// <summary>
    /// Begins a synchronized output region. The terminal buffers all subsequent
    /// output until <see cref="EndSynchronizedOutput"/> is called.
    /// </summary>
    /// <remarks>
    /// On VT100-based outputs, this sets an internal flag that causes the next
    /// <see cref="Flush"/> to wrap its content in DEC Mode 2026 markers.
    /// On non-VT100 outputs, this is a no-op.
    /// </remarks>
    void BeginSynchronizedOutput();

    /// <summary>
    /// Ends a synchronized output region. The terminal commits all buffered
    /// output atomically.
    /// </summary>
    /// <remarks>
    /// On VT100-based outputs, this clears the synchronized output flag.
    /// On non-VT100 outputs, this is a no-op.
    /// </remarks>
    void EndSynchronizedOutput();
}
```

## Implementation Contracts

### Vt100Output (FR-002, FR-003, FR-015)

```csharp
public sealed partial class Vt100Output : IOutput
{
    private bool _synchronizedOutput; // Protected by existing _lock

    public void BeginSynchronizedOutput()
    {
        // Sets _synchronizedOutput = true under lock
    }

    public void EndSynchronizedOutput()
    {
        // Sets _synchronizedOutput = false under lock
    }

    public void Flush()
    {
        // When _synchronizedOutput is true:
        //   Writes: "\x1b[?2026h" + buffer_content + "\x1b[?2026l"
        // When false:
        //   Writes: buffer_content (unchanged behavior)
    }
}
```

### Win32Output (FR-004)

```csharp
public void BeginSynchronizedOutput() { } // No-op
public void EndSynchronizedOutput() { }   // No-op
```

### PlainTextOutput (FR-005)

```csharp
public void BeginSynchronizedOutput() { } // No-op
public void EndSynchronizedOutput() { }   // No-op
```

### DummyOutput (FR-006)

```csharp
public void BeginSynchronizedOutput() { } // No-op
public void EndSynchronizedOutput() { }   // No-op
```

### Windows10Output (FR-007)

```csharp
public void BeginSynchronizedOutput() => _vt100Output.BeginSynchronizedOutput();
public void EndSynchronizedOutput() => _vt100Output.EndSynchronizedOutput();
```

### ConEmuOutput (FR-008)

```csharp
public void BeginSynchronizedOutput() => _vt100Output.BeginSynchronizedOutput();
public void EndSynchronizedOutput() => _vt100Output.EndSynchronizedOutput();
```
