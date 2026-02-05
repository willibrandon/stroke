# Research: Full-Screen Examples

**Feature**: 064-fullscreen-examples
**Date**: 2026-02-05

## Overview

This document analyzes all 25 Python Prompt Toolkit full-screen examples to identify API patterns, dependencies, and C# translation requirements.

## Python Example Analysis

### Main Examples (10)

| # | Python File | Key APIs Used | Complexity | C# Dependencies |
|---|-------------|---------------|------------|-----------------|
| 1 | `hello-world.py` | Application, Layout, Box, Frame, TextArea, KeyBindings | Basic | Application<object>, Layout, Box, Frame, TextArea, KeyBindings |
| 2 | `dummy-app.py` | Application | Basic | Application<object> (minimal) |
| 3 | `no-layout.py` | Application (no layout) | Basic | Application<object> (null layout) |
| 4 | `buttons.py` | Button, Label, HSplit, VSplit, Box, Frame, TextArea, Style, focus_next/focus_previous | Intermediate | Button, Label, HSplit, VSplit, Box, Frame, TextArea, Style, FocusFunctions |
| 5 | `calculator.py` | TextArea (accept_handler), HSplit, Window, SearchToolbar, Document, Style | Intermediate | TextArea, HSplit, Window, SearchToolbar, Document, Style |
| 6 | `split-screen.py` | Buffer, BufferControl, Window, VSplit, HSplit, FormattedTextControl, on_text_changed | Intermediate | Buffer, BufferControl, Window, VSplit, HSplit, FormattedTextControl |
| 7 | `pager.py` | TextArea (read_only, scrollbar, line_numbers), SearchToolbar, PygmentsLexer, FormattedTextControl | Intermediate | TextArea, SearchToolbar, PygmentsLexer, FormattedTextControl |
| 8 | `full-screen-demo.py` | MenuContainer, MenuItem, RadioList, Checkbox, ProgressBar, Dialog, CompletionsMenu, Float, WordCompleter | Advanced | MenuContainer, MenuItem, RadioList, Checkbox, ProgressBar, Dialog, CompletionsMenu, Float, WordCompleter |
| 9 | `text-editor.py` | TextArea, SearchToolbar, MenuContainer, MenuItem, Dialog, Button, yes_no_dialog | Advanced | TextArea, SearchToolbar, MenuContainer, MenuItem, Dialog, Button, Dialogs.YesNoDialog |
| 10 | `ansi-art-and-textarea.py` | FormattedTextControl (ANSI art), TextArea, VSplit | Intermediate | FormattedTextControl, TextArea, VSplit |

### ScrollablePanes Examples (2)

| # | Python File | Key APIs Used | Complexity | C# Dependencies |
|---|-------------|---------------|------------|-----------------|
| 1 | `simple-example.py` | ScrollablePane, Frame, TextArea, HSplit, focus_next/focus_previous | Intermediate | ScrollablePane, Frame, TextArea, HSplit, FocusFunctions |
| 2 | `with-completion-menu.py` | ScrollablePane, TextArea, WordCompleter, CompletionsMenu, FloatContainer | Intermediate | ScrollablePane, TextArea, WordCompleter, CompletionsMenu, FloatContainer |

### SimpleDemos Examples (13)

| # | Python File | Key APIs Used | Complexity | C# Dependencies |
|---|-------------|---------------|------------|-----------------|
| 1 | `horizontal-split.py` | HSplit, Window, FormattedTextControl | Basic | HSplit, Window, FormattedTextControl |
| 2 | `vertical-split.py` | VSplit, Window, FormattedTextControl | Basic | VSplit, Window, FormattedTextControl |
| 3 | `alignment.py` | Window (align), FormattedTextControl, WindowAlign | Basic | Window, FormattedTextControl, WindowAlign enum |
| 4 | `horizontal-align.py` | VSplit (align), Window, FormattedTextControl, HorizontalAlign | Intermediate | VSplit, Window, FormattedTextControl, HorizontalAlign enum |
| 5 | `vertical-align.py` | HSplit (align), Window, FormattedTextControl, VerticalAlign | Intermediate | HSplit, Window, FormattedTextControl, VerticalAlign enum |
| 6 | `floats.py` | FloatContainer, Float, Window, Frame, FormattedTextControl | Intermediate | FloatContainer, Float, Window, Frame, FormattedTextControl |
| 7 | `float-transparency.py` | FloatContainer, Float (transparent), Window | Intermediate | FloatContainer, Float, Window |
| 8 | `focus.py` | Window, BufferControl, Buffer, focus (by name) | Intermediate | Window, BufferControl, Buffer, Layout.Focus() |
| 9 | `margins.py` | Window, TextArea, NumberedMargin, ScrollbarMargin | Intermediate | Window, TextArea, NumberedMargin, ScrollbarMargin |
| 10 | `line-prefixes.py` | Window, BufferControl, get_line_prefix | Intermediate | Window, BufferControl, GetLinePrefixCallable |
| 11 | `colorcolumn.py` | Window, TextArea, ColorColumn | Intermediate | Window, TextArea, ColorColumn |
| 12 | `cursorcolumn-cursorline.py` | Window (cursorLine, cursorColumn), TextArea | Intermediate | Window, TextArea |
| 13 | `autocompletion.py` | TextArea, WordCompleter, CompletionsMenu, FloatContainer | Intermediate | TextArea, WordCompleter, CompletionsMenu, FloatContainer |

