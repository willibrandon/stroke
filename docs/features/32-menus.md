# Feature 32: Completion Menus

## Overview

Implement the completion menu containers that display completions in single-column or multi-column layouts with scrolling and mouse support.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/menus.py`

## Public API

### CompletionsMenuControl Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Control for drawing the completions menu.
/// </summary>
public sealed class CompletionsMenuControl : IUIControl
{
    /// <summary>
    /// Minimum width of the menu control.
    /// </summary>
    public const int MinWidth = 7;

    /// <summary>
    /// Returns false - menu is not focusable.
    /// </summary>
    public bool HasFocus();

    /// <summary>
    /// Return preferred width based on completions.
    /// </summary>
    public int? PreferredWidth(int maxAvailableWidth);

    /// <summary>
    /// Return preferred height (number of completions).
    /// </summary>
    public int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        Func<int, int, FormattedText>? getLinePrefix);

    /// <summary>
    /// Create UIContent for the menu.
    /// </summary>
    public UIContent CreateContent(int width, int height);

    /// <summary>
    /// Handle mouse events (click, scroll).
    /// </summary>
    public NotImplementedOrNone MouseHandler(MouseEvent mouseEvent);
}
```

### CompletionsMenu Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Container that displays completions in a single column menu.
/// </summary>
public sealed class CompletionsMenu : ConditionalContainer
{
    /// <summary>
    /// Creates a CompletionsMenu.
    /// </summary>
    /// <param name="maxHeight">Maximum height of the menu.</param>
    /// <param name="scrollOffset">Number of items to keep visible around cursor.</param>
    /// <param name="extraFilter">Additional filter for visibility.</param>
    /// <param name="displayArrows">Display scroll arrows.</param>
    /// <param name="zIndex">Z-index for layering (default: 10^8).</param>
    public CompletionsMenu(
        int? maxHeight = null,
        object? scrollOffset = null,
        object? extraFilter = null,
        object? displayArrows = null,
        int zIndex = 100_000_000);
}
```

### MultiColumnCompletionMenuControl Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Control for displaying completions in multiple columns.
/// </summary>
public sealed class MultiColumnCompletionMenuControl : IUIControl
{
    /// <summary>
    /// Creates a MultiColumnCompletionMenuControl.
    /// </summary>
    /// <param name="minRows">Minimum rows to display.</param>
    /// <param name="suggestedMaxColumnWidth">Suggested maximum column width.</param>
    public MultiColumnCompletionMenuControl(
        int minRows = 3,
        int suggestedMaxColumnWidth = 30);

    /// <summary>
    /// Minimum number of rows.
    /// </summary>
    public int MinRows { get; }

    /// <summary>
    /// Suggested maximum column width.
    /// </summary>
    public int SuggestedMaxColumnWidth { get; }

    /// <summary>
    /// Current scroll position.
    /// </summary>
    public int Scroll { get; set; }

    /// <summary>
    /// Reset the control state.
    /// </summary>
    public void Reset();

    /// <summary>
    /// Returns false - menu is not focusable.
    /// </summary>
    public bool HasFocus();

    /// <summary>
    /// Return preferred width based on completions.
    /// </summary>
    public int? PreferredWidth(int maxAvailableWidth);

    /// <summary>
    /// Return preferred height for completions.
    /// </summary>
    public int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        Func<int, int, FormattedText>? getLinePrefix);

    /// <summary>
    /// Create UIContent for the menu.
    /// </summary>
    public UIContent CreateContent(int width, int height);

    /// <summary>
    /// Handle mouse events (click, scroll, arrow clicks).
    /// </summary>
    public NotImplementedOrNone MouseHandler(MouseEvent mouseEvent);

    /// <summary>
    /// Get key bindings for left/right navigation.
    /// </summary>
    public IKeyBindingsBase GetKeyBindings();
}
```

### MultiColumnCompletionsMenu Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Container that displays completions in multiple columns.
/// </summary>
public sealed class MultiColumnCompletionsMenu : HSplit
{
    /// <summary>
    /// Creates a MultiColumnCompletionsMenu.
    /// </summary>
    /// <param name="minRows">Minimum rows to display.</param>
    /// <param name="suggestedMaxColumnWidth">Suggested maximum column width.</param>
    /// <param name="showMeta">Show meta information below completions.</param>
    /// <param name="extraFilter">Additional filter for visibility.</param>
    /// <param name="zIndex">Z-index for layering (default: 10^8).</param>
    public MultiColumnCompletionsMenu(
        int minRows = 3,
        int suggestedMaxColumnWidth = 30,
        object? showMeta = null,
        object? extraFilter = null,
        int zIndex = 100_000_000);
}
```

### Helper Functions

```csharp
namespace Stroke.Layout;

/// <summary>
/// Menu utility functions.
/// </summary>
internal static class MenuUtils
{
    /// <summary>
    /// Get styled fragments for a menu item.
    /// </summary>
    /// <param name="completion">The completion item.</param>
    /// <param name="isCurrentCompletion">True if currently selected.</param>
    /// <param name="width">Available width.</param>
    /// <param name="spaceAfter">Add space after item.</param>
    internal static StyleAndTextTuples GetMenuItemFragments(
        Completion completion,
        bool isCurrentCompletion,
        int width,
        bool spaceAfter = false);

