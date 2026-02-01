# Contract: Button

**Namespace**: `Stroke.Widgets.Base`
**Python Source**: `prompt_toolkit/widgets/base.py` lines 386-473

## API

```csharp
/// <summary>
/// Clickable button widget.
/// </summary>
public class Button : IMagicContainer
{
    public Button(
        string text,
        Action? handler = null,
        int width = 12,
        string leftSymbol = "<",
        string rightSymbol = ">");

    public string Text { get; set; }
    public string LeftSymbol { get; }
    public string RightSymbol { get; }
    public Action? Handler { get; set; }
    public int Width { get; }
    public FormattedTextControl Control { get; }
    public Window Window { get; }

    // IMagicContainer
    public IContainer PtContainer();  // returns Window
}
```

## Text Fragment Generation

```csharp
private IReadOnlyList<StyleAndTextTuple> GetTextFragments()
{
    // Calculate available width for text centering
    int availableWidth = Width
        - (UnicodeWidth.GetWidth(LeftSymbol) + UnicodeWidth.GetWidth(RightSymbol))
        + (Text.Length - UnicodeWidth.GetWidth(Text));
    string centeredText = Text.PadLeft((availableWidth + Text.Length) / 2)
                              .PadRight(Math.Max(0, availableWidth));

    // Mouse handler on all text fragments
    Action<MouseEvent> mouseHandler = (e) => {
        if (Handler != null && e.EventType == MouseEventType.MouseUp)
            Handler();
    };

    return [
        new("class:button.arrow", LeftSymbol, mouseHandler),
        new("[SetCursorPosition]", ""),
        new("class:button.text", centeredText, mouseHandler),
        new("class:button.arrow", RightSymbol, mouseHandler),
    ];
}
```

## Key Bindings

- **Space**: Invoke handler
- **Enter**: Invoke handler

## Window Configuration

- `align: WindowAlign.Center`
- `height: 1`
- `width: Width`
- `style: () => focused ? "class:button.focused" : "class:button"`
- `dontExtendWidth: false`
- `dontExtendHeight: true`
