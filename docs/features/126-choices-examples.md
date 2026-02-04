# Feature 126: Choices Examples (Complete Set)

## Overview

Implement ALL 8 Python Prompt Toolkit choices examples in the `Stroke.Examples.Choices` project. These examples demonstrate various capabilities of the `Dialogs.Choice<T>()` method: basic selection, default values, custom styling, frames, bottom toolbars, style changes on accept, scrollable lists, and mouse support.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/examples/choices/`

| # | Python File | C# File | Description |
|---|-------------|---------|-------------|
| 1 | `simple-selection.py` | `SimpleSelection.cs` | Basic option selection |
| 2 | `default.py` | `Default.cs` | Pre-selected default value with HTML message |
| 3 | `color.py` | `Color.cs` | Custom styling with colored options |
| 4 | `with-frame.py` | `WithFrame.cs` | Frame around choices (hidden on accept) |
| 5 | `frame-and-bottom-toolbar.py` | `FrameAndBottomToolbar.cs` | Frame + instructional toolbar |
| 6 | `gray-frame-on-accept.py` | `GrayFrameOnAccept.cs` | Frame color changes on accept |
| 7 | `many-choices.py` | `ManyChoices.cs` | Scrollable list with 99 options |
| 8 | `mouse-support.py` | `MouseSupport.cs` | Mouse click selection |

## Python Source Code

### 1. simple-selection.py

```python
from prompt_toolkit.shortcuts import choice

def main() -> None:
    result = choice(
        message="Please select a dish:",
        options=[
            ("pizza", "Pizza with mushrooms"),
            ("salad", "Salad with tomatoes"),
            ("sushi", "Sushi"),
        ],
    )
    print(result)
```

### 2. default.py

```python
from prompt_toolkit.formatted_text import HTML
from prompt_toolkit.shortcuts import choice

def main() -> None:
    result = choice(
        message=HTML("<u>Please select a dish</u>:"),
        options=[
            ("pizza", "Pizza with mushrooms"),
            ("salad", "Salad with tomatoes"),
            ("sushi", "Sushi"),
        ],
        default="salad",
    )
    print(result)
```

### 3. color.py

```python
from prompt_toolkit.formatted_text import HTML
from prompt_toolkit.shortcuts import choice
from prompt_toolkit.styles import Style

def main() -> None:
    style = Style.from_dict(
        {
            "input-selection": "fg:#ff0000",
            "number": "fg:#884444 bold",
            "selected-option": "underline",
            "frame.border": "#884444",
        }
    )

    result = choice(
        message=HTML("<u>Please select a dish</u>:"),
        options=[
            ("pizza", "Pizza with mushrooms"),
            (
                "salad",
                HTML("<ansigreen>Salad</ansigreen> with <ansired>tomatoes</ansired>"),
            ),
            ("sushi", "Sushi"),
        ],
        style=style,
    )
    print(result)
```

### 4. with-frame.py

```python
from prompt_toolkit.filters import is_done
from prompt_toolkit.formatted_text import HTML
from prompt_toolkit.shortcuts import choice
from prompt_toolkit.styles import Style

def main() -> None:
    style = Style.from_dict(
        {
            "frame.border": "#884444",
            "selected-option": "bold underline",
        }
    )

    result = choice(
        message=HTML("<u>Please select a dish</u>:"),
        options=[
            ("pizza", "Pizza with mushrooms"),
            ("salad", "Salad with tomatoes"),
            ("sushi", "Sushi"),
        ],
        style=style,
        # Use `~is_done`, if you only want to show the frame while editing and
        # hide it when the input is accepted.
        # Use `True`, if you always want to show the frame.
        show_frame=~is_done,
    )
    print(result)
```

### 5. frame-and-bottom-toolbar.py

```python
from prompt_toolkit.filters import is_done
from prompt_toolkit.formatted_text import HTML
from prompt_toolkit.shortcuts import choice
from prompt_toolkit.styles import Style

def main() -> None:
    style = Style.from_dict(
        {
            "frame.border": "#ff4444",
            "selected-option": "bold",
            # We use 'noreverse' because the default style for 'bottom-toolbar'
            # uses 'reverse'.
            "bottom-toolbar": "#ffffff bg:#333333 noreverse",
        }
    )

    result = choice(
        message=HTML("<u>Please select a dish</u>:"),
        options=[
            ("pizza", "Pizza with mushrooms"),
            ("salad", "Salad with tomatoes"),
            ("sushi", "Sushi"),
        ],
        style=style,
        bottom_toolbar=HTML(
            " Press <b>[Up]</b>/<b>[Down]</b> to select, <b>[Enter]</b> to accept."
        ),
        # Use `~is_done`, if you only want to show the frame while editing and
        # hide it when the input is accepted.
        # Use `True`, if you always want to show the frame.
        show_frame=~is_done,
    )
    print(result)
