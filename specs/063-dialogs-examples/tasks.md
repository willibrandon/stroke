# Tasks: Dialogs Examples (Complete Set)

**Input**: Design documents from `/specs/063-dialogs-examples/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: TUI Driver end-to-end verification (manual verification as fallback). No unit tests required for examples.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each dialog example.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

All paths relative to repository root:
- **Project**: `examples/Stroke.Examples.Dialogs/`
- **Solution**: `examples/Stroke.Examples.sln`

---

## Phase 1: Setup (Project Infrastructure)

**Purpose**: Create project structure and entry point

- [ ] T001 Create directory `examples/Stroke.Examples.Dialogs/`
- [ ] T002 Create project file `examples/Stroke.Examples.Dialogs/Stroke.Examples.Dialogs.csproj` with .NET 10, Stroke reference
- [ ] T003 Create entry point `examples/Stroke.Examples.Dialogs/Program.cs` with dictionary routing and graceful Ctrl+C/Ctrl+D handling
- [ ] T004 Add project to `examples/Stroke.Examples.sln` solution file
- [ ] T005 Verify build succeeds with `dotnet build examples/Stroke.Examples.sln`

**Checkpoint**: Project builds and runs with usage help (no examples yet)

---

## Phase 2: User Story 1 - Simple Message Dialog (Priority: P1) 🎯 MVP

**Goal**: Display a simple message dialog with title and multi-line text

**Independent Test**: Run `dotnet run -- MessageBox`, verify dialog shows "Example dialog window" title, dismiss with Enter

### Implementation for User Story 1

- [ ] T006 [US1] Implement MessageBox example in `examples/Stroke.Examples.Dialogs/MessageBox.cs` (port from `messagebox.py`)
- [ ] T007 [US1] Register MessageBox in Program.cs dictionary routing
- [ ] T008 [US1] Set MessageBox as default when no arguments provided (FR-003)
- [ ] T009 [US1] Verify with TUI Driver: dialog appears, Enter dismisses

**Checkpoint**: MessageBox example works end-to-end — this is the MVP

---

## Phase 3: User Story 2 - Yes/No Confirmation Dialog (Priority: P1)

**Goal**: Display Yes/No buttons and return boolean result

**Independent Test**: Run `dotnet run -- YesNoDialog`, select Yes → prints "Result = True", select No → prints "Result = False"

### Implementation for User Story 2

- [ ] T010 [US2] Implement YesNoDialog example in `examples/Stroke.Examples.Dialogs/YesNoDialog.cs` (port from `yes_no_dialog.py`)
- [ ] T011 [US2] Register YesNoDialog in Program.cs dictionary routing
- [ ] T012 [US2] Verify with TUI Driver: Yes returns True, Tab+Enter returns False

**Checkpoint**: YesNoDialog example works independently

---

## Phase 4: User Story 3 - Text Input Dialog (Priority: P1)

**Goal**: Capture user-typed text and return it

**Independent Test**: Run `dotnet run -- InputDialog`, type "Alice", press Enter → prints "Result = Alice"

### Implementation for User Story 3

- [ ] T013 [US3] Implement InputDialog example in `examples/Stroke.Examples.Dialogs/InputDialog.cs` (port from `input_dialog.py`)
- [ ] T014 [US3] Register InputDialog in Program.cs dictionary routing
- [ ] T015 [US3] Verify with TUI Driver: typed text captured and printed, empty input returns empty string (not null)

**Checkpoint**: All P1 stories complete — core dialog patterns demonstrated

---

## Phase 5: User Story 4 - Custom Button Dialog (Priority: P2)

**Goal**: Display custom buttons (Yes/No/Maybe) with nullable return type

**Independent Test**: Run `dotnet run -- ButtonDialog`, select "Maybe..." → prints "Result = " (null)

### Implementation for User Story 4

- [ ] T016 [P] [US4] Implement ButtonDialog example in `examples/Stroke.Examples.Dialogs/ButtonDialog.cs` (port from `button_dialog.py`)
- [ ] T017 [US4] Register ButtonDialog in Program.cs dictionary routing
- [ ] T018 [US4] Verify with TUI Driver: three buttons, nullable value works

**Checkpoint**: ButtonDialog example works independently

---

## Phase 6: User Story 5 - Password Input Dialog (Priority: P2)

**Goal**: Mask password input with asterisks while preserving actual input

**Independent Test**: Run `dotnet run -- PasswordDialog`, type "secret" → asterisks display, result is "secret"

### Implementation for User Story 5

- [ ] T019 [P] [US5] Implement PasswordDialog example in `examples/Stroke.Examples.Dialogs/PasswordDialog.cs` (port from `password_dialog.py`)
- [ ] T020 [US5] Register PasswordDialog in Program.cs dictionary routing
- [ ] T021 [US5] Verify with TUI Driver: input masked, actual value returned

**Checkpoint**: PasswordDialog example works independently

---

## Phase 7: User Story 6 - Radio List Selection Dialog (Priority: P2)

**Goal**: Single-selection list with both plain text and HTML-styled options

**Independent Test**: Run `dotnet run -- RadioDialog`, select "Green" → prints "Result = green", second dialog shows colored backgrounds

### Implementation for User Story 6

- [ ] T022 [P] [US6] Implement RadioDialog example in `examples/Stroke.Examples.Dialogs/RadioDialog.cs` (port from `radio_dialog.py`)
- [ ] T023 [US6] Register RadioDialog in Program.cs dictionary routing
- [ ] T024 [US6] Verify with TUI Driver: arrow navigation, two dialogs (plain + HTML styled)

**Checkpoint**: RadioDialog example works independently

---

## Phase 8: User Story 7 - Checkbox Multi-Selection Dialog (Priority: P2)

**Goal**: Multi-selection with custom styling and follow-up dialog

**Independent Test**: Run `dotnet run -- CheckboxDialog`, select items → follow-up shows selections or "*starves*"

### Implementation for User Story 7

- [ ] T025 [P] [US7] Implement CheckboxDialog example in `examples/Stroke.Examples.Dialogs/CheckboxDialog.cs` (port from `checkbox_dialog.py`)
- [ ] T026 [US7] Register CheckboxDialog in Program.cs dictionary routing
- [ ] T027 [US7] Verify with TUI Driver: Space toggles, custom pastel styling, follow-up dialog

**Checkpoint**: All P2 stories complete — advanced selection patterns demonstrated

---

## Phase 9: User Story 8 - Progress Dialog with Background Task (Priority: P3)

**Goal**: Background worker with progress bar and log text area

**Independent Test**: Run `dotnet run -- ProgressDialog`, watch progress increase, file names appear in log

### Implementation for User Story 8

- [ ] T028 [P] [US8] Implement ProgressDialog example in `examples/Stroke.Examples.Dialogs/ProgressDialog.cs` (port from `progress_dialog.py`)
- [ ] T029 [US8] Add file enumeration worker with UnauthorizedAccessException handling (edge case)
- [ ] T030 [US8] Register ProgressDialog in Program.cs dictionary routing
- [ ] T031 [US8] Verify with TUI Driver: progress bar advances, auto-closes at 100%

**Checkpoint**: ProgressDialog example works independently

---

## Phase 10: User Story 9 - Custom Styled Message Dialog (Priority: P3)

**Goal**: Custom colors via Style.FromDict() with HTML-styled title

**Independent Test**: Run `dotnet run -- StyledMessageBox`, verify green terminal aesthetic colors

### Implementation for User Story 9

- [ ] T032 [P] [US9] Implement StyledMessageBox example in `examples/Stroke.Examples.Dialogs/StyledMessageBox.cs` (port from `styled_messagebox.py`)
- [ ] T033 [US9] Register StyledMessageBox in Program.cs dictionary routing
- [ ] T034 [US9] Verify with TUI Driver: green background, HTML title styling

**Checkpoint**: All P3 stories complete — all 9 examples implemented

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and documentation

- [ ] T035 Verify all 9 examples build with `dotnet build examples/Stroke.Examples.sln`
- [ ] T036 Verify unknown example name shows usage help and exits with code 1 (FR-004)
- [ ] T037 Verify Ctrl+C graceful exit for all examples (FR-005)
- [ ] T038 Verify Ctrl+D graceful exit for all examples (FR-006)
- [ ] T039 Run quickstart.md validation steps

**Checkpoint**: Feature complete — all success criteria met

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **User Stories (Phase 2-10)**: All depend on Setup (Phase 1) completion
  - User stories can proceed sequentially in priority order (P1 → P2 → P3)
  - P2 stories (US4-US7) can run in parallel after P1 complete
  - P3 stories (US8-US9) can run in parallel after P2 complete
- **Polish (Phase 11)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Setup — No dependencies on other stories
- **US2 (P1)**: Can start after Setup — No dependencies on other stories
- **US3 (P1)**: Can start after Setup — No dependencies on other stories
- **US4 (P2)**: Can start after Setup — Independently testable
- **US5 (P2)**: Can start after Setup — Independently testable
- **US6 (P2)**: Can start after Setup — Independently testable
- **US7 (P2)**: Can start after Setup — Independently testable
- **US8 (P3)**: Can start after Setup — Independently testable
- **US9 (P3)**: Can start after Setup — Independently testable

### Within Each User Story

1. Implement example file
2. Register in Program.cs dictionary
3. Verify with TUI Driver

### Parallel Opportunities

- After Setup complete, all example implementations marked [P] can run in parallel (different files)
- T016, T019, T022, T025 can all run in parallel (P2 examples)
- T028, T032 can run in parallel (P3 examples)

---

## Parallel Example: P2 User Stories

```bash
# Launch all P2 example implementations together (different files):
Task: "Implement ButtonDialog in examples/Stroke.Examples.Dialogs/ButtonDialog.cs"
Task: "Implement PasswordDialog in examples/Stroke.Examples.Dialogs/PasswordDialog.cs"
Task: "Implement RadioDialog in examples/Stroke.Examples.Dialogs/RadioDialog.cs"
Task: "Implement CheckboxDialog in examples/Stroke.Examples.Dialogs/CheckboxDialog.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T005)
2. Complete Phase 2: User Story 1 - MessageBox (T006-T009)
3. **STOP and VALIDATE**: Test MessageBox independently
4. Demo: `dotnet run -- MessageBox` works

### Incremental Delivery

1. Complete Setup → Project builds
2. Add US1 (MessageBox) → MVP ready
3. Add US2 (YesNoDialog) → Boolean return demo
4. Add US3 (InputDialog) → Text input demo
5. Add US4-US7 (P2 stories) → Advanced patterns
6. Add US8-US9 (P3 stories) → Background tasks and styling
7. Each story adds a new dialog example without breaking previous ones

### Sequential Execution (Recommended)

Since all examples share Program.cs routing:

1. T001-T005: Setup
2. T006-T009: US1 MessageBox
3. T010-T012: US2 YesNoDialog
4. T013-T015: US3 InputDialog
5. T016-T018: US4 ButtonDialog
6. T019-T021: US5 PasswordDialog
7. T022-T024: US6 RadioDialog
8. T025-T027: US7 CheckboxDialog
9. T028-T031: US8 ProgressDialog
10. T032-T034: US9 StyledMessageBox
11. T035-T039: Polish

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is one dialog example — independently testable
- Commit after each user story phase completes
- Total: 39 tasks across 11 phases
- No unit tests required — examples demonstrate API usage with TUI Driver verification
