# Feature 73: Dialogs

## Overview

Implement pre-built dialog shortcut functions for common interactive patterns: yes/no confirmation, button selection, text input, message display, radio list selection, checkbox list selection, and progress dialogs.

## Python Prompt Toolkit Reference

**Source:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/shortcuts/dialogs.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/widgets/dialogs.py`

## Public API

### Dialog Widget

```csharp
namespace Stroke.Widgets;

/// <summary>
/// Simple dialog window base for input, message, and confirmation dialogs.
/// Title and body can be changed at runtime.
/// </summary>
public sealed class Dialog : IContainer
{
    /// <summary>
    /// Creates a dialog.
    /// </summary>
    /// <param name="body">Child container for dialog content.</param>
    /// <param name="title">Text displayed in the dialog heading.</param>
    /// <param name="buttons">Buttons displayed at the bottom.</param>
    /// <param name="modal">Whether the dialog captures all input.</param>
    /// <param name="width">Dialog width.</param>
    /// <param name="withBackground">Whether to show a background overlay.</param>
    public Dialog(
        IContainer body,
        AnyFormattedText? title = null,
        IReadOnlyList<Button>? buttons = null,
        bool modal = true,
        Dimension? width = null,
        bool withBackground = false);

    /// <summary>
    /// The dialog body content.
    /// </summary>
    public IContainer Body { get; set; }

    /// <summary>
    /// The dialog title.
    /// </summary>
    public AnyFormattedText? Title { get; set; }
}
```

### Dialog Shortcut Functions

```csharp
namespace Stroke.Shortcuts;

public static class Dialogs
{
    /// <summary>
    /// Display a Yes/No confirmation dialog.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="text">Message text.</param>
    /// <param name="yesText">Text for Yes button.</param>
    /// <param name="noText">Text for No button.</param>
    /// <param name="style">Optional style.</param>
    /// <returns>Application that returns true for Yes, false for No.</returns>
    public static Application<bool> YesNoDialog(
        AnyFormattedText? title = null,
        AnyFormattedText? text = null,
        string yesText = "Yes",
        string noText = "No",
        IStyle? style = null);

    /// <summary>
    /// Display a dialog with custom button choices.
    /// </summary>
    /// <typeparam name="T">Type of value associated with each button.</typeparam>
    /// <param name="title">Dialog title.</param>
    /// <param name="text">Message text.</param>
    /// <param name="buttons">List of (buttonText, value) tuples.</param>
    /// <param name="style">Optional style.</param>
    /// <returns>Application that returns the selected button's value.</returns>
    public static Application<T> ButtonDialog<T>(
        AnyFormattedText? title = null,
        AnyFormattedText? text = null,
        IReadOnlyList<(string Text, T Value)>? buttons = null,
        IStyle? style = null);

    /// <summary>
    /// Display a text input dialog.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="text">Prompt text.</param>
    /// <param name="okText">Text for OK button.</param>
    /// <param name="cancelText">Text for Cancel button.</param>
    /// <param name="completer">Optional completer for input.</param>
    /// <param name="validator">Optional validator.</param>
    /// <param name="password">Whether to hide input.</param>
    /// <param name="style">Optional style.</param>
    /// <param name="default">Default text value.</param>
    /// <returns>Application that returns entered text or null if cancelled.</returns>
    public static Application<string?> InputDialog(
        AnyFormattedText? title = null,
        AnyFormattedText? text = null,
        string okText = "OK",
        string cancelText = "Cancel",
        ICompleter? completer = null,
        IValidator? validator = null,
        bool password = false,
        IStyle? style = null,
        string @default = "");

    /// <summary>
    /// Display a simple message dialog.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="text">Message text.</param>
    /// <param name="okText">Text for OK button.</param>
    /// <param name="style">Optional style.</param>
    /// <returns>Application that returns when dismissed.</returns>
    public static Application<object?> MessageDialog(
        AnyFormattedText? title = null,
        AnyFormattedText? text = null,
        string okText = "Ok",
        IStyle? style = null);

