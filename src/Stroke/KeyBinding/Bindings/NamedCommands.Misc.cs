using Stroke.Core;

namespace Stroke.KeyBinding.Bindings;

public static partial class NamedCommands
{
    /// <summary>
    /// Registers the 7 miscellaneous commands.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's miscellaneous commands from
    /// <c>named_commands.py</c> lines 597-692.
    /// </remarks>
    static partial void RegisterMiscCommands()
    {
        RegisterInternal("undo", Undo);
        RegisterInternal("insert-comment", InsertComment);
        RegisterInternal("vi-editing-mode", ViEditingMode);
        RegisterInternal("emacs-editing-mode", EmacsEditingMode);
        RegisterInternal("prefix-meta", PrefixMeta);
        RegisterInternal("operate-and-get-next", OperateAndGetNext);
        RegisterInternal("edit-and-execute-command", EditAndExecuteCommand);
    }

    /// <summary>Incremental undo.</summary>
    private static NotImplementedOrNone? Undo(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.Undo();
        return null;
    }

    /// <summary>
    /// Without numeric argument, comment all lines.
    /// With numeric argument, uncomment all lines.
    /// In any case accept the input.
    /// </summary>
    private static NotImplementedOrNone? InsertComment(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;

        Func<string, string> change;
        if (@event.Arg != 1)
        {
            change = line => line.StartsWith('#') ? line[1..] : line;
        }
        else
        {
            change = line => "#" + line;
        }

        var newText = string.Join("\n",
            buff.Document.Text.Split('\n').Select(change));

        buff.Document = new Document(text: newText, cursorPosition: 0);

        // Accept input.
        buff.ValidateAndHandle();
        return null;
    }

    /// <summary>Switch to Vi editing mode.</summary>
    private static NotImplementedOrNone? ViEditingMode(KeyPressEvent @event)
    {
        @event.GetApp().EditingMode = EditingMode.Vi;
        return null;
    }

    /// <summary>Switch to Emacs editing mode.</summary>
    private static NotImplementedOrNone? EmacsEditingMode(KeyPressEvent @event)
    {
        @event.GetApp().EditingMode = EditingMode.Emacs;
        return null;
    }

    /// <summary>
    /// Metafy the next character typed. This is for keyboards without a meta key.
    /// </summary>
    private static NotImplementedOrNone? PrefixMeta(KeyPressEvent @event)
    {
        // 'first' should be true, because we want to insert it at the current
        // position in the queue.
        @event.GetApp().KeyProcessor.Feed(new KeyPress(Input.Keys.Escape), first: true);
        return null;
    }

    /// <summary>
    /// Accept the current line for execution and fetch the next line relative to
    /// the current line from the history for editing.
    /// </summary>
    private static NotImplementedOrNone? OperateAndGetNext(KeyPressEvent @event)
    {
        var buff = @event.CurrentBuffer!;
        var newIndex = buff.WorkingIndex + 1;

        // Accept the current input.
        buff.ValidateAndHandle();

        // Set the new index at the start of the next run.
        @event.GetApp().PreRunCallables.Add(() =>
        {
            // Only set if within bounds. We can't check _workingLines directly
            // (it's private), so we use GoToHistory which handles bounds internally.
            buff.GoToHistory(newIndex);
        });

        return null;
    }

    /// <summary>Invoke an editor on the current command line, and accept the result.</summary>
    private static NotImplementedOrNone? EditAndExecuteCommand(KeyPressEvent @event)
    {
        // Fire-and-forget, matching Python source which does not await the call.
        _ = @event.CurrentBuffer!.OpenInEditorAsync(validateAndHandle: true);
        return null;
    }
}
