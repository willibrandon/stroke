# Feature 124: Get Password Example

## Overview

Implement the `GetPassword.cs` example demonstrating secure password input with character masking. This shows how to collect sensitive input where characters are hidden (typically shown as asterisks or nothing at all). Essential for authentication flows, API key entry, and any sensitive data collection.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/examples/prompts/get-password.py`

```python
#!/usr/bin/env python
"""
get_password function.
"""

from prompt_toolkit import prompt

if __name__ == "__main__":
    password = prompt("Password: ", is_password=True)
    print(f"You said: {password}")
```

## Public API (Example Code)

### GetPassword.cs

```csharp
// Password input with character masking.
//
// Port of Python Prompt Toolkit's examples/prompts/get-password.py

using Stroke.Shortcuts;

var password = Prompt.RunPrompt("Password: ", isPassword: true);
Console.WriteLine($"You said: {password}");
```

## Project Structure

```
examples/
└── Stroke.Examples.Prompts/
    ├── Stroke.Examples.Prompts.csproj
    ├── Program.cs                         # Entry point with example routing
    ├── GetInput.cs                        # Example 1 (already implemented)
    ├── GetInputWithDefault.cs             # Example 2 (Feature 123)
    └── GetPassword.cs                     # Example 3 (this feature)
```

## Implementation Notes

### Example Code

```csharp
// Password input with character masking.
//
// Port of Python Prompt Toolkit's examples/prompts/get-password.py
//
// Demonstrates:
// - Using the isPassword parameter to hide typed characters
// - Secure input collection (characters shown as '*' or hidden)
// - Full editing capabilities still work (backspace, Ctrl+U, etc.)

using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

public static class GetPassword
{
    public static void Run()
    {
        var password = Prompt.RunPrompt("Password: ", isPassword: true);
        Console.WriteLine($"You said: {password}");
    }
}
```

### Program.cs Update

Add the new example to the switch statement:

```csharp
case "GetPassword":
    GetPassword.Run();
    break;
```

And update the help message to include it in the available examples list.

## Dependencies

- Feature 122: Get Input Example (establishes examples infrastructure)
- Feature 47: Prompt Session (PromptSession<TResult> with `isPassword` parameter)
- Feature 46: Shortcut Utils (Prompt.RunPrompt static method)

All dependencies are already implemented and tested. The `isPassword` parameter already exists in `Prompt.RunPrompt()` (line 48 in Prompt.cs).

## Implementation Tasks

1. Create `GetPassword.cs` in `examples/Stroke.Examples.Prompts/`
2. Update `Program.cs` to route `GetPassword` example
3. Update help message with new example name
4. Verify example builds: `dotnet build examples/Stroke.Examples.sln`
5. Verify example runs: `dotnet run --project examples/Stroke.Examples.Prompts -- GetPassword`
6. Test that typed characters are masked (not visible as plaintext)
7. Test that editing works (backspace, etc.)
8. Test that actual password value is captured correctly

## Acceptance Criteria

- [ ] `GetPassword.cs` exists in examples project
- [ ] Example is routable via `dotnet run -- GetPassword`
- [ ] Prompt shows "Password: " and waits for input
- [ ] Typed characters are masked (shown as '*' or hidden, not plaintext)
- [ ] Backspace and other editing keys work correctly on masked input
- [ ] Program echoes "You said: {password}" with the actual typed value
- [ ] Example code matches Python original behavior exactly

## Verification with TUI Driver

After implementation, verify using TUI Driver MCP tools:

```javascript
// Launch the example
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "GetPassword"],
  cols: 80,
  rows: 24
});

// Wait for prompt to appear
await tui_wait_for_text({ session_id: session.id, text: "Password:" });

// Type a password
await tui_send_text({ session_id: session.id, text: "secret123" });

// Capture screen - should NOT show "secret123" in plaintext
const textBeforeEnter = await tui_text({ session_id: session.id });
// Verify "secret123" is NOT visible (only asterisks or nothing)

// Press Enter
await tui_press_key({ session_id: session.id, key: "Enter" });

// Verify output includes the actual password
await tui_wait_for_text({ session_id: session.id, text: "You said: secret123" });

// Close session
await tui_close({ session_id: session.id });
```

**Test editing masked input:**

```javascript
// Launch fresh session
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "GetPassword"],
  cols: 80,
  rows: 24
});

// Wait for prompt
await tui_wait_for_text({ session_id: session.id, text: "Password:" });

// Type, then backspace, then type more
await tui_send_text({ session_id: session.id, text: "wrongpass" });
await tui_press_keys({ session_id: session.id, keys: ["Backspace", "Backspace", "Backspace", "Backspace"] });
await tui_send_text({ session_id: session.id, text: "word" });
await tui_press_key({ session_id: session.id, key: "Enter" });

// Should have "wrongword" (wrong + word after deleting "pass")
await tui_wait_for_text({ session_id: session.id, text: "You said: wrongword" });

await tui_close({ session_id: session.id });
```

## Why This Example?

1. **Zero New Features**: Uses existing `isPassword` parameter in `Prompt.RunPrompt()`
2. **Security Essential**: Every CLI tool with auth needs password input
3. **User Expectation**: Password masking is a standard, expected behavior
4. **Quick Win**: ~10 lines of code, demonstrates another core parameter
5. **Foundation**: Many prompts examples build on password input patterns
