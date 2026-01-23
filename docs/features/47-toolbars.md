# Feature 47: Toolbars

## Overview

Implement the toolbar widgets for displaying contextual information including FormattedTextToolbar, SystemToolbar, ArgToolbar, SearchToolbar, CompletionsToolbar, and ValidationToolbar.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/widgets/toolbars.py`

## Public API

### FormattedTextToolbar Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Window that displays formatted text. Used for simple toolbars.
/// </summary>
public sealed class FormattedTextToolbar : Window
{
    /// <summary>
    /// Creates a FormattedTextToolbar.
    /// </summary>
    /// <param name="text">The formatted text to display.</param>
    /// <param name="style">Style string for the toolbar.</param>
    public FormattedTextToolbar(
        AnyFormattedText text,
        string style = "");
}
```

### SystemToolbar Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Toolbar for executing system shell commands.
/// </summary>
public sealed class SystemToolbar : IContainer
{
    /// <summary>
    /// Creates a SystemToolbar.
    /// </summary>
    /// <param name="prompt">Prompt text shown before input.</param>
    /// <param name="enableGlobalBindings">Enable global key bindings.</param>
    public SystemToolbar(
        AnyFormattedText prompt = default,  // "Shell command: "
        IFilter? enableGlobalBindings = null);

    /// <summary>
    /// The prompt text.
    /// </summary>
    public AnyFormattedText Prompt { get; }

    /// <summary>
    /// Filter for enabling global bindings.
    /// </summary>
    public IFilter EnableGlobalBindings { get; }

    /// <summary>
    /// The system command buffer.
    /// </summary>
    public Buffer SystemBuffer { get; }

    /// <summary>
    /// The buffer control.
    /// </summary>
    public BufferControl BufferControl { get; }

    /// <summary>
    /// The window.
    /// </summary>
    public Window Window { get; }

    /// <summary>
    /// The container.
    /// </summary>
    public ConditionalContainer Container { get; }

    Container IContainer.GetContainer();
}
```

### ArgToolbar Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Toolbar showing the current repeat argument count.
/// </summary>
public sealed class ArgToolbar : IContainer
{
    /// <summary>
    /// Creates an ArgToolbar.
    /// </summary>
    public ArgToolbar();

    /// <summary>
    /// The window.
    /// </summary>
    public Window Window { get; }

    /// <summary>
    /// The container (conditional on has_arg).
    /// </summary>
    public ConditionalContainer Container { get; }

    Container IContainer.GetContainer();
}
```

### SearchToolbar Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Toolbar for incremental search input.
/// </summary>
public sealed class SearchToolbar : IContainer
{
    /// <summary>
    /// Creates a SearchToolbar.
    /// </summary>
    /// <param name="searchBuffer">Buffer for search input.</param>
    /// <param name="viMode">Display '/' and '?' instead of I-search.</param>
    /// <param name="textIfNotSearching">Text to show when not searching.</param>
    /// <param name="forwardSearchPrompt">Prompt for forward search.</param>
    /// <param name="backwardSearchPrompt">Prompt for backward search.</param>
    /// <param name="ignoreCase">Case insensitive search filter.</param>
    public SearchToolbar(
        Buffer? searchBuffer = null,
        bool viMode = false,
        AnyFormattedText textIfNotSearching = default,
        AnyFormattedText forwardSearchPrompt = default,  // "I-search: "
        AnyFormattedText backwardSearchPrompt = default,  // "I-search backward: "
        IFilter? ignoreCase = null);

    /// <summary>
    /// The search buffer.
    /// </summary>
    public Buffer SearchBuffer { get; }

    /// <summary>
    /// The search buffer control.
    /// </summary>
    public SearchBufferControl Control { get; }

    /// <summary>
    /// The container.
    /// </summary>
    public ConditionalContainer Container { get; }

    Container IContainer.GetContainer();
}
```

### CompletionsToolbar Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Toolbar showing completions in a horizontal list.
/// </summary>
public sealed class CompletionsToolbar : IContainer
{
    /// <summary>
    /// Creates a CompletionsToolbar.
    /// </summary>
    public CompletionsToolbar();

    /// <summary>
    /// The container (conditional on has_completions).
    /// </summary>
    public ConditionalContainer Container { get; }

    Container IContainer.GetContainer();
}
```

### ValidationToolbar Class

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Toolbar showing validation errors.
/// </summary>
public sealed class ValidationToolbar : IContainer
{
    /// <summary>
    /// Creates a ValidationToolbar.
    /// </summary>
    /// <param name="showPosition">Show line and column in error message.</param>
    public ValidationToolbar(bool showPosition = false);

