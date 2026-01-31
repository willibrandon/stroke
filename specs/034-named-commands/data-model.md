# Data Model: Named Commands

**Feature**: 034-named-commands
**Date**: 2026-01-30

## Entities

### NamedCommands (Static Registry)

The central registry mapping Readline command name strings to `Binding` objects. This is a static class, not an instantiable entity.

| Field | Type | Description |
|-------|------|-------------|
| `_commands` | `ConcurrentDictionary<string, Binding>` | Internal dictionary mapping command names to Binding objects |

**Behavior**:
- Initialized in static constructor with all 49 built-in commands
- Thread-safe via `ConcurrentDictionary`
- Supports runtime registration of new commands or overrides

### Named Command Entry (Logical)

Each entry in the registry is a key-value pair.

| Field | Type | Description |
|-------|------|-------------|
| Name | `string` | Readline command name in kebab-case (e.g., "forward-char") |
| Binding | `Binding` | Immutable binding with handler, filter, and macro recording settings |

### Binding (Existing Entity — Reused)

Already defined in `Stroke.KeyBinding.Binding`. Named commands create `Binding` instances with:
- `Keys`: `[Keys.Any]` (placeholder; named commands are looked up by name, not key sequence)
- `Handler`: The command's handler function as `KeyHandlerCallable`
- `Filter`: `Always` (default — named commands are always available)
- `Eager`: `Never` (default)
- `IsGlobal`: `Never` (default)
- `SaveBefore`: `_ => true` (default)
- `RecordInMacro`: `Always` (default), except `call-last-kbd-macro` which uses `Never`

## Command Catalog

### Movement Commands (10)

| Readline Name | Handler Method | Description |
|---------------|---------------|-------------|
| `beginning-of-buffer` | `BeginningOfBuffer` | Set cursor position to 0 |
| `end-of-buffer` | `EndOfBuffer` | Set cursor position to end of text |
| `beginning-of-line` | `BeginningOfLine` | Move cursor to start of current line |
| `end-of-line` | `EndOfLine` | Move cursor to end of current line |
| `forward-char` | `ForwardChar` | Move cursor right by `event.Arg` characters |
| `backward-char` | `BackwardChar` | Move cursor left by `event.Arg` characters |
| `forward-word` | `ForwardWord` | Move to end of next word (by `event.Arg`) |
| `backward-word` | `BackwardWord` | Move to start of previous word (by `event.Arg`) |
| `clear-screen` | `ClearScreen` | Clear renderer screen |
| `redraw-current-line` | `RedrawCurrentLine` | No-op (defined by Readline but not implemented in PTK) |

### History Commands (6)

| Readline Name | Handler Method | Description |
|---------------|---------------|-------------|
| `accept-line` | `AcceptLine` | Validate and handle current input |
| `previous-history` | `PreviousHistory` | Move backward in history by `event.Arg` |
| `next-history` | `NextHistory` | Move forward in history by `event.Arg` |
| `beginning-of-history` | `BeginningOfHistory` | Jump to first history entry (index 0) |
| `end-of-history` | `EndOfHistory` | Jump to current input (last working line) |
| `reverse-search-history` | `ReverseSearchHistory` | Activate incremental backward search |

### Text Modification Commands (9)

| Readline Name | Handler Method | Description |
|---------------|---------------|-------------|
| `end-of-file` | `EndOfFile` | Exit the application |
| `delete-char` | `DeleteChar` | Delete character at cursor (forward); bell if nothing deleted |
| `backward-delete-char` | `BackwardDeleteChar` | Delete character behind cursor; negative arg deletes forward |
| `self-insert` | `SelfInsert` | Insert `event.Data` repeated `event.Arg` times |
| `transpose-chars` | `TransposeChars` | Emacs transpose: no-op at pos 0, swap before cursor at end/newline, else move right then swap |
| `uppercase-word` | `UppercaseWord` | Uppercase current/following word(s) by `event.Arg` |
| `downcase-word` | `DowncaseWord` | Lowercase current/following word(s) by `event.Arg` |
| `capitalize-word` | `CapitalizeWord` | Title-case current/following word(s) by `event.Arg` |
| `quoted-insert` | `QuotedInsert` | Set `App.QuotedInsert = true` |