    /// <summary>
    /// Display a single-selection radio list dialog.
    /// </summary>
    /// <typeparam name="T">Type of values.</typeparam>
    /// <param name="title">Dialog title.</param>
    /// <param name="text">Description text.</param>
    /// <param name="okText">Text for OK button.</param>
    /// <param name="cancelText">Text for Cancel button.</param>
    /// <param name="values">List of (value, displayText) options.</param>
    /// <param name="default">Default selected value.</param>
    /// <param name="style">Optional style.</param>
    /// <returns>Application that returns selected value or null if cancelled.</returns>
    public static Application<T?> RadioListDialog<T>(
        AnyFormattedText? title = null,
        AnyFormattedText? text = null,
        string okText = "Ok",
        string cancelText = "Cancel",
        IReadOnlyList<(T Value, AnyFormattedText Display)>? values = null,
        T? @default = default,
        IStyle? style = null);

    /// <summary>
    /// Display a multi-selection checkbox list dialog.
    /// </summary>
    /// <typeparam name="T">Type of values.</typeparam>
    /// <param name="title">Dialog title.</param>
    /// <param name="text">Description text.</param>
    /// <param name="okText">Text for OK button.</param>
    /// <param name="cancelText">Text for Cancel button.</param>
    /// <param name="values">List of (value, displayText) options.</param>
    /// <param name="defaultValues">Initially selected values.</param>
    /// <param name="style">Optional style.</param>
    /// <returns>Application that returns list of selected values or null if cancelled.</returns>
    public static Application<IReadOnlyList<T>?> CheckboxListDialog<T>(
        AnyFormattedText? title = null,
        AnyFormattedText? text = null,
        string okText = "Ok",
        string cancelText = "Cancel",
        IReadOnlyList<(T Value, AnyFormattedText Display)>? values = null,
        IReadOnlyList<T>? defaultValues = null,
        IStyle? style = null);

    /// <summary>
    /// Display a progress dialog with callback for updates.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="text">Description text.</param>
    /// <param name="runCallback">Callback receiving setPercentage and logText functions.</param>
    /// <param name="style">Optional style.</param>
    /// <returns>Application that runs until callback completes.</returns>
    public static Application<object?> ProgressDialog(
        AnyFormattedText? title = null,
        AnyFormattedText? text = null,
        Action<Action<int>, Action<string>>? runCallback = null,
        IStyle? style = null);
}
```

## Project Structure

```
src/Stroke/
├── Widgets/
│   └── Dialog.cs
└── Shortcuts/
    └── Dialogs.cs
tests/Stroke.Tests/
├── Widgets/
│   └── DialogTests.cs
└── Shortcuts/
    └── DialogsTests.cs
```

## Implementation Notes

### Dialog Widget

```csharp
public sealed class Dialog : IContainer
{
    private readonly Box _container;

    public Dialog(
        IContainer body,
        AnyFormattedText? title = null,
        IReadOnlyList<Button>? buttons = null,
        bool modal = true,
        Dimension? width = null,
        bool withBackground = false)
    {
        Body = body;
        Title = title;
        buttons ??= Array.Empty<Button>();

        // Key bindings for button navigation
        var buttonsKb = new KeyBindings();
        if (buttons.Count > 1)
        {
            var firstSelected = Filters.HasFocus(buttons[0]);
            var lastSelected = Filters.HasFocus(buttons[^1]);

            buttonsKb.Add("left", FocusFunctions.FocusPrevious,
                filter: ~firstSelected);
            buttonsKb.Add("right", FocusFunctions.FocusNext,
                filter: ~lastSelected);
        }

        IContainer frameBody;
        if (buttons.Count > 0)
        {
            frameBody = new HSplit(
                // Body with padding
                new Box(
                    body: new DynamicContainer(() => Body),
                    padding: Dimension.Preferred(1, max: 1),
                    paddingBottom: Dimension.Exact(0)),
                // Buttons
                new Box(
                    body: new VSplit(buttons,
                        padding: 1,
                        keyBindings: buttonsKb),
                    height: Dimension.Between(1, 3, preferred: 3)));
        }
        else
        {
            frameBody = body;
        }

        // Key bindings for dialog
        var kb = new KeyBindings();
        kb.Add("tab", FocusFunctions.FocusNext,
            filter: ~Filters.HasCompletions);
        kb.Add("s-tab", FocusFunctions.FocusPrevious,
            filter: ~Filters.HasCompletions);

        var frame = new Shadow(
            new Frame(
                title: () => Title,
                body: frameBody,
                style: "class:dialog.body",
                width: withBackground ? null : width,
                keyBindings: kb,
                modal: modal));

        _container = withBackground
            ? new Box(frame, style: "class:dialog", width: width)
            : frame;
    }

    public IContainer Body { get; set; }
    public AnyFormattedText? Title { get; set; }

