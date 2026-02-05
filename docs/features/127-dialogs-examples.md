# Feature 127: Dialogs Examples (Complete Set)

## Overview

Implement ALL 9 Python Prompt Toolkit dialog examples in the `Stroke.Examples.Dialogs` project. These examples demonstrate the various dialog shortcut functions: message boxes, yes/no confirmation, button dialogs, input dialogs, password input, radio lists, checkbox lists, progress dialogs, and custom styling.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/examples/dialogs/`

| # | Python File | C# File | Description |
|---|-------------|---------|-------------|
| 1 | `messagebox.py` | `MessageBox.cs` | Simple message dialog with OK button |
| 2 | `yes_no_dialog.py` | `YesNoDialog.cs` | Yes/No confirmation dialog |
| 3 | `button_dialog.py` | `ButtonDialog.cs` | Custom button choices (Yes/No/Maybe) |
| 4 | `input_dialog.py` | `InputDialog.cs` | Text input prompt |
| 5 | `password_dialog.py` | `PasswordDialog.cs` | Masked password input |
| 6 | `radio_dialog.py` | `RadioDialog.cs` | Radio list selection with colors |
| 7 | `checkbox_dialog.py` | `CheckboxDialog.cs` | Multi-select checkbox list with styling |
| 8 | `progress_dialog.py` | `ProgressDialog.cs` | Background task with progress bar |
| 9 | `styled_messagebox.py` | `StyledMessageBox.cs` | Custom styled dialog |

## Python Source Code

### 1. messagebox.py

```python
from prompt_toolkit.shortcuts import message_dialog

def main():
    message_dialog(
        title="Example dialog window",
        text="Do you want to continue?\nPress ENTER to quit.",
    ).run()
```

### 2. yes_no_dialog.py

```python
from prompt_toolkit.shortcuts import yes_no_dialog

def main():
    result = yes_no_dialog(
        title="Yes/No dialog example", text="Do you want to confirm?"
    ).run()

    print(f"Result = {result}")
```

### 3. button_dialog.py

```python
from prompt_toolkit.shortcuts import button_dialog

def main():
    result = button_dialog(
        title="Button dialog example",
        text="Are you sure?",
        buttons=[("Yes", True), ("No", False), ("Maybe...", None)],
    ).run()

    print(f"Result = {result}")
```

### 4. input_dialog.py

```python
from prompt_toolkit.shortcuts import input_dialog

def main():
    result = input_dialog(
        title="Input dialog example", text="Please type your name:"
    ).run()

    print(f"Result = {result}")
```

### 5. password_dialog.py

```python
from prompt_toolkit.shortcuts import input_dialog

def main():
    result = input_dialog(
        title="Password dialog example",
        text="Please type your password:",
        password=True,
    ).run()

    print(f"Result = {result}")
```

### 6. radio_dialog.py

```python
from prompt_toolkit.formatted_text import HTML
from prompt_toolkit.shortcuts import radiolist_dialog

def main():
    result = radiolist_dialog(
        values=[
            ("red", "Red"),
            ("green", "Green"),
            ("blue", "Blue"),
            ("orange", "Orange"),
        ],
        title="Radiolist dialog example",
        text="Please select a color:",
    ).run()

    print(f"Result = {result}")

    # With HTML.
    result = radiolist_dialog(
        values=[
            ("red", HTML('<style bg="red" fg="white">Red</style>')),
            ("green", HTML('<style bg="green" fg="white">Green</style>')),
            ("blue", HTML('<style bg="blue" fg="white">Blue</style>')),
            ("orange", HTML('<style bg="orange" fg="white">Orange</style>')),
        ],
        title=HTML("Radiolist dialog example <reverse>with colors</reverse>"),
        text="Please select a color:",
    ).run()

    print(f"Result = {result}")
```

### 7. checkbox_dialog.py

```python
from prompt_toolkit.formatted_text import HTML
from prompt_toolkit.shortcuts import checkboxlist_dialog, message_dialog
from prompt_toolkit.styles import Style

