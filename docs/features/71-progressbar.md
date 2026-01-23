# Feature 71: Progress Bar

## Overview

Implement a terminal progress bar system with multiple concurrent progress counters, customizable formatters, and automatic refresh. The progress bar runs in a background thread while the main thread processes work.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/shortcuts/progress_bar/`

## Public API

### ProgressBar Class

```csharp
namespace Stroke.Shortcuts.ProgressBar;

/// <summary>
/// Progress bar context manager for displaying progress during long-running operations.
/// </summary>
/// <example>
/// using (var pb = new ProgressBar(title: "Processing"))
/// {
///     foreach (var item in pb.Iterate(data))
///     {
///         ProcessItem(item);
///     }
/// }
/// </example>
public sealed class ProgressBar : IDisposable
{
    /// <summary>
    /// Creates a new progress bar.
    /// </summary>
    /// <param name="title">Text displayed above the progress bars.</param>
    /// <param name="formatters">List of formatters for progress display.</param>
    /// <param name="bottomToolbar">Text displayed in the bottom toolbar.</param>
    /// <param name="style">Style for the progress bar.</param>
    /// <param name="keyBindings">Optional key bindings.</param>
    /// <param name="cancelCallback">Callback invoked on Ctrl-C.</param>
    /// <param name="file">TextWriter for output (default: stderr).</param>
    /// <param name="colorDepth">Color depth to use.</param>
    /// <param name="output">Optional Output instance.</param>
    /// <param name="input">Optional Input instance.</param>
    public ProgressBar(
        AnyFormattedText? title = null,
        IReadOnlyList<Formatter>? formatters = null,
        AnyFormattedText? bottomToolbar = null,
        IStyle? style = null,
        KeyBindings? keyBindings = null,
        Action? cancelCallback = null,
        TextWriter? file = null,
        ColorDepth? colorDepth = null,
        IOutput? output = null,
        IInput? input = null);

    /// <summary>
    /// The title displayed above the progress bars.
    /// </summary>
    public AnyFormattedText? Title { get; set; }

    /// <summary>
    /// The formatters used to display progress.
    /// </summary>
    public IReadOnlyList<Formatter> Formatters { get; }

    /// <summary>
    /// Text displayed in the bottom toolbar.
    /// </summary>
    public AnyFormattedText? BottomToolbar { get; set; }

    /// <summary>
    /// All active counters.
    /// </summary>
    public IReadOnlyList<ProgressBarCounter<object>> Counters { get; }

    /// <summary>
    /// Start a new counter for iterating over data.
    /// </summary>
    /// <typeparam name="T">Type of items in the data.</typeparam>
    /// <param name="data">Data to iterate over.</param>
    /// <param name="label">Label for this counter.</param>
    /// <param name="removeWhenDone">Whether to remove counter when finished.</param>
    /// <param name="total">Total count if known (otherwise calculated from data).</param>
    /// <returns>Counter that can be enumerated.</returns>
    public ProgressBarCounter<T> Iterate<T>(
        IEnumerable<T>? data = null,
        AnyFormattedText? label = null,
        bool removeWhenDone = false,
        int? total = null);

    /// <summary>
    /// Invalidate and trigger a redraw.
    /// </summary>
    public void Invalidate();

    /// <summary>
    /// Dispose and cleanup.
    /// </summary>
    public void Dispose();
}
```

### ProgressBarCounter Class

```csharp
namespace Stroke.Shortcuts.ProgressBar;

/// <summary>
/// An individual progress counter within a ProgressBar.
/// A progress bar can have multiple concurrent counters.
/// </summary>
/// <typeparam name="T">Type of items being tracked.</typeparam>
public sealed class ProgressBarCounter<T> : IEnumerable<T>
{
    /// <summary>
    /// When the counter started.
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    /// When the counter stopped (if stopped).
    /// </summary>
    public DateTime? StopTime { get; }

    /// <summary>
    /// The data being iterated over.
    /// </summary>
    public IEnumerable<T>? Data { get; }

    /// <summary>
    /// Number of items completed so far.
    /// </summary>
    public int ItemsCompleted { get; }

    /// <summary>
    /// Label for this counter.
    /// </summary>
    public AnyFormattedText Label { get; set; }

    /// <summary>
    /// Whether to remove this counter when done.
    /// </summary>
    public bool RemoveWhenDone { get; }

    /// <summary>
    /// Total number of items (if known).
    /// </summary>
    public int? Total { get; }

    /// <summary>
    /// Whether the counter has completed successfully.
    /// </summary>
    public bool Done { get; set; }

