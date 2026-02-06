using Stroke.Core;

namespace Stroke.KeyBinding.Bindings;

public static partial class NamedCommands
{
    /// <summary>
    /// Registers the 6 history commands.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's history commands from
    /// <c>named_commands.py</c> lines 168-227.
    /// </remarks>
    static partial void RegisterHistoryCommands()
    {
        RegisterInternal("accept-line", AcceptLine);
        RegisterInternal("previous-history", PreviousHistory);
        RegisterInternal("next-history", NextHistory);
        RegisterInternal("beginning-of-history", BeginningOfHistory);
        RegisterInternal("end-of-history", EndOfHistory);
        RegisterInternal("reverse-search-history", ReverseSearchHistory);
    }

    /// <summary>Accept the line regardless of where the cursor is.</summary>
    private static NotImplementedOrNone? AcceptLine(KeyPressEvent @event)
    {
        var buffer = @event.CurrentBuffer!;
        var app = @event.GetApp();
        app.CreateBackgroundTask(async _ =>
        {
            await buffer.ValidateAndHandleAsync();
            app.Invalidate();
        });
        return null;
    }

    /// <summary>Move back through the history list, fetching the previous command.</summary>
    private static NotImplementedOrNone? PreviousHistory(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.HistoryBackward(count: @event.Arg);
        return null;
    }

    /// <summary>Move forward through the history list, fetching the next command.</summary>
    private static NotImplementedOrNone? NextHistory(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.HistoryForward(count: @event.Arg);
        return null;
    }

    /// <summary>Move to the first line in the history.</summary>
    private static NotImplementedOrNone? BeginningOfHistory(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.GoToHistory(0);
        return null;
    }

    /// <summary>
    /// Move to the end of the input history, i.e., the line currently being entered.
    /// </summary>
    private static NotImplementedOrNone? EndOfHistory(KeyPressEvent @event)
    {
        // Port of Python: buff.history_forward(count=10**100) then
        // buff.go_to_history(len(buff._working_lines) - 1)
        var buff = @event.CurrentBuffer!;
        buff.HistoryForward(count: 1_000_000_000);
        buff.GoToHistory(buff.WorkingLineCount - 1);
        return null;
    }

    /// <summary>
    /// Search backward starting at the current line and moving up through
    /// the history as necessary. This is an incremental search.
    /// </summary>
    private static NotImplementedOrNone? ReverseSearchHistory(KeyPressEvent @event)
    {
        @event.GetApp().StartSearch(SearchDirection.Backward);
        return null;
    }
}
