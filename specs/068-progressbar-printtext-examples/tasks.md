# Tasks: Progress Bar and Print Text Examples

**Input**: Design documents from `/specs/068-progressbar-printtext-examples/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: No xUnit tests — example projects *are* the tests (visual verification via TUI Driver per spec).

**Organization**: Tasks grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create both project files, directories, and solution entries so all user stories can build

- [ ] T001 [P] Create PrintText project file at `examples/Stroke.Examples.PrintText/Stroke.Examples.PrintText.csproj` per contracts/print-text-examples.md (net10.0, Exe, LangVersion 13, Nullable, ImplicitUsings, ProjectReference to `../../src/Stroke/Stroke.csproj`)
- [ ] T002 [P] Create ProgressBar project file at `examples/Stroke.Examples.ProgressBar/Stroke.Examples.ProgressBar.csproj` per contracts/progress-bar-examples.md (same settings, ProjectReference to `../../src/Stroke/Stroke.csproj`)
- [ ] T003 Add both new projects to `examples/Stroke.Examples.sln` with new GUIDs and all 6 build configuration mappings (Debug/Release × Any CPU/x64/x86)
- [ ] T004 Verify solution builds: `dotnet build examples/Stroke.Examples.sln` compiles with zero errors

**Checkpoint**: Both projects exist, solution builds. Implementation can begin.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create Program.cs routing entry points for both projects — all example tasks depend on these

**⚠️ CRITICAL**: No example implementation can begin until the routing shell exists

- [ ] T005 [P] Create PrintText routing in `examples/Stroke.Examples.PrintText/Program.cs` — `internal static class Program` with `Dictionary<string, Action>` containing all 9 routing entries (initially referencing stub classes), `Main(string[] args)` with routing logic, `ShowUsage()` listing all examples. Namespace: `Stroke.Examples.PrintText`. Per contracts/print-text-examples.md
- [ ] T006 [P] Create ProgressBar routing in `examples/Stroke.Examples.ProgressBar/Program.cs` — `internal static class Program` with `Dictionary<string, Func<Task>>` containing all 15 routing entries (initially referencing stub classes), `async Task<int> Main(string[] args)` with routing logic and top-level catch for KeyboardInterruptException/EOFException, `ShowUsage()`. Namespace: `Stroke.Examples.ProgressBarExamples`. Per contracts/progress-bar-examples.md

**Checkpoint**: Both programs compile with stub classes. `dotnet run -- --help` shows usage for each project.

---

## Phase 3: User Story 1 — Print Formatted Text to Terminal (Priority: P1) 🎯 MVP

**Goal**: Implement all 9 print text examples using existing Stroke infrastructure (FormattedTextOutput, Html, Ansi, Style, ColorDepth, Widgets). Zero external dependencies.

**Independent Test**: Run each example via `dotnet run --project examples/Stroke.Examples.PrintText -- <name>` and verify formatted output appears with correct colors/styles, process exits cleanly.

### Implementation for User Story 1

- [ ] T007 [P] [US1] Implement `PrintFormattedText.Run()` in `examples/Stroke.Examples.PrintText/PrintFormattedText.cs` — 4 formatting methods (FormattedText tuples with Style.FromDict, Html with style classes, Html with inline styles, Ansi escape sequences). Port of `print-text/print-formatted-text.py`. FR-010
- [ ] T008 [P] [US1] Implement `AnsiColors.Run()` in `examples/Stroke.Examples.PrintText/AnsiColors.cs` — display all 16 ANSI foreground colors and 16 background colors using FormattedText tuples. Port of `print-text/ansi-colors.py`. FR-006
- [ ] T009 [P] [US1] Implement `Ansi.Run()` in `examples/Stroke.Examples.PrintText/Ansi.cs` — demonstrate bold, italic, underline, strikethrough, 256-color via raw ANSI escape sequences using `new Ansi(...)`. Port of `print-text/ansi.py`. FR-007
- [ ] T010 [P] [US1] Implement `HtmlExample.Run()` in `examples/Stroke.Examples.PrintText/Html.cs` — demonstrate `<b>`, `<i>`, `<ansired>`, `<style>` tags, and `Html.Format()` interpolation. Class named `HtmlExample` to avoid collision with `Html` type. Port of `print-text/html.py`. FR-008
- [ ] T011 [P] [US1] Implement `NamedColors.Run()` in `examples/Stroke.Examples.PrintText/NamedColors.cs` — iterate `Stroke.Styles.NamedColors.Colors`, display each color at Depth4Bit, Depth8Bit, Depth24Bit. Port of `print-text/named-colors.py`. FR-009
- [ ] T012 [P] [US1] Implement `PrintFrame.Run()` in `examples/Stroke.Examples.PrintText/PrintFrame.cs` — render bordered Frame + TextArea via `FormattedTextOutput.PrintContainer()`. Port of `print-text/print-frame.py`. FR-011
- [ ] T013 [P] [US1] Implement `TrueColorDemo.Run()` in `examples/Stroke.Examples.PrintText/TrueColorDemo.cs` — 7 RGB gradients (red, green, blue, yellow, magenta, cyan, gray) each at 3 color depths, i in 0..255 step 4. Port of `print-text/true-color-demo.py`. FR-012
- [ ] T014 [P] [US1] Implement `PygmentsTokens.Run()` in `examples/Stroke.Examples.PrintText/PygmentsTokens.cs` — syntax-highlighted text using PygmentsTokens list with custom Style. Port of `print-text/pygments-tokens.py`. FR-013
- [ ] T015 [P] [US1] Implement `LogoAnsiArt.Run()` in `examples/Stroke.Examples.PrintText/LogoAnsiArt.cs` — ANSI art logo using 24-bit true color RGB background blocks via `new Ansi(...)`. Port of `print-text/prompt-toolkit-logo-ansi-art.py`. FR-014
- [ ] T016 [US1] Build and smoke-test all 9 PrintText examples: `dotnet build examples/Stroke.Examples.PrintText/Stroke.Examples.PrintText.csproj` then run 2-3 examples via TUI Driver confirming visible output and clean exit

**Checkpoint**: All 9 print text examples are fully functional. User Story 1 is independently testable and deliverable as MVP.

---

## Phase 4: User Story 2 — Basic Progress Bar Iteration (Priority: P2)

**Goal**: Implement the foundational progress bar example that all other progress bar examples build upon.

**Independent Test**: Run `dotnet run --project examples/Stroke.Examples.ProgressBar -- simple-progress-bar` and verify bar fills from 0% to 100% over ~8 seconds, exits cleanly.

**Dependency**: Feature 71 (ProgressBar API) must be implemented for runtime testing.

### Implementation for User Story 2

- [ ] T017 [US2] Implement `SimpleProgressBar.Run()` in `examples/Stroke.Examples.ProgressBar/SimpleProgressBar.cs` — `await using var pb = new ProgressBar(); await foreach (var i in pb.Iterate(Enumerable.Range(0, 800))) await Task.Delay(10);`. Port of `progress-bar/simple-progress-bar.py`. FR-021
- [ ] T018 [P] [US2] Implement `UnknownLength.Run()` in `examples/Stroke.Examples.ProgressBar/UnknownLength.cs` — IEnumerable<int> generator with yield (no known total), shows elapsed time but no ETA. Port of `progress-bar/unknown-length.py`. FR-023
- [ ] T019 [US2] Verify SimpleProgressBar and UnknownLength build: `dotnet build examples/Stroke.Examples.ProgressBar/Stroke.Examples.ProgressBar.csproj` (runtime test deferred to Feature 71)

**Checkpoint**: Basic progress bar pattern compiles. Foundation for all other progress bar stories.

---

## Phase 5: User Story 3 — Styled and Custom-Formatted Progress Bars (Priority: P3)

**Goal**: Implement 8 styled progress bar examples demonstrating formatter composability, custom colors, bar characters, spinning wheels, iterations-per-second, and rainbow gradients.

**Independent Test**: Run each styled example and visually confirm custom formatting matches the Python originals.

**Dependency**: Feature 71 (ProgressBar API) must be implemented for runtime testing.

### Implementation for User Story 3

- [ ] T020 [P] [US3] Implement `Styled1.Run()` in `examples/Stroke.Examples.ProgressBar/Styled1.cs` — Style.FromDict with 10 style keys (title, label, percentage, bar-a, bar-b, bar-c, current, total, time-elapsed, time-left). 1600 items, 10ms sleep. Port of `progress-bar/styled-1.py`. FR-027
- [ ] T021 [P] [US3] Implement `Styled2.Run()` in `examples/Stroke.Examples.ProgressBar/Styled2.cs` — custom formatters: Label(), SpinningWheel(), Text(), Bar(sym_a/sym_b/sym_c), TimeLeft(). Custom style. 20 items, 1s sleep. Port of `progress-bar/styled-2.py`. FR-028
- [ ] T022 [P] [US3] Implement `StyledAptGet.Run()` in `examples/Stroke.Examples.ProgressBar/StyledAptGet.cs` — apt-get install format: Label(suffix), Percentage(), Bar(#,#,.), Progress(), TimeLeft(), TimeElapsed(). Yellow label style. 1600 items. Port of `progress-bar/styled-apt-get-install.py`. FR-029
- [ ] T023 [P] [US3] Implement `StyledRainbow.Run()` in `examples/Stroke.Examples.ProgressBar/StyledRainbow.cs` — Prompt.Confirm() for color depth, Rainbow(Bar()), Rainbow(TimeLeft()). 20 items, 1s sleep. Port of `progress-bar/styled-rainbow.py`. FR-030
- [ ] T024 [P] [US3] Implement `StyledTqdm1.Run()` in `examples/Stroke.Examples.ProgressBar/StyledTqdm1.cs` — tqdm format: Bar(█,█, ), Progress(), Percentage(), TimeElapsed(), TimeLeft(), IterationsPerSecond(). Cyan style. 1600 items. Port of `progress-bar/styled-tqdm-1.py`. FR-031
- [ ] T025 [P] [US3] Implement `StyledTqdm2.Run()` in `examples/Stroke.Examples.ProgressBar/StyledTqdm2.cs` — tqdm reverse-video bar: Bar( , , ), reverse style. Percentage(), Progress(), TimeElapsed(), TimeLeft(), IterationsPerSecond(). 1600 items. Port of `progress-bar/styled-tqdm-2.py`. FR-032
- [ ] T026 [P] [US3] Implement `ColoredTitleLabel.Run()` in `examples/Stroke.Examples.ProgressBar/ColoredTitleLabel.cs` — Html title with bg:yellow/fg:black, Html label with ansired. 800 items, 10ms sleep. Port of `progress-bar/colored-title-and-label.py`. FR-025
- [ ] T027 [P] [US3] Implement `ScrollingTaskName.Run()` in `examples/Stroke.Examples.ProgressBar/ScrollingTaskName.cs` — very long label string, custom title warning about window size. 800 items, 10ms sleep. Port of `progress-bar/scrolling-task-name.py`. FR-026
- [ ] T028 [US3] Verify all 8 styled examples build: `dotnet build examples/Stroke.Examples.ProgressBar/Stroke.Examples.ProgressBar.csproj` (runtime test deferred to Feature 71)

**Checkpoint**: All 8 styled/formatted progress bar examples compile. Formatter composability demonstrated across all patterns.

---

## Phase 6: User Story 4 — Parallel and Nested Progress Bars (Priority: P4)

**Goal**: Implement 4 parallel/nested progress bar examples demonstrating thread-safety, dynamic bar management, and concurrent task tracking.

**Independent Test**: Run TwoTasks and verify two bars update independently; run NestedProgressBars and verify inner bars appear/disappear.

**Dependency**: Feature 71 (ProgressBar API) must be implemented for runtime testing.

### Implementation for User Story 4

- [ ] T029 [P] [US4] Implement `TwoTasks.Run()` in `examples/Stroke.Examples.ProgressBar/TwoTasks.cs` — 2 threads with IsBackground=true, Thread t1: 100 items/50ms, Thread t2: 150 items/80ms. Join with 500ms timeout. Port of `progress-bar/two-tasks.py`. FR-022
- [ ] T030 [P] [US4] Implement `NestedProgressBars.Run()` in `examples/Stroke.Examples.ProgressBar/NestedProgressBars.cs` — outer bar 6 iterations, inner bar 200 iterations with removeWhenDone: true. Html title and bottomToolbar. Port of `progress-bar/nested-progress-bars.py`. FR-024
- [ ] T031 [P] [US4] Implement `ManyParallelTasks.Run()` in `examples/Stroke.Examples.ProgressBar/ManyParallelTasks.cs` — 8 threads with varying totals (8-220) and sleep times (0.05-3s). Html title and bottomToolbar. IsBackground=true, 500ms join timeout. Port of `progress-bar/many-parallel-tasks.py`. FR-034
- [ ] T032 [P] [US4] Implement `LotOfParallelTasks.Run()` in `examples/Stroke.Examples.ProgressBar/LotOfParallelTasks.cs` — 160 threads, random total (50-200), random sleep (0.05-0.20s), RunTask (complete) vs StopTask (break at random point, label += " BREAK"). IsBackground=true, 500ms join timeout. Port of `progress-bar/a-lot-of-parallel-tasks.py`. FR-035
- [ ] T033 [US4] Verify all 4 parallel/nested examples build: `dotnet build examples/Stroke.Examples.ProgressBar/Stroke.Examples.ProgressBar.csproj` (runtime test deferred to Feature 71)

**Checkpoint**: All 4 parallel/nested progress bar examples compile. Threading model validated.

---

## Phase 7: User Story 5 — Progress Bar with Key Bindings (Priority: P5)

**Goal**: Implement the advanced progress bar example combining KeyBindings, PatchStdout, and cancel mechanism.

**Independent Test**: Run CustomKeyBindings, press 'f' to see text above bar, press 'q' to cancel, verify clean exit.

**Dependency**: Feature 71 (ProgressBar API) must be implemented for runtime testing.

### Implementation for User Story 5

- [ ] T034 [US5] Implement `CustomKeyBindings.Run()` in `examples/Stroke.Examples.ProgressBar/CustomKeyBindings.cs` — KeyBindings: 'f' prints "You pressed `f`." via PatchStdout, 'q' sets cancel flag and breaks loop, 'x' raises KeyboardInterruptException. Html bottom toolbar with key hints. 800 items, 10ms sleep, check cancel each iteration. Port of `progress-bar/custom-key-bindings.py`. FR-033
- [ ] T035 [US5] Verify CustomKeyBindings builds: `dotnet build examples/Stroke.Examples.ProgressBar/Stroke.Examples.ProgressBar.csproj` (runtime test deferred to Feature 71)

**Checkpoint**: All 15 progress bar examples compile. Key bindings integration validated.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across both projects

- [ ] T036 Verify full solution builds cleanly: `dotnet build examples/Stroke.Examples.sln` with zero warnings and zero errors
- [ ] T037 Run comprehensive TUI Driver verification on all 9 PrintText examples — launch each one individually, capture text output, verify visible formatted content and clean exit per quickstart.md
- [ ] T038 Verify PrintText Program.cs usage: `dotnet run --project examples/Stroke.Examples.PrintText` shows all 9 example names; running with unknown name shows error and exits with code 1 (FR-004, FR-005)
- [ ] T039 Verify ProgressBar Program.cs usage: `dotnet run --project examples/Stroke.Examples.ProgressBar` shows all 15 example names; running with unknown name shows error and exits with code 1 (FR-018, FR-019)
- [ ] T040 Verify each example class matches its Python Prompt Toolkit original — spot-check 3-5 examples for faithful port behavior (FR-037)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 PrintText (Phase 3)**: Depends on Foundational — can start immediately after Phase 2
- **US2 Basic ProgressBar (Phase 4)**: Depends on Foundational — can start in parallel with US1
- **US3 Styled ProgressBar (Phase 5)**: Depends on Foundational — can start in parallel with US1/US2
- **US4 Parallel ProgressBar (Phase 6)**: Depends on Foundational — can start in parallel with US1/US2/US3
- **US5 KeyBindings ProgressBar (Phase 7)**: Depends on Foundational — can start in parallel with all others
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Independent — all APIs exist. **Recommended MVP scope.**
- **US2 (P2)**: Independent of US1 — but depends on Feature 71 for runtime testing
- **US3 (P3)**: Independent of US1/US2 — depends on Feature 71 for runtime testing
- **US4 (P4)**: Independent of US1/US2/US3 — depends on Feature 71 for runtime testing
- **US5 (P5)**: Independent of all others — depends on Feature 71 for runtime testing

### External Dependency

- **Feature 71 (ProgressBar API)**: Blocks runtime testing of US2-US5 (15 examples). All progress bar examples will be written to compile against the expected API surface. Full verification deferred until Feature 71 lands.

### Within Each User Story

- All example files are independent (different classes, different files) → [P] parallel
- Verification task depends on all example files within that story

### Parallel Opportunities

```text
Phase 2:  T005 ──┐
          T006 ──┤  (2 files in parallel)
                 ↓
