namespace Stroke.Lexers;

/// <summary>
/// Interface for Pygments-compatible lexer implementations.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines the contract for external lexer implementations that can be
/// used with the PygmentsLexer class. Implementations should tokenize source code
/// and return token information in a format compatible with Pygments.
/// </para>
/// <para>
/// External packages (e.g., TextMateSharp adapters) implement this interface to provide
/// actual syntax highlighting functionality.
/// </para>
/// <para>
/// This is a faithful port of the implicit interface used by Python Prompt Toolkit's
/// <c>PygmentsLexer</c> when interacting with Pygments lexer classes.
/// </para>
/// <para>
/// Implementations MUST be thread-safe for concurrent <see cref="GetTokensUnprocessed"/> calls.
/// </para>
/// </remarks>
public interface IPygmentsLexer
{
    /// <summary>
    /// Gets the name of the lexer (e.g., "Python", "JavaScript", "HTML").
    /// </summary>
    /// <remarks>
    /// <para>
    /// This name is used by <see cref="RegexSync.ForLanguage"/> to determine
    /// an appropriate synchronization pattern.
    /// </para>
    /// <para>
    /// Must not be <c>null</c> or empty.
    /// </para>
    /// </remarks>
    string Name { get; }

    /// <summary>
    /// Tokenizes the given text and yields token information.
    /// </summary>
    /// <param name="text">The source text to tokenize. May be <c>null</c> or empty.</param>
    /// <returns>
    /// An enumerable of tuples containing:
    /// <list type="bullet">
    ///   <item><c>Index</c>: The character offset where the token starts (0-based)</item>
    ///   <item><c>TokenType</c>: The token type as a path (e.g., ["Name", "Exception"])</item>
    ///   <item><c>Text</c>: The actual text of the token</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Tokens must be yielded in order by index. The sum of all token text lengths
    /// should equal the input text length.
    /// </para>
    /// <para>
    /// For <c>null</c> input: May throw <see cref="ArgumentNullException"/> or return empty.
    /// For empty input: Returns empty enumerable.
    /// </para>
    /// <para>
    /// Token types follow the Pygments hierarchy:
    /// <list type="bullet">
    ///   <item><c>["Keyword"]</c> → class:pygments.keyword</item>
    ///   <item><c>["Name", "Function"]</c> → class:pygments.name.function</item>
    ///   <item><c>["String", "Double"]</c> → class:pygments.string.double</item>
    /// </list>
    /// </para>
    /// </remarks>
    IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)> GetTokensUnprocessed(string text);
}
