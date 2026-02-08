using Stroke.Shortcuts;
using Stroke.Shortcuts.ProgressBarFormatters;
using Stroke.Styles;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// apt-get install style progress bar.
/// Port of Python Prompt Toolkit's styled-apt-get-install.py example.
/// </summary>
public static class StyledAptGet
{
    public static async Task Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["label"] = "bg:#ffff00 #000000",
            ["percentage"] = "bg:#ffff00 #000000",
            ["current"] = "#448844",
            ["bar"] = "",
        });

        var customFormatters = new List<Formatter>
        {
            new Label(),
            new Text(": [", style: "class:percentage"),
            new Percentage(),
            new Text("]", style: "class:percentage"),
            new Text(" "),
            new Bar(symA: "#", symB: "#", symC: "."),
            new Text("  "),
        };

        await using var pb = new ProgressBar(style: style, formatters: customFormatters);
        foreach (var i in pb.Iterate(Enumerable.Range(0, 1600), label: "Installing"))
        {
            Thread.Sleep(10);
        }
    }
}
