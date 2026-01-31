namespace Stroke.KeyBinding.Bindings;

public static partial class NamedCommands
{
    /// <summary>
    /// Registers the 10 movement commands.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's movement commands from
    /// <c>named_commands.py</c> lines 56-165.
    /// </remarks>
    static partial void RegisterMovementCommands()
    {
        RegisterInternal("beginning-of-buffer", BeginningOfBuffer);
        RegisterInternal("end-of-buffer", EndOfBuffer);
        RegisterInternal("beginning-of-line", BeginningOfLine);
        RegisterInternal("end-of-line", EndOfLine);
        RegisterInternal("forward-char", ForwardChar);
        RegisterInternal("backward-char", BackwardChar);
        RegisterInternal("forward-word", ForwardWord);
        RegisterInternal("backward-word", BackwardWord);
        RegisterInternal("clear-screen", ClearScreen);
        RegisterInternal("redraw-current-line", RedrawCurrentLine);
    }

    /// <summary>Move to the start of the buffer.</summary>
    private static NotImplementedOrNone? BeginningOfBuffer(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        buff.CursorPosition = 0;
        return null;
    }

    /// <summary>Move to the end of the buffer.</summary>
    private static NotImplementedOrNone? EndOfBuffer(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        buff.CursorPosition = buff.Document.Text.Length;
        return null;
    }

    /// <summary>Move to the start of the current line.</summary>
    private static NotImplementedOrNone? BeginningOfLine(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        buff.CursorPosition += buff.Document.GetStartOfLinePosition(afterWhitespace: false);
        return null;
    }

    /// <summary>Move to the end of the current line.</summary>
    private static NotImplementedOrNone? EndOfLine(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        buff.CursorPosition += buff.Document.GetEndOfLinePosition();
        return null;
    }

    /// <summary>Move forward a character.</summary>
    private static NotImplementedOrNone? ForwardChar(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        buff.CursorPosition += buff.Document.GetCursorRightPosition(count: @event.Arg);
        return null;
    }

    /// <summary>Move back a character.</summary>
    private static NotImplementedOrNone? BackwardChar(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        buff.CursorPosition += buff.Document.GetCursorLeftPosition(count: @event.Arg);
        return null;
    }

    /// <summary>
    /// Move forward to the end of the next word. Words are composed of letters and digits.
    /// </summary>
    private static NotImplementedOrNone? ForwardWord(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        var pos = buff.Document.FindNextWordEnding(count: @event.Arg);

        if (pos.HasValue)
        {
            buff.CursorPosition += pos.Value;
        }

        return null;
    }

    /// <summary>
    /// Move back to the start of the current or previous word. Words are composed of
    /// letters and digits.
    /// </summary>
    private static NotImplementedOrNone? BackwardWord(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        var pos = buff.Document.FindPreviousWordBeginning(count: @event.Arg);

        if (pos.HasValue)
        {
            buff.CursorPosition += pos.Value;
        }

        return null;
    }

    /// <summary>Clear the screen and redraw everything at the top of the screen.</summary>
    private static NotImplementedOrNone? ClearScreen(KeyPressEvent @event)
    {
        @event.GetApp().Renderer.Clear();
        return null;
    }

    /// <summary>
    /// Refresh the current line.
    /// (Readline defines this command, but prompt-toolkit doesn't implement it.)
    /// </summary>
    private static NotImplementedOrNone? RedrawCurrentLine(KeyPressEvent @event)
    {
        // No-op — defined by Readline but not implemented in Python Prompt Toolkit.
        return null;
    }
}
