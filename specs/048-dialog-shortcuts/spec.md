# Feature Specification: Dialog Shortcut Functions

**Feature Branch**: `048-dialog-shortcuts`
**Created**: 2026-02-01
**Status**: Draft
**Input**: User description: "Implement pre-built dialog shortcut functions for common interactive patterns: yes/no confirmation, button selection, text input, message display, radio list selection, checkbox list selection, and progress dialogs."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Confirm a Destructive Action (Priority: P1)

A developer building a CLI tool needs to ask the user for Yes/No confirmation before performing a destructive operation (e.g., deleting files, overwriting data). They call a single function that displays a titled dialog with a message and two buttons ("Yes" and "No"), and receive a boolean result indicating the user's choice.

**Why this priority**: Confirmation dialogs are the most common dialog pattern across all interactive applications. They protect users from accidental destructive actions and are the simplest dialog to implement, establishing the foundation (`_CreateApp` helper, key bindings, focus management) that all other dialog types build upon.

**Independent Test**: Can be fully tested by calling the function with a title and message text, verifying the returned application contains a dialog with correct structure (title, label body, two buttons), and that button handlers call exit with the correct boolean values.

**Acceptance Scenarios**:

1. **Given** a developer calls the yes/no dialog function with title "Confirm" and text "Delete?", **When** the dialog is created, **Then** an application is returned containing a dialog with the title "Confirm", a label showing "Delete?", and two buttons labeled "Yes" and "No"
2. **Given** a yes/no dialog is displayed, **When** the user activates the "Yes" button, **Then** the application exits with result `true`
3. **Given** a yes/no dialog is displayed, **When** the user activates the "No" button, **Then** the application exits with result `false`
4. **Given** a yes/no dialog is displayed, **When** the user presses Tab, **Then** focus moves to the next focusable element in the dialog

---

### User Story 2 - Display an Informational Message (Priority: P1)

A developer needs to show a simple message to the user (e.g., an operation result, an informational notice) and wait for them to acknowledge it by pressing a single "Ok" button. They call a message dialog function and await its completion.

**Why this priority**: Message dialogs are equally fundamental to confirmation dialogs. They serve as the simplest dialog variant (single button, no return value), making them ideal for testing the core dialog-to-application pipeline.

**Independent Test**: Can be tested by calling the message dialog function with a title and text, verifying the application structure contains a single "Ok" button, and that pressing Ok exits the application.

**Acceptance Scenarios**:

1. **Given** a developer calls the message dialog function with title "Info" and text "Operation complete", **When** the dialog is created, **Then** an application is returned with a dialog containing the title, message, and a single "Ok" button
2. **Given** a message dialog is displayed, **When** the user activates the "Ok" button, **Then** the application exits
3. **Given** a message dialog is displayed, **When** the user presses Tab, **Then** focus cycles through the dialog's focusable elements

---

### User Story 3 - Collect Text Input (Priority: P2)

A developer needs to prompt the user for a text value (e.g., a file name, a search query, a configuration value). They call an input dialog function that displays a text field within a dialog, with optional validation and auto-completion. The function returns the entered text, or null if the user cancels.

**Why this priority**: Text input dialogs add significant interactivity beyond simple button clicks. They involve text areas, Enter-to-submit behavior (focus transfer to OK button), optional validation display, optional password masking, and optional completion — making them the most complex single-value dialog.

**Independent Test**: Can be tested by calling the input dialog function with title/text/default parameters, verifying the structure contains a text area and validation toolbar, and that OK returns the text content while Cancel returns null.

**Acceptance Scenarios**:

1. **Given** a developer calls the input dialog with default text "hello", **When** the dialog is created, **Then** the text area contains "hello"
2. **Given** an input dialog is displayed, **When** the user types text and activates OK, **Then** the application exits with the entered text
3. **Given** an input dialog is displayed, **When** the user activates Cancel, **Then** the application exits with null
4. **Given** an input dialog with a validator, **When** the user enters invalid text, **Then** the validation toolbar displays an error message
5. **Given** an input dialog with password mode enabled, **When** the user types, **Then** the input is masked (not displayed as plaintext)
6. **Given** an input dialog is displayed, **When** the user presses Enter in the text field, **Then** focus moves to the OK button (accept handler returns `true` to keep the text in the buffer)
7. **Given** an input dialog is displayed, **When** the user presses Tab, **Then** focus moves between the text field, OK button, and Cancel button

