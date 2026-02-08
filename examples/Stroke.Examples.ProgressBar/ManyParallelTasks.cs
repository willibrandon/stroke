using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// 8 concurrent parallel tasks with HTML title and bottom toolbar.
/// Port of Python Prompt Toolkit's many-parallel-tasks.py example.
/// </summary>
public static class ManyParallelTasks
{
    public static async Task Run()
    {
        await using var pb = new ProgressBar(
            title: new Html("<b>Example of many parallel tasks.</b>"),
            bottomToolbar: new Html("<b>[Control-L]</b> clear  <b>[Control-C]</b> abort"));

        void RunTask(string label, int total, int sleepMs)
        {
            foreach (var i in pb.Iterate(Enumerable.Range(0, total), label: label))
                Thread.Sleep(sleepMs);
        }

        var threads = new[]
        {
            new Thread(() => RunTask("First task", 50, 100)) { IsBackground = true },
            new Thread(() => RunTask("Second task", 100, 100)) { IsBackground = true },
            new Thread(() => RunTask("Third task", 8, 3000)) { IsBackground = true },
            new Thread(() => RunTask("Fourth task", 200, 100)) { IsBackground = true },
            new Thread(() => RunTask("Fifth task", 40, 200)) { IsBackground = true },
            new Thread(() => RunTask("Sixth task", 220, 100)) { IsBackground = true },
            new Thread(() => RunTask("Seventh task", 85, 50)) { IsBackground = true },
            new Thread(() => RunTask("Eight task", 200, 50)) { IsBackground = true },
        };

        foreach (var t in threads)
            t.Start();

        // Wait for threads with timeout for Windows Ctrl-C compatibility.
        foreach (var t in threads)
        {
            while (t.IsAlive)
                t.Join(TimeSpan.FromMilliseconds(500));
        }
    }
}
