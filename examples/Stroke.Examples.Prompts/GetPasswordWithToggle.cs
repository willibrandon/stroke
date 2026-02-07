using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Password input with Ctrl-T toggle between masked and visible display.
/// Port of Python Prompt Toolkit's get-password-with-toggle-display-shortcut.py example.
/// </summary>
public static class GetPasswordWithToggle
{
    public static void Run()
    {
        var hidden = true;
        var bindings = new KeyBindings();

        bindings.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlT)])(_ =>
        {
            hidden = !hidden;
            return null;
        });

        try
        {
            Console.WriteLine("Type Control-T to toggle password visible.");
            var session = new PromptSession<string>(
                isPassword: new Condition(() => hidden),
                keyBindings: bindings);
            var password = session.Prompt("Password: ");
            Console.WriteLine($"You said: {password}");
        }
        catch (KeyboardInterruptException)
        {
            // Ctrl+C pressed - exit gracefully
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully
        }
    }
}
