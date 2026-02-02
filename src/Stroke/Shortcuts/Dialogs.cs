using Stroke.Application;
using Stroke.Application.Bindings;
using Stroke.Completion;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Styles;
using Stroke.Validation;
using Stroke.Widgets.Base;
using Stroke.Widgets.Dialogs;
using Stroke.Widgets.Lists;
using Stroke.Widgets.Toolbars;

namespace Stroke.Shortcuts;

/// <summary>
/// Pre-built dialog shortcut functions for common interactive patterns.
/// Each method composes widget primitives into a ready-to-run Application.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.shortcuts.dialogs</c> module.
/// </para>
/// <para>
/// Factory methods (e.g., <c>YesNoDialog</c>) return <c>Application&lt;T&gt;</c> objects
/// that can be run with <c>RunAsync()</c>. Async convenience methods (e.g.,
/// <c>YesNoDialogAsync</c>) create and run the application in one call.
/// </para>
/// </remarks>
public static class Dialogs
{
    /// <summary>
    /// Wrap a dialog container in an Application with merged key bindings,
    /// mouse support, optional styling, and full-screen mode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Port of Python Prompt Toolkit's <c>_create_app</c> function.
    /// Python's <c>AnyContainer</c> parameter maps to <c>IContainer</c> in C#.
    /// </para>
    /// <para>
    /// Key binding priority: <c>DefaultKeyBindings.Load()</c> is first (lower priority),
    /// dialog tab/shift-tab bindings are second (higher priority, last wins in KeyProcessor).
    /// </para>
    /// </remarks>
    private static Application<T> CreateApp<T>(IContainer dialog, IStyle? style)
    {
        var bindings = new KeyBindings();
        bindings.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlI)])(FocusFunctions.FocusNext);
        bindings.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.BackTab)])(FocusFunctions.FocusPrevious);

        return new Application<T>(
            layout: new Layout.Layout(new AnyContainer(dialog)),
            keyBindings: new MergedKeyBindings(DefaultKeyBindings.Load(), bindings),
            mouseSupport: true,
            style: style,
            fullScreen: true);
    }

    /// <summary>
    /// Button handler that exits the current application with no result (null/default).
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_return_none()</c>.
    /// Calls <c>AppContext.GetApp().Exit()</c> with no arguments, which exits
    /// with <c>default(T)</c> through the typed application's internal delegation.
    /// </remarks>
    private static void ReturnNone()
    {
        Application.AppContext.GetApp().Exit();
    }

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
    {
        void YesHandler() => Application.AppContext.GetApp().Exit(result: true);
        void NoHandler() => Application.AppContext.GetApp().Exit(result: false);

        var dialog = new Dialog(
            title: title,
            body: new AnyContainer(new Label(text: text, dontExtendHeight: true)),
            buttons:
            [
                new Button(text: yesText, handler: YesHandler),
                new Button(text: noText, handler: NoHandler),
            ],
            withBackground: true);

        return CreateApp<bool>(dialog.PtContainer(), style);
    }

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
    {
        var dialog = new Dialog(
            title: title,
            body: new AnyContainer(new Label(text: text, dontExtendHeight: true)),
            buttons: [new Button(text: okText, handler: ReturnNone)],
            withBackground: true);

        return CreateApp<object?>(dialog.PtContainer(), style);
    }

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
    {
        var okButton = new Button(text: okText, handler: () => { });
        var cancelButton = new Button(text: cancelText, handler: ReturnNone);

        var textfield = new TextArea(
            text: default_,
            multiline: false,
            password: password,
            completer: completer,
            validator: validator,
            acceptHandler: buf =>
            {
                Application.AppContext.GetApp().Layout.Focus(okButton.Window);
                return true; // Keep text.
            });

        void OkHandler() => Application.AppContext.GetApp().Exit(result: textfield.Text);
        okButton.Handler = OkHandler;

        var dialog = new Dialog(
            title: title,
            body: new AnyContainer(new HSplit(
                children: (IReadOnlyList<IContainer>)
                [
                    new Label(text: text, dontExtendHeight: true).PtContainer(),
                    textfield.PtContainer(),
                    new ValidationToolbar().PtContainer(),
                ],
                windowTooSmall: null,
                align: VerticalAlign.Justify,
                padding: new Dimension(preferred: 1, max: 1),
                paddingChar: null,
                paddingStyle: "",
                width: null,
                height: null,
                zIndex: null,
                modal: false,
                keyBindings: null,
                styleGetter: () => "")),
            buttons: [okButton, cancelButton],
            withBackground: true);

        return CreateApp<string?>(dialog.PtContainer(), style);
    }

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
    {
        buttons ??= [];

        var dialog = new Dialog(
            title: title,
            body: new AnyContainer(new Label(text: text, dontExtendHeight: true)),
            buttons:
            [
                ..buttons.Select(b =>
                {
                    var value = b.Value;
                    return new Button(
                        text: b.Text,
                        handler: () => Application.AppContext.GetApp().Exit(result: value));
                }),
            ],
            withBackground: true);

        return CreateApp<T>(dialog.PtContainer(), style);
    }

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
    {
        values ??= [];

        var radioList = new RadioList<T>(values: values, @default: default_);

        void OkHandler() => Application.AppContext.GetApp().Exit(result: radioList.CurrentValue);

        var dialog = new Dialog(
            title: title,
            body: new AnyContainer(new HSplit(
                children: (IReadOnlyList<IContainer>)
                [
                    new Label(text: text, dontExtendHeight: true).PtContainer(),
                    radioList.PtContainer(),
                ],
                padding: 1)),
            buttons:
            [
                new Button(text: okText, handler: OkHandler),
                new Button(text: cancelText, handler: ReturnNone),
            ],
            withBackground: true);

        return CreateApp<T?>(dialog.PtContainer(), style);
    }

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
    {
        values ??= [];

        var cbList = new CheckboxList<T>(values: values, defaultValues: defaultValues);

        void OkHandler() =>
            Application.AppContext.GetApp().Exit(
                result: (IReadOnlyList<T>?)cbList.CurrentValues.AsReadOnly());

        var dialog = new Dialog(
            title: title,
            body: new AnyContainer(new HSplit(
                children: (IReadOnlyList<IContainer>)
                [
                    new Label(text: text, dontExtendHeight: true).PtContainer(),
                    cbList.PtContainer(),
                ],
                padding: 1)),
            buttons:
            [
                new Button(text: okText, handler: OkHandler),
                new Button(text: cancelText, handler: ReturnNone),
            ],
            withBackground: true);

        return CreateApp<IReadOnlyList<T>?>(dialog.PtContainer(), style);
    }

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
    {
        var progressbar = new ProgressBar();
        var textArea = new TextArea(
            focusable: false,
            // Prefer this text area as big as possible, to avoid having a window
            // that keeps resizing when we add text to it.
            height: new Dimension(preferred: int.MaxValue));

        var dialog = new Dialog(
            body: new AnyContainer(new HSplit(
                children: (IReadOnlyList<IContainer>)
                [
                    new Box(body: new AnyContainer(new Label(text: text))).PtContainer(),
                    new Box(
                        body: new AnyContainer(textArea),
                        padding: Dimension.Exact(1)).PtContainer(),
                    progressbar.PtContainer(),
                ])),
            title: title,
            withBackground: true);

        var app = CreateApp<object?>(dialog.PtContainer(), style);

        void SetPercentage(int value)
        {
            progressbar.Percentage = value;
            app.Invalidate();
        }

        void LogText(string logMessage)
        {
            if (app._actionChannel is { } channel)
            {
                channel.Writer.TryWrite(() => textArea.Buffer.InsertText(logMessage));
                app.Invalidate();
            }
        }

        // Run the callback in a background task. When done, exit the app.
        app.PreRunCallables.Add(() =>
        {
            _ = app.CreateBackgroundTask(async ct =>
            {
                try
                {
                    await Task.Run(() => (runCallback ?? (static (_, _) => { }))(SetPercentage, LogText), ct);
                }
                finally
                {
                    app.Exit();
                }
            });
        });

        return app;
    }

    // ──────────────────────────────────────────────
    // Async convenience methods
    // ──────────────────────────────────────────────

    /// <summary>
    /// Display a Yes/No dialog and run it. Returns <c>true</c> for Yes, <c>false</c> for No.
    /// </summary>
    /// <inheritdoc cref="YesNoDialog" path="/param"/>
    public static Task<bool> YesNoDialogAsync(
        AnyFormattedText title = default,
        AnyFormattedText text = default,
        string yesText = "Yes",
        string noText = "No",
        IStyle? style = null)
        => YesNoDialog(title, text, yesText, noText, style).RunAsync();

    /// <summary>
    /// Display a simple message box and wait until the user presses Ok.
    /// </summary>
    /// <inheritdoc cref="MessageDialog" path="/param"/>
    public static async Task MessageDialogAsync(
        AnyFormattedText title = default,
        AnyFormattedText text = default,
        string okText = "Ok",
        IStyle? style = null)
        => await MessageDialog(title, text, okText, style).RunAsync();

    /// <summary>
    /// Display a text input box and run it. Returns the entered text, or null when cancelled.
    /// </summary>
    /// <inheritdoc cref="InputDialog" path="/param"/>
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
        => InputDialog(title, text, okText, cancelText, completer, validator, password, style, default_).RunAsync();

    /// <summary>
    /// Display a dialog with button choices and run it. Returns the value of the selected button.
    /// </summary>
    /// <inheritdoc cref="ButtonDialog{T}" path="/param"/>
    /// <inheritdoc cref="ButtonDialog{T}" path="/typeparam"/>
    public static Task<T> ButtonDialogAsync<T>(
        AnyFormattedText title = default,
        AnyFormattedText text = default,
        IReadOnlyList<(string Text, T Value)>? buttons = null,
        IStyle? style = null)
        => ButtonDialog(title, text, buttons, style).RunAsync();

    /// <summary>
    /// Display a single-selection radio list dialog and run it.
    /// Returns the selected value, or default on cancel.
    /// </summary>
    /// <inheritdoc cref="RadioListDialog{T}" path="/param"/>
    /// <inheritdoc cref="RadioListDialog{T}" path="/typeparam"/>
    public static Task<T?> RadioListDialogAsync<T>(
        AnyFormattedText title = default,
        AnyFormattedText text = default,
        string okText = "Ok",
        string cancelText = "Cancel",
        IReadOnlyList<(T Value, AnyFormattedText Label)>? values = null,
        T? default_ = default,
        IStyle? style = null)
        => RadioListDialog(title, text, okText, cancelText, values, default_, style).RunAsync();

    /// <summary>
    /// Display a multi-selection checkbox list dialog and run it.
    /// Returns the list of selected values, or null on cancel.
    /// </summary>
    /// <inheritdoc cref="CheckboxListDialog{T}" path="/param"/>
    /// <inheritdoc cref="CheckboxListDialog{T}" path="/typeparam"/>
    public static Task<IReadOnlyList<T>?> CheckboxListDialogAsync<T>(
        AnyFormattedText title = default,
        AnyFormattedText text = default,
        string okText = "Ok",
        string cancelText = "Cancel",
        IReadOnlyList<(T Value, AnyFormattedText Label)>? values = null,
        IReadOnlyList<T>? defaultValues = null,
        IStyle? style = null)
        => CheckboxListDialog(title, text, okText, cancelText, values, defaultValues, style).RunAsync();

    /// <summary>
    /// Display a progress dialog and run it. The callback executes on a background thread.
    /// </summary>
    /// <inheritdoc cref="ProgressDialog" path="/param"/>
    public static async Task ProgressDialogAsync(
        AnyFormattedText title = default,
        AnyFormattedText text = default,
        Action<Action<int>, Action<string>>? runCallback = null,
        IStyle? style = null)
        => await ProgressDialog(title, text, runCallback, style).RunAsync();
}
