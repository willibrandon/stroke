# API Contract: Dialogs Static Class

**Feature**: 048-dialog-shortcuts
**Namespace**: `Stroke.Shortcuts`
**Port of**: `prompt_toolkit.shortcuts.dialogs`

## Class Definition

```csharp
/// <summary>
/// Pre-built dialog shortcut functions for common interactive patterns.
/// Each method composes widget primitives into a ready-to-run Application.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.shortcuts.dialogs</c> module.
/// </para>
/// <para>
/// Factory methods (e.g., <see cref="YesNoDialog"/>) return <c>Application&lt;T&gt;</c> objects
/// that can be run with <c>RunAsync()</c>. Async convenience methods (e.g.,
/// <see cref="YesNoDialogAsync"/>) create and run the application in one call.
/// </para>
/// </remarks>
public static class Dialogs
```

## Factory Methods

### YesNoDialog

```csharp
/// <summary>
/// Display a Yes/No dialog. Returns an application that exits with a boolean result.
/// </summary>
/// <param name="title">Dialog title.</param>
/// <param name="text">Dialog body text.</param>
/// <param name="yesText">Text for the Yes button. Defaults to "Yes".</param>
/// <param name="noText">Text for the No button. Defaults to "No".</param>
/// <param name="style">Optional custom style.</param>
/// <returns>An application that returns <c>true</c> for Yes, <c>false</c> for No.</returns>
public static Application<bool> YesNoDialog(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string yesText = "Yes",
    string noText = "No",
    IStyle? style = null)
```

### MessageDialog

```csharp
/// <summary>
/// Display a simple message box and wait until the user presses Ok.
/// </summary>
/// <param name="title">Dialog title.</param>
/// <param name="text">Dialog body text.</param>
/// <param name="okText">Text for the Ok button. Defaults to "Ok".</param>
/// <param name="style">Optional custom style.</param>
/// <returns>An application that exits with no meaningful result.</returns>
public static Application<object?> MessageDialog(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string okText = "Ok",
    IStyle? style = null)
```

### InputDialog

```csharp
/// <summary>
/// Display a text input box. Returns the entered text, or null when cancelled.
/// </summary>
/// <param name="title">Dialog title.</param>
/// <param name="text">Dialog body text (label above text field).</param>
/// <param name="okText">Text for the OK button. Defaults to "OK".</param>
/// <param name="cancelText">Text for the Cancel button. Defaults to "Cancel".</param>
/// <param name="completer">Optional completer for the text field.</param>
/// <param name="validator">Optional validator for the text field.</param>
/// <param name="password">Whether to mask the input. Defaults to false.</param>
/// <param name="style">Optional custom style.</param>
/// <param name="default_">Default text in the input field.</param>
/// <returns>An application that returns the entered text, or null on cancel.</returns>
public static Application<string?> InputDialog(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string okText = "OK",
    string cancelText = "Cancel",
    ICompleter? completer = null,
    IValidator? validator = null,
    FilterOrBool password = default,
    IStyle? style = null,
    string default_ = "")
```

### ButtonDialog\<T\>

```csharp
/// <summary>
/// Display a dialog with button choices. Returns the value associated with the selected button.
/// </summary>
/// <typeparam name="T">The type of value associated with each button.</typeparam>
/// <param name="title">Dialog title.</param>
/// <param name="text">Dialog body text.</param>
/// <param name="buttons">List of (text, value) tuples defining the buttons.</param>
/// <param name="style">Optional custom style.</param>
/// <returns>An application that returns the value of the selected button.</returns>
public static Application<T> ButtonDialog<T>(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    IReadOnlyList<(string Text, T Value)>? buttons = null,
    IStyle? style = null)
```

### RadioListDialog\<T\>

```csharp
/// <summary>
/// Display a single-selection radio list dialog. The user selects one option
/// from a list using Arrow keys and Enter.
/// </summary>
/// <typeparam name="T">The type of values in the list.</typeparam>
/// <param name="title">Dialog title.</param>
/// <param name="text">Dialog body text.</param>
/// <param name="okText">Text for the Ok button. Defaults to "Ok".</param>
/// <param name="cancelText">Text for the Cancel button. Defaults to "Cancel".</param>
/// <param name="values">List of (value, label) tuples for the radio options.</param>
/// <param name="default_">Default selected value.</param>
/// <param name="style">Optional custom style.</param>
/// <returns>An application that returns the selected value, or default on cancel.</returns>
public static Application<T?> RadioListDialog<T>(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string okText = "Ok",
    string cancelText = "Cancel",
    IReadOnlyList<(T Value, AnyFormattedText Label)>? values = null,
    T? default_ = default,
    IStyle? style = null)
```

