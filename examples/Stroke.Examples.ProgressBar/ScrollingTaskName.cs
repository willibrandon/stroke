namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar with a long label that scrolls when terminal is narrow.
/// Port of Python Prompt Toolkit's scrolling-task-name.py example.
/// </summary>
/// <remarks>
/// Requires Feature 71 (ProgressBar API) for runtime testing.
/// </remarks>
public static class ScrollingTaskName
{
    public static async Task Run()
    {
        // TODO: Uncomment when Feature 71 (ProgressBar shortcut API) is implemented.
        // await using var pb = new ProgressBar(
        //     title: "Scrolling task name (make sure the window is not too big).");
        // await foreach (var i in pb.Iterate(
        //     Enumerable.Range(0, 800),
        //     label: "This is a very very very long task that requires horizontal scrolling ..."))
        // {
        //     await Task.Delay(10);
        // }
        await Task.CompletedTask;
    }
}
