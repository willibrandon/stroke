namespace Stroke.KeyBinding.Bindings;

public static partial class NamedCommands
{
    /// <summary>
    /// Registers the 3 completion commands.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's completion commands from
    /// <c>named_commands.py</c> lines 509-531.
    /// </remarks>
    static partial void RegisterCompletionCommands()
    {
        RegisterInternal("complete", Complete);
        RegisterInternal("menu-complete", MenuComplete);
        RegisterInternal("menu-complete-backward", MenuCompleteBackward);
    }

    /// <summary>Attempt to perform completion (Readline-style).</summary>
    private static NotImplementedOrNone? Complete(KeyPressEvent @event)
    {
        CompletionBindings.DisplayCompletionsLikeReadline(@event);
        return null;
    }

    /// <summary>
    /// Generate completions, or go to the next completion. (This is the default
    /// way of completing input in prompt_toolkit.)
    /// </summary>
    private static NotImplementedOrNone? MenuComplete(KeyPressEvent @event)
    {
        CompletionBindings.GenerateCompletions(@event);
        return null;
    }

    /// <summary>Move backward through the list of possible completions.</summary>
    private static NotImplementedOrNone? MenuCompleteBackward(KeyPressEvent @event)
    {
        @event.CurrentBuffer!.CompletePrevious();
        return null;
    }
}