results = checkboxlist_dialog(
    title="CheckboxList dialog",
    text="What would you like in your breakfast ?",
    values=[
        ("eggs", "Eggs"),
        ("bacon", HTML("<blue>Bacon</blue>")),
        ("croissants", "20 Croissants"),
        ("daily", "The breakfast of the day"),
    ],
    style=Style.from_dict(
        {
            "dialog": "bg:#cdbbb3",
            "button": "bg:#bf99a4",
            "checkbox": "#e8612c",
            "dialog.body": "bg:#a9cfd0",
            "dialog shadow": "bg:#c98982",
            "frame.label": "#fcaca3",
            "dialog.body label": "#fd8bb6",
        }
    ),
).run()
if results:
    message_dialog(
        title="Room service",
        text="You selected: {}\nGreat choice sir !".format(",".join(results)),
    ).run()
else:
    message_dialog("*starves*").run()
```

### 8. progress_dialog.py

```python
import os
import time

from prompt_toolkit.shortcuts import progress_dialog

def worker(set_percentage, log_text):
    """
    This worker function is called by `progress_dialog`. It will run in a
    background thread.

    The `set_percentage` function can be used to update the progress bar, while
    the `log_text` function can be used to log text in the logging window.
    """
    percentage = 0
    for dirpath, dirnames, filenames in os.walk("../.."):
        for f in filenames:
            log_text(f"{dirpath} / {f}\n")
            set_percentage(percentage + 1)
            percentage += 2

            if percentage == 100:
                break
        if percentage == 100:
            break

    # Show 100% for a second, before quitting.
    set_percentage(100)
    time.sleep(1)


def main():
    progress_dialog(
        title="Progress dialog example",
        text="As an examples, we walk through the filesystem and print all directories",
        run_callback=worker,
    ).run()
```

### 9. styled_messagebox.py

```python
from prompt_toolkit.formatted_text import HTML
from prompt_toolkit.shortcuts import message_dialog
from prompt_toolkit.styles import Style

# Custom color scheme.
example_style = Style.from_dict(
    {
        "dialog": "bg:#88ff88",
        "dialog frame-label": "bg:#ffffff #000000",
        "dialog.body": "bg:#000000 #00ff00",
        "dialog shadow": "bg:#00aa00",
    }
)


def main():
    message_dialog(
        title=HTML(
            '<style bg="blue" fg="white">Styled</style> '
            '<style fg="ansired">dialog</style> window'
        ),
        text="Do you want to continue?\nPress ENTER to quit.",
        style=example_style,
    ).run()
```

## Public API (Example Code)

### 1. MessageBox.cs

```csharp
// Simple message dialog example.
//
// Port of Python Prompt Toolkit's examples/dialogs/messagebox.py
//
// Demonstrates:
// - Basic Dialogs.MessageDialog() usage
// - Multi-line text with \n
// - OK button to dismiss

using Stroke.Shortcuts;

namespace Stroke.Examples.Dialogs;

public static class MessageBox
{
    public static void Run()
    {
        Dialogs.MessageDialog(
            title: "Example dialog window",
            text: "Do you want to continue?\nPress ENTER to quit.");
    }
}
```

### 2. YesNoDialog.cs

```csharp
// Yes/No confirmation dialog example.
//
// Port of Python Prompt Toolkit's examples/dialogs/yes_no_dialog.py
//
// Demonstrates:
// - Dialogs.YesNoDialog() returning bool
// - Simple confirmation pattern

using Stroke.Shortcuts;

namespace Stroke.Examples.Dialogs;

public static class YesNoDialog
{
    public static void Run()
    {
        var result = Dialogs.YesNoDialog(
            title: "Yes/No dialog example",
            text: "Do you want to confirm?");

        Console.WriteLine($"Result = {result}");
    }
}
```

### 3. ButtonDialog.cs

```csharp
// Button dialog example with custom choices.
//
// Port of Python Prompt Toolkit's examples/dialogs/button_dialog.py
//
// Demonstrates:
// - Dialogs.ButtonDialog<T>() with custom button values
// - Nullable return type for "Maybe" option
// - Multiple button choices

using Stroke.Shortcuts;

namespace Stroke.Examples.Dialogs;

