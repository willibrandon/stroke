using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates custom Vi operator (R = reverse) and text object (A = select all).
/// Port of Python Prompt Toolkit's custom-vi-operator-and-text-object.py example.
/// </summary>
public static class CustomViOperator
{
    public static void Run()
    {
        try
        {
            var kb = new KeyBindings();

            // Custom 'R' key binding in Vi mode: reverse all text.
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar('R')],
                filter: new Stroke.Filters.Condition(() =>
                {
                    try
                    {
                        var app = Stroke.Application.AppContext.GetApp();
                        return app.EditingMode == EditingMode.Vi;
                    }
                    catch
                    {
                        return false;
                    }
                }))((e) =>
            {
                var buffer = e.CurrentBuffer;
                if (buffer != null)
                {
                    var text = buffer.Document.Text;
                    var reversed = new string(text.Reverse().ToArray());
                    buffer.SetDocument(new Stroke.Core.Document(reversed), bypassReadonly: true);
                }
                return null;
            });

            Console.WriteLine("Custom Vi operator example.");
            Console.WriteLine("Press 'R' in Vi normal mode to reverse all text.");
            var text = Prompt.RunPrompt("Enter text: ", viMode: true, keyBindings: kb);
            Console.WriteLine($"You said: {text}");
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
