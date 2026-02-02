# Tasks: Dialog Shortcut Functions

**Input**: Design documents from `/specs/048-dialog-shortcuts/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/dialogs-api.md, quickstart.md

**Tests**: Included — spec requires 80% coverage (NF-002) and Constitution VIII mandates real-world testing.

**Organization**: Tasks grouped by user story. All 7 dialog types map to 7 user stories (US1–US7). Shared infrastructure in Phase 2 (Foundational) unblocks all stories.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Shortcuts/Dialogs.cs`
- **Tests**: `tests/Stroke.Tests/Shortcuts/DialogsTests.cs`
- **Python ref**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/shortcuts/dialogs.py`

---

## Phase 1: Setup

**Purpose**: Create the source and test files with class scaffolding

- [x] T001 Create `src/Stroke/Shortcuts/Dialogs.cs` with namespace `Stroke.Shortcuts`, static class declaration, required using directives (Stroke.Application, Stroke.Widgets, Stroke.Layout, Stroke.KeyBinding, Stroke.Filters, Stroke.Completion, Stroke.Validation, Stroke.FormattedText, Stroke.Styles), and XML doc comment on the class
- [x] T002 [P] Create `tests/Stroke.Tests/Shortcuts/DialogsTests.cs` with namespace, test class declaration, and required using directives

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement the two private helpers that ALL 7 dialog factory methods depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 Implement `CreateApp<T>(IContainer dialog, IStyle? style)` private static method in `src/Stroke/Shortcuts/Dialogs.cs` — creates `var bindings = new KeyBindings()`, adds Tab via `bindings.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Tab)], handler => handler.Set(FocusFunctions.FocusNext))`, adds BackTab via `bindings.Add<KeyHandlerCallable>([new KeyOrChar(Keys.BackTab)], handler => handler.Set(FocusFunctions.FocusPrevious))`, returns `new Application<T>(layout: new Layout.Layout(dialog), keyBindings: new MergedKeyBindings(DefaultKeyBindings.Load(), bindings), mouseSupport: true, style: style, fullScreen: true)`. Note: DefaultKeyBindings first, dialog bindings second — last wins in KeyProcessor (matches Python). Reference: Python `_create_app()` lines 313–325, quickstart.md lines 48–62
- [x] T004 Implement `ReturnNone()` private static method in `src/Stroke/Shortcuts/Dialogs.cs` — calls `AppContext.GetApp().Exit()` with no arguments (exits with default(T)). Reference: Python `_return_none()` at lines 328–330

**Checkpoint**: Foundation ready — CreateApp and ReturnNone are available for all dialog factory methods

---

## Phase 3: User Story 1 — Confirm a Destructive Action (Priority: P1) 🎯 MVP

**Goal**: Implement YesNoDialog and MessageDialog — the two simplest dialog types that establish the factory method + async wrapper pattern

**Independent Test**: Call YesNoDialog/MessageDialog, verify returned Application contains correct Dialog structure (title, label, buttons), verify button handlers exit with correct values

### Tests for US1 & US2

- [x] T005 [P] [US1] Write tests for YesNoDialog in `tests/Stroke.Tests/Shortcuts/DialogsTests.cs`: structure verification (dialog contains Label + 2 Buttons with correct text), Yes button handler exits with `true`, No button handler exits with `false`, custom button text (yesText="Confirm", noText="Deny"), default parameter values match ("Yes"/"No"), Tab scenario (application has Tab/BackTab key bindings)
- [x] T006 [P] [US2] Write tests for MessageDialog in `tests/Stroke.Tests/Shortcuts/DialogsTests.cs`: structure verification (dialog contains Label + 1 Button with "Ok" text), Ok button handler exits the application, custom ok text (okText="Got it"), default "Ok" casing (capital O, lowercase k)

### Implementation for US1 & US2

- [x] T007 [US1] Implement `YesNoDialog(AnyFormattedText title, AnyFormattedText text, string yesText, string noText, IStyle? style)` in `src/Stroke/Shortcuts/Dialogs.cs` — creates yes/no handlers calling AppContext.GetApp().Exit(result: true/false), builds Dialog with Label body + 2 Buttons, withBackground: true, returns CreateApp<bool>(dialog.Container, style). Include XML doc comment. Reference: Python lines 44–72
- [x] T008 [US2] Implement `MessageDialog(AnyFormattedText title, AnyFormattedText text, string okText, IStyle? style)` in `src/Stroke/Shortcuts/Dialogs.cs` — builds Dialog with Label body + 1 Button (handler=ReturnNone), withBackground: true, returns CreateApp<object?>(dialog.Container, style). Include XML doc comment. Reference: Python lines 157–173

**Checkpoint**: YesNoDialog and MessageDialog work independently. Simplest dialog types validated.

---

## Phase 4: User Story 3 — Collect Text Input (Priority: P2)

**Goal**: Implement InputDialog — text field with Enter-to-submit focus transfer, optional validation/completion/password masking

**Independent Test**: Call InputDialog with default text, verify TextArea + ValidationToolbar structure, verify OK handler returns text, Cancel returns null, AcceptHandler focuses OK button

### Tests for US3

- [x] T009 [US3] Write tests for InputDialog in `tests/Stroke.Tests/Shortcuts/DialogsTests.cs`: structure verification (dialog body is HSplit with Label + TextArea + ValidationToolbar), default text ("hello") populates TextArea, OK handler exits with text content, Cancel handler exits with null (ReturnNone), default "OK" casing (both uppercase), password mode parameter propagation, AcceptHandler returns true (keep text), accept handler focuses OK button via Layout.Focus

### Implementation for US3

- [x] T010 [US3] Implement `InputDialog(AnyFormattedText title, AnyFormattedText text, string okText, string cancelText, ICompleter? completer, IValidator? validator, FilterOrBool password, IStyle? style, string default_)` in `src/Stroke/Shortcuts/Dialogs.cs` — creates TextArea(text: default_, multiline: false, password, completer, validator, acceptHandler: accept), accept handler calls AppContext.GetApp().Layout.Focus(okButton.Window) and returns true, OK handler exits with textfield.Text, Cancel handler is ReturnNone, Dialog body is HSplit([Label, textfield, ValidationToolbar()], padding: new Dimension(preferred: 1, max: 1)), withBackground: true. Include XML doc comment. Reference: Python lines 105–154

**Checkpoint**: InputDialog works with text entry, validation, and cancel.

---

## Phase 5: User Story 4 — Choose from Custom Buttons (Priority: P2)

**Goal**: Implement ButtonDialog<T> — generic dialog with arbitrary button choices

**Independent Test**: Call ButtonDialog with (text, value) tuples, verify each button exits with its associated value

### Tests for US4

- [x] T011 [US4] Write tests for ButtonDialog<T> in `tests/Stroke.Tests/Shortcuts/DialogsTests.cs`: structure verification (dialog contains buttons matching tuple count), button handler exits with correct typed value, empty buttons list (dialog renders with no buttons), generic type works with int and string

### Implementation for US4

- [x] T012 [US4] Implement `ButtonDialog<T>(AnyFormattedText title, AnyFormattedText text, IReadOnlyList<(string Text, T Value)>? buttons, IStyle? style)` in `src/Stroke/Shortcuts/Dialogs.cs` — for each (text, value) tuple create Button with handler that calls AppContext.GetApp().Exit(result: value) (use closure capture for value), Dialog body is Label, withBackground: true. Handle null buttons as empty list. Include XML doc comment. Reference: Python lines 78–102

**Checkpoint**: ButtonDialog works with typed button values.

---

## Phase 6: User Story 5 — Select from a Radio List (Priority: P2)

**Goal**: Implement RadioListDialog<T> — single-selection list with OK/Cancel

**Independent Test**: Call RadioListDialog with values, verify RadioList embedded in dialog, OK returns current selection, Cancel returns null/default

### Tests for US5

- [x] T013 [US5] Write tests for RadioListDialog<T> in `tests/Stroke.Tests/Shortcuts/DialogsTests.cs`: structure verification (dialog body HSplit with Label + RadioList), OK handler exits with current_value, Cancel handler exits with null/default (ReturnNone), null values defaults to empty list, default_ parameter pre-selects value, "Ok" casing (capital O, lowercase k)

### Implementation for US5

- [x] T014 [US5] Implement `RadioListDialog<T>(AnyFormattedText title, AnyFormattedText text, string okText, string cancelText, IReadOnlyList<(T Value, AnyFormattedText Label)>? values, T? default_, IStyle? style)` in `src/Stroke/Shortcuts/Dialogs.cs` — null-coalesce values to empty list, create RadioList<T>(values, @default: default_), OK handler exits with radioList.CurrentValue, Cancel handler is ReturnNone, Dialog body is HSplit([Label, radioList], padding: 1), withBackground: true. Include XML doc comment. Reference: Python lines 176–212

**Checkpoint**: RadioListDialog works with single selection and cancel.

---

## Phase 7: User Story 6 — Select Multiple from a Checkbox List (Priority: P3)

**Goal**: Implement CheckboxListDialog<T> — multi-selection list with OK/Cancel

**Independent Test**: Call CheckboxListDialog with values and default selections, verify CheckboxList embedded, OK returns selected values list, Cancel returns null

### Tests for US6

- [x] T015 [US6] Write tests for CheckboxListDialog<T> in `tests/Stroke.Tests/Shortcuts/DialogsTests.cs`: structure verification (dialog body HSplit with Label + CheckboxList), OK handler exits with current_values list, Cancel handler exits with null (ReturnNone), null values defaults to empty list, defaultValues pre-checks items, "Ok" casing (capital O, lowercase k)

### Implementation for US6

- [x] T016 [US6] Implement `CheckboxListDialog<T>(AnyFormattedText title, AnyFormattedText text, string okText, string cancelText, IReadOnlyList<(T Value, AnyFormattedText Label)>? values, IReadOnlyList<T>? defaultValues, IStyle? style)` in `src/Stroke/Shortcuts/Dialogs.cs` — null-coalesce values to empty list, create CheckboxList<T>(values, defaultValues: defaultValues), OK handler exits with cbList.CurrentValues, Cancel handler is ReturnNone, Dialog body is HSplit([Label, cbList], padding: 1), withBackground: true. Include XML doc comment. Reference: Python lines 215–251

**Checkpoint**: CheckboxListDialog works with multi-selection and cancel.

---

## Phase 8: User Story 7 — Show Progress for a Long-Running Task (Priority: P3)

**Goal**: Implement ProgressDialog — background task execution with thread-safe UI updates

**Independent Test**: Call ProgressDialog, verify ProgressBar + TextArea structure, verify PreRunCallables registration, verify setPercentage/logText callback mechanisms

### Tests for US7

- [x] T017 [US7] Write tests for ProgressDialog in `tests/Stroke.Tests/Shortcuts/DialogsTests.cs`: structure verification (dialog body HSplit with Box(Label) + Box(TextArea) + ProgressBar, no buttons), PreRunCallables has one entry registered, null runCallback handled (no exception), setPercentage callback updates ProgressBar.Percentage, logText callback marshals via _actionChannel when available, logText callback handles null _actionChannel gracefully (silent drop)

### Implementation for US7

- [x] T018 [US7] Implement `ProgressDialog(AnyFormattedText title, AnyFormattedText text, Action<Action<int>, Action<string>>? runCallback, IStyle? style)` in `src/Stroke/Shortcuts/Dialogs.cs` — creates ProgressBar(), TextArea(focusable: false, height: Dimension(preferred: int.MaxValue)), Dialog body is HSplit([Box(Label(text)), Box(textArea, padding: Dimension.Exact(1)), progressBar]) with title, withBackground: true, no buttons. Create app via CreateApp<object?>. Define setPercentage: sets progressBar.Percentage + app.Invalidate(). Define logText: checks app._actionChannel is not null, then TryWrite(() => textArea.Buffer.InsertText(text)) + app.Invalidate(). Register PreRunCallables callback that calls app.CreateBackgroundTask wrapping Task.Run(() => (runCallback ?? noOp)(setPercentage, logText)) in try/finally with app.Exit() in finally. Include XML doc comment. Reference: Python lines 254–310

**Checkpoint**: ProgressDialog works with background execution, thread-safe updates, and automatic exit.

---

## Phase 9: Async Wrappers

**Purpose**: Implement all 7 async convenience methods that create-and-run in one call

- [x] T019 [P] Implement all 7 async wrapper methods in `src/Stroke/Shortcuts/Dialogs.cs`: YesNoDialogAsync (returns Task<bool>), MessageDialogAsync (returns Task), InputDialogAsync (returns Task<string?>), ButtonDialogAsync<T> (returns Task<T>), RadioListDialogAsync<T> (returns Task<T?>), CheckboxListDialogAsync<T> (returns Task<IReadOnlyList<T>?>), ProgressDialogAsync (returns Task). Each method calls the corresponding factory method, then .RunAsync(). Include XML doc comments on all 7 methods. Reference: contracts/dialogs-api.md §Async Convenience Methods
- [x] T020 [P] Write tests for async wrapper methods in `tests/Stroke.Tests/Shortcuts/DialogsTests.cs`: verify each async method compiles with correct return type, verify method signatures match contracts (parameter names and defaults identical to factory methods)

**Checkpoint**: All 14 public methods (7 factory + 7 async) are implemented and tested.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Edge cases, validation, and final quality checks

- [x] T021 Write edge case tests in `tests/Stroke.Tests/Shortcuts/DialogsTests.cs`: empty buttons list for ButtonDialog, empty values for RadioListDialog (OK returns default), empty values for CheckboxListDialog (OK returns empty list), custom button text for YesNoDialog/MessageDialog, InputDialog with multi-line default_ text in single-line TextArea, Left/Right arrow key navigation between buttons in multi-button dialogs (FR-010 — inherited from Dialog widget, verify it works in shortcut context)
- [x] T022 Verify all public methods have XML doc comments (`///`) in `src/Stroke/Shortcuts/Dialogs.cs` — check 7 factory methods + 7 async wrappers match contracts/dialogs-api.md signatures and documentation
- [x] T023 Run `dotnet build src/Stroke/Stroke.csproj` and verify zero warnings
- [x] T024 Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~Shortcuts.Dialogs"` and verify all tests pass
- [x] T025 Verify `src/Stroke/Shortcuts/Dialogs.cs` stays under 1,000 LOC (target ~250 LOC per NF-001)
- [x] T026 Verify `tests/Stroke.Tests/Shortcuts/DialogsTests.cs` stays under 1,000 LOC (target ~400 LOC per plan.md)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on T001 (Dialogs.cs file exists) — BLOCKS all user stories
- **US1 + US2 (Phase 3)**: Depends on T003, T004 (CreateApp + ReturnNone)
- **US3 (Phase 4)**: Depends on T003, T004 (uses CreateApp + ReturnNone)
- **US4 (Phase 5)**: Depends on T003 only (ButtonDialog has no cancel — no ReturnNone)
- **US5 (Phase 6)**: Depends on T003, T004 (uses CreateApp + ReturnNone)
- **US6 (Phase 7)**: Depends on T003, T004 (uses CreateApp + ReturnNone)
- **US7 (Phase 8)**: Depends on T003 only (ProgressDialog has no cancel button — no ReturnNone; exits via app.Exit() in finally)
- **Async Wrappers (Phase 9)**: Depends on ALL factory methods (T007, T008, T010, T012, T014, T016, T018)
- **Polish (Phase 10)**: Depends on Phases 3–9 complete

