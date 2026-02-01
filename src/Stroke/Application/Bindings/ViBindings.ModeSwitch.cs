using Stroke.Core;
using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;

namespace Stroke.Application.Bindings;

public static partial class ViBindings
{
    /// <summary>
    /// Registers Vi mode switching bindings (Escape, i, I, a, A, o, O,
    /// v, V, Ctrl-V, r, R, Insert).
    /// </summary>
    static partial void RegisterModeSwitch(KeyBindings kb)
    {
        var viNavMode = ViFilters.ViNavigationMode;
        var viInsMode = ViFilters.ViInsertMode;
        var viSelMode = ViFilters.ViSelectionMode;
        var isReadOnly = AppFilters.IsReadOnly;

        // ============================================================
        // Escape — universal handler for returning to navigation mode
        // ============================================================

        // Escape: from any mode → Navigation.
        // In Insert/Replace: move cursor left by 1.
        // If selection active: clear it.
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape)],
            filter: default)( // No filter — always active
            (@event) =>
            {
                var buffer = @event.CurrentBuffer!;
                var viState = @event.GetApp().ViState;

                if (viState.InputMode == InputMode.Insert ||
                    viState.InputMode == InputMode.Replace)
                {
                    buffer.CursorPosition +=
                        buffer.Document.GetCursorLeftPosition();
                }

                viState.InputMode = InputMode.Navigation;

                if (buffer.SelectionState is not null)
                {
                    buffer.ExitSelection();
                }

                return null;
            });

        // ============================================================
        // Insert key toggle (Navigation ↔ Insert)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Insert)],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.GetApp().ViState.InputMode = InputMode.Insert;
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Insert)],
            filter: new FilterOrBool(viInsMode))(
            (@event) =>
            {
                @event.GetApp().ViState.InputMode = InputMode.Navigation;
                return null;
            });

        // ============================================================
        // i / I — enter insert mode
        // ============================================================

        // i — cursor stays
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('i')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                @event.GetApp().ViState.InputMode = InputMode.Insert;
                return null;
            });

        // I in navigation mode — cursor to first non-blank
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('I')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                @event.GetApp().ViState.InputMode = InputMode.Insert;
                @event.CurrentBuffer!.CursorPosition +=
                    @event.CurrentBuffer!.Document.GetStartOfLinePosition(
                        afterWhitespace: true);
                return null;
            });

        // I in block selection mode — enter insert-multiple
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('I')],
            filter: new FilterOrBool(
                ((Filter)InBlockSelection).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                InsertInBlockSelection(@event, after: false);
                return null;
            });

        // ============================================================
        // a / A — enter insert mode with cursor adjustment
        // ============================================================

        // a — cursor right one
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('a')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                @event.CurrentBuffer!.CursorPosition +=
                    @event.CurrentBuffer!.Document.GetCursorRightPosition();
                @event.GetApp().ViState.InputMode = InputMode.Insert;
                return null;
            });

        // A in navigation mode — cursor to end of line
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('A')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                @event.CurrentBuffer!.CursorPosition +=
                    @event.CurrentBuffer!.Document.GetEndOfLinePosition();
                @event.GetApp().ViState.InputMode = InputMode.Insert;
                return null;
            });

        // A in block selection mode — enter insert-multiple (after)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('A')],
            filter: new FilterOrBool(
                ((Filter)InBlockSelection).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                InsertInBlockSelection(@event, after: true);
                return null;
            });

        // ============================================================
        // o / O — open line below / above
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('o')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                @event.CurrentBuffer!.InsertLineBelow(
                    copyMargin: !AppFilters.InPasteMode.Invoke());
                @event.GetApp().ViState.InputMode = InputMode.Insert;
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('O')],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                @event.CurrentBuffer!.InsertLineAbove(
                    copyMargin: !AppFilters.InPasteMode.Invoke());
                @event.GetApp().ViState.InputMode = InputMode.Insert;
                return null;
            });

        // ============================================================
        // v / V / Ctrl-V — enter visual mode (from navigation)
        // ============================================================

        // v — enter character selection
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('v')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.StartSelection(
                    selectionType: SelectionType.Characters);
                return null;
            });

        // V — enter line selection
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('V')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.StartSelection(
                    selectionType: SelectionType.Lines);
                return null;
            });

        // Ctrl-V — enter block selection
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlV)],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.StartSelection(
                    selectionType: SelectionType.Block);
                return null;
            });

        // ============================================================
        // r / R — replace mode
        // ============================================================

        // r — replace single character
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('r')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.GetApp().ViState.InputMode = InputMode.ReplaceSingle;
                return null;
            });

        // R — replace mode (overwrite)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('R')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.GetApp().ViState.InputMode = InputMode.Replace;
                return null;
            });
    }

    /// <summary>
    /// Helper for I/A in block selection mode: enter InsertMultiple mode.
    /// </summary>
    private static void InsertInBlockSelection(KeyPressEvent @event, bool after)
    {
        var buff = @event.CurrentBuffer!;
        var positions = new List<int>();

        int index = 0;
        foreach (var (from, to) in buff.Document.SelectionRanges())
        {
            var pos = after ? to : from;
            positions.Add(pos);
            if (index == 0)
            {
                buff.CursorPosition = pos;
            }
            index++;
        }

        buff.MultipleCursorPositions = positions;
        @event.GetApp().ViState.InputMode = InputMode.InsertMultiple;
        buff.ExitSelection();
    }
}
