# Feature Specification: Choice Input

**Feature Branch**: `056-choice-input`
**Created**: 2026-02-03
**Status**: Draft
**Input**: User description: "Implement ChoiceInput class and choice function - a selection prompt for choosing among options using RadioList widget"
**Python PTK Reference**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/shortcuts/choice_input.py` (Python Prompt Toolkit v3.0+)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Select Option from List (Priority: P1)

A terminal application developer needs to prompt users to select one option from a predefined list of choices. The selection interface should display all options clearly and allow intuitive navigation.

**Why this priority**: This is the core functionality of the choice input feature. Without basic selection capability, the component has no value.

**Independent Test**: Can be fully tested by creating a choice prompt with 3+ options, navigating between them, and selecting one. Delivers immediate value for any CLI application requiring user selections.

**Acceptance Scenarios**:

1. **Given** a ChoiceInput with message "Select a dish:" and options [(pizza, "Pizza"), (salad, "Salad"), (sushi, "Sushi")], **When** the prompt displays, **Then** the message appears above the numbered options list with the first option highlighted using bold text (via `class:selected-option` style) [FR-001, FR-002, FR-003, FR-007]
2. **Given** a displayed choice prompt, **When** user presses Down arrow, **Then** the selection moves to the next option (or wraps to first if at last option) [FR-004]
3. **Given** a displayed choice prompt, **When** user presses Up arrow, **Then** the selection moves to the previous option (or wraps to last if at first option) [FR-004]
4. **Given** a displayed choice prompt with option 2 selected, **When** user presses Enter, **Then** the prompt returns the value associated with option 2 as the typed result T [FR-006]
5. **Given** a choice prompt with options numbered 1-5, **When** user presses key "3", **Then** option 3 becomes selected [FR-005]
6. **Given** a choice prompt with a single option, **When** the prompt displays, **Then** that option is selected and Enter returns its value [FR-002, FR-006]

**Traceability**: US1 → FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, FR-007

---

### User Story 2 - Cancel Selection (Priority: P2)

A user needs the ability to cancel a selection prompt without choosing any option, typically via Ctrl+C.

**Why this priority**: Cancellation is essential for user control but depends on the basic selection functionality being in place first.

**Independent Test**: Can be tested by displaying a choice prompt and pressing Ctrl+C. Delivers value by preventing users from being trapped in mandatory selections.

**Acceptance Scenarios**:

1. **Given** a choice prompt with interrupt enabled (default: `enableInterrupt=true`), **When** user presses Ctrl+C, **Then** the prompt raises `KeyboardInterrupt` exception (or the type specified by `interruptException` parameter) [FR-008]
2. **Given** a choice prompt with `enableInterrupt=false`, **When** user presses Ctrl+C, **Then** the key press is ignored and the prompt remains displayed awaiting selection [FR-008]
3. **Given** a choice prompt with `interruptException=typeof(OperationCanceledException)`, **When** user presses Ctrl+C, **Then** an `OperationCanceledException` is raised instead of `KeyboardInterrupt` [FR-008]
4. **Given** a user navigating a choice prompt (has pressed Up/Down arrows), **When** user then presses Ctrl+C, **Then** the navigation is abandoned and the interrupt exception is raised [FR-008]

**Traceability**: US2 → FR-008

**Return Type Clarification**:
- **On successful selection** (Enter pressed): Returns `T` (the value associated with the selected option)
- **On cancellation** (Ctrl+C with `enableInterrupt=true`): Throws exception (no return value)
- **On cancellation** (Ctrl+C with `enableInterrupt=false`): No effect, prompt continues

---

### User Story 3 - Visual Customization (Priority: P3)

A developer wants to customize the appearance of the choice prompt to match their application's visual style, including borders, colors, and selection symbols.

**Why this priority**: Customization enhances user experience but is not required for basic functionality.

**Independent Test**: Can be tested by creating choice prompts with various style configurations and visually verifying the output matches expectations.

**Acceptance Scenarios**:

1. **Given** a choice prompt with `showFrame=true`, **When** the prompt displays, **Then** a border frame surrounds the entire selection area using `class:frame.border` style (default: `#884444` brownish-red) [FR-010, FR-014]
2. **Given** a choice prompt with `symbol="*"`, **When** an option is selected, **Then** the `*` symbol appears before the selected option instead of the default `>` [FR-013]
3. **Given** a choice prompt with custom `style` containing `["selected-option"] = "bold underline"`, **When** the prompt displays, **Then** the selected option uses bold underline styling instead of default bold-only [FR-014]
4. **Given** a choice prompt with `bottomToolbar="Use ↑↓ to navigate"`, **When** the prompt displays with known renderer height, **Then** the toolbar text appears at the bottom of the screen with `class:bottom-toolbar.text` style [FR-011]

