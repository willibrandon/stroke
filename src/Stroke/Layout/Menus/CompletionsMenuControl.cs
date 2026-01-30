using Stroke.Application;
using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;

using AppContext = Stroke.Application.AppContext;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Layout.Menus;

/// <summary>
/// Helper for drawing the completion menu to the screen.
/// </summary>
/// <remarks>
/// <para>
/// Renders a single-column list of completion items with optional meta information column.
/// Each item is styled and padded to a uniform width. The currently selected completion
/// uses a distinct style class.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>CompletionsMenuControl</c> class from <c>layout/menus.py</c>.
/// </para>
/// <para>
/// This class is stateless and inherently thread-safe.
/// </para>
/// </remarks>
internal sealed class CompletionsMenuControl : IUIControl
{
    /// <summary>
    /// Preferred minimum width of the menu control.
    /// The CompletionsMenu class defines a width of 8, and there is a scrollbar of 1.
    /// </summary>
    public const int MinWidth = 7;

    /// <summary>
    /// Gets whether this control is focusable. Always returns <c>false</c>.
    /// </summary>
    public bool IsFocusable => false;

    /// <summary>
    /// Returns the preferred width: menu column width + meta column width,
    /// or 0 if no completions are active.
    /// </summary>
    /// <param name="maxAvailableWidth">Maximum width available from parent.</param>
    /// <returns>Preferred width or 0.</returns>
    public int? PreferredWidth(int maxAvailableWidth)
    {
        // Python lines 67-75
        var completeState = AppContext.GetApp().CurrentBuffer.CompleteState;
        if (completeState is not null)
        {
            var menuWidth = GetMenuWidth(500, completeState);
            var menuMetaWidth = GetMenuMetaWidth(500, completeState);
            return menuWidth + menuMetaWidth;
        }
        return 0;
    }

