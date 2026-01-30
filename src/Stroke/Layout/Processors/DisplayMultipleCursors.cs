using Stroke.FormattedText;
using AppContext = Stroke.Application.AppContext;

namespace Stroke.Layout.Processors;

/// <summary>
/// Displays all cursors when in Vi block insert mode.
/// Applies "multiple-cursors" style class.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>DisplayMultipleCursors</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </remarks>
public sealed class DisplayMultipleCursors : IProcessor
{
    private const string FragmentSuffix = " class:multiple-cursors";

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput transformationInput)
    {
        var (bufferControl, document, lineNumber, sourceToDisplay, fragments, _, _) =
            transformationInput.Unpack();

        var buff = bufferControl.Buffer;

        if (Stroke.Application.ViFilters.ViInsertMultipleMode.Invoke())
        {
            var cursorPositions = buff.MultipleCursorPositions;
            var exploded = LayoutUtils.ExplodeTextFragments(fragments);

            // If any cursor appears on the current line, highlight that.
            var startPos = document.TranslateRowColToIndex(lineNumber, 0);
            var endPos = startPos + document.Lines[lineNumber].Length;

            foreach (var p in cursorPositions)
            {
                if (startPos <= p && p <= endPos)
                {
                    var column = sourceToDisplay(p - startPos);

                    // Replace fragment.
                    if (column < exploded.Count)
                    {
                        var oldFragment = exploded[column];
                        exploded[column] = new StyleAndTextTuple(
                            oldFragment.Style + FragmentSuffix,
                            oldFragment.Text,
                            oldFragment.MouseHandler);
                    }
                    else
                    {
                        // Cursor needs to be displayed after the current text.
                        exploded.Add(new StyleAndTextTuple(FragmentSuffix, " "));
                    }
                }
            }

            return new Transformation(exploded);
        }
        else
        {
            return new Transformation(fragments);
        }
    }
}
