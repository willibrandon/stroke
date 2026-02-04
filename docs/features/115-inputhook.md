# Feature 115: Input Hook & GUI Integration

## Overview

Implement GUI framework integration for Stroke, enabling two scenarios:

1. **Terminal + GUI Hybrid** — A terminal app that also runs a GUI window (both responsive)
2. **Embedded Terminal** — A GUI app with an embedded Stroke prompt control

This is a documented deviation from Python Prompt Toolkit's file-descriptor-based approach. .NET's async/await, SynchronizationContext, and GUI framework dispatcher patterns require a different architecture.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/eventloop/inputhook.py`

Python PTK uses file descriptors and pipes for signaling between the selector and input hook. This approach doesn't translate well to .NET where:
- File descriptors aren't exposed the same way
- GUI frameworks have their own event loop/dispatcher patterns
- `async/await` with `SynchronizationContext` handles most marshaling

## Supported Frameworks

| Framework | Package | Platform | Notes |
|-----------|---------|----------|-------|
| Avalonia | `Stroke.Hooks.Avalonia` | Windows, macOS, Linux | Primary cross-platform target |
| WPF | `Stroke.Hooks.Wpf` | Windows | Enterprise/legacy Windows apps |
| WinForms | `Stroke.Hooks.WinForms` | Windows | Legacy Windows apps |
| WinUI 3 | `Stroke.Hooks.WinUI` | Windows | Modern Windows apps |
| MAUI | `Stroke.Hooks.Maui` | Windows, macOS, iOS, Android, Linux* | *Linux via Avalonia backend |

## Public API

### Core Interface (Stroke.EventLoop)

```csharp
namespace Stroke.EventLoop;

/// <summary>
/// Interface for input hooks that integrate with GUI framework event loops.
/// </summary>
/// <remarks>
/// Implementations yield to their GUI framework's event loop while waiting
/// for Stroke input, keeping the UI responsive.
/// </remarks>
public interface IInputHook
{
    /// <summary>
    /// Process GUI events until input is ready.
    /// </summary>
    /// <param name="inputReady">Triggered when Stroke needs control back.</param>
    /// <returns>Task that completes when the hook should stop.</returns>
    Task RunUntilInputReadyAsync(CancellationToken inputReady);
}
```

### Embedded Terminal Control Interface

```csharp
namespace Stroke.Embedding;

/// <summary>
/// Interface for GUI controls that host an embedded Stroke terminal.
/// </summary>
/// <remarks>
/// Implementations capture keyboard input, feed it to Stroke via PipeInput,
/// and render VT100 output to styled text in the GUI control.
/// </remarks>
public interface IStrokeTerminalControl
{
    /// <summary>
    /// The pipe input that receives keystrokes from the GUI control.
    /// </summary>
    IPipeInput Input { get; }

    /// <summary>
    /// The output that renders to the GUI control.
    /// </summary>
    IOutput Output { get; }

    /// <summary>
    /// Current terminal size in rows and columns.
    /// </summary>
    Size Size { get; }

    /// <summary>
    /// Event raised when the control is resized.
    /// </summary>
    event EventHandler<Size>? SizeChanged;

    /// <summary>
    /// Focus the terminal control for keyboard input.
    /// </summary>
    void Focus();

    /// <summary>
    /// Clear the terminal content.
    /// </summary>
    void Clear();
}
```

### VT100 to Rich Text Parser

```csharp
namespace Stroke.Embedding;

/// <summary>
/// Parses VT100 escape sequences into styled text segments for GUI rendering.
/// </summary>
public sealed class Vt100ToRichTextParser
{
    /// <summary>
    /// Parse VT100 output into styled segments.
    /// </summary>
    /// <param name="vt100Text">Raw text with VT100 escape sequences.</param>
    /// <returns>Sequence of styled text segments.</returns>
    public IEnumerable<StyledSegment> Parse(string vt100Text);
}

