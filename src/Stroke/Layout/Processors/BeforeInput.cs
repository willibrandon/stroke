using Stroke.FormattedText;

namespace Stroke.Layout.Processors;

/// <summary>
/// Insert text before the input (on line 0 only).
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>BeforeInput</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </remarks>
public class BeforeInput : IProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BeforeInput"/> class.
    /// </summary>
    /// <param name="text">This can be either plain text or formatted text
    /// (or a callable that returns any of those).</param>
    /// <param name="style">Style to be applied to this prompt/prefix.</param>
    public BeforeInput(AnyFormattedText text, string style = "")
    {
        Text = text;
        Style = style;
    }

    /// <summary>The text to prepend.</summary>
    public AnyFormattedText Text { get; }

    /// <summary>Style to apply.</summary>
    public string Style { get; }

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput ti)
    {
        if (ti.LineNumber == 0)
        {
            // Get fragments.
            var fragmentsBefore = FormattedTextUtils.ToFormattedText(Text, Style);
            var fragments = new List<StyleAndTextTuple>(fragmentsBefore);
            fragments.AddRange(ti.Fragments);

            var shiftPosition = FormattedTextUtils.FragmentListLen(fragmentsBefore);
            return new Transformation(
                fragments,
                sourceToDisplay: i => i + shiftPosition,
                displayToSource: i => i - shiftPosition);
        }
        else
        {
            return new Transformation(ti.Fragments);
        }
    }

    /// <summary>Returns "BeforeInput({Text}, style={Style})".</summary>
    public override string ToString()
    {
        return $"BeforeInput({Text}, style={Style})";
    }
}
