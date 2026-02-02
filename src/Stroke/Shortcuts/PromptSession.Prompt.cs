using Stroke.Application;
using Stroke.AutoSuggest;
using Stroke.Clipboard;
using Stroke.Completion;
using Stroke.Core;
using Stroke.CursorShapes;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Processors;
using Stroke.Lexers;
using Stroke.Output;
using Stroke.Styles;
using Stroke.Validation;

using Buffer = Stroke.Core.Buffer;

namespace Stroke.Shortcuts;

public partial class PromptSession<TResult>
{
    /// <summary>
    /// Display the prompt and return the user's input (blocking).
    /// </summary>
    /// <remarks>
    /// <para>Port of Python Prompt Toolkit's <c>PromptSession.prompt</c>.</para>
    /// <para>Non-null parameters update session state permanently (current + future calls).</para>
    /// </remarks>
    public TResult Prompt(
        AnyFormattedText? message = null,
        FilterOrBool? multiline = null,
        FilterOrBool? wrapLines = null,
        FilterOrBool? isPassword = null,
        bool? viMode = null,
        EditingMode? editingMode = null,
        FilterOrBool? completeWhileTyping = null,
        FilterOrBool? validateWhileTyping = null,
        FilterOrBool? enableHistorySearch = null,
        FilterOrBool? searchIgnoreCase = null,
        ILexer? lexer = null,
        FilterOrBool? enableSystemPrompt = null,
        FilterOrBool? enableSuspend = null,
        FilterOrBool? enableOpenInEditor = null,
        IValidator? validator = null,
        ICompleter? completer = null,
        bool? completeInThread = null,
        int? reserveSpaceForMenu = null,
        CompleteStyle? completeStyle = null,
        IAutoSuggest? autoSuggest = null,
        IStyle? style = null,
        IStyleTransformation? styleTransformation = null,
        FilterOrBool? swapLightAndDarkColors = null,
        ColorDepth? colorDepth = null,
        ICursorShapeConfig? cursor = null,
        FilterOrBool? includeDefaultPygmentsStyle = null,
        IClipboard? clipboard = null,
        object? promptContinuation = null,
        AnyFormattedText? rprompt = null,
        AnyFormattedText? bottomToolbar = null,
        FilterOrBool? mouseSupport = null,
        IReadOnlyList<IProcessor>? inputProcessors = null,
        AnyFormattedText? placeholder = null,
        IKeyBindingsBase? keyBindings = null,
        object? tempfileSuffix = null,
        object? tempfile = null,
        double? refreshInterval = null,
        FilterOrBool? showFrame = null,
        object default_ = default!,
        bool acceptDefault = false,
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true,
        bool inThread = false,
        InputHook? inputHook = null)
    {
        // Apply per-prompt overrides (non-null values update session state permanently)
        ApplyOverrides(
            message, multiline, wrapLines, isPassword, viMode, editingMode,
            completeWhileTyping, validateWhileTyping, enableHistorySearch,
            searchIgnoreCase, lexer, enableSystemPrompt, enableSuspend,
            enableOpenInEditor, validator, completer, completeInThread,
            reserveSpaceForMenu, completeStyle, autoSuggest, style,
            styleTransformation, swapLightAndDarkColors, colorDepth, cursor,
            includeDefaultPygmentsStyle, clipboard, promptContinuation,
            rprompt, bottomToolbar, mouseSupport, inputProcessors, placeholder,
            keyBindings, tempfileSuffix, tempfile, refreshInterval, showFrame);

        // Add pre-run callables, then reset buffer (matching Python source ordering)
        AddPreRunCallables(preRun, acceptDefault);
        var defaultDoc = ResolveDefaultDocument(default_);
        DefaultBuffer.Reset(document: defaultDoc);

        // Set app refresh interval
        App.RefreshInterval = RefreshInterval == 0 ? null : RefreshInterval;

        // Dumb terminal check
        if (_output is null && PlatformUtils.IsDumbTerminal())
        {
            return DumbPromptRun(setExceptionHandler, handleSigint, inThread);
        }

        // Run the application
        return App.Run(
            setExceptionHandler: setExceptionHandler,
            handleSigint: handleSigint,
            inThread: inThread,
            inputHook: inputHook);
    }

