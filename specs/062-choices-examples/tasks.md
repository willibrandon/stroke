# Tasks: Choices Examples (Complete Set)

**Input**: Design documents from `/specs/062-choices-examples/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, quickstart.md

**Tests**: TUI Driver verification only (no unit tests for examples project per plan.md)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Path Conventions

- **Project location**: `examples/Stroke.Examples.Choices/`
- **Solution file**: `examples/Stroke.Examples.sln`

---

## Phase 1: Setup (Project Infrastructure)

**Purpose**: Create project structure and entry point

- [x] T001 Create `examples/Stroke.Examples.Choices/Stroke.Examples.Choices.csproj` with .NET 10, Stroke reference
- [x] T002 Create `examples/Stroke.Examples.Choices/Program.cs` with dictionary-based routing (case-insensitive, default to SimpleSelection)
- [x] T003 Add Stroke.Examples.Choices project to `examples/Stroke.Examples.sln`
- [x] T004 Verify empty project builds: `dotnet build examples/Stroke.Examples.sln`

**Checkpoint**: Project skeleton builds successfully

---

## Phase 2: User Story 1 + 8 - Basic Selection + Command-Line Routing (Priority: P1) 🎯 MVP

**Goal**: Enable running the SimpleSelection example with optional command-line routing

**Independent Test**: Run `dotnet run --project examples/Stroke.Examples.Choices` and verify arrow key navigation + Enter confirmation works

### Implementation for User Story 1 + 8

- [x] T005 [US1] Create `examples/Stroke.Examples.Choices/SimpleSelection.cs` with `Run()` method using `Dialogs.Choice<T>()` for basic 3-option selection (pizza, salad, sushi)
- [x] T006 [US8] Update `examples/Stroke.Examples.Choices/Program.cs` to wire SimpleSelection.Run to dictionary and handle unknown example names with helpful error message

### TUI Driver Verification for User Story 1

- [ ] T007 [US1] Verify SimpleSelection with TUI Driver: launch, arrow key navigation, Enter confirmation, verify output

**Checkpoint**: SimpleSelection example fully functional, command-line routing works

---

## Phase 3: User Story 2 - Default Value Example (Priority: P1)

**Goal**: Demonstrate pre-selected default value functionality

**Independent Test**: Run `dotnet run -- default`, press Enter immediately, verify "salad" is returned

### Implementation for User Story 2

- [x] T008 [P] [US2] Create `examples/Stroke.Examples.Choices/Default.cs` with `Run()` method using `Dialogs.Choice<T>()` with `defaultValue: "salad"` and `new Html("<u>...</u>:")` message
- [x] T009 [US2] Update `examples/Stroke.Examples.Choices/Program.cs` to add Default.Run to examples dictionary

### TUI Driver Verification for User Story 2

- [ ] T010 [US2] Verify Default with TUI Driver: launch, verify salad pre-selected, press Enter immediately, verify "salad" output

**Checkpoint**: Default example fully functional

---

## Phase 4: User Story 3 - Custom Styling Example (Priority: P2)

**Goal**: Demonstrate Style.FromDict() with custom colors and HTML formatted labels

**Independent Test**: Run `dotnet run -- color` and verify custom colors are visible

### Implementation for User Story 3

- [x] T011 [P] [US3] Create `examples/Stroke.Examples.Choices/Color.cs` with `Run()` method using `Style.FromDict()` with custom colors (#ff0000 selection, #884444 numbers, underline selected-option) and HTML formatted option labels (green Salad, red tomatoes)
- [x] T012 [US3] Update `examples/Stroke.Examples.Choices/Program.cs` to add Color.Run to examples dictionary

### TUI Driver Verification for User Story 3

- [ ] T013 [US3] Verify Color with TUI Driver: launch, verify styled rendering, make selection, verify output

**Checkpoint**: Color example fully functional with custom styling

---

## Phase 5: User Story 4 - Frame Examples (Priority: P2)

**Goal**: Demonstrate frame borders with conditional visibility

**Independent Test**: Run `dotnet run -- with-frame`, make selection, verify frame disappears

### Implementation for User Story 4

- [x] T014 [P] [US4] Create `examples/Stroke.Examples.Choices/WithFrame.cs` with `Run()` method using `showFrame: ~AppFilters.IsDone` for conditional frame visibility
- [x] T015 [P] [US4] Create `examples/Stroke.Examples.Choices/GrayFrameOnAccept.cs` with `Run()` method using `showFrame: true` and style with `"accepted frame.border": "#888888"` for color transition
- [x] T016 [US4] Update `examples/Stroke.Examples.Choices/Program.cs` to add WithFrame.Run and GrayFrameOnAccept.Run to examples dictionary

### TUI Driver Verification for User Story 4

- [ ] T017 [US4] Verify WithFrame with TUI Driver: launch, verify frame visible, press Enter, verify frame disappears
- [ ] T018 [US4] Verify GrayFrameOnAccept with TUI Driver: launch, verify red frame (#ff4444), press Enter, verify gray frame (#888888)

**Checkpoint**: Frame examples fully functional with conditional visibility

---

## Phase 6: User Story 5 - Bottom Toolbar Example (Priority: P2)

**Goal**: Demonstrate bottom toolbar with instructional text

**Independent Test**: Run `dotnet run -- frame-and-bottom-toolbar` and verify toolbar shows navigation instructions

### Implementation for User Story 5

- [x] T019 [P] [US5] Create `examples/Stroke.Examples.Choices/FrameAndBottomToolbar.cs` with `Run()` method using `bottomToolbar: new Html(" Press <b>[Up]</b>/<b>[Down]</b> to select.")` and `showFrame: ~AppFilters.IsDone`
- [x] T020 [US5] Update `examples/Stroke.Examples.Choices/Program.cs` to add FrameAndBottomToolbar.Run to examples dictionary

### TUI Driver Verification for User Story 5

- [ ] T021 [US5] Verify FrameAndBottomToolbar with TUI Driver: launch, verify toolbar text, make selection, verify frame and toolbar disappear

**Checkpoint**: FrameAndBottomToolbar example fully functional

---

## Phase 7: User Story 6 - Scrollable List Example (Priority: P2)

**Goal**: Demonstrate scrolling behavior with 99 options

**Independent Test**: Run `dotnet run -- many-choices`, navigate down 50 times, verify scrolling

### Implementation for User Story 6

- [x] T022 [P] [US6] Create `examples/Stroke.Examples.Choices/ManyChoices.cs` with `Run()` method generating 99 options (1-99) using LINQ and `Dialogs.Choice<T>()`
- [x] T023 [US6] Update `examples/Stroke.Examples.Choices/Program.cs` to add ManyChoices.Run to examples dictionary

### TUI Driver Verification for User Story 6

- [ ] T024 [US6] Verify ManyChoices with TUI Driver: launch, verify initial options visible, navigate down repeatedly, verify scrolling reveals higher numbers

**Checkpoint**: ManyChoices example fully functional with scrolling

---

## Phase 8: User Story 7 - Mouse Support Example (Priority: P3)

**Goal**: Demonstrate mouse click selection

**Independent Test**: Run `dotnet run -- mouse-support`, click on option with mouse, verify selection moves

### Implementation for User Story 7

- [x] T025 [P] [US7] Create `examples/Stroke.Examples.Choices/MouseSupport.cs` with `Run()` method using `mouseSupport: true`
- [x] T026 [US7] Update `examples/Stroke.Examples.Choices/Program.cs` to add MouseSupport.Run to examples dictionary

### TUI Driver Verification for User Story 7

- [ ] T027 [US7] Verify MouseSupport with TUI Driver: launch, click on Sushi option, verify selection moves, verify keyboard still works

**Checkpoint**: MouseSupport example fully functional

---

## Phase 9: Polish & Edge Cases

**Purpose**: Ensure graceful exit handling and final validation

- [x] T028 Verify Ctrl+C graceful exit in all examples (no stack traces)
- [x] T029 Verify Ctrl+D graceful exit in all examples (no stack traces)
- [x] T030 Run full solution build: `dotnet build examples/Stroke.Examples.sln`
- [ ] T031 Validate all examples against quickstart.md documentation

**Checkpoint**: All examples complete and verified

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - start immediately
- **User Story 1+8 (Phase 2)**: Depends on Setup completion
- **User Stories 2-7 (Phases 3-8)**: Depend on Phase 2 (Program.cs routing exists)
- **Polish (Phase 9)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1+8 (P1)**: Foundation - must complete first
- **User Story 2 (P1)**: Can start after Phase 2 - independent of other examples
- **User Story 3 (P2)**: Can start after Phase 2 - independent of other examples
- **User Story 4 (P2)**: Can start after Phase 2 - independent of other examples
- **User Story 5 (P2)**: Can start after Phase 2 - independent of other examples
- **User Story 6 (P2)**: Can start after Phase 2 - independent of other examples
- **User Story 7 (P3)**: Can start after Phase 2 - independent of other examples

### Parallel Opportunities

- T008, T011, T014, T015, T019, T022, T025: All example files can be created in parallel (different files)
- TUI Driver verifications should run sequentially (single terminal session)

---

## Parallel Example: Example Files

```bash
# After Phase 2 completes, all example files can be created in parallel:
Task: "Create examples/Stroke.Examples.Choices/Default.cs"
Task: "Create examples/Stroke.Examples.Choices/Color.cs"
Task: "Create examples/Stroke.Examples.Choices/WithFrame.cs"
Task: "Create examples/Stroke.Examples.Choices/GrayFrameOnAccept.cs"
Task: "Create examples/Stroke.Examples.Choices/FrameAndBottomToolbar.cs"
Task: "Create examples/Stroke.Examples.Choices/ManyChoices.cs"
Task: "Create examples/Stroke.Examples.Choices/MouseSupport.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 + 8 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: SimpleSelection + routing
3. **STOP and VALIDATE**: Test SimpleSelection independently
4. User can run `dotnet run` and make a selection

### Incremental Delivery

1. Complete Setup → Project builds
2. Add SimpleSelection + routing → MVP works
3. Add Default → Test independently
4. Add Color → Test independently
5. Add WithFrame + GrayFrameOnAccept → Test independently
6. Add FrameAndBottomToolbar → Test independently
7. Add ManyChoices → Test independently
8. Add MouseSupport → Test independently
9. Each example adds value without breaking previous examples

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- All examples should be independently runnable
- Use `new Html(...)` constructor, NOT `Html.Parse()` (per research.md)
- Use `~AppFilters.IsDone` for conditional frame visibility (per research.md)
- Commit after each task or logical group
