using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Two parallel progress bars on separate threads.
/// Port of Python Prompt Toolkit's two-tasks.py example.
/// </summary>
public static class TwoTasks
{
    public static async Task Run()
    {
        await using var pb = new ProgressBar();

        void Task1()
        {
            foreach (var i in pb.Iterate(Enumerable.Range(0, 100)))
                Thread.Sleep(50);
        }

        void Task2()
        {
            foreach (var i in pb.Iterate(Enumerable.Range(0, 150)))
                Thread.Sleep(80);
        }

        var t1 = new Thread(Task1) { IsBackground = true };
        var t2 = new Thread(Task2) { IsBackground = true };
        t1.Start();
        t2.Start();

        // Wait for threads with timeout for Windows Ctrl-C compatibility.
        foreach (var t in new[] { t1, t2 })
        {
            while (t.IsAlive)
                t.Join(TimeSpan.FromMilliseconds(500));
        }
    }
}
