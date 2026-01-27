# Quickstart: Styles System

**Feature**: 018-styles-system
**Date**: 2026-01-26

## Overview

The Styles System provides a comprehensive API for defining and applying visual styles to terminal text. It supports ANSI colors, HTML/CSS named colors, hex color codes, and allows styles to be transformed dynamically.

## Basic Usage

### Creating a Simple Style

```csharp
using Stroke.Styles;

// Create a style with rules
var style = new Style(new[]
{
    ("title", "#ff0000 bold"),
    ("subtitle", "#666666 italic"),
    ("warning", "ansiyellow bg:ansiblack"),
});

// Get attributes for a class
var attrs = style.GetAttrsForStyleStr("class:title");
// attrs.Color == "ff0000", attrs.Bold == true

// Use inline styles
attrs = style.GetAttrsForStyleStr("bold underline #00ff00");
// attrs.Bold == true, attrs.Underline == true, attrs.Color == "00ff00"

// Combine class and inline styles (inline takes precedence)
attrs = style.GetAttrsForStyleStr("class:title italic");
// attrs.Color == "ff0000", attrs.Bold == true, attrs.Italic == true
```

### Creating Style from Dictionary

```csharp
var styleDict = new Dictionary<string, string>
{
    ["title"] = "#ff0000 bold",
    ["subtitle"] = "#666666 italic",
    ["error"] = "bg:ansired ansiwhite",
};

var style = Style.FromDict(styleDict);
```

### Using Named Colors

```csharp
// ANSI colors
var style = new Style(new[]
{
    ("info", "ansiblue"),
    ("success", "ansigreen"),
    ("error", "ansired bold"),
});

// HTML/CSS named colors
var style2 = new Style(new[]
{
    ("title", "Crimson bold"),
    ("link", "DodgerBlue underline"),
    ("muted", "DarkGray"),
});

// Hex colors
var style3 = new Style(new[]
{
    ("brand", "#ff5500"),           // 6-digit hex
    ("highlight", "#ff0 bg:#333"),  // 3-digit hex
});
```

## Merging Styles

```csharp
var baseStyle = new Style(new[]
{
    ("title", "#ffffff bold"),
    ("text", "#cccccc"),
});

var themeStyle = new Style(new[]
{
    ("title", "#ff0000"),  // Override title color, keep bold
});

var mergedStyle = StyleMerger.MergeStyles(new[] { baseStyle, themeStyle });
// Later styles override earlier ones
```

## Style Transformations

### Dark Mode Support

```csharp
// Swap light and dark colors
var darkModeTransform = new SwapLightAndDarkStyleTransformation();

var attrs = style.GetAttrsForStyleStr("class:title");
var darkAttrs = darkModeTransform.TransformAttrs(attrs);
// Light colors become dark, dark become light
```

### Brightness Adjustment

```csharp
// For dark backgrounds: ensure minimum brightness
var transform = new AdjustBrightnessStyleTransformation(
    minBrightness: 0.3f,
    maxBrightness: 1.0f);

// For light backgrounds: limit maximum brightness
var transform2 = new AdjustBrightnessStyleTransformation(
    minBrightness: 0.0f,
    maxBrightness: 0.7f);
```

### Conditional Transformations

```csharp
using Stroke.Filters;

// Apply transformation based on condition
bool isDarkMode = true;
var conditionalTransform = new ConditionalStyleTransformation(
    new SwapLightAndDarkStyleTransformation(),
    new Condition(() => isDarkMode));

// Transform only applies when condition is true
var attrs = conditionalTransform.TransformAttrs(originalAttrs);
```

### Dynamic Transformations

```csharp
// Transformation that can change at runtime
IStyleTransformation? currentTransform = null;

var dynamicTransform = new DynamicStyleTransformation(
    () => currentTransform);

// Later: change the transformation
currentTransform = new ReverseStyleTransformation();
```

## Default Styles

```csharp
// Use built-in UI styles
var uiStyle = DefaultStyles.DefaultUiStyle;

// Get styling for UI elements
var searchAttrs = uiStyle.GetAttrsForStyleStr("class:search");
var menuAttrs = uiStyle.GetAttrsForStyleStr("class:completion-menu");

// Use Pygments-style syntax highlighting
var pygmentsStyle = DefaultStyles.DefaultPygmentsStyle;
var keywordAttrs = pygmentsStyle.GetAttrsForStyleStr("class:pygments.keyword");
```

## Pygments Token Utilities

Create styles from Pygments-compatible token dictionaries:

```csharp
// Convert a token path to a class name
var className = PygmentsStyleUtils.PygmentsTokenToClassName(new[] { "Name", "Exception" });
// Returns: "pygments.name.exception"

// Create a style from a token dictionary
var tokenStyles = new Dictionary<string[], string>
{
    { new[] { "Keyword" }, "bold #ff79c6" },
    { new[] { "Name", "Function" }, "#50fa7b" },
    { new[] { "String" }, "#f1fa8c" },
    { new[] { "Comment" }, "#6272a4" },
};

var pygmentsStyle = PygmentsStyleUtils.StyleFromPygmentsDict(tokenStyles);
// Creates rules for "pygments.keyword", "pygments.name.function", etc.

// Use with hierarchical class matching
var attrs = pygmentsStyle.GetAttrsForStyleStr("class:pygments.name.function");
// attrs.Color == "50fa7b"
```

## Style String Syntax

### Attributes

| Attribute | Enable | Disable |
|-----------|--------|---------|
| Bold | `bold` | `nobold` |
| Italic | `italic` | `noitalic` |
| Underline | `underline` | `nounderline` |
| Strike | `strike` | `nostrike` |
| Blink | `blink` | `noblink` |
| Reverse | `reverse` | `noreverse` |
| Hidden | `hidden` | `nohidden` |
| Dim | `dim` | `nodim` |

### Colors

```text
#ff0000          # Foreground: 6-digit hex
#f00             # Foreground: 3-digit hex (expands to ff0000)
ansiblue         # Foreground: ANSI color
Crimson          # Foreground: Named color
fg:#ff0000       # Explicit foreground
bg:#ff0000       # Background color
```

### Classes

```text
class:title                    # Single class
class:title,subtitle           # Multiple classes (comma-separated)
class:menu.item.selected       # Hierarchical class (matches menu, menu.item, menu.item.selected)
```

### Special

```text
noinherit        # Reset to defaults before applying styles
```

## Hierarchical Class Names

Class names with dots are expanded to match all prefixes:

```csharp
var style = new Style(new[]
{
    ("menu", "bg:#333333"),
    ("menu.item", "#ffffff"),
    ("menu.item.selected", "reverse"),
});

// "class:menu.item.selected" matches all three rules
var attrs = style.GetAttrsForStyleStr("class:menu.item.selected");
// Result: bg=#333333, color=#ffffff, reverse=true
```

## Thread Safety

All style types are thread-safe:
- `Attrs`, `Style`, `DummyStyle` are immutable
- Transformations are stateless or use thread-safe patterns
- Dynamic types invoke callables which should be thread-safe

```csharp
// Safe to use from multiple threads
var style = new Style(rules);
Parallel.For(0, 100, i =>
{
    var attrs = style.GetAttrsForStyleStr($"class:item{i % 10}");
});
```
