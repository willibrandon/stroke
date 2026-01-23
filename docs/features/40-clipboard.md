# Feature 40: Clipboard System

## Overview

Implement the clipboard system including the abstract Clipboard class, ClipboardData, and implementations for in-memory storage with kill ring support.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/clipboard/base.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/clipboard/in_memory.py`

## Public API

### ClipboardData Class

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Text on the clipboard with selection type information.
/// </summary>
public sealed class ClipboardData
{
    /// <summary>
    /// Creates ClipboardData.
    /// </summary>
    /// <param name="text">The clipboard text.</param>
    /// <param name="type">The selection type.</param>
    public ClipboardData(
        string text = "",
        SelectionType type = SelectionType.Characters);

    /// <summary>
    /// The clipboard text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// The selection type (Characters, Lines, Block).
    /// </summary>
    public SelectionType Type { get; }
}
```

### Clipboard Abstract Class

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Abstract base class for clipboard implementations.
/// Implementations can be in-memory, system clipboard, or persistent.
/// </summary>
public abstract class Clipboard
{
    /// <summary>
    /// Set data on the clipboard.
    /// </summary>
    /// <param name="data">The ClipboardData to store.</param>
    public abstract void SetData(ClipboardData data);

    /// <summary>
    /// Shortcut for setting plain text on clipboard.
    /// Creates ClipboardData with Characters selection type.
    /// </summary>
    /// <param name="text">The text to store.</param>
    public virtual void SetText(string text);

    /// <summary>
    /// Rotate the kill ring (for Emacs mode).
    /// Default implementation does nothing.
    /// </summary>
    public virtual void Rotate();

    /// <summary>
    /// Get data from the clipboard.
    /// </summary>
    /// <returns>The current ClipboardData.</returns>
    public abstract ClipboardData GetData();
}
```

### InMemoryClipboard Class

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Default clipboard implementation that keeps data in memory.
/// Implements a kill ring for Emacs mode.
/// </summary>
public sealed class InMemoryClipboard : Clipboard
{
    /// <summary>
    /// Creates an InMemoryClipboard.
    /// </summary>
    /// <param name="data">Optional initial data.</param>
    /// <param name="maxSize">Maximum kill ring size (default 60).</param>
    public InMemoryClipboard(
        ClipboardData? data = null,
        int maxSize = 60);

    /// <summary>
    /// Maximum size of the kill ring.
    /// </summary>
    public int MaxSize { get; }

    public override void SetData(ClipboardData data);

    public override ClipboardData GetData();

    /// <summary>
    /// Rotate the kill ring.
    /// Moves the first item to the end.
    /// </summary>
    public override void Rotate();
}
```

### DummyClipboard Class

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Clipboard implementation that doesn't remember anything.
/// </summary>
public sealed class DummyClipboard : Clipboard
{
    public override void SetData(ClipboardData data);

    public override void SetText(string text);

    public override void Rotate();

    public override ClipboardData GetData();
}
```

### DynamicClipboard Class

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Clipboard that dynamically returns any Clipboard.
/// </summary>
public sealed class DynamicClipboard : Clipboard
{
    /// <summary>
    /// Creates a DynamicClipboard.
    /// </summary>
    /// <param name="getClipboard">Callable that returns a Clipboard instance.</param>
    public DynamicClipboard(Func<Clipboard?> getClipboard);

    public override void SetData(ClipboardData data);

    public override void SetText(string text);

    public override void Rotate();

    public override ClipboardData GetData();
}
```

## Project Structure

```
src/Stroke/
└── Clipboard/
    ├── ClipboardData.cs
    ├── Clipboard.cs
    ├── InMemoryClipboard.cs
    ├── DummyClipboard.cs
    └── DynamicClipboard.cs
tests/Stroke.Tests/
└── Clipboard/
    ├── ClipboardDataTests.cs
    ├── InMemoryClipboardTests.cs
    └── DynamicClipboardTests.cs
```

## Implementation Notes

### Kill Ring

The kill ring is a circular buffer of clipboard entries:
1. Each `SetData` adds to the front
2. `GetData` returns the front item
3. `Rotate` moves front to back
4. Size is capped at `MaxSize` (oldest removed)

This enables Emacs yank-pop (M-y) behavior:
1. C-y yanks most recent kill
2. M-y cycles through previous kills
3. Each M-y calls `Rotate()` then `GetData()`

### Selection Type Preservation

`ClipboardData` preserves the selection type:
- `Characters`: Normal text selection
- `Lines`: Line-wise selection (Vi: dd, yy)
- `Block`: Block/column selection (Vi: Ctrl-V)

Paste behavior differs by type:
- Characters: Insert inline
- Lines: Insert above/below current line
- Block: Insert as rectangular block

### DummyClipboard

Returns empty `ClipboardData` from `GetData()`:
- `SetData` does nothing
- `SetText` does nothing
- `Rotate` does nothing
- `GetData` returns `new ClipboardData()`

Used when clipboard functionality should be disabled.

### DynamicClipboard

Delegates to dynamically chosen clipboard:
- `getClipboard()` called on each operation
- If returns null, uses `DummyClipboard`
- Enables runtime clipboard switching

Use case: Switch between in-memory and system clipboard based on configuration.

### Thread Safety

InMemoryClipboard should be thread-safe:
- Kill ring operations may occur from multiple threads
- Use locking or concurrent collection

## Dependencies

- `Stroke.Selection.SelectionType` (Feature 02) - Selection type enum

## Implementation Tasks

1. Implement `ClipboardData` class
2. Implement `Clipboard` abstract base class
3. Implement `InMemoryClipboard` with kill ring
4. Implement `DummyClipboard` class
5. Implement `DynamicClipboard` class
6. Ensure thread safety for InMemoryClipboard
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] ClipboardData stores text and selection type
- [ ] InMemoryClipboard implements kill ring correctly
- [ ] Rotate moves front item to back
- [ ] MaxSize limits kill ring size
- [ ] DummyClipboard returns empty data
- [ ] DynamicClipboard delegates correctly
- [ ] Thread safety maintained
- [ ] Unit tests achieve 80% coverage
