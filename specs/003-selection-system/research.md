# Research: Selection System

**Feature**: 003-selection-system
**Date**: 2026-01-23

## Python Source Analysis

**Source File**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/selection.py`

### Public API Inventory

| Python | C# Equivalent | Status |
|--------|---------------|--------|
| `SelectionType` enum | `SelectionType` enum | ✅ Implemented |
| `SelectionType.CHARACTERS` | `SelectionType.Characters` | ✅ Implemented |
| `SelectionType.LINES` | `SelectionType.Lines` | ✅ Implemented |
| `SelectionType.BLOCK` | `SelectionType.Block` | ✅ Implemented |
| `PasteMode` enum | `PasteMode` enum | ✅ Implemented |
| `PasteMode.EMACS` | `PasteMode.Emacs` | ✅ Implemented |
| `PasteMode.VI_AFTER` | `PasteMode.ViAfter` | ✅ Implemented |
| `PasteMode.VI_BEFORE` | `PasteMode.ViBefore` | ✅ Implemented |
| `SelectionState` class | `SelectionState` class | ✅ Implemented |
| `SelectionState.__init__()` | `SelectionState()` constructor | ✅ Implemented |
| `SelectionState.original_cursor_position` | `SelectionState.OriginalCursorPosition` | ✅ Implemented |
| `SelectionState.type` | `SelectionState.Type` | ✅ Implemented |
| `SelectionState.shift_mode` | `SelectionState.ShiftMode` | ✅ Implemented |
| `SelectionState.enter_shift_mode()` | `SelectionState.EnterShiftMode()` | ✅ Implemented |
| `SelectionState.__repr__()` | `SelectionState.ToString()` | ⚠️ Missing |

### Decisions

#### Decision 1: ToString() Format

**Decision**: Use C# naming conventions in ToString() output

**Rationale**: Python's `__repr__` uses snake_case (`original_cursor_position`) while C# conventions require PascalCase (`OriginalCursorPosition`). The format serves the same debugging purpose in both languages.

**Alternatives Considered**:
1. Match Python's exact snake_case format - Rejected (violates C# conventions, confusing for C# developers)
2. Omit ToString() entirely - Rejected (violates FR-007, reduces debuggability)

#### Decision 2: Enum String Representation

**Decision**: Accept C# enum default ToString() behavior

**Rationale**: Python's enum `__repr__` shows `<SelectionType.LINES: 'LINES'>` but C#'s enum ToString() shows `Lines`. This is acceptable per Constitution I (C# language constraints).

**Alternatives Considered**:
1. Custom ToString() on enums to match Python format - Rejected (over-engineering for debugging output)

## Best Practices Applied

1. **Sealed class**: `SelectionState` is sealed (not designed for inheritance)
2. **XML documentation**: All types have `///` doc comments
3. **Immutable where possible**: `OriginalCursorPosition` and `Type` are read-only
4. **Mutable where needed**: `ShiftMode` can be changed via `EnterShiftMode()`

## Dependencies

None. These are pure value types in Stroke.Core with no external dependencies.

## Integration Points

The selection types will be used by:
- `Buffer` class (to track active selection state)
- `Document.GetSelectionTuples()` method (already exists)
- Vi visual mode bindings (future implementation)
- Clipboard operations (future implementation)
