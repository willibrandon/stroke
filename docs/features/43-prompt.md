# Feature 43: Prompt Session

## Overview

Implement the high-level PromptSession class and prompt function that provides a GNU Readline-like interface for terminal input. This is the primary entry point for most users of the library.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/shortcuts/prompt.py`

## Public API

### CompleteStyle Enum

```csharp
namespace Stroke.Shortcuts;

/// <summary>
/// How to display autocompletions for the prompt.
/// </summary>
public enum CompleteStyle
{
    /// <summary>
    /// Single column dropdown menu.
    /// </summary>
    Column,

    /// <summary>
    /// Multi-column dropdown menu.
    /// </summary>
    MultiColumn,

    /// <summary>
    /// Readline-style: show completions below input.
    /// </summary>
    ReadlineLike
}
```

### PromptSession Class

```csharp
namespace Stroke.Shortcuts;

/// <summary>
/// PromptSession for a prompt application, which can be used as a GNU Readline
/// replacement. This is a wrapper around prompt_toolkit functionality.
/// </summary>
public sealed class PromptSession<T>
{
    /// <summary>
    /// Creates a PromptSession.
    /// </summary>
    public PromptSession(
        AnyFormattedText message = default,
        bool multiline = false,
        bool wrapLines = true,
        bool isPassword = false,
        bool viMode = false,
        EditingMode editingMode = EditingMode.Emacs,
        bool completeWhileTyping = true,
        bool validateWhileTyping = true,
        bool enableHistorySearch = false,
        bool searchIgnoreCase = false,
        Lexer? lexer = null,
        bool enableSystemPrompt = false,
        bool enableSuspend = false,
        bool enableOpenInEditor = false,
        Validator? validator = null,
        Completer? completer = null,
        bool completeInThread = false,
        int reserveSpaceForMenu = 8,
        CompleteStyle completeStyle = CompleteStyle.Column,
        AutoSuggest? autoSuggest = null,
        BaseStyle? style = null,
        StyleTransformation? styleTransformation = null,
        bool swapLightAndDarkColors = false,
        ColorDepth? colorDepth = null,
        CursorShapeConfig? cursor = null,
        bool includeDefaultPygmentsStyle = true,
        History? history = null,
        Clipboard? clipboard = null,
        PromptContinuationText? promptContinuation = null,
        AnyFormattedText rprompt = default,
        AnyFormattedText bottomToolbar = default,
        bool mouseSupport = false,
        IList<Processor>? inputProcessors = null,
        AnyFormattedText? placeholder = null,
        KeyBindingsBase? keyBindings = null,
        bool eraseWhenDone = false,
        string? tempfileSuffix = ".txt",
        string? tempfile = null,
        float refreshInterval = 0,
        bool showFrame = false,
        Input? input = null,
        Output? output = null,
        Type interruptException = typeof(KeyboardInterruptException),
        Type eofException = typeof(EOFException));

    /// <summary>
    /// The prompt message.
    /// </summary>
    public AnyFormattedText Message { get; set; }

    /// <summary>
    /// The editing mode (Emacs or Vi).
    /// </summary>
    public EditingMode EditingMode { get; set; }

    /// <summary>
    /// The history instance.
    /// </summary>
    public History History { get; }

    /// <summary>
    /// The default buffer.
    /// </summary>
    public Buffer DefaultBuffer { get; }

    /// <summary>
    /// The search buffer.
    /// </summary>
    public Buffer SearchBuffer { get; }

    /// <summary>
    /// The layout.
    /// </summary>
    public Layout Layout { get; }

    /// <summary>
    /// The application.
    /// </summary>
    public Application<T> App { get; }

    /// <summary>
    /// The input interface.
    /// </summary>
    public Input Input { get; }

    /// <summary>
    /// The output interface.
    /// </summary>
    public Output Output { get; }

    /// <summary>
    /// Display the prompt and wait for input.
    /// </summary>
    /// <param name="message">Override the session message.</param>
    /// <param name="default">Default input text.</param>
    /// <param name="acceptDefault">Accept default without user edit.</param>
    /// <param name="preRun">Callable run before starting.</param>
    /// <param name="inThread">Run in background thread.</param>
    /// <returns>The input text.</returns>
    public T Prompt(
        AnyFormattedText? message = null,
        string @default = "",
        bool acceptDefault = false,
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true,
        bool inThread = false,
        InputHook? inputhook = null);

    /// <summary>
    /// Display the prompt asynchronously.
    /// </summary>
    public Task<T> PromptAsync(
        AnyFormattedText? message = null,
        string @default = "",
        bool acceptDefault = false,
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true);
}
```

### prompt Function

```csharp
namespace Stroke.Shortcuts;

