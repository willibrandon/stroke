# Data Model: Toolbar Widgets

**Feature**: 044-toolbar-widgets
**Date**: 2026-02-01

## Entities

### FormattedTextToolbar

**Extends**: `Window` (Stroke.Layout.Containers)

| Field | Type | Notes |
|-------|------|-------|
| (inherited from Window) | — | Content, style, height, dontExtendHeight |

**Construction-time state**:
- `text` → wrapped as `Func<IReadOnlyList<StyleAndTextTuple>>` via `() => FormattedTextUtils.ToFormattedText(text)` (lazy evaluation)
- `style` → passed to Window's `style` parameter
- `dontExtendHeight: true` → passed to Window
- `height: new Dimension(min: 1)` → passed to Window
- **Deviation (Constitution I)**: Python's `**kw` parameter forwarding to `FormattedTextControl` is omitted. C#'s typed constructor pattern does not support kwargs. Only `(text, style)` are accepted. This means `FormattedTextControl` parameters like `focusable`, `key_bindings`, `show_cursor` cannot be passed through `FormattedTextToolbar`.

**Relationships**: Contains a `FormattedTextControl` (as Window.Content).

---

### SystemToolbar

**Implements**: `IMagicContainer`

| Field | Type | Mutability | Notes |
|-------|------|------------|-------|
| Prompt | `AnyFormattedText` | Readonly | Stored at construction |
| EnableGlobalBindings | `IFilter` | Readonly | Converted from `FilterOrBool` |
| SystemBuffer | `Buffer` | Reference to mutable | Created with `name: BufferNames.System` |
| BufferControl | `BufferControl` | Reference | Created with BeforeInput, SimpleLexer, key bindings |
| Window | `Window` | Reference | Wraps BufferControl, height=1 |
| Container | `ConditionalContainer` | Reference | Wraps Window, filter=HasFocus(SystemBuffer) |

**Private Fields**:
- `_bindings: IKeyBindingsBase` — three-group merged key bindings (not exposed as property), built in `BuildKeyBindings()`

**Private Methods**:
- `GetDisplayBeforeText()` → returns `[("class:system-toolbar", "Shell command: "), ("class:system-toolbar.text", SystemBuffer.Text), ("", "\n")]`. Note: hard-codes "Shell command: " regardless of the `Prompt` property value (matches Python behavior).

**Key Bindings** (built in `BuildKeyBindings()`):
- **Emacs group** (`ConditionalKeyBindings` gated by `EmacsFilters.EmacsMode`):
  - Escape, Ctrl-G, Ctrl-C (each with `HasFocus(SystemBuffer)` filter) → `SystemBuffer.Reset()` then `Layout.FocusLast()`
  - Enter (with `HasFocus(SystemBuffer)` filter) → async: `await RunSystemCommandAsync(SystemBuffer.Text, displayBeforeText: GetDisplayBeforeText())`, then `SystemBuffer.Reset(appendToHistory: true)`, then `Layout.FocusLast()`
- **Vi group** (`ConditionalKeyBindings` gated by `ViFilters.ViMode`):
  - Escape, Ctrl-C (each with `HasFocus(SystemBuffer)` filter) → in order: (1) `ViState.InputMode = InputMode.Navigation`, (2) `SystemBuffer.Reset()`, (3) `Layout.FocusLast()`
  - Enter (with `HasFocus(SystemBuffer)` filter) → in order: (1) `ViState.InputMode = InputMode.Navigation`, (2) `await RunSystemCommandAsync(...)`, (3) `SystemBuffer.Reset(appendToHistory: true)`, (4) `Layout.FocusLast()`
- **Global group** (`ConditionalKeyBindings` gated by `EnableGlobalBindings`):
  - `Keys.Escape`, `"!"` (two-key M-!), filter `~HasFocus(SystemBuffer) & EmacsMode`, `isGlobal: true` → `Layout.Focus(Window)`
  - `"!"`, filter `~HasFocus(SystemBuffer) & ViMode & ViNavigationMode`, `isGlobal: true` → `ViState.InputMode = InputMode.Insert`, then `Layout.Focus(Window)`
- All three merged via `MergedKeyBindings`

**Relationships**: Owns Buffer, BufferControl, Window, ConditionalContainer. References `AppContext` for command execution via `KeyPressEventExtensions.GetApp()`.

---

### ArgToolbar

**Implements**: `IMagicContainer`

| Field | Type | Mutability | Notes |
|-------|------|------------|-------|
| Window | `Window` | Reference | Contains FormattedTextControl, height=1 |
| Container | `ConditionalContainer` | Reference | filter=AppFilters.HasArg |

**Display Logic**: Reads `AppContext.GetApp().KeyProcessor.Arg`, converting null to empty string (`arg ?? ""`). If `"-"`, displays `"-1"`. Format: `[("class:arg-toolbar", "Repeat: "), ("class:arg-toolbar.text", arg)]`.

---

### SearchToolbar

**Implements**: `IMagicContainer`

| Field | Type | Mutability | Notes |
|-------|------|------------|-------|
| SearchBuffer | `Buffer` | Reference to mutable | Provided or auto-created |
| Control | `SearchBufferControl` | Reference | With BeforeInput, SimpleLexer, ignore_case |
| Container | `ConditionalContainer` | Reference | filter=is_searching Condition |

