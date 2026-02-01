# Contract: RadioList\<T\>

**Namespace**: `Stroke.Widgets.Lists`
**Python Source**: `prompt_toolkit/widgets/base.py` lines 901-947

## API

```csharp
/// <summary>
/// List of radio buttons. Only one can be checked at a time.
/// </summary>
public class RadioList<T> : DialogList<T>
{
    public RadioList(
        IReadOnlyList<(T Value, AnyFormattedText Label)> values,
        T? @default = default,
        bool showNumbers = false,
        bool selectOnFocus = false,
        string openCharacter = "(",
        string selectCharacter = "*",
        string closeCharacter = ")",
        string containerStyle = "class:radio-list",
        string defaultStyle = "class:radio",
        string selectedStyle = "class:radio-selected",
        string checkedStyle = "class:radio-checked",
        string numberStyle = "class:radio-number",
        bool multipleSelection = false,
        bool showCursor = true,
        bool showScrollbar = true);
}
```

## Notes

- Always passes `multipleSelection: false` to base class (Python hardcodes this)
- The `multipleSelection` parameter exists in Python's constructor signature but is always overridden to `False`
- Converts single `default` value to `defaultValues: [default]` list for base class
- When `default` is null, passes `defaultValues: null` to use first item
