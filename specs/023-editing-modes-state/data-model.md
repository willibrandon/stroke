# Data Model: Editing Modes and State

**Feature**: 023-editing-modes-state
**Date**: 2026-01-27

## Entity Overview

This feature introduces 6 public types for managing Vi and Emacs editing mode state:

| Entity | Type | Mutability | Thread Safety |
|--------|------|------------|---------------|
| EditingMode | Enum | Immutable | Inherent |
| BufferNames | Static class | Immutable | Inherent |
| InputMode | Enum | Immutable | Inherent |
| CharacterFind | Record | Immutable | Inherent |
| ViState | Class | Mutable | Lock-protected |
| EmacsState | Class | Mutable | Lock-protected |

---

## Entities

### EditingMode

**Purpose**: Identifies which key binding set is active (Vi or Emacs).

**Type**: Enum

| Value | Description |
|-------|-------------|
| `Vi` | Vi-style modal editing with Navigation/Insert/Replace modes |
| `Emacs` | Emacs-style editing with chord-based key bindings |

**Relationships**: Referenced by Application layer to determine active key binding set.

**Source**: `prompt_toolkit/enums.py` line 6-9

---

### BufferNames

**Purpose**: Provides standard constant names for well-known buffers.

**Type**: Static class with string constants

| Constant | Value | Description |
|----------|-------|-------------|
| `SearchBuffer` | `"SEARCH_BUFFER"` | Buffer for search input |
| `DefaultBuffer` | `"DEFAULT_BUFFER"` | Main editing buffer |
| `SystemBuffer` | `"SYSTEM_BUFFER"` | System messages buffer |

**Relationships**: Used by Buffer and Application layers for buffer identification.

**Source**: `prompt_toolkit/enums.py` lines 13-19

---

### InputMode

**Purpose**: Represents Vi input mode states.

**Type**: Enum

| Value | Description |
|-------|-------------|
| `Insert` | Normal text insertion mode |
| `InsertMultiple` | Insert mode for multiple cursors |
| `Navigation` | Vi normal mode for navigation/commands |
| `Replace` | Overwrite mode (like `R` command) |
| `ReplaceSingle` | Replace single character (like `r` command) |

**State Transitions**:
- `Insert` → `Navigation`: Escape key
- `Navigation` → `Insert`: `i`, `a`, `o`, etc.
- `Navigation` → `Replace`: `R` command
- `Navigation` → `ReplaceSingle`: `r` command
- `ReplaceSingle` → `Insert`: After single character replaced

**Relationships**: Stored in ViState.InputMode property.

**Source**: `prompt_toolkit/key_binding/vi_state.py` lines 19-27

---

### CharacterFind

**Purpose**: Stores the target character and direction for Vi f/F/t/T commands.

**Type**: Immutable record

| Field | Type | Description |
|-------|------|-------------|
| `Character` | `string` | The target character to find |
| `Backwards` | `bool` | True for F/T (backwards), False for f/t (forwards) |

**Validation**: Character should be a single character string (enforced by caller).

**Relationships**: Stored in ViState.LastCharacterFind for repeat (;) and reverse (,) commands.

**Source**: `prompt_toolkit/key_binding/vi_state.py` lines 29-32

---

### ViState

**Purpose**: Mutable container for all Vi-specific navigation state.

**Type**: Thread-safe mutable class

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `InputMode` | `InputMode` | `Insert` | Current Vi input mode |
| `LastCharacterFind` | `CharacterFind?` | `null` | Last f/F/t/T search |
| `OperatorFunc` | `OperatorFuncDelegate?` | `null` | Pending operator callback |
| `OperatorArg` | `int?` | `null` | Count argument for operator |
| `NamedRegisters` | `Dictionary<string, ClipboardData>` | empty | Vi named registers (a-z) |
| `WaitingForDigraph` | `bool` | `false` | Awaiting second digraph char |
| `DigraphSymbol1` | `string?` | `null` | First digraph character |
| `TildeOperator` | `bool` | `false` | Whether ~ acts as operator |
| `RecordingRegister` | `string?` | `null` | Register being recorded to |
| `CurrentRecording` | `string` | `""` | Accumulated macro content |
| `TemporaryNavigationMode` | `bool` | `false` | Ctrl+O temporary navigation |

**State Rules**:
1. When `InputMode` set to `Navigation`:
   - `WaitingForDigraph` → `false`
   - `OperatorFunc` → `null`
   - `OperatorArg` → `null`

2. When `Reset()` called:
   - `InputMode` → `Insert`
   - `WaitingForDigraph` → `false`
   - `OperatorFunc` → `null`
   - `OperatorArg` → `null`
   - `RecordingRegister` → `null`
   - `CurrentRecording` → `""`

**Thread Safety**: All state access protected by single `Lock` instance.

**Relationships**:
- Uses `ClipboardData` from Stroke.Clipboard
- Uses `KeyPressEvent` from Stroke.KeyBinding (via OperatorFuncDelegate)

**Source**: `prompt_toolkit/key_binding/vi_state.py` lines 35-107

---

### EmacsState

**Purpose**: Mutable container for Emacs-specific state (macro recording).

**Type**: Thread-safe mutable class

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `Macro` | `List<KeyPress>?` | empty list | Last recorded macro |
| `CurrentRecording` | `List<KeyPress>?` | `null` | In-progress recording |

**Computed Properties**:
- `IsRecording`: Returns `CurrentRecording != null`

**Methods**:
| Method | Behavior |
|--------|----------|
| `StartMacro()` | Sets `CurrentRecording` to new empty list |
| `EndMacro()` | Copies `CurrentRecording` to `Macro`, sets `CurrentRecording` to `null` |
| `Reset()` | Sets `CurrentRecording` to `null` |

**Thread Safety**: All state access protected by single `Lock` instance.

**Relationships**:
- Uses `KeyPress` from Stroke.Input

**Source**: `prompt_toolkit/key_binding/emacs_state.py` lines 10-36

---

## Delegate Types

### OperatorFuncDelegate

**Purpose**: Callback signature for pending Vi operator functions.

```csharp
public delegate NotImplementedOrNone OperatorFuncDelegate(KeyPressEvent e, object? textObject);
```

**Parameters**:
- `e`: The key press event that completes the operator
- `textObject`: The text object (placeholder type until ITextObject defined)

**Returns**: `NotImplementedOrNone` indicating if event was handled.

**Rationale**: Python uses `Callable[[KeyPressEvent, TextObject], None]`. Since TextObject interface is future work, use `object?` as placeholder.

---

## Namespace Organization

```
Stroke.KeyBinding/
├── EditingMode.cs       # EditingMode enum
├── BufferNames.cs       # BufferNames static class
├── InputMode.cs         # InputMode enum
├── CharacterFind.cs     # CharacterFind record
├── ViState.cs           # ViState class + OperatorFuncDelegate
└── EmacsState.cs        # EmacsState class
```

All types in `Stroke.KeyBinding` namespace per Constitution III (KeyBinding depends on Core, Input).

---

## Dependencies

| This Feature Uses | From | For |
|-------------------|------|-----|
| `KeyPress` | `Stroke.Input` | EmacsState macro storage |
| `ClipboardData` | `Stroke.Clipboard` | ViState named registers |
| `KeyPressEvent` | `Stroke.KeyBinding` | OperatorFuncDelegate parameter |
| `NotImplementedOrNone` | `Stroke.KeyBinding` | OperatorFuncDelegate return type |

| Used By (Future) | For |
|------------------|-----|
| Key binding handlers | Mode-conditional behavior |
| Application layer | Editing mode configuration |
| Filter system | InViMode, InEmacsMode filters |
