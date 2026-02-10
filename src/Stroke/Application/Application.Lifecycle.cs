using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Styles;

namespace Stroke.Application;

/// <summary>
/// Application lifecycle methods: Exit, Reset, RunSystemCommand, SuspendToBackground, PrintText.
/// </summary>
public partial class Application<TResult>
{
    /// <summary>
    /// Exit the application with a result or exception.
    /// </summary>
    /// <param name="result">Result value to return from RunAsync.</param>
    /// <param name="exception">Exception to throw from RunAsync.</param>
    /// <param name="style">Style to apply to content on exit. Stored in <see cref="ExitStyle"/>.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the application is not running (message: "Application is not running") —
    /// i.e., <see cref="RunAsync"/> has not been called or has already completed,
    /// or when the result has already been set (message: "Result has already been set.").
    /// </exception>
    public void Exit(
        TResult? result = default,
        Exception? exception = null,
        string style = "")
    {
        // Check IsCompleted FIRST: during RunAsync cleanup, _isRunning may already
        // be false while _future is still alive (see RunAsync's outer finally block).
        // Checking _isRunning first would yield the wrong diagnostic ("not running"
        // instead of "already set") in that race window.
        if (_future?.Task.IsCompleted == true)
            throw new InvalidOperationException("Result has already been set.");

        if (!_isRunning || _future is null)
            throw new InvalidOperationException("Application is not running.");

        ExitStyle = style;

        if (exception is not null)
        {
            _future.TrySetException(exception);
        }
        else
        {
            _future.TrySetResult(result!);
        }
    }

