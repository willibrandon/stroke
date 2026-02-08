using Stroke.Output;
using Stroke.Shortcuts;
using Stroke.Shortcuts.ProgressBarFormatters;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Rainbow-colored progress bar with color depth prompt.
/// Port of Python Prompt Toolkit's styled-rainbow.py example.
/// </summary>
public static class StyledRainbow
{
    public static async Task Run()
    {
        var trueColor = Prompt.Confirm("Yes true colors?");

        var customFormatters = new List<Formatter>
        {
            new Label(),
            new Text(" "),
            new Rainbow(new Bar()),
            new Text(" left: "),
            new Rainbow(new TimeLeft()),
        };

        var colorDepth = trueColor ? ColorDepth.Depth24Bit : ColorDepth.Depth8Bit;

        await using var pb = new ProgressBar(formatters: customFormatters, colorDepth: colorDepth);
        foreach (var i in pb.Iterate(Enumerable.Range(0, 20), label: "Downloading..."))
        {
            Thread.Sleep(1000);
        }
    }
}
