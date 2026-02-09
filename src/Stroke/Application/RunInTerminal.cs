namespace Stroke.Application;

/// <summary>
/// Utilities for temporarily suspending the application UI to execute
/// code that outputs directly to the terminal.
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.application.run_in_terminal</c>.
/// </summary>
/// <remarks>
/// This class is thread-safe. All methods coordinate with the current
/// <see cref="Application{TResult}"/> via <see cref="AppContext"/>.
/// </remarks>
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
    /// <returns>A task containing the function's return value.</returns>
    public static Task<T> RunAsync<T>(
        Func<T> func,
        bool renderCliDone = false,
        bool inExecutor = false)
    {
        ArgumentNullException.ThrowIfNull(func);

        return RunCoreAsync(async () =>
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
        });
    }

    /// <summary>
    /// Run a synchronous action on the terminal above the current application.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="renderCliDone">Render in 'done' state before executing.</param>
    /// <param name="inExecutor">Run on a thread pool thread.</param>
    public static Task RunAsync(
        Action action,
        bool renderCliDone = false,
        bool inExecutor = false)
    {
        ArgumentNullException.ThrowIfNull(action);

        return RunCoreAsync(async () =>
        {
            await using (InTerminal(renderCliDone))
            {
                if (inExecutor)
                {
                    await Task.Run(action);
                }
                else
                {
                    action();
                }
            }
        });
    }

    /// <summary>
    /// Async disposable context that suspends the current application and allows
    /// direct terminal I/O within the block.
    /// </summary>
    /// <param name="renderCliDone">Render in 'done' state before suspending.</param>
    /// <returns>An <see cref="IAsyncDisposable"/> that resumes the application on dispose.
    /// When disposed: re-attaches input, re-enters raw mode, and redraws the application.
    /// Must be disposed (via <c>await using</c>) to restore the application state.
    /// Failure to dispose will leave the application in a suspended state.</returns>
    public static IAsyncDisposable InTerminal(bool renderCliDone = false)
    {
        return new InTerminalContext(renderCliDone);
    }

    private static async Task<T> RunCoreAsync<T>(Func<Task<T>> func)
    {
        return await func();
    }

    private static async Task RunCoreAsync(Func<Task> func)
    {
        await func();
    }

    /// <summary>
    /// Async disposable that manages the suspend/resume lifecycle of the application.
    /// Corresponds to Python's <c>in_terminal</c> async context manager.
    /// </summary>
    private sealed class InTerminalContext : IAsyncDisposable
    {
        private readonly bool _renderCliDone;
        private IApplication? _app;
        private TaskCompletionSource<object?>? _newFuture;
        private IDisposable? _cookedMode;
        private IDisposable? _detach;
        private bool _initialized;

        public InTerminalContext(bool renderCliDone)
        {
            _renderCliDone = renderCliDone;

            // Eagerly initialize — mirrors the Python context manager's __aenter__
            Initialize();
        }

        private void Initialize()
        {
            _app = AppContext.GetAppOrNull();
            if (_app is null || !_app.IsRunning)
            {
                _app = null;
                _initialized = false;
                return;
            }

            // Chain to previous run_in_terminal call.
            var previousFuture = _app.RunningInTerminalFuture;
            _newFuture = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            _app.RunningInTerminalFuture = _newFuture;

            // Wait for the previous run_in_terminal to finish (synchronously — we're in a hot path).
            if (previousFuture is not null)
            {
                previousFuture.Task.GetAwaiter().GetResult();
            }

            // Wait for all CPR responses to arrive.
            if (_app.Output.RespondsToCpr)
            {
                _app.Renderer.WaitForCprResponsesAsync().GetAwaiter().GetResult();
            }

            // Draw interface in 'done' state, or erase.
            if (_renderCliDone)
            {
                // Use the Redraw method with renderAsDone
                if (_app.EraseWhenDone)
                {
                    _app.Renderer.Erase();
                }
                else
                {
                    _app.Renderer.Render(_app, _app.Layout, isDone: true);
                }
            }
            else
            {
                _app.Renderer.Erase();
            }

            // Disable rendering.
            _app.RunningInTerminal = true;

            // Detach input and enter cooked mode.
            _detach = _app.Input.Detach();
            _cookedMode = _app.Input.CookedMode();

            _initialized = true;
        }

        public ValueTask DisposeAsync()
        {
            if (!_initialized || _app is null)
            {
                return ValueTask.CompletedTask;
            }

            try
            {
                // Dispose cooked mode and detach (reverse order).
                _cookedMode?.Dispose();
                _detach?.Dispose();

                // Re-enable rendering.
                _app.RunningInTerminal = false;
                _app.Renderer.Reset();
                _app.Renderer.RequestAbsoluteCursorPosition();
                _app.Invalidate();
            }
            finally
            {
                // Signal completion of this run_in_terminal call.
                _newFuture?.TrySetResult(null);
            }

            return ValueTask.CompletedTask;
        }
    }
}
