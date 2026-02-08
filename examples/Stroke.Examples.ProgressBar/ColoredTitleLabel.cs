using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar with HTML-colored title and label text.
/// Port of Python Prompt Toolkit's colored-title-and-label.py example.
/// </summary>
public static class ColoredTitleLabel
{
    public static async Task Run()
    {
        var title = new Html("Downloading <style bg=\"yellow\" fg=\"black\">4 files...</style>");
        var label = new Html("<ansired>some file</ansired>: ");

        await using var pb = new ProgressBar(title: title);
        foreach (var i in pb.Iterate(Enumerable.Range(0, 800), label: label))
        {
            Thread.Sleep(10);
        }
    }
}
