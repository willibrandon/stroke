using Stroke.FormattedText;

namespace Stroke.Layout.Processors;

/// <summary>
/// Insert text after the input (on the last line only).
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>AfterInput</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </remarks>
public sealed class AfterInput : IProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AfterInput"/> class.
    /// </summary>
    /// <param name="text">This can be either plain text or formatted text
    /// (or a callable that returns any of those).</param>
    /// <param name="style">Style to be applied to this prompt/suffix.</param>
    public AfterInput(AnyFormattedText text, string style = "")
    {
        Text = text;
        Style = style;
    }

    /// <summary>The text to append.</summary>
    public AnyFormattedText Text { get; }

    /// <summary>Style to apply.</summary>
    public string Style { get; }

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput ti)
    {
        // Insert fragments after the last line.
        if (ti.LineNumber == ti.Document.LineCount - 1)
        {
            // Get fragments.
            var fragmentsAfter = FormattedTextUtils.ToFormattedText(Text, Style);
            var fragments = new List<StyleAndTextTuple>(ti.Fragments);
            fragments.AddRange(fragmentsAfter);
            return new Transformation(fragments);
        }
        else
        {
            return new Transformation(ti.Fragments);
        }
    }

    /// <summary>Returns "AfterInput({Text}, style={Style})".</summary>
    public override string ToString()
    {
        return $"AfterInput({Text}, style={Style})";
    }
}