    /// <summary>
    /// Whether the counter has stopped (may not be 100% complete).
    /// </summary>
    public bool Stopped { get; set; }

    /// <summary>
    /// Current percentage (0-100).
    /// </summary>
    public float Percentage { get; }

    /// <summary>
    /// Time elapsed since start.
    /// </summary>
    public TimeSpan TimeElapsed { get; }

    /// <summary>
    /// Estimated time remaining.
    /// </summary>
    public TimeSpan? TimeLeft { get; }

    /// <summary>
    /// Mark one item as completed.
    /// Can be called manually when not iterating.
    /// </summary>
    public void ItemCompleted();

    /// <summary>
    /// Get enumerator for the data.
    /// </summary>
    public IEnumerator<T> GetEnumerator();
}
```

### Formatter Classes

```csharp
namespace Stroke.Shortcuts.ProgressBar;

/// <summary>
/// Base class for progress bar formatters.
/// </summary>
public abstract class Formatter
{
    /// <summary>
    /// Format the progress for display.
    /// </summary>
    /// <param name="progressBar">The parent progress bar.</param>
    /// <param name="progress">The counter to format.</param>
    /// <param name="width">Available width in columns.</param>
    /// <returns>Formatted text to display.</returns>
    public abstract AnyFormattedText Format(
        ProgressBar progressBar,
        ProgressBarCounter<object> progress,
        int width);

    /// <summary>
    /// Get the preferred width for this formatter.
    /// </summary>
    /// <param name="progressBar">The parent progress bar.</param>
    /// <returns>Dimension specification.</returns>
    public virtual Dimension GetWidth(ProgressBar progressBar) => Dimension.Any();
}

/// <summary>
/// Display plain text.
/// </summary>
public sealed class Text : Formatter
{
    public Text(AnyFormattedText text, string style = "");
}

/// <summary>
/// Display the counter label.
/// </summary>
public sealed class Label : Formatter
{
    public Label(Dimension? width = null, string suffix = "");
}

/// <summary>
/// Display percentage (e.g., "  75%").
/// </summary>
public sealed class Percentage : Formatter { }

/// <summary>
/// Display the progress bar itself (e.g., "[=====>   ]").
/// </summary>
public sealed class Bar : Formatter
{
    public Bar(
        string start = "[",
        string end = "]",
        string symA = "=",
        string symB = ">",
        string symC = " ",
        string unknown = "#");
}

/// <summary>
/// Display progress as text (e.g., "8/20").
/// </summary>
public sealed class Progress : Formatter { }

/// <summary>
/// Display elapsed time (e.g., "01:23").
/// </summary>
public sealed class TimeElapsed : Formatter { }

/// <summary>
/// Display estimated time remaining (e.g., "02:45").
/// </summary>
public sealed class TimeLeft : Formatter { }

/// <summary>
/// Display iterations per second.
/// </summary>
public sealed class IterationsPerSecond : Formatter { }

/// <summary>
/// Display a spinning wheel animation.
/// </summary>
public sealed class SpinningWheel : Formatter { }

/// <summary>
/// Add rainbow colors to another formatter.
/// </summary>
public sealed class Rainbow : Formatter
{
    public Rainbow(Formatter formatter);
}
```

### Factory Functions

```csharp
namespace Stroke.Shortcuts.ProgressBar;

public static class ProgressBarFactory
{
    /// <summary>
    /// Create the default list of formatters.
    /// </summary>
    /// <returns>Default formatters: Label, Percentage, Bar, Progress, TimeLeft.</returns>
    public static IReadOnlyList<Formatter> CreateDefaultFormatters();
}
```

## Project Structure

```
src/Stroke/
└── Shortcuts/
    └── ProgressBar/
        ├── ProgressBar.cs
        ├── ProgressBarCounter.cs
        ├── Formatter.cs
        ├── Formatters/
        │   ├── Text.cs
        │   ├── Label.cs
        │   ├── Percentage.cs
        │   ├── Bar.cs
        │   ├── Progress.cs
        │   ├── TimeElapsed.cs
        │   ├── TimeLeft.cs
        │   ├── IterationsPerSecond.cs
        │   ├── SpinningWheel.cs
        │   └── Rainbow.cs
        └── ProgressBarFactory.cs
tests/Stroke.Tests/
└── Shortcuts/
    └── ProgressBar/
        ├── ProgressBarTests.cs
        ├── ProgressBarCounterTests.cs
        └── FormatterTests.cs
```

## Implementation Notes

### ProgressBar Lifecycle

```csharp
public sealed class ProgressBar : IDisposable
{
    private readonly List<ProgressBarCounter<object>> _counters = new();
    private Application<object?>? _app;
    private Thread? _thread;
    private ManualResetEventSlim _appStarted = new(false);

