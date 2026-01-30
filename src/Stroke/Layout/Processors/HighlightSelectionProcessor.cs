using Stroke.FormattedText;

namespace Stroke.Layout.Processors;

/// <summary>
/// Processor that highlights the selection in the document.
/// Applies "selected" style class.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>HighlightSelectionProcessor</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </remarks>
public sealed class HighlightSelectionProcessor : IProcessor
{
    private const string SelectedFragment = " class:selected ";

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput transformationInput)
    {
        var (bufferControl, document, lineNumber, sourceToDisplay, fragments, _, _) =
            transformationInput.Unpack();

        // In case of selection, highlight all matches.
        var selectionAtLine = document.SelectionRangeAtLine(lineNumber);

        if (selectionAtLine is not null)
        {
            var (from, to) = selectionAtLine.Value;
            from = sourceToDisplay(from);
            to = sourceToDisplay(to);

            var exploded = LayoutUtils.ExplodeTextFragments(fragments);

            if (from == 0 && to == 0 && exploded.Count == 0)
            {
                // When this is an empty line, insert a space in order to
                // visualize the selection.
                return new Transformation(
                    new List<StyleAndTextTuple> { new(SelectedFragment, " ") });
            }
            else
            {
                for (int i = from; i < to; i++)
                {
                    if (i < exploded.Count)
                    {
                        var oldFragment = exploded[i];
                        exploded[i] = new StyleAndTextTuple(
                            oldFragment.Style + SelectedFragment,
                            oldFragment.Text,
                            oldFragment.MouseHandler);
                    }
                    else if (i == exploded.Count)
                    {
                        exploded.Add(new StyleAndTextTuple(SelectedFragment, " "));
                    }
                }

                return new Transformation(exploded);
            }
        }

        return new Transformation(fragments);
    }
}
