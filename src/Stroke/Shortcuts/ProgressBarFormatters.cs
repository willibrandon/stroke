using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout;

namespace Stroke.Shortcuts.ProgressBarFormatters;

/// <summary>
/// Base class for progress bar formatters.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>Formatter</c> from <c>prompt_toolkit.shortcuts.progress_bar.formatters</c>.
/// </remarks>
public abstract class Formatter
{
    /// <summary>
    /// Format this column for the given progress counter.
    /// </summary>
    public abstract AnyFormattedText Format(ProgressBar progressBar, ProgressBarCounter progress, int width);

    /// <summary>
    /// Return the width for this formatter column.
    /// </summary>
    public virtual Dimension GetWidth(ProgressBar progressBar) => new();
}

/// <summary>
/// Display plain text.
/// </summary>
public sealed class Text : Formatter
{
    private readonly FormattedText.FormattedText _text;

    /// <summary>Initializes a new text formatter.</summary>
    public Text(AnyFormattedText text, string style = "")
    {
        _text = FormattedTextUtils.ToFormattedText(text, style);
    }

    /// <inheritdoc/>
    public override AnyFormattedText Format(ProgressBar progressBar, ProgressBarCounter progress, int width) => _text;

    /// <inheritdoc/>
    public override Dimension GetWidth(ProgressBar progressBar) =>
        Dimension.Exact(FormattedTextUtils.FragmentListWidth(_text));
}

/// <summary>
/// Display the name of the current task.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>Label</c> formatter.
/// Supports scrolling when the label is too long for the available width.
/// </remarks>
public sealed class Label : Formatter
{
    private readonly Dimension? _width;
    private readonly string _suffix;

    /// <summary>Initializes a new label formatter.</summary>
    public Label(Dimension? width = null, string suffix = "")
    {
        _width = width;
        _suffix = suffix;
    }

    private FormattedText.FormattedText AddSuffix(AnyFormattedText label)
    {
        var fragments = FormattedTextUtils.ToFormattedText(label, "class:label");
        var result = new List<StyleAndTextTuple>(fragments);
        result.Add(new StyleAndTextTuple("", _suffix));
        return new FormattedText.FormattedText(result);
    }

    /// <inheritdoc/>
    public override AnyFormattedText Format(ProgressBar progressBar, ProgressBarCounter progress, int width)
    {
        var label = AddSuffix(progress.Label);
        var cwidth = FormattedTextUtils.FragmentListWidth(label);

        if (cwidth > width)
        {
            var exploded = LayoutUtils.ExplodeTextFragments(label);
            var maxScroll = cwidth - width;
            var currentScroll = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() * 3 % maxScroll);
            // Take a slice starting from currentScroll
            var sliced = new List<StyleAndTextTuple>();
            for (var i = currentScroll; i < exploded.Count; i++)
                sliced.Add(exploded[i]);
            return new FormattedText.FormattedText(sliced);
        }

        return label;
    }

    /// <inheritdoc/>
    public override Dimension GetWidth(ProgressBar progressBar)
    {
        if (_width is not null)
            return _width;

        var allLabels = progressBar.Counters.Select(c => AddSuffix(c.Label)).ToList();
        if (allLabels.Count > 0)
        {
            var maxWidth = allLabels.Max(l => FormattedTextUtils.FragmentListWidth(l));
            return new Dimension(preferred: maxWidth, max: maxWidth);
        }

        return new Dimension();
    }
}

/// <summary>
/// Display the progress as a percentage.
/// </summary>
public sealed class Percentage : Formatter
{
    /// <inheritdoc/>
    public override AnyFormattedText Format(ProgressBar progressBar, ProgressBarCounter progress, int width)
    {
        var pct = Math.Round(progress.Percentage, 1);
        return new Html($"<percentage>{pct,5:F1}%</percentage>");
    }

    /// <inheritdoc/>
    public override Dimension GetWidth(ProgressBar progressBar) => Dimension.Exact(6);
}

/// <summary>
/// Display the progress bar itself.
/// </summary>
public sealed class Bar : Formatter
{
    private readonly string _start;
    private readonly string _end;
    private readonly string _symA;
    private readonly string _symB;
    private readonly string _symC;
    private readonly string _unknown;