public static class ButtonDialog
{
    public static void Run()
    {
        var result = Dialogs.ButtonDialog<bool?>(
            title: "Button dialog example",
            text: "Are you sure?",
            buttons: [("Yes", true), ("No", false), ("Maybe...", null)]);

        Console.WriteLine($"Result = {result}");
    }
}
```

### 4. InputDialog.cs

```csharp
// Input dialog example.
//
// Port of Python Prompt Toolkit's examples/dialogs/input_dialog.py
//
// Demonstrates:
// - Dialogs.InputDialog() for text input
// - Returns user-entered string

using Stroke.Shortcuts;

namespace Stroke.Examples.Dialogs;

public static class InputDialog
{
    public static void Run()
    {
        var result = Dialogs.InputDialog(
            title: "Input dialog example",
            text: "Please type your name:");

        Console.WriteLine($"Result = {result}");
    }
}
```

### 5. PasswordDialog.cs

```csharp
// Password input dialog example.
//
// Port of Python Prompt Toolkit's examples/dialogs/password_dialog.py
//
// Demonstrates:
// - Dialogs.InputDialog() with password: true
// - Masked input display

using Stroke.Shortcuts;

namespace Stroke.Examples.Dialogs;

public static class PasswordDialog
{
    public static void Run()
    {
        var result = Dialogs.InputDialog(
            title: "Password dialog example",
            text: "Please type your password:",
            password: true);

        Console.WriteLine($"Result = {result}");
    }
}
```

### 6. RadioDialog.cs

```csharp
// Radio list dialog example.
//
// Port of Python Prompt Toolkit's examples/dialogs/radio_dialog.py
//
// Demonstrates:
// - Dialogs.RadioListDialog<T>() for single selection
// - Plain text options
// - HTML styled options with colored backgrounds
// - HTML styled title

using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Dialogs;

public static class RadioDialog
{
    public static void Run()
    {
        // Plain text version
        var result = Dialogs.RadioListDialog(
            title: "Radiolist dialog example",
            text: "Please select a color:",
            values: (IReadOnlyList<(string, AnyFormattedText)>)[
                ("red", "Red"),
                ("green", "Green"),
                ("blue", "Blue"),
                ("orange", "Orange"),
            ]);

        Console.WriteLine($"Result = {result}");

        // With HTML styled options
        result = Dialogs.RadioListDialog(
            title: new Html("Radiolist dialog example <reverse>with colors</reverse>"),
            text: "Please select a color:",
            values: (IReadOnlyList<(string, AnyFormattedText)>)[
                ("red", new Html("<style bg=\"red\" fg=\"white\">Red</style>")),
                ("green", new Html("<style bg=\"green\" fg=\"white\">Green</style>")),
                ("blue", new Html("<style bg=\"blue\" fg=\"white\">Blue</style>")),
                ("orange", new Html("<style bg=\"orange\" fg=\"white\">Orange</style>")),
            ]);

        Console.WriteLine($"Result = {result}");
    }
}
```

### 7. CheckboxDialog.cs

```csharp
// Checkbox list dialog example.
//
// Port of Python Prompt Toolkit's examples/dialogs/checkbox_dialog.py
//
// Demonstrates:
// - Dialogs.CheckboxListDialog<T>() for multi-selection
// - HTML styled option labels
// - Custom dialog styling with Style.FromDict()
// - Chained dialogs based on result

using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Dialogs;

public static class CheckboxDialog
{
    public static void Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["dialog"] = "bg:#cdbbb3",
            ["button"] = "bg:#bf99a4",
            ["checkbox"] = "#e8612c",
            ["dialog.body"] = "bg:#a9cfd0",
            ["dialog shadow"] = "bg:#c98982",
            ["frame.label"] = "#fcaca3",
            ["dialog.body label"] = "#fd8bb6",
        });

        var results = Dialogs.CheckboxListDialog(
            title: "CheckboxList dialog",
            text: "What would you like in your breakfast ?",
            values: (IReadOnlyList<(string, AnyFormattedText)>)[
                ("eggs", "Eggs"),
                ("bacon", new Html("<blue>Bacon</blue>")),
                ("croissants", "20 Croissants"),
                ("daily", "The breakfast of the day"),
            ],
            style: style);

        if (results != null && results.Count > 0)
        {
            Dialogs.MessageDialog(
                title: "Room service",
                text: $"You selected: {string.Join(",", results)}\nGreat choice sir !");
        }
        else
        {
            Dialogs.MessageDialog(title: "*starves*");
        }
    }
}
```

### 8. ProgressDialog.cs

```csharp
// Progress dialog example.
//
// Port of Python Prompt Toolkit's examples/dialogs/progress_dialog.py
//
// Demonstrates:
// - Dialogs.ProgressDialog() with background worker
// - set_percentage callback for progress bar updates
// - log_text callback for logging output
// - File system walking simulation

