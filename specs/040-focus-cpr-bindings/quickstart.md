# Quickstart: Focus & CPR Bindings

**Feature**: 040-focus-cpr-bindings
**Date**: 2026-01-31

## Overview

This feature adds two small binding modules to `Stroke.Application.Bindings`:

1. **FocusFunctions** — Two handler functions (`FocusNext`, `FocusPrevious`) for navigating between visible focusable windows in a layout.
2. **CprBindings** — A binding loader (`LoadCprBindings`) that registers a handler for terminal CPR (Cursor Position Report) responses.

## Files to Create

| File | Purpose |
|------|---------|
| `src/Stroke/Application/Bindings/FocusFunctions.cs` | FocusNext, FocusPrevious handler functions |
| `src/Stroke/Application/Bindings/CprBindings.cs` | LoadCprBindings() factory method |
| `tests/Stroke.Tests/Application/Bindings/FocusCprBindingsTests.cs` | Tests for both modules |

## Implementation Order

1. **FocusFunctions.cs** — No dependencies beyond existing infrastructure. Two methods, each a single line of delegation.
2. **CprBindings.cs** — No dependencies beyond existing infrastructure. One factory method with one binding registration.
3. **FocusCprBindingsTests.cs** — Tests for both modules.

## Key Patterns

### Handler function pattern (from existing codebase)
```csharp
public static NotImplementedOrNone? HandlerName(KeyPressEvent @event)
{
    @event.GetApp().Layout.FocusNext();
    return null;
}
```

### Binding loader with saveBefore pattern
```csharp
public static KeyBindings LoadCprBindings()
{
    var kb = new KeyBindings();
    kb.Add<KeyHandlerCallable>(
        [new KeyOrChar(Keys.CPRResponse)],
        saveBefore: _ => false)(CprHandler);
    return kb;
}
```

### CPR data parsing pattern
```csharp
var parts = @event.Data[2..^1].Split(';');
var row = int.Parse(parts[0]);
var col = int.Parse(parts[1]);
```

## Build & Test

```bash
dotnet build src/Stroke/Stroke.csproj
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~FocusCprBindings"
```

## Python Reference

- `focus.py`: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/focus.py` (27 lines)
- `cpr.py`: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/cpr.py` (31 lines)
