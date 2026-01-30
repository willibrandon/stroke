using Stroke.FormattedText;

namespace Stroke.Layout.Processors;

/// <summary>
/// Processor that masks the input for passwords.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>PasswordProcessor</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </remarks>
public sealed class PasswordProcessor : IProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordProcessor"/> class.
    /// </summary>
    /// <param name="char">The mask character. Defaults to "*".</param>
    public PasswordProcessor(string @char = "*")
    {
        Char = @char;
    }

    /// <summary>The mask character.</summary>
    public string Char { get; }

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput ti)
    {
        var fragments = new List<StyleAndTextTuple>(ti.Fragments.Count);

        foreach (var fragment in ti.Fragments)
        {
            // Replace each character with the mask character, preserving style and handler
            var maskedText = string.Concat(Enumerable.Repeat(Char, fragment.Text.Length));
            fragments.Add(new StyleAndTextTuple(fragment.Style, maskedText, fragment.MouseHandler));
        }

        return new Transformation(fragments);
    }
}
