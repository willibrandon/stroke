# Feature 119: Run in Terminal

## Overview

Implement utilities for running functions on the terminal above the current application, temporarily suspending the prompt to allow terminal output and then restoring the prompt.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/application/run_in_terminal.py`

## Public API

### run_in_terminal

```csharp
namespace Stroke.Application;

/// <summary>
/// Utilities for running functions in the terminal while an application is active.
/// </summary>
public static class RunInTerminal
{
    /// <summary>
    /// Run a function on the terminal above the current application or prompt.
    /// </summary>
    /// <typeparam name="T">Return type of the function.</typeparam>
    /// <param name="func">The synchronous function to execute.</param>
    /// <param name="renderCliDone">
    /// When true, render the interface in 'Done' state first, then execute.
    /// When false, erase the interface before executing.
    /// </param>
    /// <param name="inExecutor">
    /// When true, run in a thread pool executor. Use for long blocking functions
    /// to avoid blocking the event loop.
    /// </param>
    /// <returns>A task that completes with the function's return value.</returns>
    /// <remarks>
    /// This hides the prompt, runs the function (which can safely write to terminal),
    /// then re-renders the prompt, causing the function's output to scroll above it.
    ///
    /// For async operations, use InTerminal context manager directly.
    /// </remarks>
    /// <example>
    /// await RunInTerminal.RunAsync(() =>
    /// {
    ///     Console.WriteLine("This appears above the prompt!");
    ///     return 42;
    /// });
    /// </example>
    public static Task<T> RunAsync<T>(
        Func<T> func,
        bool renderCliDone = false,
        bool inExecutor = false);

    /// <summary>
    /// Run an action on the terminal above the current application.
    /// </summary>
    public static Task RunAsync(
        Action action,
        bool renderCliDone = false,
        bool inExecutor = false);
}
```

### in_terminal

```csharp
namespace Stroke.Application;

/// <summary>
/// Async context manager for running code in the terminal while suspending the application.
/// </summary>
public static class RunInTerminal
{
    /// <summary>
    /// Create an async disposable that suspends the application and allows terminal output.
    /// </summary>
    /// <param name="renderCliDone">
    /// When true, render the interface in 'Done' state first.
    /// When false, erase the interface.
    /// </param>
    /// <returns>Async disposable for the terminal session.</returns>
    /// <example>
    /// await using (RunInTerminal.InTerminal())
    /// {
    ///     Console.WriteLine("Output above prompt");
    ///     await SomeAsyncOperation();
    ///     Console.WriteLine("More output");
    /// }
    /// // Prompt is restored here
    /// </example>
    public static IAsyncDisposable InTerminal(bool renderCliDone = false);
}
```

## Project Structure

```
src/Stroke/
└── Application/
    └── RunInTerminal.cs
tests/Stroke.Tests/
└── Application/
    └── RunInTerminalTests.cs
```

## Implementation Notes

### InTerminal Implementation

```csharp
public static class RunInTerminal
{
    public static async Task<T> RunAsync<T>(
        Func<T> func,
        bool renderCliDone = false,
        bool inExecutor = false)
    {
        await using (InTerminal(renderCliDone))
        {
            if (inExecutor)
            {
                return await Task.Run(func);
            }
            else
            {
                return func();
            }
        }
    }

    public static async Task RunAsync(
        Action action,
        bool renderCliDone = false,
        bool inExecutor = false)
    {
        await RunAsync(() => { action(); return 0; }, renderCliDone, inExecutor);
    }

    public static IAsyncDisposable InTerminal(bool renderCliDone = false)
    {
        return new InTerminalContext(renderCliDone);
    }

    private sealed class InTerminalContext : IAsyncDisposable
    {
        private readonly bool _renderCliDone;
        private Application? _app;
        private TaskCompletionSource? _previousRunInTerminalTcs;
        private readonly TaskCompletionSource _newRunInTerminalTcs = new();

