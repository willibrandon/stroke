using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;

namespace Stroke.Layout.Margins;

/// <summary>
/// Margin for displaying a prompt string.
/// </summary>
/// <remarks>
/// <para>
/// Displays a prompt in the margin area, with an optional continuation
/// prompt for subsequent lines. This is typically used for REPL-style
/// prompts (e.g., ">>> " for the first line, "... " for continuations).
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>PromptMargin</c> class from <c>layout/margins.py</c>.
/// </para>
/// </remarks>
[Obsolete("Use FormattedTextControl with left margins instead.")]
public sealed class PromptMargin : IMargin
{
    /// <summary>
    /// Gets the function returning the prompt fragments.
    /// </summary>
    public Func<IReadOnlyList<StyleAndTextTuple>> GetPrompt { get; }

    /// <summary>
    /// Gets the function returning continuation prompt fragments, or null.
    /// </summary>
    public Func<int, int, bool, IReadOnlyList<StyleAndTextTuple>>? GetContinuation { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PromptMargin"/> class.
    /// </summary>
    /// <param name="getPrompt">Function returning prompt fragments for the first line.</param>
    /// <param name="getContinuation">
    /// Function returning continuation prompt fragments.
    /// Parameters: (width, lineNumber, isSoftWrap).
    /// </param>
    public PromptMargin(
        Func<IReadOnlyList<StyleAndTextTuple>> getPrompt,
        Func<int, int, bool, IReadOnlyList<StyleAndTextTuple>>? getContinuation = null)
    {
        ArgumentNullException.ThrowIfNull(getPrompt);

        GetPrompt = getPrompt;
        GetContinuation = getContinuation;
    }

    /// <inheritdoc/>
    public int GetWidth(Func<UIContent> getUIContent)
    {
        var prompt = GetPrompt();
        return FormattedTextUtils.FragmentListWidth(prompt);
    }

    /// <inheritdoc/>
    public IReadOnlyList<StyleAndTextTuple> CreateMargin(
        WindowRenderInfo windowRenderInfo,
        int width,
        int height)
    {
        var result = new List<StyleAndTextTuple>();

        for (int i = 0; i < height; i++)
        {
            if (i == 0)
            {
                // First line: show prompt
                var prompt = GetPrompt();
                result.AddRange(prompt);
            }
            else if (GetContinuation != null)
            {
                // Subsequent lines: show continuation
                var continuation = GetContinuation(width, i, false);
                result.AddRange(continuation);
            }
            else
            {
                // No continuation function: empty space
                result.Add(new StyleAndTextTuple("", new string(' ', width)));
            }

            // Add newline for all but the last line
            if (i < height - 1)
            {
                result.Add(new StyleAndTextTuple("", "\n"));
            }
        }

        return result;
    }
}
