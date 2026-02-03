using Stroke.Input;
using Stroke.Output;

namespace Stroke.Application;

/// <summary>
/// Static utilities for accessing the current application and session from
/// anywhere in the call stack using async-local storage.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's context management from
/// <c>prompt_toolkit.application.current</c>.
/// </para>
/// <para>
/// Uses <see cref="AsyncLocal{T}"/> for the session context, which flows
/// across async/await boundaries.
/// </para>
/// </remarks>
public static class AppContext
{
    private static readonly AsyncLocal<AppSession> _currentAppSession = new();

    /// <summary>
    /// Get the current AppSession. If none has been set, creates a default one.
    /// </summary>
    /// <returns>The current AppSession.</returns>
    public static AppSession GetAppSession()
    {
        var session = _currentAppSession.Value;
        if (session is null)
        {
            session = new AppSession();
            _currentAppSession.Value = session;
        }
        return session;
    }

    /// <summary>
    /// Get the current active (running) Application.
    /// If no application is running, returns a <see cref="DummyApplication"/>.
    /// </summary>
    /// <returns>The running application or a DummyApplication.</returns>
    public static Application<object?> GetApp()
    {
        var session = GetAppSession();
        var app = session.App;

        // Check if session.App is any Application<T> (generic invariance means
        // Application<string> is NOT Application<object?>, so we can't use pattern matching).
        // Instead, check the generic type definition and use Unsafe.As.
        if (app is not null)
        {
            var appType = app.GetType();
            if (appType.IsGenericType && appType.GetGenericTypeDefinition() == typeof(Application<>))
            {
                return System.Runtime.CompilerServices.Unsafe.As<Application<object?>>(app);
            }
        }

        return new DummyApplication();
    }

    /// <summary>
    /// Get the current active (running) Application, or null if none is running.
    /// </summary>
    /// <returns>The running application or null.</returns>
    public static Application<object?>? GetAppOrNull()
    {
        var session = GetAppSession();
        var app = session.App;

        // Check if session.App is any Application<T> (generic invariance issue).
        if (app is not null)
        {
            var appType = app.GetType();
            if (appType.IsGenericType && appType.GetGenericTypeDefinition() == typeof(Application<>))
            {
                return System.Runtime.CompilerServices.Unsafe.As<Application<object?>>(app);
            }
        }

        return null;
    }

    /// <summary>
    /// Set the given application as active in the current AppSession.
    /// Returns an IDisposable that restores the previous application on dispose.
    /// This should only be called by the Application itself.
    /// </summary>
    /// <param name="app">The application to set as active.</param>
    /// <returns>An IDisposable that restores the previous application.</returns>
    public static IDisposable SetApp(Application<object?> app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var session = GetAppSession();
        var previousApp = session.App;
        session.App = app;

        return new AppScopeDisposable(session, previousApp);
    }

    /// <summary>
    /// Create a separate AppSession. Useful for Telnet/SSH server scenarios
    /// where multiple sessions run concurrently.
    /// </summary>
    /// <param name="input">Input for the new session. Falls back to current session's explicit input.</param>
    /// <param name="output">Output for the new session. Falls back to current session's explicit output.</param>
    /// <returns>A new AppSession that is set as current. Dispose to restore previous.</returns>
    public static AppSession CreateAppSession(IInput? input = null, IOutput? output = null)
    {
        // Fall back to current session's explicit input/output (avoiding lazy creation)
        var currentSession = GetAppSession();
        input ??= currentSession.ExplicitInput;
        output ??= currentSession.ExplicitOutput;

        var session = new AppSession(input, output);
        session.PreviousSession = _currentAppSession.Value;
        _currentAppSession.Value = session;

        return session;
    }

    /// <summary>
    /// Create an AppSession that prefers TTY input/output, even when stdin/stdout are piped.
    /// When no TTY is available, falls back to DummyInput/DummyOutput.
    /// </summary>
    /// <returns>A new AppSession using TTY I/O (or dummy fallback). Dispose to restore previous.</returns>
    public static AppSession CreateAppSessionFromTty()
    {
        IInput input;
        IOutput output;

        try
        {
            input = InputFactory.Create(alwaysPreferTty: true);
        }
        catch
        {
            input = new DummyInput();
        }

        try
        {
            output = OutputFactory.Create(alwaysPreferTty: true);
        }
        catch
        {
            output = new DummyOutput();
        }

        return CreateAppSession(input, output);
    }

    /// <summary>
    /// Internal: Restore previous session when an AppSession is disposed.
    /// </summary>
    internal static void RestorePreviousSession(AppSession disposedSession)
    {
        // Only restore if this session is currently active
        if (_currentAppSession.Value == disposedSession)
        {
            _currentAppSession.Value = disposedSession.PreviousSession!;
            disposedSession.PreviousSession = null;
        }
    }

    /// <summary>
    /// Disposable scope that restores the previous application when disposed.
    /// </summary>
    private sealed class AppScopeDisposable : IDisposable
    {
        private readonly AppSession _session;
        private readonly object? _previousApp;
        private bool _disposed;

        internal AppScopeDisposable(AppSession session, object? previousApp)
        {
            _session = session;
            _previousApp = previousApp;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _session.App = _previousApp;
        }
    }
}
