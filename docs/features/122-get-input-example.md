# Feature 122: Get Input Example (First Example)

## Overview

Implement the first and simplest Stroke example: `GetInput.cs`. This establishes the examples infrastructure and demonstrates the most basic prompt usage. This is the canonical "hello world" of prompt toolkit - a user types input and the program echoes it back.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/examples/prompts/get-input.py`

```python
#!/usr/bin/env python
"""
The most simple prompt example.
"""

from prompt_toolkit import prompt

if __name__ == "__main__":
    answer = prompt("Give me some input: ")
    print(f"You said: {answer}")
```

## Public API (Example Code)

### GetInput.cs

```csharp
// The most simple prompt example.
//
// Port of Python Prompt Toolkit's examples/prompts/get-input.py

using Stroke.Shortcuts;

var answer = Prompt.RunPrompt("Give me some input: ");
Console.WriteLine($"You said: {answer}");
```

## Project Structure

```
examples/
├── Stroke.Examples.sln
└── Stroke.Examples.Prompts/
    ├── Stroke.Examples.Prompts.csproj
    ├── Program.cs                       # Entry point (runs GetInput by default)
    └── GetInput.cs                      # First example implementation
```

## Implementation Notes

### Solution File (Stroke.Examples.sln)

The examples solution is separate from the main Stroke.sln to keep example code isolated:

```
examples/Stroke.Examples.sln
```

This references all example projects as they are added.

### Project File (Stroke.Examples.Prompts.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>13</LangVersion>
    <RootNamespace>Stroke.Examples.Prompts</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Stroke\Stroke.csproj" />
  </ItemGroup>

</Project>
```

### Program.cs (Entry Point)

The entry point allows running specific examples by name or defaults to GetInput:

```csharp
// Stroke.Examples.Prompts entry point
//
// Usage:
//   dotnet run                    # Runs GetInput (default)
//   dotnet run -- GetInput        # Runs GetInput explicitly
//   dotnet run -- <ExampleName>   # Runs named example

namespace Stroke.Examples.Prompts;

public static class Program
{
    public static void Main(string[] args)
    {
        var exampleName = args.Length > 0 ? args[0] : "GetInput";

        switch (exampleName)
        {
            case "GetInput":
                GetInput.Run();
                break;
            default:
                Console.WriteLine($"Unknown example: {exampleName}");
                Console.WriteLine("Available examples: GetInput");
                Environment.Exit(1);
                break;
        }
    }
}
```

### GetInput.cs

```csharp
// The most simple prompt example.
//
// Port of Python Prompt Toolkit's examples/prompts/get-input.py
//
// Demonstrates:
// - Basic prompt usage with Prompt.RunPrompt()
// - Single-line text input
// - Echo back user input

using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

public static class GetInput
{
    public static void Run()
    {
        var answer = Prompt.RunPrompt("Give me some input: ");
        Console.WriteLine($"You said: {answer}");
    }
}
```

## Dependencies

- Feature 47: Prompt Session (PromptSession<TResult>)
- Feature 46: Shortcut Utils (Prompt static class)
- Feature 30: Application System (Application<TResult>)
- Feature 21: Output System (IOutput, Vt100Output)
- Feature 14: Input System (IInput, Vt100Input)

All dependencies are already implemented and tested.

## Implementation Tasks

1. Create `examples/` directory structure
2. Create `Stroke.Examples.sln` solution file
3. Create `Stroke.Examples.Prompts/Stroke.Examples.Prompts.csproj` project
4. Create `Stroke.Examples.Prompts/Program.cs` entry point
5. Create `Stroke.Examples.Prompts/GetInput.cs` example
6. Verify example builds: `dotnet build examples/Stroke.Examples.sln`
7. Verify example runs: `dotnet run --project examples/Stroke.Examples.Prompts`
8. Test interactive input/output behavior manually
9. (Optional) Add integration test using TUI Driver

## Acceptance Criteria

- [ ] `examples/` directory exists with proper structure
- [ ] `Stroke.Examples.sln` builds successfully
- [ ] `Stroke.Examples.Prompts.csproj` references main Stroke project
- [ ] `dotnet run --project examples/Stroke.Examples.Prompts` launches prompt
- [ ] User can type text and press Enter
- [ ] Program echoes "You said: {input}" and exits
- [ ] Example code matches Python original behavior exactly
- [ ] No external dependencies beyond Stroke library

## Verification with TUI Driver

After implementation, verify using TUI Driver MCP tools:

```javascript
// Launch the example
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts"],
  cols: 80,
  rows: 24
});

// Wait for prompt to appear
await tui_wait_for_text({ session_id: session.id, text: "Give me some input:" });

// Type input
await tui_send_text({ session_id: session.id, text: "Hello, World!" });

// Press Enter
await tui_press_key({ session_id: session.id, key: "Enter" });

// Verify output
await tui_wait_for_text({ session_id: session.id, text: "You said: Hello, World!" });

// Close session
await tui_close({ session_id: session.id });
```

## Why This Example First?

1. **Canonical Starting Point**: Example #1 in examples-mapping.md
2. **Minimal Code**: 11 lines Python → ~10 lines C# (excluding boilerplate)
3. **Full Stack Test**: Exercises PromptSession, Application, Input, Output, Rendering
4. **User First Contact**: What every new user tries first
5. **Foundation**: Establishes examples/ infrastructure for all 128 remaining examples

## Next Examples (Phase 1 Continuation)

After GetInput.cs, the implementation order from examples-mapping.md Phase 1:

| Priority | Example | Description |
|----------|---------|-------------|
| 2 | GetPassword.cs | Password input with masking |
| 3 | GetMultilineInput.cs | Multi-line text input |
| 4 | MessageBox.cs (Dialogs) | Simple message dialog |
| 5 | YesNoDialog.cs (Dialogs) | Boolean confirmation |
| 6 | InputDialog.cs (Dialogs) | Text input dialog |

This establishes the Prompts and Dialogs example projects in Phase 1.
