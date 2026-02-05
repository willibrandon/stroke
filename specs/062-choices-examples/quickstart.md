# Quickstart: Choices Examples

**Feature**: 062-choices-examples
**Date**: 2026-02-04

## Overview

The `Stroke.Examples.Choices` project demonstrates 8 ways to use the `Dialogs.Choice<T>()` API for creating interactive selection prompts.

## Running Examples

### All Examples

```bash
# From repository root
cd /Users/brandon/src/stroke

# Build the examples solution
dotnet build examples/Stroke.Examples.sln

# Run an example by name
dotnet run --project examples/Stroke.Examples.Choices -- SimpleSelection
dotnet run --project examples/Stroke.Examples.Choices -- Default
dotnet run --project examples/Stroke.Examples.Choices -- Color
dotnet run --project examples/Stroke.Examples.Choices -- WithFrame
dotnet run --project examples/Stroke.Examples.Choices -- FrameAndBottomToolbar
dotnet run --project examples/Stroke.Examples.Choices -- GrayFrameOnAccept
dotnet run --project examples/Stroke.Examples.Choices -- ManyChoices
dotnet run --project examples/Stroke.Examples.Choices -- MouseSupport

# Run default example (SimpleSelection)
dotnet run --project examples/Stroke.Examples.Choices
```

### Example Names (case-insensitive)

| Name | Description |
|------|-------------|
| `SimpleSelection` | Basic 3-option selection |
| `Default` | Pre-selected default + HTML message |
| `Color` | Custom styling with colored text |
| `WithFrame` | Frame border that hides on accept |
| `FrameAndBottomToolbar` | Frame + navigation instructions |
| `GrayFrameOnAccept` | Frame color changes when accepted |
| `ManyChoices` | 99 scrollable options |
| `MouseSupport` | Click to select options |

## Controls

- **↑/↓ arrows**: Navigate options
- **k/j**: Navigate (Vi-style)
- **1-9**: Jump to numbered option
- **Enter**: Accept selection
- **Ctrl+C**: Cancel (exits gracefully)
- **Ctrl+D**: EOF (exits gracefully)
- **Mouse click**: Select option (when enabled)

## Code Patterns

### Minimal Example (SimpleSelection)

```csharp
using Stroke.Shortcuts;

var result = Dialogs.Choice(
    "Please select a dish:",
    [
        ("pizza", "Pizza with mushrooms"),
        ("salad", "Salad with tomatoes"),
        ("sushi", "Sushi"),
    ]);
Console.WriteLine(result);
```

### With HTML Formatting (Default)

```csharp
using Stroke.FormattedText;
using Stroke.Shortcuts;

var result = Dialogs.Choice(
    Html.Parse("<u>Please select a dish</u>:"),
    [
        ("pizza", "Pizza with mushrooms"),
        ("salad", "Salad with tomatoes"),
        ("sushi", "Sushi"),
    ],
    defaultValue: "salad");
Console.WriteLine(result);
```

### Custom Styling (Color)

```csharp
using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

var style = Style.FromDict(new Dictionary<string, string>
{
    ["input-selection"] = "fg:#ff0000",
    ["number"] = "fg:#884444 bold",
    ["selected-option"] = "underline",
    ["frame.border"] = "#884444",
});

var result = Dialogs.Choice(
    Html.Parse("<u>Please select a dish</u>:"),
    [
        ("pizza", "Pizza with mushrooms"),
        ("salad", Html.Parse("<ansigreen>Salad</ansigreen> with <ansired>tomatoes</ansired>")),
        ("sushi", "Sushi"),
    ],
    style: style);
```

### Conditional Frame (WithFrame)

```csharp
using Stroke.Filters;
using Stroke.Shortcuts;

var result = Dialogs.Choice(
    "Please select a dish:",
    options,
    showFrame: ~AppFilters.IsDone);  // Frame hides after selection
```

### Style Change on Accept (GrayFrameOnAccept)

```csharp
var style = Style.FromDict(new Dictionary<string, string>
{
    ["frame.border"] = "#ff4444",
    ["accepted frame.border"] = "#888888",  // Changes to gray on accept
});

var result = Dialogs.Choice(
    "Please select:",
    options,
    style: style,
    showFrame: true);  // Frame stays visible
```

## Project Structure

```
examples/Stroke.Examples.Choices/
├── Stroke.Examples.Choices.csproj
├── Program.cs              # Entry point with example routing
├── SimpleSelection.cs      # Example 1
├── Default.cs              # Example 2
├── Color.cs                # Example 3
├── WithFrame.cs            # Example 4
├── FrameAndBottomToolbar.cs # Example 5
├── GrayFrameOnAccept.cs    # Example 6
├── ManyChoices.cs          # Example 7
└── MouseSupport.cs         # Example 8
```

## Verification with TUI Driver

```bash
# Use TUI Driver MCP to automate testing
tui_launch "dotnet run --project examples/Stroke.Examples.Choices -- SimpleSelection"
tui_wait_for_text "Please select a dish:"
tui_press_key "Down"
tui_press_key "Down"
tui_press_key "Enter"
tui_text  # Should show "sushi"
tui_close
```
