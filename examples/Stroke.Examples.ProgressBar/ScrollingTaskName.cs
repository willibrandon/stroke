using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar with a long label that scrolls when terminal is narrow.
/// Port of Python Prompt Toolkit's scrolling-task-name.py example.
/// </summary>
public static class ScrollingTaskName
{
    public static async Task Run()
    {
        await using var pb = new ProgressBar(
            title: "Scrolling task name (make sure the window is not too big).");
        foreach (var i in pb.Iterate(
            Enumerable.Range(0, 800),
            label: "This is a very very very long task that requires horizontal scrolling ..."))
        {
            Thread.Sleep(10);
        }
    }
}
