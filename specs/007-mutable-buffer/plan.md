# Implementation Plan: Buffer (Mutable Text Container)

**Branch**: `007-mutable-buffer` | **Date**: 2026-01-24 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/007-mutable-buffer/spec.md`

## Summary

Implement the Buffer class as a mutable text container that wraps an immutable Document, providing text editing operations (insert, delete, cursor movement), undo/redo with (text, cursor_position) tuples, history navigation via working_lines deque, selection/clipboard operations with three paste modes (Emacs, Vi-before, Vi-after), completion state management, validation, auto-suggest integration, and thread-safe operations per Constitution XI.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (Stroke.Core layer - zero external dependencies per Constitution III)
**Storage**: N/A (in-memory only - undo stack, redo stack, working_lines)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Cross-platform (.NET 10 - Linux, macOS, Windows 10+)
**Project Type**: Library (class library in Stroke.Core namespace)
**Performance Goals**: Document caching via FastDictCache (size=10), lazy property evaluation, minimal allocations for common operations
**Constraints**: Thread-safe per Constitution XI; all mutable state requires synchronization; file size ≤1000 LOC per Constitution X
**Scale/Scope**: 35+ methods on Buffer class; 5 supporting types (CompletionState, YankNthArgState, ValidationState, EditReadOnlyBufferException, BufferOperations)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | All 35+ Python Buffer methods mapped 1:1 in api-mapping.md; CompletionState, YankNthArgState, ValidationState, EditReadOnlyBuffer ported exactly |
| II. Immutability by Default | ✅ PASS | Buffer wraps immutable Document; CompletionState is immutable; ClipboardData immutable; ValidationState is enum |
| III. Layered Architecture | ✅ PASS | Buffer lives in Stroke.Core; depends only on Document, SelectionState, ClipboardData (all Stroke.Core or Stroke.Clipboard which is peer) |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | No platform-specific code; external editor uses VISUAL/EDITOR env vars with cross-platform fallbacks |
| V. Complete Editing Mode Parity | ✅ PASS | PasteMode supports Emacs, Vi-before, Vi-after; selection supports Characters, Lines, Block; yank-nth-arg/yank-last-arg Emacs ops |
| VI. Performance-Conscious Design | ✅ PASS | FastDictCache for Document instances; lazy evaluation in Document; no global mutable state |
| VII. Full Scope Commitment | ✅ PASS | All 36 functional requirements will be implemented; all 12 user stories will be addressed |
| VIII. Real-World Testing | ✅ PASS | Tests use real Document, SelectionState, ClipboardData instances; no mocks |
| IX. Adherence to Planning Documents | ✅ PASS | api-mapping.md Buffer section consulted; all mapped APIs will be implemented |
| X. Source Code File Size Limits | ✅ PASS | Buffer class split into partial files: Buffer.cs, Buffer.Editing.cs, Buffer.History.cs, Buffer.Completion.cs, Buffer.Selection.cs, Buffer.Search.cs |
| XI. Thread Safety by Default | ✅ PASS | All mutable state (working_lines, cursor_position, undo/redo stacks, completion_state, etc.) protected by Lock |

## Project Structure

### Documentation (this feature)

```text
specs/007-mutable-buffer/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 research output
├── data-model.md        # Phase 1 data model
├── quickstart.md        # Phase 1 quickstart guide
├── contracts/           # Phase 1 API contracts
│   └── buffer-api.md    # Buffer API contract
├── checklists/
│   └── requirements.md  # Requirements checklist
└── tasks.md             # Phase 2 task list (from /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/
├── Core/
│   ├── Buffer.cs                    # Main Buffer class (constructor, properties, events)
│   ├── Buffer.Editing.cs            # Text editing methods (insert, delete, transform)
│   ├── Buffer.Navigation.cs         # Cursor movement (left, right, up, down, auto_up/down)
│   ├── Buffer.History.cs            # History navigation (forward, backward, yank-nth-arg)
│   ├── Buffer.Completion.cs         # Completion state management
│   ├── Buffer.Selection.cs          # Selection and clipboard operations
│   ├── Buffer.Search.cs             # Search operations
│   ├── Buffer.UndoRedo.cs           # Undo/redo stack management
│   ├── Buffer.Validation.cs         # Validation support
│   ├── Buffer.ExternalEditor.cs     # Open in external editor
│   ├── CompletionState.cs           # Immutable completion state
│   ├── YankNthArgState.cs           # Yank-nth-arg tracking
│   ├── ValidationState.cs           # Validation state enum
│   ├── EditReadOnlyBufferException.cs # Exception for read-only edits
│   └── BufferOperations.cs          # Static indent/unindent/reshape_text functions
└── Validation/
    └── IValidator.cs                # Stub interface (full impl in Feature 09)