/// <summary>
/// Prompt shortcut functions.
/// </summary>
public static class PromptFunctions
{
    /// <summary>
    /// Display a prompt and return the input.
    /// Creates a new PromptSession for each call.
    /// </summary>
    public static string Prompt(
        AnyFormattedText? message = null,
        History? history = null,
        EditingMode? editingMode = null,
        bool? viMode = null,
        Lexer? lexer = null,
        Completer? completer = null,
        bool? completeInThread = null,
        bool? isPassword = null,
        KeyBindingsBase? keyBindings = null,
        AnyFormattedText? bottomToolbar = null,
        BaseStyle? style = null,
        ColorDepth? colorDepth = null,
        CursorShapeConfig? cursor = null,
        bool? includeDefaultPygmentsStyle = null,
        StyleTransformation? styleTransformation = null,
        bool? swapLightAndDarkColors = null,
        AnyFormattedText? rprompt = null,
        bool? multiline = null,
        PromptContinuationText? promptContinuation = null,
        bool? wrapLines = null,
        bool? enableHistorySearch = null,
        bool? searchIgnoreCase = null,
        bool? completeWhileTyping = null,
        bool? validateWhileTyping = null,
        CompleteStyle? completeStyle = null,
        AutoSuggest? autoSuggest = null,
        Validator? validator = null,
        Clipboard? clipboard = null,
        bool? mouseSupport = null,
        IList<Processor>? inputProcessors = null,
        AnyFormattedText? placeholder = null,
        int? reserveSpaceForMenu = null,
        bool? enableSystemPrompt = null,
        bool? enableSuspend = null,
        bool? enableOpenInEditor = null,
        string? tempfileSuffix = null,
        string? tempfile = null,
        bool? showFrame = null,
        string @default = "",
        bool acceptDefault = false,
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true,
        bool inThread = false,
        InputHook? inputhook = null);

    /// <summary>
    /// Display a yes/no confirmation prompt.
    /// </summary>
    public static bool Confirm(
        AnyFormattedText message = default,
        string suffix = " (y/n) ");

    /// <summary>
    /// Create a PromptSession for confirmation.
    /// </summary>
    public static PromptSession<bool> CreateConfirmSession(
        AnyFormattedText message,
        string suffix = " (y/n) ");
}
```

## Project Structure

```
src/Stroke/
└── Shortcuts/
    ├── CompleteStyle.cs
    ├── PromptSession.cs
    └── PromptFunctions.cs
tests/Stroke.Tests/
└── Shortcuts/
    ├── PromptSessionTests.cs
    └── ConfirmTests.cs
```

## Implementation Notes

### PromptSession Architecture

PromptSession wraps:
1. **Buffer**: The default input buffer
2. **Layout**: The UI layout with prompt, input, toolbars
3. **Application**: The event loop and rendering
4. **History**: Command history storage
5. **KeyBindings**: Prompt-specific bindings

### Layout Structure

```
┌────────────────────────────────────────────┐
│ [Prompt lines above input]                 │
├────────────────────────────────────────────┤
│ prompt> user input here                    │
│         └─ completion menu (float)         │
├────────────────────────────────────────────┤
│ [Validation toolbar]                       │
│ [System toolbar]                           │
│ [Search toolbar]                           │
│ [Bottom toolbar]                           │
└────────────────────────────────────────────┘
```

### Dynamic Properties

Many properties can be set per-prompt() call:
- Setting `None` keeps the session default
- Setting a value updates both current and future prompts
- To clear a completer/validator, use DummyCompleter/DummyValidator

### Prompt Key Bindings

- **Enter**: Accept input (single-line mode)
- **Ctrl-C**: Raise interrupt exception
- **Ctrl-D**: Raise EOF exception (on empty input)
- **Ctrl-Z**: Suspend to background (if enabled)
- **Tab**: Complete (readline-like mode)

### Multiline Prompt Support

```csharp
// Prompt with lines above input
var session = new PromptSession<string>(
    message: "Line 1\nLine 2\n> "
);
```

The prompt is split:
- Lines before the last `\n` shown above
- Last line shown as the inline prompt

### Prompt Continuation

For multiline input, continuation text shown for wrapped lines:

```csharp
promptContinuation: (width, lineNumber, wrapCount) =>
    new string(' ', width)  // Indent by prompt width
```

### Dumb Terminal Support

When `TERM=dumb`:
- Minimal rendering (no cursor movement)
- Print prompt, read input character by character
- Print each typed character
- No completion menus

### Threading

- `inThread: true`: Run prompt in background thread
- `completeInThread: true`: Run completer in background thread
- Prevents blocking event loop for expensive operations

## Dependencies

- `Stroke.Application` (Feature 31) - Application class
- `Stroke.Layout` (Feature 29) - Layout system
- `Stroke.Layout.Menus` (Feature 32) - Completion menus
- `Stroke.Buffer` (Feature 06) - Buffer class
- `Stroke.History` (Feature 33) - History
- `Stroke.Completion` (Feature 36-37) - Completers
- `Stroke.Validation` (Feature 35) - Validators
- `Stroke.KeyBinding` (Feature 19-21) - Key bindings
- `Stroke.Styles` (Feature 14) - Styles

## Implementation Tasks

1. Implement `CompleteStyle` enum
2. Implement `PromptSession<T>` class structure
3. Create default buffer with accept handler
4. Create search buffer
5. Create layout with all containers
6. Create application with key bindings
7. Implement `Prompt()` method
8. Implement `PromptAsync()` method
9. Implement dumb terminal fallback
10. Implement `prompt()` function
11. Implement `Confirm()` function
12. Write comprehensive unit tests

## Acceptance Criteria

- [ ] PromptSession creates working prompt application
- [ ] All parameters properly configure the session
- [ ] Prompt() displays and returns input
- [ ] PromptAsync() works asynchronously
- [ ] Completion menus display correctly
- [ ] History navigation works
- [ ] Key bindings work (Enter, Ctrl-C, Ctrl-D)
- [ ] Multiline prompts display correctly
- [ ] Dumb terminal fallback works
- [ ] confirm() returns boolean correctly
- [ ] Unit tests achieve 80% coverage