        public InTerminalContext(bool renderCliDone)
        {
            _renderCliDone = renderCliDone;
        }

        public async Task EnterAsync()
        {
            _app = Current.GetAppOrNone();
            if (_app == null || !_app.IsRunning)
            {
                return;
            }

            // Chain to previous run_in_terminal if any
            _previousRunInTerminalTcs = _app.RunningInTerminalTcs;
            _app.RunningInTerminalTcs = _newRunInTerminalTcs;

            // Wait for previous to complete
            if (_previousRunInTerminalTcs != null)
            {
                await _previousRunInTerminalTcs.Task;
            }

            // Wait for all CPR responses
            if (_app.Output.RespondsToCpr)
            {
                await _app.Renderer.WaitForCprResponsesAsync();
            }

            // Draw in done state or erase
            if (_renderCliDone)
            {
                _app.Redraw(renderAsDone: true);
            }
            else
            {
                _app.Renderer.Erase();
            }

            // Disable rendering
            _app.RunningInTerminal = true;

            // Detach input
            _app.Input.Detach();
            _app.Input.CookedMode();
        }

        public async ValueTask DisposeAsync()
        {
            if (_app == null || !_app.IsRunning)
            {
                return;
            }

            try
            {
                // Restore input
                _app.Input.RawMode();
                _app.Input.Attach();

                // Restore rendering
                _app.RunningInTerminal = false;
                _app.Renderer.Reset();
                _app.RequestAbsoluteCursorPosition();
                _app.Redraw();
            }
            finally
            {
                // Signal completion
                if (!_newRunInTerminalTcs.Task.IsCompleted)
                {
                    _newRunInTerminalTcs.SetResult();
                }
            }
        }
    }
}
```

### Usage Example: Simple Output

```csharp
var session = new PromptSession();

while (true)
{
    var command = await session.PromptAsync("> ");

    if (command == "info")
    {
        // Print information above the prompt
        await RunInTerminal.RunAsync(() =>
        {
            Console.WriteLine("═══════════════════════════");
            Console.WriteLine("Application Information");
            Console.WriteLine("═══════════════════════════");
            Console.WriteLine($"Version: 1.0.0");
            Console.WriteLine($"Time: {DateTime.Now}");
            Console.WriteLine();
        });
    }
    else if (command == "exit")
    {
        break;
    }
}
```

### Usage Example: Async Operations

```csharp
// For async operations, use InTerminal directly
await using (RunInTerminal.InTerminal())
{
    Console.WriteLine("Fetching data...");
    var data = await httpClient.GetStringAsync("https://api.example.com/data");
    Console.WriteLine($"Received: {data}");
}
// Prompt is restored
```

### Usage Example: Shell Command

```csharp
// Run a shell command with output shown above prompt
await RunInTerminal.RunAsync(async () =>
{
    var process = Process.Start(new ProcessStartInfo
    {
        FileName = "ls",
        Arguments = "-la",
        RedirectStandardOutput = false,
        UseShellExecute = false
    });
    await process!.WaitForExitAsync();
}, inExecutor: true);
```

## Dependencies

- Feature 31: Application
- Feature 23: Renderer
- Feature 50: Input

## Implementation Tasks

1. Implement InTerminalContext class
2. Handle chaining of multiple run_in_terminal calls
3. Wait for CPR responses before detaching
4. Handle renderCliDone mode
5. Implement proper cleanup on dispose
6. Implement RunAsync helper methods
7. Write unit tests

## Acceptance Criteria

- [ ] RunAsync hides prompt during execution
- [ ] Output appears above prompt after restoration
- [ ] renderCliDone shows "done" state before running
- [ ] Multiple concurrent calls are properly serialized
- [ ] CPR responses are awaited before detaching input
- [ ] Input is properly detached and reattached
- [ ] Renderer state is properly reset
- [ ] Async operations work with InTerminal
- [ ] Unit tests achieve 80% coverage
