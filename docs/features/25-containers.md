# Feature 25: Layout Containers

## Overview

Implement the container hierarchy (HSplit, VSplit, FloatContainer, Window) that forms the structure of the layout system.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/containers.py`

## Public API

### IContainer Interface

```csharp
namespace Stroke.Layout;

/// <summary>
/// Base interface for user interface layout containers.
/// </summary>
public interface IContainer
{
    /// <summary>
    /// Reset the state of this container and all children.
    /// </summary>
    void Reset();

    /// <summary>
    /// Return the desired width for this container.
    /// </summary>
    Dimension PreferredWidth(int maxAvailableWidth);

    /// <summary>
    /// Return the desired height for this container.
    /// </summary>
    Dimension PreferredHeight(int width, int maxAvailableHeight);

    /// <summary>
    /// Write the content to the screen.
    /// </summary>
    void WriteToScreen(
        Screen screen,
        MouseHandlers mouseHandlers,
        WritePosition writePosition,
        string parentStyle,
        bool eraseBg,
        int? zIndex);

    /// <summary>
    /// When true, key bindings from parent containers are not taken into
    /// account if a user control in this container is focused.
    /// </summary>
    bool IsModal();

    /// <summary>
    /// Returns key bindings that become active when any user control
    /// in this container has the focus.
    /// </summary>
    IKeyBindingsBase? GetKeyBindings();

    /// <summary>
    /// Return the list of child Container objects.
    /// </summary>
    IReadOnlyList<IContainer> GetChildren();
}
```

### Alignment Enums

```csharp
namespace Stroke.Layout;

/// <summary>
/// Alignment for HSplit.
/// </summary>
public enum VerticalAlign
{
    Top,
    Center,
    Bottom,
    Justify
}

/// <summary>
/// Alignment for VSplit.
/// </summary>
public enum HorizontalAlign
{
    Left,
    Center,
    Right,
    Justify
}

/// <summary>
/// Alignment for Window content.
/// </summary>
public enum WindowAlign
{
    Left,
    Right,
    Center
}
```

### HSplit Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Several layouts, one stacked above/under the other.
/// </summary>
public sealed class HSplit : IContainer
{
    /// <summary>
    /// Creates an HSplit container.
    /// </summary>
    /// <param name="children">List of child containers.</param>
    /// <param name="windowTooSmall">Container displayed when not enough space.</param>
    /// <param name="align">Vertical alignment.</param>
    /// <param name="padding">Size for padding between children.</param>
    /// <param name="paddingChar">Character for padding.</param>
    /// <param name="paddingStyle">Style for padding.</param>
    /// <param name="width">Override width.</param>
    /// <param name="height">Override height.</param>
    /// <param name="zIndex">Z-index for layering.</param>
    /// <param name="modal">True if modal.</param>
    /// <param name="keyBindings">Associated key bindings.</param>
    /// <param name="style">Style string.</param>
    public HSplit(
        IEnumerable<object> children,
        IContainer? windowTooSmall = null,
        VerticalAlign align = VerticalAlign.Justify,
        object? padding = null,
        char? paddingChar = null,
        string paddingStyle = "",
        object? width = null,
        object? height = null,
        int? zIndex = null,
        bool modal = false,
        IKeyBindingsBase? keyBindings = null,
        object? style = null);

    /// <summary>
    /// The child containers.
    /// </summary>
    public IReadOnlyList<IContainer> Children { get; }

    /// <summary>
    /// The vertical alignment.
    /// </summary>
    public VerticalAlign Align { get; }

    // IContainer implementation...
}
```

### VSplit Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Several layouts, one stacked left/right of the other.
/// </summary>
public sealed class VSplit : IContainer
{
    /// <summary>
    /// Creates a VSplit container.
    /// </summary>
    /// <param name="children">List of child containers.</param>
    /// <param name="windowTooSmall">Container displayed when not enough space.</param>
    /// <param name="align">Horizontal alignment.</param>
    /// <param name="padding">Size for padding between children.</param>
    /// <param name="paddingChar">Character for padding.</param>
    /// <param name="paddingStyle">Style for padding.</param>
    /// <param name="width">Override width.</param>
    /// <param name="height">Override height.</param>
    /// <param name="zIndex">Z-index for layering.</param>
    /// <param name="modal">True if modal.</param>
    /// <param name="keyBindings">Associated key bindings.</param>
    /// <param name="style">Style string.</param>
    public VSplit(
        IEnumerable<object> children,
        IContainer? windowTooSmall = null,
        HorizontalAlign align = HorizontalAlign.Justify,
        object? padding = null,
        char? paddingChar = null,
        string paddingStyle = "",
        object? width = null,
        object? height = null,
        int? zIndex = null,
        bool modal = false,
        IKeyBindingsBase? keyBindings = null,
        object? style = null);

