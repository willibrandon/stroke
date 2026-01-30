using Stroke.Application;
using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Layout.Menus;

/// <summary>
/// Control that shows the meta information of the currently selected completion.
/// </summary>
/// <remarks>
/// <para>
/// Used as the meta row in the <see cref="MultiColumnCompletionsMenu"/>. Displays
/// the <c>DisplayMeta</c> text of the currently selected completion, styled with
/// the "class:completion-menu.multi-column-meta" style class.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>_SelectedCompletionMetaControl</c> class from
/// <c>layout/menus.py</c>.
/// </para>
/// <para>
/// This class is stateless and inherently thread-safe.
/// </para>
/// </remarks>
internal sealed class SelectedCompletionMetaControl : IUIControl
{
    /// <summary>
    /// Gets whether this control is focusable. Always returns <c>false</c>.
    /// </summary>
    public bool IsFocusable => false;

    /// <summary>
    /// Returns the preferred width: widest meta text + 2, or full available width
    /// when 30+ completions exist.
    /// </summary>
    /// <param name="maxAvailableWidth">Maximum width available from parent.</param>
    /// <returns>Preferred width or 0.</returns>
    public int? PreferredWidth(int maxAvailableWidth)
    {
        // Python lines 688-713
        var app = AppContext.GetApp();
        var completeState = app.CurrentBuffer.CompleteState;
        if (completeState is not null)
        {
            // Performance optimization: when there are many completions,
            // just return max available width.
            if (completeState.Completions.Count >= 30)
            {
                return maxAvailableWidth;
            }

            var sampleCount = Math.Min(completeState.Completions.Count, 100);
            var maxMetaWidth = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                var w = UnicodeWidth.GetWidth(completeState.Completions[i].DisplayMetaText);
                if (w > maxMetaWidth)
                    maxMetaWidth = w;
            }
            return 2 + maxMetaWidth;
        }
        return 0;
    }

    /// <summary>
    /// Returns 1 (always a single row).
    /// </summary>
    public int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        GetLinePrefixCallable? getLinePrefix) => 1;

    /// <summary>
    /// Creates content showing the selected completion's meta text,
    /// or empty content if no completion is selected.
    /// </summary>
    /// <param name="width">The available width in columns.</param>
    /// <param name="height">The available height in rows.</param>
    /// <returns>The UI content for this frame.</returns>
    public UIContent CreateContent(int width, int height)
    {
        // Python lines 724-730
        var fragments = GetTextFragments();

        IReadOnlyList<StyleAndTextTuple> GetLine(int i) => fragments;

        return new UIContent(
            getLine: GetLine,
            lineCount: fragments.Count > 0 ? 1 : 0);
    }

    /// <summary>
    /// Gets styled text fragments for the selected completion's meta.
    /// </summary>
    /// <returns>Styled fragments, or empty list if no meta is available.</returns>
    private static IReadOnlyList<StyleAndTextTuple> GetTextFragments()
    {
        // Python lines 732-748
        const string style = "class:completion-menu.multi-column-meta";
        var state = AppContext.GetApp().CurrentBuffer.CompleteState;

        if (state is not null
            && state.CurrentCompletion is not null
            && !string.IsNullOrEmpty(state.CurrentCompletion.DisplayMetaText))
        {
            // Check DisplayMetaText for existence, but render DisplayMeta
            var displayMeta = state.CurrentCompletion.DisplayMeta is not null
                ? FormattedTextUtils.ToFormattedText(state.CurrentCompletion.DisplayMeta.Value)
                : FormattedText.FormattedText.Empty;

            var fragments = new List<StyleAndTextTuple>(displayMeta.Count + 2)
            {
                new("", " ")
            };
            fragments.AddRange(displayMeta);
            fragments.Add(new("", " "));

            return FormattedTextUtils.ToFormattedText(
                (AnyFormattedText)new FormattedText.FormattedText(fragments),
                style: style);
        }

        return [];
    }
}
