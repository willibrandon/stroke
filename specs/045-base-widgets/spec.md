# Feature Specification: Base Widgets

**Feature Branch**: `045-base-widgets`
**Created**: 2026-02-01
**Status**: Draft
**Input**: User description: "Implement the reusable widget components for building full-screen applications including TextArea, Label, Button, Frame, Shadow, Box, RadioList, CheckboxList, ProgressBar, and line widgets."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Text Input with TextArea (Priority: P1)

A developer building a full-screen application needs an editable text input field. They create a `TextArea` widget with configurable options (multiline, password masking, syntax highlighting, auto-completion, validation) and embed it in their layout. The TextArea provides a high-level abstraction over Buffer, BufferControl, and Window, offering sane defaults while exposing the underlying components for advanced customization.

**Why this priority**: Text input is the most fundamental interactive widget. Without it, no REPL, editor, or input form can function. Every other widget depends on TextArea or its compositional approach.

**Independent Test**: Can be fully tested by creating a TextArea instance with various configurations, verifying the Buffer receives text, the Document updates correctly, and the Window renders with expected margins and styles.

**Acceptance Scenarios**:

1. **Given** a default TextArea, **When** the user types text, **Then** the Buffer stores it and Text/Document properties reflect the current content
2. **Given** a TextArea with `password: true`, **When** the user types, **Then** the display shows asterisks instead of the actual characters
3. **Given** a TextArea with `multiline: false`, **When** created, **Then** the window height is exactly 1 and Enter does not insert newlines
4. **Given** a TextArea with `readOnly: true`, **When** the user attempts to type, **Then** the Buffer rejects modifications
5. **Given** a TextArea with a completer, **When** the user types, **Then** completions appear based on the configured completer
6. **Given** a TextArea with `lineNumbers: true`, **When** rendered, **Then** a NumberedMargin appears on the left
7. **Given** a TextArea with `scrollbar: true`, **When** rendered with multiline content, **Then** a ScrollbarMargin appears on the right

---

### User Story 2 - Button Interaction (Priority: P1)

A developer needs clickable buttons for forms and dialogs. They create `Button` widgets with captions and click handlers. Buttons respond to Enter, Space, and mouse click events, triggering the associated handler.

**Why this priority**: Buttons are essential for any interactive dialog, form, or confirmation workflow. They are the primary action trigger in full-screen applications.

**Independent Test**: Can be fully tested by creating a Button, simulating key presses (Enter, Space) and mouse clicks, and verifying the handler is invoked.

**Acceptance Scenarios**:

1. **Given** a Button with a handler, **When** the user presses Enter or Space, **Then** the handler is called
2. **Given** a Button with a handler, **When** the user clicks with the mouse, **Then** the handler is called on MOUSE_UP
3. **Given** a Button with text "OK" and width 12, **When** rendered, **Then** the text is centered between left and right symbols: `<    OK    >`
4. **Given** a Button with custom symbols `[` and `]`, **When** rendered, **Then** the button displays `[   OK   ]`
5. **Given** a focused Button, **When** rendered, **Then** the style is `class:button.focused`; when unfocused, the style is `class:button`

---

### User Story 3 - Frame and Shadow Decoration (Priority: P2)

A developer needs to visually group content with a border and optional title. They wrap a container in a `Frame` to draw box-drawing characters around it. For dialog-style elevation, they wrap the Frame in a `Shadow` to create a visual depth effect.

**Why this priority**: Frames and shadows provide essential visual structure for dialogs and grouped content. They are prerequisites for the Dialog widget.

**Independent Test**: Can be tested by creating a Frame around a Label, verifying the border characters render correctly, and wrapping in Shadow to verify the offset transparent windows.

**Acceptance Scenarios**:

1. **Given** a Frame with a title, **When** rendered, **Then** box-drawing characters surround the body with the title displayed in the top border between pipe characters
2. **Given** a Frame without a title, **When** rendered, **Then** the top border is a continuous horizontal line without title area
3. **Given** a Frame body that changes at runtime, **When** the body attribute is reassigned, **Then** the Frame renders the new body via DynamicContainer
4. **Given** a Shadow wrapping a container, **When** rendered, **Then** transparent Float windows with `class:shadow` style appear offset below and to the right of the body

