# Research: Styles System

**Feature**: 018-styles-system
**Date**: 2026-01-26
**Status**: Complete

## Research Questions

### 1. HLS Color Space Conversion

**Question**: How should HLS (Hue, Lightness, Saturation) color space conversion be implemented for brightness adjustments?

**Decision**: Implement HLS conversion using standard algorithms in a `ColorUtils` internal static class.

**Rationale**:
- Python uses `colorsys.rgb_to_hls` and `colorsys.hls_to_rgb` from the standard library
- .NET has no built-in HLS conversion (System.Drawing has HSL but not in .NET Core/10)
- The conversion algorithms are simple and well-documented
- Implementing directly avoids external dependencies (Constitution III compliance)

**Implementation**:
```csharp
internal static class ColorUtils
{
    public static (double H, double L, double S) RgbToHls(double r, double g, double b);
    public static (double R, double G, double B) HlsToRgb(double h, double l, double s);
}
```

**Alternatives Considered**:
- System.Drawing.Color: Not available in .NET Core/10 without Windows Forms package
- Third-party color libraries: Would violate Constitution III (zero external dependencies)

**Algorithm** (standard HLS conversion):
```csharp
// RGB to HLS
public static (double H, double L, double S) RgbToHls(double r, double g, double b)
{
    var maxC = Math.Max(Math.Max(r, g), b);
    var minC = Math.Min(Math.Min(r, g), b);
    var l = (minC + maxC) / 2.0;
    if (minC == maxC) return (0.0, l, 0.0);  // Achromatic

    var s = l <= 0.5 ? (maxC - minC) / (maxC + minC) : (maxC - minC) / (2.0 - maxC - minC);
    var h = maxC == r ? ((g - b) / (maxC - minC)) % 6.0
          : maxC == g ? (b - r) / (maxC - minC) + 2.0
          : (r - g) / (maxC - minC) + 4.0;
    h /= 6.0;
    return (h < 0 ? h + 1 : h, l, s);
}

// HLS to RGB
public static (double R, double G, double B) HlsToRgb(double h, double l, double s)
{
    if (s == 0.0) return (l, l, l);  // Achromatic
    var m2 = l <= 0.5 ? l * (1.0 + s) : l + s - l * s;
    var m1 = 2.0 * l - m2;
    return (HueToRgb(m1, m2, h + 1.0/3.0), HueToRgb(m1, m2, h), HueToRgb(m1, m2, h - 1.0/3.0));
}
```

### 1a. Brightness Interpolation Formula

**Question**: What is the exact formula for AdjustBrightnessStyleTransformation?

**Decision**: Use linear interpolation to map lightness into [minBrightness, maxBrightness] range.

**Formula** (from Python style_transformation.py:215-221):
```
new_brightness = minBrightness + (maxBrightness - minBrightness) * original_brightness
```

**Implementation**:
```csharp
private float InterpolateBrightness(float brightness, float minBrightness, float maxBrightness)
{
    return minBrightness + (maxBrightness - minBrightness) * brightness;
}
```

**Example**: With minBrightness=0.3, maxBrightness=1.0:
- Original brightness 0.0 → 0.3 (dark becomes lighter)
- Original brightness 0.5 → 0.65
- Original brightness 1.0 → 1.0 (bright stays bright)

**Range Validation** (from Python style_transformation.py:166-167):
```python
assert 0 <= min_brightness <= 1
assert 0 <= max_brightness <= 1
```
Values outside [0.0, 1.0] MUST throw `ArgumentOutOfRangeException`. This validation occurs when `TransformAttrs` is called, not in the constructor (since callables may return dynamic values).

### 1b. Luminosity Inversion Algorithm

**Question**: What is the exact algorithm for SwapLightAndDarkStyleTransformation?

**Decision**: For ANSI colors, use lookup table. For hex colors, invert lightness in HLS space.

