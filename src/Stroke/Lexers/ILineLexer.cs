namespace Stroke.Lexers;

/// <summary>
/// Interface for line-by-line lexer implementations.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="IPygmentsLexer"/> which tokenizes an entire document at once,
/// <see cref="ILineLexer"/> tokenizes one line at a time, carrying opaque state between
/// lines for correct multi-line construct handling (strings, comments, etc.).
/// </para>
/// <para>
/// This interface is optimized for lexer engines that naturally operate line-by-line,
/// such as TextMate grammar engines. Use <see cref="LineLexer"/> to adapt an
/// <see cref="ILineLexer"/> to the <see cref="ILexer"/> interface expected by
/// <see cref="Layout.Controls.BufferControl"/>.
/// </para>
/// <para>
/// Implementations MUST be thread-safe for concurrent <see cref="TokenizeLine"/> calls
/// with independent state objects.
/// </para>
/// </remarks>
public interface ILineLexer
{
    /// <summary>
    /// Gets the name of the lexer (e.g., "C#", "Python", "JSON").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Tokenizes a single line of text.
    /// </summary>
    /// <param name="line">The line text to tokenize (without trailing newline).</param>
    /// <param name="prevState">
    /// The state from the previous line's <see cref="LineLexResult.State"/>,
    /// or <c>null</c> for the first line of a document.
    /// </param>
    /// <returns>
    /// A <see cref="LineLexResult"/> containing the tokens and the state to pass
    /// to the next line.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="line"/> is <c>null</c>.</exception>
    LineLexResult TokenizeLine(string line, object? prevState);
}

/// <summary>
/// Result of tokenizing a single line via <see cref="ILineLexer.TokenizeLine"/>.
/// </summary>
/// <param name="Tokens">
/// The tokens found in the line. Each token contains:
/// <list type="bullet">
///   <item><c>Index</c>: Character offset where the token starts (0-based within the line)</item>
///   <item><c>TokenType</c>: Pygments-compatible token type path (e.g., ["Keyword"], ["Name", "Function"])</item>
///   <item><c>Text</c>: The actual text of the token</item>
/// </list>
/// </param>
/// <param name="State">
/// Opaque state to pass as <c>prevState</c> to the next line's tokenization.
/// This captures parsing context needed for multi-line constructs (strings, comments, etc.).
/// </param>
public sealed record LineLexResult(
    IReadOnlyList<(int Index, IReadOnlyList<string> TokenType, string Text)> Tokens,
    object? State);
