using Stroke.FormattedText;
using AppContext = Stroke.Application.AppContext;

namespace Stroke.Layout.Processors;

/// <summary>
/// Make trailing whitespace visible by replacing spaces with a visible character.
/// </summary>
/// <remarks>
/// <para>
/// Deviation: Python uses "class:training-whitespace" (typo). C# uses
/// "class:trailing-whitespace" (corrected per clarification session 2026-01-29).
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ShowTrailingWhiteSpaceProcessor</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </para>
/// </remarks>
public sealed class ShowTrailingWhiteSpaceProcessor : IProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowTrailingWhiteSpaceProcessor"/> class.
    /// </summary>
    /// <param name="getChar">Callable that returns one character. If null, uses encoding-aware default.</param>
    /// <param name="style">Style for replacement characters.</param>
    public ShowTrailingWhiteSpaceProcessor(
        Func<string>? getChar = null,
        string style = "class:trailing-whitespace")
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

        if (fragments.Count > 0 && fragments[^1].Text.EndsWith(' '))
        {
            var replacement = new StyleAndTextTuple(Style, GetChar());
            var exploded = LayoutUtils.ExplodeTextFragments(fragments);

            // Walk backwards through all the fragments and replace whitespace.
            for (int i = exploded.Count - 1; i >= 0; i--)
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
