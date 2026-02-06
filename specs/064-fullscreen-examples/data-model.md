# Data Model: Full-Screen Examples

**Feature**: 064-fullscreen-examples
**Date**: 2026-02-05

## Overview

This document defines the example catalog structure and relationships between examples for the Stroke.Examples.FullScreen project.

## Example Catalog

### Entity: Example

Each example is a self-contained C# class demonstrating one or more Stroke full-screen capabilities.

| Attribute | Type | Description |
|-----------|------|-------------|
| Name | string | Example identifier (e.g., "HelloWorld") |
| Category | ExampleCategory | Main, ScrollablePanes, or SimpleDemos |
| Priority | P1/P2/P3 | Implementation priority from spec |
| PythonFile | string | Original Python filename |
| CSharpFile | string | Target C# filename |
| Dependencies | List<Example> | Examples that should be implemented first |
| Concepts | List<string> | Stroke APIs/patterns demonstrated |
| LOC | int | Estimated lines of code |

### Entity: ExampleCategory

| Value | Description | Directory |
|-------|-------------|-----------|
| Main | Core full-screen examples | Root of project |
| ScrollablePanes | Scrollable container demos | ScrollablePanes/ |
| SimpleDemos | Focused API demonstrations | SimpleDemos/ |

## Complete Example Inventory

### Main Examples (10)

| Name | PythonFile | Priority | Concepts | Est. LOC |
|------|------------|----------|----------|----------|
| HelloWorld | hello-world.py | P1 | Application, Layout, Box, Frame, TextArea, KeyBindings, Exit | 45 |
| DummyApp | dummy-app.py | P1 | Application (minimal) | 20 |
| NoLayout | no-layout.py | P1 | Application (null layout) | 20 |
| Buttons | buttons.py | P1 | Button, Label, HSplit, VSplit, Box, Frame, Style, FocusNext/Previous, Handler | 90 |
| Calculator | calculator.py | P1 | TextArea.AcceptHandler, HSplit, Window, SearchToolbar, Document, Style, REPL pattern | 100 |
| SplitScreen | split-screen.py | P2 | Buffer, BufferControl, Window, VSplit, HSplit, FormattedTextControl, OnTextChanged, mouse_support | 160 |
| Pager | pager.py | P2 | TextArea (read_only, scrollbar, line_numbers), SearchToolbar, PygmentsLexer, FormattedTextControl, enable_page_navigation_bindings | 110 |
| FullScreenDemo | full-screen-demo.py | P2 | MenuContainer, MenuItem, RadioList, Checkbox, ProgressBar, Dialog, CompletionsMenu, Float, WordCompleter, mouse_support | 230 |
| TextEditor | text-editor.py | P2 | TextArea, SearchToolbar, MenuContainer, MenuItem, Dialog, Button, Dialogs.YesNoDialog, file operations | 350 |
| AnsiArtAndTextArea | ansi-art-and-textarea.py | P2 | FormattedTextControl (ANSI), TextArea, VSplit, large string constant | 400* |

*Note: LOC includes embedded ANSI art string (~23KB in Python original)

### ScrollablePanes Examples (2)

| Name | PythonFile | Priority | Concepts | Est. LOC |
|------|------------|----------|----------|----------|
| SimpleExample | simple-example.py | P3 | ScrollablePane, Frame, TextArea, HSplit, FocusNext/Previous | 50 |
| WithCompletionMenu | with-completion-menu.py | P3 | ScrollablePane, TextArea, WordCompleter, CompletionsMenu, FloatContainer | 100 |

### SimpleDemos Examples (13)

| Name | PythonFile | Priority | Concepts | Est. LOC |
|------|------------|----------|----------|----------|
| HorizontalSplit | horizontal-split.py | P3 | HSplit, Window, FormattedTextControl | 50 |
| VerticalSplit | vertical-split.py | P3 | VSplit, Window, FormattedTextControl | 50 |
| Alignment | alignment.py | P3 | Window.Align (Left, Center, Right), FormattedTextControl | 70 |
| HorizontalAlign | horizontal-align.py | P3 | VSplit.Align (HorizontalAlign enum), Window | 200 |
| VerticalAlign | vertical-align.py | P3 | HSplit.Align (VerticalAlign enum), Window | 150 |
| Floats | floats.py | P3 | FloatContainer, Float (left, right, top, bottom, center), Frame | 120 |
| FloatTransparency | float-transparency.py | P3 | FloatContainer, Float.Transparent, Window | 100 |
| Focus | focus.py | P3 | Window, BufferControl, Buffer, Layout.Focus(name), custom key bindings | 100 |
| Margins | margins.py | P3 | Window, TextArea, NumberedMargin, ScrollbarMargin | 80 |
| LinePrefixes | line-prefixes.py | P3 | Window, BufferControl, GetLinePrefixCallable | 110 |
| ColorColumn | colorcolumn.py | P3 | Window.ColorColumns, TextArea, ColorColumn class | 70 |
| CursorHighlight | cursorcolumn-cursorline.py | P3 | Window.CursorLine, Window.CursorColumn, TextArea | 70 |
| AutoCompletion | autocompletion.py | P3 | TextArea, WordCompleter, CompletionsMenu, FloatContainer | 80 |

