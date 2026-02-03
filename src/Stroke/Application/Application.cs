using Stroke.Clipboard;
using Stroke.Core;
using Stroke.CursorShapes;
using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.Output;
using Stroke.Rendering;
using Stroke.Styles;

// Alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Application;

/// <summary>
/// The main Application class that orchestrates layout, key bindings, rendering,
/// input processing, and the event loop. This is the central entry point for
/// creating interactive terminal applications.
/// </summary>
/// <typeparam name="TResult">The type of result returned when the application exits.</typeparam>
/// <remarks>
/// <para>
/// <b>Thread safety contract:</b>
/// <list type="bullet">
/// <item><b>Safe from any thread:</b> <see cref="Invalidate"/>, <see cref="Exit"/>,
/// <see cref="CreateBackgroundTask"/>, property getters for immutable properties
/// (FullScreen, EraseWhenDone, MouseSupport, etc.), <see cref="RenderCounter"/> (Interlocked).</item>
/// <item><b>Async context only:</b> <see cref="RunAsync"/>, <see cref="Run"/>,
/// <see cref="Reset"/>, <c>_Redraw()</c>, <see cref="RunSystemCommandAsync"/>,
/// <see cref="PrintText"/>, <see cref="SuspendToBackground"/>. These methods perform
/// rendering, key processing, or I/O that must not be concurrent.</item>
/// <item><b>Mutable property setters</b> (Layout, Style, KeyBindings, Clipboard, EditingMode,
/// QuotedInsert, TtimeoutLen, TimeoutLen, ExitStyle): synchronized via Lock.</item>
/// </list>
/// </para>
/// <para>
/// <b>Generic covariance note:</b> C# classes are invariant, so <c>Application&lt;string&gt;</c>
/// cannot be assigned to <c>Application&lt;object?&gt;</c>. Internal components that need to
/// accept any application (CombinedRegistry, Renderer, KeyPressEvent) use
/// <c>Application&lt;object?&gt;</c> as the parameter type. The Application class provides an
/// internal property that returns itself cast to <c>Application&lt;object?&gt;</c>
/// via <c>Unsafe.As</c>. This is an implementation detail not visible in the public API.
/// </para>
/// <para>
/// <b>Inheritance:</b> This class is NOT sealed because <see cref="DummyApplication"/>
/// inherits from it. User subclassing is not recommended but not prevented, matching
/// Python Prompt Toolkit's design where Application is a concrete class that
/// DummyApplication extends.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Application</c> class from
/// <c>prompt_toolkit.application.application</c>.
/// </para>
/// </remarks>
public partial class Application<TResult> : IApplicationDoneCheck
{
    private readonly Lock _lock = new();
    private readonly ColorDepthOption _colorDepthOption;
    private readonly IFilter _includeDefaultPygmentsStyle;

    // Mutable properties (synchronized via _lock)
    private Layout.Layout _layout;
    private IStyle? _style;
    private IStyleTransformation _styleTransformation;
    private IKeyBindingsBase? _keyBindings;
    private IClipboard _clipboard;
    private EditingMode _editingMode;
    private bool _quotedInsert;
    private double _ttimeoutLen;
    private double? _timeoutLen;
    private string _exitStyle;
    private double? _refreshInterval;

    // Internal state
    internal bool _isRunning;
    internal TaskCompletionSource<TResult>? _future;
    internal int _invalidated;
    internal List<Event<object>> _invalidateEvents = [];
    internal double _lastRedrawTime;
    internal int _renderCounter;
    internal HashSet<Task> _backgroundTasks = [];
    internal CancellationTokenSource? _backgroundTasksCts;

    // Redraw signaling channel. ScheduleRedraw writes, RunAsync loop reads.
    // Bounded(1) with DropWrite coalesces multiple signals into one redraw.
    internal System.Threading.Channels.Channel<bool>? _redrawChannel;

