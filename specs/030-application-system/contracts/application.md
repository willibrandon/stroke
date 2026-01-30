# Contract: Application\<TResult\>

**Namespace**: `Stroke.Application`
**Source**: `prompt_toolkit.application.application.Application`

## Class Signature

```csharp
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
/// <see cref="Reset"/>, <c>_redraw()</c>, <see cref="RunSystemCommandAsync"/>,
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
/// internal property or method that returns itself cast to <c>Application&lt;object?&gt;</c>
/// via <c>Unsafe.As</c> or a non-generic base interface. This is an implementation detail
/// not visible in the public API.
/// </para>
/// </remarks>
/// <para>
/// <b>Inheritance:</b> This class is NOT sealed because <see cref="DummyApplication"/>
/// inherits from it. User subclassing is not recommended but not prevented, matching
/// Python Prompt Toolkit's design where Application is a concrete class that
/// DummyApplication extends.
/// </para>
public class Application<TResult>
{
    // --- Constructor ---

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
        Layout? layout = null,
        IStyle? style = null,
        FilterOrBool includeDefaultPygmentsStyle = default, // true
        IStyleTransformation? styleTransformation = null,
        IKeyBindingsBase? keyBindings = null,
        IClipboard? clipboard = null,
        bool fullScreen = false,
        ColorDepthOption colorDepth = default,
        FilterOrBool mouseSupport = default, // false
        FilterOrBool? enablePageNavigationBindings = null,
        FilterOrBool pasteMode = default, // false
        EditingMode editingMode = EditingMode.Emacs,
        bool eraseWhenDone = false,
        FilterOrBool reverseViSearchDirection = default, // false
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
        IOutput? output = null);

    // --- Properties (public, mutable) ---

    /// <summary>The root layout for this application.</summary>
    public Layout Layout { get; set; }

    /// <summary>User-provided custom style. Null means use defaults only.</summary>
    public IStyle? Style { get; set; }

    /// <summary>Style transformation applied to merged style output.</summary>
    public IStyleTransformation StyleTransformation { get; set; }

    /// <summary>Application-level key bindings.</summary>
    public IKeyBindingsBase? KeyBindings { get; set; }

    /// <summary>Clipboard implementation.</summary>
    public IClipboard Clipboard { get; set; }

    /// <summary>Current editing mode (Vi or Emacs).</summary>
    public EditingMode EditingMode { get; set; }

    /// <summary>Whether quoted insert mode is active.</summary>
    public bool QuotedInsert { get; set; }

    /// <summary>
    /// Escape flush timeout in seconds. When this time elapses after an escape key,
    /// the escape is flushed as a standalone key. Like Vim's ttimeoutlen.
    /// </summary>
    public double TtimeoutLen { get; set; }

    /// <summary>
    /// Key sequence timeout in seconds. Maximum time to wait for a multi-key sequence
    /// to complete before dispatching what's available. Like Vim's timeoutlen.
    /// Null disables the timeout.
    /// </summary>
    public double? TimeoutLen { get; set; }

    /// <summary>
    /// Style string applied to the output content when the application exits.
    /// Set via the <see cref="Exit"/> method's <c>style</c> parameter.
    /// Reset to empty string during <see cref="Reset"/>.
    /// When non-empty, the renderer applies this style to the final output in the "done" render.
    /// </summary>
    public string ExitStyle { get; set; }

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
    public double? RefreshInterval { get; }

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

    /// <summary>Render counter incremented each time the UI is rendered. Used for cache invalidation.</summary>
    public int RenderCounter { get; }

    /// <summary>The key processor instance.</summary>
    public KeyProcessor KeyProcessor { get; }

    /// <summary>
    /// List of callables executed before each run. Items execute after <see cref="Reset"/>
    /// but before the first render. The list is cleared after execution. Items added between
    /// <see cref="Run"/> calls accumulate. Items added during a run execute on the next run.
    /// </summary>
    public List<Action> PreRunCallables { get; }

    // --- Computed Properties ---

    /// <summary>
    /// The active color depth, resolved from the explicit value, callable, or output default.
    /// </summary>
    public ColorDepth ColorDepth { get; }

    /// <summary>
    /// The currently focused Buffer, obtained from <c>Layout.CurrentBuffer</c>.
    /// If the focused control is not a BufferControl (i.e., Layout.CurrentBuffer is null),
    /// returns a new dummy Buffer named "dummy-buffer". A new dummy instance is created
    /// on each access (not a singleton). This avoids null checks throughout the codebase.
    /// </summary>
    public Buffer CurrentBuffer { get; }

    /// <summary>
    /// The SearchState for the currently focused BufferControl.
    /// If the focused control is a BufferControl, returns its SearchState.
    /// Otherwise, returns a new default SearchState instance. A new dummy instance is
    /// created on each access (not a singleton, not null). This avoids null checks
    /// throughout the codebase.
    /// </summary>
    public SearchState CurrentSearchState { get; }

    /// <summary>True when the application is currently active/running.</summary>
    public bool IsRunning { get; }

    /// <summary>True when the application future has been completed (result or exception set).</summary>
    public bool IsDone { get; }

    /// <summary>True when a redraw has been scheduled but not yet executed.</summary>
    public bool Invalidated { get; }

    // --- Events ---

    /// <summary>Fired during Reset().</summary>
    public Event<Application<TResult>> OnReset { get; }

    /// <summary>Fired when Invalidate() is called.</summary>
    public Event<Application<TResult>> OnInvalidate { get; }

    /// <summary>
    /// Fired immediately before rendering. If a <c>beforeRender</c> callback was passed
    /// to the constructor, it is registered as the first handler of this event during
    /// construction. Additional handlers can be added via the <c>+=</c> operator.
    /// Constructor callback fires first, then any subsequently added handlers.
    /// </summary>
    public Event<Application<TResult>> BeforeRender { get; }

    /// <summary>
    /// Fired immediately after rendering. If an <c>afterRender</c> callback was passed
    /// to the constructor, it is registered as the first handler of this event during
    /// construction. Additional handlers can be added via the <c>+=</c> operator.
    /// Constructor callback fires first, then any subsequently added handlers.
    /// </summary>
    public Event<Application<TResult>> AfterRender { get; }

    // --- Methods ---

    /// <summary>
    /// Reset the application to a clean state. Execution order:
    /// <list type="number">
    /// <item>Set <see cref="ExitStyle"/> to empty string</item>
    /// <item>Create new empty <c>_backgroundTasks</c> set</item>
    /// <item>Call <see cref="Renderer"/>.Reset() — clears cached screen, style cache, cursor position</item>
    /// <item>Call <see cref="KeyProcessor"/>.Reset() — clears key buffer, argument, input queue</item>
    /// <item>Call <see cref="Layout"/>.Reset() — resets all containers in the tree</item>
    /// <item>Call <see cref="ViState"/>.Reset() — resets input mode to Insert, clears registers</item>
    /// <item>Call <see cref="EmacsState"/>.Reset() — stops macro recording</item>
    /// <item>Fire <see cref="OnReset"/> event</item>
    /// <item>Ensure a focusable control has focus: if the current control is not focusable, iterate all windows and focus the first one whose content is focusable</item>
    /// </list>
    /// Does NOT clear buffer contents (preserves text between runs for REPL scenarios).
    /// Does NOT clear the focus stack (preserves focus history).
    /// </summary>
    public void Reset();

    /// <summary>
    /// Thread-safe way to schedule a UI repaint. Safe to call from any thread.
    /// <list type="bullet">
    /// <item>If the application is not running, this is a no-op (no exception).</item>
    /// <item>If the application's async context loop is null or closed, this is a no-op.</item>
    /// <item>If a redraw is already scheduled (<c>_invalidated == true</c>), this is a no-op
    /// (concurrent calls coalesce to a single redraw).</item>
    /// <item>Otherwise, sets <c>_invalidated = true</c>, fires <see cref="OnInvalidate"/>,
    /// and schedules a redraw respecting <see cref="MinRedrawInterval"/> and
    /// <see cref="MaxRenderPostponeTime"/>.</item>
    /// </list>
    /// Synchronization: uses Interlocked or Lock for the <c>_invalidated</c> flag to ensure
    /// concurrent calls from multiple threads safely coalesce.
    /// </summary>
    public void Invalidate();

    /// <summary>
    /// Run the application asynchronously until Exit() is called.
    /// Returns the value passed to Exit().
    /// </summary>
    /// <param name="preRun">Optional callback executed after reset but before rendering.</param>
    /// <param name="setExceptionHandler">Display exceptions in terminal instead of crashing.</param>
    /// <param name="handleSigint">Handle SIGINT signal by sending to key processor.</param>
    /// <returns>The result value passed to Exit().</returns>
    /// <exception cref="InvalidOperationException">The application is already running.</exception>
    /// <exception cref="EndOfStreamException">The input stream was closed unexpectedly.</exception>
    public Task<TResult> RunAsync(
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true);

    /// <summary>
    /// Blocking run that waits until the UI is finished.
    /// Creates a new async context if needed.
    /// </summary>
    /// <param name="preRun">Optional callback executed after reset.</param>
    /// <param name="setExceptionHandler">Display exceptions in terminal.</param>
    /// <param name="handleSigint">Handle SIGINT signal.</param>
    /// <param name="inThread">Run on a background thread.</param>
    /// <param name="inputHook">Custom event loop integration hook.</param>
    /// <returns>The result value.</returns>
    /// <exception cref="InvalidOperationException">The application is already running.</exception>
    /// <exception cref="EndOfStreamException">The input stream was closed unexpectedly.</exception>
    public TResult Run(
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true,
        bool inThread = false,
        InputHook? inputHook = null);

    /// <summary>
    /// Exit the application with a result or exception.
    /// </summary>
    /// <param name="result">Result value to return from RunAsync.</param>
    /// <param name="exception">Exception to throw from RunAsync.</param>
    /// <param name="style">Style to apply to content on exit. Stored in <see cref="ExitStyle"/>.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when: (1) the application is not running (message: "Application is not running") —
    /// i.e., <see cref="RunAsync"/> has not been called or has already completed; or
    /// (2) the result has already been set (message: "Result has already been set") —
    /// i.e., <see cref="Exit"/> was already called.
    /// </exception>
    public void Exit(
        TResult? result = default,
        Exception? exception = null,
        string style = "");

    /// <summary>
    /// Start a background task that will be cancelled when the application exits.
    /// The <paramref name="taskFactory"/> receives a <see cref="CancellationToken"/> that
    /// is linked to a per-application <see cref="CancellationTokenSource"/>. All background
    /// tasks share the same token source — when the application exits, all tokens are
    /// cancelled simultaneously. The task is tracked in a <c>HashSet&lt;Task&gt;</c>
    /// (protected by Lock) and removed when the task completes.
    /// </summary>
    /// <param name="taskFactory">Factory that creates the task, given a cancellation token.
    /// The token is per-application (shared across all background tasks).</param>
    /// <returns>The running task. Returns a completed task if the application is not running.</returns>
    public Task CreateBackgroundTask(Func<CancellationToken, Task> taskFactory);

    /// <summary>
    /// Cancel all background tasks and wait for cancellation to complete.
    /// </summary>
    public Task CancelAndWaitForBackgroundTasksAsync();

    /// <summary>
    /// Run a system command while the application is suspended.
    /// Shell resolution is platform-specific:
    /// <list type="bullet">
    /// <item>Unix: <c>/bin/sh -c "{command}"</c></item>
    /// <item>Windows: <c>cmd /c "{command}"</c></item>
    /// </list>
    /// The application is hidden (renderer erased, raw mode exited) before
    /// the command executes, and resumed afterward.
    /// </summary>
    /// <param name="command">Shell command to execute.</param>
    /// <param name="waitForEnter">Wait for ENTER after command finishes.</param>
    /// <param name="displayBeforeText">Text to display before the command.</param>
    /// <param name="waitText">Prompt text while waiting for ENTER.</param>
    public Task RunSystemCommandAsync(
        string command,
        bool waitForEnter = true,
        AnyFormattedText displayBeforeText = default,
        string waitText = "Press ENTER to continue...");

    /// <summary>
    /// Suspend the process to background (Unix only, via SIGTSTP).
    /// No-op on Windows (SIGTSTP is not available on Windows).
    /// On Unix: erases renderer output, resets terminal mode, sends SIGTSTP to
    /// the process (or process group if <paramref name="suspendGroup"/> is true).
    /// On resume (SIGCONT): re-enters raw mode and redraws.
    /// </summary>
    /// <param name="suspendGroup">When true, suspend the whole process group.</param>
    public void SuspendToBackground(bool suspendGroup = true);

    /// <summary>
    /// Print formatted text to the output.
    /// When the UI is running, this should be called through RunInTerminal.
    /// </summary>
    /// <param name="text">Formatted text to print.</param>
    /// <param name="style">Style to use. Defaults to the application's merged style.</param>
    public void PrintText(AnyFormattedText text, IStyle? style = null);

    /// <summary>
    /// Return a sorted list of used style strings. Useful for debugging.
    /// </summary>
    public List<string> GetUsedStyleStrings();
}
```

