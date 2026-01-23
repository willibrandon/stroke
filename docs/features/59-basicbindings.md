# Feature 59: Basic Key Bindings

## Overview

Implement the basic key bindings that are shared between Emacs and Vi modes, including cursor movement, deletion, self-insert, bracketed paste handling, and common readline commands.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/basic.py`

## Public API

### BasicBindings Class

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class BasicBindings
{
    /// <summary>
    /// Load basic key bindings shared by Emacs and Vi modes.
    /// </summary>
    public static KeyBindingsBase LoadBasicBindings();
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Bindings/
        └── BasicBindings.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        └── BasicBindingsTests.cs
```

## Implementation Notes

### Filter Conditions

```csharp
private static IFilter HasTextBeforeCursor => Condition.Create(() =>
    Application.Current?.CurrentBuffer.Text.Length > 0);

private static IFilter InQuotedInsert => Condition.Create(() =>
    Application.Current?.QuotedInsert == true);

private static IFilter InsertMode => Filters.ViInsertMode | Filters.EmacsInsertMode;
```

### Ignored Keys

Keys that should not trigger any action (prevents them from being inserted):

```csharp
// Register all control keys, function keys, and special keys as no-ops
var ignoredKeys = new[]
{
    "c-a", "c-b", "c-c", "c-d", "c-e", "c-f", "c-g", "c-h", "c-i", "c-j",
    "c-k", "c-l", "c-m", "c-n", "c-o", "c-p", "c-q", "c-r", "c-s", "c-t",
    "c-u", "c-v", "c-w", "c-x", "c-y", "c-z",
    "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8", "f9", "f10",
    "f11", "f12", "f13", "f14", "f15", "f16", "f17", "f18", "f19", "f20",
    "f21", "f22", "f23", "f24",
    "c-@", "c-\\", "c-]", "c-^", "c-_",
    "backspace", "up", "down", "right", "left",
    "s-up", "s-down", "s-right", "s-left",
    "home", "end", "s-home", "s-end",
    "delete", "s-delete", "c-delete",
    "pageup", "pagedown", "s-tab", "tab",
    "c-s-left", "c-s-right", "c-s-home", "c-s-end",
    "c-left", "c-right", "c-up", "c-down", "c-home", "c-end",
    "insert", "s-insert", "c-insert",
    "<sigint>"
};

foreach (var key in ignoredKeys)
{
    bindings.Add(key, e => { }); // No-op handler
}
bindings.Add(Keys.Ignore, e => { });
```

### Readline-Style Bindings

```csharp
// Movement
bindings.Add("home", NamedCommands.GetByName("beginning-of-line"));
bindings.Add("end", NamedCommands.GetByName("end-of-line"));
bindings.Add("left", NamedCommands.GetByName("backward-char"));
bindings.Add("right", NamedCommands.GetByName("forward-char"));
bindings.Add("c-up", NamedCommands.GetByName("previous-history"));
bindings.Add("c-down", NamedCommands.GetByName("next-history"));
bindings.Add("c-l", NamedCommands.GetByName("clear-screen"));

// Editing (insert mode only)
bindings.Add("c-k", NamedCommands.GetByName("kill-line"),
    filter: InsertMode);
bindings.Add("c-u", NamedCommands.GetByName("unix-line-discard"),
    filter: InsertMode);
bindings.Add("backspace", NamedCommands.GetByName("backward-delete-char"),
    filter: InsertMode, saveBefore: IfNoRepeat);
bindings.Add("delete", NamedCommands.GetByName("delete-char"),
    filter: InsertMode, saveBefore: IfNoRepeat);
bindings.Add("c-delete", NamedCommands.GetByName("delete-char"),
    filter: InsertMode, saveBefore: IfNoRepeat);
bindings.Add("c-t", NamedCommands.GetByName("transpose-chars"),
    filter: InsertMode);
bindings.Add("c-w", NamedCommands.GetByName("unix-word-rubout"),
    filter: InsertMode);
```

### Self-Insert Binding

```csharp
// Insert any typed character
bindings.Add(Keys.Any, NamedCommands.GetByName("self-insert"),
    filter: InsertMode, saveBefore: IfNoRepeat);
```

### Tab Completion

