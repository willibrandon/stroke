# Data Model: Dialog Shortcut Functions

**Feature**: 048-dialog-shortcuts
**Date**: 2026-02-02

## Entities

This feature introduces no new data entities. The `Dialogs` class is a stateless static class containing factory methods. All data structures used (Dialog, Label, Button, TextArea, RadioList, CheckboxList, ProgressBar, Application, KeyBindings) are pre-existing from prior features.

## Static Class: Dialogs

**Namespace**: `Stroke.Shortcuts`
**Type**: `public static class`
**State**: None (all methods are static, no mutable fields)

### Public Methods (Factory — return Application<T>)

| Method | Return Type | Parameters |
|--------|-------------|------------|
| `YesNoDialog` | `Application<bool>` | title, text, yesText, noText, style |
| `MessageDialog` | `Application<object?>` | title, text, okText, style |
| `InputDialog` | `Application<string?>` | title, text, okText, cancelText, completer, validator, password, style, default_ |
| `ButtonDialog<T>` | `Application<T>` | title, text, buttons, style |
| `RadioListDialog<T>` | `Application<T?>` | title, text, okText, cancelText, values, default_, style |
| `CheckboxListDialog<T>` | `Application<IReadOnlyList<T>?>` | title, text, okText, cancelText, values, defaultValues, style |
| `ProgressDialog` | `Application<object?>` | title, text, runCallback, style |

### Public Methods (Async — run Application and return result)

| Method | Return Type | Delegates to |
|--------|-------------|--------------|
| `YesNoDialogAsync` | `Task<bool>` | `YesNoDialog(...).RunAsync()` |
| `MessageDialogAsync` | `Task` | `MessageDialog(...).RunAsync()` |
| `InputDialogAsync` | `Task<string?>` | `InputDialog(...).RunAsync()` |
| `ButtonDialogAsync<T>` | `Task<T>` | `ButtonDialog<T>(...).RunAsync()` |
| `RadioListDialogAsync<T>` | `Task<T?>` | `RadioListDialog<T>(...).RunAsync()` |
| `CheckboxListDialogAsync<T>` | `Task<IReadOnlyList<T>?>` | `CheckboxListDialog<T>(...).RunAsync()` |
| `ProgressDialogAsync` | `Task` | `ProgressDialog(...).RunAsync()` |

### Private Methods

| Method | Purpose |
|--------|---------|
| `CreateApp` | Wraps dialog in Application with merged key bindings, mouse support, style, full-screen |
| `ReturnNone` | Cancel button handler — calls `AppContext.GetApp().Exit()` |

## Widget Composition Hierarchy

Each dialog function assembles widgets in this hierarchy:

```
Application<T>
└── Layout
    └── Dialog (withBackground=true)
        ├── Frame
        │   ├── Shadow
        │   │   └── Box (withBackground wrapper)
        │   └── HSplit (frame_body)
        │       ├── Box (body padding)
        │       │   └── [body content — varies per dialog type]
        │       └── Box (button row)
        │           └── VSplit (buttons with left/right key bindings)
        └── KeyBindings (tab/s-tab focus navigation)
```

### Body Content per Dialog Type

| Dialog | Body Content |
|--------|-------------|
| YesNoDialog | `Label(text)` |
| MessageDialog | `Label(text)` |
| InputDialog | `HSplit([Label(text), TextArea, ValidationToolbar], padding)` |
| ButtonDialog | `Label(text)` |
| RadioListDialog | `HSplit([Label(text), RadioList], padding=1)` |
| CheckboxListDialog | `HSplit([Label(text), CheckboxList], padding=1)` |
| ProgressDialog | `HSplit([Box(Label(text)), Box(TextArea), ProgressBar])` |

## Relationships to Existing Entities

```
Dialogs (static) ──creates──▶ Application<T>
    │                              │
    ├──uses──▶ Dialog              ├──contains──▶ Layout
    ├──uses──▶ Label               ├──contains──▶ MergedKeyBindings
    ├──uses──▶ Button              └──contains──▶ IStyle?
    ├──uses──▶ TextArea
    ├──uses──▶ RadioList<T>
    ├──uses──▶ CheckboxList<T>
    ├──uses──▶ ProgressBar
    ├──uses──▶ ValidationToolbar
    ├──uses──▶ Box
    ├──uses──▶ HSplit
    ├──uses──▶ KeyBindings
    ├──uses──▶ MergedKeyBindings
    ├──uses──▶ DefaultKeyBindings
    ├──uses──▶ FocusFunctions
    └──uses──▶ AppContext
```
