using Stroke.FormattedText;
using Stroke.Shortcuts;

using HtmlType = Stroke.FormattedText.Html;

namespace Stroke.Examples.PrintText;

/// <summary>
/// Demonstrate HTML-like formatting with &lt;b&gt;, &lt;i&gt;, &lt;ansired&gt;,
/// &lt;style&gt; tags, and string interpolation.
/// Port of Python Prompt Toolkit's html.py example.
/// </summary>
public static class HtmlExample
{
    public static void Run()
    {
        Title("Special formatting");
        FormattedTextOutput.Print(new HtmlType("    <b>Bold</b>"));
        FormattedTextOutput.Print(new HtmlType("    <blink>Blink</blink>"));
        FormattedTextOutput.Print(new HtmlType("    <i>Italic</i>"));
        FormattedTextOutput.Print(new HtmlType("    <reverse>Reverse</reverse>"));
        FormattedTextOutput.Print(new HtmlType("    <u>Underline</u>"));
        FormattedTextOutput.Print(new HtmlType("    <s>Strike</s>"));
        FormattedTextOutput.Print(new HtmlType("    <hidden>Hidden</hidden> (hidden)"));

        // ANSI colors.
        Title("ANSI colors");
        FormattedTextOutput.Print(new HtmlType("    <ansired>ANSI Red</ansired>"));
        FormattedTextOutput.Print(new HtmlType("    <ansiblue>ANSI Blue</ansiblue>"));

        // Other named colors.
        Title("Named colors");
        FormattedTextOutput.Print(new HtmlType("    <orange>orange</orange>"));
        FormattedTextOutput.Print(new HtmlType("    <purple>purple</purple>"));

        // Background colors.
        Title("Background colors");
        FormattedTextOutput.Print(new HtmlType("    <style fg=\"ansiwhite\" bg=\"ansired\">ANSI Red</style>"));
        FormattedTextOutput.Print(new HtmlType("    <style fg=\"ansiwhite\" bg=\"ansiblue\">ANSI Blue</style>"));

        // Interpolation.
        Title("HTML interpolation (see source)");
        FormattedTextOutput.Print(new HtmlType("    <i>{0}</i>").Format("<test>"));
        FormattedTextOutput.Print(new HtmlType("    <b>{0}</b>").Format("<test>"));
        FormattedTextOutput.Print(new HtmlType("    <u>%s</u>") % "<text>");

        FormattedTextOutput.Print("");
    }

    private static void Title(string text)
    {
        FormattedTextOutput.Print(new HtmlType("\n<u><b>{0}</b></u>").Format(text));
    }
}
