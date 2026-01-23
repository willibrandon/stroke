# Feature 86: Mouse Events

## Overview

Implement mouse event types, buttons, modifiers, and event data for handling mouse input in the terminal.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/mouse_events.py`

## Public API

### MouseEventType Enum

```csharp
namespace Stroke.Input;

/// <summary>
/// Type of mouse event.
/// </summary>
public enum MouseEventType
{
    /// <summary>
    /// Mouse button released (any button).
    /// </summary>
    MouseUp,

    /// <summary>
    /// Left mouse button pressed down.
    /// </summary>
    MouseDown,

    /// <summary>
    /// Scroll wheel moved up.
    /// </summary>
    ScrollUp,

    /// <summary>
    /// Scroll wheel moved down.
    /// </summary>
    ScrollDown,

    /// <summary>
    /// Mouse moved while left button held down.
    /// </summary>
    MouseMove
}
```

### MouseButton Enum

```csharp
namespace Stroke.Input;

/// <summary>
/// Mouse button identifier.
/// </summary>
public enum MouseButton
{
    /// <summary>Left mouse button.</summary>
    Left,

    /// <summary>Middle mouse button.</summary>
    Middle,

    /// <summary>Right mouse button.</summary>
    Right,

    /// <summary>No button (scrolling or moving without button).</summary>
    None,

    /// <summary>Unknown button (button pressed but unidentified).</summary>
    Unknown
}
```

### MouseModifier Enum

```csharp
namespace Stroke.Input;

/// <summary>
/// Keyboard modifier held during mouse event.
/// </summary>
[Flags]
public enum MouseModifier
{
    /// <summary>No modifier.</summary>
    None = 0,

    /// <summary>Shift key held.</summary>
    Shift = 1,

    /// <summary>Alt key held.</summary>
    Alt = 2,

    /// <summary>Control key held.</summary>
    Control = 4
}
```

### MouseEvent

```csharp
namespace Stroke.Input;

/// <summary>
/// Mouse event data sent to UIControl.HandleMouse.
/// </summary>
public sealed record MouseEvent
{
    /// <summary>
    /// Position of the mouse in the control's coordinate space.
    /// </summary>
    public Point Position { get; init; }

    /// <summary>
    /// Type of mouse event.
    /// </summary>
    public MouseEventType EventType { get; init; }

    /// <summary>
    /// Which button was pressed/released.
    /// </summary>
    public MouseButton Button { get; init; }

    /// <summary>
    /// Keyboard modifiers held during the event.
    /// </summary>
    public MouseModifier Modifiers { get; init; }

    /// <summary>
    /// Create a mouse event.
    /// </summary>
    public MouseEvent(
        Point position,
        MouseEventType eventType,
        MouseButton button,
        MouseModifier modifiers)
    {
        Position = position;
        EventType = eventType;
        Button = button;
        Modifiers = modifiers;
    }
}
```

## Project Structure

```
src/Stroke/
└── Input/
    ├── MouseEventType.cs
    ├── MouseButton.cs
    ├── MouseModifier.cs
    └── MouseEvent.cs
tests/Stroke.Tests/
└── Input/
    └── MouseEventTests.cs
```

## Implementation Notes

### Mouse Handler Integration

Mouse events flow from input parsing to UI controls:

```csharp
// In Window class
public void HandleMouse(MouseEvent mouseEvent)
{
    // Translate from window coordinates to content coordinates
    var contentPos = TranslateToContent(mouseEvent.Position);

    // Look up handler in MouseHandlers grid
    var handler = MouseHandlers[contentPos.Y, contentPos.X];
    handler?.Invoke(new MouseEvent(
        contentPos,
        mouseEvent.EventType,
        mouseEvent.Button,
        mouseEvent.Modifiers));
}
```

### VT100 Mouse Parsing

The VT100 input parser translates escape sequences to mouse events:

```csharp
// SGR mouse encoding: \x1b[<Btn;Col;Row M/m
private MouseEvent? ParseSgrMouse(string sequence)
{
    // Format: \x1b[<btn;col;rowM (press) or m (release)
    var match = Regex.Match(sequence, @"\x1b\[<(\d+);(\d+);(\d+)([Mm])");
    if (!match.Success) return null;

    var btnCode = int.Parse(match.Groups[1].Value);
    var col = int.Parse(match.Groups[2].Value) - 1;
    var row = int.Parse(match.Groups[3].Value) - 1;
    var isRelease = match.Groups[4].Value == "m";

    var button = (btnCode & 0x03) switch
    {
        0 => MouseButton.Left,
        1 => MouseButton.Middle,
        2 => MouseButton.Right,
        _ => MouseButton.None
    };

    var modifiers = MouseModifier.None;
    if ((btnCode & 0x04) != 0) modifiers |= MouseModifier.Shift;
    if ((btnCode & 0x08) != 0) modifiers |= MouseModifier.Alt;
    if ((btnCode & 0x10) != 0) modifiers |= MouseModifier.Control;

    var eventType = isRelease ? MouseEventType.MouseUp
        : (btnCode & 0x40) != 0 ? MouseEventType.MouseMove
        : (btnCode & 0x60) == 0x60 ? MouseEventType.ScrollUp
        : (btnCode & 0x61) == 0x61 ? MouseEventType.ScrollDown
        : MouseEventType.MouseDown;

    return new MouseEvent(
        new Point(col, row),
        eventType,
        button,
        modifiers);
}
```

### MouseHandlers Grid

The renderer maintains a 2D grid of mouse handlers:

```csharp
public sealed class MouseHandlers
{
    private readonly Dictionary<Point, Action<MouseEvent>?> _handlers = new();

    public Action<MouseEvent>? this[int row, int col]
    {
        get => _handlers.GetValueOrDefault(new Point(col, row));
        set => _handlers[new Point(col, row)] = value;
    }

    public void SetHandlerForRegion(
        int x, int y, int width, int height,
        Action<MouseEvent>? handler)
    {
        for (var row = y; row < y + height; row++)
            for (var col = x; col < x + width; col++)
                this[row, col] = handler;
    }
}
```

## Dependencies

- Feature 4: Point data structure
- Feature 23: Input parsing

## Implementation Tasks

1. Implement `MouseEventType` enum
2. Implement `MouseButton` enum
3. Implement `MouseModifier` flags enum
4. Implement `MouseEvent` record
5. Add mouse parsing to VT100 input parser
6. Implement `MouseHandlers` grid class
7. Integrate with Window for event dispatch
8. Write unit tests

## Acceptance Criteria

- [ ] MouseEventType has all event types
- [ ] MouseButton identifies all buttons
- [ ] MouseModifier supports multiple modifiers (flags)
- [ ] MouseEvent stores all event data
- [ ] VT100 parser correctly decodes mouse sequences
- [ ] MouseHandlers grid stores handlers by position
- [ ] Window translates coordinates for controls
- [ ] Unit tests achieve 80% coverage
