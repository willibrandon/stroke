# Feature 128: Full-Screen Examples (Complete Set)

## Overview

Implement ALL 25 Python Prompt Toolkit full-screen examples in the `Stroke.Examples.FullScreen` project. These examples demonstrate the Application class, layout containers (HSplit, VSplit, FloatContainer), widgets (TextArea, Button, Frame), and various full-screen application patterns.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/`

### Main Examples

| # | Python File | C# File | Description |
|---|-------------|---------|-------------|
| 1 | `hello-world.py` | `HelloWorld.cs` | Simple TextArea displaying "Hello World!" |
| 2 | `no-layout.py` | `NoLayout.cs` | Empty full screen application without layout |
| 3 | `dummy-app.py` | `DummyApp.cs` | Simplest possible Application (not full screen) |
| 4 | `buttons.py` | `Buttons.cs` | Button widgets with click handlers |
| 5 | `calculator.py` | `Calculator.cs` | Calculator REPL with expression evaluation |
| 6 | `full-screen-demo.py` | `FullScreenDemo.cs` | Comprehensive widget showcase |
| 7 | `pager.py` | `Pager.cs` | File pager with syntax highlighting and search |
| 8 | `split-screen.py` | `SplitScreen.cs` | Vertical split with live text reversal |
| 9 | `text-editor.py` | `TextEditor.cs` | Notepad-like text editor with menus |
| 10 | `ansi-art-and-textarea.py` | `AnsiArtAndTextArea.cs` | ANSI art logo with editable TextArea |

### Scrollable Panes (`scrollable-panes/`)

| # | Python File | C# File | Description |
|---|-------------|---------|-------------|
| 11 | `simple-example.py` | `ScrollablePanes/SimpleExample.cs` | Basic ScrollablePane with multiple TextAreas |
| 12 | `with-completion-menu.py` | `ScrollablePanes/WithCompletionMenu.cs` | ScrollablePane with autocompletion |

### Simple Demos (`simple-demos/`)

| # | Python File | C# File | Description |
|---|-------------|---------|-------------|
| 13 | `alignment.py` | `SimpleDemos/Alignment.cs` | WindowAlign (LEFT/CENTER/RIGHT) |
| 14 | `autocompletion.py` | `SimpleDemos/Autocompletion.cs` | BufferControl with CompletionsMenu |
| 15 | `colorcolumn.py` | `SimpleDemos/ColorColumn.cs` | Colored column markers at positions |
| 16 | `cursorcolumn-cursorline.py` | `SimpleDemos/CursorColumnLine.cs` | Cursor column/line highlighting |
| 17 | `float-transparency.py` | `SimpleDemos/FloatTransparency.cs` | Float transparency attribute demo |
| 18 | `floats.py` | `SimpleDemos/Floats.cs` | Float positioning (left/right/top/bottom/center) |
| 19 | `focus.py` | `SimpleDemos/Focus.cs` | Programmatic focus control with hotkeys |
| 20 | `horizontal-align.py` | `SimpleDemos/HorizontalAlign.cs` | HorizontalAlign in VSplit |
| 21 | `horizontal-split.py` | `SimpleDemos/HorizontalSplit.cs` | Basic HSplit example |
| 22 | `line-prefixes.py` | `SimpleDemos/LinePrefixes.cs` | Custom line prefixes with get_line_prefix |
| 23 | `margins.py` | `SimpleDemos/Margins.cs` | NumberedMargin and ScrollbarMargin |
| 24 | `vertical-align.py` | `SimpleDemos/VerticalAlign.cs` | VerticalAlign in HSplit |
| 25 | `vertical-split.py` | `SimpleDemos/VerticalSplit.cs` | Basic VSplit example |

## Representative Python Examples

### hello-world.py

```python
from prompt_toolkit.application import Application
from prompt_toolkit.key_binding import KeyBindings
from prompt_toolkit.layout import Layout
from prompt_toolkit.widgets import Box, Frame, TextArea

root_container = Box(Frame(TextArea(text="Hello world!\nPress control-c to quit.", width=40, height=10)))
layout = Layout(container=root_container)

kb = KeyBindings()

@kb.add("c-c")
def _(event):
    event.app.exit()

application = Application(layout=layout, key_bindings=kb, full_screen=True)

def main():
    application.run()
```

### buttons.py (key pattern)

```python
# Event handlers update shared TextArea
def button1_clicked():
    text_area.text = "Button 1 clicked"

button1 = Button("Button 1", handler=button1_clicked)

# Layout with Box/HSplit/VSplit
root_container = Box(HSplit([
    Label(text="Press `Tab` to move the focus."),
    VSplit([
        Box(body=HSplit([button1, button2, button3, button4], padding=1), style="class:left-pane"),
        Box(body=Frame(text_area), style="class:right-pane"),
    ])
]))

# Tab navigation
kb.add("tab")(focus_next)
kb.add("s-tab")(focus_previous)
```

### calculator.py (REPL pattern)

```python
def accept(buff):
    try:
        output = f"\n\nIn:  {input_field.text}\nOut: {eval(input_field.text)}"
    except BaseException as e:
        output = f"\n\n{e}"
    output_field.buffer.document = Document(text=output_field.text + output, cursor_position=len(new_text))

