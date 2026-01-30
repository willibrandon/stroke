using System.Text.RegularExpressions;
using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using AppContext = Stroke.Application.AppContext;

namespace Stroke.Layout.Processors;

/// <summary>
/// Processor that highlights search matches in the document.
/// Applies "search" and "search.current" style classes.
/// </summary>
/// <remarks>
/// <para>
/// Note that this doesn't support multiline search matches yet.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>HighlightSearchProcessor</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </para>
/// </remarks>
public class HighlightSearchProcessor : IProcessor
{
    /// <summary>Style class for matches (default "search").</summary>
    protected virtual string ClassName => "search";

    /// <summary>Style class for current match (default "search.current").</summary>
    protected virtual string ClassNameCurrent => "search.current";

    /// <summary>
    /// Get the search text for this processor.
    /// </summary>
    /// <param name="bufferControl">The buffer control to get search text from.</param>
    /// <returns>The search text.</returns>
    protected virtual string GetSearchText(BufferControl bufferControl)
    {
        return bufferControl.SearchState.Text;
    }

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput transformationInput)
    {
        var (bufferControl, document, lineNumber, sourceToDisplay, fragments, _, _) =
            transformationInput.Unpack();

        var searchText = GetSearchText(bufferControl);
        var searchMatchFragment = $" class:{ClassName} ";
        var searchMatchCurrentFragment = $" class:{ClassNameCurrent} ";

        if (!string.IsNullOrEmpty(searchText) && !AppContext.GetApp().IsDone)
        {
            // For each search match, replace the style string.
            var lineText = FormattedTextUtils.FragmentListToText(fragments);
            var exploded = LayoutUtils.ExplodeTextFragments(fragments);

            var options = bufferControl.SearchState.IgnoreCase()
                ? RegexOptions.IgnoreCase
                : RegexOptions.None;

            // Get cursor column.
            int? cursorColumn = null;
            if (document.CursorPositionRow == lineNumber)
            {
                cursorColumn = sourceToDisplay(document.CursorPositionCol);
            }

            foreach (Match match in Regex.Matches(lineText, Regex.Escape(searchText), options))
            {
                bool onCursor = cursorColumn is not null &&
                    match.Index <= cursorColumn.Value && cursorColumn.Value < match.Index + match.Length;

                for (int i = match.Index; i < match.Index + match.Length; i++)
                {
                    if (i < exploded.Count)
                    {
                        var oldFragment = exploded[i];
                        var suffix = onCursor ? searchMatchCurrentFragment : searchMatchFragment;
                        exploded[i] = new StyleAndTextTuple(
                            oldFragment.Style + suffix,
                            oldFragment.Text,
                            oldFragment.MouseHandler);
                    }
                }
            }

            return new Transformation(exploded);
        }

        return new Transformation(fragments);
    }
}
