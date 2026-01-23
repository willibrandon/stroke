# Feature 44: Dialogs

## Overview

Implement the dialog shortcut functions for displaying modal dialogs including yes/no dialogs, message dialogs, input dialogs, radiolist dialogs, checkboxlist dialogs, and progress dialogs.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/shortcuts/dialogs.py`

## Public API

### yes_no_dialog

```csharp
namespace Stroke.Shortcuts;

public static class Dialogs
{
    /// <summary>
    /// Display a Yes/No dialog.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="text">Dialog body text.</param>
    /// <param name="yesText">Text for Yes button.</param>
    /// <param name="noText">Text for No button.</param>
    /// <param name="style">Style to apply.</param>
    /// <returns>Application that returns true for Yes, false for No.</returns>
    public static Application<bool> YesNoDialog(
        AnyFormattedText title = default,
        AnyFormattedText text = default,
        string yesText = "Yes",
        string noText = "No",
        BaseStyle? style = null);
}
```

### button_dialog

```csharp
/// <summary>
/// Display a dialog with button choices.
/// </summary>
/// <typeparam name="T">Type of button values.</typeparam>
/// <param name="title">Dialog title.</param>
/// <param name="text">Dialog body text.</param>
/// <param name="buttons">List of (text, value) tuples for buttons.</param>
/// <param name="style">Style to apply.</param>
/// <returns>Application that returns the selected button's value.</returns>
public static Application<T> ButtonDialog<T>(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    IList<(string Text, T Value)>? buttons = null,
    BaseStyle? style = null);
```

### input_dialog

```csharp
/// <summary>
/// Display a text input dialog.
/// </summary>
/// <param name="title">Dialog title.</param>
/// <param name="text">Dialog body text.</param>
/// <param name="okText">OK button text.</param>
/// <param name="cancelText">Cancel button text.</param>
/// <param name="completer">Optional completer for input.</param>
/// <param name="validator">Optional validator for input.</param>
/// <param name="password">Hide input with asterisks.</param>
/// <param name="style">Style to apply.</param>
/// <param name="default">Default input value.</param>
/// <returns>Application that returns input text or null if cancelled.</returns>
public static Application<string?> InputDialog(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string okText = "OK",
    string cancelText = "Cancel",
    Completer? completer = null,
    Validator? validator = null,
    object? password = null,
    BaseStyle? style = null,
    string @default = "");
```

### message_dialog

```csharp
/// <summary>
/// Display a simple message dialog and wait for Enter.
/// </summary>
/// <param name="title">Dialog title.</param>
/// <param name="text">Dialog body text.</param>
/// <param name="okText">OK button text.</param>
/// <param name="style">Style to apply.</param>
/// <returns>Application that returns when dismissed.</returns>
public static Application<object?> MessageDialog(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string okText = "Ok",
    BaseStyle? style = null);
```

### radiolist_dialog

```csharp
/// <summary>
/// Display a dialog with radio button choices.
/// </summary>
/// <typeparam name="T">Type of values.</typeparam>
/// <param name="title">Dialog title.</param>
/// <param name="text">Dialog body text.</param>
/// <param name="okText">OK button text.</param>
/// <param name="cancelText">Cancel button text.</param>
/// <param name="values">List of (value, label) tuples.</param>
/// <param name="default">Default selected value.</param>
/// <param name="style">Style to apply.</param>
/// <returns>Application that returns the selected value.</returns>
public static Application<T?> RadiolistDialog<T>(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string okText = "Ok",
    string cancelText = "Cancel",
    IList<(T Value, AnyFormattedText Label)>? values = null,
    T? @default = default,
    BaseStyle? style = null);
```

### checkboxlist_dialog

```csharp
/// <summary>
/// Display a dialog with checkbox choices.
/// </summary>
/// <typeparam name="T">Type of values.</typeparam>
/// <param name="title">Dialog title.</param>
/// <param name="text">Dialog body text.</param>
/// <param name="okText">OK button text.</param>
/// <param name="cancelText">Cancel button text.</param>
/// <param name="values">List of (value, label) tuples.</param>
/// <param name="defaultValues">Default selected values.</param>
/// <param name="style">Style to apply.</param>
/// <returns>Application that returns list of selected values.</returns>
public static Application<IList<T>?> CheckboxlistDialog<T>(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string okText = "Ok",
    string cancelText = "Cancel",
    IList<(T Value, AnyFormattedText Label)>? values = null,
    IList<T>? defaultValues = null,
    BaseStyle? style = null);
