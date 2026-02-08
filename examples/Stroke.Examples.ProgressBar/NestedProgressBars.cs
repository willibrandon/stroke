using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Nested progress bars with inner bars that appear and disappear.
/// Port of Python Prompt Toolkit's nested-progress-bars.py example.
/// </summary>
public static class NestedProgressBars
{
    public static async Task Run()
    {
        await using var pb = new ProgressBar(
            title: new Html("<b fg=\"#aa00ff\">Nested progress bars</b>"),
            bottomToolbar: new Html(" <b>[Control-L]</b> clear  <b>[Control-C]</b> abort"));

        foreach (var i in pb.Iterate(Enumerable.Range(0, 6), label: "Main task"))
        {
            foreach (var j in pb.Iterate(
                Enumerable.Range(0, 200),
                label: $"Subtask <{i + 1}>",
                removeWhenDone: true))
            {
                Thread.Sleep(10);
            }
        }
    }
}