tests/Stroke.Tests/
├── Core/
│   ├── BufferTests.cs               # Basic Buffer tests
│   ├── BufferEditingTests.cs        # Text editing tests
│   ├── BufferNavigationTests.cs     # Cursor movement tests
│   ├── BufferHistoryTests.cs        # History navigation tests
│   ├── BufferCompletionTests.cs     # Completion tests
│   ├── BufferSelectionTests.cs      # Selection/clipboard tests
│   ├── BufferSearchTests.cs         # Search tests
│   ├── BufferUndoRedoTests.cs       # Undo/redo tests
│   ├── BufferValidationTests.cs     # Validation tests
│   ├── BufferThreadSafetyTests.cs   # Concurrent access tests
│   ├── CompletionStateTests.cs      # CompletionState tests
│   ├── YankNthArgStateTests.cs      # YankNthArgState tests
│   ├── ValidationStateTests.cs      # ValidationState enum tests
│   └── BufferOperationsTests.cs     # indent/unindent/reshape tests
```

**Structure Decision**: Single project structure in `src/Stroke/` with Buffer implementation split across multiple partial class files to stay under 1000 LOC per file (Constitution X). Tests organized by functional area in `tests/Stroke.Tests/Core/`.

## Complexity Tracking

> **No Constitution violations requiring justification.**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |

## Dependencies

### Already Implemented (Features 01-06)

| Feature | Types Used |
|---------|------------|
| 001 | Point, Size, WritePosition |
| 002 | Document (immutable text container) |
| 003 | SelectionState, SelectionType, PasteMode |
| 004 | ClipboardData, IClipboard |
| 005 | Suggestion, IAutoSuggest |
| 006 | FastDictCache, SimpleCache, Memoization |

### Stub Interfaces (Implemented as stubs, full impl in later features)

| Interface | Feature | Stub Notes |
|-----------|---------|------------|
| IHistory | 07 | Already exists with `GetStrings()` method |
| ICompleter | 08 | Needs stub for `GetCompletions`, `GetCompletionsAsync` |
| Completion | 08 | Needs stub record with `Text`, `StartPosition`, `Display`, `DisplayMeta`, `Style`, `SelectedStyle` |
| CompleteEvent | 08 | Needs stub record with `TextInserted`, `CompletionRequested` |
| IValidator | 09 | Needs stub with `Validate`, `ValidateAsync` |
| ValidationError | 09 | Needs stub class with `CursorPosition`, `Message` |
| SearchState | 10 | Needs stub with `Text`, `Direction`, `IgnoreCase` |
| SearchDirection | 10 | Needs stub enum with `Forward`, `Backward` |

## Implementation Strategy

### File Size Management (Constitution X)

The Python `buffer.py` is 2030 lines. To stay under 1000 LOC per file, Buffer will be split into partial classes:

| File | Responsibility | Est. LOC |
|------|----------------|----------|
| Buffer.cs | Constructor, properties, events, Document cache | ~250 |
| Buffer.Editing.cs | InsertText, Delete, DeleteBeforeCursor, Transform methods | ~200 |
| Buffer.Navigation.cs | CursorLeft/Right/Up/Down, AutoUp/AutoDown | ~150 |
| Buffer.History.cs | HistoryForward/Backward, YankNthArg/YankLastArg, working_lines | ~200 |
| Buffer.Completion.cs | StartCompletion, CompleteNext/Prev, GoToCompletion, Apply, Cancel | ~250 |
| Buffer.Selection.cs | StartSelection, CopySelection, CutSelection, PasteClipboardData | ~150 |
| Buffer.Search.cs | DocumentForSearch, GetSearchPosition, ApplySearch | ~200 |
| Buffer.UndoRedo.cs | SaveToUndoStack, Undo, Redo | ~100 |
| Buffer.Validation.cs | Validate, ValidateAsync, ValidationState | ~100 |
| Buffer.ExternalEditor.cs | OpenInEditor, tempfile handling | ~150 |

### Thread Safety (Constitution XI)

All mutable fields protected by `System.Threading.Lock`:

```csharp
private readonly Lock _lock = new();

// Protected fields:
private int _cursorPosition;
private readonly List<(string Text, int CursorPosition)> _undoStack = [];
private readonly List<(string Text, int CursorPosition)> _redoStack = [];
private readonly List<string> _workingLines = [];
private int _workingIndex;
private CompletionState? _completeState;
private SelectionState? _selectionState;
private ValidationState? _validationState;
private ValidationError? _validationError;
private Suggestion? _suggestion;
private YankNthArgState? _yankNthArgState;
private Document? _documentBeforePaste;
private string? _historySearchText;
private int? _preferredColumn;
```

### Event System

Events implemented using C# events (delegates):

```csharp
public event Action<Buffer>? OnTextChanged;
public event Action<Buffer>? OnTextInsert;
public event Action<Buffer>? OnCursorPositionChanged;
public event Action<Buffer>? OnCompletionsChanged;
public event Action<Buffer>? OnSuggestionSet;
```

### Async Operations Pattern

Python's `_only_one_at_a_time` decorator maps to a semaphore-based pattern:

```csharp
private readonly SemaphoreSlim _asyncCompletionLock = new(1, 1);
private readonly SemaphoreSlim _asyncSuggestionLock = new(1, 1);
private readonly SemaphoreSlim _asyncValidationLock = new(1, 1);
```

### Filter Parameters

Filter parameters accept both `bool` and `Func<bool>`:

```csharp
public Func<bool> CompleteWhileTyping { get; }
public Func<bool> ValidateWhileTyping { get; }
public Func<bool> EnableHistorySearch { get; }
public Func<bool> ReadOnly { get; }
public Func<bool> Multiline { get; }
```

Constructor overloads accept either:
```csharp
public Buffer(bool completeWhileTyping = false, ...)
public Buffer(Func<bool>? completeWhileTyping = null, ...)
```
