# Tasks: Choices Examples (Complete Set)

**Input**: Design documents from `/specs/062-choices-examples/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, quickstart.md ✅

**Tests**: No unit tests required per plan.md. TUI Driver MCP verification in final phase.

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1-US8)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Create project structure and solution integration

- [ ] T001 Create project directory `examples/Stroke.Examples.Choices/`
- [ ] T002 Create `examples/Stroke.Examples.Choices/Stroke.Examples.Choices.csproj` with Stroke reference
- [ ] T003 Add Stroke.Examples.Choices project to `examples/Stroke.Examples.sln`
- [ ] T004 Create `examples/Stroke.Examples.Choices/Program.cs` with dictionary-based routing to 8 examples

**Checkpoint**: Project builds with `dotnet build examples/Stroke.Examples.sln` (SC-001 partial)

---

## Phase 2: User Story 1 - Basic Selection (Priority: P1) 🎯 MVP

**Goal**: Demonstrate the simplest `Dialogs.Choice<T>()` usage with three food options

**Independent Test**: Run `dotnet run --project examples/Stroke.Examples.Choices -- SimpleSelection`, navigate with arrows, press Enter, verify printed value

### Implementation for User Story 1

- [ ] T005 [US1] Implement SimpleSelection example in `examples/Stroke.Examples.Choices/SimpleSelection.cs`

**Checkpoint**: SimpleSelection example runs, displays 3 options, accepts selection, prints result

---

## Phase 3: User Story 2 - Default Value Selection (Priority: P1)

**Goal**: Demonstrate pre-selected default option with HTML-formatted message

**Independent Test**: Run `dotnet run --project examples/Stroke.Examples.Choices -- Default`, press Enter immediately, verify "salad" is printed

### Implementation for User Story 2

- [ ] T006 [US2] Implement Default example in `examples/Stroke.Examples.Choices/Default.cs`

**Checkpoint**: Default example shows underlined message, pre-selects "salad", prints on Enter

---

## Phase 4: User Story 3 - Custom Styling (Priority: P2)

**Goal**: Demonstrate custom visual styling with colored numbers, underlined selection, and ANSI-colored option labels

**Independent Test**: Run `dotnet run --project examples/Stroke.Examples.Choices -- Color`, verify dark red bold numbers, green "Salad", red "tomatoes", underlined selection

### Implementation for User Story 3

- [ ] T007 [US3] Implement Color example in `examples/Stroke.Examples.Choices/Color.cs`

**Checkpoint**: Color example displays custom styles matching Python PTK example

---

## Phase 5: User Story 4 - Conditional Frame Display (Priority: P2)

**Goal**: Demonstrate frame that disappears on accept using `~AppFilters.IsDone` filter

**Independent Test**: Run `dotnet run --project examples/Stroke.Examples.Choices -- WithFrame`, observe frame during selection, verify frame disappears after Enter

### Implementation for User Story 4

- [ ] T008 [US4] Implement WithFrame example in `examples/Stroke.Examples.Choices/WithFrame.cs`

**Checkpoint**: WithFrame example shows frame during selection, hides on accept

---

## Phase 6: User Story 5 - Frame with Bottom Toolbar (Priority: P2)

**Goal**: Combine frame border with instructional bottom toolbar that disappears on accept

**Independent Test**: Run `dotnet run --project examples/Stroke.Examples.Choices -- FrameAndBottomToolbar`, verify frame and toolbar visible, both disappear on accept

### Implementation for User Story 5

- [ ] T009 [US5] Implement FrameAndBottomToolbar example in `examples/Stroke.Examples.Choices/FrameAndBottomToolbar.cs`

**Checkpoint**: FrameAndBottomToolbar shows red frame + styled toolbar, both hide on accept

---

## Phase 7: User Story 7 - Scrollable List Navigation (Priority: P2)

**Goal**: Demonstrate automatic scrolling with 99 options generated via LINQ

**Independent Test**: Run `dotnet run --project examples/Stroke.Examples.Choices -- ManyChoices`, navigate to option 51, verify scrolling works and "51" is printed

**Note**: Moved before US6 because P2 priority

### Implementation for User Story 7

- [ ] T010 [US7] Implement ManyChoices example in `examples/Stroke.Examples.Choices/ManyChoices.cs`

**Checkpoint**: ManyChoices displays 99 options, scrolls correctly, prints selected number

---

## Phase 8: User Story 6 - Style Change on Accept (Priority: P3)

**Goal**: Demonstrate frame color changing from red to gray upon acceptance (advanced styling)

**Independent Test**: Run `dotnet run --project examples/Stroke.Examples.Choices -- GrayFrameOnAccept`, observe red frame, press Enter, verify frame turns gray and remains visible

### Implementation for User Story 6

- [ ] T011 [US6] Implement GrayFrameOnAccept example in `examples/Stroke.Examples.Choices/GrayFrameOnAccept.cs`

**Checkpoint**: GrayFrameOnAccept shows red frame, changes to gray on accept, stays visible

---

## Phase 9: User Story 8 - Mouse Support (Priority: P3)

**Goal**: Enable mouse click selection in addition to keyboard navigation

**Independent Test**: Run `dotnet run --project examples/Stroke.Examples.Choices -- MouseSupport`, click an option with mouse, press Enter, verify printed value

### Implementation for User Story 8

- [ ] T012 [US8] Implement MouseSupport example in `examples/Stroke.Examples.Choices/MouseSupport.cs`

**Checkpoint**: MouseSupport allows clicking options, keyboard still works, prints selected value

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Verify all examples handle edge cases and meet success criteria

- [ ] T013 Verify all 8 examples handle Ctrl+C gracefully (FR-014, SC-008)
- [ ] T014 Verify all 8 examples handle Ctrl+D gracefully (FR-014)
- [ ] T015 Verify unknown example name shows error with list of valid examples (FR-005, SC-009)
- [ ] T016 Verify no argument defaults to SimpleSelection (FR-004)
- [ ] T017 Run TUI Driver verification per `specs/062-choices-examples/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **User Stories (Phases 2-9)**: All depend on Setup completion
  - User stories are independent - can proceed in parallel if desired
  - Recommended: Follow priority order (P1 → P2 → P3) for incremental delivery
