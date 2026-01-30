using Stroke.Core;
using Stroke.FormattedText;
using AppContext = Stroke.Application.AppContext;

namespace Stroke.Layout.Processors;

/// <summary>
/// Highlights matching bracket pairs when cursor is on or after a bracket.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>HighlightMatchingBracketProcessor</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </remarks>
public sealed class HighlightMatchingBracketProcessor : IProcessor
{
    private static readonly string ClosingBraces = "])}>";

    private readonly SimpleCache<object, List<(int Row, int Col)>> _positionsCache = new(8);

    /// <summary>
    /// Initializes a new instance of the <see cref="HighlightMatchingBracketProcessor"/> class.
    /// </summary>
    /// <param name="chars">Bracket characters to match. Default "[](){}&lt;&gt;".</param>
    /// <param name="maxCursorDistance">Maximum search distance from cursor. Default 1000.</param>
    public HighlightMatchingBracketProcessor(
        string chars = "[](){}<>",
        int maxCursorDistance = 1000)
    {
        Chars = chars;
        MaxCursorDistance = maxCursorDistance;
    }

    /// <summary>Bracket characters to match.</summary>
    public string Chars { get; }

    /// <summary>Maximum search distance from cursor.</summary>
    public int MaxCursorDistance { get; }

    private List<(int Row, int Col)> GetPositionsToHighlight(Document document)
    {
        int? pos = null;
        var doc = document;

        // Try for the character under the cursor.
        if (doc.CurrentChar != '\0' && Chars.Contains(doc.CurrentChar))
        {
            pos = doc.FindMatchingBracketPosition(
                startPos: doc.CursorPosition - MaxCursorDistance,
                endPos: doc.CursorPosition + MaxCursorDistance);
        }
        // Try for the character before the cursor.
        else if (doc.CharBeforeCursor != '\0'
            && ClosingBraces.Contains(doc.CharBeforeCursor)
            && Chars.Contains(doc.CharBeforeCursor))
        {
            doc = new Document(doc.Text, doc.CursorPosition - 1);

            pos = doc.FindMatchingBracketPosition(
                startPos: doc.CursorPosition - MaxCursorDistance,
                endPos: doc.CursorPosition + MaxCursorDistance);
        }

        // Return a list of (row, col) tuples that need to be highlighted.
        if (pos is not null && pos != 0)
        {
            var absPos = pos.Value + doc.CursorPosition; // pos is relative
            var (row, col) = doc.TranslateIndexToPosition(absPos);
            return
            [
                (row, col),
                (doc.CursorPositionRow, doc.CursorPositionCol),
            ];
        }

        return [];
    }

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput transformationInput)
    {
        var (bufferControl, document, lineNumber, sourceToDisplay, fragments, _, _) =
            transformationInput.Unpack();

        // When the application is in the 'done' state, don't highlight.
        if (AppContext.GetApp().IsDone)
        {
            return new Transformation(fragments);
        }

        // Get the highlight positions.
        var key = (object)(AppContext.GetApp().RenderCounter, document.Text, document.CursorPosition);
        var positions = _positionsCache.Get(
            key, () => GetPositionsToHighlight(document));

        // Apply if positions were found at this line.
        if (positions.Count > 0)
        {
            IReadOnlyList<StyleAndTextTuple> result = fragments;

            foreach (var (row, col) in positions)
            {
                if (row == lineNumber)
                {
                    var displayCol = sourceToDisplay(col);
                    var exploded = LayoutUtils.ExplodeTextFragments(result);

                    if (displayCol < exploded.Count)
                    {
                        var oldFragment = exploded[displayCol];
                        var suffix = col == document.CursorPositionCol
                            ? " class:matching-bracket.cursor "
                            : " class:matching-bracket.other ";

                        exploded[displayCol] = new StyleAndTextTuple(
                            oldFragment.Style + suffix,
                            oldFragment.Text,
                            oldFragment.MouseHandler);
                    }

                    result = exploded;
                }
            }

            return new Transformation(result);
        }

        return new Transformation(fragments);
    }
}