    public IContainer Container => _container;
}
```

### YesNoDialog

```csharp
public static Application<bool> YesNoDialog(
    AnyFormattedText? title = null,
    AnyFormattedText? text = null,
    string yesText = "Yes",
    string noText = "No",
    IStyle? style = null)
{
    void YesHandler() => Application.Current!.Exit(true);
    void NoHandler() => Application.Current!.Exit(false);

    var dialog = new Dialog(
        title: title,
        body: new Label(text: text, dontExtendHeight: true),
        buttons: new[]
        {
            new Button(text: yesText, handler: YesHandler),
            new Button(text: noText, handler: NoHandler)
        },
        withBackground: true);

    return CreateApp(dialog, style);
}
```

### InputDialog

```csharp
public static Application<string?> InputDialog(
    AnyFormattedText? title = null,
    AnyFormattedText? text = null,
    string okText = "OK",
    string cancelText = "Cancel",
    ICompleter? completer = null,
    IValidator? validator = null,
    bool password = false,
    IStyle? style = null,
    string @default = "")
{
    Button okButton = null!;
    TextArea textfield = null!;

    bool Accept(Buffer buf)
    {
        Application.Current!.Layout.Focus(okButton);
        return true; // Keep text
    }

    void OkHandler() => Application.Current!.Exit(textfield.Text);
    void CancelHandler() => Application.Current!.Exit<string?>(null);

    okButton = new Button(text: okText, handler: OkHandler);
    var cancelButton = new Button(text: cancelText, handler: CancelHandler);

    textfield = new TextArea(
        text: @default,
        multiline: false,
        password: password,
        completer: completer,
        validator: validator,
        acceptHandler: Accept);

    var dialog = new Dialog(
        title: title,
        body: new HSplit(
            new Label(text: text, dontExtendHeight: true),
            textfield,
            new ValidationToolbar(),
            padding: Dimension.Preferred(1, max: 1)),
        buttons: new[] { okButton, cancelButton },
        withBackground: true);

    return CreateApp(dialog, style);
}
```

### RadioListDialog

```csharp
public static Application<T?> RadioListDialog<T>(
    AnyFormattedText? title = null,
    AnyFormattedText? text = null,
    string okText = "Ok",
    string cancelText = "Cancel",
    IReadOnlyList<(T Value, AnyFormattedText Display)>? values = null,
    T? @default = default,
    IStyle? style = null)
{
    values ??= Array.Empty<(T, AnyFormattedText)>();

    var radioList = new RadioList<T>(values: values, @default: @default);

    void OkHandler() => Application.Current!.Exit(radioList.CurrentValue);
    void CancelHandler() => Application.Current!.Exit<T?>(default);

    var dialog = new Dialog(
        title: title,
        body: new HSplit(
            new Label(text: text, dontExtendHeight: true),
            radioList,
            padding: 1),
        buttons: new[]
        {
            new Button(text: okText, handler: OkHandler),
            new Button(text: cancelText, handler: CancelHandler)
        },
        withBackground: true);

    return CreateApp(dialog, style);
}
```

### ProgressDialog

```csharp
public static Application<object?> ProgressDialog(
    AnyFormattedText? title = null,
    AnyFormattedText? text = null,
    Action<Action<int>, Action<string>>? runCallback = null,
    IStyle? style = null)
{
    runCallback ??= (_, _) => { };

    var progressBar = new ProgressBarWidget();
    var textArea = new TextArea(
        focusable: false,
        height: Dimension.Preferred(int.MaxValue));

    var dialog = new Dialog(
        title: title,
        body: new HSplit(
            new Box(new Label(text: text)),
            new Box(textArea, padding: Dimension.Exact(1)),
            progressBar),
        withBackground: true);

    var app = CreateApp(dialog, style);

    void SetPercentage(int value)
    {
        progressBar.Percentage = value;
        app.Invalidate();
    }

    void LogText(string logText)
    {
        app.Loop?.CallSoonThreadsafe(() =>
        {
            textArea.Buffer.InsertText(logText);
            app.Invalidate();
        });
    }

    void Start()
    {
        try
        {
            runCallback(SetPercentage, LogText);
        }
        finally
        {
            app.Exit();
        }
    }

    app.PreRunCallables.Add(() =>
        EventLoopUtils.RunInExecutorWithContextAsync(Start));

    return app;
}
```

### CreateApp Helper

```csharp
private static Application<T> CreateApp<T>(
    IContainer dialog,
    IStyle? style)
{
    var bindings = new KeyBindings();
    bindings.Add("tab", FocusFunctions.FocusNext);
    bindings.Add("s-tab", FocusFunctions.FocusPrevious);

    return new Application<T>(
        layout: new Layout(dialog),
        keyBindings: KeyBindings.Merge(
            DefaultKeyBindings.Load(),
            bindings),
        mouseSupport: true,
        style: style,
        fullScreen: true);
}
```

### Usage Examples

```csharp
// Yes/No confirmation
var result = await Dialogs.YesNoDialog(
    title: "Confirm Delete",
    text: "Are you sure you want to delete this file?").RunAsync();

