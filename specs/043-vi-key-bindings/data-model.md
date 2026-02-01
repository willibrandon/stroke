# Data Model: Vi Key Bindings

**Feature**: 043-vi-key-bindings
**Date**: 2026-01-31

## Entities

### TextObjectType (Enum)

**Namespace**: `Stroke.KeyBinding`
**Immutability**: Inherently immutable (enum)

| Value | Description | Python Equivalent |
|-------|-------------|-------------------|
| `Exclusive` | End position not included in range | `TextObjectType.EXCLUSIVE` |
| `Inclusive` | End position included in range | `TextObjectType.INCLUSIVE` |
| `Linewise` | Full lines from start to end | `TextObjectType.LINEWISE` |
| `Block` | Rectangular column selection | `TextObjectType.BLOCK` |

### TextObject (Sealed Class)

**Namespace**: `Stroke.KeyBinding`
**Immutability**: Immutable (all properties init-only or computed)
**Thread Safety**: Inherently thread-safe (immutable)

| Property/Method | Type | Description |
|----------------|------|-------------|
| `Start` | `int` | Start offset relative to cursor position |
| `End` | `int` | End offset relative to cursor position (default: 0) |
| `Type` | `TextObjectType` | How the boundary is interpreted (default: Exclusive) |
| `SelectionType` | `SelectionType` (computed) | Maps TextObjectType to Core.SelectionType |
| `Sorted()` | `(int, int)` | Returns (min, max) of Start and End |
| `OperatorRange(Document)` | `(int, int)` | Returns absolute (from, to) positions adjusted for type semantics |
| `GetLineNumbers(Buffer)` | `(int, int)` | Returns (startLine, endLine) for linewise operations |
| `Cut(Buffer)` | `(Document, ClipboardData)` | Cuts the text object range from buffer, returns new document and cut data |

**Mapping: TextObjectType → SelectionType**:
| TextObjectType | SelectionType |
|---------------|---------------|
| Exclusive | Characters |
| Inclusive | Characters |
| Linewise | Lines |
| Block | Block |

### ViBindings (Static Class)

**Namespace**: `Stroke.Application.Bindings`
**Immutability**: Stateless (no instance state)
**Thread Safety**: Inherently thread-safe (stateless)

| Method | Returns | Description |
|--------|---------|-------------|
| `LoadViBindings()` | `IKeyBindingsBase` | All Vi bindings wrapped in `ConditionalKeyBindings(kb, ViFilters.ViMode)` |

**Internal registration helpers** (private):
| Method | Description |
|--------|-------------|
| `RegisterTextObject(kb, keys, handler, ...)` | Registers 1-3 handlers per text object (operator-pending, nav, selection) |
| `RegisterOperator(kb, keys, operatorFunc, ...)` | Registers 2 handlers per operator (nav=set pending, selection=execute) |

### Existing Entities (No Changes)

| Entity | Namespace | Role in Vi Bindings |
|--------|-----------|---------------------|
| `ViState` | `Stroke.KeyBinding` | Mutable state container: InputMode, OperatorFunc, registers, macros |
| `InputMode` | `Stroke.KeyBinding` | Enum: Insert, InsertMultiple, Navigation, Replace, ReplaceSingle |
| `CharacterFind` | `Stroke.KeyBinding` | Record: Character + Backwards for f/F/t/T repeat |
| `OperatorFuncDelegate` | `Stroke.KeyBinding` | Delegate: `(KeyPressEvent, TextObject) → NotImplementedOrNone` (UPDATE: `object?` → `TextObject`) |
| `ClipboardData` | `Stroke.Clipboard` | Text + SelectionType for register storage |
| `Buffer` | `Stroke.Core` | Mutable editing state: cursor, text, undo, selection |
| `Document` | `Stroke.Core` | Immutable text + cursor: navigation methods |

## State Transitions

### Vi Mode State Machine

