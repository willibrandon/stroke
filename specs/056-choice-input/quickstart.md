# Quickstart: Choice Input

**Feature**: 056-choice-input
**Date**: 2026-02-03

## Overview

ChoiceInput provides a selection prompt for choosing among a set of options. It displays options in a numbered list, allows navigation with arrow keys or number keys, and returns the selected value when the user presses Enter.

## Basic Usage

### Using the Convenience Function

The simplest way to prompt for a choice:

```csharp
using Stroke.Shortcuts;

// Simple selection
string dish = Dialogs.Choice(
    message: "Please select a dish:",
    options: new[]
    {
        ("pizza", (AnyFormattedText)"Pizza with mushrooms"),
        ("salad", (AnyFormattedText)"Salad with tomatoes"),
        ("sushi", (AnyFormattedText)"Sushi"),
    });

Console.WriteLine($"You selected: {dish}");
```

### Using the ChoiceInput Class

For more control over configuration:

```csharp
using Stroke.Shortcuts;

var choiceInput = new ChoiceInput<string>(
    message: "Please select a dish:",
    options: new[]
    {
        ("pizza", (AnyFormattedText)"Pizza with mushrooms"),
        ("salad", (AnyFormattedText)"Salad with tomatoes"),
        ("sushi", (AnyFormattedText)"Sushi"),
    },
    defaultValue: "pizza");

string result = choiceInput.Prompt();
```

### Async Usage

For async applications:

```csharp
string result = await Dialogs.ChoiceAsync(
    message: "Select an option:",
    options: new[]
    {
        (1, (AnyFormattedText)"Option One"),
        (2, (AnyFormattedText)"Option Two"),
        (3, (AnyFormattedText)"Option Three"),
    });
```

## Common Patterns

### With Default Value

Pre-select an option:

```csharp
var color = Dialogs.Choice(
    message: "Select a color:",
    options: new[]
    {
        ("red", (AnyFormattedText)"Red"),
        ("green", (AnyFormattedText)"Green"),
        ("blue", (AnyFormattedText)"Blue"),
    },
    defaultValue: "blue");  // Blue is pre-selected
```

### With Frame Border

Display with a visual frame:

```csharp
var result = Dialogs.Choice(
    message: "Select priority:",
    options: new[]
    {
        ("high", (AnyFormattedText)"High - Urgent"),
        ("medium", (AnyFormattedText)"Medium - Normal"),
        ("low", (AnyFormattedText)"Low - Can wait"),
    },
    showFrame: true);
```

### With Bottom Toolbar

Add help text at the bottom:

```csharp
var result = Dialogs.Choice(
    message: "Select environment:",
    options: new[]
    {
        ("dev", (AnyFormattedText)"Development"),
        ("staging", (AnyFormattedText)"Staging"),
        ("prod", (AnyFormattedText)"Production"),
    },
    bottomToolbar: "Use ↑↓ to navigate, Enter to select, Ctrl+C to cancel");
```

### With Custom Symbol

Change the selection indicator:

```csharp
var result = Dialogs.Choice(
    message: "Select:",
    options: new[]
    {
        ("a", (AnyFormattedText)"Option A"),
        ("b", (AnyFormattedText)"Option B"),
    },
    symbol: "→");  // Uses arrow instead of ">"
```

### With Mouse Support

Enable clicking on options:

```csharp
var result = Dialogs.Choice(
    message: "Click or press to select:",
    options: new[]
    {
        ("x", (AnyFormattedText)"Choice X"),
        ("y", (AnyFormattedText)"Choice Y"),
    },
    mouseSupport: true);
```

### Handling Cancellation

Catch when user presses Ctrl+C:

```csharp
try
{
    var result = Dialogs.Choice(
        message: "Select an item:",
        options: new[] { ("a", (AnyFormattedText)"A"), ("b", (AnyFormattedText)"B") },
        enableInterrupt: true);  // default

    Console.WriteLine($"Selected: {result}");
}
catch (KeyboardInterrupt)
{
    Console.WriteLine("Selection cancelled.");
}
```

### Disabling Cancellation

Prevent Ctrl+C from cancelling:

```csharp
// User cannot cancel - must select an option
var result = Dialogs.Choice(
    message: "You must choose:",
    options: new[] { ("yes", (AnyFormattedText)"Yes"), ("no", (AnyFormattedText)"No") },
    enableInterrupt: false);
```

### Custom Exception on Cancel

Throw a specific exception type:

```csharp
try
{
    var result = Dialogs.Choice(
        message: "Select:",
        options: new[] { ("a", (AnyFormattedText)"A") },
        interruptException: typeof(OperationCanceledException));
}
catch (OperationCanceledException)
{
    // Handle custom exception type
}
```

## Keyboard Navigation

| Key | Action |
|-----|--------|
| `↑` / `k` | Move selection up |
| `↓` / `j` | Move selection down |
| `1-9` | Jump directly to option 1-9 |
| `Enter` | Confirm selection |
| `Ctrl+C` | Cancel (if enabled) |
| `Ctrl+Z` | Suspend to background (Unix only, if enabled) |

## Output Example

```
Please select a dish:
  1. Pizza with mushrooms
> 2. Salad with tomatoes
  3. Sushi
```

The `>` symbol indicates the currently selected option.

## Style Customization

Create a custom style:

```csharp
var style = Style.FromDict(new Dictionary<string, string>
{
    ["frame.border"] = "#00ff00",      // Green border
    ["selected-option"] = "bold underline", // Bold + underline selection
});

var result = Dialogs.Choice(
    message: "Select:",
    options: new[] { ("a", (AnyFormattedText)"A"), ("b", (AnyFormattedText)"B") },
    style: style,
    showFrame: true);
```

## Integration with Formatted Text

Use HTML or ANSI formatting in labels:

```csharp
var result = Dialogs.Choice(
    message: Html.Parse("<b>Select a level:</b>"),
    options: new[]
    {
        ("easy", Html.Parse("<green>Easy</green> - For beginners")),
        ("medium", Html.Parse("<yellow>Medium</yellow> - Standard difficulty")),
        ("hard", Html.Parse("<red>Hard</red> - Expert mode")),
    });
```
