using Stroke.FormattedText;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar with HTML-colored title and label text.
/// Port of Python Prompt Toolkit's colored-title-and-label.py example.
/// </summary>
/// <remarks>
/// Requires Feature 71 (ProgressBar API) for runtime testing.
/// </remarks>
public static class ColoredTitleLabel
{
    public static async Task Run()
    {
        // TODO: Uncomment when Feature 71 (ProgressBar shortcut API) is implemented.
        // var title = new Html("Downloading <style bg=\"yellow\" fg=\"black\">4 files...</style>");
        // var label = new Html("<ansired>some file</ansired>: ");
        //
        // await using var pb = new ProgressBar(title: title);
        // await foreach (var i in pb.Iterate(Enumerable.Range(0, 800), label: label))
        // {
        //     await Task.Delay(10);
        // }
        await Task.CompletedTask;
    }
}
