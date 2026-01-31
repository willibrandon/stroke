using Stroke.Clipboard;
using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using KeyPress = Stroke.KeyBinding.KeyPress;

namespace Stroke.Application.Bindings;

/// <summary>
/// Key binding loader for basic key bindings shared between Emacs and Vi modes.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.basic</c> module.
/// Provides a single factory method that creates and returns a <see cref="KeyBindings"/>
/// instance containing all basic key bindings: ignored keys, readline movement/editing,
/// self-insert, tab completion, history navigation, auto up/down, selection delete,
/// Ctrl+D, Enter multiline, Ctrl+J re-dispatch, Ctrl+Z literal, bracketed paste,
/// and quoted insert.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. The factory method creates a
/// new <see cref="KeyBindings"/> instance on each call.
/// </para>
/// </remarks>
public static class BasicBindings
{
    /// <summary>
    /// Composite filter: Vi insert mode OR Emacs insert mode.
    /// </summary>
    private static readonly IFilter InsertMode =
        ((Filter)ViFilters.ViInsertMode) | EmacsFilters.EmacsInsertMode;

    /// <summary>
    /// Dynamic condition: current buffer has any text.
    /// Despite the name (retained for Python API fidelity), this checks whether the
    /// buffer contains any text at all, not specifically text before the cursor position.
    /// </summary>
    private static readonly IFilter HasTextBeforeCursor =
        new Condition(() => AppContext.GetApp().CurrentBuffer.Text.Length > 0);

    /// <summary>
    /// Dynamic condition: quoted insert mode is active.
    /// </summary>
    private static readonly IFilter InQuotedInsert =
        new Condition(() => AppContext.GetApp().QuotedInsert);

    /// <summary>
    /// Save-before callback that returns false for repeated events,
    /// preventing excessive undo snapshots during rapid key presses.
    /// </summary>
    private static bool IfNoRepeat(KeyPressEvent @event) => !@event.IsRepeat;

    /// <summary>
    /// Shared no-op handler for ignored keys. Prevents keys from falling through
    /// to the self-insert handler.
    /// </summary>
    private static NotImplementedOrNone? Ignore(KeyPressEvent @event) => null;