```

### 6. gray-frame-on-accept.py

```python
from prompt_toolkit.formatted_text import HTML
from prompt_toolkit.shortcuts import choice
from prompt_toolkit.styles import Style

def main() -> None:
    style = Style.from_dict(
        {
            "selected-option": "bold",
            "frame.border": "#ff4444",
            "accepted frame.border": "#888888",
        }
    )

    result = choice(
        message=HTML("<u>Please select a dish</u>:"),
        options=[
            ("pizza", "Pizza with mushrooms"),
            ("salad", "Salad with tomatoes"),
            ("sushi", "Sushi"),
        ],
        style=style,
        show_frame=True,
    )
    print(result)
```

### 7. many-choices.py

```python
from prompt_toolkit.shortcuts import choice

def main() -> None:
    result = choice(
        message="Please select an option:",
        options=[(i, f"Option {i}") for i in range(1, 100)],
    )
    print(result)
```

### 8. mouse-support.py

```python
from prompt_toolkit.formatted_text import HTML
from prompt_toolkit.shortcuts import choice

def main() -> None:
    result = choice(
        message=HTML("<u>Please select a dish</u>:"),
        options=[
            ("pizza", "Pizza with mushrooms"),
            ("salad", "Salad with tomatoes"),
            ("sushi", "Sushi"),
        ],
        mouse_support=True,
    )
    print(result)
```

## Public API (Example Code)

### 1. SimpleSelection.cs

```csharp
// Basic option selection example.
//
// Port of Python Prompt Toolkit's examples/choices/simple-selection.py
//
// Demonstrates:
// - Basic Dialogs.Choice<T>() usage
// - Tuple-based options (value, display text)
// - Arrow key navigation
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
        Console.WriteLine(result);
    }
}
```

### 2. Default.cs

```csharp
// Default value selection example.
//
// Port of Python Prompt Toolkit's examples/choices/default.py
//
// Demonstrates:
// - Pre-selected default value
// - HTML formatted message with underline
// - Selection starts on specified default option

using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

public static class Default
{
    public static void Run()
    {
        var result = Dialogs.Choice(
            message: Html.Parse("<u>Please select a dish</u>:"),
            options: [
                ("pizza", "Pizza with mushrooms"),
                ("salad", "Salad with tomatoes"),
                ("sushi", "Sushi"),
            ],
            defaultValue: "salad");
        Console.WriteLine(result);
    }
}
```

### 3. Color.cs

```csharp
// Custom styling example with colored options.
//
// Port of Python Prompt Toolkit's examples/choices/color.py
//
// Demonstrates:
// - Custom Style with multiple style rules
// - HTML formatted option labels with ANSI colors
// - Style classes: input-selection, number, selected-option, frame.border

using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Choices;

public static class Color
{
    public static void Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["input-selection"] = "fg:#ff0000",
            ["number"] = "fg:#884444 bold",
            ["selected-option"] = "underline",
            ["frame.border"] = "#884444",
        });

        var result = Dialogs.Choice(
            message: Html.Parse("<u>Please select a dish</u>:"),
            options: [
                ("pizza", "Pizza with mushrooms"),
                ("salad", Html.Parse("<ansigreen>Salad</ansigreen> with <ansired>tomatoes</ansired>")),
                ("sushi", "Sushi"),
            ],
            style: style);
        Console.WriteLine(result);
    }
}
```

### 4. WithFrame.cs

```csharp
// Frame around choices example.
//
// Port of Python Prompt Toolkit's examples/choices/with-frame.py
//
// Demonstrates:
// - Frame border around the selection UI
// - ~IsDone filter: frame visible during editing, hidden on accept
// - Custom frame border color via style

using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Choices;

public static class WithFrame
{
    public static void Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["frame.border"] = "#884444",
            ["selected-option"] = "bold underline",
        });

        var result = Dialogs.Choice(
            message: Html.Parse("<u>Please select a dish</u>:"),
            options: [
                ("pizza", "Pizza with mushrooms"),
                ("salad", "Salad with tomatoes"),
                ("sushi", "Sushi"),
            ],
            style: style,
            // Use ~IsDone if you only want to show the frame while editing and
            // hide it when the input is accepted.
            // Use true if you always want to show the frame.
            showFrame: ~AppFilters.IsDone);
        Console.WriteLine(result);
    }
}
```

### 5. FrameAndBottomToolbar.cs

```csharp
// Frame and bottom toolbar example.
//
// Port of Python Prompt Toolkit's examples/choices/frame-and-bottom-toolbar.py
//
// Demonstrates:
// - Frame border around selection
// - Bottom toolbar with instructional text
// - Custom toolbar styling with noreverse
// - ~IsDone filter for frame visibility