## Supporting Types

```csharp
/// <summary>
/// Delegate type for Application event handlers.
/// </summary>
public delegate void ApplicationEventHandler<TResult>(Application<TResult> application);

/// <summary>
/// Represents a color depth option that can be a fixed value, a callable, or null (auto-detect).
/// </summary>
public readonly struct ColorDepthOption
{
    public ColorDepthOption(ColorDepth value);
    public ColorDepthOption(Func<ColorDepth?> factory);

    public static implicit operator ColorDepthOption(ColorDepth value);
    public static implicit operator ColorDepthOption(ColorDepth? value);
    public static implicit operator ColorDepthOption(Func<ColorDepth?> factory);

    /// <summary>
    /// Resolve the color depth using this priority order:
    /// 1. If a fixed <see cref="ColorDepth"/> value was provided, return it.
    /// 2. If a callable (<see cref="Func{ColorDepth?}"/>) was provided, invoke it.
    ///    If the callable returns non-null, return that value.
    /// 3. Fall back to <paramref name="output"/>.GetDefaultColorDepth().
    /// </summary>
    /// <param name="output">The output device for fallback color depth detection.</param>
    /// <returns>The resolved color depth.</returns>
    public ColorDepth Resolve(IOutput output);
}

/// <summary>
/// Delegate for input hook integration. Called when the application is idle
/// and waiting for input, allowing custom event loop processing.
/// </summary>
/// <param name="context">The input hook context providing file descriptor info.</param>
public delegate void InputHook(InputHookContext context);

/// <summary>
/// Context provided to InputHook callbacks.
/// </summary>
public sealed class InputHookContext
{
    /// <summary>The file descriptor to monitor for input.</summary>
    public int FileDescriptor { get; }

    /// <summary>Signal that input is available.</summary>
    public void InputIsReady();
}
```
