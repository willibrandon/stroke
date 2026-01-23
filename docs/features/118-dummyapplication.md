# Feature 118: Dummy Application

## Overview

Implement DummyApplication - a placeholder Application instance used when no real Application is running. This is returned by `GetApp()` when called outside of a running application context.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/application/dummy.py`

## Public API

### DummyApplication

```csharp
namespace Stroke.Application;

/// <summary>
/// A placeholder Application used when no real Application is running.
/// Returned by GetApp() when called outside of a running application context.
/// </summary>
/// <remarks>
/// DummyApplication throws NotImplementedException for all operations
/// that require a running application. It is used to prevent null
/// reference exceptions while still making it clear that operations
/// requiring an application context are not available.
/// </remarks>
public sealed class DummyApplication : Application<object?>
{
    /// <summary>
    /// Create a dummy application instance.
    /// </summary>
    public DummyApplication();

    /// <summary>
    /// Not supported on DummyApplication.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public override object? Run(
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true,
        bool inThread = false,
        InputHook? inputHook = null);

    /// <summary>
    /// Not supported on DummyApplication.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public override Task<object?> RunAsync(
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true,
        double slowCallbackDuration = 0.5);

    /// <summary>
    /// Not supported on DummyApplication.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public override Task RunSystemCommandAsync(
        string command,
        bool waitForEnter = true,
        AnyFormattedText displayBeforeText = default,
        string waitText = "");

    /// <summary>
    /// Not supported on DummyApplication.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public override void SuspendToBackground(bool suspendGroup = true);
}
```

## Project Structure

```
src/Stroke/
└── Application/
    └── DummyApplication.cs
tests/Stroke.Tests/
└── Application/
    └── DummyApplicationTests.cs
```

## Implementation Notes

### DummyApplication Implementation

```csharp
namespace Stroke.Application;

public sealed class DummyApplication : Application<object?>
{
    private static readonly Lazy<DummyApplication> _instance =
        new(() => new DummyApplication());

    /// <summary>
    /// Singleton instance of DummyApplication.
    /// </summary>
    public static DummyApplication Instance => _instance.Value;

    public DummyApplication()
        : base(
            output: DummyOutput.Instance,
            input: DummyInput.Instance)
    {
    }

    public override object? Run(
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true,
        bool inThread = false,
        InputHook? inputHook = null)
    {
        throw new NotImplementedException(
            "A DummyApplication is not supposed to run.");
    }

    public override Task<object?> RunAsync(
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true,
        double slowCallbackDuration = 0.5)
    {
        throw new NotImplementedException(
            "A DummyApplication is not supposed to run.");
    }

    public override Task RunSystemCommandAsync(
        string command,
        bool waitForEnter = true,
        AnyFormattedText displayBeforeText = default,
        string waitText = "")
    {
        throw new NotImplementedException();
    }

    public override void SuspendToBackground(bool suspendGroup = true)
    {
        throw new NotImplementedException();
    }
}
```

### DummyOutput Implementation

```csharp
namespace Stroke.Output;

/// <summary>
/// Dummy output that does nothing.
/// </summary>
public sealed class DummyOutput : IOutput
{
    public static readonly DummyOutput Instance = new();

    private DummyOutput() { }

    public void Write(string data) { }
    public void WriteRaw(string data) { }
    public void SetTitle(string title) { }
    public void ClearTitle() { }
    public void Flush() { }
    public void EraseScreen() { }
    public void EnterAlternateScreen() { }
    public void QuitAlternateScreen() { }
    public void EnableMouseSupport() { }
    public void DisableMouseSupport() { }
    public void EnableBracketedPaste() { }
    public void DisableBracketedPaste() { }
    public void SetCursorShape(CursorShape shape) { }
    public void ResetCursorShape() { }
    public void HideCursor() { }
    public void ShowCursor() { }
    public void SetCursorPosition(int row, int column) { }
    public void CursorUp(int count) { }
    public void CursorDown(int count) { }
    public void CursorForward(int count) { }
    public void CursorBackward(int count) { }
    public void ScrollBufferUp(int count) { }
    public void ScrollBufferDown(int count) { }
    public void SetAttributes(Attrs attrs, Color? fg, Color? bg) { }
    public void ResetAttributes() { }
    public Size GetSize() => new(80, 24);
    public bool RespondsToCpr => false;
    public string Encoding => "utf-8";
    public ColorDepth GetDefaultColorDepth() => ColorDepth.Default;
    public void Bell() { }
}
```

### DummyInput Implementation

```csharp
namespace Stroke.Input;

/// <summary>
/// Dummy input that does nothing.
/// </summary>
public sealed class DummyInput : IInput
{
    public static readonly DummyInput Instance = new();

    private DummyInput() { }

    public IAsyncEnumerable<KeyPress> ReadAsync(
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<KeyPress>();
    }

    public IDisposable RawMode() => NullDisposable.Instance;
    public IDisposable CookedMode() => NullDisposable.Instance;
    public IDisposable Detach() => NullDisposable.Instance;
    public void Close() { }
    public IntPtr FileNo() => IntPtr.Zero;
    public bool Closed => true;
    public string TypeName => "DummyInput";
}
```

### Usage in GetApp

```csharp
namespace Stroke.Application;

public static class Current
{
    private static readonly AsyncLocal<Application?> _current = new();

    /// <summary>
    /// Get the current running application, or DummyApplication if none.
    /// </summary>
    public static Application GetApp()
    {
        return _current.Value ?? DummyApplication.Instance;
    }

    /// <summary>
    /// Get the current running application, or null if none.
    /// </summary>
    public static Application? GetAppOrNone()
    {
        return _current.Value;
    }

    /// <summary>
    /// Set the current application context.
    /// </summary>
    internal static void SetApp(Application? app)
    {
        _current.Value = app;
    }
}
```

### Usage Example

```csharp
// Outside of running application
var app = Current.GetApp();
// app is DummyApplication

// Check if running
if (Current.GetAppOrNone() == null)
{
    Console.WriteLine("No application is running");
}

// Inside running application
await new Application(layout: myLayout).RunAsync(async () =>
{
    var app = Current.GetApp();
    // app is the real Application instance
});
```

## Dependencies

- Feature 31: Application
- Feature 50: Input (DummyInput)
- Feature 51: Output (DummyOutput)

## Implementation Tasks

1. Implement DummyOutput class
2. Implement DummyInput class
3. Implement DummyApplication class
4. Override all methods to throw NotImplementedException
5. Integrate with GetApp/GetAppOrNone
6. Write unit tests

## Acceptance Criteria

- [ ] DummyApplication uses DummyInput/DummyOutput
- [ ] Run/RunAsync throw NotImplementedException
- [ ] RunSystemCommandAsync throws NotImplementedException
- [ ] SuspendToBackground throws NotImplementedException
- [ ] GetApp returns DummyApplication when no app running
- [ ] GetAppOrNone returns null when no app running
- [ ] Unit tests achieve 80% coverage
