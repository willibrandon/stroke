# Data Model: Prompt Session

**Feature**: 047-prompt-session
**Date**: 2026-02-01

## Entities

### CompleteStyle (Enum)

Immutable enum controlling how autocompletions are displayed in the prompt.

| Value | Description | Python Equivalent |
|-------|-------------|-------------------|
| `Column` | Single-column dropdown menu near cursor | `CompleteStyle.COLUMN` |
| `MultiColumn` | Multi-column dropdown menu near cursor | `CompleteStyle.MULTI_COLUMN` |
| `ReadlineLike` | Completions printed below input line (GNU Readline style) | `CompleteStyle.READLINE_LIKE` |

**Relationships**: Referenced by `PromptSession.CompleteStyle` property. Determines which completion menu Float is visible in the layout (CompletionsMenu vs MultiColumnCompletionsMenu) and whether Tab triggers `DisplayCompletionsLikeReadline`.

---

### PromptSession\<TResult\> (Mutable Session)

The main session entity. Holds all prompt configuration, owns Buffer, Layout, and Application. Reusable across multiple `Prompt()` calls.

#### State Fields (Mutable, Lock-Protected)

| Field | Type | Default | Python Equivalent |
|-------|------|---------|-------------------|
| `Message` | `AnyFormattedText` | `""` | `message` |
| `Multiline` | `FilterOrBool` | `false` | `multiline` |
| `WrapLines` | `FilterOrBool` | `true` | `wrap_lines` |
| `IsPassword` | `FilterOrBool` | `false` | `is_password` |
| `CompleteWhileTyping` | `FilterOrBool` | `true` | `complete_while_typing` |
| `ValidateWhileTyping` | `FilterOrBool` | `true` | `validate_while_typing` |
| `EnableHistorySearch` | `FilterOrBool` | `false` | `enable_history_search` |
| `SearchIgnoreCase` | `FilterOrBool` | `false` | `search_ignore_case` |
| `EnableSystemPrompt` | `FilterOrBool` | `false` | `enable_system_prompt` |
| `EnableSuspend` | `FilterOrBool` | `false` | `enable_suspend` |
| `EnableOpenInEditor` | `FilterOrBool` | `false` | `enable_open_in_editor` |
| `MouseSupport` | `FilterOrBool` | `false` | `mouse_support` |
| `SwapLightAndDarkColors` | `FilterOrBool` | `false` | `swap_light_and_dark_colors` |
| `IncludeDefaultPygmentsStyle` | `FilterOrBool` | `true` | `include_default_pygments_style` |
| `ShowFrame` | `FilterOrBool` | `false` | `show_frame` |
| `Lexer` | `ILexer?` | `null` | `lexer` |
| `Completer` | `ICompleter?` | `null` | `completer` |
| `CompleteInThread` | `bool` | `false` | `complete_in_thread` |
| `Validator` | `IValidator?` | `null` | `validator` |
| `AutoSuggest` | `IAutoSuggest?` | `null` | `auto_suggest` |
| `Style` | `IStyle?` | `null` | `style` |
| `StyleTransformation` | `IStyleTransformation?` | `null` | `style_transformation` |
| `ColorDepth` | `ColorDepth?` | `null` | `color_depth` |
| `Cursor` | `ICursorShapeConfig?` | `null` | `cursor` |
| `Clipboard` | `IClipboard` | `InMemoryClipboard` | `clipboard` |
| `KeyBindings` | `IKeyBindingsBase?` | `null` | `key_bindings` |
| `PromptContinuation` | `object?` | `null` | `prompt_continuation` |
| `RPrompt` | `AnyFormattedText` | `null` | `rprompt` |
| `BottomToolbar` | `AnyFormattedText` | `null` | `bottom_toolbar` |
| `InputProcessors` | `IReadOnlyList<IProcessor>?` | `null` | `input_processors` |
| `Placeholder` | `AnyFormattedText?` | `null` | `placeholder` |
| `CompleteStyle` | `CompleteStyle` | `Column` | `complete_style` |
| `ReserveSpaceForMenu` | `int` | `8` | `reserve_space_for_menu` |
| `RefreshInterval` | `double` | `0` | `refresh_interval` |
| `TempfileSuffix` | `object?` | `".txt"` | `tempfile_suffix` |
| `Tempfile` | `object?` | `null` | `tempfile` |

#### Immutable Fields (Set Once in Constructor)

| Field | Type | Default | Python Equivalent | Notes |
|-------|------|---------|-------------------|-------|
| `InterruptException` | `Type` | `typeof(KeyboardInterruptException)` | `interrupt_exception` | Must be assignable to `Exception`; instantiated via `Activator.CreateInstance` |
| `EofException` | `Type` | `typeof(EOFException)` | `eof_exception` | Must be assignable to `Exception`; instantiated via `Activator.CreateInstance` |
| `_input` | `IInput?` | `null` | `_input` | Passed to Application constructor |
| `_output` | `IOutput?` | `null` | `_output` | Passed to Application constructor; null triggers dumb terminal detection |

