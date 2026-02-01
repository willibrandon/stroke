using Stroke.Clipboard;
using Stroke.Core;
using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;

namespace Stroke.Application.Bindings;

public static partial class ViBindings
{
    /// <summary>
    /// Registers miscellaneous Vi bindings: doubled-key operators (dd, yy, cc, S, C, D),
    /// single-char operations (x, X, s), paste (p, P), undo/redo, join, tilde,
    /// doubled-key transforms (guu, gUU, g~~), indent/unindent (&gt;&gt;, &lt;&lt;),
    /// search (#, *), increment/decrement (Ctrl-A, Ctrl-X), scroll (z commands),
    /// numeric args, macros, and catch-all handlers.
    /// </summary>
    static partial void RegisterMisc(KeyBindings kb)
    {
        var viNavMode = ViFilters.ViNavigationMode;
        var viSelMode = ViFilters.ViSelectionMode;
        var isReadOnly = AppFilters.IsReadOnly;

        // ============================================================
        // Doubled-key operators: dd, yy/Y, cc/S, C, D
        // (T023)
        // ============================================================

        // dd — delete line(s)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('d'), new KeyOrChar('d')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var buffer = @event.CurrentBuffer!;
                var lines = buffer.Document.Lines;
                var row = buffer.Document.CursorPositionRow;

                var before = string.Join("\n", lines.Take(row));
                var deleted = string.Join("\n", lines.Skip(row).Take(@event.Arg));
                var after = string.Join("\n", lines.Skip(row + @event.Arg));

                if (before.Length > 0 && after.Length > 0)
                    before += "\n";

                buffer.Document = new Document(
                    text: before + after,
                    cursorPosition: before.Length + after.Length - after.TrimStart(' ').Length + (after.Length - after.TrimStart(' ').Length > 0 ? 0 : 0));

                // Cursor at start of first 'after' line, after leading whitespace
                var newCursorPos = before.Length;
                if (after.Length > 0)
                {
                    var trimmedAfter = after.TrimStart(' ');
                    newCursorPos = before.Length + (after.Length - trimmedAfter.Length);
                }
                buffer.Document = new Document(
                    text: before + after,
                    cursorPosition: newCursorPos);

                @event.GetApp().Clipboard.SetData(
                    new ClipboardData(deleted, SelectionType.Lines));
                return null;
            });

        // yy / Y — yank line(s)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('y'), new KeyOrChar('y')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var text = string.Join("\n",
                    @event.CurrentBuffer!.Document.LinesFromCurrent
                        .Take(@event.Arg));
                @event.GetApp().Clipboard.SetData(
                    new ClipboardData(text, SelectionType.Lines));
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('Y')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var text = string.Join("\n",
                    @event.CurrentBuffer!.Document.LinesFromCurrent
                        .Take(@event.Arg));
                @event.GetApp().Clipboard.SetData(
                    new ClipboardData(text, SelectionType.Lines));
                return null;
            });

        // cc / S — change current line (delete content after whitespace, enter insert)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('c'), new KeyOrChar('c')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                ChangeCurrentLine(@event);
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('S')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                ChangeCurrentLine(@event);
                return null;
            });

        // C — change to end of line
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('C')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                var buffer = @event.CurrentBuffer!;
                var deleted = buffer.Delete(
                    count: buffer.Document.GetEndOfLinePosition());
                @event.GetApp().Clipboard.SetText(deleted);
                @event.GetApp().ViState.InputMode = InputMode.Insert;
                return null;
            });

        // D — delete to end of line
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('D')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var buffer = @event.CurrentBuffer!;
                var deleted = buffer.Delete(
                    count: buffer.Document.GetEndOfLinePosition());
                @event.GetApp().Clipboard.SetText(deleted);
                return null;
            });

        // ============================================================
        // Register-aware paste (T024): ",{reg},p and ",{reg},P
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('"'), new KeyOrChar(Keys.Any), new KeyOrChar('p')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var c = @event.KeySequence[1].Data;
                if (ViRegisterNames.Contains(c))
                {
                    var data = @event.GetApp().ViState.GetNamedRegister(c);
                    if (data is not null)
                    {
                        @event.CurrentBuffer!.PasteClipboardData(
                            data, count: @event.Arg,
                            pasteMode: PasteMode.ViAfter);
                    }
                }
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('"'), new KeyOrChar(Keys.Any), new KeyOrChar('P')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var c = @event.KeySequence[1].Data;
                if (ViRegisterNames.Contains(c))
                {
                    var data = @event.GetApp().ViState.GetNamedRegister(c);
                    if (data is not null)
                    {
                        @event.CurrentBuffer!.PasteClipboardData(
                            data, count: @event.Arg,
                            pasteMode: PasteMode.ViBefore);
                    }
                }
                return null;
            });

        // ============================================================
        // Single-character operations: x, X, s (T049)
        // ============================================================

        // x — delete char after cursor
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('x')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var buff = @event.CurrentBuffer!;
                var count = Math.Min(@event.Arg,
                    buff.Document.CurrentLineAfterCursor.Length);
                if (count > 0)
                {
                    var text = buff.Delete(count: count);
                    @event.GetApp().Clipboard.SetText(text);
                }
                return null;
            });

        // X — delete char before cursor
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('X')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var buff = @event.CurrentBuffer!;
                var count = Math.Min(@event.Arg,
                    buff.Document.CurrentLineBeforeCursor.Length);
                if (count > 0)
                {
                    var text = buff.DeleteBeforeCursor(count: count);
                    @event.GetApp().Clipboard.SetText(text);
                }
                return null;
            });

        // s — substitute (delete char + enter insert)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('s')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                var text = @event.CurrentBuffer!.Delete(count: @event.Arg);
                @event.GetApp().Clipboard.SetText(text);
                @event.GetApp().ViState.InputMode = InputMode.Insert;
                return null;
            });

        // ============================================================
        // Join: J, g,J in navigation mode (T050)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('J')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                for (int i = 0; i < @event.Arg; i++)
                    @event.CurrentBuffer!.JoinNextLine();
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('g'), new KeyOrChar('J')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                for (int i = 0; i < @event.Arg; i++)
                    @event.CurrentBuffer!.JoinNextLine(separator: "");
                return null;
            });

        // ============================================================
        // Paste: p, P (T035)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('p')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.PasteClipboardData(
                    @event.GetApp().Clipboard.GetData(),
                    count: @event.Arg,
                    pasteMode: PasteMode.ViAfter);
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('P')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.PasteClipboardData(
                    @event.GetApp().Clipboard.GetData(),
                    count: @event.Arg,
                    pasteMode: PasteMode.ViBefore);
                return null;
            });

        // ============================================================
        // Undo/Redo: u, Ctrl-R (T036)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('u')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                for (int i = 0; i < @event.Arg; i++)
                    @event.CurrentBuffer!.Undo();
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlR)],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                for (int i = 0; i < @event.Arg; i++)
                    @event.CurrentBuffer!.Redo();
                return null;
            });

        // ============================================================
        // Standalone tilde (when not operator): ~ (T051)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('~')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)TildeOperatorFilter).Invert())))(
            (@event) =>
            {
                var buffer = @event.CurrentBuffer!;
                var c = buffer.Document.CurrentChar;

                if (c != '\0' && c != '\n')
                {
                    var swapped = char.IsUpper(c)
                        ? char.ToLower(c).ToString()
                        : char.ToUpper(c).ToString();
                    buffer.InsertText(swapped, overwrite: true);
                }
                return null;
            });

        // ============================================================
        // Doubled-key transforms: guu, gUU, g~~ (T051)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('g'), new KeyOrChar('u'), new KeyOrChar('u')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                @event.CurrentBuffer!.TransformCurrentLine(s => s.ToLower());
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('g'), new KeyOrChar('U'), new KeyOrChar('U')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                @event.CurrentBuffer!.TransformCurrentLine(s => s.ToUpper());
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('g'), new KeyOrChar('~'), new KeyOrChar('~')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                @event.CurrentBuffer!.TransformCurrentLine(s =>
                    new string(s.Select(c =>
                        char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c))
                        .ToArray()));
                return null;
            });

        // ============================================================
        // Indent/Unindent: >>, << (T051)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('>'), new KeyOrChar('>')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var buffer = @event.CurrentBuffer!;
                var currentRow = buffer.Document.CursorPositionRow;
                BufferOperations.Indent(buffer, currentRow,
                    currentRow + @event.Arg);
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('<'), new KeyOrChar('<')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var currentRow = @event.CurrentBuffer!.Document.CursorPositionRow;
                BufferOperations.Unindent(@event.CurrentBuffer!, currentRow,
                    currentRow + @event.Arg);
                return null;
            });

        // ============================================================
        // Search: # (backward), * (forward) (T039)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('#')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var b = @event.CurrentBuffer!;
                var searchState = @event.GetApp().CurrentSearchState;
                searchState.Text = b.Document.GetWordUnderCursor();
                searchState.Direction = SearchDirection.Backward;
                b.ApplySearch(searchState, count: @event.Arg,
                    includeCurrentPosition: false);
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('*')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var b = @event.CurrentBuffer!;
                var searchState = @event.GetApp().CurrentSearchState;
                searchState.Text = b.Document.GetWordUnderCursor();
                searchState.Direction = SearchDirection.Forward;
                b.ApplySearch(searchState, count: @event.Arg,
                    includeCurrentPosition: false);
                return null;
            });

        // ============================================================
        // Numeric args: 1-9, 0 with has_arg (T054)
        // ============================================================

        for (int n = 1; n <= 9; n++)
        {
            var digit = (char)('0' + n);
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(digit)],
                filter: new FilterOrBool(
                    ((Filter)viNavMode)
                        .Or(viSelMode)
                        .Or(ViFilters.ViWaitingForTextObjectMode)))(
                (@event) =>
                {
                    @event.AppendToArgCount(digit.ToString());
                    return null;
                });
        }

        // 0 — appends to count when has_arg; otherwise it's a motion (handled in text objects)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('0')],
            filter: new FilterOrBool(
                ((Filter)AppFilters.HasArg).And(
                    ((Filter)viNavMode)
                        .Or(viSelMode)
                        .Or(ViFilters.ViWaitingForTextObjectMode))))(
            (@event) =>
            {
                @event.AppendToArgCount("0");
                return null;
            });

        // ============================================================
        // Increment/Decrement: Ctrl-A, Ctrl-X (T052)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlA)],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                IncrementOrDecrement(@event, increment: true);
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlX)],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                IncrementOrDecrement(@event, increment: false);
                return null;
            });

        // ============================================================
        // G with has_arg — go to nth history line (T055)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('G')],
            filter: new FilterOrBool(
                ((Filter)AppFilters.HasArg).And(viNavMode)))(
            (@event) =>
            {
                @event.CurrentBuffer!.GoToHistory(@event.Arg - 1);
                return null;
            });

        // ============================================================
        // Ctrl-O — temporary navigation mode from insert (T055)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlO)],
            filter: new FilterOrBool(
                ((Filter)ViFilters.ViInsertMode)
                    .Or(ViFilters.ViReplaceMode)))(
            (@event) =>
            {
                @event.GetApp().ViState.TemporaryNavigationMode = true;
                return null;
            });

        // ============================================================
        // Scroll z commands (T053)
        // ============================================================

        var viNavOrSel = new FilterOrBool(
            ((Filter)viNavMode).Or(viSelMode));

        // z,t / z,+ / z,Enter — scroll cursor to top
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('z'), new KeyOrChar('t')],
            filter: viNavOrSel)(
            (@event) =>
            {
                var w = @event.GetApp().Layout.CurrentWindow;
                w.VerticalScroll = @event.CurrentBuffer!.Document.CursorPositionRow;
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('z'), new KeyOrChar('+')],
            filter: viNavOrSel)(
            (@event) =>
            {
                var w = @event.GetApp().Layout.CurrentWindow;
                w.VerticalScroll = @event.CurrentBuffer!.Document.CursorPositionRow;
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('z'), new KeyOrChar(Keys.ControlM)],
            filter: viNavOrSel)(
            (@event) =>
            {
                var w = @event.GetApp().Layout.CurrentWindow;
                w.VerticalScroll = @event.CurrentBuffer!.Document.CursorPositionRow;
                return null;
            });

        // z,b / z,- — scroll cursor to bottom
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('z'), new KeyOrChar('b')],
            filter: viNavOrSel)(
            (@event) =>
            {
                @event.GetApp().Layout.CurrentWindow.VerticalScroll = 0;
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('z'), new KeyOrChar('-')],
            filter: viNavOrSel)(
            (@event) =>
            {
                @event.GetApp().Layout.CurrentWindow.VerticalScroll = 0;
                return null;
            });

        // z,z — scroll cursor to center
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('z'), new KeyOrChar('z')],
            filter: viNavOrSel)(
            (@event) =>
            {
                var w = @event.GetApp().Layout.CurrentWindow;
                var b = @event.CurrentBuffer!;

                if (w.RenderInfo is not null)
                {
                    var info = w.RenderInfo;
                    var scrollHeight = info.WindowHeight / 2;

                    var y = Math.Max(0, b.Document.CursorPositionRow - 1);
                    int height = 0;
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

                    w.VerticalScroll = y;
                }
                return null;
            });

        // ============================================================
        // Unknown text object catch-all (T055)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Any)],
            filter: new FilterOrBool(ViFilters.ViWaitingForTextObjectMode))(
            (@event) =>
            {
                @event.GetApp().Output.Bell();
                return null;
            });
    }

    /// <summary>
    /// Change current line: copy whole line, delete after whitespace, enter insert.
    /// </summary>
    private static void ChangeCurrentLine(KeyPressEvent @event)
    {
        var buffer = @event.CurrentBuffer!;

        // Copy the whole line to clipboard as linewise
        var data = new ClipboardData(
            buffer.Document.CurrentLine, SelectionType.Lines);
        @event.GetApp().Clipboard.SetData(data);

        // Delete after whitespace on current line
        buffer.CursorPosition +=
            buffer.Document.GetStartOfLinePosition(afterWhitespace: true);
        buffer.Delete(count: buffer.Document.GetEndOfLinePosition());
        @event.GetApp().ViState.InputMode = InputMode.Insert;
    }

    /// <summary>
    /// Increment or decrement the number at/after cursor.
    /// </summary>
    private static void IncrementOrDecrement(KeyPressEvent @event, bool increment)
    {
        var buffer = @event.CurrentBuffer!;
        var doc = buffer.Document;
        var line = doc.CurrentLineAfterCursor;

        // Find the first number on the current line at or after cursor
        int numStart = -1;
        int numEnd = -1;

        for (int i = 0; i < line.Length; i++)
        {
            if (char.IsDigit(line[i]))
            {
                numStart = i;
                // Check for negative sign
                if (numStart > 0 && line[numStart - 1] == '-')
                {
                    numStart--;
                }
                numEnd = i + 1;
                while (numEnd < line.Length && char.IsDigit(line[numEnd]))
                    numEnd++;
                break;
            }
        }

        if (numStart < 0)
            return; // No number found

        var numStr = line[numStart..numEnd];
        if (long.TryParse(numStr, out var num))
        {
            var newNum = increment ? num + @event.Arg : num - @event.Arg;
            var newStr = newNum.ToString();

            // Move to start of number and replace
            var posOffset = numStart;
            buffer.CursorPosition += posOffset;
            buffer.Delete(count: numEnd - numStart);
            buffer.InsertText(newStr);
            // Position cursor at end of inserted number - 1
            buffer.CursorPosition +=
                buffer.Document.GetCursorLeftPosition();
        }
    }
}
