# Tasks: Choice Input

**Input**: Design documents from `/specs/056-choice-input/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/ChoiceInput.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Shortcuts/`
- **Tests**: `tests/Stroke.Tests/Shortcuts/`
- Paths based on plan.md project structure

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create file structure and shared utilities

- [x] T001 Create `KeyboardInterrupt` exception class in `src/Stroke/Shortcuts/KeyboardInterrupt.cs` with standard exception constructors (default, message, message+inner)
- [x] T002 [P] Create private helper method `CreateDefaultChoiceInputStyle()` returning `Style.FromDict()` with `frame.border=#884444` and `selected-option=bold` - this will be placed in ChoiceInput.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core ChoiceInput<T> class structure that all user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 Create `ChoiceInput<T>` sealed class skeleton in `src/Stroke/Shortcuts/ChoiceInput.cs` with:
  - Constructor accepting all 12 parameters from contract (message, options, defaultValue, mouseSupport, style, symbol, bottomToolbar, showFrame, enableSuspend, enableInterrupt, interruptException, keyBindings)
  - Read-only properties for all configuration (Message, Options, Default, MouseSupport, Style, Symbol, BottomToolbar, ShowFrame, EnableSuspend, EnableInterrupt, InterruptException, KeyBindings)
  - Parameter validation: ArgumentNullException if options is null, ArgumentException if options is empty [FR-018]
  - Default value handling: interruptException defaults to typeof(KeyboardInterrupt), enableInterrupt defaults to true
- [x] T004 Implement private `CreateApplication()` method in `src/Stroke/Shortcuts/ChoiceInput.cs` that builds:
  - RadioList<T> widget with options, showNumbers=true, selectCharacter=symbol, defaultValue
  - Layout using HSplit with Box(Label) for message, Box(RadioList) for options
  - Application<T> with fullScreen=false, mouseSupport parameter
  - Focus set to radio_list via Layout.focusedElement

**Checkpoint**: Foundation ready - ChoiceInput can be instantiated and creates Application internally

---

## Phase 3: User Story 1 - Select Option from List (Priority: P1) 🎯 MVP

**Goal**: Core selection functionality - display options, navigate, select with Enter

**Independent Test**: Create a choice prompt with 3+ options, navigate with arrows, press number keys, select with Enter

### Implementation for User Story 1

- [x] T005 [US1] Configure RadioList in `CreateApplication()` in `src/Stroke/Shortcuts/ChoiceInput.cs` for navigation [FR-004, FR-005]:
  - Up/Down arrow navigation with wrap-around (delegated to RadioList)
  - Number keys 1-9 for direct selection (delegated to RadioList showNumbers=true)
  - k/j vi-style navigation (delegated to RadioList)
- [x] T006 [US1] Add Enter key binding in `CreateApplication()` in `src/Stroke/Shortcuts/ChoiceInput.cs` [FR-006]:
  - Bind Enter to exit Application with RadioList.CurrentValue as result
  - Use Application.Exit(result) pattern
- [x] T007 [US1] Implement `Prompt()` method in `src/Stroke/Shortcuts/ChoiceInput.cs` [FR-016]:
  - Call CreateApplication() to build Application<T>
  - Call Application.Run() to execute
  - Return the typed result T
- [x] T008 [US1] Implement default value selection in `src/Stroke/Shortcuts/ChoiceInput.cs` [FR-007]:
  - Pass defaultValue to RadioList constructor
  - If default doesn't match any option, RadioList selects first option (built-in behavior)

**Checkpoint**: User Story 1 complete - Basic selection prompt works with navigation and Enter to confirm

---

## Phase 4: User Story 2 - Cancel Selection (Priority: P2)

**Goal**: Ctrl+C cancellation with configurable exception type

**Independent Test**: Display prompt, press Ctrl+C, verify exception thrown (or ignored when disabled)

### Implementation for User Story 2

- [x] T009 [US2] Add Ctrl+C key binding in `CreateApplication()` in `src/Stroke/Shortcuts/ChoiceInput.cs` [FR-008]:
  - Create Condition filter from EnableInterrupt FilterOrBool
  - Bind Ctrl+C with filter to Application.Exit(exception: interruptException instance)
  - Also bind SIGINT signal handler with same behavior
- [x] T010 [US2] Ensure Ctrl+C does nothing when EnableInterrupt evaluates to false [FR-008]:
  - Key binding filter prevents handler execution when condition is false
  - Verify prompt remains displayed awaiting selection

**Checkpoint**: User Story 2 complete - Cancellation works with configurable exception, respects enableInterrupt setting

---

## Phase 5: User Story 6 - Async Application Integration (Priority: P2)

