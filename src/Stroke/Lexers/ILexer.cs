using Stroke.Core;
using Stroke.FormattedText;

namespace Stroke.Lexers;

/// <summary>
/// Base interface for all lexers.
/// </summary>
/// <remarks>
/// <para>
/// A lexer takes a Document and returns a function that maps line numbers to styled text fragments.
/// This enables lazy, on-demand lexing of individual lines.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Lexer</c> abstract base class
/// from <c>prompt_toolkit.lexers.base</c>.
/// </para>
/// <para>
/// Implementations must be thread-safe per Constitution XI.
/// </para>
/// </remarks>
public interface ILexer
{
    /// <summary>
    /// Takes a <see cref="Document"/> and returns a callable that takes a line number
    /// and returns a list of (style_str, text) tuples for that line.
    /// </summary>
    /// <param name="document">The document to lex.</param>
    /// <returns>
    /// A function that accepts a line number (0-based) and returns the styled tokens for that line.
    /// For invalid line numbers (negative or beyond document bounds), returns an empty list.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <c>null</c>.</exception>
    Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document);

    /// <summary>
    /// When this changes, <see cref="LexDocument"/> could give a different output.
    /// </summary>
    /// <returns>
    /// A value that changes when the lexer's output may change.
    /// Used for cache invalidation in dynamic lexers.
    /// </returns>
    /// <remarks>
    /// <para>
    /// For stateless lexers, this typically returns the lexer instance itself
    /// (identity-based comparison).
    /// </para>
    /// <para>
    /// For dynamic lexers, this returns the hash of the currently active lexer.
    /// </para>
    /// </remarks>
    object InvalidationHash();
}
