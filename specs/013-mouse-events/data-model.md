# Data Model: Mouse Events

**Feature**: 013-mouse-events
**Date**: 2026-01-25

## Entity Definitions

### MouseEventType (Enum)

Represents the type of mouse interaction.

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

### MouseButton (Enum)

Represents which mouse button was pressed.

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

### MouseModifiers (Flags Enum)

Represents modifier keys held during a mouse event.

```csharp
namespace Stroke.Input;

/// <summary>
/// Modifier keys held during a mouse event.
/// </summary>
[Flags]
public enum MouseModifiers
{
    /// <summary>
    /// No modifier keys held.
    /// </summary>
    None = 0,

    /// <summary>
    /// Shift key held.
    /// </summary>
    Shift = 1,

    /// <summary>
    /// Alt key held.
    /// </summary>
    Alt = 2,

    /// <summary>
    /// Control key held.
    /// </summary>
    Control = 4
}
```

### MouseEvent (Record Struct)

Immutable value type capturing a complete mouse event.

```csharp
namespace Stroke.Input;

/// <summary>
/// Mouse event, sent to UIControl.MouseHandler.
/// </summary>
/// <param name="Position">The position in the terminal (column, row).</param>
/// <param name="EventType">The type of mouse event.</param>
/// <param name="Button">The mouse button.</param>
/// <param name="Modifiers">The modifier keys held.</param>
public readonly record struct MouseEvent(
    Point Position,
    MouseEventType EventType,
    MouseButton Button,
    MouseModifiers Modifiers)
{
    /// <summary>
    /// Returns a string representation of the mouse event.
    /// </summary>
    public override string ToString() =>
        $"MouseEvent({Position}, {EventType}, {Button}, {Modifiers})";
}
```

**Relationships**:
- Uses `Point` from `Stroke.Core.Primitives`
- Composes `MouseEventType`, `MouseButton`, `MouseModifiers`

### NotImplementedOrNone (Abstract Class)

Return value from mouse handlers signaling whether an event was handled.

```csharp
namespace Stroke.Layout;

/// <summary>
/// Return value from mouse handlers.
/// Used to indicate whether the event was handled or should bubble up.
/// </summary>
public abstract class NotImplementedOrNone
{
    // Private constructor prevents external inheritance
    private NotImplementedOrNone() { }

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

**Design Notes**:
- Abstract class with private constructor prevents external subclassing
- Two singleton instances: `NotImplemented` and `None`
- Reference equality (`is NotImplementedOrNone.NotImplemented`) for comparison

### MouseHandlers (Mutable Class)

2D grid of mouse event handlers populated by the renderer during layout.

```csharp
namespace Stroke.Layout;

/// <summary>
/// Two dimensional raster of callbacks for mouse events.
/// </summary>
public sealed class MouseHandlers
{
    private readonly Lock _lock = new();
    // Map y (row) to x (column) to handler
    private readonly Dictionary<int, Dictionary<int, Func<MouseEvent, NotImplementedOrNone>>> _handlers = new();

    /// <summary>
    /// Set a mouse handler for a rectangular region.
    /// </summary>
    /// <param name="xMin">Minimum X coordinate (inclusive).</param>
    /// <param name="xMax">Maximum X coordinate (exclusive).</param>
    /// <param name="yMin">Minimum Y coordinate (inclusive).</param>
    /// <param name="yMax">Maximum Y coordinate (exclusive).</param>
    /// <param name="handler">The mouse handler callback. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if handler is null.</exception>
    public void SetMouseHandlerForRange(
        int xMin,
        int xMax,
        int yMin,
        int yMax,
        Func<MouseEvent, NotImplementedOrNone> handler);

    /// <summary>
    /// Get the mouse handler at a specific position.
    /// </summary>
    /// <param name="x">X coordinate (column).</param>
    /// <param name="y">Y coordinate (row).</param>
    /// <returns>The handler at the position, or null if none.</returns>
    public Func<MouseEvent, NotImplementedOrNone>? GetHandler(int x, int y);

    /// <summary>
    /// Clear all handlers.
    /// </summary>
    public void Clear();
}
```

**Storage Strategy**:
- Nested dictionaries: `y → x → handler`
- Matches Python's `defaultdict(lambda: defaultdict(...))` pattern
- Sparse storage: only positions with handlers are stored
- O(1) lookup at any position

**Thread Safety**:
- Thread-safe per Constitution XI
- Uses `System.Threading.Lock` with `EnterScope()` pattern
- All public methods (SetMouseHandlerForRange, GetHandler, Clear) are synchronized
- Individual operations are atomic; compound operations require external synchronization

**Exception Handling**:
- If a registered handler throws an exception during invocation, the exception propagates to the caller
- SetMouseHandlerForRange throws ArgumentNullException if handler is null

## Entity Relationship Diagram

```text
┌─────────────────────┐
│   MouseEvent        │ (record struct, immutable)
├─────────────────────┤
│ + Position: Point   │───────► Stroke.Core.Point
│ + EventType         │───────► MouseEventType (enum)
│ + Button            │───────► MouseButton (enum)
│ + Modifiers         │───────► MouseModifiers ([Flags] enum)
└─────────────────────┘
           │
           │ passed to
           ▼
┌─────────────────────────────────────────────────┐
│   MouseHandlers                                  │ (mutable class)
├─────────────────────────────────────────────────┤
│ - _handlers: Dict<int, Dict<int, Handler>>       │
├─────────────────────────────────────────────────┤
│ + SetMouseHandlerForRange(...)                   │
│ + GetHandler(x, y): Handler?                     │
│ + Clear()                                        │
└─────────────────────────────────────────────────┘
           │
           │ returns
           ▼
┌─────────────────────────────────────┐
│   NotImplementedOrNone              │ (abstract class)
├─────────────────────────────────────┤
│ + NotImplemented (singleton)        │
│ + None (singleton)                  │
└─────────────────────────────────────┘
```

## Validation Rules

| Entity | Rule | Error Condition |
|--------|------|-----------------|
| MouseEvent | Position can be any value | No validation (caller responsibility) |
| MouseHandlers.SetMouseHandlerForRange | xMin < xMax, yMin < yMax | Empty region: no positions affected |
| MouseHandlers.GetHandler | Any x, y values accepted | Returns null for unregistered positions |

## State Transitions

**MouseHandlers Lifecycle**:
1. Created empty by renderer
2. Populated via `SetMouseHandlerForRange` during layout pass
3. Queried via `GetHandler` when mouse events occur
4. Cleared via `Clear()` before next layout pass
5. Re-populated for new layout

```text
[Empty] ──SetHandler──► [Populated] ──GetHandler──► [Populated]
                              │                          │
                              └──────── Clear ───────────┘
                                          │
                                          ▼
                                      [Empty]
```
