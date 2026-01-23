# Feature 03: Clipboard System

## Overview

Implement the clipboard abstraction and implementations for storing and retrieving text with selection type information.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/clipboard/base.py`
**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/clipboard/in_memory.py`

## Public API

### ClipboardData Class

```csharp
namespace Stroke.Core.Clipboard;

/// <summary>
/// Text on the clipboard with selection type information.
/// </summary>
public sealed class ClipboardData
{
    /// <summary>
    /// Creates clipboard data.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <param name="type">The selection type.</param>
    public ClipboardData(string text = "", SelectionType type = SelectionType.Characters);

    /// <summary>
    /// The text content.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// The selection type.
    /// </summary>
    public SelectionType Type { get; }
}
```

### IClipboard Interface (Abstract Base)

```csharp
namespace Stroke.Core.Clipboard;

/// <summary>
/// Abstract base for clipboards. An implementation can be in memory,
/// can share the X11 or Windows clipboard, or can be persistent.
/// </summary>
public interface IClipboard
{
    /// <summary>
    /// Set data to the clipboard.
    /// </summary>
    /// <param name="data">The clipboard data to set.</param>
    void SetData(ClipboardData data);

    /// <summary>
    /// Shortcut for setting plain text on clipboard.
    /// </summary>
    /// <param name="text">The text to set.</param>
    void SetText(string text);

    /// <summary>
    /// For Emacs mode, rotate the kill ring.
    /// </summary>
    void Rotate();

    /// <summary>
    /// Return clipboard data.
    /// </summary>
    /// <returns>The current clipboard data.</returns>
    ClipboardData GetData();
}
```

### DummyClipboard Class

```csharp
namespace Stroke.Core.Clipboard;

/// <summary>
/// Clipboard implementation that doesn't remember anything.
/// </summary>
public sealed class DummyClipboard : IClipboard
{
    public void SetData(ClipboardData data);
    public void SetText(string text);
    public void Rotate();
    public ClipboardData GetData();
}
```

### InMemoryClipboard Class

```csharp
namespace Stroke.Core.Clipboard;

/// <summary>
/// Default clipboard implementation that stores data in memory.
/// Implements a kill ring for Emacs-style yank-pop operations.
/// </summary>
public sealed class InMemoryClipboard : IClipboard
{
    /// <summary>
    /// Creates an in-memory clipboard with the specified kill ring size.
    /// </summary>
    /// <param name="maxSize">Maximum number of items in the kill ring (default: 60).</param>
    public InMemoryClipboard(int maxSize = 60);

    public void SetData(ClipboardData data);
    public void SetText(string text);
    public void Rotate();
    public ClipboardData GetData();
}
```

### DynamicClipboard Class

```csharp
namespace Stroke.Core.Clipboard;

/// <summary>
/// Clipboard class that dynamically returns any Clipboard.
/// </summary>
public sealed class DynamicClipboard : IClipboard
{
    /// <summary>
    /// Creates a dynamic clipboard.
    /// </summary>
    /// <param name="getClipboard">Function that returns the actual clipboard to use.</param>
    public DynamicClipboard(Func<IClipboard?> getClipboard);

    public void SetData(ClipboardData data);
    public void SetText(string text);
    public void Rotate();
    public ClipboardData GetData();
}
```

## Project Structure

```
src/Stroke/
└── Core/
    └── Clipboard/
        ├── ClipboardData.cs
        ├── IClipboard.cs
        ├── DummyClipboard.cs
        ├── InMemoryClipboard.cs
        └── DynamicClipboard.cs
tests/Stroke.Tests/
└── Core/
    └── Clipboard/
        ├── ClipboardDataTests.cs
        ├── DummyClipboardTests.cs
        ├── InMemoryClipboardTests.cs
        └── DynamicClipboardTests.cs
```

## Implementation Notes

### Kill Ring

The `InMemoryClipboard` implements an Emacs-style kill ring. The `Rotate()` method cycles through previous clipboard entries, enabling the yank-pop functionality.

### DynamicClipboard Fallback

When `getClipboard` returns `null`, `DynamicClipboard` falls back to a `DummyClipboard`.

## Dependencies

- `Stroke.Core.Selection` (SelectionType from Feature 02)

## Implementation Tasks

1. Implement `ClipboardData` class
2. Implement `IClipboard` interface
3. Implement `DummyClipboard` class
4. Implement `InMemoryClipboard` with kill ring
5. Implement `DynamicClipboard` class
6. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All clipboard types match Python Prompt Toolkit semantics
- [ ] Kill ring rotation works correctly
- [ ] DynamicClipboard fallback works correctly
- [ ] Unit tests achieve 80% coverage