---

### User Story 4 - Choose from Custom Buttons (Priority: P2)

A developer needs to present the user with a set of custom action choices (e.g., "Save", "Save As", "Discard", "Cancel"), each associated with a different return value. They call a button dialog function that displays the choices as buttons and returns the value associated with the selected button.

**Why this priority**: Button dialogs extend the yes/no pattern to arbitrary choices. They introduce generic typing (each button maps to a typed value) and variable button counts, adding flexibility without significant new complexity.

**Independent Test**: Can be tested by calling the button dialog with a list of (text, value) tuples, verifying each button is created with the correct text, and that activating a button exits with the corresponding value.

**Acceptance Scenarios**:

1. **Given** a developer calls the button dialog with buttons [("Save", 1), ("Cancel", 2)], **When** the dialog is created, **Then** two buttons labeled "Save" and "Cancel" appear
2. **Given** a button dialog is displayed, **When** the user activates the "Save" button, **Then** the application exits with value 1
3. **Given** a button dialog is displayed, **When** the user activates the "Cancel" button, **Then** the application exits with value 2
4. **Given** a button dialog is displayed, **When** the user presses Tab, **Then** focus cycles through the dialog's focusable elements

---

### User Story 5 - Select from a Radio List (Priority: P2)

A developer needs the user to select exactly one option from a list of choices (e.g., a color theme, a configuration preset). They call a radio list dialog function that displays a scrollable single-selection list with OK and Cancel buttons. OK returns the selected value; Cancel returns a default/null value.

**Why this priority**: Radio list dialogs combine the Dialog widget with the RadioList widget, demonstrating composite widget usage. Single-selection is a common interaction pattern for settings and configuration flows.

**Independent Test**: Can be tested by calling the radio list dialog with a set of values, verifying the RadioList widget is embedded in the dialog body, and that OK returns the current selection while Cancel returns default.

**Acceptance Scenarios**:

1. **Given** a developer calls the radio list dialog with values [("red", "Red"), ("blue", "Blue")], **When** the dialog is created, **Then** a radio list with two options appears
2. **Given** a radio list dialog with default value "blue", **When** the dialog is created, **Then** "blue" is pre-selected
3. **Given** a radio list dialog is displayed, **When** the user selects "red" and activates OK, **Then** the application exits with "red"
4. **Given** a radio list dialog is displayed, **When** the user activates Cancel, **Then** the application exits with a default/null value
5. **Given** a radio list dialog is displayed, **When** the user presses Tab, **Then** focus moves between the radio list and the button row

---

### User Story 6 - Select Multiple from a Checkbox List (Priority: P3)

A developer needs the user to select zero or more options from a list (e.g., features to enable, files to process). They call a checkbox list dialog that displays a multi-selection list with OK and Cancel buttons. OK returns the list of selected values; Cancel returns null.

**Why this priority**: Checkbox list dialogs mirror radio list dialogs but for multi-selection. They're slightly less common than single-selection and add the concept of returning a collection rather than a single value.

**Independent Test**: Can be tested by calling the checkbox list dialog with values and optional default selections, verifying CheckboxList is embedded, and that OK returns the current selections while Cancel returns null.

**Acceptance Scenarios**:

1. **Given** a developer calls the checkbox list dialog with three values and two default selections, **When** the dialog is created, **Then** the two defaults are pre-checked
2. **Given** a checkbox list dialog is displayed, **When** the user toggles selections and activates OK, **Then** the application exits with the list of currently selected values
3. **Given** a checkbox list dialog is displayed, **When** the user activates Cancel, **Then** the application exits with null
4. **Given** a checkbox list dialog is displayed, **When** the user presses Tab, **Then** focus moves between the checkbox list and the button row

---

### User Story 7 - Show Progress for a Long-Running Task (Priority: P3)

A developer needs to run a long-running background task while showing the user a progress bar and scrollable log output. They call a progress dialog function, providing a callback that receives `setPercentage` and `logText` functions. The dialog displays a progress bar and text area that update as the callback runs, and the dialog closes automatically when the callback completes.

