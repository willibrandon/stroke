namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar for iteration with no known total length.
/// Port of Python Prompt Toolkit's unknown-length.py example.
/// </summary>
/// <remarks>
/// <para>
/// Uses a generator (yield return) so the progress bar cannot determine
/// the total count and shows only elapsed time instead of an ETA.
/// </para>
/// <para>
/// Requires Feature 71 (ProgressBar API) for runtime testing.
/// </para>
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
        // TODO: Uncomment when Feature 71 (ProgressBar shortcut API) is implemented.
        // await using var pb = new ProgressBar();
        // await foreach (var i in pb.Iterate(Data()))
        // {
        //     await Task.Delay(100);
        // }
        await Task.CompletedTask;
    }
}
