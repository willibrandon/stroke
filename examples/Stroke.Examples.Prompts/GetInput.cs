using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// The simplest prompt example - equivalent to Python Prompt Toolkit's get-input.py.
/// </summary>
public static class GetInput
{
    public static void Run()
    {
        var answer = Prompt.RunPrompt("Give me some input: ");
        Console.WriteLine($"You said: {answer}");
    }
}
