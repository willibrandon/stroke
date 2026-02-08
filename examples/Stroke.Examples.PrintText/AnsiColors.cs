using Stroke.FormattedText;
using Stroke.Shortcuts;

using FormattedTextType = Stroke.FormattedText.FormattedText;

namespace Stroke.Examples.PrintText;

/// <summary>
/// Display all 16 ANSI foreground and 16 ANSI background colors.
/// Port of Python Prompt Toolkit's ansi-colors.py example.
/// </summary>
public static class AnsiColors
{
    public static void Run()
    {
        StyleAndTextTuple wideSpace = new("", "       ");
        StyleAndTextTuple space = new("", " ");

        FormattedTextOutput.Print(new Html("\n<u>Foreground colors</u>"));
        FormattedTextOutput.Print(new FormattedTextType(
        [
            new("ansiblack", "ansiblack"),
            wideSpace,
            new("ansired", "ansired"),
            wideSpace,
            new("ansigreen", "ansigreen"),
            wideSpace,
            new("ansiyellow", "ansiyellow"),
            wideSpace,
            new("ansiblue", "ansiblue"),
            wideSpace,
            new("ansimagenta", "ansimagenta"),
            wideSpace,
            new("ansicyan", "ansicyan"),
            wideSpace,
            new("ansigray", "ansigray"),
            wideSpace,
            new("", "\n"),
            new("ansibrightblack", "ansibrightblack"),
            space,
            new("ansibrightred", "ansibrightred"),
            space,
            new("ansibrightgreen", "ansibrightgreen"),
            space,
            new("ansibrightyellow", "ansibrightyellow"),
            space,
            new("ansibrightblue", "ansibrightblue"),
            space,
            new("ansibrightmagenta", "ansibrightmagenta"),
            space,
            new("ansibrightcyan", "ansibrightcyan"),
            space,
            new("ansiwhite", "ansiwhite"),
            space,
        ]));

        FormattedTextOutput.Print(new Html("\n<u>Background colors</u>"));
        FormattedTextOutput.Print(new FormattedTextType(
        [
            new("bg:ansiblack ansiwhite", "ansiblack"),
            wideSpace,
            new("bg:ansired", "ansired"),
            wideSpace,
            new("bg:ansigreen", "ansigreen"),
            wideSpace,
            new("bg:ansiyellow", "ansiyellow"),
            wideSpace,
            new("bg:ansiblue ansiwhite", "ansiblue"),
            wideSpace,
            new("bg:ansimagenta", "ansimagenta"),
            wideSpace,
            new("bg:ansicyan", "ansicyan"),
            wideSpace,
            new("bg:ansigray", "ansigray"),
            wideSpace,
            new("", "\n"),
            new("bg:ansibrightblack", "ansibrightblack"),
            space,
            new("bg:ansibrightred", "ansibrightred"),
            space,
            new("bg:ansibrightgreen", "ansibrightgreen"),
            space,
            new("bg:ansibrightyellow", "ansibrightyellow"),
            space,
            new("bg:ansibrightblue", "ansibrightblue"),
            space,
            new("bg:ansibrightmagenta", "ansibrightmagenta"),
            space,
            new("bg:ansibrightcyan", "ansibrightcyan"),
            space,
            new("bg:ansiwhite", "ansiwhite"),
            space,
        ]));

        FormattedTextOutput.Print("");
    }
}
