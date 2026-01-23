# Feature 17: Mouse Events

## Overview

Implement the mouse event system for handling mouse clicks, drags, scrolls, and movements in the terminal.

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
    /// Mouse button released. Fired for all buttons (left, right, middle).
    /// </summary>
    MouseUp,

    /// <summary>
    /// Left mouse button pressed. Not fired for middle or right buttons.
    /// </summary>
    MouseDown,

    /// <summary>
    /// Scroll wheel up.
    /// </summary>
    ScrollUp,

    /// <summary>
    /// Scroll wheel down.
    /// </summary>
    ScrollDown,

    /// <summary>
    /// Mouse moved while left button is held down.
    /// </summary>
    MouseMove
}
```

### MouseButton Enum

```csharp
namespace Stroke.Input;

/// <summary>
/// Mouse button that was pressed.
/// </summary>
public enum MouseButton
{
    /// <summary>
    /// Left mouse button.
    /// </summary>
    Left,

    /// <summary>
    /// Middle mouse button.
    /// </summary>
    Middle,

    /// <summary>
    /// Right mouse button.
    /// </summary>
    Right,

    /// <summary>
    /// No button pressed (scrolling or just moving).
    /// </summary>
    None,

    /// <summary>
    /// A button was pressed but we don't know which one.
    /// </summary>
    Unknown
}
```

### MouseModifier Enum

```csharp
namespace Stroke.Input;

/// <summary>
/// Modifier key held during mouse event.
/// </summary>
public enum MouseModifier
{
    /// <summary>
    /// Shift key.
    /// </summary>
    Shift,

    /// <summary>
    /// Alt key.
    /// </summary>
    Alt,

    /// <summary>
    /// Control key.
    /// </summary>
    Control
}
```

### MouseEvent Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Mouse event, sent to UIControl.MouseHandler.
/// </summary>
public sealed class MouseEvent
{
    /// <summary>
    /// Creates a mouse event.
    /// </summary>
    /// <param name="position">The position in the terminal.</param>
    /// <param name="eventType">The type of mouse event.</param>
    /// <param name="button">The mouse button.</param>
    /// <param name="modifiers">The modifier keys held.</param>
    public MouseEvent(
        Point position,
        MouseEventType eventType,
        MouseButton button,
        IReadOnlySet<MouseModifier> modifiers);

    /// <summary>
    /// The position in the terminal (row, column).
    /// </summary>
    public Point Position { get; }

    /// <summary>
    /// The type of mouse event.
    /// </summary>
    public MouseEventType EventType { get; }

    /// <summary>
    /// The mouse button.
    /// </summary>
    public MouseButton Button { get; }

    /// <summary>
    /// The modifier keys held.
    /// </summary>
    public IReadOnlySet<MouseModifier> Modifiers { get; }

    public override string ToString();
}
```

### MouseHandlers Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// 2D grid of mouse event handlers.
/// The renderer populates this grid during layout.
/// </summary>
public sealed class MouseHandlers
{
    /// <summary>
    /// Creates a mouse handlers grid.
    /// </summary>
    public MouseHandlers();

    /// <summary>
    /// Set a mouse handler for a region.
    /// </summary>
    /// <param name="writePosition">The position and size of the region.</param>
    /// <param name="handler">The mouse handler callback.</param>
    public void SetHandler(
        WritePosition writePosition,
        Func<MouseEvent, NotImplementedOrNone>? handler);

    /// <summary>
    /// Get the mouse handler at a position.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>The handler, or null if none.</returns>
    public Func<MouseEvent, NotImplementedOrNone>? GetHandler(Point position);

    /// <summary>
    /// Clear all handlers.
    /// </summary>
    public void Clear();
}
```

### NotImplementedOrNone Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Return value from mouse handlers.
/// Used to indicate whether the event was handled or should bubble up.
/// </summary>
public abstract class NotImplementedOrNone
{
    /// <summary>
    /// Event was not handled, should bubble up.
    /// </summary>
    public static readonly NotImplementedOrNone NotImplemented = new NotImplementedValue();

    /// <summary>
    /// Event was handled.
    /// </summary>
    public static readonly NotImplementedOrNone None = new NoneValue();

    private sealed class NotImplementedValue : NotImplementedOrNone { }
    private sealed class NoneValue : NotImplementedOrNone { }
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
src/Stroke/
└── Layout/
    ├── MouseHandlers.cs
    └── NotImplementedOrNone.cs
tests/Stroke.Tests/
└── Input/
    ├── MouseEventTypeTests.cs
    ├── MouseButtonTests.cs
    └── MouseEventTests.cs
```

## Implementation Notes

### Mouse Handler Grid

The `MouseHandlers` class maintains a 2D grid that maps screen positions to handler callbacks. During layout:
1. Each `Window` registers its mouse handler for its region
2. Floats register handlers with higher z-index priority
3. When a mouse event occurs, the grid is queried for the handler at that position

### Event Bubbling

When a mouse event is not handled (`NotImplemented` is returned), it bubbles up to parent containers.

### Modifier Detection

The VT100 mouse protocols encode modifier keys in the button state:
- Bit 2 (4): Shift
- Bit 3 (8): Alt/Meta
- Bit 4 (16): Control

### Mouse Protocols

Different terminals support different mouse protocols:
- **X10**: Basic click reporting
- **Normal tracking**: Click and release
- **Button event tracking**: Movement while pressed
- **Any event tracking**: All movements
- **SGR extended**: Supports coordinates > 223

## Dependencies

- `Stroke.Core.Point` (Feature 00) - Point struct for position

## Implementation Tasks

1. Implement `MouseEventType` enum
2. Implement `MouseButton` enum
3. Implement `MouseModifier` enum
4. Implement `MouseEvent` class
5. Implement `MouseHandlers` class
6. Implement `NotImplementedOrNone` class
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All mouse event types match Python Prompt Toolkit semantics
- [ ] Mouse handler grid works correctly
- [ ] Modifier detection is correct
- [ ] Unit tests achieve 80% coverage
