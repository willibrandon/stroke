# Quickstart: System Clipboard

**Feature**: 066-system-clipboard
**Date**: 2026-02-07

## Enable System Clipboard in a Stroke Application

### Basic Usage

```csharp
using Stroke.Clipboard;
using Stroke.Shortcuts;

// Create a SystemClipboard (auto-detects platform)
var clipboard = new SystemClipboard();

// Pass it to a prompt session
var session = new PromptSession<string>(clipboard: clipboard);
var result = session.Prompt(">>> ");
```

That's it. Users can now:
- **Paste** text from other apps using Ctrl-Y (Emacs yank)
- **Cut** text using Ctrl-W (Emacs kill-word) and paste in other apps

### Without System Clipboard (default)

```csharp
// Default behavior — in-memory clipboard only (no change from before)
var session = new PromptSession<string>();
```

Applications that don't opt in experience zero behavior change.

### Direct Clipboard Operations

```csharp
var clipboard = new SystemClipboard();

// Write to OS clipboard
clipboard.SetText("Hello from Stroke!");

// Read from OS clipboard
var data = clipboard.GetData();
Console.WriteLine(data.Text); // Whatever is on the OS clipboard

// Write with selection type
clipboard.SetData(new ClipboardData("line1\nline2", SelectionType.Lines));

// Read preserves selection type when text matches
var readBack = clipboard.GetData();
// readBack.Type == SelectionType.Lines (if clipboard wasn't changed externally)
```

### Error Handling

```csharp
try
{
    var clipboard = new SystemClipboard();
}
catch (ClipboardProviderNotAvailableException ex)
{
    // On Linux: "No clipboard tool found. Install one of: xclip, xsel, wl-clipboard"
    // On WSL: "clip.exe or powershell.exe not accessible in WSL environment"
    Console.WriteLine(ex.Message);
}
```

Construction throws if no clipboard mechanism is available. Once constructed, all operations are best-effort (failures silently return empty data).

## Platform Requirements

| Platform | Mechanism | Pre-installed? |
|----------|-----------|----------------|
| Windows | Win32 API | Yes |
| macOS | pbcopy/pbpaste | Yes |
| Linux (Wayland) | wl-copy/wl-paste | Install `wl-clipboard` |
| Linux (X11) | xclip or xsel | Install `xclip` or `xsel` |
| WSL | clip.exe/powershell.exe | Yes (via Windows interop) |

## Build & Test

```bash
# Build
dotnet build src/Stroke/Stroke.csproj

# Run all clipboard tests
dotnet test tests/Stroke.Tests/ --filter "FullyQualifiedName~Clipboard"

# Run only SystemClipboard tests
dotnet test tests/Stroke.Tests/ --filter "FullyQualifiedName~SystemClipboard"
```