```

### progress_dialog

```csharp
/// <summary>
/// Display a progress dialog with log output.
/// </summary>
/// <param name="title">Dialog title.</param>
/// <param name="text">Dialog body text.</param>
/// <param name="runCallback">Callback that receives setPercentage and logText functions.</param>
/// <param name="style">Style to apply.</param>
/// <returns>Application that returns when work completes.</returns>
public static Application<object?> ProgressDialog(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    Action<Action<int>, Action<string>>? runCallback = null,
    BaseStyle? style = null);
```

## Project Structure

```
src/Stroke/
└── Shortcuts/
    └── Dialogs.cs
tests/Stroke.Tests/
└── Shortcuts/
    └── DialogsTests.cs
```

## Implementation Notes

### Dialog Structure

All dialogs use a common pattern:
1. Create a `Dialog` widget with body and buttons
2. Wrap in `_create_app()` which adds key bindings and styling
3. Return `Application<T>` that can be run with `.Run()`

### _create_app Implementation

```csharp
private static Application<T> CreateApp<T>(Container dialog, BaseStyle? style)
{
    var bindings = new KeyBindings();
    bindings.Add("tab", FocusNext);
    bindings.Add("s-tab", FocusPrevious);

    return new Application<T>(
        layout: new Layout(dialog),
        keyBindings: MergeKeyBindings(LoadKeyBindings(), bindings),
        mouseSupport: true,
        style: style,
        fullScreen: true
    );
}
```

### Button Handler Pattern

```csharp
// Yes/No dialog
void YesHandler() => GetApp().Exit(result: true);
void NoHandler() => GetApp().Exit(result: false);

// Button dialog with values
void ButtonHandler(T value) => GetApp().Exit(result: value);
```

### Input Dialog Focus Flow

1. User types in TextArea
2. Press Enter → Focus moves to OK button
3. Press Enter on OK → Exit with text
4. Press Enter on Cancel → Exit with null

### Progress Dialog Threading

```csharp
runCallback: (setPercentage, logText) =>
{
    for (int i = 0; i <= 100; i++)
    {
        setPercentage(i);
        logText($"Progress: {i}%\n");
        Thread.Sleep(100);
    }
}
```

- `setPercentage`: Updates progress bar (0-100)
- `logText`: Appends text to log area
- Runs in executor thread, UI updates via `CallSoonThreadSafe`

### Cancel Behavior

- Cancel button calls `GetApp().Exit()` without result
- This returns `null` or `default(T)` depending on type
- Input dialog returns `null` for string
- List dialogs return `null` for list

## Dependencies

- `Stroke.Application` (Feature 31) - Application class
- `Stroke.Widgets.Dialog` (Feature 45) - Dialog widget
- `Stroke.Widgets.Button` (Feature 45) - Button widget
- `Stroke.Widgets.TextArea` (Feature 45) - TextArea widget
- `Stroke.Widgets.RadioList` (Feature 45) - RadioList widget
- `Stroke.Widgets.CheckboxList` (Feature 45) - CheckboxList widget
- `Stroke.Widgets.ProgressBar` (Feature 45) - ProgressBar widget
- `Stroke.KeyBinding.Defaults` (Feature 21) - Default key bindings

## Implementation Tasks

1. Implement `YesNoDialog` function
2. Implement `ButtonDialog<T>` function
3. Implement `InputDialog` function
4. Implement `MessageDialog` function
5. Implement `RadiolistDialog<T>` function
6. Implement `CheckboxlistDialog<T>` function
7. Implement `ProgressDialog` function
8. Implement `CreateApp<T>` helper
9. Write comprehensive unit tests

## Acceptance Criteria

- [ ] YesNoDialog returns boolean correctly
- [ ] ButtonDialog returns selected value
- [ ] InputDialog returns text or null
- [ ] MessageDialog displays and dismisses
- [ ] RadiolistDialog allows single selection
- [ ] CheckboxlistDialog allows multiple selection
- [ ] ProgressDialog updates progress bar
- [ ] Tab/Shift-Tab navigate between buttons
- [ ] All dialogs display in full screen
- [ ] Cancel returns null/default appropriately
- [ ] Unit tests achieve 80% coverage
