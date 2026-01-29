using Stroke.Core;
using Stroke.FormattedText;

namespace Stroke.Lexers;

/// <summary>
/// Lexer that doesn't do any tokenizing and returns the whole input as one token.
/// </summary>
/// <remarks>
/// <para>
/// This is the simplest lexer implementation, applying a single style to all text
/// without any syntax analysis. It's used as a fallback when no specific lexer
/// is available or needed.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SimpleLexer</c> class
/// from <c>prompt_toolkit.lexers.base</c>.
/// </para>
/// <para>
/// This type is thread-safe. It is immutable after construction.
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="SimpleLexer"/> class.
/// </remarks>
/// <param name="style">
/// The style string to apply to all text. Defaults to empty string.
/// If <c>null</c> is passed, it is treated as empty string <c>""</c>.
/// </param>
public sealed class SimpleLexer(string style = "") : ILexer
{
    /// <summary>
    /// Gets the style string applied to all text.
    /// </summary>
    public string Style { get; } = style ?? "";

    /// <inheritdoc/>
    /// <remarks>
    /// Returns a function that:
    /// <list type="bullet">
    ///   <item>For valid line numbers: returns a single-element list with (Style, lineText)</item>
    ///   <item>For invalid line numbers: returns an empty list</item>
    /// </list>
    /// </remarks>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var lines = document.Lines;
        var style = Style;

        return lineNo =>
        {
            if (lineNo < 0 || lineNo >= lines.Count)
                return [];

            return new[] { new StyleAndTextTuple(style, lines[lineNo]) };
        };
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns <c>this</c> instance, as the lexer output only changes if the instance changes.
    /// </remarks>
    public object InvalidationHash() => this;
}
