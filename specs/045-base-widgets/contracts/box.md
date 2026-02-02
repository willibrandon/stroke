# Contract: Box

**Namespace**: `Stroke.Widgets.Base`
**Python Source**: `prompt_toolkit/widgets/base.py` lines 610-691

## API

```csharp
/// <summary>
/// Add padding around a container.
/// </summary>
public class Box : IMagicContainer
{
    public Box(
        AnyContainer body,
        Dimension? padding = null,
        Dimension? paddingLeft = null,
        Dimension? paddingRight = null,
        Dimension? paddingTop = null,
        Dimension? paddingBottom = null,
        Dimension? width = null,
        Dimension? height = null,
        string style = "",
        string? @char = null,
        bool modal = false,
        IKeyBindingsBase? keyBindings = null);

    // Runtime-changeable padding
    public Dimension? Padding { get; set; }
    public Dimension? PaddingLeft { get; set; }
    public Dimension? PaddingRight { get; set; }
    public Dimension? PaddingTop { get; set; }
    public Dimension? PaddingBottom { get; set; }
    public AnyContainer Body { get; set; }

    // Component access
    public HSplit Container { get; }

    // IMagicContainer
    public IContainer PtContainer();  // returns Container
}
```

## Layout Structure

```
HSplit(width, height, style, modal, keyBindings=null) [
  Window(height=top(), char=char),
  VSplit [
    Window(width=left(), char=char),
    body,
    Window(width=right(), char=char)
  ],
  Window(height=bottom(), char=char)
]
```

## Padding Resolution

Each side resolves padding with fallback:
```
left()  → PaddingLeft  ?? Padding
right() → PaddingRight ?? Padding
top()   → PaddingTop   ?? Padding
bottom()→ PaddingBottom ?? Padding
```

## Notes

- Python source passes `key_bindings=None` explicitly to HSplit (not `keyBindings` param)
- `char` parameter for fill character (e.g., to fill padding with a specific character)