    /// <summary>
    /// The child containers.
    /// </summary>
    public IReadOnlyList<IContainer> Children { get; }

    /// <summary>
    /// The horizontal alignment.
    /// </summary>
    public HorizontalAlign Align { get; }

    // IContainer implementation...
}
```

### FloatContainer Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Container which can contain another container for the background,
/// as well as a list of floating containers on top of it.
/// </summary>
public sealed class FloatContainer : IContainer
{
    /// <summary>
    /// Creates a FloatContainer.
    /// </summary>
    /// <param name="content">The background container.</param>
    /// <param name="floats">List of floating containers.</param>
    /// <param name="modal">True if modal.</param>
    /// <param name="keyBindings">Associated key bindings.</param>
    /// <param name="style">Style string.</param>
    /// <param name="zIndex">Z-index for the whole container.</param>
    public FloatContainer(
        object content,
        IReadOnlyList<Float> floats,
        bool modal = false,
        IKeyBindingsBase? keyBindings = null,
        object? style = null,
        int? zIndex = null);

    /// <summary>
    /// The background content.
    /// </summary>
    public IContainer Content { get; }

    /// <summary>
    /// The floating containers.
    /// </summary>
    public IReadOnlyList<Float> Floats { get; }

    // IContainer implementation...
}
```

### Float Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Float for use in a FloatContainer.
/// </summary>
public sealed class Float
{
    /// <summary>
    /// Creates a Float.
    /// </summary>
    /// <param name="content">The floating container content.</param>
    /// <param name="top">Distance to top edge.</param>
    /// <param name="right">Distance to right edge.</param>
    /// <param name="bottom">Distance to bottom edge.</param>
    /// <param name="left">Distance to left edge.</param>
    /// <param name="width">Width (int or callable).</param>
    /// <param name="height">Height (int or callable).</param>
    /// <param name="xcursor">Position near cursor X.</param>
    /// <param name="ycursor">Position near cursor Y.</param>
    /// <param name="attachToWindow">Attach to cursor from this window.</param>
    /// <param name="hideWhenCoveringContent">Hide when covering content.</param>
    /// <param name="allowCoverCursor">Allow covering the cursor.</param>
    /// <param name="zIndex">Z-index (must be >= 1).</param>
    /// <param name="transparent">Draw transparently.</param>
    public Float(
        object content,
        int? top = null,
        int? right = null,
        int? bottom = null,
        int? left = null,
        object? width = null,
        object? height = null,
        bool xcursor = false,
        bool ycursor = false,
        object? attachToWindow = null,
        bool hideWhenCoveringContent = false,
        bool allowCoverCursor = false,
        int zIndex = 1,
        bool transparent = false);

    public int? Left { get; }
    public int? Right { get; }
    public int? Top { get; }
    public int? Bottom { get; }
    public bool XCursor { get; }
    public bool YCursor { get; }
    public Window? AttachToWindow { get; }
    public IContainer Content { get; }
    public bool HideWhenCoveringContent { get; }
    public bool AllowCoverCursor { get; }
    public int ZIndex { get; }
    public IFilter Transparent { get; }

    public int? GetWidth();
    public int? GetHeight();
}
```

### ConditionalContainer Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Wrapper around a container that can hide it based on a filter.
/// </summary>
public sealed class ConditionalContainer : IContainer
{
    public ConditionalContainer(object content, IFilter? filter = null);

    public IContainer Content { get; }
    public IFilter Filter { get; }

    // IContainer implementation...
}
```

### DynamicContainer Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Container that can dynamically return any Container.
/// </summary>
public sealed class DynamicContainer : IContainer
{
    public DynamicContainer(Func<IContainer?> getContainer);

