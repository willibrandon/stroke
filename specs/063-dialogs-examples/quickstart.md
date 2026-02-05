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
dotnet run --project examples/Stroke.Examples.Dialogs -- <ExampleName>

# Examples:
dotnet run --project examples/Stroke.Examples.Dialogs -- MessageBox
dotnet run --project examples/Stroke.Examples.Dialogs -- YesNoDialog
dotnet run --project examples/Stroke.Examples.Dialogs -- ButtonDialog
dotnet run --project examples/Stroke.Examples.Dialogs -- InputDialog
dotnet run --project examples/Stroke.Examples.Dialogs -- PasswordDialog
dotnet run --project examples/Stroke.Examples.Dialogs -- RadioDialog
dotnet run --project examples/Stroke.Examples.Dialogs -- CheckboxDialog
dotnet run --project examples/Stroke.Examples.Dialogs -- ProgressDialog
dotnet run --project examples/Stroke.Examples.Dialogs -- StyledMessageBox
```

### Run Default Example

```bash
# Without arguments, runs MessageBox (default)
dotnet run --project examples/Stroke.Examples.Dialogs
```

## Example Descriptions

| Example | Description | Key API |
|---------|-------------|---------|
| `MessageBox` | Simple message dialog with OK button | `Dialogs.MessageDialog()` |
| `YesNoDialog` | Yes/No confirmation returning boolean | `Dialogs.YesNoDialog()` |
| `ButtonDialog` | Custom buttons with nullable values | `Dialogs.ButtonDialog<T>()` |
| `InputDialog` | Text input with prompt | `Dialogs.InputDialog()` |
| `PasswordDialog` | Masked password input | `Dialogs.InputDialog(password: true)` |
| `RadioDialog` | Single-selection list (plain + styled) | `Dialogs.RadioListDialog<T>()` |
| `CheckboxDialog` | Multi-selection with custom styling | `Dialogs.CheckboxListDialog<T>()` |
| `ProgressDialog` | Background task with progress bar | `Dialogs.ProgressDialog()` |
| `StyledMessageBox` | Custom colors via Style.FromDict() | `Style.FromDict()` + HTML title |

## Keyboard Navigation

All dialog examples support:
- **Tab** / **Shift+Tab**: Move focus between buttons/fields
- **Enter**: Activate focused button
- **Space**: Toggle checkbox selection (CheckboxDialog only)
- **Arrow keys**: Navigate lists (RadioDialog, CheckboxDialog)
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
// Example: Verify MessageBox
const session = await tui.launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Dialogs", "--", "MessageBox"]
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
- The `StyledMessageBox` example uses bright green background

### ProgressDialog runs too long

- Example enumerates files in parent directories
- Progress is capped at 100%; dialog auto-closes after 1 second at 100%

### Terminal window too small

- Dialog APIs handle small terminals gracefully — content may clip but won't crash
- Resize terminal to at least 80x24 for best experience
- This is handled by the underlying Stroke dialog implementation, not the examples