input_field.accept_handler = accept
```

### split-screen.py (reactive updates)

```python
def default_buffer_changed(_):
    right_buffer.text = left_buffer.text[::-1]

left_buffer.on_text_changed += default_buffer_changed
```

## Public API (C# Examples)

### HelloWorld.cs

```csharp
using Stroke.Application;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Widgets;

namespace Stroke.Examples.FullScreen;

public static class HelloWorld
{
    public static void Run()
    {
        var rootContainer = new Box(new Frame(new TextArea(
            text: "Hello world!\nPress control-c to quit.", width: 40, height: 10)));

        var kb = new KeyBindings();
        kb.Add("c-c", (KeyPressEventArgs e) => e.App.Exit());

        var application = new Application<object?>(
            layout: new Layout(container: rootContainer),
            keyBindings: kb,
            fullScreen: true);

        application.Run();
    }
}
```

### Buttons.cs

```csharp
using Stroke.Application;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Stroke.Layout;
using Stroke.Styles;
using Stroke.Widgets;

namespace Stroke.Examples.FullScreen;

public static class Buttons
{
    public static void Run()
    {
        var textArea = new TextArea(focusable: true);
        var button1 = new Button("Button 1", handler: () => textArea.Text = "Button 1 clicked");
        var button2 = new Button("Button 2", handler: () => textArea.Text = "Button 2 clicked");
        var button3 = new Button("Button 3", handler: () => textArea.Text = "Button 3 clicked");
        var button4 = new Button("Exit", handler: () => AppContext.GetApp<object?>().Exit());

        var rootContainer = new Box(new HSplit([
            new Label(text: "Press `Tab` to move the focus."),
            new VSplit([
                new Box(body: new HSplit([button1, button2, button3, button4], padding: 1), padding: 1, style: "class:left-pane"),
                new Box(body: new Frame(textArea), padding: 1, style: "class:right-pane"),
            ]),
        ]));

        var kb = new KeyBindings();
        kb.Add("tab", FocusFunctions.FocusNext);
        kb.Add("s-tab", FocusFunctions.FocusPrevious);

        var style = new Style([
            ("left-pane", "bg:#888800 #000000"),
            ("right-pane", "bg:#00aa00 #000000"),
            ("button focused", "bg:#ff0000"),
        ]);

        new Application<object?>(layout: new Layout(container: rootContainer, focusedElement: button1),
            keyBindings: kb, style: style, fullScreen: true).Run();
    }
}
```

### SplitScreen.cs (reactive updates)

```csharp
using Stroke.Application;
using Stroke.Core;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Controls;

namespace Stroke.Examples.FullScreen;

public static class SplitScreen
{
    public static void Run()
    {
        var leftBuffer = new Buffer();
        var rightBuffer = new Buffer();

        var leftWindow = new Window(new BufferControl(buffer: leftBuffer));
        var rightWindow = new Window(new BufferControl(buffer: rightBuffer));

        var body = new VSplit([leftWindow, new Window(width: 1, @char: '|'), rightWindow]);

        var rootContainer = new HSplit([
            new Window(height: 1, content: new FormattedTextControl(() => [("class:title", " Hello world (Press [Ctrl-Q] to quit.)")]), align: WindowAlign.Center),
            new Window(height: 1, @char: '-'),
            body,
        ]);

        var kb = new KeyBindings();
        kb.Add("c-c", eager: true, handler: (KeyPressEventArgs e) => e.App.Exit());
        kb.Add("c-q", eager: true, handler: (KeyPressEventArgs e) => e.App.Exit());

        leftBuffer.OnTextChanged += _ => rightBuffer.Text = new string(leftBuffer.Text.Reverse().ToArray());

        new Application<object?>(layout: new Layout(rootContainer, focusedElement: leftWindow),
            keyBindings: kb, mouseSupport: true, fullScreen: true).Run();
    }
}
```

## Project Structure

```
examples/Stroke.Examples.FullScreen/
├── Stroke.Examples.FullScreen.csproj
├── Program.cs                        # Entry point with dictionary-based routing
├── HelloWorld.cs                     # Examples 1-10 (main)
├── NoLayout.cs
├── DummyApp.cs
├── Buttons.cs
├── Calculator.cs
├── FullScreenDemo.cs
├── Pager.cs
├── SplitScreen.cs
├── TextEditor.cs
├── AnsiArtAndTextArea.cs
├── ScrollablePanes/                  # Examples 11-12
│   ├── SimpleExample.cs
│   └── WithCompletionMenu.cs
└── SimpleDemos/                      # Examples 13-25
    ├── Alignment.cs
    ├── Autocompletion.cs
    ├── ColorColumn.cs
    ├── CursorColumnLine.cs
    ├── FloatTransparency.cs
    ├── Floats.cs
    ├── Focus.cs
    ├── HorizontalAlign.cs
    ├── HorizontalSplit.cs
    ├── LinePrefixes.cs
    ├── Margins.cs
    ├── VerticalAlign.cs
    └── VerticalSplit.cs
