# Research: Open in Editor Bindings

**Feature**: 041-open-in-editor-bindings
**Date**: 2026-01-31

## Research Tasks

### R1: Existing Infrastructure Availability

**Question**: What existing infrastructure is already implemented that this feature depends on?

**Finding**: All infrastructure is already in place:

| Dependency | Status | Location |
|-----------|--------|----------|
| `Buffer.OpenInEditorAsync` | Implemented | `src/Stroke/Core/Buffer.ExternalEditor.cs` |
| `edit-and-execute-command` named command | Registered | `src/Stroke/KeyBinding/Bindings/NamedCommands.Misc.cs:22,115-120` |
| `NamedCommands.GetByName` | Implemented | `src/Stroke/KeyBinding/Bindings/NamedCommands.cs` |
| `KeyBindings.Add` | Implemented | `src/Stroke/KeyBinding/KeyBindings.cs` |
| `MergedKeyBindings` | Implemented | `src/Stroke/KeyBinding/MergedKeyBindings.cs` |
| `EmacsFilters.EmacsMode` | Implemented | `src/Stroke/Application/EmacsFilters.cs` |
| `ViFilters.ViNavigationMode` | Implemented | `src/Stroke/Application/ViFilters.cs` |
| `AppFilters.HasSelection` | Implemented | `src/Stroke/Application/AppFilters.cs` |

**Decision**: No new infrastructure needed. Feature is purely a binding registration layer.
**Rationale**: Buffer editing, editor resolution, temp file management, and the named command are all handled by `Buffer.ExternalEditor.cs` and `NamedCommands.Misc.cs`.

### R2: Binding Pattern for Emacs + Vi + Combined Loaders

**Question**: What is the established pattern for binding loaders that have Emacs, Vi, and combined variants?

**Finding**: `PageNavigationBindings.cs` is the closest precedent:

```csharp
// Combined: merges Emacs + Vi
public static IKeyBindingsBase LoadPageNavigationBindings()
{
    return new ConditionalKeyBindings(
        new MergedKeyBindings(
            LoadEmacsPageNavigationBindings(),
            LoadViPageNavigationBindings()
        ),
        AppFilters.BufferHasFocus
    );
}

// Emacs: wraps bindings in ConditionalKeyBindings with EmacsMode filter
public static IKeyBindingsBase LoadEmacsPageNavigationBindings()
{
    var kb = new KeyBindings();
    kb.Add<KeyHandlerCallable>([keys])(handler);
    return new ConditionalKeyBindings(kb, EmacsFilters.EmacsMode);
}

// Vi: wraps bindings in ConditionalKeyBindings with ViMode filter
public static IKeyBindingsBase LoadViPageNavigationBindings()
{
    var kb = new KeyBindings();
    kb.Add<KeyHandlerCallable>([keys])(handler);
    return new ConditionalKeyBindings(kb, ViFilters.ViMode);
}
```

**Decision**: Follow the `PageNavigationBindings` pattern but with key differences matching Python source:
1. The Python source applies `emacs_mode & ~has_selection` as a filter *on the individual binding*, not as a `ConditionalKeyBindings` wrapper. The Emacs loader uses `filter:` parameter on `Add`, matching `BasicBindings.cs` pattern.
2. The Python combined loader uses `merge_key_bindings` directly — no outer `ConditionalKeyBindings`. The PageNavigationBindings wraps with `BufferHasFocus` but the Python `open_in_editor.py` does not.
3. Python returns `KeyBindings` from Emacs/Vi loaders and `KeyBindingsBase` from combined loader.

**Rationale**: Faithful port requires matching Python's exact filter application strategy.

### R3: Filter Composition for Emacs Binding

**Question**: How to express `emacs_mode & ~has_selection` in Stroke's filter system?

**Finding**: The Stroke filter system supports:
- `IFilter.And(IFilter)` or `filter1 & filter2` operator for AND composition
- `IFilter.Invert()` or `~filter` operator for NOT
- `FilterOrBool` wrapper for passing to `KeyBindings.Add`
- `AppFilters.HasSelection` is the equivalent of Python's `has_selection`
- `EmacsFilters.EmacsMode` is the equivalent of Python's `emacs_mode`

Example from `BasicBindings.cs`:
```csharp
var notHasSelection = new FilterOrBool(AppFilters.HasSelection.Invert());
kb.Add<Binding>([new KeyOrChar(Keys.PageUp)], filter: notHasSelection)(handler);
```

**Decision**: Use `EmacsFilters.EmacsMode` AND `AppFilters.HasSelection.Invert()` composed via filter operators, passed as `FilterOrBool` to `kb.Add`.
**Rationale**: Matches Python's `emacs_mode & ~has_selection` directly.

### R4: Key Sequence for Ctrl-X Ctrl-E

**Question**: How to register a multi-key sequence (Ctrl-X followed by Ctrl-E) in Stroke?

**Finding**: Multi-key sequences are registered as arrays of `KeyOrChar`:
```csharp
// From PageNavigationBindings.cs — Esc followed by 'v':
kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Escape), new KeyOrChar('v')])(handler);
```

