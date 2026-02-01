using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;

namespace Stroke.Application.Bindings;

public static partial class ViBindings
{
    /// <summary>
    /// Registers Vi navigation bindings (direct handlers, not text objects).
    /// </summary>
    static partial void RegisterNavigation(KeyBindings kb)
    {
        var viNavMode = ViFilters.ViNavigationMode;
        var viSelMode = ViFilters.ViSelectionMode;

        // ============================================================
        // Arrow key navigation (Down, Ctrl-N, Up, Ctrl-P)
        // These are direct handlers, not text objects.
        // ============================================================

        // Up / Ctrl-P in navigation mode — auto up
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Up)],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.AutoUp(count: @event.Arg);
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlP)],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.AutoUp(count: @event.Arg);
                return null;
            });

        // k in navigation mode — auto up with go_to_start_of_line_if_history_changes
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('k')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.AutoUp(
                    count: @event.Arg,
                    goToStartOfLineIfHistoryChanges: true);
                return null;
            });

        // Down / Ctrl-N in navigation mode — auto down
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Down)],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.AutoDown(count: @event.Arg);
                return null;
            });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlN)],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.AutoDown(count: @event.Arg);
                return null;
            });

        // j in navigation mode — auto down with go_to_start_of_line_if_history_changes
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('j')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.AutoDown(
                    count: @event.Arg,
                    goToStartOfLineIfHistoryChanges: true);
                return null;
            });

        // ============================================================
        // Backspace in navigation mode
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlH)],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                @event.CurrentBuffer!.CursorPosition +=
                    @event.CurrentBuffer!.Document.GetCursorLeftPosition(count: @event.Arg);
                return null;
            });

        // ============================================================
        // Enter in navigation mode (when not returnable: go to next line)
        // ============================================================

        // Enter when returnable — accept line (handled via named commands)
        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlM)],
            filter: new FilterOrBool(
                ((Filter)viNavMode).And(IsReturnable)))(
            NamedCommands.GetByName("accept-line"));

        // Enter in insert mode (single-line, returnable) — accept line
        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlM)],
            filter: new FilterOrBool(
                ((Filter)IsReturnable).And(
                    ((Filter)AppFilters.IsMultiline).Invert())))(
            NamedCommands.GetByName("accept-line"));

        // Enter when not returnable — start of next line
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlM)],
            filter: new FilterOrBool(
                ((Filter)IsReturnable).Invert().And(viNavMode)))(
            (@event) =>
            {
                var b = @event.CurrentBuffer!;
                b.CursorDown(count: @event.Arg);
                b.CursorPosition += b.Document.GetStartOfLinePosition(afterWhitespace: true);
                return null;
            });

        // + — start of next line (same as Enter when not returnable)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('+')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var buffer = @event.CurrentBuffer!;
                buffer.CursorPosition += buffer.Document.GetCursorDownPosition(
                    count: @event.Arg);
                buffer.CursorPosition += buffer.Document.GetStartOfLinePosition(
                    afterWhitespace: true);
                return null;
            });

        // - — start of previous line
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('-')],
            filter: new FilterOrBool(viNavMode))(
            (@event) =>
            {
                var buffer = @event.CurrentBuffer!;
                buffer.CursorPosition += buffer.Document.GetCursorUpPosition(
                    count: @event.Arg);
                buffer.CursorPosition += buffer.Document.GetStartOfLinePosition(
                    afterWhitespace: true);
                return null;
            });

        // ============================================================
        // Sentence navigation stubs (( and ))
        // ============================================================

        // ( — begin of sentence (stub — TODO in Python source)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('(')],
            filter: new FilterOrBool(viNavMode))(
            (@event) => null);

        // ) — end of sentence (stub — TODO in Python source)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(')')],
            filter: new FilterOrBool(viNavMode))(
            (@event) => null);
    }
}
