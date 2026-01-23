# Feature 105: Dummy Layout

## Overview

Implement the `CreateDummyLayout` utility function - a factory that creates a minimal default layout for use when an Application is created without specifying a layout.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/dummy.py`

## Public API

### CreateDummyLayout

```csharp
namespace Stroke.Layout;

/// <summary>
/// Factory for creating default layouts.
/// </summary>
public static class DummyLayout
{
    /// <summary>
    /// Create a dummy layout for use in an Application that doesn't have
    /// a layout specified. When ENTER is pressed, the application quits.
    /// </summary>
    /// <returns>A minimal layout with quit instruction.</returns>
    /// <remarks>
    /// The layout displays: "No layout specified. Press ENTER to quit."
    /// with "ENTER" shown in reverse video.
    /// </remarks>
    public static Layout CreateDummyLayout();
}
```

## Project Structure

```
src/Stroke/
└── Layout/
    └── DummyLayout.cs
tests/Stroke.Tests/
└── Layout/
    └── DummyLayoutTests.cs
```

## Implementation Notes

### CreateDummyLayout Implementation

```csharp
namespace Stroke.Layout;

public static class DummyLayout
{
    public static Layout CreateDummyLayout()
    {
        // Create key bindings for the dummy layout
        var keyBindings = new KeyBindings();

        // Bind Enter to exit the application
        keyBindings.Add(Keys.Enter, (KeyPressEvent e) =>
        {
            e.App.Exit();
        });

        // Create the content control with formatted message
        var control = new FormattedTextControl(
            text: new HTML(
                "No layout specified. Press <reverse>ENTER</reverse> to quit."
            ),
            keyBindings: keyBindings
        );

        // Create a window with minimum height of 1
        var window = new Window(
            content: control,
            height: Dimension.Exact(1)
        );

        // Return layout with the window focused
        return new Layout(
            container: window,
            focusedElement: window
        );
    }
}
```

### Alternative Implementation Without HTML

```csharp
public static Layout CreateDummyLayout()
{
    var keyBindings = new KeyBindings();

    keyBindings.Add(Keys.Enter, (KeyPressEvent e) =>
    {
        e.App.Exit();
    });

    // Using FormattedText tuples for styling
    var text = new FormattedText(new[]
    {
        ("", "No layout specified. Press "),
        ("class:reverse", "ENTER"),
        ("", " to quit.")
    });

    var control = new FormattedTextControl(
        text: text,
        keyBindings: keyBindings
    );

    var window = new Window(
        content: control,
        height: new Dimension(min: 1)
    );

    return new Layout(
        container: window,
        focusedElement: window
    );
}
```

### Usage in Application

```csharp
public sealed class Application
{
    public Application(
        Layout? layout = null,
        // ... other parameters
    )
    {
        // Use dummy layout if none specified
        Layout = layout ?? DummyLayout.CreateDummyLayout();
        // ...
    }
}
```

### Usage Example

```csharp
// Application with no layout - uses dummy layout
var app = new Application();
await app.RunAsync();  // Shows "No layout specified. Press ENTER to quit."
                       // Pressing Enter exits

// Application with custom layout
var customLayout = new Layout(
    new Window(new FormattedTextControl("Hello, World!"))
);
var app2 = new Application(layout: customLayout);
await app2.RunAsync();
```

## Dependencies

- Feature 25: Containers (Window)
- Feature 26: Controls (FormattedTextControl)
- Feature 19: Key bindings (KeyBindings)
- Feature 14: Layout
- Feature 17: Formatted text (HTML, FormattedText)

## Implementation Tasks

1. Implement CreateDummyLayout function
2. Create key binding for Enter key
3. Create formatted text with reverse video
4. Create Window with minimum height
5. Create Layout with focused element
6. Integrate into Application constructor
7. Write unit tests

## Acceptance Criteria

- [ ] CreateDummyLayout returns valid Layout
- [ ] Layout displays instruction message
- [ ] ENTER is highlighted with reverse video
- [ ] Pressing Enter exits the application
- [ ] Window has minimum height of 1
- [ ] Window is focused by default
- [ ] Application uses dummy layout when none specified
- [ ] Unit tests achieve 80% coverage
