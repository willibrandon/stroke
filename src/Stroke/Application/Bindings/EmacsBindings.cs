using Stroke.Clipboard;
using Stroke.Completion;
using Stroke.Core;
using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;

namespace Stroke.Application.Bindings;

/// <summary>
/// Key binding loaders for Emacs editing mode.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.emacs</c> module.
/// Provides two factory methods: <see cref="LoadEmacsBindings"/> for 78 core Emacs editing
/// bindings and <see cref="LoadEmacsShiftSelectionBindings"/> for 34 shift-selection bindings.
/// The third loader (<c>LoadEmacsSearchBindings</c>) is implemented in <see cref="SearchBindings"/>.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. Each factory method creates a new
/// <see cref="KeyBindings"/> instance on each call.
/// </para>
/// </remarks>
public static partial class EmacsBindings
{
    /// <summary>
    /// Dynamic condition: current buffer is returnable (has a return handler).
    /// </summary>
    private static readonly IFilter IsReturnable =
        new Condition(() => AppContext.GetApp().CurrentBuffer.IsReturnable);

    /// <summary>
    /// Dynamic condition: key processor arg is exactly "-" (negative prefix).
    /// </summary>
    private static readonly IFilter IsArg =
        new Condition(() => ((KeyProcessor)AppContext.GetApp().KeyProcessor).Arg == "-");

