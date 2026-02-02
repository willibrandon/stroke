using Stroke.Application;
using Stroke.AutoSuggest;
using Stroke.Clipboard;
using Stroke.Completion;
using Stroke.Core;
using Stroke.CursorShapes;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.History;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Processors;
using Stroke.Lexers;
using Stroke.Output;
using Stroke.Styles;
using Stroke.Validation;

using Buffer = Stroke.Core.Buffer;

namespace Stroke.Shortcuts;

/// <summary>
/// PromptSession for a prompt application, which can be used as a GNU Readline replacement.
/// </summary>
/// <typeparam name="TResult">The type of result returned when the prompt completes.</typeparam>
/// <remarks>
/// <para>This is a wrapper around Buffer, Layout, Application, History, and KeyBindings
/// that provides a cohesive prompt experience. Create a session once and call
/// <see cref="Prompt(AnyFormattedText?, FilterOrBool?, FilterOrBool?, FilterOrBool?, bool?, EditingMode?, FilterOrBool?, FilterOrBool?, FilterOrBool?, FilterOrBool?, ILexer?, FilterOrBool?, FilterOrBool?, FilterOrBool?, IValidator?, ICompleter?, bool?, int?, CompleteStyle?, IAutoSuggest?, IStyle?, IStyleTransformation?, FilterOrBool?, ColorDepth?, ICursorShapeConfig?, FilterOrBool?, IClipboard?, object?, AnyFormattedText?, AnyFormattedText?, FilterOrBool?, IReadOnlyList{IProcessor}?, AnyFormattedText?, IKeyBindingsBase?, object?, object?, double?, FilterOrBool?, object, bool, Action?, bool, bool, bool, InputHook?)"/>
/// or <see cref="PromptAsync"/> repeatedly for a REPL-like experience with persistent history.</para>
/// <para><b>Thread safety:</b> Session properties are Lock-protected for safe reads/writes
/// from multiple threads. However, only one Prompt or PromptAsync
/// call should be active at a time per session instance.</para>
/// </remarks>
public partial class PromptSession<TResult>
{
    private readonly Lock _lock = new();

    // Mutable state (Lock-protected)
    private AnyFormattedText _message;
    private FilterOrBool _multiline;
    private FilterOrBool _wrapLines;
    private FilterOrBool _isPassword;
    private FilterOrBool _completeWhileTyping;
    private FilterOrBool _validateWhileTyping;
    private FilterOrBool _enableHistorySearch;
    private FilterOrBool _searchIgnoreCase;
    private FilterOrBool _enableSystemPrompt;
    private FilterOrBool _enableSuspend;
    private FilterOrBool _enableOpenInEditor;
    private FilterOrBool _mouseSupport;
    private FilterOrBool _swapLightAndDarkColors;
    private FilterOrBool _includeDefaultPygmentsStyle;
    private FilterOrBool _showFrame;
    private ILexer? _lexer;
    private ICompleter? _completer;
    private bool _completeInThread;
    private IValidator? _validator;
    private IAutoSuggest? _autoSuggest;
    private IStyle? _style;
    private IStyleTransformation? _styleTransformation;
    private ColorDepth? _colorDepth;
    private ICursorShapeConfig? _cursor;
    private IClipboard _clipboard;
    private IKeyBindingsBase? _keyBindings;
    private object? _promptContinuation;
    private AnyFormattedText _rprompt;
    private AnyFormattedText _bottomToolbar;
    private IReadOnlyList<IProcessor>? _inputProcessors;
    private AnyFormattedText? _placeholder;
    private CompleteStyle _completeStyle;
    private int _reserveSpaceForMenu;
    private double _refreshInterval;
    private object? _tempfileSuffix;
    private object? _tempfile;