**Traceability**: US3 → FR-010, FR-011, FR-013, FR-014

---

### User Story 4 - Mouse Interaction (Priority: P3)

A user wants to click on an option with their mouse to select it instead of using keyboard navigation.

**Why this priority**: Mouse support is a convenience feature that enhances usability but keyboard navigation covers the core use case.

**Independent Test**: Can be tested by enabling mouse support and clicking on different options to verify selection changes.

**Acceptance Scenarios**:

1. **Given** a choice prompt with `mouseSupport=true` on a terminal that supports mouse events, **When** user clicks on option 2, **Then** option 2 becomes selected [FR-009]
2. **Given** a choice prompt with `mouseSupport=false` (default), **When** user clicks anywhere, **Then** no selection change occurs [FR-009]
3. **Given** a choice prompt with `mouseSupport=true` on a terminal that does NOT support mouse events, **When** user attempts to click, **Then** the terminal's default behavior occurs (graceful degradation) [FR-009, NFR-002]

**Traceability**: US4 → FR-009

---

### User Story 5 - Background Suspension (Priority: P4)

On Unix systems, a user wants to suspend the application to background using Ctrl+Z while a choice prompt is displayed.

**Why this priority**: This is a platform-specific feature that only applies to Unix systems and is rarely used in modern workflows.

**Independent Test**: Can be tested on Unix by enabling suspend and pressing Ctrl+Z to verify the process suspends.

**Acceptance Scenarios**:

1. **Given** a choice prompt on Unix with `enableSuspend=true`, **When** user presses Ctrl+Z, **Then** the process suspends to background (via SIGTSTP) [FR-012]
2. **Given** a choice prompt with `enableSuspend=false` (default), **When** user presses Ctrl+Z, **Then** the key press is ignored [FR-012]
3. **Given** a choice prompt on Windows with `enableSuspend=true`, **When** user presses Ctrl+Z, **Then** the key press is ignored (suspend not supported on Windows; no error raised) [FR-012, XP-001]

**Traceability**: US5 → FR-012

---

### User Story 6 - Async Application Integration (Priority: P2)

A developer building an async/await application needs to display choice prompts without blocking the main thread.

**Why this priority**: Modern .NET applications extensively use async patterns; supporting them is essential for framework adoption.

**Independent Test**: Can be tested by awaiting `PromptAsync()` or `Dialogs.ChoiceAsync()` in an async method.

**Acceptance Scenarios**:

1. **Given** an async application, **When** calling `await choiceInput.PromptAsync()`, **Then** the prompt displays and the method returns the selected value without blocking [FR-016]
2. **Given** an async application, **When** calling `await Dialogs.ChoiceAsync(...)`, **Then** the convenience method creates and runs a ChoiceInput asynchronously [FR-016, FR-017]

**Traceability**: US6 → FR-016, FR-017

---

### Edge Cases

