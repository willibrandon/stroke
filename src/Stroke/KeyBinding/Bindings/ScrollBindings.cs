using Stroke.Application;
using Stroke.Core;
using Stroke.Layout.Containers;
using Stroke.Layout.Windows;

namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Static scroll functions for navigating through long multiline buffers.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.scroll</c> module.
/// Provides 8 scroll functions: forward, backward, half-page down/up, one-line down/up,
/// page down/up.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. All mutable state is accessed through
/// <see cref="Window.VerticalScroll"/> and
/// <see cref="Stroke.Core.Buffer.CursorPosition"/>, which handle their own synchronization.
/// </para>
/// </remarks>
public static class ScrollBindings
{
    /// <summary>
    /// Scroll window down by one full window height.
    /// Moves the cursor down by the number of logical lines that fill the window height,
    /// accounting for variable line heights from wrapped content.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    public static NotImplementedOrNone? ScrollForward(KeyPressEvent @event)
    {
        ScrollForwardInternal(@event, half: false);
        return null;
    }

    /// <summary>
    /// Scroll window up by one full window height.
    /// Moves the cursor up by the number of logical lines that fill the window height,
    /// accounting for variable line heights from wrapped content.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    public static NotImplementedOrNone? ScrollBackward(KeyPressEvent @event)
    {
        ScrollBackwardInternal(@event, half: false);
        return null;
    }

    /// <summary>
    /// Scroll window down by half a page. Same as <see cref="ScrollForward"/> but scrolls
    /// only half the window height.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    public static NotImplementedOrNone? ScrollHalfPageDown(KeyPressEvent @event)
    {
        ScrollForwardInternal(@event, half: true);
        return null;
    }

    /// <summary>
    /// Scroll window up by half a page. Same as <see cref="ScrollBackward"/> but scrolls
    /// only half the window height.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    public static NotImplementedOrNone? ScrollHalfPageUp(KeyPressEvent @event)
    {
        ScrollBackwardInternal(@event, half: true);
        return null;
    }

    /// <summary>
    /// Scroll the viewport down by one line. Adjusts the cursor position only when necessary
    /// to keep it within the visible area (when cursor is at the top scroll offset boundary).
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    public static NotImplementedOrNone? ScrollOneLineDown(KeyPressEvent @event)
    {
        var w = @event.GetApp().Layout.CurrentWindow;
        var b = @event.GetApp().CurrentBuffer;

        if (w.RenderInfo is { } info)
        {
            if (w.VerticalScroll < info.ContentHeight - info.WindowHeight)
            {
                // When the cursor is at the top, move to the next line.
                // (Otherwise, only scroll.)
                if (info.CursorPosition.Y <= info.ConfiguredScrollOffsets.Top)
                {
                    b.CursorPosition += b.Document.GetCursorDownPosition();
                }

                w.VerticalScroll += 1;
            }
        }

        return null;
    }

    /// <summary>
    /// Scroll the viewport up by one line. Adjusts the cursor position only when necessary
    /// to keep it within the visible area (when cursor would fall below the visible region).
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    public static NotImplementedOrNone? ScrollOneLineUp(KeyPressEvent @event)
    {
        var w = @event.GetApp().Layout.CurrentWindow;
        var b = @event.GetApp().CurrentBuffer;

        if (w.RenderInfo is { } info)
        {
            if (w.VerticalScroll > 0)
            {
                var firstLineHeight = info.GetHeightForLine(info.FirstVisibleLine());

                var cursorUp = info.CursorPosition.Y - (
                    info.WindowHeight
                    - 1
                    - firstLineHeight
                    - info.ConfiguredScrollOffsets.Bottom
                );

                // Move cursor up, as many steps as the height of the first line.
                for (var i = 0; i < Math.Max(0, cursorUp); i++)
                {
                    b.CursorPosition += b.Document.GetCursorUpPosition();
                }

                // Scroll window
                w.VerticalScroll -= 1;
            }
        }

        return null;
    }