### User Story Dependencies

- **US1 & US2 (P1)**: Independent of each other, can be implemented in parallel after Phase 2
- **US3 (P2)**: Independent of US1/US2 — can start after Phase 2
- **US4 (P2)**: Independent of all others — can start after Phase 2
- **US5 (P2)**: Independent of all others — can start after Phase 2
- **US6 (P3)**: Independent of all others — can start after Phase 2
- **US7 (P3)**: Independent of all others — can start after Phase 2

### Within Each User Story

- Tests written FIRST (TDD: verify they fail before implementation)
- Implementation follows test
- Story complete before moving to next priority (in sequential mode)

### Parallel Opportunities

All user stories write to the SAME file (`Dialogs.cs`), so parallel execution requires care:

- **Safe parallel**: T005 + T006 (test files, independent test methods)
- **Safe parallel**: T019 + T020 (async wrappers: one in source, one in tests)
- **Sequential by necessity**: T007 → T008 → T010 → T012 → T014 → T016 → T018 (all modify same source file)
- **Tests can parallel with next story impl**: e.g., T011 (US4 tests) can run alongside T010 (US3 impl)

---

## Parallel Example: Phase 3 (US1 + US2)

```bash
# Launch tests for US1 and US2 together (different test methods, same file but no conflict):
Task: T005 "Write tests for YesNoDialog in tests/Stroke.Tests/Shortcuts/DialogsTests.cs"
Task: T006 "Write tests for MessageDialog in tests/Stroke.Tests/Shortcuts/DialogsTests.cs"

# Then implement sequentially (same source file):
Task: T007 "Implement YesNoDialog in src/Stroke/Shortcuts/Dialogs.cs"
Task: T008 "Implement MessageDialog in src/Stroke/Shortcuts/Dialogs.cs"
```