---

### User Story 4 - RadioList and CheckboxList Selection (Priority: P2)

A developer needs selection lists for user choices. `RadioList` provides single-selection (radio buttons), while `CheckboxList` provides multi-selection (checkboxes). Both support keyboard navigation (up/down/j/k), page navigation, Enter/Space to toggle, number shortcuts, and character-based jump-to-item.

**Why this priority**: Selection widgets are critical for configuration dialogs, option pickers, and forms. They share a common base class with rich keyboard interaction.

**Independent Test**: Can be tested by creating lists with values, simulating keyboard navigation, verifying selection state changes, and checking rendered formatted text output.

**Acceptance Scenarios**:

1. **Given** a RadioList with 3 items, **When** the user presses Down then Enter, **Then** the second item becomes the `CurrentValue`
2. **Given** a CheckboxList with 3 items, **When** the user presses Space on items 1 and 3, **Then** both are in `CurrentValues`
3. **Given** a RadioList, **When** the user selects a new item, **Then** the previous selection is deselected (only one at a time)
4. **Given** a list with `showNumbers: true`, **When** the user presses "2", **Then** the cursor jumps to the second item
5. **Given** a list with items "Apple", "Banana", "Cherry", **When** the user presses "b", **Then** the cursor jumps to "Banana"
6. **Given** a list, **When** the user clicks a mouse on a row, **Then** that row is selected/toggled

---

### User Story 5 - Dialog Composition (Priority: P2)

A developer needs to build dialog windows by composing Frame, Shadow, Box, and Button widgets. The `Dialog` class combines a body, optional title, and optional button row with Tab/Shift-Tab focus cycling and Left/Right navigation between buttons.

**Why this priority**: Dialogs are the high-level composition that brings Frame, Shadow, Box, and Button together. They are essential for confirmations, input prompts, and messages.

**Independent Test**: Can be tested by creating a Dialog with a body and buttons, verifying the container hierarchy (Shadow > Frame > HSplit with body Box and button VSplit), and checking focus cycling key bindings.

**Acceptance Scenarios**:

1. **Given** a Dialog with buttons, **When** rendered, **Then** the body appears above the button row, wrapped in a Frame with Shadow
2. **Given** a Dialog with multiple buttons, **When** the user presses Tab, **Then** focus cycles forward; Shift-Tab cycles backward
3. **Given** a Dialog with multiple buttons, **When** a button is focused and Left/Right is pressed, **Then** focus moves between buttons
4. **Given** a Dialog with `withBackground: true`, **When** rendered, **Then** the Frame+Shadow is wrapped in an additional Box with `class:dialog` style

---

### User Story 6 - Label, Box, and Line Widgets (Priority: P3)

A developer needs static display widgets. `Label` shows non-editable text. `Box` adds padding around content. `VerticalLine` and `HorizontalLine` provide visual separators. `ProgressBar` displays completion percentage.

**Why this priority**: These are simpler utility widgets that support the more complex widgets above. They enhance visual layout but are not interactive.

**Independent Test**: Can be tested by creating each widget and verifying container output, dimensions, and style strings.

**Acceptance Scenarios**:

1. **Given** a Label with text "Hello", **When** rendered, **Then** the text appears in a non-focusable Window with `class:label` style
2. **Given** a Label with no explicit width, **When** rendered, **Then** the preferred width equals the longest line of text
3. **Given** a Box with padding of 2, **When** rendered, **Then** padding Windows surround the body on all four sides
4. **Given** a ProgressBar at 45%, **When** rendered, **Then** the label shows "45%" and the used/unused bar widths are proportional
5. **Given** a VerticalLine, **When** rendered, **Then** a Window with width=1 and `Border.Vertical` char appears
6. **Given** a HorizontalLine, **When** rendered, **Then** a Window with height=1 and `Border.Horizontal` char appears

---

### User Story 7 - Checkbox Convenience Wrapper (Priority: P3)

A developer needs a simple single-checkbox toggle. The `Checkbox` class wraps `CheckboxList` with a single item, exposing a `Checked` boolean property for easy state management.