## Dependency Graph

```
                                    HelloWorld (P1)
                                         │
                    ┌────────────────────┼────────────────────┐
                    │                    │                    │
              DummyApp (P1)        NoLayout (P1)        Buttons (P1)
                                                             │
                                                    ┌────────┴────────┐
                                                    │                 │
                                            Calculator (P1)    SplitScreen (P2)
                                                    │                 │
                                                    └────────┬────────┘
                                                             │
                                    ┌────────────────────────┼────────────────────────┐
                                    │                        │                        │
                              Pager (P2)            FullScreenDemo (P2)        SimpleDemos/*
                                    │                        │
                                    └───────────┬────────────┘
                                                │
                                         TextEditor (P2)
```

**SimpleDemos dependency order:**
```
HorizontalSplit ─┬─> Alignment ─┬─> Floats ─> FloatTransparency
VerticalSplit ───┘              │
                                ├─> HorizontalAlign
                                ├─> VerticalAlign
                                ├─> Focus
                                ├─> Margins ─> LinePrefixes
                                ├─> ColorColumn ─> CursorHighlight
                                └─> AutoCompletion
```

**ScrollablePanes dependency order:**
```
SimpleExample ─> WithCompletionMenu
```

## API Usage Matrix

| Example | App | Layout | HSplit | VSplit | Float | Window | Buffer | TextArea | Button | Frame | Box | Label | Menu | Radio | Check | Dialog | Style |
|---------|-----|--------|--------|--------|-------|--------|--------|----------|--------|-------|-----|-------|------|-------|-------|--------|-------|
| HelloWorld | ✓ | ✓ | | | | | | ✓ | | ✓ | ✓ | | | | | | |
| DummyApp | ✓ | | | | | | | | | | | | | | | | |
| NoLayout | ✓ | | | | | | | | | | | | | | | | |
| Buttons | ✓ | ✓ | ✓ | ✓ | | | | ✓ | ✓ | ✓ | ✓ | ✓ | | | | | ✓ |
| Calculator | ✓ | ✓ | ✓ | | | ✓ | | ✓ | | | | | | | | | ✓ |
| SplitScreen | ✓ | ✓ | ✓ | ✓ | | ✓ | ✓ | | | | | | | | | | |
| Pager | ✓ | ✓ | ✓ | | | ✓ | | ✓ | | | | | | | | | ✓ |
| FullScreenDemo | ✓ | ✓ | ✓ | ✓ | ✓ | | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| TextEditor | ✓ | ✓ | ✓ | ✓ | | ✓ | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | | | ✓ | ✓ |
| AnsiArt | ✓ | ✓ | | ✓ | | ✓ | | ✓ | | | | | | | | | |
| ScrollablePanes/* | ✓ | ✓ | ✓ | | ✓* | | | ✓ | | ✓ | | | | | | | |
| SimpleDemos/* | ✓ | ✓ | ✓ | ✓ | ✓* | ✓ | ✓* | ✓* | | ✓* | | | | | | | |

*Only some examples in category

## Validation Rules

### Example Naming
- C# class name matches example name (PascalCase)
- Class is `internal static` with public `Run()` method
- Namespace is `Stroke.Examples.FullScreenExamples` (or `.ScrollablePanes`, `.SimpleDemos`)

### Example Structure
```csharp
namespace Stroke.Examples.FullScreenExamples;

/// <summary>
/// [Description from Python docstring]
/// Port of Python Prompt Toolkit's [filename].py example.
/// </summary>
internal static class ExampleName
{
    public static void Run()
    {
        // Implementation
    }
}
```

### Error Handling
- All examples catch `KeyboardInterrupt` and `EOFException` for graceful exit
- No stack traces on Ctrl+C
- Calculator handles evaluation errors gracefully

## State Transitions

### Application Lifecycle
```
Created -> Running -> Done
              │
              └──> Exit(result) -> Done
```

### Buffer Text Change Event
```
User Input -> Buffer.Text modified -> OnTextChanged fired -> Handler updates other buffers
```

### Accept Handler Flow (Calculator)
```
User types -> Enter pressed -> AcceptHandler invoked -> Output updated -> Input cleared
```