### Kill and Yank Commands (10)

| Readline Name | Handler Method | Description |
|---------------|---------------|-------------|
| `kill-line` | `KillLine` | Kill to end of line; negative arg kills to start; at newline, delete newline |
| `kill-word` | `KillWord` | Kill to next word end; concatenate on repeat |
| `unix-word-rubout` | `UnixWordRubout` | Kill previous WORD (whitespace boundary); concatenate on repeat; bell if nothing |
| `backward-kill-word` | `BackwardKillWord` | Kill previous word (non-alphanumeric boundary) via `UnixWordRubout(WORD=false)` |
| `delete-horizontal-space` | `DeleteHorizontalSpace` | Delete tabs/spaces around cursor |
| `unix-line-discard` | `UnixLineDiscard` | Kill backward to line start; at column 0 delete one char back |
| `yank` | `Yank` | Paste clipboard with Emacs paste mode |
| `yank-nth-arg` | `YankNthArg` | Insert nth word from previous history entry |
| `yank-last-arg` | `YankLastArg` | Insert last word from previous history entry |
| `yank-pop` | `YankPop` | Rotate clipboard and replace previously yanked text |

### Completion Commands (3)

| Readline Name | Handler Method | Description |
|---------------|---------------|-------------|
| `complete` | `Complete` | Readline-style completion display |
| `menu-complete` | `MenuComplete` | Generate completions or advance to next |
| `menu-complete-backward` | `MenuCompleteBackward` | Move backward through completions |

### Macro Commands (4)

| Readline Name | Handler Method | Description |
|---------------|---------------|-------------|
| `start-kbd-macro` | `StartKbdMacro` | Begin recording keystrokes |
| `end-kbd-macro` | `EndKbdMacro` | Stop recording and save macro |
| `call-last-kbd-macro` | `CallLastKbdMacro` | Replay last recorded macro (`RecordInMacro=false`) |
| `print-last-kbd-macro` | `PrintLastKbdMacro` | Print macro via RunInTerminal |

### Miscellaneous Commands (7)

| Readline Name | Handler Method | Description |
|---------------|---------------|-------------|
| `undo` | `Undo` | Incremental undo |
| `insert-comment` | `InsertComment` | Comment (arg=1) or uncomment (arg!=1) all lines, then accept |
| `vi-editing-mode` | `ViEditingMode` | Switch to Vi mode |
| `emacs-editing-mode` | `EmacsEditingMode` | Switch to Emacs mode |
| `prefix-meta` | `PrefixMeta` | Feed Escape key to key processor |
| `operate-and-get-next` | `OperateAndGetNext` | Accept input and queue next history index |
| `edit-and-execute-command` | `EditAndExecuteCommand` | Open current line in editor |

## Relationships

```text
NamedCommands (static registry)
    └── contains ──→ Dictionary<string, Binding>
                         └── each entry maps:
                              string (command name)  ──→  Binding
                                                            ├── Handler (KeyHandlerCallable)
                                                            │     └── operates on:
                                                            │           ├── KeyPressEvent.CurrentBuffer (Buffer)
                                                            │           │     └── contains Document (immutable)
                                                            │           ├── KeyPressEvent.App (Application)
                                                            │           │     ├── .Clipboard (IClipboard)
                                                            │           │     ├── .Renderer
                                                            │           │     ├── .EmacsState
                                                            │           │     ├── .KeyProcessor
                                                            │           │     ├── .Layout
                                                            │           │     ├── .Output
                                                            │           │     ├── .EditingMode
                                                            │           │     ├── .QuotedInsert
                                                            │           │     └── .PreRunCallables
                                                            │           └── KeyPressEvent properties
                                                            │                 ├── .Arg (repeat count)
                                                            │                 ├── .ArgPresent
                                                            │                 ├── .Data (key data)
                                                            │                 └── .IsRepeat
                                                            ├── RecordInMacro (IFilter)
                                                            └── Keys ([Keys.Any])
```

## State Transitions

The named commands registry itself has no state transitions — it is initialized once at static construction time and then only modified by explicit `Register` calls. The individual commands modify `Buffer`, `Application`, and clipboard state, but those state transitions are owned by those respective classes.