    public ProgressBar(...)
    {
        Title = title;
        Formatters = formatters ?? ProgressBarFactory.CreateDefaultFormatters();
        BottomToolbar = bottomToolbar;

        // Set default cancel callback to send SIGINT to main thread
        if (cancelCallback == null && Platform.InMainThread)
        {
            cancelCallback = () =>
            {
                // Send interrupt to main thread
                Process.GetCurrentProcess().Kill(); // Simplified
            };
        }

        _cancelCallback = cancelCallback;
        _output = output ?? AppSession.Current.Output;
        _input = input ?? AppSession.Current.Input;
        _colorDepth = colorDepth;
    }

    public void Start()
    {
        // Build layout
        var titleToolbar = new ConditionalContainer(
            new Window(
                new FormattedTextControl(() => Title),
                height: 1,
                style: "class:progressbar,title"),
            filter: Condition.Create(() => Title != null));

        var bottomToolbarContainer = new ConditionalContainer(
            new Window(
                new FormattedTextControl(() => BottomToolbar),
                style: "class:bottom-toolbar",
                height: 1),
            filter: ~Filters.IsDone
                & Filters.RendererHeightIsKnown
                & Condition.Create(() => BottomToolbar != null));

        var progressControls = Formatters.Select(f =>
            new Window(
                content: new ProgressControl(this, f, _cancelCallback),
                width: () => f.GetWidth(this))).ToList();

        _app = new Application<object?>(
            minRedrawInterval: TimeSpan.FromMilliseconds(50),
            layout: new Layout(
                new HSplit(
                    titleToolbar,
                    new VSplit(progressControls, height: () =>
                        Dimension.Exact(_counters.Count)),
                    new Window(),
                    bottomToolbarContainer)),
            style: Style,
            keyBindings: KeyBindings,
            refreshInterval: TimeSpan.FromMilliseconds(300),
            colorDepth: _colorDepth,
            output: _output,
            input: _input);

        // Run in background thread
        _thread = new Thread(() =>
        {
            try
            {
                _app.Run(preRun: () => _appStarted.Set());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        });
        _thread.Start();
    }

    public void Dispose()
    {
        _appStarted.Wait();

        if (_app?.IsRunning == true && _app.Loop != null)
        {
            _app.Loop.CallSoonThreadsafe(() => _app.Exit());
        }

        _thread?.Join();
    }
}
```

### ProgressBarCounter Implementation

```csharp
public sealed class ProgressBarCounter<T> : IEnumerable<T>
{
    private readonly ProgressBar _progressBar;
    private bool _done;
    private DateTime? _stopTime;

    internal ProgressBarCounter(
        ProgressBar progressBar,
        IEnumerable<T>? data,
        AnyFormattedText? label,
        bool removeWhenDone,
        int? total)
    {
        _progressBar = progressBar;
        StartTime = DateTime.Now;
        Data = data;
        Label = label;
        RemoveWhenDone = removeWhenDone;

        if (total == null && data is ICollection<T> collection)
            Total = collection.Count;
        else
            Total = total;
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (Data == null)
            throw new InvalidOperationException("No data defined to iterate over.");

        try
        {
            foreach (var item in Data)
            {
                yield return item;
                ItemCompleted();
            }
            Done = true;
        }
        finally
        {
            Stopped = true;
        }
    }

    public void ItemCompleted()
    {
        ItemsCompleted++;
        _progressBar.Invalidate();
    }

    public float Percentage =>
        Total == null ? 0 : ItemsCompleted * 100f / Math.Max(Total.Value, 1);

    public TimeSpan TimeElapsed =>
        (_stopTime ?? DateTime.Now) - StartTime;

    public TimeSpan? TimeLeft
    {
        get
        {
            if (Total == null || Percentage == 0) return null;
            if (Done || Stopped) return TimeSpan.Zero;
            return TimeSpan.FromTicks(
                (long)(TimeElapsed.Ticks * (100 - Percentage) / Percentage));
        }
    }
}
```

### Bar Formatter

```csharp
public sealed class Bar : Formatter
{
    private readonly string _start, _end, _symA, _symB, _symC, _unknown;

    public Bar(
        string start = "[",
        string end = "]",
        string symA = "=",
        string symB = ">",
        string symC = " ",
        string unknown = "#")
    {
        _start = start;
        _end = end;
        _symA = symA;
        _symB = symB;
        _symC = symC;
        _unknown = unknown;
    }

