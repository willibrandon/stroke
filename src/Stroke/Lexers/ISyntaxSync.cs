using Stroke.Core;

namespace Stroke.Lexers;

/// <summary>
/// Syntax synchronizer for finding a safe start position for lexing.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SyntaxSync</c> abstract class
/// from <c>prompt_toolkit.lexers.pygments</c>. In C#, interfaces are preferred over
/// abstract classes when there is no shared implementation.
/// </para>
/// <para>
/// Implementations determine where lexing should start to produce correct results for
/// a given line. This is critical for performance with large documents - starting from
/// a safe position near the target line avoids re-lexing the entire document.
/// </para>
/// </remarks>
public interface ISyntaxSync
{
    /// <summary>
    /// Returns the position from where lexing can safely start.
    /// </summary>
    /// <param name="document">The document being lexed.</param>
    /// <param name="lineNo">The target line number (0-based) we want to highlight.</param>
    /// <returns>
    /// A tuple (Row, Column) indicating where lexing should start:
    /// <list type="bullet">
    ///   <item>Row: Line number (0-based) to start from. Must be ≤ <paramref name="lineNo"/>.</item>
    ///   <item>Column: Character offset (0-based) within that line to start from.</item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <c>null</c>.</exception>
    /// <remarks>
    /// A "safe" starting position is one from which lexing will produce correct syntax
    /// highlighting for the target line. Examples: start of document, function definition,
    /// class declaration, or tag boundary.
    /// </remarks>
    (int Row, int Column) GetSyncStartPosition(Document document, int lineNo);
}
