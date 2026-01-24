# Data Model: Buffer (Mutable Text Container)

**Date**: 2026-01-24
**Feature**: 007-mutable-buffer

## Entity Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                  Buffer                                      │
│  (Mutable text container wrapping immutable Document)                       │
├─────────────────────────────────────────────────────────────────────────────┤
│  CORE STATE                                                                 │
│  ─────────────                                                              │
│  _workingLines: List<string>     # History entries + current text           │
│  _workingIndex: int              # Index into _workingLines                 │
│  _cursorPosition: int            # Current cursor position                  │
│                                                                             │
│  UNDO/REDO                                                                  │
│  ─────────                                                                  │
│  _undoStack: List<(string, int)> # (text, cursor_position) tuples          │
│  _redoStack: List<(string, int)> # (text, cursor_position) tuples          │
│                                                                             │
│  SELECTION                                                                  │
│  ─────────                                                                  │
│  _selectionState: SelectionState? # Current selection state                │
│  _preferredColumn: int?          # Column preference for up/down           │
│  _documentBeforePaste: Document? # For kill ring rotation                  │
│                                                                             │
│  COMPLETION                                                                 │
│  ──────────                                                                 │
│  _completeState: CompletionState? # Current completion state               │
│  _yankNthArgState: YankNthArgState? # Emacs yank state                    │
│                                                                             │
│  VALIDATION                                                                 │
│  ──────────                                                                 │
│  _validationState: ValidationState # Valid, Invalid, Unknown               │
│  _validationError: ValidationError? # Error details if invalid            │
│                                                                             │
│  SUGGESTION                                                                 │
│  ──────────                                                                 │
│  _suggestion: Suggestion?        # Current auto-suggest                    │
│                                                                             │
│  HISTORY SEARCH                                                             │
│  ──────────────                                                             │
│  _historySearchText: string?     # Prefix for history filtering            │
│                                                                             │
│  CONFIGURATION                                                              │
│  ─────────────                                                              │
│  Completer: ICompleter           # Completion provider                     │
│  AutoSuggest: IAutoSuggest?      # Auto-suggest provider                   │
│  History: IHistory               # History storage                         │
│  Validator: IValidator?          # Input validator                         │
│  Name: string                    # Buffer name (e.g., "DEFAULT_BUFFER")    │
│  TempfileSuffix: Func<string>    # Suffix for external editor temp files   │
│  Tempfile: Func<string>          # Full path for external editor           │
│  AcceptHandler: Func<Buffer,bool>? # Called on input acceptance           │
│  TextWidth: int                  # Width for text reshaping (Vi 'gq')      │
│  MaxNumberOfCompletions: int     # Limit completions (default: 10000)      │
│                                                                             │
│  FILTERS (Func<bool>)                                                       │
│  ─────────────────────                                                      │
│  CompleteWhileTyping             # Enable async completion                 │
│  ValidateWhileTyping             # Enable async validation                 │
│  EnableHistorySearch             # Enable prefix-based history filter      │
│  ReadOnly                        # Prevent modifications                   │
│  Multiline                       # Allow multiple lines                    │
│                                                                             │
│  CACHING                                                                    │
│  ───────                                                                    │
│  _documentCache: FastDictCache   # Cache Document instances                │
│                                                                             │
│  ASYNC LOCKS                                                                │
│  ──────────                                                                 │
│  _completionLock: SemaphoreSlim  # One completion at a time                │
│  _suggestionLock: SemaphoreSlim  # One suggestion at a time                │
│  _validationLock: SemaphoreSlim  # One validation at a time                │
│                                                                             │
│  THREAD SAFETY                                                              │
│  ─────────────                                                              │
│  _lock: Lock                     # Protects all mutable state              │
└─────────────────────────────────────────────────────────────────────────────┘
         │
         │ wraps (returns cached instances)
         ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Document (immutable)                           │
│  (From Feature 02 - already implemented)                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│  Text: string                    # Document text                            │
│  CursorPosition: int             # Cursor position                          │
│  Selection: SelectionState?      # Optional selection                       │
└─────────────────────────────────────────────────────────────────────────────┘
         │
         │ references
         ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          CompletionState (mutable)                          │
