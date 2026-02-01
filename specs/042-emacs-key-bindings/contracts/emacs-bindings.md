# API Contract: EmacsBindings

**Feature**: 042-emacs-key-bindings
**Namespace**: `Stroke.Application.Bindings`
**Python Source**: `prompt_toolkit.key_binding.bindings.emacs`

## Public API

### Class: EmacsBindings (static)

```csharp
/// <summary>
/// Key binding loaders for Emacs editing mode.
/// Port of Python Prompt Toolkit's prompt_toolkit.key_binding.bindings.emacs module.
/// This type is stateless and inherently thread-safe.
/// </summary>
public static partial class EmacsBindings
{
    /// <summary>
    /// Load core Emacs key bindings for movement, editing, kill ring, history,
    /// completion, selection, macros, character search, and numeric arguments.
    /// </summary>
    /// <returns>
    /// An IKeyBindingsBase wrapping all core Emacs bindings, conditional on EmacsMode.
    /// </returns>
    public static IKeyBindingsBase LoadEmacsBindings();

    /// <summary>
    /// Load Emacs shift-selection key bindings for selecting text with Shift+movement keys.
    /// </summary>
    /// <returns>
    /// An IKeyBindingsBase wrapping all shift-selection bindings, conditional on EmacsMode.
    /// </returns>
    public static IKeyBindingsBase LoadEmacsShiftSelectionBindings();
}
```

## Binding Inventory: LoadEmacsBindings()

### Named Command Bindings

| # | Key Sequence | Named Command | Filter | saveBefore |
|---|-------------|---------------|--------|------------|
| 1 | `Ctrl-A` | `beginning-of-line` | *(none)* | default |
| 2 | `Ctrl-B` | `backward-char` | *(none)* | default |
| 3 | `Ctrl-Delete` | `kill-word` | `insert_mode` | default |
| 4 | `Ctrl-E` | `end-of-line` | *(none)* | default |
| 5 | `Ctrl-F` | `forward-char` | *(none)* | default |
| 6 | `Ctrl-Left` | `backward-word` | *(none)* | default |
| 7 | `Ctrl-Right` | `forward-word` | *(none)* | default |
| 8 | `Ctrl-X, r, y` | `yank` | `insert_mode` | default |
| 9 | `Ctrl-Y` | `yank` | `insert_mode` | default |
| 10 | `Escape, b` | `backward-word` | *(none)* | default |
| 11 | `Escape, c` | `capitalize-word` | `insert_mode` | default |
| 12 | `Escape, d` | `kill-word` | `insert_mode` | default |
| 13 | `Escape, f` | `forward-word` | *(none)* | default |
| 14 | `Escape, l` | `downcase-word` | `insert_mode` | default |
| 15 | `Escape, u` | `uppercase-word` | `insert_mode` | default |
| 16 | `Escape, y` | `yank-pop` | `insert_mode` | default |
| 17 | `Escape, Backspace` | `backward-kill-word` | `insert_mode` | default |
| 18 | `Escape, \` | `delete-horizontal-space` | `insert_mode` | default |
| 19 | `Ctrl-Home` | `beginning-of-buffer` | *(none)* | default |
| 20 | `Ctrl-End` | `end-of-buffer` | *(none)* | default |
| 21 | `Ctrl-_` | `undo` | `insert_mode` | `false` |
| 22 | `Ctrl-X, Ctrl-U` | `undo` | `insert_mode` | `false` |
| 23 | `Escape, <` | `beginning-of-history` | `~has_selection` | default |
| 24 | `Escape, >` | `end-of-history` | `~has_selection` | default |
| 25 | `Escape, .` | `yank-last-arg` | `insert_mode` | default |
| 26 | `Escape, _` | `yank-last-arg` | `insert_mode` | default |
| 27 | `Escape, Ctrl-Y` | `yank-nth-arg` | `insert_mode` | default |
| 28 | `Escape, #` | `insert-comment` | `insert_mode` | default |
| 29 | `Ctrl-O` | `operate-and-get-next` | *(none)* | default |
| 30 | `Ctrl-Q` | `quoted-insert` | `~has_selection` | default |
| 31 | `Ctrl-X, (` | `start-kbd-macro` | *(none)* | default |
| 32 | `Ctrl-X, )` | `end-kbd-macro` | *(none)* | default |
| 33 | `Ctrl-X, e` | `call-last-kbd-macro` | *(none)* | default |
| 34 | `Escape, Enter` | `accept-line` | `insert_mode & is_returnable` | default |
| 35 | `Enter` | `accept-line` | `insert_mode & is_returnable & ~is_multiline` | default |

### Inline Handler Bindings

