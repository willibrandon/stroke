# Contract: Prompt

**Namespace**: `Stroke.Shortcuts`
**File**: `src/Stroke/Shortcuts/Prompt.cs`
**Python Source**: `prompt_toolkit.shortcuts.prompt.prompt`, `confirm`, `create_confirm_session`

## Static Class Declaration

```csharp
/// <summary>
/// Provides static convenience functions for common prompt operations.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's module-level <c>prompt</c>, <c>confirm</c>,
/// and <c>create_confirm_session</c> functions from <c>prompt_toolkit.shortcuts.prompt</c>.
/// </remarks>
public static class Prompt
```

## Methods

### Prompt (Blocking)

```csharp
/// <summary>
/// Display a prompt and return the user's input. Creates a new <see cref="PromptSession{TResult}"/>
/// for each call.
/// </summary>
public static string Prompt(
    AnyFormattedText? message = null,
    IHistory? history = null,
    EditingMode? editingMode = null,
    double? refreshInterval = null,
    bool? viMode = null,
    ILexer? lexer = null,
    ICompleter? completer = null,
    bool? completeInThread = null,
    bool? isPassword = null,
    IKeyBindingsBase? keyBindings = null,
    AnyFormattedText? bottomToolbar = null,
    IStyle? style = null,
    ColorDepth? colorDepth = null,
    ICursorShapeConfig? cursor = null,
    FilterOrBool? includeDefaultPygmentsStyle = null,
    IStyleTransformation? styleTransformation = null,
    FilterOrBool? swapLightAndDarkColors = null,
    AnyFormattedText? rprompt = null,
    FilterOrBool? multiline = null,
    object? promptContinuation = null,
    FilterOrBool? wrapLines = null,
    FilterOrBool? enableHistorySearch = null,
    FilterOrBool? searchIgnoreCase = null,
    FilterOrBool? completeWhileTyping = null,
    FilterOrBool? validateWhileTyping = null,
    CompleteStyle? completeStyle = null,
    IAutoSuggest? autoSuggest = null,
    IValidator? validator = null,
    IClipboard? clipboard = null,
    FilterOrBool? mouseSupport = null,
    IReadOnlyList<IProcessor>? inputProcessors = null,
    AnyFormattedText? placeholder = null,
    int? reserveSpaceForMenu = null,
    FilterOrBool? enableSystemPrompt = null,
    FilterOrBool? enableSuspend = null,
    FilterOrBool? enableOpenInEditor = null,
    object? tempfileSuffix = null,
    object? tempfile = null,
    FilterOrBool? showFrame = null,
    string default_ = "",
    bool acceptDefault = false,
    Action? preRun = null,
    bool setExceptionHandler = true,
    bool handleSigint = true,
    bool inThread = false,
    InputHook? inputHook = null)
```

**Behavior**: Creates `PromptSession<string>(history: history)`, then calls `session.Prompt(message, ...)` passing all other parameters. History is the only parameter passed to the session constructor (not to `Prompt()`), because history cannot be changed per-prompt.

### PromptAsync

```csharp
/// <summary>
/// Display a prompt and return the user's input asynchronously.
/// </summary>
public static Task<string> PromptAsync(
    AnyFormattedText? message = null,
    // ... same session-level parameters as Prompt ...
    string default_ = "",
    bool acceptDefault = false,
    Action? preRun = null,
    bool setExceptionHandler = true,
    bool handleSigint = true)
```

**Behavior**: Same as `Prompt` but calls `session.PromptAsync(...)`.

### CreateConfirmSession

```csharp
/// <summary>
/// Create a <see cref="PromptSession{TResult}"/> configured for yes/no confirmation.
/// </summary>
public static PromptSession<bool> CreateConfirmSession(
    AnyFormattedText message,
    string suffix = " (y/n) ")
```

**Behavior**:
1. Creates `KeyBindings` with 4 bindings:
   - `"y"` and `"Y"` → set buffer text to "y", exit with `true`
   - `"n"` and `"N"` → set buffer text to "n", exit with `false`
   - `Keys.Any` → no-op (ignore all other input)
2. Merges `message` and `suffix` into `completeMessage` via `FormattedTextUtils.Merge`
3. Creates and returns `PromptSession<bool>(completeMessage, keyBindings: bindings)`

### Confirm (Blocking)

```csharp
/// <summary>
/// Display a confirmation prompt that returns true (y/Y) or false (n/N).
/// </summary>
public static bool Confirm(
    AnyFormattedText message = default,    // default: "Confirm?"
    string suffix = " (y/n) ")
```

**Behavior**: Calls `CreateConfirmSession(message, suffix).Prompt()`.

### ConfirmAsync

```csharp
/// <summary>
/// Display a confirmation prompt asynchronously.
/// </summary>
public static Task<bool> ConfirmAsync(
    AnyFormattedText message = default,    // default: "Confirm?"
    string suffix = " (y/n) ")
```

**Behavior**: Calls `CreateConfirmSession(message, suffix).PromptAsync()`.

## Python → C# Naming

| Python | C# |
|--------|-----|
| `prompt(message, ...)` | `Prompt.Prompt(message, ...)` |
| N/A (Python has no async standalone) | `Prompt.PromptAsync(message, ...)` |
| `create_confirm_session(message, suffix)` | `Prompt.CreateConfirmSession(message, suffix)` |
| `confirm(message, suffix)` | `Prompt.Confirm(message, suffix)` |
| N/A | `Prompt.ConfirmAsync(message, suffix)` |

## Notes

- Python's `prompt` is a module-level function. C# wraps it in a static class `Prompt` since C# doesn't support free-standing functions.
- `ConfirmAsync` is a C# addition (not in Python) to provide async parity. This is a justified deviation per Constitution I — C# async patterns expect async alternatives.
- The `message` parameter for `Confirm` defaults to `"Confirm?"` matching Python. Since `AnyFormattedText` can be constructed from string, this works via implicit conversion.