using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Choices;

public static class FrameAndBottomToolbar
{
    public static void Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["frame.border"] = "#ff4444",
            ["selected-option"] = "bold",
            // We use 'noreverse' because the default style for 'bottom-toolbar'
            // uses 'reverse'.
            ["bottom-toolbar"] = "#ffffff bg:#333333 noreverse",
        });

        var result = Dialogs.Choice(
            message: Html.Parse("<u>Please select a dish</u>:"),
            options: [
                ("pizza", "Pizza with mushrooms"),
                ("salad", "Salad with tomatoes"),
                ("sushi", "Sushi"),
            ],
            style: style,
            bottomToolbar: Html.Parse(" Press <b>[Up]</b>/<b>[Down]</b> to select, <b>[Enter]</b> to accept."),
            // Use ~IsDone if you only want to show the frame while editing and
            // hide it when the input is accepted.
            // Use true if you always want to show the frame.
            showFrame: ~AppFilters.IsDone);
        Console.WriteLine(result);
    }
}
```

### 6. GrayFrameOnAccept.cs

```csharp
// Gray frame on accept example.
//
// Port of Python Prompt Toolkit's examples/choices/gray-frame-on-accept.py
//
// Demonstrates:
// - Frame always visible (showFrame: true)
// - Style changes on accept using "accepted frame.border" class
// - Red frame (#ff4444) during editing, gray frame (#888888) on accept

using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Choices;

public static class GrayFrameOnAccept
{
    public static void Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["selected-option"] = "bold",
            ["frame.border"] = "#ff4444",
            ["accepted frame.border"] = "#888888",
        });

        var result = Dialogs.Choice(
            message: Html.Parse("<u>Please select a dish</u>:"),
            options: [
                ("pizza", "Pizza with mushrooms"),
                ("salad", "Salad with tomatoes"),
                ("sushi", "Sushi"),
            ],
            style: style,
            showFrame: true);
        Console.WriteLine(result);
    }
}
```

### 7. ManyChoices.cs

```csharp
// Many choices example with scrollable list.
//
// Port of Python Prompt Toolkit's examples/choices/many-choices.py
//
// Demonstrates:
// - Large option list (99 items)
// - Automatic scrolling when list exceeds screen height
// - LINQ-generated options

using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

public static class ManyChoices
{
    public static void Run()
    {
        var result = Dialogs.Choice(
            message: "Please select an option:",
            options: Enumerable.Range(1, 99)
                .Select(i => (i, $"Option {i}"))
                .ToList());
        Console.WriteLine(result);
    }
}
```

### 8. MouseSupport.cs

```csharp
// Mouse support example.
//
// Port of Python Prompt Toolkit's examples/choices/mouse-support.py
//
// Demonstrates:
// - Mouse click selection on options
// - Combined keyboard and mouse navigation
// - mouseSupport: true parameter

using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

public static class MouseSupport
{
    public static void Run()
    {
        var result = Dialogs.Choice(
            message: Html.Parse("<u>Please select a dish</u>:"),
            options: [
                ("pizza", "Pizza with mushrooms"),
                ("salad", "Salad with tomatoes"),
                ("sushi", "Sushi"),
            ],
            mouseSupport: true);
        Console.WriteLine(result);
    }
}
```

## Project Structure

```
examples/
├── Stroke.Examples.sln
├── Stroke.Examples.Prompts/              # Existing (Feature 122)
│   └── ...
└── Stroke.Examples.Choices/              # This feature
    ├── Stroke.Examples.Choices.csproj
    ├── Program.cs                        # Entry point with example routing
    ├── SimpleSelection.cs                # Example 1
    ├── Default.cs                        # Example 2
    ├── Color.cs                          # Example 3
    ├── WithFrame.cs                      # Example 4
    ├── FrameAndBottomToolbar.cs          # Example 5
    ├── GrayFrameOnAccept.cs              # Example 6
    ├── ManyChoices.cs                    # Example 7
    └── MouseSupport.cs                   # Example 8
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
//   dotnet run                            # Runs SimpleSelection (default)
//   dotnet run -- SimpleSelection         # Runs SimpleSelection explicitly
//   dotnet run -- Default                 # Default value selection
//   dotnet run -- Color                   # Styled colored options
//   dotnet run -- WithFrame               # Frame around choices
//   dotnet run -- FrameAndBottomToolbar   # Frame + toolbar
//   dotnet run -- GrayFrameOnAccept       # Style changes on accept
//   dotnet run -- ManyChoices             # Scrollable 99-option list
//   dotnet run -- MouseSupport            # Mouse click selection

