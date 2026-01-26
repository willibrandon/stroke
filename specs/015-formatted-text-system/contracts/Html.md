# Contract: Html

**Namespace**: `Stroke.FormattedText`

## Class Definition

```csharp
/// <summary>
/// Parses HTML-like markup into formatted text.
/// </summary>
/// <remarks>
/// <para>
/// Supports a subset of HTML for styling text:
/// <list type="bullet">
///   <item><c>&lt;b&gt;</c> for bold</item>
///   <item><c>&lt;i&gt;</c> for italic</item>
///   <item><c>&lt;u&gt;</c> for underline</item>
///   <item><c>&lt;s&gt;</c> for strikethrough</item>
///   <item><c>&lt;style fg="color" bg="color"&gt;</c> for colors</item>
///   <item>Any other element becomes a CSS class</item>
/// </list>
/// </para>
/// <para>
/// Example: <c>&lt;b&gt;bold&lt;/b&gt;</c> produces <c>("class:b", "bold")</c>
/// </para>
/// </remarks>
public sealed class Html : IFormattedText
{
    /// <summary>
    /// Gets the original HTML input string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new <see cref="Html"/> instance by parsing the given markup.
    /// </summary>
    /// <param name="value">The HTML-like markup to parse.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="XmlException">Thrown when the markup is malformed.</exception>
    /// <exception cref="ArgumentException">Thrown when fg or bg attribute contains a space.</exception>
    public Html(string value);

    /// <summary>
    /// Returns the parsed formatted text fragments.
    /// </summary>
    public IReadOnlyList<StyleAndTextTuple> ToFormattedText();

    /// <summary>
    /// Creates a new <see cref="Html"/> with format arguments escaped.
    /// </summary>
    /// <param name="args">Format arguments (will be HTML-escaped).</param>
    /// <returns>A new Html instance with substituted values.</returns>
    /// <remarks>
    /// Uses positional placeholders: <c>{0}</c>, <c>{1}</c>, etc.
    /// Special characters in arguments are escaped: &amp; &lt; &gt; &quot;
    /// </remarks>
    public Html Format(params object[] args);

    /// <summary>
    /// Creates a new <see cref="Html"/> with format arguments escaped (named parameters).
    /// </summary>
    /// <param name="args">Named format arguments (will be HTML-escaped).</param>
    /// <returns>A new Html instance with substituted values.</returns>
    public Html Format(IDictionary<string, object> args);

    /// <summary>
    /// Returns a string representation of this Html.
    /// </summary>
    public override string ToString();
}
```

## Static Methods

```csharp
/// <summary>
/// Escapes special HTML characters in a string.
/// </summary>
/// <param name="text">The text to escape.</param>
/// <returns>The escaped text with &amp;, &lt;, &gt;, and &quot; replaced.</returns>
public static string HtmlEscape(object text);
```

## Parsing Rules

| Input | Output Style |
|-------|--------------|
| `<b>text</b>` | `"class:b"` |
| `<i>text</i>` | `"class:i"` |
| `<u>text</u>` | `"class:u"` |
| `<s>text</s>` | `"class:s"` |
| `<style fg="red">text</style>` | `"fg:red"` |
| `<style bg="blue">text</style>` | `"bg:blue"` |
| `<style fg="red" bg="blue">text</style>` | `"fg:red bg:blue"` |
| `<style color="red">text</style>` | `"fg:red"` (color is alias for fg) |
| `<username>text</username>` | `"class:username"` |
| `<outer><inner>text</inner></outer>` | `"class:outer,inner"` |
| `plain text` | `""` |

## HTML Entities

| Entity | Character |
|--------|-----------|
| `&amp;` | `&` |
| `&lt;` | `<` |
| `&gt;` | `>` |
| `&quot;` | `"` |

## Usage Examples

```csharp
// Basic usage
var html = new Html("<b>Hello</b> <i>World</i>");
var fragments = html.ToFormattedText();
// [("class:b", "Hello"), ("", " "), ("class:i", "World")]

// With colors
var colored = new Html("<style fg=\"red\" bg=\"blue\">Alert</style>");
// [("fg:red bg:blue", "Alert")]

// Nested elements
var nested = new Html("<warning><b>Error</b></warning>");
// [("class:warning,b", "Error")]

// Safe interpolation
var user = "<script>alert('xss')</script>";
var safe = new Html("<b>{0}</b>").Format(user);
// Produces: [("class:b", "<script>alert('xss')</script>")]
// The < and > are HTML entities in the source, rendered as literal text

// %-style interpolation
var msg = new Html("<b>%s</b>") % "hello";
// [("class:b", "hello")]
```
