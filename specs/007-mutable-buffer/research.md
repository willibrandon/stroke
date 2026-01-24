# Research: Buffer (Mutable Text Container)

**Date**: 2026-01-24
**Feature**: 007-mutable-buffer

## Research Tasks

### 1. Thread Safety Strategy for Mutable Buffer State

**Decision**: Use `System.Threading.Lock` (.NET 9+) with `EnterScope()` pattern.

**Rationale**: Constitution XI mandates thread safety for all mutable state. The `Lock` type is the modern .NET synchronization primitive with better performance than `object` locks. The `EnterScope()` pattern provides automatic release via `IDisposable`.

**Alternatives Considered**:
- `ReaderWriterLockSlim`: Rejected - Buffer operations are mostly read-modify-write, not read-heavy
- `Monitor`: Rejected - `Lock` is the modern equivalent with better ergonomics
- Lock-free with `Interlocked`: Rejected - Complex compound operations (text + cursor position) require atomicity

**Implementation Pattern**:
```csharp
private readonly Lock _lock = new();

public string Text
{
    get
    {
        using (_lock.EnterScope())
        {
            return _workingLines[_workingIndex];
        }
    }
    set
    {
        using (_lock.EnterScope())
        {
            // Atomic text + cursor position update
        }
    }
}
```

### 2. Async Completion/Suggestion/Validation Pattern

**Decision**: Use `SemaphoreSlim(1, 1)` to ensure only one async operation runs at a time per type, with retry-on-document-change semantics.

**Rationale**: Python Prompt Toolkit uses the `_only_one_at_a_time` decorator pattern to ensure only one completion/suggestion/validation runs at a time. When the document changes during an operation, the operation restarts.

**Alternatives Considered**:
- `CancellationToken` only: Rejected - Need retry semantics, not just cancellation
- Channel-based queuing: Rejected - Over-engineered; Python just ignores new requests while one is running

**Implementation Pattern**:
```csharp
private readonly SemaphoreSlim _completionLock = new(1, 1);
private bool _completionRetryRequested;

private async Task RunCompletionAsync()
{
    if (!await _completionLock.WaitAsync(0))
        return; // Another completion is running

    try
    {
        while (true)
        {
            _completionRetryRequested = false;
            var document = Document;

            // Do completion work...

            if (!_completionRetryRequested && Document == document)
                break;
        }
    }
    finally
    {
        _completionLock.Release();
    }
}
```

### 3. Working Lines Storage (History + Current)

**Decision**: Use `List<string>` with index management, not `Deque<string>`.

**Rationale**: Python uses `deque` for efficient left-side insertions (prepending history). In C#, `List<string>` with history at index 0-N and current at end provides equivalent functionality. History loading prepends by inserting at index 0 (O(n) but history loading is async and infrequent).

**Alternatives Considered**:
- `LinkedList<string>`: Rejected - Slower iteration, no index access
- Custom deque implementation: Rejected - Unnecessary complexity; List insertions are acceptable for history loading

**Data Structure**:
```
Index 0:   oldest history entry
Index 1:   second oldest history entry
...
Index N-1: most recent history entry
Index N:   current input (working text)
```

### 4. Event System Design

**Decision**: Use C# `event Action<Buffer>?` delegates matching Python's `Event[Buffer]` pattern.

**Rationale**: Python Prompt Toolkit's `Event` class allows multiple handlers to be registered. C# events provide the same semantics with native language support.

**Alternatives Considered**:
- Custom `Event<T>` class: Rejected - C# events provide native support
- `IObservable<T>`: Rejected - Over-engineered; simple notification pattern is sufficient

**Events**:
```csharp
public event Action<Buffer>? OnTextChanged;
public event Action<Buffer>? OnTextInsert;
public event Action<Buffer>? OnCursorPositionChanged;
public event Action<Buffer>? OnCompletionsChanged;
public event Action<Buffer>? OnSuggestionSet;
```

### 5. Filter Parameter Design (bool vs Func<bool>)

**Decision**: Store as `Func<bool>` internally, provide implicit conversion from `bool`.

**Rationale**: Python Prompt Toolkit's `FilterOrBool` allows both constant booleans and dynamic filters. In C#, we can achieve this by storing `Func<bool>` and providing an implicit operator for bool values.

