# Research: Choices Examples

**Feature**: 062-choices-examples
**Date**: 2026-02-04

## Research Summary

No NEEDS CLARIFICATION items identified in Technical Context. All dependencies are confirmed implemented and documented.

## Dependency Verification

### Dialogs.Choice<T>() API

**Source**: `src/Stroke/Shortcuts/Dialogs.cs:544-571`
**Status**: ✅ Implemented (Feature 48: Dialog Shortcuts)

Available parameters match Python Prompt Toolkit's `choice()` function:
- `message`: AnyFormattedText (supports HTML, plain text)
- `options`: IReadOnlyList<(T Value, AnyFormattedText Label)>
- `defaultValue`: T? (pre-selection)
- `mouseSupport`: bool (mouse clicks)
- `style`: IStyle? (custom styling)
- `symbol`: string (selection indicator, default ">")
- `bottomToolbar`: AnyFormattedText? (toolbar text)
- `showFrame`: FilterOrBool (frame visibility, supports `~AppFilters.IsDone`)
- `enableSuspend`: FilterOrBool (Ctrl+Z on Unix)
- `enableInterrupt`: FilterOrBool (Ctrl+C handling)
- `interruptException`: Type? (exception type for Ctrl+C)
- `keyBindings`: IKeyBindingsBase? (custom bindings)

### Style.FromDict() API

**Source**: Feature 18: Styles System
**Status**: ✅ Implemented

Supports style dictionaries with keys:
- `"input-selection"`: Selection highlight style
- `"number"`: Number prefix style
- `"selected-option"`: Selected item style
- `"frame.border"`: Frame border color
- `"bottom-toolbar"`: Toolbar style
- `"accepted frame.border"`: Frame color after acceptance

### Html.Parse() API

**Source**: Feature 15: Formatted Text
**Status**: ✅ Implemented

Supports HTML tags: `<u>`, `<b>`, `<i>`, `<ansigreen>`, `<ansired>`, etc.

### AppFilters.IsDone Filter

**Source**: Feature 32: Application Filters
**Status**: ✅ Implemented

Supports negation via `~` operator for conditional visibility.

## Python → C# Patterns

### Python `choice()` Function
```python
from prompt_toolkit.shortcuts import choice
result = choice(message="...", options=[...])
```

### C# `Dialogs.Choice<T>()` Method
```csharp
using Stroke.Shortcuts;
var result = Dialogs.Choice("...", [("value", "Label"), ...]);
```

### Key Translations

| Python | C# |
|--------|-----|
| `("pizza", "Pizza with mushrooms")` | `("pizza", "Pizza with mushrooms")` |
| `HTML("<u>text</u>")` | `Html.Parse("<u>text</u>")` |
| `Style.from_dict({...})` | `Style.FromDict(new Dictionary<string, string> {...})` |
| `~is_done` | `~AppFilters.IsDone` |
| `mouse_support=True` | `mouseSupport: true` |
| `show_frame=True` | `showFrame: true` |
| `bottom_toolbar=HTML(...)` | `bottomToolbar: Html.Parse(...)` |

## Conclusions

All dependencies are verified as implemented. No blocking issues identified. Proceed directly to implementation.