    /// <summary>
    /// Scroll page down. Sets the vertical scroll offset to the last visible line index
    /// and positions the cursor at the beginning of the newly visible content
    /// (first non-whitespace character).
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    public static NotImplementedOrNone? ScrollPageDown(KeyPressEvent @event)
    {
        var w = @event.GetApp().Layout.CurrentWindow;
        var b = @event.GetApp().CurrentBuffer;

        if (w.RenderInfo is { } info)
        {
            // Scroll down one page.
            var lineIndex = Math.Max(info.LastVisibleLine(), w.VerticalScroll + 1);
            w.VerticalScroll = lineIndex;

            b.CursorPosition = b.Document.TranslateRowColToIndex(lineIndex, 0);
            b.CursorPosition += b.Document.GetStartOfLinePosition(afterWhitespace: true);
        }

        return null;
    }

    /// <summary>
    /// Scroll page up. Positions the cursor at the first visible line (ensuring at least
    /// one line of movement) and resets the vertical scroll offset to 0.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    public static NotImplementedOrNone? ScrollPageUp(KeyPressEvent @event)
    {
        var w = @event.GetApp().Layout.CurrentWindow;
        var b = @event.GetApp().CurrentBuffer;

        if (w.RenderInfo is { } info)
        {
            // Put cursor at the first visible line. (But make sure that the cursor
            // moves at least one line up.)
            var lineIndex = Math.Max(
                0,
                Math.Min(info.FirstVisibleLine(), b.Document.CursorPositionRow - 1)
            );

            b.CursorPosition = b.Document.TranslateRowColToIndex(lineIndex, 0);
            b.CursorPosition += b.Document.GetStartOfLinePosition(afterWhitespace: true);

            // Set the scroll offset. We can safely set it to zero; the Window will
            // make sure that it scrolls at least until the cursor becomes visible.
            w.VerticalScroll = 0;
        }

        return null;
    }

    /// <summary>
    /// Internal implementation for scroll forward with configurable half-page option.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <param name="half">If true, scroll only half a page.</param>
    private static void ScrollForwardInternal(KeyPressEvent @event, bool half)
    {
        var w = @event.GetApp().Layout.CurrentWindow;
        var b = @event.GetApp().CurrentBuffer;

        if (w.RenderInfo is { } info)
        {
            var uiContent = info.UIContent;

            // Height to scroll.
            var scrollHeight = info.WindowHeight;
            if (half)
            {
                scrollHeight /= 2;
            }

            // Calculate how many lines is equivalent to that vertical space.
            var y = b.Document.CursorPositionRow + 1;
            var height = 0;
            while (y < uiContent.LineCount)
            {
                var lineHeight = info.GetHeightForLine(y);

                if (height + lineHeight < scrollHeight)
                {
                    height += lineHeight;
                    y += 1;
                }
                else
                {
                    break;
                }
            }

            b.CursorPosition = b.Document.TranslateRowColToIndex(y, 0);
        }
    }

    /// <summary>
    /// Internal implementation for scroll backward with configurable half-page option.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <param name="half">If true, scroll only half a page.</param>
    private static void ScrollBackwardInternal(KeyPressEvent @event, bool half)
    {
        var w = @event.GetApp().Layout.CurrentWindow;
        var b = @event.GetApp().CurrentBuffer;

        if (w.RenderInfo is { } info)
        {
            // Height to scroll.
            var scrollHeight = info.WindowHeight;
            if (half)
            {
                scrollHeight /= 2;
            }

            // Calculate how many lines is equivalent to that vertical space.
            var y = Math.Max(0, b.Document.CursorPositionRow - 1);
            var height = 0;
            while (y > 0)
            {
                var lineHeight = info.GetHeightForLine(y);

                if (height + lineHeight < scrollHeight)
                {
                    height += lineHeight;
                    y -= 1;
                }
                else
                {
                    break;
                }
            }

            b.CursorPosition = b.Document.TranslateRowColToIndex(y, 0);
        }
    }
}
