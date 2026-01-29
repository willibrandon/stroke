using Stroke.Core;
using Stroke.FormattedText;

namespace Stroke.Lexers;

/// <summary>
/// Lexer that can dynamically return any Lexer at runtime.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DynamicLexer</c> class
/// from <c>prompt_toolkit.lexers.base</c>.
/// </para>
/// <para>
/// The callback is invoked once per <see cref="LexDocument"/> call. If the callback
/// returns <c>null</c>, an internal <see cref="SimpleLexer"/> with empty style is used.
/// </para>
/// <para>
/// The callback invocation is not synchronized. If the callback accesses shared state,
/// the caller is responsible for thread safety within the callback.
/// </para>
/// </remarks>
public sealed class DynamicLexer : ILexer
{
    private readonly Func<ILexer?> _getLexer;
    private readonly SimpleLexer _fallback = new SimpleLexer("");

    /// <summary>
    /// Initializes a new instance with the specified lexer callback.
    /// </summary>
    /// <param name="getLexer">
    /// Callback that returns the lexer to use. May return <c>null</c> to use fallback.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getLexer"/> is <c>null</c>.</exception>
    public DynamicLexer(Func<ILexer?> getLexer)
    {
        ArgumentNullException.ThrowIfNull(getLexer);
        _getLexer = getLexer;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Invokes the callback to get the current lexer, then delegates to that lexer's
    /// <see cref="ILexer.LexDocument"/> method. If callback returns <c>null</c>,
    /// uses the internal fallback <see cref="SimpleLexer"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <c>null</c>.</exception>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var lexer = _getLexer() ?? _fallback;
        return lexer.LexDocument(document);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns the <see cref="ILexer.InvalidationHash"/> of the currently active lexer
    /// (from callback or fallback). This allows cache invalidation when the active lexer changes.
    /// </remarks>
    public object InvalidationHash()
    {
        var lexer = _getLexer() ?? _fallback;
        return lexer.InvalidationHash();
    }
}