    /// <summary>
    /// Reset the application to a clean state. Execution order:
    /// <list type="number">
    /// <item>Set <see cref="ExitStyle"/> to empty string</item>
    /// <item>Create new empty background tasks set</item>
    /// <item>Call Renderer.Reset() — clears cached screen, style cache, cursor position</item>
    /// <item>Call KeyProcessor.Reset() — clears key buffer, argument, input queue</item>
    /// <item>Call Layout.Reset() — resets all containers in the tree</item>
    /// <item>Call ViState.Reset() — resets input mode to Insert, clears registers</item>
    /// <item>Call EmacsState.Reset() — stops macro recording</item>
    /// <item>Fire OnReset event</item>
    /// <item>Ensure a focusable control has focus</item>
    /// </list>
    /// Does NOT clear buffer contents (preserves text between runs for REPL scenarios).
    /// Does NOT clear the focus stack (preserves focus history).
    /// </summary>
    public void Reset()
    {
        // 1. Clear exit style
        ExitStyle = "";

        // 2. New empty background tasks set
        _backgroundTasks = [];

        // 3. Reset renderer
        Renderer.Reset();

        // 4. Reset key processor
        KeyProcessor.Reset();

        // 5. Reset layout
        Layout.Reset();

        // 6. Reset Vi state
        ViState.Reset();

        // 7. Reset Emacs state
        EmacsState.Reset();

        // 8. Fire OnReset event
        OnReset.Fire();

        // 9. Ensure a focusable control has focus
        var layout = Layout;
        if (!layout.CurrentControl.IsFocusable)
        {
            foreach (var w in layout.FindAllWindows())
            {
                if (w.Content.IsFocusable)
                {
                    layout.CurrentWindow = w;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Run a system command while the application is suspended.
    /// Port of Python Prompt Toolkit's <c>Application.run_system_command</c>.
    /// </summary>
    /// <param name="command">Shell command to execute.</param>
    /// <param name="waitForEnter">Wait for ENTER after command finishes.</param>
    /// <param name="displayBeforeText">Text to display before the command.</param>
    /// <param name="waitText">Prompt text while waiting for ENTER.</param>
    public async Task RunSystemCommandAsync(
        string command,
        bool waitForEnter = true,
        AnyFormattedText displayBeforeText = default,
        string waitText = "Press ENTER to continue...")
    {
        ArgumentNullException.ThrowIfNull(command);

        await using (RunInTerminal.InTerminal())
        {
            // Display before text if any
            if (!displayBeforeText.IsEmpty)
            {
                PrintText(displayBeforeText);
            }

            // Run the command.
            // Use ArgumentList (not Arguments) to avoid quoting issues when the
            // command contains double quotes. ArgumentList passes each argument
            // as a separate argv element, matching Python's Popen(shell=True)
            // behavior which calls execvp(['/bin/sh', '-c', command]).
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                UseShellExecute = false,
            };

            if (OperatingSystem.IsWindows())
            {
                startInfo.FileName = "cmd";
                startInfo.ArgumentList.Add("/c");
                startInfo.ArgumentList.Add(command);
            }
            else
            {
                startInfo.FileName = "/bin/sh";
                startInfo.ArgumentList.Add("-c");
                startInfo.ArgumentList.Add(command);
            }

            try
            {
                using var process = System.Diagnostics.Process.Start(startInfo);
                if (process is not null)
                {
                    await process.WaitForExitAsync();
                }
            }
            catch
            {
                // Swallow process execution errors
            }

            if (waitForEnter)
            {
                Output.Write($"\r\n{waitText}");
                Output.Flush();

                // Wait for Enter in cooked mode.
                // Python uses a sub-application (PromptSession) here to avoid
                // blocking the event loop and to share I/O. We use direct fd
                // reading instead: on POSIX, Vt100Input.ReadLineFromFd() calls
                // read() on the terminal fd, bypassing .NET's Console class which
                // manages its own termios state and conflicts with our CookedMode.
                // On Windows, the default IInput.ReadLineFromFd() falls through
                // to Console.ReadLine(), which is safe since Windows has no
                // termios conflict.
                Input.ReadLineFromFd();
            }
        }
    }

    /// <summary>
    /// Suspend the process to background (Unix only, via SIGTSTP).
    /// No-op on Windows.
    /// Port of Python Prompt Toolkit's <c>Application.suspend_to_background</c>.
    /// </summary>
    /// <param name="suspendGroup">When true, suspend the whole process group.</param>
    public void SuspendToBackground(bool suspendGroup = true)
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            return;

        // Use RunInTerminal to suspend UI, send SIGTSTP, then resume.
        // Python: run_in_terminal(run) where run() calls os.kill(0, SIGTSTP)
        _ = RunInTerminal.RunAsync(() =>
        {
            // pid=0 sends to the entire process group (default, matches Python behavior).
            // pid=Process.Id sends only to the current process.
            int pid = suspendGroup ? 0 : Environment.ProcessId;
            UnixSignals.Kill(pid, UnixSignals.SIGTSTP);
        });
    }

    /// <summary>
    /// Print formatted text to the output.
    /// </summary>
    /// <param name="text">Formatted text to print.</param>
    /// <param name="style">Style to use. Defaults to the application's merged style.</param>
    public void PrintText(AnyFormattedText text, IStyle? style = null)
    {
        style ??= MergedStyle;
        Rendering.RendererUtils.PrintFormattedText(Output, text, style);
    }

    /// <summary>
    /// If the current control is a <c>BufferControl</c> with a linked
    /// <c>SearchBufferControl</c>, set the search direction and focus the search control.
    /// </summary>
    /// <param name="direction">The search direction to set.</param>
    /// <remarks>
    /// This method exists so that <c>NamedCommands</c> (KeyBinding layer) can initiate
    /// search without importing <c>Stroke.Layout.Controls</c> types, preserving the
    /// layered architecture (Constitution III).
    /// </remarks>
    public void StartSearch(SearchDirection direction)
    {
        if (Layout.CurrentControl is BufferControl bc && bc.SearchBufferControl is { } sbc)
        {
            CurrentSearchState.Direction = direction;
            Layout.Focus(new Layout.FocusableElement(sbc));
        }
    }

    /// <summary>
    /// Return a sorted list of used style strings. Useful for debugging.
    /// </summary>
    public List<string> GetUsedStyleStrings()
    {
        var attrs = Renderer.AttrsForStyle;
        if (attrs is null)
            return [];

        var result = attrs.Keys.Where(k => !string.IsNullOrEmpty(k)).ToList();
        result.Sort();
        return result;
    }

    // ════════════════════════════════════════════════════════════════════════
    // IApplication — EXPLICIT IMPLEMENTATIONS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Explicit implementation of <see cref="IApplication.RunningInTerminal"/>.
    /// Routes to the internal field used by <see cref="RunInTerminal"/>.
    /// </summary>
    bool IApplication.RunningInTerminal
    {
        get => _runningInTerminal;
        set => _runningInTerminal = value;
    }

    /// <summary>
    /// Explicit implementation of <see cref="IApplication.RunningInTerminalFuture"/>.
    /// Routes to the internal field used by <see cref="RunInTerminal"/>.
    /// </summary>
    TaskCompletionSource<object?>? IApplication.RunningInTerminalFuture
    {
        get => _runningInTerminalFuture;
        set => _runningInTerminalFuture = value;
    }

    /// <summary>
    /// Explicit non-generic <see cref="IApplication.Exit"/> implementation.
    /// Converts the boxed <paramref name="result"/> to <typeparamref name="TResult"/>
    /// and delegates to the typed <see cref="Exit(TResult?, Exception?, string)"/> method.
    /// </summary>
    void IApplication.Exit(object? result, Exception? exception, string style)
    {
        if (exception is not null)
        {
            Exit(default, exception, style);
        }
        else if (result is null)
        {
            Exit(default, null, style);
        }
        else
        {
            Exit((TResult)result, null, style);
        }
    }

}
