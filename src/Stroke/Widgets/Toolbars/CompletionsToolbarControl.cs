using Stroke.Application;
using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Widgets.Toolbars;

/// <summary>
/// Internal UIControl that renders completions horizontally with pagination arrows.
/// </summary>
/// <remarks>
/// <para>
/// Displays completion items in a single line with "&lt;" and "&gt;" arrow indicators
/// when completions extend beyond the visible area. The currently selected completion
/// is highlighted with the current-completion style.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>_CompletionsToolbarControl</c> from <c>widgets/toolbars.py</c>.
/// </para>
/// </remarks>
internal class CompletionsToolbarControl : IUIControl
{
    /// <summary>Gets whether the control is focusable (always false).</summary>
    public bool IsFocusable => false;

    /// <summary>
    /// Creates the UIContent for the given dimensions.
    /// </summary>
    public UIContent CreateContent(int width, int height)
    {
        var allFragments = new List<StyleAndTextTuple>();

        var completeState = AppContext.GetApp().CurrentBuffer.CompleteState;
        if (completeState != null)
        {
            var completions = completeState.Completions;
            var index = completeState.CompleteIndex; // Can be null!

            if (completions.Count > 0)
            {
                // Width of the completions without the left/right arrows in the margins.
                var contentWidth = width - 6;

                // Booleans indicating whether we stripped from the left/right
                var cutLeft = false;
                var cutRight = false;

                // Create Menu content.
                var fragments = new List<StyleAndTextTuple>();

                for (var i = 0; i < completions.Count; i++)
                {
                    var c = completions[i];

                    // When there is no more place for the next completion
                    if (FormattedTextUtils.FragmentListLen(fragments) + c.DisplayText.Length >= contentWidth)
                    {
                        // If the current one was not yet displayed, page to the next sequence.
                        if (i <= (index ?? 0))
                        {
                            fragments.Clear();
                            cutLeft = true;
                        }
                        // If the current one is visible, stop here.
                        else
                        {
                            cutRight = true;
                            break;
                        }
                    }

                    var style = i == index
                        ? "class:completion-toolbar.completion.current"
                        : "class:completion-toolbar.completion";

                    fragments.AddRange(
                        FormattedTextUtils.ToFormattedText(
                            (AnyFormattedText)c.DisplayText,
                            style));
                    fragments.Add(new("", " "));
                }

                // Extend/strip until the content width.
                var currentLen = FormattedTextUtils.FragmentListLen(fragments);
                if (contentWidth > currentLen)
                {
                    fragments.Add(new("", new string(' ', contentWidth - currentLen)));
                }

                // Safety trim: limit fragment count to contentWidth
                if (fragments.Count > contentWidth)
                {
                    fragments.RemoveRange(contentWidth, fragments.Count - contentWidth);
                }

                // Build result with arrow indicators
                allFragments.Add(new("", " "));
                allFragments.Add(new("class:completion-toolbar.arrow", cutLeft ? "<" : " "));
                allFragments.Add(new("", " "));

                allFragments.AddRange(fragments);

                allFragments.Add(new("", " "));
                allFragments.Add(new("class:completion-toolbar.arrow", cutRight ? ">" : " "));
                allFragments.Add(new("", " "));
            }
        }

        IReadOnlyList<StyleAndTextTuple> GetLine(int i) => allFragments;

        return new UIContent(
            getLine: GetLine,
            lineCount: 1);
    }

    /// <inheritdoc/>
    public NotImplementedOrNone MouseHandler(MouseEvent mouseEvent) =>
        NotImplementedOrNone.NotImplemented;
}