    // Action channel for marshaling callbacks (flush timeout, SIGINT) to the
    // RunAsync async context. Unbounded because actions must not be dropped.
    internal System.Threading.Channels.Channel<Action>? _actionChannel;

    // RunInTerminal state
    internal bool _runningInTerminal;
    internal TaskCompletionSource<object?>? _runningInTerminalFuture;

    /// <summary>
    /// Create a new Application instance.
    /// </summary>
    /// <param name="layout">The root layout. Defaults to a dummy layout if null.</param>
    /// <param name="style">User-provided style. Merged with default UI and Pygments styles.</param>
    /// <param name="includeDefaultPygmentsStyle">Whether to include Pygments syntax highlighting style.</param>
    /// <param name="styleTransformation">Transformation applied to the merged style output.</param>
    /// <param name="keyBindings">Application-level key bindings.</param>
    /// <param name="clipboard">Clipboard implementation. Defaults to InMemoryClipboard.</param>
    /// <param name="fullScreen">When true, use the alternate screen buffer.</param>
    /// <param name="colorDepth">Explicit color depth, callable, or null for auto-detection.</param>
    /// <param name="mouseSupport">Filter controlling mouse support. <c>default</c> (FilterOrBool struct default) is treated as <c>false</c>.</param>
    /// <param name="enablePageNavigationBindings">Filter for page navigation. <c>null</c> defaults to a Condition that returns <c>fullScreen</c>. <c>default</c> FilterOrBool without HasValue is treated as <c>false</c>.</param>
    /// <param name="pasteMode">Filter controlling paste mode. <c>default</c> is treated as <c>false</c>.</param>
    /// <param name="editingMode">Initial editing mode (Vi or Emacs).</param>
    /// <param name="eraseWhenDone">Clear terminal output when the application finishes.</param>
    /// <param name="reverseViSearchDirection">Reverse Vi search direction (for Readline compatibility). <c>default</c> is treated as <c>false</c>.</param>
    /// <param name="minRedrawInterval">Minimum seconds between redraws. Null means no throttle.</param>
    /// <param name="maxRenderPostponeTime">Max seconds to postpone rendering under load. Default 0.01.</param>
    /// <param name="refreshInterval">Auto-invalidation interval in seconds. Null disables.</param>
    /// <param name="terminalSizePollingInterval">Polling interval for terminal size. Default 0.5s.</param>
    /// <param name="cursor">Cursor shape configuration.</param>
    /// <param name="onReset">Callback invoked during reset.</param>
    /// <param name="onInvalidate">Callback invoked when UI is invalidated.</param>
    /// <param name="beforeRender">Callback invoked before rendering.</param>
    /// <param name="afterRender">Callback invoked after rendering.</param>
    /// <param name="input">Input implementation. Defaults to AppSession's input.</param>
    /// <param name="output">Output implementation. Defaults to AppSession's output.</param>
    public Application(
        Layout.Layout? layout = null,
        IStyle? style = null,
        FilterOrBool includeDefaultPygmentsStyle = default,
        IStyleTransformation? styleTransformation = null,
        IKeyBindingsBase? keyBindings = null,
        IClipboard? clipboard = null,
        bool fullScreen = false,
        ColorDepthOption colorDepth = default,
        FilterOrBool mouseSupport = default,
        FilterOrBool? enablePageNavigationBindings = null,
        FilterOrBool pasteMode = default,
        EditingMode editingMode = EditingMode.Emacs,
        bool eraseWhenDone = false,
        FilterOrBool reverseViSearchDirection = default,
        double? minRedrawInterval = null,
        double? maxRenderPostponeTime = 0.01,
        double? refreshInterval = null,
        double? terminalSizePollingInterval = 0.5,
        ICursorShapeConfig? cursor = null,
        Action<Application<TResult>>? onReset = null,
        Action<Application<TResult>>? onInvalidate = null,
        Action<Application<TResult>>? beforeRender = null,
        Action<Application<TResult>>? afterRender = null,
        IInput? input = null,
        IOutput? output = null)
    {
        // Resolve enablePageNavigationBindings: null means condition on fullScreen
        if (enablePageNavigationBindings is null)
        {
            enablePageNavigationBindings = new FilterOrBool(
                new Condition(() => FullScreen));
        }

        // Convert FilterOrBool values to IFilter.
        // If includeDefaultPygmentsStyle has no value (default struct), treat as true.
        _includeDefaultPygmentsStyle = includeDefaultPygmentsStyle.HasValue
            ? FilterUtils.ToFilter(includeDefaultPygmentsStyle)
            : Always.Instance;
        PasteMode = FilterUtils.ToFilter(pasteMode);
        MouseSupport = FilterUtils.ToFilter(mouseSupport);
        ReverseViSearchDirection = FilterUtils.ToFilter(reverseViSearchDirection);
        EnablePageNavigationBindings = FilterUtils.ToFilter(enablePageNavigationBindings.Value);

        // Layout
        _layout = layout ?? DummyLayout.Create();

        // Style
        _style = style;
        _styleTransformation = styleTransformation ?? DummyStyleTransformation.Instance;
        _colorDepthOption = colorDepth;

        // Key bindings
        _keyBindings = keyBindings;
        DefaultBindings = DefaultKeyBindings.Load();
        PageNavigationBindings = DefaultKeyBindings.LoadPageNavigation();

        // Clipboard
        _clipboard = clipboard ?? new InMemoryClipboard();

        // Editing
        FullScreen = fullScreen;
        _editingMode = editingMode;
        EraseWhenDone = eraseWhenDone;

        // Timing
        MinRedrawInterval = minRedrawInterval;
        MaxRenderPostponeTime = maxRenderPostponeTime;
        _refreshInterval = refreshInterval;
        TerminalSizePollingInterval = terminalSizePollingInterval;

        // Cursor
        Cursor = cursor ?? new SimpleCursorShapeConfig();

        // Events
        OnReset = new Event<Application<TResult>>(this, onReset);
        OnInvalidate = new Event<Application<TResult>>(this, onInvalidate);
        BeforeRender = new Event<Application<TResult>>(this, beforeRender);
        AfterRender = new Event<Application<TResult>>(this, afterRender);

        // I/O
        var session = AppContext.GetAppSession();
        Output = output ?? session.Output;
        Input = input ?? session.Input;

        // Pre-run callables
        PreRunCallables = [];

        // Running state
        _isRunning = false;
        _future = null;
        _quotedInsert = false;
        _exitStyle = "";

        // Vi/Emacs state
        ViState = new ViState();
        EmacsState = new EmacsState();

        // Timeouts
        _ttimeoutLen = 0.5;
        _timeoutLen = 1.0;

        // Create merged style
        MergedStyle = CreateMergedStyle();

        // Renderer
        Renderer = new Renderer(
            MergedStyle,
            Output,
            fullScreen: FullScreen,
            mouseSupport: MouseSupport,
            cprNotSupportedCallback: CprNotSupportedCallback);

        // Render counter
        _renderCounter = 0;

        // Invalidation state
        _invalidated = 0;
        _invalidateEvents = [];
        _lastRedrawTime = 0.0;

        // Key processor (with combined registry)
        KeyProcessor = new KeyProcessor(new CombinedRegistry(UnsafeCast));

        // RunInTerminal state
        _runningInTerminal = false;
        _runningInTerminalFuture = null;

        // Reset (trigger initialize callback)
        Reset();
    }

