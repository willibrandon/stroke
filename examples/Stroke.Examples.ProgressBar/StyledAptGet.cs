using Stroke.Styles;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// apt-get install style progress bar.
/// Port of Python Prompt Toolkit's styled-apt-get-install.py example.
/// </summary>
/// <remarks>
/// Requires Feature 71 (ProgressBar API) for runtime testing.
/// </remarks>
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

        // TODO: Uncomment when Feature 71 (ProgressBar shortcut API) is implemented.
        // var customFormatters = new IProgressBarFormatter[]
        // {
        //     new formatters.Label(),
        //     new formatters.Text(": [", style: "class:percentage"),
        //     new formatters.Percentage(),
        //     new formatters.Text("]", style: "class:percentage"),
        //     new formatters.Text(" "),
        //     new formatters.Bar(symA: "#", symB: "#", symC: "."),
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
