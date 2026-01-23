# Feature 114: Named Colors

## Overview

Implement the NAMED_COLORS dictionary - a mapping of 140 CSS/HTML color names to their hex values, matching the colors supported by all modern web browsers.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/styles/named_colors.py`

## Public API

### NAMED_COLORS

```csharp
namespace Stroke.Styles;

/// <summary>
/// Named color constants supported by all modern web browsers.
/// Contains 140 standard color names mapped to hex values.
/// </summary>
/// <remarks>
/// Color names are case-insensitive when used in styles.
/// Reference: https://www.w3schools.com/colors/colors_names.asp
/// </remarks>
public static class NamedColors
{
    /// <summary>
    /// Dictionary mapping color names to hex values.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Colors;

    // Individual color constants
    public const string AliceBlue = "#f0f8ff";
    public const string AntiqueWhite = "#faebd7";
    public const string Aqua = "#00ffff";
    public const string Aquamarine = "#7fffd4";
    public const string Azure = "#f0ffff";
    public const string Beige = "#f5f5dc";
    public const string Bisque = "#ffe4c4";
    public const string Black = "#000000";
    public const string BlanchedAlmond = "#ffebcd";
    public const string Blue = "#0000ff";
    public const string BlueViolet = "#8a2be2";
    public const string Brown = "#a52a2a";
    public const string BurlyWood = "#deb887";
    public const string CadetBlue = "#5f9ea0";
    public const string Chartreuse = "#7fff00";
    public const string Chocolate = "#d2691e";
    public const string Coral = "#ff7f50";
    public const string CornflowerBlue = "#6495ed";
    public const string Cornsilk = "#fff8dc";
    public const string Crimson = "#dc143c";
    public const string Cyan = "#00ffff";
    public const string DarkBlue = "#00008b";
    public const string DarkCyan = "#008b8b";
    public const string DarkGoldenRod = "#b8860b";
    public const string DarkGray = "#a9a9a9";
    public const string DarkGreen = "#006400";
    public const string DarkGrey = "#a9a9a9";
    public const string DarkKhaki = "#bdb76b";
    public const string DarkMagenta = "#8b008b";
    public const string DarkOliveGreen = "#556b2f";
    public const string DarkOrange = "#ff8c00";
    public const string DarkOrchid = "#9932cc";
    public const string DarkRed = "#8b0000";
    public const string DarkSalmon = "#e9967a";
    public const string DarkSeaGreen = "#8fbc8f";
    public const string DarkSlateBlue = "#483d8b";
    public const string DarkSlateGray = "#2f4f4f";
    public const string DarkSlateGrey = "#2f4f4f";
    public const string DarkTurquoise = "#00ced1";
    public const string DarkViolet = "#9400d3";
    public const string DeepPink = "#ff1493";
    public const string DeepSkyBlue = "#00bfff";
    public const string DimGray = "#696969";
    public const string DimGrey = "#696969";
    public const string DodgerBlue = "#1e90ff";
    public const string FireBrick = "#b22222";
    public const string FloralWhite = "#fffaf0";
    public const string ForestGreen = "#228b22";
    public const string Fuchsia = "#ff00ff";
    public const string Gainsboro = "#dcdcdc";
    public const string GhostWhite = "#f8f8ff";
    public const string Gold = "#ffd700";
    public const string GoldenRod = "#daa520";
    public const string Gray = "#808080";
    public const string Green = "#008000";
    public const string GreenYellow = "#adff2f";
    public const string Grey = "#808080";
    public const string HoneyDew = "#f0fff0";
    public const string HotPink = "#ff69b4";
    public const string IndianRed = "#cd5c5c";
    public const string Indigo = "#4b0082";
    public const string Ivory = "#fffff0";
    public const string Khaki = "#f0e68c";
    public const string Lavender = "#e6e6fa";
    public const string LavenderBlush = "#fff0f5";
    public const string LawnGreen = "#7cfc00";
    public const string LemonChiffon = "#fffacd";
    public const string LightBlue = "#add8e6";
    public const string LightCoral = "#f08080";
    public const string LightCyan = "#e0ffff";
    public const string LightGoldenRodYellow = "#fafad2";
    public const string LightGray = "#d3d3d3";
    public const string LightGreen = "#90ee90";
    public const string LightGrey = "#d3d3d3";
    public const string LightPink = "#ffb6c1";
    public const string LightSalmon = "#ffa07a";
    public const string LightSeaGreen = "#20b2aa";
    public const string LightSkyBlue = "#87cefa";
    public const string LightSlateGray = "#778899";
    public const string LightSlateGrey = "#778899";
    public const string LightSteelBlue = "#b0c4de";
    public const string LightYellow = "#ffffe0";
    public const string Lime = "#00ff00";
    public const string LimeGreen = "#32cd32";
    public const string Linen = "#faf0e6";
    public const string Magenta = "#ff00ff";
    public const string Maroon = "#800000";
    public const string MediumAquaMarine = "#66cdaa";
    public const string MediumBlue = "#0000cd";
    public const string MediumOrchid = "#ba55d3";
    public const string MediumPurple = "#9370db";
    public const string MediumSeaGreen = "#3cb371";
    public const string MediumSlateBlue = "#7b68ee";
    public const string MediumSpringGreen = "#00fa9a";
    public const string MediumTurquoise = "#48d1cc";
    public const string MediumVioletRed = "#c71585";
    public const string MidnightBlue = "#191970";
    public const string MintCream = "#f5fffa";
    public const string MistyRose = "#ffe4e1";
    public const string Moccasin = "#ffe4b5";
    public const string NavajoWhite = "#ffdead";
    public const string Navy = "#000080";
    public const string OldLace = "#fdf5e6";
    public const string Olive = "#808000";
    public const string OliveDrab = "#6b8e23";
    public const string Orange = "#ffa500";
    public const string OrangeRed = "#ff4500";
    public const string Orchid = "#da70d6";
    public const string PaleGoldenRod = "#eee8aa";
    public const string PaleGreen = "#98fb98";
    public const string PaleTurquoise = "#afeeee";
    public const string PaleVioletRed = "#db7093";
    public const string PapayaWhip = "#ffefd5";
    public const string PeachPuff = "#ffdab9";
    public const string Peru = "#cd853f";
    public const string Pink = "#ffc0cb";
    public const string Plum = "#dda0dd";
    public const string PowderBlue = "#b0e0e6";
    public const string Purple = "#800080";
    public const string RebeccaPurple = "#663399";
    public const string Red = "#ff0000";
    public const string RosyBrown = "#bc8f8f";
    public const string RoyalBlue = "#4169e1";
    public const string SaddleBrown = "#8b4513";
    public const string Salmon = "#fa8072";
    public const string SandyBrown = "#f4a460";
    public const string SeaGreen = "#2e8b57";
    public const string SeaShell = "#fff5ee";
    public const string Sienna = "#a0522d";
    public const string Silver = "#c0c0c0";
    public const string SkyBlue = "#87ceeb";
    public const string SlateBlue = "#6a5acd";
    public const string SlateGray = "#708090";
    public const string SlateGrey = "#708090";
    public const string Snow = "#fffafa";
    public const string SpringGreen = "#00ff7f";
    public const string SteelBlue = "#4682b4";
    public const string Tan = "#d2b48c";
    public const string Teal = "#008080";
    public const string Thistle = "#d8bfd8";
    public const string Tomato = "#ff6347";
    public const string Turquoise = "#40e0d0";
    public const string Violet = "#ee82ee";
    public const string Wheat = "#f5deb3";
    public const string White = "#ffffff";
    public const string WhiteSmoke = "#f5f5f5";
    public const string Yellow = "#ffff00";
    public const string YellowGreen = "#9acd32";

