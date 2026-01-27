# Contracts: Style Interfaces and Implementations

**Feature**: 018-styles-system
**Date**: 2026-01-26

## IStyle Interface

```csharp
namespace Stroke.Styles;

/// <summary>
/// Abstract base interface for prompt_toolkit styles.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>BaseStyle</c> abstract class
/// from <c>prompt_toolkit.styles.base</c>.
/// </para>
/// </remarks>
public interface IStyle
{
    /// <summary>
    /// Return <see cref="Attrs"/> for the given style string.
    /// </summary>
    /// <param name="styleStr">The style string. Can contain inline styling and class names (e.g., "class:title").</param>
    /// <param name="default">Default Attrs to use if no styling was defined. Uses <see cref="DefaultAttrs.Default"/> if null.</param>
    /// <returns>Computed attributes for the style string.</returns>
    Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null);

    /// <summary>
    /// The list of style rules used to create this style.
    /// </summary>
    /// <remarks>
    /// Required for <see cref="DynamicStyle"/> and merged styles to work correctly.
    /// </remarks>
    IReadOnlyList<(string ClassNames, string StyleDef)> StyleRules { get; }

    /// <summary>
    /// Invalidation hash for the style. When this changes over time, the renderer
    /// knows that something in the style changed and everything has to be redrawn.
    /// </summary>
    object InvalidationHash { get; }
}
```

## DummyStyle Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// A style that doesn't style anything.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DummyStyle</c> class
/// from <c>prompt_toolkit.styles.base</c>.
/// </para>
/// <para>
/// This type is thread-safe. It is stateless and immutable.
/// </para>
/// </remarks>
public sealed class DummyStyle : IStyle
{
    /// <summary>
    /// Singleton instance of the dummy style.
    /// </summary>
    public static readonly DummyStyle Instance = new();

    private DummyStyle();

    /// <inheritdoc/>
    public Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null);

    /// <inheritdoc/>
    public IReadOnlyList<(string, string)> StyleRules { get; }

    /// <inheritdoc/>
    public object InvalidationHash { get; }
}
```

## DynamicStyle Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Style class that can dynamically return another Style.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DynamicStyle</c> class
/// from <c>prompt_toolkit.styles.base</c>.
/// </para>
/// <para>
/// This type is thread-safe. The underlying callable may be invoked from multiple
/// threads; thread safety of the callable is the caller's responsibility.
/// </para>
/// </remarks>
public sealed class DynamicStyle : IStyle
{
    /// <summary>
    /// Creates a dynamic style.
    /// </summary>
    /// <param name="getStyle">Callable that returns a <see cref="IStyle"/> instance, or null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getStyle"/> is null.</exception>
    public DynamicStyle(Func<IStyle?> getStyle);

    /// <inheritdoc/>
    public Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null);

    /// <inheritdoc/>
    public IReadOnlyList<(string, string)> StyleRules { get; }

    /// <inheritdoc/>
    public object InvalidationHash { get; }
}
```

## Style Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Create a Style instance from a list of style rules.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Style</c> class
/// from <c>prompt_toolkit.styles.style</c>.
/// </para>
/// <para>
/// The <paramref name="styleRules"/> is a list of (classnames, style) tuples.
/// The classnames are whitespace-separated class names and the style string
/// is like a Pygments style definition with additions (reverse, blink).
/// </para>
/// <para>
/// Later rules always override previous rules.
/// </para>
/// <example>
/// <code>
/// var style = new Style(new[]
/// {
///     ("title", "#ff0000 bold underline"),
///     ("something-else", "reverse"),
///     ("class1 class2", "reverse"),
/// });
/// </code>
/// </example>
/// <para>
/// This type is thread-safe. The style is immutable after construction;
/// cached computations use thread-safe caching.
/// </para>
/// </remarks>
public sealed class Style : IStyle
{
    /// <summary>
    /// Creates a style from a list of rules.
    /// </summary>
    /// <param name="styleRules">List of (classnames, style definition) tuples.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="styleRules"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when any class name contains invalid characters.</exception>
    public Style(IEnumerable<(string ClassNames, string StyleDef)> styleRules);

    /// <summary>
    /// Create a Style from a dictionary.
    /// </summary>
    /// <param name="styleDict">Dictionary of class names to style definitions.</param>
    /// <param name="priority">Rule priority ordering.</param>
    /// <returns>A new Style instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="styleDict"/> is null.</exception>
    public static Style FromDict(
        IReadOnlyDictionary<string, string> styleDict,
        Priority priority = Priority.DictKeyOrder);

    /// <inheritdoc/>
    public Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null);

    /// <inheritdoc/>
    public IReadOnlyList<(string, string)> StyleRules { get; }

    /// <inheritdoc/>
    public object InvalidationHash { get; }
}
```

## StyleParser Static Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Style parsing utilities.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>parse_color</c> function
/// from <c>prompt_toolkit.styles.style</c>.
/// </para>
/// </remarks>
public static class StyleParser
{
    /// <summary>
    /// Parse and validate a color format.
    /// </summary>
    /// <remarks>
    /// Supports:
    /// <list type="bullet">
    ///   <item>ANSI color names: "ansiblue", "ansired", etc.</item>
    ///   <item>Named colors: "red", "blue", "AliceBlue", etc.</item>
    ///   <item>Hex codes: "#RGB", "#RRGGBB", "RRGGBB"</item>
    ///   <item>Empty string or "default" for default color</item>
    /// </list>
    /// </remarks>
    /// <param name="text">The color string to parse.</param>
    /// <returns>
    /// Normalized color string:
    /// - ANSI names are preserved (e.g., "ansiblue")
    /// - Named colors are converted to lowercase hex (e.g., "f0f8ff")
    /// - Hex codes are normalized to lowercase 6-digit (e.g., "ff0000")
    /// - Empty string or "default" returned as-is
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the color format is invalid.</exception>
    public static string ParseColor(string text);
}
```

## StyleMerger Static Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Style merging utilities.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>merge_styles</c> function
/// from <c>prompt_toolkit.styles.style</c>.
/// </para>
/// </remarks>
public static class StyleMerger
{
    /// <summary>
    /// Merge multiple Style objects into one.
    /// </summary>
    /// <param name="styles">The styles to merge. Null entries are filtered out.</param>
    /// <returns>A merged style. Later styles override earlier ones.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="styles"/> is null.</exception>
    public static IStyle MergeStyles(IEnumerable<IStyle?> styles);
}
```

## DefaultStyles Static Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Default style definitions.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>default_ui_style</c>
/// and <c>default_pygments_style</c> functions from <c>prompt_toolkit.styles.defaults</c>.
/// </para>
/// </remarks>
public static class DefaultStyles
{
    /// <summary>
    /// Gets the default UI style for prompt_toolkit applications.
    /// </summary>
    /// <remarks>
    /// This is a merged style containing:
    /// <list type="bullet">
    ///   <item>PROMPT_TOOLKIT_STYLE rules for UI elements</item>
    ///   <item>COLORS_STYLE rules for ANSI and named colors</item>
    ///   <item>WIDGETS_STYLE rules for widgets</item>
    /// </list>
    /// </remarks>
    public static IStyle DefaultUiStyle { get; }

    /// <summary>
    /// Gets the default Pygments style for syntax highlighting.
    /// </summary>
    public static IStyle DefaultPygmentsStyle { get; }
}
```