```csharp
bindings.Add("c-i", NamedCommands.GetByName("menu-complete"),
    filter: InsertMode);
bindings.Add("s-tab", NamedCommands.GetByName("menu-complete-backward"),
    filter: InsertMode);
```

### History Navigation

```csharp
bindings.Add("pageup", NamedCommands.GetByName("previous-history"),
    filter: ~Filters.HasSelection);
bindings.Add("pagedown", NamedCommands.GetByName("next-history"),
    filter: ~Filters.HasSelection);
```

### Auto Up/Down Movement

```csharp
bindings.Add("up", e =>
{
    e.CurrentBuffer.AutoUp(count: e.Arg);
});

bindings.Add("down", e =>
{
    e.CurrentBuffer.AutoDown(count: e.Arg);
});
```

### Delete in Selection

```csharp
bindings.Add("delete", e =>
{
    var data = e.CurrentBuffer.CutSelection();
    e.App.Clipboard.SetData(data);
}, filter: Filters.HasSelection);
```

### Ctrl-D Delete or Exit

```csharp
bindings.Add("c-d", NamedCommands.GetByName("delete-char"),
    filter: HasTextBeforeCursor & InsertMode);
```

### Enter in Multiline

```csharp
bindings.Add("enter", e =>
{
    e.CurrentBuffer.Newline(copyMargin: !Filters.InPasteMode());
}, filter: InsertMode & Filters.IsMultiline);
```

### Ctrl-J as Enter

```csharp
bindings.Add("c-j", e =>
{
    // Treat \n as \r (enter)
    // Some terminals send \n instead of \r when pressing enter
    e.KeyProcessor.Feed(new KeyPress(Keys.ControlM, "\r"), first: true);
});
```

### Ctrl-Z Insert

```csharp
bindings.Add("c-z", e =>
{
    // Insert literal Ctrl-Z
    // In MSDOS, Ctrl-Z means EOF
    // In Python REPL, Ctrl-Z + Enter quits
    e.CurrentBuffer.InsertText(e.Data);
});
```

### Bracketed Paste

```csharp
bindings.Add(Keys.BracketedPaste, e =>
{
    var data = e.Data;

    // Normalize line endings
    data = data.Replace("\r\n", "\n");
    data = data.Replace("\r", "\n");

    e.CurrentBuffer.InsertText(data);
});
```

### Quoted Insert

```csharp
bindings.Add(Keys.Any, e =>
{
    // Handle quoted insert - insert next character literally
    e.CurrentBuffer.InsertText(e.Data, overwrite: false);
    e.App.QuotedInsert = false;
}, filter: InQuotedInsert, eager: true);
```

### Save Before Helper

```csharp
private static bool IfNoRepeat(KeyPressEvent e) => !e.IsRepeat;
```

## Dependencies

- `Stroke.KeyBinding.KeyBindings` (Feature 19) - KeyBindings class
- `Stroke.KeyBinding.Bindings.NamedCommands` (Feature 56) - Named commands
- `Stroke.Filters` (Feature 12) - Filter conditions
- `Stroke.Core.Buffer` (Feature 06) - Buffer operations

## Implementation Tasks

1. Implement `LoadBasicBindings` method
2. Register all ignored keys
3. Register readline-style movement bindings
4. Register editing bindings with insert mode filter
5. Register self-insert for any key
6. Register tab completion bindings
7. Register history navigation bindings
8. Register up/down auto movement
9. Register delete in selection
10. Register Ctrl-D conditional delete
11. Register enter in multiline
12. Register Ctrl-J as enter
13. Register Ctrl-Z insert
14. Register bracketed paste handling
15. Register quoted insert handling
16. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All control keys are ignored by default
- [ ] All function keys are ignored by default
- [ ] Movement keys work (Home, End, Left, Right)
- [ ] Delete keys work (Backspace, Delete)
- [ ] Self-insert works for printable characters
- [ ] Tab completion bindings work
- [ ] History navigation works (PageUp, PageDown)
- [ ] Up/Down auto-navigate in multiline
- [ ] Delete removes selection when selected
- [ ] Ctrl-D deletes or is ignored based on buffer
- [ ] Enter inserts newline in multiline mode
- [ ] Bracketed paste normalizes line endings
- [ ] Quoted insert inserts literal characters
- [ ] Unit tests achieve 80% coverage