if (result)
{
    File.Delete(path);
}

// Button dialog
var choice = await Dialogs.ButtonDialog(
    title: "Select Action",
    text: "What would you like to do?",
    buttons: new[]
    {
        ("Save", "save"),
        ("Save As", "saveas"),
        ("Don't Save", "discard"),
        ("Cancel", "cancel")
    }).RunAsync();

// Input dialog
var name = await Dialogs.InputDialog(
    title: "Enter Name",
    text: "Please enter your name:",
    @default: "John").RunAsync();

// Radio list
var color = await Dialogs.RadioListDialog(
    title: "Select Color",
    text: "Choose your favorite color:",
    values: new[]
    {
        ("red", "Red"),
        ("green", "Green"),
        ("blue", "Blue")
    }).RunAsync();

// Checkbox list
var options = await Dialogs.CheckboxListDialog(
    title: "Select Options",
    text: "Choose which features to enable:",
    values: new[]
    {
        ("logging", "Enable Logging"),
        ("caching", "Enable Caching"),
        ("metrics", "Enable Metrics")
    }).RunAsync();

// Progress dialog
await Dialogs.ProgressDialog(
    title: "Processing",
    text: "Processing files...",
    runCallback: (setProgress, log) =>
    {
        for (var i = 0; i <= 100; i += 10)
        {
            setProgress(i);
            log($"Processing step {i / 10}...\n");
            Thread.Sleep(500);
        }
    }).RunAsync();
```

## Dependencies

- `Stroke.Widgets.Button` (Feature 41) - Button widget
- `Stroke.Widgets.Label` (Feature 42) - Label widget
- `Stroke.Widgets.TextArea` (Feature 40) - Text input
- `Stroke.Widgets.RadioList` (Feature 43) - Radio selection
- `Stroke.Widgets.CheckboxList` (Feature 44) - Checkbox selection
- `Stroke.Widgets.Frame` (Feature 45) - Frame container
- `Stroke.Widgets.Shadow` (Feature 46) - Shadow effect
- `Stroke.Widgets.Box` (Feature 47) - Box container
- `Stroke.Widgets.ValidationToolbar` (Feature 48) - Validation display
- `Stroke.Widgets.ProgressBar` (Feature 56) - Progress bar widget
- `Stroke.Layout.HSplit` (Feature 22) - Vertical stacking
- `Stroke.Layout.VSplit` (Feature 22) - Horizontal stacking
- `Stroke.Application` (Feature 37) - Application lifecycle
- `Stroke.KeyBinding` (Feature 19) - Key bindings

## Implementation Tasks

1. Implement `Dialog` widget with frame and buttons
2. Implement button navigation key bindings
3. Implement `YesNoDialog` function
4. Implement `ButtonDialog<T>` function
5. Implement `InputDialog` function
6. Implement `MessageDialog` function
7. Implement `RadioListDialog<T>` function
8. Implement `CheckboxListDialog<T>` function
9. Implement `ProgressDialog` function
10. Implement `CreateApp` helper
11. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Dialog displays title, body, and buttons
- [ ] Tab/Shift-Tab navigates between focusable elements
- [ ] Left/Right arrows navigate between buttons
- [ ] YesNoDialog returns true for Yes, false for No
- [ ] ButtonDialog returns value associated with clicked button
- [ ] InputDialog returns text or null on cancel
- [ ] InputDialog validates input when validator provided
- [ ] MessageDialog closes on OK click
- [ ] RadioListDialog returns selected value
- [ ] CheckboxListDialog returns list of selected values
- [ ] ProgressDialog updates percentage
- [ ] ProgressDialog logs text to text area
- [ ] Dialogs render with background when specified
- [ ] Modal dialogs capture all input
- [ ] Unit tests achieve 80% coverage