```

## Program.cs

```csharp
namespace Stroke.Examples.FullScreen;

public static class Program
{
    private static readonly Dictionary<string, Action> Examples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["HelloWorld"] = HelloWorld.Run,
        ["NoLayout"] = NoLayout.Run,
        ["DummyApp"] = DummyApp.Run,
        ["Buttons"] = Buttons.Run,
        ["Calculator"] = Calculator.Run,
        ["FullScreenDemo"] = FullScreenDemo.Run,
        ["Pager"] = Pager.Run,
        ["SplitScreen"] = SplitScreen.Run,
        ["TextEditor"] = TextEditor.Run,
        ["AnsiArtAndTextArea"] = AnsiArtAndTextArea.Run,
        ["ScrollablePanes/SimpleExample"] = ScrollablePanes.SimpleExample.Run,
        ["ScrollablePanes/WithCompletionMenu"] = ScrollablePanes.WithCompletionMenu.Run,
        ["SimpleDemos/Alignment"] = SimpleDemos.Alignment.Run,
        // ... remaining 12 SimpleDemos entries
    };

    public static void Main(string[] args)
    {
        var exampleName = args.Length > 0 ? args[0] : "HelloWorld";
        if (Examples.TryGetValue(exampleName, out var runExample))
        {
            try { runExample(); }
            catch (KeyboardInterrupt) { }
            catch (EOFException) { }
        }
        else
        {
            Console.WriteLine($"Unknown example: {exampleName}");
            Console.WriteLine($"Available: {string.Join(", ", Examples.Keys)}");
            Environment.Exit(1);
        }
    }
}
```

## Key Concepts Demonstrated

| Category | Examples | Stroke API |
|----------|----------|------------|
| Basic App | HelloWorld, NoLayout, DummyApp | `Application`, `Layout`, `fullScreen` |
| Widgets | Buttons, FullScreenDemo | `Button`, `TextArea`, `RadioList`, `Checkbox`, `ProgressBar` |
| REPL Pattern | Calculator | `TextArea.AcceptHandler`, `Document` |
| Reactive | SplitScreen | `Buffer.OnTextChanged` event |
| Menus | FullScreenDemo, TextEditor | `MenuContainer`, `MenuItem` |
| File Viewer | Pager | `TextArea(readOnly: true)`, `PygmentsLexer`, `SearchToolbar` |
| Scrolling | ScrollablePanes/* | `ScrollablePane`, `FloatContainer` |
| Alignment | Alignment, HorizontalAlign, VerticalAlign | `WindowAlign`, `HorizontalAlign`, `VerticalAlign` |
| Floats | Floats, FloatTransparency | `Float(left:/right:/top:/bottom:/transparent:)` |
| Focus | Focus | `Layout.Focus()`, `focus_next`, `focus_previous` |
| Margins | Margins, LinePrefixes | `NumberedMargin`, `ScrollbarMargin`, `get_line_prefix` |
| Cursor | ColorColumn, CursorColumnLine | `ColorColumn`, `cursorcolumn`, `cursorline` |

## Dependencies

All dependencies already implemented:
- Feature 30: Application System
- Feature 29: Layout Containers (HSplit, VSplit, FloatContainer)
- Feature 45: Base Widgets (TextArea, Button, Frame, Dialog)
- Feature 44: Toolbar Widgets (SearchToolbar)
- Feature 25: Lexer System (PygmentsLexer)
- Feature 12: Completion System (WordCompleter, CompletionsMenu)

## Acceptance Criteria

### General
- [ ] All 25 examples build and run without errors
- [ ] Ctrl+C exits gracefully in all examples
- [ ] Project included in `Stroke.Examples.sln`

### Key Behaviors
- [ ] HelloWorld: Framed text area displays, Ctrl+C exits
- [ ] Buttons: Tab navigates, clicking updates text area
- [ ] Calculator: Expressions evaluated, results appended
- [ ] SplitScreen: Left typing shows reversed text on right
- [ ] Pager: Line numbers, syntax highlighting, search with /
- [ ] FullScreenDemo: Menus open, all widgets functional
- [ ] TextEditor: File open/edit/find operations work
- [ ] ScrollablePanes: Scroll through 20 TextAreas
- [ ] SimpleDemos/Focus: a/b/c/d keys focus specific windows
- [ ] SimpleDemos/Floats: Five floats at corners and center

## Verification with TUI Driver

```javascript
// HelloWorld
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.FullScreen"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: session.id, text: "Hello world!" });
await tui_press_key({ session_id: session.id, key: "Ctrl+c" });
await tui_close({ session_id: session.id });

// Calculator
const calc = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.FullScreen", "--", "Calculator"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: calc.id, text: ">>>" });
await tui_send_text({ session_id: calc.id, text: "4 + 4" });
await tui_press_key({ session_id: calc.id, key: "Enter" });
await tui_wait_for_text({ session_id: calc.id, text: "Out: 8" });
await tui_close({ session_id: calc.id });
```
