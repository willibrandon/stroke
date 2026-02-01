using Stroke.Core;
using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;

namespace Stroke.Application.Bindings;

public static partial class EmacsBindings
{
    /// <summary>
    /// Load Emacs shift-selection key bindings for selecting text with Shift+movement keys.
    /// </summary>
    /// <returns>
    /// An <see cref="IKeyBindingsBase"/> wrapping all 34 shift-selection bindings,
    /// conditional on <see cref="EmacsFilters.EmacsMode"/>.
    /// </returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>load_emacs_shift_selection_bindings()</c>.
    /// </remarks>
    public static IKeyBindingsBase LoadEmacsShiftSelectionBindings()
    {
        var kb = new KeyBindings();
        var notHasSelection = new FilterOrBool(AppFilters.HasSelection.Invert());
        var shiftMode = new FilterOrBool(SearchFilters.ShiftSelectionMode);

        // ============================================================
        // Start selection (10 bindings, filter: ~has_selection)
        // ============================================================

        Keys[] shiftKeys =
        [
            Keys.ShiftLeft, Keys.ShiftRight,
            Keys.ShiftUp, Keys.ShiftDown,
            Keys.ShiftHome, Keys.ShiftEnd,
            Keys.ControlShiftLeft, Keys.ControlShiftRight,
            Keys.ControlShiftHome, Keys.ControlShiftEnd,
        ];

        foreach (var key in shiftKeys)
        {
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(key)],
                filter: notHasSelection)(ShiftStartSelection);
        }

        // ============================================================
        // Extend selection (10 bindings, filter: shift_selection_mode)
        // ============================================================

        foreach (var key in shiftKeys)
        {
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(key)],
                filter: shiftMode)(ShiftExtendSelection);
        }

        // ============================================================
        // Replace/cancel (4 bindings, filter: shift_selection_mode)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Any)],
            filter: shiftMode)(ShiftReplaceSelection);

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlM)],
            filter: new FilterOrBool(
                ((Filter)SearchFilters.ShiftSelectionMode).And(AppFilters.IsMultiline)))(
            ShiftNewline);

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlH)],
            filter: shiftMode)(ShiftDelete);

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlY)],
            filter: shiftMode)(ShiftYank);

        // ============================================================
        // Cancel movement (10 bindings, filter: shift_selection_mode)
        // ============================================================

        Keys[] cancelKeys =
        [
            Keys.Left, Keys.Right,
            Keys.Up, Keys.Down,
            Keys.Home, Keys.End,
            Keys.ControlLeft, Keys.ControlRight,
            Keys.ControlHome, Keys.ControlEnd,
        ];

        foreach (var key in cancelKeys)
        {
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(key)],
                filter: shiftMode)(ShiftCancelMove);
        }

        return new ConditionalKeyBindings(kb, EmacsFilters.EmacsMode);
    }

    #region Shift-Selection Handler Methods

    /// <summary>
    /// Maps a shift-key event to its unshifted movement equivalent.
    /// </summary>
    private static void UnshiftMove(KeyPressEvent @event)
    {
        var keyOrChar = @event.KeySequence[0].Key;

        if (!keyOrChar.IsKey) return;
        var key = keyOrChar.Key;

        if (key == Keys.ShiftUp)
        {
            @event.CurrentBuffer!.AutoUp(count: @event.Arg);
            return;
        }
        if (key == Keys.ShiftDown)
        {
            @event.CurrentBuffer!.AutoDown(count: @event.Arg);
            return;
        }

        var keyToCommand = new Dictionary<Keys, string>
        {
            [Keys.ShiftLeft] = "backward-char",
            [Keys.ShiftRight] = "forward-char",
            [Keys.ShiftHome] = "beginning-of-line",
            [Keys.ShiftEnd] = "end-of-line",
            [Keys.ControlShiftLeft] = "backward-word",
            [Keys.ControlShiftRight] = "forward-word",
            [Keys.ControlShiftHome] = "beginning-of-buffer",
            [Keys.ControlShiftEnd] = "end-of-buffer",
        };

        if (keyToCommand.TryGetValue(key, out var commandName))
        {
            var binding = NamedCommands.GetByName(commandName);
            binding.Call(@event);
        }
    }

    /// <summary>
    /// Start shift-mode selection with Shift+movement key.
    /// </summary>
    private static NotImplementedOrNone? ShiftStartSelection(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        if (buff.Text.Length == 0) return null;

        buff.StartSelection(selectionType: SelectionType.Characters);

        if (buff.SelectionState is not null)
        {
            buff.SelectionState.EnterShiftMode();
        }

        var originalPosition = buff.CursorPosition;
        UnshiftMove(@event);
        if (buff.CursorPosition == originalPosition)
        {
            buff.ExitSelection();
        }
        return null;
    }

    /// <summary>
    /// Extend shift-mode selection.
    /// </summary>
    private static NotImplementedOrNone? ShiftExtendSelection(KeyPressEvent @event)
    {
        UnshiftMove(@event);
        var buff = @event.CurrentBuffer!;

        if (buff.SelectionState is not null)
        {
            if (buff.CursorPosition == buff.SelectionState.OriginalCursorPosition)
            {
                buff.ExitSelection();
            }
        }
        return null;
    }

    /// <summary>
    /// Replace selection with typed character.
    /// </summary>
    private static NotImplementedOrNone? ShiftReplaceSelection(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.CutSelection();
        NamedCommands.GetByName("self-insert").Call(@event);
        return null;
    }

    /// <summary>
    /// Replace selection with newline.
    /// </summary>
    private static NotImplementedOrNone? ShiftNewline(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.CutSelection();
        @event.CurrentBuffer!.Newline(copyMargin: !AppFilters.InPasteMode.Invoke());
        return null;
    }

    /// <summary>
    /// Delete selection.
    /// </summary>
    private static NotImplementedOrNone? ShiftDelete(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.CutSelection();
        return null;
    }

    /// <summary>
    /// Yank (paste) over selection.
    /// </summary>
    private static NotImplementedOrNone? ShiftYank(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        if (buff.SelectionState is not null)
        {
            buff.CutSelection();
        }
        NamedCommands.GetByName("yank").Call(@event);
        return null;
    }

    /// <summary>
    /// Cancel selection and re-feed the key press.
    /// </summary>
    private static NotImplementedOrNone? ShiftCancelMove(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.ExitSelection();
        var keyPress = @event.KeySequence[0];
        ((KeyProcessor)@event.KeyProcessor).Feed(keyPress, first: true);
        return null;
    }

    #endregion
}
