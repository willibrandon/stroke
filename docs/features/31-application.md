# Feature 31: Application

## Overview

Implement the Application class that orchestrates the entire prompt_toolkit runtime including layout, key bindings, rendering, and the event loop.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/application/application.py`

## Public API

### Application Class

```csharp
namespace Stroke.Application;

/// <summary>
/// The main Application class that glues everything together.
/// </summary>
/// <typeparam name="TResult">The result type when the application exits.</typeparam>
public sealed class Application<TResult>
{
    /// <summary>
    /// Creates an Application.
    /// </summary>
    /// <param name="layout">The layout for the application.</param>
    /// <param name="style">Custom style to apply.</param>
    /// <param name="includeDefaultPygmentsStyle">Include default syntax highlighting style.</param>
    /// <param name="styleTransformation">Style transformation to apply.</param>
    /// <param name="keyBindings">Key bindings for the application.</param>
    /// <param name="clipboard">Clipboard implementation.</param>
    /// <param name="fullScreen">Run on alternate screen buffer.</param>
    /// <param name="colorDepth">Color depth or callable returning color depth.</param>
    /// <param name="mouseSupport">Enable mouse support.</param>
    /// <param name="enablePageNavigationBindings">Enable page navigation bindings.</param>
    /// <param name="pasteMode">Enable paste mode.</param>
    /// <param name="editingMode">Editing mode (Emacs or Vi).</param>
    /// <param name="eraseWhenDone">Clear output when finished.</param>
    /// <param name="reverseViSearchDirection">Reverse Vi search direction.</param>
    /// <param name="minRedrawInterval">Minimum seconds between redraws.</param>
    /// <param name="maxRenderPostponeTime">Maximum postpone time for rendering.</param>
    /// <param name="refreshInterval">Auto-invalidate interval.</param>
    /// <param name="terminalSizePollingInterval">Terminal size polling interval.</param>
    /// <param name="cursor">Cursor shape configuration.</param>
    /// <param name="onReset">Callback during reset.</param>
    /// <param name="onInvalidate">Callback when UI is invalidated.</param>
    /// <param name="beforeRender">Callback before rendering.</param>
    /// <param name="afterRender">Callback after rendering.</param>
    /// <param name="input">Input implementation.</param>
    /// <param name="output">Output implementation.</param>
    public Application(
        Layout? layout = null,
        IBaseStyle? style = null,
        object? includeDefaultPygmentsStyle = null,
        IStyleTransformation? styleTransformation = null,
        IKeyBindingsBase? keyBindings = null,
        IClipboard? clipboard = null,
        bool fullScreen = false,
        object? colorDepth = null,
        object? mouseSupport = null,
        object? enablePageNavigationBindings = null,
        object? pasteMode = null,
        EditingMode editingMode = EditingMode.Emacs,
        bool eraseWhenDone = false,
        object? reverseViSearchDirection = null,
        double? minRedrawInterval = null,
        double? maxRenderPostponeTime = 0.01,
        double? refreshInterval = null,
        double? terminalSizePollingInterval = 0.5,
        object? cursor = null,
        Action<Application<TResult>>? onReset = null,
        Action<Application<TResult>>? onInvalidate = null,
        Action<Application<TResult>>? beforeRender = null,
        Action<Application<TResult>>? afterRender = null,
        IInput? input = null,
        IOutput? output = null);

    /// <summary>
    /// The layout for the application.
    /// </summary>
    public Layout Layout { get; set; }

    /// <summary>
    /// The custom style.
    /// </summary>
    public IBaseStyle? Style { get; set; }

    /// <summary>
    /// The style transformation.
    /// </summary>
    public IStyleTransformation StyleTransformation { get; set; }

    /// <summary>
    /// The application key bindings.
    /// </summary>
    public IKeyBindingsBase? KeyBindings { get; set; }

    /// <summary>
    /// The clipboard implementation.
    /// </summary>
    public IClipboard Clipboard { get; set; }

    /// <summary>
    /// True to run on alternate screen buffer.
    /// </summary>
    public bool FullScreen { get; }

    /// <summary>
    /// The active color depth.
    /// </summary>
    public ColorDepth ColorDepth { get; }

    /// <summary>
    /// Mouse support filter.
    /// </summary>
    public IFilter MouseSupport { get; }

