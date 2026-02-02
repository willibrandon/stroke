using Stroke.Application;
using Stroke.AutoSuggest;
using Stroke.Clipboard;
using Stroke.Completion;
using Stroke.CursorShapes;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.History;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Stroke.Layout.Processors;
using Stroke.Lexers;
using Stroke.Output;
using Stroke.Styles;
using Stroke.Validation;

namespace Stroke.Shortcuts;

/// <summary>
/// Provides static convenience functions for common prompt operations.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's module-level <c>prompt</c>, <c>confirm</c>,
/// and <c>create_confirm_session</c> functions from <c>prompt_toolkit.shortcuts.prompt</c>.
/// </remarks>
public static class Prompt
{
    /// <summary>
    /// Display a prompt and return the user's input (blocking). Creates a new
    /// <c>PromptSession&lt;string&gt;</c> for each call.
    /// </summary>
    /// <remarks>
    /// <para>Port of Python Prompt Toolkit's <c>prompt()</c> function.</para>
    /// <para>Named <c>RunPrompt</c> instead of <c>Prompt</c> because C# CS0542 prohibits
    /// member names matching the enclosing type. The async equivalent is
    /// <see cref="PromptAsync"/>.</para>
    /// </remarks>
    public static string RunPrompt(
        AnyFormattedText? message = null,
        IHistory? history = null,
        EditingMode? editingMode = null,
        double? refreshInterval = null,
        bool? viMode = null,
        ILexer? lexer = null,
        ICompleter? completer = null,
        bool? completeInThread = null,
        bool? isPassword = null,
        IKeyBindingsBase? keyBindings = null,
        AnyFormattedText? bottomToolbar = null,
        IStyle? style = null,
        ColorDepth? colorDepth = null,
        ICursorShapeConfig? cursor = null,
        FilterOrBool? includeDefaultPygmentsStyle = null,
        IStyleTransformation? styleTransformation = null,
        FilterOrBool? swapLightAndDarkColors = null,
        AnyFormattedText? rprompt = null,
        FilterOrBool? multiline = null,
        object? promptContinuation = null,
        FilterOrBool? wrapLines = null,
        FilterOrBool? enableHistorySearch = null,
        FilterOrBool? searchIgnoreCase = null,
        FilterOrBool? completeWhileTyping = null,
        FilterOrBool? validateWhileTyping = null,
        CompleteStyle? completeStyle = null,
        IAutoSuggest? autoSuggest = null,
        IValidator? validator = null,
        IClipboard? clipboard = null,
        FilterOrBool? mouseSupport = null,
        IReadOnlyList<IProcessor>? inputProcessors = null,
        AnyFormattedText? placeholder = null,
        int? reserveSpaceForMenu = null,
        FilterOrBool? enableSystemPrompt = null,
        FilterOrBool? enableSuspend = null,
        FilterOrBool? enableOpenInEditor = null,
        object? tempfileSuffix = null,
        object? tempfile = null,
        FilterOrBool? showFrame = null,
        string default_ = "",
        bool acceptDefault = false,
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true,
        bool inThread = false,
        InputHook? inputHook = null)
    {
        var session = new PromptSession<string>(history: history);
        return session.Prompt(
            message: message,
            multiline: multiline,
            wrapLines: wrapLines,
            isPassword: isPassword is not null ? new FilterOrBool(isPassword.Value) : (FilterOrBool?)null,
            viMode: viMode,
            editingMode: editingMode,
            completeWhileTyping: completeWhileTyping,
            validateWhileTyping: validateWhileTyping,
            enableHistorySearch: enableHistorySearch,
            searchIgnoreCase: searchIgnoreCase,
            lexer: lexer,
            enableSystemPrompt: enableSystemPrompt,
            enableSuspend: enableSuspend,
            enableOpenInEditor: enableOpenInEditor,
            validator: validator,
            completer: completer,
            completeInThread: completeInThread,
            reserveSpaceForMenu: reserveSpaceForMenu,
            completeStyle: completeStyle,
            autoSuggest: autoSuggest,
            style: style,
            styleTransformation: styleTransformation,
            swapLightAndDarkColors: swapLightAndDarkColors,
            colorDepth: colorDepth,
            cursor: cursor,
            includeDefaultPygmentsStyle: includeDefaultPygmentsStyle,
            clipboard: clipboard,
            promptContinuation: promptContinuation,
            rprompt: rprompt,
            bottomToolbar: bottomToolbar,
            mouseSupport: mouseSupport,
            inputProcessors: inputProcessors,
            placeholder: placeholder,
            keyBindings: keyBindings,
            tempfileSuffix: tempfileSuffix,
            tempfile: tempfile,
            refreshInterval: refreshInterval,
            showFrame: showFrame,
            default_: default_,
            acceptDefault: acceptDefault,
            preRun: preRun,
            setExceptionHandler: setExceptionHandler,
            handleSigint: handleSigint,
            inThread: inThread,
            inputHook: inputHook);
    }

