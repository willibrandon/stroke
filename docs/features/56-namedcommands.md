# Feature 56: Named Commands

## Overview

Implement the Readline-compatible named command system that provides a registry of editing commands accessible by name, enabling key bindings to reference commands symbolically.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/named_commands.py`

## Public API

### Named Command Registry

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class NamedCommands
{
    /// <summary>
    /// Get a binding by its Readline command name.
    /// </summary>
    /// <param name="name">The command name (e.g., "forward-char").</param>
    /// <returns>The binding for that command.</returns>
    /// <exception cref="KeyNotFoundException">Command not found.</exception>
    public static Binding GetByName(string name);

    /// <summary>
    /// Register a command with a name.
    /// </summary>
    /// <param name="name">The command name.</param>
    /// <param name="handler">The command handler.</param>
    public static void Register(string name, Action<KeyPressEvent> handler);
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Bindings/
        └── NamedCommands.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        └── NamedCommandsTests.cs
```

## Implementation Notes

### Registered Commands

#### Movement Commands

| Name | Description |
|------|-------------|
| `beginning-of-buffer` | Move to start of buffer |
| `end-of-buffer` | Move to end of buffer |
| `beginning-of-line` | Move to start of line |
| `end-of-line` | Move to end of line |
| `forward-char` | Move forward one character |
| `backward-char` | Move backward one character |
| `forward-word` | Move forward one word |
| `backward-word` | Move backward one word |
| `clear-screen` | Clear and redraw screen |
| `redraw-current-line` | Redraw current line |

#### History Commands

| Name | Description |
|------|-------------|
| `accept-line` | Accept input and execute |
| `previous-history` | Previous history entry |
| `next-history` | Next history entry |
| `beginning-of-history` | First history entry |
| `end-of-history` | Last history entry |
| `reverse-search-history` | Search history backward |
| `forward-search-history` | Search history forward |
| `yank-nth-arg` | Insert nth arg from previous command |
| `yank-last-arg` | Insert last arg from previous command |
| `operate-and-get-next` | Accept and fetch next history |

#### Text Modification Commands

| Name | Description |
|------|-------------|
| `delete-char` | Delete character at cursor |
| `backward-delete-char` | Delete character before cursor |
| `self-insert` | Insert typed character |
| `transpose-chars` | Swap last two characters |
| `transpose-words` | Swap last two words |
| `upcase-word` | Uppercase word |
| `downcase-word` | Lowercase word |
| `capitalize-word` | Capitalize word |
| `overwrite-mode` | Toggle overwrite mode |

#### Kill and Yank Commands

| Name | Description |
|------|-------------|
| `kill-line` | Kill to end of line |
| `backward-kill-line` | Kill to start of line |
| `kill-whole-line` | Kill entire line |
| `kill-word` | Kill word forward |
| `backward-kill-word` | Kill word backward |
| `unix-word-rubout` | Kill word backward (unix) |
| `unix-line-discard` | Kill line backward |
| `yank` | Paste from kill ring |
| `yank-pop` | Cycle through kill ring |

#### Completing Commands

| Name | Description |
|------|-------------|
| `complete` | Complete current word |
| `menu-complete` | Complete with menu |
| `menu-complete-backward` | Complete with menu backward |

#### Macro Commands

| Name | Description |
|------|-------------|
| `start-kbd-macro` | Start recording macro |
| `end-kbd-macro` | Stop recording macro |
| `call-last-kbd-macro` | Execute last macro |

#### Miscellaneous Commands

| Name | Description |
|------|-------------|
| `undo` | Undo last change |
| `insert-comment` | Insert comment character |
| `quoted-insert` | Insert next char literally |
| `delete-horizontal-space` | Delete whitespace around cursor |

### Implementation Pattern

