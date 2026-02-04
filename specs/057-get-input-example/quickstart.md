# Quickstart: Get Input Example

## Overview

The GetInput example demonstrates the simplest possible Stroke prompt — equivalent to Python Prompt Toolkit's `get-input.py`. It prompts for text input, accepts it on Enter, and echoes the result.

## Prerequisites

- .NET 10 SDK installed
- Stroke library built (`dotnet build src/Stroke/Stroke.csproj`)

## Running the Example

### Default (runs GetInput)
```bash
dotnet run --project examples/Stroke.Examples.Prompts
```

### By Name
```bash
dotnet run --project examples/Stroke.Examples.Prompts -- GetInput
```

## Expected Behavior

1. Terminal displays: `Give me some input: `
2. User types any text (e.g., "Hello, World!")
3. User presses Enter
4. Terminal displays: `You said: Hello, World!`
5. Program exits

## Source Code

### GetInput.cs
```csharp
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
```

### Python Equivalent
```python
from prompt_toolkit import prompt

answer = prompt("Give me some input: ")
print(f"You said: {answer}")
```

## Key API: `Prompt.RunPrompt()`

| Parameter | Type | Description |
|-----------|------|-------------|
| `message` | `AnyFormattedText?` | The prompt text to display (optional) |
| *returns* | `string` | The user's input text |

The `Prompt.RunPrompt()` method creates a `PromptSession<string>` internally, handles terminal setup/teardown, key bindings, and returns the user's input as a string.

## Keyboard Support

- **Enter**: Submit input
- **Ctrl+C**: Cancel (throws `KeyboardInterruptException`)
- **Backspace**: Delete character before cursor
- **Arrow keys**: Move cursor
- Full Emacs keybindings enabled by default

## Testing

Run the TUI Driver verification:

```javascript
// Launch the example
const session = await tui.launch('dotnet', ['run', '--project', 'examples/Stroke.Examples.Prompts']);

// Wait for prompt
await tui.waitForText(session, 'Give me some input: ');

// Type input and submit
await tui.sendText(session, 'Hello, World!');
await tui.pressKey(session, 'Enter');

// Verify output
await tui.waitForText(session, 'You said: Hello, World!');
```

## Next Steps

After this example works:
- `GetInputWithDefault.cs` — prompt with pre-filled default value
- `GetPassword.cs` — masked password input
- `ColoredPrompt.cs` — styled prompt with colors
