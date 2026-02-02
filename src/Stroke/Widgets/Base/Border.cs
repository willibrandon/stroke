namespace Stroke.Widgets.Base;

/// <summary>
/// Box drawing characters. (Thin)
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>Border</c> class from <c>widgets/base.py</c>.
/// </remarks>
public static class Border
{
    /// <summary>Horizontal line: ─ (U+2500)</summary>
    public const string Horizontal = "\u2500";

    /// <summary>Vertical line: │ (U+2502)</summary>
    public const string Vertical = "\u2502";

    /// <summary>Top-left corner: ┌ (U+250C)</summary>
    public const string TopLeft = "\u250c";

    /// <summary>Top-right corner: ┐ (U+2510)</summary>
    public const string TopRight = "\u2510";

    /// <summary>Bottom-left corner: └ (U+2514)</summary>
    public const string BottomLeft = "\u2514";

    /// <summary>Bottom-right corner: ┘ (U+2518)</summary>
    public const string BottomRight = "\u2518";
}
