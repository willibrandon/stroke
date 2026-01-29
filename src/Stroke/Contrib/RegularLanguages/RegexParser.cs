using System.Text.RegularExpressions;

namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Parser for converting regular expression grammar strings into parse trees.
/// </summary>
/// <remarks>
/// <para>
/// This parser processes multiline regex strings, ignoring whitespace
/// (except in character classes) and supporting #-style comments.
/// </para>
/// <para>
/// The parser supports Python-style named groups <c>(?P&lt;name&gt;...)</c>
/// for compatibility with Python Prompt Toolkit grammars.
/// </para>
/// </remarks>
public static partial class RegexParser
{
    // Regular expression for tokenizing other regular expressions
    // Pattern matches (in order of specificity):
    // - Named groups: (?P<name>
    // - Comment groups: (?#...)
    // - Lookahead: (?= or (?!
    // - Lookbehind: (?<= or (?<
    // - Non-capturing: (?:
    // - Flags: (?i, (?m, etc
    // - Back reference: (?P=name)
    // - Plain groups: ( and )
    // - Repetition: {n,m}, *, +, ?, *?, +?, ??
    // - Comments: #...\n
    // - Escapes: \.
    // - Character classes: [...]
    // - Other characters
    [GeneratedRegex(
        @"^(\(\?P<[a-zA-Z0-9_-]+>|\(\?#[^)]*\)|\(\?=|\(\?!|\(\?<=|\(\?<|\(\?:|\(\?[iLmsux]|\(\?P=[a-zA-Z]+\)|\(|\)|\{[^{}]*\}|\*\?|\+\?|\?\?|\*|\+|\?|#.*\n|\\.|(\[([^\]\\]|\\.)*\])|[^(){}]|.)",
        RegexOptions.Compiled)]
    private static partial Regex TokenizerRegex();

    /// <summary>
    /// Tokenize a regular expression string.
    /// </summary>
    /// <param name="input">
    /// The regular expression string to tokenize.
    /// Supports Python-style named groups <c>(?P&lt;name&gt;...)</c>,
    /// #-style comments, and verbose whitespace.
    /// </param>
    /// <returns>A list of token strings.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="input"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the input cannot be tokenized.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Comments (<c>#...\n</c>) and whitespace are not included in the output.
    /// </para>
    /// <para>
    /// Tokens include:
    /// <list type="bullet">
    ///   <item><c>(?P&lt;name&gt;</c> - Start of named group</item>
    ///   <item><c>(?:</c> - Start of non-capturing group</item>
    ///   <item><c>(</c> - Start of group</item>
    ///   <item><c>)</c> - End of group</item>
    ///   <item><c>(?!</c> - Negative lookahead</item>
    ///   <item><c>(?=</c> - Positive lookahead</item>
    ///   <item><c>*</c>, <c>+</c>, <c>?</c> - Greedy repetition</item>
    ///   <item><c>*?</c>, <c>+?</c>, <c>??</c> - Non-greedy repetition</item>
    ///   <item><c>|</c> - Alternation</item>
    ///   <item><c>[...]</c> - Character class</item>
    ///   <item><c>\.</c> - Escaped character</item>
    ///   <item>Literal characters</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static IReadOnlyList<string> TokenizeRegex(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var tokens = new List<string>();
        var remaining = input;

        while (remaining.Length > 0)
        {
            var match = TokenizerRegex().Match(remaining);
            if (match.Success)
            {
                var token = remaining[..match.Length];
                remaining = remaining[match.Length..];

                // Skip whitespace and comments (except in character classes)
                if (!string.IsNullOrWhiteSpace(token) && !token.StartsWith('#'))
                {
                    tokens.Add(token);
                }
            }
            else
            {
                throw new ArgumentException("Could not tokenize input regex.", nameof(input));
            }
        }

        return tokens;
    }

    /// <summary>
    /// Parse a list of regex tokens into a parse tree.
    /// </summary>
    /// <param name="tokens">Tokens from <see cref="TokenizeRegex"/>.</param>
    /// <returns>The root node of the parse tree.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="tokens"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the tokens contain syntax errors (unmatched parentheses, etc.).
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown if the tokens contain unsupported constructs:
    /// <list type="bullet">
    ///   <item>Positive lookahead <c>(?=...)</c></item>
    ///   <item><c>{n,m}</c> style repetition</item>
    /// </list>
    /// </exception>
    public static Node ParseRegex(IReadOnlyList<string> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        // We add a closing brace because that represents the final pop of the stack
        // and reverse the tokens for easy popping
        var stack = new List<string> { ")" };
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            stack.Add(tokens[i]);
        }

        var result = Parse(stack);

        if (stack.Count != 0)
        {
            throw new ArgumentException("Unmatched parentheses.", nameof(tokens));
        }

        return result;
    }

    private static Node Parse(List<string> tokens)
    {
        var orList = new List<List<Node>>();
        var result = new List<Node>();

        Node WrapResult()
        {
            if (orList.Count == 0)
            {
                return Wrap(result);
            }
            else
            {
                orList.Add(result);
                return new AnyNode(orList.Select(Wrap).ToList());
            }
        }

        while (tokens.Count > 0)
        {
            var t = Pop(tokens);

            if (t.StartsWith("(?P<"))
            {
                // Named variable: (?P<name>...)
                var varName = t[4..^1]; // Extract name from (?P<name>
                result.Add(new Variable(Parse(tokens), varName));
            }
            else if (t is "*" or "*?")
            {
                var greedy = t == "*";
                if (result.Count == 0)
                {
                    throw new ArgumentException("Nothing to repeat.");
                }
                result[^1] = new Repeat(result[^1], greedy: greedy);
            }
            else if (t is "+" or "+?")
            {
                var greedy = t == "+";
                if (result.Count == 0)
                {
                    throw new ArgumentException("Nothing to repeat.");
                }
                result[^1] = new Repeat(result[^1], minRepeat: 1, greedy: greedy);
            }
            else if (t is "?" or "??")
            {
                if (result.Count == 0)
                {
                    throw new ArgumentException("Nothing to repeat.");
                }
                var greedy = t == "?";
                result[^1] = new Repeat(result[^1], minRepeat: 0, maxRepeat: 1, greedy: greedy);
            }
            else if (t == "|")
            {
                orList.Add(result);
                result = new List<Node>();
            }
            else if (t is "(" or "(?:")
            {
                result.Add(Parse(tokens));
            }
            else if (t == "(?!")
            {
                result.Add(new Lookahead(Parse(tokens), negative: true));
            }
            else if (t == "(?=")
            {
                throw new NotSupportedException("Positive lookahead not yet supported.");
            }
            else if (t == ")")
            {
                return WrapResult();
            }
            else if (t.StartsWith('#'))
            {
                // Comment - skip
            }
            else if (t.StartsWith('{'))
            {
                throw new NotSupportedException($"{t}-style repetition not yet supported");
            }
            else if (t.StartsWith("(?"))
            {
                throw new NotSupportedException($"'{t}' not supported");
            }
            else if (string.IsNullOrWhiteSpace(t))
            {
                // Whitespace - skip
            }
            else
            {
                result.Add(new RegexNode(t));
            }
        }

        throw new ArgumentException("Expecting ')' token");
    }

    private static Node Wrap(IReadOnlyList<Node> nodes)
    {
        return nodes.Count == 1 ? nodes[0] : new NodeSequence(nodes);
    }

    private static string Pop(List<string> tokens)
    {
        var last = tokens[^1];
        tokens.RemoveAt(tokens.Count - 1);
        return last;
    }
}
