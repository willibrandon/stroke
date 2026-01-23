# Feature 79: Clipboard

## Overview

Implement clipboard functionality for copy/paste operations with support for selection types and kill ring (Emacs-style).

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/clipboard/`

## Public API

### ClipboardData

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Data stored on the clipboard.
/// </summary>
/// <param name="Text">The clipboard text content.</param>
/// <param name="Type">The selection type (Characters, Lines, Block).</param>
public sealed record ClipboardData(
    string Text = "",
    SelectionType Type = SelectionType.Characters
);
```

### Clipboard Abstract Base

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Abstract base class for clipboard implementations.
/// Implementations can be in-memory, system clipboard, or persistent.
/// </summary>
public abstract class Clipboard
{
    /// <summary>
    /// Set data to the clipboard.
    /// </summary>
    /// <param name="data">The clipboard data to store.</param>
    public abstract void SetData(ClipboardData data);

    /// <summary>
    /// Set plain text to the clipboard.
    /// </summary>
    /// <param name="text">The text to store.</param>
    public virtual void SetText(string text) => SetData(new ClipboardData(text));

    /// <summary>
    /// Rotate the kill ring (for Emacs mode).
    /// </summary>
    public virtual void Rotate() { }

    /// <summary>
    /// Get data from the clipboard.
    /// </summary>
    /// <returns>The current clipboard data.</returns>
    public abstract ClipboardData GetData();
}
```

### InMemoryClipboard

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Default clipboard implementation that stores data in memory.
/// Implements a kill ring for Emacs mode.
/// </summary>
public sealed class InMemoryClipboard : Clipboard
{
    /// <summary>
    /// Maximum number of items in the kill ring.
    /// </summary>
    public int MaxSize { get; }

    /// <summary>
    /// Create an in-memory clipboard.
    /// </summary>
    /// <param name="data">Initial clipboard data.</param>
    /// <param name="maxSize">Maximum kill ring size (default: 60).</param>
    public InMemoryClipboard(ClipboardData? data = null, int maxSize = 60);

    /// <inheritdoc/>
    public override void SetData(ClipboardData data);

    /// <inheritdoc/>
    public override ClipboardData GetData();

    /// <inheritdoc/>
    public override void Rotate();
}
```

### DummyClipboard

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Clipboard implementation that doesn't store anything.
/// </summary>
public sealed class DummyClipboard : Clipboard
{
    /// <inheritdoc/>
    public override void SetData(ClipboardData data) { }

    /// <inheritdoc/>
    public override void SetText(string text) { }

    /// <inheritdoc/>
    public override void Rotate() { }

    /// <inheritdoc/>
    public override ClipboardData GetData() => new();
}
```

### DynamicClipboard

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Clipboard that dynamically returns another clipboard.
/// </summary>
public sealed class DynamicClipboard : Clipboard
{
    /// <summary>
    /// Create a dynamic clipboard.
    /// </summary>
    /// <param name="getClipboard">Callback to get the actual clipboard.</param>
    public DynamicClipboard(Func<Clipboard?> getClipboard);

    /// <inheritdoc/>
    public override void SetData(ClipboardData data);

    /// <inheritdoc/>
    public override void SetText(string text);

    /// <inheritdoc/>
    public override void Rotate();

    /// <inheritdoc/>
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
    └── ClipboardTests.cs
```

## Implementation Notes

### Kill Ring Implementation

The kill ring stores multiple clipboard entries and allows rotating through them:

```csharp
public sealed class InMemoryClipboard : Clipboard
{
    private readonly LinkedList<ClipboardData> _ring = new();
    private readonly int _maxSize;

    public InMemoryClipboard(ClipboardData? data = null, int maxSize = 60)
    {
        if (maxSize < 1)
            throw new ArgumentOutOfRangeException(nameof(maxSize));

        _maxSize = maxSize;
        if (data != null)
            SetData(data);
    }

    public override void SetData(ClipboardData data)
    {
        _ring.AddFirst(data);

        while (_ring.Count > _maxSize)
            _ring.RemoveLast();
    }

    public override ClipboardData GetData()
    {
        return _ring.First?.Value ?? new ClipboardData();
    }

    public override void Rotate()
    {
        if (_ring.Count > 0)
        {
            var first = _ring.First!.Value;
            _ring.RemoveFirst();
            _ring.AddLast(first);
        }
    }
}
```

### Integration with Buffer

The clipboard is used by Buffer for cut/copy/paste operations:

```csharp
// In Buffer
public void CutSelection()
{
    if (SelectionState == null) return;

    var (startPos, endPos) = Document.SelectionRange(SelectionState);
    var selectedText = Document.Text.Substring(startPos, endPos - startPos);

    Clipboard.SetData(new ClipboardData(selectedText, SelectionState.Type));
    Document = new Document(
        Document.Text.Remove(startPos, endPos - startPos),
        startPos);
    SelectionState = null;
}

public void Paste(PasteMode mode = PasteMode.Emacs)
{
    var data = Clipboard.GetData();
    // Handle paste based on mode and selection type
}
```

## Dependencies

- Feature 80: Selection types (SelectionType, PasteMode)

## Implementation Tasks

1. Implement `ClipboardData` record
2. Implement `Clipboard` abstract base class
3. Implement `InMemoryClipboard` with kill ring
4. Implement `DummyClipboard`
5. Implement `DynamicClipboard`
6. Integrate clipboard with Buffer
7. Write unit tests

## Acceptance Criteria

- [ ] ClipboardData stores text and selection type
- [ ] InMemoryClipboard stores items in kill ring
- [ ] Rotate() cycles through kill ring
- [ ] MaxSize limit is enforced
- [ ] DummyClipboard returns empty data
- [ ] DynamicClipboard delegates to callback
- [ ] Unit tests achieve 80% coverage
