# Feature 61: Scroll Bindings

## Overview

Implement the scroll key bindings for navigating through long multiline buffers, including page up/down, half-page scrolling, and single-line scrolling.

## Python Prompt Toolkit Reference

**Source:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/scroll.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/page_navigation.py`

## Public API

### Scroll Functions

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class ScrollFunctions
{
    /// <summary>
    /// Scroll window down (forward).
    /// </summary>
    /// <param name="event">Key press event.</param>
    /// <param name="half">If true, scroll half a page.</param>
    public static void ScrollForward(KeyPressEvent @event, bool half = false);

    /// <summary>
    /// Scroll window up (backward).
    /// </summary>
    /// <param name="event">Key press event.</param>
    /// <param name="half">If true, scroll half a page.</param>
    public static void ScrollBackward(KeyPressEvent @event, bool half = false);

    /// <summary>
    /// Scroll down half a page.
    /// </summary>
    public static void ScrollHalfPageDown(KeyPressEvent @event);

    /// <summary>
    /// Scroll up half a page.
    /// </summary>
    public static void ScrollHalfPageUp(KeyPressEvent @event);

    /// <summary>
    /// Scroll down one line.
    /// </summary>
    public static void ScrollOneLineDown(KeyPressEvent @event);

    /// <summary>
    /// Scroll up one line.
    /// </summary>
    public static void ScrollOneLineUp(KeyPressEvent @event);

    /// <summary>
    /// Scroll page down (cursor at top after scroll).
    /// </summary>
    public static void ScrollPageDown(KeyPressEvent @event);

    /// <summary>
    /// Scroll page up (cursor at bottom after scroll).
    /// </summary>
    public static void ScrollPageUp(KeyPressEvent @event);
}
```