    /// <summary>
    /// Paste mode filter.
    /// </summary>
    public IFilter PasteMode { get; }

    /// <summary>
    /// The editing mode.
    /// </summary>
    public EditingMode EditingMode { get; set; }

    /// <summary>
    /// Erase output when done.
    /// </summary>
    public bool EraseWhenDone { get; }

    /// <summary>
    /// Reverse Vi search direction filter.
    /// </summary>
    public IFilter ReverseViSearchDirection { get; }

    /// <summary>
    /// Page navigation bindings filter.
    /// </summary>
    public IFilter EnablePageNavigationBindings { get; }

    /// <summary>
    /// Minimum redraw interval in seconds.
    /// </summary>
    public double? MinRedrawInterval { get; }

    /// <summary>
    /// Maximum render postpone time in seconds.
    /// </summary>
    public double? MaxRenderPostponeTime { get; }

    /// <summary>
    /// Refresh interval in seconds.
    /// </summary>
    public double? RefreshInterval { get; }

    /// <summary>
    /// Terminal size polling interval in seconds.
    /// </summary>
    public double? TerminalSizePollingInterval { get; }

    /// <summary>
    /// Cursor shape configuration.
    /// </summary>
    public ICursorShapeConfig Cursor { get; }

    /// <summary>
    /// Event fired during reset.
    /// </summary>
    public Event<Application<TResult>> OnReset { get; }

    /// <summary>
    /// Event fired when UI is invalidated.
    /// </summary>
    public Event<Application<TResult>> OnInvalidate { get; }

    /// <summary>
    /// Event fired before rendering.
    /// </summary>
    public Event<Application<TResult>> BeforeRender { get; }

    /// <summary>
    /// Event fired after rendering.
    /// </summary>
    public Event<Application<TResult>> AfterRender { get; }

    /// <summary>
    /// The output implementation.
    /// </summary>
    public IOutput Output { get; }

    /// <summary>
    /// The input implementation.
    /// </summary>
    public IInput Input { get; }

    /// <summary>
    /// Callables to execute before run.
    /// </summary>
    public IList<Action> PreRunCallables { get; }

    /// <summary>
    /// True when the application is running.
    /// </summary>
    public bool IsRunning { get; }

    /// <summary>
    /// True when the application is done.
    /// </summary>
    public bool IsDone { get; }

    /// <summary>
    /// True when a redraw has been scheduled.
    /// </summary>
    public bool Invalidated { get; }

    /// <summary>
    /// Quoted insert mode flag.
    /// </summary>
    public bool QuotedInsert { get; set; }

    /// <summary>
    /// Vi state for Vi key bindings.
    /// </summary>
    public ViState ViState { get; }

    /// <summary>
    /// Emacs state for Emacs key bindings.
    /// </summary>
    public EmacsState EmacsState { get; }

    /// <summary>
    /// Timeout for flushing escape keys (like Vim's ttimeoutlen).
    /// </summary>
    public double TtimeoutLen { get; set; }

    /// <summary>
    /// Timeout for key sequences (like Vim's timeoutlen).
    /// </summary>
    public double TimeoutLen { get; set; }

    /// <summary>
    /// The renderer instance.
    /// </summary>
    public Renderer Renderer { get; }

    /// <summary>
    /// Render counter (increased every render).
    /// </summary>
    public int RenderCounter { get; }

    /// <summary>
    /// The key processor instance.
    /// </summary>
    public KeyProcessor KeyProcessor { get; }

    /// <summary>
    /// The currently focused buffer.
    /// </summary>
    public Buffer CurrentBuffer { get; }

    /// <summary>
    /// The current search state.
    /// </summary>
    public SearchState CurrentSearchState { get; }

    /// <summary>
    /// Reset the application state.
    /// </summary>
    public void Reset();

    /// <summary>
    /// Thread-safe invalidation trigger.
    /// </summary>
    public void Invalidate();

    /// <summary>
    /// Run the application asynchronously.
    /// </summary>
    /// <param name="preRun">Optional callback called after reset.</param>
    /// <param name="setExceptionHandler">Set exception handler.</param>
    /// <param name="handleSigint">Handle SIGINT signal.</param>
    /// <param name="slowCallbackDuration">Slow callback warning threshold.</param>
    /// <returns>The application result.</returns>
    public Task<TResult> RunAsync(
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true,
        double slowCallbackDuration = 0.5);

