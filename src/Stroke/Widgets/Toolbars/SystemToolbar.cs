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
using Stroke.Layout.Processors;
using Stroke.Lexers;

// Alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Widgets.Toolbars;

/// <summary>
/// Toolbar for entering and executing system shell commands.
/// </summary>
/// <remarks>
/// <para>
/// Provides a command prompt that appears when focused, with Emacs and Vi
/// mode-specific key bindings for cancel, execute, and global focus shortcuts.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>SystemToolbar</c> from <c>widgets/toolbars.py</c>.
/// </para>
/// </remarks>
public class SystemToolbar : IMagicContainer
{
    private readonly IKeyBindingsBase _bindings;

    /// <summary>Gets the prompt text.</summary>
    public AnyFormattedText Prompt { get; }

    /// <summary>Gets the filter controlling global binding registration.</summary>
    public IFilter EnableGlobalBindings { get; }

    /// <summary>Gets the system command buffer.</summary>
    public Buffer SystemBuffer { get; }

    /// <summary>Gets the buffer control displaying the system buffer.</summary>
    public BufferControl BufferControl { get; }

    /// <summary>Gets the window containing the buffer control (height=1, style="class:system-toolbar").</summary>
    public Window Window { get; }

    /// <summary>Gets the conditional container (visible when system buffer is focused).</summary>
    public ConditionalContainer Container { get; }

    /// <summary>
    /// Initializes a new SystemToolbar.
    /// </summary>
    /// <param name="prompt">The prompt text shown before user input. Default: "Shell command: ".</param>
    /// <param name="enableGlobalBindings">Whether to register global key bindings (M-! for Emacs, ! for Vi). Default: true.</param>
    public SystemToolbar(
        AnyFormattedText prompt = default,
        FilterOrBool enableGlobalBindings = default)
    {
        Prompt = prompt.IsEmpty ? "Shell command: " : prompt;
        EnableGlobalBindings = enableGlobalBindings.HasValue
            ? FilterUtils.ToFilter(enableGlobalBindings)
            : Always.Instance;

        SystemBuffer = new Buffer(name: BufferNames.System);

        _bindings = BuildKeyBindings();

        Func<AnyFormattedText> promptGetter = () => Prompt;
        BufferControl = new BufferControl(
            buffer: SystemBuffer,
            lexer: new SimpleLexer(style: "class:system-toolbar.text"),
            inputProcessors: [new BeforeInput(promptGetter, style: "class:system-toolbar")],
            keyBindings: _bindings);

        Window = new Window(
            content: BufferControl,
            height: Dimension.Exact(1),
            style: "class:system-toolbar");

        Container = new ConditionalContainer(
            content: new AnyContainer(Window),
            filter: new FilterOrBool(AppFilters.HasFocus(SystemBuffer)));
    }

    private IReadOnlyList<StyleAndTextTuple> GetDisplayBeforeText()
    {
        return
        [
            new("class:system-toolbar", "Shell command: "),
            new("class:system-toolbar.text", SystemBuffer.Text),
            new("", "\n"),
        ];
    }

    private IKeyBindingsBase BuildKeyBindings()
    {
        var focused = (Filter)AppFilters.HasFocus(SystemBuffer);

        // Emacs bindings
        var emacsBindings = new KeyBindings();

        // Escape, Ctrl-G, Ctrl-C → cancel
        KeyHandlerCallable emacsCancel = (@event) =>
        {
            SystemBuffer.Reset();
            @event.GetApp().Layout.FocusLast();
            return null;
        };

        emacsBindings.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape)],
            filter: new FilterOrBool(focused))(emacsCancel);
        emacsBindings.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlG)],
            filter: new FilterOrBool(focused))(emacsCancel);
        emacsBindings.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlC)],
            filter: new FilterOrBool(focused))(emacsCancel);

        // Enter → async execute
        emacsBindings.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlM)],
            filter: new FilterOrBool(focused))((@event) =>
        {
            var app = @event.GetApp();
            var text = SystemBuffer.Text;
            var displayBeforeText = GetDisplayBeforeText();
            _ = app.CreateBackgroundTask(async ct =>
            {
                await app.RunSystemCommandAsync(
                    text,
                    displayBeforeText: new Stroke.FormattedText.FormattedText(displayBeforeText));
                SystemBuffer.Reset(appendToHistory: true);
                app.Layout.FocusLast();
            });
            return null;
        });

        // Vi bindings
        var viBindings = new KeyBindings();

        // Escape, Ctrl-C → cancel (vi)
        KeyHandlerCallable viCancel = (@event) =>
        {
            @event.GetApp().ViState.InputMode = InputMode.Navigation;
            SystemBuffer.Reset();
            @event.GetApp().Layout.FocusLast();
            return null;
        };

        viBindings.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape)],
            filter: new FilterOrBool(focused))(viCancel);
        viBindings.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlC)],
            filter: new FilterOrBool(focused))(viCancel);

        // Enter → async execute (vi)
        viBindings.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlM)],
            filter: new FilterOrBool(focused))((@event) =>
        {
            var app = @event.GetApp();
            app.ViState.InputMode = InputMode.Navigation;
            var text = SystemBuffer.Text;
            var displayBeforeText = GetDisplayBeforeText();
            _ = app.CreateBackgroundTask(async ct =>
            {
                await app.RunSystemCommandAsync(
                    text,
                    displayBeforeText: new Stroke.FormattedText.FormattedText(displayBeforeText));
                SystemBuffer.Reset(appendToHistory: true);
                app.Layout.FocusLast();
            });
            return null;
        });

        // Global bindings
        var globalBindings = new KeyBindings();

        // M-! (Keys.Escape + "!") for Emacs: ~focused & emacs_mode
        var notFocused = ~focused;
        var emacsFocusFilter = (Filter)notFocused & EmacsFilters.EmacsMode;
        globalBindings.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('!')],
            filter: new FilterOrBool(emacsFocusFilter),
            isGlobal: true)((@event) =>
        {
            @event.GetApp().Layout.Focus(Window);
            return null;
        });

        // "!" for Vi (navigation mode): ~focused & vi_mode & vi_navigation_mode
        var viFocusFilter = (Filter)((Filter)notFocused & ViFilters.ViMode) & ViFilters.ViNavigationMode;
        globalBindings.Add<KeyHandlerCallable>(
            [new KeyOrChar('!')],
            filter: new FilterOrBool(viFocusFilter),
            isGlobal: true)((@event) =>
        {
            @event.GetApp().ViState.InputMode = InputMode.Insert;
            @event.GetApp().Layout.Focus(Window);
            return null;
        });

        return new MergedKeyBindings(
            new ConditionalKeyBindings(emacsBindings, EmacsFilters.EmacsMode),
            new ConditionalKeyBindings(viBindings, ViFilters.ViMode),
            new ConditionalKeyBindings(globalBindings, EnableGlobalBindings));
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Container;
}
