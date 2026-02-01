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
    /// Valid Vi register names: a-z and 0-9.
    /// </summary>
    private static readonly string ViRegisterNames =
        "abcdefghijklmnopqrstuvwxyz0123456789";

    /// <summary>
    /// Registers a text object with up to 3 handler registrations:
    /// operator-pending mode, navigation move, and selection extend.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>create_text_object_decorator</c>.
    /// </remarks>
    private static void RegisterTextObject(
        KeyBindings kb,
        KeyOrChar[] keys,
        Func<KeyPressEvent, TextObject> handler,
        FilterOrBool filter = default,
        bool noMoveHandler = false,
        bool noSelectionHandler = false,
        bool eager = false)
    {
        IFilter filterValue = filter.HasValue
            ? (filter.IsFilter ? filter.FilterValue! : (filter.BoolValue ? Always.Instance : Never.Instance))
            : Always.Instance;

        // 1. Operator-pending mode: execute the pending operator with this text object.
        var operatorPendingFilter = new FilterOrBool(
            ((Filter)ViFilters.ViWaitingForTextObjectMode).And(filterValue));
        var eagerFilter = eager ? new FilterOrBool(true) : default;

        kb.Add<KeyHandlerCallable>(keys, filter: operatorPendingFilter, eager: eagerFilter)(
            (@event) =>
            {
                var viState = @event.GetApp().ViState;

                // Arguments are multiplied.
                @event.SetArg((viState.OperatorArg ?? 1) * (@event.Arg));

                // Call the text object handler.
                var textObj = handler(@event);

                // Get the operator function.
                var operatorFunc = viState.OperatorFunc;

                if (textObj is not null && operatorFunc is not null)
                {
                    // Call the operator function with the text object.
                    operatorFunc(@event, textObj);
                }

                // Clear operator.
                @event.GetApp().ViState.OperatorFunc = null;
                @event.GetApp().ViState.OperatorArg = null;

                return null;
            });

        // 2. Navigation mode: move cursor by text object's start offset.
        if (!noMoveHandler)
        {
            var navFilter = new FilterOrBool(
                ((Filter)ViFilters.ViWaitingForTextObjectMode).Invert()
                    .And(filterValue)
                    .And(ViFilters.ViNavigationMode));

            kb.Add<KeyHandlerCallable>(keys, filter: navFilter, eager: eagerFilter)(
                (@event) =>
                {
                    var textObject = handler(@event);
                    @event.CurrentBuffer!.CursorPosition += textObject.Start;
                    return null;
                });
        }

        // 3. Selection mode: extend selection by text object.
        if (!noSelectionHandler)
        {
            var selFilter = new FilterOrBool(
                ((Filter)ViFilters.ViWaitingForTextObjectMode).Invert()
                    .And(filterValue)
                    .And(ViFilters.ViSelectionMode));

            kb.Add<KeyHandlerCallable>(keys, filter: selFilter, eager: eagerFilter)(
                (@event) =>
                {
                    var textObject = handler(@event);
                    var buff = @event.CurrentBuffer!;
                    var selectionState = buff.SelectionState;

                    if (selectionState is null)
                        return null;

                    // When the text object has both a start and end position
                    // (like 'iw' or 'i('), turn this into a selection.
                    if (textObject.End != 0)
                    {
                        var (start, end) = textObject.OperatorRange(buff.Document);
                        start += buff.CursorPosition;
                        end += buff.CursorPosition;

                        // Take selection type from text object.
                        var selType = textObject.Type == TextObjectType.Linewise
                            ? SelectionType.Lines
                            : SelectionType.Characters;

                        // Replace selection state with new positions and type.
                        buff.SelectionState = new SelectionState(start, selType);
                        buff.CursorPosition = end;
                    }
                    else
                    {
                        @event.CurrentBuffer!.CursorPosition += textObject.Start;
                    }

                    return null;
                });
        }
    }

    /// <summary>
    /// Registers an operator with 2 handler registrations:
    /// navigation mode (set pending) and selection mode (execute on selection).
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>create_operator_decorator</c>.
    /// </remarks>
    private static void RegisterOperator(
        KeyBindings kb,
        KeyOrChar[] keys,
        OperatorFuncDelegate operatorFunc,
        FilterOrBool filter = default,
        bool eager = false)
    {
        IFilter filterValue = filter.HasValue
            ? (filter.IsFilter ? filter.FilterValue! : (filter.BoolValue ? Always.Instance : Never.Instance))
            : Always.Instance;

        var eagerFilter = eager ? new FilterOrBool(true) : default;

        // 1. Navigation mode: store operator function for later execution with a text object.
        var navFilter = new FilterOrBool(
            ((Filter)ViFilters.ViWaitingForTextObjectMode).Invert()
                .And(filterValue)
                .And(ViFilters.ViNavigationMode));

        kb.Add<KeyHandlerCallable>(keys, filter: navFilter, eager: eagerFilter)(
            (@event) =>
            {
                @event.GetApp().ViState.OperatorFunc = operatorFunc;
                @event.GetApp().ViState.OperatorArg = @event.Arg;
                return null;
            });

        // 2. Selection mode: create TextObject from current selection and execute immediately.
        var selFilter = new FilterOrBool(
            ((Filter)ViFilters.ViWaitingForTextObjectMode).Invert()
                .And(filterValue)
                .And(ViFilters.ViSelectionMode));

        kb.Add<KeyHandlerCallable>(keys, filter: selFilter, eager: eagerFilter)(
            (@event) =>
            {
                var buff = @event.CurrentBuffer!;
                var selectionState = buff.SelectionState;

                if (selectionState is not null)
                {
                    // Create text object from selection.
                    TextObjectType textObjType;
                    if (selectionState.Type == SelectionType.Lines)
                    {
                        textObjType = TextObjectType.Linewise;
                    }
                    else if (selectionState.Type == SelectionType.Block)
                    {
                        textObjType = TextObjectType.Block;
                    }
                    else
                    {
                        textObjType = TextObjectType.Inclusive;
                    }

                    var textObject = new TextObject(
                        selectionState.OriginalCursorPosition - buff.CursorPosition,
                        type: textObjType);

                    // Execute operator.
                    operatorFunc(@event, textObject);

                    // Quit selection mode.
                    buff.ExitSelection();
                }

                return null;
            });
    }

    /// <summary>
    /// Creates and registers delete and change operators.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>create_delete_and_change_operators</c>.
    /// Called 4 times: (delete, no register), (change, no register),
    /// (delete, with register), (change, with register).
    /// </remarks>
    private static void CreateDeleteAndChangeOperators(
        KeyBindings kb,
        bool deleteOnly,
        bool withRegister)
    {
        KeyOrChar[] handlerKeys;
        if (withRegister)
        {
            handlerKeys = [
                new KeyOrChar('"'),
                new KeyOrChar(Keys.Any),
                new KeyOrChar(deleteOnly ? 'd' : 'c')
            ];
        }
        else
        {
            handlerKeys = [new KeyOrChar(deleteOnly ? 'd' : 'c')];
        }

        var isReadOnly = AppFilters.IsReadOnly;

        RegisterOperator(kb, handlerKeys, (@event, textObject) =>
        {
            ClipboardData? clipboardData = null;
            var buff = @event.CurrentBuffer!;

            if (textObject is not null)
            {
                var (newDocument, data) = textObject.Cut(buff);
                buff.Document = newDocument;
                clipboardData = data;
            }

            // Set deleted/changed text to clipboard or named register.
            if (clipboardData is not null && clipboardData.Text.Length > 0)
            {
                if (withRegister)
                {
                    var regName = @event.KeySequence[1].Data;
                    if (ViRegisterNames.Contains(regName))
                    {
                        @event.GetApp().ViState.SetNamedRegister(regName, clipboardData);
                    }
                }
                else
                {
                    @event.GetApp().Clipboard.SetData(clipboardData);
                }
            }

            // Only go back to insert mode in case of 'change'.
            if (!deleteOnly)
            {
                @event.GetApp().ViState.InputMode = InputMode.Insert;
            }

            return NotImplementedOrNone.None;
        }, filter: new FilterOrBool(((Filter)isReadOnly).Invert()));
    }

    /// <summary>
    /// Creates and registers transform operators from <see cref="ViTransformFunctions"/>.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>create_transform_handler</c>.
    /// Registers 5 operators: g,?, g,u, g,U, g,~, and ~ (when TildeOperator=true).
    /// </remarks>
    private static void CreateTransformHandlers(KeyBindings kb)
    {
        var isReadOnly = AppFilters.IsReadOnly;

        foreach (var (transformKeys, transformFilter, transformFunc) in ViTransformFunctions)
        {
            // Capture loop variables for closure.
            var keys = transformKeys;
            var func = transformFunc;

            RegisterOperator(kb, keys, (@event, textObject) =>
            {
                var buff = @event.CurrentBuffer!;
                var (start, end) = textObject.OperatorRange(buff.Document);

                if (start < end)
                {
                    // Transform.
                    buff.TransformRegion(
                        buff.CursorPosition + start,
                        buff.CursorPosition + end,
                        func);

                    // Move cursor.
                    buff.CursorPosition += textObject.End != 0 ? textObject.End : textObject.Start;
                }

                return NotImplementedOrNone.None;
            }, filter: new FilterOrBool(((Filter)transformFilter).And(((Filter)isReadOnly).Invert())));
        }
    }

    /// <summary>
    /// Registers all Vi operator bindings.
    /// </summary>
    static partial void RegisterOperators(KeyBindings kb)
    {
        var isReadOnly = AppFilters.IsReadOnly;

        // Delete and change operators (4 calls × 1 registration each = 4 operator registrations).
        CreateDeleteAndChangeOperators(kb, deleteOnly: true, withRegister: false);   // d
        CreateDeleteAndChangeOperators(kb, deleteOnly: false, withRegister: false);  // c
        CreateDeleteAndChangeOperators(kb, deleteOnly: true, withRegister: true);    // "x,d
        CreateDeleteAndChangeOperators(kb, deleteOnly: false, withRegister: true);   // "x,c

        // Transform operators (5 registrations from ViTransformFunctions).
        CreateTransformHandlers(kb);

        // Yank operator.
        RegisterOperator(kb, [new KeyOrChar('y')], (@event, textObject) =>
        {
            var (_, clipboardData) = textObject.Cut(@event.CurrentBuffer!);
            if (clipboardData.Text.Length > 0)
            {
                @event.GetApp().Clipboard.SetData(clipboardData);
            }
            return NotImplementedOrNone.None;
        });

        // Yank to named register.
        RegisterOperator(kb,
            [new KeyOrChar('"'), new KeyOrChar(Keys.Any), new KeyOrChar('y')],
            (@event, textObject) =>
            {
                var c = @event.KeySequence[1].Data;
                if (ViRegisterNames.Contains(c))
                {
                    var (_, clipboardData) = textObject.Cut(@event.CurrentBuffer!);
                    @event.GetApp().ViState.SetNamedRegister(c, clipboardData);
                }
                return NotImplementedOrNone.None;
            });

        // Indent operator.
        RegisterOperator(kb, [new KeyOrChar('>')], (@event, textObject) =>
        {
            var buff = @event.CurrentBuffer!;
            var (fromLine, toLine) = textObject.GetLineNumbers(buff);
            BufferOperations.Indent(buff, fromLine, toLine + 1, count: @event.Arg);
            return NotImplementedOrNone.None;
        });

        // Unindent operator.
        RegisterOperator(kb, [new KeyOrChar('<')], (@event, textObject) =>
        {
            var buff = @event.CurrentBuffer!;
            var (fromLine, toLine) = textObject.GetLineNumbers(buff);
            BufferOperations.Unindent(buff, fromLine, toLine + 1, count: @event.Arg);
            return NotImplementedOrNone.None;
        });

        // Reshape operator.
        RegisterOperator(kb,
            [new KeyOrChar('g'), new KeyOrChar('q')],
            (@event, textObject) =>
            {
                var buff = @event.CurrentBuffer!;
                var (fromLine, toLine) = textObject.GetLineNumbers(buff);
                BufferOperations.ReshapeText(buff, fromLine, toLine);
                return NotImplementedOrNone.None;
            });
    }
}
