using Stroke.FormattedText;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Nested progress bars with inner bars that appear and disappear.
/// Port of Python Prompt Toolkit's nested-progress-bars.py example.
/// </summary>
/// <remarks>
/// Requires Feature 71 (ProgressBar API) for runtime testing.
/// </remarks>
public static class NestedProgressBars
{
    public static async Task Run()
    {
        // TODO: Uncomment when Feature 71 (ProgressBar shortcut API) is implemented.
        // await using var pb = new ProgressBar(
        //     title: new Html("<b fg=\"#aa00ff\">Nested progress bars</b>"),
        //     bottomToolbar: new Html(" <b>[Control-L]</b> clear  <b>[Control-C]</b> abort"));
        //
        // await foreach (var i in pb.Iterate(Enumerable.Range(0, 6), label: "Main task"))
        // {
        //     await foreach (var j in pb.Iterate(
        //         Enumerable.Range(0, 200),
        //         label: $"Subtask <{i + 1}>",
        //         removeWhenDone: true))
        //     {
        //         await Task.Delay(10);
        //     }
        // }
        await Task.CompletedTask;
    }
}