/// <summary>
/// A segment of text with associated style.
/// </summary>
/// <param name="Text">The text content.</param>
/// <param name="Style">The style attributes (colors, bold, etc.).</param>
public readonly record struct StyledSegment(string Text, Attrs Style);
```

## Project Structure

```
src/
├── Stroke/
│   ├── EventLoop/
│   │   └── IInputHook.cs
│   └── Embedding/
│       ├── IStrokeTerminalControl.cs
│       ├── Vt100ToRichTextParser.cs
│       ├── StyledSegment.cs
│       └── TerminalControlOutput.cs
│
├── Stroke.Hooks.Avalonia/
│   ├── AvaloniaInputHook.cs
│   └── AvaloniaTerminalControl.cs
│
├── Stroke.Hooks.Wpf/
│   ├── WpfInputHook.cs
│   └── WpfTerminalControl.cs
│
├── Stroke.Hooks.WinForms/
│   ├── WinFormsInputHook.cs
│   └── WinFormsTerminalControl.cs
│
├── Stroke.Hooks.WinUI/
│   ├── WinUIInputHook.cs
│   └── WinUITerminalControl.cs
│
└── Stroke.Hooks.Maui/
    ├── MauiInputHook.cs
    └── MauiTerminalControl.cs

tests/
├── Stroke.Tests/
│   └── Embedding/
│       ├── Vt100ToRichTextParserTests.cs
│       └── TerminalControlOutputTests.cs
│
└── Stroke.Hooks.*.Tests/
    └── (framework-specific integration tests)
```

## Scenario A: Terminal + GUI Hybrid

For apps that run in a terminal but also have a GUI window.

### WPF Input Hook

```csharp
namespace Stroke.Hooks.Wpf;

/// <summary>
/// Input hook that yields to the WPF Dispatcher while waiting for input.
/// </summary>
public sealed class WpfInputHook : IInputHook
{
    public static WpfInputHook Instance { get; } = new();

    public async Task RunUntilInputReadyAsync(CancellationToken inputReady)
    {
        while (!inputReady.IsCancellationRequested)
        {
            // Yield to WPF dispatcher to process UI events
            await Dispatcher.Yield(DispatcherPriority.Background);
        }
    }
}
```

### Avalonia Input Hook

```csharp
namespace Stroke.Hooks.Avalonia;

/// <summary>
/// Input hook that yields to the Avalonia dispatcher while waiting for input.
/// </summary>
public sealed class AvaloniaInputHook : IInputHook
{
    public static AvaloniaInputHook Instance { get; } = new();

    public async Task RunUntilInputReadyAsync(CancellationToken inputReady)
    {
        while (!inputReady.IsCancellationRequested)
        {
            // Yield to Avalonia dispatcher
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

            // Small delay to prevent tight loop
            await Task.Delay(10, inputReady).ConfigureAwait(false);
        }
    }
}
```

### Usage

```csharp
// Terminal app with WPF window
var session = new PromptSession<string>();

// Show a WPF window in background
var window = new MyWpfWindow();
window.Show();

// Prompt stays responsive, WPF window stays responsive
var result = await session.PromptAsync(
    message: ">>> ",
    inputHook: WpfInputHook.Instance
);
```

## Scenario B: Embedded Terminal in GUI

For GUI apps that want to embed a Stroke prompt control.

### WPF Terminal Control

```csharp
namespace Stroke.Hooks.Wpf;

/// <summary>
/// WPF control that hosts an embedded Stroke terminal.
/// </summary>
public class WpfTerminalControl : Control, IStrokeTerminalControl
{
    private readonly SimplePipeInput _pipeInput = new();
    private readonly TerminalControlOutput _output;
    private readonly RichTextBox _textBox;

    public IPipeInput Input => _pipeInput;
    public IOutput Output => _output;
    public Size Size => new(_textBox.ViewportHeight / LineHeight, _textBox.ViewportWidth / CharWidth);

    public event EventHandler<Size>? SizeChanged;

    public WpfTerminalControl()
    {
        _textBox = new RichTextBox { IsReadOnly = true };
        _output = new TerminalControlOutput(this);

        // Capture keyboard input
        PreviewKeyDown += OnPreviewKeyDown;
        PreviewTextInput += OnPreviewTextInput;

        // Track resize
        SizeChanged += (s, e) => OnSizeChanged();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Convert WPF key to Stroke key and feed to pipe input
        var strokeKey = KeyConverter.ToStrokeKey(e.Key, Keyboard.Modifiers);
        if (strokeKey != null)
        {
            _pipeInput.SendText(strokeKey.ToAnsiSequence());
            e.Handled = true;
        }
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Regular text input
        _pipeInput.SendText(e.Text);
        e.Handled = true;
    }