    public Func<IContainer?> GetContainer { get; }

    // IContainer implementation...
}
```

### Container Utilities

```csharp
namespace Stroke.Layout;

/// <summary>
/// Container utilities.
/// </summary>
public static class ContainerUtils
{
    /// <summary>
    /// Turn any object into a Container.
    /// Accepts Container, Window, UIControl, or objects with __pt_container__.
    /// </summary>
    public static IContainer ToContainer(object value);

    /// <summary>
    /// Turn any object into a Window.
    /// </summary>
    public static Window ToWindow(object value);

    /// <summary>
    /// Test whether the given value could be a valid container.
    /// </summary>
    public static bool IsContainer(object? value);
}
```

## Project Structure

```
src/Stroke/
└── Layout/
    ├── IContainer.cs
    ├── VerticalAlign.cs
    ├── HorizontalAlign.cs
    ├── WindowAlign.cs
    ├── HSplit.cs
    ├── VSplit.cs
    ├── FloatContainer.cs
    ├── Float.cs
    ├── ConditionalContainer.cs
    ├── DynamicContainer.cs
    └── ContainerUtils.cs
tests/Stroke.Tests/
└── Layout/
    ├── HSplitTests.cs
    ├── VSplitTests.cs
    ├── FloatContainerTests.cs
    ├── FloatTests.cs
    ├── ConditionalContainerTests.cs
    └── DynamicContainerTests.cs
```

## Implementation Notes

### Size Division Algorithm

For HSplit/VSplit, sizes are allocated using a weighted algorithm:

1. Start with minimum sizes for all children
2. Calculate dimensions for all children
3. If total minimum exceeds available space, show "Window too small"
4. Distribute remaining space proportionally by weight
5. First fill to preferred size, then to max

### Float Positioning

Float positions are calculated as follows:
- If `left` and `width` given: use directly
- If `left` and `right` given: calculate width
- If `width` and `right` given: calculate left
- If `xcursor`: position near cursor X
- If only `width`: center horizontally
- Otherwise: use preferred width and center

Similar logic applies for vertical positioning.

### Z-Index Handling

- Floats have a minimum z-index of 1
- Container z-index is summed with float z-index
- Higher z-index floats are drawn later (on top)
- Cursor-positioned floats get very high z-index to draw last

### Float Drawing Pattern

FloatContainer uses Screen's deferred drawing mechanism for float composition:

```csharp
// In FloatContainer.WriteToScreen():
foreach (var fl in Floats)
{
    // Queue the float's drawing function at its z-index
    screen.DrawWithZIndex(fl.ZIndex, () =>
    {
        // Calculate position, render float content to screen
        var floatWritePosition = CalculateFloatPosition(fl, writePosition);
        fl.Content.WriteToScreen(screen, mouseHandlers, floatWritePosition, ...);
    });
}

// At the end of the root container's WriteToScreen:
screen.DrawAllFloats();  // Executes all queued draws in z-index order
```

This pattern ensures floats are drawn after all background content, and in correct z-order even when floats dynamically add more floats during rendering.

### Magic Container Protocol

Objects can implement `__pt_container__` (via interface `IHasContainer`) to be treated as containers:

```csharp
public interface IHasContainer
{
    IContainer Container { get; }
}
```

## Dependencies

- `Stroke.Layout.Dimension` (Feature 24) - Dimension system
- `Stroke.Layout.Screen` (Feature 22) - Screen buffer (same namespace)
- `Stroke.Layout.Window` (Feature 27) - Window container (same namespace)
- `Stroke.Filters` (Feature 12) - Filter system
- `Stroke.KeyBinding` (Feature 19) - Key bindings

## Implementation Tasks

1. Implement `IContainer` interface
2. Implement alignment enums
3. Implement `HSplit` class
4. Implement `VSplit` class
5. Implement `FloatContainer` class
6. Implement `Float` class
7. Implement `ConditionalContainer` class
8. Implement `DynamicContainer` class
9. Implement `ContainerUtils` static class
10. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All container types match Python Prompt Toolkit semantics
- [ ] Size division algorithm works correctly
- [ ] Float positioning works correctly
- [ ] Z-index handling works correctly
- [ ] Conditional and dynamic containers work
- [ ] Unit tests achieve 80% coverage