    /// <summary>
    /// Display a prompt and return the user's input asynchronously.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's async equivalent. Creates a new session per call.
    /// </remarks>
    public static Task<string> PromptAsync(
        AnyFormattedText? message = null,
        IHistory? history = null,
        EditingMode? editingMode = null,
        double? refreshInterval = null,
        bool? viMode = null,
        ILexer? lexer = null,
        ICompleter? completer = null,
        bool? completeInThread = null,
        bool? isPassword = null,
        IKeyBindingsBase? keyBindings = null,
        AnyFormattedText? bottomToolbar = null,
        IStyle? style = null,
        ColorDepth? colorDepth = null,
        ICursorShapeConfig? cursor = null,
        FilterOrBool? includeDefaultPygmentsStyle = null,
        IStyleTransformation? styleTransformation = null,
        FilterOrBool? swapLightAndDarkColors = null,
        AnyFormattedText? rprompt = null,
        FilterOrBool? multiline = null,
        object? promptContinuation = null,
        FilterOrBool? wrapLines = null,
        FilterOrBool? enableHistorySearch = null,
        FilterOrBool? searchIgnoreCase = null,
        FilterOrBool? completeWhileTyping = null,
        FilterOrBool? validateWhileTyping = null,
        CompleteStyle? completeStyle = null,
        IAutoSuggest? autoSuggest = null,
        IValidator? validator = null,
        IClipboard? clipboard = null,
        FilterOrBool? mouseSupport = null,
        IReadOnlyList<IProcessor>? inputProcessors = null,
        AnyFormattedText? placeholder = null,
        int? reserveSpaceForMenu = null,
        FilterOrBool? enableSystemPrompt = null,
        FilterOrBool? enableSuspend = null,
        FilterOrBool? enableOpenInEditor = null,
        object? tempfileSuffix = null,
        object? tempfile = null,
        FilterOrBool? showFrame = null,
        string default_ = "",
        bool acceptDefault = false,
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true)
    {
        var session = new PromptSession<string>(history: history);
        return session.PromptAsync(
            message: message,
            multiline: multiline,
            wrapLines: wrapLines,
            isPassword: isPassword is not null ? new FilterOrBool(isPassword.Value) : (FilterOrBool?)null,
            viMode: viMode,
            editingMode: editingMode,
            completeWhileTyping: completeWhileTyping,
            validateWhileTyping: validateWhileTyping,
            enableHistorySearch: enableHistorySearch,
            searchIgnoreCase: searchIgnoreCase,
            lexer: lexer,
            enableSystemPrompt: enableSystemPrompt,
            enableSuspend: enableSuspend,
            enableOpenInEditor: enableOpenInEditor,
            validator: validator,
            completer: completer,
            completeInThread: completeInThread,
            reserveSpaceForMenu: reserveSpaceForMenu,
            completeStyle: completeStyle,
            autoSuggest: autoSuggest,
            style: style,
            styleTransformation: styleTransformation,
            swapLightAndDarkColors: swapLightAndDarkColors,
            colorDepth: colorDepth,
            cursor: cursor,
            includeDefaultPygmentsStyle: includeDefaultPygmentsStyle,
            clipboard: clipboard,
            promptContinuation: promptContinuation,
            rprompt: rprompt,
            bottomToolbar: bottomToolbar,
            mouseSupport: mouseSupport,
            inputProcessors: inputProcessors,
            placeholder: placeholder,
            keyBindings: keyBindings,
            tempfileSuffix: tempfileSuffix,
            tempfile: tempfile,
            refreshInterval: refreshInterval,
            showFrame: showFrame,
            default_: default_,
            acceptDefault: acceptDefault,
            preRun: preRun,
            setExceptionHandler: setExceptionHandler,
            handleSigint: handleSigint);
    }