    // Immutable (set once in constructor)
    private readonly IInput? _input;
    private readonly IOutput? _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromptSession{TResult}"/> class.
    /// </summary>
    public PromptSession(
        AnyFormattedText message = default,
        FilterOrBool multiline = default,
        FilterOrBool wrapLines = default,
        FilterOrBool isPassword = default,
        bool viMode = false,
        EditingMode editingMode = EditingMode.Emacs,
        FilterOrBool completeWhileTyping = default,
        FilterOrBool validateWhileTyping = default,
        FilterOrBool enableHistorySearch = default,
        FilterOrBool searchIgnoreCase = default,
        ILexer? lexer = null,
        FilterOrBool enableSystemPrompt = default,
        FilterOrBool enableSuspend = default,
        FilterOrBool enableOpenInEditor = default,
        IValidator? validator = null,
        ICompleter? completer = null,
        bool completeInThread = false,
        int reserveSpaceForMenu = 8,
        CompleteStyle completeStyle = CompleteStyle.Column,
        IAutoSuggest? autoSuggest = null,
        IStyle? style = null,
        IStyleTransformation? styleTransformation = null,
        FilterOrBool swapLightAndDarkColors = default,
        ColorDepth? colorDepth = null,
        ICursorShapeConfig? cursor = null,
        FilterOrBool includeDefaultPygmentsStyle = default,
        IHistory? history = null,
        IClipboard? clipboard = null,
        object? promptContinuation = null,
        AnyFormattedText rprompt = default,
        AnyFormattedText bottomToolbar = default,
        FilterOrBool mouseSupport = default,
        IReadOnlyList<IProcessor>? inputProcessors = null,
        AnyFormattedText? placeholder = null,
        IKeyBindingsBase? keyBindings = null,
        bool eraseWhenDone = false,
        object? tempfileSuffix = null,
        object? tempfile = null,
        double refreshInterval = 0,
        FilterOrBool showFrame = default,
        IInput? input = null,
        IOutput? output = null,
        Type? interruptException = null,
        Type? eofException = null)
    {
        // Validate and set exception types (immutable)
        interruptException ??= typeof(KeyboardInterruptException);
        eofException ??= typeof(EOFException);
        ValidateExceptionType(interruptException, nameof(interruptException));
        ValidateExceptionType(eofException, nameof(eofException));
        InterruptException = interruptException;
        EofException = eofException;

        // Store immutable I/O references
        _input = input;
        _output = output;

        // Initialize mutable state with correct defaults.
        // FilterOrBool: default struct is falsy, so properties that default to true
        // need explicit handling via HasValue sentinel detection.
        _message = message;
        _multiline = multiline;
        _wrapLines = wrapLines.HasValue ? wrapLines : new FilterOrBool(true);
        _isPassword = isPassword;
        _completeWhileTyping = completeWhileTyping.HasValue ? completeWhileTyping : new FilterOrBool(true);
        _validateWhileTyping = validateWhileTyping.HasValue ? validateWhileTyping : new FilterOrBool(true);
        _enableHistorySearch = enableHistorySearch;
        _searchIgnoreCase = searchIgnoreCase;
        _enableSystemPrompt = enableSystemPrompt;
        _enableSuspend = enableSuspend;
        _enableOpenInEditor = enableOpenInEditor;
        _mouseSupport = mouseSupport;
        _swapLightAndDarkColors = swapLightAndDarkColors;
        _includeDefaultPygmentsStyle = includeDefaultPygmentsStyle.HasValue
            ? includeDefaultPygmentsStyle
            : new FilterOrBool(true);
        _showFrame = showFrame;
        _lexer = lexer;
        _completer = completer;
        _completeInThread = completeInThread;
        _validator = validator;
        _autoSuggest = autoSuggest;
        _style = style;
        _styleTransformation = styleTransformation;
        _colorDepth = colorDepth;
        _cursor = cursor;
        _clipboard = clipboard ?? new InMemoryClipboard();
        _keyBindings = keyBindings;
        _promptContinuation = promptContinuation;
        _rprompt = rprompt;
        _bottomToolbar = bottomToolbar;
        _inputProcessors = inputProcessors;
        _placeholder = placeholder;
        _completeStyle = completeStyle;
        _reserveSpaceForMenu = reserveSpaceForMenu;
        _refreshInterval = refreshInterval;
        _tempfileSuffix = tempfileSuffix ?? ".txt";
        _tempfile = tempfile;

        // History is immutable after construction (shared with DefaultBuffer)
        History = history ?? new InMemoryHistory();

        // Create owned objects
        DefaultBuffer = CreateDefaultBuffer();
        SearchBuffer = CreateSearchBuffer();
        Layout = CreateLayout();

        // viMode convenience: takes precedence over editingMode parameter (Edge Case 1)
        if (viMode)
        {
            editingMode = EditingMode.Vi;
        }

        App = CreateApplication(editingMode, eraseWhenDone);
    }