**Algorithm** (from Python style_transformation.py:340-374):
1. If color is null/empty/"default", return as-is
2. If color is ANSI name, look up opposite in `OPPOSITE_ANSI_COLOR_NAMES`
3. Otherwise, parse hex color, convert to HLS, invert lightness (`l = 1 - l`), convert back to hex

**Formula**:
```
inverted_lightness = 1.0 - original_lightness
```

**Implementation**:
```csharp
public static string? GetOppositeColor(string? colorName)
{
    if (string.IsNullOrEmpty(colorName) || colorName == "default")
        return colorName;

    // Try ANSI lookup
    if (OppositeAnsiColorNames.Opposites.TryGetValue(colorName, out var opposite))
        return opposite;

    // Parse hex, invert lightness
    var (r, g, b) = ParseHex(colorName);
    var (h, l, s) = RgbToHls(r, g, b);
    l = 1.0 - l;  // Invert lightness
    var (r2, g2, b2) = HlsToRgb(h, l, s);
    return ToHex(r2, g2, b2);
}
```

### 2. ANSI Color to RGB Mapping

**Question**: How should ANSI color names be converted to RGB for brightness transformations?

**Decision**: Include a static lookup table `AnsiColorsToRgb` with standard terminal color values.

**Rationale**:
- Python imports `ANSI_COLORS_TO_RGB` from `prompt_toolkit.output.vt100`
- These are standardized terminal color values
- Needed for `AdjustBrightnessStyleTransformation` to work with ANSI colors

**Implementation**:
```csharp
internal static class AnsiColorsToRgb
{
    public static readonly IReadOnlyDictionary<string, (int R, int G, int B)> Colors = new Dictionary<string, (int, int, int)>
    {
        ["ansiblack"] = (0, 0, 0),
        ["ansired"] = (205, 0, 0),
        // ... etc
    };
}
```

**Source**: Python Prompt Toolkit `output/vt100.py` lines defining ANSI_COLORS_TO_RGB

### 3. Style Caching Strategy

**Question**: What caching approach should be used for style computation?

**Decision**: Use `SimpleCache<TKey, TValue>` from Stroke.Core (Feature 06) with style string as key.

**Rationale**:
- Already implemented in Stroke.Core with thread-safe LRU semantics
- Matches Python Prompt Toolkit's use of `SimpleCache` for style caching
- Cache size of 8-32 entries is sufficient (Python uses maxsize=1 for merged style)

**Implementation**:
```csharp
public sealed class Style : IStyle
{
    private readonly SimpleCache<string, Attrs> _cache = new(maxSize: 32);

    public Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null)
    {
        var defaultAttrs = @default ?? DefaultAttrs.Default;
        return _cache.Get(styleStr, () => ComputeAttrs(styleStr, defaultAttrs));
    }
}
```

### 4. Style String Parsing

**Question**: What is the complete grammar for style strings?

**Decision**: Support the exact grammar from Python Prompt Toolkit.

**Grammar**:
```
style_string := token*
token := attribute | color | class_ref | special

attribute := "bold" | "nobold" | "italic" | "noitalic" | "underline" | "nounderline"
           | "strike" | "nostrike" | "blink" | "noblink" | "reverse" | "noreverse"
           | "hidden" | "nohidden" | "dim" | "nodim"

color := hex_color | ansi_color | named_color | prefixed_color
hex_color := "#" [0-9a-fA-F]{3} | "#" [0-9a-fA-F]{6}
ansi_color := "ansi" identifier
named_color := identifier (from NAMED_COLORS)
prefixed_color := ("fg:" | "bg:") color

class_ref := "class:" identifier ("," identifier)*
class_name := [a-z0-9._-]+

special := "noinherit"

ignored := "roman" | "sans" | "mono" | "border:" identifier | "[" ... "]"
```

**Rationale**: Faithful port requires exact parsing semantics from Python implementation.

### 5. Class Name Expansion Algorithm

**Question**: How should hierarchical class names like `a.b.c` be expanded?