    /// <summary>
    /// Run the application synchronously.
    /// </summary>
    /// <param name="preRun">Optional callback called after reset.</param>
    /// <param name="setExceptionHandler">Set exception handler.</param>
    /// <param name="handleSigint">Handle SIGINT signal.</param>
    /// <param name="inThread">Run in a background thread.</param>
    /// <param name="inputHook">Input hook function.</param>
    /// <returns>The application result.</returns>
    public TResult Run(
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true,
        bool inThread = false,
        Func<Func<Task>, Task>? inputHook = null);

    /// <summary>
    /// Exit the application with a result.
    /// </summary>
    /// <param name="result">The result value.</param>
    /// <param name="style">Style to apply when exiting.</param>
    public void Exit(TResult? result = default, string style = "");

    /// <summary>
    /// Exit the application with an exception.
    /// </summary>
    /// <param name="exception">The exception to set.</param>
    /// <param name="style">Style to apply when exiting.</param>
    public void Exit(Exception exception, string style = "");

    /// <summary>
    /// Create a background task for the application.
    /// </summary>
    /// <param name="coroutine">The task to run.</param>
    /// <returns>The created task.</returns>
    public Task CreateBackgroundTask(Func<Task> coroutine);

    /// <summary>
    /// Cancel and wait for all background tasks.
    /// </summary>
    public Task CancelAndWaitForBackgroundTasks();

    /// <summary>
    /// Run a system command while hiding the prompt.
    /// </summary>
    /// <param name="command">Shell command to execute.</param>
    /// <param name="waitForEnter">Wait for Enter key when done.</param>
    /// <param name="displayBeforeText">Text to display before command.</param>
    /// <param name="waitText">Text for wait prompt.</param>
    public Task RunSystemCommand(
        string command,
        bool waitForEnter = true,
        object? displayBeforeText = null,
        string waitText = "Press ENTER to continue...");

    /// <summary>
    /// Suspend the application to background (Unix only).
    /// </summary>
    /// <param name="suspendGroup">Suspend the whole process group.</param>
    public void SuspendToBackground(bool suspendGroup = true);

    /// <summary>
    /// Print formatted text to the output.
    /// </summary>
    /// <param name="text">Text to print.</param>
    /// <param name="style">Style to apply.</param>
    public void PrintText(object text, IBaseStyle? style = null);

    /// <summary>
    /// Get list of used style strings (for debugging).
    /// </summary>
    public IList<string> GetUsedStyleStrings();
}
```

### EditingMode Enum

```csharp
namespace Stroke.Application;

/// <summary>
/// Editing mode for the application.
/// </summary>
public enum EditingMode
{
    /// <summary>
    /// Emacs editing mode.
    /// </summary>
    Emacs,

    /// <summary>
    /// Vi editing mode.
    /// </summary>
    Vi
}
```

### AppSession Class

```csharp
namespace Stroke.Application;

/// <summary>
/// Application session context.
/// </summary>
public sealed class AppSession : IDisposable
{
    /// <summary>
    /// Creates an AppSession.
    /// </summary>
    /// <param name="input">Input for this session.</param>
    /// <param name="output">Output for this session.</param>
    public AppSession(IInput? input = null, IOutput? output = null);

    /// <summary>
    /// The input for this session.
    /// </summary>
    public IInput Input { get; }

    /// <summary>
    /// The output for this session.
    /// </summary>
    public IOutput Output { get; }

    /// <summary>
    /// The current application.
    /// </summary>
    public Application<object>? App { get; }

    public void Dispose();
}
```

### Current Application Functions

```csharp
namespace Stroke.Application;

/// <summary>
/// Application context utilities.
/// </summary>
public static class AppContext
{
    /// <summary>
    /// Get the current application session.
    /// </summary>
    public static AppSession GetAppSession();

    /// <summary>
    /// Get the current application.
    /// </summary>
    public static Application<object> GetApp();

    /// <summary>
    /// Get the current application, or null.
    /// </summary>
    public static Application<object>? GetAppOrNull();

    /// <summary>
    /// Create a new application session.
    /// </summary>
    /// <param name="input">Input for the session.</param>
    /// <param name="output">Output for the session.</param>
    public static IDisposable CreateAppSession(
        IInput? input = null,
        IOutput? output = null);