    // ═══════════════════════════════════════════════════════════════════
    // IMMUTABLE PROPERTIES
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>Gets the interrupt exception type (set once in constructor).</summary>
    public Type InterruptException { get; }

    /// <summary>Gets the EOF exception type (set once in constructor).</summary>
    public Type EofException { get; }

    // ═══════════════════════════════════════════════════════════════════
    // OWNED OBJECTS (immutable references, created in constructor)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>Gets the shared history instance.</summary>
    public IHistory History { get; }

    /// <summary>Gets the default input buffer.</summary>
    public Buffer DefaultBuffer { get; }

    /// <summary>Gets the search buffer.</summary>
    public Buffer SearchBuffer { get; }

    /// <summary>Gets the prompt layout.</summary>
    public Layout.Layout Layout { get; }

    /// <summary>Gets the prompt application.</summary>
    public Application<TResult> App { get; }

    // ═══════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES (delegate to App)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>Gets or sets the editing mode. Delegates to <see cref="App"/>.</summary>
    public EditingMode EditingMode
    {
        get => App.EditingMode;
        set => App.EditingMode = value;
    }

    /// <summary>Gets the input device. Delegates to <see cref="App"/>.</summary>
    public IInput Input => App.Input;

    /// <summary>Gets the output device. Delegates to <see cref="App"/>.</summary>
    public IOutput Output => App.Output;

    // ═══════════════════════════════════════════════════════════════════
    // MUTABLE PROPERTIES (Lock-protected)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>Gets or sets the prompt message text.</summary>
    public AnyFormattedText Message
    {
        get { using (_lock.EnterScope()) { return _message; } }
        set { using (_lock.EnterScope()) { _message = value; } }
    }

    /// <summary>Gets or sets the multiline filter.</summary>
    public FilterOrBool Multiline
    {
        get { using (_lock.EnterScope()) { return _multiline; } }
        set { using (_lock.EnterScope()) { _multiline = value; } }
    }

    /// <summary>Gets or sets the wrap lines filter.</summary>
    public FilterOrBool WrapLines
    {
        get { using (_lock.EnterScope()) { return _wrapLines; } }
        set { using (_lock.EnterScope()) { _wrapLines = value; } }
    }

    /// <summary>Gets or sets the password mode filter.</summary>
    public FilterOrBool IsPassword
    {
        get { using (_lock.EnterScope()) { return _isPassword; } }
        set { using (_lock.EnterScope()) { _isPassword = value; } }
    }

    /// <summary>Gets or sets the complete-while-typing filter.</summary>
    public FilterOrBool CompleteWhileTyping
    {
        get { using (_lock.EnterScope()) { return _completeWhileTyping; } }
        set { using (_lock.EnterScope()) { _completeWhileTyping = value; } }
    }

    /// <summary>Gets or sets the validate-while-typing filter.</summary>
    public FilterOrBool ValidateWhileTyping
    {
        get { using (_lock.EnterScope()) { return _validateWhileTyping; } }
        set { using (_lock.EnterScope()) { _validateWhileTyping = value; } }
    }

    /// <summary>Gets or sets the history search filter.</summary>
    public FilterOrBool EnableHistorySearch
    {
        get { using (_lock.EnterScope()) { return _enableHistorySearch; } }
        set { using (_lock.EnterScope()) { _enableHistorySearch = value; } }
    }

    /// <summary>Gets or sets the search ignore case filter.</summary>
    public FilterOrBool SearchIgnoreCase
    {
        get { using (_lock.EnterScope()) { return _searchIgnoreCase; } }
        set { using (_lock.EnterScope()) { _searchIgnoreCase = value; } }
    }

    /// <summary>Gets or sets the system prompt filter.</summary>
    public FilterOrBool EnableSystemPrompt
    {
        get { using (_lock.EnterScope()) { return _enableSystemPrompt; } }
        set { using (_lock.EnterScope()) { _enableSystemPrompt = value; } }
    }

    /// <summary>Gets or sets the suspend filter.</summary>
    public FilterOrBool EnableSuspend
    {
        get { using (_lock.EnterScope()) { return _enableSuspend; } }
        set { using (_lock.EnterScope()) { _enableSuspend = value; } }
    }

