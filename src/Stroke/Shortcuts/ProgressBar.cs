using Stroke.Application;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Output;
using Stroke.Shortcuts.ProgressBarFormatters;
using Stroke.Styles;

namespace Stroke.Shortcuts;

/// <summary>
/// Progress bar context manager for tracking iteration progress.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>ProgressBar</c> from
/// <c>prompt_toolkit.shortcuts.progress_bar.base</c>.
/// </para>
/// <para>
/// Usage:
/// <code>
/// await using var pb = new ProgressBar(title: "Downloading...");
/// foreach (var item in pb.Iterate(data, label: "File 1"))
/// {
///     await Task.Delay(10);
/// }
/// </code>
/// </para>
/// <para>
/// The progress bar runs in a background thread, rendering the UI while the main thread
/// iterates over data. Multiple counters can run in parallel on separate threads.
/// </para>
/// </remarks>
public sealed class ProgressBar : IAsyncDisposable
{
    private readonly List<ProgressBarCounter> _counters = [];
    private readonly Dictionary<object, ProgressBarCounter> _counterMap = [];
    private readonly Lock _lock = new();
    private readonly IList<Formatter> _formatters;
    private readonly Action? _cancelCallback;
    private readonly ManualResetEventSlim _appStarted = new(false);
    private Application<object?> _app = null!;
    private Thread? _thread;

    /// <summary>
    /// Title text displayed above the progress bars.
    /// </summary>
    public AnyFormattedText Title { get; }

    /// <summary>
    /// Bottom toolbar text.
    /// </summary>
    public AnyFormattedText BottomToolbar { get; }

    /// <summary>
    /// Current list of active counters.
    /// </summary>
    public IReadOnlyList<ProgressBarCounter> Counters
    {
        get
        {
            using (_lock.EnterScope())
                return _counters.ToList();
        }
    }

    /// <summary>
    /// Creates and starts a new progress bar.
    /// </summary>
    /// <param name="title">Text displayed above the progress bars.</param>
    /// <param name="formatters">Custom formatter list, or null for defaults.</param>
    /// <param name="bottomToolbar">Text for the bottom toolbar.</param>
    /// <param name="style">Custom style.</param>
    /// <param name="keyBindings">Additional key bindings.</param>
    /// <param name="cancelCallback">Called on Ctrl+C. If null and on main thread, sends SIGINT.</param>
    /// <param name="colorDepth">Color depth override.</param>
    /// <param name="output">Output device override.</param>
    /// <param name="input">Input device override.</param>
    public ProgressBar(
        AnyFormattedText title = default,
        IList<Formatter>? formatters = null,
        AnyFormattedText bottomToolbar = default,
        IStyle? style = null,
        KeyBindings? keyBindings = null,
        Action? cancelCallback = null,
        ColorDepth? colorDepth = null,
        IOutput? output = null,
        IInput? input = null)
    {
        Title = title;
        BottomToolbar = bottomToolbar;
        _formatters = formatters ?? ProgressBarFormatters.FormatterUtils.CreateDefaultFormatters();

        // Default cancel: send SIGINT to process (same as Python's keyboard_interrupt_to_main_thread)
        _cancelCallback = cancelCallback ?? DefaultCancelCallback;

        var appOutput = output ?? Application.AppContext.GetAppSession().Output;
        var appInput = input ?? Application.AppContext.GetAppSession().Input;

        Start(style, keyBindings, colorDepth, appOutput, appInput);
    }

