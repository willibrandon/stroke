# Feature 54: Emacs Key Bindings

## Overview

Implement the default Emacs editing mode key bindings including movement, editing, kill ring, search, selection, macros, and completion bindings.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/emacs.py`

## Public API

### Binding Loaders

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class EmacsBindings
{
    /// <summary>
    /// Load core Emacs key bindings.
    /// </summary>
    public static KeyBindingsBase LoadEmacsBindings();

    /// <summary>
    /// Load Emacs search key bindings.
    /// </summary>
    public static KeyBindingsBase LoadEmacsSearchBindings();

    /// <summary>
    /// Load Emacs shift-selection key bindings.
    /// </summary>
    public static KeyBindingsBase LoadEmacsShiftSelectionBindings();
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Bindings/
        ├── EmacsBindings.cs
        ├── EmacsSearchBindings.cs
        └── EmacsShiftSelectionBindings.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        ├── EmacsBindingsTests.cs
        └── EmacsSearchBindingsTests.cs
```

## Implementation Notes

### Core Emacs Bindings

| Key | Command | Description |
|-----|---------|-------------|
| `Ctrl-A` | beginning-of-line | Move to start of line |
| `Ctrl-E` | end-of-line | Move to end of line |
| `Ctrl-B` | backward-char | Move backward one character |
| `Ctrl-F` | forward-char | Move forward one character |
| `Ctrl-N` | auto-down | Move to next line |
| `Ctrl-P` | auto-up | Move to previous line |
| `Ctrl-Left` | backward-word | Move to previous word |
| `Ctrl-Right` | forward-word | Move to next word |
| `M-b` | backward-word | Move to previous word |
| `M-f` | forward-word | Move to next word |
| `Ctrl-Home` | beginning-of-buffer | Move to start |
| `Ctrl-End` | end-of-buffer | Move to end |

### Kill Ring Bindings

| Key | Command | Description |
|-----|---------|-------------|
| `Ctrl-K` | kill-line | Kill to end of line |
| `Ctrl-U` | backward-kill-line | Kill to start of line |
| `Ctrl-W` | kill-region | Kill selection |
| `M-d` | kill-word | Kill word forward |
| `M-Backspace` | backward-kill-word | Kill word backward |
| `Ctrl-Delete` | kill-word | Kill word forward |
| `Ctrl-Y` | yank | Paste from kill ring |
| `M-y` | yank-pop | Cycle through kill ring |
| `Ctrl-X r y` | yank | Rectangle yank |

### Editing Bindings