    /// <summary>
    /// Load basic key bindings shared by Emacs and Vi modes.
    /// </summary>
    /// <returns>
    /// A new <see cref="KeyBindings"/> instance containing all 118 basic key bindings.
    /// </returns>
    public static KeyBindings LoadBasicBindings()
    {
        var kb = new KeyBindings();

        // ============================================================
        // Group 1: Ignored keys (90 bindings)
        // Don't do anything by default. Don't catch them in the 'Any'
        // handler which would insert them as data.
        // ============================================================

        // 26 control keys: Ctrl+A through Ctrl+Z
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlA)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlB)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlC)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlD)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlE)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlF)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlG)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlH)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlI)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlJ)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlK)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlL)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlM)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlN)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlO)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlP)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlQ)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlR)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlS)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlT)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlU)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlV)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlW)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlX)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlY)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlZ)])(Ignore);

        // 24 function keys: F1 through F24
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F1)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F2)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F3)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F4)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F5)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F6)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F7)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F8)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F9)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F10)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F11)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F12)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F13)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F14)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F15)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F16)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F17)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F18)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F19)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F20)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F21)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F22)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F23)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F24)])(Ignore);

        // 5 control-punctuation keys
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlAt)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlBackslash)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlSquareClose)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlCircumflex)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlUnderscore)])(Ignore);

        // 5 base navigation keys (Backspace = ControlH in Keys enum)
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlH)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Up)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Down)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Right)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Left)])(Ignore);

        // 4 shift-arrow keys
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ShiftUp)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ShiftDown)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ShiftRight)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ShiftLeft)])(Ignore);

        // 4 home/end keys (Home, End, Shift+Home, Shift+End)
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Home)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.End)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ShiftHome)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ShiftEnd)])(Ignore);

        // 3 delete variants
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Delete)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ShiftDelete)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlDelete)])(Ignore);

        // 2 page keys
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.PageUp)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.PageDown)])(Ignore);

        // 2 tab keys
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.BackTab)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlI)])(Ignore);

        // 4 ctrl+shift navigation keys
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlShiftLeft)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlShiftRight)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlShiftHome)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlShiftEnd)])(Ignore);

        // 6 ctrl navigation keys
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlLeft)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlRight)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlUp)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlDown)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlHome)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlEnd)])(Ignore);

        // 3 insert variants
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Insert)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ShiftInsert)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlInsert)])(Ignore);

        // SIGINT and Ignore
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.SIGINT)])(Ignore);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Ignore)])(Ignore);

        // ============================================================
        // Group 2: Readline movement bindings (7 bindings, no filter)
        // ============================================================

        kb.Add<Binding>([new KeyOrChar(Keys.Home)])(
            NamedCommands.GetByName("beginning-of-line"));
        kb.Add<Binding>([new KeyOrChar(Keys.End)])(
            NamedCommands.GetByName("end-of-line"));
        kb.Add<Binding>([new KeyOrChar(Keys.Left)])(
            NamedCommands.GetByName("backward-char"));
        kb.Add<Binding>([new KeyOrChar(Keys.Right)])(
            NamedCommands.GetByName("forward-char"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlUp)])(
            NamedCommands.GetByName("previous-history"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlDown)])(
            NamedCommands.GetByName("next-history"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlL)])(
            NamedCommands.GetByName("clear-screen"));

        // ============================================================
        // Group 3: Readline editing bindings (7 bindings, filter: InsertMode)
        // Plus self-insert, tab completion, and Ctrl+W — matching Python
        // source order exactly (lines 157-177 of basic.py).
        // ============================================================

        var insertModeFilter = new FilterOrBool(InsertMode);

        kb.Add<Binding>([new KeyOrChar(Keys.ControlK)], filter: insertModeFilter)(
            NamedCommands.GetByName("kill-line"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlU)], filter: insertModeFilter)(
            NamedCommands.GetByName("unix-line-discard"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlH)], filter: insertModeFilter,
            saveBefore: IfNoRepeat)(
            NamedCommands.GetByName("backward-delete-char"));
        kb.Add<Binding>([new KeyOrChar(Keys.Delete)], filter: insertModeFilter,
            saveBefore: IfNoRepeat)(
            NamedCommands.GetByName("delete-char"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlDelete)], filter: insertModeFilter,
            saveBefore: IfNoRepeat)(
            NamedCommands.GetByName("delete-char"));

        // Group 4: Self-insert (1 binding) — registered here per Python source order
        kb.Add<Binding>([new KeyOrChar(Keys.Any)], filter: insertModeFilter,
            saveBefore: IfNoRepeat)(
            NamedCommands.GetByName("self-insert"));

        // Remaining editing bindings (continuing Python source order)
        kb.Add<Binding>([new KeyOrChar(Keys.ControlT)], filter: insertModeFilter)(
            NamedCommands.GetByName("transpose-chars"));

        // Group 5: Tab completion (2 bindings, filter: InsertMode)
        kb.Add<Binding>([new KeyOrChar(Keys.ControlI)], filter: insertModeFilter)(
            NamedCommands.GetByName("menu-complete"));
        kb.Add<Binding>([new KeyOrChar(Keys.BackTab)], filter: insertModeFilter)(
            NamedCommands.GetByName("menu-complete-backward"));

        // Ctrl+W (continuing Python source order)
        kb.Add<Binding>([new KeyOrChar(Keys.ControlW)], filter: insertModeFilter)(
            NamedCommands.GetByName("unix-word-rubout"));

        // ============================================================
        // Group 6: History navigation (2 bindings, filter: ~HasSelection)
        // ============================================================

        var notHasSelection = new FilterOrBool(AppFilters.HasSelection.Invert());

        kb.Add<Binding>([new KeyOrChar(Keys.PageUp)], filter: notHasSelection)(
            NamedCommands.GetByName("previous-history"));
        kb.Add<Binding>([new KeyOrChar(Keys.PageDown)], filter: notHasSelection)(
            NamedCommands.GetByName("next-history"));

        // ============================================================
        // Group 7: Ctrl+D (1 binding, filter: HasTextBeforeCursor & InsertMode)
        // ============================================================

        var ctrlDFilter = new FilterOrBool(
            ((Filter)HasTextBeforeCursor).And(InsertMode));

        kb.Add<Binding>([new KeyOrChar(Keys.ControlD)], filter: ctrlDFilter)(
            NamedCommands.GetByName("delete-char"));

        // ============================================================
        // Group 8: Enter multiline (1 binding, filter: InsertMode & IsMultiline)
        // ============================================================

        var enterFilter = new FilterOrBool(
            ((Filter)InsertMode).And(AppFilters.IsMultiline));

        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlM)],
            filter: enterFilter)(@event =>
        {
            @event.CurrentBuffer!.Newline(
                copyMargin: !AppFilters.InPasteMode.Invoke());
            return null;
        });

        // ============================================================
        // Group 9: Ctrl+J re-dispatch (1 binding, no filter)
        // ============================================================

        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlJ)])(@event =>
        {
            ((KeyProcessor)@event.KeyProcessor).Feed(
                new KeyPress(Keys.ControlM, "\r"), first: true);
            return null;
        });

        // ============================================================
        // Group 10: Auto up/down (2 bindings, no filter)
        // ============================================================

        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Up)])(@event =>
        {
            @event.CurrentBuffer!.AutoUp(count: @event.Arg);
            return null;
        });

        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Down)])(@event =>
        {
            @event.CurrentBuffer!.AutoDown(count: @event.Arg);
            return null;
        });

        // ============================================================
        // Group 11: Delete selection (1 binding, filter: HasSelection)
        // ============================================================

        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Delete)],
            filter: new FilterOrBool(AppFilters.HasSelection))(@event =>
        {
            ClipboardData data = @event.CurrentBuffer!.CutSelection();
            @event.GetApp().Clipboard.SetData(data);
            return null;
        });

        // ============================================================
        // Group 12: Ctrl+Z literal insert (1 binding, no filter)
        // ============================================================

        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlZ)])(@event =>
        {
            @event.CurrentBuffer!.InsertText(@event.Data);
            return null;
        });

        // ============================================================
        // Group 13: Bracketed paste (1 binding, no filter)
        // ============================================================

        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.BracketedPaste)])(@event =>
        {
            var data = @event.Data;

            // Be sure to use \n as line ending.
            // Some terminals (like iTerm2) seem to paste \r\n line endings
            // in a bracketed paste.
            data = data.Replace("\r\n", "\n");
            data = data.Replace("\r", "\n");

            @event.CurrentBuffer!.InsertText(data);
            return null;
        });

        // ============================================================
        // Group 14: Quoted insert (1 binding, filter: InQuotedInsert, eager: true)
        // ============================================================

        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Any)],
            filter: new FilterOrBool(InQuotedInsert),
            eager: true)(@event =>
        {
            @event.CurrentBuffer!.InsertText(@event.Data, overwrite: false);
            @event.GetApp().QuotedInsert = false;
            return null;
        });

        return kb;
    }
}
