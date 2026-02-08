using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar with custom Style affecting 10 visual elements.
/// Port of Python Prompt Toolkit's styled-1.py example.
/// </summary>
public static class Styled1
{
    public static async Task Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["title"] = "#4444ff underline",
            ["label"] = "#ff4400 bold",
            ["percentage"] = "#00ff00",
            ["bar-a"] = "bg:#00ff00 #004400",
            ["bar-b"] = "bg:#00ff00 #000000",
            ["bar-c"] = "#000000 underline",
            ["current"] = "#448844",
            ["total"] = "#448844",
            ["time-elapsed"] = "#444488",
            ["time-left"] = "bg:#88ff88 #000000",
        });

        await using var pb = new ProgressBar(
            style: style,
            title: "Progress bar example with custom styling.");
        foreach (var i in pb.Iterate(Enumerable.Range(0, 1600), label: "Downloading..."))
        {
            Thread.Sleep(10);
        }
    }
}