using Stroke.Shortcuts;

namespace Stroke.Examples.Dialogs;

public static class ProgressDialog
{
    public static void Run()
    {
        Dialogs.ProgressDialog(
            title: "Progress dialog example",
            text: "As an example, we walk through the filesystem and print all directories",
            runCallback: Worker);
    }

    private static void Worker(Action<int> setPercentage, Action<string> logText)
    {
        // This worker function is called by ProgressDialog. It runs in a
        // background thread.
        //
        // The setPercentage function updates the progress bar, while
        // logText logs text in the logging window.

        var percentage = 0;
        var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

        try
        {
            foreach (var file in Directory.EnumerateFiles(baseDir, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(baseDir, file);
                logText($"{relativePath}\n");
                percentage += 2;
                setPercentage(Math.Min(percentage, 99));
                Thread.Sleep(100);

                if (percentage >= 100)
                    break;
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip directories we can't access
        }

        // Show 100% for a second, before quitting.
        setPercentage(100);
        Thread.Sleep(1000);
    }
}
```

### 9. StyledMessageBox.cs

```csharp
// Styled message dialog example.
//
// Port of Python Prompt Toolkit's examples/dialogs/styled_messagebox.py
//
// Demonstrates:
// - Custom dialog styling with Style.FromDict()
// - HTML formatted title with inline styles
// - Green terminal aesthetic

using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Dialogs;

public static class StyledMessageBox
{
    public static void Run()
    {
        // Custom color scheme - green terminal aesthetic
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["dialog"] = "bg:#88ff88",
            ["dialog frame-label"] = "bg:#ffffff #000000",
            ["dialog.body"] = "bg:#000000 #00ff00",
            ["dialog shadow"] = "bg:#00aa00",
        });

        Dialogs.MessageDialog(
            title: new Html(
                "<style bg=\"blue\" fg=\"white\">Styled</style> " +
                "<style fg=\"ansired\">dialog</style> window"),
            text: "Do you want to continue?\nPress ENTER to quit.",
            style: style);
    }
}
```

## Project Structure

```
examples/
├── Stroke.Examples.sln
├── Stroke.Examples.Prompts/              # Existing
├── Stroke.Examples.Choices/              # Existing (Feature 126)
└── Stroke.Examples.Dialogs/              # This feature
    ├── Stroke.Examples.Dialogs.csproj
    ├── Program.cs                        # Entry point with example routing
    ├── MessageBox.cs                     # Example 1
    ├── YesNoDialog.cs                    # Example 2
    ├── ButtonDialog.cs                   # Example 3
    ├── InputDialog.cs                    # Example 4
    ├── PasswordDialog.cs                 # Example 5
    ├── RadioDialog.cs                    # Example 6
    ├── CheckboxDialog.cs                 # Example 7
    ├── ProgressDialog.cs                 # Example 8
    └── StyledMessageBox.cs               # Example 9
```

## Implementation Notes

### Project File (Stroke.Examples.Dialogs.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>13</LangVersion>
    <RootNamespace>Stroke.Examples.Dialogs</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Stroke\Stroke.csproj" />
  </ItemGroup>

</Project>
```

### Program.cs (Entry Point)

