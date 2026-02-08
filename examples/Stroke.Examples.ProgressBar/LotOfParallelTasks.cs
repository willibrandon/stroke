using Stroke.FormattedText;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// 160 parallel tasks with random durations, some breaking early.
/// Port of Python Prompt Toolkit's a-lot-of-parallel-tasks.py example.
/// </summary>
/// <remarks>
/// Requires Feature 71 (ProgressBar API) for runtime testing.
/// </remarks>
public static class LotOfParallelTasks
{
    public static async Task Run()
    {
        // TODO: Uncomment when Feature 71 (ProgressBar shortcut API) is implemented.
        // var random = new Random();
        //
        // await using var pb = new ProgressBar(
        //     title: new Html("<b>Example of many parallel tasks.</b>"),
        //     bottomToolbar: new Html("<b>[Control-L]</b> clear  <b>[Control-C]</b> abort"));
        //
        // void RunTask(string label, int total, int sleepMs)
        // {
        //     foreach (var i in pb.Iterate(Enumerable.Range(0, total), label: label))
        //         Thread.Sleep(sleepMs);
        // }
        //
        // void StopTask(string label, int total, int sleepMs)
        // {
        //     // Stop at some random index.
        //     var stopI = random.Next(total);
        //     var bar = pb.Iterate(Enumerable.Range(0, total), label: label);
        //     foreach (var i in bar)
        //     {
        //         if (i == stopI)
        //         {
        //             bar.Label = $"{label} BREAK";
        //             break;
        //         }
        //         Thread.Sleep(sleepMs);
        //     }
        // }
        //
        // var threads = new List<Thread>();
        // var taskFuncs = new Action<string, int, int>[] { RunTask, StopTask };
        //
        // for (var i = 0; i < 160; i++)
        // {
        //     var label = $"Task {i}";
        //     var total = random.Next(50, 200);
        //     var sleepMs = random.Next(5, 20) * 10;
        //     var func = taskFuncs[random.Next(taskFuncs.Length)];
        //
        //     threads.Add(new Thread(() => func(label, total, sleepMs)) { IsBackground = true });
        // }
        //
        // foreach (var t in threads)
        //     t.Start();
        //
        // // Wait for threads with timeout for Windows Ctrl-C compatibility.
        // foreach (var t in threads)
        // {
        //     while (t.IsAlive)
        //         t.Join(TimeSpan.FromMilliseconds(500));
        // }
        await Task.CompletedTask;
    }
}