namespace Stroke.Examples.Choices;

public static class Program
{
    private static readonly Dictionary<string, Action> Examples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SimpleSelection"] = SimpleSelection.Run,
        ["Default"] = Default.Run,
        ["Color"] = Color.Run,
        ["WithFrame"] = WithFrame.Run,
        ["FrameAndBottomToolbar"] = FrameAndBottomToolbar.Run,
        ["GrayFrameOnAccept"] = GrayFrameOnAccept.Run,
        ["ManyChoices"] = ManyChoices.Run,
        ["MouseSupport"] = MouseSupport.Run,
    };

    public static void Main(string[] args)
    {
        var exampleName = args.Length > 0 ? args[0] : "SimpleSelection";

        if (Examples.TryGetValue(exampleName, out var runExample))
        {
            runExample();
        }
        else
        {
            Console.WriteLine($"Unknown example: {exampleName}");
            Console.WriteLine($"Available examples: {string.Join(", ", Examples.Keys)}");
            Environment.Exit(1);
        }
    }
}
```

### Key Concepts Demonstrated

| Example | Key Concept | Stroke API |
|---------|-------------|------------|
| SimpleSelection | Basic selection | `Dialogs.Choice<T>()` |
| Default | Pre-selected value | `defaultValue` parameter |
| Color | Custom styling | `Style.FromDict()`, `Html.Parse()` for labels |
| WithFrame | Conditional frame | `showFrame: ~AppFilters.IsDone` |
| FrameAndBottomToolbar | Toolbar + frame | `bottomToolbar` parameter |
| GrayFrameOnAccept | Accept styling | `"accepted frame.border"` style class |
| ManyChoices | Scrollable list | Large option count + LINQ |
| MouseSupport | Mouse interaction | `mouseSupport: true` |

### Style Classes Used

| Style Class | Purpose | Examples Using |
|-------------|---------|----------------|
| `input-selection` | Container styling | Color |
| `number` | Option number prefix | Color |
| `selected-option` | Currently selected item | Color, WithFrame, GrayFrameOnAccept, FrameAndBottomToolbar |
| `frame.border` | Frame border color | Color, WithFrame, FrameAndBottomToolbar, GrayFrameOnAccept |
| `accepted frame.border` | Border color after accept | GrayFrameOnAccept |
| `bottom-toolbar` | Toolbar styling | FrameAndBottomToolbar |

## Dependencies

- Feature 56: ChoiceInput<T> (selection prompt widget) — **already implemented**
- Feature 48: Dialog Shortcuts (Dialogs.Choice<T>()) — **already implemented**
- Feature 45: Base Widgets (RadioList<T>, Dialog, Frame) — **already implemented**
- Feature 32: Application Filters (AppFilters.IsDone) — **already implemented**
- Feature 18: Styles System (Style.FromDict()) — **already implemented**
- Feature 15: Formatted Text (Html.Parse()) — **already implemented**

All dependencies are already implemented and tested.

## Implementation Tasks

1. Create `examples/Stroke.Examples.Choices/` directory (if not exists)
2. Create `Stroke.Examples.Choices.csproj` project file
3. Create `Program.cs` entry point with dictionary-based routing
4. Create `SimpleSelection.cs` example
5. Create `Default.cs` example
6. Create `Color.cs` example
7. Create `WithFrame.cs` example
8. Create `FrameAndBottomToolbar.cs` example
9. Create `GrayFrameOnAccept.cs` example
10. Create `ManyChoices.cs` example
11. Create `MouseSupport.cs` example
12. Add project to `Stroke.Examples.sln`
13. Verify all examples build: `dotnet build examples/Stroke.Examples.sln`
14. Test each example manually with TUI Driver

## Acceptance Criteria

### General
- [ ] `examples/Stroke.Examples.Choices/` directory exists with all 8 examples
- [ ] `Stroke.Examples.Choices.csproj` builds successfully
- [ ] Project is included in `Stroke.Examples.sln`
- [ ] All examples run without errors

### SimpleSelection
- [ ] Displays "Please select a dish:" message
- [ ] Shows 3 options with numbers
- [ ] Arrow keys navigate options
- [ ] Enter confirms selection
- [ ] Prints selected value (e.g., "pizza")

### Default
- [ ] Displays HTML underlined message
- [ ] "salad" option is pre-selected (cursor starts there)
- [ ] Pressing Enter immediately selects "salad"

### Color
- [ ] Custom red foreground on selection
- [ ] Bold dark red numbers
- [ ] Underlined selected option
- [ ] "Salad" displays in green, "tomatoes" in red

### WithFrame
- [ ] Frame visible during selection
- [ ] Frame disappears when Enter is pressed
- [ ] Selected option shows bold underline

### FrameAndBottomToolbar
- [ ] Frame visible during selection
- [ ] Bottom toolbar shows navigation instructions
- [ ] Toolbar has white text on dark gray background
- [ ] Frame and toolbar disappear on accept

### GrayFrameOnAccept
- [ ] Red frame (#ff4444) during selection
- [ ] Frame turns gray (#888888) when Enter is pressed
- [ ] Frame remains visible after accept

### ManyChoices
- [ ] 99 options displayed (Option 1 through Option 99)
- [ ] List scrolls when navigating past visible area
- [ ] Can navigate to any option with arrow keys

### MouseSupport
- [ ] Mouse click selects an option
- [ ] Click + Enter confirms selection
- [ ] Keyboard navigation still works

## Verification with TUI Driver

### Test SimpleSelection

```javascript
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Choices"],
  cols: 80, rows: 24
});

