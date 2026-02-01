# Contract: CheckboxList\<T\>

**Namespace**: `Stroke.Widgets.Lists`
**Python Source**: `prompt_toolkit/widgets/base.py` lines 950-980

## API

```csharp
/// <summary>
/// List of checkbox buttons. Several can be checked at the same time.
/// </summary>
public class CheckboxList<T> : DialogList<T>
{
    public CheckboxList(
        IReadOnlyList<(T Value, AnyFormattedText Label)> values,
        IReadOnlyList<T>? defaultValues = null,
        string openCharacter = "[",
        string selectCharacter = "*",
        string closeCharacter = "]",
        string containerStyle = "class:checkbox-list",
        string defaultStyle = "class:checkbox",
        string selectedStyle = "class:checkbox-selected",
        string checkedStyle = "class:checkbox-checked");
}
```

## Notes

- Always passes `multipleSelection: true` to base class
- Simpler constructor than RadioList — no `default`, `showNumbers`, `selectOnFocus`, `showCursor`, `showScrollbar` parameters (uses base defaults)
