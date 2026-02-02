# Data Model: Base Widgets

**Feature**: 045-base-widgets
**Date**: 2026-02-01

## Entity Model

### Border (Static Constants)

| Field | Type | Description |
|-------|------|-------------|
| Horizontal | `string` (const) | `"\u2500"` (─) |
| Vertical | `string` (const) | `"\u2502"` (│) |
| TopLeft | `string` (const) | `"\u250c"` (┌) |
| TopRight | `string` (const) | `"\u2510"` (┐) |
| BottomLeft | `string` (const) | `"\u2514"` (└) |
| BottomRight | `string` (const) | `"\u2518"` (┘) |

**Relationships**: Referenced by Frame (border drawing), VerticalLine, HorizontalLine.
**State**: Stateless (static constants).

---

### TextArea

| Field | Type | Mutable | Description |
|-------|------|---------|-------------|
| Completer | `ICompleter?` | Yes | Auto-completion provider |
| CompleteWhileTyping | `FilterOrBool` | Yes | Whether to complete while typing |
| Lexer | `ILexer?` | Yes | Syntax highlighting lexer |
| AutoSuggest | `IAutoSuggest?` | Yes | Input suggestion provider |
| ReadOnly | `FilterOrBool` | Yes | Whether input is read-only |
| WrapLines | `FilterOrBool` | Yes | Whether to wrap long lines |
| Validator | `IValidator?` | Yes | Input validator |
| Buffer | `Buffer` | No (ref) | The underlying mutable buffer |
| Control | `BufferControl` | No (ref) | The buffer display control |
| Window | `Window` | No (ref) | The rendering window |

**Properties** (computed):
- `Text`: get/set → delegates to Buffer.Text / Document setter
- `Document`: get/set → delegates to Buffer.Document / SetDocument(bypass)
- `AcceptHandler`: get/set → delegates to Buffer.AcceptHandler

**Relationships**: Composes Buffer, BufferControl, Window. Optionally references SearchToolbar.
**Thread Safety**: Delegates to Buffer (already thread-safe). Mutable configuration fields (`Completer`, `Lexer`, `AutoSuggest`, `ReadOnly`, `WrapLines`, `Validator`, `CompleteWhileTyping`) are reference-type or small value-type fields. Individual field writes are atomic for reference types in .NET (guaranteed by the CLR memory model). These fields are read via lambda closures captured in the Buffer/BufferControl constructors (e.g., `readOnly: () => FilterUtils.IsTrue(this.ReadOnly)`), so reads always see the latest written value. No additional Lock synchronization is needed — the atomic write guarantee is sufficient because each field is independently readable/writable and there are no compound read-modify-write operations on these fields.

---

### Label

| Field | Type | Mutable | Description |
|-------|------|---------|-------------|
| Text | `AnyFormattedText` | Yes | Display text (can be callable) |
| FormattedTextControl | `FormattedTextControl` | No (ref) | Inner display control |
| Window | `Window` | No (ref) | Rendering window |

**Width Calculation**: When no explicit width, computes `Dimension(preferred: longestLine)` from text fragments.
**Relationships**: Composes FormattedTextControl + Window.
**Thread Safety**: Stateless construction. Text field is reference-type, atomic write.

---

### Button

| Field | Type | Mutable | Description |
|-------|------|---------|-------------|
| Text | `string` | Yes | Button caption |
| LeftSymbol | `string` | No | Left border character (default `<`) |
| RightSymbol | `string` | No | Right border character (default `>`) |
| Handler | `Action?` | Yes | Click handler |
| Width | `int` | No | Button width in characters |
| Control | `FormattedTextControl` | No (ref) | Inner control |
| Window | `Window` | No (ref) | Rendering window |

**Text Fragment Generation**: Produces `[(style, leftSymbol, mouseHandler), ([SetCursorPosition], ""), (style, centeredText, mouseHandler), (style, rightSymbol, mouseHandler)]`.
**Key Bindings**: Space and Enter trigger handler.
**Mouse**: MOUSE_UP triggers handler.
**Style**: `class:button.focused` when focused, `class:button` when unfocused.
**Thread Safety**: `Handler` (`Action?`) and `Text` (`string`) are both reference-type fields. Individual writes are atomic under the CLR memory model. No Lock synchronization is needed — the `FormattedTextControl` reads `Text` via a lambda closure and always sees the latest written value. `Handler` is read by key/mouse handlers; a null-check guard prevents invocation of a cleared handler. There are no compound read-modify-write operations on these fields.