## API Pattern Translation

### Pattern 1: Application Creation

**Python:**
```python
application = Application(
    layout=Layout(container, focused_element=element),
    key_bindings=kb,
    style=style,
    mouse_support=True,
    full_screen=True
)
application.run()
```

**C#:**
```csharp
var application = new Application<object>(
    layout: new Layout(container, focusedElement: element),
    keyBindings: kb,
    style: style,
    mouseSupport: true,
    fullScreen: true
);
await application.RunAsync();
```

### Pattern 2: Key Bindings

**Python:**
```python
kb = KeyBindings()

@kb.add("c-c")
def _(event):
    event.app.exit()

kb.add("tab")(focus_next)
kb.add("s-tab")(focus_previous)
```

**C#:**
```csharp
var kb = new KeyBindings();
kb.Add(Keys.ControlC, (e) => e.App.Exit());
kb.Add(Keys.Tab, FocusFunctions.FocusNext);
kb.Add(Keys.ShiftTab, FocusFunctions.FocusPrevious);
```

### Pattern 3: Layout Containers

**Python:**
```python
root = HSplit([
    Window(FormattedTextControl("Top")),
    Window(height=1, char="-"),
    VSplit([
        Window(BufferControl(buffer=left)),
        Window(width=1, char="|"),
        Window(BufferControl(buffer=right)),
    ]),
])
```

**C#:**
```csharp
var root = new HSplit([
    new Window(new FormattedTextControl("Top")),
    new Window(height: Dimension.Exact(1), @char: '-'),
    new VSplit([
        new Window(new BufferControl(left)),
        new Window(width: Dimension.Exact(1), @char: '|'),
        new Window(new BufferControl(right)),
    ]),
]);
```

### Pattern 4: Widgets with Handlers

**Python:**
```python
button = Button("Click me", handler=on_click)
text_area = TextArea(focusable=True)

def on_click():
    text_area.text = "Clicked!"
```

**C#:**
```csharp
TextArea textArea = null!;
var button = new Button("Click me", handler: () => textArea.Text = "Clicked!");
textArea = new TextArea(focusable: true);
```

### Pattern 5: Buffer Change Events

**Python:**
```python
left_buffer = Buffer()
right_buffer = Buffer()

def on_change(_):
    right_buffer.text = left_buffer.text[::-1]

left_buffer.on_text_changed += on_change
```

**C#:**
```csharp
var leftBuffer = new Buffer();
var rightBuffer = new Buffer();

leftBuffer.OnTextChanged += (_) => {
    var reversed = new string(leftBuffer.Text.Reverse().ToArray());
    rightBuffer.Text = reversed;
};
```

### Pattern 6: Accept Handler (REPL Pattern)

**Python:**
```python
input_field = TextArea(
    height=1,
    prompt=">>> ",
    multiline=False,
)

def accept(buff):
    result = eval(input_field.text)  # Don't do this in production!
    output_field.text += f"\nIn: {input_field.text}\nOut: {result}"

input_field.accept_handler = accept
```

**C#:**
```csharp
var inputField = new TextArea(
    height: Dimension.Exact(1),
    prompt: ">>> ",
    multiline: false
);

inputField.AcceptHandler = (buff) => {
    try {
        // Safe expression evaluation would go here
        var result = EvaluateExpression(inputField.Text);
        outputField.Text += $"\nIn: {inputField.Text}\nOut: {result}";
    } catch (Exception ex) {
        outputField.Text += $"\n{ex.Message}";
    }
};
```

### Pattern 7: Styling

**Python:**
```python
style = Style([
    ("left-pane", "bg:#888800 #000000"),
    ("right-pane", "bg:#00aa00 #000000"),
    ("button focused", "bg:#ff0000"),
])

# Or using from_dict:
style = Style.from_dict({
    "status": "reverse",
    "status.key": "#ffaa00",
})
```

**C#:**
```csharp
var style = new Style([
    ("left-pane", "bg:#888800 #000000"),
    ("right-pane", "bg:#00aa00 #000000"),
    ("button focused", "bg:#ff0000"),
]);

// Or using FromDict:
var style = Style.FromDict(new Dictionary<string, string> {
    ["status"] = "reverse",
    ["status.key"] = "#ffaa00",
});
```

## Identified Dependencies

### Required Stroke APIs (all implemented)

