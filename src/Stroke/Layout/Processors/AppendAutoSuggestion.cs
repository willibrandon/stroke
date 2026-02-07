using Stroke.FormattedText;

namespace Stroke.Layout.Processors;

/// <summary>
/// Append the auto suggestion to the input (on the last line only).
/// The user can then press the right arrow to insert the suggestion.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>AppendAutoSuggestion</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </remarks>
public sealed class AppendAutoSuggestion : IProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppendAutoSuggestion"/> class.
    /// </summary>
    /// <param name="style">Style for suggestion text. Defaults to "class:auto-suggestion".</param>
    public AppendAutoSuggestion(string style = "class:auto-suggestion")
    {
        Style = style;
    }

    /// <summary>Style for suggestion text.</summary>
    public string Style { get; }

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput ti)
    {
        // Insert fragments after the last line.
        if (ti.LineNumber == ti.Document.LineCount - 1)
        {
            var buffer = ti.BufferControl.Buffer;

            string suggestion;
            if (buffer.Suggestion is not null && ti.Document.IsCursorAtTheEnd)
            {
                var text = buffer.Suggestion.Text;

                // When the document is multiline and the suggestion is also
                // multiline, step aside entirely — a custom multiline processor
                // (e.g. AppendMultilineAutoSuggestionInAnyLine) owns rendering.
                if (ti.Document.LineCount > 1 && text.Contains('\n'))
                    suggestion = "";
                else
                    suggestion = text;
            }
            else
            {
                suggestion = "";
            }

            var fragments = new List<StyleAndTextTuple>(ti.Fragments)
            {
                new(Style, suggestion)
            };
            return new Transformation(fragments);
        }
        else
        {
            return new Transformation(ti.Fragments);
        }
    }
}