    /// <summary>
    /// The text control.
    /// </summary>
    public FormattedTextControl Control { get; }

    /// <summary>
    /// The container (conditional on has_validation_error).
    /// </summary>
    public ConditionalContainer Container { get; }

    Container IContainer.GetContainer();
}
```

## Project Structure

```
src/Stroke/
└── Widgets/
    └── Toolbars/
        ├── FormattedTextToolbar.cs
        ├── SystemToolbar.cs
        ├── ArgToolbar.cs
        ├── SearchToolbar.cs
        ├── CompletionsToolbar.cs
        ├── CompletionsToolbarControl.cs
        └── ValidationToolbar.cs
tests/Stroke.Tests/
└── Widgets/
    └── Toolbars/
        ├── SystemToolbarTests.cs
        ├── ArgToolbarTests.cs
        ├── SearchToolbarTests.cs
        ├── CompletionsToolbarTests.cs
        └── ValidationToolbarTests.cs
```

## Implementation Notes

### FormattedTextToolbar

Simple Window subclass with:
- `FormattedTextControl` for content
- `dontExtendHeight: true`
- `height: Dimension(min: 1)`

### SystemToolbar Key Bindings

**Emacs mode:**
- `Escape/Ctrl-G/Ctrl-C`: Cancel and hide
- `Enter`: Run system command

**Vi mode:**
- `Escape/Ctrl-C`: Cancel, set navigation mode, hide
- `Enter`: Run command, set navigation mode

**Global bindings (when not focused):**
- `M-!` (Emacs): Focus system toolbar
- `!` (Vi navigation): Focus system toolbar, switch to insert

### ArgToolbar Display

```
Repeat: {arg}
```

Shows when `has_arg` filter is true. Displays the repeat count (e.g., "5" for `5dd`).

### SearchToolbar Display

```
I-search: {query}           (forward, emacs)
I-search backward: {query}  (backward, emacs)
/                           (forward, vi)
?                           (backward, vi)
```

Shows when `is_searching` condition is true (control in search_links).

### CompletionsToolbar Layout

```
 < completion1 completion2 completion3 >
```

- Shows `<` when completions trimmed from left
- Shows `>` when completions trimmed from right
- Current completion highlighted with `class:completion-toolbar.completion.current`
- Width calculation: `content_width = width - 6` (for arrows and spacing)

### ValidationToolbar Display

```
Error message (line=1 column=5)  (if showPosition)
Error message                    (if !showPosition)
```

Shows when `has_validation_error` filter is true.

### Style Classes

- `class:system-toolbar`
- `class:system-toolbar.text`
- `class:search-toolbar`
- `class:search-toolbar.prompt`
- `class:search-toolbar.text`
- `class:arg-toolbar`
- `class:arg-toolbar.text`
- `class:completion-toolbar`
- `class:completion-toolbar.completion`
- `class:completion-toolbar.completion.current`
- `class:completion-toolbar.arrow`
- `class:validation-toolbar`

## Dependencies

- `Stroke.Layout.Containers` (Feature 25) - Container classes
- `Stroke.Layout.Controls` (Feature 26) - Control classes
- `Stroke.Layout.Window` (Feature 27) - Window class
- `Stroke.Core.Buffer` (Feature 06) - Buffer class
- `Stroke.KeyBinding` (Feature 19) - Key bindings
- `Stroke.Filters` (Feature 12) - Filter system
- `Stroke.Search` (Feature 34) - Search direction

## Implementation Tasks

1. Implement `FormattedTextToolbar` class
2. Implement `SystemToolbar` with key bindings
3. Implement `ArgToolbar` with formatted text
4. Implement `SearchToolbar` with search control
5. Implement `_CompletionsToolbarControl` UIControl
6. Implement `CompletionsToolbar` class
7. Implement `ValidationToolbar` class
8. Write comprehensive unit tests

## Acceptance Criteria

- [ ] FormattedTextToolbar displays text correctly
- [ ] SystemToolbar accepts shell commands
- [ ] SystemToolbar key bindings work (Emacs and Vi)
- [ ] ArgToolbar shows repeat count
- [ ] SearchToolbar shows appropriate prompts
- [ ] CompletionsToolbar paginates completions
- [ ] ValidationToolbar shows error messages
- [ ] All toolbars conditionally visible
- [ ] Unit tests achieve 80% coverage