    public override AnyFormattedText Format(
        ProgressBar progressBar,
        ProgressBarCounter<object> progress,
        int width)
    {
        string symA, symB, symC;
        float percent;

        if (progress.Done || progress.Total != null || progress.Stopped)
        {
            symA = _symA;
            symB = _symB;
            symC = _symC;
            percent = progress.Done ? 1.0f : progress.Percentage / 100f;
        }
        else
        {
            // Unknown total - animate
            symA = _symC;
            symB = _unknown;
            symC = _symC;
            percent = (float)(DateTime.Now.Ticks / 10000 * 20 % 100) / 100f;
        }

        // Calculate bar
        width -= UnicodeWidth.GetWidth(_start + symB + _end);
        var pbA = (int)(percent * width);
        var barA = new string(_symA[0], pbA);
        var barC = new string(_symC[0], width - pbA);

        return new Html(
            $"<bar>{_start}<bar-a>{barA}</bar-a><bar-b>{symB}</bar-b>" +
            $"<bar-c>{barC}</bar-c>{_end}</bar>");
    }

    public override Dimension GetWidth(ProgressBar progressBar) =>
        Dimension.Min(9);
}
```

### Default Formatters

```csharp
public static IReadOnlyList<Formatter> CreateDefaultFormatters() =>
    new Formatter[]
    {
        new Label(),
        new Text(" "),
        new Percentage(),
        new Text(" "),
        new Bar(),
        new Text(" "),
        new Progress(),
        new Text(" "),
        new Text("eta [", style: "class:time-left"),
        new TimeLeft(),
        new Text("]", style: "class:time-left"),
        new Text(" ")
    };
```

### Usage Examples

```csharp
// Simple usage
using var pb = new ProgressBar(title: "Downloading files");
pb.Start();

foreach (var file in pb.Iterate(files, label: "Files"))
{
    await DownloadAsync(file);
}

// Multiple concurrent counters
using var pb = new ProgressBar();
pb.Start();

var images = pb.Iterate(imageUrls, label: "Images");
var docs = pb.Iterate(docUrls, label: "Documents");

Parallel.ForEach(images.Concat(docs), item => Process(item));

// Custom formatters
var formatters = new Formatter[]
{
    new Rainbow(new Label()),
    new Text(" "),
    new SpinningWheel(),
    new Text(" "),
    new Bar(symA: "█", symB: "▓", symC: "░"),
    new Text(" "),
    new Percentage()
};

using var pb = new ProgressBar(formatters: formatters);
```

## Dependencies

- `Stroke.Application` (Feature 37) - Application lifecycle
- `Stroke.Layout` (Feature 22) - HSplit, VSplit, Window, ConditionalContainer
- `Stroke.Layout.Controls.FormattedTextControl` (Feature 26) - Text display
- `Stroke.Layout.Controls.UIControl` (Feature 24) - Custom control
- `Stroke.Filters` (Feature 12) - IsDone, RendererHeightIsKnown
- `Stroke.KeyBinding` (Feature 19) - Key bindings for Ctrl-C, Ctrl-L
- `Stroke.Utils.UnicodeWidth` (Feature 69) - Character width calculation

## Implementation Tasks

1. Implement `ProgressBar` class with lifecycle management
2. Implement `ProgressBarCounter<T>` with iteration support
3. Implement `Formatter` base class
4. Implement `Text` formatter
5. Implement `Label` formatter with scrolling
6. Implement `Percentage` formatter
7. Implement `Bar` formatter with animation
8. Implement `Progress` formatter (N/M display)
9. Implement `TimeElapsed` formatter
10. Implement `TimeLeft` formatter
11. Implement `IterationsPerSecond` formatter
12. Implement `SpinningWheel` formatter
13. Implement `Rainbow` decorator formatter
14. Implement `CreateDefaultFormatters` factory
15. Implement `ProgressControl` UIControl
16. Write comprehensive unit tests

## Acceptance Criteria

- [ ] ProgressBar runs in background thread
- [ ] Multiple counters can run concurrently
- [ ] Counters update as items are processed
- [ ] Bar shows animation for unknown totals
- [ ] Percentage calculates correctly
- [ ] TimeElapsed updates correctly
- [ ] TimeLeft estimates remaining time
- [ ] Ctrl-C triggers cancel callback
- [ ] Ctrl-L clears and redraws
- [ ] Labels scroll when too wide
- [ ] Rainbow animates colors
- [ ] SpinningWheel animates
- [ ] ProgressBar disposes cleanly
- [ ] RemoveWhenDone removes completed counters
- [ ] Unit tests achieve 80% coverage