    /// <summary>Initializes a new bar formatter.</summary>
    public Bar(string start = "[", string end = "]", string symA = "=", string symB = ">", string symC = " ", string unknown = "#")
    {
        _start = start;
        _end = end;
        _symA = symA;
        _symB = symB;
        _symC = symC;
        _unknown = unknown;
    }

    /// <inheritdoc/>
    public override AnyFormattedText Format(ProgressBar progressBar, ProgressBarCounter progress, int width)
    {
        string symA, symB, symC;
        double percent;

        if (progress.Done || progress.Total is > 0 || progress.Stopped)
        {
            symA = _symA;
            symB = _symB;
            symC = _symC;

            percent = progress.Done ? 1.0 : progress.Percentage / 100.0;
        }
        else
        {
            symA = _symC;
            symB = _unknown;
            symC = _symC;

            percent = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0 * 20 % 100 / 100.0;
        }

        width -= UnicodeWidth.GetWidth(_start + symB + _end);
        if (width < 0) width = 0;

        var pbA = (int)(percent * width);
        var barA = new string(symA[0], pbA);
        var barB = symB;
        var barC = new string(symC[0], Math.Max(0, width - pbA));

        return new Html(
            $"<bar>{Escape(_start)}<bar-a>{Escape(barA)}</bar-a><bar-b>{Escape(barB)}</bar-b><bar-c>{Escape(barC)}</bar-c>{Escape(_end)}</bar>");
    }

    private static string Escape(string s) => Html.Escape(s);

    /// <inheritdoc/>
    public override Dimension GetWidth(ProgressBar progressBar) => new(min: 9);
}

/// <summary>
/// Display the progress as text. E.g. "8/20".
/// </summary>
public sealed class Progress : Formatter
{
    /// <inheritdoc/>
    public override AnyFormattedText Format(ProgressBar progressBar, ProgressBarCounter progress, int width)
    {
        var current = progress.ItemsCompleted;
        var total = progress.Total?.ToString() ?? "?";
        return new Html($"<current>{current,3}</current>/<total>{total,3}</total>");
    }

    /// <inheritdoc/>
    public override Dimension GetWidth(ProgressBar progressBar)
    {
        var allLengths = progressBar.Counters
            .Select(c => $"{c.Total?.ToString() ?? "?",3}".Length)
            .ToList();
        allLengths.Add(1);
        return Dimension.Exact(allLengths.Max() * 2 + 1);
    }
}

/// <summary>
/// Display the elapsed time.
/// </summary>
public sealed class TimeElapsed : Formatter
{
    /// <inheritdoc/>
    public override AnyFormattedText Format(ProgressBar progressBar, ProgressBarCounter progress, int width)
    {
        var text = FormatterUtils.FormatTimeDelta(progress.TimeElapsed).PadLeft(width);
        return new Html($"<time-elapsed>{Html.Escape(text)}</time-elapsed>");
    }

    /// <inheritdoc/>
    public override Dimension GetWidth(ProgressBar progressBar)
    {
        var allValues = progressBar.Counters.Select(c => FormatterUtils.FormatTimeDelta(c.TimeElapsed).Length).ToList();
        return allValues.Count > 0 ? Dimension.Exact(allValues.Max()) : Dimension.Exact(0);
    }
}

/// <summary>
/// Display the time left.
/// </summary>
public sealed class TimeLeft : Formatter
{
    private const string Unknown = "?:??:??";

    /// <inheritdoc/>
    public override AnyFormattedText Format(ProgressBar progressBar, ProgressBarCounter progress, int width)
    {
        var timeLeft = progress.TimeLeft;
        var text = timeLeft.HasValue ? FormatterUtils.FormatTimeDelta(timeLeft.Value) : Unknown;
        return new Html($"<time-left>{Html.Escape(text.PadLeft(width))}</time-left>");
    }

    /// <inheritdoc/>
    public override Dimension GetWidth(ProgressBar progressBar)
    {
        var allValues = progressBar.Counters
            .Select(c => c.TimeLeft.HasValue ? FormatterUtils.FormatTimeDelta(c.TimeLeft.Value).Length : 7)
            .ToList();
        return allValues.Count > 0 ? Dimension.Exact(allValues.Max()) : Dimension.Exact(0);
    }
}

