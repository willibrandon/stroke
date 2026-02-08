using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

using PygmentsTokensType = Stroke.FormattedText.PygmentsTokens;

namespace Stroke.Examples.PrintText;

/// <summary>
/// Display syntax-highlighted text using Pygments token types.
/// Port of Python Prompt Toolkit's pygments-tokens.py example.
/// </summary>
public static class PygmentsTokens
{
    public static void Run()
    {
        // 1. Printing a manually constructed list of (Token, text) tuples.
        var text = new (string TokenType, string Text)[]
        {
            ("Token.Keyword", "print"),
            ("Token.Punctuation", "("),
            ("Token.Literal.String.Double", "\""),
            ("Token.Literal.String.Double", "hello"),
            ("Token.Literal.String.Double", "\""),
            ("Token.Punctuation", ")"),
            ("Token.Text", "\n"),
        };

        FormattedTextOutput.Print(new PygmentsTokensType(text));

        // 2. With a custom style.
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["pygments.keyword"] = "underline",
            ["pygments.literal.string"] = "bg:#00ff00 #ffffff",
        });
        FormattedTextOutput.Print(new PygmentsTokensType(text), style: style);
    }
}
