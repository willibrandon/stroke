# Feature 108: Mouse Handlers

## Overview

Implement the MouseHandlers class - a two-dimensional raster of mouse event callbacks used for handling mouse interactions across the screen buffer.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/mouse_handlers.py`

## Public API

### MouseHandler Delegate

```csharp
namespace Stroke.Layout;

/// <summary>
/// Delegate for handling mouse events.
/// </summary>
/// <param name="mouseEvent">The mouse event.</param>
/// <returns>NotImplemented if not handled, null otherwise.</returns>
public delegate object? MouseHandler(MouseEvent mouseEvent);
```

### MouseHandlers

```csharp
namespace Stroke.Layout;

/// <summary>
/// Two dimensional raster of callbacks for mouse events.
/// Maps screen coordinates to mouse event handlers.
/// </summary>
public sealed class MouseHandlers
{
    /// <summary>
    /// Create a new mouse handlers collection.
    /// </summary>
    public MouseHandlers();

    /// <summary>
    /// Get the mouse handler at the specified position.
    /// Returns a dummy handler that returns NotImplemented if none is set.
    /// </summary>
    /// <param name="x">Column position.</param>
    /// <param name="y">Row position.</param>
    /// <returns>The mouse handler at that position.</returns>
    public MouseHandler this[int x, int y] { get; }

    /// <summary>
    /// Set a mouse handler for a rectangular region.
    /// </summary>
    /// <param name="xMin">Minimum X coordinate (inclusive).</param>
    /// <param name="xMax">Maximum X coordinate (exclusive).</param>
    /// <param name="yMin">Minimum Y coordinate (inclusive).</param>
    /// <param name="yMax">Maximum Y coordinate (exclusive).</param>
    /// <param name="handler">The handler to set for this region.</param>
    public void SetMouseHandlerForRange(
        int xMin,
        int xMax,
        int yMin,
        int yMax,
        MouseHandler handler);
}
```

## Project Structure

```
src/Stroke/
└── Layout/
    └── MouseHandlers.cs
tests/Stroke.Tests/
└── Layout/
    └── MouseHandlersTests.cs
```

## Implementation Notes

### MouseHandlers Implementation

```csharp
public sealed class MouseHandlers
{
    // Map y (row) to x (column) to handlers
    // Using nested dictionaries for sparse storage
    private readonly Dictionary<int, Dictionary<int, MouseHandler>> _mouseHandlers = new();

    private static readonly MouseHandler _dummyHandler = _ => NotImplemented.Value;

    public MouseHandlers() { }

    public MouseHandler this[int x, int y]
    {
        get
        {
            if (_mouseHandlers.TryGetValue(y, out var row) &&
                row.TryGetValue(x, out var handler))
            {
                return handler;
            }
            return _dummyHandler;
        }
    }

    public void SetMouseHandlerForRange(
        int xMin,
        int xMax,
        int yMin,
        int yMax,
        MouseHandler handler)
    {
        for (int y = yMin; y < yMax; y++)
        {
            if (!_mouseHandlers.TryGetValue(y, out var row))
            {
                row = new Dictionary<int, MouseHandler>();
                _mouseHandlers[y] = row;
            }

            for (int x = xMin; x < xMax; x++)
            {
                row[x] = handler;
            }
        }
    }
}
```

### NotImplemented Sentinel

```csharp
namespace Stroke;

/// <summary>
/// Sentinel value indicating a handler did not process an event.
/// </summary>
public sealed class NotImplemented
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static readonly NotImplemented Value = new();

    private NotImplemented() { }
}
```

### Usage Example

```csharp
var handlers = new MouseHandlers();

// Set a handler for a button region (columns 10-20, row 5)
handlers.SetMouseHandlerForRange(10, 20, 5, 6, (MouseEvent e) =>
{
    if (e.EventType == MouseEventType.MouseUp)
    {
        Console.WriteLine("Button clicked!");
        return null; // Handled
    }
    return NotImplemented.Value; // Not handled
});

// Get handler at position
var handler = handlers[15, 5];
var result = handler(mouseEvent);

if (result != NotImplemented.Value)
{
    // Event was handled
}
```

## Dependencies

- Feature 17: Mouse Events

## Implementation Tasks

1. Define MouseHandler delegate
2. Implement NotImplemented sentinel
3. Implement MouseHandlers with sparse storage
4. Implement SetMouseHandlerForRange
5. Implement indexer for position lookup
6. Write unit tests

## Acceptance Criteria

- [ ] MouseHandlers stores handlers by position
- [ ] SetMouseHandlerForRange sets handlers for rectangular regions
- [ ] Unset positions return dummy handler
- [ ] Sparse storage is memory efficient
- [ ] Unit tests achieve 80% coverage
