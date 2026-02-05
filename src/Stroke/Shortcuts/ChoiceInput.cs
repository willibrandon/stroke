using Stroke.Application;
using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Styles;
using Stroke.Widgets.Base;
using Stroke.Widgets.Lists;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Shortcuts;

/// <summary>
/// Input selection prompt. Ask the user to choose among a set of options.
/// </summary>
/// <typeparam name="T">The type of value returned when an option is selected.</typeparam>
/// <remarks>
/// <para>
/// Example usage:
/// </para>
/// <code>
/// var choiceInput = new ChoiceInput&lt;string&gt;(
///     message: "Please select a dish:",
///     options: new[]
///     {
///         ("pizza", (AnyFormattedText)"Pizza with mushrooms"),
///         ("salad", (AnyFormattedText)"Salad with tomatoes"),
///         ("sushi", (AnyFormattedText)"Sushi"),
///     },
///     defaultValue: "pizza");
///
/// string result = choiceInput.Prompt();
/// </code>
/// <para>
/// Thread safety: This class is thread-safe. All configuration is immutable after construction.
/// The underlying Application handles thread-safe rendering and input processing.
/// </para>
/// <para>
/// Accessibility: Selection state is communicated through both visual styling (bold text)
/// AND semantic structure (list position) for screen reader compatibility.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ChoiceInput</c> class from
/// <c>prompt_toolkit.shortcuts.choice_input</c>.
/// </para>
/// </remarks>
public sealed class ChoiceInput<T>
{
    /// <summary>
    /// Gets the message/prompt text displayed above the options.
    /// </summary>
    public AnyFormattedText Message { get; }

    /// <summary>
    /// Gets the available options as value-label pairs.
    /// </summary>
    public IReadOnlyList<(T Value, AnyFormattedText Label)> Options { get; }

    /// <summary>
    /// Gets the default value that is pre-selected when the prompt displays.
    /// </summary>
    public T? Default { get; }

    /// <summary>
    /// Gets a value indicating whether mouse support is enabled.
    /// </summary>
    public bool MouseSupport { get; }

    /// <summary>
    /// Gets the style used for rendering.
    /// </summary>
    public IStyle Style { get; }

    /// <summary>
    /// Gets the symbol displayed in front of the selected choice.
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// Gets the bottom toolbar content, if any.
    /// </summary>
    public AnyFormattedText? BottomToolbar { get; }

    /// <summary>
    /// Gets the filter or bool controlling frame display.
    /// </summary>
    public FilterOrBool ShowFrame { get; }

    /// <summary>
    /// Gets the filter or bool controlling suspend-to-background capability.
    /// </summary>
    public FilterOrBool EnableSuspend { get; }

    /// <summary>
    /// Gets the filter or bool controlling interrupt handling.
    /// </summary>
    public FilterOrBool EnableInterrupt { get; }

    /// <summary>
    /// Gets the exception type raised on interrupt (Ctrl+C).
    /// </summary>
    public Type InterruptException { get; }

