using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Basic progress bar iterating over 800 items.
/// Port of Python Prompt Toolkit's simple-progress-bar.py example.
/// </summary>
public static class SimpleProgressBar
{
    public static async Task Run()
    {
        await using var pb = new ProgressBar();
        foreach (var i in pb.Iterate(Enumerable.Range(0, 800)))
        {
            Thread.Sleep(10);
        }
    }
}