---

### Frame

| Field | Type | Mutable | Description |
|-------|------|---------|-------------|
| Title | `AnyFormattedText` | Yes | Title text (runtime-changeable) |
| Body | `AnyContainer` | Yes | Inner content (runtime-changeable) |
| Container | `HSplit` | No (ref) | The composed border layout |

**Layout Structure**:
```
HSplit [
  ConditionalContainer(
    content: VSplit[TopLeft, Horizontal, "|", Label(title), "|", Horizontal, TopRight],  // with title
    alternative: VSplit[TopLeft, Horizontal, TopRight],                                    // without title
    filter: has_title
  ),
  VSplit[Vertical, DynamicContainer(body), Vertical],
  VSplit[BottomLeft, Horizontal, BottomRight]
]
```

**Thread Safety**: Title and Body are reference-type, atomic write. DynamicContainer handles runtime body changes.

---

### Shadow

| Field | Type | Mutable | Description |
|-------|------|---------|-------------|
| Container | `FloatContainer` | No (ref) | Body with shadow floats |

**Layout Structure**:
```
FloatContainer(
  content: body,
  floats: [
    Float(bottom=-1, height=1, left=1, right=-1, transparent=true, style="class:shadow"),  // bottom shadow
    Float(bottom=-1, top=1, width=1, right=-1, transparent=true, style="class:shadow")     // right shadow
  ]
)
```

**Thread Safety**: Stateless after construction.

---

### Box

| Field | Type | Mutable | Description |
|-------|------|---------|-------------|
| Padding | `Dimension?` | Yes | Overall padding |
| PaddingLeft | `Dimension?` | Yes | Left padding override |
| PaddingRight | `Dimension?` | Yes | Right padding override |
| PaddingTop | `Dimension?` | Yes | Top padding override |
| PaddingBottom | `Dimension?` | Yes | Bottom padding override |
| Body | `AnyContainer` | Yes | Inner content |
| Container | `HSplit` | No (ref) | The composed padding layout |

**Layout Structure**:
```
HSplit [
  Window(height=top),
  VSplit [Window(width=left), body, Window(width=right)],
  Window(height=bottom)
]
```

**Thread Safety**: Padding fields are value-type/nullable, read by lambda closures. Individual writes atomic.

---

### DialogList\<T\>

| Field | Type | Mutable | Description |
|-------|------|---------|-------------|
| Values | `IReadOnlyList<(T Value, AnyFormattedText Label)>` | No | Available items |
| ShowNumbers | `bool` | Yes | Whether to show number shortcuts |
| CurrentValue | `T` | Yes | Selected value (single-select mode) |
| CurrentValues | `List<T>` | Yes | Selected values (multi-select mode) |
| _selectedIndex | `int` | Yes | Cursor position |
| MultipleSelection | `bool` | No | Selection mode flag |
| Control | `FormattedTextControl` | No (ref) | Inner display control |
| Window | `Window` | No (ref) | Rendering window with scrollbar |

**Key Bindings**: Up/Down/j/k (navigate), PageUp/PageDown, Enter/Space (toggle), 1-9 (number jump), Any (character jump).
**Mouse**: MOUSE_UP on row selects/toggles.

**Thread Safety**:
- `Lock _lock` protects `_selectedIndex`, `CurrentValue`, `CurrentValues`
- **Atomic operations**: Individual property reads/writes (e.g., `SelectedIndex` getter/setter) each acquire and release the lock independently
- **Compound operations requiring single lock scope**: The `_HandleEnter` logic (read `_selectedIndex` → lookup `values[index].Value` → modify `CurrentValues` add/remove or set `CurrentValue`) MUST execute under a single lock acquisition to prevent interleaving where another thread changes `_selectedIndex` between the read and the lookup
- **All key/mouse handlers** MUST acquire the lock before any mutation of `_selectedIndex`, `CurrentValue`, or `CurrentValues`
- **`CurrentValues` (`List<T>`)**: All mutations (Add, Remove, Clear, reassignment) MUST be performed while holding the lock. Concurrent read-without-lock is NOT safe because `List<T>` is not thread-safe; callers performing compound read-then-write operations (e.g., check-then-add) MUST use external synchronization
- **External synchronization**: Callers performing compound operations across multiple property accesses (e.g., read `SelectedIndex` then set `CurrentValue`) must synchronize externally since each individual property access is independently atomic