- **Empty options list**: System MUST throw `ArgumentException` during construction with message "Options cannot be empty" [FR-018]
- **Null options parameter**: System MUST throw `ArgumentNullException` during construction [FR-018]
- **Default value doesn't match any option**: First option is selected (no error) [FR-007]
- **Navigation wrap at boundaries**: Down at last option wraps to first; Up at first option wraps to last [FR-004]
- **Terminal resize during prompt**: Layout adapts to new dimensions on next render cycle [NFR-004]
- **Formatted text labels with newlines**: Multi-line labels render correctly within their option row
- **Single-option list**: Displays the single option as selected; Enter returns its value
- **Very long option labels**: Labels exceeding terminal width are truncated (not wrapped) by the underlying RadioList widget
- **Maximum options**: No hard limit; performance may degrade with >1000 options due to rendering overhead
- **Options with control characters**: Rendered as-is by FormattedText system; may produce unexpected visual results
- **Options with emoji**: Rendered using Unicode width calculations; require terminal emoji support
- **Rapid key repeat (holding Down arrow)**: Each key event processed sequentially; selection moves one step per event
- **Multiple simultaneous key presses**: Processed in the order received by the input system; no special handling

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display a message/prompt text above the list of options using a Label widget
- **FR-002**: System MUST display options as a numbered list where each option shows its 1-based number prefix and label
- **FR-003**: System MUST highlight the currently selected option using `bold` text style (via `class:selected-option`) which is visually distinct from unselected options that use the default style
- **FR-004**: System MUST allow navigation between options using Up/Down arrow keys (and `k`/`j` for vi-style) with wrap-around behavior at list boundaries
- **FR-005**: System MUST allow direct selection of options by pressing number keys 1-9 for the first 9 options; options 10+ require arrow navigation
- **FR-006**: System MUST accept the current selection when Enter is pressed and return the associated value of type `T`
- **FR-007**: System MUST support a default value that is pre-selected when the prompt displays; if the default value doesn't match any option, the first option is selected
- **FR-008**: System MUST support interrupt handling via Ctrl+C (and SIGINT signal) that raises a configurable exception (default: `KeyboardInterrupt`), controlled by `enableInterrupt` parameter
- **FR-009**: System MUST support optional mouse interaction for clicking to select options, controlled by `mouseSupport` parameter
- **FR-010**: System MUST support optional visual frame around the selection area, controlled by `showFrame` parameter
- **FR-011**: System MUST support optional bottom toolbar for displaying help text, visible only when `bottomToolbar` is set, IsDone is false, and RendererHeightIsKnown is true
- **FR-012**: System MUST support optional suspend-to-background via Ctrl+Z on Unix platforms (no effect on Windows), controlled by `enableSuspend` parameter and `PlatformUtils.SuspendToBackgroundSupported`
- **FR-013**: System MUST support custom selection symbol (default: `">"`) displayed before the selected option
- **FR-014**: System MUST support custom styling via `IStyle` parameter; when null, use default style with `frame.border=#884444` and `selected-option=bold`
- **FR-015**: System MUST support formatted text (AnyFormattedText) for message, option labels, and toolbar—not limited to plain strings
- **FR-016**: System MUST provide both synchronous `Prompt()` and asynchronous `PromptAsync()` methods returning type `T`
- **FR-017**: System MUST provide convenience functions `Dialogs.Choice<T>()` and `Dialogs.ChoiceAsync<T>()` that create and run a ChoiceInput in one call
- **FR-018**: System MUST validate constructor parameters: throw `ArgumentNullException` if options is null; throw `ArgumentException` if options is empty

### Non-Functional Requirements

- **NFR-001**: Thread safety: ChoiceInput configuration MUST be thread-safe (immutable after construction per Constitution XI)
- **NFR-002**: Keyboard response latency MUST be <16ms (60fps frame time) for navigation operations
- **NFR-003**: Memory: Option list storage MUST use O(n) memory where n is the number of options
- **NFR-004**: Rendering MUST adapt to terminal resize events within one render cycle
- **NFR-005**: Accessibility: Selection state MUST be communicated through both visual styling AND semantic structure (list position) for screen reader compatibility

### Cross-Platform Requirements

