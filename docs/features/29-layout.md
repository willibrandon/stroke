# Feature 29: Layout Manager

## Overview

Implement the Layout class that wraps the container hierarchy and manages focus, search links, and container traversal.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/layout.py`

## Public API

### Layout Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// The layout for a prompt_toolkit Application.
/// This keeps track of which user control is focused.
/// </summary>
public sealed class Layout
{
    /// <summary>
    /// Creates a Layout.
    /// </summary>
    /// <param name="container">The root container.</param>
    /// <param name="focusedElement">Element to focus initially.</param>
    public Layout(object container, object? focusedElement = null);

    /// <summary>
    /// The root container.
    /// </summary>
    public IContainer Container { get; set; }

    /// <summary>
    /// Map search BufferControl back to the original BufferControl.
    /// This tracks active searches.
    /// </summary>
    public IDictionary<SearchBufferControl, BufferControl> SearchLinks { get; }

    /// <summary>
    /// List of visible windows (updated after each render).
    /// </summary>
    public IList<Window> VisibleWindows { get; }

    /// <summary>
    /// Find all Window objects in this layout.
    /// </summary>
    public IEnumerable<Window> FindAllWindows();

    /// <summary>
    /// Find all UIControl objects in this layout.
    /// </summary>
    public IEnumerable<IUIControl> FindAllControls();

    /// <summary>
    /// Focus the given UI element.
    /// </summary>
    /// <param name="value">
    /// Can be a UIControl, Buffer, buffer name (string), Window, or Container.
    /// </param>
    public void Focus(object value);

    /// <summary>
    /// Check whether the given control has the focus.
    /// </summary>
    public bool HasFocus(object value);

    /// <summary>
    /// Get the currently focused UIControl.
    /// </summary>
    public IUIControl CurrentControl { get; set; }

    /// <summary>
    /// Get the currently focused Window.
    /// </summary>
    public Window CurrentWindow { get; set; }

    /// <summary>
    /// True if we are currently searching.
    /// </summary>
    public bool IsSearching { get; }

    /// <summary>
    /// Return the BufferControl being searched, or null.
    /// </summary>
    public BufferControl? SearchTargetBufferControl { get; }

    /// <summary>
    /// Return all focusable windows in the modal area.
    /// </summary>
    public IEnumerable<Window> GetFocusableWindows();

    /// <summary>
    /// Return visible focusable windows.
    /// </summary>
    public IReadOnlyList<Window> GetVisibleFocusableWindows();

    /// <summary>
    /// The currently focused Buffer, or null.
    /// </summary>
    public Buffer? CurrentBuffer { get; }

    /// <summary>
    /// Look for a buffer with the given name.
    /// </summary>
    public Buffer? GetBufferByName(string bufferName);

    /// <summary>
    /// True if the currently focused control is a BufferControl.
    /// </summary>
    public bool BufferHasFocus { get; }

    /// <summary>
    /// Get the previously focused UIControl.
    /// </summary>
    public IUIControl PreviousControl { get; }

    /// <summary>
    /// Give focus to the last focused control.
    /// </summary>
    public void FocusLast();

    /// <summary>
    /// Focus the next visible/focusable Window.
    /// </summary>
    public void FocusNext();

    /// <summary>
    /// Focus the previous visible/focusable Window.
    /// </summary>
    public void FocusPrevious();

    /// <summary>
    /// Walk through all layout nodes.
    /// </summary>
    public IEnumerable<IContainer> Walk();

    /// <summary>
    /// Walk through containers in the current modal part of the layout.
    /// </summary>
    public IEnumerable<IContainer> WalkThroughModalArea();

    /// <summary>
    /// Update child-to-parent relationships.
    /// </summary>
    public void UpdateParentsRelations();

    /// <summary>
    /// Reset the layout state.
    /// </summary>
    public void Reset();

    /// <summary>
    /// Return the parent container for a given container.
    /// </summary>
    public IContainer? GetParent(IContainer container);
}
```

### InvalidLayoutError Exception

```csharp
namespace Stroke.Layout;

/// <summary>
/// Exception raised for invalid layouts.
/// </summary>
public sealed class InvalidLayoutError : Exception
{
    public InvalidLayoutError(string message) : base(message) { }
}
```

### Walk Function

```csharp
namespace Stroke.Layout;

/// <summary>
/// Layout utilities.
/// </summary>
public static class LayoutUtils
{
    /// <summary>
    /// Walk through a layout, starting at the given container.
    /// </summary>
    /// <param name="container">The starting container.</param>
    /// <param name="skipHidden">Skip hidden ConditionalContainers.</param>
    public static IEnumerable<IContainer> Walk(
        IContainer container,
        bool skipHidden = false);
}
```

## Project Structure

```
src/Stroke/
└── Layout/
    ├── Layout.cs
    ├── InvalidLayoutError.cs
    └── LayoutUtils.cs
tests/Stroke.Tests/
└── Layout/
    ├── LayoutTests.cs
    └── LayoutUtilsTests.cs
```

## Implementation Notes

### Focus Stack

The Layout maintains a focus stack:
- Each focus change pushes to the stack
- `FocusLast()` pops from the stack
- `CurrentWindow` returns top of stack
- `PreviousControl` returns second from top

### Focus Resolution

When `Focus(value)` is called:
1. If string: find BufferControl by buffer name
2. If Buffer: find BufferControl with that buffer
3. If UIControl: find Window containing it
4. If Window: focus directly
5. If Container: focus most recently focused window inside, or first focusable

### Modal Area

Modal containers restrict focus navigation:
- `IsModal()` returns true for modal containers
- `WalkThroughModalArea()` walks only within modal boundary
- `GetFocusableWindows()` only returns windows in modal area

### Search Links

Search links track active searches:
- Key: SearchBufferControl doing the search
- Value: BufferControl being searched
- `IsSearching` checks if current control is in search links
- Used to apply search highlighting

### Parent Relations

Child-to-parent mapping is updated during render:
- `UpdateParentsRelations()` rebuilds the map
- Used for walking up the tree to find modal root
- `GetParent()` returns parent of given container

### Focus Order

`FocusNext()` and `FocusPrevious()`:
1. Get visible focusable windows
2. Find current window index
3. Move to next/previous in circular order
4. Focus the new window

## Dependencies

- `Stroke.Layout.IContainer` (Feature 25) - Container interface
- `Stroke.Layout.Window` (Feature 27) - Window container
- `Stroke.Layout.BufferControl` (Feature 26) - Buffer control
- `Stroke.Layout.SearchBufferControl` (Feature 26) - Search buffer control
- `Stroke.Core.Buffer` (Feature 02) - Buffer class

## Implementation Tasks

1. Implement `Layout` class
2. Implement focus stack management
3. Implement `Focus(value)` with all overloads
4. Implement `HasFocus(value)` checking
5. Implement modal area walking
6. Implement search links management
7. Implement parent relations tracking
8. Implement `InvalidLayoutError` exception
9. Implement `LayoutUtils.Walk` function
10. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Layout class matches Python Prompt Toolkit semantics
- [ ] Focus management works correctly
- [ ] Focus stack maintains history
- [ ] Modal area restriction works
- [ ] Search links track active searches
- [ ] Parent relations are tracked
- [ ] Focus navigation works correctly
- [ ] Unit tests achieve 80% coverage
