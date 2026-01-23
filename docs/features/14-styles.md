# Feature 14: Styles System

## Overview

Implement the styling system for defining and applying visual styles to formatted text.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/styles/base.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/styles/style.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/styles/named_colors.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/styles/defaults.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/styles/style_transformation.py`

## Public API

### Attrs Record

```csharp
namespace Stroke.Styles;

/// <summary>
/// Style attributes.
/// </summary>
public readonly record struct Attrs(
    string? Color,
    string? BgColor,
    bool? Bold,
    bool? Underline,
    bool? Strike,
    bool? Italic,
    bool? Blink,
    bool? Reverse,
    bool? Hidden,
    bool? Dim);
```

### Default Attrs

```csharp
namespace Stroke.Styles;

/// <summary>
/// Default attributes constants.
/// </summary>
public static class DefaultAttrs
{
    /// <summary>
    /// The default Attrs with all values set.
    /// </summary>
    public static readonly Attrs Default = new(
        Color: "",
        BgColor: "",
        Bold: false,
        Underline: false,
        Strike: false,
        Italic: false,
        Blink: false,
        Reverse: false,
        Hidden: false,
        Dim: false);

    /// <summary>
    /// Empty Attrs with all values null (inherit from parent).
    /// </summary>
    public static readonly Attrs Empty = new(
        Color: null,
        BgColor: null,
        Bold: null,
        Underline: null,
        Strike: null,
        Italic: null,
        Blink: null,
        Reverse: null,
        Hidden: null,
        Dim: null);
}
```

### ANSI Color Names

```csharp
namespace Stroke.Styles;

/// <summary>
/// ANSI color name constants.
/// </summary>
public static class AnsiColorNames
{
    /// <summary>
    /// List of standard ANSI color names.
    /// </summary>
    public static readonly IReadOnlyList<string> Names = new[]
    {
        "ansidefault",
        // Dark colors
        "ansiblack",
        "ansired",
        "ansigreen",
        "ansiyellow",
        "ansiblue",
        "ansimagenta",
        "ansicyan",
        "ansigray",
        // Bright colors
        "ansibrightblack",
        "ansibrightred",
        "ansibrightgreen",
        "ansibrightyellow",
        "ansibrightblue",
        "ansibrightmagenta",
        "ansibrightcyan",
        "ansiwhite",
    };

    /// <summary>
    /// Aliases for backwards compatibility.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Aliases;
}
```

### Named Colors

```csharp
namespace Stroke.Styles;

/// <summary>
/// 140 named HTML/CSS colors.
/// </summary>
public static class NamedColors
{
    /// <summary>
    /// Dictionary mapping color names to hex values.
    /// E.g., "AliceBlue" -> "F0F8FF"
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Colors;
}
```

### IStyle Interface (Abstract Base)

```csharp
namespace Stroke.Styles;

/// <summary>
/// Abstract base interface for styles.
/// </summary>
public interface IStyle
{
    /// <summary>
    /// Return Attrs for the given style string.
    /// </summary>
    /// <param name="styleStr">The style string (can contain inline styling and class names).</param>
    /// <param name="default">Default Attrs to use if no styling was defined.</param>
    Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null);

    /// <summary>
    /// The list of style rules used to create this style.
    /// </summary>
    IReadOnlyList<(string ClassNames, string StyleDef)> StyleRules { get; }

    /// <summary>
    /// Invalidation hash. When this changes, the renderer knows to redraw.
    /// </summary>
    object InvalidationHash { get; }
}
```

### DummyStyle Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// A style that doesn't style anything.
/// </summary>
public sealed class DummyStyle : IStyle
{
    public static readonly DummyStyle Instance = new();

    public Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null);
    public IReadOnlyList<(string, string)> StyleRules => Array.Empty<(string, string)>();
    public object InvalidationHash => 1;
}
```

### DynamicStyle Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Style class that can dynamically return another Style.
/// </summary>
public sealed class DynamicStyle : IStyle
{
    /// <summary>
    /// Creates a dynamic style.
    /// </summary>
    /// <param name="getStyle">Callable that returns a Style instance.</param>
    public DynamicStyle(Func<IStyle?> getStyle);

