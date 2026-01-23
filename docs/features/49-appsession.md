# Feature 49: App Session

## Overview

Implement the AppSession context management system for tracking the current application, input, and output within interactive terminal sessions. This provides global access to the running application via context variables.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/application/current.py`

## Public API

### AppSession Class

```csharp
namespace Stroke.Application;

/// <summary>
/// An AppSession is an interactive session, usually connected to one terminal.
/// Within one such session, interaction with many applications can happen, one
/// after the other. The input/output device is not supposed to change during
/// one session.
///
/// Warning: Always use the CreateAppSession function to create an instance,
/// so that it gets activated correctly.
/// </summary>
public sealed class AppSession
{
    /// <summary>
    /// Creates an AppSession.
    /// </summary>
    /// <param name="input">Default input for applications in this session.</param>
    /// <param name="output">Default output for applications in this session.</param>
    public AppSession(Input? input = null, Output? output = null);

    /// <summary>
    /// The current application (set dynamically by SetApp).
    /// </summary>
    public Application? App { get; internal set; }

    /// <summary>
    /// The input interface. Created lazily if not provided.
    /// </summary>
    public Input Input { get; }

    /// <summary>
    /// The output interface. Created lazily if not provided.
    /// </summary>
    public Output Output { get; }
}
```

### Static Functions

```csharp
namespace Stroke.Application;

/// <summary>
/// Functions for managing the current application session.
/// </summary>
public static class AppContext
{
    /// <summary>
    /// Get the current AppSession.
    /// </summary>
    public static AppSession GetAppSession();

    /// <summary>
    /// Get the current active (running) Application.
    /// If no Application is running, returns a DummyApplication.
    /// </summary>
    public static Application GetApp();

    /// <summary>
    /// Get the current active Application, or null if none running.
    /// </summary>
    public static Application? GetAppOrNone();

    /// <summary>
    /// Context manager that sets the given Application active in an AppSession.
    /// This should only be called by the Application itself.
    /// </summary>
    /// <param name="app">The application to set as active.</param>
    /// <returns>Disposable that resets the app when disposed.</returns>
    public static IDisposable SetApp(Application app);

    /// <summary>
    /// Create a separate AppSession. Useful for Telnet/SSH servers
    /// where multiple independent sessions exist.
    /// </summary>
    /// <param name="input">Optional input override.</param>
    /// <param name="output">Optional output override.</param>
    /// <returns>Disposable scope with the new session active.</returns>
    public static AppSessionScope CreateAppSession(
        Input? input = null,
        Output? output = null);

    /// <summary>
    /// Create AppSession that always prefers the TTY input/output.
    /// Even if stdin/stdout are pipes, uses stderr's terminal.
    /// </summary>
    /// <returns>Disposable scope with the TTY session active.</returns>
    public static AppSessionScope CreateAppSessionFromTty();
}
```

### AppSessionScope Class

```csharp
namespace Stroke.Application;

/// <summary>
/// Scope for an app session, providing access to the session
/// and automatic cleanup when disposed.
/// </summary>
public sealed class AppSessionScope : IDisposable
{
    /// <summary>
    /// The app session for this scope.
    /// </summary>
    public AppSession Session { get; }

    /// <summary>
    /// Disposes the scope and resets to the previous session.
    /// </summary>
    public void Dispose();
}
```

## Project Structure

```
src/Stroke/
└── Application/
    ├── AppSession.cs
    ├── AppContext.cs
    ├── AppSessionScope.cs
    └── DummyApplication.cs
tests/Stroke.Tests/
└── Application/
    ├── AppSessionTests.cs
    ├── AppContextTests.cs
    └── CreateAppSessionTests.cs
```

## Implementation Notes

### AsyncLocal-Based Context

Use `AsyncLocal<T>` for .NET's equivalent of Python's `ContextVar`:

```csharp
private static readonly AsyncLocal<AppSession> _currentAppSession = new();

static AppContext()
{
    // Initialize with default session
    _currentAppSession.Value = new AppSession();
}

public static AppSession GetAppSession()
{
    return _currentAppSession.Value ?? new AppSession();
}
```

### Lazy Input/Output Creation

```csharp
private Input? _input;
private Output? _output;

public Input Input
{
    get
    {
        if (_input is null)
            _input = InputFactory.CreateInput();
        return _input;
    }
}

public Output Output
{
    get
    {
        if (_output is null)
            _output = OutputFactory.CreateOutput();
        return _output;
    }
}
```

### SetApp Context Manager

```csharp
public static IDisposable SetApp(Application app)
{
    var session = GetAppSession();
    var previousApp = session.App;
    session.App = app;

    return Disposable.Create(() => session.App = previousApp);
}
```

### CreateAppSession Implementation

```csharp
public static AppSessionScope CreateAppSession(
    Input? input = null,
    Output? output = null)
{
    // Fall back to current session's input/output
    var currentSession = GetAppSession();
    input ??= currentSession._input;  // Use private field, not property
    output ??= currentSession._output;

    var session = new AppSession(input, output);
    var previousSession = _currentAppSession.Value;
    _currentAppSession.Value = session;

    return new AppSessionScope(session, () =>
    {
        _currentAppSession.Value = previousSession;
    });
}
```

### DummyApplication

When no application is running, `GetApp()` returns a `DummyApplication`:

```csharp
public sealed class DummyApplication : Application
{
    public DummyApplication() : base(/* minimal setup */)
    {
    }
}
```

This avoids null checks throughout the codebase.

### TTY Session Creation

```csharp
public static AppSessionScope CreateAppSessionFromTty()
{
    var input = InputFactory.CreateInput(alwaysPreferTty: true);
    var output = OutputFactory.CreateOutput(alwaysPreferTty: true);

    return CreateAppSession(input, output);
}
```

### Thread Safety

- `AsyncLocal<T>` provides correct behavior across threads and async contexts
- Each async context gets its own session value
- Use `ExecutionContext.Capture()` and `Run()` for propagation if needed

### Use Cases

1. **Single Application**: Default session, one app at a time
2. **Telnet/SSH Server**: Multiple sessions, each with own input/output
3. **Piped Input**: `CreateAppSessionFromTty()` for terminal interaction despite pipes
4. **Nested Applications**: Multiple apps using `SetApp` scopes

## Dependencies

- `Stroke.Input.Abstractions` (Feature 04) - Input interface
- `Stroke.Rendering.Output` (Feature 08) - Output interface
- `Stroke.Application` (Feature 31) - Application class

## Implementation Tasks

1. Implement `AppSession` class with lazy I/O
2. Implement `AppContext` static class
3. Implement `AppSessionScope` disposable
4. Implement `GetAppSession()` function
5. Implement `GetApp()` and `GetAppOrNone()` functions
6. Implement `SetApp()` context manager
7. Implement `CreateAppSession()` function
8. Implement `CreateAppSessionFromTty()` function
9. Implement `DummyApplication` class
10. Write comprehensive unit tests

## Acceptance Criteria

- [ ] AppSession stores input/output/app correctly
- [ ] Input/Output created lazily when accessed
- [ ] GetAppSession() returns current session
- [ ] GetApp() returns app or DummyApplication
- [ ] GetAppOrNone() returns app or null
- [ ] SetApp() activates app in session
- [ ] CreateAppSession() creates isolated session
- [ ] CreateAppSessionFromTty() uses TTY devices
- [ ] AsyncLocal provides correct async context
- [ ] Unit tests achieve 80% coverage