### Page Navigation Bindings

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class PageNavigationBindings
{
    /// <summary>
    /// Load both Vi and Emacs page navigation bindings.
    /// </summary>
    public static KeyBindingsBase LoadPageNavigationBindings();

    /// <summary>
    /// Load Emacs page navigation bindings.
    /// </summary>
    public static KeyBindingsBase LoadEmacsPageNavigationBindings();

    /// <summary>
    /// Load Vi page navigation bindings.
    /// </summary>
    public static KeyBindingsBase LoadViPageNavigationBindings();
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Bindings/
        ├── ScrollFunctions.cs
        └── PageNavigationBindings.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        ├── ScrollFunctionsTests.cs
        └── PageNavigationBindingsTests.cs
```

## Implementation Notes

### ScrollForward Implementation

```csharp
public static void ScrollForward(KeyPressEvent @event, bool half = false)
{
    var window = @event.App.Layout.CurrentWindow;
    var buffer = @event.App.CurrentBuffer;

    if (window?.RenderInfo == null) return;

    var info = window.RenderInfo;
    var uiContent = info.UiContent;

    // Calculate scroll height
    var scrollHeight = info.WindowHeight;
    if (half)
        scrollHeight /= 2;

    // Calculate how many lines equal that vertical space
    var y = buffer.Document.CursorPositionRow + 1;
    var height = 0;

    while (y < uiContent.LineCount)
    {
        var lineHeight = info.GetHeightForLine(y);

        if (height + lineHeight < scrollHeight)
        {
            height += lineHeight;
            y++;
        }
        else
        {
            break;
        }
    }

    buffer.CursorPosition = buffer.Document.TranslateRowColToIndex(y, 0);
}
```

### ScrollBackward Implementation

```csharp
public static void ScrollBackward(KeyPressEvent @event, bool half = false)
{
    var window = @event.App.Layout.CurrentWindow;
    var buffer = @event.App.CurrentBuffer;

    if (window?.RenderInfo == null) return;

    var info = window.RenderInfo;

    // Calculate scroll height
    var scrollHeight = info.WindowHeight;
    if (half)
        scrollHeight /= 2;

    // Calculate how many lines equal that vertical space
    var y = Math.Max(0, buffer.Document.CursorPositionRow - 1);
    var height = 0;

    while (y > 0)
    {
        var lineHeight = info.GetHeightForLine(y);

        if (height + lineHeight < scrollHeight)
        {
            height += lineHeight;
            y--;
        }
        else
        {
            break;
        }
    }

    buffer.CursorPosition = buffer.Document.TranslateRowColToIndex(y, 0);
}
```

### Half-Page Scroll

```csharp
public static void ScrollHalfPageDown(KeyPressEvent @event)
{
    ScrollForward(@event, half: true);
}

public static void ScrollHalfPageUp(KeyPressEvent @event)
{
    ScrollBackward(@event, half: true);
}
```

### Single Line Scroll

```csharp
public static void ScrollOneLineDown(KeyPressEvent @event)
{
    var window = @event.App.Layout.CurrentWindow;
    var buffer = @event.App.CurrentBuffer;

    if (window?.RenderInfo == null) return;

    var info = window.RenderInfo;

    // Check if we can scroll further
    if (window.VerticalScroll < info.ContentHeight - info.WindowHeight)
    {
        // Move cursor if at top scroll offset
        if (info.CursorPosition.Y <= info.ConfiguredScrollOffsets.Top)
        {
            buffer.CursorPosition += buffer.Document.GetCursorDownPosition();
        }

        window.VerticalScroll++;
    }
}

public static void ScrollOneLineUp(KeyPressEvent @event)
{
    var window = @event.App.Layout.CurrentWindow;
    var buffer = @event.App.CurrentBuffer;

    if (window?.RenderInfo == null) return;

    var info = window.RenderInfo;

    if (window.VerticalScroll > 0)
    {
        var firstLineHeight = info.GetHeightForLine(info.FirstVisibleLine());

        var cursorUp = info.CursorPosition.Y -
            (info.WindowHeight - 1 - firstLineHeight - info.ConfiguredScrollOffsets.Bottom);

        // Move cursor up
        for (var i = 0; i < Math.Max(0, cursorUp); i++)
        {
            buffer.CursorPosition += buffer.Document.GetCursorUpPosition();
        }

        window.VerticalScroll--;
    }
}
```

### Page Up/Down

```csharp
public static void ScrollPageDown(KeyPressEvent @event)
{
    var window = @event.App.Layout.CurrentWindow;
    var buffer = @event.App.CurrentBuffer;

    if (window?.RenderInfo == null) return;

    // Scroll down one page
    var lineIndex = Math.Max(
        window.RenderInfo.LastVisibleLine(),
        window.VerticalScroll + 1);
    window.VerticalScroll = lineIndex;

    buffer.CursorPosition = buffer.Document.TranslateRowColToIndex(lineIndex, 0);
    buffer.CursorPosition += buffer.Document.GetStartOfLinePosition(afterWhitespace: true);
}

public static void ScrollPageUp(KeyPressEvent @event)
{
    var window = @event.App.Layout.CurrentWindow;
    var buffer = @event.App.CurrentBuffer;

    if (window?.RenderInfo == null) return;

    // Put cursor at first visible line (ensure at least one line movement)
    var lineIndex = Math.Max(0, Math.Min(
        window.RenderInfo.FirstVisibleLine(),
        buffer.Document.CursorPositionRow - 1));

    buffer.CursorPosition = buffer.Document.TranslateRowColToIndex(lineIndex, 0);
    buffer.CursorPosition += buffer.Document.GetStartOfLinePosition(afterWhitespace: true);

    // Reset scroll offset
    window.VerticalScroll = 0;
}
```

### Emacs Page Navigation Bindings

```csharp
public static KeyBindingsBase LoadEmacsPageNavigationBindings()
{
    var bindings = new KeyBindings();

    bindings.Add("c-v", ScrollPageDown);
    bindings.Add("pagedown", ScrollPageDown);
    bindings.Add("escape", "v", ScrollPageUp);
    bindings.Add("pageup", ScrollPageUp);

    return new ConditionalKeyBindings(bindings, Filters.EmacsMode);
}
```

### Vi Page Navigation Bindings

```csharp
public static KeyBindingsBase LoadViPageNavigationBindings()
{
    var bindings = new KeyBindings();

    bindings.Add("c-f", ScrollForward);
    bindings.Add("c-b", ScrollBackward);
    bindings.Add("c-d", ScrollHalfPageDown);
    bindings.Add("c-u", ScrollHalfPageUp);
    bindings.Add("c-e", ScrollOneLineDown);
    bindings.Add("c-y", ScrollOneLineUp);
    bindings.Add("pagedown", ScrollPageDown);
    bindings.Add("pageup", ScrollPageUp);

    return new ConditionalKeyBindings(bindings, Filters.ViMode);
}
```

### Combined Navigation Bindings

```csharp
public static KeyBindingsBase LoadPageNavigationBindings()
{
    // Only enable when a Buffer is focused
    return new ConditionalKeyBindings(
        KeyBindings.Merge(
            LoadEmacsPageNavigationBindings(),
            LoadViPageNavigationBindings()),
        Filters.BufferHasFocus);
}
```

## Dependencies

- `Stroke.KeyBinding.KeyBindings` (Feature 19) - KeyBindings class
- `Stroke.Layout.Window` (Feature 25) - Window with RenderInfo
- `Stroke.Core.Buffer` (Feature 06) - Buffer operations
- `Stroke.Core.Document` (Feature 01) - Document navigation
- `Stroke.Filters` (Feature 12) - Filter conditions

## Implementation Tasks

1. Implement `ScrollForward` function
2. Implement `ScrollBackward` function
3. Implement `ScrollHalfPageDown` function
4. Implement `ScrollHalfPageUp` function
5. Implement `ScrollOneLineDown` function
6. Implement `ScrollOneLineUp` function
7. Implement `ScrollPageDown` function
8. Implement `ScrollPageUp` function
9. Implement `LoadEmacsPageNavigationBindings`
10. Implement `LoadViPageNavigationBindings`
11. Implement `LoadPageNavigationBindings`
12. Write comprehensive unit tests

## Acceptance Criteria

- [ ] ScrollForward moves cursor down by page
- [ ] ScrollBackward moves cursor up by page
- [ ] Half-page scroll moves by half window height
- [ ] Single line scroll adjusts vertical offset
- [ ] PageDown positions cursor at top after scroll
- [ ] PageUp positions cursor at bottom after scroll
- [ ] Emacs bindings use Ctrl-V/Escape-V
- [ ] Vi bindings use Ctrl-F/Ctrl-B/Ctrl-D/Ctrl-U/Ctrl-E/Ctrl-Y
- [ ] Bindings only active when buffer is focused
- [ ] Line heights are considered for variable-height content
- [ ] Unit tests achieve 80% coverage