    /// <summary>Gets or sets the open-in-editor filter.</summary>
    public FilterOrBool EnableOpenInEditor
    {
        get { using (_lock.EnterScope()) { return _enableOpenInEditor; } }
        set { using (_lock.EnterScope()) { _enableOpenInEditor = value; } }
    }

    /// <summary>Gets or sets the mouse support filter.</summary>
    public FilterOrBool MouseSupport
    {
        get { using (_lock.EnterScope()) { return _mouseSupport; } }
        set { using (_lock.EnterScope()) { _mouseSupport = value; } }
    }

    /// <summary>Gets or sets the swap light and dark colors filter.</summary>
    public FilterOrBool SwapLightAndDarkColors
    {
        get { using (_lock.EnterScope()) { return _swapLightAndDarkColors; } }
        set { using (_lock.EnterScope()) { _swapLightAndDarkColors = value; } }
    }

    /// <summary>Gets or sets the include default Pygments style filter.</summary>
    public FilterOrBool IncludeDefaultPygmentsStyle
    {
        get { using (_lock.EnterScope()) { return _includeDefaultPygmentsStyle; } }
        set { using (_lock.EnterScope()) { _includeDefaultPygmentsStyle = value; } }
    }

    /// <summary>Gets or sets the show frame filter.</summary>
    public FilterOrBool ShowFrame
    {
        get { using (_lock.EnterScope()) { return _showFrame; } }
        set { using (_lock.EnterScope()) { _showFrame = value; } }
    }

    /// <summary>Gets or sets the lexer.</summary>
    public ILexer? Lexer
    {
        get { using (_lock.EnterScope()) { return _lexer; } }
        set { using (_lock.EnterScope()) { _lexer = value; } }
    }

    /// <summary>Gets or sets the completer.</summary>
    public ICompleter? Completer
    {
        get { using (_lock.EnterScope()) { return _completer; } }
        set { using (_lock.EnterScope()) { _completer = value; } }
    }

    /// <summary>Gets or sets whether completion runs in a background thread.</summary>
    public bool CompleteInThread
    {
        get { using (_lock.EnterScope()) { return _completeInThread; } }
        set { using (_lock.EnterScope()) { _completeInThread = value; } }
    }

    /// <summary>Gets or sets the validator.</summary>
    public IValidator? Validator
    {
        get { using (_lock.EnterScope()) { return _validator; } }
        set { using (_lock.EnterScope()) { _validator = value; } }
    }

    /// <summary>Gets or sets the auto-suggest provider.</summary>
    public IAutoSuggest? AutoSuggest
    {
        get { using (_lock.EnterScope()) { return _autoSuggest; } }
        set { using (_lock.EnterScope()) { _autoSuggest = value; } }
    }

    /// <summary>Gets or sets the style.</summary>
    public IStyle? Style
    {
        get { using (_lock.EnterScope()) { return _style; } }
        set { using (_lock.EnterScope()) { _style = value; } }
    }

    /// <summary>Gets or sets the style transformation.</summary>
    public IStyleTransformation? StyleTransformation
    {
        get { using (_lock.EnterScope()) { return _styleTransformation; } }
        set { using (_lock.EnterScope()) { _styleTransformation = value; } }
    }

    /// <summary>Gets or sets the color depth.</summary>
    public ColorDepth? ColorDepth
    {
        get { using (_lock.EnterScope()) { return _colorDepth; } }
        set { using (_lock.EnterScope()) { _colorDepth = value; } }
    }

    /// <summary>Gets or sets the cursor shape configuration.</summary>
    public ICursorShapeConfig? Cursor
    {
        get { using (_lock.EnterScope()) { return _cursor; } }
        set { using (_lock.EnterScope()) { _cursor = value; } }
    }

    /// <summary>Gets or sets the clipboard.</summary>
    public IClipboard Clipboard
    {
        get { using (_lock.EnterScope()) { return _clipboard; } }
        set { using (_lock.EnterScope()) { _clipboard = value; } }
    }

    /// <summary>Gets or sets the user key bindings.</summary>
    public IKeyBindingsBase? KeyBindings
    {
        get { using (_lock.EnterScope()) { return _keyBindings; } }
        set { using (_lock.EnterScope()) { _keyBindings = value; } }
    }