| Key | Command | Description |
|-----|---------|-------------|
| `Ctrl-D` | delete-char | Delete character |
| `Backspace` | delete-backward-char | Delete character backward |
| `Ctrl-_` | undo | Undo last change |
| `Ctrl-X Ctrl-U` | undo | Undo last change |
| `M-c` | capitalize-word | Capitalize word |
| `M-l` | downcase-word | Lowercase word |
| `M-u` | uppercase-word | Uppercase word |
| `M-\` | delete-horizontal-space | Delete whitespace |

### History Bindings

| Key | Command | Description |
|-----|---------|-------------|
| `M-<` | beginning-of-history | First history entry |
| `M->` | end-of-history | Last history entry |
| `M-.` | yank-last-arg | Insert last arg from history |
| `M-_` | yank-last-arg | Insert last arg from history |
| `Ctrl-O` | operate-and-get-next | Accept and fetch next |

### Completion Bindings

| Key | Command | Description |
|-----|---------|-------------|
| `M-/` | complete | Start/next completion |
| `M-*` | insert-all-completions | Insert all completions |

### Selection Bindings

| Key | Command | Description |
|-----|---------|-------------|
| `Ctrl-@` | start-selection | Start selection |
| `Ctrl-Space` | start-selection | Start selection |
| `Ctrl-G` | cancel | Cancel selection/menu |
| `M-w` | copy-region | Copy selection |

### Macro Bindings

| Key | Command | Description |
|-----|---------|-------------|
| `Ctrl-X (` | start-kbd-macro | Start recording |
| `Ctrl-X )` | end-kbd-macro | Stop recording |
| `Ctrl-X e` | call-last-kbd-macro | Execute macro |

### Accept Input Bindings

| Key | Condition | Description |
|-----|-----------|-------------|
| `Enter` | single-line | Accept input |
| `M-Enter` | always | Accept input |

### Character Search

| Key | Description |
|-----|-------------|
| `Ctrl-]` + char | Search forward for character |
| `M-Ctrl-]` + char | Search backward for character |

### Numeric Argument

| Key | Description |
|-----|-------------|
| `M-0` through `M-9` | Build numeric argument |
| `M--` | Negative argument |

### Emacs Search Bindings

| Key | Condition | Description |
|-----|-----------|-------------|
| `Ctrl-R` | not searching | Start reverse search |
| `Ctrl-S` | not searching | Start forward search |
| `Ctrl-R` | searching | Next reverse match |
| `Ctrl-S` | searching | Next forward match |
| `Ctrl-C` | searching | Abort search |
| `Ctrl-G` | searching | Abort search |
| `Enter` | searching | Accept search |
| `Escape` | searching | Accept search |
| `Up` | searching | Previous match |
| `Down` | searching | Next match |

### Emacs Read-Only Bindings

When buffer is read-only (like a pager):

| Key | Description |
|-----|-------------|
| `/` | Start forward search |
| `?` | Start reverse search |
| `n` | Next match |
| `N` | Previous match |

### Shift Selection Bindings

| Key | Effect |
|-----|--------|
| `Shift-Left/Right/Up/Down` | Start/extend selection |
| `Shift-Home/End` | Select to line start/end |
| `Ctrl-Shift-Left/Right` | Select by word |
| `Ctrl-Shift-Home/End` | Select to buffer start/end |
| Arrow without Shift | Cancel selection |
| Any character | Replace selection |
| `Backspace` | Delete selection |
| `Ctrl-Y` | Paste, replacing selection |

### Filter Conditions

The bindings use these filter conditions:

- `emacs_mode`: Only when editing mode is Emacs
- `emacs_insert_mode`: Emacs mode and not in special state
- `has_selection`: When text is selected
- `shift_selection_mode`: When selection was started with Shift
- `is_multiline`: When multiline input enabled
- `is_returnable`: When buffer can accept input
- `is_read_only`: When buffer is read-only
- `has_arg`: When numeric argument is active

### Binding Implementation Pattern

```csharp
public static KeyBindingsBase LoadEmacsBindings()
{
    var bindings = new KeyBindings();

    // Movement
    bindings.Add("c-a", GetByName("beginning-of-line"));
    bindings.Add("c-e", GetByName("end-of-line"));
    bindings.Add("c-b", GetByName("backward-char"));
    bindings.Add("c-f", GetByName("forward-char"));

    // Editing
    bindings.Add("c-d", GetByName("delete-char"),
        filter: Filters.EmacsInsertMode);

    // Kill ring
    bindings.Add("c-k", GetByName("kill-line"),
        filter: Filters.EmacsInsertMode);
    bindings.Add("c-y", GetByName("yank"),
        filter: Filters.EmacsInsertMode);
    bindings.Add("escape", "y", GetByName("yank-pop"),
        filter: Filters.EmacsInsertMode);

    // Wrap in conditional for emacs mode
    return new ConditionalKeyBindings(bindings, Filters.EmacsMode);
}
```

## Dependencies

- `Stroke.KeyBinding.KeyBindings` (Feature 19) - KeyBindings class
- `Stroke.KeyBinding.Bindings.NamedCommands` (Feature 56) - Command registry
- `Stroke.Filters` (Feature 12) - Core filter infrastructure
- `Stroke.Filters` (Feature 121) - EmacsMode, EmacsInsertMode, EmacsSelectionMode
- `Stroke.Search` (Feature 53) - Search functions

## Implementation Tasks

1. Implement `LoadEmacsBindings` with all core bindings
2. Implement `LoadEmacsSearchBindings` with search bindings
3. Implement `LoadEmacsShiftSelectionBindings` with selection bindings
4. Register all bindings with appropriate filters
5. Handle numeric argument input
6. Implement character search handlers
7. Implement indent/unindent handlers
8. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All movement bindings work correctly
- [ ] Kill ring operations work (kill, yank, yank-pop)
- [ ] Word case changes work (capitalize, upcase, downcase)
- [ ] Selection bindings work
- [ ] Search bindings work
- [ ] Shift-selection bindings work
- [ ] Macro recording and playback work
- [ ] Numeric arguments affect repeat count
- [ ] Bindings only active in Emacs mode
- [ ] Unit tests achieve 80% coverage
