# Feature 70: Shortcut Utilities

## Overview

Implement high-level shortcut functions for printing formatted text, clearing the screen, setting the terminal title, and rendering containers non-interactively.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/shortcuts/utils.py`

## Public API

### Print Functions

```csharp
namespace Stroke.Shortcuts;

public static class ShortcutUtils
{
    /// <summary>
    /// Print text to stdout with formatting support.
    /// Compatible with Python's print() but supports FormattedText, HTML, and ANSI.
    /// If an Application is running, prints above the application (like patch_stdout).
    /// </summary>
    /// <param name="values">Objects to print. Can be FormattedText, HTML, ANSI, or plain objects.</param>
    /// <param name="sep">Separator between values (default: space).</param>
    /// <param name="end">String appended after last value (default: newline).</param>
    /// <param name="file">Optional TextWriter for output.</param>
    /// <param name="flush">Whether to flush output after printing.</param>
    /// <param name="style">Style instance for color scheme.</param>
    /// <param name="output">Optional Output instance.</param>
    /// <param name="colorDepth">Color depth to use.</param>
    /// <param name="styleTransformation">Style transformation to apply.</param>
    /// <param name="includeDefaultPygmentsStyle">Include default syntax highlighting style.</param>
    public static void PrintFormattedText(
        params object[] values);

    public static void PrintFormattedText(
        object[] values,
        string sep = " ",
        string end = "\n",
        TextWriter? file = null,
        bool flush = false,
        IStyle? style = null,
        IOutput? output = null,
        ColorDepth? colorDepth = null,
        IStyleTransformation? styleTransformation = null,
        bool includeDefaultPygmentsStyle = true);

    /// <summary>
    /// Print any layout container to output in a non-interactive way.
    /// Useful for rendering complex layouts without user interaction.
    /// </summary>
    /// <param name="container">The container to render.</param>
    /// <param name="file">Optional TextWriter for output.</param>
    /// <param name="style">Style instance for the rendering.</param>
    /// <param name="includeDefaultPygmentsStyle">Include default syntax highlighting style.</param>
    public static void PrintContainer(
        IContainer container,
        TextWriter? file = null,
        IStyle? style = null,
        bool includeDefaultPygmentsStyle = true);

    /// <summary>
    /// Clear the terminal screen.
    /// </summary>
    public static void Clear();

    /// <summary>
    /// Set the terminal title.
    /// </summary>
    /// <param name="text">Title text to display.</param>
    public static void SetTitle(string text);

    /// <summary>
    /// Clear the terminal title (set to empty string).
    /// </summary>
    public static void ClearTitle();
}
```

## Project Structure

```
src/Stroke/
└── Shortcuts/
    └── ShortcutUtils.cs
tests/Stroke.Tests/
└── Shortcuts/
    └── ShortcutUtilsTests.cs