│  (Tracks state during completion)                                           │
├─────────────────────────────────────────────────────────────────────────────┤
│  OriginalDocument: Document      # Document before completion started       │
│  Completions: List<Completion>   # Available completions                    │
│  CompleteIndex: int?             # Selected completion (null = none)        │
├─────────────────────────────────────────────────────────────────────────────┤
│  + GoToIndex(int? index)         # Select a completion                      │
│  + NewTextAndPosition()          # Get (text, cursor) for current selection │
│  + CurrentCompletion: Completion? # Get currently selected completion       │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                          YankNthArgState (mutable)                          │
│  (Tracks Emacs yank-nth-arg/yank-last-arg state)                           │
├─────────────────────────────────────────────────────────────────────────────┤
│  HistoryPosition: int            # Position in history (negative index)     │
│  N: int                          # Argument index to yank                   │
│  PreviousInsertedWord: string    # Word inserted in last yank operation    │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                          ValidationState (enum)                             │
├─────────────────────────────────────────────────────────────────────────────┤
│  Valid = 0                       # Input is valid                           │
│  Invalid = 1                     # Input is invalid                         │
│  Unknown = 2                     # Not yet validated                        │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Entity Definitions

### Buffer

The primary mutable text container that wraps an immutable Document.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| _workingLines | `List<string>` | History entries + current text | Never empty; last entry is current |
| _workingIndex | `int` | Index into _workingLines | 0 ≤ index < _workingLines.Count |
| _cursorPosition | `int` | Current cursor position | 0 ≤ pos ≤ text.Length |
| _undoStack | `List<(string, int)>` | Undo history | (text, cursor_position) tuples |
| _redoStack | `List<(string, int)>` | Redo history | Cleared on new edit |
| _selectionState | `SelectionState?` | Active selection | null = no selection |
| _preferredColumn | `int?` | Column for up/down navigation | Set after vertical movement |
| _documentBeforePaste | `Document?` | For clipboard rotation | Set after paste, cleared on edit |
| _completeState | `CompletionState?` | Active completion | null = no completion active |
| _yankNthArgState | `YankNthArgState?` | Emacs yank state | null = no yank in progress |
| _validationState | `ValidationState` | Validation result | Default: Unknown |
| _validationError | `ValidationError?` | Error if invalid | null if valid or unknown |
| _suggestion | `Suggestion?` | Auto-suggest | null = no suggestion |
| _historySearchText | `string?` | History filter prefix | null = no filter |

**Configuration Properties**:

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| Completer | `ICompleter` | Completion provider | DummyCompleter |
| AutoSuggest | `IAutoSuggest?` | Suggestion provider | null |
| History | `IHistory` | History storage | InMemoryHistory |
| Validator | `IValidator?` | Input validator | null |
| Name | `string` | Buffer identifier | "" |
| TempfileSuffix | `Func<string>` | Editor temp file suffix | () => "" |
| Tempfile | `Func<string>` | Editor temp file path | () => "" |
| AcceptHandler | `Func<Buffer, bool>?` | Accept callback | null |
| TextWidth | `int` | Width for reshaping | 0 (use 80) |
| MaxNumberOfCompletions | `int` | Completion limit | 10000 |

**Filter Properties**:

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| CompleteWhileTyping | `Func<bool>` | Enable async completion | () => false |
| ValidateWhileTyping | `Func<bool>` | Enable async validation | () => false |
| EnableHistorySearch | `Func<bool>` | Enable history prefix filter | () => false |
| ReadOnly | `Func<bool>` | Prevent modifications | () => false |
| Multiline | `Func<bool>` | Allow newlines | () => true |

### CompletionState

Tracks the state of an active completion operation.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| OriginalDocument | `Document` | Document when completion started | Immutable |
| Completions | `List<Completion>` | Available completions | Mutable list |
| CompleteIndex | `int?` | Selected index | null or 0 ≤ index < Count |

**Computed Properties**:

| Property | Type | Description |
|----------|------|-------------|
| CurrentCompletion | `Completion?` | Completions[CompleteIndex] or null |

**Methods**:

| Method | Returns | Description |
|--------|---------|-------------|
| GoToIndex(int?) | void | Set CompleteIndex |
| NewTextAndPosition() | (string, int) | Compute new text and cursor |

### YankNthArgState

Tracks state for Emacs yank-nth-arg and yank-last-arg operations.

| Field | Type | Description | Default |
|-------|------|-------------|---------|
| HistoryPosition | `int` | Position in history (negative) | 0 |
| N | `int` | Argument index | -1 (last) or 1 (first) |
| PreviousInsertedWord | `string` | Previously yanked word | "" |

### ValidationState

Enum representing validation result.

| Value | Description |
|-------|-------------|
| Valid | Input passed validation |
| Invalid | Input failed validation |
| Unknown | Not yet validated |

### EditReadOnlyBufferException

Exception thrown when attempting to modify a read-only buffer.