**Alternatives Considered**:
- Separate `bool` and `Func<bool>` overloads: Rejected - API bloat
- Union type: Not available in C#

**Implementation**:
```csharp
public sealed class FilterOrBool
{
    private readonly Func<bool> _filter;

    public FilterOrBool(bool value) => _filter = () => value;
    public FilterOrBool(Func<bool> filter) => _filter = filter;

    public static implicit operator FilterOrBool(bool value) => new(value);
    public static implicit operator FilterOrBool(Func<bool> filter) => new(filter);

    public bool Invoke() => _filter();
}
```

Or simply use `Func<bool>` with extension method:
```csharp
// Usage: completeWhileTyping: true or completeWhileTyping: () => someCondition
```

### 6. Document Cache Strategy

**Decision**: Use `FastDictCache<(string, int, SelectionState?), Document>` with size=10.

**Rationale**: Python Prompt Toolkit caches Document instances to avoid repeated allocations. FastDictCache (Feature 06) provides this functionality.

**Cache Key**: Tuple of (text, cursorPosition, selectionState) uniquely identifies a Document.

**Implementation**:
```csharp
private readonly FastDictCache<(string Text, int CursorPosition, SelectionState? Selection), Document>
    _documentCache = new(10);

public Document Document => _documentCache.Get(
    (Text, CursorPosition, SelectionState),
    key => new Document(key.Text, key.CursorPosition, key.Selection)
);
```

### 7. Undo/Redo Stack Design

**Decision**: Use `List<(string Text, int CursorPosition)>` for both stacks.

**Rationale**: Python stores `(text, cursor_position)` tuples. C# value tuples provide equivalent functionality with good performance.

**Behavior**:
- `SaveToUndoStack`: If top of stack has same text, update cursor position; otherwise push new entry
- `Undo`: Pop from undo stack, push current to redo stack, restore state
- `Redo`: Pop from redo stack, push current to undo stack, restore state
- New edit: Clear redo stack

### 8. CompletionState Immutability

**Decision**: CompletionState is mutable (despite Python docstring saying "immutable").

**Rationale**: Python's CompletionState has a `go_to_index` method that mutates `complete_index`. The docstring says "immutable" but the implementation is mutable. We follow the actual behavior.

**Implementation**:
```csharp
public sealed class CompletionState
{
    public Document OriginalDocument { get; }
    public IReadOnlyList<Completion> Completions { get; }
    public int? CompleteIndex { get; private set; }

    public void GoToIndex(int? index)
    {
        if (Completions.Count > 0)
        {
            Debug.Assert(index is null || (index >= 0 && index < Completions.Count));
            CompleteIndex = index;
        }
    }

    public (string NewText, int NewCursorPosition) NewTextAndPosition()
    {
        // Implementation...
    }
}
```

### 9. External Editor Integration

**Decision**: Use `Process.Start` with VISUAL/EDITOR environment variables, fallback to common editor paths.

**Rationale**: Cross-platform editor detection using environment variables is standard Unix practice. Windows users typically set EDITOR if they want this feature.

**Fallback Order**:
1. `VISUAL` environment variable
2. `EDITOR` environment variable
3. `/usr/bin/editor` (Debian alternatives)
4. `/usr/bin/nano`
5. `/usr/bin/pico`
6. `/usr/bin/vi`
7. `/usr/bin/emacs`

### 10. Stub Interface Strategy

**Decision**: Create minimal stub interfaces in Stroke.Core or appropriate namespace for types not yet implemented.

**Stubs Needed**:
- `Stroke.Completion.ICompleter` - empty interface with GetCompletions/GetCompletionsAsync stubs
- `Stroke.Completion.Completion` - record with required properties
- `Stroke.Completion.CompleteEvent` - record with TextInserted, CompletionRequested
- `Stroke.Validation.IValidator` - interface with Validate/ValidateAsync
- `Stroke.Validation.ValidationError` - class with CursorPosition, Message
- `Stroke.Core.SearchState` - class with Text, Direction, IgnoreCase
- `Stroke.Core.SearchDirection` - enum Forward, Backward

Note: `IHistory` already exists in `Stroke.History` with `GetStrings()` method. Need to add `AppendString(string)` method stub.

## Resolved Clarifications

All technical decisions have been made. No items remain as NEEDS CLARIFICATION.
