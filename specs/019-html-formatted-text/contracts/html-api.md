# API Contract: Html Class

**Namespace**: `Stroke.FormattedText`
**Type**: `sealed class`
**Implements**: `IFormattedText`

## Class Definition

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// Parses HTML-like markup into formatted text.
/// </summary>
public sealed class Html : IFormattedText
{
    /// <summary>
    /// Gets the original HTML input string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new Html instance by parsing the given markup.
    /// </summary>
    /// <param name="value">The HTML-like markup to parse.</param>
    /// <exception cref="ArgumentNullException">value is null</exception>
    /// <exception cref="FormatException">markup is malformed XML</exception>
    public Html(string value);

    /// <summary>
    /// Returns the parsed formatted text fragments.
    /// </summary>
    public IReadOnlyList<StyleAndTextTuple> ToFormattedText();

    /// <summary>
    /// Creates a new Html with format arguments escaped (positional).
    /// </summary>
    /// <param name="args">Format arguments (will be HTML-escaped).</param>
    /// <returns>A new Html instance with substituted values.</returns>
    public Html Format(params object[] args);

    /// <summary>
    /// Creates a new Html with format arguments escaped (named).
    /// </summary>
    /// <param name="args">Named format arguments (will be HTML-escaped).</param>
    /// <returns>A new Html instance with substituted values.</returns>
    public Html Format(IDictionary<string, object> args);

    /// <summary>
    /// Escapes special HTML characters in a string.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>Escaped text with &amp;, &lt;, &gt;, &quot; replaced.</returns>
    public static string Escape(object? text);

    /// <summary>
    /// Formats HTML with %s-style substitution (single value).
    /// </summary>
    /// <param name="html">The Html template.</param>
    /// <param name="value">The value to substitute.</param>
    /// <returns>A new Html with the value escaped and substituted.</returns>
    public static Html operator %(Html html, object value);

    /// <summary>
    /// Formats HTML with %s-style substitution (multiple values).
    /// </summary>
    /// <param name="html">The Html template.</param>
    /// <param name="values">The values to substitute.</param>
    /// <returns>A new Html with all values escaped and substituted.</returns>
    public static Html operator %(Html html, object[] values);

    /// <inheritdoc />
    public override string ToString();
}
```

## Supported Markup Elements

| Element | Output Style | Example |
|---------|--------------|---------|
| `<b>` | `class:b` | `<b>bold</b>` |
| `<i>` | `class:i` | `<i>italic</i>` |
| `<u>` | `class:u` | `<u>underline</u>` |
| `<s>` | `class:s` | `<s>strikethrough</s>` |
| `<style>` | (no class) | `<style fg="red">colored</style>` |
| `<any>` | `class:any` | `<error>message</error>` |

## Supported Attributes

| Attribute | Applies To | Output | Example |
|-----------|------------|--------|---------|
| `fg` | Any element | `fg:value` | `<style fg="ansired">` |
| `bg` | Any element | `bg:value` | `<style bg="#00ff00">` |
| `color` | Any element | `fg:value` (alias) | `<style color="blue">` |

## Error Conditions

| Condition | Exception | Message |
|-----------|-----------|---------|
| `value` is `null` | `ArgumentNullException` | (standard) |
| Malformed XML | `FormatException` | "Invalid HTML markup: {details}" |
| Space in `fg` | `FormatException` | "\"fg\" attribute contains a space." |
| Space in `bg` | `FormatException` | "\"bg\" attribute contains a space." |
| Space in `color` | `FormatException` | "\"color\" attribute contains a space." |

## Usage Examples

### Basic Formatting

```csharp
var html = new Html("<b>Bold</b> and <i>italic</i>");
var fragments = html.ToFormattedText();
// [("class:b", "Bold"), ("", " and "), ("class:i", "italic")]
```

### Colors

```csharp
var html = new Html("<style fg='ansired' bg='ansiwhite'>Alert!</style>");
var fragments = html.ToFormattedText();
// [("fg:ansired bg:ansiwhite", "Alert!")]
```

### Safe Formatting

```csharp
var html = new Html("<b>{0}</b>").Format("<script>");
// Produces: <b>&lt;script&gt;</b>
```

### Percent Operator

```csharp
var html = new Html("<b>%s</b>") % "<script>";
// Produces: <b>&lt;script&gt;</b>
```

### Nested Elements

```csharp
var html = new Html("<b><i>Bold Italic</i></b>");
var fragments = html.ToFormattedText();
// [("class:b,i", "Bold Italic")]
```
