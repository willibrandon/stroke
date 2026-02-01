# Contract: Dialog

**Namespace**: `Stroke.Widgets.Dialogs`
**Python Source**: `prompt_toolkit/widgets/dialogs.py` lines 29-108

## API

```csharp
/// <summary>
/// Simple dialog window. Base for input dialogs, message dialogs, and confirmation dialogs.
/// </summary>
public class Dialog : IMagicContainer
{
    public Dialog(
        AnyContainer body,
        AnyFormattedText title = default,
        IReadOnlyList<Button>? buttons = null,
        bool modal = true,
        Dimension? width = null,
        bool withBackground = false);

    // Runtime-changeable
    public AnyContainer Body { get; set; }
    public AnyFormattedText Title { get; set; }

    // Component access (Box or Shadow depending on withBackground)
    public IMagicContainer Container { get; }

    // IMagicContainer
    public IContainer PtContainer();
}
```

## Layout Structure

### With Buttons

```
frame_body = HSplit [
  Box(
    body: DynamicContainer(() => this.Body),
    padding: D(preferred=1, max=1),
    paddingBottom: 0
  ),
  Box(
    body: VSplit(buttons, padding=1, keyBindings=buttonsKb),
    height: D(min=1, max=3, preferred=3)
  )
]
```

### Without Buttons

```
frame_body = body  (used directly)
```

### Button Navigation

When >1 button:
```
buttonsKb.Add("left", filter: ~firstSelected)(focusPrevious)
buttonsKb.Add("right", filter: ~lastSelected)(focusNext)
```

### Dialog Key Bindings

```
kb.Add("tab", filter: ~hasCompletions)(focusNext)
kb.Add("s-tab", filter: ~hasCompletions)(focusPrevious)
```

### Frame + Shadow Composition

```
frame = Shadow(
  Frame(
    title: () => this.Title,
    body: frame_body,
    style: "class:dialog.body",
    width: withBackground ? null : width,
    keyBindings: kb,
    modal: modal
  )
)
```

### With Background

```
container = Box(body: frame, style: "class:dialog", width: width)
```

### Without Background

```
container = frame  (Shadow directly)
```

## Notes

- Dialog uses `has_completions` filter to prevent Tab from cycling focus when completion menu is open
- `has_focus(buttons[0])` and `has_focus(buttons[-1])` used for Left/Right boundary conditions
- `focus_next` and `focus_previous` are imported from `prompt_toolkit.key_binding.bindings.focus` — these are `FocusFunctions.FocusNext` and `FocusFunctions.FocusPrevious` in Stroke
