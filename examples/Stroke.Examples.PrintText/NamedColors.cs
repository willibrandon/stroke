using Stroke.FormattedText;
using Stroke.Output;
using Stroke.Shortcuts;

using FormattedTextType = Stroke.FormattedText.FormattedText;
using NamedColorsDict = Stroke.Styles.NamedColors;

namespace Stroke.Examples.PrintText;

/// <summary>
/// Display all named colors at 4-bit, 8-bit, and 24-bit color depths.
/// Port of Python Prompt Toolkit's named-colors.py example.
/// </summary>
public static class NamedColors
{
    public static void Run()
    {
        var tokens = new FormattedTextType(
            NamedColorsDict.Colors.OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                .Select(kvp => new StyleAndTextTuple("fg:" + kvp.Key, kvp.Key + "  ")));

        FormattedTextOutput.Print(new Html("\n<u>Named colors, using 16 color output.</u>"));
        FormattedTextOutput.Print("(Note that it doesn't really make sense to use named colors ");
        FormattedTextOutput.Print("with only 16 color output.)");
        FormattedTextOutput.Print(tokens, colorDepth: ColorDepth.Depth4Bit);

        FormattedTextOutput.Print(new Html("\n<u>Named colors, use 256 colors.</u>"));
        FormattedTextOutput.Print(tokens);

        FormattedTextOutput.Print(new Html("\n<u>Named colors, using True color output.</u>"));
        FormattedTextOutput.Print(tokens, colorDepth: ColorDepth.Depth24Bit);
    }
}