**Why this priority**: Progress dialogs are the most complex dialog type, involving background execution, thread-safe UI updates, and automatic dismissal. They're used less frequently than other dialog types but are critical for operations like file processing, network transfers, or batch operations.

**Independent Test**: Can be tested by calling the progress dialog with a callback that sets percentage values and logs text, verifying the progress bar widget and text area are embedded in the dialog, and that the application exits when the callback completes.

**Acceptance Scenarios**:

1. **Given** a developer calls the progress dialog with a callback, **When** the dialog is created, **Then** the dialog contains a progress bar widget and a text area for log output
2. **Given** a progress dialog is running, **When** the callback calls setPercentage(50), **Then** the progress bar updates to 50%
3. **Given** a progress dialog is running, **When** the callback calls logText("Step 1 done"), **Then** the text appears in the text area
4. **Given** a progress dialog is running, **When** the callback completes (returns), **Then** the application exits automatically
5. **Given** a progress dialog is running, **When** the callback throws an exception, **Then** the application still exits (via `finally` block) to ensure terminal recovery
6. **Given** a progress dialog is running, **When** the callback calls `setPercentage` and `logText` concurrently from multiple threads, **Then** updates are applied without race conditions or UI corruption

---

### Edge Cases

- What happens when a button dialog is called with an empty button list? The dialog should still render with just the body text and no buttons.
- What happens when a radio list dialog is called with no values? An empty list should be displayed; OK returns the default value.
- What happens when a checkbox list dialog is called with no values? An empty list should be displayed; OK returns an empty list.
- What happens when a progress dialog callback throws an exception? The application should still exit (via finally block) to avoid leaving the terminal in a broken state.
- What happens when the progress dialog callback calls logText from a background thread? The update must be marshaled to the UI thread via the event loop's thread-safe callback mechanism.
- What happens when custom button text is provided (e.g., yesText="Confirm")? The button labels should display the custom text.
- What happens when the progress dialog's `runCallback` parameter is null? The background task executes a no-op (matching Python's `lambda *a: None` default) and the dialog immediately exits. Implementation: if `runCallback` is null, substitute a no-op `Action` before passing to the background task.
- What happens when a progress dialog callback calls both `setPercentage` and `logText` concurrently from multiple threads? Both operations are independently thread-safe: `setPercentage` writes to a Lock-protected `ProgressBar.Percentage` property + calls `Invalidate()` (atomic via `Interlocked`); `logText` marshals `Buffer.InsertText()` to the async context via `_actionChannel.Writer.TryWrite()`.
- What happens when InputDialog's `default_` parameter contains multi-line text but the TextArea is configured as single-line (`multiline=False`)? The text is inserted as-is into the TextArea; the TextArea's single-line mode prevents further newline insertion by the user. This matches Python's behavior where the `default` parameter is passed directly to `TextArea(text=default)`.
- What happens when a ProgressDialog is created (factory method called) but `RunAsync()` is never invoked? The `logText` callback gracefully handles the null `_actionChannel` by checking for null before writing, matching Python's `if loop is not None:` guard. No exception is thrown; the log call is silently dropped.
- What happens when a user clicks a button or list item with the mouse? All dialogs support mouse interaction (`mouseSupport: true` in CreateApp). Button clicks trigger the button's handler, list item clicks select the item, matching the standard widget mouse behavior.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a yes/no dialog function that returns a boolean based on user selection
- **FR-002**: System MUST provide a message dialog function that displays text and waits for user acknowledgment
- **FR-003**: System MUST provide an input dialog function that collects text input with optional validation, completion, and password masking
- **FR-004**: System MUST provide a button dialog function that displays arbitrary button choices and returns the value associated with the selected button
- **FR-005**: System MUST provide a radio list dialog function for single-selection from a list of options
- **FR-006**: System MUST provide a checkbox list dialog function for multi-selection from a list of options
- **FR-007**: System MUST provide a progress dialog function that displays a progress bar and log text area, updated via callback functions
- **FR-008**: All dialog functions MUST return an application object that can be run to display the dialog and obtain the result
- **FR-009**: All dialogs MUST support Tab/Shift-Tab navigation between focusable elements
- **FR-010**: All dialogs with multiple buttons MUST support Left/Right arrow key navigation between buttons
- **FR-011**: All dialogs MUST render with a background overlay (withBackground=true)
- **FR-012**: All dialogs MUST merge default key bindings with custom dialog key bindings
- **FR-013**: All dialogs MUST support mouse interaction
- **FR-014**: All dialogs MUST run in full-screen mode
- **FR-015**: All dialog functions MUST accept an optional style parameter for custom styling
- **FR-016**: The input dialog MUST transfer focus to the OK button when Enter is pressed in the text field, via an `AcceptHandler` (`Func<Buffer, bool>`) that returns `true` to keep the text in the buffer
- **FR-017**: The progress dialog callback MUST execute in a background thread, with UI updates marshaled to the main thread
- **FR-018**: The progress dialog MUST automatically exit when the callback completes, even if the callback throws an exception
- **FR-019**: Cancel actions in input, radio list, and checkbox list dialogs MUST exit the application with a null/default result
- **FR-020**: All dialog functions MUST faithfully port the corresponding Python Prompt Toolkit functions from `shortcuts/dialogs.py`
- **FR-021**: System MUST provide async convenience methods (suffixed with `Async`) for each dialog function that create and run the application in one call, returning `Task<T>` matching the factory method's result type
- **FR-022**: The progress dialog's `logText` callback MUST check for null `_actionChannel` before writing, gracefully dropping the call if the application is not yet running (matching Python's `if loop is not None:` guard)
- **FR-023**: The progress dialog MUST schedule its background task via `PreRunCallables` to ensure `CreateBackgroundTask` is called after the application's background task infrastructure is initialized during `RunAsync`
- **FR-024**: All public methods on the `Dialogs` class MUST include XML documentation comments describing parameters, return values, and behavior