    /// <summary>
    /// Try to get a hex color value by name.
    /// </summary>
    /// <param name="name">Color name (case-insensitive).</param>
    /// <param name="hexValue">The hex value if found.</param>
    /// <returns>True if the color name was found.</returns>
    public static bool TryGetColor(string name, out string hexValue);

    /// <summary>
    /// Get a hex color value by name.
    /// </summary>
    /// <param name="name">Color name (case-insensitive).</param>
    /// <returns>The hex value.</returns>
    /// <exception cref="KeyNotFoundException">If color name not found.</exception>
    public static string GetColor(string name);
}
```

## Project Structure

```
src/Stroke/
└── Styles/
    └── NamedColors.cs
tests/Stroke.Tests/
└── Styles/
    └── NamedColorsTests.cs
```

## Implementation Notes

### NamedColors Implementation

```csharp
namespace Stroke.Styles;

public static class NamedColors
{
    private static readonly Dictionary<string, string> _colors;
    private static readonly Dictionary<string, string> _colorsLower;

    static NamedColors()
    {
        _colors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AliceBlue"] = "#f0f8ff",
            ["AntiqueWhite"] = "#faebd7",
            ["Aqua"] = "#00ffff",
            ["Aquamarine"] = "#7fffd4",
            ["Azure"] = "#f0ffff",
            // ... all 140 colors
            ["Yellow"] = "#ffff00",
            ["YellowGreen"] = "#9acd32"
        };

        Colors = _colors.AsReadOnly();
    }

    public static readonly IReadOnlyDictionary<string, string> Colors;

    // All const declarations...

    public static bool TryGetColor(string name, out string hexValue)
    {
        return _colors.TryGetValue(name, out hexValue!);
    }

    public static string GetColor(string name)
    {
        if (_colors.TryGetValue(name, out var hexValue))
            return hexValue;
        throw new KeyNotFoundException($"Unknown color name: {name}");
    }
}
```

### Usage in Style Parsing

```csharp
public static class StyleParser
{
    public static Color ParseColor(string colorSpec)
    {
        // Check for hex color
        if (colorSpec.StartsWith('#'))
        {
            return Color.FromHex(colorSpec);
        }

        // Check for named color
        if (NamedColors.TryGetColor(colorSpec, out var hexValue))
        {
            return Color.FromHex(hexValue);
        }

        // Check for ANSI color
        if (colorSpec.StartsWith("ansi"))
        {
            return Color.FromAnsi(colorSpec);
        }

        throw new ArgumentException($"Invalid color: {colorSpec}");
    }
}
```

### Usage Example

```csharp
// Use named color in style
var style = Style.FromDict(new Dictionary<string, string>
{
    ["keyword"] = $"fg:{NamedColors.DodgerBlue} bold",
    ["string"] = $"fg:{NamedColors.ForestGreen}",
    ["error"] = $"fg:{NamedColors.Crimson} bg:{NamedColors.MistyRose}"
});

// Lookup color by name
var hex = NamedColors.GetColor("CornflowerBlue"); // "#6495ed"

// Case-insensitive lookup
if (NamedColors.TryGetColor("DARKSLATEGRAY", out var color))
{
    Console.WriteLine($"Color: {color}"); // "#2f4f4f"
}
```

## Dependencies

- None (standalone utility)

## Implementation Tasks

1. Define all 140 color constants
2. Create case-insensitive dictionary
3. Implement TryGetColor method
4. Implement GetColor method
5. Write unit tests

## Acceptance Criteria

- [ ] All 140 standard colors defined
- [ ] Color lookup is case-insensitive
- [ ] Both Gray/Grey variants included
- [ ] Hex values are lowercase
- [ ] TryGetColor handles unknown names
- [ ] Unit tests achieve 80% coverage
