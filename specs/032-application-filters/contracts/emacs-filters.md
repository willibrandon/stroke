# Contract: EmacsFilters

**Feature**: 032-application-filters
**Python Source**: `prompt_toolkit/filters/app.py` (lines 362-385)

## Static Class: EmacsFilters

**Namespace**: `Stroke.Application`
**Dependencies**: `Stroke.Filters` (IFilter, Condition), `Stroke.KeyBinding` (EditingMode), `Stroke.Core` (Buffer)

### Properties (IFilter)

```csharp
/// <summary>True when Emacs editing mode is active.</summary>
public static IFilter EmacsMode { get; }
// Logic: AppContext.GetApp().EditingMode == EditingMode.Emacs

/// <summary>
/// True when Emacs insert mode is active (Emacs mode, no selection, not read-only).
/// </summary>
public static IFilter EmacsInsertMode { get; }
// Logic:
//   if (EditingMode != Emacs || SelectionState != null || ReadOnly) return false;
//   return true;

/// <summary>
/// True when Emacs selection mode is active (Emacs mode with selection).
/// </summary>
public static IFilter EmacsSelectionMode { get; }
// Logic: EditingMode == Emacs && CurrentBuffer.SelectionState is not null
```

### Behavioral Notes

- All properties are static readonly `IFilter` backed by `new Condition(() => ...)`.
- `EmacsInsertMode` returns false when a selection is active (selection mode overrides insert).
- `EmacsInsertMode` returns false when the buffer is read-only.
- All filters return `false` when not in Emacs editing mode.
- All filters return `false` when no application is running (DummyApplication uses Emacs mode by default, but has no selection and default buffer is not read-only — so `EmacsMode` returns true with DummyApplication, `EmacsInsertMode` returns true with DummyApplication).

**Important DummyApplication Note**: Unlike Vi filters which all return false with DummyApplication (because default mode is Emacs), the Emacs filters `EmacsMode` and `EmacsInsertMode` will return **true** with DummyApplication because:
- `DummyApplication` defaults to `EditingMode.Emacs`
- The dummy buffer has no selection and is not read-only

This matches Python PTK behavior where `get_app()` returns an app with Emacs editing mode.
