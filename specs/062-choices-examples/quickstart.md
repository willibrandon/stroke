# Quickstart: Choices Examples (Complete Set)

**Feature**: 062-choices-examples
**Date**: 2026-02-04

## Getting Started

### Prerequisites

- .NET 10 SDK installed
- Stroke library built (`dotnet build src/Stroke.sln`)

### Build the Examples

```bash
cd /Users/brandon/src/stroke
dotnet build examples/Stroke.Examples.sln
```

### Run Examples

```bash
# Default example (SimpleSelection)
dotnet run --project examples/Stroke.Examples.Choices

# Run specific examples
dotnet run --project examples/Stroke.Examples.Choices -- SimpleSelection
dotnet run --project examples/Stroke.Examples.Choices -- Default
dotnet run --project examples/Stroke.Examples.Choices -- Color
dotnet run --project examples/Stroke.Examples.Choices -- WithFrame
dotnet run --project examples/Stroke.Examples.Choices -- FrameAndBottomToolbar
dotnet run --project examples/Stroke.Examples.Choices -- GrayFrameOnAccept
dotnet run --project examples/Stroke.Examples.Choices -- ManyChoices
dotnet run --project examples/Stroke.Examples.Choices -- MouseSupport
```

## Example Overview

| Example | Key Feature | Try This |
|---------|-------------|----------|
| SimpleSelection | Basic selection | Arrow keys + Enter |
| Default | Pre-selected value | Press Enter immediately |
| Color | Custom styling | Notice colored text |
| WithFrame | Frame border | Frame disappears on Enter |
| FrameAndBottomToolbar | Toolbar | Read instructions at bottom |
| GrayFrameOnAccept | Accept styling | Watch frame turn gray |
| ManyChoices | Scrollable list | Navigate through 99 options |
| MouseSupport | Mouse clicks | Click on options |

## Key APIs Demonstrated

### Basic Selection

```csharp
using Stroke.Shortcuts;

var result = Dialogs.Choice(
    message: "Please select a dish:",
    options: [
        ("pizza", "Pizza with mushrooms"),
        ("salad", "Salad with tomatoes"),
        ("sushi", "Sushi"),
    ]);
Console.WriteLine(result);
```

### With Default Value

```csharp
using Stroke.FormattedText;
using Stroke.Shortcuts;

var result = Dialogs.Choice(
    message: new Html("<u>Please select a dish</u>:"),
    options: [
        ("pizza", "Pizza with mushrooms"),
        ("salad", "Salad with tomatoes"),
        ("sushi", "Sushi"),
    ],
    defaultValue: "salad");
```

### With Custom Styling

```csharp
using Stroke.Styles;

var style = Style.FromDict(new Dictionary<string, string>
{
    ["input-selection"] = "fg:#ff0000",
    ["number"] = "fg:#884444 bold",
    ["selected-option"] = "underline",
    ["frame.border"] = "#884444",
});

var result = Dialogs.Choice(
    message: new Html("<u>Please select a dish</u>:"),
    options: [...],
    style: style);
```

### With Frame (Filter-Controlled)

```csharp
using Stroke.Application;

var result = Dialogs.Choice(
    message: new Html("<u>Please select a dish</u>:"),
    options: [...],
    showFrame: ~AppFilters.IsDone);  // Frame visible during editing only
```

### With Bottom Toolbar

```csharp
var result = Dialogs.Choice(
    message: new Html("<u>Please select a dish</u>:"),
    options: [...],
    bottomToolbar: new Html(" Press <b>[Up]</b>/<b>[Down]</b> to select."),
    showFrame: ~AppFilters.IsDone);
```

### With Accept-State Styling

```csharp
var style = Style.FromDict(new Dictionary<string, string>
{
    ["frame.border"] = "#ff4444",           // Red during editing
    ["accepted frame.border"] = "#888888",  // Gray after accept
});

var result = Dialogs.Choice(
    message: ...,
    options: [...],
    style: style,
    showFrame: true);  // Always show frame
```

### With Mouse Support

```csharp
var result = Dialogs.Choice(
    message: new Html("<u>Please select a dish</u>:"),
    options: [...],
    mouseSupport: true);  // Enable mouse clicks
```

## Controls

| Key | Action |
|-----|--------|
| ↑ / k | Move selection up |
| ↓ / j | Move selection down |
| Enter | Confirm selection |
| 1-9 | Jump to numbered option |
| Ctrl+C | Exit (graceful) |
| Ctrl+D | Exit (graceful) |
| Mouse click | Select option (if enabled) |

## Troubleshooting

### "Unknown example" Error

Ensure you're using exact example names (case-insensitive):
- ✅ `SimpleSelection`, `simpleselection`
- ❌ `Simple-Selection`, `simple_selection`

### Frame Not Showing

The `showFrame` parameter controls visibility:
- `~AppFilters.IsDone` → visible during editing, hidden after
- `true` → always visible
- Default (`default`) → no frame

### Mouse Not Working

Ensure you're running the `MouseSupport` example which has `mouseSupport: true`.