### Key Entities

- **Dialog Shortcut Function**: A static factory method that composes widget primitives (Dialog, Label, Button, TextArea, RadioList, CheckboxList, ProgressBar) into a ready-to-run application. Each function encapsulates layout construction, key binding setup, and result handling.
- **CreateApp Helper**: A shared private function that wraps a dialog container in an Application with merged key bindings (default + Tab/Shift-Tab focus), mouse support, optional styling, and full-screen mode.

### Port Mapping Reference

#### Symbol Mapping (9 symbols)

| Python Symbol | C# Equivalent | Visibility | Return Type |
|--------------|---------------|------------|-------------|
| `yes_no_dialog()` | `Dialogs.YesNoDialog()` | public static | `Application<bool>` |
| `button_dialog()` | `Dialogs.ButtonDialog<T>()` | public static | `Application<T>` |
| `input_dialog()` | `Dialogs.InputDialog()` | public static | `Application<string?>` |
| `message_dialog()` | `Dialogs.MessageDialog()` | public static | `Application<object?>` |
| `radiolist_dialog()` | `Dialogs.RadioListDialog<T>()` | public static | `Application<T?>` |
| `checkboxlist_dialog()` | `Dialogs.CheckboxListDialog<T>()` | public static | `Application<IReadOnlyList<T>?>` |
| `progress_dialog()` | `Dialogs.ProgressDialog()` | public static | `Application<object?>` |
| `_create_app()` | `Dialogs.CreateApp<T>()` | private static | `Application<T>` |
| `_return_none()` | `Dialogs.ReturnNone()` | private static | `void` |

#### Parameter Name Transformations

All parameter names follow the standard `snake_case` → `camelCase` convention:

| Python Parameter | C# Parameter | Used In |
|-----------------|-------------|---------|
| `yes_text` | `yesText` | YesNoDialog |
| `no_text` | `noText` | YesNoDialog |
| `ok_text` | `okText` | MessageDialog, InputDialog, RadioListDialog, CheckboxListDialog |
| `cancel_text` | `cancelText` | InputDialog, RadioListDialog, CheckboxListDialog |
| `run_callback` | `runCallback` | ProgressDialog |
| `default` | `default_` | InputDialog, RadioListDialog (trailing underscore avoids C# keyword collision) |
| `default_values` | `defaultValues` | CheckboxListDialog |

#### Default Value Casing

Note the intentional casing differences between dialog types, matching the Python source exactly:

| Dialog | Parameter | Default Value |
|--------|-----------|--------------|
| MessageDialog | `okText` | `"Ok"` (capital O, lowercase k) |
| InputDialog | `okText` | `"OK"` (both uppercase) |
| RadioListDialog | `okText` | `"Ok"` (capital O, lowercase k) |
| CheckboxListDialog | `okText` | `"Ok"` (capital O, lowercase k) |

#### Return Type Nullability

Python's type annotations do not reflect cancel-returns-None behavior. C# contracts add explicit nullability for type safety:

| Dialog | Python Return | C# Return | Rationale |
|--------|--------------|-----------|-----------|
| YesNoDialog | `Application[bool]` | `Application<bool>` | No cancel — always returns true or false |
| MessageDialog | `Application[None]` | `Application<object?>` | C# has no `Application<void>`; `object?` maps to Python's `None` |
| InputDialog | `Application[str]` | `Application<string?>` | Cancel returns null (Python returns None) |
| ButtonDialog | `Application[_T]` | `Application<T>` | No cancel button — value always selected |
| RadioListDialog | `Application[_T]` | `Application<T?>` | Cancel returns default/null |
| CheckboxListDialog | `Application[list[_T]]` | `Application<IReadOnlyList<T>?>` | Cancel returns null |
| ProgressDialog | `Application[None]` | `Application<object?>` | Same as MessageDialog |

#### Tuple Element Ordering

Button parameters use different tuple orderings matching the Python source:

| Dialog | Python Type | C# Type | Element Order |
|--------|-----------|---------|---------------|
| ButtonDialog | `list[tuple[str, _T]]` | `IReadOnlyList<(string Text, T Value)>` | Text first, value second |
| RadioListDialog | `Sequence[tuple[_T, AnyFormattedText]]` | `IReadOnlyList<(T Value, AnyFormattedText Label)>` | Value first, label second |
| CheckboxListDialog | `Sequence[tuple[_T, AnyFormattedText]]` | `IReadOnlyList<(T Value, AnyFormattedText Label)>` | Value first, label second |

#### Generic Type Constraints

`ButtonDialog<T>`, `RadioListDialog<T>`, and `CheckboxListDialog<T>` use unconstrained generic type parameters, matching Python's `TypeVar("_T")`. No `where T : class` constraint is applied.

**Nullability note**: `RadioListDialog<T>` returns `Application<T?>`. For reference types, `T?` is nullable. For value types, `T?` becomes `Nullable<T>`. This means `RadioListDialog<int>` returns `Application<int?>` — the caller can check `HasValue` on cancel. This is a deliberate C# adaptation that improves type safety over Python's untyped `None` return. The api-mapping.md specifies `Task<T>` for `RadioListDialogAsync`, but the correct signature is `Task<T?>` to reflect the cancel-returns-null semantic. **This spec takes precedence** for the nullable return type.

### Behavioral Specification

#### CreateApp Helper Behavior

The `CreateApp<T>` private helper performs the following (matching Python's `_create_app`):

1. Creates a `KeyBindings` instance with Tab → `FocusFunctions.FocusNext` and Shift-Tab → `FocusFunctions.FocusPrevious`
2. Merges these bindings with `DefaultKeyBindings.Load()` via `MergedKeyBindings`
3. Constructs `Application<T>` with: `Layout(dialog)`, merged key bindings, `mouseSupport: true`, optional style, `fullScreen: true`

**Dual Tab Binding Note**: The Dialog widget already has Tab/Shift-Tab bindings with a `~HasCompletions` filter (to allow Tab to trigger completion when available). The CreateApp helper adds ADDITIONAL unconditional Tab/Shift-Tab bindings. This matches the Python source where both exist — the Dialog's filtered bindings take precedence when completions are available, and the CreateApp's unconditional bindings serve as a fallback.

#### ReturnNone Cancel Handler

The `ReturnNone` private helper calls `AppContext.GetApp().Exit()` with no arguments. This exits the current application with `default(T)`:

- For `Application<object?>`: exits with `null`
- For `Application<string?>`: exits with `null`
- For `Application<T?>`: exits with `default(T?)` = `null` for reference types, `null` for `Nullable<T>`
- For `Application<IReadOnlyList<T>?>`: exits with `null`

This uses `AppContext.GetApp()` (matching Python's `get_app()`) rather than capturing the specific app instance, matching the Python pattern exactly.

**Type coercion mechanism**: `AppContext.GetApp()` returns `Application<object?>` (untyped). Calling `.Exit()` on this stores `null` into the `TaskCompletionSource<object?>`. However, the *actual* running application (e.g., `Application<string?>`) has its own `TaskCompletionSource<string?>` — the exit value flows through the application's internal result-setting mechanism, not through the return type of `AppContext.GetApp()`. The untyped wrapper delegates to the real typed application instance. No boxing/coercion issues arise because `default(T)` is used by the concrete `Application<T>.Exit()` method.

#### InputDialog Accept Handler

When the user presses Enter in the InputDialog's TextArea:

1. The `AcceptHandler` (`Func<Buffer, bool>`) is invoked
2. The handler calls `AppContext.GetApp().Layout.Focus(okButton.Window)` to transfer focus to the OK button
3. The handler returns `true` to keep the text in the buffer (not clear it)
4. The user can then press Enter again on the focused OK button to confirm, or Tab to Cancel

#### Label Constructor Defaults

All dialog functions create `Label(text, dontExtendHeight: true)`. In the Stroke implementation, `Label.DontExtendHeight` already defaults to `true`, so the explicit parameter is technically redundant but is included for clarity and to match the Python source's explicit `dont_extend_height=True`.

#### withBackground Consistency

All 7 dialog functions pass `withBackground: true` to the Dialog widget constructor. This wraps the dialog's Frame+Shadow in a Box with `style: "class:dialog"`, creating a full-screen overlay background behind the dialog.

### Thread Safety Specification

The ProgressDialog is the only dialog with thread safety concerns. All other dialogs are stateless factory functions with no concurrent access patterns.

#### setPercentage Callback

The `setPercentage(int)` callback is safe to call from any thread:

1. Sets `ProgressBar.Percentage` — property is protected by `Lock` (thread-safe)
2. Calls `app.Invalidate()` — uses `Interlocked.CompareExchange` (thread-safe)

No marshaling to the UI thread is required.

#### logText Callback

The `logText(string)` callback marshals the `Buffer.InsertText()` call to the application's async context:

1. Writes an `Action` to `app._actionChannel.Writer.TryWrite(() => textArea.Buffer.InsertText(text))`
2. Calls `app.Invalidate()` to trigger a redraw

The `_actionChannel` is an `internal` field on `Application<TResult>`, accessible from `Stroke.Shortcuts` (same assembly). It is a `Channel<Action>` initialized during `RunAsync()`.

#### _actionChannel Null-Check

The `_actionChannel` is `null` before `RunAsync()` starts. The `logText` callback MUST check for null before writing, matching Python's `if loop is not None:` guard. If `_actionChannel` is null, the log call is silently dropped.

#### Background Task Lifecycle

The progress callback runs in this lifecycle:

1. **Factory phase**: `ProgressDialog()` creates the app and registers a `PreRunCallables` callback
2. **Pre-run phase**: During `RunAsync()`, after `Reset()` initializes `_backgroundTasksCts` and `_actionChannel`, the pre-run callable fires
3. **Background phase**: The pre-run callable calls `CreateBackgroundTask(async ct => { ... })`, which wraps `Task.Run(() => runCallback(setPercentage, logText))` in a tracked background task
4. **Completion phase**: When the callback completes (or throws), `finally { app.Exit(); }` exits the application
5. **Cleanup phase**: `RunAsync()` cancels remaining background tasks and waits for completion

**Timing constraint**: `CreateBackgroundTask` only works during `RunAsync` (after `_backgroundTasksCts` is initialized). `PreRunCallables` is the correct scheduling point because it executes after `Reset()` but before the first render — exactly when the background task infrastructure is ready.

### Documented Deviations from Python Source

#### ProgressDialog TextArea Height

Python uses `height=D(preferred=10**10)` (10,000,000,000) for the log text area. Since `10**10` exceeds `int.MaxValue` (2,147,483,647) and the C# `Dimension` constructor accepts `int`, the port uses `Dimension(preferred: int.MaxValue)`. Both values serve the identical purpose: "make this text area as large as possible." The behavioral difference is zero — the layout engine clamps the dimension to available space regardless.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 7 dialog shortcut functions (yes/no, message, input, button, radio list, checkbox list, progress) are implemented and callable
- **SC-002**: Each dialog function produces a correctly structured application with the expected widget hierarchy (dialog → frame → shadow → body + buttons)
- **SC-003**: Unit tests achieve at least 80% code coverage across all dialog shortcut functions
- **SC-004**: All dialog functions match the Python Prompt Toolkit source (`shortcuts/dialogs.py`) in parameter signatures, default values, and behavior
- **SC-005**: Focus navigation (Tab, Shift-Tab, Left, Right arrows) works correctly within all dialog types
- **SC-006**: Progress dialog correctly updates the progress bar and text area from a background callback without race conditions or UI corruption
- **SC-007**: All 7 dialog types can be instantiated and their applications created without runtime errors

### Non-Functional Requirements

- **NF-001**: `Dialogs.cs` MUST stay under 1,000 LOC (estimated ~250 LOC based on Python source of 331 lines)
- **NF-002**: Unit tests MUST achieve at least 80% code coverage across all dialog shortcut functions, covering all 7 dialog types with structure verification, handler behavior, and edge cases
- **NF-003**: All public methods MUST have XML doc comments (`///`) per Constitution Technical Standards

### Async Wrapper Testing Strategy

The 7 async convenience methods (`YesNoDialogAsync`, `MessageDialogAsync`, `InputDialogAsync`, `ButtonDialogAsync<T>`, `RadioListDialogAsync<T>`, `CheckboxListDialogAsync<T>`, `ProgressDialogAsync`) are thin wrappers that call the corresponding factory method and `RunAsync()`. They are implicitly covered by the factory method tests (which verify the `Application<T>` structure and handler behavior). Explicit async wrapper tests should verify compilation and return type correctness, but do not need to duplicate the behavioral testing of the underlying factory methods.

## Assumptions (Validated)

All assumptions have been validated against the current codebase:

- ✅ **Dialog withBackground**: The Dialog widget's `withBackground=true` wraps the Frame+Shadow in a `Box` with `style: "class:dialog"` (validated: `Dialog.cs` lines 124-134).
- ✅ **Widget constructors**: All supporting widgets (Button, Label, TextArea, RadioList, CheckboxList, ProgressBar, ValidationToolbar) are fully implemented with constructor signatures compatible with the Python dialog usage patterns (validated: Research R5).
- ✅ **Application.Exit()**: Calling `Exit()` with no arguments exits with `default(TResult)` — the `result` parameter defaults to `default` (validated: `Application.Lifecycle.cs` lines 31-55).
- ✅ **TextArea.AcceptHandler**: The property type is `Func<Buffer, bool>?` (not `Action<Buffer>`), allowing the handler to return `true` to keep the text (validated: `TextArea.cs` line 94).
- ✅ **_actionChannel access**: The field is `internal`, accessible from `Stroke.Shortcuts` within the same assembly (validated: `Application.cs` line 93).
- ✅ **FocusFunctions**: `FocusNext` and `FocusPrevious` have signature `NotImplementedOrNone? Handler(KeyPressEvent)`, matching the `KeyHandlerCallable` delegate expected by `KeyBindings.Add` (validated: `FocusFunctions.cs` lines 27-42).
- ✅ **MergedKeyBindings**: The merge infrastructure is available. `DefaultKeyBindings.Load()` returns a `MergedKeyBindings` instance (validated: `DefaultKeyBindings.cs`).
- ✅ **CreateBackgroundTask**: Signature is `Task CreateBackgroundTask(Func<CancellationToken, Task>)`, thread-safe via Lock, requires `_backgroundTasksCts` to be initialized (validated: `Application.RunAsync.cs` lines 549-573).
- ✅ **PreRunCallables**: Type is `List<Action>`, executed during `PreRun()` after `Reset()` but before first render, then cleared (validated: `Application.cs` line 472, `Application.RunAsync.cs` lines 365-379).
- ✅ **Invalidate()**: Fully thread-safe via `Interlocked.CompareExchange` (validated: `Application.RunAsync.cs` lines 447-483).
