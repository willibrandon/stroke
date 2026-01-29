namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Static class for compiling grammar expressions.
/// </summary>
public static class Grammar
{
    /// <summary>
    /// Compile a regular expression grammar into a CompiledGrammar.
    /// </summary>
    /// <param name="expression">
    /// Regular expression with Python-style named groups (?P&lt;varname&gt;...).
    /// Whitespace is ignored (like Python's re.VERBOSE).
    /// Comments starting with # are stripped.
    /// </param>
    /// <param name="escapeFuncs">
    /// Optional dictionary mapping variable names to escape functions.
    /// Used when inserting completions back into the input.
    /// </param>
    /// <param name="unescapeFuncs">
    /// Optional dictionary mapping variable names to unescape functions.
    /// Used when extracting variable values for validation/completion.
    /// </param>
    /// <returns>A compiled grammar that can be used for matching, completion, lexing, and validation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="expression"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the expression is syntactically invalid.</exception>
    /// <exception cref="NotSupportedException">
    /// Thrown for unsupported features like positive lookahead <c>(?=...)</c> or <c>{n,m}</c> repetition.
    /// </exception>
    /// <example>
    /// <code>
    /// var grammar = Grammar.Compile(@"
    ///     \s*
    ///     (
    ///         pwd |
    ///         ls |
    ///         (cd \s+ (?P&lt;directory&gt;[^\s]+)) |
    ///         (cat \s+ (?P&lt;filename&gt;[^\s]+))
    ///     )
    ///     \s*
    /// ");
    /// </code>
    /// </example>
    public static CompiledGrammar Compile(
        string expression,
        IDictionary<string, Func<string, string>>? escapeFuncs = null,
        IDictionary<string, Func<string, string>>? unescapeFuncs = null)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var tokens = RegexParser.TokenizeRegex(expression);
        var rootNode = RegexParser.ParseRegex(tokens);

        return new CompiledGrammar(rootNode, escapeFuncs, unescapeFuncs);
    }
}
