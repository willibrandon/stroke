# Tasks: Get Input Example (First Example)

**Input**: Design documents from `/specs/122-get-input-example/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, quickstart.md ✓

**Tests**: TUI Driver verification (real terminal testing per Constitution VIII). No unit tests — verification is via TUI Driver interactive testing.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the examples directory structure and solution file

- [ ] T001 Create `examples/` directory at repository root
- [ ] T002 Create `examples/Stroke.Examples.sln` solution file (FR-002)
- [ ] T003 Create `examples/Stroke.Examples.Prompts/` directory

**Checkpoint**: Directory structure exists, solution file created

---

## Phase 2: Foundational — Examples Infrastructure (US2)

**Purpose**: Build the examples solution independently — MUST complete before any example can run

**⚠️ CRITICAL**: No example work can begin until this phase is complete

**Goal**: Enable `dotnet build examples/Stroke.Examples.sln` to succeed

**Independent Test**: `dotnet build examples/Stroke.Examples.sln` completes with zero errors and zero warnings (SC-001)

- [ ] T004 [US2] Create `examples/Stroke.Examples.Prompts/Stroke.Examples.Prompts.csproj` with required elements (FR-003):
  - `<TargetFramework>net10.0</TargetFramework>`
  - `<OutputType>Exe</OutputType>`
  - `<LangVersion>13</LangVersion>`
  - `<Nullable>enable</Nullable>`
  - `<ImplicitUsings>enable</ImplicitUsings>`
  - `<ProjectReference Include="..\..\src\Stroke\Stroke.csproj" />`
- [ ] T005 [US2] Add `Stroke.Examples.Prompts.csproj` to `examples/Stroke.Examples.sln`
- [ ] T006 [US2] Create minimal `examples/Stroke.Examples.Prompts/Program.cs` with empty Main entry point
- [ ] T007 [US2] Verify `dotnet build examples/Stroke.Examples.sln` succeeds with zero errors and zero warnings (SC-001, US2-AS1)

**Checkpoint**: Foundation ready — `dotnet build examples/Stroke.Examples.sln` passes. Example implementation can now begin.

---

## Phase 3: User Story 1 — Run the Simplest Prompt Example (Priority: P1) 🎯 MVP

**Goal**: User runs the example, sees prompt, types input, sees echoed output

**Independent Test**: `dotnet run --project examples/Stroke.Examples.Prompts`, type text, press Enter, verify "You said: {text}" appears

### Implementation for User Story 1

- [ ] T008 [US1] Create `examples/Stroke.Examples.Prompts/GetInput.cs` with static `Run()` method (FR-004, FR-005, FR-009):
  - Prompt with exact text: `"Give me some input: "`
  - Echo with exact format: `"You said: {input}"`
  - Use `Prompt.RunPrompt()` from `Stroke.Shortcuts`
- [ ] T009 [US1] Update `examples/Stroke.Examples.Prompts/Program.cs` to call `GetInput.Run()` by default (FR-007)
- [ ] T010 [US1] Verify example behavior matches Python `get-input.py` exactly (SC-005):
  - Same prompt text
  - Same output format
  - Enter submits, Ctrl+C interrupts
- [ ] T011 [US1] TUI Driver verification — normal input (US1-AS2):
  - Launch example
  - Wait for "Give me some input: " (< 2 seconds per SC-002)
  - Send "Hello, World!"
  - Press Enter
  - Verify "You said: Hello, World!" appears (< 100ms per SC-003)
- [ ] T012 [US1] TUI Driver verification — empty input (US1-AS3):
  - Launch example
  - Wait for prompt
  - Press Enter immediately
  - Verify "You said: " appears
- [ ] T013 [US1] TUI Driver verification — Unicode input (US1-AS4, SC-007):
  - Launch example
  - Wait for prompt
  - Send "こんにちは 🎉"
  - Press Enter
  - Verify "You said: こんにちは 🎉" appears with correct display width

**Checkpoint**: User Story 1 complete. Example runs, accepts input, echoes output. Matches Python behavior exactly.

---

## Phase 4: User Story 3 — Run Named Example via Command Line (Priority: P2)

**Goal**: User runs specific example by name via `-- ExampleName` argument

**Independent Test**: `dotnet run --project examples/Stroke.Examples.Prompts -- GetInput` runs GetInput example

### Implementation for User Story 3

- [ ] T014 [US3] Refactor `examples/Stroke.Examples.Prompts/Program.cs` to implement dictionary-based routing (FR-006):
  - Create `Dictionary<string, Action>` mapping example names to `Run()` methods
  - Parse first command-line argument as example name
  - Match case-sensitively (`GetInput` works, `getinput` fails)
  - Default to first alphabetically when no argument (FR-007)
- [ ] T015 [US3] Implement error handling for unknown examples (FR-008):
  - Display format: `Unknown example: '{name}'. Available examples: {comma-separated alphabetical list}`
- [ ] T016 [US3] TUI Driver verification — named example routing (US3-AS1):
  - Launch with `-- GetInput`
  - Verify "Give me some input: " appears
- [ ] T017 [US3] TUI Driver verification — unknown example error (US3-AS2):
  - Launch with `-- UnknownExample`
  - Verify "Unknown example: 'UnknownExample'" appears
  - Verify "Available examples: GetInput" appears
- [ ] T018 [US3] TUI Driver verification — default behavior (US3-AS3):
  - Launch with no arguments
  - Verify GetInput runs (same as US1-AS1)

**Checkpoint**: User Story 3 complete. Named example routing works. Error messages display correctly.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and code quality checks

- [ ] T019 Verify `GetInput.cs` is under 15 lines per SC-004 counting rules
- [ ] T020 Verify all TUI Driver verifications pass (SC-006 complete script)
- [ ] T021 Run `dotnet build examples/Stroke.Examples.sln` one final time — confirm zero warnings

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational completion
- **User Story 3 (Phase 4)**: Depends on User Story 1 (builds on basic example)
- **Polish (Phase 5)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 2 (Infrastructure)**: Foundation — must complete first
- **User Story 1 (P1)**: Can start after US2 — independent MVP
- **User Story 3 (P2)**: Can start after US1 — adds command-line routing to existing example

### Task Dependencies

```
T001 → T002 → T003 → T004 → T005 → T006 → T007 (Setup + Foundation)
                                              ↓
                                     T008 → T009 → T010 → T011/T012/T013 (US1)
                                                               ↓
                                              T014 → T015 → T016/T017/T018 (US3)
                                                               ↓
                                                      T019/T020/T021 (Polish)