    /// <summary>
    /// Protected constructor for DummyApplication to bypass normal initialization.
    /// </summary>
    protected Application(
        IInput input,
        IOutput output,
        bool isDummy)
    {
        // Minimal initialization for DummyApplication
        _layout = DummyLayout.Create();
        _style = null;
        _styleTransformation = DummyStyleTransformation.Instance;
        _colorDepthOption = default;
        _includeDefaultPygmentsStyle = Always.Instance;
        _keyBindings = null;
        _clipboard = new InMemoryClipboard();
        _editingMode = EditingMode.Emacs;
        _quotedInsert = false;
        _ttimeoutLen = 0.5;
        _timeoutLen = 1.0;
        _exitStyle = "";

        FullScreen = false;
        EraseWhenDone = false;
        MouseSupport = Never.Instance;
        PasteMode = Never.Instance;
        ReverseViSearchDirection = Never.Instance;
        EnablePageNavigationBindings = Never.Instance;
        MinRedrawInterval = null;
        MaxRenderPostponeTime = null;
        RefreshInterval = null;
        TerminalSizePollingInterval = null;
        Cursor = new SimpleCursorShapeConfig();

        OnReset = new Event<Application<TResult>>(this);
        OnInvalidate = new Event<Application<TResult>>(this);
        BeforeRender = new Event<Application<TResult>>(this);
        AfterRender = new Event<Application<TResult>>(this);

        Output = output;
        Input = input;

        PreRunCallables = [];

        ViState = new ViState();
        EmacsState = new EmacsState();

        DefaultBindings = DefaultKeyBindings.Load();
        PageNavigationBindings = DefaultKeyBindings.LoadPageNavigation();

        MergedStyle = CreateMergedStyle();
        Renderer = new Renderer(MergedStyle, Output);
        _renderCounter = 0;
        KeyProcessor = new KeyProcessor(new CombinedRegistry(UnsafeCast));
    }

