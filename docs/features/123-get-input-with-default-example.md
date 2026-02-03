# Feature 123: Get Input With Default Example

## Overview

Implement the `GetInputWithDefault.cs` example demonstrating pre-populated input fields. This shows how to provide a default value that users can edit rather than typing from scratch. A common UX pattern for forms, configuration, and any input where a reasonable default exists.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/examples/prompts/get-input-with-default.py`

```python
#!/usr/bin/env python
"""
Example of a prompt with a default value.
The input is pre-filled, but the user can still edit the default.
"""

import getpass

from prompt_toolkit import prompt

if __name__ == "__main__":
    answer = prompt("What is your name: ", default=f"{getpass.getuser()}")
    print(f"You said: {answer}")
```

## Public API (Example Code)

### GetInputWithDefault.cs

```csharp
// Example of a prompt with a default value.
// The input is pre-filled, but the user can still edit the default.
//
// Port of Python Prompt Toolkit's examples/prompts/get-input-with-default.py

using Stroke.Shortcuts;

var answer = Prompt.RunPrompt("What is your name: ", default_: Environment.UserName);
Console.WriteLine($"You said: {answer}");
```

## Project Structure

```
examples/
└── Stroke.Examples.Prompts/
    ├── Stroke.Examples.Prompts.csproj
    ├── Program.cs                         # Entry point with example routing
    ├── GetInput.cs                        # Example 1 (already implemented)
    └── GetInputWithDefault.cs             # Example 2 (this feature)
```

## Implementation Notes

### Example Code

```csharp
// Example of a prompt with a default value.
// The input is pre-filled, but the user can still edit the default.
//
// Port of Python Prompt Toolkit's examples/prompts/get-input-with-default.py
//
// Demonstrates:
// - Using the default_ parameter to pre-fill input
// - Editable default values (user can modify or accept as-is)
// - Environment.UserName as .NET equivalent of getpass.getuser()

using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

public static class GetInputWithDefault
{
    public static void Run()
    {
        var answer = Prompt.RunPrompt("What is your name: ", default_: Environment.UserName);
        Console.WriteLine($"You said: {answer}");
    }
}
```

### Program.cs Update

Add the new example to the switch statement:

```csharp
case "GetInputWithDefault":
    GetInputWithDefault.Run();
    break;
```

And update the help message to include it in the available examples list.

## Dependencies

- Feature 122: Get Input Example (establishes examples infrastructure)
- Feature 47: Prompt Session (PromptSession<TResult> with `default_` parameter)
- Feature 46: Shortcut Utils (Prompt.RunPrompt static method)

All dependencies are already implemented and tested. The `default_` parameter already exists in `Prompt.RunPrompt()` (line 79 in Prompt.cs).

## Implementation Tasks

1. Create `GetInputWithDefault.cs` in `examples/Stroke.Examples.Prompts/`
2. Update `Program.cs` to route `GetInputWithDefault` example
3. Update help message with new example name
4. Verify example builds: `dotnet build examples/Stroke.Examples.sln`
5. Verify example runs: `dotnet run --project examples/Stroke.Examples.Prompts -- GetInputWithDefault`
6. Test that default value appears in input field
7. Test that default can be edited before submitting
8. Test that pressing Enter immediately accepts the default

## Acceptance Criteria

- [ ] `GetInputWithDefault.cs` exists in examples project
- [ ] Example is routable via `dotnet run -- GetInputWithDefault`
- [ ] Prompt shows "What is your name: " with cursor after pre-filled username
- [ ] User can press Enter to accept default value as-is
- [ ] User can edit the pre-filled text before pressing Enter
- [ ] Program echoes "You said: {input}" with the final value
- [ ] Example code matches Python original behavior exactly

## Verification with TUI Driver

After implementation, verify using TUI Driver MCP tools:

```javascript
// Launch the example
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "GetInputWithDefault"],
  cols: 80,
  rows: 24
});

// Wait for prompt to appear
await tui_wait_for_text({ session_id: session.id, text: "What is your name:" });

// Verify default value is pre-filled (should contain username)
const text = await tui_text({ session_id: session.id });
// Text should contain the username in the input area

// Accept default by pressing Enter
await tui_press_key({ session_id: session.id, key: "Enter" });

// Verify output includes the username
await tui_wait_for_text({ session_id: session.id, text: "You said:" });

// Close session
await tui_close({ session_id: session.id });
```

**Test editing the default:**

```javascript
// Launch fresh session
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "GetInputWithDefault"],
  cols: 80,
  rows: 24
});

// Wait for prompt
await tui_wait_for_text({ session_id: session.id, text: "What is your name:" });

// Clear the default and type new value
await tui_press_key({ session_id: session.id, key: "Ctrl+u" }); // Kill line (clear input)
await tui_send_text({ session_id: session.id, text: "Alice" });
await tui_press_key({ session_id: session.id, key: "Enter" });

// Verify custom input
await tui_wait_for_text({ session_id: session.id, text: "You said: Alice" });

await tui_close({ session_id: session.id });
```

## Why This Example?

1. **Zero New Features**: Uses existing `default_` parameter in `Prompt.RunPrompt()`
2. **Common UX Pattern**: Pre-filled defaults are ubiquitous in CLI tools
3. **Immediate Value**: Demonstrates editable defaults - a non-obvious capability
4. **Quick Win**: ~10 lines of code, establishes pattern for more examples
