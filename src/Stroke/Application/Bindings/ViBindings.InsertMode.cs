using Stroke.Clipboard;
using Stroke.Core;
using Stroke.Filters;
using Stroke.Input;
using Stroke.Input.Vt100;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;

namespace Stroke.Application.Bindings;

public static partial class ViBindings
{
    /// <summary>
    /// Registers Vi insert mode bindings: completion (Ctrl-N/P/G/Y/E),
    /// quoted-insert (Ctrl-V), indent/unindent (Ctrl-T/D), line/file completion
    /// stubs, replace mode, replace-single, insert-multiple mode handlers,
    /// digraph input (Ctrl-K), and macro recording/playback.
    /// </summary>
    static partial void RegisterInsertMode(KeyBindings kb)
    {
        var viInsMode = ViFilters.ViInsertMode;
        var viRepMode = ViFilters.ViReplaceMode;
        var viRepSingleMode = ViFilters.ViReplaceSingleMode;
        var viInsMultiMode = ViFilters.ViInsertMultipleMode;
        var viDigraphMode = ViFilters.ViDigraphMode;
        var viNavMode = ViFilters.ViNavigationMode;
        var viRecMacro = ViFilters.ViRecordingMacro;
        var isReadOnly = AppFilters.IsReadOnly;

        // ============================================================
        // Insert mode: quoted-insert (Ctrl-V)
        // ============================================================

        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlV)],
            filter: new FilterOrBool(viInsMode))(
            NamedCommands.GetByName("quoted-insert"));

        // ============================================================
        // Insert mode: completion (Ctrl-N, Ctrl-P, Ctrl-G/Y, Ctrl-E)
        // ============================================================

        // Ctrl-N — complete next
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlN)],
            filter: new FilterOrBool(viInsMode))(
            (@event) =>
            {
                var b = @event.CurrentBuffer!;
                if (b.CompleteState is not null)
                {
                    b.CompleteNext();
                }
                else
                {
                    b.StartCompletion(selectFirst: true);
                }
                return null;
            });

        // Ctrl-P — complete previous
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlP)],
            filter: new FilterOrBool(viInsMode))(
            (@event) =>
            {
                var b = @event.CurrentBuffer!;
                if (b.CompleteState is not null)
                {
                    b.CompletePrevious();
                }
                else
                {
                    b.StartCompletion(selectLast: true);
                }
                return null;
            });

        // Ctrl-G / Ctrl-Y — accept current completion
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlG)],
            filter: new FilterOrBool(viInsMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.DismissCompletion();
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlY)],
            filter: new FilterOrBool(viInsMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.DismissCompletion();
                return null;
            });

        // Ctrl-E — cancel completion
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlE)],
            filter: new FilterOrBool(viInsMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.CancelCompletion();
                return null;
            });

        // ============================================================
        // Insert mode: indent/unindent (Ctrl-T/D) and Enter
        // ============================================================

        // Ctrl-T — indent
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlT)],
            filter: new FilterOrBool(viInsMode))(
            (@event) =>
            {
                var buffer = @event.CurrentBuffer!;
                var currentRow = buffer.Document.CursorPositionRow;
                BufferOperations.Indent(buffer, currentRow,
                    currentRow + @event.Arg);
                return null;
            });

        // Ctrl-D — unindent
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlD)],
            filter: new FilterOrBool(viInsMode))(
            (@event) =>
            {
                var currentRow = @event.CurrentBuffer!.Document.CursorPositionRow;
                BufferOperations.Unindent(@event.CurrentBuffer!, currentRow,
                    currentRow + @event.Arg);
                return null;
            });

        // ============================================================
        // Insert mode: line/file completion stubs (Ctrl-X Ctrl-L, Ctrl-X Ctrl-F)
        // ============================================================

        // Ctrl-X Ctrl-L — line completion (stub)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlX), new KeyOrChar(Keys.ControlL)],
            filter: new FilterOrBool(viInsMode))(
            (@event) =>
            {
                // TODO: start_history_lines_completion not yet ported
                return null;
            });

        // Ctrl-X Ctrl-F — file completion (stub per Python source)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlX), new KeyOrChar(Keys.ControlF)],
            filter: new FilterOrBool(viInsMode))(
            (@event) =>
            {
                // TODO per Python source
                return null;
            });

        // ============================================================
        // Replace mode: Any key overwrites (T059)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Any)],
            filter: new FilterOrBool(viRepMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.InsertText(@event.Data, overwrite: true);
                return null;
            });

        // ============================================================
        // Replace-single mode: Any key overwrites, cursor back, return to nav (T059)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Any)],
            filter: new FilterOrBool(viRepSingleMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.InsertText(@event.Data, overwrite: true);
                @event.CurrentBuffer!.CursorPosition -= 1;
                @event.GetApp().ViState.InputMode = InputMode.Navigation;
                return null;
            });

        // ============================================================
        // Insert-multiple mode (T059)
        // ============================================================

        // Any key — insert at multiple cursors
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Any)],
            filter: new FilterOrBool(viInsMultiMode))(
            (@event) =>
            {
                InsertTextMultipleCursors(@event);
                return null;
            });

        // Backspace — delete before multiple cursors
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlH)],
            filter: new FilterOrBool(viInsMultiMode))(
            (@event) =>
            {
                DeleteBeforeMultipleCursors(@event);
                return null;
            });

        // Delete — delete after multiple cursors
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Delete)],
            filter: new FilterOrBool(viInsMultiMode))(
            (@event) =>
            {
                DeleteAfterMultipleCursors(@event);
                return null;
            });

        // Left — move all cursors left
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Left)],
            filter: new FilterOrBool(viInsMultiMode))(
            (@event) =>
            {
                var buff = @event.CurrentBuffer!;
                var newPositions = new List<int>();
                foreach (var p in buff.MultipleCursorPositions)
                {
                    var pos = p;
                    if (buff.Document.TranslateIndexToPosition(pos).Col > 0)
                        pos -= 1;
                    newPositions.Add(pos);
                }
                buff.MultipleCursorPositions = newPositions;
                if (buff.Document.CursorPositionCol > 0)
                    buff.CursorPosition -= 1;
                return null;
            });

        // Right — move all cursors right
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Right)],
            filter: new FilterOrBool(viInsMultiMode))(
            (@event) =>
            {
                var buff = @event.CurrentBuffer!;
                var newPositions = new List<int>();
                foreach (var p in buff.MultipleCursorPositions)
                {
                    var pos = p;
                    var (row, column) = buff.Document.TranslateIndexToPosition(pos);
                    if (column < buff.Document.Lines[row].Length)
                        pos += 1;
                    newPositions.Add(pos);
                }
                buff.MultipleCursorPositions = newPositions;
                if (!buff.Document.IsCursorAtTheEndOfLine)
                    buff.CursorPosition += 1;
                return null;
            });

        // Up/Down — no-op in insert-multiple mode
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Up)],
            filter: new FilterOrBool(viInsMultiMode))(
            (@event) => null);

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Down)],
            filter: new FilterOrBool(viInsMultiMode))(
            (@event) => null);

        // ============================================================
        // Digraph mode (T060)
        // ============================================================

        // Ctrl-K — enter digraph mode
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlK)],
            filter: new FilterOrBool(
                ((Filter)viInsMode).Or(viRepMode)))(
            (@event) =>
            {
                @event.GetApp().ViState.WaitingForDigraph = true;
                return null;
            });

        // First digraph symbol
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Any)],
            filter: new FilterOrBool(
                ((Filter)viDigraphMode).And(
                    ((Filter)DigraphSymbol1Given).Invert())))(
            (@event) =>
            {
                @event.GetApp().ViState.DigraphSymbol1 = @event.Data;
                return null;
            });

        // Second digraph symbol — lookup and insert
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Any)],
            filter: new FilterOrBool(
                ((Filter)viDigraphMode).And(DigraphSymbol1Given)))(
            (@event) =>
            {
                try
                {
                    var symbol1 = @event.GetApp().ViState.DigraphSymbol1 ?? "";
                    var symbol2 = @event.Data;

                    // Lookup digraph
                    int? codePoint = null;
                    if (symbol1.Length > 0 && symbol2.Length > 0)
                    {
                        codePoint = Digraphs.Lookup(symbol1[0], symbol2[0]);
                        if (codePoint is null)
                        {
                            // Try reversing
                            codePoint = Digraphs.Lookup(symbol2[0], symbol1[0]);
                        }
                    }

                    if (codePoint is not null)
                    {
                        var overwrite =
                            @event.GetApp().ViState.InputMode == InputMode.Replace;
                        @event.CurrentBuffer!.InsertText(
                            char.ConvertFromUtf32(codePoint.Value),
                            overwrite: overwrite);
                    }
                    else
                    {
                        @event.GetApp().Output.Bell();
                    }
                }
                finally
                {
                    @event.GetApp().ViState.WaitingForDigraph = false;
                    @event.GetApp().ViState.DigraphSymbol1 = null;
                }
                return null;
            });

        // ============================================================
        // Macro recording/playback (T041)
        // ============================================================

        // q,{reg} — start recording
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('q'), new KeyOrChar(Keys.Any)],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)viRecMacro).Invert())))(
            (@event) =>
            {
                var c = @event.KeySequence[1].Data;
                if (ViRegisterNames.Contains(c))
                {
                    var viState = @event.GetApp().ViState;
                    viState.RecordingRegister = c;
                    viState.CurrentRecording = "";
                }
                return null;
            });

        // q — stop recording
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('q')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(viRecMacro)))(
            (@event) =>
            {
                var viState = @event.GetApp().ViState;
                if (viState.RecordingRegister is not null)
                {
                    viState.SetNamedRegister(
                        viState.RecordingRegister,
                        new ClipboardData(viState.CurrentRecording));
                    viState.RecordingRegister = null;
                    viState.CurrentRecording = "";
                }
                return null;
            });

        // @,{reg} — execute macro
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('@'), new KeyOrChar(Keys.Any)],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var c = @event.KeySequence[1].Data;
                var macroData = @event.GetApp().ViState.GetNamedRegister(c);
                if (macroData is not null && macroData.Text.Length > 0)
                {
                    // Parse macro text into Input.KeyPress via VT100 parser,
                    // then convert to KeyBinding.KeyPress for the KeyProcessor.
                    var inputKeys = new List<Stroke.Input.KeyPress>();
                    var parser = new Vt100Parser(inputKeys.Add);
                    parser.Feed(macroData.Text);
                    parser.Flush();

                    var kbKeys = inputKeys.Select(ik =>
                        new KeyBinding.KeyPress(
                            new KeyOrChar(ik.Key), ik.Data)).ToList();

                    for (int i = 0; i < @event.Arg; i++)
                    {
                        @event.GetApp().KeyProcessor.FeedMultiple(kbKeys, first: true);
                    }
                }
                return null;
            });
    }

    /// <summary>
    /// Insert text at multiple cursor positions.
    /// </summary>
    private static void InsertTextMultipleCursors(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        var originalText = buff.Text;
        var positions = buff.MultipleCursorPositions;

        // Construct new text
        var parts = new List<string>();
        int p = 0;
        foreach (var p2 in positions)
        {
            parts.Add(originalText[p..p2]);
            parts.Add(@event.Data);
            p = p2;
        }
        parts.Add(originalText[p..]);

        // Shift all cursor positions
        var newPositions = new List<int>();
        for (int i = 0; i < positions.Count; i++)
        {
            newPositions.Add(positions[i] + i + 1);
        }

        buff.Text = string.Join("", parts);
        buff.MultipleCursorPositions = newPositions;
        buff.CursorPosition += 1;
    }

    /// <summary>
    /// Backspace using multiple cursors.
    /// </summary>
    private static void DeleteBeforeMultipleCursors(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        var originalText = buff.Text;
        var positions = buff.MultipleCursorPositions;

        var parts = new List<string>();
        bool deletedSomething = false;
        int p = 0;

        foreach (var p2 in positions)
        {
            if (p2 > 0 && originalText[p2 - 1] != '\n')
            {
                parts.Add(originalText[p..(p2 - 1)]);
                deletedSomething = true;
            }
            else
            {
                parts.Add(originalText[p..p2]);
            }
            p = p2;
        }
        parts.Add(originalText[p..]);

        if (deletedSomething)
        {
            // Shift cursor positions by accumulating lengths
            var newPositions = new List<int>();
            int cumLen = 0;
            for (int i = 0; i < parts.Count - 1; i++)
            {
                cumLen += parts[i].Length;
                newPositions.Add(cumLen);
            }

            buff.Text = string.Join("", parts);
            buff.MultipleCursorPositions = newPositions;
            buff.CursorPosition -= 1;
        }
        else
        {
            @event.GetApp().Output.Bell();
        }
    }

    /// <summary>
    /// Delete using multiple cursors.
    /// </summary>
    private static void DeleteAfterMultipleCursors(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        var originalText = buff.Text;
        var positions = buff.MultipleCursorPositions;

        var parts = new List<string>();
        bool deletedSomething = false;
        int p = 0;

        foreach (var p2 in positions)
        {
            parts.Add(originalText[p..p2]);
            if (p2 >= originalText.Length || originalText[p2] == '\n')
            {
                p = p2;
            }
            else
            {
                p = p2 + 1;
                deletedSomething = true;
            }
        }
        parts.Add(originalText[p..]);

        if (deletedSomething)
        {
            var newPositions = new List<int>();
            int cumLen = 0;
            for (int i = 0; i < parts.Count - 1; i++)
            {
                cumLen += parts[i].Length;
                newPositions.Add(cumLen);
            }

            buff.Text = string.Join("", parts);
            buff.MultipleCursorPositions = newPositions;
        }
        else
        {
            @event.GetApp().Output.Bell();
        }
    }
}