**Decision**: Split on `.` and generate all prefixes: `["a", "a.b", "a.b.c"]`

**Implementation**:
```csharp
private static IEnumerable<string> ExpandClassName(string className)
{
    var parts = className.ToLowerInvariant().Split('.');
    for (int i = 1; i <= parts.Length; i++)
    {
        yield return string.Join(".", parts.Take(i));
    }
}
```

**Rationale**: Matches Python Prompt Toolkit's `_expand_classname` function exactly.

### 6. Style Rule Matching Algorithm

**Question**: How should rules be matched when multiple classes are present?

**Decision**: Generate all combinations of current classes with new classes and match against rules.

**Algorithm from Python**:
1. Start with default attrs
2. Apply rules matching empty class set (global defaults)
3. For each class in style string:
   - Expand class name (a.b.c → a, a.b, a.b.c)
   - Generate all combinations with previously seen classes
   - Apply matching rules in order
4. Apply inline styles last

**Rationale**: Complex but necessary for correct style precedence. Python uses `itertools.combinations` for this.

### 7. FilterOrBool Integration

**Question**: How should `ConditionalStyleTransformation` integrate with the Filter system?

**Decision**: Accept `FilterOrBool` in constructor, use `FilterUtils.ToFilter()` for evaluation.

**Implementation**:
```csharp
public sealed class ConditionalStyleTransformation : IStyleTransformation
{
    private readonly IStyleTransformation _transformation;
    private readonly IFilter _filter;

    public ConditionalStyleTransformation(IStyleTransformation transformation, FilterOrBool filter)
    {
        _transformation = transformation;
        _filter = FilterUtils.ToFilter(filter);
    }
}
```

**Rationale**: Matches Python Prompt Toolkit's use of `to_filter(filter)` in constructor.

### 8. Invalidation Hash Strategy

**Question**: What should be used for invalidation hashes?

**Decision**: Use composite tuples and object identity, following Python patterns.

**Implementation**:
| Type | InvalidationHash |
|------|------------------|
| Style | `id(class_names_and_attrs)` → use RuntimeHelpers.GetHashCode |
| DummyStyle | Constant `1` |
| DynamicStyle | Delegate to underlying style |
| _MergedStyle | Tuple of source style hashes |
| StyleTransformation | Class name + id, or property-based tuple |

### 9. Thread Safety Requirements

**Question**: Which types need thread safety mechanisms?

**Decision**:
- **Immutable types (inherently safe)**: `Attrs`, `Style`, `DummyStyle`, `Priority`, `AnsiColorNames`, `NamedColors`
- **Stateless types (inherently safe)**: `DummyStyleTransformation`, `ReverseStyleTransformation`
- **Types with mutable state (need Lock)**: None identified - all style types are immutable after construction
- **Types with callable delegates (need consideration)**: `DynamicStyle`, `DynamicStyleTransformation`, `SetDefaultColorStyleTransformation`, `AdjustBrightnessStyleTransformation` - the callables may be called from multiple threads, but this is the caller's responsibility

**Rationale**: Python Prompt Toolkit styles are inherently immutable after construction. Thread safety comes from immutability, not locking.

### 10. Default Styles Content

**Question**: What should the default UI and Pygments styles contain?

**Decision**: Port exactly from Python Prompt Toolkit `defaults.py`.

**Default UI Style Rules** (from PROMPT_TOOLKIT_STYLE + COLORS_STYLE + WIDGETS_STYLE):
- 68 rules from PROMPT_TOOLKIT_STYLE
- 17 ANSI color rules + 140 named color rules from COLORS_STYLE
- 19 rules from WIDGETS_STYLE
- Total: 244 rules

**Default Pygments Style Rules** (from PYGMENTS_DEFAULT_STYLE):
- 34 rules for syntax highlighting tokens

**Rationale**: Constitution I (Faithful Port) requires exact replication.

