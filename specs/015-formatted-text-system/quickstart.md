# Quickstart: Formatted Text System

**Feature**: 015-formatted-text-system
**Date**: 2026-01-25

## Overview

The Formatted Text System provides a unified way to represent styled text throughout Stroke applications. It supports multiple input formats (HTML-like markup, ANSI escape sequences, plain strings) and converts them all to a canonical list of styled fragments.

## Quick Examples

### Basic Usage

```csharp
using Stroke.FormattedText;

// Plain text (no styling)
FormattedText plain = "Hello, World!";

// HTML-like markup
var html = new Html("<b>Bold</b> and <i>italic</i>");

// ANSI escape sequences
var ansi = new Ansi("\x1b[31mRed\x1b[0m text");

// Convert to canonical form
FormattedText ft = FormattedTextUtils.ToFormattedText(html);
```

### HTML Styling

```csharp
// Text decoration
var styled = new Html("<b>Bold</b> <i>Italic</i> <u>Underline</u> <s>Strike</s>");

// Colors
var colored = new Html("<style fg=\"red\" bg=\"blue\">Colored text</style>");

// Custom classes (for your stylesheets)
var custom = new Html("<username>admin</username>");
// Produces style "class:username"

// Nested elements
var nested = new Html("<error><b>Critical!</b></error>");
// Produces style "class:error,b"

// Safe interpolation (escapes user input)
string userInput = "<script>alert('xss')</script>";
var safe = new Html("<b>{0}</b>").Format(userInput);
// The < and > in userInput are escaped
```

### ANSI Parsing

```csharp
// Basic colors
var red = new Ansi("\x1b[31mRed text\x1b[0m");

// Bold
var bold = new Ansi("\x1b[1mBold text\x1b[0m");

// Combined
var combined = new Ansi("\x1b[1;32mBold green\x1b[0m");

// 256 colors
var color256 = new Ansi("\x1b[38;5;196mBright red\x1b[0m");

// True color (24-bit RGB)
var trueColor = new Ansi("\x1b[38;2;255;128;0mOrange\x1b[0m");

// Safe interpolation (neutralizes escape sequences)
string untrusted = "text\x1bwith\x1bescape";
var sanitized = new Ansi("\x1b[1m{0}\x1b[0m").Format(untrusted);
// Escape characters replaced with '?'
```

### Template Interpolation

```csharp
// Templates preserve formatting of inserted values
var template = new Template("Welcome, {}!");
var result = template.Format(new Html("<b>Admin</b>"));
// Result: "Welcome, " + bold "Admin" + "!"

// Multiple placeholders
var multi = new Template("{} says: {}");
var message = multi.Format(
    new Html("<i>Alice</i>"),
    "Hello!");
```

### Utility Functions

```csharp
var fragments = new FormattedText(
    ("class:header", "Title\n"),
    ("", "Line 1\n"),
    ("class:footer", "End"));

// Get plain text
string plain = FormattedTextUtils.ToPlainText(fragments);
// "Title\nLine 1\nEnd"

// Character count
int len = FormattedTextUtils.FragmentListLen(fragments);
// 15

// Display width (handles CJK double-width)
var cjk = new FormattedText(("", "日本語")); // 3 chars
int width = FormattedTextUtils.FragmentListWidth(cjk);
// 6 (each CJK char is width 2)

// Split by lines
var lines = FormattedTextUtils.SplitLines(fragments).ToList();
// 3 lists, one per line

// Merge multiple formatted texts
var merged = FormattedTextUtils.Merge(
    new Html("<b>Hello</b>"),
    " ",
    new Html("<i>World</i>"));
```

### AnyFormattedText (Union Type)

Many Stroke APIs accept `AnyFormattedText` for flexible input:

```csharp
// All these are valid AnyFormattedText values:
AnyFormattedText text1 = "Plain string";
AnyFormattedText text2 = new Html("<b>Bold</b>");
AnyFormattedText text3 = new Ansi("\x1b[31mRed\x1b[0m");
AnyFormattedText text4 = () => DateTime.Now.ToString(); // Lazy

// Use in APIs
Completion completion = new(
    Text: "hello",
    Display: new Html("<b>hello</b>"),  // Implicit conversion
    DisplayMeta: "greeting");
```

## Common Patterns

### Building Dynamic Content

```csharp
// Using merge for dynamic composition
public AnyFormattedText BuildPrompt(string user, bool isAdmin)
{
    var parts = new List<AnyFormattedText>
    {
        new Html($"<username>{user}</username>"),
        " "
    };

    if (isAdmin)
        parts.Add(new Html("<admin>[ADMIN]</admin> "));

    parts.Add("> ");

    return FormattedTextUtils.Merge(parts);
}
```

### Processing External Command Output

```csharp
// Parse output from tools that use ANSI colors
string gitOutput = RunCommand("git", "status --short");
var formatted = new Ansi(gitOutput);
var fragments = formatted.ToFormattedText();
```

### Conditional Styling

```csharp
public AnyFormattedText FormatStatus(Status status)
{
    return status switch
    {
        Status.Success => new Html("<success>✓ Success</success>"),
        Status.Warning => new Html("<warning>⚠ Warning</warning>"),
        Status.Error => new Html("<error>✗ Error</error>"),
        _ => status.ToString()
    };
}
```

## Style String Reference

| Style | Meaning |
|-------|---------|
| `""` | No styling |
| `"class:name"` | CSS-like class |
| `"class:a,b"` | Multiple classes |
| `"fg:red"` | Foreground color |
| `"bg:blue"` | Background color |
| `"bold"` | Bold text |
| `"italic"` | Italic text |
| `"underline"` | Underlined text |
| `"strike"` | Strikethrough |
| `"dim"` | Dim/faint |
| `"blink"` | Blinking |
| `"reverse"` | Reverse video |
| `"hidden"` | Hidden text |

Colors can be:
- ANSI names: `ansiblack`, `ansired`, `ansigreen`, etc.
- Hex codes: `#ff0000`, `#00ff00`, etc.
