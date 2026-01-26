# Quickstart: Mouse Events

**Feature**: 013-mouse-events
**Date**: 2026-01-25

## Overview

This feature provides the mouse event system for Stroke, enabling terminal applications to handle mouse clicks, scrolls, drags, and movements.

## Key Types

| Type | Namespace | Purpose |
|------|-----------|---------|
| `MouseEventType` | `Stroke.Input` | Enum: MouseUp, MouseDown, ScrollUp, ScrollDown, MouseMove |
| `MouseButton` | `Stroke.Input` | Enum: Left, Middle, Right, None, Unknown |
| `MouseModifiers` | `Stroke.Input` | [Flags] enum: None, Shift, Alt, Control |
| `MouseEvent` | `Stroke.Input` | Immutable record struct for event data |
| `MouseHandlers` | `Stroke.Layout` | 2D grid of mouse event handlers |
| `NotImplementedOrNone` | `Stroke.Layout` | Handler return type for event bubbling |

## Basic Usage

### Creating Mouse Events

```csharp
using Stroke.Input;
using Stroke.Core;

// Create a left-click event at position (10, 5)
var clickEvent = new MouseEvent(
    Position: new Point(10, 5),
    EventType: MouseEventType.MouseDown,
    Button: MouseButton.Left,
    Modifiers: MouseModifiers.None);

// Create a scroll event with Shift held
var scrollEvent = new MouseEvent(
    new Point(10, 5),
    MouseEventType.ScrollUp,
    MouseButton.None,
    MouseModifiers.Shift);

// Check modifiers using bitwise operations
if ((scrollEvent.Modifiers & MouseModifiers.Shift) != 0)
{
    // Shift is held
}
```

### Registering Mouse Handlers

```csharp
using Stroke.Layout;
using Stroke.Input;

var handlers = new MouseHandlers();

// Define a handler
NotImplementedOrNone HandleMouseEvent(MouseEvent e)
{
    Console.WriteLine($"Click at {e.Position}");
    return NotImplementedOrNone.None; // Event handled
}

// Register for a rectangular region (x: 0-79, y: 0-23)
handlers.SetMouseHandlerForRange(0, 80, 0, 24, HandleMouseEvent);

// Retrieve handler at a position
var handler = handlers.GetHandler(10, 5);
if (handler != null)
{
    var result = handler(clickEvent);
    if (result is NotImplementedOrNone.NotImplemented)
    {
        // Event not handled, bubble up
    }
}
```

### Event Bubbling Pattern

```csharp
NotImplementedOrNone MyHandler(MouseEvent e)
{
    if (e.EventType == MouseEventType.MouseDown)
    {
        // Handle clicks
        return NotImplementedOrNone.None; // Consumed
    }

    // Let other events bubble up
    return NotImplementedOrNone.NotImplemented;
}
```

## File Locations

### Source Files

```text
src/Stroke/Input/
├── MouseEventType.cs
├── MouseButton.cs
├── MouseModifiers.cs
└── MouseEvent.cs

src/Stroke/Layout/
├── MouseHandlers.cs
└── NotImplementedOrNone.cs
```

### Test Files

```text
tests/Stroke.Tests/Input/
├── MouseEventTypeTests.cs
├── MouseButtonTests.cs
├── MouseModifiersTests.cs
└── MouseEventTests.cs

tests/Stroke.Tests/Layout/
├── MouseHandlersTests.cs
└── NotImplementedOrNoneTests.cs
```

## Implementation Order

1. **MouseEventType enum** - Simple enum, no dependencies
2. **MouseButton enum** - Simple enum, no dependencies
3. **MouseModifiers enum** - [Flags] enum, no dependencies
4. **MouseEvent record struct** - Depends on enums + Point
5. **NotImplementedOrNone class** - Handler return type
6. **MouseHandlers class** - Depends on MouseEvent, NotImplementedOrNone
7. **Tests** - After each implementation

## Testing Strategy

- Test each enum has correct values
- Test MouseEvent construction and ToString
- Test MouseModifiers flag combinations
- Test MouseHandlers registration and retrieval
- Test edge cases: empty regions, out-of-bounds queries, overlapping regions
- No mocks required - all types are self-contained

## Dependencies

- `Stroke.Core.Point` - Already exists at `src/Stroke/Core/Primitives/Point.cs`
- No external NuGet packages required