- **XP-001**: Ctrl+Z suspend MUST only function on Unix platforms where `PlatformUtils.SuspendToBackgroundSupported` returns true; on Windows, the key binding is registered but has no effect (no error)
- **XP-002**: Mouse support MUST gracefully degrade on terminals that don't support mouse events (no crash, keyboard navigation remains available)
- **XP-003**: Key input MUST accept both ANSI escape sequences (VT100 terminals) and Windows console virtual key codes via the platform input abstraction layer

### Key Entities

- **ChoiceInput<T>**: Generic sealed class that manages the choice selection prompt. Holds immutable configuration (message, options, style settings) and creates the Application for execution. Thread-safe by design (immutable after construction).
- **Option**: A (T Value, AnyFormattedText Label) tuple where the value is returned when selected and the label is displayed to the user
- **Selection State**: Current position in the options list, tracked internally by the underlying RadioList widget using its SelectedIndex property
- **KeyboardInterrupt**: Default exception type thrown on Ctrl+C interrupt; user can override via `interruptException` parameter

### Default Style Definition

The default style (when `style` parameter is null) MUST match Python PTK exactly:

```csharp
Style.FromDict(new Dictionary<string, string>
{
    ["frame.border"] = "#884444",    // Brownish-red border
    ["selected-option"] = "bold"      // Bold text for selected item
})
```

### Traceability Matrix

| Requirement | User Stories | Acceptance Scenarios |
|-------------|--------------|---------------------|
| FR-001 | US1 | US1-1 |
| FR-002 | US1 | US1-1, US1-6 |
| FR-003 | US1 | US1-1 |
| FR-004 | US1 | US1-2, US1-3 |
| FR-005 | US1 | US1-5 |
| FR-006 | US1 | US1-4, US1-6 |
| FR-007 | US1 | US1-1 |
| FR-008 | US2 | US2-1, US2-2, US2-3, US2-4 |
| FR-009 | US4 | US4-1, US4-2, US4-3 |
| FR-010 | US3 | US3-1 |
| FR-011 | US3 | US3-4 |
| FR-012 | US5 | US5-1, US5-2, US5-3 |
| FR-013 | US3 | US3-2 |
| FR-014 | US3 | US3-1, US3-3 |
| FR-015 | US1, US3 | US1-1, US3-4 |
| FR-016 | US6 | US6-1 |
| FR-017 | US6 | US6-2 |
| FR-018 | - | Edge Cases |

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: User interaction timing: Given a practiced user with a choice prompt displaying 10 options, the user can navigate to a specific option and press Enter within 5 seconds. Measured via TUI automation tests timing the sequence: launch → navigate to option 7 → press Enter.
- **SC-002**: Keyboard response latency: Navigation key presses (Up/Down/1-9) MUST update the visual selection within 16ms (one 60fps frame). Measured via TUI automation capturing timestamps between key send and screen update.
- **SC-003**: Terminal size compatibility: Choice prompt MUST render correctly (all options visible, selection indicator visible, no overlapping text) at terminal sizes 40x10, 80x24, 120x40, and 200x50. Verified via TUI screenshot comparison at each size.
- **SC-004**: All acceptance scenarios pass verification testing using TUI automation
- **SC-005**: Feature achieves 80% or higher test coverage. Critical paths that MUST be covered:
  - Constructor validation (null/empty options)
  - Navigation (Up/Down/wrap)
  - Number key selection (1-9)
  - Enter to confirm
  - Ctrl+C interrupt (enabled/disabled)
  - Default value handling (match/no match)
  - Style application
- **SC-006**: API fidelity verification: All public members of `ChoiceInput<T>` MUST have 1:1 correspondence with Python PTK's `ChoiceInput` class. Verified by automated comparison of public API surface against Python module introspection.

## Assumptions

All assumptions have been validated against the current codebase (Feature 055 complete):