```
Navigation ──(i,I,a,A,o,O)──→ Insert        [gated on ~is_read_only]
Navigation ──(R)──────────────→ Replace       [gated on ~is_read_only]
Navigation ──(r)──────────────→ ReplaceSingle [gated on ~is_read_only]
Navigation ──(v)──────────────→ Selection (Characters)
Navigation ──(V)──────────────→ Selection (Lines)
Navigation ──(Ctrl-V)─────────→ Selection (Block)
Navigation ──(d,c,y,>,<,g?)───→ OperatorPending (ViState.OperatorFunc set)

Insert ──(Escape)─────────────→ Navigation [cursor left by 1, clamped at col 0]
Replace ──(Escape)────────────→ Navigation [cursor left by 1, clamped at col 0]
ReplaceSingle ──(any char)────→ Navigation [insert w/ overwrite, cursor back by 1 = on replaced char]
Selection ──(Escape)──────────→ Navigation [cursor stays, ExitSelection()]
Selection ──(d,c,y,>,<)───────→ Navigation (after operator applied to selection)
OperatorPending ──(motion/obj)→ Navigation (after operator applied)
OperatorPending ──(Escape)────→ Navigation (cancelled: InputMode setter clears OperatorFunc)
OperatorPending ──(unknown key)→ OperatorPending (bell; operator NOT cancelled)

Insert ──(Ctrl-O)─────────────→ Navigation (temporary: ViState.TemporaryNavigationMode = true)
Replace ──(Ctrl-O)────────────→ Navigation (temporary: same mechanism)
Navigation(temp) ──(cmd done)──→ [previous mode] (KeyProcessor resets TemporaryNavigationMode)
Navigation(temp) ──(op pending)→ Navigation(temp) persists until full command completes

Selection ──(v when Characters)→ Navigation (ExitSelection: same key = exit)
Selection ──(v when Lines)─────→ Selection(Characters) (different key = switch type)
Selection ──(V when Lines)─────→ Navigation (same key = exit)
Selection ──(V when Characters)→ Selection(Lines) (different key = switch type)
[Same pattern for Ctrl-V / Block]

Selection(Block) ──(I)────────→ InsertMultiple [block-only, via in_block_selection filter]
Selection(Block) ──(A)────────→ InsertMultiple [block-only, via in_block_selection filter]
InsertMultiple ──(Escape)─────→ Navigation [applies buffered edits across all block lines]
```

**Undo boundaries**: Mode transitions that use `save_before=True` on the handler registration
create undo save points. Entering insert mode (i/I/a/A/o/O), typing, and exiting (Escape)
constitutes one undo unit.

### Operator-Pending Flow

```
1. User presses operator key (e.g., 'd') in navigation mode
2. ViState.OperatorFunc = deleteOperator
3. ViState.OperatorArg = event.Arg (numeric count from any preceding digits)
4. ViWaitingForTextObjectMode becomes true (OperatorFunc != null)
5. User may press additional digits (these accumulate as the motion count)
6. User presses motion/text object key (e.g., 'w')
7. Count multiplication: event._arg = (ViState.OperatorArg ?? 1) * (event.Arg ?? 1)
   Example: 2d3w → OperatorArg=2, event.Arg=3, effective count = 6
8. Text object handler computes TextObject(start, end, type) using the multiplied count
9. If textObject is not null AND OperatorFunc is not null:
   OperatorFunc is called with (event, textObject)
10. ViState.OperatorFunc = null
11. ViState.OperatorArg = null
12. Back to navigation mode
```

**Operator cancellation**: If an unrecognized key is pressed during operator-pending state
(caught by the `Keys.Any` handler gated on `vi_waiting_for_text_object_mode`), the terminal
bell sounds but the operator state is NOT cleared. The user must press Escape to cancel,
which sets `InputMode = Navigation`, and the `InputMode` setter clears `OperatorFunc`.

**Doubled-key operators**: `dd`, `cc`, `yy`, `>>`, `<<`, `guu`, `gUU`, `g~~` are all
special-case `@handle` bindings (e.g., `@handle("d", "d", filter=vi_navigation_mode)`),
NOT operator+motion composition. They directly perform the linewise operation.

## Relationships

```
ViBindings ──uses──→ ViState (read/write mode, operator state, registers)
ViBindings ──uses──→ ViFilters (filter conditions for binding activation)
ViBindings ──uses──→ AppFilters (HasSelection, IsReadOnly, etc.)
ViBindings ──uses──→ NamedCommands (quoted-insert, accept-line)
ViBindings ──uses──→ Buffer (cursor movement, text editing)
ViBindings ──uses──→ Document (position calculations)
ViBindings ──uses──→ TextObject (motion/text object result type)
ViBindings ──uses──→ Digraphs (Ctrl-K digraph lookup)
ViBindings ──uses──→ SearchOperations (n/N search navigation)
ViBindings ──uses──→ ScrollBindings (z commands, Ctrl-F/B)

TextObject ──uses──→ Document (OperatorRange calculation)
TextObject ──uses──→ Buffer (GetLineNumbers, Cut)
TextObject ──uses──→ ClipboardData (Cut returns clipboard data)
TextObject ──uses──→ TextObjectType (range type classification)
TextObject ──uses──→ SelectionType (mapping for clipboard paste mode)
```
