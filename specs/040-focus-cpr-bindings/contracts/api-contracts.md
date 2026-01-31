# API Contracts: Focus & CPR Bindings

**Feature**: 040-focus-cpr-bindings
**Date**: 2026-01-31

## Module 1: FocusFunctions

**Namespace**: `Stroke.Application.Bindings`
**File**: `src/Stroke/Application/Bindings/FocusFunctions.cs`
**Python Source**: `prompt_toolkit.key_binding.bindings.focus`

### Class: FocusFunctions

```csharp
/// <summary>
/// Focus navigation handler functions for moving between visible focusable windows.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.focus</c> module.
/// This type is stateless and inherently thread-safe.
/// </remarks>
public static class FocusFunctions
{
    /// <summary>
    /// Focus the next visible window. Often bound to the Tab key.
    /// </summary>
    public static NotImplementedOrNone? FocusNext(KeyPressEvent @event);

    /// <summary>
    /// Focus the previous visible window. Often bound to the BackTab (Shift+Tab) key.
    /// </summary>
    public static NotImplementedOrNone? FocusPrevious(KeyPressEvent @event);
}
```

### API Mapping

| Python | C# | Notes |
|--------|-----|-------|
| `focus_next(event)` | `FocusFunctions.FocusNext(event)` | Delegates to `Layout.FocusNext()` |
| `focus_previous(event)` | `FocusFunctions.FocusPrevious(event)` | Delegates to `Layout.FocusPrevious()` |

---

## Module 2: CprBindings

**Namespace**: `Stroke.Application.Bindings`
**File**: `src/Stroke/Application/Bindings/CprBindings.cs`
**Python Source**: `prompt_toolkit.key_binding.bindings.cpr`

### Class: CprBindings

```csharp
/// <summary>
/// Key binding loader for Cursor Position Report (CPR) response handling.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.cpr</c> module.
/// This type is stateless and inherently thread-safe.
/// </remarks>
public static class CprBindings
{
    /// <summary>
    /// Load key bindings for handling CPR (Cursor Position Report) responses.
    /// </summary>
    /// <returns>
    /// A new <see cref="KeyBindings"/> instance containing a single binding for
    /// <see cref="Keys.CPRResponse"/> with saveBefore disabled.
    /// </returns>
    public static KeyBindings LoadCprBindings();
}
```

### API Mapping

| Python | C# | Notes |
|--------|-----|-------|
| `load_cpr_bindings()` | `CprBindings.LoadCprBindings()` | Returns `KeyBindings` with 1 CPR response binding |

### Binding Registration Details

| Key | Handler | saveBefore | Notes |
|-----|---------|------------|-------|
| `Keys.CPRResponse` | *(internal handler)* | `_ => false` | Parses row/col from `@event.Data`, reports row to `Renderer.ReportAbsoluteCursorRow()` |

### CPR Data Format

```
Input:  "\x1b[35;1R"   (ESC [ row ; col R)
Parse:  data[2..^1]    → "35;1"
Split:  "35;1".Split(';') → ["35", "1"]
Result: row=35, col=1  → Renderer.ReportAbsoluteCursorRow(35)
```