| # | Assumption | Validation | Feature Reference |
|---|------------|------------|-------------------|
| A1 | RadioList widget is fully implemented | ✅ Verified: `src/Stroke/Widgets/Lists/RadioList.cs` exists with generic type support, showNumbers, selectCharacter parameters | Feature 045 |
| A2 | Application.Exit(result, exception) lifecycle methods exist | ✅ Verified: `src/Stroke/Application/Application.cs` supports typed results and exception exit | Feature 030/031 |
| A3 | Filter system provides IsDone and RendererHeightIsKnown | ✅ Verified: `src/Stroke/Filters/AppFilters.cs` exports both filters | Feature 017/032 |
| A4 | HSplit, ConditionalContainer, Box, Frame, Label containers available | ✅ Verified: All exist in `src/Stroke/Layout/Containers/` and `src/Stroke/Widgets/Base/` | Feature 029/045 |
| A5 | KeyBindings system supports merge_key_bindings | ✅ Verified: `MergedKeyBindings` class in `src/Stroke/KeyBinding/` | Feature 022 |
| A6 | Style.FromDict() method exists | ✅ Verified: `src/Stroke/Styles/Style.cs` has FromDict static method | Feature 018 |
| A7 | DynamicKeyBindings proxy type available | ✅ Verified: `src/Stroke/KeyBinding/Proxies/DynamicKeyBindings.cs` | Feature 022 |
| A8 | PlatformUtils.SuspendToBackgroundSupported exists | ✅ Verified: `src/Stroke/Core/PlatformUtils.cs` has this property | Feature 024 |

## Python → C# API Mapping

### Naming Transformation Rules

| Python Convention | C# Convention | Example |
|-------------------|---------------|---------|
| `snake_case` methods | `PascalCase` methods | `prompt_async` → `PromptAsync` |
| `snake_case` parameters | `camelCase` parameters | `mouse_support` → `mouseSupport` |
| `snake_case` properties | `PascalCase` properties | `enable_interrupt` → `EnableInterrupt` |
| `_private` methods | `private` access modifier | `_create_application` → `private CreateApplication` |
| `module.function()` | `Class.Method()` | `choice()` → `Dialogs.Choice<T>()` |

### Public API Mapping

| Python PTK Member | C# Equivalent | Notes |
|-------------------|---------------|-------|
| `ChoiceInput` class | `ChoiceInput<T>` class | Generic type parameter added |
| `__init__` | Constructor | Same parameters |
| `prompt()` | `Prompt()` | Returns `T` |
| `prompt_async()` | `PromptAsync()` | Returns `Task<T>` |
| `choice()` function | `Dialogs.Choice<T>()` | Static method |
| (none in Python) | `Dialogs.ChoiceAsync<T>()` | **Intentional addition** for async convenience |

### Intentional Deviations

| Deviation | Rationale |
|-----------|-----------|
| Added `Dialogs.ChoiceAsync<T>()` | .NET convention to provide async versions of convenience methods; Python typically uses same method with async context |
| `KeyboardInterrupt` inherits from `Exception` | C# exception hierarchy differs from Python; cannot inherit from `BaseException` directly |
| Options type is `IReadOnlyList` not `Sequence` | C# idiom for read-only collection parameters |
| Properties have getters only | Immutability by default per Constitution II |

## Clarifications

### Session 2026-02-03

- Q: When navigating past list boundaries (Down at last option, Up at first option), should selection wrap or stop? → A: Wrap navigation (Down at last wraps to first, Up at first wraps to last)
- Q: For lists with more than 9 options, should number keys (1-9) still work for first 9 items? → A: Yes, number keys 1-9 select first 9 options; options 10+ require arrow navigation
- Q: What exception type is default for Ctrl+C interrupt? → A: `KeyboardInterrupt` (mirrors Python's `KeyboardInterrupt`)
- Q: What happens to return value on cancellation? → A: Exception is thrown; no return value. Caller must catch exception.
- Q: How does Ctrl+Z behave on Windows? → A: Key press is ignored silently (no error, no action)
