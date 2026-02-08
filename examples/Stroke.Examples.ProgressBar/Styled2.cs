using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Shortcuts.ProgressBarFormatters;
using Stroke.Styles;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar with custom formatters: SpinningWheel, Bar, TimeLeft.
/// Port of Python Prompt Toolkit's styled-2.py example.
/// </summary>
public static class Styled2
{
    public static async Task Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["progressbar title"] = "#0000ff",
            ["item-title"] = "#ff4400 underline",
            ["percentage"] = "#00ff00",
            ["bar-a"] = "bg:#00ff00 #004400",
            ["bar-b"] = "bg:#00ff00 #000000",
            ["bar-c"] = "bg:#000000 #000000",
            ["tildes"] = "#444488",
            ["time-left"] = "bg:#88ff88 #ffffff",
            ["spinning-wheel"] = "bg:#ffff00 #000000",
        });

        var customFormatters = new List<Formatter>
        {
            new Label(),
            new Text(" "),
            new SpinningWheel(),
            new Text(" "),
            new Text(new Html("<tildes>~~~</tildes>")),
            new Bar(symA: "#", symB: "#", symC: "."),
            new Text(" left: "),
            new TimeLeft(),
        };

        await using var pb = new ProgressBar(
            title: "Progress bar example with custom formatter.",
            formatters: customFormatters,
            style: style);
        foreach (var i in pb.Iterate(Enumerable.Range(0, 20), label: "Downloading..."))
        {
            Thread.Sleep(1000);
        }
    }
}
