# Feature 55: Vi Key Bindings

## Overview

Implement the Vi editing mode key bindings including navigation mode, insert mode, visual mode, operators, motions, text objects, registers, and search bindings.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/vi.py`

## Public API

### Binding Loaders

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class ViBindings
{
    /// <summary>
    /// Load Vi mode key bindings.
    /// </summary>
    public static KeyBindingsBase LoadViBindings();

    /// <summary>
    /// Load Vi search key bindings.
    /// </summary>
    public static KeyBindingsBase LoadViSearchBindings();
}
```

### TextObjectType Enum

```csharp
namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Type of text object motion.
/// </summary>
public enum TextObjectType
{
    /// <summary>
    /// Motion does not include end character.
    /// </summary>
    Exclusive,

    /// <summary>
    /// Motion includes end character.
    /// </summary>
    Inclusive,

    /// <summary>
    /// Motion operates on whole lines.
    /// </summary>
    Linewise,

    /// <summary>
    /// Motion operates on rectangular block.
    /// </summary>
    Block
}
```

### TextObject Class

```csharp
namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Return type for text object handlers.
/// </summary>
public sealed class TextObject
{
    /// <summary>
    /// Creates a TextObject.
    /// </summary>
    /// <param name="start">Start position relative to cursor.</param>
    /// <param name="end">End position relative to cursor.</param>
    /// <param name="type">Type of text object.</param>
    public TextObject(int start, int end = 0, TextObjectType type = TextObjectType.Exclusive);

    /// <summary>
    /// Start position relative to cursor.
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// End position relative to cursor.
    /// </summary>
    public int End { get; }

    /// <summary>
    /// Type of text object.
    /// </summary>
    public TextObjectType Type { get; }

    /// <summary>
    /// Get selection type for this text object.
    /// </summary>
    public SelectionType SelectionType { get; }

    /// <summary>
    /// Return (start, end) with start <= end.
    /// </summary>
    public (int Start, int End) Sorted();

    /// <summary>
    /// Get operator range for a document.
    /// </summary>
    public (int Start, int End) OperatorRange(Document document);

    /// <summary>
    /// Get line numbers for this text object.
    /// </summary>
    public (int StartLine, int EndLine) GetLineNumbers(Buffer buffer);

    /// <summary>
    /// Cut text object from buffer.
    /// </summary>
    public (Document NewDocument, ClipboardData Data) Cut(Buffer buffer);
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Bindings/
        ├── ViBindings.cs
        ├── ViSearchBindings.cs
        ├── TextObject.cs
        └── TextObjectType.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        ├── ViBindingsTests.cs
        ├── TextObjectTests.cs
        └── ViSearchBindingsTests.cs
```

## Implementation Notes

### Vi Modes

| Mode | Filter | Description |
|------|--------|-------------|
| Navigation | `vi_navigation_mode` | Normal mode, movement and commands |
| Insert | `vi_insert_mode` | Text insertion |
| Insert Multiple | `vi_insert_multiple_mode` | Insert at multiple cursors |
| Replace | `vi_replace_mode` | Overwrite characters |
| Replace Single | `vi_replace_single_mode` | Overwrite one character |
| Visual | `vi_selection_mode` | Character selection |
| Visual Line | `vi_selection_mode` + linewise | Line selection |
| Visual Block | `vi_selection_mode` + block | Block selection |

### Navigation Mode Bindings

| Key | Description |
|-----|-------------|
| `h` | Move left |
| `j` | Move down |
| `k` | Move up |
| `l` | Move right |
| `w` | Word forward |
| `b` | Word backward |
| `e` | End of word |
| `W`, `B`, `E` | WORD movements (whitespace-delimited) |
| `0` | Start of line |
| `$` | End of line |
| `^` | First non-blank |
| `gg` | Start of buffer |
| `G` | End of buffer |
| `{` | Paragraph backward |
| `}` | Paragraph forward |
| `%` | Matching bracket |

### Mode Switching

| Key | From | To |
|-----|------|-----|
| `i` | Navigation | Insert |
| `I` | Navigation | Insert at line start |
| `a` | Navigation | Insert after cursor |
| `A` | Navigation | Insert at line end |
| `o` | Navigation | Insert on new line below |
| `O` | Navigation | Insert on new line above |
| `R` | Navigation | Replace mode |
| `r` | Navigation | Replace single character |
| `v` | Navigation | Visual character |
| `V` | Navigation | Visual line |
| `Ctrl-V` | Navigation | Visual block |
| `Escape` | Insert/Visual | Navigation |

### Operators

| Key | Operator | Description |
|-----|----------|-------------|
| `d` | delete | Delete text |
| `c` | change | Delete and enter insert mode |
| `y` | yank | Copy to register |
| `>` | indent | Indent lines |
| `<` | unindent | Unindent lines |
| `gU` | uppercase | Convert to uppercase |
| `gu` | lowercase | Convert to lowercase |
| `g~` | swap-case | Toggle case |
| `gq` | format | Format text |

### Operator + Motion