| # | Key Sequence | Handler | Filter |
|---|-------------|---------|--------|
| 36 | `Escape` | *(no-op)* | *(none)* |
| 37 | `Ctrl-N` | `AutoDown` | *(none)* |
| 38 | `Ctrl-P` | `AutoUp` | *(none)* |
| 39-48 | `Escape, 0`..`Escape, 9` | `HandleDigit` | *(none)* |
| 39-48 | `0`..`9` | `HandleDigit` | `has_arg` |
| 49 | `Escape, -` | `MetaDash` | `~has_arg` |
| 50 | `-` | `DashWhenArg` | `is_arg` |
| 51 | `Ctrl-], Any` | `GotoChar` | *(none)* |
| 52 | `Escape, Ctrl-], Any` | `GotoCharBackwards` | *(none)* |
| 53 | `Escape, a` | `PrevSentence` | *(none)* |
| 54 | `Escape, e` | `EndOfSentence` | *(none)* |
| 55 | `Escape, t` | `SwapCharacters` | `insert_mode` |
| 56 | `Escape, *` | `InsertAllCompletions` | `insert_mode` |
| 57 | `Ctrl-X, Ctrl-X` | `ToggleStartEnd` | *(none)* |
| 58 | `Ctrl-@` | `StartSelection` | *(none)* |
| 59 | `Ctrl-G` | `Cancel` | `~has_selection` |
| 60 | `Ctrl-G` | `CancelSelection` | `has_selection` |
| 61 | `Ctrl-W` | `CutSelection` | `has_selection` |
| 62 | `Ctrl-X, r, k` | `CutSelection` | `has_selection` |
| 63 | `Escape, w` | `CopySelection` | `has_selection` |
| 64 | `Escape, Left` | `StartOfWord` | *(none)* |
| 65 | `Escape, Right` | `StartNextWord` | *(none)* |
| 66 | `Escape, /` | `Complete` | `insert_mode` |
| 67 | `Ctrl-C, >` | `IndentSelection` | `has_selection` |
| 68 | `Ctrl-C, <` | `UnindentSelection` | `has_selection` |

**Total**: 78 registrations (35 named command + 43 inline handler; digit rows 39-48 each represent 2 registrations: Meta+digit and plain digit)

## Binding Inventory: LoadEmacsShiftSelectionBindings()

### Start Selection (filter: ~has_selection)

| Key | Action |
|-----|--------|
| `Shift-Left` | Start selection + backward-char |
| `Shift-Right` | Start selection + forward-char |
| `Shift-Up` | Start selection + auto_up |
| `Shift-Down` | Start selection + auto_down |
| `Shift-Home` | Start selection + beginning-of-line |
| `Shift-End` | Start selection + end-of-line |
| `Ctrl-Shift-Left` | Start selection + backward-word |
| `Ctrl-Shift-Right` | Start selection + forward-word |
| `Ctrl-Shift-Home` | Start selection + beginning-of-buffer |
| `Ctrl-Shift-End` | Start selection + end-of-buffer |

### Extend Selection (filter: shift_selection_mode)

Same 10 keys as above, but extend the existing selection.

### Replace/Cancel (filter: shift_selection_mode)

| Key | Action |
|-----|--------|
| `Any` | Cut selection, self-insert |
| `Enter` | Cut selection, newline (filter: `& is_multiline`) |
| `Backspace` | Cut selection |
| `Ctrl-Y` | Cut selection, yank |

### Cancel Movement (filter: shift_selection_mode)

| Key | Action |
|-----|--------|
| `Left` | Exit selection, re-feed key |
| `Right` | Exit selection, re-feed key |
| `Up` | Exit selection, re-feed key |
| `Down` | Exit selection, re-feed key |
| `Home` | Exit selection, re-feed key |
| `End` | Exit selection, re-feed key |
| `Ctrl-Left` | Exit selection, re-feed key |
| `Ctrl-Right` | Exit selection, re-feed key |
| `Ctrl-Home` | Exit selection, re-feed key |
| `Ctrl-End` | Exit selection, re-feed key |

**Total**: 34 registrations

## Internal API (Private)

### Private Static Filters

```csharp
private static readonly IFilter IsReturnable;  // CurrentBuffer.IsReturnable
private static readonly IFilter IsArg;         // KeyProcessor.Arg == "-"
```

### Private Static Handler Methods

All return `NotImplementedOrNone?` and accept `KeyPressEvent`.