    /// <summary>
    /// Returns the preferred height: number of completions,
    /// or 0 if no completions are active.
    /// </summary>
    public int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        GetLinePrefixCallable? getLinePrefix)
    {
        // Python lines 77-88
        var completeState = AppContext.GetApp().CurrentBuffer.CompleteState;
        if (completeState is not null)
        {
            return completeState.Completions.Count;
        }
        return 0;
    }

    /// <summary>
    /// Creates the UI content with one line per completion item,
    /// each styled and padded to the computed width.
    /// </summary>
    /// <param name="width">The available width in columns.</param>
    /// <param name="height">The available height in rows.</param>
    /// <returns>The UI content for this frame.</returns>
    public UIContent CreateContent(int width, int height)
    {
        // Python lines 90-125
        var completeState = AppContext.GetApp().CurrentBuffer.CompleteState;
        if (completeState is not null)
        {
            var completions = completeState.Completions;
            var index = completeState.CompleteIndex; // Can be null

            // Calculate width of completions menu.
            var menuWidth = GetMenuWidth(width, completeState);
            var menuMetaWidth = GetMenuMetaWidth(width - menuWidth, completeState);
            var showMeta = ShowMeta(completeState);

            IReadOnlyList<StyleAndTextTuple> GetLine(int i)
            {
                var c = completions[i];
                var isCurrentCompletion = i == index;
                var result = new List<StyleAndTextTuple>(
                    MenuUtils.GetMenuItemFragments(c, isCurrentCompletion, menuWidth, spaceAfter: true));

                if (showMeta)
                {
                    result.AddRange(GetMenuItemMetaFragments(c, isCurrentCompletion, menuMetaWidth));
                }
                return result;
            }

            return new UIContent(
                getLine: GetLine,
                cursorPosition: new Point(0, index ?? 0),
                lineCount: completions.Count);
        }

        return new UIContent();
    }

    /// <summary>
    /// Handles mouse events: click selects and closes, scroll navigates by 3.
    /// </summary>
    /// <param name="mouseEvent">The mouse event.</param>
    /// <returns>NotImplementedOrNone indicating whether the event was handled.</returns>
    public NotImplementedOrNone MouseHandler(MouseEvent mouseEvent)
    {
        // Python lines 182-201
        var b = AppContext.GetApp().CurrentBuffer;

        if (mouseEvent.EventType == MouseEventType.MouseUp)
        {
            // Select completion at the clicked row and close the menu.
            var index = mouseEvent.Position.Y;
            var completeState = b.CompleteState;
            if (completeState is not null && index < completeState.Completions.Count)
            {
                b.GoToCompletion(index);
                b.DismissCompletion();
            }
            return NotImplementedOrNone.None;
        }

        if (mouseEvent.EventType == MouseEventType.ScrollDown)
        {
            // Scroll down (next 3 completions).
            b.CompleteNext(count: 3, disableWrapAround: true);
            return NotImplementedOrNone.None;
        }

        if (mouseEvent.EventType == MouseEventType.ScrollUp)
        {
            // Scroll up (previous 3 completions).
            b.CompletePrevious(count: 3, disableWrapAround: true);
            return NotImplementedOrNone.None;
        }

        return NotImplementedOrNone.NotImplemented;
    }

    /// <summary>
    /// Returns whether any completion in the state has <c>DisplayMetaText</c>.
    /// </summary>
    /// <remarks>
    /// Python line 131: checks <c>display_meta_text</c> (plain text), not <c>display_meta</c>.
    /// </remarks>
    private static bool ShowMeta(CompletionState completeState)
    {
        foreach (var c in completeState.Completions)
        {
            if (!string.IsNullOrEmpty(c.DisplayMetaText))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the width of the completion text column, clamped to maxWidth
    /// and floored to MinWidth.
    /// </summary>
    private static int GetMenuWidth(int maxWidth, CompletionState completeState)
    {
        // Python lines 133-143
        var maxDisplayWidth = 0;
        foreach (var c in completeState.Completions)
        {
            var w = UnicodeWidth.GetWidth(c.DisplayText);
            if (w > maxDisplayWidth)
                maxDisplayWidth = w;
        }
        return Math.Min(maxWidth, Math.Max(MinWidth, maxDisplayWidth + 2));
    }

    /// <summary>
    /// Returns the width of the meta column, sampling at most 200 completions.
    /// Returns 0 if ShowMeta is false.
    /// </summary>
    private static int GetMenuMetaWidth(int maxWidth, CompletionState completeState)
    {
        // Python lines 145-164
        if (!ShowMeta(completeState))
            return 0;

        var completions = completeState.Completions;
        var sampleCount = Math.Min(completions.Count, 200);

        var maxMetaWidth = 0;
        for (int i = 0; i < sampleCount; i++)
        {
            var w = UnicodeWidth.GetWidth(completions[i].DisplayMetaText);
            if (w > maxMetaWidth)
                maxMetaWidth = w;
        }

        return Math.Min(maxWidth, maxMetaWidth + 2);
    }

    /// <summary>
    /// Returns styled fragments for a single completion's meta column entry.
    /// </summary>
    private static IReadOnlyList<StyleAndTextTuple> GetMenuItemMetaFragments(
        CompletionItem completion, bool isCurrentCompletion, int width)
    {
        // Python lines 166-180
        var styleStr = isCurrentCompletion
            ? "class:completion-menu.meta.completion.current"
            : "class:completion-menu.meta.completion";

        var displayMeta = completion.DisplayMeta is not null
            ? FormattedTextUtils.ToFormattedText(completion.DisplayMeta.Value)
            : FormattedText.FormattedText.Empty;

        var (text, tw) = MenuUtils.TrimFormattedText(displayMeta, width - 2);
        var paddingLength = Math.Max(0, width - 1 - tw);
        var padding = new string(' ', paddingLength);

        var fragments = new List<StyleAndTextTuple>(text.Count + 2)
        {
            new("", " ")
        };
        fragments.AddRange(text);
        fragments.Add(new("", padding));

        return FormattedTextUtils.ToFormattedText(
            (AnyFormattedText)new FormattedText.FormattedText(fragments),
            style: styleStr);
    }
}