Operators combine with motions:
- `dw` - delete word
- `d$` - delete to end of line
- `c3w` - change 3 words
- `yy` - yank line (doubled operator = line)
- `dd` - delete line

### Text Objects

| Key | Description |
|-----|-------------|
| `iw` | inner word |
| `aw` | a word (includes whitespace) |
| `iW` | inner WORD |
| `aW` | a WORD |
| `is` | inner sentence |
| `as` | a sentence |
| `ip` | inner paragraph |
| `ap` | a paragraph |
| `i"` | inside double quotes |
| `a"` | around double quotes |
| `i'` | inside single quotes |
| `a'` | around single quotes |
| `i(`, `ib` | inside parentheses |
| `a(`, `ab` | around parentheses |
| `i[` | inside brackets |
| `a[` | around brackets |
| `i{`, `iB` | inside braces |
| `a{`, `aB` | around braces |
| `i<` | inside angle brackets |
| `a<` | around angle brackets |

### Registers

| Register | Description |
|----------|-------------|
| `"a` - `"z` | Named registers |
| `"0` - `"9` | Numbered registers (history) |
| `""` | Unnamed register (default) |
| `"+` | System clipboard |
| `"*` | Selection clipboard |

### Search

| Key | Description |
|-----|-------------|
| `/` | Search forward |
| `?` | Search backward |
| `n` | Next match |
| `N` | Previous match |
| `*` | Search word under cursor forward |
| `#` | Search word under cursor backward |

### Character Find

| Key | Description |
|-----|-------------|
| `f{char}` | Find char forward |
| `F{char}` | Find char backward |
| `t{char}` | Till char forward |
| `T{char}` | Till char backward |
| `;` | Repeat find |
| `,` | Repeat find reverse |

### Miscellaneous

| Key | Description |
|-----|-------------|
| `.` | Repeat last change |
| `u` | Undo |
| `Ctrl-R` | Redo |
| `p` | Paste after |
| `P` | Paste before |
| `x` | Delete char |
| `X` | Delete char before |
| `J` | Join lines |
| `~` | Toggle case |
| `q{reg}` | Start macro recording |
| `q` | Stop macro recording |
| `@{reg}` | Play macro |
| `@@` | Repeat last macro |

### Operator Implementation Pattern

```csharp
private void RegisterOperator(KeyBindings bindings, string key, Action<Buffer, TextObject> operation)
{
    bindings.Add(key, e =>
    {
        // Set operator pending state
        e.App.ViState.Operator = operation;
        e.App.ViState.WaitingForMotion = true;
    }, filter: Filters.ViNavigationMode & ~Filters.ViOperatorWaiting);
}

private void RegisterMotion(KeyBindings bindings, string key, Func<KeyPressEvent, TextObject> motion)
{
    // Navigation mode: just move
    bindings.Add(key, e =>
    {
        var obj = motion(e);
        e.CurrentBuffer.CursorPosition += obj.End;
    }, filter: Filters.ViNavigationMode & ~Filters.ViOperatorWaiting);

    // Operator pending: execute operator with motion
    bindings.Add(key, e =>
    {
        var obj = motion(e);
        e.App.ViState.Operator?.Invoke(e.CurrentBuffer, obj);
        e.App.ViState.ClearOperator();
    }, filter: Filters.ViOperatorWaiting);

    // Visual mode: extend selection
    bindings.Add(key, e =>
    {
        var obj = motion(e);
        e.CurrentBuffer.CursorPosition += obj.End;
    }, filter: Filters.ViSelectionMode);
}
```

## Dependencies

- `Stroke.KeyBinding.KeyBindings` (Feature 19) - KeyBindings class
- `Stroke.KeyBinding.ViState` (Feature 38) - Vi state management
- `Stroke.Search` (Feature 53) - Search functions
- `Stroke.Clipboard` (Feature 40) - Clipboard for registers
- `Stroke.Filters` (Feature 12) - Core filter infrastructure
- `Stroke.Filters` (Feature 121) - ViMode, ViNavigationMode, ViInsertMode, ViSelectionMode

## Implementation Tasks

1. Implement `TextObject` class with all methods
2. Implement `TextObjectType` enum
3. Implement navigation mode bindings
4. Implement operators (d, c, y, >, <, etc.)
5. Implement motions (w, b, e, $, 0, etc.)
6. Implement text objects (iw, aw, i", etc.)
7. Implement mode switching (i, a, v, etc.)
8. Implement registers ("a, "+, etc.)
9. Implement character find (f, F, t, T)
10. Implement dot repeat (.)
11. Implement macros (q, @)
12. Implement `LoadViSearchBindings`
13. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All navigation mode bindings work
- [ ] Operators combine with motions correctly
- [ ] Text objects select correct ranges
- [ ] Mode switching works (i, a, v, V, Ctrl-V)
- [ ] Registers store and retrieve text
- [ ] Character find and repeat work
- [ ] Dot repeat replays last change
- [ ] Macro recording and playback work
- [ ] Search and n/N work
- [ ] Visual mode selection works
- [ ] Operator + motion = linewise for doubled operators
- [ ] Unit tests achieve 80% coverage
