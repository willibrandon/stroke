# Contract: VerticalLine & HorizontalLine

**Namespace**: `Stroke.Widgets.Base`
**Python Source**: `prompt_toolkit/widgets/base.py` lines 1008-1033

## API

```csharp
/// <summary>
/// A simple vertical line with a width of 1.
/// </summary>
public class VerticalLine : IMagicContainer
{
    public VerticalLine();

    public Window Window { get; }
    public IContainer PtContainer();  // returns Window
}

/// <summary>
/// A simple horizontal line with a height of 1.
/// </summary>
public class HorizontalLine : IMagicContainer
{
    public HorizontalLine();

    public Window Window { get; }
    public IContainer PtContainer();  // returns Window
}
```

## Implementation

```
VerticalLine.Window = new Window(
    char: Border.Vertical,
    style: "class:line,vertical-line",
    width: Dimension.Exact(1));

HorizontalLine.Window = new Window(
    char: Border.Horizontal,
    style: "class:line,horizontal-line",
    height: Dimension.Exact(1));
```

## Notes

- Stateless after construction
- No thread safety needed