**Constructor Parameters**:
- `searchBuffer: Buffer?` — optional, creates new if null
- `viMode: bool` — selects "/" / "?" vs "I-search:" prompts
- `textIfNotSearching: AnyFormattedText` — shown when not searching
- `forwardSearchPrompt: AnyFormattedText` — default "I-search: "
- `backwardSearchPrompt: AnyFormattedText` — default "I-search backward: "
- `ignoreCase: FilterOrBool` — passed to SearchBufferControl

**is_searching Condition**: `() => AppContext.GetApp().Layout.SearchLinks.ContainsKey(control)` — used for BOTH the `ConditionalContainer` filter AND the `BeforeInput` prompt selection logic.

**BeforeInput Logic**: Dynamic prompt selection:
1. If `!is_searching()` → return `textIfNotSearching` (avoids accessing `SearcherSearchState.Direction`, which may be null when not searching)
2. If `control.SearcherSearchState.Direction == SearchDirection.Backward` → return `"?"` (vi) or `backwardSearchPrompt`
3. Else → return `"/"` (vi) or `forwardSearchPrompt`

**Integration Gap**: `SearchBufferControl` constructor does not accept `inputProcessors`. Python passes `input_processors=[BeforeInput(...)]` to SearchBufferControl. Resolution: extend `SearchBufferControl` constructor to accept and forward `inputProcessors` to `BufferControl.base()` — this exposes an existing parameter, not a new API.

---

### CompletionsToolbarControl (internal)

**Extends**: `IUIControl` (direct implementation)

| Field | Type | Mutability | Notes |
|-------|------|------------|-------|
| (none) | — | — | Stateless; all data read from AppContext |

**CreateContent Algorithm**:
1. Read `AppContext.GetApp().CurrentBuffer.CompleteState`
2. If null or no completions → return `UIContent(getLine: _ => allFragments, lineCount: 1)` with empty `allFragments`
3. Get `index = CompleteState.CompleteIndex` (can be null, treated as 0 via `index ?? 0` for page-forward comparison)
4. Calculate `contentWidth = width - 6`
5. Iterate completions, accumulating fragments:
   - Each completion: `FormattedTextUtils.ToFormattedText(c.DisplayText, style: currentOrNormalStyle)`
   - Each completion followed by separator: `("", " ")`
   - When `FragmentListLen(fragments) + c.DisplayText.Length >= contentWidth`:
     - If `i <= (index ?? 0)`: clear fragments, set `cutLeft = true` (page forward)
     - Else: set `cutRight = true`, break (page ends)
6. Pad to contentWidth: append `("", spaces)` to fill
7. Safety trim: `fragments = fragments[:contentWidth]` (limits fragment count, not character count)
8. Wrap with arrow indicators: `" " + ("<" or " ") + " " + content + " " + (">" or " ") + " "`

**Style Classes**:
- `class:completion-toolbar.completion` — normal completion
- `class:completion-toolbar.completion.current` — selected completion
- `class:completion-toolbar.arrow` — arrow indicators

---

### CompletionsToolbar

**Implements**: `IMagicContainer`

| Field | Type | Mutability | Notes |
|-------|------|------------|-------|
| Container | `ConditionalContainer` | Reference | filter=AppFilters.HasCompletions |

**Contains**: Window wrapping CompletionsToolbarControl, height=1, style="class:completion-toolbar".

---

### ValidationToolbar

**Implements**: `IMagicContainer`

| Field | Type | Mutability | Notes |
|-------|------|------------|-------|
| Control | `FormattedTextControl` | Reference | Displays error text |
| Container | `ConditionalContainer` | Reference | filter=AppFilters.HasValidationError |

**Constructor Parameters**:
- `showPosition: bool` — whether to include line/column in display

**Display Logic**: Reads `AppContext.GetApp().CurrentBuffer.ValidationError`.
- If present: formats message. If `showPosition`, appends `" (line={row+1} column={col+1})"` using `Document.TranslateIndexToPosition()`. Returns `[("class:validation-toolbar", text)]`.
- If null: returns empty fragments `[]`.

**Style Classes**: `class:validation-toolbar` (applied to text fragments, NOT the Window — Window has no style parameter)

## State Transitions

No state machines in the toolbar widgets. Visibility is controlled by filters evaluated each render cycle:
- FormattedTextToolbar: Always visible (no filter)
- SystemToolbar: Visible when SystemBuffer has focus
- ArgToolbar: Visible when KeyProcessor.Arg is not null
- SearchToolbar: Visible when SearchBufferControl is in Layout.SearchLinks
- CompletionsToolbar: Visible when CurrentBuffer.CompleteState is not null with completions
- ValidationToolbar: Visible when CurrentBuffer.ValidationError is not null

## Validation Rules

- `FormattedTextToolbar`: text parameter must resolve to valid formatted text (enforced by FormattedTextUtils.ToFormattedText)
- `SystemToolbar`: prompt defaults to "Shell command: "; enableGlobalBindings defaults to true
- `SearchToolbar`: searchBuffer defaults to new Buffer(); viMode defaults to false; ignoreCase defaults to false
- `CompletionsToolbarControl`: Gracefully handles width < 7 (content_width ≤ 0) by producing empty/minimal content
- `ValidationToolbar`: showPosition defaults to false
