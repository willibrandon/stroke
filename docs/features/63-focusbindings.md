# Feature 63: Focus Bindings

## Overview

Implement the focus navigation bindings and CPR (Cursor Position Request) response handling bindings.

## Python Prompt Toolkit Reference

**Source:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/focus.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/cpr.py`

## Public API

### Focus Functions

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class FocusFunctions
{
    /// <summary>
    /// Focus the next visible Window.
    /// Often bound to the Tab key.
    /// </summary>
    public static void FocusNext(KeyPressEvent @event);

    /// <summary>
    /// Focus the previous visible Window.
    /// Often bound to the BackTab (Shift+Tab) key.
    /// </summary>
    public static void FocusPrevious(KeyPressEvent @event);
}
```

### CPR Bindings

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class CprBindings
{
    /// <summary>
    /// Load key bindings for handling CPR (Cursor Position Request) responses.
    /// </summary>
    public static KeyBindings LoadCprBindings();
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Bindings/
        ├── FocusFunctions.cs
        └── CprBindings.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        ├── FocusFunctionsTests.cs
        └── CprBindingsTests.cs
```

## Implementation Notes

### Focus Functions

```csharp
public static void FocusNext(KeyPressEvent @event)
{
    @event.App.Layout.FocusNext();
}

public static void FocusPrevious(KeyPressEvent @event)
{
    @event.App.Layout.FocusPrevious();
}
```

### CPR Response Handler

The CPR response arrives as a key press with the format `\x1b[<row>;<col>R`:

```csharp
public static KeyBindings LoadCprBindings()
{
    var bindings = new KeyBindings();

    // Handle CPR response, don't save before (no undo needed)
    bindings.Add(Keys.CprResponse, e =>
    {
        // Data looks like "\x1b[35;1R"
        // Parse row/col information
        var data = e.Data;
        var content = data.Substring(2, data.Length - 3); // Remove ESC[ and R
        var parts = content.Split(';');
        var row = int.Parse(parts[0]);
        var col = int.Parse(parts[1]);

        // Report absolute cursor position to the renderer
        e.App.Renderer.ReportAbsoluteCursorRow(row);
    }, saveBefore: e => false);

    return bindings;
}
```

### Integration with Layout

The `Layout` class implements focus traversal:

```csharp
// In Layout class
public void FocusNext()
{
    var windows = GetVisibleFocusableWindows().ToList();
    if (windows.Count == 0) return;

    var currentIndex = windows.IndexOf(CurrentWindow);
    var nextIndex = (currentIndex + 1) % windows.Count;
    Focus(windows[nextIndex]);
}

public void FocusPrevious()
{
    var windows = GetVisibleFocusableWindows().ToList();
    if (windows.Count == 0) return;

    var currentIndex = windows.IndexOf(CurrentWindow);
    var prevIndex = (currentIndex - 1 + windows.Count) % windows.Count;
    Focus(windows[prevIndex]);
}
```

### Common Focus Binding Registration

Applications typically register focus bindings like this:

```csharp
// In application setup
bindings.Add("tab", FocusFunctions.FocusNext,
    filter: ~Filters.HasCompletion);
bindings.Add("s-tab", FocusFunctions.FocusPrevious,
    filter: ~Filters.HasCompletion);
```

## Dependencies

- `Stroke.KeyBinding.KeyBindings` (Feature 19) - KeyBindings class
- `Stroke.Layout.Layout` (Feature 29) - Layout with focus management
- `Stroke.Rendering.Renderer` (Feature 57) - Renderer for CPR handling
- `Stroke.Input.Keys` (Feature 03) - Key constants

## Implementation Tasks

1. Implement `FocusNext` function
2. Implement `FocusPrevious` function
3. Implement `LoadCprBindings` method
4. Implement CPR response parsing
5. Connect CPR response to renderer
6. Write comprehensive unit tests

## Acceptance Criteria

- [ ] FocusNext cycles to next focusable window
- [ ] FocusPrevious cycles to previous focusable window
- [ ] Focus wraps around at ends of window list
- [ ] CPR response is correctly parsed
- [ ] Row number is reported to renderer
- [ ] CPR binding does not create undo point
- [ ] Unit tests achieve 80% coverage
