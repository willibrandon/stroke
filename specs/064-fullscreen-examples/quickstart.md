# Quickstart: Full-Screen Examples

**Feature**: 064-fullscreen-examples
**Date**: 2026-02-05

## Building the Examples

```bash
# From repository root
cd examples
dotnet build Stroke.Examples.FullScreen
```

## Running Examples

### Command-Line Interface

```bash
# Run a specific example
dotnet run --project Stroke.Examples.FullScreen -- <example-name>

# Examples:
dotnet run --project Stroke.Examples.FullScreen -- HelloWorld
dotnet run --project Stroke.Examples.FullScreen -- Buttons
dotnet run --project Stroke.Examples.FullScreen -- Calculator
dotnet run --project Stroke.Examples.FullScreen -- SplitScreen
dotnet run --project Stroke.Examples.FullScreen -- Pager
dotnet run --project Stroke.Examples.FullScreen -- FullScreenDemo
dotnet run --project Stroke.Examples.FullScreen -- TextEditor

# SimpleDemos (use directory prefix or just name)
dotnet run --project Stroke.Examples.FullScreen -- HorizontalSplit
dotnet run --project Stroke.Examples.FullScreen -- Floats
dotnet run --project Stroke.Examples.FullScreen -- Focus

# ScrollablePanes
dotnet run --project Stroke.Examples.FullScreen -- SimpleExample
dotnet run --project Stroke.Examples.FullScreen -- WithCompletionMenu

# List all available examples
dotnet run --project Stroke.Examples.FullScreen -- --help
```

### Example Names (case-insensitive)

**Main Examples:**
- `HelloWorld` - Basic framed text area
- `DummyApp` - Minimal application
- `NoLayout` - Application without layout
- `Buttons` - Button widgets with click handlers
- `Calculator` - REPL-style calculator
- `SplitScreen` - Reactive split-screen editor
- `Pager` - File viewer with syntax highlighting
- `FullScreenDemo` - Widget showcase
- `TextEditor` - Full text editor with menus
- `AnsiArtAndTextArea` - ANSI art display

**ScrollablePanes:**
- `SimpleExample` - Basic scrollable pane
- `WithCompletionMenu` - Scrollable with completion

**SimpleDemos:**
- `HorizontalSplit` - HSplit container demo
- `VerticalSplit` - VSplit container demo
- `Alignment` - Window alignment options
- `HorizontalAlign` - HSplit horizontal alignment
- `VerticalAlign` - HSplit vertical alignment
- `Floats` - Floating windows
- `FloatTransparency` - Transparent floats
- `Focus` - Programmatic focus control
- `Margins` - Line numbers and scrollbar
- `LinePrefixes` - Custom line prefixes
- `ColorColumn` - Column highlighting
- `CursorHighlight` - Cursor line/column highlighting
- `AutoCompletion` - Completion menu demo

## Example Interaction Guide

### HelloWorld
- **Expected**: Framed text area with "Hello world!\nPress control-c to quit."
- **Exit**: Press Ctrl+C

### Buttons
- **Navigation**: Tab/Shift+Tab between buttons
- **Action**: Enter or click to activate button
- **Expected**: Text area shows which button was clicked
- **Exit**: Click Exit button or press Ctrl+C

### Calculator
- **Input**: Type math expressions (e.g., `4 + 4`, `10 * 5`)
- **Execute**: Press Enter
- **Expected**: "In:" and "Out:" lines appear in output area
- **Search**: Ctrl+R for reverse search in history
- **Exit**: Ctrl+C or Ctrl+Q

### SplitScreen
- **Input**: Type in left pane
- **Expected**: Reversed text appears in right pane immediately
- **Exit**: Ctrl+Q or Ctrl+C

### Pager
- **Navigation**: Arrow keys, Page Up/Down
- **Search**: Press `/` to search, then type search term
- **Expected**: Line numbers, syntax highlighting, scrollbar
- **Exit**: Press `q` or Ctrl+C

### FullScreenDemo
- **Menus**: Click menu items or use Alt+key
- **Widgets**: Tab between widgets, interact with each
- **Exit**: File → Exit or click No button

