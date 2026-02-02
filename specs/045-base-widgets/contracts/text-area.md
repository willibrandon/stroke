# Contract: TextArea

**Namespace**: `Stroke.Widgets.Base`
**Python Source**: `prompt_toolkit/widgets/base.py` lines 112-322
**API Mapping**: `docs/api-mapping.md` lines 2429-2502

## API

```csharp
/// <summary>
/// A simple input field. High-level abstraction over Buffer, BufferControl, and Window.
/// </summary>
public class TextArea : IMagicContainer
{
    // Constructor
    public TextArea(
        string text = "",
        FilterOrBool multiline = default,      // defaults to true
        FilterOrBool password = default,       // defaults to false
        ILexer? lexer = null,
        IAutoSuggest? autoSuggest = null,
        ICompleter? completer = null,
        FilterOrBool completeWhileTyping = default,  // defaults to true
        Func<Buffer, bool>? acceptHandler = null,
        IHistory? history = null,
        FilterOrBool focusable = default,      // defaults to true
        FilterOrBool focusOnClick = default,   // defaults to false
        FilterOrBool wrapLines = default,      // defaults to true
        FilterOrBool readOnly = default,       // defaults to false
        Dimension? width = null,
        Dimension? height = null,
        FilterOrBool dontExtendHeight = default,  // defaults to false
        FilterOrBool dontExtendWidth = default,   // defaults to false
        bool lineNumbers = false,
        GetLinePrefixCallable? getLinePrefix = null,
        bool scrollbar = false,
        string style = "",
        SearchToolbar? searchField = null,
        FilterOrBool previewSearch = default,  // defaults to true
        AnyFormattedText prompt = default,
        IReadOnlyList<IProcessor>? inputProcessors = null,
        string name = "");

    // Writable configuration properties
    public ICompleter? Completer { get; set; }
    public FilterOrBool CompleteWhileTyping { get; set; }
    public ILexer? Lexer { get; set; }
    public IAutoSuggest? AutoSuggest { get; set; }
    public FilterOrBool ReadOnly { get; set; }
    public FilterOrBool WrapLines { get; set; }
    public IValidator? Validator { get; set; }

    // Computed properties
    public string Text { get; set; }           // delegates to Buffer
    public Document Document { get; set; }     // setter bypasses read-only
    public Func<Buffer, bool>? AcceptHandler { get; set; }  // delegates to Buffer

    // Component access
    public Buffer Buffer { get; }
    public BufferControl Control { get; }
    public Window Window { get; }

    // IMagicContainer
    public IContainer PtContainer();  // returns Window
}
```

## Construction Logic

1. Extract `SearchBufferControl` from `searchField?.Control` if provided
2. Store mutable config fields (Completer, Lexer, etc.) for runtime changes
3. Create Buffer with dynamic delegates: `readOnly: () => FilterUtils.IsTrue(this.ReadOnly)`
4. Create BufferControl with processor pipeline:
   - `ConditionalProcessor(AppendAutoSuggestion(), HasFocus(buffer) & ~IsDone)`
   - `ConditionalProcessor(PasswordProcessor(), ToFilter(password))`
   - `BeforeInput(prompt, style="class:text-area.prompt")`
   - Plus user-provided `inputProcessors`
5. Configure margins based on multiline/scrollbar/lineNumbers
6. Create Window with `style="class:text-area " + style`

## Key Behaviors

- **Document setter**: Calls `Buffer.SetDocument(value, bypassReadonly: true)`
- **Single-line mode**: When `multiline=false`, height is `Dimension.Exact(1)`, no margins
- **Default height**: When no height given, uses `Dimension(min: 1)`
