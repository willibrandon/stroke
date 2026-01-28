# Research: Editing Modes and State

**Feature**: 023-editing-modes-state
**Date**: 2026-01-27

## Research Summary

All technical context items resolved. No NEEDS CLARIFICATION markers remain.

---

## 1. Python Prompt Toolkit Source Analysis

### Decision
Port directly from three Python source files with exact API fidelity.

### Source Files Analyzed

**`prompt_toolkit/enums.py`**:
- `EditingMode` enum: `VI = "VI"`, `EMACS = "EMACS"`
- Constants: `SEARCH_BUFFER`, `DEFAULT_BUFFER`, `SYSTEM_BUFFER`

**`prompt_toolkit/key_binding/vi_state.py`**:
- `InputMode` enum: `INSERT`, `INSERT_MULTIPLE`, `NAVIGATION`, `REPLACE`, `REPLACE_SINGLE`
- `CharacterFind` class: stores `character` (str) and `backwards` (bool)
- `ViState` class: mutable state container with:
  - `input_mode` property with side effects on Navigation transition
  - `last_character_find`, `operator_func`, `operator_arg`
  - `named_registers` (dict mapping str to ClipboardData)
  - `waiting_for_digraph`, `digraph_symbol1`
  - `tilde_operator`
  - `recording_register`, `current_recording`
  - `temporary_navigation_mode`
  - `reset()` method

**`prompt_toolkit/key_binding/emacs_state.py`**:
- `EmacsState` class: mutable state container with:
  - `macro` (list of KeyPress or None)
  - `current_recording` (list of KeyPress or None)
  - `is_recording` property
  - `start_macro()`, `end_macro()`, `reset()` methods

### Rationale
100% API fidelity required per Constitution I. Direct port with naming convention adjustments only.

### Alternatives Considered
- **Inventing additional APIs**: Rejected per Constitution I (forbidden behavior)
- **Merging state classes**: Rejected to maintain 1:1 Python structure

---

## 2. Thread Safety Strategy

### Decision
Use `System.Threading.Lock` with `EnterScope()` pattern for all mutable state access.

### Implementation Pattern
```csharp
public sealed class ViState
{
    private readonly Lock _lock = new();
    private InputMode _inputMode = InputMode.Insert;

    public InputMode InputMode
    {
        get { using (_lock.EnterScope()) return _inputMode; }
        set
        {
            using (_lock.EnterScope())
            {
                if (value == InputMode.Navigation)
                {
                    // Clear state on Navigation mode transition
                    _waitingForDigraph = false;
                    _operatorFunc = null;
                    _operatorArg = null;
                }
                _inputMode = value;
            }
        }
    }
}
```

### Rationale
Constitution XI mandates thread safety for all mutable state. `Lock` type (.NET 9+) provides lightweight synchronization with `EnterScope()` pattern for automatic release.

### Alternatives Considered
- **`lock` keyword with object**: Works but `Lock` type is preferred per Constitution XI
- **ReaderWriterLockSlim**: Overkill for simple state; adds complexity without benefit
- **Interlocked for atomic primitives**: Insufficient for compound state transitions

---

## 3. Type Mappings

### Decision
Use established Stroke types for dependencies.

### Mappings

| Python Type | C# Type | Location |
|-------------|---------|----------|
| `KeyPress` | `Stroke.Input.KeyPress` | Already exists in `Input/KeyPress.cs` |
| `ClipboardData` | `Stroke.Clipboard.ClipboardData` | Already exists in `Clipboard/ClipboardData.cs` |
| `KeyPressEvent` | `Stroke.KeyBinding.KeyPressEvent` | Already exists in `KeyBinding/KeyPressEvent.cs` |
| `TextObject` | Placeholder delegate | Future feature; use `Func<KeyPressEvent, object?, NotImplementedOrNone>` |

### Rationale
Reuse existing types per layered architecture. TextObject interface not yet implemented (out of scope), so OperatorFunc uses generic delegate.

### Alternatives Considered
- **Define ITextObject interface now**: Rejected as out of scope per spec
- **Use object for OperatorFunc**: Less type-safe; delegate provides clear signature

---

