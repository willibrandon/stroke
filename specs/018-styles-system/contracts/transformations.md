# Contracts: Style Transformations

**Feature**: 018-styles-system
**Date**: 2026-01-26

## IStyleTransformation Interface

```csharp
namespace Stroke.Styles;

/// <summary>
/// Abstract base interface for style transformations.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>StyleTransformation</c>
/// abstract class from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// Style transformations are applied after styles are computed, allowing
/// post-processing like dark mode, contrast adjustment, or color inversion.
/// </para>
/// </remarks>
public interface IStyleTransformation
{
    /// <summary>
    /// Transform the given <see cref="Attrs"/> and return a new <see cref="Attrs"/>.
    /// </summary>
    /// <param name="attrs">The attributes to transform.</param>
    /// <returns>Transformed attributes.</returns>
    /// <remarks>
    /// Color formats can be either "ansi..." names or 6-digit lowercase hex (without '#' prefix).
    /// </remarks>
    Attrs TransformAttrs(Attrs attrs);

    /// <summary>
    /// Invalidation hash for the transformation. When this changes, cached styles should be invalidated.
    /// </summary>
    object InvalidationHash { get; }
}
```

## DummyStyleTransformation Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Style transformation that doesn't transform anything.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DummyStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This type is thread-safe. It is stateless.
/// </para>
/// </remarks>
public sealed class DummyStyleTransformation : IStyleTransformation
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static readonly DummyStyleTransformation Instance = new();

    private DummyStyleTransformation();

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs);

    /// <inheritdoc/>
    public object InvalidationHash { get; }
}
```

## ReverseStyleTransformation Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Style transformation that swaps the 'reverse' attribute.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>ReverseStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This type is thread-safe. It is stateless.
/// </para>
/// </remarks>
public sealed class ReverseStyleTransformation : IStyleTransformation
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public ReverseStyleTransformation();

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs);

    /// <inheritdoc/>
    public object InvalidationHash { get; }
}
```

## SwapLightAndDarkStyleTransformation Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Turn dark colors into light colors and the other way around.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SwapLightAndDarkStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This is meant to make color schemes that work on a dark background usable
/// on a light background (and the other way around).
/// </para>
/// <para>
/// Notice that this doesn't swap foreground and background like "reverse" does.
/// It turns light green into dark green and the other way around.
/// Foreground and background colors are considered individually.
/// </para>
/// <para>
/// This type is thread-safe. It is stateless.
/// </para>
/// </remarks>
public sealed class SwapLightAndDarkStyleTransformation : IStyleTransformation
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public SwapLightAndDarkStyleTransformation();

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs);

    /// <inheritdoc/>
    public object InvalidationHash { get; }
}
```

## SetDefaultColorStyleTransformation Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Set default foreground/background color for output that doesn't specify anything.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SetDefaultColorStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This is useful for overriding the terminal default colors.
/// </para>
/// <para>
/// This type is thread-safe. The color callables may be invoked from multiple threads;
/// thread safety of the callables is the caller's responsibility.
/// </para>
/// </remarks>
public sealed class SetDefaultColorStyleTransformation : IStyleTransformation
{
    /// <summary>
    /// Creates a new instance with static color values.
    /// </summary>
    /// <param name="fg">Foreground color string.</param>
    /// <param name="bg">Background color string.</param>
    public SetDefaultColorStyleTransformation(string fg, string bg);

    /// <summary>
    /// Creates a new instance with dynamic color callables.
    /// </summary>
    /// <param name="fg">Callable that returns foreground color string.</param>
    /// <param name="bg">Callable that returns background color string.</param>
    /// <exception cref="ArgumentNullException">Thrown when fg or bg is null.</exception>
    public SetDefaultColorStyleTransformation(Func<string> fg, Func<string> bg);

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs);

    /// <inheritdoc/>
    public object InvalidationHash { get; }
}
```

## AdjustBrightnessStyleTransformation Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Adjust the brightness to improve rendering on dark or light backgrounds.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>AdjustBrightnessStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// For dark backgrounds, increase <paramref name="minBrightness"/>.
/// For light backgrounds, decrease <paramref name="maxBrightness"/>.
/// Usually only one setting is adjusted.
/// </para>
/// <para>
/// This will only change brightness for text with a foreground color defined
/// but no background color. Works best for 256 or true color output.
/// </para>
/// <para>
/// This type is thread-safe. The brightness callables may be invoked from multiple threads;
/// thread safety of the callables is the caller's responsibility.
/// </para>
/// </remarks>
public sealed class AdjustBrightnessStyleTransformation : IStyleTransformation
{
    /// <summary>
    /// Creates a new instance with static brightness values.
    /// </summary>
    /// <param name="minBrightness">Minimum brightness (0.0-1.0). Default: 0.0.</param>
    /// <param name="maxBrightness">Maximum brightness (0.0-1.0). Default: 1.0.</param>
    public AdjustBrightnessStyleTransformation(float minBrightness = 0.0f, float maxBrightness = 1.0f);