### CheckboxListDialog\<T\>

```csharp
/// <summary>
/// Display a multi-selection checkbox list dialog. The user selects zero or more
/// options using Arrow keys and Enter/Space.
/// </summary>
/// <typeparam name="T">The type of values in the list.</typeparam>
/// <param name="title">Dialog title.</param>
/// <param name="text">Dialog body text.</param>
/// <param name="okText">Text for the Ok button. Defaults to "Ok".</param>
/// <param name="cancelText">Text for the Cancel button. Defaults to "Cancel".</param>
/// <param name="values">List of (value, label) tuples for the checkbox options.</param>
/// <param name="defaultValues">Values to pre-check.</param>
/// <param name="style">Optional custom style.</param>
/// <returns>An application that returns the list of selected values, or null on cancel.</returns>
public static Application<IReadOnlyList<T>?> CheckboxListDialog<T>(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string okText = "Ok",
    string cancelText = "Cancel",
    IReadOnlyList<(T Value, AnyFormattedText Label)>? values = null,
    IReadOnlyList<T>? defaultValues = null,
    IStyle? style = null)
```

### ProgressDialog

```csharp
/// <summary>
/// Display a progress dialog with a progress bar and log text area.
/// The callback runs in a background task and receives functions to update
/// the progress percentage and log text.
/// </summary>
/// <param name="title">Dialog title.</param>
/// <param name="text">Dialog body text.</param>
/// <param name="runCallback">Callback receiving (setPercentage, logText) functions.
/// Executes on a background thread.</param>
/// <param name="style">Optional custom style.</param>
/// <returns>An application that exits when the callback completes.</returns>
public static Application<object?> ProgressDialog(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    Action<Action<int>, Action<string>>? runCallback = null,
    IStyle? style = null)
```

## Async Convenience Methods

```csharp
public static Task<bool> YesNoDialogAsync(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string yesText = "Yes",
    string noText = "No",
    IStyle? style = null)

public static Task MessageDialogAsync(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string okText = "Ok",
    IStyle? style = null)

public static Task<string?> InputDialogAsync(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string okText = "OK",
    string cancelText = "Cancel",
    ICompleter? completer = null,
    IValidator? validator = null,
    FilterOrBool password = default,
    IStyle? style = null,
    string default_ = "")

public static Task<T> ButtonDialogAsync<T>(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    IReadOnlyList<(string Text, T Value)>? buttons = null,
    IStyle? style = null)

public static Task<T?> RadioListDialogAsync<T>(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string okText = "Ok",
    string cancelText = "Cancel",
    IReadOnlyList<(T Value, AnyFormattedText Label)>? values = null,
    T? default_ = default,
    IStyle? style = null)

public static Task<IReadOnlyList<T>?> CheckboxListDialogAsync<T>(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    string okText = "Ok",
    string cancelText = "Cancel",
    IReadOnlyList<(T Value, AnyFormattedText Label)>? values = null,
    IReadOnlyList<T>? defaultValues = null,
    IStyle? style = null)

public static Task ProgressDialogAsync(
    AnyFormattedText title = default,
    AnyFormattedText text = default,
    Action<Action<int>, Action<string>>? runCallback = null,
    IStyle? style = null)
```

## Private Methods

```csharp
/// <summary>
/// Wrap a dialog container in an Application with merged key bindings,
/// mouse support, optional styling, and full-screen mode.
/// </summary>
/// <remarks>
/// Python's <c>_create_app</c> accepts <c>AnyContainer</c>. In C#, the parameter type
/// is <c>IContainer</c> because <c>Dialog</c>'s <c>Container</c> property (which is the
/// outermost wrapper — Box when withBackground=true, Shadow otherwise) implements
/// <c>IContainer</c>. The Python <c>AnyContainer</c> union type maps to C#'s
/// <c>IContainer</c> interface.
/// </remarks>
private static Application<T> CreateApp<T>(IContainer dialog, IStyle? style)

/// <summary>
/// Button handler that exits the current application with no result (null/default).
/// Equivalent to Python's <c>_return_none()</c>.
/// </summary>
private static void ReturnNone()
```

## Generic Type Constraints

`ButtonDialog<T>`, `RadioListDialog<T>`, and `CheckboxListDialog<T>` are unconstrained (no `where` clause), matching Python's `TypeVar("_T")`.

**Nullability consideration for RadioListDialog**: The return type `Application<T?>` works for both reference types (`T? = T | null`) and value types (`T? = Nullable<T>`). No `where T : class` constraint is required. The api-mapping.md lists `Task<T>` for `RadioListDialogAsync`, but the correct return type is `Task<T?>` to reflect the cancel-returns-null semantic per the spec.