For Ctrl-X Ctrl-E, the keys are `Keys.ControlX` and `Keys.ControlE`:
```csharp
kb.Add<Binding>([new KeyOrChar(Keys.ControlX), new KeyOrChar(Keys.ControlE)], filter: ...)(handler);
```

**Decision**: Use `[new KeyOrChar(Keys.ControlX), new KeyOrChar(Keys.ControlE)]` for the Emacs binding.
**Rationale**: Matches Python's `key_bindings.add("c-x", "c-e", filter=...)` directly.

### R5: Named Command Handler Type

**Question**: What type does `NamedCommands.GetByName` return and how is it used in binding registration?

**Finding**: `GetByName` returns a `Binding` object. Existing patterns use it with `Add<Binding>`:
```csharp
// From BasicBindings.cs:
kb.Add<Binding>([new KeyOrChar(Keys.Home)])(
    NamedCommands.GetByName("beginning-of-line"));
```

The Python source uses:
```python
key_bindings.add("c-x", "c-e", filter=emacs_mode & ~has_selection)(
    get_by_name("edit-and-execute-command")
)
```

**Decision**: Use `kb.Add<Binding>([keys], filter: ...)(NamedCommands.GetByName("edit-and-execute-command"))`.
**Rationale**: Matches both the Python source pattern and the existing Stroke `BasicBindings` pattern.

### R6: Return Types for Loaders

**Question**: What should the return types be for each loader function?

**Finding**: The api-mapping.md specifies:
- `LoadOpenInEditorBindings()` → `IKeyBindingsBase` (combined returns merged type)
- `LoadEmacsOpenInEditorBindings()` → `KeyBindings`
- `LoadViOpenInEditorBindings()` → `KeyBindings`

However, looking at the Python source:
- `load_open_in_editor_bindings()` returns `KeyBindingsBase` (via `merge_key_bindings`)
- `load_emacs_open_in_editor_bindings()` returns `KeyBindings`
- `load_vi_open_in_editor_bindings()` returns `KeyBindings`

The Python source does NOT wrap individual Emacs/Vi loaders in a conditional. The filter is applied *per-binding* via the `filter=` parameter on `.add()`.

**Decision**: Return `KeyBindings` from Emacs/Vi loaders (filter applied per-binding), `IKeyBindingsBase` from combined loader (via `MergedKeyBindings`).
**Rationale**: Matches both api-mapping.md and Python source return types exactly.

### R7: Spec Assumption Validation

**Question**: Are all 6 assumptions from spec.md validated with concrete evidence?

**Finding**: Each assumption verified against codebase:

| Spec Assumption | Validated | Evidence |
|----------------|-----------|----------|
| 1. `NamedCommands` registry supports `GetByName` | Yes | `src/Stroke/KeyBinding/Bindings/NamedCommands.cs` — `public static Binding GetByName(string name)` |
| 2. `KeyBindings.Add` with key sequences, handlers, filters | Yes | `src/Stroke/KeyBinding/KeyBindings.cs:76` — `public Func<T, T> Add<T>(KeyOrChar[] keys, FilterOrBool filter = default, ...)` |
| 3. Filters `EmacsMode`, `ViNavigationMode`, `HasSelection` available | Yes | `src/Stroke/Application/EmacsFilters.cs:18`, `src/Stroke/Application/ViFilters.cs:26`, `src/Stroke/Application/AppFilters.cs:25` |
| 4. `Application.RunInTerminalAsync` implemented | Yes | `src/Stroke/Application/RunInTerminal.cs` — `RunAsync()` methods with `InTerminal()` context |
| 5. `MergedKeyBindings` merge mechanism exists | Yes | `src/Stroke/KeyBinding/MergedKeyBindings.cs:42` — `public MergedKeyBindings(params IKeyBindingsBase[] registries)` |
| 6. `Buffer.Document` property is settable | Yes | Buffer uses `_workingLines[_workingIndex]` pattern in `Buffer.ExternalEditor.cs:48` |

**Decision**: All 6 assumptions are validated. No blockers.

### R8: Key Enum Values for Ctrl-X Ctrl-E

**Question**: Do `Keys.ControlX` and `Keys.ControlE` exist in the Stroke `Keys` enum?

**Finding**: Both values confirmed in `src/Stroke/Input/Keys.cs`:
- `Keys.ControlE` — line 64
- `Keys.ControlX` — line 159

**Decision**: Use `Keys.ControlX` and `Keys.ControlE` directly. No custom key definitions needed.

## Summary

All NEEDS CLARIFICATION items resolved. Key decisions:

1. **All dependencies already implemented** — no new infrastructure needed
2. **Follow per-binding filter pattern** (not ConditionalKeyBindings wrapper) for Emacs/Vi loaders — matches Python source
3. **No outer ConditionalKeyBindings** on combined loader — Python doesn't wrap with BufferHasFocus
4. **Ctrl-X Ctrl-E** as two-key sequence: `[Keys.ControlX, Keys.ControlE]`
5. **`Add<Binding>`** with `NamedCommands.GetByName("edit-and-execute-command")` — matches BasicBindings pattern
6. **Return types**: `KeyBindings` for individual loaders, `IKeyBindingsBase` for combined
