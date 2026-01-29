# Contract: IMargin Interface

**Namespace**: `Stroke.Layout.Margins`
**Python Equivalent**: `prompt_toolkit.layout.margins.Margin`

## Interface Definition

```csharp
/// <summary>
/// Interface for Window margins (line numbers, scrollbars, prompts).
/// </summary>
/// <remarks>
/// <para>
/// Margins are rendered alongside the main Window content. Left margins appear
/// to the left of the content, right margins to the right. Common uses include
/// line numbers, scrollbars, and continuation prompts.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Margin</c> abstract base class from
/// <c>layout/margins.py</c>.
/// </para>
/// </remarks>
public interface IMargin
{
    /// <summary>
    /// Calculate the width this margin requires.
    /// </summary>
    /// <param name="getUIContent">
    /// Function that returns the UIContent being rendered.
    /// Call this to determine width based on content (e.g., line count for line numbers).
    /// </param>
    /// <returns>Width in character cells.</returns>
    int GetWidth(Func<UIContent> getUIContent);

    /// <summary>
    /// Create the margin content for rendering.
    /// </summary>
    /// <param name="windowRenderInfo">
    /// Render information from the Window, including scroll state and line mappings.
    /// </param>
    /// <param name="width">Width allocated for this margin.</param>
    /// <param name="height">Height of the visible area.</param>
    /// <returns>
    /// Formatted text tuples for the entire margin area.
    /// Should contain newlines to separate rows.
    /// </returns>
    IReadOnlyList<StyleAndTextTuple> CreateMargin(
        WindowRenderInfo windowRenderInfo,
        int width,
        int height);
}
```

## Implementations

### NumberedMargin

```csharp
/// <summary>
/// Displays line numbers in the margin.
/// </summary>
public sealed class NumberedMargin : IMargin
{
    /// <summary>
    /// When true, show line numbers relative to cursor position.
    /// </summary>
    public IFilter Relative { get; }

    /// <summary>
    /// When true, show tildes (~) below the document end (Vi style).
    /// </summary>
    public IFilter DisplayTildes { get; }

    public NumberedMargin(
        FilterOrBool relative = default,
        FilterOrBool displayTildes = default);

    // GetWidth: calculates digits needed for line count
    // CreateMargin: renders right-justified line numbers, highlights current line
}
```

### ScrollbarMargin

```csharp
/// <summary>
/// Displays a vertical scrollbar.
/// </summary>
public sealed class ScrollbarMargin : IMargin
{
    /// <summary>
    /// When true, show up/down arrow symbols at top/bottom.
    /// </summary>
    public IFilter DisplayArrows { get; }

    /// <summary>
    /// Symbol for up arrow (default: "^").
    /// </summary>
    public string UpArrowSymbol { get; }

    /// <summary>
    /// Symbol for down arrow (default: "v").
    /// </summary>
    public string DownArrowSymbol { get; }

    public ScrollbarMargin(
        FilterOrBool displayArrows = default,
        string upArrowSymbol = "^",
        string downArrowSymbol = "v");

    // GetWidth: always returns 1
    // CreateMargin: calculates scrollbar thumb position from WindowRenderInfo
}
```

### ConditionalMargin

```csharp
/// <summary>
/// Shows or hides another margin based on a filter condition.
/// </summary>
public sealed class ConditionalMargin : IMargin
{
    /// <summary>
    /// The margin to conditionally display.
    /// </summary>
    public IMargin Margin { get; }

    /// <summary>
    /// When false, margin is hidden (GetWidth returns 0).
    /// </summary>
    public IFilter Filter { get; }

    public ConditionalMargin(IMargin margin, FilterOrBool filter);

    // GetWidth: returns Margin.GetWidth() if Filter is true, else 0
    // CreateMargin: delegates to Margin if Filter is true, else empty
}
```

### PromptMargin

```csharp
/// <summary>
/// Displays a prompt on the first line and continuation on subsequent lines.
/// </summary>
[Obsolete("Use Window.GetLinePrefix instead")]
public sealed class PromptMargin : IMargin
{
    /// <summary>
    /// Function returning the prompt for the first line.
    /// </summary>
    public Func<IReadOnlyList<StyleAndTextTuple>> GetPrompt { get; }

    /// <summary>
    /// Function returning continuation for subsequent lines.
    /// Parameters: (lineNumber, wrapCount, isLastLine).
    /// </summary>
    public Func<int, int, bool, IReadOnlyList<StyleAndTextTuple>>? GetContinuation { get; }

    public PromptMargin(
        Func<IReadOnlyList<StyleAndTextTuple>> getPrompt,
        Func<int, int, bool, IReadOnlyList<StyleAndTextTuple>>? getContinuation = null);
}
```

## Usage Examples

### Line Numbers

```csharp
var window = new Window(
    content: control,
    leftMargins: [new NumberedMargin()]);

// Output:
//   1 │ First line
//   2 │ Second line
//   3 │ Third line
```

### Relative Line Numbers (Vi style)

```csharp
var window = new Window(
    content: control,
    leftMargins: [new NumberedMargin(relative: true, displayTildes: true)]);

// With cursor on line 5:
//   3 │ ...
//   2 │ ...
//   1 │ ...
//   5 │ Current line (highlighted)
//   1 │ ...
//   2 │ ...
//   ~ │
```

### Scrollbar

```csharp
var window = new Window(
    content: control,
    rightMargins: [new ScrollbarMargin(displayArrows: true)]);

// Output:
// ^
// ░
// █ (thumb position)
// ░
// v
```

### Conditional Margin

```csharp
var showLineNumbers = new Condition(() => settings.ShowLineNumbers);
var window = new Window(
    content: control,
    leftMargins: [new ConditionalMargin(new NumberedMargin(), showLineNumbers)]);
```

## Margin Styling

Margins use specific style classes:

| Element | Style Class |
|---------|-------------|
| Line numbers | `class:line-number` |
| Current line number | `class:line-number,current-line-number` |
| Tildes | `class:tilde` |
| Scrollbar background | `class:scrollbar.background` |
| Scrollbar button (thumb) | `class:scrollbar.button` |
| Scrollbar arrow | `class:scrollbar.arrow` |
| Prompt | User-defined |
| Continuation | User-defined |

## Thread Safety

Margin implementations should be stateless. All state comes from WindowRenderInfo at render time.

## Related Contracts

- [Window.md](./Window.md) - Window container
- [WindowRenderInfo.md](./WindowRenderInfo.md) - Render state for margin access
