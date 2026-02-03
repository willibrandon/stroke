# Feature 125: Simple Selection Example (Choices)

## Overview

Implement the `SimpleSelection.cs` example in the new Choices example project. This demonstrates the `Dialogs.Choice<T>()` method for presenting users with a list of options and capturing their selection. A fundamental UI pattern for menus, configuration, and any scenario requiring user selection from predefined options.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/examples/choices/simple-selection.py`

```python
#!/usr/bin/env python
"""
Simple example showing how to use the `choice` dialog.
"""

from prompt_toolkit.shortcuts import choice

if __name__ == "__main__":
    result = choice(
        message="Please select a dish:",
        options=[
            ("pizza", "Pizza with mushrooms"),
            ("salad", "Salad with tomatoes"),
            ("sushi", "Sushi"),
        ],
    )
    print(f"You selected: {result}")
```

## Public API (Example Code)

### SimpleSelection.cs

```csharp
// Simple example showing how to use the Choice dialog.
//
// Port of Python Prompt Toolkit's examples/choices/simple-selection.py

using Stroke.Shortcuts;

var result = Dialogs.Choice(
    message: "Please select a dish:",
    options: [
        ("pizza", "Pizza with mushrooms"),
        ("salad", "Salad with tomatoes"),
        ("sushi", "Sushi"),
    ]);
Console.WriteLine($"You selected: {result}");
```

## Project Structure

```
examples/
├── Stroke.Examples.sln
├── Stroke.Examples.Prompts/               # Existing (Feature 122)
│   └── ...
└── Stroke.Examples.Choices/               # NEW (this feature)
    ├── Stroke.Examples.Choices.csproj
    ├── Program.cs                         # Entry point with example routing
    └── SimpleSelection.cs                 # Example 1 for Choices project
```

## Implementation Notes

### Project File (Stroke.Examples.Choices.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>13</LangVersion>
    <RootNamespace>Stroke.Examples.Choices</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Stroke\Stroke.csproj" />
  </ItemGroup>

</Project>
```

### Program.cs (Entry Point)

```csharp
// Stroke.Examples.Choices entry point
//
// Usage:
//   dotnet run                        # Runs SimpleSelection (default)
//   dotnet run -- SimpleSelection     # Runs SimpleSelection explicitly
//   dotnet run -- <ExampleName>       # Runs named example

namespace Stroke.Examples.Choices;

public static class Program
{
    public static void Main(string[] args)
    {
        var exampleName = args.Length > 0 ? args[0] : "SimpleSelection";

        switch (exampleName)
        {
            case "SimpleSelection":
                SimpleSelection.Run();
                break;
            default:
                Console.WriteLine($"Unknown example: {exampleName}");
                Console.WriteLine("Available examples: SimpleSelection");
                Environment.Exit(1);
                break;
        }
    }
}
```

### SimpleSelection.cs

```csharp
// Simple example showing how to use the Choice dialog.
//
// Port of Python Prompt Toolkit's examples/choices/simple-selection.py
//
// Demonstrates:
// - Using Dialogs.Choice<T>() for option selection
// - Tuple-based options (value, display text)
// - Arrow key and number key navigation
// - Enter to confirm selection

using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

public static class SimpleSelection
{
    public static void Run()
    {
        var result = Dialogs.Choice(
            message: "Please select a dish:",
            options: [
                ("pizza", "Pizza with mushrooms"),
                ("salad", "Salad with tomatoes"),
                ("sushi", "Sushi"),
            ]);
        Console.WriteLine($"You selected: {result}");
    }
}
```

### Solution Update

Add the new project to `Stroke.Examples.sln`:

```
dotnet sln examples/Stroke.Examples.sln add examples/Stroke.Examples.Choices/Stroke.Examples.Choices.csproj
```

## Dependencies

- Feature 56: ChoiceInput<T> (ChoiceInput selection prompt widget)
- Feature 48: Dialog Shortcuts (Dialogs.Choice<T>() static method)
- Feature 45: Base Widgets (RadioList<T>, Dialog)
- Feature 30: Application System (Application<TResult>)