## 4. Operator Function Signature

### Decision
```csharp
public delegate NotImplementedOrNone OperatorFuncDelegate(KeyPressEvent e, object? textObject);
```

### Rationale
Python uses `Callable[[KeyPressEvent, TextObject], None]`. Since `TextObject` interface is not yet defined (future feature per spec), use `object?` as placeholder. Return type uses existing `NotImplementedOrNone` for consistency with key binding system.

### Alternatives Considered
- **Action<KeyPressEvent, object?>**: Doesn't communicate success/failure
- **Func<...>**: Works but custom delegate provides semantic clarity

---

## 5. EmacsState Macro Storage

### Decision
Store macros as `List<KeyPress>` using the Input namespace's `KeyPress` type.

### Python Behavior
- `macro`: `list[KeyPress] | None` - initialized to empty list `[]`
- `current_recording`: `list[KeyPress] | None` - None when not recording
- `end_macro()`: copies `current_recording` to `macro`, sets `current_recording` to None

### C# Implementation
```csharp
public List<KeyPress>? Macro { get; private set; } = new();
public List<KeyPress>? CurrentRecording { get; private set; }
```

### Rationale
Direct port of Python behavior. Empty list for `Macro` matches Python's `[]` initialization.

### Alternatives Considered
- **ImmutableList<KeyPress>**: Adds complexity; Python uses mutable list
- **IReadOnlyList<KeyPress>**: Would require copying on access; performance concern

---

## 6. ViState Recording Storage

### Decision
Store macro recordings as `string` for Vi (different from Emacs).

### Python Behavior
```python
self.recording_register: str | None = None  # Register name or None
self.current_recording: str = ""  # Accumulated macro data
```

### Rationale
Vi records macros as raw string data (keystrokes), not structured `KeyPress` objects like Emacs. This matches Python Prompt Toolkit's design.

### Alternatives Considered
- **Use List<KeyPress> like Emacs**: Incorrect; Python uses string for Vi macros

---

## 7. Named Registers Dictionary

### Decision
Use `Dictionary<string, ClipboardData>` with thread-safe access via Lock.

### Implementation
```csharp
private readonly Dictionary<string, ClipboardData> _namedRegisters = new();

public ClipboardData? GetNamedRegister(string name)
{
    using (_lock.EnterScope())
    {
        return _namedRegisters.TryGetValue(name, out var data) ? data : null;
    }
}

public void SetNamedRegister(string name, ClipboardData data)
{
    using (_lock.EnterScope())
    {
        _namedRegisters[name] = data;
    }
}
```

### Rationale
Python exposes `named_registers` as public dict. C# should provide thread-safe accessor methods rather than exposing mutable collection directly.

### Alternatives Considered
- **ConcurrentDictionary**: Overkill when Lock already protects all state
- **Expose IReadOnlyDictionary**: Would still require copying for thread safety

---

## 8. Enum Value Mapping

### Decision
Use C# enum with string values matching Python exactly.

### EditingMode
```csharp
public enum EditingMode
{
    Vi,    // Python: "VI"
    Emacs  // Python: "EMACS"
}
```

### InputMode
```csharp
public enum InputMode
{
    Insert,          // Python: "vi-insert"
    InsertMultiple,  // Python: "vi-insert-multiple"
    Navigation,      // Python: "vi-navigation"
    Replace,         // Python: "vi-replace"
    ReplaceSingle    // Python: "vi-replace-single"
}
```

### Rationale
Python enum values are strings for serialization/display. C# enums use PascalCase names per Constitution I naming conventions. If string values are needed, use `ToString()` or extension methods.

### Alternatives Considered
- **Store string value in enum**: C# enums are integers; would need attribute or extension
- **Use string constants**: Loses type safety of enum

---

## Best Practices Summary

1. **Thread Safety**: All mutable state protected by single `Lock` instance per class
2. **Immutability**: `CharacterFind` is immutable (record or readonly struct)
3. **API Fidelity**: Exact 1:1 mapping with Python APIs, PascalCase naming only change
4. **Documentation**: XML docs on all public types and members
5. **Testing**: Concurrent stress tests with 10+ threads per Constitution XI