```

## Implementation Notes

### PrintFormattedText

```csharp
public static void PrintFormattedText(
    object[] values,
    string sep = " ",
    string end = "\n",
    TextWriter? file = null,
    bool flush = false,
    IStyle? style = null,
    IOutput? output = null,
    ColorDepth? colorDepth = null,
    IStyleTransformation? styleTransformation = null,
    bool includeDefaultPygmentsStyle = true)
{
    if (output != null && file != null)
        throw new ArgumentException("Cannot specify both output and file");

    // Create Output object
    if (output == null)
    {
        output = file != null
            ? OutputFactory.Create(file)
            : AppSession.Current.Output;
    }

    // Get color depth
    colorDepth ??= output.GetDefaultColorDepth();

    // Convert values to formatted text
    StyleAndTextTuples ToText(object? val)
    {
        // Normal lists are treated as plain text
        if (val is IList list && val is not FormattedText)
            return FormattedTextConverter.ToFormattedText($"{list}");
        return FormattedTextConverter.ToFormattedText(val, autoConvert: true);
    }

    var fragments = new List<(string Style, string Text)>();

    for (var i = 0; i < values.Length; i++)
    {
        fragments.AddRange(ToText(values[i]));

        if (!string.IsNullOrEmpty(sep) && i != values.Length - 1)
            fragments.AddRange(ToText(sep));
    }

    fragments.AddRange(ToText(end));

    // Create merged style
    var mergedStyle = CreateMergedStyle(style, includeDefaultPygmentsStyle);

    void Render()
    {
        Renderer.PrintFormattedText(
            output,
            fragments,
            mergedStyle,
            colorDepth.Value,
            styleTransformation);

        if (flush)
            output.Flush();
    }

    // If an application is running, print above the app
    var app = Application.GetCurrentOrNull();
    if (app?.Loop != null)
    {
        app.Loop.CallSoonThreadsafe(() =>
            app.RunInTerminal(Render));
    }
    else
    {
        Render();
    }
}
```

### PrintContainer

```csharp
public static void PrintContainer(
    IContainer container,
    TextWriter? file = null,
    IStyle? style = null,
    bool includeDefaultPygmentsStyle = true)
{
    var output = file != null
        ? OutputFactory.Create(file)
        : AppSession.Current.Output;

    var mergedStyle = CreateMergedStyle(style, includeDefaultPygmentsStyle);

    var app = new Application<object?>(
        layout: new Layout(container),
        output: output,
        input: new DummyInput(),
        style: mergedStyle);

    try
    {
        app.Run(inThread: true);
    }
    catch (EndOfStreamException)
    {
        // Expected when using DummyInput
    }
}
```

### CreateMergedStyle

```csharp
private static IStyle CreateMergedStyle(
    IStyle? style,
    bool includeDefaultPygmentsStyle)
{
    var styles = new List<IStyle> { Styles.DefaultUiStyle() };

    if (includeDefaultPygmentsStyle)
        styles.Add(Styles.DefaultPygmentsStyle());

    if (style != null)
        styles.Add(style);

    return StyleMerger.Merge(styles);
}
```

### Clear

```csharp
public static void Clear()
{
    var output = AppSession.Current.Output;
    output.EraseScreen();
    output.CursorGoto(0, 0);
    output.Flush();
}
```

### SetTitle and ClearTitle

```csharp
public static void SetTitle(string text)
{
    var output = AppSession.Current.Output;
    output.SetTitle(text);
}

public static void ClearTitle()
{
    SetTitle("");
}
```

### Usage Examples

```csharp
// Simple text printing
ShortcutUtils.PrintFormattedText("Hello, World!");

// Formatted text with HTML
ShortcutUtils.PrintFormattedText(
    new Html("<b>Bold</b> and <i>italic</i>"));

// With custom style
var style = Style.FromDict(new Dictionary<string, string>
{
    ["hello"] = "#ff0066",
    ["world"] = "#884444 italic"
});
ShortcutUtils.PrintFormattedText(
    new Html("<hello>Hello</hello> <world>world</world>!"),
    style: style);

// Print a container
ShortcutUtils.PrintContainer(
    new Frame(new TextArea(text: "Hello world!")));

// Terminal control
ShortcutUtils.SetTitle("My Application");
ShortcutUtils.Clear();
ShortcutUtils.ClearTitle();
```

### Integration with Running Application

When an Application is running, `PrintFormattedText` automatically:
1. Erases the current application display
2. Prints the text
3. Re-renders the application

This provides seamless output without requiring `patch_stdout`.

## Dependencies

- `Stroke.Rendering.Renderer` (Feature 57) - PrintFormattedText function
- `Stroke.Application` (Feature 37) - Application lifecycle
- `Stroke.Application.AppSession` (Feature 49) - Current session output
- `Stroke.Styles` (Feature 30) - Style merging
- `Stroke.Layout` (Feature 22) - Container rendering
- `Stroke.Input.DummyInput` (Feature 08) - Non-interactive input

## Implementation Tasks

1. Implement `PrintFormattedText` with all overloads
2. Implement `PrintContainer` for non-interactive rendering
3. Implement `Clear` screen function
4. Implement `SetTitle` and `ClearTitle`
5. Implement `CreateMergedStyle` helper
6. Handle running application integration
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] PrintFormattedText supports plain text
- [ ] PrintFormattedText supports FormattedText, HTML, ANSI
- [ ] PrintFormattedText uses sep and end correctly
- [ ] PrintFormattedText can write to custom TextWriter
- [ ] PrintFormattedText respects color depth
- [ ] PrintFormattedText works with running Application
- [ ] PrintContainer renders containers non-interactively
- [ ] Clear erases screen and moves cursor to origin
- [ ] SetTitle sets terminal title
- [ ] ClearTitle clears terminal title
- [ ] Unit tests achieve 80% coverage