    /// <summary>
    /// Trim formatted text to max width, append dots if too long.
    /// </summary>
    /// <param name="formattedText">Text to trim.</param>
    /// <param name="maxWidth">Maximum width.</param>
    /// <returns>Tuple of (trimmed text, actual width).</returns>
    internal static (StyleAndTextTuples Text, int Width) TrimFormattedText(
        StyleAndTextTuples formattedText,
        int maxWidth);
}
```

## Project Structure

```
src/Stroke/
└── Layout/
    ├── Menus/
    │   ├── CompletionsMenuControl.cs
    │   ├── CompletionsMenu.cs
    │   ├── MultiColumnCompletionMenuControl.cs
    │   ├── MultiColumnCompletionsMenu.cs
    │   ├── SelectedCompletionMetaControl.cs
    │   └── MenuUtils.cs
tests/Stroke.Tests/
└── Layout/
    └── Menus/
        ├── CompletionsMenuControlTests.cs
        ├── CompletionsMenuTests.cs
        ├── MultiColumnCompletionMenuControlTests.cs
        ├── MultiColumnCompletionsMenuTests.cs
        └── MenuUtilsTests.cs
```

## Implementation Notes

### CompletionsMenuControl Rendering

1. Get completion state from current buffer
2. Calculate menu width (max of completion display widths + 2)
3. Calculate meta width if any completions have meta
4. For each line:
   - Get completion at index
   - Apply current/normal style
   - Trim text to fit width
   - Add padding
   - Add meta column if needed

### Completion Item Styling

Styles used:
- `class:completion-menu.completion` - Normal completion
- `class:completion-menu.completion.current` - Selected completion
- `class:completion-menu.meta.completion` - Meta text
- `class:completion-menu.meta.completion.current` - Selected meta

### MultiColumnCompletionMenuControl Layout

1. Calculate column width based on widest completion
2. Determine number of visible columns from available width
3. Group completions into columns (row-major order)
4. Track scroll position to show current completion
5. Render left/right arrows when content is hidden
6. Left arrow at `x=0`, right arrow at last column

### Mouse Handling

CompletionsMenuControl:
- `MOUSE_UP`: Select completion and close menu
- `SCROLL_DOWN`: Navigate to next completions (count=3)
- `SCROLL_UP`: Navigate to previous completions (count=3)

MultiColumnCompletionMenuControl:
- `MOUSE_UP`: Select completion or scroll via arrows
- `SCROLL_DOWN/UP`: Scroll columns horizontally
- Arrow clicks: Scroll left/right

### Multi-Column Key Bindings

The control exposes key bindings for navigation:
- `left` (global): Move to previous column
- `right` (global): Move to next column

Bindings are only active when:
- Completions exist
- A completion is selected
- This menu is visible

### Meta Information Display

MultiColumnCompletionsMenu shows meta in a separate row:
- Uses `_SelectedCompletionMetaControl`
- Shows meta of currently selected completion
- Only visible when completions have meta data

### Column Width Calculation

1. Calculate based on max completion display width + 1
2. Cache per completion state to avoid recomputation
3. Respect `suggestedMaxColumnWidth` when possible
4. Divide if there's room for multiple columns

### Performance Optimizations

- Cache column width calculations per CompletionState
- Limit meta width calculation to first 200 completions
- Use WeakKeyDictionary for completion state caching

## Styles

```
class:completion-menu                    - Menu container
class:completion-menu.completion         - Normal completion item
class:completion-menu.completion.current - Selected completion item
class:completion-menu.meta.completion    - Meta text for item
class:completion-menu.meta.completion.current - Meta text for selected
class:completion-menu.multi-column-meta  - Meta row in multi-column
class:scrollbar                          - Scrollbar/arrows
```

## Dependencies

- `Stroke.Layout.Window` (Feature 27) - Window container
- `Stroke.Layout.ConditionalContainer` (Feature 25) - Container visibility
- `Stroke.Layout.HSplit` (Feature 25) - Vertical layout
- `Stroke.Layout.ScrollbarMargin` (Feature 28) - Scrollbar margin
- `Stroke.Layout.Dimension` (Feature 24) - Dimension system
- `Stroke.Completion.Completion` (Feature 35) - Completion class
- `Stroke.Core.Buffer` (Feature 02) - Buffer and CompletionState
- `Stroke.Filters` (Feature 12) - has_completions, is_done filters

## Implementation Tasks

1. Implement `MenuUtils` helper functions
2. Implement `CompletionsMenuControl` class
3. Implement `CompletionsMenu` class
4. Implement `MultiColumnCompletionMenuControl` class
5. Implement `MultiColumnCompletionsMenu` class
6. Implement `_SelectedCompletionMetaControl` class
7. Implement mouse handling
8. Implement key bindings for multi-column navigation
9. Implement column width caching
10. Write comprehensive unit tests

## Acceptance Criteria

- [ ] CompletionsMenu matches Python Prompt Toolkit semantics
- [ ] MultiColumnCompletionsMenu matches Python Prompt Toolkit semantics
- [ ] Mouse handling works (click, scroll)
- [ ] Key navigation works in multi-column mode
- [ ] Meta information displays correctly
- [ ] Scroll arrows display correctly
- [ ] Styling matches expected classes
- [ ] Performance is acceptable for large completion lists
- [ ] Unit tests achieve 80% coverage
