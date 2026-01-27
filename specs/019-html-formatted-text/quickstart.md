# Quickstart: HTML Formatted Text

## Overview

The `Html` class parses HTML-like markup into styled text fragments for terminal rendering.

## Installation

The `Html` class is part of the core Stroke library:

```csharp
using Stroke.FormattedText;
```

## Basic Usage

### Simple Formatting

```csharp
// Bold text
var html = new Html("<b>Hello World</b>");
var fragments = html.ToFormattedText();
// fragments[0]: Style="class:b", Text="Hello World"

// Italic text
var html = new Html("<i>Emphasized</i>");

// Underline and strikethrough
var html = new Html("<u>Important</u> <s>Deleted</s>");
```

### Colors

```csharp
// Foreground color
var html = new Html("<style fg='ansired'>Error!</style>");

// Background color
var html = new Html("<style bg='ansiblue'>Highlighted</style>");

// Both colors
var html = new Html("<style fg='white' bg='red'>Alert</style>");

// Using 'color' as alias for 'fg'
var html = new Html("<style color='green'>Success</style>");
```

### Custom Style Classes

Any element name becomes a style class:

```csharp
var html = new Html("<error>Failed to connect</error>");
// Style: "class:error"

var html = new Html("<username>john</username>");
// Style: "class:username"
```

### Nested Elements

Nested elements accumulate style classes:

```csharp
var html = new Html("<b><i>Bold and Italic</i></b>");
// Style: "class:b,i"

var html = new Html("<outer><inner>Nested</inner></outer>");
// Style: "class:outer,inner"
```

## Safe String Formatting

### Format Method (Positional)

```csharp
var template = new Html("Hello <b>{0}</b>!");
var result = template.Format("World");
// Result: "Hello <b>World</b>!"

// Special characters are escaped
var result = template.Format("<script>");
// Result: "Hello <b>&lt;script&gt;</b>!"
```

### Format Method (Named)

```csharp
var template = new Html("User: <b>{name}</b>");
var result = template.Format(new Dictionary<string, object> { ["name"] = "John" });
// Result: "User: <b>John</b>"
```

### Percent Operator

```csharp
var result = new Html("<b>%s</b>") % "World";
// Result: "Hello World"

var result = new Html("%s and %s") % new object[] { "foo", "bar" };
// Result: "foo and bar"
```

### Manual Escaping

```csharp
var escaped = Html.Escape("<script>alert('xss')</script>");
// Result: "&lt;script&gt;alert('xss')&lt;/script&gt;"

var html = new Html($"<b>{Html.Escape(userInput)}</b>");
```

## Error Handling

```csharp
// Malformed XML throws FormatException
try
{
    var html = new Html("<b>Unclosed");
}
catch (FormatException ex)
{
    // "Invalid HTML markup: ..."
}

// Spaces in color attributes throw FormatException
try
{
    var html = new Html("<style fg='red blue'>Text</style>");
}
catch (FormatException ex)
{
    // "\"fg\" attribute contains a space."
}

// Null throws ArgumentNullException
try
{
    var html = new Html(null!);
}
catch (ArgumentNullException ex)
{
    // Standard null parameter message
}
```

## Integration with FormattedText

The `Html` class implements `IFormattedText`:

```csharp
IFormattedText formatted = new Html("<b>Hello</b>");
IReadOnlyList<StyleAndTextTuple> fragments = formatted.ToFormattedText();

// Can be used anywhere IFormattedText is expected
void RenderText(IFormattedText text)
{
    foreach (var (style, content, _) in text.ToFormattedText())
    {
        // Apply style and render content
    }
}

RenderText(new Html("<b>Bold</b>"));
```

## Preserved Original Value

The original markup is preserved for inspection:

```csharp
var html = new Html("<b>Hello</b>");
Console.WriteLine(html.Value);  // "<b>Hello</b>"
Console.WriteLine(html.ToString());  // "Html(<b>Hello</b>)"
```

## Supported HTML Entities

Standard HTML entities are decoded:

```csharp
var html = new Html("a &lt; b &gt; c");  // "a < b > c"
var html = new Html("fish &amp; chips"); // "fish & chips"
var html = new Html("say &quot;hi&quot;"); // "say \"hi\""
var html = new Html("&#60;");  // "<" (numeric)
var html = new Html("&#x3C;"); // "<" (hex)
```