```

### Parallel Opportunities

Within each phase, tasks marked [P] can run in parallel. However, this feature has a linear dependency chain because:

1. Infrastructure must exist before any example code
2. GetInput.cs must exist before Program.cs can reference it
3. Basic example must work before routing can be added

TUI Driver verification tasks (T011, T012, T013) can run in parallel after T010.
TUI Driver verification tasks (T016, T017, T018) can run in parallel after T015.

---

## Parallel Example: User Story 1 TUI Verification

```bash
# After T010 completes, launch all TUI verifications together:
Task: "TUI Driver verification — normal input in examples/Stroke.Examples.Prompts"
Task: "TUI Driver verification — empty input in examples/Stroke.Examples.Prompts"
Task: "TUI Driver verification — Unicode input in examples/Stroke.Examples.Prompts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: Foundational (T004-T007)
3. Complete Phase 3: User Story 1 (T008-T013)
4. **STOP and VALIDATE**: Run example, verify it works
5. This is a complete, shippable MVP

### Incremental Delivery

1. Setup + Foundational → Build succeeds
2. Add User Story 1 → Basic example works (MVP!)
3. Add User Story 3 → Command-line routing works
4. Each story adds value without breaking previous stories

---

## Notes

- No unit tests — verification is via TUI Driver (Constitution VIII: real terminal testing)
- GetInput.cs should be ~10 lines (well under 15-line limit per SC-004)
- Program.cs should be ~30-40 lines with full routing
- File paths use forward slashes for cross-platform compatibility in docs
- All verification uses TUI Driver MCP tools (tui_launch, tui_wait_for_text, tui_send_text, tui_press_key)