```csharp
// Stroke.Examples.Dialogs entry point
//
// Usage:
//   dotnet run                            # Runs MessageBox (default)
//   dotnet run -- MessageBox              # Simple message dialog
//   dotnet run -- YesNoDialog             # Yes/No confirmation
//   dotnet run -- ButtonDialog            # Custom button choices
//   dotnet run -- InputDialog             # Text input
//   dotnet run -- PasswordDialog          # Password input (masked)
//   dotnet run -- RadioDialog             # Radio list selection
//   dotnet run -- CheckboxDialog          # Multi-select checkbox
//   dotnet run -- ProgressDialog          # Progress bar with worker
//   dotnet run -- StyledMessageBox        # Custom styled dialog

namespace Stroke.Examples.Dialogs;

public static class Program
{
    private static readonly Dictionary<string, Action> Examples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["MessageBox"] = MessageBox.Run,
        ["YesNoDialog"] = YesNoDialog.Run,
        ["ButtonDialog"] = ButtonDialog.Run,
        ["InputDialog"] = InputDialog.Run,
        ["PasswordDialog"] = PasswordDialog.Run,
        ["RadioDialog"] = RadioDialog.Run,
        ["CheckboxDialog"] = CheckboxDialog.Run,
        ["ProgressDialog"] = ProgressDialog.Run,
        ["StyledMessageBox"] = StyledMessageBox.Run,
    };

    public static void Main(string[] args)
    {
        var exampleName = args.Length > 0 ? args[0] : "MessageBox";

        if (Examples.TryGetValue(exampleName, out var runExample))
        {
            try
            {
                runExample();
            }
            catch (KeyboardInterrupt)
            {
                // Ctrl+C pressed - exit gracefully
            }
            catch (EOFException)
            {
                // Ctrl+D pressed - exit gracefully
            }
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
| MessageBox | Simple alert | `Dialogs.MessageDialog()` |
| YesNoDialog | Boolean confirmation | `Dialogs.YesNoDialog()` |
| ButtonDialog | Custom choices | `Dialogs.ButtonDialog<T>()` |
| InputDialog | Text input | `Dialogs.InputDialog()` |
| PasswordDialog | Masked input | `Dialogs.InputDialog(password: true)` |
| RadioDialog | Single selection | `Dialogs.RadioListDialog<T>()` |
| CheckboxDialog | Multi-selection | `Dialogs.CheckboxListDialog<T>()` |
| ProgressDialog | Background task | `Dialogs.ProgressDialog()` |
| StyledMessageBox | Custom styling | `Style.FromDict()`, `Html` title |

### Style Classes Used

| Style Class | Purpose | Examples Using |
|-------------|---------|----------------|
| `dialog` | Dialog background | CheckboxDialog, StyledMessageBox |
| `dialog.body` | Body area background | CheckboxDialog, StyledMessageBox |
| `dialog shadow` | Shadow effect | CheckboxDialog, StyledMessageBox |
| `dialog frame-label` | Title label | StyledMessageBox |
| `dialog.body label` | Body text labels | CheckboxDialog |
| `button` | Button styling | CheckboxDialog |
| `checkbox` | Checkbox marker color | CheckboxDialog |
| `frame.label` | Frame title color | CheckboxDialog |

## Dependencies

- Feature 48: Dialog Shortcuts (all dialog functions) — **already implemented**
- Feature 45: Base Widgets (RadioList, CheckboxList, Dialog, Button) — **already implemented**
- Feature 18: Styles System (Style.FromDict()) — **already implemented**
- Feature 15: Formatted Text (Html) — **already implemented**

All dependencies are already implemented and tested.

## Implementation Tasks

1. Create `examples/Stroke.Examples.Dialogs/` directory
2. Create `Stroke.Examples.Dialogs.csproj` project file
3. Create `Program.cs` entry point with dictionary-based routing
4. Create `MessageBox.cs` example
5. Create `YesNoDialog.cs` example
6. Create `ButtonDialog.cs` example
7. Create `InputDialog.cs` example
8. Create `PasswordDialog.cs` example
9. Create `RadioDialog.cs` example
10. Create `CheckboxDialog.cs` example
11. Create `ProgressDialog.cs` example
12. Create `StyledMessageBox.cs` example
13. Add project to `Stroke.Examples.sln`
14. Verify all examples build: `dotnet build examples/Stroke.Examples.sln`
15. Test each example manually with TUI Driver

## Acceptance Criteria

### General
- [ ] `examples/Stroke.Examples.Dialogs/` directory exists with all 9 examples
- [ ] `Stroke.Examples.Dialogs.csproj` builds successfully
- [ ] Project is included in `Stroke.Examples.sln`
- [ ] All examples run without errors

### MessageBox
- [ ] Displays dialog with title "Example dialog window"
- [ ] Shows multi-line text with \n
- [ ] OK button dismisses dialog
- [ ] Enter key dismisses dialog

### YesNoDialog
- [ ] Displays Yes and No buttons
- [ ] Tab switches between buttons
- [ ] Returns true for Yes, false for No
- [ ] Prints result to console

### ButtonDialog
- [ ] Shows three buttons: Yes, No, Maybe...
- [ ] Returns true, false, or null based on selection
- [ ] Tab navigates between buttons

### InputDialog
- [ ] Shows text input field
- [ ] User can type name
- [ ] OK returns typed text
- [ ] Cancel returns null

### PasswordDialog
- [ ] Input is masked with asterisks
- [ ] Otherwise behaves like InputDialog

### RadioDialog
- [ ] First run shows plain text options
- [ ] Second run shows HTML colored options
- [ ] Arrow keys navigate options
- [ ] Enter confirms selection

### CheckboxDialog
- [ ] Custom styled dialog background
- [ ] Multiple items can be selected with Space
- [ ] HTML styled "Bacon" option in blue
- [ ] Shows follow-up message with selections

### ProgressDialog
- [ ] Progress bar updates during execution
- [ ] Log text area shows file names
- [ ] Completes at 100% then closes
- [ ] Works with background thread

### StyledMessageBox
- [ ] Green terminal color scheme
- [ ] HTML styled title with blue and red text
- [ ] Custom shadow color

## Verification with TUI Driver

### Test MessageBox

```javascript
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Dialogs"],
  cols: 80, rows: 24
});