    /// <summary>
    /// Set the current application.
    /// </summary>
    /// <param name="app">The application to set.</param>
    public static IDisposable SetApp(Application<object> app);
}
```

### Run In Terminal

```csharp
namespace Stroke.Application;

/// <summary>
/// Run code in terminal context.
/// </summary>
public static class RunInTerminal
{
    /// <summary>
    /// Run a function in the terminal.
    /// </summary>
    /// <param name="func">Function to run.</param>
    /// <param name="renderCliDone">Render CLI as done before running.</param>
    /// <param name="inExecutor">Run in executor thread.</param>
    public static Task RunInTerminalAsync(
        Func<Task> func,
        bool renderCliDone = false,
        bool inExecutor = true);

    /// <summary>
    /// Context manager for running in terminal.
    /// </summary>
    /// <param name="renderCliDone">Render CLI as done.</param>
    public static IAsyncDisposable InTerminal(bool renderCliDone = false);
}
```

## Project Structure

```
src/Stroke/
└── Application/
    ├── Application.cs
    ├── EditingMode.cs
    ├── AppSession.cs
    ├── AppContext.cs
    ├── RunInTerminal.cs
    ├── Event.cs
    └── CombinedRegistry.cs
tests/Stroke.Tests/
└── Application/
    ├── ApplicationTests.cs
    ├── AppSessionTests.cs
    ├── AppContextTests.cs
    └── RunInTerminalTests.cs
```

## Implementation Notes

### Application Lifecycle

1. **Initialization**:
   - Create layout (or use dummy layout)
   - Initialize key bindings with defaults
   - Create renderer, key processor
   - Initialize Vi and Emacs state
   - Call Reset()

2. **Run Loop (async)**:
   - Enter raw mode
   - Attach input handlers
   - Draw initial UI
   - Process input events
   - Exit when done

3. **Exit**:
   - Final render
   - Reset renderer
   - Detach handlers
   - Return result or throw exception

### Key Binding Registry

The Application uses a `_CombinedRegistry` that merges:
1. Key bindings from current focused control
2. Key bindings from parent containers
3. Application-level key bindings
4. Page navigation bindings (conditional)
5. Default bindings

### Invalidation

Thread-safe invalidation:
- Only schedule one redraw at a time
- Respect `min_redraw_interval` for throttling
- Use `max_render_postpone_time` for high-CPU situations
- Fire `on_invalidate` event

### Style Merging

Application merges styles in order:
1. Default UI style
2. Pygments style (conditional)
3. Custom user style

### Context Variables

Application uses context variables to track:
- Current application (`set_app`)
- Application session (`get_app_session`)
- Needed for key binding callbacks to access correct app

### Background Tasks

Application manages background tasks:
- `create_background_task` starts a new task
- Tasks are cancelled when application exits
- Exceptions in tasks are reported via event loop handler

### Signal Handling

On Unix:
- SIGWINCH triggers resize handling
- SIGTSTP enables suspend to background
- SIGINT sends <sigint> to key processor

## Dependencies

- `Stroke.Layout` (Feature 29) - Layout class
- `Stroke.Rendering.Renderer` (Feature 23) - Renderer
- `Stroke.KeyBinding.KeyProcessor` (Feature 19) - Key processing
- `Stroke.Input` (Feature 14) - Input abstraction
- `Stroke.Output` (Feature 16) - Output abstraction
- `Stroke.Styles` (Feature 03) - Style system
- `Stroke.Clipboard` (Feature 42) - Clipboard

## Implementation Tasks

1. Implement `Application<TResult>` class
2. Implement `EditingMode` enum
3. Implement `AppSession` class
4. Implement `AppContext` static methods
5. Implement `RunInTerminal` utilities
6. Implement `Event<T>` class
7. Implement `_CombinedRegistry` for merged key bindings
8. Implement style merging
9. Implement invalidation with throttling
10. Implement background task management
11. Implement signal handling (Unix)
12. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Application class matches Python Prompt Toolkit semantics
- [ ] Run/RunAsync work correctly
- [ ] Exit with result or exception works
- [ ] Key binding merging works correctly
- [ ] Invalidation is thread-safe
- [ ] Style merging works correctly
- [ ] Background tasks are managed properly
- [ ] Signal handling works on Unix
- [ ] Unit tests achieve 80% coverage