Phase 3:  T007 ─┐
          T008 ─┤
          T009 ─┤
          T010 ─┤
          T011 ─┤  (9 files in parallel — all independent)
          T012 ─┤
          T013 ─┤
          T014 ─┤
          T015 ─┤
                ↓
          T016   (verify)

Phases 4-7 can also run in parallel with Phase 3 (different projects/files)
```

---

## Parallel Example: User Story 1

```bash
# Launch all 9 PrintText example implementations together:
Task: "Implement PrintFormattedText in examples/Stroke.Examples.PrintText/PrintFormattedText.cs"
Task: "Implement AnsiColors in examples/Stroke.Examples.PrintText/AnsiColors.cs"
Task: "Implement Ansi in examples/Stroke.Examples.PrintText/Ansi.cs"
Task: "Implement HtmlExample in examples/Stroke.Examples.PrintText/Html.cs"
Task: "Implement NamedColors in examples/Stroke.Examples.PrintText/NamedColors.cs"
Task: "Implement PrintFrame in examples/Stroke.Examples.PrintText/PrintFrame.cs"
Task: "Implement TrueColorDemo in examples/Stroke.Examples.PrintText/TrueColorDemo.cs"
Task: "Implement PygmentsTokens in examples/Stroke.Examples.PrintText/PygmentsTokens.cs"
Task: "Implement LogoAnsiArt in examples/Stroke.Examples.PrintText/LogoAnsiArt.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (2 .csproj + solution update)
2. Complete Phase 2: Foundational (2 Program.cs files)
3. Complete Phase 3: User Story 1 (9 PrintText examples)
4. **STOP and VALIDATE**: Run all 9 examples via TUI Driver
5. Deliver: 9 fully working print text examples with zero external dependencies

### Incremental Delivery

1. Setup + Foundational → Both projects buildable
2. Add US1 (9 PrintText examples) → Test independently → **MVP delivered!**
3. Add US2 (2 basic ProgressBar examples) → Compiles (runtime test pending Feature 71)
4. Add US3 (8 styled ProgressBar examples) → Compiles (runtime test pending Feature 71)
5. Add US4 (4 parallel ProgressBar examples) → Compiles (runtime test pending Feature 71)
6. Add US5 (1 key bindings ProgressBar example) → Compiles (runtime test pending Feature 71)
7. Polish → Full validation when Feature 71 arrives

### Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable (PrintText) or compilable (ProgressBar)
- Commit after each phase or logical group
- Stop at any checkpoint to validate story independently