    /// <summary>
    /// Creates a new instance with dynamic brightness callables.
    /// </summary>
    /// <param name="minBrightness">Callable that returns minimum brightness (0.0-1.0).</param>
    /// <param name="maxBrightness">Callable that returns maximum brightness (0.0-1.0).</param>
    /// <exception cref="ArgumentNullException">Thrown when minBrightness or maxBrightness is null.</exception>
    public AdjustBrightnessStyleTransformation(Func<float> minBrightness, Func<float> maxBrightness);

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs);

    /// <inheritdoc/>
    public object InvalidationHash { get; }
}
```

## ConditionalStyleTransformation Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Apply the style transformation depending on a condition.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>ConditionalStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This type is thread-safe. The filter may be invoked from multiple threads;
/// thread safety of the filter is the caller's responsibility.
/// </para>
/// </remarks>
public sealed class ConditionalStyleTransformation : IStyleTransformation
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="transformation">The transformation to apply when filter is true.</param>
    /// <param name="filter">The filter condition.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transformation"/> is null.</exception>
    public ConditionalStyleTransformation(
        IStyleTransformation transformation,
        FilterOrBool filter);

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs);

    /// <inheritdoc/>
    public object InvalidationHash { get; }
}
```

## DynamicStyleTransformation Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Style transformation that dynamically returns another transformation.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DynamicStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This type is thread-safe. The callable may be invoked from multiple threads;
/// thread safety of the callable is the caller's responsibility.
/// </para>
/// </remarks>
public sealed class DynamicStyleTransformation : IStyleTransformation
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="getTransformation">Callable that returns a transformation, or null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getTransformation"/> is null.</exception>
    public DynamicStyleTransformation(Func<IStyleTransformation?> getTransformation);

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs);

    /// <inheritdoc/>
    public object InvalidationHash { get; }
}
```

## StyleTransformationMerger Static Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Style transformation merging utilities.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>merge_style_transformations</c>
/// function from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// </remarks>
public static class StyleTransformationMerger
{
    /// <summary>
    /// Merge multiple style transformations together.
    /// </summary>
    /// <param name="transformations">Transformations to merge. Null entries are filtered out.</param>
    /// <returns>A merged transformation that applies all transformations in sequence.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transformations"/> is null.</exception>
    public static IStyleTransformation MergeStyleTransformations(
        IEnumerable<IStyleTransformation?> transformations);
}
```

## Internal Types

### ColorUtils (Internal)

```csharp
namespace Stroke.Styles;

/// <summary>
/// Internal color conversion utilities.
/// </summary>
internal static class ColorUtils
{
    /// <summary>
    /// Convert RGB to HLS (Hue, Lightness, Saturation).
    /// </summary>
    /// <param name="r">Red component (0.0-1.0).</param>
    /// <param name="g">Green component (0.0-1.0).</param>
    /// <param name="b">Blue component (0.0-1.0).</param>
    /// <returns>Tuple of (Hue 0.0-1.0, Lightness 0.0-1.0, Saturation 0.0-1.0).</returns>
    public static (double H, double L, double S) RgbToHls(double r, double g, double b);

    /// <summary>
    /// Convert HLS to RGB.
    /// </summary>
    /// <param name="h">Hue (0.0-1.0).</param>
    /// <param name="l">Lightness (0.0-1.0).</param>
    /// <param name="s">Saturation (0.0-1.0).</param>
    /// <returns>Tuple of (Red 0.0-1.0, Green 0.0-1.0, Blue 0.0-1.0).</returns>
    public static (double R, double G, double B) HlsToRgb(double h, double l, double s);

    /// <summary>
    /// Get the opposite color (inverted luminosity).
    /// </summary>
    /// <param name="colorName">Color as ANSI name or 6-digit hex.</param>
    /// <returns>Opposite color, or null/empty if input was null/empty.</returns>
    public static string? GetOppositeColor(string? colorName);
}
```

### AnsiColorsToRgb (Internal)

```csharp
namespace Stroke.Styles;

/// <summary>
/// Mapping of ANSI color names to RGB values for color transformations.
/// </summary>
internal static class AnsiColorsToRgb
{
    /// <summary>
    /// Dictionary mapping ANSI color names to RGB tuples.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, (int R, int G, int B)> Colors;
}
```

### OppositeAnsiColorNames (Internal)

```csharp
namespace Stroke.Styles;

/// <summary>
/// Mapping of ANSI colors to their opposite (for light/dark swapping).
/// </summary>
internal static class OppositeAnsiColorNames
{
    /// <summary>
    /// Dictionary mapping ANSI color names to their opposite.
    /// E.g., "ansiblack" → "ansiwhite"
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Opposites;
}
```