    public Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null);
    public IReadOnlyList<(string, string)> StyleRules { get; }
    public object InvalidationHash { get; }
}
```

### Priority Enum

```csharp
namespace Stroke.Styles;

/// <summary>
/// Priority of style rules when created from a dictionary.
/// </summary>
public enum Priority
{
    /// <summary>
    /// Use dictionary key order (default).
    /// </summary>
    DictKeyOrder,

    /// <summary>
    /// More precise keys (more elements) get higher priority.
    /// </summary>
    MostPrecise
}
```

### Style Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Create a Style instance from a list of style rules.
/// </summary>
public sealed class Style : IStyle
{
    /// <summary>
    /// Creates a style from a list of rules.
    /// </summary>
    /// <param name="styleRules">List of (classnames, style) tuples.</param>
    public Style(IEnumerable<(string ClassNames, string StyleDef)> styleRules);

    /// <summary>
    /// Create a Style from a dictionary.
    /// </summary>
    /// <param name="styleDict">Dictionary of class names to style definitions.</param>
    /// <param name="priority">Rule priority.</param>
    public static Style FromDict(
        IReadOnlyDictionary<string, string> styleDict,
        Priority priority = Priority.DictKeyOrder);

    public Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null);
    public IReadOnlyList<(string, string)> StyleRules { get; }
    public object InvalidationHash { get; }
}
```

### ParseColor Function

```csharp
namespace Stroke.Styles;

/// <summary>
/// Style parsing utilities.
/// </summary>
public static class StyleParser
{
    /// <summary>
    /// Parse/validate color format.
    /// Supports: ANSI names, named colors, hex codes (#RGB, #RRGGBB).
    /// </summary>
    public static string ParseColor(string text);
}
```

### MergeStyles Function

```csharp
namespace Stroke.Styles;

/// <summary>
/// Style merging utilities.
/// </summary>
public static class StyleMerger
{
    /// <summary>
    /// Merge multiple Style objects into one.
    /// </summary>
    public static IStyle MergeStyles(IEnumerable<IStyle?> styles);
}
```

### Style Transformations

```csharp
namespace Stroke.Styles;

/// <summary>
/// Abstract base for style transformations.
/// </summary>
public interface IStyleTransformation
{
    /// <summary>
    /// Transform the given Attrs.
    /// </summary>
    Attrs TransformAttrs(Attrs attrs);
}

/// <summary>
/// Style transformation that does nothing.
/// </summary>
public sealed class DummyStyleTransformation : IStyleTransformation
{
    public static readonly DummyStyleTransformation Instance = new();
    public Attrs TransformAttrs(Attrs attrs) => attrs;
}

/// <summary>
/// Style transformation that reverses foreground and background.
/// </summary>
public sealed class ReverseStyleTransformation : IStyleTransformation
{
    public Attrs TransformAttrs(Attrs attrs);
}

/// <summary>
/// Swap light and dark colors.
/// </summary>
public sealed class SwapLightAndDarkStyleTransformation : IStyleTransformation
{
    public Attrs TransformAttrs(Attrs attrs);
}

/// <summary>
/// Set default foreground/background colors.
/// </summary>
public sealed class SetDefaultColorStyleTransformation : IStyleTransformation
{
    public SetDefaultColorStyleTransformation(string? fg = null, string? bg = null);
    public Attrs TransformAttrs(Attrs attrs);
}

/// <summary>
/// Adjust brightness of all colors.
/// </summary>
public sealed class AdjustBrightnessStyleTransformation : IStyleTransformation
{
    /// <param name="minBrightness">Value between 0.0 and 1.0.</param>
    /// <param name="maxBrightness">Value between 0.0 and 1.0.</param>
    public AdjustBrightnessStyleTransformation(
        float minBrightness = 0.0f,
        float maxBrightness = 1.0f);
    public Attrs TransformAttrs(Attrs attrs);
}

/// <summary>
/// Apply transformation conditionally.
/// </summary>
public sealed class ConditionalStyleTransformation : IStyleTransformation
{
    public ConditionalStyleTransformation(
        IStyleTransformation transformation,
        Func<bool> filter);
    public Attrs TransformAttrs(Attrs attrs);
}

/// <summary>
/// Dynamic style transformation.
/// </summary>
public sealed class DynamicStyleTransformation : IStyleTransformation
{
    public DynamicStyleTransformation(Func<IStyleTransformation?> getTransformation);
    public Attrs TransformAttrs(Attrs attrs);
}
```

