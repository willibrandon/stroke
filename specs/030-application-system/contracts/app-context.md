# Contract: AppSession, AppContext, RunInTerminal

**Namespace**: `Stroke.Application`
**Source**: `prompt_toolkit.application.current`, `prompt_toolkit.application.run_in_terminal`

## AppSession

```csharp
/// <summary>
/// An interactive session connected to one terminal. Within one session,
/// multiple applications can run sequentially. The input/output device is
/// not supposed to change during one session.
/// </summary>
/// <remarks>
/// Always use <see cref="AppContext.CreateAppSession"/> to create an instance
/// so that it gets activated correctly.
/// This class is thread-safe.
/// </remarks>
public sealed class AppSession : IDisposable
{
    /// <summary>
    /// Create a new AppSession with optional input/output.
    /// </summary>
    /// <param name="input">Default input for applications in this session.</param>
    /// <param name="output">Default output for applications in this session.</param>
    public AppSession(IInput? input = null, IOutput? output = null);

    /// <summary>
    /// The default input for this session. Created lazily via InputFactory if not provided.
    /// </summary>
    public IInput Input { get; }

    /// <summary>
    /// The default output for this session. Created lazily via OutputFactory if not provided.
    /// </summary>
    public IOutput Output { get; }

    /// <summary>
    /// The currently active application in this session, or null if none is running.
    /// Set internally by the application during RunAsync via <see cref="AppContext.SetApp"/>.
    /// </summary>
    /// <remarks>
    /// Internal visibility rationale: this property is set only by the Application's
    /// RunAsync lifecycle. Exposing it publicly would allow external code to break the
    /// context invariant (that App reflects the actually running application). The
    /// public API provides <see cref="AppContext.GetApp"/> and <see cref="AppContext.SetApp"/>
    /// with proper scoping semantics instead.
    /// </remarks>
    internal Application<object?>? App { get; set; }

    /// <summary>
    /// Dispose of the session, restoring the previous session as current in the
    /// <see cref="AsyncLocal{T}"/> context. Disposal does NOT close or dispose
    /// the underlying <see cref="Input"/> or <see cref="Output"/> devices — those
    /// are shared resources managed by the caller. Disposal is idempotent:
    /// calling Dispose() multiple times has no additional effect.
    /// </summary>
    public void Dispose();
}
```

## AppContext

```csharp
/// <summary>
/// Static utilities for accessing the current application and session from
/// anywhere in the call stack using async-local storage.
/// </summary>
public static class AppContext
{
    /// <summary>
    /// Get the current active (running) Application.
    /// If no application is running, returns a <see cref="DummyApplication"/>.
    /// </summary>
    /// <returns>The running application or a DummyApplication.</returns>
    public static Application<object?> GetApp();

    /// <summary>
    /// Get the current active (running) Application, or null if none is running.
    /// </summary>
    /// <returns>The running application or null.</returns>
    public static Application<object?>? GetAppOrNull();

    /// <summary>
    /// Get the current AppSession.
    /// </summary>
    /// <returns>The current AppSession.</returns>
    public static AppSession GetAppSession();

    /// <summary>
    /// Set the given application as active in the current AppSession.
    /// Returns an IDisposable that restores the previous application on dispose.
    /// This should only be called by the Application itself.
    /// </summary>
    /// <param name="app">The application to set as active.</param>
    /// <returns>An IDisposable that restores the previous application.</returns>
    public static IDisposable SetApp(Application<object?> app);

    /// <summary>
    /// Create a separate AppSession. Useful for Telnet/SSH server scenarios
    /// where multiple sessions run concurrently.
    /// </summary>
    /// <param name="input">Input for the new session. Falls back to current session's input.</param>
    /// <param name="output">Output for the new session. Falls back to current session's output.</param>
    /// <returns>A new AppSession that is set as current. Dispose to restore previous.</returns>
    public static AppSession CreateAppSession(IInput? input = null, IOutput? output = null);

    /// <summary>
    /// Create an AppSession that prefers TTY input/output, even when stdin/stdout are piped.
    /// On Unix: opens <c>/dev/tty</c> for both input and output.
    /// On Windows: uses <c>CONIN$</c> and <c>CONOUT$</c> console handles.
    /// When no TTY is available (e.g., running in a Docker container without a PTY,
    /// or in a non-interactive CI environment), falls back to <see cref="DummyInput"/>
    /// and <see cref="DummyOutput"/>. No exception is thrown in this case.
    /// </summary>
    /// <returns>A new AppSession using TTY I/O (or dummy fallback). Dispose to restore previous.</returns>
    public static AppSession CreateAppSessionFromTty();
}
```

## RunInTerminal

```csharp
/// <summary>
/// Utilities for temporarily suspending the application UI to execute
/// code that outputs directly to the terminal.
/// </summary>
public static class RunInTerminal
{
    /// <summary>
    /// Run a synchronous function on the terminal above the current application.
    /// The application is hidden, the function executes, and the application redraws.
    /// </summary>
    /// <typeparam name="T">Return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="renderCliDone">Render in 'done' state before executing.</param>
    /// <param name="inExecutor">Run on a thread pool thread (for blocking functions).</param>
    /// <returns>The function's return value.</returns>
    public static Task<T> RunAsync<T>(
        Func<T> func,
        bool renderCliDone = false,
        bool inExecutor = false);

    /// <summary>
    /// Run a synchronous action on the terminal above the current application.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="renderCliDone">Render in 'done' state before executing.</param>
    /// <param name="inExecutor">Run on a thread pool thread.</param>
    public static Task RunAsync(
        Action action,
        bool renderCliDone = false,
        bool inExecutor = false);

    /// <summary>
    /// Async disposable context that suspends the current application and allows
    /// direct terminal I/O within the block.
    /// </summary>
    /// <param name="renderCliDone">Render in 'done' state before suspending.</param>
    /// <returns>An <see cref="IAsyncDisposable"/> that resumes the application on dispose.
    /// When disposed: re-attaches input, re-enters raw mode, and redraws the application.
    /// Must be disposed (via <c>await using</c>) to restore the application state.
    /// Failure to dispose will leave the application in a suspended state.</returns>
    public static IAsyncDisposable InTerminal(bool renderCliDone = false);
}
```

## DummyApplication

```csharp
/// <summary>
/// When no Application is running, <see cref="AppContext.GetApp"/> returns an
/// instance of this class. It uses DummyInput and DummyOutput and throws
/// NotImplementedException if someone tries to run it.
/// </summary>
public sealed class DummyApplication : Application<object?>
{
    public DummyApplication();

    /// <summary>Throws NotImplementedException.</summary>
    public new object? Run(...);

    /// <summary>Throws NotImplementedException.</summary>
    public new Task<object?> RunAsync(...);

    /// <summary>Throws NotImplementedException.</summary>
    public new Task RunSystemCommandAsync(...);

    /// <summary>Throws NotImplementedException.</summary>
    public new void SuspendToBackground(bool suspendGroup = true);
}
```