| Namespace | Classes/Types |
|-----------|---------------|
| `Stroke.Application` | `Application<T>` |
| `Stroke.Layout` | `Layout`, `Dimension`, `D` |
| `Stroke.Layout.Containers` | `HSplit`, `VSplit`, `FloatContainer`, `Float`, `Window`, `ScrollablePane`, `ConditionalContainer` |
| `Stroke.Layout.Controls` | `BufferControl`, `FormattedTextControl` |
| `Stroke.Layout.Margins` | `NumberedMargin`, `ScrollbarMargin` |
| `Stroke.Layout.Menus` | `MenuContainer`, `MenuItem`, `CompletionsMenu` |
| `Stroke.Widgets.Base` | `Label`, `Button`, `Frame`, `Box`, `TextArea`, `ProgressBar` |
| `Stroke.Widgets.Lists` | `RadioList<T>`, `CheckboxList<T>`, `Checkbox` |
| `Stroke.Widgets.Dialogs` | `Dialog` |
| `Stroke.Widgets.Toolbars` | `SearchToolbar` |
| `Stroke.KeyBinding` | `KeyBindings`, `Keys` |
| `Stroke.KeyBinding.Bindings` | `FocusFunctions` |
| `Stroke.Core` | `Buffer`, `Document` |
| `Stroke.Styles` | `Style`, `Attrs` |
| `Stroke.Completion` | `WordCompleter` |
| `Stroke.Lexers` | `PygmentsLexer` |
| `Stroke.Shortcuts` | `Dialogs` (for YesNoDialog in TextEditor) |

### Enums Required

- `WindowAlign` (Left, Center, Right)
- `HorizontalAlign` (Left, Center, Right, Justify)
- `VerticalAlign` (Top, Center, Bottom, Justify)

## Implementation Order

Based on dependency analysis and learning progression:

### Phase 1: Basic Examples (P1)
1. `HelloWorld.cs` - Foundation (Box, Frame, TextArea, KeyBindings)
2. `DummyApp.cs` - Minimal Application
3. `NoLayout.cs` - Application without layout

### Phase 2: Layout Demos (P3 SimpleDemos subset)
4. `SimpleDemos/HorizontalSplit.cs` - HSplit basics
5. `SimpleDemos/VerticalSplit.cs` - VSplit basics
6. `SimpleDemos/Alignment.cs` - Window alignment
7. `SimpleDemos/HorizontalAlign.cs` - HSplit alignment
8. `SimpleDemos/VerticalAlign.cs` - VSplit alignment

### Phase 3: Interactive Widgets (P1-P2)
9. `Buttons.cs` - Button handlers, focus navigation
10. `Calculator.cs` - REPL pattern, accept handler

### Phase 4: Buffer and Events (P2)
11. `SplitScreen.cs` - Buffer change events, reactive updates

### Phase 5: Floats and Overlays (P3)
12. `SimpleDemos/Floats.cs` - FloatContainer, Float positioning
13. `SimpleDemos/FloatTransparency.cs` - Float transparency

### Phase 6: Advanced Features (P3)
14. `SimpleDemos/Focus.cs` - Programmatic focus control
15. `SimpleDemos/Margins.cs` - Line numbers, scrollbar
16. `SimpleDemos/LinePrefixes.cs` - Custom line prefixes
17. `SimpleDemos/ColorColumn.cs` - Column highlighting
18. `SimpleDemos/CursorHighlight.cs` - Cursor highlighting
19. `SimpleDemos/AutoCompletion.cs` - Completion menu

### Phase 7: Scrollable Content (P3)
20. `ScrollablePanes/SimpleExample.cs` - ScrollablePane
21. `ScrollablePanes/WithCompletionMenu.cs` - Scrollable + completion

### Phase 8: File Viewing (P2)
22. `Pager.cs` - Read-only viewer, syntax highlighting, search

### Phase 9: Full Applications (P2)
23. `AnsiArtAndTextArea.cs` - ANSI art rendering
24. `FullScreenDemo.cs` - Widget showcase, menus
25. `TextEditor.cs` - Complete editor with menus, dialogs

## Decisions

### Decision 1: Expression Evaluation in Calculator
**Decision**: Use DataTable.Compute() for safe arithmetic expression evaluation
**Rationale**: Avoids security risks of eval() while supporting basic math
**Alternatives Rejected**:
- Roslyn scripting (too heavyweight for example)
- Manual parser (out of scope for example code)

### Decision 2: ANSI Art Storage
**Decision**: Embed ANSI art as string constants in AnsiArtAndTextArea.cs
**Rationale**: Matches Python example which has large inline string
**Alternatives Rejected**:
- External resource file (adds complexity)
- Generated art (different from Python original)

### Decision 3: File Reading in Pager
**Decision**: Read the Pager.cs source file itself (self-referential like Python)
**Rationale**: Matches Python behavior exactly; demonstrates file reading
**Alternatives Rejected**:
- Embedded resource (different behavior)
- External sample file (adds dependency)

### Decision 4: Program.cs Routing
**Decision**: Case-insensitive dictionary mapping with example names matching Python filenames
**Rationale**: Consistent with Dialogs/Choices examples; user-friendly
**Alternatives Rejected**:
- Reflection-based discovery (adds magic)
- Separate executables (breaks project pattern)