    /// <summary>Gets or sets the prompt continuation text or callable.</summary>
    public object? PromptContinuation
    {
        get { using (_lock.EnterScope()) { return _promptContinuation; } }
        set { using (_lock.EnterScope()) { _promptContinuation = value; } }
    }

    /// <summary>Gets or sets the right prompt text.</summary>
    public AnyFormattedText RPrompt
    {
        get { using (_lock.EnterScope()) { return _rprompt; } }
        set { using (_lock.EnterScope()) { _rprompt = value; } }
    }

    /// <summary>Gets or sets the bottom toolbar text.</summary>
    public AnyFormattedText BottomToolbar
    {
        get { using (_lock.EnterScope()) { return _bottomToolbar; } }
        set { using (_lock.EnterScope()) { _bottomToolbar = value; } }
    }

    /// <summary>Gets or sets the input processors.</summary>
    public IReadOnlyList<IProcessor>? InputProcessors
    {
        get { using (_lock.EnterScope()) { return _inputProcessors; } }
        set { using (_lock.EnterScope()) { _inputProcessors = value; } }
    }

    /// <summary>Gets or sets the placeholder text.</summary>
    public AnyFormattedText? Placeholder
    {
        get { using (_lock.EnterScope()) { return _placeholder; } }
        set { using (_lock.EnterScope()) { _placeholder = value; } }
    }

    /// <summary>Gets or sets the completion display style.</summary>
    public CompleteStyle CompleteStyle
    {
        get { using (_lock.EnterScope()) { return _completeStyle; } }
        set { using (_lock.EnterScope()) { _completeStyle = value; } }
    }

    /// <summary>Gets or sets the number of lines to reserve for the completion menu.</summary>
    public int ReserveSpaceForMenu
    {
        get { using (_lock.EnterScope()) { return _reserveSpaceForMenu; } }
        set { using (_lock.EnterScope()) { _reserveSpaceForMenu = value; } }
    }

    /// <summary>Gets or sets the auto-refresh interval in seconds.</summary>
    public double RefreshInterval
    {
        get { using (_lock.EnterScope()) { return _refreshInterval; } }
        set { using (_lock.EnterScope()) { _refreshInterval = value; } }
    }

    /// <summary>Gets or sets the temp file suffix.</summary>
    public object? TempfileSuffix
    {
        get { using (_lock.EnterScope()) { return _tempfileSuffix; } }
        set { using (_lock.EnterScope()) { _tempfileSuffix = value; } }
    }

    /// <summary>Gets or sets the temp file path or callable.</summary>
    public object? Tempfile
    {
        get { using (_lock.EnterScope()) { return _tempfile; } }
        set { using (_lock.EnterScope()) { _tempfile = value; } }
    }

    // ═══════════════════════════════════════════════════════════════════
    // DYNCOND — Dynamic condition factory
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a <see cref="Condition"/> that dynamically reads a <see cref="FilterOrBool"/>
    /// property from this session at evaluation time.
    /// </summary>
    /// <param name="getter">A function that reads the Lock-protected backing field.</param>
    /// <returns>A <see cref="Condition"/> that evaluates the property value.</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_dyncond</c> method. The returned Condition
    /// closes over the session and reads the property through the Lock at render time,
    /// making the UI reactive to property changes between prompt calls.
    /// </remarks>
    private Condition DynCond(Func<FilterOrBool> getter)
    {
        return new Condition(() => FilterUtils.ToFilter(getter()).Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // VALIDATION HELPERS
    // ═══════════════════════════════════════════════════════════════════

    private static void ValidateExceptionType(Type type, string paramName)
    {
        if (!typeof(Exception).IsAssignableFrom(type))
            throw new ArgumentException(
                $"Type '{type.FullName}' must be assignable to Exception.", paramName);

        if (type.IsAbstract)
            throw new ArgumentException(
                $"Type '{type.FullName}' must be a concrete (non-abstract) type.", paramName);

        // Verify it has a parameterless constructor
        try
        {
            Activator.CreateInstance(type);
        }
        catch (MissingMethodException)
        {
            throw new ArgumentException(
                $"Type '{type.FullName}' must have a parameterless constructor.", paramName);
        }
    }
}
