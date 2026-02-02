# Contract: Checkbox

**Namespace**: `Stroke.Widgets.Lists`
**Python Source**: `prompt_toolkit/widgets/base.py` lines 983-1005

## API

```csharp
/// <summary>
/// Convenience wrapper: creates a 1-item CheckboxList.
/// </summary>
public class Checkbox : CheckboxList<string>
{
    // Class-level override
    public new bool ShowScrollbar => false;

    public Checkbox(
        AnyFormattedText text = default,
        bool @checked = false);

    /// <summary>
    /// Gets or sets whether the checkbox is checked.
    /// </summary>
    public bool Checked { get; set; }
}
```

## Implementation

- Creates a single-item values list: `[("value", text)]`
- `Checked` getter: `"value" in CurrentValues`
- `Checked` setter: sets `CurrentValues = ["value"]` or `CurrentValues = []`
- `ShowScrollbar` overridden to `false` (class-level, not instance-level in Python)
