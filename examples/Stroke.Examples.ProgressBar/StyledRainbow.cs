using Stroke.Output;
using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Rainbow-colored progress bar with color depth prompt.
/// Port of Python Prompt Toolkit's styled-rainbow.py example.
/// </summary>
/// <remarks>
/// Requires Feature 71 (ProgressBar API) for runtime testing.
/// </remarks>
public static class StyledRainbow
{
    public static async Task Run()
    {
        var trueColor = Prompt.Confirm("Yes true colors?");

        var colorDepth = trueColor ? ColorDepth.Depth24Bit : ColorDepth.Depth8Bit;

        // TODO: Uncomment when Feature 71 (ProgressBar shortcut API) is implemented.
        // var customFormatters = new IProgressBarFormatter[]
        // {
        //     new formatters.Label(),
        //     new formatters.Text(" "),
        //     new formatters.Rainbow(new formatters.Bar()),
        //     new formatters.Text(" left: "),
        //     new formatters.Rainbow(new formatters.TimeLeft()),
        // };
        //
        // await using var pb = new ProgressBar(formatters: customFormatters, colorDepth: colorDepth);
        // await foreach (var i in pb.Iterate(Enumerable.Range(0, 20), label: "Downloading..."))
        // {
        //     await Task.Delay(1000);
        // }
        _ = colorDepth;
        await Task.CompletedTask;
    }
}
