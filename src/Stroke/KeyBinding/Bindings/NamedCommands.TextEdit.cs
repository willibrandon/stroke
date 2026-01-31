namespace Stroke.KeyBinding.Bindings;

public static partial class NamedCommands
{
    /// <summary>
    /// Registers the 9 text modification commands.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's text modification commands from
    /// <c>named_commands.py</c> lines 229-341.
    /// </remarks>
    static partial void RegisterTextEditCommands()
    {
        RegisterInternal("end-of-file", EndOfFile);
        RegisterInternal("delete-char", DeleteChar);
        RegisterInternal("backward-delete-char", BackwardDeleteChar);
        RegisterInternal("self-insert", SelfInsert);
        RegisterInternal("transpose-chars", TransposeChars);
        RegisterInternal("uppercase-word", UppercaseWord);
        RegisterInternal("downcase-word", DowncaseWord);
        RegisterInternal("capitalize-word", CapitalizeWord);
        RegisterInternal("quoted-insert", QuotedInsert);
    }

    /// <summary>Exit the application.</summary>
    private static NotImplementedOrNone? EndOfFile(KeyPressEvent @event)
    {
        @event.GetApp().Exit();
        return null;
    }

    /// <summary>Delete character at the cursor (forward delete).</summary>
    private static NotImplementedOrNone? DeleteChar(KeyPressEvent @event)
    {
        var deleted = @event.CurrentBuffer!.Delete(count: @event.Arg);
        if (string.IsNullOrEmpty(deleted))
        {
            @event.GetApp().Output.Bell();
        }

        return null;
    }

    /// <summary>Delete the character behind the cursor.</summary>
    private static NotImplementedOrNone? BackwardDeleteChar(KeyPressEvent @event)
    {
        string deleted;
        if (@event.Arg < 0)
        {
            // When a negative argument has been given, delete forward.
            deleted = @event.CurrentBuffer!.Delete(count: -@event.Arg);
        }
        else
        {
            deleted = @event.CurrentBuffer!.DeleteBeforeCursor(count: @event.Arg);
        }

        if (string.IsNullOrEmpty(deleted))
        {
            @event.GetApp().Output.Bell();
        }

        return null;
    }

    /// <summary>Insert the typed character(s).</summary>
    private static NotImplementedOrNone? SelfInsert(KeyPressEvent @event)
    {
        var data = @event.Data;
        @event.CurrentBuffer!.InsertText(string.Concat(Enumerable.Repeat(data, @event.Arg)));
        return null;
    }

    /// <summary>
    /// Emulate Emacs transpose-char behavior: at the beginning of the buffer,
    /// do nothing. At the end of a line or buffer, swap the characters before
    /// the cursor. Otherwise, move the cursor right, and then swap the
    /// characters before the cursor.
    /// </summary>
    private static NotImplementedOrNone? TransposeChars(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        var p = buff.CursorPosition;

        if (p == 0)
        {
            return null;
        }

        if (p == buff.Document.Text.Length || buff.Document.Text[p] == '\n')
        {
            buff.SwapCharactersBeforeCursor();
        }
        else
        {
            buff.CursorPosition += buff.Document.GetCursorRightPosition();
            buff.SwapCharactersBeforeCursor();
        }

        return null;
    }

    /// <summary>Uppercase the current (or following) word.</summary>
    private static NotImplementedOrNone? UppercaseWord(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;

        for (var i = 0; i < @event.Arg; i++)
        {
            var pos = buff.Document.FindNextWordEnding();
            if (pos is null or 0) break;
            var words = buff.Document.TextAfterCursor[..pos.Value];
            buff.InsertText(words.ToUpperInvariant(), overwrite: true);
        }

        return null;
    }

    /// <summary>Lowercase the current (or following) word.</summary>
    private static NotImplementedOrNone? DowncaseWord(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;

        for (var i = 0; i < @event.Arg; i++)
        {
            var pos = buff.Document.FindNextWordEnding();
            if (pos is null or 0) break;
            var words = buff.Document.TextAfterCursor[..pos.Value];
            buff.InsertText(words.ToLowerInvariant(), overwrite: true);
        }

        return null;
    }

    /// <summary>Capitalize the current (or following) word.</summary>
    private static NotImplementedOrNone? CapitalizeWord(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;

        for (var i = 0; i < @event.Arg; i++)
        {
            var pos = buff.Document.FindNextWordEnding();
            if (pos is null or 0) break;
            var words = buff.Document.TextAfterCursor[..pos.Value];
            buff.InsertText(TitleCase(words), overwrite: true);
        }

        return null;
    }

    /// <summary>
    /// Title-case a string, matching Python's <c>str.title()</c> behavior:
    /// uppercase the first letter of each word, lowercase the rest.
    /// </summary>
    private static string TitleCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var chars = input.ToCharArray();
        var previousIsWordChar = false;

        for (var i = 0; i < chars.Length; i++)
        {
            if (char.IsLetter(chars[i]))
            {
                chars[i] = previousIsWordChar
                    ? char.ToLowerInvariant(chars[i])
                    : char.ToUpperInvariant(chars[i]);
                previousIsWordChar = true;
            }
            else
            {
                previousIsWordChar = char.IsDigit(chars[i]);
            }
        }

        return new string(chars);
    }

    /// <summary>
    /// Add the next character typed to the line verbatim. This is how to insert
    /// key sequences like C-q, for example.
    /// </summary>
    private static NotImplementedOrNone? QuotedInsert(KeyPressEvent @event)
    {
        @event.GetApp().QuotedInsert = true;
        return null;
    }
}
