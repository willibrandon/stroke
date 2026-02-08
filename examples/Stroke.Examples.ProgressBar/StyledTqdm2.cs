using Stroke.Styles;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// tqdm-style progress bar with reverse-video bar.
/// Port of Python Prompt Toolkit's styled-tqdm-2.py example.
/// </summary>
/// <remarks>
/// Requires Feature 71 (ProgressBar API) for runtime testing.
/// </remarks>
public static class StyledTqdm2
{
    public static async Task Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["bar-a"] = "reverse",
        });

        // TODO: Uncomment when Feature 71 (ProgressBar shortcut API) is implemented.
        // var customFormatters = new IProgressBarFormatter[]
        // {
        //     new formatters.Label(suffix: ": "),
        //     new formatters.Percentage(),
        //     new formatters.Bar(start: "|", end: "|", symA: " ", symB: " ", symC: " "),
        //     new formatters.Text(" "),
        //     new formatters.Progress(),
        //     new formatters.Text(" ["),
        //     new formatters.TimeElapsed(),
        //     new formatters.Text("<"),
        //     new formatters.TimeLeft(),
        //     new formatters.Text(", "),
        //     new formatters.IterationsPerSecond(),
        //     new formatters.Text("it/s]"),
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