All dependencies are already implemented and tested. The `Dialogs.Choice<T>()` method exists in `Dialogs.cs` (lines 544-599).

## Implementation Tasks

1. Create `examples/Stroke.Examples.Choices/` directory
2. Create `Stroke.Examples.Choices.csproj` project file
3. Create `Program.cs` entry point with example routing
4. Create `SimpleSelection.cs` example
5. Add project to `Stroke.Examples.sln`
6. Verify example builds: `dotnet build examples/Stroke.Examples.sln`
7. Verify example runs: `dotnet run --project examples/Stroke.Examples.Choices`
8. Test arrow key navigation (up/down)
9. Test number key selection (1, 2, 3)
10. Test Enter to confirm selection
11. Test Ctrl+C to cancel (if applicable)

## Acceptance Criteria

- [ ] `examples/Stroke.Examples.Choices/` directory exists
- [ ] `Stroke.Examples.Choices.csproj` builds successfully
- [ ] Project is included in `Stroke.Examples.sln`
- [ ] `dotnet run --project examples/Stroke.Examples.Choices` launches selection UI
- [ ] Three dish options are displayed with message "Please select a dish:"
- [ ] Arrow keys (up/down) navigate between options
- [ ] Number keys (1, 2, 3) select options directly
- [ ] Enter confirms the current selection
- [ ] Program echoes "You selected: {value}" with the tuple's first element (e.g., "pizza")
- [ ] Example code matches Python original behavior exactly

## Verification with TUI Driver

After implementation, verify using TUI Driver MCP tools:

```javascript
// Launch the example
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Choices"],
  cols: 80,
  rows: 24
});

// Wait for dialog to appear
await tui_wait_for_text({ session_id: session.id, text: "Please select a dish:" });

// Verify all options are shown
await tui_wait_for_text({ session_id: session.id, text: "Pizza with mushrooms" });
await tui_wait_for_text({ session_id: session.id, text: "Salad with tomatoes" });
await tui_wait_for_text({ session_id: session.id, text: "Sushi" });

// Select with arrow down and Enter
await tui_press_key({ session_id: session.id, key: "Down" });  // Move to salad
await tui_press_key({ session_id: session.id, key: "Enter" }); // Confirm

// Verify output
await tui_wait_for_text({ session_id: session.id, text: "You selected: salad" });

// Close session
await tui_close({ session_id: session.id });
```

**Test number key selection:**

```javascript
// Launch fresh session
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Choices"],
  cols: 80,
  rows: 24
});

// Wait for dialog
await tui_wait_for_text({ session_id: session.id, text: "Please select a dish:" });

// Press 3 to select sushi directly, then Enter
await tui_press_key({ session_id: session.id, key: "3" });
await tui_press_key({ session_id: session.id, key: "Enter" });

// Verify sushi was selected
await tui_wait_for_text({ session_id: session.id, text: "You selected: sushi" });

await tui_close({ session_id: session.id });
```

**Test keyboard navigation (j/k vim-style):**

```javascript
// Launch fresh session
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Choices"],
  cols: 80,
  rows: 24
});

// Wait for dialog
await tui_wait_for_text({ session_id: session.id, text: "Please select a dish:" });

// Use j to go down twice (to sushi), k to go back up (to salad)
await tui_press_keys({ session_id: session.id, keys: ["j", "j", "k", "Enter"] });

// Should select salad
await tui_wait_for_text({ session_id: session.id, text: "You selected: salad" });

await tui_close({ session_id: session.id });
```

## Why This Example?

1. **Zero New Features**: Uses existing `Dialogs.Choice<T>()` method
2. **New Example Project**: Establishes the Choices project for 6+ choice examples
3. **Rich Interaction**: Demonstrates arrow keys, number keys, vim keys (j/k)
4. **Common Pattern**: Selection menus are ubiquitous in CLI tools
5. **Foundation**: Required before more complex choice examples (multi-select, etc.)

## Next Choices Examples

After SimpleSelection.cs, from examples-mapping.md:

| Example | Description |
|---------|-------------|
| MultiSelect.cs | Multiple selection with checkboxes |
| CustomStyling.cs | Styled choice dialogs |
| WithDefault.cs | Pre-selected default option |