    private void Start(IStyle? style, KeyBindings? keyBindings, ColorDepth? colorDepth, IOutput output, IInput input)
    {
        // Build layout
        var titleToolbar = new ConditionalContainer(
            new AnyContainer(new Window(
                new FormattedTextControl(
                    () => FormattedTextUtils.ToFormattedText(Title, style: "class:progressbar,title")),
                height: Dimension.Exact(1),
                style: "class:progressbar,title")),
            filter: new Condition(() => !Title.IsEmpty));

        var bottomToolbar = new ConditionalContainer(
            new AnyContainer(new Window(
                new FormattedTextControl(
                    () => FormattedTextUtils.ToFormattedText(BottomToolbar, style: "class:bottom-toolbar.text")),
                style: "class:bottom-toolbar",
                height: Dimension.Exact(1))),
            filter: new FilterOrBool(
                AppFilters.IsDone.Invert()
                    .And(AppFilters.RendererHeightIsKnown)
                    .And(new Condition(() => !BottomToolbar.IsEmpty))));

        var progressControls = _formatters.Select(f =>
            (IContainer)new Window(
                content: new ProgressControl(this, f, _cancelCallback),
                widthGetter: () => f.GetWidth(this)))
            .ToList();

        var progressBody = new DynamicContainer(() =>
        {
            var count = Counters.Count;
            return new AnyContainer(
                new VSplit(
                    (IReadOnlyList<IContainer>)progressControls,
                    windowTooSmall: null,
                    align: HorizontalAlign.Justify,
                    padding: null,
                    paddingChar: null,
                    paddingStyle: "",
                    width: null,
                    height: new Dimension(preferred: count, max: count),
                    zIndex: null,
                    modal: false,
                    keyBindings: null,
                    styleGetter: () => ""));
        });

        _app = new Application<object?>(
            layout: new Layout.Layout(new AnyContainer(
                new HSplit(
                    (IReadOnlyList<IContainer>)[titleToolbar, progressBody, new Window(), bottomToolbar]))),
            style: style,
            keyBindings: keyBindings,
            minRedrawInterval: 0.05,
            refreshInterval: 0.3,
            colorDepth: colorDepth,
            output: output,
            input: input);

        // Run application in background thread
        _thread = new Thread(() =>
        {
            try
            {
                _app.Run(preRun: () => _appStarted.Set());
            }
            catch (KeyboardInterruptException)
            {
                // Expected exit signal — user pressed a key bound to interrupt.
            }
            catch (EOFException)
            {
                // Expected exit signal.
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        })
        {
            IsBackground = true,
            Name = "ProgressBar-UI"
        };
        _thread.Start();
    }

    /// <summary>
    /// Create a new counter and iterate over the given data.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="data">Data to iterate over.</param>
    /// <param name="label">Display label for this counter.</param>
    /// <param name="removeWhenDone">Hide this counter when complete.</param>
    /// <param name="total">Override total count (if data doesn't implement ICollection).</param>
    /// <returns>An enumerable that tracks progress as you iterate.</returns>
    public ProgressBarCounter<T> Iterate<T>(
        IEnumerable<T> data,
        AnyFormattedText label = default,
        bool removeWhenDone = false,
        int? total = null)
    {
        var counter = new ProgressBarCounter<T>(this, data, label, removeWhenDone, total);
        AddCounter(counter);
        return counter;
    }

    /// <summary>
    /// Trigger a UI redraw.
    /// </summary>
    public void Invalidate()
    {
        if (_app?.IsRunning == true)
            _app.Invalidate();
    }

    internal void AddCounter<T>(ProgressBarCounter<T> counter)
    {
        var view = ProgressBarCounter.From(counter);
        using (_lock.EnterScope())
        {
            _counters.Add(view);
            _counterMap[counter] = view;
        }
    }

    internal void RemoveCounter<T>(ProgressBarCounter<T> counter)
    {
        using (_lock.EnterScope())
        {
            if (_counterMap.TryGetValue(counter, out var view))
            {
                _counters.Remove(view);
                _counterMap.Remove(counter);
            }
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        // Wait for the app to be started
        _appStarted.Wait(TimeSpan.FromSeconds(5));

        // Exit the application. The try/catch guards against a TOCTOU race where
        // the background thread sets IsRunning=false between our check and Exit() call.
        try
        {
            if (_app is { IsRunning: true })
            {
                _app.Exit();
            }
        }
        catch (InvalidOperationException)
        {
            // App already exited on the background thread — safe to ignore.
        }

        // Wait for the UI thread to finish
        if (_thread is { IsAlive: true })
        {
            _thread.Join(TimeSpan.FromSeconds(5));
        }

        _appStarted.Dispose();
        await ValueTask.CompletedTask;
    }

    private static void DefaultCancelCallback()
    {
        // Send SIGINT to current process (equivalent to Python's os.kill(os.getpid(), signal.SIGINT)).
        // SIGINT is catchable, allowing cleanup code (finally blocks, IAsyncDisposable) to execute.
        // Process.Kill() would send SIGKILL which is uncatchable and terminates immediately.
        if (OperatingSystem.IsWindows())
            WindowsInterrupt();
        else
            UnixInterrupt();
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void WindowsInterrupt()
    {
        GenerateConsoleCtrlEvent(0, 0);
    }

    private static void UnixInterrupt()
    {
        kill(Environment.ProcessId, 2); // SIGINT = 2
    }

    [System.Runtime.InteropServices.DllImport("libc", EntryPoint = "kill")]
    private static extern int kill(int pid, int sig);

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
}
