# Feature 45: Widgets

## Overview

Implement the reusable widget components for building full-screen applications including TextArea, Label, Button, Frame, Shadow, Box, RadioList, CheckboxList, ProgressBar, and line widgets.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/widgets/base.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/widgets/dialogs.py`

## Public API

### Border Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Box drawing characters (thin).
/// </summary>
public static class Border
{
    public const char Horizontal = '\u2500';
    public const char Vertical = '\u2502';
    public const char TopLeft = '\u250c';
    public const char TopRight = '\u2510';
    public const char BottomLeft = '\u2514';
    public const char BottomRight = '\u2518';
}
```

### TextArea Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// A simple input field with sane defaults.
/// Implements __pt_container__ for use in layouts.
/// </summary>
public sealed class TextArea : IContainer
{
    /// <summary>
    /// Creates a TextArea.
    /// </summary>
    public TextArea(
        string text = "",
        bool multiline = true,
        bool password = false,
        Lexer? lexer = null,
        AutoSuggest? autoSuggest = null,
        Completer? completer = null,
        bool completeWhileTyping = true,
        Validator? validator = null,
        BufferAcceptHandler? acceptHandler = null,
        History? history = null,
        bool focusable = true,
        bool focusOnClick = false,
        bool wrapLines = true,
        bool readOnly = false,
        AnyDimension? width = null,
        AnyDimension? height = null,
        bool dontExtendHeight = false,
        bool dontExtendWidth = false,
        bool lineNumbers = false,
        GetLinePrefixCallable? getLinePrefix = null,
        bool scrollbar = false,
        string style = "",
        SearchToolbar? searchField = null,
        bool previewSearch = true,
        AnyFormattedText prompt = default,
        IList<Processor>? inputProcessors = null,
        string name = "");

    /// <summary>
    /// The underlying buffer.
    /// </summary>
    public Buffer Buffer { get; }

    /// <summary>
    /// The buffer control.
    /// </summary>
    public BufferControl Control { get; }

    /// <summary>
    /// The window.
    /// </summary>
    public Window Window { get; }

    /// <summary>
    /// The buffer text.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// The buffer document.
    /// </summary>
    public Document Document { get; set; }

    /// <summary>
    /// The accept handler.
    /// </summary>
    public BufferAcceptHandler? AcceptHandler { get; set; }

    Container IContainer.GetContainer();
}
```

### Label Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Widget that displays text. Not editable or focusable.
/// </summary>
public sealed class Label : IContainer
{
    /// <summary>
    /// Creates a Label.
    /// </summary>
    public Label(
        AnyFormattedText text,
        string style = "",
        AnyDimension? width = null,
        bool dontExtendHeight = true,
        bool dontExtendWidth = false,
        WindowAlign align = WindowAlign.Left,
        bool wrapLines = true);

    /// <summary>
    /// The label text.
    /// </summary>
    public AnyFormattedText Text { get; set; }

    Container IContainer.GetContainer();
}
```

### Button Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Clickable button widget.
/// </summary>
public sealed class Button : IContainer
{
    /// <summary>
    /// Creates a Button.
    /// </summary>
    public Button(
        string text,
        Action? handler = null,
        int width = 12,
        string leftSymbol = "<",
        string rightSymbol = ">");

    /// <summary>
    /// The button caption.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// The left bracket symbol.
    /// </summary>
    public string LeftSymbol { get; }

    /// <summary>
    /// The right bracket symbol.
    /// </summary>
    public string RightSymbol { get; }

    /// <summary>
    /// Click handler.
    /// </summary>
    public Action? Handler { get; set; }

    /// <summary>
    /// Button width.
    /// </summary>
    public int Width { get; }

    Container IContainer.GetContainer();
}
```

### Frame Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Draw a border around any container with optional title.
/// </summary>
public sealed class Frame : IContainer
{
    /// <summary>
    /// Creates a Frame.
    /// </summary>
    public Frame(
        AnyContainer body,
        AnyFormattedText title = default,
        string style = "",
        AnyDimension? width = null,
        AnyDimension? height = null,
        KeyBindings? keyBindings = null,
        bool modal = false);

    /// <summary>
    /// The frame title.
    /// </summary>
    public AnyFormattedText Title { get; set; }

    /// <summary>
    /// The frame body.
    /// </summary>
    public AnyContainer Body { get; set; }

    Container IContainer.GetContainer();
}
```