    /// <summary>
    /// Display the prompt and return the user's input (async).
    /// </summary>
    /// <remarks>
    /// <para>Port of Python Prompt Toolkit's <c>PromptSession.prompt_async</c>.</para>
    /// <para>Non-null parameters update session state permanently (current + future calls).</para>
    /// </remarks>
    public async Task<TResult> PromptAsync(
        AnyFormattedText? message = null,
        FilterOrBool? multiline = null,
        FilterOrBool? wrapLines = null,
        FilterOrBool? isPassword = null,
        bool? viMode = null,
        EditingMode? editingMode = null,
        FilterOrBool? completeWhileTyping = null,
        FilterOrBool? validateWhileTyping = null,
        FilterOrBool? enableHistorySearch = null,
        FilterOrBool? searchIgnoreCase = null,
        ILexer? lexer = null,
        FilterOrBool? enableSystemPrompt = null,
        FilterOrBool? enableSuspend = null,
        FilterOrBool? enableOpenInEditor = null,
        IValidator? validator = null,
        ICompleter? completer = null,
        bool? completeInThread = null,
        int? reserveSpaceForMenu = null,
        CompleteStyle? completeStyle = null,
        IAutoSuggest? autoSuggest = null,
        IStyle? style = null,
        IStyleTransformation? styleTransformation = null,
        FilterOrBool? swapLightAndDarkColors = null,
        ColorDepth? colorDepth = null,
        ICursorShapeConfig? cursor = null,
        FilterOrBool? includeDefaultPygmentsStyle = null,
        IClipboard? clipboard = null,
        object? promptContinuation = null,
        AnyFormattedText? rprompt = null,
        AnyFormattedText? bottomToolbar = null,
        FilterOrBool? mouseSupport = null,
        IReadOnlyList<IProcessor>? inputProcessors = null,
        AnyFormattedText? placeholder = null,
        IKeyBindingsBase? keyBindings = null,
        object? tempfileSuffix = null,
        object? tempfile = null,
        double? refreshInterval = null,
        FilterOrBool? showFrame = null,
        object default_ = default!,
        bool acceptDefault = false,
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true)
    {
        // Apply per-prompt overrides
        ApplyOverrides(
            message, multiline, wrapLines, isPassword, viMode, editingMode,
            completeWhileTyping, validateWhileTyping, enableHistorySearch,
            searchIgnoreCase, lexer, enableSystemPrompt, enableSuspend,
            enableOpenInEditor, validator, completer, completeInThread,
            reserveSpaceForMenu, completeStyle, autoSuggest, style,
            styleTransformation, swapLightAndDarkColors, colorDepth, cursor,
            includeDefaultPygmentsStyle, clipboard, promptContinuation,
            rprompt, bottomToolbar, mouseSupport, inputProcessors, placeholder,
            keyBindings, tempfileSuffix, tempfile, refreshInterval, showFrame);

        // Add pre-run callables, then reset buffer (matching Python source ordering)
        AddPreRunCallables(preRun, acceptDefault);
        var defaultDoc = ResolveDefaultDocument(default_);
        DefaultBuffer.Reset(document: defaultDoc);

        // Set app refresh interval
        App.RefreshInterval = RefreshInterval == 0 ? null : RefreshInterval;

        // Dumb terminal check
        if (_output is null && PlatformUtils.IsDumbTerminal())
        {
            return await DumbPromptRunAsync(setExceptionHandler, handleSigint);
        }

        // Run the application
        return await App.RunAsync(
            setExceptionHandler: setExceptionHandler,
            handleSigint: handleSigint);
    }

