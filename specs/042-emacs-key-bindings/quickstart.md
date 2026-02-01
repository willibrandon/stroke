# Quickstart: Emacs Key Bindings

**Feature**: 042-emacs-key-bindings
**Date**: 2026-01-31

## Overview

This feature adds two binding loader methods to a new `EmacsBindings` static class:
- `LoadEmacsBindings()` — 78 core Emacs editing bindings (movement, kill ring, editing, history, selection, macros, completion, character search, numeric arguments)
- `LoadEmacsShiftSelectionBindings()` — 34 shift+arrow selection bindings

The third loader, `LoadEmacsSearchBindings()`, is already implemented in `SearchBindings`.

## File Layout

| File | Purpose | Est. LOC |
|------|---------|----------|
| `src/Stroke/Application/Bindings/EmacsBindings.cs` | `LoadEmacsBindings()` + inline handlers | ~600-700 |
| `src/Stroke/Application/Bindings/EmacsBindings.ShiftSelection.cs` | `LoadEmacsShiftSelectionBindings()` + shift handlers | ~300-400 |

## Implementation Pattern

Follow the same pattern as `OpenInEditorBindings.cs`, `BasicBindings.cs`, and `SearchBindings.cs`:

```csharp
using Stroke.Clipboard;
using Stroke.Completion;
using Stroke.Core;
using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;

namespace Stroke.Application.Bindings;

public static partial class EmacsBindings
{
    // Private filters (module-level conditions from Python source)
    private static readonly IFilter IsReturnable =
        new Condition(() => AppContext.GetApp().CurrentBuffer.IsReturnable);

    private static readonly IFilter IsArg =
        new Condition(() => ((KeyProcessor)AppContext.GetApp().KeyProcessor).Arg == "-");

    // Loader method
    public static IKeyBindingsBase LoadEmacsBindings()
    {
        var kb = new KeyBindings();
        var insertMode = EmacsFilters.EmacsInsertMode;

        // Named command bindings
        kb.Add<Binding>([new KeyOrChar(Keys.ControlA)])(
            NamedCommands.GetByName("beginning-of-line"));

        // Inline handler bindings
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlN)])(AutoDown);

        // ... all other bindings ...

        return new ConditionalKeyBindings(kb, EmacsFilters.EmacsMode);
    }

    // Private handler methods
    private static NotImplementedOrNone? AutoDown(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.AutoDown();
        return null;
    }
}
```

## Key Registration Patterns

### Named Command (no filter)
```csharp
kb.Add<Binding>([new KeyOrChar(Keys.ControlA)])(
    NamedCommands.GetByName("beginning-of-line"));
```

### Named Command (with filter)
```csharp
kb.Add<Binding>(
    [new KeyOrChar(Keys.ControlDelete)],
    filter: new FilterOrBool(insertMode))(
    NamedCommands.GetByName("kill-word"));
```

### Named Command (with saveBefore: false)
```csharp
kb.Add<Binding>(
    [new KeyOrChar(Keys.ControlUnderscore)],
    saveBefore: _ => false,
    filter: new FilterOrBool(insertMode))(
    NamedCommands.GetByName("undo"));
```

### Named Command (multi-key sequence)
```csharp
kb.Add<Binding>(
    [new KeyOrChar(Keys.ControlX), new KeyOrChar('r'), new KeyOrChar('y')],
    filter: new FilterOrBool(insertMode))(
    NamedCommands.GetByName("yank"));
```

### Inline Handler
```csharp
kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlN)])(AutoDown);
```

### Escape Key Sequences (Meta keys)
```csharp
kb.Add<Binding>(
    [new KeyOrChar(Keys.Escape), new KeyOrChar('b')])(
    NamedCommands.GetByName("backward-word"));
```

### Composite Filter
```csharp
var filter = new FilterOrBool(
    ((Filter)insertMode).And(IsReturnable));
kb.Add<Binding>(
    [new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.ControlM)],
    filter: filter)(
    NamedCommands.GetByName("accept-line"));
```

### Eager Binding
```csharp
kb.Add<KeyHandlerCallable>(
    [new KeyOrChar(Keys.Escape)],
    eager: new FilterOrBool(true))(handler);
```

## Build & Test

```bash
# Build
dotnet build src/Stroke/Stroke.csproj

# Run tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~EmacsBindings"
```

## Dependencies Verified

All dependencies are from lower layers and already implemented:
- Stroke.KeyBinding (Feature 022)
- Stroke.Input (Feature 014)
- Stroke.Filters (Feature 017)
- Stroke.Core (Features 002, 003, 007)
- Stroke.Application filters (Feature 032)
- Stroke.KeyBinding.Bindings.NamedCommands (Feature 034)
- Stroke.Completion (Feature 012)
- Stroke.Clipboard (Feature 004)
