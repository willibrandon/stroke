# Tasks: Open in Editor Bindings

**Input**: Design documents from `/specs/041-open-in-editor-bindings/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/OpenInEditorBindings.md, quickstart.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: No setup tasks needed — project structure, dependencies, and all infrastructure already exist. The `Stroke.Application.Bindings` namespace and `tests/Stroke.Tests/Application/Bindings/` directory are already established by prior features (034–040).

**Checkpoint**: Ready to proceed directly to user story implementation.

---

## Phase 2: User Story 1 — Emacs User Opens Buffer in External Editor (Priority: P1)

**Goal**: Register Ctrl-X Ctrl-E binding for opening the buffer in an external editor in Emacs mode, filtered to Emacs mode with no active selection.

**Independent Test**: Call `LoadEmacsOpenInEditorBindings()`, verify it returns a `KeyBindings` with exactly 1 binding using keys `[ControlX, ControlE]`, handler `edit-and-execute-command`, and filter that activates only in Emacs mode without selection.

**Spec References**: FR-001, FR-009, SC-001 (partial), SC-002 (Emacs), SC-007 (partial)

### Tests for User Story 1

> **Note**: Filter tests (T004–T006) require a `CreateEnvironment` helper that sets up `AppContext.SetApp` scope — see `quickstart.md` §Step 2 for the pattern. This helper is part of the test class fixture and is created when writing the first test.

- [X] T001 [P] [US1] Write test `LoadEmacs_ReturnsKeyBindingsWithOneBinding` verifying `LoadEmacsOpenInEditorBindings()` returns `KeyBindings` with exactly 1 binding in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`
- [X] T002 [P] [US1] Write test `LoadEmacs_BindingHasCorrectKeySequence` verifying the binding has 2-key sequence `[Keys.ControlX, Keys.ControlE]` in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`
- [X] T003 [P] [US1] Write test `LoadEmacs_BindingHandlerIsEditAndExecuteCommand` verifying the handler is `NamedCommands.GetByName("edit-and-execute-command")` in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`
- [X] T004 [P] [US1] Write test `LoadEmacs_FilterActivatesInEmacsModeWithoutSelection` verifying filter returns true when in Emacs mode with no selection in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`
- [X] T005 [P] [US1] Write test `LoadEmacs_FilterDeactivatesWhenSelectionActive` verifying filter returns false when selection is active in Emacs mode in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`
- [X] T006 [P] [US1] Write test `LoadEmacs_FilterDeactivatesInViMode` verifying filter returns false when in Vi editing mode in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`

### Implementation for User Story 1

- [X] T007 [US1] Implement `LoadEmacsOpenInEditorBindings()` in `src/Stroke/Application/Bindings/OpenInEditorBindings.cs` — create static class, add method that creates `KeyBindings`, registers `[Keys.ControlX, Keys.ControlE]` with filter `EmacsFilters.EmacsMode & AppFilters.HasSelection.Invert()` and handler `NamedCommands.GetByName("edit-and-execute-command")` via `Add<Binding>`

**Checkpoint**: Emacs binding loader is functional — all T001–T006 tests pass.

---

## Phase 3: User Story 2 — Vi User Opens Buffer in External Editor (Priority: P1)

**Goal**: Register 'v' binding for opening the buffer in an external editor in Vi navigation mode.

**Independent Test**: Call `LoadViOpenInEditorBindings()`, verify it returns a `KeyBindings` with exactly 1 binding using key `'v'`, handler `edit-and-execute-command`, and filter that activates only in Vi navigation mode.

**Spec References**: FR-002, FR-010, SC-001 (partial), SC-002 (Vi), SC-007 (partial)

### Tests for User Story 2

> **Note**: Filter tests (T011–T012) require `CreateEnvironment` with `EditingMode.Vi` and `AppContext.SetApp` scope — same helper as US1.

- [X] T008 [P] [US2] Write test `LoadVi_ReturnsKeyBindingsWithOneBinding` verifying `LoadViOpenInEditorBindings()` returns `KeyBindings` with exactly 1 binding in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`
- [X] T009 [P] [US2] Write test `LoadVi_BindingHasCorrectKey` verifying the binding has single character key `'v'` in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`
- [X] T010 [P] [US2] Write test `LoadVi_BindingHandlerIsEditAndExecuteCommand` verifying the handler is `NamedCommands.GetByName("edit-and-execute-command")` in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`
- [X] T011 [P] [US2] Write test `LoadVi_FilterActivatesInViNavigationMode` verifying filter returns true when in Vi navigation mode in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`
- [X] T012 [P] [US2] Write test `LoadVi_FilterDeactivatesInViInsertMode` verifying filter returns false when in Vi insert mode in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`

### Implementation for User Story 2

- [X] T013 [US2] Implement `LoadViOpenInEditorBindings()` in `src/Stroke/Application/Bindings/OpenInEditorBindings.cs` — add method that creates `KeyBindings`, registers `[new KeyOrChar('v')]` with filter `ViFilters.ViNavigationMode` and handler `NamedCommands.GetByName("edit-and-execute-command")` via `Add<Binding>`

