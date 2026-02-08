namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Basic progress bar iterating over 800 items.
/// Port of Python Prompt Toolkit's simple-progress-bar.py example.
/// </summary>
/// <remarks>
/// Requires Feature 71 (ProgressBar API) for runtime testing.
/// </remarks>
public static class SimpleProgressBar
{
    public static async Task Run()
    {
        // TODO: Uncomment when Feature 71 (ProgressBar shortcut API) is implemented.
        // await using var pb = new ProgressBar();
        // await foreach (var i in pb.Iterate(Enumerable.Range(0, 800)))
        // {
        //     await Task.Delay(10);
        // }
        await Task.CompletedTask;
    }
}
