using Stroke.Application;
using Stroke.Application.Bindings;
using Stroke.Clipboard;
using Stroke.Core;
using Stroke.CursorShapes;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Stroke.Output;
using Stroke.Styles;

using Buffer = Stroke.Core.Buffer;

namespace Stroke.Shortcuts;

public partial class PromptSession<TResult>
{
    /// <summary>
    /// Creates the Application instance with merged key bindings, dynamic style, and clipboard.
    /// </summary>
    /// <param name="editingMode">The initial editing mode.</param>
    /// <param name="eraseWhenDone">Whether to erase the prompt after completion.</param>
    /// <returns>The configured application.</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_create_application</c>.
    /// </remarks>
    private Application<TResult> CreateApplication(EditingMode editingMode, bool eraseWhenDone)
    {
        // Load standard key bindings
        var autoSuggestBindings = AutoSuggestBindings.LoadAutoSuggestBindings();
        var openInEditorBindings = OpenInEditorBindings.LoadOpenInEditorBindings();
        var promptBindings = CreatePromptBindings();

        // Merge style transformations: user + conditional swap light/dark
        var mergedStyleTransformation = StyleTransformationMerger.MergeStyleTransformations(
        [
            new DynamicStyleTransformation(() => StyleTransformation),
            new ConditionalStyleTransformation(
                new SwapLightAndDarkStyleTransformation(),
                DynCond(() => SwapLightAndDarkColors)),
        ]);

        // Create application with merged key bindings in priority order:
        // Inner: [auto-suggest, conditional open-in-editor, prompt-specific]
        // Outer: [DynamicKeyBindings(user)]
        var application = new Application<TResult>(
            layout: Layout,
            style: new DynamicStyle(() => Style),
            styleTransformation: mergedStyleTransformation,
            includeDefaultPygmentsStyle: DynCond(() => IncludeDefaultPygmentsStyle),
            clipboard: new DynamicClipboard(() => Clipboard),
            keyBindings: new MergedKeyBindings(
                new MergedKeyBindings(
                    autoSuggestBindings,
                    new ConditionalKeyBindings(
                        openInEditorBindings,
                        ((Filter)DynCond(() => EnableOpenInEditor))
                            .And((Filter)AppFilters.HasFocus(DefaultBuffer))),
                    promptBindings),
                new DynamicKeyBindings(() => KeyBindings)),
            mouseSupport: DynCond(() => MouseSupport),
            editingMode: editingMode,
            eraseWhenDone: eraseWhenDone,
            reverseViSearchDirection: true,
            cursor: new DynamicCursorShapeConfig(() => Cursor),
            refreshInterval: _refreshInterval == 0 ? null : _refreshInterval,
            input: _input,
            output: _output);

        return application;
    }

    /// <summary>
    /// Creates prompt-specific key bindings: Enter (accept), Ctrl-C (interrupt),
    /// Ctrl-D (EOF), Tab (readline completions), Ctrl-Z (suspend).
    /// </summary>
    /// <returns>The prompt key bindings.</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_create_prompt_bindings</c>.
    /// </remarks>
    private KeyBindings CreatePromptBindings()
    {
        var kb = new KeyBindings();
        var defaultFocused = AppFilters.HasFocus(BufferNames.Default);

        // Enter: accept input in single-line mode
        var doAccept = new Condition(() =>
            !FilterUtils.ToFilter(Multiline).Invoke()
            && App.Layout.HasFocus(DefaultBuffer));

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlM)],
            filter: new FilterOrBool(((Filter)doAccept).And((Filter)defaultFocused)))(
            (@event) =>
            {
                DefaultBuffer.ValidateAndHandle();
                return null;
            });

        // Tab: display completions like readline
        var readlineCompleteStyle = new Condition(() =>
            CompleteStyle == CompleteStyle.ReadlineLike);

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlI)],
            filter: new FilterOrBool(
                ((Filter)readlineCompleteStyle).And((Filter)defaultFocused)))(
            (@event) =>
            {
                CompletionBindings.DisplayCompletionsLikeReadline(@event);
                return null;
            });

        // Ctrl-C: keyboard interrupt
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlC)],
            filter: new FilterOrBool(defaultFocused))(
            (@event) =>
            {
                var exception = (Exception)Activator.CreateInstance(InterruptException)!;
                @event.GetApp().Exit(exception: exception, style: "class:aborting");
                return null;
            });

        // <sigint> handler
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.SIGINT)])(
            (@event) =>
            {
                var exception = (Exception)Activator.CreateInstance(InterruptException)!;
                @event.GetApp().Exit(exception: exception, style: "class:aborting");
                return null;
            });

        // Ctrl-D: EOF on empty buffer
        var ctrlDCondition = new Condition(() =>
        {
            var app = Application.AppContext.GetApp();
            return app.CurrentBuffer.Name == BufferNames.Default
                && string.IsNullOrEmpty(app.CurrentBuffer.Document.Text);
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlD)],
            filter: new FilterOrBool(
                ((Filter)ctrlDCondition).And((Filter)defaultFocused)))(
            (@event) =>
            {
                var exception = (Exception)Activator.CreateInstance(EofException)!;
                @event.GetApp().Exit(exception: exception, style: "class:exiting");
                return null;
            });

        // Ctrl-Z: suspend to background
        var suspendSupported = new Condition(() => PlatformUtils.SuspendToBackgroundSupported);
        var enableSuspendCond = new Condition(() =>
            FilterUtils.ToFilter(EnableSuspend).Invoke());

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlZ)],
            filter: new FilterOrBool(
                ((Filter)suspendSupported).And((Filter)enableSuspendCond)))(
            (@event) =>
            {
                @event.GetApp().SuspendToBackground();
                return null;
            });

        return kb;
    }

    /// <summary>
    /// Creates a dumb terminal prompt that uses character-by-character echo.
    /// </summary>
    /// <param name="message">The prompt message to display.</param>
    /// <returns>An application configured for dumb terminal mode.</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_dumb_prompt</c> context manager.
    /// In C#, the caller manages the lifecycle using try/finally.
    /// </remarks>
    internal Application<TResult> DumbPrompt(AnyFormattedText message)
    {
        // Write prompt to real output
        Output.Write(FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(Message)));
        Output.Flush();

        // Key bindings: prompt bindings + user bindings
        IKeyBindingsBase keyBindings = CreatePromptBindings();
        var userKb = KeyBindings;
        if (userKb is not null)
        {
            keyBindings = new MergedKeyBindings(userKb, keyBindings);
        }

        // Create minimal application with DummyOutput
        var application = new Application<TResult>(
            input: Input,
            output: new DummyOutput(),
            layout: Layout,
            keyBindings: keyBindings);

        // Subscribe to text changes to echo characters
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

        // Note: cleanup happens in the Prompt method's finally block (CleanupDumbPrompt)

        return application;
    }

    /// <summary>
    /// Cleans up after a dumb prompt run.
    /// </summary>
    internal void CleanupDumbPrompt(Action<Buffer> onTextChanged)
    {
        Output.Write("\r\n");
        Output.Flush();
        DefaultBuffer.OnTextChanged -= onTextChanged;
    }
}