**Checkpoint**: Both Emacs and Vi binding loaders are functional — all T001–T012 tests pass.

---

## Phase 4: User Story 3 — Combined Binding Loader (Priority: P2)

**Goal**: Provide a single entry point that merges both Emacs and Vi open-in-editor bindings into one `IKeyBindingsBase`.

**Independent Test**: Call `LoadOpenInEditorBindings()`, verify it returns an `IKeyBindingsBase` (specifically `MergedKeyBindings`) whose flattened `.Bindings` collection contains exactly 2 bindings (1 Emacs + 1 Vi).

**Spec References**: FR-003, SC-001, SC-007 (complete)

### Tests for User Story 3

- [X] T014 [P] [US3] Write test `LoadCombined_ReturnsMergedKeyBindings` verifying `LoadOpenInEditorBindings()` returns a `MergedKeyBindings` instance in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`
- [X] T015 [P] [US3] Write test `LoadCombined_ContainsTwoBindingsTotal` verifying the flattened `.Bindings` collection has exactly 2 bindings in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`
- [X] T016 [P] [US3] Write test `LoadCombined_ContainsBothEmacsAndViBindings` verifying one binding has `[ControlX, ControlE]` keys and the other has `'v'` key in `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`

### Implementation for User Story 3

- [X] T017 [US3] Implement `LoadOpenInEditorBindings()` in `src/Stroke/Application/Bindings/OpenInEditorBindings.cs` — add method that returns `new MergedKeyBindings(LoadEmacsOpenInEditorBindings(), LoadViOpenInEditorBindings())`

**Checkpoint**: All three binding loaders are functional — all T001–T016 tests pass.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and XML documentation

- [X] T018 Verify all tests pass via `dotnet test tests/Stroke.Tests --filter "FullyQualifiedName~OpenInEditorBindings"`
- [X] T019 Add XML documentation comments (`<summary>`, `<returns>`, `<remarks>` with Python source reference and thread safety note) to all three public methods and the class declaration in `src/Stroke/Application/Bindings/OpenInEditorBindings.cs` per contract in `specs/041-open-in-editor-bindings/contracts/OpenInEditorBindings.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Skipped — no setup needed
- **Phase 2 (US1 Emacs)**: Can start immediately — creates the source and test files
- **Phase 3 (US2 Vi)**: Depends on T007 (source file must exist) — adds to same file
- **Phase 4 (US3 Combined)**: Depends on T007 and T013 (both loaders must exist)
- **Phase 5 (Polish)**: Depends on all implementation tasks (T007, T013, T017)

### User Story Dependencies

- **User Story 1 (P1 Emacs)**: No dependencies — creates both source and test files
- **User Story 2 (P1 Vi)**: Depends on US1 (same source file created in T007)
- **User Story 3 (P2 Combined)**: Depends on US1 + US2 (merges both loaders)

### Within Each User Story

- Tests are written first and MUST FAIL before implementation
- Implementation follows — tests then pass
- All test tasks within a story marked [P] can run in parallel

### Parallel Opportunities

- T001–T006 (US1 tests) can all run in parallel
- T008–T012 (US2 tests) can all run in parallel
- T014–T016 (US3 tests) can all run in parallel
- US1 and US2 tests can be written in parallel (same test file, different regions)

---

## Parallel Example: User Story 1

```text
# Launch all tests for User Story 1 together:
T001: "LoadEmacs_ReturnsKeyBindingsWithOneBinding"
T002: "LoadEmacs_BindingHasCorrectKeySequence"
T003: "LoadEmacs_BindingHandlerIsEditAndExecuteCommand"
T004: "LoadEmacs_FilterActivatesInEmacsModeWithoutSelection"
T005: "LoadEmacs_FilterDeactivatesWhenSelectionActive"
T006: "LoadEmacs_FilterDeactivatesInViMode"

# Then implement:
T007: "Implement LoadEmacsOpenInEditorBindings() in OpenInEditorBindings.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 2: US1 tests (T001–T006) + implementation (T007)
2. **STOP and VALIDATE**: Run `dotnet test --filter "FullyQualifiedName~OpenInEditorBindings"` — 6 tests pass
3. Emacs binding is immediately usable

### Incremental Delivery

1. Add US1 (Emacs binding) → 6 tests pass → Functional Emacs Ctrl-X Ctrl-E binding
2. Add US2 (Vi binding) → 11 tests pass → Functional Vi 'v' binding
3. Add US3 (Combined loader) → 16 tests pass → Unified API for both bindings
4. Polish (T018–T019) → XML docs, final validation

---

## Notes

- All 16 tests are in a single file (`OpenInEditorBindingsTests.cs`) — estimated ~200 LOC, well under 1,000 limit
- Implementation is a single file (`OpenInEditorBindings.cs`) — estimated ~60 LOC, well under 1,000 limit
- No new entities, infrastructure, or dependencies — pure binding registration
- Test environment follows `AutoSuggestBindingsTests` convention with `CreateEnvironment` helper
- Filter tests require `AppContext.SetApp` scope for filter evaluation
