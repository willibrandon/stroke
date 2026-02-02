# Contract: PromptSession\<TResult\>

**Namespace**: `Stroke.Shortcuts`
**Files**: `src/Stroke/Shortcuts/PromptSession.cs` (+ 5 partial class files)
**Python Source**: `prompt_toolkit.shortcuts.prompt.PromptSession`

## Class Declaration

```csharp
/// <summary>
/// PromptSession for a prompt application, which can be used as a GNU Readline replacement.
/// </summary>
/// <typeparam name="TResult">The type of result returned when the prompt completes.</typeparam>
/// <remarks>
/// <para>This is a wrapper around Buffer, Layout, Application, History, and KeyBindings
/// that provides a cohesive prompt experience. Create a session once and call
/// <see cref="Prompt"/> or <see cref="PromptAsync"/> repeatedly for a REPL-like experience
/// with persistent history.</para>
/// <para><b>Thread safety:</b> Session properties are Lock-protected for safe reads/writes
/// from multiple threads. However, only one <see cref="Prompt"/> or <see cref="PromptAsync"/>
/// call should be active at a time per session instance.</para>
/// </remarks>
public partial class PromptSession<TResult>
```

## Constructor

```csharp
public PromptSession(
    AnyFormattedText message = default,           // Prompt text (default: "")
    FilterOrBool multiline = default,             // default: false
    FilterOrBool wrapLines = default,             // default: true (special handling)
    FilterOrBool isPassword = default,            // default: false
    bool viMode = false,                          // Convenience: sets EditingMode.Vi
    EditingMode editingMode = EditingMode.Emacs,
    FilterOrBool completeWhileTyping = default,   // default: true (special handling)
    FilterOrBool validateWhileTyping = default,   // default: true (special handling)
    FilterOrBool enableHistorySearch = default,    // default: false
    FilterOrBool searchIgnoreCase = default,       // default: false
    ILexer? lexer = null,
    FilterOrBool enableSystemPrompt = default,     // default: false
    FilterOrBool enableSuspend = default,          // default: false
    FilterOrBool enableOpenInEditor = default,     // default: false
    IValidator? validator = null,
    ICompleter? completer = null,
    bool completeInThread = false,
    int reserveSpaceForMenu = 8,
    CompleteStyle completeStyle = CompleteStyle.Column,
    IAutoSuggest? autoSuggest = null,
    IStyle? style = null,
    IStyleTransformation? styleTransformation = null,
    FilterOrBool swapLightAndDarkColors = default, // default: false
    ColorDepth? colorDepth = null,
    ICursorShapeConfig? cursor = null,
    FilterOrBool includeDefaultPygmentsStyle = default, // default: true (special handling)
    IHistory? history = null,                      // default: new InMemoryHistory()
    IClipboard? clipboard = null,                  // default: new InMemoryClipboard()
    object? promptContinuation = null,             // string | PromptContinuationCallable | AnyFormattedText
    AnyFormattedText rprompt = default,
    AnyFormattedText bottomToolbar = default,
    FilterOrBool mouseSupport = default,           // default: false
    IReadOnlyList<IProcessor>? inputProcessors = null,
    AnyFormattedText? placeholder = null,
    IKeyBindingsBase? keyBindings = null,
    bool eraseWhenDone = false,
    object? tempfileSuffix = null,                 // string | Func<string> (default: ".txt")
    object? tempfile = null,                       // string | Func<string>
    double refreshInterval = 0,
    FilterOrBool showFrame = default,              // default: false
    IInput? input = null,
    IOutput? output = null,
    Type? interruptException = null,               // default: typeof(KeyboardInterruptException)
    Type? eofException = null)                     // default: typeof(EOFException)
```

## Public Properties (Mutable, Lock-Protected)

All properties listed in the constructor are exposed as public get/set properties with Lock synchronization.

```csharp
public AnyFormattedText Message { get; set; }
public FilterOrBool Multiline { get; set; }
public FilterOrBool WrapLines { get; set; }
public FilterOrBool IsPassword { get; set; }
public FilterOrBool CompleteWhileTyping { get; set; }
public FilterOrBool ValidateWhileTyping { get; set; }
public FilterOrBool EnableHistorySearch { get; set; }
public FilterOrBool SearchIgnoreCase { get; set; }
public FilterOrBool EnableSystemPrompt { get; set; }
public FilterOrBool EnableSuspend { get; set; }
public FilterOrBool EnableOpenInEditor { get; set; }
public FilterOrBool MouseSupport { get; set; }
public FilterOrBool SwapLightAndDarkColors { get; set; }
public FilterOrBool IncludeDefaultPygmentsStyle { get; set; }
public FilterOrBool ShowFrame { get; set; }
public ILexer? Lexer { get; set; }
public ICompleter? Completer { get; set; }
public bool CompleteInThread { get; set; }
public IValidator? Validator { get; set; }
public IAutoSuggest? AutoSuggest { get; set; }
public IStyle? Style { get; set; }
public IStyleTransformation? StyleTransformation { get; set; }
public ColorDepth? ColorDepth { get; set; }
public ICursorShapeConfig? Cursor { get; set; }
public IClipboard Clipboard { get; set; }
public IKeyBindingsBase? KeyBindings { get; set; }
public object? PromptContinuation { get; set; }
public AnyFormattedText RPrompt { get; set; }
public AnyFormattedText BottomToolbar { get; set; }
public IReadOnlyList<IProcessor>? InputProcessors { get; set; }
public AnyFormattedText? Placeholder { get; set; }
public CompleteStyle CompleteStyle { get; set; }
public int ReserveSpaceForMenu { get; set; }
public double RefreshInterval { get; set; }
public object? TempfileSuffix { get; set; }
public object? Tempfile { get; set; }
public Type InterruptException { get; }            // Immutable (set once)
public Type EofException { get; }                   // Immutable (set once)
```

