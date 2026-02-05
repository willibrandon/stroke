# Quickstart: Dialogs Examples

**Feature**: 063-dialogs-examples
**Date**: 2026-02-04

## Prerequisites

- .NET 10 SDK installed
- Stroke repository cloned
- Terminal with VT100 support (most modern terminals)

## Running Examples

### Build All Examples

```bash
cd /Users/brandon/src/stroke
dotnet build examples/Stroke.Examples.sln
```

### Run a Specific Example

```bash
# General pattern
dotnet run --project examples/Stroke.Examples.Dialogs -- <example-name>

# Examples:
dotnet run --project examples/Stroke.Examples.Dialogs -- message-box
dotnet run --project examples/Stroke.Examples.Dialogs -- yes-no-dialog
dotnet run --project examples/Stroke.Examples.Dialogs -- button-dialog
dotnet run --project examples/Stroke.Examples.Dialogs -- input-dialog
dotnet run --project examples/Stroke.Examples.Dialogs -- password-dialog
dotnet run --project examples/Stroke.Examples.Dialogs -- radio-dialog
dotnet run --project examples/Stroke.Examples.Dialogs -- checkbox-dialog
dotnet run --project examples/Stroke.Examples.Dialogs -- progress-dialog
dotnet run --project examples/Stroke.Examples.Dialogs -- styled-message-box
```

### Run Default Example

```bash
# Without arguments, runs message-box (default)
dotnet run --project examples/Stroke.Examples.Dialogs
```

## Example Descriptions

| Example | Description | Key API |
|---------|-------------|---------|
| `message-box` | Simple message dialog with OK button | `Dialogs.MessageDialog()` |
| `yes-no-dialog` | Yes/No confirmation returning boolean | `Dialogs.YesNoDialog()` |
| `button-dialog` | Custom buttons with nullable values | `Dialogs.ButtonDialog<T>()` |
| `input-dialog` | Text input with prompt | `Dialogs.InputDialog()` |
| `password-dialog` | Masked password input | `Dialogs.InputDialog(password: true)` |
| `radio-dialog` | Single-selection list (plain + styled) | `Dialogs.RadioListDialog<T>()` |
| `checkbox-dialog` | Multi-selection with custom styling | `Dialogs.CheckboxListDialog<T>()` |
| `progress-dialog` | Background task with progress bar | `Dialogs.ProgressDialog()` |
| `styled-message-box` | Custom colors via Style.FromDict() | `Style.FromDict()` + HTML title |

## Keyboard Navigation

All dialog examples support:
- **Tab** / **Shift+Tab**: Move focus between buttons/fields
- **Enter**: Activate focused button
- **Space**: Toggle checkbox selection (checkbox-dialog only)
- **Arrow keys**: Navigate lists (radio-dialog, checkbox-dialog)
- **Ctrl+C**: Cancel and exit gracefully
- **Ctrl+D**: Exit gracefully (EOF)

## Mouse Support

Dialogs have mouse support enabled:
- Click buttons to activate
- Click list items to select
- Click text fields to focus

## Code Structure

Each example follows this pattern:

```csharp
internal static class ExampleName
{
    public static void Run()
    {
        var result = Dialogs.SomeDialog(
            title: "Title",
            text: "Body text"
        ).Run();

        Console.WriteLine($"Result = {result}");
    }
}
```

## Verifying Examples Work

### Manual Verification

1. Run each example
2. Verify the dialog appears with correct content
3. Interact with the dialog (buttons, input, selections)
4. Verify result prints correctly to console
5. Test Ctrl+C exits without stack trace

### TUI Driver Verification (Automated)

```javascript
// Example: Verify message-box
const session = await tui.launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Dialogs", "--", "message-box"]
});

await tui.waitForText(session, "Example dialog window", 5000);
await tui.waitForText(session, "Do you want to continue?", 5000);
await tui.pressKey(session, "Enter");
await tui.waitForIdle(session);
await tui.close(session);
```

## Troubleshooting

### Dialog doesn't appear

- Ensure terminal supports VT100 escape sequences
- Try running in a different terminal (e.g., iTerm2, Windows Terminal, Gnome Terminal)

### Colors look wrong

- Check terminal color scheme supports 256-color or true color
- The `styled-message-box` example uses bright green background

### ProgressDialog runs too long

- Example enumerates files in parent directories
- Progress is capped at 100%; dialog auto-closes after 1 second at 100%

### Terminal window too small

- Dialog APIs handle small terminals gracefully — content may clip but won't crash
- Resize terminal to at least 80x24 for best experience
- This is handled by the underlying Stroke dialog implementation, not the examples
