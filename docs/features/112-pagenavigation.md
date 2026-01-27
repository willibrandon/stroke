# Feature 112: Page Navigation Bindings

## Overview

Implement page navigation key bindings for scrolling through long content, including Emacs and Vi style bindings for page up/down, half-page scrolling, and line-by-line scrolling.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/page_navigation.py`

## Public API

### LoadPageNavigationBindings

```csharp
namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Page navigation key bindings for scrolling through content.
/// </summary>
public static class PageNavigationBindings
{
    /// <summary>
    /// Load both Vi and Emacs bindings for page navigation.
    /// Only active when a Buffer is focused.
    /// </summary>
    /// <returns>Combined page navigation key bindings.</returns>
    public static IKeyBindings LoadPageNavigationBindings();

    /// <summary>
    /// Load Emacs-style page navigation bindings.
    /// </summary>
    /// <returns>Emacs page navigation key bindings.</returns>
    /// <remarks>
    /// Bindings:
    /// - Ctrl+V / PageDown: Scroll page down
    /// - Escape+V / PageUp: Scroll page up
    /// </remarks>
    public static IKeyBindings LoadEmacsPageNavigationBindings();

    /// <summary>
    /// Load Vi-style page navigation bindings.
    /// </summary>
    /// <returns>Vi page navigation key bindings.</returns>
    /// <remarks>
    /// Bindings:
    /// - Ctrl+F: Scroll forward (full page)
    /// - Ctrl+B: Scroll backward (full page)
    /// - Ctrl+D: Scroll half page down
    /// - Ctrl+U: Scroll half page up
    /// - Ctrl+E: Scroll one line down
    /// - Ctrl+Y: Scroll one line up
    /// - PageDown: Scroll page down
    /// - PageUp: Scroll page up
    /// </remarks>
    public static IKeyBindings LoadViPageNavigationBindings();
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Bindings/
        └── PageNavigationBindings.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        └── PageNavigationBindingsTests.cs
```

## Implementation Notes

### PageNavigationBindings Implementation

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class PageNavigationBindings
{
    public static IKeyBindings LoadPageNavigationBindings()
    {
        // Only enable when a Buffer is focused
        return new ConditionalKeyBindings(
            KeyBindings.Merge(
                LoadEmacsPageNavigationBindings(),
                LoadViPageNavigationBindings()
            ),
            Filters.BufferHasFocus
        );
    }

    public static IKeyBindings LoadEmacsPageNavigationBindings()
    {
        var kb = new KeyBindings();

        kb.Add(Keys.ControlV, ScrollBindings.ScrollPageDown);
        kb.Add(Keys.PageDown, ScrollBindings.ScrollPageDown);
        kb.Add(Keys.Escape, Keys.V, ScrollBindings.ScrollPageUp);
        kb.Add(Keys.PageUp, ScrollBindings.ScrollPageUp);

        return new ConditionalKeyBindings(kb, Filters.EmacsMode);
    }

    public static IKeyBindings LoadViPageNavigationBindings()
    {
        var kb = new KeyBindings();

        kb.Add(Keys.ControlF, ScrollBindings.ScrollForward);
        kb.Add(Keys.ControlB, ScrollBindings.ScrollBackward);
        kb.Add(Keys.ControlD, ScrollBindings.ScrollHalfPageDown);
        kb.Add(Keys.ControlU, ScrollBindings.ScrollHalfPageUp);
        kb.Add(Keys.ControlE, ScrollBindings.ScrollOneLineDown);
        kb.Add(Keys.ControlY, ScrollBindings.ScrollOneLineUp);
        kb.Add(Keys.PageDown, ScrollBindings.ScrollPageDown);
        kb.Add(Keys.PageUp, ScrollBindings.ScrollPageUp);

        return new ConditionalKeyBindings(kb, Filters.ViMode);
    }
}
```

### Scroll Functions Reference

These bindings use scroll functions from Feature 61 (ScrollBindings):

```csharp
namespace Stroke.KeyBinding.Bindings;

public static partial class ScrollBindings
{
    // From scroll.py - used by page navigation
    public static void ScrollForward(KeyPressEvent e);
    public static void ScrollBackward(KeyPressEvent e);
    public static void ScrollHalfPageDown(KeyPressEvent e);
    public static void ScrollHalfPageUp(KeyPressEvent e);
    public static void ScrollOneLineDown(KeyPressEvent e);
    public static void ScrollOneLineUp(KeyPressEvent e);
    public static void ScrollPageDown(KeyPressEvent e);
    public static void ScrollPageUp(KeyPressEvent e);
}
```

### Usage Example

```csharp
// Include page navigation in application
var app = new Application(
    layout: myLayout,
    keyBindings: KeyBindings.Merge(
        BasicBindings.LoadBasicBindings(),
        PageNavigationBindings.LoadPageNavigationBindings()
    )
);

// Or include just Vi bindings
var viApp = new Application(
    layout: myLayout,
    editingMode: EditingMode.Vi,
    keyBindings: KeyBindings.Merge(
        BasicBindings.LoadBasicBindings(),
        PageNavigationBindings.LoadViPageNavigationBindings()
    )
);
```

## Dependencies

- Feature 121: App Filters (BufferHasFocus, EmacsMode, ViMode)
- Feature 19: Key Bindings (ConditionalKeyBindings)
- Feature 61: Scroll Bindings

## Implementation Tasks

1. Implement LoadEmacsPageNavigationBindings
2. Implement LoadViPageNavigationBindings
3. Implement LoadPageNavigationBindings (combined)
4. Wire to scroll functions from ScrollBindings
5. Apply conditional filters
6. Write unit tests

## Acceptance Criteria

- [ ] Emacs bindings work in Emacs mode
- [ ] Vi bindings work in Vi mode
- [ ] Bindings only active when Buffer has focus
- [ ] All scroll functions are correctly mapped
- [ ] PageUp/PageDown work in both modes
- [ ] Unit tests achieve 80% coverage