---

## Implementation Strategy

### MVP First (US1 + US2 Only)

1. Complete Phase 1: Setup (T001, T002)
2. Complete Phase 2: Foundational (T003, T004)
3. Complete Phase 3: YesNoDialog + MessageDialog (T005–T008)
4. **STOP and VALIDATE**: `dotnet test --filter "FullyQualifiedName~Shortcuts.Dialogs"`
5. Two simplest dialogs working — CreateApp pattern proven

### Incremental Delivery

1. Setup + Foundational → CreateApp/ReturnNone ready
2. US1 + US2 → YesNoDialog + MessageDialog (MVP — proves the pattern)
3. US3 → InputDialog (adds TextArea, AcceptHandler, validation)
4. US4 → ButtonDialog (adds generic typing)
5. US5 → RadioListDialog (adds RadioList composition)
6. US6 → CheckboxListDialog (adds CheckboxList composition)
7. US7 → ProgressDialog (adds background execution, thread safety)
8. Async Wrappers → All 7 *Async methods
9. Polish → Edge cases, doc verification, build validation

### Single-Developer Strategy (Recommended)

Since all source goes into one file (`Dialogs.cs`), sequential story implementation is natural:

1. T001 → T002 → T003 → T004 (setup + foundation)
2. T005 → T007 → T006 → T008 (US1 tests → impl → US2 tests → impl)
3. T009 → T010 (US3 tests → impl)
4. T011 → T012 (US4 tests → impl)
5. T013 → T014 (US5 tests → impl)
6. T015 → T016 (US6 tests → impl)
7. T017 → T018 (US7 tests → impl)
8. T019 → T020 (async wrappers + tests)
9. T021 → T022 → T023 → T024 → T025 → T026 (polish)

---

## Notes

- All 7 dialog types write to the same `Dialogs.cs` file (~250 LOC total)
- Python source is 331 lines — C# port is expected to be shorter due to constructor syntax
- Thread safety concerns exist ONLY in ProgressDialog (US7) — all others are pure factory functions
- The `_actionChannel` internal field access works because `Stroke.Shortcuts` is in the same assembly as `Stroke.Application`
- Constitution VIII: No mocks — tests use real Application, Dialog, Button, Label, etc. instances
