# Feature 77: HTML Formatted Text

## Overview

Implement HTML-like markup parsing for styled formatted text, allowing users to write styled content using familiar XML-like syntax.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/formatted_text/html.py`

## Public API

### HTML Class

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// HTML formatted text.
/// Takes HTML-like markup and converts it to styled formatted text.
/// </summary>
/// <remarks>
/// Supports the following elements:
/// <list type="bullet">
///   <item><c>&lt;style fg="color" bg="color"&gt;...&lt;/style&gt;</c> - Set foreground/background</item>
///   <item><c>&lt;b&gt;...&lt;/b&gt;</c> - Bold</item>
///   <item><c>&lt;i&gt;...&lt;/i&gt;</c> - Italic</item>
///   <item><c>&lt;u&gt;...&lt;/u&gt;</c> - Underline</item>
///   <item><c>&lt;s&gt;...&lt;/s&gt;</c> - Strikethrough</item>
/// </list>
///
/// All element names become available as style classes.
/// E.g., <c>&lt;username&gt;...&lt;/username&gt;</c> can be styled by setting
/// a style for the "username" class.
/// </remarks>
/// <example>
/// <code>
/// var html = new Html("&lt;style fg=\"ansired\"&gt;Error:&lt;/style&gt; &lt;b&gt;Something went wrong&lt;/b&gt;");
/// var styled = new Html("&lt;username&gt;john&lt;/username&gt;: Hello!");
/// </code>
/// </example>
public sealed class Html : IFormattedText
{
    /// <summary>
    /// The original HTML string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// The parsed formatted text.
    /// </summary>
    public FormattedText FormattedText { get; }

    /// <summary>
    /// Create HTML formatted text from markup.
    /// </summary>
    /// <param name="value">HTML-like markup string.</param>
    /// <exception cref="XmlException">If the markup is not well-formed.</exception>
    /// <exception cref="ArgumentException">If attributes contain invalid values.</exception>
    public Html(string value);

    /// <summary>
    /// Format the HTML string with escaped arguments.
    /// Like string.Format but arguments are HTML-escaped.
    /// </summary>
    /// <param name="args">Positional arguments.</param>
    /// <returns>New HTML instance with formatted content.</returns>
    public Html Format(params object[] args);

    /// <summary>
    /// Format the HTML string with escaped named arguments.
    /// </summary>
    /// <param name="args">Named arguments dictionary.</param>
    /// <returns>New HTML instance with formatted content.</returns>
    public Html Format(IDictionary<string, object> args);

    /// <summary>
    /// Get the styled text fragments.
    /// </summary>
    /// <returns>List of style and text tuples.</returns>
    public IReadOnlyList<(string Style, string Text)> GetFormattedText();
}
```

### HTML Escape Function

```csharp
namespace Stroke.FormattedText;

public static class HtmlUtilities
{
    /// <summary>
    /// Escape text for safe inclusion in HTML markup.
    /// </summary>
    /// <param name="text">Text to escape.</param>
    /// <returns>HTML-escaped string.</returns>
    public static string HtmlEscape(object text);
}
```

## Project Structure

```
src/Stroke/
└── FormattedText/
    ├── Html.cs
    └── HtmlUtilities.cs
tests/Stroke.Tests/
└── FormattedText/
    └── HtmlTests.cs
```

## Implementation Notes

### Parsing Algorithm

The parser uses XML DOM parsing with stack-based style tracking:

```csharp
public Html(string value)
{
    Value = value;

    // Wrap in root element for parsing
    var xml = XDocument.Parse($"<html-root>{value}</html-root>");

    var result = new List<(string Style, string Text)>();
    var nameStack = new Stack<string>();
    var fgStack = new Stack<string>();
    var bgStack = new Stack<string>();

    string GetCurrentStyle()
    {
        var parts = new List<string>();
        if (nameStack.Count > 0)
            parts.Add("class:" + string.Join(",", nameStack.Reverse()));
        if (fgStack.Count > 0)
            parts.Add("fg:" + fgStack.Peek());
        if (bgStack.Count > 0)
            parts.Add("bg:" + bgStack.Peek());
        return string.Join(" ", parts);
    }

    void ProcessNode(XNode node)
    {
        switch (node)
        {
            case XText text:
                result.Add((GetCurrentStyle(), text.Value));
                break;

            case XElement element:
                var addToStack = element.Name.LocalName is not ("html-root" or "style");
                var fg = element.Attribute("fg")?.Value ?? element.Attribute("color")?.Value;
                var bg = element.Attribute("bg")?.Value;

                if (fg?.Contains(' ') == true)
                    throw new ArgumentException("'fg' attribute contains a space.");
                if (bg?.Contains(' ') == true)
                    throw new ArgumentException("'bg' attribute contains a space.");

                if (addToStack) nameStack.Push(element.Name.LocalName);
                if (fg != null) fgStack.Push(fg);
                if (bg != null) bgStack.Push(bg);

                foreach (var child in element.Nodes())
                    ProcessNode(child);

                if (addToStack) nameStack.Pop();
                if (fg != null) fgStack.Pop();
                if (bg != null) bgStack.Pop();
                break;
        }
    }

    ProcessNode(xml.Root!);
    FormattedText = new FormattedText(result);
}
```

### Format Method

```csharp
public Html Format(params object[] args)
{
    var escapedArgs = args.Select(HtmlUtilities.HtmlEscape).ToArray();
    return new Html(string.Format(Value, escapedArgs));
}
```

### HTML Escaping

```csharp
public static string HtmlEscape(object text)
{
    var str = text?.ToString() ?? "";
    return str
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;");
}
```

### Supported Elements

| Element | Style Class | Purpose |
|---------|-------------|---------|
| `<b>` | `class:b` | Bold |
| `<i>` | `class:i` | Italic |
| `<u>` | `class:u` | Underline |
| `<s>` | `class:s` | Strikethrough |
| `<style>` | (none) | Container for fg/bg attributes |
| `<custom>` | `class:custom` | User-defined style classes |

### Style Attribute Support

The `<style>` element and custom elements support:
- `fg="color"` - Foreground color
- `bg="color"` - Background color
- `color="color"` - Alias for `fg`

Color values can be:
- ANSI color names: `ansired`, `ansiblue`, etc.
- Hex colors: `#ff0000`, `#00ff44`
- Named colors defined in the style sheet

## Dependencies

- Feature 8: FormattedText base types
- System.Xml.Linq for XML parsing

## Implementation Tasks

1. Implement `Html` class with XML parsing
2. Implement stack-based style tracking
3. Implement `Format` method with escaping
4. Implement `HtmlEscape` utility function
5. Add support for `color` attribute alias
6. Validate attribute values (no spaces)
7. Implement `IFormattedText` interface
8. Write unit tests

## Acceptance Criteria

- [ ] Parses `<style fg="..." bg="...">` elements
- [ ] Parses `<b>`, `<i>`, `<u>`, `<s>` elements
- [ ] Custom element names become style classes
- [ ] Nested elements produce combined styles
- [ ] Format() escapes arguments
- [ ] HtmlEscape escapes special characters
- [ ] Throws on malformed XML
- [ ] Throws on attributes with spaces
- [ ] Unit tests achieve 80% coverage