/// <summary>
/// Display the iterations per second.
/// </summary>
public sealed class IterationsPerSecond : Formatter
{
    /// <inheritdoc/>
    public override AnyFormattedText Format(ProgressBar progressBar, ProgressBarCounter progress, int width)
    {
        var elapsed = progress.TimeElapsed.TotalSeconds;
        var value = elapsed > 0 ? progress.ItemsCompleted / elapsed : 0;
        return new Html($"<iterations-per-second>{value:F2}</iterations-per-second>");
    }

    /// <inheritdoc/>
    public override Dimension GetWidth(ProgressBar progressBar)
    {
        var allValues = progressBar.Counters.Select(c =>
        {
            var elapsed = c.TimeElapsed.TotalSeconds;
            var value = elapsed > 0 ? c.ItemsCompleted / elapsed : 0;
            return $"{value:F2}".Length;
        }).ToList();
        return allValues.Count > 0 ? Dimension.Exact(allValues.Max()) : Dimension.Exact(0);
    }
}

/// <summary>
/// Display a spinning wheel.
/// </summary>
public sealed class SpinningWheel : Formatter
{
    private const string Characters = @"/-\|";

    /// <inheritdoc/>
    public override AnyFormattedText Format(ProgressBar progressBar, ProgressBarCounter progress, int width)
    {
        var index = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() * 3) % Characters.Length;
        return new Html($"<spinning-wheel>{Html.Escape(Characters[index].ToString())}</spinning-wheel>");
    }

    /// <inheritdoc/>
    public override Dimension GetWidth(ProgressBar progressBar) => Dimension.Exact(1);
}

/// <summary>
/// Add rainbow colors to any formatter.
/// </summary>
public sealed class Rainbow : Formatter
{
    private static readonly string[] Colors = Enumerable.Range(0, 100)
        .Select(h => HueToColor(h / 100.0))
        .ToArray();

    private readonly Formatter _formatter;

    /// <summary>Initializes a new rainbow formatter wrapping the given formatter.</summary>
    public Rainbow(Formatter formatter)
    {
        _formatter = formatter;
    }

    /// <inheritdoc/>
    public override AnyFormattedText Format(ProgressBar progressBar, ProgressBarCounter progress, int width)
    {
        var result = _formatter.Format(progressBar, progress, width);
        var formatted = FormattedTextUtils.ToFormattedText(result);
        var exploded = LayoutUtils.ExplodeTextFragments(formatted);

        var shift = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() * 3) % Colors.Length;
        var colored = new List<StyleAndTextTuple>(exploded.Count);

        for (var i = 0; i < exploded.Count; i++)
        {
            var fragment = exploded[i];
            colored.Add(new StyleAndTextTuple(
                fragment.Style + " " + Colors[(i + shift) % Colors.Length], fragment.Text));
        }

        return new FormattedText.FormattedText(colored);
    }

    /// <inheritdoc/>
    public override Dimension GetWidth(ProgressBar progressBar) => _formatter.GetWidth(progressBar);

    private static string HueToColor(double hue)
    {
        var i = (int)(hue * 6.0);
        var f = hue * 6.0 - i;
        var q = (int)(255 * (1.0 - f));
        var t = (int)(255 * f);
        i %= 6;

        var (r, g, b) = i switch
        {
            0 => (255, t, 0),
            1 => (q, 255, 0),
            2 => (0, 255, t),
            3 => (0, q, 255),
            4 => (t, 0, 255),
            _ => (255, 0, q),
        };

        return $"#{r:x2}{g:x2}{b:x2}";
    }
}

/// <summary>
/// Utility methods for progress bar formatters.
/// </summary>
public static class FormatterUtils
{
    /// <summary>
    /// Format a <see cref="TimeSpan"/> as hh:mm:ss or mm:ss.
    /// </summary>
    internal static string FormatTimeDelta(TimeSpan ts)
    {
        // Use TotalHours (not h specifier) to avoid wrapping at 24 hours.
        // Matches Python's str(timedelta) which preserves total hours.
        var totalHours = (int)ts.TotalHours;
        var result = $"{totalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        if (result.StartsWith("0:"))
            result = result[2..];
        return result;
    }

    /// <summary>
    /// Create the default list of formatters.
    /// </summary>
    public static List<Formatter> CreateDefaultFormatters() =>
    [
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
        new Text(" "),
    ];
}