### TextEditor
- **Menus**: File (New/Open/Save), Edit (Find), etc.
- **Editing**: Standard text editing with Vi/Emacs keys
- **Exit**: File → Exit (prompts to save if unsaved changes)

## TUI Driver Verification

### Basic Verification Script (HelloWorld)

```javascript
// Launch the example
const session = await tui.launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.FullScreen", "--", "HelloWorld"],
  cols: 80,
  rows: 24
});

// Wait for the UI to render
await tui.waitForText(session, "Hello world!", { timeout: 10000 });

// Verify frame border appears
const text = await tui.text(session);
assert(text.includes("┌") && text.includes("┐"));  // Frame corners
assert(text.includes("Hello world!"));
assert(text.includes("Press control-c to quit."));

// Exit gracefully
await tui.pressKey(session, "Ctrl+c");
await tui.waitForIdle(session);

// Verify clean exit
await tui.close(session);
```

### Calculator Verification Script

```javascript
const session = await tui.launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.FullScreen", "--", "Calculator"],
  cols: 80,
  rows: 24
});

// Wait for prompt
await tui.waitForText(session, ">>>", { timeout: 10000 });

// Enter expression
await tui.sendText(session, "4 + 4");
await tui.pressKey(session, "Enter");
await tui.waitForIdle(session);

// Verify output
const text = await tui.text(session);
assert(text.includes("In:  4 + 4"));
assert(text.includes("Out: 8"));

// Test error handling
await tui.sendText(session, "1/0");
await tui.pressKey(session, "Enter");
await tui.waitForIdle(session);

// Should show error, not crash
const errorText = await tui.text(session);
assert(errorText.includes("error") || errorText.includes("divide"));

// Exit
await tui.pressKey(session, "Ctrl+c");
await tui.close(session);
```

### SplitScreen Verification Script

```javascript
const session = await tui.launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.FullScreen", "--", "SplitScreen"],
  cols: 80,
  rows: 24
});

// Wait for UI
await tui.waitForText(session, "Hello world", { timeout: 10000 });

// Type in left pane
await tui.sendText(session, "abc");
await tui.waitForIdle(session);

// Verify reversed text appears in right pane
const text = await tui.text(session);
assert(text.includes("cba"));

// Exit
await tui.pressKey(session, "Ctrl+q");
await tui.close(session);
```

### Buttons Verification Script

```javascript
const session = await tui.launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.FullScreen", "--", "Buttons"],
  cols: 80,
  rows: 24
});

// Wait for UI
await tui.waitForText(session, "Button 1", { timeout: 10000 });

// Press Enter on first button (should be focused)
await tui.pressKey(session, "Enter");
await tui.waitForIdle(session);

// Verify text area updated
let text = await tui.text(session);
assert(text.includes("Button 1 clicked"));

// Tab to next button and click
await tui.pressKey(session, "Tab");
await tui.pressKey(session, "Enter");
await tui.waitForIdle(session);

text = await tui.text(session);
assert(text.includes("Button 2 clicked"));

// Tab to Exit and click
await tui.pressKey(session, "Tab");
await tui.pressKey(session, "Tab");
await tui.pressKey(session, "Tab");  // Skip Button 3
await tui.pressKey(session, "Enter");

await tui.close(session);
```

## Troubleshooting

### "Unknown example" Error
- Check spelling (names are case-insensitive)
- Run `--help` to see available examples
- Ensure you're in the correct directory

### Terminal Rendering Issues
- Ensure terminal supports ANSI/VT100 escape sequences
- Try resizing terminal to at least 80x24
- On Windows, ensure Windows Terminal or ConEmu (not legacy cmd.exe)

### Mouse Not Working
- SplitScreen and FullScreenDemo support mouse by default
- Ensure terminal has mouse support enabled
- May need to disable other mouse-grabbing applications

### Ctrl+C Not Exiting
- Some examples use Ctrl+Q as alternative
- For Pager, press `q` to quit
- Force quit with Ctrl+Z followed by `kill %1` if needed