    /// <summary>
    /// Append styled text to the control.
    /// </summary>
    internal void AppendStyledText(IEnumerable<StyledSegment> segments)
    {
        foreach (var segment in segments)
        {
            var run = new Run(segment.Text)
            {
                Foreground = ToBrush(segment.Style.Color),
                Background = ToBrush(segment.Style.BgColor),
                FontWeight = segment.Style.Bold ? FontWeights.Bold : FontWeights.Normal,
                FontStyle = segment.Style.Italic ? FontStyles.Italic : FontStyles.Normal
            };
            // ... append to RichTextBox
        }
    }
}
```

### Usage

```xaml
<!-- In XAML -->
<stroke:WpfTerminalControl x:Name="terminal" />
```

```csharp
// In code-behind
var session = new PromptSession<string>(
    input: terminal.Input,
    output: terminal.Output
);

// Run prompt in the embedded terminal
var result = await session.PromptAsync(">>> ");
```

## Implementation Challenges

### 1. Keyboard Capture

GUI frameworks intercept certain keys (Tab, Enter, arrows) before controls receive them. Each framework has different mechanisms:

| Framework | Solution |
|-----------|----------|
| WPF | `PreviewKeyDown` (tunneling event) |
| WinForms | `KeyPreview = true` on form |
| Avalonia | `KeyDown` with handled routing |
| WinUI | `PreviewKeyDown` |

### 2. VT100 Parsing

The `Vt100ToRichTextParser` must handle:
- SGR codes (colors, bold, italic, underline)
- Cursor movement (relative positioning)
- Screen clearing (full, line, to-end)
- Character sets and special characters

### 3. Cursor Rendering

Options:
1. **Hide GUI cursor, draw Stroke's** — Custom caret rendering
2. **Sync cursors** — Update GUI cursor position to match Stroke's
3. **Overlay** — Transparent overlay with cursor drawing

### 4. Performance

Rapid redraws (completions, search) must not choke the GUI thread:
- Batch VT100 parsing
- Virtualize long output (only render visible portion)
- Throttle redraws with min interval

## Dependencies

- Feature 050: Event Loop Utilities (already implemented)
- Existing: `PipeInput`, `Vt100Output`, full prompt machinery

## Implementation Phases

### Phase 1: Core Infrastructure
1. Define `IInputHook` interface
2. Implement `Vt100ToRichTextParser`
3. Define `IStrokeTerminalControl` interface
4. Implement `TerminalControlOutput` (IOutput that routes to control)
5. Add `inputHook` parameter to `PromptSession`

### Phase 2: Avalonia (Cross-Platform)
1. Implement `AvaloniaInputHook`
2. Implement `AvaloniaTerminalControl`
3. Create `Stroke.Hooks.Avalonia` NuGet package
4. Integration tests

### Phase 3: WPF (Windows)
1. Implement `WpfInputHook`
2. Implement `WpfTerminalControl`
3. Create `Stroke.Hooks.Wpf` NuGet package
4. Integration tests

### Phase 4: WinForms (Windows Legacy)
1. Implement `WinFormsInputHook`
2. Implement `WinFormsTerminalControl`
3. Create `Stroke.Hooks.WinForms` NuGet package
4. Integration tests

### Phase 5: WinUI & MAUI
1. Implement `WinUIInputHook` and `WinUITerminalControl`
2. Implement `MauiInputHook` and `MauiTerminalControl`
3. Create NuGet packages
4. Integration tests

## Acceptance Criteria

### Core
- [ ] `IInputHook` interface defined with async pattern
- [ ] `Vt100ToRichTextParser` handles all SGR codes
- [ ] `IStrokeTerminalControl` interface supports embedding
- [ ] `PromptSession` accepts optional `inputHook` parameter
- [ ] Unit tests achieve 80% coverage on core components

### Per Framework
- [ ] Input hook keeps GUI responsive during prompt
- [ ] Terminal control captures all keyboard input
- [ ] VT100 output renders correctly as styled text
- [ ] Resize events propagate to Stroke
- [ ] Focus management works correctly
- [ ] Integration tests pass on target platform

### NuGet Packages
- [ ] Each framework has its own package with minimal dependencies
- [ ] Packages follow naming convention `Stroke.Hooks.<Framework>`
- [ ] README and samples included in packages

## Notes

This feature enables "Embed Stroke prompts in any .NET UI framework" — a unique capability in the .NET ecosystem. Nothing else currently provides this level of integration between a terminal prompt toolkit and GUI frameworks.