- **Polish (Phase 10)**: Depends on all user stories being complete

### User Story Dependencies

All user stories are **independent** and can be implemented in any order after Setup:

| Story | Phase | Priority | Dependencies |
|-------|-------|----------|--------------|
| US1: SimpleSelection | 2 | P1 | Setup only |
| US2: Default | 3 | P1 | Setup only |
| US3: Color | 4 | P2 | Setup only |
| US4: WithFrame | 5 | P2 | Setup only |
| US5: FrameAndBottomToolbar | 6 | P2 | Setup only |
| US7: ManyChoices | 7 | P2 | Setup only |
| US6: GrayFrameOnAccept | 8 | P3 | Setup only |
| US8: MouseSupport | 9 | P3 | Setup only |

### Parallel Opportunities

After Setup completes, ALL example implementations can run in parallel:

```bash
# All these tasks operate on different files:
T005 [US1] SimpleSelection.cs
T006 [US2] Default.cs
T007 [US3] Color.cs
T008 [US4] WithFrame.cs
T009 [US5] FrameAndBottomToolbar.cs
T010 [US7] ManyChoices.cs
T011 [US6] GrayFrameOnAccept.cs
T012 [US8] MouseSupport.cs
```

---

## Parallel Example: All Examples

```bash
# After Setup (T001-T004) completes, launch all example tasks in parallel:
Task: "Implement SimpleSelection in examples/Stroke.Examples.Choices/SimpleSelection.cs" [US1]
Task: "Implement Default in examples/Stroke.Examples.Choices/Default.cs" [US2]
Task: "Implement Color in examples/Stroke.Examples.Choices/Color.cs" [US3]
Task: "Implement WithFrame in examples/Stroke.Examples.Choices/WithFrame.cs" [US4]
Task: "Implement FrameAndBottomToolbar in examples/Stroke.Examples.Choices/FrameAndBottomToolbar.cs" [US5]
Task: "Implement ManyChoices in examples/Stroke.Examples.Choices/ManyChoices.cs" [US7]
Task: "Implement GrayFrameOnAccept in examples/Stroke.Examples.Choices/GrayFrameOnAccept.cs" [US6]
Task: "Implement MouseSupport in examples/Stroke.Examples.Choices/MouseSupport.cs" [US8]
```

---

## Implementation Strategy

### MVP First (User Stories 1-2 Only)

1. Complete Phase 1: Setup (T001-T004)
2. Complete Phase 2: User Story 1 - SimpleSelection (T005)
3. **STOP and VALIDATE**: Test SimpleSelection independently
4. Complete Phase 3: User Story 2 - Default (T006)
5. Deploy/demo if ready - covers foundational `Dialogs.Choice<T>()` patterns

### Incremental Delivery

1. Setup → Foundation ready
2. Add US1 (SimpleSelection) → Test → MVP!
3. Add US2 (Default) → Test → Core patterns complete
4. Add US3-US5, US7 (P2 stories) → Test → Styling/frames/scrolling complete
5. Add US6, US8 (P3 stories) → Test → All features demonstrated
6. Polish phase → Final verification

### Parallel Team Strategy

With multiple developers:

1. One developer completes Setup (T001-T004)
2. Once Setup is done, assign examples to developers:
   - Developer A: SimpleSelection, Default (P1)
   - Developer B: Color, WithFrame, FrameAndBottomToolbar (P2)
   - Developer C: ManyChoices, GrayFrameOnAccept, MouseSupport (P2/P3)
3. All examples implement independently, no conflicts

---

## Notes

- Each example file is ~15-35 lines (well under 1000 LOC limit)
- All examples follow same pattern: static class with `Run()` method
- Ctrl+C/Ctrl+D handling comes from `Dialogs.Choice<T>()` API (already implemented)
- No unit tests - TUI Driver verification in Polish phase
- Code patterns documented in `quickstart.md` for reference during implementation