**Goal**: Async support for modern .NET applications

**Independent Test**: Call `await PromptAsync()` in async method, verify it completes without blocking

### Implementation for User Story 6

- [x] T011 [US6] Implement `PromptAsync()` method in `src/Stroke/Shortcuts/ChoiceInput.cs` [FR-016]:
  - Call CreateApplication() to build Application<T>
  - Call Application.RunAsync() and await result
  - Return the typed result T
- [x] T012 [US6] Add `Dialogs.Choice<T>()` convenience method in `src/Stroke/Shortcuts/Dialogs.cs` [FR-017]:
  - Accept all 12 parameters matching ChoiceInput constructor
  - Create ChoiceInput<T> instance
  - Call and return Prompt() result
- [x] T013 [US6] Add `Dialogs.ChoiceAsync<T>()` convenience method in `src/Stroke/Shortcuts/Dialogs.cs` [FR-017]:
  - Accept all 12 parameters matching ChoiceInput constructor
  - Create ChoiceInput<T> instance
  - Call and return await PromptAsync() result

**Checkpoint**: User Story 6 complete - Both sync and async APIs available with convenience methods

---

## Phase 6: User Story 3 - Visual Customization (Priority: P3)

**Goal**: Frame, custom symbol, custom styles, bottom toolbar

**Independent Test**: Create prompt with showFrame=true, custom symbol, custom style, and bottomToolbar - verify visual output

### Implementation for User Story 3

- [x] T014 [US3] Add ConditionalContainer for Frame in `CreateApplication()` in `src/Stroke/Shortcuts/ChoiceInput.cs` [FR-010]:
  - Wrap HSplit layout in ConditionalContainer
  - Create Condition filter from ShowFrame FilterOrBool
  - When true, wrap content in Frame widget with class:frame.border style
- [x] T015 [US3] Add bottom toolbar support in `CreateApplication()` in `src/Stroke/Shortcuts/ChoiceInput.cs` [FR-011]:
  - Add ConditionalContainer for toolbar below RadioList in HSplit
  - Filter: bottomToolbar != null AND ~IsDone AND RendererHeightIsKnown
  - Use FormattedTextToolbar or Window with FormattedTextControl for toolbar content
- [x] T016 [US3] Apply custom style in `CreateApplication()` in `src/Stroke/Shortcuts/ChoiceInput.cs` [FR-014]:
  - If style parameter is null, use CreateDefaultChoiceInputStyle()
  - Pass style to Application constructor
  - Style classes applied: frame.border, selected-option, bottom-toolbar.text

**Checkpoint**: User Story 3 complete - Visual customization fully functional

---

## Phase 7: User Story 4 - Mouse Interaction (Priority: P3)

**Goal**: Click-to-select functionality

**Independent Test**: Enable mouseSupport=true, click on different options, verify selection changes

### Implementation for User Story 4

- [x] T017 [US4] Verify mouse support passes through in `CreateApplication()` in `src/Stroke/Shortcuts/ChoiceInput.cs` [FR-009]:
  - Pass mouseSupport parameter to Application constructor
  - RadioList handles mouse click events internally (already implemented)
  - Graceful degradation on terminals without mouse support (Application handles this)

**Checkpoint**: User Story 4 complete - Mouse interaction works when enabled

---

## Phase 8: User Story 5 - Background Suspension (Priority: P4)

**Goal**: Ctrl+Z suspend on Unix platforms

**Independent Test**: On Unix, enable enableSuspend=true, press Ctrl+Z, verify process suspends

### Implementation for User Story 5

- [x] T018 [US5] Add Ctrl+Z key binding in `CreateApplication()` in `src/Stroke/Shortcuts/ChoiceInput.cs` [FR-012]:
  - Create Condition filter: EnableSuspend.Evaluate() AND PlatformUtils.SuspendToBackgroundSupported
  - Bind Ctrl+Z with filter to Application.SuspendToBackground() or equivalent
  - On Windows, filter evaluates false so key binding has no effect [XP-001]

**Checkpoint**: User Story 5 complete - Platform-appropriate suspend behavior

---

## Phase 9: Cross-Cutting - Key Binding Merge

**Purpose**: Support user-provided additional key bindings

- [x] T019 Add key binding merge support in `CreateApplication()` in `src/Stroke/Shortcuts/ChoiceInput.cs`:
  - If keyBindings parameter is not null, wrap in DynamicKeyBindings
  - Use MergedKeyBindings to combine: local bindings (Enter, Ctrl+C, Ctrl+Z), user bindings, RadioList bindings
  - Local bindings have highest priority

---

## Phase 10: Polish & Verification

**Purpose**: Documentation, edge cases, and validation

