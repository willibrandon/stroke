using System.Collections;
using Stroke.FormattedText;

namespace Stroke.Shortcuts;

/// <summary>
/// An individual counter within a <see cref="ProgressBar"/>.
/// Each call to <see cref="ProgressBar.Iterate{T}"/> creates one counter.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>ProgressBarCounter</c> from
/// <c>prompt_toolkit.shortcuts.progress_bar.base</c>.
/// </para>
/// <para>
/// This class is thread-safe for property access. Multiple threads can update
/// <see cref="ItemsCompleted"/>, <see cref="Label"/>, <see cref="Done"/>, and
/// <see cref="Stopped"/> concurrently.
/// </para>
/// </remarks>
public sealed class ProgressBarCounter<T> : IEnumerable<T>
{
    private readonly ProgressBar _progressBar;
    private readonly IEnumerable<T>? _data;
    private volatile bool _done;
    private volatile int _itemsCompleted;
    private AnyFormattedText _label;
    private DateTime? _stopTime;
    private readonly Lock _lock = new();

    /// <summary>
    /// The time this counter was created.
    /// </summary>
    public DateTime StartTime { get; } = DateTime.Now;

    /// <summary>
    /// The total number of items, or null if unknown.
    /// </summary>
    public int? Total { get; }

    /// <summary>
    /// Number of items completed so far.
    /// </summary>
    public int ItemsCompleted
    {
        get => _itemsCompleted;
        set => _itemsCompleted = value;
    }

    /// <summary>
    /// The label/description for this counter.
    /// </summary>
    public AnyFormattedText Label
    {
        get { using (_lock.EnterScope()) return _label; }
        set { using (_lock.EnterScope()) _label = value; }
    }

    /// <summary>
    /// Whether to remove this counter from display when done.
    /// </summary>
    public bool RemoveWhenDone { get; }

    /// <summary>
    /// Whether this counter has completed (reached 100%).
    /// Setting to true also sets <see cref="Stopped"/> and may remove from display.
    /// </summary>
    public bool Done
    {
        get => _done;
        set
        {
            _done = value;
            Stopped = value;
            if (value && RemoveWhenDone)
            {
                _progressBar.RemoveCounter(this);
            }
        }
    }

    /// <summary>
    /// Whether this counter has been stopped (no longer tracking time).
    /// A stopped counter may not have reached 100% (e.g., error or break).
    /// </summary>
    public bool Stopped
    {
        get
        {
            using (_lock.EnterScope())
                return _stopTime is not null;
        }
        set
        {
            using (_lock.EnterScope())
            {
                if (value)
                {
                    _stopTime ??= DateTime.Now;
                }
                else
                {
                    _stopTime = null;
                }
            }
        }
    }

    /// <summary>
    /// Current completion percentage (0-100).
    /// </summary>
    public double Percentage => Total is null or 0 ? 0 : (double)ItemsCompleted * 100 / Math.Max(Total.Value, 1);

    /// <summary>
    /// Time elapsed since this counter started.
    /// </summary>
    public TimeSpan TimeElapsed
    {
        get
        {
            using (_lock.EnterScope())
                return (_stopTime ?? DateTime.Now) - StartTime;
        }
    }

    /// <summary>
    /// Estimated time remaining, or null if unknown.
    /// </summary>
    public TimeSpan? TimeLeft
    {
        get
        {
            if (Total is null || Percentage == 0)
                return null;
            if (Done || Stopped)
                return TimeSpan.Zero;
            return TimeElapsed * (100 - Percentage) / Percentage;
        }
    }

    internal ProgressBarCounter(
        ProgressBar progressBar,
        IEnumerable<T>? data,
        AnyFormattedText label,
        bool removeWhenDone,
        int? total)
    {
        _progressBar = progressBar;
        _data = data;
        _label = label;
        RemoveWhenDone = removeWhenDone;

        if (total is not null)
        {
            Total = total;
        }
        else if (data is ICollection<T> col)
        {
            Total = col.Count;
        }
        else if (data is IReadOnlyCollection<T> roCol)
        {
            Total = roCol.Count;
        }
        else
        {
            Total = null;
        }
    }

    /// <summary>
    /// Signal that one more item has been processed.
    /// </summary>
    public void ItemCompleted()
    {
        Interlocked.Increment(ref _itemsCompleted);
        _progressBar.Invalidate();
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        if (_data is null)
            throw new InvalidOperationException("No data defined to iterate over.");

        var enumerator = _data.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
                ItemCompleted();
            }

            Done = true;
        }
        finally
        {
            Stopped = true;
            enumerator.Dispose();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Non-generic view of a <see cref="ProgressBarCounter{T}"/> for use by formatters.
/// </summary>
public sealed class ProgressBarCounter
{
    private readonly Func<int> _getItemsCompleted;
    private readonly Func<AnyFormattedText> _getLabel;
    private readonly Action<AnyFormattedText> _setLabel;
    private readonly Func<int?> _getTotal;
    private readonly Func<double> _getPercentage;
    private readonly Func<TimeSpan> _getTimeElapsed;
    private readonly Func<TimeSpan?> _getTimeLeft;
    private readonly Func<bool> _getDone;
    private readonly Func<bool> _getStopped;

    internal static ProgressBarCounter From<T>(ProgressBarCounter<T> counter) => new(
        () => counter.ItemsCompleted,
        () => counter.Label,
        v => counter.Label = v,
        () => counter.Total,
        () => counter.Percentage,
        () => counter.TimeElapsed,
        () => counter.TimeLeft,
        () => counter.Done,
        () => counter.Stopped);

    private ProgressBarCounter(
        Func<int> getItemsCompleted,
        Func<AnyFormattedText> getLabel,
        Action<AnyFormattedText> setLabel,
        Func<int?> getTotal,
        Func<double> getPercentage,
        Func<TimeSpan> getTimeElapsed,
        Func<TimeSpan?> getTimeLeft,
        Func<bool> getDone,
        Func<bool> getStopped)
    {
        _getItemsCompleted = getItemsCompleted;
        _getLabel = getLabel;
        _setLabel = setLabel;
        _getTotal = getTotal;
        _getPercentage = getPercentage;
        _getTimeElapsed = getTimeElapsed;
        _getTimeLeft = getTimeLeft;
        _getDone = getDone;
        _getStopped = getStopped;
    }

    /// <summary>Number of items completed.</summary>
    public int ItemsCompleted => _getItemsCompleted();

    /// <summary>Label text for this counter.</summary>
    public AnyFormattedText Label { get => _getLabel(); set => _setLabel(value); }

    /// <summary>Total items, or null if unknown.</summary>
    public int? Total => _getTotal();

    /// <summary>Completion percentage (0-100).</summary>
    public double Percentage => _getPercentage();

    /// <summary>Time since counter started.</summary>
    public TimeSpan TimeElapsed => _getTimeElapsed();

    /// <summary>Estimated time left, or null.</summary>
    public TimeSpan? TimeLeft => _getTimeLeft();

    /// <summary>Whether counter reached 100%.</summary>
    public bool Done => _getDone();

    /// <summary>Whether counter has stopped tracking time.</summary>
    public bool Stopped => _getStopped();
}
