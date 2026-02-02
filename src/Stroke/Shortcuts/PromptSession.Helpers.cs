using Stroke.Application;
using Stroke.Filters;
using Stroke.FormattedText;

using Buffer = Stroke.Core.Buffer;

namespace Stroke.Shortcuts;

public partial class PromptSession<TResult>
{
    /// <summary>
    /// Gets the prompt text as formatted fragments.
    /// </summary>
    /// <returns>Formatted text for the prompt message.</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_get_prompt</c>.
    /// </remarks>
    private IReadOnlyList<StyleAndTextTuple> GetPrompt()
    {
        return FormattedTextUtils.ToFormattedText(Message, style: "class:prompt");
    }

    /// <summary>
    /// Gets the continuation text for multiline prompts.
    /// </summary>
    /// <param name="width">Width of the first-line prompt.</param>
    /// <param name="lineNumber">Current line number (0-based).</param>
    /// <param name="wrapCount">Number of times the current line has wrapped.</param>
    /// <returns>Formatted continuation text.</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_get_continuation</c>.
    /// </remarks>
    private IReadOnlyList<StyleAndTextTuple> GetContinuation(
        int width, int lineNumber, int wrapCount)
    {
        var promptContinuation = PromptContinuation;

        AnyFormattedText continuation;
        if (promptContinuation is PromptContinuationCallable callable)
        {
            continuation = callable(width, lineNumber, wrapCount);
        }
        else
        {
            continuation = promptContinuation switch
            {
                string s => (AnyFormattedText)s,
                FormattedText.FormattedText ft => (AnyFormattedText)ft,
                not null => (AnyFormattedText)promptContinuation.ToString(),
                _ => default,
            };
        }

        // When no continuation provided and multiline, use spaces matching prompt width
        if (continuation.Value is null && FilterUtils.ToFilter(Multiline).Invoke())
        {
            continuation = new string(' ', width);
        }

        return FormattedTextUtils.ToFormattedText(continuation, style: "class:prompt-continuation");
    }

    /// <summary>
    /// Returns the line prefix for each line in the input.
    /// First line gets the prompt or arg display; subsequent lines get continuation text.
    /// </summary>
    /// <param name="lineNumber">The line number (0-based).</param>
    /// <param name="wrapCount">Number of wraps for this line.</param>
    /// <param name="getPromptText2">Function returning the first input line prompt fragments.</param>
    /// <returns>Formatted text prefix for the line.</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_get_line_prefix</c>.
    /// </remarks>
    private IReadOnlyList<StyleAndTextTuple> GetLinePrefix(
        int lineNumber,
        int wrapCount,
        Func<IReadOnlyList<StyleAndTextTuple>> getPromptText2)
    {
        // First line: display the "arg" or the prompt
        if (lineNumber == 0 && wrapCount == 0)
        {
            if (!FilterUtils.ToFilter(Multiline).Invoke()
                && App.KeyProcessor.Arg is not null)
            {
                return InlineArg();
            }
            else
            {
                return getPromptText2();
            }
        }

        // For subsequent lines, display continuation text
        var promptWidth = FormattedTextUtils.FragmentListWidth(getPromptText2());
        return GetContinuation(promptWidth, lineNumber, wrapCount);
    }

    /// <summary>
    /// Gets the arg toolbar text for multiline mode.
    /// </summary>
    /// <returns>Formatted arg text.</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_get_arg_text</c>.
    /// </remarks>
    private IReadOnlyList<StyleAndTextTuple> GetArgText()
    {
        var arg = App.KeyProcessor.Arg;
        if (arg is null)
        {
            // Should not happen because of the HasArg filter in the layout
            return [];
        }

        var argStr = arg == "-" ? "-1" : arg;

        return
        [
            new StyleAndTextTuple("class:arg-toolbar", "Repeat: "),
            new StyleAndTextTuple("class:arg-toolbar.text", argStr),
        ];
    }

    /// <summary>
    /// Gets the inline arg prefix for single-line mode.
    /// </summary>
    /// <returns>Formatted arg prefix text.</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_inline_arg</c>.
    /// </remarks>
    private IReadOnlyList<StyleAndTextTuple> InlineArg()
    {
        var app = Application.AppContext.GetApp();
        if (app.KeyProcessor.Arg is null)
        {
            return [];
        }

        var arg = app.KeyProcessor.Arg;
        return
        [
            new StyleAndTextTuple("class:prompt.arg", "(arg: "),
            new StyleAndTextTuple("class:prompt.arg.text", arg),
            new StyleAndTextTuple("class:prompt.arg", ") "),
        ];
    }
}
