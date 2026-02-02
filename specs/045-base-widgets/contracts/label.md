# Contract: Label

**Namespace**: `Stroke.Widgets.Base`
**Python Source**: `prompt_toolkit/widgets/base.py` lines 324-383

## API

```csharp
/// <summary>
/// Widget that displays text. Not editable or focusable.
/// </summary>
public class Label : IMagicContainer
{
    public Label(
        AnyFormattedText text,
        string style = "",
        Dimension? width = null,
        bool dontExtendHeight = true,
        bool dontExtendWidth = false,
        WindowAlign align = WindowAlign.Left,
        FilterOrBool wrapLines = default);  // defaults to true

    // Writable field for runtime text changes
    public AnyFormattedText Text { get; set; }

    // Component access
    public FormattedTextControl FormattedTextControl { get; }
    public Window Window { get; }

    // IMagicContainer
    public IContainer PtContainer();  // returns Window
}
```

## Width Calculation

When `width` is null, the label auto-calculates preferred width:
```
if text is empty → Dimension(preferred: 0)
else → Dimension(preferred: max line width using get_cwidth)
```

This uses `FormattedTextUtils.ToFormattedText()` to resolve the text, then `FormattedTextUtils.FragmentListToText()` to get plain text, then splits by newline and finds the longest line using `UnicodeWidth.GetWidth()`.

## Key Behaviors

- **Non-focusable**: Window has no focusable control
- **Dynamic text**: `FormattedTextControl` uses `() => this.Text` lambda
- **Style**: `"class:label " + style`
- **Default**: `dontExtendHeight=true` (unlike most widgets)
