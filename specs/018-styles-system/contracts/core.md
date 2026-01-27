# Contracts: Core Style Types

**Feature**: 018-styles-system
**Date**: 2026-01-26

## Attrs Record Struct

```csharp
namespace Stroke.Styles;

/// <summary>
/// Style attributes for terminal text formatting.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Attrs</c> NamedTuple
/// from <c>prompt_toolkit.styles.base</c>.
/// </para>
/// <para>
/// Nullable fields allow inheritance from parent styles. When merging multiple
/// Attrs, the last non-null value for each field is used.
/// </para>
/// </remarks>
/// <param name="Color">Foreground color: hex "rrggbb" (no #), ANSI name, or empty for default.</param>
/// <param name="BgColor">Background color: hex "rrggbb" (no #), ANSI name, or empty for default.</param>
/// <param name="Bold">Bold text attribute.</param>
/// <param name="Underline">Underlined text attribute.</param>
/// <param name="Strike">Strikethrough text attribute.</param>
/// <param name="Italic">Italic text attribute.</param>
/// <param name="Blink">Blinking text attribute.</param>
/// <param name="Reverse">Reversed foreground/background colors.</param>
/// <param name="Hidden">Hidden text attribute.</param>
/// <param name="Dim">Dim/faint text attribute.</param>
public readonly record struct Attrs(
    string? Color = null,
    string? BgColor = null,
    bool? Bold = null,
    bool? Underline = null,
    bool? Strike = null,
    bool? Italic = null,
    bool? Blink = null,
    bool? Reverse = null,
    bool? Hidden = null,
    bool? Dim = null);
```

## DefaultAttrs Static Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Default <see cref="Attrs"/> constants.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DEFAULT_ATTRS</c> constant
/// and the <c>_EMPTY_ATTRS</c> internal constant from <c>prompt_toolkit.styles</c>.
/// </para>
/// </remarks>
public static class DefaultAttrs
{
    /// <summary>
    /// The default Attrs with all values set to their defaults (empty strings for colors, false for booleans).
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
    /// Empty Attrs with all values null (for inheritance from parent styles).
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

## Priority Enum

```csharp
namespace Stroke.Styles;

/// <summary>
/// Priority of style rules when created from a dictionary.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Priority</c> enum
/// from <c>prompt_toolkit.styles.style</c>.
/// </para>
/// </remarks>
public enum Priority
{
    /// <summary>
    /// Use dictionary key order. Rules at the end override rules at the beginning.
    /// </summary>
    DictKeyOrder,

    /// <summary>
    /// Keys defined with more precision (more elements) get higher priority.
    /// </summary>
    MostPrecise
}
```

## AnsiColorNames Static Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// ANSI color name constants.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>ANSI_COLOR_NAMES</c>
/// and <c>ANSI_COLOR_NAMES_ALIASES</c> from <c>prompt_toolkit.styles.base</c>.
/// </para>
/// </remarks>
public static class AnsiColorNames
{
    /// <summary>
    /// List of standard ANSI color names (17 total).
    /// </summary>
    public static readonly IReadOnlyList<string> Names;

    /// <summary>
    /// Aliases for backwards compatibility (10 total).
    /// Maps old names to current canonical names.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Aliases;

    /// <summary>
    /// Checks if the given name is a valid ANSI color name or alias.
    /// </summary>
    /// <param name="name">The color name to check.</param>
    /// <returns><c>true</c> if valid ANSI color; otherwise, <c>false</c>.</returns>
    public static bool IsAnsiColor(string name);

    /// <summary>
    /// Resolves an alias to its canonical ANSI color name.
    /// Returns the input unchanged if not an alias.
    /// </summary>
    /// <param name="name">The color name or alias.</param>
    /// <returns>The canonical ANSI color name.</returns>
    public static string ResolveAlias(string name);
}
```

## NamedColors Static Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// 140 named HTML/CSS colors.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>NAMED_COLORS</c>
/// from <c>prompt_toolkit.styles.named_colors</c>.
/// </para>
/// </remarks>
public static class NamedColors
{
    /// <summary>
    /// Dictionary mapping color names to hex values (without # prefix).
    /// E.g., "AliceBlue" → "f0f8ff"
    /// </summary>
    /// <remarks>
    /// Keys are case-sensitive (PascalCase as in CSS spec).
    /// Use <see cref="TryGetHexValue"/> for case-insensitive lookup.
    /// </remarks>
    public static readonly IReadOnlyDictionary<string, string> Colors;

    /// <summary>
    /// Tries to get the hex value for a named color (case-insensitive lookup).
    /// </summary>
    /// <param name="name">The color name.</param>
    /// <param name="hexValue">The hex value if found (lowercase, no # prefix).</param>
    /// <returns><c>true</c> if the color was found; otherwise, <c>false</c>.</returns>
    public static bool TryGetHexValue(string name, out string hexValue);
}
```