await tui_wait_for_text({ session_id: session.id, text: "Example dialog window" });
await tui_wait_for_text({ session_id: session.id, text: "Do you want to continue?" });
await tui_press_key({ session_id: session.id, key: "Enter" });
await tui_wait_for_idle({ session_id: session.id });
await tui_close({ session_id: session.id });
```

### Test YesNoDialog

```javascript
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Dialogs", "--", "YesNoDialog"],
  cols: 80, rows: 24
});

await tui_wait_for_text({ session_id: session.id, text: "Do you want to confirm?" });
await tui_wait_for_text({ session_id: session.id, text: "Yes" });
await tui_press_key({ session_id: session.id, key: "Enter" });
await tui_wait_for_text({ session_id: session.id, text: "Result = True" });
await tui_close({ session_id: session.id });
```

### Test InputDialog

```javascript
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Dialogs", "--", "InputDialog"],
  cols: 80, rows: 24
});

await tui_wait_for_text({ session_id: session.id, text: "Please type your name:" });
await tui_send_text({ session_id: session.id, text: "Alice" });
await tui_press_key({ session_id: session.id, key: "Enter" });
await tui_wait_for_text({ session_id: session.id, text: "Result = Alice" });
await tui_close({ session_id: session.id });
```

### Test ProgressDialog

```javascript
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Dialogs", "--", "ProgressDialog"],
  cols: 80, rows: 24
});

await tui_wait_for_text({ session_id: session.id, text: "Progress dialog example" });
// Wait for progress to complete (may take several seconds)
await tui_wait_for_idle({ session_id: session.id, timeout_ms: 30000 });
await tui_close({ session_id: session.id });
```

## Notes on Dialog API

The Stroke dialog shortcuts follow Python Prompt Toolkit's pattern where:
- `Dialogs.MessageDialog()` is a synchronous wrapper
- `Dialogs.MessageDialogAsync()` is the async variant
- All dialogs return their result directly (not an Application object)

This differs slightly from Python where you call `.run()` on the returned Application:

```python
# Python
result = yes_no_dialog(title="...", text="...").run()
```

```csharp
// C#
var result = Dialogs.YesNoDialog(title: "...", text: "...");
```
