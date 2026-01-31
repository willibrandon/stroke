using InputKeyPress = Stroke.Input.KeyPress;

namespace Stroke.KeyBinding.Bindings;

public static partial class NamedCommands
{
    /// <summary>
    /// Registers the 4 macro commands.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's macro commands from
    /// <c>named_commands.py</c> lines 538-590.
    /// </remarks>
    static partial void RegisterMacroCommands()
    {
        RegisterInternal("start-kbd-macro", StartKbdMacro);
        RegisterInternal("end-kbd-macro", EndKbdMacro);
        RegisterInternal("call-last-kbd-macro", CallLastKbdMacro, recordInMacro: false);
        RegisterInternal("print-last-kbd-macro", PrintLastKbdMacro);
    }

    /// <summary>Begin saving the characters typed into the current keyboard macro.</summary>
    private static NotImplementedOrNone? StartKbdMacro(KeyPressEvent @event)
    {
        @event.GetApp().EmacsState.StartMacro();
        return null;
    }

    /// <summary>
    /// Stop saving the characters typed into the current keyboard macro and save the definition.
    /// </summary>
    private static NotImplementedOrNone? EndKbdMacro(KeyPressEvent @event)
    {
        @event.GetApp().EmacsState.EndMacro();
        return null;
    }

    /// <summary>
    /// Re-execute the last keyboard macro defined, by making the characters in the
    /// macro appear as if typed at the keyboard.
    /// </summary>
    /// <remarks>
    /// This command is registered with <c>recordInMacro: false</c> to prevent the
    /// macro replay key sequence from being recorded into the macro itself, which
    /// would cause infinite recursion.
    /// </remarks>
    private static NotImplementedOrNone? CallLastKbdMacro(KeyPressEvent @event)
    {
        var macro = @event.GetApp().EmacsState.Macro;

        if (macro is { Count: > 0 })
        {
            // Convert Input.KeyPress to KeyBinding.KeyPress for the key processor.
            var keyPresses = macro.Select(
                inputKp => new KeyPress(new KeyOrChar(inputKp.Key), inputKp.Data));
            @event.GetApp().KeyProcessor.FeedMultiple(keyPresses, first: true);
        }

        return null;
    }

    /// <summary>Print the last keyboard macro.</summary>
    private static NotImplementedOrNone? PrintLastKbdMacro(KeyPressEvent @event)
    {
        var app = @event.GetApp();

        void PrintMacro()
        {
            var macro = app.EmacsState.Macro;
            if (macro is not null)
            {
                foreach (var k in macro)
                {
                    Console.WriteLine(k);
                }
            }
        }

        _ = Application.RunInTerminal.RunAsync(PrintMacro);
        return null;
    }
}
