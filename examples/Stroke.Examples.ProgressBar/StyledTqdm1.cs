using Stroke.Shortcuts;
using Stroke.Shortcuts.ProgressBarFormatters;
using Stroke.Styles;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// tqdm-inspired progress bar format with iterations per second.
/// Port of Python Prompt Toolkit's styled-tqdm-1.py example.
/// </summary>
public static class StyledTqdm1
{
    public static async Task Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            [""] = "cyan",
        });

        var customFormatters = new List<Formatter>
        {
            new Label(suffix: ": "),
            new Bar(start: "|", end: "|", symA: "#", symB: "#", symC: "-"),
            new Text(" "),
            new Progress(),
            new Text(" "),
            new Percentage(),
            new Text(" [elapsed: "),
            new TimeElapsed(),
            new Text(" left: "),
            new TimeLeft(),
            new Text(", "),
            new IterationsPerSecond(),
            new Text(" iters/sec]"),
            new Text("  "),
        };

        await using var pb = new ProgressBar(style: style, formatters: customFormatters);
        foreach (var i in pb.Iterate(Enumerable.Range(0, 1600), label: "Installing"))
        {
            Thread.Sleep(10);
        }
    }
}