### Shadow Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Draw a shadow underneath/behind this container.
/// </summary>
public sealed class Shadow : IContainer
{
    /// <summary>
    /// Creates a Shadow.
    /// </summary>
    public Shadow(AnyContainer body);

    Container IContainer.GetContainer();
}
```

### Box Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Add padding around a container.
/// </summary>
public sealed class Box : IContainer
{
    /// <summary>
    /// Creates a Box.
    /// </summary>
    public Box(
        AnyContainer body,
        AnyDimension? padding = null,
        AnyDimension? paddingLeft = null,
        AnyDimension? paddingRight = null,
        AnyDimension? paddingTop = null,
        AnyDimension? paddingBottom = null,
        AnyDimension? width = null,
        AnyDimension? height = null,
        string style = "",
        char? @char = null,
        bool modal = false,
        KeyBindings? keyBindings = null);

    /// <summary>
    /// The box body.
    /// </summary>
    public AnyContainer Body { get; set; }

    Container IContainer.GetContainer();
}
```

### RadioList Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// List of radio buttons. Only one can be checked at a time.
/// </summary>
public sealed class RadioList<T> : IContainer
{
    /// <summary>
    /// Creates a RadioList.
    /// </summary>
    public RadioList(
        IList<(T Value, AnyFormattedText Label)> values,
        T? @default = default,
        bool showNumbers = false,
        bool selectOnFocus = false,
        string openCharacter = "(",
        string selectCharacter = "*",
        string closeCharacter = ")",
        string containerStyle = "class:radio-list",
        string defaultStyle = "class:radio",
        string selectedStyle = "class:radio-selected",
        string checkedStyle = "class:radio-checked",
        string numberStyle = "class:radio-number",
        bool showCursor = true,
        bool showScrollbar = true);

    /// <summary>
    /// The list of values and labels.
    /// </summary>
    public IList<(T Value, AnyFormattedText Label)> Values { get; }

    /// <summary>
    /// The currently selected value.
    /// </summary>
    public T CurrentValue { get; set; }

    Container IContainer.GetContainer();
}
```

### CheckboxList Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// List of checkboxes. Multiple can be checked at a time.
/// </summary>
public sealed class CheckboxList<T> : IContainer
{
    /// <summary>
    /// Creates a CheckboxList.
    /// </summary>
    public CheckboxList(
        IList<(T Value, AnyFormattedText Label)> values,
        IList<T>? defaultValues = null,
        string openCharacter = "[",
        string selectCharacter = "*",
        string closeCharacter = "]",
        string containerStyle = "class:checkbox-list",
        string defaultStyle = "class:checkbox",
        string selectedStyle = "class:checkbox-selected",
        string checkedStyle = "class:checkbox-checked");

    /// <summary>
    /// The list of values and labels.
    /// </summary>
    public IList<(T Value, AnyFormattedText Label)> Values { get; }

    /// <summary>
    /// The currently checked values.
    /// </summary>
    public IList<T> CurrentValues { get; }

    Container IContainer.GetContainer();
}
```

### Checkbox Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Single checkbox (convenience wrapper around CheckboxList).
/// </summary>
public sealed class Checkbox : IContainer
{
    /// <summary>
    /// Creates a Checkbox.
    /// </summary>
    public Checkbox(AnyFormattedText text = default, bool @checked = false);

    /// <summary>
    /// Whether the checkbox is checked.
    /// </summary>
    public bool Checked { get; set; }

    Container IContainer.GetContainer();
}
```

### ProgressBar Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Progress bar widget.
/// </summary>
public sealed class ProgressBar : IContainer
{
    /// <summary>
    /// Creates a ProgressBar.
    /// </summary>
    public ProgressBar();

    /// <summary>
    /// The progress percentage (0-100).
    /// </summary>
    public int Percentage { get; set; }

    Container IContainer.GetContainer();
}
```

### VerticalLine Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// A simple vertical line with width of 1.
/// </summary>
public sealed class VerticalLine : IContainer
{
    public VerticalLine();

    Container IContainer.GetContainer();
}
```

### HorizontalLine Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// A simple horizontal line with height of 1.
/// </summary>
public sealed class HorizontalLine : IContainer
{
    public HorizontalLine();

    Container IContainer.GetContainer();
}
```