### 11. ValueOrCallable Pattern

**Question**: How should the `OneOf<T, Func<T>>` union types be implemented for SetDefaultColorStyleTransformation and AdjustBrightnessStyleTransformation?

**Decision**: Use constructor overloading with internal callable storage, matching Python's behavior.

**Rationale**:
- Python stores both values and callables in the same field and uses `to_str()`/`to_float()` at runtime
- Creating a full `OneOf<T1, T2>` discriminated union adds complexity beyond what's needed
- Constructor overloading provides a simple, idiomatic C# solution
- External `OneOf` packages are prohibited by Constitution III (zero external dependencies)

**Implementation**:
```csharp
public sealed class SetDefaultColorStyleTransformation : IStyleTransformation
{
    private readonly Func<string> _fg;
    private readonly Func<string> _bg;

    /// <summary>Creates instance with static color values.</summary>
    public SetDefaultColorStyleTransformation(string fg, string bg)
    {
        _fg = () => fg;
        _bg = () => bg;
    }

    /// <summary>Creates instance with dynamic color callables.</summary>
    public SetDefaultColorStyleTransformation(Func<string> fg, Func<string> bg)
    {
        _fg = fg ?? throw new ArgumentNullException(nameof(fg));
        _bg = bg ?? throw new ArgumentNullException(nameof(bg));
    }
}
```

**Note**: The contracts reference `OneOf<T, Func<T>>` but implementation should use constructor overloading instead. The contract notation indicates the conceptual union type from Python's type hints.

## Dependencies Verified

| Dependency | Status | Notes |
|------------|--------|-------|
| Stroke.Core.SimpleCache | ✅ Available | Feature 06 implemented |
| Stroke.Filters.IFilter | ✅ Available | Feature 17 implemented |
| Stroke.Filters.FilterOrBool | ✅ Available | Feature 17 implemented |
| Stroke.Filters.FilterUtils | ✅ Available | Feature 17 implemented |

## API Mapping Verification

All APIs verified against `docs/api-mapping.md` lines 1820-1902:

| Python API | Stroke API | Status |
|------------|------------|--------|
| BaseStyle | IStyle | ✅ Mapped |
| Style | Style | ✅ Mapped |
| DummyStyle | DummyStyle | ✅ Mapped |
| DynamicStyle | DynamicStyle | ✅ Mapped |
| Attrs | Attrs | ✅ Mapped |
| StyleTransformation | IStyleTransformation | ✅ Mapped |
| SwapLightAndDarkStyleTransformation | SwapLightAndDarkStyleTransformation | ✅ Mapped |
| ReverseStyleTransformation | ReverseStyleTransformation | ✅ Mapped |
| SetDefaultColorStyleTransformation | SetDefaultColorStyleTransformation | ✅ Mapped |
| AdjustBrightnessStyleTransformation | AdjustBrightnessStyleTransformation | ✅ Mapped |
| DummyStyleTransformation | DummyStyleTransformation | ✅ Mapped |
| ConditionalStyleTransformation | ConditionalStyleTransformation | ✅ Mapped |
| DynamicStyleTransformation | DynamicStyleTransformation | ✅ Mapped |
| Priority | Priority | ✅ Mapped |
| DEFAULT_ATTRS | Attrs.Default / DefaultAttrs.Default | ✅ Mapped |
| ANSI_COLOR_NAMES | AnsiColorNames.Names | ✅ Mapped |
| NAMED_COLORS | NamedColors.Colors | ✅ Mapped |
| merge_styles | StyleMerger.MergeStyles | ✅ Mapped |
| parse_color | StyleParser.ParseColor | ✅ Mapped |
| default_ui_style | DefaultStyles.DefaultUiStyle | ✅ Mapped |
| default_pygments_style | DefaultStyles.DefaultPygmentsStyle | ✅ Mapped |
| merge_style_transformations | StyleTransformationMerger.MergeStyleTransformations | ✅ Mapped |