**Note**: `eraseWhenDone` is a constructor parameter passed directly to `CreateApplication()` (and then to Application's constructor). It is NOT stored as a mutable session property — matching Python where it's only used during `_create_application`.

#### Owned Objects (Set/Created in Constructor, Immutable After Construction)

| Field | Type | Created By | Python Equivalent | Notes |
|-------|------|------------|-------------------|-------|
| `History` | `IHistory` | Constructor param (default: `new InMemoryHistory()`) | `history` | Shared with DefaultBuffer; NOT updated per-prompt (not in Python's `_fields` tuple) |
| `DefaultBuffer` | `Buffer` | `_CreateDefaultBuffer()` | `default_buffer` | |
| `SearchBuffer` | `Buffer` | `_CreateSearchBuffer()` | `search_buffer` | |
| `Layout` | `Layout` | `_CreateLayout()` | `layout` | |
| `App` | `Application<TResult>` | `_CreateApplication()` | `app` | |

#### Computed Properties (Delegates to App)

| Property | Type | Delegates To |
|----------|------|-------------|
| `EditingMode` | `EditingMode` (get/set) | `App.EditingMode` |
| `Input` | `IInput` (get) | `App.Input` |
| `Output` | `IOutput` (get) | `App.Output` |

#### State Transitions

```
Constructor → [History set, DefaultBuffer, SearchBuffer, Layout, App created]
    │          (Layout and App are built ONCE in constructor, not per-prompt)
    │
    ├─ Prompt()/PromptAsync() called
    │   ├─ Per-prompt overrides applied (non-null params update session state permanently)
    │   ├─ PreRunCallables appended to App.PreRunCallables (pre_run + accept_default)
    │   │   └─ App.RunAsync() calls and clears PreRunCallables, preventing accumulation
    │   ├─ DefaultBuffer.Reset(default document) — clears buffer text, completion state
    │   ├─ App.RefreshInterval = session.RefreshInterval (not reactive, set once per call)
    │   ├─ Dumb terminal check: if _output==null && IsDumbTerminal() → _DumbPrompt
    │   └─ App.Run()/RunAsync() → returns TResult (or throws interrupt/eof exception)
    │
    ├─ Prompt() called again (session reuse)
    │   ├─ State that RESETS: buffer text, buffer completion state, buffer cursor position
    │   ├─ State that PERSISTS: history, all session properties (message, completer, etc.)
    │   └─ State that MAY HAVE CHANGED: any property updated via per-prompt overrides
    │
    └─ [Session discarded by GC]
```

---

### KeyboardInterruptException (New Exception Type)

| Field | Type | Description |
|-------|------|-------------|
| (inherits Exception) | | Default exception thrown on Ctrl-C |

---

### EOFException (New Exception Type)

| Field | Type | Description |
|-------|------|-------------|
| (inherits Exception) | | Default exception thrown on Ctrl-D with empty buffer |

---

### PromptContinuationCallable (Delegate)

```
delegate AnyFormattedText PromptContinuationCallable(int promptWidth, int lineNumber, int wrapCount)
```

Callable form of `PromptContinuationText`. Called for each continuation line in multiline prompts to generate the prefix text.

---

### _RPrompt (Internal Window Subclass)

Internal `Window` subclass for displaying right-aligned prompt text.

| Field | Type | Description |
|-------|------|-------------|
| (inherits Window) | | `FormattedTextControl` with `WindowAlign.Right`, style `"class:rprompt"` |

**Relationships**: Created as a Float in the prompt layout. Content reads from `PromptSession.RPrompt` property.

## Validation Rules

- `ReserveSpaceForMenu` must be ≥ 0
- `InterruptException` must be a concrete (non-abstract) type assignable to `Exception` with a parameterless constructor; the constructor MUST validate this via `Activator.CreateInstance(type)` test or type reflection check, throwing `ArgumentException` if the type is invalid — this catches misconfiguration at construction time rather than at Ctrl-C time
- `EofException` must be a concrete (non-abstract) type assignable to `Exception` with a parameterless constructor; same validation as `InterruptException`
- Only one `Prompt()` / `PromptAsync()` call should be active per session at a time (documented constraint, not enforced at runtime — matches Python)

## Relationships Diagram

```
PromptSession<TResult>
    ├── owns → Buffer (DefaultBuffer)          ← DynamicCompleter, DynamicValidator, DynamicAutoSuggest read session properties
    ├── owns → Buffer (SearchBuffer)           ← used by SearchToolbar and SearchBufferControl
    ├── owns → Layout                          ← FloatContainer with HSplit + completion menus + right prompt
    │   ├── contains → Window (multiline prompt area above input)
    │   ├── contains → Window (default_buffer_window) ← BufferControl
    │   ├── contains → Window (search_buffer_control) ← for non-multiline search
    │   ├── contains → Float (CompletionsMenu)
    │   ├── contains → Float (MultiColumnCompletionsMenu)
    │   ├── contains → Float (_RPrompt)
    │   ├── contains → ValidationToolbar
    │   ├── contains → SystemToolbar
    │   ├── contains → Window (arg text, multiline)
    │   ├── contains → SearchToolbar (multiline)
    │   ├── contains → Window (bottom_toolbar)
    │   └── optional → Frame (wrapping main_input_container)
    ├── owns → Application<TResult>            ← DynamicStyle, DynamicClipboard, merged KeyBindings
    ├── references → IHistory                  ← shared with DefaultBuffer, persists across Prompt() calls
    └── references → CompleteStyle             ← determines which completion Float is visible
```