await tui_wait_for_text({ session_id: session.id, text: "Please select a dish:" });
await tui_wait_for_text({ session_id: session.id, text: "Pizza with mushrooms" });
await tui_press_key({ session_id: session.id, key: "Down" });
await tui_press_key({ session_id: session.id, key: "Enter" });
await tui_wait_for_text({ session_id: session.id, text: "salad" });
await tui_close({ session_id: session.id });
```

### Test Default

```javascript
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Choices", "--", "Default"],
  cols: 80, rows: 24
});

await tui_wait_for_text({ session_id: session.id, text: "Please select a dish" });
// Press Enter immediately - should select "salad" (the default)
await tui_press_key({ session_id: session.id, key: "Enter" });
await tui_wait_for_text({ session_id: session.id, text: "salad" });
await tui_close({ session_id: session.id });
```

### Test ManyChoices (scrolling)

```javascript
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Choices", "--", "ManyChoices"],
  cols: 80, rows: 24
});

await tui_wait_for_text({ session_id: session.id, text: "Please select an option:" });
// Navigate down many times to trigger scrolling
for (let i = 0; i < 50; i++) {
  await tui_press_key({ session_id: session.id, key: "Down" });
}
await tui_wait_for_idle({ session_id: session.id });
// Option 51 should be visible after scrolling
const text = await tui_text({ session_id: session.id });
// Verify we can see higher-numbered options
await tui_press_key({ session_id: session.id, key: "Enter" });
await tui_close({ session_id: session.id });
```

### Test MouseSupport

```javascript
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Choices", "--", "MouseSupport"],
  cols: 80, rows: 24
});

await tui_wait_for_text({ session_id: session.id, text: "Please select a dish" });
// Get snapshot to find clickable elements
const snapshot = await tui_snapshot({ session_id: session.id });
// Click on "Sushi" option (approximate coordinates)
await tui_click_at({ session_id: session.id, x: 10, y: 5 });
await tui_press_key({ session_id: session.id, key: "Enter" });
await tui_close({ session_id: session.id });
```

## Notes on Filter Usage

The `~is_done` filter in Python becomes `~AppFilters.IsDone` in C#:

```python
# Python
show_frame=~is_done
```

```csharp
// C#
showFrame: ~AppFilters.IsDone
```

This filter returns `true` while the application is running and `false` once the user has made a selection. The `~` operator inverts the filter, so:
- During selection: `~IsDone` = `true` → frame visible
- After Enter: `~IsDone` = `false` → frame hidden

## Notes on Style Class Prefixes

The `accepted` prefix in style classes (e.g., `"accepted frame.border"`) applies styles only when the application is in the "accepted" state (after the user confirms their selection). This enables visual feedback transitions.

## Relationship to Feature 125

Feature 125 (Simple Selection Example) established the Choices project structure and implemented the first example. This feature (126) completes the remaining 7 examples and provides comprehensive coverage of all choice input scenarios.

If the project structure from Feature 125 already exists, skip those setup steps and focus on adding the 7 new example files.