**Why this priority**: A convenience wrapper for a common pattern. Lower priority because developers can use CheckboxList directly.

**Independent Test**: Can be tested by creating a Checkbox, toggling its state, and verifying the `Checked` property reflects the underlying CheckboxList state.

**Acceptance Scenarios**:

1. **Given** a Checkbox with `checked: true`, **When** created, **Then** `Checked` returns true and the item appears selected
2. **Given** a Checkbox with `checked: false`, **When** the user presses Space, **Then** `Checked` becomes true
3. **Given** a Checkbox, **When** `Checked` is set to false programmatically, **Then** the visual state updates to unchecked

---

### Edge Cases

- What happens when RadioList or CheckboxList is created with an empty values list? The system throws `ArgumentException` (matching Python's `assert values` guard).
- What happens when a Button handler is null and the user presses Enter? Nothing happens — the handler null-check guards the invocation, no exception is thrown.
- What happens when ProgressBar percentage is set outside 0-100? The label updates with the raw value (e.g., "-5%" or "150%") and the weight-based dimension calculation proceeds with those values. `D(weight=negative)` produces a Dimension with weight ≤ 0, which the layout system treats as zero allocation. The Python source does not clamp, so the port matches that behavior.
- What happens when TextArea text is set while `readOnly: true`? The `Document` setter bypasses read-only via `Buffer.SetDocument(..., bypassReadonly: true)`.
- What happens when `TextArea.Text` setter is called with null? It MUST set an empty `Document` (equivalent to `new Document("")`), not throw. This matches Python's behavior where setting text to `""` produces an empty document.
- What happens when a RadioList default value is not in the values list? Falls back to the first value (matching Python behavior).
- What happens when `RadioList<T>` is passed a `default` value that is null for a reference type T? When `default` is null, `defaultValues: null` is passed to the base class, which falls back to the first item. For value types, `default(T)` is used (e.g., 0 for int); if that value is not in the values list, falls back to first item.
- What happens when Label text is empty? The preferred width is 0.
- What happens when Frame title is set to empty string at runtime? The `ConditionalContainer` reactively switches to the no-title top border row. The `Condition` filter re-evaluates when the layout system calls `PreferredWidth`/`PreferredHeight`/`WriteToScreen`, detecting the empty title and rendering the alternative content.
- What happens when `Button.Width` is smaller than the combined width of `LeftSymbol + RightSymbol`? The available text width computes to zero or negative. The centering algorithm's `PadRight(Math.Max(0, availableWidth))` clamps to zero padding. The symbols are still rendered, but no text is visible between them. This matches Python behavior.
- What happens when Dialog buttons list is null vs. empty? Both result in "no buttons" — when `buttons` is null or an empty list, the body is used directly as frame content without the button row HSplit. Null and empty are functionally equivalent.
- What happens when `Box.Padding` and all per-side paddings are null? Each side resolves to `null ?? null = null`. No padding Windows are created — the body fills the entire HSplit space with no surrounding padding.
- What happens when `DialogList.Values` contains duplicate `T` values? Selection operates by index, not by value equality. The `_HandleEnter` logic reads `values[selectedIndex].Value`, so duplicate values are selected/deselected by their position. `CurrentValues` uses `List<T>` default equality (reference equality for classes, value equality for structs) for the `Contains`/`Remove` operations.
- What happens when `DialogList` character jump is pressed and multiple items start with the same character? The implementation cycles through matching items on repeated presses of the same character, advancing to the next match each time. This matches the Python behavior where repeated presses cycle through all items starting with that character.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `Border` static class with Unicode box-drawing character constants (Horizontal `─` U+2500, Vertical `│` U+2502, TopLeft `┌` U+250C, TopRight `┐` U+2510, BottomLeft `└` U+2514, BottomRight `┘` U+2518) as `const string` fields
- **FR-002**: System MUST provide a `TextArea` widget that wraps Buffer, BufferControl, and Window with 26 configurable constructor parameters: `text`, `multiline`, `password`, `lexer`, `autoSuggest`, `completer`, `completeWhileTyping`, `acceptHandler`, `history`, `focusable`, `focusOnClick`, `wrapLines`, `readOnly`, `width`, `height`, `dontExtendHeight`, `dontExtendWidth`, `lineNumbers`, `getLinePrefix`, `scrollbar`, `style`, `searchField`, `previewSearch`, `prompt`, `inputProcessors`, and `name`. The `name` parameter maps to `Buffer.Name` for identification. See contract [text-area.md](contracts/text-area.md) for full parameter defaults and behaviors.
- **FR-003**: TextArea MUST expose writable configuration properties (`Completer`, `CompleteWhileTyping`, `Lexer`, `AutoSuggest`, `ReadOnly`, `WrapLines`, `Validator`) as get/set, computed delegation properties (`Text`, `Document`, `AcceptHandler`) as get/set delegating to Buffer, and readonly component access (`Buffer`, `Control`, `Window`). All `FilterOrBool` parameters MUST be bridged to `Func<bool>` via lambda closures (e.g., `readOnly: () => FilterUtils.IsTrue(this.ReadOnly)`) to support runtime mutation.
- **FR-004**: TextArea `Document` setter MUST bypass read-only restriction via `Buffer.SetDocument(value, bypassReadonly: true)`. TextArea `Text` setter with a null value MUST set an empty `Document` (not throw).
- **FR-005**: System MUST provide a `Label` widget that displays formatted text in a non-focusable, non-editable Window with style `"class:label " + style`
- **FR-006**: Label MUST auto-calculate preferred width from the longest line of text when no explicit width is provided, using `UnicodeWidth.GetWidth()` for width measurement. When text is empty, preferred width is 0.
- **FR-007**: System MUST provide a `Button` widget with Enter key, Space key, and mouse click (MOUSE_UP event type) handlers that invoke the configured `Action? Handler`
- **FR-008**: Button MUST display text centered between configurable left/right symbols (default `<` and `>`) within the specified width (default 12). The centering algorithm MUST: (1) compute available width as `Width - UnicodeWidth.GetWidth(LeftSymbol) - UnicodeWidth.GetWidth(RightSymbol) + (Text.Length - UnicodeWidth.GetWidth(Text))`, (2) pad-left to `(availableWidth + Text.Length) / 2`, (3) pad-right to fill remaining space. When `Width` is smaller than the combined width of `LeftSymbol + RightSymbol`, the text area has zero or negative width — the symbols are still rendered, matching Python behavior.
- **FR-009**: Button MUST apply style `class:button.focused` when focused and `class:button` when unfocused. Text fragments MUST use sub-styles: `class:button.arrow` for left/right symbols, `class:button.text` for centered text. All fragments MUST include a mouse handler.
- **FR-010**: System MUST provide a `Frame` widget that draws box-drawing border characters around a body container with an optional title. Constructor parameters: `body` (AnyContainer), `title` (AnyFormattedText, default empty), `style` (string, default ""), `width` (Dimension?, default null), `height` (Dimension?, default null), `keyBindings` (IKeyBindingsBase?, default null), `modal` (bool, default false). The Frame's HSplit uses style `"class:frame " + style` and passes through width, height, keyBindings, and modal.
- **FR-011**: Frame MUST conditionally display the title row using `ConditionalContainer` with `alternativeContent`: when a title is present, display `VSplit[TopLeft, Horizontal, "|", Label(Template(" {} ").Format(title)), "|", Horizontal, TopRight]`; when absent, display `VSplit[TopLeft, Horizontal, TopRight]`. The title Label uses style `class:frame.label` with `dontExtendWidth: true`. All border windows use style `class:frame.border`. The title row MUST react to runtime `Title` property changes via the filter `Condition`.
- **FR-012**: Frame `Body` MUST be a runtime-changeable get/set property. The body container MUST be wrapped with `DynamicContainer(() => this.Body)` to support runtime body changes. Frame `Title` MUST also be a runtime-changeable get/set property.
- **FR-013**: System MUST provide a `Shadow` widget that wraps a body in a `FloatContainer` with two transparent `Float` windows using `class:shadow` style. "Transparent" means `transparent=true` on the Float — the Float renders its content but allows underlying content to show through where no characters are drawn by the Float's content window. The bottom shadow strip: `Float(bottom=-1, height=1, left=1, right=-1)` (cell offsets: 1 row below body bottom, starting 1 column right of body left). The right shadow strip: `Float(bottom=-1, top=1, width=1, right=-1)` (cell offsets: 1 column right of body right, starting 1 row below body top). All offset values are cell-based offsets, not relative positions.
- **FR-014**: System MUST provide a `Box` widget that adds configurable padding around a body container. Constructor parameters: `body`, `padding` (Dimension?, overall default), `paddingLeft/Right/Top/Bottom` (Dimension?, per-side overrides), `width`, `height`, `style`, `@char` (string?, fill character for padding Windows — null means no fill character), `modal`, `keyBindings` (IKeyBindingsBase?, default null). Note: Python's `Box.__init__` accepts `key_bindings` but always passes `key_bindings=None` to the inner `HSplit` — the parameter is accepted for API fidelity but is NOT forwarded to the layout container. Padding resolution uses fallback logic: each side resolves as `PaddingLeft ?? Padding`, `PaddingRight ?? Padding`, etc. When both overall `Padding` and all per-side paddings are null, no padding Windows are created — the body fills the entire space.
- **FR-015**: System MUST provide a `DialogList<T>` base class (C# public name for Python's internal `_DialogList`; the Python underscore-prefix convention for "internal" base classes maps to a public generic class in C#). The class MUST support:
  - **Constructor parameters**: `values`, `defaultValues`, `selectOnFocus` (bool — when true, auto-selects the focused item when focus enters the list), `openCharacter`, `selectCharacter`, `closeCharacter`, `containerStyle`, `defaultStyle`, `numberStyle` (style for the "N. " number prefix, e.g., `class:radio-number`), `selectedStyle`, `checkedStyle`, `multipleSelection`, `showScrollbar`, `showCursor`, `showNumbers`
  - **8 key binding groups** with exact behaviors:
    1. **Up / k**: Move cursor up, clamp at index 0
    2. **Down / j**: Move cursor down, clamp at last index. The `j`/`k` keys are Vi-style navigation aliases and are always active regardless of editing mode.
    3. **PageUp**: Move cursor up by the number of visible lines (from `Window.RenderInfo`)
    4. **PageDown**: Move cursor down by the number of visible lines
    5. **Enter**: Toggle selection (add/remove for multi-select, set for single-select)
    6. **Space**: Same as Enter (toggle selection)
    7. **Number shortcuts (1-9)**: Jump to Nth item (1-indexed). Only active when `showNumbers=true`; when `showNumbers=false`, number key presses are ignored (fall through to character jump).
    8. **Character jump**: On any printable character, jump to the first item whose label starts with that character (case-insensitive). When multiple items start with the same character, cycles through them on repeated presses.
  - **Mouse**: MOUSE_UP on a row selects/toggles that item
  - **ScrollbarMargin**: Added when `showScrollbar=true`
- **FR-016**: System MUST provide `RadioList<T>` extending `DialogList<T>` with radio-button styling (`(`, `*`, `)`, `class:radio-list`, `class:radio`, `class:radio-selected`, `class:radio-checked`, `class:radio-number`). RadioList MUST always override `multipleSelection` to `false` in the base class constructor call, regardless of any value passed by the caller. Constructor accepts a single `default` value (converted to `defaultValues: [default]` for the base class; null passes `defaultValues: null` to use first item).
- **FR-017**: System MUST provide `CheckboxList<T>` extending `DialogList<T>` with checkbox styling (`[`, `*`, `]`, `class:checkbox-list`, `class:checkbox`, `class:checkbox-selected`, `class:checkbox-checked`). CheckboxList always passes `multipleSelection: true` to the base class. CheckboxList intentionally omits `showNumbers`, `selectOnFocus`, `showCursor`, `showScrollbar` from its constructor — these use the base class defaults (`false`, `false`, `true`, `true` respectively). This matches the Python source where `CheckboxList.__init__` only passes style parameters.
- **FR-018**: System MUST provide `Checkbox` as a convenience wrapper extending `CheckboxList<string>` with a single item `("value", text)` and a `Checked` boolean get/set property mapping to `"value" in CurrentValues`. `Checkbox` MUST override `ShowScrollbar` to `false` at the class level (as a `new` property or override, not via constructor parameter), matching the Python class-level `show_scrollbar = False` attribute.
- **FR-019**: System MUST provide a `ProgressBar` widget using `FloatContainer` to layer a percentage label over proportionally-weighted used/unused bar windows (VSplit with `D(weight=percentage)` for used, `D(weight=100-percentage)` for unused). Default percentage MUST be 60 (matching Python). The percentage is not clamped to 0-100 to match Python behavior. Setting `Percentage` MUST also update `Label.Text` to `"{percentage}%"`. Styles: `class:progress-bar.used` for the filled portion, `class:progress-bar` for the unfilled portion.
- **FR-020**: System MUST provide `VerticalLine` (Window with `width=Dimension.Exact(1)`, `char=Border.Vertical`, `style="class:line,vertical-line"`) and `HorizontalLine` (Window with `height=Dimension.Exact(1)`, `char=Border.Horizontal`, `style="class:line,horizontal-line"`) separator widgets
- **FR-021**: System MUST provide a `Dialog` widget that composes Frame, Shadow, Box, and Buttons. Tab/Shift-Tab MUST cycle focus forward/backward, gated by a `~hasCompletions` filter (Tab/Shift-Tab are suppressed when a completion menu is open, allowing the completion system to handle Tab). Left/Right arrow keys MUST navigate between buttons when more than one button is present. Boundary suppression: each binding is gated by a `Condition` filter — Left checks that the currently focused window is NOT the first button (suppressed at boundary), Right checks that it is NOT the last button (suppressed at boundary). These filters evaluate dynamically based on the `Application`'s current focus position within the button VSplit. Focus navigation uses `FocusFunctions.FocusNext` and `FocusFunctions.FocusPrevious`. When buttons list is null or empty, the body is used directly as frame content without a button row. When buttons is a non-empty list, the frame body is an HSplit with a body Box (padding `D(preferred=1, max=1)`, paddingBottom=0) and a button Box (`VSplit(buttons, padding=1)`, height `D(min=1, max=3, preferred=3)`). With `withBackground: true`, the Frame+Shadow is wrapped in a `Box(style="class:dialog", width=width)`.
- **FR-022**: All widgets MUST implement the `IMagicContainer` interface, returning their inner container from `PtContainer()`. This is the C# equivalent of Python's `__pt_container__()` protocol. Widgets do NOT implement `IContainer` directly — they compose containers and expose them through `IMagicContainer`.
- **FR-023**: `RadioList` and `CheckboxList` (and by extension `DialogList<T>`) MUST throw `ArgumentException` when created with an empty values list, matching Python's `assert` guard.
- **FR-024**: The following properties MUST be get/set (runtime-mutable): TextArea (`Text`, `Document`, `AcceptHandler`, `Completer`, `CompleteWhileTyping`, `Lexer`, `AutoSuggest`, `ReadOnly`, `WrapLines`, `Validator`), Button (`Text`, `Handler`), Frame (`Title`, `Body`), Dialog (`Title`, `Body`), Label (`Text`), ProgressBar (`Percentage`), Box (`Body`, `Padding`, `PaddingLeft`, `PaddingRight`, `PaddingTop`, `PaddingBottom`), DialogList (`ShowNumbers`, `CurrentValue`, `CurrentValues`, `ShowScrollbar`), Checkbox (`Checked`).

### Key Entities

- **Border**: Static class holding 6 Unicode box-drawing character constants (`const string`)
- **TextArea**: High-level editable text input wrapping Buffer + BufferControl + Window (22 constructor parameters)
- **Label**: Non-editable formatted text display with auto-width calculation
- **Button**: Clickable widget with handler, key bindings (Space/Enter), and mouse support (MOUSE_UP)
- **Frame**: Border decorator with optional title using box-drawing characters, ConditionalContainer for title switching, DynamicContainer for body
- **Shadow**: Visual depth decorator using two transparent offset Float windows (`class:shadow`)
- **Box**: Padding decorator using surrounding Windows in HSplit/VSplit with per-side fallback logic
- **DialogList\<T\>**: Base class for selection lists with 8 key binding groups, mouse handling, and scrollbar margin (C# public name for Python's internal `_DialogList`)
- **RadioList\<T\>**: Single-selection list extending DialogList\<T\> (always overrides `multipleSelection=false`)
- **CheckboxList\<T\>**: Multi-selection list extending DialogList\<T\> (always passes `multipleSelection=true`)
- **Checkbox**: Convenience single-checkbox wrapper extending CheckboxList\<string\> with `Checked` property and class-level `ShowScrollbar=false`
- **ProgressBar**: Percentage display (default 60%) using weighted VSplit windows under FloatContainer, thread-safe via Lock
- **VerticalLine / HorizontalLine**: Single-dimension separator widgets using Border constants
- **Dialog**: Composite widget combining Frame + Shadow + Box + Buttons with Tab/Shift-Tab focus cycling (gated by `~hasCompletions`) and Left/Right button navigation

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 15 widget classes compile and pass unit tests with ≥80% code coverage. Note: `test-mapping.md` maps only 2 Button tests (lines 708-726). Additional tests beyond the mapping MUST be written as needed to reach the coverage target — the mapping is a minimum, not a ceiling.
- **SC-002**: All widgets correctly implement `IMagicContainer` and can be passed as `AnyContainer` arguments to `HSplit`, `VSplit`, `FloatContainer`, `ConditionalContainer`, `DynamicContainer`, `Frame`, `Box`, `Shadow`, and `Dialog`. Verified by unit tests that construct each widget and call `PtContainer()` returning a non-null `IContainer`, and by composition tests embedding widgets in at least HSplit and VSplit containers.
- **SC-003**: TextArea supports the following configuration combinations without runtime errors: (multiline=true + scrollbar + lineNumbers), (multiline=false + single-line height), (password=true + no margins), (readOnly=true + text setter bypasses), (completer + completeWhileTyping), (lexer + validator). These 6 representative combinations cover the key interaction points; exhaustive combinatorial testing of all 7+ boolean parameters is not required.
- **SC-004**: Button click handlers fire consistently: verified by 3 tests — one for Enter key, one for Space key, one for MOUSE_UP mouse event. Handler invocation count matches expected call count.
- **SC-005**: RadioList maintains single-selection invariant — verified by test that selects item A, then selects item B, then asserts `CurrentValue == B` and `CurrentValues` contains exactly one element.
- **SC-006**: CheckboxList correctly manages multi-selection state — verified by test that toggles items A and C, asserts both are in `CurrentValues`, then toggles A again and asserts only C remains.
- **SC-007**: Frame renders correct box-drawing characters for all 6 border constants (TopLeft, TopRight, BottomLeft, BottomRight, Horizontal, Vertical). Verified by unit tests that: (1) construct a Frame and inspect the border Window `char` properties matching `Border.*` constants, (2) verify ConditionalContainer switches between title and no-title top rows when Title transitions between empty and non-empty values.
- **SC-008**: Dialog Tab/Shift-Tab and Left/Right focus cycling verified by tests that: (1) create a Dialog with 3 buttons, (2) simulate Tab key and verify focus moves to next button, (3) simulate Shift-Tab and verify focus moves to previous button, (4) simulate Right/Left and verify focus moves between buttons with boundary clamping.
- **SC-009**: Keyboard navigation in DialogList verified by 8 tests mapping to the 8 key binding groups: (1) Up/k moves cursor up with clamping, (2) Down/j moves cursor down with clamping, (3) PageUp jumps by visible lines, (4) PageDown jumps by visible lines, (5) Enter toggles selection, (6) Space toggles selection, (7) number shortcut jumps to Nth item when showNumbers=true, (8) character press jumps to matching item. "Identical to Python" means: same cursor position after same key sequence, same selection state after same toggle sequence.

## Assumptions & Dependencies

### Infrastructure Dependencies (from prior features)

The following are required from prior feature implementations. Each is feature-pinned to the feature that introduced it:

| Dependency | Namespace | Feature | Used By |
|-----------|-----------|---------|---------|
| `IContainer`, `IMagicContainer`, `AnyContainer` | Stroke.Layout.Containers | 029 | All widgets (implement IMagicContainer) |
| `Window` | Stroke.Layout.Containers | 029 | All widgets (inner rendering window) |
| `HSplit`, `VSplit` | Stroke.Layout.Containers | 029 | Frame, Box, Dialog, ProgressBar |
| `FloatContainer`, `Float` | Stroke.Layout.Containers | 029 | Shadow, ProgressBar |
| `ConditionalContainer` | Stroke.Layout.Containers | 029 | Frame (title switching) |
| `DynamicContainer` | Stroke.Layout.Containers | 029 | Frame, Dialog (runtime body changes) |
| `FormattedTextControl` | Stroke.Layout.Controls | 029 | Label, Button, DialogList |
| `BufferControl` | Stroke.Layout.Controls | 029 | TextArea |
| `Buffer`, `Document` | Stroke.Core | 007, 002 | TextArea |
| `ScrollbarMargin`, `NumberedMargin`, `ConditionalMargin` | Stroke.Layout.Margins | 029 | TextArea, DialogList |
| `PasswordProcessor`, `AppendAutoSuggestion`, `BeforeInput`, `ConditionalProcessor` | Stroke.Layout.Processors | 031 | TextArea |
| `DynamicCompleter` | Stroke.Completion | 012 | TextArea |
| `DynamicValidator` | Stroke.Validation | 009 | TextArea |
| `DynamicAutoSuggest` | Stroke.AutoSuggest | 005 | TextArea |
| `DynamicLexer` | Stroke.Lexers | 025 | TextArea |
| `Template` | Stroke.FormattedText | 015 | Frame (title rendering: `Template(" {} ").Format(this.Title)`) |
| `AnyFormattedText` | Stroke.FormattedText | 015 | Label, Frame, Dialog, DialogList |
| `IFilter`, `Condition`, `FilterOrBool`, `FilterUtils` | Stroke.Filters | 017 | TextArea, Frame, Dialog, DialogList |
| `KeyBindings` | Stroke.KeyBinding | 022 | Button, DialogList, Dialog |
| `WindowAlign` | Stroke.Layout | 029 | Button (`WindowAlign.Center`) |
| `Dimension`, `D` | Stroke.Layout | 016 | All widgets with size constraints |
| `UnicodeWidth` | Stroke.Utilities | 024 | Button (text centering), Label (width calculation) |
| `SearchToolbar` | Stroke.Widgets.Toolbars | 044 | TextArea (`searchField` parameter) |
| `FocusFunctions.FocusNext`, `FocusFunctions.FocusPrevious` | Stroke.KeyBinding.Bindings | 040 | Dialog (Tab/Shift-Tab and Left/Right button navigation) |

### Runtime Assumptions

- `SearchToolbar` (Feature 044) exposes a `Control` property returning `SearchBufferControl`, which TextArea extracts for search integration. TextArea's `searchField` parameter depends on this specific API surface.
- `DynamicContainer` accepts a `Func<AnyContainer>?` parameter (confirmed from source: `DynamicContainer.cs` line 31). Callers pass `() => this.Body` directly when `Body` is `AnyContainer`. No `.ToContainer()` conversion is needed.
- `Window` constructor currently accepts `Dimension?` for width/height. For dynamic dimensions (Label width calculation, ProgressBar weights), Window constructor overloads accepting `Func<Dimension?>` for width/height and `Func<string?>` for char MAY need to be added as a prerequisite. Research RT-3 identifies this need. If such overloads are not available, widgets will use the existing lambda-wrapping approach internally.
- The `IFilter`/`Condition`/`FilterOrBool` system supports runtime evaluation for conditional rendering and behavior gating.
- `StyleAndTextTuple` supports an optional `Action<MouseEvent>?` mouse handler as the third element, matching Python's 3-tuple `(style, text, handler)` pattern. Used by Button and DialogList for click handling.