```csharp
// Core handlers
private static NotImplementedOrNone? Ignore(KeyPressEvent @event);
private static NotImplementedOrNone? AutoDown(KeyPressEvent @event);
private static NotImplementedOrNone? AutoUp(KeyPressEvent @event);
private static NotImplementedOrNone? HandleDigit(KeyPressEvent @event);
private static NotImplementedOrNone? MetaDash(KeyPressEvent @event);
private static NotImplementedOrNone? DashWhenArg(KeyPressEvent @event);
private static NotImplementedOrNone? GotoChar(KeyPressEvent @event);
private static NotImplementedOrNone? GotoCharBackwards(KeyPressEvent @event);
private static NotImplementedOrNone? PrevSentence(KeyPressEvent @event);
private static NotImplementedOrNone? EndOfSentence(KeyPressEvent @event);
private static NotImplementedOrNone? SwapCharacters(KeyPressEvent @event);
private static NotImplementedOrNone? InsertAllCompletions(KeyPressEvent @event);
private static NotImplementedOrNone? ToggleStartEnd(KeyPressEvent @event);
private static NotImplementedOrNone? StartSelection(KeyPressEvent @event);
private static NotImplementedOrNone? Cancel(KeyPressEvent @event);
private static NotImplementedOrNone? CancelSelection(KeyPressEvent @event);
private static NotImplementedOrNone? CutSelection(KeyPressEvent @event);
private static NotImplementedOrNone? CopySelection(KeyPressEvent @event);
private static NotImplementedOrNone? StartOfWord(KeyPressEvent @event);
private static NotImplementedOrNone? StartNextWord(KeyPressEvent @event);
private static NotImplementedOrNone? Complete(KeyPressEvent @event);
private static NotImplementedOrNone? IndentSelection(KeyPressEvent @event);
private static NotImplementedOrNone? UnindentSelection(KeyPressEvent @event);

// Helper (non-handler)
private static void CharacterSearch(Buffer buff, string @char, int count);

// Shift-selection handlers
private static void UnshiftMove(KeyPressEvent @event);
private static NotImplementedOrNone? ShiftStartSelection(KeyPressEvent @event);
private static NotImplementedOrNone? ShiftExtendSelection(KeyPressEvent @event);
private static NotImplementedOrNone? ShiftReplaceSelection(KeyPressEvent @event);
private static NotImplementedOrNone? ShiftNewline(KeyPressEvent @event);
private static NotImplementedOrNone? ShiftDelete(KeyPressEvent @event);
private static NotImplementedOrNone? ShiftYank(KeyPressEvent @event);
private static NotImplementedOrNone? ShiftCancelMove(KeyPressEvent @event);
```

## Dependencies

| Dependency | Source | Used For |
|-----------|--------|----------|
| `KeyBindings` | Stroke.KeyBinding | Binding registration |
| `ConditionalKeyBindings` | Stroke.KeyBinding | Emacs mode gating |
| `Binding` | Stroke.KeyBinding | Named command instances |
| `KeyOrChar` | Stroke.KeyBinding | Key sequence elements |
| `KeyPressEvent` | Stroke.KeyBinding | Handler parameter |
| `KeyHandlerCallable` | Stroke.KeyBinding | Handler delegate type |
| `NotImplementedOrNone` | Stroke.KeyBinding | Handler return type |
| `FilterOrBool` | Stroke.Filters | Filter wrapper for Add<T>() |
| `NamedCommands` | Stroke.KeyBinding.Bindings | Named command lookups |
| `KeyPressEventExtensions` | Stroke.KeyBinding.Bindings | `GetApp()` extension |
| `EmacsFilters` | Stroke.Application | EmacsMode, EmacsInsertMode |
| `AppFilters` | Stroke.Application | HasSelection, IsMultiline, HasArg, InPasteMode |
| `SearchFilters` | Stroke.Application | ShiftSelectionMode |
| `ViFilters` | Stroke.Application | ViSearchDirectionReversed (not used here directly) |
| `AppContext` | Stroke.Application | GetApp() for filter conditions |
| `Buffer` | Stroke.Core | Buffer operations |
| `BufferOperations` | Stroke.Core | Indent/Unindent |
| `Document` | Stroke.Core | Document queries |
| `SelectionState` | Stroke.Core | Selection management |
| `SelectionType` | Stroke.Core | Characters selection type |
| `Keys` | Stroke.Input | Key constants |
| `KeyPress` | Stroke.KeyBinding | Key press construction |
| `KeyProcessor` | Stroke.KeyBinding | Feed() for re-processing, Arg property |
| `CompleteEvent` | Stroke.Completion | Completion trigger |
| `ICompleter` | Stroke.Completion | Get completions |
| `IClipboard` | Stroke.Clipboard | Clipboard access |
| `ClipboardData` | Stroke.Clipboard | Clipboard data type |
