using Stroke.FormattedText;
using Stroke.Output;
using Stroke.Shortcuts;

using FormattedTextType = Stroke.FormattedText.FormattedText;

namespace Stroke.Examples.PrintText;

/// <summary>
/// Display 7 RGB color gradients each rendered at 3 color depths.
/// Port of Python Prompt Toolkit's true-color-demo.py example.
/// </summary>
public static class TrueColorDemo
{
    public static void Run()
    {
        FormattedTextOutput.Print(new Html("\n<u>True color test.</u>"));

        string[] templates =
        [
            "bg:#{0:x2}0000",         // Red
            "bg:#00{0:x2}00",         // Green
            "bg:#0000{0:x2}",         // Blue
            "bg:#{0:x2}{0:x2}00",     // Yellow
            "bg:#{0:x2}00{0:x2}",     // Magenta
            "bg:#00{0:x2}{0:x2}",     // Cyan
            "bg:#{0:x2}{0:x2}{0:x2}", // Gray
        ];

        foreach (var template in templates)
        {
            var fragments = new List<StyleAndTextTuple>();
            for (var i = 0; i < 256; i += 4)
            {
                fragments.Add(new(string.Format(template, i), " "));
            }

            var ft = new FormattedTextType(fragments);
            FormattedTextOutput.Print(ft, colorDepth: ColorDepth.Depth4Bit);
            FormattedTextOutput.Print(ft, colorDepth: ColorDepth.Depth8Bit);
            FormattedTextOutput.Print(ft, colorDepth: ColorDepth.Depth24Bit);
            FormattedTextOutput.Print("");
        }
    }
}
