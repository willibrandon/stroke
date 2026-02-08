using Stroke.Layout.Containers;
using Stroke.Shortcuts;
using Stroke.Widgets.Base;

namespace Stroke.Examples.PrintText;

/// <summary>
/// Render a bordered Frame containing a TextArea using PrintContainer.
/// Port of Python Prompt Toolkit's print-frame.py example.
/// </summary>
public static class PrintFrame
{
    public static void Run()
    {
        FormattedTextOutput.PrintContainer(
            new AnyContainer(new Frame(
                new AnyContainer(new TextArea(text: "Hello world!\n")),
                title: "Stage: parse")));
    }
}