```csharp
public sealed class EditReadOnlyBufferException : Exception
{
    public EditReadOnlyBufferException()
        : base("Attempt editing of read-only Buffer.") { }
}
```

## State Transitions

### Text Change Triggers

When text changes (via `Text` setter or `Document` setter):
1. Clear `_validationState` → `Unknown`
2. Clear `_validationError` → `null`
3. Clear `_completeState` → `null`
4. Clear `_yankNthArgState` → `null`
5. Clear `_documentBeforePaste` → `null`
6. Clear `_selectionState` → `null`
7. Clear `_suggestion` → `null`
8. Clear `_preferredColumn` → `null`
9. Fire `OnTextChanged` event
10. Reset `_historySearchText` → `null` (only if not traversing history)

### Cursor Position Change Triggers

When cursor position changes:
1. Clear `_completeState` → `null`
2. Clear `_yankNthArgState` → `null`
3. Clear `_documentBeforePaste` → `null`
4. Clear `_preferredColumn` → `null`
5. Fire `OnCursorPositionChanged` event

### Undo/Redo State Machine

```
                    ┌─────────────┐
         edit       │    EDIT     │   push to undo
    ┌───────────────│             │───────────────┐
    │               └─────────────┘               │
    │                      │                      ▼
    │                      │ undo         ┌─────────────┐
    │                      │              │    UNDO     │
    │                      ▼              │   STACK     │
    │               ┌─────────────┐       └─────────────┘
    │               │   UNDONE    │               │
    │               │             │◄──────────────┘
    │               └─────────────┘         undo
    │                      │
    │                      │ redo
    │                      ▼
    │               ┌─────────────┐       ┌─────────────┐
    │               │   REDONE    │       │    REDO     │
    │               │             │◄──────│   STACK     │
    │               └─────────────┘       └─────────────┘
    │                      │
    │                      │ new edit
    └──────────────────────┘
          clears redo stack
```

### History Navigation Flow

```
   _workingLines:  [history_0, history_1, ..., history_n, current_text]
   _workingIndex:  ────────────────────────────────────────────▲
                          ▲                                    │
                          │                                    │
                   history_backward()                   history_forward()
                          │                                    │
                          │                                    │
        ◄─────────────────┴────────────────────────────────────►
```

### Completion State Flow

```
┌────────────┐  start_completion   ┌─────────────────┐
│  NO STATE  │ ──────────────────► │ COMPLETING      │
│            │                     │ (loading...)    │
└────────────┘                     └─────────────────┘
      ▲                                    │
      │                                    │ completions loaded
      │                                    ▼
      │                            ┌─────────────────┐
      │                            │ COMPLETE_STATE  │
      │                            │ (index=null)    │
      │                            └─────────────────┘
      │                                    │
      │                                    │ complete_next/prev
      │                                    ▼
      │                            ┌─────────────────┐
      │ cancel_completion          │ COMPLETE_STATE  │
      └────────────────────────────│ (index=N)       │
                                   └─────────────────┘
                                           │
                                           │ apply_completion
                                           ▼
                                   ┌─────────────────┐
                                   │   APPLIED       │
                                   │   (NO STATE)    │
                                   └─────────────────┘
```

## Validation Rules

### Buffer Constraints

| Rule | Validation |
|------|------------|
| Cursor bounds | `0 ≤ cursorPosition ≤ text.Length` |
| Working index | `0 ≤ workingIndex < workingLines.Count` |
| Working lines | Must have at least one entry (current) |
| Complete index | `null` or `0 ≤ index < completions.Count` |
| Read-only | Throw `EditReadOnlyBufferException` if `ReadOnly()` and edit attempted |

### Automatic Clamping

| Property | Clamping Behavior |
|----------|-------------------|
| CursorPosition | Clamped to `[0, text.Length]` |
| PreferredColumn | Clamped to actual line length during navigation |

## Relationships

```
Buffer ──────────────┬─── wraps ───────► Document (immutable)
                     │
                     ├─── has ─────────► CompletionState?
                     │                         │
                     │                         └─── references ───► Document
                     │                         └─── contains ─────► List<Completion>
                     │
                     ├─── has ─────────► YankNthArgState?
                     │
                     ├─── has ─────────► SelectionState?
                     │
                     ├─── has ─────────► ValidationState
                     │                   ValidationError?
                     │
                     ├─── has ─────────► Suggestion?
                     │
                     ├─── uses ────────► ICompleter
                     │
                     ├─── uses ────────► IAutoSuggest?
                     │
                     ├─── uses ────────► IHistory
                     │
                     └─── uses ────────► IValidator?
```