**Validation**: `values` list must not be empty (throws `ArgumentException`).

---

### RadioList\<T\>

Extends `DialogList<T>` with `MultipleSelection = false`.

| Parameter | Default |
|-----------|---------|
| OpenCharacter | `(` |
| SelectCharacter | `*` |
| CloseCharacter | `)` |
| ContainerStyle | `class:radio-list` |
| DefaultStyle | `class:radio` |
| SelectedStyle | `class:radio-selected` |
| CheckedStyle | `class:radio-checked` |

---

### CheckboxList\<T\>

Extends `DialogList<T>` with `MultipleSelection = true`.

| Parameter | Default |
|-----------|---------|
| OpenCharacter | `[` |
| SelectCharacter | `*` |
| CloseCharacter | `]` |
| ContainerStyle | `class:checkbox-list` |
| DefaultStyle | `class:checkbox` |
| SelectedStyle | `class:checkbox-selected` |
| CheckedStyle | `class:checkbox-checked` |

---

### Checkbox

Extends `CheckboxList<string>` with a single item `("value", text)`.

| Field | Type | Mutable | Description |
|-------|------|---------|-------------|
| Checked | `bool` | Yes (property) | Maps to `"value" in CurrentValues` |

**Validation**: `ShowScrollbar = false` (class-level override).

---

### ProgressBar

| Field | Type | Mutable | Description |
|-------|------|---------|-------------|
| _percentage | `int` | Yes | Current percentage (default 60) |
| Label | `Label` | No (ref) | Percentage text display |
| Container | `FloatContainer` | No (ref) | Layered bar + label |

**Layout Structure**:
```
FloatContainer(
  content: Window(height=1),
  floats: [
    Float(content: Label("60%"), top=0, bottom=0),
    Float(left=0, top=0, right=0, bottom=0,
      content: VSplit [
        Window(style="class:progress-bar.used", width=D(weight=percentage)),
        Window(style="class:progress-bar", width=D(weight=100-percentage))
      ]
    )
  ]
)
```

**Thread Safety**: `Lock _lock` protects `_percentage` read/write. Setting `Percentage` MUST update both `_percentage` and `Label.Text` within the same lock scope to prevent a reader from seeing a percentage value inconsistent with the label text. The compound operation (set `_percentage` → format string → update `Label.Text`) is a single atomic unit under the lock.

---

### VerticalLine / HorizontalLine

| Widget | Inner Window Config |
|--------|-------------------|
| VerticalLine | `Window(char=Border.Vertical, style="class:line,vertical-line", width=1)` |
| HorizontalLine | `Window(char=Border.Horizontal, style="class:line,horizontal-line", height=1)` |

**Thread Safety**: Stateless after construction.

---

### Dialog

| Field | Type | Mutable | Description |
|-------|------|---------|-------------|
| Body | `AnyContainer` | Yes | Dialog content (runtime-changeable) |
| Title | `AnyFormattedText` | Yes | Dialog title (runtime-changeable) |
| Container | `Box` or `Shadow` | No (ref) | The composed dialog layout |

**Layout Structure** (with buttons):
```
Shadow(
  Frame(
    title: lambda: self.title,
    body: HSplit [
      Box(body: DynamicContainer(lambda: self.body), padding=D(preferred=1, max=1), paddingBottom=0),
      Box(body: VSplit(buttons, padding=1, keyBindings=buttonsKb), height=D(min=1, max=3, preferred=3))
    ],
    style="class:dialog.body",
    keyBindings=kb,  // Tab/Shift-Tab
    modal=modal
  )
)
```

**With Background**: Wraps in `Box(body=frame, style="class:dialog", width=width)`.
**Without Buttons**: Uses body directly as frame content.
**Key Bindings**: Tab/Shift-Tab for focus cycling. Left/Right for button navigation (when >1 button).
**Thread Safety**: Body and Title are reference-type, atomic write.
