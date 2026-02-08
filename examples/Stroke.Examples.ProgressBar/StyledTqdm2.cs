using Stroke.Shortcuts;
using Stroke.Shortcuts.ProgressBarFormatters;
using Stroke.Styles;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// tqdm-style progress bar with reverse-video bar.
/// Port of Python Prompt Toolkit's styled-tqdm-2.py example.
/// </summary>
public static class StyledTqdm2
{
    public static async Task Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["bar-a"] = "reverse",
        });

        var customFormatters = new List<Formatter>
        {
            new Label(suffix: ": "),
            new Percentage(),
            new Bar(start: "|", end: "|", symA: " ", symB: " ", symC: " "),
            new Text(" "),
            new Progress(),
            new Text(" ["),
            new TimeElapsed(),
            new Text("<"),
            new TimeLeft(),
            new Text(", "),
            new IterationsPerSecond(),
            new Text("it/s]"),
        };

        await using var pb = new ProgressBar(style: style, formatters: customFormatters);
        foreach (var i in pb.Iterate(Enumerable.Range(0, 1600), label: "Installing"))
        {
            Thread.Sleep(10);
        }
    }
}
