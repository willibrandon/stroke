using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar for iteration with no known total length.
/// Port of Python Prompt Toolkit's unknown-length.py example.
/// </summary>
/// <remarks>
/// Uses a generator (yield return) so the progress bar cannot determine
/// the total count and shows only elapsed time instead of an ETA.
/// </remarks>
public static class UnknownLength
{
    /// <summary>
    /// A generator that produces items. The progress bar cannot estimate
    /// completion time since the total is unknown.
    /// </summary>
    private static IEnumerable<int> Data()
    {
        for (var i = 0; i < 1000; i++)
            yield return i;
    }

    public static async Task Run()
    {
        await using var pb = new ProgressBar();
        foreach (var i in pb.Iterate(Data()))
        {
            Thread.Sleep(100);
        }
    }
}
