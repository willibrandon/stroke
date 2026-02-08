using Stroke.Styles;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// tqdm-inspired progress bar format with iterations per second.
/// Port of Python Prompt Toolkit's styled-tqdm-1.py example.
/// </summary>
/// <remarks>
/// Requires Feature 71 (ProgressBar API) for runtime testing.
/// </remarks>
public static class StyledTqdm1
{
    public static async Task Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            [""] = "cyan",
        });

        // TODO: Uncomment when Feature 71 (ProgressBar shortcut API) is implemented.
        // var customFormatters = new IProgressBarFormatter[]
        // {
        //     new formatters.Label(suffix: ": "),
        //     new formatters.Bar(start: "|", end: "|", symA: "#", symB: "#", symC: "-"),
        //     new formatters.Text(" "),
        //     new formatters.Progress(),
        //     new formatters.Text(" "),
        //     new formatters.Percentage(),
        //     new formatters.Text(" [elapsed: "),
        //     new formatters.TimeElapsed(),
        //     new formatters.Text(" left: "),
        //     new formatters.TimeLeft(),
        //     new formatters.Text(", "),
        //     new formatters.IterationsPerSecond(),
        //     new formatters.Text(" iters/sec]"),
        //     new formatters.Text("  "),
        // };
        //
        // await using var pb = new ProgressBar(style: style, formatters: customFormatters);
        // await foreach (var i in pb.Iterate(Enumerable.Range(0, 1600), label: "Installing"))
        // {
        //     await Task.Delay(10);
        // }
        _ = style;
        await Task.CompletedTask;
    }
}
