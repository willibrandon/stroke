# Feature 13: Formatted Text System

## Overview

Implement the formatted text system for representing styled text throughout the application.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/formatted_text/base.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/formatted_text/html.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/formatted_text/ansi.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/formatted_text/utils.py`

## Public API

### StyleAndTextTuple Types

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// A single style and text tuple. Can optionally include a mouse handler.
/// </summary>
public readonly record struct StyleAndTextTuple(
    string Style,
    string Text,
    Func<MouseEvent, bool>? MouseHandler = null);

/// <summary>
/// List of style and text tuples.
/// </summary>
public class StyleAndTextTuples : List<StyleAndTextTuple>
{
    public StyleAndTextTuples() { }
    public StyleAndTextTuples(IEnumerable<StyleAndTextTuple> items) : base(items) { }
}
```

### IFormattedText Interface

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// Interface for objects that can be converted to formatted text.
/// Implements the __pt_formatted_text__ protocol.
/// </summary>
public interface IFormattedText
{
    /// <summary>
    /// Return the formatted text representation.
    /// </summary>
    StyleAndTextTuples ToFormattedText();
}
```

### FormattedText Class

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// A list of (style, text) tuples representing formatted text.
/// </summary>
public sealed class FormattedText : StyleAndTextTuples, IFormattedText
{
    public FormattedText() { }
    public FormattedText(IEnumerable<StyleAndTextTuple> items) : base(items) { }

    public StyleAndTextTuples ToFormattedText() => this;

    public override string ToString();
}
```

### FormattedTextConverter

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// Converts various types to FormattedText.
/// </summary>
public static class FormattedTextConverter
{
    /// <summary>
    /// Convert any value to FormattedText.
    /// Accepts: string, IFormattedText, StyleAndTextTuples, Func that returns any of these, or null.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="style">Additional style to apply to all fragments.</param>
    /// <param name="autoConvert">If true, convert any object to string first.</param>
    public static FormattedText ToFormattedText(
        object? value,
        string style = "",
        bool autoConvert = false);

    /// <summary>
    /// Check whether the input is valid formatted text.
    /// </summary>
    public static bool IsFormattedText(object? value);
}
```

### Template Class

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// Template for string interpolation with formatted text.
/// </summary>
public sealed class Template
{
    /// <summary>
    /// Creates a template.
    /// </summary>
    /// <param name="text">Plain text with {} placeholders.</param>
    public Template(string text);

    /// <summary>
    /// The template text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Format the template with the given values.
    /// </summary>
    public Func<FormattedText> Format(params object?[] values);
}
```

### HTML Class

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// HTML formatted text.
/// Take something HTML-like for use as formatted string.
///
/// Supports:
/// - &lt;style fg="color" bg="color"&gt;...&lt;/style&gt; for inline styling
/// - &lt;i&gt;, &lt;b&gt;, &lt;u&gt;, &lt;s&gt; for italic, bold, underline, strike
/// - Any element name becomes a CSS class (e.g., &lt;username&gt; -> class:username)
/// </summary>
public sealed class HTML : IFormattedText
{
    /// <summary>
    /// Creates HTML formatted text.
    /// </summary>
    /// <param name="value">HTML-like string.</param>
    public HTML(string value);

    /// <summary>
    /// The original HTML value.
    /// </summary>
    public string Value { get; }

    public StyleAndTextTuples ToFormattedText();

    /// <summary>
    /// Like string.Format, but with proper HTML escaping.
    /// </summary>
    public HTML Format(params object[] args);

    public override string ToString();
}
```

### HtmlEscape Function

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// HTML utility functions.
/// </summary>
public static class HtmlUtils
{
    /// <summary>
    /// Escape special HTML characters.
    /// </summary>
    public static string HtmlEscape(string text);
}
```

### ANSI Class

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// ANSI formatted text.
/// Parse ANSI escape sequences into formatted text.
///
/// Characters between \001 and \002 are treated as zero-width escapes.
/// </summary>
public sealed class ANSI : IFormattedText
{
    /// <summary>
    /// Creates ANSI formatted text.
    /// </summary>
    /// <param name="value">String with ANSI escape sequences.</param>
    public ANSI(string value);

    /// <summary>
    /// The original ANSI value.
    /// </summary>
    public string Value { get; }

    public StyleAndTextTuples ToFormattedText();

    /// <summary>
    /// Like string.Format, but escape ANSI sequences in arguments.
    /// </summary>
    public ANSI Format(params object[] args);

