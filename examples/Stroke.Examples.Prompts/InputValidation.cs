using Stroke.Shortcuts;
using Stroke.Validation;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates input validation with ValidatorBase.FromCallable().
/// Port of Python Prompt Toolkit's input-validation.py example.
/// </summary>
public static class InputValidation
{
    public static void Run()
    {
        try
        {
            var validator = ValidatorBase.FromCallable(
                text => text.Contains('@'),
                errorMessage: "Not a valid email address (does not contain '@')",
                moveCursorToEnd: true);

            // Validate on Enter.
            var text = Prompt.RunPrompt(
                "Enter email: ",
                validator: validator,
                validateWhileTyping: false);
            Console.WriteLine($"You said: {text}");

            // Validate while typing.
            text = Prompt.RunPrompt(
                "Enter email: ",
                validator: validator,
                validateWhileTyping: true);
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
