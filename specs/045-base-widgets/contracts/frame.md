# Contract: Frame

**Namespace**: `Stroke.Widgets.Base`
**Python Source**: `prompt_toolkit/widgets/base.py` lines 476-571

## API

```csharp
/// <summary>
/// Draw a border around any container, optionally with a title.
/// </summary>
public class Frame : IMagicContainer
{
    public Frame(
        AnyContainer body,
        AnyFormattedText title = default,
        string style = "",
        Dimension? width = null,
        Dimension? height = null,
        IKeyBindingsBase? keyBindings = null,
        bool modal = false);

    // Runtime-changeable
    public AnyFormattedText Title { get; set; }
    public AnyContainer Body { get; set; }

    // Component access
    public HSplit Container { get; }

    // IMagicContainer
    public IContainer PtContainer();  // returns Container
}
```

## Layout Structure

```
HSplit(style="class:frame " + style, width, height, keyBindings, modal) [
  ConditionalContainer(
    content: top_row_with_title,
    filter: has_title,
    alternativeContent: top_row_without_title
  ),
  VSplit(padding=0) [
    Window(width=1, char=Border.Vertical, style="class:frame.border"),
    DynamicContainer(() => this.Body),
    Window(width=1, char=Border.Vertical, style="class:frame.border")
  ],
  VSplit(height=1) [
    Window(width=1, height=1, char=Border.BottomLeft, style="class:frame.border"),
    Window(char=Border.Horizontal, style="class:frame.border"),
    Window(width=1, height=1, char=Border.BottomRight, style="class:frame.border")
  ]
]
```

### Top Row With Title (height=1)
```
VSplit [
  Window(1x1, Border.TopLeft),
  Window(Border.Horizontal),
  Window(1x1, "|"),
  Label(() => Template(" {} ").Format(this.Title), style="class:frame.label", dontExtendWidth=true),
  Window(1x1, "|"),
  Window(Border.Horizontal),
  Window(1x1, Border.TopRight)
]
```

### Top Row Without Title (height=1)
```
VSplit [
  Window(1x1, Border.TopLeft),
  Window(Border.Horizontal),
  Window(1x1, Border.TopRight)
]
```

## Key Behaviors

- **Dynamic title**: Switches between title/no-title via `ConditionalContainer` and `Condition`
- **Dynamic body**: `DynamicContainer(() => this.Body)` enables runtime body swapping
- **Border windows**: All use `style="class:frame.border"` via `partial(Window, style=...)` pattern