### MergeStyleTransformations

```csharp
namespace Stroke.Styles;

/// <summary>
/// Style transformation utilities.
/// </summary>
public static class StyleTransformationMerger
{
    /// <summary>
    /// Merge multiple style transformations.
    /// </summary>
    public static IStyleTransformation MergeStyleTransformations(
        IEnumerable<IStyleTransformation?> transformations);
}
```

### Default Styles

```csharp
namespace Stroke.Styles;

/// <summary>
/// Default style definitions.
/// </summary>
public static class DefaultStyles
{
    /// <summary>
    /// Default UI style for prompt_toolkit applications.
    /// </summary>
    public static IStyle DefaultUiStyle { get; }

    /// <summary>
    /// Default Pygments style.
    /// </summary>
    public static IStyle DefaultPygmentsStyle { get; }
}
```

## Project Structure

```
src/Stroke/
└── Styles/
    ├── Attrs.cs
    ├── DefaultAttrs.cs
    ├── AnsiColorNames.cs
    ├── NamedColors.cs
    ├── IStyle.cs
    ├── DummyStyle.cs
    ├── DynamicStyle.cs
    ├── Priority.cs
    ├── Style.cs
    ├── StyleParser.cs
    ├── StyleMerger.cs
    ├── IStyleTransformation.cs
    ├── DummyStyleTransformation.cs
    ├── ReverseStyleTransformation.cs
    ├── SwapLightAndDarkStyleTransformation.cs
    ├── SetDefaultColorStyleTransformation.cs
    ├── AdjustBrightnessStyleTransformation.cs
    ├── ConditionalStyleTransformation.cs
    ├── DynamicStyleTransformation.cs
    ├── StyleTransformationMerger.cs
    └── DefaultStyles.cs
tests/Stroke.Tests/
└── Styles/
    ├── AttrsTests.cs
    ├── StyleTests.cs
    ├── StyleParserTests.cs
    ├── StyleMergerTests.cs
    └── StyleTransformationTests.cs
```

## Implementation Notes

### Style String Parsing

Style strings can contain:
- Colors: `#ff0000`, `red`, `ansiblue`, `fg:color`, `bg:color`
- Attributes: `bold`, `nobold`, `italic`, `noitalic`, `underline`, `strike`, `blink`, `reverse`, `hidden`, `dim`
- Classes: `class:name`, `class:name1,name2`
- Special: `noinherit` (don't inherit from parent)

### Class Name Expansion

Class names with dots are expanded:
- `class:a.b.c` → matches rules for `a`, `a.b`, `a.b.c`

### Color Parsing

The `ParseColor` function handles:
- ANSI color names: `ansiblue`, `ansired`, etc.
- Named colors: `red`, `blue`, `AliceBlue`, etc.
- Hex codes: `#RGB`, `#RRGGBB`
- Empty string or `default` for default color

### Style Rule Matching

When getting attrs for a style string:
1. Start with default attrs
2. Apply rules matching the empty class name (global defaults)
3. For each class in the style string, apply matching rules
4. For combinations of classes, apply matching combination rules
5. Apply inline styles last (highest priority)

## Dependencies

- `Stroke.Core.Cache` (Feature 05) - For style caching
- `Stroke.Filters` (Feature 12) - For conditional transformations

## Implementation Tasks

1. Implement `Attrs` record
2. Implement `DefaultAttrs` constants
3. Implement `AnsiColorNames` with names and aliases
4. Implement `NamedColors` with 140 colors
5. Implement `IStyle` interface
6. Implement `DummyStyle` and `DynamicStyle`
7. Implement `Priority` enum
8. Implement `Style` class with parsing
9. Implement `StyleParser.ParseColor`
10. Implement `StyleMerger.MergeStyles`
11. Implement all style transformations
12. Implement `StyleTransformationMerger`
13. Implement `DefaultStyles`
14. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All style types match Python Prompt Toolkit semantics
- [ ] Style string parsing handles all cases correctly
- [ ] Color parsing supports all formats
- [ ] Style transformations work correctly
- [ ] Unit tests achieve 80% coverage
