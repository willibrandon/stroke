# Quickstart: Basic Key Bindings

**Feature**: 037-basic-key-bindings
**Date**: 2026-01-30

## Overview

This feature implements `BasicBindings.LoadBasicBindings()` — a factory method returning a `KeyBindings` instance with all basic key bindings shared between Emacs and Vi modes. It is a faithful port of Python Prompt Toolkit's `key_binding/bindings/basic.py`.

## Build Sequence

### Phase 1: Implementation File

**File**: `src/Stroke/Application/Bindings/BasicBindings.cs`

1. Define static class `BasicBindings` in `Stroke.Application.Bindings`
2. Define private static filters: `InsertMode`, `HasTextBeforeCursor`, `InQuotedInsert`
3. Define private static `IfNoRepeat` save-before callback
4. Define private static `Ignore` no-op handler
5. Implement `LoadBasicBindings()`:
   - Create `KeyBindings` instance
   - Register ignored keys (all control, function, nav, modifier combos)
   - Register readline movement bindings via `NamedCommands.GetByName()`
   - Register readline editing bindings with `InsertMode` filter
   - Register self-insert with `InsertMode` filter and `IfNoRepeat`
   - Register tab completion bindings
   - Register history navigation with `~HasSelection` filter
   - Register Ctrl+D with `HasTextBeforeCursor & InsertMode` filter
   - Register Enter multiline handler
   - Register Ctrl+J re-dispatch handler
   - Register Up/Down auto-navigation handlers
   - Register Delete selection handler
   - Register Ctrl+Z literal insert handler
   - Register bracketed paste handler
   - Register quoted insert handler (eager)
   - Return the `KeyBindings` instance

### Phase 2: Test Files

**File 1**: `tests/Stroke.Tests/Application/Bindings/BasicBindingsIgnoredKeysTests.cs`
- Verify all ignored keys produce no-op bindings
- Verify ignored keys don't alter buffer content
- Verify binding count for ignored key group

**File 2**: `tests/Stroke.Tests/Application/Bindings/BasicBindingsReadlineTests.cs`
- Verify readline movement bindings map to correct named commands
- Verify readline editing bindings have correct filters
- Verify self-insert, tab completion, history nav bindings
- Verify filter compositions (InsertMode, ~HasSelection)
- Verify saveBefore callbacks

**File 3**: `tests/Stroke.Tests/Application/Bindings/BasicBindingsHandlerTests.cs`
- Test Enter multiline handler (newline insertion, paste mode)
- Test Ctrl+J re-dispatch
- Test Up/Down auto-navigation
- Test Delete selection (cut + clipboard)
- Test Ctrl+D conditional delete
- Test Ctrl+Z literal insert
- Test bracketed paste line ending normalization
- Test quoted insert (literal insert + mode deactivation)

## Key API Patterns

### Registration with Named Commands

```csharp
// Named command binding (uses Add<Binding> for filter composition)
kb.Add<Binding>([new KeyOrChar(Keys.Home)])(
    NamedCommands.GetByName("beginning-of-line"));

// Named command with filter
kb.Add<Binding>([new KeyOrChar(Keys.ControlK)], filter: InsertMode)(
    NamedCommands.GetByName("kill-line"));

// Named command with filter + saveBefore
kb.Add<Binding>([new KeyOrChar(Keys.ControlH)], filter: InsertMode, saveBefore: IfNoRepeat)(
    NamedCommands.GetByName("backward-delete-char"));
```

### Registration with Inline Handlers

```csharp
// Inline handler (uses Add<KeyHandlerCallable>)
kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Up)])(@event =>
{
    @event.CurrentBuffer!.AutoUp(count: @event.Arg);
    return null;
});

// Inline handler with filter
kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlM)],
    filter: new FilterOrBool(((Filter)InsertMode) & AppFilters.IsMultiline))(@event =>
{
    @event.CurrentBuffer!.Newline(copyMargin: !((Filter)AppFilters.InPasteMode).Invoke());
    return null;
});
```

### Filter Composition

```csharp
// OR composition for insert mode
private static readonly IFilter InsertMode =
    ((Filter)ViFilters.ViInsertMode) | EmacsFilters.EmacsInsertMode;

// AND composition for Ctrl+D
var ctrlDFilter = ((Filter)HasTextBeforeCursor) & InsertMode;

// NOT composition for history nav
var notSelected = new FilterOrBool(~(Filter)AppFilters.HasSelection);
```

## Dependencies

All dependencies are existing features — no new packages or infrastructure required.

| Dependency | Feature | Status |
|------------|---------|--------|
| `KeyBindings` | 022 | Complete |
| `NamedCommands` | 034 | Complete (49 commands) |
| `AppFilters` | 032 | Complete |
| `ViFilters` | 032 | Complete |
| `EmacsFilters` | 032 | Complete |
| `Buffer` (AutoUp, AutoDown, CutSelection, Newline, InsertText) | 007 | Complete |
| `KeyProcessor` (Feed) | 030 | Complete |
| `Application` (QuotedInsert, Clipboard) | 030 | Complete |
| `Keys` enum | 011 | Complete |
| `Filter` operators | 017 | Complete |

## Estimated LOC

| File | Estimated LOC |
|------|---------------|
| `BasicBindings.cs` | ~250-350 |
| `BasicBindingsIgnoredKeysTests.cs` | ~300-400 |
| `BasicBindingsReadlineTests.cs` | ~400-500 |
| `BasicBindingsHandlerTests.cs` | ~500-600 |
| **Total** | ~1,450-1,850 |

All files well within the 1,000 LOC limit per Constitution X.
