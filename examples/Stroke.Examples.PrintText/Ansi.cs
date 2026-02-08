using Stroke.FormattedText;
using Stroke.Shortcuts;

using AnsiType = Stroke.FormattedText.Ansi;

namespace Stroke.Examples.PrintText;

/// <summary>
/// Demonstrate ANSI escape sequences for bold, italic, underline, strikethrough,
/// and 256-color output.
/// Port of Python Prompt Toolkit's ansi.py example.
/// </summary>
public static class Ansi
{
    public static void Run()
    {
        Title("Special formatting");
        FormattedTextOutput.Print(new AnsiType("    \x1b[1mBold"));
        FormattedTextOutput.Print(new AnsiType("    \x1b[6mBlink"));
        FormattedTextOutput.Print(new AnsiType("    \x1b[3mItalic"));
        FormattedTextOutput.Print(new AnsiType("    \x1b[7mReverse"));
        FormattedTextOutput.Print(new AnsiType("    \x1b[4mUnderline"));
        FormattedTextOutput.Print(new AnsiType("    \x1b[9mStrike"));
        FormattedTextOutput.Print(new AnsiType("    \x1b[8mHidden\x1b[0m (Hidden)"));

        // ANSI colors.
        Title("ANSI colors");
        FormattedTextOutput.Print(new AnsiType("    \x1b[91mANSI Red"));
        FormattedTextOutput.Print(new AnsiType("    \x1b[94mANSI Blue"));

        // Other named colors.
        Title("Named colors");
        FormattedTextOutput.Print(new AnsiType("    \x1b[38;5;214morange"));
        FormattedTextOutput.Print(new AnsiType("    \x1b[38;5;90mpurple"));

        // Background colors.
        Title("Background colors");
        FormattedTextOutput.Print(new AnsiType("    \x1b[97;101mANSI Red"));
        FormattedTextOutput.Print(new AnsiType("    \x1b[97;104mANSI Blue"));

        FormattedTextOutput.Print("");
    }

    private static void Title(string text)
    {
        FormattedTextOutput.Print(new Html("\n<u><b>{0}</b></u>").Format(text));
    }
}
