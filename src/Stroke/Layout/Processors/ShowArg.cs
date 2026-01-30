using Stroke.FormattedText;
using AppContext = Stroke.Application.AppContext;

namespace Stroke.Layout.Processors;

/// <summary>
/// Display the 'arg' (repeat count) in front of the input.
/// </summary>
/// <remarks>
/// <para>
/// This was used by the <c>PromptSession</c>, but now it uses the
/// <c>Window.GetLinePrefix</c> function instead.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ShowArg</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </para>
/// </remarks>
public sealed class ShowArg : BeforeInput
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowArg"/> class.
    /// </summary>
    public ShowArg() : base((Func<AnyFormattedText>)GetTextFragments)
    {
    }

    private static AnyFormattedText GetTextFragments()
    {
        var app = AppContext.GetApp();
        if (app.KeyProcessor.Arg is null)
        {
            return new FormattedText.FormattedText([]);
        }
        else
        {
            var arg = app.KeyProcessor.Arg;
            return new FormattedText.FormattedText(
            [
                new StyleAndTextTuple("class:prompt.arg", "(arg: "),
                new StyleAndTextTuple("class:prompt.arg.text", arg),
                new StyleAndTextTuple("class:prompt.arg", ") "),
            ]);
        }
    }

    /// <summary>Returns "ShowArg()".</summary>
    public override string ToString()
    {
        return "ShowArg()";
    }
}
