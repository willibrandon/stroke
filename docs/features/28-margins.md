# Feature 28: Window Margins

## Overview

Implement the margin system for displaying line numbers, scrollbars, and custom content alongside Window content.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/margins.py`

## Public API

### IMargin Interface

```csharp
namespace Stroke.Layout;

/// <summary>
/// Base interface for a margin.
/// </summary>
public interface IMargin
{
    /// <summary>
    /// Return the width that this margin will consume.
    /// </summary>
    /// <param name="getUIContent">Callable that creates UIContent.</param>
    int GetWidth(Func<UIContent> getUIContent);

    /// <summary>
    /// Create the margin content.
    /// </summary>
    /// <param name="windowRenderInfo">Render info from the window.</param>
    /// <param name="width">The width available.</param>
    /// <param name="height">The height available.</param>
    StyleAndTextTuples CreateMargin(
        WindowRenderInfo windowRenderInfo,
        int width,
        int height);
}
```

### NumberedMargin Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Margin that displays line numbers.
/// </summary>
public sealed class NumberedMargin : IMargin
{
    /// <summary>
    /// Creates a numbered margin.
    /// </summary>
    /// <param name="relative">
    /// Show numbers relative to cursor position (like Vi relativenumber).
    /// </param>
    /// <param name="displayTildes">
    /// Display tildes after end of document (like Vi).
    /// </param>
    public NumberedMargin(object? relative = null, object? displayTildes = null);

    /// <summary>
    /// Filter for relative numbering.
    /// </summary>
    public IFilter Relative { get; }

    /// <summary>
    /// Filter for displaying tildes.
    /// </summary>
    public IFilter DisplayTildes { get; }

    // IMargin implementation...
}
```

### ScrollbarMargin Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Margin displaying a scrollbar.
/// </summary>
public sealed class ScrollbarMargin : IMargin
{
    /// <summary>
    /// Creates a scrollbar margin.
    /// </summary>
    /// <param name="displayArrows">Display scroll up/down arrows.</param>
    /// <param name="upArrowSymbol">Symbol for up arrow.</param>
    /// <param name="downArrowSymbol">Symbol for down arrow.</param>
    public ScrollbarMargin(
        object? displayArrows = null,
        string upArrowSymbol = "^",
        string downArrowSymbol = "v");

    /// <summary>
    /// Filter for displaying arrows.
    /// </summary>
    public IFilter DisplayArrows { get; }

    /// <summary>
    /// Up arrow symbol.
    /// </summary>
    public string UpArrowSymbol { get; }

    /// <summary>
    /// Down arrow symbol.
    /// </summary>
    public string DownArrowSymbol { get; }

    // IMargin implementation...
}
```

### ConditionalMargin Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Wrapper around other Margin classes to show/hide them.
/// </summary>
public sealed class ConditionalMargin : IMargin
{
    /// <summary>
    /// Creates a conditional margin.
    /// </summary>
    /// <param name="margin">The margin to wrap.</param>
    /// <param name="filter">Filter for visibility.</param>
    public ConditionalMargin(IMargin margin, object? filter = null);

    /// <summary>
    /// The wrapped margin.
    /// </summary>
    public IMargin Margin { get; }

    /// <summary>
    /// The visibility filter.
    /// </summary>
    public IFilter Filter { get; }

    // IMargin implementation...
}
```

### PromptMargin Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Margin that displays the prompt.
/// </summary>
public sealed class PromptMargin : IMargin
{
    /// <summary>
    /// Creates a prompt margin.
    /// </summary>
    /// <param name="getPrompt">Callable that returns the prompt.</param>
    /// <param name="getContinuationPrompt">Callable that returns continuation prompt.</param>
    public PromptMargin(
        Func<FormattedText> getPrompt,
        Func<int, int, bool, FormattedText>? getContinuationPrompt = null);

    /// <summary>
    /// Callable that returns the prompt.
    /// </summary>
    public Func<FormattedText> GetPrompt { get; }

    /// <summary>
    /// Callable that returns the continuation prompt.
    /// Called with (line_number, wrap_count, is_soft_wrap).
    /// </summary>
    public Func<int, int, bool, FormattedText>? GetContinuationPrompt { get; }

    // IMargin implementation...
}
```

## Project Structure

```
src/Stroke/
└── Layout/
    ├── IMargin.cs
    ├── NumberedMargin.cs
    ├── ScrollbarMargin.cs
    ├── ConditionalMargin.cs
    └── PromptMargin.cs
tests/Stroke.Tests/
└── Layout/
    ├── NumberedMarginTests.cs
    ├── ScrollbarMarginTests.cs
    ├── ConditionalMarginTests.cs
    └── PromptMarginTests.cs
```

## Implementation Notes

### NumberedMargin Width Calculation

The width is calculated as:
```csharp
int lineCount = getUIContent().LineCount;
int width = Math.Max(3, lineCount.ToString().Length + 1);
```

This ensures minimum width of 3 and accounts for padding.

### NumberedMargin Rendering

For each visible line:
1. Check if this is a continuation of the previous line
2. If same line number, skip (don't repeat)
3. If current line, use `class:line-number.current` style
4. If relative mode, show distance from cursor
5. Right-justify the number within the width

### ScrollbarMargin Rendering

Scrollbar is calculated as:
1. Calculate fraction of content visible
2. Calculate fraction of content above viewport
3. Determine scrollbar height (proportional to visible fraction)
4. Determine scrollbar position (proportional to scroll position)
5. Render `class:scrollbar.button` for the thumb
6. Render `class:scrollbar` for the background
7. Optionally render arrows at top/bottom

### ConditionalMargin Behavior

When filter returns false:
- `GetWidth` returns 0
- `CreateMargin` returns empty list

When filter returns true:
- Delegates to wrapped margin

### PromptMargin Behavior

The prompt margin:
1. For line 0, wrap_count 0: show main prompt
2. For other lines: show continuation prompt if provided
3. Continuation prompt receives (line_number, wrap_count, is_soft_wrap)

## Styles

### NumberedMargin Styles
- `class:line-number` - Normal line numbers
- `class:line-number.current` - Current line number
- `class:tilde` - Tildes after content

### ScrollbarMargin Styles
- `class:scrollbar` - Scrollbar background
- `class:scrollbar.button` - Scrollbar thumb
- `class:scrollbar.arrow` - Scroll arrows

## Dependencies

- `Stroke.Layout.UIContent` (Feature 26) - UI content
- `Stroke.Layout.WindowRenderInfo` (Feature 27) - Window render info
- `Stroke.Core.FormattedText` (Feature 13) - Formatted text
- `Stroke.Filters` (Feature 12) - Filter system

## Implementation Tasks

1. Implement `IMargin` interface
2. Implement `NumberedMargin` class
3. Implement `ScrollbarMargin` class
4. Implement `ConditionalMargin` class
5. Implement `PromptMargin` class
6. Implement relative line numbering
7. Implement scrollbar positioning
8. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All margin types match Python Prompt Toolkit semantics
- [ ] NumberedMargin displays correct line numbers
- [ ] ScrollbarMargin displays correct position
- [ ] ConditionalMargin hides/shows correctly
- [ ] PromptMargin displays prompts correctly
- [ ] Relative numbering works correctly
- [ ] Unit tests achieve 80% coverage