    /// <summary>
    /// Load core Emacs key bindings for movement, editing, kill ring, history,
    /// completion, selection, macros, character search, and numeric arguments.
    /// </summary>
    /// <returns>
    /// An <see cref="IKeyBindingsBase"/> wrapping all 78 core Emacs bindings,
    /// conditional on <see cref="EmacsFilters.EmacsMode"/>.
    /// </returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>load_emacs_bindings()</c>.
    /// </remarks>
    public static IKeyBindingsBase LoadEmacsBindings()
    {
        var kb = new KeyBindings();
        var insertMode = EmacsFilters.EmacsInsertMode;

        // ============================================================
        // Escape: no-op (must be first binding per NFR-003)
        // Prevents unhandled Escape from inserting into the input stream.
        // ============================================================

        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Escape)])(Ignore);

        // ============================================================
        // Movement bindings (12 registrations)
        // ============================================================

        kb.Add<Binding>([new KeyOrChar(Keys.ControlA)])(
            NamedCommands.GetByName("beginning-of-line"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlB)])(
            NamedCommands.GetByName("backward-char"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlE)])(
            NamedCommands.GetByName("end-of-line"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlF)])(
            NamedCommands.GetByName("forward-char"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlLeft)])(
            NamedCommands.GetByName("backward-word"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlRight)])(
            NamedCommands.GetByName("forward-word"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('b')])(
            NamedCommands.GetByName("backward-word"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('f')])(
            NamedCommands.GetByName("forward-word"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlHome)])(
            NamedCommands.GetByName("beginning-of-buffer"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlEnd)])(
            NamedCommands.GetByName("end-of-buffer"));

        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlN)])(AutoDown);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlP)])(AutoUp);

        // ============================================================
        // Editing bindings (5 registrations)
        // ============================================================

        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlDelete)],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("kill-word"));

        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlUnderscore)],
            saveBefore: _ => false,
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("undo"));

        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlX), new KeyOrChar(Keys.ControlU)],
            saveBefore: _ => false,
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("undo"));

        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('c')],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("capitalize-word"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('l')],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("downcase-word"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('u')],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("uppercase-word"));

        // ============================================================
        // Kill ring bindings (7 registrations)
        // ============================================================

        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('d')],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("kill-word"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.ControlH)],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("backward-kill-word"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('\\')],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("delete-horizontal-space"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlY)],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("yank"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('y')],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("yank-pop"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlX), new KeyOrChar('r'), new KeyOrChar('y')],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("yank"));

        // ============================================================
        // History bindings (7 registrations)
        // ============================================================

        var notHasSelection = new FilterOrBool(
            AppFilters.HasSelection.Invert());

        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('<')],
            filter: notHasSelection)(
            NamedCommands.GetByName("beginning-of-history"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('>')],
            filter: notHasSelection)(
            NamedCommands.GetByName("end-of-history"));

        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('.')],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("yank-last-arg"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('_')],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("yank-last-arg"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.ControlY)],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("yank-nth-arg"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('#')],
            filter: new FilterOrBool(insertMode))(
            NamedCommands.GetByName("insert-comment"));
        kb.Add<Binding>([new KeyOrChar(Keys.ControlO)])(
            NamedCommands.GetByName("operate-and-get-next"));

        // ============================================================
        // Quoted insert (1 registration)
        // ============================================================

        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlQ)],
            filter: new FilterOrBool(AppFilters.HasSelection.Invert()))(
            NamedCommands.GetByName("quoted-insert"));

        // ============================================================
        // Macro bindings (3 registrations)
        // ============================================================

        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlX), new KeyOrChar('(')])(
            NamedCommands.GetByName("start-kbd-macro"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlX), new KeyOrChar(')')])(
            NamedCommands.GetByName("end-kbd-macro"));
        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlX), new KeyOrChar('e')])(
            NamedCommands.GetByName("call-last-kbd-macro"));

        // ============================================================
        // Numeric arguments (22 registrations)
        // ============================================================

        for (int i = 0; i <= 9; i++)
        {
            char c = (char)('0' + i);
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.Escape), new KeyOrChar(c)])(HandleDigit);
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(c)],
                filter: new FilterOrBool(AppFilters.HasArg))(HandleDigit);
        }

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('-')],
            filter: new FilterOrBool(AppFilters.HasArg.Invert()))(MetaDash);

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('-')],
            filter: new FilterOrBool(IsArg))(DashWhenArg);

        // ============================================================
        // Accept-line bindings (2 registrations)
        // ============================================================

        kb.Add<Binding>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.ControlM)],
            filter: new FilterOrBool(
                ((Filter)insertMode).And(IsReturnable)))(
            NamedCommands.GetByName("accept-line"));

        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlM)],
            filter: new FilterOrBool(
                ((Filter)insertMode).And(IsReturnable).And(
                    AppFilters.IsMultiline.Invert())))(
            NamedCommands.GetByName("accept-line"));

        // ============================================================
        // Character search (2 registrations)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlSquareClose), new KeyOrChar(Keys.Any)])(GotoChar);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.ControlSquareClose),
             new KeyOrChar(Keys.Any)])(GotoCharBackwards);

        // ============================================================
        // Placeholder bindings (3 registrations)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('a')])(PrevSentence);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('e')])(EndOfSentence);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('t')],
            filter: new FilterOrBool(insertMode))(SwapCharacters);

        // ============================================================
        // Completion and miscellaneous (8 registrations)
        // ============================================================

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('*')],
            filter: new FilterOrBool(insertMode))(InsertAllCompletions);

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlX), new KeyOrChar(Keys.ControlX)])(ToggleStartEnd);

        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlAt)])(StartSelection);

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlG)],
            filter: new FilterOrBool(AppFilters.HasSelection.Invert()))(Cancel);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlG)],
            filter: new FilterOrBool(AppFilters.HasSelection))(CancelSelection);

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlW)],
            filter: new FilterOrBool(AppFilters.HasSelection))(CutSelection);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlX), new KeyOrChar('r'), new KeyOrChar('k')],
            filter: new FilterOrBool(AppFilters.HasSelection))(CutSelection);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('w')],
            filter: new FilterOrBool(AppFilters.HasSelection))(CopySelection);

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.Left)])(StartOfWord);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.Right)])(StartNextWord);

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('/')],
            filter: new FilterOrBool(insertMode))(Complete);

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlC), new KeyOrChar('>')],
            filter: new FilterOrBool(AppFilters.HasSelection))(IndentSelection);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlC), new KeyOrChar('<')],
            filter: new FilterOrBool(AppFilters.HasSelection))(UnindentSelection);

        return new ConditionalKeyBindings(kb, EmacsFilters.EmacsMode);
    }

    #region Private Handler Methods

    /// <summary>
    /// No-op handler for Escape key. Prevents unhandled Escape from
    /// inserting into the input stream.
    /// </summary>
    private static NotImplementedOrNone? Ignore(KeyPressEvent @event) => null;

    /// <summary>
    /// Move cursor down or navigate to next history entry.
    /// </summary>
    private static NotImplementedOrNone? AutoDown(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.AutoDown();
        return null;
    }

    /// <summary>
    /// Move cursor up or navigate to previous history entry.
    /// </summary>
    private static NotImplementedOrNone? AutoUp(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.AutoUp(count: @event.Arg);
        return null;
    }

    /// <summary>
    /// Handle numeric argument digit input (0-9).
    /// Accumulates digits into the event's argument count.
    /// </summary>
    private static NotImplementedOrNone? HandleDigit(KeyPressEvent @event)
    {
        @event.AppendToArgCount(@event.Data);
        return null;
    }

    /// <summary>
    /// Handle Meta+dash for negative argument prefix.
    /// Only sets the prefix if no argument is currently set.
    /// </summary>
    private static NotImplementedOrNone? MetaDash(KeyPressEvent @event)
    {
        if (!@event.ArgPresent)
        {
            @event.AppendToArgCount("-");
        }
        return null;
    }

    /// <summary>
    /// Handle dash when arg is exactly "-" (maintains negative state).
    /// </summary>
    private static NotImplementedOrNone? DashWhenArg(KeyPressEvent @event)
    {
        ((KeyProcessor)@event.KeyProcessor).Arg = "-";
        return null;
    }

    /// <summary>
    /// Search for a character on the current line.
    /// </summary>
    private static void CharacterSearch(Core.Buffer buff, string @char, int count)
    {
        int? match;
        if (count < 0)
        {
            match = buff.Document.FindBackwards(@char, inCurrentLine: true, count: -count);
        }
        else
        {
            match = buff.Document.Find(@char, inCurrentLine: true, count: count);
        }

        if (match is not null)
        {
            buff.CursorPosition += match.Value;
        }
    }

    /// <summary>
    /// Go to character forward (Ctrl-]).
    /// </summary>
    private static NotImplementedOrNone? GotoChar(KeyPressEvent @event)
    {
        CharacterSearch(@event.CurrentBuffer!, @event.Data, @event.Arg);
        return null;
    }

    /// <summary>
    /// Go to character backward (Meta-Ctrl-]).
    /// </summary>
    private static NotImplementedOrNone? GotoCharBackwards(KeyPressEvent @event)
    {
        CharacterSearch(@event.CurrentBuffer!, @event.Data, -@event.Arg);
        return null;
    }

    /// <summary>
    /// Move to previous sentence (placeholder — TODO in Python source).
    /// </summary>
    private static NotImplementedOrNone? PrevSentence(KeyPressEvent @event) => null;

    /// <summary>
    /// Move to end of sentence (placeholder — TODO in Python source).
    /// </summary>
    private static NotImplementedOrNone? EndOfSentence(KeyPressEvent @event) => null;

    /// <summary>
    /// Swap the last two words before the cursor (placeholder — TODO in Python source).
    /// </summary>
    private static NotImplementedOrNone? SwapCharacters(KeyPressEvent @event) => null;

    /// <summary>
    /// Insert all possible completions of the preceding text.
    /// </summary>
    private static NotImplementedOrNone? InsertAllCompletions(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;

        var completeEvent = new CompleteEvent(TextInserted: false, CompletionRequested: true);
        var completions = buff.Completer.GetCompletions(buff.Document, completeEvent).ToList();

        var textToInsert = string.Join(" ", completions.Select(c => c.Text));
        buff.InsertText(textToInsert);
        return null;
    }

    /// <summary>
    /// Toggle cursor between start and end of current line.
    /// </summary>
    private static NotImplementedOrNone? ToggleStartEnd(KeyPressEvent @event)
    {
        var buffer = @event.CurrentBuffer!;

        if (buffer.Document.IsCursorAtTheEndOfLine)
        {
            buffer.CursorPosition += buffer.Document.GetStartOfLinePosition(
                afterWhitespace: false);
        }
        else
        {
            buffer.CursorPosition += buffer.Document.GetEndOfLinePosition();
        }
        return null;
    }

    /// <summary>
    /// Start character selection at current cursor (if buffer is not empty).
    /// </summary>
    private static NotImplementedOrNone? StartSelection(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        if (buff.Text.Length > 0)
        {
            buff.StartSelection(selectionType: SelectionType.Characters);
        }
        return null;
    }

    /// <summary>
    /// Cancel completion menu and validation state.
    /// </summary>
    private static NotImplementedOrNone? Cancel(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.DismissCompletion();
        @event.CurrentBuffer!.DismissValidation();
        return null;
    }

    /// <summary>
    /// Cancel active selection.
    /// </summary>
    private static NotImplementedOrNone? CancelSelection(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.ExitSelection();
        return null;
    }

    /// <summary>
    /// Cut selected text to clipboard.
    /// </summary>
    private static NotImplementedOrNone? CutSelection(KeyPressEvent @event)
    {
        var data = @event.CurrentBuffer!.CutSelection();
        @event.GetApp().Clipboard.SetData(data);
        return null;
    }

    /// <summary>
    /// Copy selected text to clipboard without removing.
    /// </summary>
    private static NotImplementedOrNone? CopySelection(KeyPressEvent @event)
    {
        var data = @event.CurrentBuffer!.CopySelection();
        @event.GetApp().Clipboard.SetData(data);
        return null;
    }

    /// <summary>
    /// Move cursor to start of previous word.
    /// </summary>
    private static NotImplementedOrNone? StartOfWord(KeyPressEvent @event)
    {
        var buffer = @event.CurrentBuffer!;
        buffer.CursorPosition +=
            buffer.Document.FindPreviousWordBeginning(count: @event.Arg) ?? 0;
        return null;
    }

    /// <summary>
    /// Move cursor to start of next word.
    /// </summary>
    private static NotImplementedOrNone? StartNextWord(KeyPressEvent @event)
    {
        var buffer = @event.CurrentBuffer!;
        buffer.CursorPosition +=
            buffer.Document.FindNextWordBeginning(count: @event.Arg)
            ?? buffer.Document.GetEndOfDocumentPosition();
        return null;
    }

    /// <summary>
    /// Start or cycle completion.
    /// </summary>
    private static NotImplementedOrNone? Complete(KeyPressEvent @event)
    {
        var b = @event.CurrentBuffer!;
        if (b.CompleteState != null)
        {
            b.CompleteNext();
        }
        else
        {
            b.StartCompletion(selectFirst: true);
        }
        return null;
    }

    /// <summary>
    /// Indent selected text.
    /// </summary>
    private static NotImplementedOrNone? IndentSelection(KeyPressEvent @event)
    {
        var buffer = @event.CurrentBuffer!;

        buffer.CursorPosition += buffer.Document.GetStartOfLinePosition(
            afterWhitespace: true);

        var (from, to) = buffer.Document.SelectionRange();
        var (fromRow, _) = buffer.Document.TranslateIndexToPosition(from);
        var (toRow, _) = buffer.Document.TranslateIndexToPosition(to);

        BufferOperations.Indent(buffer, fromRow, toRow + 1, count: @event.Arg);
        return null;
    }

    /// <summary>
    /// Unindent selected text.
    /// </summary>
    private static NotImplementedOrNone? UnindentSelection(KeyPressEvent @event)
    {
        var buffer = @event.CurrentBuffer!;

        var (from, to) = buffer.Document.SelectionRange();
        var (fromRow, _) = buffer.Document.TranslateIndexToPosition(from);
        var (toRow, _) = buffer.Document.TranslateIndexToPosition(to);

        BufferOperations.Unindent(buffer, fromRow, toRow + 1, count: @event.Arg);
        return null;
    }

    #endregion
}