```csharp
private static readonly Dictionary<string, Binding> _commands = new();

static NamedCommands()
{
    // Register all built-in commands
    Register("beginning-of-buffer", e =>
    {
        e.CurrentBuffer.CursorPosition = 0;
    });

    Register("end-of-buffer", e =>
    {
        e.CurrentBuffer.CursorPosition = e.CurrentBuffer.Text.Length;
    });

    Register("beginning-of-line", e =>
    {
        var buff = e.CurrentBuffer;
        buff.CursorPosition += buff.Document.GetStartOfLinePosition(afterWhitespace: false);
    });

    Register("end-of-line", e =>
    {
        var buff = e.CurrentBuffer;
        buff.CursorPosition += buff.Document.GetEndOfLinePosition();
    });

    Register("forward-char", e =>
    {
        var buff = e.CurrentBuffer;
        buff.CursorPosition += buff.Document.GetCursorRightPosition(count: e.Arg);
    });

    Register("backward-char", e =>
    {
        var buff = e.CurrentBuffer;
        buff.CursorPosition += buff.Document.GetCursorLeftPosition(count: e.Arg);
    });

    // ... all other commands
}

public static Binding GetByName(string name)
{
    if (_commands.TryGetValue(name, out var binding))
        return binding;
    throw new KeyNotFoundException($"Unknown Readline command: {name}");
}

public static void Register(string name, Action<KeyPressEvent> handler)
{
    _commands[name] = new Binding(handler);
}
```

### Command Handler Signature

All commands receive a `KeyPressEvent` and can access:
- `e.CurrentBuffer` - The current buffer
- `e.App` - The application
- `e.Arg` - Numeric argument (repeat count)
- `e.Data` - Raw key data

### Repeat Count Support

Commands should respect `e.Arg` for repeat count:

```csharp
Register("forward-char", e =>
{
    var buff = e.CurrentBuffer;
    // Move by count characters
    buff.CursorPosition += buff.Document.GetCursorRightPosition(count: e.Arg);
});

Register("forward-word", e =>
{
    var buff = e.CurrentBuffer;
    // Find next word ending, repeated count times
    var pos = buff.Document.FindNextWordEnding(count: e.Arg);
    if (pos.HasValue)
        buff.CursorPosition += pos.Value;
});
```

### Kill Ring Integration

Kill commands add to the kill ring:

```csharp
Register("kill-line", e =>
{
    var buff = e.CurrentBuffer;
    var data = buff.DeleteToEndOfLine();
    e.App.Clipboard.SetData(data);
});

Register("yank", e =>
{
    var data = e.App.Clipboard.GetData();
    e.CurrentBuffer.Paste(data, before: false);
});

Register("yank-pop", e =>
{
    e.App.Clipboard.Rotate();
    var data = e.App.Clipboard.GetData();
    e.CurrentBuffer.Paste(data, before: false, replace: true);
});
```

## Dependencies

- `Stroke.KeyBinding.Binding` (Feature 19) - Binding class
- `Stroke.KeyBinding.KeyPressEvent` (Feature 19) - Event class
- `Stroke.Core.Buffer` (Feature 06) - Buffer operations
- `Stroke.Core.Document` (Feature 01) - Document queries
- `Stroke.Clipboard` (Feature 40) - Kill ring operations

## Implementation Tasks

1. Create command registry dictionary
2. Implement all movement commands
3. Implement all history commands
4. Implement all text modification commands
5. Implement all kill and yank commands
6. Implement completion commands
7. Implement macro commands
8. Implement miscellaneous commands
9. Implement `GetByName` with error handling
10. Implement `Register` for custom commands
11. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All Readline command names are registered
- [ ] GetByName returns correct bindings
- [ ] GetByName throws for unknown commands
- [ ] Movement commands work correctly
- [ ] Kill/yank commands integrate with kill ring
- [ ] Repeat count is respected
- [ ] Commands can be called from bindings
- [ ] Custom commands can be registered
- [ ] Unit tests achieve 80% coverage
