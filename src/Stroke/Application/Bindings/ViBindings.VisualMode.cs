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
    /// Registers Vi visual mode bindings: selection extension (j, k),
    /// operations (x cut, J/g,J join), toggle (v, V, Ctrl-V),
    /// block selection insert/append (I, A), and auto-word (a,w / a,W).
    /// </summary>
    static partial void RegisterVisualMode(KeyBindings kb)
    {
        var viSelMode = ViFilters.ViSelectionMode;
        var isReadOnly = AppFilters.IsReadOnly;

        // ============================================================
        // Visual selection extend: j, k in selection mode (T043)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('k')],
            filter: new FilterOrBool(viSelMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.CursorUp(count: @event.Arg);
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('j')],
            filter: new FilterOrBool(viSelMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.CursorDown(count: @event.Arg);
                return null;
            });

        // ============================================================
        // Visual mode operations: x cut, J/g,J join (T044)
        // ============================================================

        // x in selection mode — cut selection
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('x')],
            filter: new FilterOrBool(viSelMode))(
            (@event) =>
            {
                var clipboardData = @event.CurrentBuffer!.CutSelection(viMode: true);
                @event.GetApp().Clipboard.SetData(clipboardData);
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('J')],
            filter: new FilterOrBool(
                ((Filter)viSelMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                @event.CurrentBuffer!.JoinSelectedLines();
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('g'), new KeyOrChar('J')],
            filter: new FilterOrBool(
                ((Filter)viSelMode).And(((Filter)isReadOnly).Invert())))(
            (@event) =>
            {
                @event.CurrentBuffer!.JoinSelectedLines(separator: "");
                return null;
            });

        // ============================================================
        // Visual mode toggle: v, V, Ctrl-V in selection mode (T045)
        // ============================================================

        // v in selection mode — toggle characters
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('v')],
            filter: new FilterOrBool(viSelMode))(
            (@event) =>
            {
                var selState = @event.CurrentBuffer!.SelectionState;
                if (selState is not null)
                {
                    if (selState.Type != SelectionType.Characters)
                    {
                        @event.CurrentBuffer!.SelectionState = new SelectionState(
                            selState.OriginalCursorPosition, SelectionType.Characters);
                    }
                    else
                    {
                        @event.CurrentBuffer!.ExitSelection();
                    }
                }
                return null;
            });

        // V in selection mode — toggle lines
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('V')],
            filter: new FilterOrBool(viSelMode))(
            (@event) =>
            {
                var selState = @event.CurrentBuffer!.SelectionState;
                if (selState is not null)
                {
                    if (selState.Type != SelectionType.Lines)
                    {
                        @event.CurrentBuffer!.SelectionState = new SelectionState(
                            selState.OriginalCursorPosition, SelectionType.Lines);
                    }
                    else
                    {
                        @event.CurrentBuffer!.ExitSelection();
                    }
                }
                return null;
            });

        // Ctrl-V in selection mode — toggle block
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlV)],
            filter: new FilterOrBool(viSelMode))(
            (@event) =>
            {
                var selState = @event.CurrentBuffer!.SelectionState;
                if (selState is not null)
                {
                    if (selState.Type != SelectionType.Block)
                    {
                        @event.CurrentBuffer!.SelectionState = new SelectionState(
                            selState.OriginalCursorPosition, SelectionType.Block);
                    }
                    else
                    {
                        @event.CurrentBuffer!.ExitSelection();
                    }
                }
                return null;
            });

        // ============================================================
        // Visual mode: a,w / a,W auto-word (T046)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('a'), new KeyOrChar('w')],
            filter: new FilterOrBool(viSelMode))(
            (@event) =>
            {
                var buffer = @event.CurrentBuffer!;
                if (buffer.SelectionState is not null &&
                    buffer.SelectionState.Type == SelectionType.Lines)
                {
                    buffer.SelectionState = new SelectionState(
                        buffer.SelectionState.OriginalCursorPosition,
                        SelectionType.Characters);
                }
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('a'), new KeyOrChar('W')],
            filter: new FilterOrBool(viSelMode))(
            (@event) =>
            {
                var buffer = @event.CurrentBuffer!;
                if (buffer.SelectionState is not null &&
                    buffer.SelectionState.Type == SelectionType.Lines)
                {
                    buffer.SelectionState = new SelectionState(
                        buffer.SelectionState.OriginalCursorPosition,
                        SelectionType.Characters);
                }
                return null;
            });
    }
}