    // --- Properties (public, mutable with Lock) ---

    /// <summary>The root layout for this application.</summary>
    public Layout.Layout Layout
    {
        get { using (_lock.EnterScope()) { return _layout; } }
        set { using (_lock.EnterScope()) { _layout = value; } }
    }

    /// <summary>User-provided custom style. Null means use defaults only.</summary>
    public IStyle? Style
    {
        get { using (_lock.EnterScope()) { return _style; } }
        set { using (_lock.EnterScope()) { _style = value; } }
    }

    /// <summary>Style transformation applied to merged style output.</summary>
    public IStyleTransformation StyleTransformation
    {
        get { using (_lock.EnterScope()) { return _styleTransformation; } }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            using (_lock.EnterScope()) { _styleTransformation = value; }
        }
    }

    /// <summary>Application-level key bindings.</summary>
    public IKeyBindingsBase? KeyBindings
    {
        get { using (_lock.EnterScope()) { return _keyBindings; } }
        set { using (_lock.EnterScope()) { _keyBindings = value; } }
    }

    /// <summary>Clipboard implementation.</summary>
    public IClipboard Clipboard
    {
        get { using (_lock.EnterScope()) { return _clipboard; } }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            using (_lock.EnterScope()) { _clipboard = value; }
        }
    }

    /// <summary>Current editing mode (Vi or Emacs).</summary>
    public EditingMode EditingMode
    {
        get { using (_lock.EnterScope()) { return _editingMode; } }
        set { using (_lock.EnterScope()) { _editingMode = value; } }
    }

    /// <summary>Whether quoted insert mode is active.</summary>
    public bool QuotedInsert
    {
        get { using (_lock.EnterScope()) { return _quotedInsert; } }
        set { using (_lock.EnterScope()) { _quotedInsert = value; } }
    }

    /// <summary>
    /// Escape flush timeout in seconds. When this time elapses after an escape key,
    /// the escape is flushed as a standalone key. Like Vim's ttimeoutlen.
    /// </summary>
    public double TtimeoutLen
    {
        get { using (_lock.EnterScope()) { return _ttimeoutLen; } }
        set { using (_lock.EnterScope()) { _ttimeoutLen = value; } }
    }

    /// <summary>
    /// Key sequence timeout in seconds. Maximum time to wait for a multi-key sequence
    /// to complete before dispatching what's available. Like Vim's timeoutlen.
    /// Null disables the timeout.
    /// </summary>
    public double? TimeoutLen
    {
        get { using (_lock.EnterScope()) { return _timeoutLen; } }
        set { using (_lock.EnterScope()) { _timeoutLen = value; } }
    }

    /// <summary>
    /// Style string applied to the output content when the application exits.
    /// Set via the <see cref="Exit"/> method's <c>style</c> parameter.
    /// Reset to empty string during <see cref="Reset"/>.
    /// </summary>
    public string ExitStyle
    {
        get { using (_lock.EnterScope()) { return _exitStyle; } }
        set { using (_lock.EnterScope()) { _exitStyle = value; } }
    }

    // --- Properties (public, read-only) ---

    /// <summary>Whether to run in full-screen mode (alternate screen buffer).</summary>
    public bool FullScreen { get; }

    /// <summary>Whether to erase output when the application finishes.</summary>
    public bool EraseWhenDone { get; }

    /// <summary>Filter controlling mouse support.</summary>
    public IFilter MouseSupport { get; }

    /// <summary>Filter controlling paste mode.</summary>
    public IFilter PasteMode { get; }

    /// <summary>Filter controlling reverse Vi search direction.</summary>
    public IFilter ReverseViSearchDirection { get; }

    /// <summary>Filter controlling page navigation bindings.</summary>
    public IFilter EnablePageNavigationBindings { get; }

    /// <summary>Minimum seconds between redraws. Null means no throttle.</summary>
    public double? MinRedrawInterval { get; }

    /// <summary>Max seconds to postpone rendering under heavy load.</summary>
    public double? MaxRenderPostponeTime { get; }

    /// <summary>Auto-invalidation interval in seconds.</summary>
    /// <remarks>
    /// Settable to allow per-prompt override from <c>PromptSession</c>.
    /// Protected by Lock for thread safety.
    /// </remarks>
    public double? RefreshInterval
    {
        get { using (_lock.EnterScope()) { return _refreshInterval; } }
        set { using (_lock.EnterScope()) { _refreshInterval = value; } }
    }

    /// <summary>Terminal size polling interval in seconds.</summary>
    public double? TerminalSizePollingInterval { get; }

    /// <summary>Cursor shape configuration.</summary>
    public ICursorShapeConfig Cursor { get; }

    /// <summary>The input device for this application.</summary>
    public IInput Input { get; }

    /// <summary>The output device for this application.</summary>
    public IOutput Output { get; }

    /// <summary>Vi editing mode state.</summary>
    public ViState ViState { get; }

    /// <summary>Emacs editing mode state.</summary>
    public EmacsState EmacsState { get; }

    /// <summary>The renderer instance.</summary>
    public Renderer Renderer { get; }

    /// <summary>The key processor instance.</summary>
    public KeyProcessor KeyProcessor { get; }

    /// <summary>
    /// List of callables executed before each run. Items execute after <see cref="Reset"/>
    /// but before the first render. The list is cleared after execution. Items added between
    /// <see cref="Run"/> calls accumulate. Items added during a run execute on the next run.
    /// </summary>
    public List<Action> PreRunCallables { get; }

    /// <summary>The merged style for rendering (UI + Pygments + user).</summary>
    internal IStyle MergedStyle { get; }

    /// <summary>The default key bindings.</summary>
    internal IKeyBindingsBase DefaultBindings { get; }

    /// <summary>The page navigation key bindings.</summary>
    internal IKeyBindingsBase PageNavigationBindings { get; }

    // --- Computed Properties ---

    /// <summary>Render counter incremented each time the UI is rendered. Used for cache invalidation.</summary>
    public int RenderCounter => _renderCounter;

    /// <summary>
    /// The active color depth, resolved from the explicit value, callable, or output default.
    /// </summary>
    public ColorDepth ColorDepth => _colorDepthOption.Resolve(Output);

    /// <summary>
    /// The currently focused Buffer, obtained from <c>Layout.CurrentBuffer</c>.
    /// If the focused control is not a BufferControl, returns a new dummy Buffer
    /// named "dummy-buffer". A new dummy instance is created on each access.
    /// </summary>
    public Buffer CurrentBuffer
    {
        get
        {
            var buffer = Layout.CurrentBuffer;
            if (buffer is null)
            {
                return new Buffer(name: "dummy-buffer");
            }
            return buffer;
        }
    }

    /// <summary>
    /// The SearchState for the currently focused BufferControl.
    /// If the focused control is a BufferControl, returns its SearchState.
    /// Otherwise, returns a new default SearchState instance.
    /// </summary>
    public SearchState CurrentSearchState
    {
        get
        {
            var control = Layout.CurrentControl;
            if (control is BufferControl bc)
            {
                return bc.SearchState;
            }
            return new SearchState();
        }
    }

    /// <summary>True when the application is currently active/running.</summary>
    public bool IsRunning => _isRunning;

    /// <summary>True when the application future has been completed (result or exception set).</summary>
    public bool IsDone => _future is not null && _future.Task.IsCompleted;

    /// <summary>True when a redraw has been scheduled but not yet executed.</summary>
    public bool Invalidated => _invalidated != 0;

    // IApplicationDoneCheck implementation
    bool IApplicationDoneCheck.IsDone => IsDone;

    // --- Events ---

    /// <summary>Fired during Reset().</summary>
    public Event<Application<TResult>> OnReset { get; }

    /// <summary>Fired when Invalidate() is called.</summary>
    public Event<Application<TResult>> OnInvalidate { get; }

    /// <summary>
    /// Fired immediately before rendering. If a <c>beforeRender</c> callback was passed
    /// to the constructor, it is registered as the first handler of this event during
    /// construction. Additional handlers can be added via the <c>+=</c> operator.
    /// </summary>
    public Event<Application<TResult>> BeforeRender { get; }

    /// <summary>
    /// Fired immediately after rendering. If an <c>afterRender</c> callback was passed
    /// to the constructor, it is registered as the first handler of this event during
    /// construction. Additional handlers can be added via the <c>+=</c> operator.
    /// </summary>
    public Event<Application<TResult>> AfterRender { get; }

    // --- Internal: Generic covariance cast ---

    /// <summary>
    /// Returns this application cast to Application&lt;object?&gt; for use by internal
    /// components that need to accept any application type.
    /// </summary>
    internal Application<object?> UnsafeCast
    {
        get
        {
            // Application<TResult> is a class (reference type). Since C# generics are invariant,
            // we use Unsafe.As to reinterpret the reference. This is safe because the internal
            // components only use the base members (Layout, KeyBindings, etc.) which are
            // independent of TResult.
            return System.Runtime.CompilerServices.Unsafe.As<Application<object?>>(this);
        }
    }

    // --- Private helpers ---

    /// <summary>
    /// Create a merged style from default UI, conditional Pygments, and dynamic user style.
    /// </summary>
    private IStyle CreateMergedStyle()
    {
        var dummyStyle = DummyStyle.Instance;
        var pygmentsStyle = DefaultStyles.DefaultPygmentsStyle;

        // Conditional pygments style: include only if filter is true
        var conditionalPygmentsStyle = new DynamicStyle(() =>
            _includeDefaultPygmentsStyle.Invoke() ? pygmentsStyle : dummyStyle);

        return StyleMerger.MergeStyles([
            DefaultStyles.DefaultUiStyle,
            conditionalPygmentsStyle,
            new DynamicStyle(() => _style),
        ]);
    }

    /// <summary>
    /// Callback invoked when CPR is not supported by the terminal.
    /// </summary>
    private void CprNotSupportedCallback()
    {
        // Currently a no-op. In the future, could log or adjust behavior.
    }
}