    /// <summary>
    /// Appends pre-run callables to the application's pre-run list.
    /// </summary>
    /// <param name="preRun">Optional user-provided pre-run callback.</param>
    /// <param name="acceptDefault">If true, auto-accept the default value via CallSoon.</param>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_add_pre_run_callables</c>.
    /// </remarks>
    private void AddPreRunCallables(Action? preRun, bool acceptDefault)
    {
        App.PreRunCallables.Add(() =>
        {
            // Execute user's pre-run first
            preRun?.Invoke();

            // If acceptDefault, schedule validation and accept on the event loop.
            // Python uses get_running_loop().call_soon() which stays on the same thread.
            // We marshal via the Application's action channel to preserve single-threaded semantics.
            if (acceptDefault)
            {
                App._actionChannel?.Writer.TryWrite(() => DefaultBuffer.ValidateAndHandle());
            }
        });
    }

    // ═══════════════════════════════════════════════════════════════════
    // PER-PROMPT OVERRIDE LOGIC
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies per-prompt parameter overrides. Non-null values update session state permanently.
    /// </summary>
    private void ApplyOverrides(
        AnyFormattedText? message,
        FilterOrBool? multiline,
        FilterOrBool? wrapLines,
        FilterOrBool? isPassword,
        bool? viMode,
        EditingMode? editingMode,
        FilterOrBool? completeWhileTyping,
        FilterOrBool? validateWhileTyping,
        FilterOrBool? enableHistorySearch,
        FilterOrBool? searchIgnoreCase,
        ILexer? lexer,
        FilterOrBool? enableSystemPrompt,
        FilterOrBool? enableSuspend,
        FilterOrBool? enableOpenInEditor,
        IValidator? validator,
        ICompleter? completer,
        bool? completeInThread,
        int? reserveSpaceForMenu,
        CompleteStyle? completeStyle,
        IAutoSuggest? autoSuggest,
        IStyle? style,
        IStyleTransformation? styleTransformation,
        FilterOrBool? swapLightAndDarkColors,
        ColorDepth? colorDepth,
        ICursorShapeConfig? cursor,
        FilterOrBool? includeDefaultPygmentsStyle,
        IClipboard? clipboard,
        object? promptContinuation,
        AnyFormattedText? rprompt,
        AnyFormattedText? bottomToolbar,
        FilterOrBool? mouseSupport,
        IReadOnlyList<IProcessor>? inputProcessors,
        AnyFormattedText? placeholder,
        IKeyBindingsBase? keyBindings,
        object? tempfileSuffix,
        object? tempfile,
        double? refreshInterval,
        FilterOrBool? showFrame)
    {
        // Explicit property-by-property null checks per Research R4
        if (message is not null) Message = message.Value;
        if (multiline is not null) Multiline = multiline.Value;
        if (wrapLines is not null) WrapLines = wrapLines.Value;
        if (isPassword is not null) IsPassword = isPassword.Value;
        if (completeWhileTyping is not null) CompleteWhileTyping = completeWhileTyping.Value;
        if (validateWhileTyping is not null) ValidateWhileTyping = validateWhileTyping.Value;
        if (enableHistorySearch is not null) EnableHistorySearch = enableHistorySearch.Value;
        if (searchIgnoreCase is not null) SearchIgnoreCase = searchIgnoreCase.Value;
        if (enableSystemPrompt is not null) EnableSystemPrompt = enableSystemPrompt.Value;
        if (enableSuspend is not null) EnableSuspend = enableSuspend.Value;
        if (enableOpenInEditor is not null) EnableOpenInEditor = enableOpenInEditor.Value;
        if (mouseSupport is not null) MouseSupport = mouseSupport.Value;
        if (swapLightAndDarkColors is not null) SwapLightAndDarkColors = swapLightAndDarkColors.Value;
        if (includeDefaultPygmentsStyle is not null) IncludeDefaultPygmentsStyle = includeDefaultPygmentsStyle.Value;
        if (showFrame is not null) ShowFrame = showFrame.Value;
        if (lexer is not null) Lexer = lexer;
        if (completer is not null) Completer = completer;
        if (completeInThread is not null) CompleteInThread = completeInThread.Value;
        if (validator is not null) Validator = validator;
        if (autoSuggest is not null) AutoSuggest = autoSuggest;
        if (style is not null) Style = style;
        if (styleTransformation is not null) StyleTransformation = styleTransformation;
        if (colorDepth is not null) ColorDepth = colorDepth;
        if (cursor is not null) Cursor = cursor;
        if (clipboard is not null) Clipboard = clipboard;
        if (keyBindings is not null) KeyBindings = keyBindings;
        if (promptContinuation is not null) PromptContinuation = promptContinuation;
        if (rprompt is not null) RPrompt = rprompt.Value;
        if (bottomToolbar is not null) BottomToolbar = bottomToolbar.Value;
        if (inputProcessors is not null) InputProcessors = inputProcessors;
        if (placeholder is not null) Placeholder = placeholder;
        if (reserveSpaceForMenu is not null) ReserveSpaceForMenu = reserveSpaceForMenu.Value;
        if (refreshInterval is not null) RefreshInterval = refreshInterval.Value;
        if (tempfileSuffix is not null) TempfileSuffix = tempfileSuffix;
        if (tempfile is not null) Tempfile = tempfile;
        if (completeStyle is not null) CompleteStyle = completeStyle.Value;

        // viMode takes precedence over editingMode
        if (viMode is true)
        {
            EditingMode = EditingMode.Vi;
        }
        else if (editingMode is not null)
        {
            EditingMode = editingMode.Value;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // DEFAULT DOCUMENT RESOLUTION
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Resolves the default_ parameter into a Document.
    /// </summary>
    private static Document? ResolveDefaultDocument(object? default_)
    {
        return default_ switch
        {
            null => null,
            Document doc => doc,
            string s when !string.IsNullOrEmpty(s) => new Document(s, s.Length),
            string => null, // empty string = no default
            _ => null,
        };
    }

    // ═══════════════════════════════════════════════════════════════════
    // DUMB TERMINAL PROMPT
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Runs a dumb terminal prompt synchronously.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_dumb_prompt</c> context manager + <c>dump_app.run()</c>.
    /// </remarks>
    private TResult DumbPromptRun(bool setExceptionHandler, bool handleSigint, bool inThread)
    {
        var (dumbApp, onTextChanged) = CreateDumbPromptApp();
        try
        {
            return dumbApp.Run(
                setExceptionHandler: setExceptionHandler,
                handleSigint: handleSigint,
                inThread: inThread);
        }
        finally
        {
            CleanupDumbPrompt(onTextChanged);
        }
    }

    /// <summary>
    /// Runs a dumb terminal prompt asynchronously.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_dumb_prompt</c> context manager + <c>await dump_app.run_async()</c>.
    /// </remarks>
    private async Task<TResult> DumbPromptRunAsync(bool setExceptionHandler, bool handleSigint)
    {
        var (dumbApp, onTextChanged) = CreateDumbPromptApp();
        try
        {
            return await dumbApp.RunAsync(
                setExceptionHandler: setExceptionHandler,
                handleSigint: handleSigint);
        }
        finally
        {
            CleanupDumbPrompt(onTextChanged);
        }
    }

    /// <summary>
    /// Creates a dumb terminal application with character echo.
    /// </summary>
    private (Application<TResult> app, Action<Buffer> onTextChanged) CreateDumbPromptApp()
    {
        // Write prompt to real output
        Output.Write(FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(Message)));
        Output.Flush();

        // Build key bindings
        IKeyBindingsBase kb = CreatePromptBindings();
        var userKb = KeyBindings;
        if (userKb is not null)
        {
            kb = new MergedKeyBindings(userKb, kb);
        }

        // Create temporary application with DummyOutput
        var dumbApp = new Application<TResult>(
            input: Input,
            output: new DummyOutput(),
            layout: Layout,
            keyBindings: kb);

        // Echo typed characters
        void OnTextChanged(Buffer _)
        {
            var text = DefaultBuffer.Document.TextBeforeCursor;
            if (text.Length > 0)
            {
                Output.Write(text[^1..]);
                Output.Flush();
            }
        }

        DefaultBuffer.OnTextChanged += OnTextChanged;
        return (dumbApp, OnTextChanged);
    }

    /// <summary>
    /// Cleans up after a dumb prompt run.
    /// </summary>
    private void CleanupDumbPrompt(Action<Buffer> onTextChanged)
    {
        Output.Write("\r\n");
        Output.Flush();
        DefaultBuffer.OnTextChanged -= onTextChanged;
    }
}
