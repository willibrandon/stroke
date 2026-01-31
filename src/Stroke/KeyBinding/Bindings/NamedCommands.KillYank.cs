using Stroke.Core;

namespace Stroke.KeyBinding.Bindings;

public static partial class NamedCommands
{
    /// <summary>
    /// Registers the 10 kill and yank commands.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's kill and yank commands from
    /// <c>named_commands.py</c> lines 348-501.
    /// </remarks>
    static partial void RegisterKillYankCommands()
    {
        RegisterInternal("kill-line", KillLine);
        RegisterInternal("kill-word", KillWord);
        RegisterInternal("unix-word-rubout", e => UnixWordRuboutImpl(e, word: true));
        RegisterInternal("backward-kill-word", BackwardKillWord);
        RegisterInternal("delete-horizontal-space", DeleteHorizontalSpace);
        RegisterInternal("unix-line-discard", UnixLineDiscard);
        RegisterInternal("yank", Yank);
        RegisterInternal("yank-nth-arg", YankNthArg);
        RegisterInternal("yank-last-arg", YankLastArg);
        RegisterInternal("yank-pop", YankPop);
    }

    /// <summary>
    /// Kill the text from the cursor to the end of the line.
    /// If we are at the end of the line, this should remove the newline.
    /// </summary>
    private static NotImplementedOrNone? KillLine(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        string deleted;

        if (@event.Arg < 0)
        {
            // Kill to start of line.
            deleted = buff.DeleteBeforeCursor(
                count: -buff.Document.GetStartOfLinePosition());
        }
        else
        {
            if (buff.Document.CurrentChar == '\n')
            {
                deleted = buff.Delete(1);
            }
            else
            {
                deleted = buff.Delete(count: buff.Document.GetEndOfLinePosition());
            }
        }

        @event.GetApp().Clipboard.SetText(deleted);
        return null;
    }

    /// <summary>
    /// Kill from point to the end of the current word, or if between words, to the
    /// end of the next word. Word boundaries are the same as forward-word.
    /// </summary>
    private static NotImplementedOrNone? KillWord(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        var pos = buff.Document.FindNextWordEnding(count: @event.Arg);

        if (pos.HasValue && pos.Value != 0)
        {
            var deleted = buff.Delete(count: pos.Value);

            if (@event.IsRepeat)
            {
                // Forward kill: append (prev + new).
                deleted = @event.GetApp().Clipboard.GetData().Text + deleted;
            }

            @event.GetApp().Clipboard.SetText(deleted);
        }

        return null;
    }

    /// <summary>
    /// Kill the word behind point, using the specified word boundary.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <param name="word">
    /// If true, use whitespace as the word boundary (WORD=true, for unix-word-rubout).
    /// If false, use non-alphanumeric characters as the boundary (WORD=false, for backward-kill-word).
    /// </param>
    private static NotImplementedOrNone? UnixWordRuboutImpl(KeyPressEvent @event, bool word)
    {
        var buff = @event.CurrentBuffer!;
        var pos = buff.Document.FindPreviousWordBeginning(count: @event.Arg, WORD: word);

        if (pos is null)
        {
            // Nothing found? Delete until the start of the document.
            // (The input starts with whitespace and no words were found before the cursor.)
            pos = -buff.CursorPosition;
        }

        if (pos.Value != 0)
        {
            var deleted = buff.DeleteBeforeCursor(count: -pos.Value);

            // If the previous key press was also the same command, concatenate deleted text.
            if (@event.IsRepeat)
            {
                // Backward kill: prepend (new + prev).
                deleted += @event.GetApp().Clipboard.GetData().Text;
            }

            @event.GetApp().Clipboard.SetText(deleted);
        }
        else
        {
            // Nothing to delete. Bell.
            @event.GetApp().Output.Bell();
        }

        return null;
    }

    /// <summary>
    /// Kills the word before point, using "not a letter nor a digit" as a word boundary.
    /// </summary>
    private static NotImplementedOrNone? BackwardKillWord(KeyPressEvent @event)
    {
        return UnixWordRuboutImpl(@event, word: false);
    }

    /// <summary>Delete all spaces and tabs around point.</summary>
    private static NotImplementedOrNone? DeleteHorizontalSpace(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        var textBeforeCursor = buff.Document.TextBeforeCursor;
        var textAfterCursor = buff.Document.TextAfterCursor;

        var deleteBefore = textBeforeCursor.Length - textBeforeCursor.TrimEnd('\t', ' ').Length;
        var deleteAfter = textAfterCursor.Length - textAfterCursor.TrimStart('\t', ' ').Length;

        buff.DeleteBeforeCursor(count: deleteBefore);
        buff.Delete(count: deleteAfter);
        return null;
    }

    /// <summary>Kill backward from the cursor to the beginning of the current line.</summary>
    private static NotImplementedOrNone? UnixLineDiscard(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;

        if (buff.Document.CursorPositionCol == 0 && buff.CursorPosition > 0)
        {
            buff.DeleteBeforeCursor(count: 1);
        }
        else
        {
            var deleted = buff.DeleteBeforeCursor(
                count: -buff.Document.GetStartOfLinePosition());
            @event.GetApp().Clipboard.SetText(deleted);
        }

        return null;
    }

    /// <summary>Paste before cursor.</summary>
    private static NotImplementedOrNone? Yank(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.PasteClipboardData(
            @event.GetApp().Clipboard.GetData(),
            pasteMode: PasteMode.Emacs,
            count: @event.Arg);
        return null;
    }

    /// <summary>
    /// Insert the first argument of the previous command. With an argument, insert
    /// the nth word from the previous command (start counting at 0).
    /// </summary>
    private static NotImplementedOrNone? YankNthArg(KeyPressEvent @event)
    {
        var n = @event.ArgPresent ? @event.Arg : (int?)null;
        @event.CurrentBuffer!.YankNthArg(n);
        return null;
    }

    /// <summary>
    /// Like yank-nth-arg, but if no argument has been given, yank the last word
    /// of each line.
    /// </summary>
    private static NotImplementedOrNone? YankLastArg(KeyPressEvent @event)
    {
        var n = @event.ArgPresent ? @event.Arg : (int?)null;
        @event.CurrentBuffer!.YankLastArg(n);
        return null;
    }

    /// <summary>
    /// Rotate the kill ring, and yank the new top. Only works following yank or yank-pop.
    /// </summary>
    private static NotImplementedOrNone? YankPop(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        var docBeforePaste = buff.DocumentBeforePaste;
        var clipboard = @event.GetApp().Clipboard;

        if (docBeforePaste is not null)
        {
            buff.Document = docBeforePaste;
            clipboard.Rotate();
            buff.PasteClipboardData(clipboard.GetData(), pasteMode: PasteMode.Emacs);
        }

        return null;
    }
}
