# Feature 46: Menu System

## Overview

Implement the hierarchical menu system including MenuContainer and MenuItem for building application menu bars with nested submenus.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/widgets/menus.py`

## Public API

### MenuItem Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// A menu item with optional handler and children.
/// </summary>
public sealed class MenuItem
{
    /// <summary>
    /// Creates a MenuItem.
    /// </summary>
    /// <param name="text">The menu item text.</param>
    /// <param name="handler">Click handler (null for submenus).</param>
    /// <param name="children">Child menu items for submenus.</param>
    /// <param name="shortcut">Keyboard shortcut to display.</param>
    /// <param name="disabled">Whether the item is disabled.</param>
    public MenuItem(
        string text = "",
        Action? handler = null,
        IList<MenuItem>? children = null,
        IList<object>? shortcut = null,
        bool disabled = false);

    /// <summary>
    /// The menu item text. Use "-" for separator.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// The click handler.
    /// </summary>
    public Action? Handler { get; }

    /// <summary>
    /// Child menu items.
    /// </summary>
    public IList<MenuItem> Children { get; }

    /// <summary>
    /// Keyboard shortcut for display.
    /// </summary>
    public IList<object>? Shortcut { get; }

    /// <summary>
    /// Whether the item is disabled.
    /// </summary>
    public bool Disabled { get; }

    /// <summary>
    /// The width needed for child items.
    /// </summary>
    public int Width { get; }
}
```

### MenuContainer Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Container with a menu bar at the top.
/// </summary>
public sealed class MenuContainer : IContainer
{
    /// <summary>
    /// Creates a MenuContainer.
    /// </summary>
    /// <param name="body">The main content below the menu.</param>
    /// <param name="menuItems">Top-level menu items.</param>
    /// <param name="floats">Additional floats to display.</param>
    /// <param name="keyBindings">Additional key bindings.</param>
    public MenuContainer(
        AnyContainer body,
        IList<MenuItem> menuItems,
        IList<Float>? floats = null,
        KeyBindingsBase? keyBindings = null);

    /// <summary>
    /// The body container.
    /// </summary>
    public AnyContainer Body { get; }

    /// <summary>
    /// Top-level menu items.
    /// </summary>
    public IList<MenuItem> MenuItems { get; }

    /// <summary>
    /// Current menu selection path.
    /// </summary>
    public IList<int> SelectedMenu { get; }

    /// <summary>
    /// Extra floats to display.
    /// </summary>
    public IList<Float>? Floats { get; }

    Container IContainer.GetContainer();
}
```

## Project Structure

```
src/Stroke/
└── Widgets/
    ├── MenuItem.cs
    └── MenuContainer.cs
tests/Stroke.Tests/
└── Widgets/
    ├── MenuItemTests.cs
    └── MenuContainerTests.cs
```

## Implementation Notes

### MenuContainer Structure

```
┌──────────────────────────────────────────────────────────────────────┐
│ File   Edit   View   Help                                             │ ← Menu bar
├────────┬─────────────────────────────────────────────────────────────┤
│ New    │                                                              │
│ Open   ▶│ ← Submenu indicator                                         │
│ Save   │                                                              │
│ ────── │ ← Separator                                                  │
│ Exit   │                                                              │
├────────┴─────────────────────────────────────────────────────────────┤
│                                                                       │
│                          Body Content                                 │
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘
```

### Selection Path

`SelectedMenu` tracks the current selection:
- `[0]`: First top-level menu selected
- `[0, 2]`: First menu, third item selected
- `[1, 0, 1]`: Second menu → first child → second subitem

### Key Bindings (Main Menu)

- **Left**: Previous menu
- **Right**: Next menu
- **Down**: Open submenu
- **Ctrl-C/Ctrl-G**: Close menu, focus body

### Key Bindings (Submenu)

- **Left**: Go to parent menu
- **Right**: Open nested submenu or next top menu
- **Up**: Previous item (skip disabled)
- **Down**: Next item (skip disabled)
- **Enter**: Activate item

### Mouse Handling

- Click on menu bar: Toggle focus and open menu
- Hover on menu item: Select item
- Click on menu item: Activate item
- Move away: Close menu

### Separator Items

Use `text = "-"` to create a separator:

```csharp
new MenuItem("-")  // Renders as ────────
```

### Disabled Items

Disabled items:
- Render differently (grayed out style)
- Cannot be selected via keyboard
- Cannot be clicked via mouse
- Skipped when navigating with Up/Down

### Submenu Floats

Submenus are rendered as floats attached to parent:
- `submenu`: Level 0 submenu (attached to menu bar)
- `submenu2`: Level 1 submenu (attached to submenu)
- `submenu3`: Level 2 submenu (attached to submenu2)

Up to 3 levels of nesting supported.

### Menu Bar Fragment Generation

```csharp
// For each menu item:
// " ItemText "
// [SetMenuPosition] marker for selected item
// class:menu-bar.selected-item when selected
```

### Submenu Fragment Generation

```csharp
// ┌──────────────────┐
// │ Item 1           │
// │ Item 2         ▶ │  ← Has children
// │ ─────────────────│  ← Separator
// │ Item 3           │
// └──────────────────┘
```

### Width Calculation

`MenuItem.Width` returns the maximum width needed for children:

```csharp
public int Width => Children.Count > 0
    ? Children.Max(c => GetCWidth(c.Text))
    : 0;
```

### Focus Management

- Menu container uses `FormattedTextControl` for the bar
- When menu bar focused, show submenus
- When focus leaves, reset `SelectedMenu` to `[0]`
- `focus_last()` returns focus to previous control

## Dependencies

- `Stroke.Layout.Containers` (Feature 25) - Container classes
- `Stroke.Layout.Controls` (Feature 26) - FormattedTextControl
- `Stroke.Layout.Float` (Feature 25) - Float containers
- `Stroke.Widgets.Shadow` (Feature 45) - Shadow effect
- `Stroke.Widgets.Border` (Feature 45) - Border characters
- `Stroke.KeyBinding` (Feature 19) - Key bindings

## Implementation Tasks

1. Implement `MenuItem` class
2. Implement `MenuItem.Width` property
3. Implement `MenuContainer` class
4. Implement menu bar rendering
5. Implement submenu rendering
6. Implement main menu key bindings
7. Implement submenu key bindings
8. Implement mouse handlers
9. Implement separator rendering
10. Implement disabled item handling
11. Implement focus management
12. Write comprehensive unit tests

## Acceptance Criteria

- [ ] MenuItem stores text, handler, children correctly
- [ ] MenuContainer displays menu bar
- [ ] Left/Right navigate between top menus
- [ ] Down opens submenu
- [ ] Enter activates menu items
- [ ] Submenus display correctly
- [ ] Nested submenus work (up to 3 levels)
- [ ] Separators render as lines
- [ ] Disabled items are skipped
- [ ] Mouse hovering selects items
- [ ] Clicking activates items
- [ ] Focus returns to body on close
- [ ] Unit tests achieve 80% coverage
