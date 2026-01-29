using System.Text.RegularExpressions;
using Stroke.Lexers;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Real IPygmentsLexer implementation for testing purposes.
/// Produces deterministic tokens for predictable test assertions.
/// </summary>
/// <remarks>
/// <para>
/// This is a <b>real implementation</b>, not a mock. It performs actual tokenization
/// and is fully functional code. Constitution VIII prohibits mocks/fakes but permits
/// real implementations created for testing.
/// </para>
/// <para>
/// The tokenization is simplified compared to a real Python lexer, but produces
/// consistent, predictable output suitable for testing PygmentsLexer.
/// </para>
/// <para>
/// This type is thread-safe. All state is local to each GetTokensUnprocessed call.
/// </para>
/// </remarks>
internal sealed partial class TestPythonLexer : IPygmentsLexer
{
    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "def", "class", "if", "else", "elif", "for", "while", "try", "except",
        "finally", "with", "return", "yield", "import", "from", "as", "raise",
        "pass", "break", "continue", "lambda", "and", "or", "not", "in", "is",
        "True", "False", "None"
    };

    private static readonly Regex TokenPattern = MyRegex();

    /// <inheritdoc/>
    public string Name => "Python";

    /// <inheritdoc/>
    public IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)> GetTokensUnprocessed(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        foreach (Match match in TokenPattern.Matches(text))
        {
            var tokenType = GetTokenType(match);
            yield return (match.Index, tokenType, match.Value);
        }
    }

    private static string[] GetTokenType(Match match)
    {
        if (match.Groups["string"].Success)
        {
            return ["String"];
        }

        if (match.Groups["comment"].Success)
        {
            return ["Comment", "Single"];
        }

        if (match.Groups["number"].Success)
        {
            return ["Number"];
        }

        if (match.Groups["identifier"].Success)
        {
            var word = match.Value;

            if (Keywords.Contains(word))
            {
                return ["Keyword"];
            }

            // Check if it's a function definition
            // We'd need lookahead context for this, so simplify to just check common patterns
            return ["Name"];
        }

        if (match.Groups["whitespace"].Success)
        {
            return ["Text"];
        }

        if (match.Groups["punctuation"].Success)
        {
            return ["Punctuation"];
        }

        return ["Text"];
    }

    [GeneratedRegex(@"
        (?<string>""[^""]*""|'[^']*')                   |  # Strings
        (?<comment>\#.*)                                |  # Comments
        (?<number>\d+\.?\d*)                            |  # Numbers
        (?<identifier>[a-zA-Z_][a-zA-Z0-9_]*)           |  # Identifiers
        (?<whitespace>\s+)                              |  # Whitespace
        (?<punctuation>[^\s\w])                            # Other punctuation
    ", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex MyRegex();
}
