using Stroke.FormattedText;
using AppContext = Stroke.Application.AppContext;

namespace Stroke.Layout.Processors;

/// <summary>
/// Make leading whitespace visible by replacing spaces with a visible character.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>ShowLeadingWhiteSpaceProcessor</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </remarks>
public sealed class ShowLeadingWhiteSpaceProcessor : IProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowLeadingWhiteSpaceProcessor"/> class.
    /// </summary>
    /// <param name="getChar">Callable that returns one character. If null, uses encoding-aware default.</param>
    /// <param name="style">Style for replacement characters.</param>
    public ShowLeadingWhiteSpaceProcessor(
        Func<string>? getChar = null,
        string style = "class:leading-whitespace")
    {
        Style = style;
        GetChar = getChar ?? DefaultGetChar;
    }

    /// <summary>Style for replacement characters.</summary>
    public string Style { get; }

    /// <summary>Callable returning the visible replacement character.</summary>
    public Func<string> GetChar { get; }

    private static string DefaultGetChar()
    {
        try
        {
            var encoding = System.Text.Encoding.GetEncoding(
                AppContext.GetApp().Output.Encoding);
            var bytes = encoding.GetBytes("\u00b7");
            var roundTripped = encoding.GetString(bytes);
            if (roundTripped == "?")
                return ".";
        }
        catch
        {
            // If encoding check fails, fall back to middot for UTF-8 environments
        }
        return "\u00b7";
    }

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput ti)
    {
        var fragments = ti.Fragments;

        // Walk through all the fragments.
        if (fragments.Count > 0 && FormattedTextUtils.FragmentListToText(fragments).StartsWith(' '))
        {
            var replacement = new StyleAndTextTuple(Style, GetChar());
            var exploded = LayoutUtils.ExplodeTextFragments(fragments);

            for (int i = 0; i < exploded.Count; i++)
            {
                if (exploded[i].Text == " ")
                {
                    exploded[i] = replacement;
                }
                else
                {
                    break;
                }
            }

            return new Transformation(exploded);
        }

        return new Transformation(fragments);
    }
}
