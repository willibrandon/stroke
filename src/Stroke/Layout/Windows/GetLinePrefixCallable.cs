using Stroke.FormattedText;

namespace Stroke.Layout.Windows;

/// <summary>
/// Delegate for getting line prefixes (continuation prompts).
/// </summary>
/// <param name="lineNumber">The line number in the document (0-based).</param>
/// <param name="wrapCount">Number of times this line has wrapped (0 for first segment).</param>
/// <returns>Styled text fragments for the line prefix.</returns>
/// <remarks>
/// <para>
/// Used by Window to render continuation prompts for wrapped lines.
/// The line prefix appears before each physical line of wrapped content.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>GetLinePrefixCallable</c> type alias.
/// </para>
/// </remarks>
public delegate IReadOnlyList<StyleAndTextTuple> GetLinePrefixCallable(
    int lineNumber,
    int wrapCount);