- [x] T020 [P] Add XML documentation comments to all public members in `src/Stroke/Shortcuts/ChoiceInput.cs`:
  - Include thread safety remarks noting immutable configuration (NFR-001)
  - Document that RadioList provides semantic list structure for screen reader compatibility (NFR-005)
- [x] T021 [P] Add XML documentation comments to `Dialogs.Choice<T>()` and `Dialogs.ChoiceAsync<T>()` in `src/Stroke/Shortcuts/Dialogs.cs`
- [x] T022 [P] Create unit tests in `tests/Stroke.Tests/Shortcuts/ChoiceInputTests.cs`:
  - Constructor validation tests (null options, empty options)
  - Default value handling tests (match, no match, first option selected)
  - Property getter tests (all 12 properties return correct values)
  - Immutability verification: confirm all properties are get-only with no public setters (NFR-001)
- [x] T023 [P] Create navigation tests in `tests/Stroke.Tests/Shortcuts/ChoiceInputTests.cs`:
  - Up/Down navigation with wrap-around
  - Number key selection (1-9)
  - Enter confirms selection
- [x] T024 [P] Create interrupt tests in `tests/Stroke.Tests/Shortcuts/ChoiceInputTests.cs`:
  - Ctrl+C with enableInterrupt=true throws KeyboardInterrupt
  - Ctrl+C with enableInterrupt=false is ignored
  - Custom interruptException type is thrown
- [x] T025 Run quickstart.md validation - execute all code examples to verify they work

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion
  - US1 (P1): Must complete first (provides Prompt() method)
  - US2 (P2), US6 (P2): Can run in parallel after US1
  - US3 (P3), US4 (P3): Can run in parallel after US1
  - US5 (P4): Can run after US1
- **Cross-Cutting (Phase 9)**: Can run after US2 (needs key binding infrastructure)
- **Polish (Phase 10)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - Core MVP
- **User Story 2 (P2)**: Depends on US1 Prompt() method existing
- **User Story 6 (P2)**: Depends on US1 CreateApplication() method existing
- **User Story 3 (P3)**: Depends on US1 CreateApplication() layout structure
- **User Story 4 (P3)**: Can start after Foundational - independent of US1
- **User Story 5 (P4)**: Depends on US2 key binding pattern

### Within Each User Story

- Core method implementation before extensions
- Key bindings after layout structure
- Story complete before moving to next priority

### Parallel Opportunities

- T001 and T002 can run in parallel (Setup phase)
- T020, T021 can run in parallel (documentation)
- T022, T023, T024 can run in parallel (test files)
- Once Foundational completes, US4 can start independently

---

## Parallel Example: Test Creation

```bash
# Launch all test files together (Phase 10):
Task: "Create unit tests in tests/Stroke.Tests/Shortcuts/ChoiceInputTests.cs"
Task: "Create navigation tests in tests/Stroke.Tests/Shortcuts/ChoiceInputTests.cs"
Task: "Create interrupt tests in tests/Stroke.Tests/Shortcuts/ChoiceInputTests.cs"

# Note: All three go to same file, so actually sequential
# But documentation tasks are truly parallel:
Task: "Add XML documentation to ChoiceInput.cs"
Task: "Add XML documentation to Dialogs.cs Choice methods"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (KeyboardInterrupt, default style)
2. Complete Phase 2: Foundational (ChoiceInput skeleton, CreateApplication)
3. Complete Phase 3: User Story 1 (navigation, Enter, Prompt)
4. **STOP and VALIDATE**: Test basic selection independently
5. Demo: Simple choice prompt with arrow navigation and Enter

### Incremental Delivery

1. Complete Setup + Foundational → ChoiceInput instantiable
2. Add User Story 1 → Test independently → Basic selection works (MVP!)
3. Add User Story 2 + 6 → Cancellation + Async → Full API surface
4. Add User Story 3 + 4 → Visual customization + Mouse → Enhanced UX
5. Add User Story 5 → Platform features → Complete feature parity
6. Polish → Tests + Docs → Production ready

### Single Developer Strategy

Execute tasks in priority order:
1. Setup (T001-T002)
2. Foundational (T003-T004)
3. US1 (T005-T008) → MVP checkpoint
4. US2 (T009-T010) + US6 (T011-T013)
5. US3 (T014-T016) + US4 (T017)
6. US5 (T018)
7. Cross-cutting (T019)
8. Polish (T020-T025)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at US1 checkpoint to validate MVP independently
- Files modified: `ChoiceInput.cs` (new), `Dialogs.cs` (add methods), `KeyboardInterrupt.cs` (new)
- Test file: `ChoiceInputTests.cs` (new)
