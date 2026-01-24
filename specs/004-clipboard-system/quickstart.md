# Quickstart: Clipboard System

**Feature**: 004-clipboard-system
**Date**: 2026-01-23

## Overview

The Clipboard System provides abstractions for storing and retrieving text with selection type information, supporting Emacs-style kill ring operations.

## Basic Usage

### Store and Retrieve Text

```csharp
using Stroke.Core;

// Create an in-memory clipboard
IClipboard clipboard = new InMemoryClipboard();

// Store text using convenience method
clipboard.SetText("Hello, World!");

// Retrieve the data
ClipboardData data = clipboard.GetData();
Console.WriteLine(data.Text);  // "Hello, World!"
Console.WriteLine(data.Type);  // Characters
```

### Store with Selection Type

```csharp
using Stroke.Core;

IClipboard clipboard = new InMemoryClipboard();

// Store line-wise selection (like Vi's yy/dd)
clipboard.SetData(new ClipboardData("line1\nline2\n", SelectionType.Lines));

// Store block selection (like Vi's Ctrl-V)
clipboard.SetData(new ClipboardData("ABC\nDEF\nGHI", SelectionType.Block));

// Retrieve preserves selection type
ClipboardData data = clipboard.GetData();
Console.WriteLine(data.Type);  // Block
```

### Kill Ring (Emacs Yank-Pop)

```csharp
using Stroke.Core;

IClipboard clipboard = new InMemoryClipboard();

// Build up kill ring
clipboard.SetText("first");
clipboard.SetText("second");
clipboard.SetText("third");

// Most recent is returned
Console.WriteLine(clipboard.GetData().Text);  // "third"

// Rotate to access previous kills
clipboard.Rotate();
Console.WriteLine(clipboard.GetData().Text);  // "second"

clipboard.Rotate();
Console.WriteLine(clipboard.GetData().Text);  // "first"

// Full rotation cycles back
clipboard.Rotate();
Console.WriteLine(clipboard.GetData().Text);  // "third"
```

### Custom Kill Ring Size

```csharp
using Stroke.Core;

// Create clipboard with small kill ring
IClipboard clipboard = new InMemoryClipboard(maxSize: 3);

clipboard.SetText("a");
clipboard.SetText("b");
clipboard.SetText("c");
clipboard.SetText("d");  // "a" is dropped (oldest)

// Only 3 items retained: [d, c, b]
```

### Initial Data

```csharp
using Stroke.Core;

// Initialize with data
var initialData = new ClipboardData("initial text", SelectionType.Lines);
IClipboard clipboard = new InMemoryClipboard(data: initialData);

Console.WriteLine(clipboard.GetData().Text);  // "initial text"
```

## Dynamic Clipboard

Use `DynamicClipboard` to switch implementations at runtime:

```csharp
using Stroke.Core;

IClipboard? activeClipboard = new InMemoryClipboard();

// Create dynamic wrapper
IClipboard dynamic = new DynamicClipboard(() => activeClipboard);

dynamic.SetText("stored in InMemoryClipboard");

// Switch implementation
activeClipboard = new DummyClipboard();

// Now operations go to dummy (discarded)
dynamic.SetText("this is discarded");

// When null, falls back to dummy behavior
activeClipboard = null;
Console.WriteLine(dynamic.GetData().Text);  // "" (empty)
```

## Dummy Clipboard

Use `DummyClipboard` when clipboard functionality should be disabled:

```csharp
using Stroke.Core;

IClipboard clipboard = new DummyClipboard();

clipboard.SetText("ignored");
clipboard.SetData(new ClipboardData("also ignored", SelectionType.Lines));

// Always returns empty data
ClipboardData data = clipboard.GetData();
Console.WriteLine(data.Text);  // ""
Console.WriteLine(data.Type);  // Characters
```

## Integration with Editor Operations

### Copy Selection

```csharp
public void CopySelection(IClipboard clipboard, string selectedText, SelectionType type)
{
    clipboard.SetData(new ClipboardData(selectedText, type));
}
```

### Cut Selection

```csharp
public string CutSelection(IClipboard clipboard, string selectedText, SelectionType type)
{
    clipboard.SetData(new ClipboardData(selectedText, type));
    return selectedText;  // Return text for undo stack
}
```

### Paste

```csharp
public ClipboardData GetPasteData(IClipboard clipboard)
{
    return clipboard.GetData();
}
```

### Emacs Yank-Pop (M-y)

```csharp
public ClipboardData YankPop(IClipboard clipboard)
{
    clipboard.Rotate();
    return clipboard.GetData();
}
```

## API Reference

### ClipboardData

| Member | Description |
|--------|-------------|
| `ClipboardData(string text = "", SelectionType type = Characters)` | Constructor |
| `string Text { get; }` | The stored text |
| `SelectionType Type { get; }` | The selection type |

### IClipboard

| Member | Description |
|--------|-------------|
| `void SetData(ClipboardData data)` | Store clipboard data |
| `ClipboardData GetData()` | Retrieve current data |
| `void SetText(string text)` | Shortcut for plain text |
| `void Rotate()` | Rotate kill ring |

### InMemoryClipboard

| Member | Description |
|--------|-------------|
| `InMemoryClipboard(ClipboardData? data = null, int maxSize = 60)` | Constructor |
| `int MaxSize { get; }` | Kill ring capacity |

### DynamicClipboard

| Member | Description |
|--------|-------------|
| `DynamicClipboard(Func<IClipboard?> getClipboard)` | Constructor |

### DummyClipboard

| Member | Description |
|--------|-------------|
| `DummyClipboard()` | Default constructor |
