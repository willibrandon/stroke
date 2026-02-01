# Contract: DialogList\<T\>

**Namespace**: `Stroke.Widgets.Lists`
**Python Source**: `prompt_toolkit/widgets/base.py` lines 697-898

## API

```csharp
/// <summary>
/// Common base class for RadioList and CheckboxList.
/// </summary>
/// <remarks>
/// Thread-safe. Mutable state protected by Lock.
/// </remarks>
public class DialogList<T> : IMagicContainer
{
    public DialogList(
        IReadOnlyList<(T Value, AnyFormattedText Label)> values,
        IReadOnlyList<T>? defaultValues = null,
        bool selectOnFocus = false,
        string openCharacter = "",
        string selectCharacter = "*",
        string closeCharacter = "",
        string containerStyle = "",
        string defaultStyle = "",
        string numberStyle = "",
        string selectedStyle = "",
        string checkedStyle = "",
        bool multipleSelection = false,
        bool showScrollbar = true,
        bool showCursor = true,
        bool showNumbers = false);

    // State
    public IReadOnlyList<(T Value, AnyFormattedText Label)> Values { get; }
    public bool ShowNumbers { get; set; }
    public T CurrentValue { get; set; }                   // single-select
    public List<T> CurrentValues { get; set; }             // multi-select
    public bool MultipleSelection { get; }

    // Style configuration
    public string OpenCharacter { get; }
    public string SelectCharacter { get; }
    public string CloseCharacter { get; }
    public string ContainerStyle { get; }
    public string DefaultStyle { get; }
    public string NumberStyle { get; }
    public string SelectedStyle { get; }
    public string CheckedStyle { get; }
    public bool ShowScrollbar { get; set; }

    // Component access
    public FormattedTextControl Control { get; }
    public Window Window { get; }

    // IMagicContainer
    public IContainer PtContainer();  // returns Window
}
```

## Key Bindings

| Key | Action |
|-----|--------|
| Up, k | Move cursor up (clamp at 0) |
| Down, j | Move cursor down (clamp at last) |
| PageUp | Move cursor up by visible lines count |
| PageDown | Move cursor down by visible lines count |
| Enter, Space | Toggle selection |
| 1-9 | Jump to Nth item (when showNumbers=true) |
| Any char | Jump to first item starting with that character |

## Selection Logic (_HandleEnter)

```
if multipleSelection:
    if val in currentValues: remove(val)
    else: append(val)
else:
    currentValue = values[selectedIndex].Value
```

## Text Fragment Generation

For each item:
```
[style] openCharacter
[SetCursorPosition] "" (if selected)
[style] selectCharacter or " " (if checked/unchecked)
[style] closeCharacter
[style + defaultStyle] " "
[style + numberStyle] "N. " (if showNumbers)
[style + defaultStyle] label text
"" "\n"
```
All fragments get mouse handler. Last newline removed.

## Validation

- `values.Count == 0` → throws `ArgumentException`
- Default value not in values → falls back to first item

## Thread Safety

- `Lock _lock` protects `_selectedIndex`, `CurrentValue`, `CurrentValues`
- All key/mouse handlers acquire lock before mutation
- **_HandleEnter compound operation**: The sequence (read `_selectedIndex` → lookup `values[_selectedIndex].Value` → add/remove from `CurrentValues` or set `CurrentValue`) MUST execute under a single lock acquisition. This prevents another thread from changing `_selectedIndex` between the read and the lookup.
- **CurrentValues (`List<T>`)**: Not inherently thread-safe. All mutations (Add, Remove, Clear, full reassignment) MUST be performed while holding `_lock`. The `Checked` state query (`currentValues.Contains(value)`) also requires the lock for consistent reads.
- **Individual property access**: Each `SelectedIndex`, `CurrentValue`, `CurrentValues` get/set independently acquires and releases the lock. Callers performing compound operations across multiple properties must synchronize externally.