### Dialog Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Simple dialog window. Base for input, message, and confirmation dialogs.
/// </summary>
public sealed class Dialog : IContainer
{
    /// <summary>
    /// Creates a Dialog.
    /// </summary>
    public Dialog(
        AnyContainer body,
        AnyFormattedText title = default,
        IList<Button>? buttons = null,
        bool modal = true,
        AnyDimension? width = null,
        bool withBackground = false);

    /// <summary>
    /// The dialog body.
    /// </summary>
    public AnyContainer Body { get; set; }

    /// <summary>
    /// The dialog title.
    /// </summary>
    public AnyFormattedText Title { get; set; }

    Container IContainer.GetContainer();
}
```

## Project Structure

```
src/Stroke/
└── Widgets/
    ├── Border.cs
    ├── TextArea.cs
    ├── Label.cs
    ├── Button.cs
    ├── Frame.cs
    ├── Shadow.cs
    ├── Box.cs
    ├── RadioList.cs
    ├── CheckboxList.cs
    ├── Checkbox.cs
    ├── ProgressBar.cs
    ├── VerticalLine.cs
    ├── HorizontalLine.cs
    └── Dialog.cs
tests/Stroke.Tests/
└── Widgets/
    ├── TextAreaTests.cs
    ├── ButtonTests.cs
    ├── FrameTests.cs
    ├── RadioListTests.cs
    ├── CheckboxListTests.cs
    ├── ProgressBarTests.cs
    └── DialogTests.cs
```

## Implementation Notes

### IContainer Interface

All widgets implement `IContainer` (equivalent to `__pt_container__`):

```csharp
public interface IContainer
{
    Container GetContainer();
}
```

This allows widgets to be used anywhere a Container is expected.

### Button Key Bindings

- **Space** and **Enter**: Trigger click handler
- Mouse click: Trigger click handler

### RadioList/CheckboxList Key Bindings

- **Up/k**: Move selection up
- **Down/j**: Move selection down
- **PageUp/PageDown**: Page navigation
- **Space/Enter**: Toggle selection
- **1-9**: Quick select by number (if showNumbers)
- **Any char**: Jump to item starting with that char

### Frame Border Characters

```
┌─────────────────────────┐
│ Title │                 │
├─────────────────────────┤
│                         │
│      Body Content       │
│                         │
└─────────────────────────┘
```

### Shadow Effect

```
┌─────────────────────────┐
│                         │
│      Dialog             │░
│                         │░
└─────────────────────────┘░
  ░░░░░░░░░░░░░░░░░░░░░░░░░░
```

Shadow uses `class:shadow` style and floats offset by (-1, +1).

### ProgressBar Structure

```
┌────────────────────────────────────┐
│████████████░░░░░░░░░░░░░│ 45%      │
└────────────────────────────────────┘
```

- Uses FloatContainer to layer percentage label over bar
- Two VSplit windows: used (colored) and unused portions
- Width determined by percentage

## Dependencies

- `Stroke.Layout.Containers` (Feature 25) - Container classes
- `Stroke.Layout.Controls` (Feature 26) - Control classes
- `Stroke.Layout.Window` (Feature 27) - Window class
- `Stroke.Layout.Dimension` (Feature 24) - Dimension class
- `Stroke.Core.Buffer` (Feature 06) - Buffer class
- `Stroke.KeyBinding` (Feature 19) - Key bindings

## Implementation Tasks

1. Implement `Border` static class
2. Implement `IContainer` interface
3. Implement `TextArea` class
4. Implement `Label` class
5. Implement `Button` class with key bindings
6. Implement `Frame` class
7. Implement `Shadow` class
8. Implement `Box` class
9. Implement `_DialogList<T>` base class
10. Implement `RadioList<T>` class
11. Implement `CheckboxList<T>` class
12. Implement `Checkbox` class
13. Implement `ProgressBar` class
14. Implement `VerticalLine` and `HorizontalLine` classes
15. Implement `Dialog` class
16. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All widgets implement IContainer
- [ ] TextArea provides editable text input
- [ ] Label displays non-editable text
- [ ] Button handles click events
- [ ] Frame draws border with optional title
- [ ] Shadow creates offset shadow effect
- [ ] Box adds padding around content
- [ ] RadioList allows single selection
- [ ] CheckboxList allows multiple selection
- [ ] ProgressBar displays percentage
- [ ] Dialog combines widgets for dialogs
- [ ] All key bindings work correctly
- [ ] Unit tests achieve 80% coverage
