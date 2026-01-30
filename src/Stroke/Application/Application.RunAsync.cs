using System.Threading.Channels;
using Stroke.Input.Typeahead;

using InputKeyPress = Stroke.Input.KeyPress;
using KBKeyPress = Stroke.KeyBinding.KeyPress;

namespace Stroke.Application;

/// <summary>
/// Application RunAsync and Run methods.
/// </summary>
public partial class Application<TResult>
{
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
    public async Task<TResult> RunAsync(
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true)
    {
        if (_isRunning)
            throw new InvalidOperationException("Application is already running.");

        _isRunning = true;
        _invalidated = 0;
        _backgroundTasksCts = new CancellationTokenSource();

        // Create the future for the result
        _future = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Set this application as the current application
        using var appScope = AppContext.SetApp(UnsafeCast);

        try
        {
            // Reset
            Reset();
            PreRun(preRun);

            // Feed typeahead input first
            var typeahead = TypeaheadBuffer.Get(Input);
            foreach (var inputKp in typeahead)
            {
                KeyProcessor.Feed(new KBKeyPress(inputKp.Key, inputKp.Data));
            }
            KeyProcessor.ProcessKeys();

            Task? flushTask = null;

            void ReadFromInput()
            {
                // Ignore when we aren't running anymore, except for CPR
                if (!_isRunning && !Renderer.WaitingForCpr)
                    return;

                // Get keys from the input object
                var keys = Input.ReadKeys();

                // Feed to key processor
                foreach (var inputKp in keys)
                {
                    KeyProcessor.Feed(new KBKeyPress(inputKp.Key, inputKp.Data));
                }
                KeyProcessor.ProcessKeys();

                // Quit when the input stream was closed
                if (Input.Closed)
                {
                    if (_future is not null && !_future.Task.IsCompleted)
                    {
                        _future.TrySetException(new EndOfStreamException("Input stream closed."));
                    }
                }
                else
                {
                    // Automatically flush keys after timeout
                    flushTask?.Dispose();
                    flushTask = CreateBackgroundTask(AutoFlushInputAsync);
                }
            }

            async Task AutoFlushInputAsync(CancellationToken ct)
            {
                await Task.Delay(TimeSpan.FromSeconds(TtimeoutLen), ct);
                FlushInput();
            }

            void FlushInput()
            {
                if (!IsDone)
                {
                    var keys = Input.FlushKeys();
                    foreach (var inputKp in keys)
                    {
                        KeyProcessor.Feed(new KBKeyPress(inputKp.Key, inputKp.Data));
                    }
                    KeyProcessor.ProcessKeys();

                    if (Input.Closed)
                    {
                        _future?.TrySetException(new EndOfStreamException("Input stream closed."));
                    }
                }
            }

            // Enter raw mode, attach input
            using var rawMode = Input.RawMode();
            using var inputAttach = Input.Attach(ReadFromInput);

            // Register SIGWINCH handler for terminal resize (Unix only).
            // Port of Python's attach_winch_signal_handler.
            using var sigwinchReg = RegisterSigwinch();

            // Register SIGINT handler if requested.
            // Port of Python's set_handle_sigint.
            using var sigintReg = handleSigint ? RegisterSigint() : null;

            // Create the redraw signaling channel.
            // ScheduleRedraw writes to this channel; the loop below reads from it.
            // Bounded(1) + DropWrite coalesces multiple signals into one redraw,
            // matching the Python Prompt Toolkit semantics where call_soon_threadsafe
            // posts to the event loop and duplicate invalidations are coalesced.
            var redrawChannel = Channel.CreateBounded<bool>(
                new BoundedChannelOptions(1)
                {
                    FullMode = BoundedChannelFullMode.DropWrite,
                    SingleReader = true,
                    SingleWriter = false,
                });
            _redrawChannel = redrawChannel;

            // Request cursor position and draw initial UI
            Renderer.RequestAbsoluteCursorPosition();
            Redraw();

            // Start auto-refresh task if configured
            if (RefreshInterval is > 0)
            {
                _ = CreateBackgroundTask(async ct =>
                {
                    while (!ct.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(RefreshInterval.Value), ct);
                        Invalidate();
                    }
                });
            }

            // Start terminal size polling
            if (TerminalSizePollingInterval is > 0)
            {
                _ = CreateBackgroundTask(async ct =>
                {
                    var lastSize = Output.GetSize();
                    while (!ct.IsCancellationRequested)
                    {
                        await Task.Delay(
                            TimeSpan.FromSeconds(TerminalSizePollingInterval.Value), ct);
                        var newSize = Output.GetSize();
                        if (newSize != lastSize)
                        {
                            lastSize = newSize;
                            Invalidate();
                        }
                    }
                });
            }

            // Main event loop: wait for either Exit (future) or redraw signals.
            // All Redraw() calls happen here, on the RunAsync async context,
            // ensuring thread-safety for the non-thread-safe Renderer.
            // This mirrors Python's event loop where _redraw runs on the main thread.
            TResult result;
            try
            {
                while (true)
                {
                    var redrawTask = redrawChannel.Reader.WaitToReadAsync().AsTask();
                    var completed = await Task.WhenAny(_future.Task, redrawTask);

                    if (completed == _future.Task)
                        break;

                    // Consume the signal (drain in case of spurious wake)
                    while (redrawChannel.Reader.TryRead(out _)) { }

                    Redraw();
                }

                result = await _future.Task;
            }
            finally
            {
                // Complete the redraw channel so any pending WaitToReadAsync
                // completes cleanly, and clear the field to prevent stale writes.
                redrawChannel.Writer.Complete();
                _redrawChannel = null;

                // In any case, when the application finishes
                try
                {
                    Redraw(renderAsDone: true);
                }
                finally
                {
                    // Reset the renderer anyway
                    Renderer.Reset();

                    // Unset is_running
                    _isRunning = false;

                    // Detach invalidation event handlers
                    _invalidateEvents = [];

                    // Wait for CPR responses
                    if (Output.RespondsToCpr)
                    {
                        await Renderer.WaitForCprResponsesAsync();
                    }

                    // Wait for run-in-terminal to terminate
                    if (_runningInTerminalFuture is not null)
                    {
                        await _runningInTerminalFuture.Task;
                    }

                    // Store unprocessed input as typeahead for next time
                    var unprocessedKeys = KeyProcessor.EmptyQueue();
                    var inputKeys = unprocessedKeys.Select(kbKp =>
                        new InputKeyPress(
                            kbKp.Key.IsKey ? kbKp.Key.Key : Stroke.Input.Keys.Any,
                            kbKp.Data)).ToList();
                    TypeaheadBuffer.Store(Input, inputKeys);
                }
            }

            return result;
        }
        finally
        {
            // Wait for background tasks
            await CancelAndWaitForBackgroundTasksAsync();

            _future = null;
            _isRunning = false;
        }
    }

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
        InputHook? inputHook = null)
    {
        if (inThread)
        {
            TResult? result = default;
            Exception? exception = null;

            var thread = new Thread(() =>
            {
                try
                {
                    result = Run(
                        preRun: preRun,
                        setExceptionHandler: setExceptionHandler,
                        handleSigint: false,
                        inputHook: inputHook);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });
            thread.Start();
            thread.Join();

            if (exception is not null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
            }

            return result!;
        }

        // Run synchronously
        var task = RunAsync(
            preRun: preRun,
            setExceptionHandler: setExceptionHandler,
            handleSigint: handleSigint);

        // Use GetAwaiter().GetResult() to unwrap AggregateException
        return task.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Execute pre-run callables, then clear the list.
    /// </summary>
    private void PreRun(Action? preRun)
    {
        if (preRun is not null)
        {
            preRun();
        }

        var callables = new List<Action>(PreRunCallables);
        PreRunCallables.Clear();

        foreach (var callable in callables)
        {
            callable();
        }
    }

    /// <summary>
    /// Render the UI. Not thread safe — must be called from the application's async context.
    /// From other threads, use <see cref="Invalidate"/> instead.
    /// </summary>
    /// <param name="renderAsDone">When true, put the cursor after the UI and optionally erase.</param>
    private void Redraw(bool renderAsDone = false)
    {
        // Only draw when no sub application was started (RunInTerminal).
        if (!_isRunning || _runningInTerminal)
            return;

        // Clear the invalidated flag
        Interlocked.Exchange(ref _invalidated, 0);

        // Update last redraw time if throttling is configured
        if (MinRedrawInterval is not null)
        {
            _lastRedrawTime = GetCurrentTime();
        }

        // Increment render counter
        Interlocked.Increment(ref _renderCounter);

        // Fire before render
        BeforeRender.Fire();

        if (renderAsDone)
        {
            if (EraseWhenDone)
            {
                Renderer.Erase();
            }
            else
            {
                // Draw in 'done' state and reset renderer.
                Renderer.Render(UnsafeCast, Layout, isDone: true);
            }
        }
        else
        {
            Renderer.Render(UnsafeCast, Layout);
        }

        // Update parent relations
        Layout.UpdateParentsRelations();

        // Fire after render
        AfterRender.Fire();

        // Update invalidation event subscriptions from controls
        UpdateInvalidateEvents();
    }

    /// <summary>
    /// Thread-safe way to schedule a UI repaint. Safe to call from any thread.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multiple rapid calls to <see cref="Invalidate"/> are coalesced into a single redraw.
    /// When <see cref="MinRedrawInterval"/> is set, redraws are throttled to at most one per interval.
    /// <see cref="MaxRenderPostponeTime"/> limits how long a redraw can be postponed under load.
    /// </para>
    /// <para>
    /// Port of Python Prompt Toolkit's <c>Application.invalidate</c>.
    /// </para>
    /// </remarks>
    public void Invalidate()
    {
        if (!_isRunning)
            return;

        // Coalesce multiple invalidations: never schedule a second redraw
        // when a previous one has not yet been executed.
        if (Interlocked.CompareExchange(ref _invalidated, 1, 0) != 0)
            return;

        // Fire the on_invalidate event
        OnInvalidate.Fire();

        // Schedule redraw, respecting MinRedrawInterval throttling
        if (MinRedrawInterval is > 0)
        {
            double diff = GetCurrentTime() - _lastRedrawTime;
            if (diff < MinRedrawInterval.Value)
            {
                // Too soon — schedule a deferred redraw
                _ = CreateBackgroundTask(async ct =>
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(MinRedrawInterval.Value - diff), ct);
                    ScheduleRedraw();
                });
            }
            else
            {
                ScheduleRedraw();
            }
        }
        else
        {
            ScheduleRedraw();
        }
    }

    /// <summary>
    /// Signal the RunAsync event loop to perform a redraw.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Thread-safe. This writes to a bounded channel that the RunAsync loop reads from.
    /// The channel capacity is 1 with DropWrite, so multiple rapid signals are coalesced
    /// into a single redraw — matching Python Prompt Toolkit's <c>call_soon_threadsafe</c>
    /// semantics where the redraw callback is posted to the event loop thread.
    /// </para>
    /// <para>
    /// This ensures Redraw() always executes on the RunAsync async context, preserving
    /// thread-safety for the non-thread-safe Renderer.
    /// </para>
    /// </remarks>
    private void ScheduleRedraw()
    {
        _redrawChannel?.Writer.TryWrite(true);
    }

    /// <summary>
    /// Attach 'invalidate' handlers to all invalidate events in the UI.
    /// Called after each render to ensure all controls can trigger redraws.
    /// </summary>
    private void UpdateInvalidateEvents()
    {
        // Remove previous event handlers
        foreach (var ev in _invalidateEvents)
        {
            ev.RemoveHandler(InvalidateHandler);
        }

        // Gather new events from all controls
        var newEvents = new List<Core.Event<object>>();
        foreach (var control in Layout.FindAllControls())
        {
            foreach (var ev in control.GetInvalidateEvents())
            {
                newEvents.Add(ev);
            }
        }

        // Add invalidate handler to new events
        foreach (var ev in newEvents)
        {
            ev.AddHandler(InvalidateHandler);
        }

        _invalidateEvents = newEvents;
    }

    /// <summary>
    /// The handler that calls Invalidate() when a control's invalidate event fires.
    /// </summary>
    private void InvalidateHandler(object _)
    {
        Invalidate();
    }

    /// <summary>
    /// Start a background task that will be cancelled when the application exits.
    /// </summary>
    /// <param name="taskFactory">Factory that creates the task, given a cancellation token.</param>
    /// <returns>The running task. Returns a completed task if the application is not running.</returns>
    public Task CreateBackgroundTask(Func<CancellationToken, Task> taskFactory)
    {
        ArgumentNullException.ThrowIfNull(taskFactory);

        if (_backgroundTasksCts is null || _backgroundTasksCts.IsCancellationRequested)
            return Task.CompletedTask;

        var token = _backgroundTasksCts.Token;
        var task = taskFactory(token);

        using (_lock.EnterScope())
        {
            _backgroundTasks.Add(task);
        }

        _ = task.ContinueWith(t =>
        {
            using (_lock.EnterScope())
            {
                _backgroundTasks.Remove(t);
            }
        }, TaskContinuationOptions.ExecuteSynchronously);

        return task;
    }

    /// <summary>
    /// Cancel all background tasks and wait for cancellation to complete.
    /// </summary>
    public async Task CancelAndWaitForBackgroundTasksAsync()
    {
        _backgroundTasksCts?.Cancel();

        List<Task> tasks;
        using (_lock.EnterScope())
        {
            tasks = [.. _backgroundTasks];
        }

        if (tasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling background tasks
            }
            catch
            {
                // Swallow exceptions from background tasks during shutdown
            }
        }

        _backgroundTasksCts?.Dispose();
        _backgroundTasksCts = null;
    }

    /// <summary>Gets the current time in seconds (unix timestamp).</summary>
    private static double GetCurrentTime()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
    }

    /// <summary>
    /// Register a SIGWINCH handler that triggers resize and redraw.
    /// Port of Python Prompt Toolkit's <c>attach_winch_signal_handler</c>.
    /// </summary>
    /// <returns>A disposable that unregisters the handler, or null on Windows.</returns>
    private IDisposable? RegisterSigwinch()
    {
        if (OperatingSystem.IsWindows())
            return null;

        try
        {
            return System.Runtime.InteropServices.PosixSignalRegistration.Create(
                System.Runtime.InteropServices.PosixSignal.SIGWINCH,
                _ => OnResize());
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    /// <summary>
    /// Register a SIGINT handler that sends sigint to the key processor.
    /// Port of Python Prompt Toolkit's <c>set_handle_sigint</c>.
    /// </summary>
    /// <returns>A disposable that unregisters the handler, or null on unsupported platforms.</returns>
    private IDisposable? RegisterSigint()
    {
        try
        {
            return System.Runtime.InteropServices.PosixSignalRegistration.Create(
                System.Runtime.InteropServices.PosixSignal.SIGINT,
                ctx =>
                {
                    ctx.Cancel = true; // Prevent default termination
                    KeyProcessor.SendSigint();
                });
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    /// <summary>
    /// Handle terminal resize: erase, request cursor position, and schedule redraw.
    /// Port of Python Prompt Toolkit's <c>Application._on_resize</c>.
    /// </summary>
    /// <remarks>
    /// This method is called from a SIGWINCH signal handler thread. The Erase and
    /// RequestAbsoluteCursorPosition calls write escape sequences to the output
    /// (simple I/O). The actual redraw is scheduled via Invalidate() to run on the
    /// RunAsync async context, preserving Renderer thread-safety.
    /// </remarks>
    private void OnResize()
    {
        Renderer.Erase(leaveAlternateScreen: false);
        Renderer.RequestAbsoluteCursorPosition();
        Invalidate();
    }
}
