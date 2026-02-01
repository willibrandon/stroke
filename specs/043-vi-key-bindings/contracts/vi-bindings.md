# Contract: ViBindings

**Feature**: 043-vi-key-bindings
**Namespace**: `Stroke.Application.Bindings`
**Date**: 2026-01-31

## ViBindings Static Class

```csharp
namespace Stroke.Application.Bindings;

using Stroke.KeyBinding;

/// <summary>
/// Key binding loaders for Vi editing mode.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.vi</c> module.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe.
/// </para>
/// </remarks>
public static partial class ViBindings
{
    /// <summary>
    /// Loads all Vi mode key bindings: navigation motions, operators, text objects,
    /// mode switches, insert mode bindings, visual mode handlers, macros, digraphs,
    /// and miscellaneous commands.
    /// </summary>
    /// <returns>
    /// An <see cref="IKeyBindingsBase"/> wrapping all Vi bindings,
    /// conditional on <see cref="ViFilters.ViMode"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Port of Python Prompt Toolkit's <c>load_vi_bindings()</c>.
    /// </para>
    /// <para>
    /// The returned bindings are wrapped in <see cref="ConditionalKeyBindings"/>
    /// gated on <see cref="ViFilters.ViMode"/> so they are only active when
    /// the application is in Vi editing mode.
    /// </para>
    /// <para>
    /// Individual bindings within are further gated by sub-mode filters
    /// (ViNavigationMode, ViInsertMode, ViSelectionMode, etc.).
    /// </para>
    /// </remarks>
    public static IKeyBindingsBase LoadViBindings();
}
```

## Internal API (Private Methods)

### Condition Helpers

```csharp
// Port of Python's @Condition helpers defined at module level
private static readonly IFilter IsReturnable;       // Current buffer has accept handler
private static readonly IFilter InBlockSelection;    // Selection type is Block
private static readonly IFilter DigraphSymbol1Given; // First digraph char entered
private static readonly IFilter SearchBufferIsEmpty; // Search buffer text is empty
private static readonly IFilter TildeOperatorFilter; // Tilde acts as operator
```

### Registration Helpers

```csharp
/// <summary>
/// Registers a text object with up to 3 handler registrations:
/// operator-pending mode, navigation move, selection extend.
/// </summary>
private static void RegisterTextObject(
    KeyBindings kb,
    KeyOrChar[] keys,
    Func<KeyPressEvent, TextObject> handler,
    FilterOrBool filter = default,
    bool noMoveHandler = false,
    bool noSelectionHandler = false,
    bool eager = false);

/// <summary>
/// Registers an operator with 2 handler registrations:
/// navigation (set pending) and selection (execute on selection).
/// </summary>
private static void RegisterOperator(
    KeyBindings kb,
    KeyOrChar[] keys,
    OperatorFuncDelegate operatorFunc,
    FilterOrBool filter = default,
    bool eager = false);
```

### Transform Functions

```csharp
/// <summary>
/// Vi transform function definitions: (keys, filter, transform).
/// Used to create g?, gu, gU, g~, and ~ operators via CreateTransformHandler factory.
/// </summary>
private static readonly (KeyOrChar[] Keys, IFilter Filter, Func<string, string> Transform)[]
    ViTransformFunctions;
```

