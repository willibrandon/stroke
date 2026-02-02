# API Contracts: Shortcut Utilities

**Feature**: 046-shortcut-utils
**Date**: 2026-02-01
**Python Source**: `prompt_toolkit/shortcuts/utils.py`

## Class: FormattedTextOutput

**Namespace**: `Stroke.Shortcuts`
**Python equivalent**: Module-level functions `print_formatted_text`, `print_container`, `_create_merged_style`

```csharp
/// <summary>
/// High-level functions for printing formatted text and rendering containers
/// directly to the terminal output.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>print_formatted_text</c> and
/// <c>print_container</c> functions from <c>prompt_toolkit.shortcuts.utils</c>.
/// </para>
/// <para>
/// This type is thread-safe. All methods are stateless and delegate to
/// thread-safe infrastructure.
/// </para>
/// </remarks>
public static class FormattedTextOutput
{
    /// <summary>
    /// Print formatted text to the terminal output.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If a prompt_toolkit Application is currently running, this will always
    /// print above the application or prompt. The method will erase the current
    /// application, print the text, and render the application again.
    /// </para>
    /// </remarks>
    /// <param name="text">The formatted text to print (string, HTML, ANSI, FormattedText).</param>
    /// <param name="sep">String inserted between values. Default: space.</param>
    /// <param name="end">String appended after the last value. Default: newline.</param>
    /// <param name="file">Optional TextWriter for output redirection. Mutually exclusive with <paramref name="output"/>.</param>
    /// <param name="flush">Whether to flush the output after printing.</param>
    /// <param name="style">Optional style for rendering.</param>
    /// <param name="output">Optional IOutput for direct output control. Mutually exclusive with <paramref name="file"/>.</param>
    /// <param name="colorDepth">Optional color depth override.</param>
    /// <param name="styleTransformation">Optional style transformation pipeline.</param>
    /// <param name="includeDefaultPygmentsStyle">Whether to include the default Pygments style. Default: true.</param>
    /// <exception cref="ArgumentException">Both <paramref name="output"/> and <paramref name="file"/> are specified.</exception>
    public static void Print(
        AnyFormattedText text,
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
    /// Print multiple values to the terminal output, matching Python's print() semantics.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Values are converted to formatted text fragments, joined with <paramref name="sep"/>,
    /// and terminated with <paramref name="end"/>. Plain lists that are not FormattedText
    /// are converted to their string representation.
    /// </para>
    /// </remarks>
    /// <param name="values">The values to print.</param>
    /// <param name="sep">String inserted between values. Default: space.</param>
    /// <param name="end">String appended after the last value. Default: newline.</param>
    /// <param name="file">Optional TextWriter for output redirection.</param>
    /// <param name="flush">Whether to flush the output after printing.</param>
    /// <param name="style">Optional style for rendering.</param>
    /// <param name="output">Optional IOutput for direct output control.</param>
    /// <param name="colorDepth">Optional color depth override.</param>
    /// <param name="styleTransformation">Optional style transformation pipeline.</param>
    /// <param name="includeDefaultPygmentsStyle">Whether to include the default Pygments style.</param>
    /// <exception cref="ArgumentException">Both <paramref name="output"/> and <paramref name="file"/> are specified.</exception>
    public static void Print(
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
    /// Print any layout container to the output in a non-interactive way.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates a temporary Application with DummyInput, renders the container once,
    /// and terminates. The Application runs on a background thread (in_thread=true).
    /// </para>
    /// </remarks>
    /// <param name="container">The layout container to render.</param>
    /// <param name="file">Optional TextWriter for output redirection.</param>
    /// <param name="style">Optional style for rendering.</param>
    /// <param name="includeDefaultPygmentsStyle">Whether to include the default Pygments style.</param>
    public static void PrintContainer(
        AnyContainer container,
        TextWriter? file = null,
        IStyle? style = null,
        bool includeDefaultPygmentsStyle = true);
}
```

### Private Helper

```csharp
/// <summary>
/// Merge user-defined style with built-in defaults.
/// </summary>
/// <param name="style">User-provided style (highest precedence).</param>
/// <param name="includeDefaultPygmentsStyle">Include default Pygments style.</param>
/// <returns>Merged style with order: default UI, [Pygments], [user].</returns>
private static IStyle CreateMergedStyle(
    IStyle? style,
    bool includeDefaultPygmentsStyle);
```

---

## Class: TerminalUtils

**Namespace**: `Stroke.Shortcuts`
**Python equivalent**: Module-level functions `clear`, `set_title`, `clear_title`

```csharp
/// <summary>
/// Terminal control utilities for clearing the screen and managing the window title.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>clear</c>, <c>set_title</c>, and
/// <c>clear_title</c> functions from <c>prompt_toolkit.shortcuts.utils</c>.
/// </para>
/// <para>
/// This type is thread-safe. All methods are stateless and delegate to
/// the current session's IOutput.
/// </para>
/// </remarks>
public static class TerminalUtils
{
    /// <summary>
    /// Clear the terminal screen.
    /// </summary>
    /// <remarks>
    /// Erases the screen, moves the cursor to position (0, 0), and flushes
    /// the output.
    /// </remarks>
    public static void Clear();

    /// <summary>
    /// Set the terminal window title.
    /// </summary>
    /// <param name="text">The title text to set.</param>
    public static void SetTitle(string text);

    /// <summary>
    /// Clear the terminal window title by setting it to an empty string.
    /// </summary>
    public static void ClearTitle();
}
```

---

## Python → C# Mapping Table

| Python Function | C# Method | Class |
|----------------|-----------|-------|
| `print_formatted_text(*values, sep, end, file, flush, style, output, color_depth, style_transformation, include_default_pygments_style)` | `Print(AnyFormattedText, ...)` | `FormattedTextOutput` |
| `print_formatted_text(*values, ...)` (multi-value) | `Print(object[], ...)` | `FormattedTextOutput` |
| `print_container(container, file, style, include_default_pygments_style)` | `PrintContainer(AnyContainer, ...)` | `FormattedTextOutput` |
| `_create_merged_style(style, include_default_pygments_style)` | `CreateMergedStyle(IStyle?, bool)` | `FormattedTextOutput` (private) |
| `clear()` | `Clear()` | `TerminalUtils` |
| `set_title(text)` | `SetTitle(string)` | `TerminalUtils` |
| `clear_title()` | `ClearTitle()` | `TerminalUtils` |

---

## Container Type Reference

The `AnyContainer` parameter in `PrintContainer` accepts the same container types used throughout the layout system. If `AnyContainer` is not yet defined as a union/interface in the Stroke codebase, the parameter type should be the layout container base type (e.g., `IContainer` or the equivalent of Python's `AnyContainer` type alias which accepts `Container`, `MagicContainer`, or any layout primitive).