    public override string ToString();
}
```

### AnsiEscape Function

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// ANSI utility functions.
/// </summary>
public static class AnsiUtils
{
    /// <summary>
    /// Escape special ANSI characters.
    /// </summary>
    public static string AnsiEscape(string text);
}
```

### Fragment List Utilities

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// Utilities for manipulating formatted text fragment lists.
/// </summary>
public static class FragmentListUtils
{
    /// <summary>
    /// Return the number of characters in the fragment list.
    /// Ignores ZeroWidthEscape fragments.
    /// </summary>
    public static int FragmentListLen(StyleAndTextTuples fragments);

    /// <summary>
    /// Return the display width of the fragment list.
    /// Takes double-width characters into account.
    /// </summary>
    public static int FragmentListWidth(StyleAndTextTuples fragments);

    /// <summary>
    /// Concatenate all text parts into a plain string.
    /// </summary>
    public static string FragmentListToText(StyleAndTextTuples fragments);

    /// <summary>
    /// Split a fragment list into lines.
    /// Yields one list per line, like string.Split.
    /// </summary>
    public static IEnumerable<StyleAndTextTuples> SplitLines(
        IEnumerable<StyleAndTextTuple> fragments);

    /// <summary>
    /// Turn any formatted text back into plain text.
    /// </summary>
    public static string ToPlainText(object? value);
}
```

### MergeFormattedText

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// Merge (concatenate) several pieces of formatted text.
/// </summary>
public static class FormattedTextMerger
{
    /// <summary>
    /// Merge multiple formatted text items into one.
    /// Returns a lazy callable.
    /// </summary>
    public static Func<FormattedText> MergeFormattedText(
        IEnumerable<object?> items);
}
```

## Project Structure

```
src/Stroke/
└── FormattedText/
    ├── StyleAndTextTuple.cs
    ├── StyleAndTextTuples.cs
    ├── IFormattedText.cs
    ├── FormattedText.cs
    ├── FormattedTextConverter.cs
    ├── Template.cs
    ├── HTML.cs
    ├── HtmlUtils.cs
    ├── ANSI.cs
    ├── AnsiUtils.cs
    ├── FragmentListUtils.cs
    └── FormattedTextMerger.cs
tests/Stroke.Tests/
└── FormattedText/
    ├── FormattedTextTests.cs
    ├── FormattedTextConverterTests.cs
    ├── TemplateTests.cs
    ├── HTMLTests.cs
    ├── ANSITests.cs
    └── FragmentListUtilsTests.cs
```

## Implementation Notes

### ZeroWidthEscape

Fragments with `[ZeroWidthEscape]` in their style are special markers for terminal control sequences that should not contribute to text width calculations.

### HTML Parsing

The HTML class uses a simple XML parser to extract:
- Element names as CSS classes
- `fg` and `bg` attributes for colors
- Nested elements build up class names (e.g., `class:container,title`)

### ANSI Parsing

The ANSI class parses:
- CSI sequences (`\x1b[...m`) for colors and attributes
- SGR parameters (0=reset, 1=bold, etc.)
- 256-color codes (38;5;N and 48;5;N)
- True color codes (38;2;R;G;B and 48;2;R;G;B)
- Cursor forward (`\x1b[NC`) as spaces

### Wide Character Support

`FragmentListWidth` uses `get_cwidth` (Unicode character width) to properly account for CJK and other double-width characters.

## Dependencies

- `Stroke.Core.UnicodeWidth` (Feature 00) - For character width calculation
- `Stroke.Input.MouseEvent` (Feature 14) - For mouse handlers

## Implementation Tasks

1. Implement `StyleAndTextTuple` record
2. Implement `StyleAndTextTuples` class
3. Implement `IFormattedText` interface
4. Implement `FormattedText` class
5. Implement `FormattedTextConverter` static class
6. Implement `Template` class
7. Implement `HTML` class with parser
8. Implement `HtmlUtils` with escaping
9. Implement `ANSI` class with parser
10. Implement `AnsiUtils` with escaping
11. Implement `FragmentListUtils` utilities
12. Implement `FormattedTextMerger`
13. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All formatted text types match Python Prompt Toolkit semantics
- [ ] HTML parsing handles all supported elements and attributes
- [ ] ANSI parsing handles all SGR codes correctly
- [ ] Wide character width calculation is correct
- [ ] Unit tests achieve 80% coverage