**5 transform function entries** (matching Python's `vi_transform_functions` list):
| Keys | Filter | Transform | Notes |
|------|--------|-----------|-------|
| g,? | Always | Rot13 | `string.Select(c => ...)` rot13 mapping |
| g,u | Always | Lowercase | `string.ToLower()` |
| g,U | Always | Uppercase | `string.ToUpper()` |
| g,~ | Always | Swap case | `string.Select(c => char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c))` |
| ~ | `TildeOperatorFilter` | Swap case | Only active when `ViState.TildeOperator == true` (mirrors Vim's `tildeop` option) |

When `TildeOperator` is false (default), the standalone `~` handler in Misc handles swap-case-at-cursor.

## Binding Categories

### Navigation Bindings (ViBindings.Navigation.cs)

Handler methods for cursor movement. Items marked "(via text object)" are registered through `RegisterTextObject` and thus automatically get operator-pending, navigation, and selection handlers:

| Key(s) | Handler | Mode Filter | Notes |
|--------|---------|-------------|-------|
| h, Left | Move left | Navigation + Selection | Registered as text object (type: Exclusive) |
| l, Space, Right | Move right | Navigation + Selection | Registered as text object (type: Exclusive) |
| j | Move down | Navigation + Selection | Registered as text object (type: Linewise, `noSelectionHandler=true`) |
| k | Move up | Navigation + Selection | Registered as text object (type: Linewise, `noSelectionHandler=true`) |
| Down, Ctrl-N | Move down | Navigation + Selection | Direct handlers, not text objects |
| Up, Ctrl-P | Move up | Navigation + Selection | Direct handlers, not text objects |
| Backspace | Move left | Navigation | Direct handler |
| w | Word forward | (via text object) | Exclusive |
| W | WORD forward | (via text object) | Exclusive |
| b | Word backward | (via text object) | Exclusive |
| B | WORD backward | (via text object) | Exclusive |
| e | End of word | (via text object) | Inclusive |
| E | End of WORD | (via text object) | Inclusive |
| 0 | Start of line (hard) | (via text object) | Exclusive. Dual behavior: digit-0 when `has_arg` (see CHK024 in Misc) |
| ^ | Start of line (soft) | (via text object) | Exclusive |
| $ | End of line | (via text object) | Inclusive |
| gg | Go to first line | (via text object) | Linewise. With arg: go to document line n |
| G | Go to last line | (via text object) | Linewise. ALSO has separate `@handle("G", filter=has_arg)` for history navigation (see Misc) |
| { | Previous paragraph | (via text object) | Exclusive |
| } | Next paragraph | (via text object) | Exclusive |
| % | Matching bracket | (via text object) | Inclusive |
| \| | Go to column | (via text object) | Exclusive |
| H | Top of screen | (via text object) | Linewise. Uses `WindowRenderInfo.FirstVisibleLine(afterScrollOffset: true)` |
| M | Middle of screen | (via text object) | Linewise. Uses `WindowRenderInfo.CenterVisibleLine(...)` |
| L | Bottom of screen | (via text object) | Linewise. Uses `WindowRenderInfo.LastVisibleLine(beforeScrollOffset: true)` |
| g,e | End of previous word | (via text object) | Inclusive |
| g,E | End of previous WORD | (via text object) | Inclusive |
| g,m | Middle of line | (via text object) | Exclusive |
| g,_ | Last non-blank of line | (via text object) | Exclusive |
| +, Enter | Start of next line | Navigation | Direct handler |
| - | Start of previous line | Navigation | Direct handler |
| n | Search next | (via text object) | Exclusive. Registered in ViBindings, not SearchBindings |
| N | Search previous | (via text object) | Exclusive. Registered in ViBindings, not SearchBindings |
| ( | Begin of sentence | Navigation | Stub in Python (`# TODO`) |
| ) | End of sentence | Navigation | Stub in Python (`# TODO`) |

### Operator Bindings (ViBindings.Operators.cs)

Each operator is registered via `RegisterOperator()` which creates 2 bindings per operator:
1. **Navigation mode**: Stores operator in `ViState.OperatorFunc`, stores count in `ViState.OperatorArg`, enters operator-pending state
2. **Selection mode**: Creates `TextObject` from current `SelectionState` and executes operator immediately

| Operator | Key(s) | Description | Cursor After |
|----------|--------|-------------|--------------|
| Delete | d | Delete text object range, store in clipboard | Start of deleted range |
| Change | c | Delete text object range, store in clipboard, enter insert mode | Start of deleted range (in insert mode) |
| Delete (register) | "x,d | Delete to named register (3-key: `"`, Any, `d`) | Start of deleted range |
| Change (register) | "x,c | Change with named register (3-key: `"`, Any, `c`) | Start of deleted range (in insert mode) |
| Yank | y | Yank (copy) text object range to clipboard | Cursor unchanged |
| Yank (register) | "x,y | Yank to named register (3-key: `"`, Any, `y`) | Cursor unchanged |
| Indent | > | Indent text object lines via `BufferOperations.Indent` | Cursor unchanged |
| Unindent | < | Unindent text object lines via `BufferOperations.Unindent` | Cursor unchanged |
| Reshape | g,q | Reshape/reformat text via `BufferOperations.ReshapeText` | Cursor unchanged |
| Rot13 | g,? | Apply rot13 transform (via `ViTransformFunctions`) | Cursor unchanged |
| Uppercase | g,U | Convert to uppercase (via `ViTransformFunctions`) | Cursor unchanged |
| Lowercase | g,u | Convert to lowercase (via `ViTransformFunctions`) | Cursor unchanged |
| Swap case | g,~ | Toggle case (via `ViTransformFunctions`) | Cursor unchanged |
| Tilde | ~ | Toggle case (via `ViTransformFunctions`, only when `ViState.TildeOperator == true`) | Cursor unchanged |

**Total operator registrations**: 14 (4 from `CreateDeleteAndChangeOperators` factory × 2 calls + 5 from `CreateTransformHandler` factory + y, "Any"y, >, <, g,q = 5 explicit)

### Text Object Bindings (ViBindings.TextObjects.cs)

**Operator-pending text objects** (registered with `noMoveHandler=true`; active only in `vi_waiting_for_text_object_mode`):

| Text Object | Key(s) | Type | Description |
|-------------|--------|------|-------------|
| Inner word | i,w | Exclusive | Word under cursor (via `FindBoundariesOfCurrentWord`) |
| A word | a,w | Exclusive | Word + surrounding space (via `FindBoundariesOfCurrentWord(includeTrailingWhitespace: true)`) |
| Inner WORD | i,W | Exclusive | WORD under cursor |
| A WORD | a,W | Exclusive | WORD + surrounding space |
| A paragraph | a,p | Exclusive | Paragraph + blank lines. Note: Python does NOT implement `ip` (inner paragraph) |
| Inner quotes | i," / i,' / i,` | Exclusive | Content between quotes (via `create_ci_ca_handles` factory) |
| A quotes | a," / a,' / a,` | Exclusive | Content including quotes (via `create_ci_ca_handles` factory) |
| Inner brackets | i,( / i,[ / i,{ / i,< | Exclusive | Content between brackets (via `create_ci_ca_handles` with `FindEnclosingBracketLeft`/`Right`) |
| A brackets | a,( / a,[ / a,{ / a,< | Exclusive | Content including brackets (via `create_ci_ca_handles` factory) |
| Inner paren (alias) | i,b | Exclusive | Same as i,( (registered in `create_ci_ca_handles` with `key='b'`) |
| A paren (alias) | a,b | Exclusive | Same as a,( |
| Inner brace (alias) | i,B | Exclusive | Same as i,{ (registered in `create_ci_ca_handles` with `key='B'`) |
| A brace (alias) | a,B | Exclusive | Same as a,{ |

**Motion text objects** (registered as both navigation moves and operator motions):

| Text Object | Key(s) | Type | Description |
|-------------|--------|------|-------------|
| Character find | f,{char} | Inclusive | Find char forward (`Keys.Any` for char) |
| Character find back | F,{char} | Exclusive | Find char backward |
| Till char | t,{char} | Inclusive | Till char forward |
| Till char back | T,{char} | Exclusive | Till char backward |
| Repeat find | ; | (depends) | Repeat last f/F/t/T (type matches original) |
| Reverse find | , | (depends) | Reverse last f/F/t/T (type matches original) |

**Total text object registrations**: 74 (42 explicit `@text_object` + 32 dynamic via `create_ci_ca_handles`). Each creates up to 3 internal bindings (operator-pending + nav move + selection extend).

### Mode Switch Bindings (ViBindings.ModeSwitch.cs)

| Key | From Mode | To Mode | Extra Behavior |
|-----|-----------|---------|----------------|
| Escape | Insert/Replace | Navigation | `InputMode = Navigation`, cursor moves left by one (`GetCursorLeftPosition()`). At column 0, cursor stays at 0 |
| Escape | Selection | Navigation | `Buffer.ExitSelection()`, cursor stays at current position (no left-by-one) |
| Escape | Navigation (with pending op) | Navigation | Clears `ViState.OperatorFunc`, `ViState.OperatorArg`. Selection cleared if present |
| Escape | Navigation (no pending op) | Navigation | No-op for mode, but still clears selection if present |
| i | Navigation | Insert | Cursor stays. Gated on `~is_read_only` |
| I | Navigation | Insert | Cursor to first non-blank via `GetStartOfLinePosition(afterWhitespace: true)`. Gated on `~is_read_only` |
| a | Navigation | Insert | Cursor right one. Gated on `~is_read_only` |
| A | Navigation | Insert | Cursor to end of line. Gated on `~is_read_only` |
| o | Navigation | Insert | `Buffer.InsertLineBelow()`. Gated on `~is_read_only` |
| O | Navigation | Insert | `Buffer.InsertLineAbove()`. Gated on `~is_read_only` |
| v | Navigation | Selection(Characters) | `Buffer.SelectionState = new SelectionState(cursorPosition, Characters)` |
| V | Navigation | Selection(Lines) | `Buffer.SelectionState = new SelectionState(cursorPosition, Lines)` |
| Ctrl-V | Navigation | Selection(Block) | `Buffer.SelectionState = new SelectionState(cursorPosition, Block)` |
| R | Navigation | Replace | `InputMode = Replace`. Gated on `~is_read_only` |
| r | Navigation | ReplaceSingle | `InputMode = ReplaceSingle`. Next `Keys.Any` replaces char, moves cursor back, returns to Navigation |
| Insert | Navigation | Insert | Toggle to insert |
| Insert | Insert | Navigation | Toggle to navigation |
| Ctrl-O | Insert/Replace | Navigation(temp) | Sets `ViState.TemporaryNavigationMode = true`. KeyProcessor manages the return: after one command completes (no pending operator, no accumulating count), `TemporaryNavigationMode` is set back to `false` and mode returns to the previous insert/replace state. If the command itself triggers a mode change (e.g., entering visual mode), the temporary flag persists until that sub-mode completes |

### Insert Mode Bindings (ViBindings.InsertMode.cs)

Note: `Ctrl-W` (delete word backward) and `Ctrl-H` (backspace) are NOT in Python's vi.py — they are handled by `BasicBindings` / `NamedCommands`. Only bindings explicitly in the Python vi.py `load_vi_bindings()` function are listed here.

| Key | Filter | Description |
|-----|--------|-------------|
| Ctrl-V | `vi_insert_mode` | Quoted insert (delegates to `quoted-insert` named command) |
| Ctrl-N | `vi_insert_mode` | Complete next |
| Ctrl-P | `vi_insert_mode` | Complete previous |
| Ctrl-G, Ctrl-Y | `vi_insert_mode` | Accept completion |
| Ctrl-E | `vi_insert_mode` | Cancel completion |
| Enter | `is_returnable & ~is_multiline` | Accept line (delegates to `accept-line` named command). Note: applies to all modes, not just insert |
| Ctrl-T | `vi_insert_mode` | Indent current line (delegates to `BufferOperations.Indent`) |
| Ctrl-D | `vi_insert_mode` | Unindent current line (delegates to `BufferOperations.Unindent`) |
| Ctrl-X, Ctrl-L | `vi_insert_mode` | Complete line from history |
| Ctrl-X, Ctrl-F | `vi_insert_mode` | Complete filename (stub in Python: TODO) |
| Ctrl-K | `vi_insert_mode \| vi_replace_mode` | Enter digraph mode (`ViState.WaitingForDigraph = true`) |
| Any | `vi_replace_mode` | Insert with `overwrite=true`, replacing character under cursor |
| Any | `vi_replace_single_mode` | Replace single char (`overwrite=true`), move cursor back by 1, set `InputMode = Navigation` |
| Any | `vi_insert_multiple_mode` | Insert at multiple cursor positions (block selection insert) |
| Backspace | `vi_insert_multiple_mode` | Delete before at multiple positions |
| Delete | `vi_insert_multiple_mode` | Delete after at multiple positions |
| Left | `vi_insert_multiple_mode` | Move all cursors left |
| Right | `vi_insert_multiple_mode` | Move all cursors right |
| Up, Down | `vi_insert_multiple_mode` | No-op (ignored for multiple positions) |

**Digraph mode handlers** (2 bindings):
| Key | Filter | Description |
|-----|--------|-------------|
| Any | `vi_digraph_mode & ~digraph_symbol_1_given` | Store first digraph symbol in `ViState.DigraphSymbol1` |
| Any | `vi_digraph_mode & digraph_symbol_1_given` | Look up digraph from `(DigraphSymbol1, key)` pair and insert character |

### Visual Mode Bindings (ViBindings.VisualMode.cs)

All gated on `vi_selection_mode`. Keys J, g,J, x also appear in Misc table — the mode filter (`vi_selection_mode` vs `vi_navigation_mode`) disambiguates.

| Key | Filter | Description |
|-----|--------|-------------|
| j | `vi_selection_mode` | Extend selection down (separate from navigation j text object) |
| k | `vi_selection_mode` | Extend selection up (separate from navigation k text object) |
| x | `vi_selection_mode` | Cut selection to clipboard |
| J | `vi_selection_mode` | Join selected lines with space |
| g,J | `vi_selection_mode` | Join selected lines without space |
| v | `vi_selection_mode` | If current selection is `Characters` → exit visual mode (`Buffer.ExitSelection()`). Otherwise → switch selection type to `Characters` |
| V | `vi_selection_mode` | If current selection is `Lines` → exit visual mode. Otherwise → switch selection type to `Lines` |
| Ctrl-V | `vi_selection_mode` | If current selection is `Block` → exit visual mode. Otherwise → switch selection type to `Block` |
| a,w / a,W | `vi_selection_mode` | Auto-word extend: extend selection to word/WORD boundary |
| I | `vi_selection_mode & in_block_selection` | Enter `InsertMultiple` mode at block selection start positions |
| A | `vi_selection_mode & in_block_selection` | Enter `InsertMultiple` mode, appending after block selection end positions |

### Miscellaneous Bindings (ViBindings.Misc.cs)

Note: Keys that appear in multiple tables (J, g,J, x, ~) are differentiated by mode filter. Navigation-mode handlers are in this table; visual/selection-mode handlers are in the Visual Mode table.

| Key(s) | Filter | Description |
|--------|--------|-------------|
| 1-9 | `vi_navigation_mode \| vi_selection_mode \| vi_waiting_for_text_object_mode` | Numeric argument (digit count). Runtime: 9 bindings from `for n in "123456789"` loop |
| 0 | `(vi_navigation_mode \| vi_selection_mode \| vi_waiting_for_text_object_mode) & has_arg` | Append 0 to numeric argument. Only active when a count is already being accumulated |
| q,{reg} | `vi_navigation_mode & ~vi_recording_macro` | Start macro recording into named register |
| q | `vi_navigation_mode & vi_recording_macro` | Stop macro recording |
| @,{reg} | `vi_navigation_mode` | Play macro from named register |
| @,@ | `vi_navigation_mode` | Replay last macro |
| x | `vi_navigation_mode` | Delete character after cursor (distinct from visual mode `x` which cuts selection) |
| X | `vi_navigation_mode` | Delete character before cursor |
| s | `vi_navigation_mode` | Substitute: delete char at cursor + enter insert mode |
| u | `vi_navigation_mode` | Undo |
| Ctrl-R | `vi_navigation_mode` | Redo |
| p | `vi_navigation_mode` | Paste after cursor. Linewise: paste below, cursor at first char of pasted line. Characterwise: insert after cursor position |
| P | `vi_navigation_mode` | Paste before cursor. Linewise: paste above, cursor at first char of pasted line. Characterwise: insert before cursor position |
| ",{reg},p | `vi_navigation_mode` | Paste from named register after (3-key sequence via `Keys.Any`) |
| ",{reg},P | `vi_navigation_mode` | Paste from named register before (3-key sequence via `Keys.Any`) |
| dd | `vi_navigation_mode` | Delete line(s). Special-case binding (NOT operator+motion). Stores as `SelectionType.LINES` |
| yy, Y | `vi_navigation_mode` | Yank line(s). Special-case binding. Stores as `SelectionType.LINES` |
| cc, S | `vi_navigation_mode` | Change line. Special-case binding. Deletes content after leading whitespace, enters insert |
| C | `vi_navigation_mode` | Change to end of line (equivalent to `c$`) |
| D | `vi_navigation_mode` | Delete to end of line (equivalent to `d$`) |
| J | `vi_navigation_mode` | Join lines with space separator (distinct from visual mode `J`) |
| g,J | `vi_navigation_mode` | Join lines without space (distinct from visual mode `g,J`) |
| ~ | `vi_navigation_mode & ~tilde_operator` | Swap case at cursor and move right. Only active when `TildeOperator` is false (default) |
| g,u,u | `vi_navigation_mode` | Lowercase entire line |
| g,U,U | `vi_navigation_mode` | Uppercase entire line |
| g,~,~ | `vi_navigation_mode` | Swap case entire line |
| >,> | `vi_navigation_mode` | Indent line (doubled key, special-case binding) |
| <,< | `vi_navigation_mode` | Unindent line (doubled key, special-case binding) |
| #, * | `vi_navigation_mode` | Search word under cursor backward/forward |
| Ctrl-A | `vi_navigation_mode` | Increment number at cursor |
| Ctrl-X | `vi_navigation_mode` | Decrement number at cursor |
| z,z | `vi_navigation_mode \| vi_selection_mode` | Scroll cursor to center |
| z,t / z,+ / z,Enter | `vi_navigation_mode \| vi_selection_mode` | Scroll cursor to top |
| z,b / z,- | `vi_navigation_mode \| vi_selection_mode` | Scroll cursor to bottom |
| G | `has_arg` | Go to nth history line (overrides text object G when count given). Calls `Buffer.GoToHistory(arg - 1)` |
| Ctrl-O | `vi_insert_mode \| vi_replace_mode` | Quick normal mode. Sets `ViState.TemporaryNavigationMode = true`. Returns to insert/replace after one navigation command completes (managed by `KeyProcessor`) |
| Keys.Any | `vi_waiting_for_text_object_mode` | Unknown text object catch-all. Sounds bell. Does NOT cancel operator state (Escape required to cancel) |

**Scroll commands note**: `Ctrl-F`/`Ctrl-B`/`Ctrl-D`/`Ctrl-U`/`Ctrl-E`/`Ctrl-Y`/`PageDown`/`PageUp` are already bound in `PageNavigationBindings.LoadViPageNavigationBindings()` and are NOT duplicated here.
