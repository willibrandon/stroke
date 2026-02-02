# Contract: Border

**Namespace**: `Stroke.Widgets.Base`
**Python Source**: `prompt_toolkit/widgets/base.py` lines 101-109

## API

```csharp
/// <summary>
/// Box drawing characters (thin lines).
/// </summary>
public static class Border
{
    public const string Horizontal = "\u2500";   // ─
    public const string Vertical = "\u2502";     // │
    public const string TopLeft = "\u250c";      // ┌
    public const string TopRight = "\u2510";     // ┐
    public const string BottomLeft = "\u2514";   // └
    public const string BottomRight = "\u2518";  // ┘
}
```

## Notes

- Static class with `const` string fields (not `char`) to match Python string semantics
- Referenced by Frame, VerticalLine, HorizontalLine
