# Feature 64: Open in Editor Bindings

## Overview

Implement the key bindings for opening the current buffer content in an external editor, typically used for editing complex multi-line input.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/open_in_editor.py`

## Public API

### OpenInEditorBindings Class

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class OpenInEditorBindings
{
    /// <summary>
    /// Load both Vi and Emacs key bindings for edit-and-execute-command.
    /// </summary>
    public static KeyBindingsBase LoadOpenInEditorBindings();

    /// <summary>
    /// Load Emacs binding for opening buffer in external editor.
    /// Pressing Ctrl-X Ctrl-E opens the buffer in $EDITOR.
    /// </summary>
    public static KeyBindings LoadEmacsOpenInEditorBindings();

    /// <summary>
    /// Load Vi binding for opening buffer in external editor.
    /// Pressing 'v' in navigation mode opens the buffer in $EDITOR.
    /// </summary>
    public static KeyBindings LoadViOpenInEditorBindings();
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Bindings/
        └── OpenInEditorBindings.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        └── OpenInEditorBindingsTests.cs
```

## Implementation Notes

### Emacs Binding

```csharp
public static KeyBindings LoadEmacsOpenInEditorBindings()
{
    var bindings = new KeyBindings();

    // Ctrl-X Ctrl-E opens editor (Emacs mode, no selection)
    bindings.Add("c-x", "c-e",
        NamedCommands.GetByName("edit-and-execute-command"),
        filter: Filters.EmacsMode & ~Filters.HasSelection);

    return bindings;
}
```

### Vi Binding

```csharp
public static KeyBindings LoadViOpenInEditorBindings()
{
    var bindings = new KeyBindings();

    // 'v' in navigation mode opens editor
    bindings.Add("v",
        NamedCommands.GetByName("edit-and-execute-command"),
        filter: Filters.ViNavigationMode);

    return bindings;
}
```

### Combined Bindings

```csharp
public static KeyBindingsBase LoadOpenInEditorBindings()
{
    return KeyBindings.Merge(
        LoadEmacsOpenInEditorBindings(),
        LoadViOpenInEditorBindings());
}
```

### edit-and-execute-command Named Command

The actual implementation is in NamedCommands:

```csharp
// In NamedCommands static constructor
Register("edit-and-execute-command", async e =>
{
    var buffer = e.CurrentBuffer;
    var app = e.App;

    // Get editor from environment
    var editor = Environment.GetEnvironmentVariable("EDITOR")
        ?? Environment.GetEnvironmentVariable("VISUAL")
        ?? "vi";

    // Create temporary file with buffer content
    var tempFile = Path.GetTempFileName();
    try
    {
        await File.WriteAllTextAsync(tempFile, buffer.Text);

        // Run editor in terminal
        await app.RunInTerminalAsync(async () =>
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = editor,
                Arguments = tempFile,
                UseShellExecute = false
            });
            await process!.WaitForExitAsync();
        });

        // Read edited content back
        var newText = await File.ReadAllTextAsync(tempFile);

        // Update buffer
        buffer.Document = new Document(newText, newText.Length);

        // Optionally accept the input
        // buffer.Validate() && app.SetResult(buffer.Document.Text);
    }
    finally
    {
        try { File.Delete(tempFile); } catch { }
    }
});
```

### Environment Variable Lookup

```csharp
private static string GetEditorCommand()
{
    // Check EDITOR first, then VISUAL, fall back to vi
    return Environment.GetEnvironmentVariable("EDITOR")
        ?? Environment.GetEnvironmentVariable("VISUAL")
        ?? (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "notepad" : "vi");
}
```

### Running Editor in Terminal

The editor must run in the terminal with the prompt toolkit UI temporarily suspended:

```csharp
await app.RunInTerminalAsync(async () =>
{
    // UI is suspended, terminal is in normal mode

    var startInfo = new ProcessStartInfo
    {
        FileName = editor,
        Arguments = $"\"{tempFile}\"",
        UseShellExecute = false,
        RedirectStandardInput = false,
        RedirectStandardOutput = false,
        RedirectStandardError = false
    };

    using var process = Process.Start(startInfo);
    if (process != null)
    {
        await process.WaitForExitAsync();
    }
});

// UI is resumed, read the edited content
```

## Dependencies

- `Stroke.KeyBinding.KeyBindings` (Feature 19) - KeyBindings class
- `Stroke.KeyBinding.Bindings.NamedCommands` (Feature 56) - Named commands
- `Stroke.Filters` (Feature 12) - Filter conditions
- `Stroke.Application` (Feature 37) - RunInTerminal support

## Implementation Tasks

1. Implement `LoadEmacsOpenInEditorBindings`
2. Implement `LoadViOpenInEditorBindings`
3. Implement `LoadOpenInEditorBindings`
4. Implement `edit-and-execute-command` named command
5. Implement editor environment variable lookup
6. Implement temporary file creation/cleanup
7. Implement RunInTerminal integration
8. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Ctrl-X Ctrl-E opens editor in Emacs mode
- [ ] 'v' opens editor in Vi navigation mode
- [ ] Bindings disabled when selection is active (Emacs)
- [ ] $EDITOR environment variable is respected
- [ ] $VISUAL environment variable is fallback
- [ ] Default editor is vi (Unix) or notepad (Windows)
- [ ] Temporary file is created with buffer content
- [ ] Edited content is read back into buffer
- [ ] Temporary file is cleaned up
- [ ] UI is properly suspended during editing
- [ ] Unit tests achieve 80% coverage
