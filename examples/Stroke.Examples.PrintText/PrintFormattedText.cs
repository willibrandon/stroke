using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

using AnsiType = Stroke.FormattedText.Ansi;
using FormattedTextType = Stroke.FormattedText.FormattedText;

namespace Stroke.Examples.PrintText;

/// <summary>
/// Demonstrate four distinct formatting methods: FormattedText tuples,
/// HTML with style classes, HTML with inline styles, and ANSI escape sequences.
/// Port of Python Prompt Toolkit's print-formatted-text.py example.
/// </summary>
public static class PrintFormattedText
{
    public static void Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["hello"] = "#ff0066",
            ["world"] = "#44ff44 italic",
        });

        // 1. Print using a list of text fragments.
        var textFragments = new FormattedTextType(
        [
            new("class:hello", "Hello "),
            new("class:world", "World"),
            new("", "\n"),
        ]);
        FormattedTextOutput.Print(textFragments, style: style);

        // 2. Print using an HTML object.
        FormattedTextOutput.Print(new Html("<hello>hello</hello> <world>world</world>\n"), style: style);

        // 3. Print using an HTML object with inline styling.
        FormattedTextOutput.Print(new Html(
            "<style fg=\"#ff0066\">hello</style> "
            + "<style fg=\"#44ff44\"><i>world</i></style>\n"));

        // 4. Print using ANSI escape sequences.
        FormattedTextOutput.Print(new AnsiType("\x1b[31mhello \x1b[32mworld\n"));
    }
}