    /// <summary>
    /// Gets the additional key bindings, if any.
    /// </summary>
    public IKeyBindingsBase? KeyBindings { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChoiceInput{T}"/> class.
    /// </summary>
    /// <param name="message">Plain text or formatted text to be shown before the options.</param>
    /// <param name="options">
    /// Sequence of (value, label) tuples. The labels can be formatted text.
    /// </param>
    /// <param name="defaultValue">
    /// Default value. If none is given or the value doesn't match any option,
    /// the first option is considered the default.
    /// </param>
    /// <param name="mouseSupport">Enable mouse support for clicking options.</param>
    /// <param name="style">
    /// <see cref="IStyle"/> instance for the color scheme.
    /// If null, a default style is used with brownish-red frame border and bold selected option.
    /// </param>
    /// <param name="symbol">Symbol to be displayed in front of the selected choice. Default is "&gt;".</param>
    /// <param name="bottomToolbar">
    /// Formatted text or callable that returns formatted text to be displayed at the bottom of the screen.
    /// </param>
    /// <param name="showFrame">
    /// When true, surround the input with a frame.
    /// Can be a bool or an <see cref="IFilter"/> for conditional display.
    /// </param>
    /// <param name="enableSuspend">
    /// When true on Unix platforms, allow Ctrl+Z to suspend the process to background.
    /// Has no effect on Windows.
    /// </param>
    /// <param name="enableInterrupt">
    /// When true (default), raise the <paramref name="interruptException"/> when Ctrl+C is pressed.
    /// </param>
    /// <param name="interruptException">
    /// The exception type that will be raised on Ctrl+C.
    /// Must be assignable to <see cref="Exception"/>. Default is <see cref="KeyboardInterrupt"/>.
    /// </param>
    /// <param name="keyBindings">
    /// Additional key bindings to merge with the default bindings.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="options"/> is empty.</exception>
    public ChoiceInput(
        AnyFormattedText message,
        IReadOnlyList<(T Value, AnyFormattedText Label)> options,
        T? defaultValue = default,
        bool mouseSupport = false,
        IStyle? style = null,
        string symbol = ">",
        AnyFormattedText? bottomToolbar = null,
        FilterOrBool showFrame = default,
        FilterOrBool enableSuspend = default,
        FilterOrBool enableInterrupt = default,
        Type? interruptException = null,
        IKeyBindingsBase? keyBindings = null)
    {
        // Parameter validation [FR-018]
        ArgumentNullException.ThrowIfNull(options);
        if (options.Count == 0)
            throw new ArgumentException("Options cannot be empty.", nameof(options));

        Message = message;
        Options = options;
        Default = defaultValue;
        MouseSupport = mouseSupport;
        Style = style ?? CreateDefaultChoiceInputStyle();
        Symbol = symbol;
        BottomToolbar = bottomToolbar;
        ShowFrame = showFrame;
        EnableSuspend = enableSuspend;
        // Default enableInterrupt to true if not specified [FR-008]
        EnableInterrupt = enableInterrupt.HasValue ? enableInterrupt : new FilterOrBool(true);
        // Default interruptException to KeyboardInterrupt [FR-008]
        InterruptException = interruptException ?? typeof(KeyboardInterrupt);
        KeyBindings = keyBindings;
    }

    /// <summary>
    /// Creates the default style for choice input prompts.
    /// </summary>
    /// <returns>A Style instance with brownish-red frame border and bold selected option.</returns>
    private static IStyle CreateDefaultChoiceInputStyle()
    {
        return Styles.Style.FromDict(new Dictionary<string, string>
        {
            ["frame.border"] = "#884444",
            ["selected-option"] = "bold",
        });
    }

    /// <summary>
    /// Creates an Application instance configured for this choice input.
    /// </summary>
    /// <returns>A configured Application&lt;T&gt; ready to be run.</returns>
    private Application<T> CreateApplication()
    {
        // Create RadioList with configured options [FR-002, FR-003, FR-004, FR-005]
        var radioList = new RadioList<T>(
            values: Options,
            @default: Default,
            selectOnFocus: true,
            openCharacter: "",
            selectCharacter: Symbol,
            closeCharacter: "",
            showCursor: false,
            showNumbers: true,
            containerStyle: "class:input-selection",
            defaultStyle: "class:option",
            selectedStyle: "",
            checkedStyle: "class:selected-option",
            numberStyle: "class:number",
            showScrollbar: false);

        // Build main container with message and options [FR-001]
        IContainer container = new HSplit(
            children: (IReadOnlyList<IContainer>)
            [
                new Box(
                    body: new AnyContainer(new Label(text: Message, dontExtendHeight: new FilterOrBool(true))),
                    paddingTop: Dimension.Exact(0),
                    paddingLeft: Dimension.Exact(1),
                    paddingRight: Dimension.Exact(1),
                    paddingBottom: Dimension.Exact(0)).PtContainer(),
                new Box(
                    body: new AnyContainer(radioList),
                    paddingTop: Dimension.Exact(0),
                    paddingLeft: Dimension.Exact(3),
                    paddingRight: Dimension.Exact(1),
                    paddingBottom: Dimension.Exact(0)).PtContainer(),
            ]);

        // Frame filter [FR-010]
        var showFrameFilter = new Condition(() => FilterUtils.IsTrue(ShowFrame));

        // Bottom toolbar filter [FR-011]
        // Condition evaluates all three sub-conditions on each invoke
        var showBottomToolbar = new Condition(() =>
            BottomToolbar is not null
            && !AppFilters.IsDone.Invoke()
            && AppFilters.RendererHeightIsKnown.Invoke());

        // Wrap in conditional frame [FR-010]
        container = new ConditionalContainer(
            content: new AnyContainer(new Frame(body: new AnyContainer(container))),
            alternativeContent: new AnyContainer(container),
            filter: new FilterOrBool(showFrameFilter));

        // Bottom toolbar [FR-011]
        var bottomToolbarContainer = new ConditionalContainer(
            content: new AnyContainer(new Window(
                content: new FormattedTextControl(
                    textGetter: () => BottomToolbar is not null
                        ? FormattedTextUtils.ToFormattedText(BottomToolbar.Value)
                        : [],
                    style: "class:bottom-toolbar.text",
                    focusable: new FilterOrBool(false),
                    keyBindings: null,
                    showCursor: false),
                style: "class:bottom-toolbar",
                dontExtendHeight: new FilterOrBool(true),
                height: new Dimension(min: 1))),
            filter: new FilterOrBool(showBottomToolbar));

        // Build layout with container, spacer, and toolbar
        var layout = new Layout.Layout(
            container: new AnyContainer(new HSplit(
                children: (IReadOnlyList<IContainer>)
                [
                    container,
                    // Spacer window between selection and toolbar
                    new ConditionalContainer(
                        content: new AnyContainer(new Window()),
                        filter: new FilterOrBool(showBottomToolbar)),
                    bottomToolbarContainer,
                ])),
            focusedElement: new FocusableElement(new AnyContainer(radioList)));

        // Create key bindings
        var kb = new KeyBinding.KeyBindings();

        // Enter key - accept input [FR-006]
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlM)],
            eager: new FilterOrBool(true))((@event) =>
        {
            AppContext.GetApp().Exit(result: radioList.CurrentValue, style: "class:accepted");
            return null;
        });

        // Ctrl+C interrupt filter [FR-008]
        var enableInterruptFilter = new Condition(() => FilterUtils.IsTrue(EnableInterrupt));

        // Ctrl+C and SIGINT - keyboard interrupt [FR-008]
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlC)],
            filter: new FilterOrBool(enableInterruptFilter))((@event) =>
        {
            var exception = (Exception)Activator.CreateInstance(InterruptException)!;
            AppContext.GetApp().Exit(exception: exception, style: "class:aborting");
            return null;
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.SIGINT)],
            filter: new FilterOrBool(enableInterruptFilter))((@event) =>
        {
            var exception = (Exception)Activator.CreateInstance(InterruptException)!;
            AppContext.GetApp().Exit(exception: exception, style: "class:aborting");
            return null;
        });

        // Ctrl+D - EOF/exit (similar to Ctrl+C but throws EOFException)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlD)],
            filter: new FilterOrBool(enableInterruptFilter))((@event) =>
        {
            AppContext.GetApp().Exit(exception: new EOFException(), style: "class:exiting");
            return null;
        });

        // Ctrl+Z suspend (Unix only) [FR-012, XP-001]
        var suspendSupported = new Condition(() => PlatformUtils.SuspendToBackgroundSupported);
        var enableSuspendFilter = new Condition(() => FilterUtils.IsTrue(EnableSuspend));

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlZ)],
            filter: new FilterOrBool(suspendSupported & enableSuspendFilter))((@event) =>
        {
            AppContext.GetApp().SuspendToBackground();
            return null;
        });

        // Merge key bindings: local bindings first, then user bindings [T019]
        IKeyBindingsBase mergedBindings;
        if (KeyBindings is not null)
        {
            mergedBindings = new MergedKeyBindings(
                kb,
                new DynamicKeyBindings(() => KeyBindings));
        }
        else
        {
            mergedBindings = kb;
        }

        // Create Application [FR-009, FR-014]
        return new Application<T>(
            layout: layout,
            fullScreen: false,
            mouseSupport: new FilterOrBool(MouseSupport),
            keyBindings: mergedBindings,
            style: Style);
    }

    /// <summary>
    /// Display the prompt and wait for the user to make a selection.
    /// </summary>
    /// <returns>The value associated with the selected option.</returns>
    /// <exception cref="KeyboardInterrupt">
    /// Thrown when user presses Ctrl+C and <see cref="EnableInterrupt"/> is true
    /// (or the type specified by <see cref="InterruptException"/>).
    /// </exception>
    public T Prompt()
    {
        return CreateApplication().Run();
    }

    /// <summary>
    /// Display the prompt asynchronously and wait for the user to make a selection.
    /// </summary>
    /// <returns>A task that completes with the value associated with the selected option.</returns>
    /// <exception cref="KeyboardInterrupt">
    /// Thrown when user presses Ctrl+C and <see cref="EnableInterrupt"/> is true
    /// (or the type specified by <see cref="InterruptException"/>).
    /// </exception>
    public Task<T> PromptAsync()
    {
        return CreateApplication().RunAsync();
    }
}
