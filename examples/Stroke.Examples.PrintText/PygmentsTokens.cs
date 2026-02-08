using Stroke.FormattedText;
using Stroke.Lexers;
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

        // 2. Printing the output of a lexer (equivalent to pygments.lex()).
        var tokens = LexPython("print(\"Hello\")");
        FormattedTextOutput.Print(new PygmentsTokensType(tokens));

        // 3. With a custom style.
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["pygments.keyword"] = "underline",
            ["pygments.string"] = "bg:#00ff00 #ffffff",
        });
        FormattedTextOutput.Print(new PygmentsTokensType(tokens), style: style);
    }

    /// <summary>
    /// Tokenize Python source code using TextMateLineLexer and return
    /// Pygments-style (TokenType, Text) tuples.
    /// Equivalent to Python's <c>list(pygments.lex(code, lexer=PythonLexer()))</c>.
    /// </summary>
    private static List<(string TokenType, string Text)> LexPython(string code)
    {
        var lexer = new TextMateLineLexer("source.python");
        var result = lexer.TokenizeLine(code, prevState: null);
        var tokens = new List<(string TokenType, string Text)>();

        foreach (var (_, tokenType, text) in result.Tokens)
        {
            // Convert IReadOnlyList<string> like ["Keyword"] to "Token.Keyword"
            var typePath = "Token." + string.Join(".", tokenType);
            tokens.Add((typePath, text));
        }

        // Add trailing newline like Python's pygments.lex() does
        tokens.Add(("Token.Text", "\n"));
        return tokens;
    }
}