## Computed Properties (Delegates to App)

```csharp
public EditingMode EditingMode { get; set; }       // Delegates to App.EditingMode
public IInput Input { get; }                        // Delegates to App.Input
public IOutput Output { get; }                      // Delegates to App.Output
```

## Owned Objects

```csharp
public IHistory History { get; }                    // Set in constructor, shared with DefaultBuffer
public Buffer DefaultBuffer { get; }                // Created by _CreateDefaultBuffer()
public Buffer SearchBuffer { get; }                 // Created by _CreateSearchBuffer()
public Layout Layout { get; }                       // Created by _CreateLayout()
public Application<TResult> App { get; }            // Created by _CreateApplication()
```

## Prompt Methods

```csharp
/// <summary>Display the prompt and return the user's input (blocking).</summary>
public TResult Prompt(
    AnyFormattedText? message = null,
    // ... all overridable session parameters (same list as constructor minus history, input, output, interrupt/eof) ...
    // Per-call parameters:
    object default_ = default,                      // string | Document (default: "")
    bool acceptDefault = false,
    Action? preRun = null,
    bool setExceptionHandler = true,
    bool handleSigint = true,
    bool inThread = false,
    InputHook? inputHook = null)

/// <summary>Display the prompt and return the user's input (async).</summary>
public async Task<TResult> PromptAsync(
    AnyFormattedText? message = null,
    // ... all overridable session parameters ...
    // Per-call parameters:
    object default_ = default,                      // string | Document (default: "")
    bool acceptDefault = false,
    Action? preRun = null,
    bool setExceptionHandler = true,
    bool handleSigint = true)
```

## Private/Internal Methods

```csharp
private Condition DynCond(string propertyName)                    // Dynamic condition factory
private Buffer CreateDefaultBuffer()                               // FR-004
private Buffer CreateSearchBuffer()                                // FR-005
private Layout CreateLayout()                                      // FR-006
private Application<TResult> CreateApplication(EditingMode, bool)  // FR-007
private KeyBindings CreatePromptBindings()                         // FR-011
private void AddPreRunCallables(Action?, bool)                     // FR-035
private IDisposable DumbPrompt(AnyFormattedText)                   // FR-014 (context manager pattern)

// Helper methods:
private StyleAndTextTuples GetPrompt()
private StyleAndTextTuples GetContinuation(int, int, int)
private StyleAndTextTuples GetLinePrefix(int, int, Func<StyleAndTextTuples>)
private StyleAndTextTuples GetArgText()
private StyleAndTextTuples InlineArg()
private Dimension GetDefaultBufferControlHeight()
```

## Python → C# Naming

| Python | C# |
|--------|-----|
| `PromptSession` | `PromptSession<TResult>` |
| `_dyncond` | `DynCond` |
| `_create_default_buffer` | `CreateDefaultBuffer` |
| `_create_search_buffer` | `CreateSearchBuffer` |
| `_create_layout` | `CreateLayout` |
| `_create_application` | `CreateApplication` |
| `_create_prompt_bindings` | `CreatePromptBindings` |
| `_dumb_prompt` | `DumbPrompt` |
| `_add_pre_run_callables` | `AddPreRunCallables` |
| `_get_prompt` | `GetPrompt` |
| `_get_continuation` | `GetContinuation` |
| `_get_line_prefix` | `GetLinePrefix` |
| `_get_arg_text` | `GetArgText` |
| `_inline_arg` | `InlineArg` |
| `_get_default_buffer_control_height` | `GetDefaultBufferControlHeight` |
| `prompt` (instance) | `Prompt` |
| `prompt_async` | `PromptAsync` |
| `editing_mode` (property) | `EditingMode` |
| `input` (property) | `Input` |
| `output` (property) | `Output` |

## Notes

- Python uses `@contextmanager` for `_dumb_prompt`. C# equivalent is `IDisposable` returned from `DumbPrompt()`, used in a `using` block.
- The `default` parameter is named `default_` in C# to avoid the reserved keyword conflict. Python source uses `default` as a keyword argument.
- `FilterOrBool` parameters with default `true` (like `wrapLines`, `completeWhileTyping`, `validateWhileTyping`, `includeDefaultPygmentsStyle`) need special constructor handling since `default(FilterOrBool)` is falsy. The constructor must detect unset values and apply the correct defaults.
