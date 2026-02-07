using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates F4 toggle between Vi and Emacs editing modes with toolbar indicator.
/// Port of Python Prompt Toolkit's switch-between-vi-emacs.py example.
/// </summary>
public static class SwitchViEmacs
{
    public static void Run()
    {
        var bindings = new KeyBindings();

        // Add an additional key binding for toggling this flag.
        bindings.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F4)])((e) =>
        {
            var app = Stroke.Application.AppContext.GetApp();
            app.EditingMode =
                app.EditingMode == EditingMode.Vi
                    ? EditingMode.Emacs
                    : EditingMode.Vi;
            return null;
        });

        AnyFormattedText BottomToolbar()
        {
            var app = Stroke.Application.AppContext.GetApp();
            var mode = app.EditingMode == EditingMode.Vi ? "Vi" : "Emacs";
            return $" [F4] Toggle mode. Current: {mode} ";
        }

        try
        {
            Prompt.RunPrompt("> ", keyBindings: bindings,
                bottomToolbar: (Func<AnyFormattedText>)BottomToolbar);
        }
        catch (KeyboardInterruptException)
        {
            // Ctrl+C pressed - exit gracefully (Python exits silently on KeyboardInterrupt)
        }
    }
}