    /// <summary>
    /// Create a <see cref="PromptSession{TResult}"/> configured for yes/no confirmation.
    /// </summary>
    /// <param name="message">The confirmation message to display.</param>
    /// <param name="suffix">The suffix appended to the message (e.g., " (y/n) ").</param>
    /// <returns>A <c>PromptSession&lt;bool&gt;</c> that accepts y/Y (true) or n/N (false).</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>create_confirm_session</c>.
    /// </remarks>
    public static PromptSession<bool> CreateConfirmSession(
        AnyFormattedText message,
        string suffix = " (y/n) ")
    {
        var bindings = new KeyBindings();

        // y/Y → true
        bindings.Add<KeyHandlerCallable>(
            [new KeyOrChar('y')])(@event =>
        {
            @event.GetApp().CurrentBuffer.Document = new Core.Document("y");
            @event.GetApp().Exit(result: true);
            return null;
        });

        bindings.Add<KeyHandlerCallable>(
            [new KeyOrChar('Y')])(@event =>
        {
            @event.GetApp().CurrentBuffer.Document = new Core.Document("y");
            @event.GetApp().Exit(result: true);
            return null;
        });

        // n/N → false
        bindings.Add<KeyHandlerCallable>(
            [new KeyOrChar('n')])(@event =>
        {
            @event.GetApp().CurrentBuffer.Document = new Core.Document("n");
            @event.GetApp().Exit(result: false);
            return null;
        });

        bindings.Add<KeyHandlerCallable>(
            [new KeyOrChar('N')])(@event =>
        {
            @event.GetApp().CurrentBuffer.Document = new Core.Document("n");
            @event.GetApp().Exit(result: false);
            return null;
        });

        // Any other key → no-op
        bindings.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Any)])(@event =>
        {
            return null;
        });

        // Merge message + suffix
        var completeMessage = FormattedTextUtils.Merge([message, (AnyFormattedText)suffix]);

        return new PromptSession<bool>(
            message: completeMessage(),
            keyBindings: bindings);
    }

    /// <summary>
    /// Display a confirmation prompt that returns true (y/Y) or false (n/N).
    /// </summary>
    /// <param name="message">The confirmation message. Defaults to "Confirm?".</param>
    /// <param name="suffix">The suffix appended to the message.</param>
    /// <returns><c>true</c> if the user confirms; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>confirm()</c>.
    /// </remarks>
    public static bool Confirm(
        AnyFormattedText message = default,
        string suffix = " (y/n) ")
    {
        if (message.IsEmpty)
        {
            message = "Confirm?";
        }

        return CreateConfirmSession(message, suffix).Prompt();
    }

    /// <summary>
    /// Display a confirmation prompt asynchronously.
    /// </summary>
    /// <param name="message">The confirmation message. Defaults to "Confirm?".</param>
    /// <param name="suffix">The suffix appended to the message.</param>
    /// <returns>A task that resolves to <c>true</c> if the user confirms; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// C# addition for async parity — not present in Python Prompt Toolkit.
    /// </remarks>
    public static Task<bool> ConfirmAsync(
        AnyFormattedText message = default,
        string suffix = " (y/n) ")
    {
        if (message.IsEmpty)
        {
            message = "Confirm?";
        }

        return CreateConfirmSession(message, suffix).PromptAsync();
    }
}
