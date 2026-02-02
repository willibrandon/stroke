# Tasks: Shortcut Utilities

**Input**: Design documents from `/specs/046-shortcut-utils/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/shortcut-utils-api.md, quickstart.md

**Tests**: Tests are required per Constitution VIII (80% line coverage, SC-008). Tests use real infrastructure only — no mocks, no fakes, no FluentAssertions.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Shortcuts/`
- **Tests**: `tests/Stroke.Tests/Shortcuts/`

---

## Phase 1: Setup

**Purpose**: Create directories and verify all infrastructure dependencies compile

- [ ] T001 Create source directory `src/Stroke/Shortcuts/` and test directory `tests/Stroke.Tests/Shortcuts/`
- [ ] T002 Verify prerequisite infrastructure compiles — confirm existence and accessibility of: `RendererUtils.PrintFormattedText` (src/Stroke/Rendering/RendererUtils.cs), `AppContext.GetAppOrNull`/`GetAppSession` (src/Stroke/Application/AppContext.cs), `RunInTerminal.RunAsync` (src/Stroke/Application/RunInTerminal.cs), `StyleMerger.MergeStyles` (src/Stroke/Styles/StyleMerger.cs), `DefaultStyles.DefaultUiStyle`/`DefaultPygmentsStyle` (src/Stroke/Styles/DefaultStyles.cs), `OutputFactory.Create` (src/Stroke/Output/OutputFactory.cs), `DummyInput` (src/Stroke/Input/DummyInput.cs), `FormattedTextUtils.ToFormattedText` (src/Stroke/FormattedText/FormattedTextUtils.cs)

**Checkpoint**: Directory structure exists and all 8 prerequisite types are accessible from the Shortcuts layer

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement the private `CreateMergedStyle` helper — required by both `Print` (US1/US2) and `PrintContainer` (US3)

**⚠️ CRITICAL**: Both US1 and US3 depend on `CreateMergedStyle`. It must be implemented first.

- [ ] T003 Implement `CreateMergedStyle` private static method in `src/Stroke/Shortcuts/FormattedTextOutput.cs` — Create the `FormattedTextOutput` static class with the private `CreateMergedStyle(IStyle?, bool)` method. Port from Python's `_create_merged_style`: build list starting with `DefaultStyles.DefaultUiStyle`, conditionally add `DefaultStyles.DefaultPygmentsStyle` (when `includeDefaultPygmentsStyle` is true), conditionally add user `style` (when non-null), return `StyleMerger.MergeStyles(styles)`. Include XML doc comments per contract. Merge order: default UI (lowest) → Pygments (conditional) → user (highest). Reference: Python source `shortcuts/utils.py` lines 202-214.

**Checkpoint**: `FormattedTextOutput.cs` exists with the class shell and `CreateMergedStyle` compiles

---

## Phase 3: User Story 1 — Print Formatted Text to Terminal (Priority: P1) 🎯 MVP

**Goal**: A developer can call `FormattedTextOutput.Print` with HTML, ANSI, or FormattedText objects and see correctly styled output with `sep`/`end` semantics matching Python's `print()`.

**Independent Test**: Call `FormattedTextOutput.Print` with various input types and capture output via `OutputFactory.Create(stdout: stringWriter)`. Verify rendered content matches expectations. Use `Vt100Output.FromPty(stringWriter)` for escape sequence inspection.

### Tests for User Story 1

- [ ] T004 [P] [US1] Write tests for `Print` single-value overload in `tests/Stroke.Tests/Shortcuts/FormattedTextOutputTests.cs` — Test plain string prints "Hello\n" (US1-AS1), HTML renders with bold styling (US1-AS2), custom `end` parameter replaces newline (US1-AS4), custom `file` TextWriter redirects output (US1-AS5), `output`+`file` conflict throws `ArgumentException` (US1-AS6), `flush: true` flushes output (US1-AS7), custom style and colorDepth render correctly (US1-AS8), `includeDefaultPygmentsStyle: false` excludes Pygments style (US1-AS9), explicit `output` parameter bypasses session default (US1-AS10), `file: TextWriter.Null` silently discards output via DummyOutput (edge case). Use `OutputFactory.Create(stdout: new StringWriter())` for capture. Use xUnit assertions only.
- [ ] T005 [P] [US1] Write tests for `Print` multi-value overload in `tests/Stroke.Tests/Shortcuts/FormattedTextOutputTests.cs` — Test multiple values with custom separator (US1-AS3), zero values prints only `end` (FR-013/edge case), empty `sep` concatenates without spacing (FR-002), empty `sep` AND empty `end` produces concatenated output with no trailing newline (edge case), plain `IList` (not `FormattedText`) is converted via `ToString()` (FR-010).

### Implementation for User Story 1

- [ ] T006 [US1] Implement `ToText` private helper method in `src/Stroke/Shortcuts/FormattedTextOutput.cs` — Port Python's `to_text(val)` inner function: if `val is IList && val is not FormattedText`, convert via `FormattedTextUtils.ToFormattedText(new AnyFormattedText(val.ToString() ?? ""))`. Otherwise, convert via `FormattedTextUtils.ToFormattedText(new AnyFormattedText(val), autoConvert: true)`. Returns `IReadOnlyList<StyleAndTextTuple>`. Reference: Python source lines 119-124.
- [ ] T007 [US1] Implement `Print(object[] values, ...)` multi-value overload in `src/Stroke/Shortcuts/FormattedTextOutput.cs` — This is the core implementation. Port from Python lines 41-164: (1) Validate `output` and `file` are not both non-null → throw `ArgumentException`. (2) Resolve output: if `output` non-null use directly; else if `file` non-null use `OutputFactory.Create(stdout: file)`; else use `AppContext.GetAppSession().Output`. (3) Resolve colorDepth: `colorDepth ?? output.GetDefaultColorDepth()`. (4) Build fragments list (`List<StyleAndTextTuple>`): convert each value via `ToText`, join with `sep` fragments between values (skip sep after last value), append `end` fragments. (5) Define render action: wrap fragments as `new AnyFormattedText(new FormattedText(fragments))` and call `RendererUtils.PrintFormattedText(output, wrappedFragments, CreateMergedStyle(style, includeDefaultPygmentsStyle), colorDepth, styleTransformation)`, then if `flush` call `output.Flush()`. (6) Dispatch: if `AppContext.GetAppOrNull()` is non-null, call `RunInTerminal.RunAsync(render).GetAwaiter().GetResult()` to synchronously block until the Task completes (ensures print finishes before returning and exceptions propagate correctly); else call `render()` directly. Include full XML doc comments per contract.
- [ ] T008 [US1] Implement `Print(AnyFormattedText text, ...)` single-value overload in `src/Stroke/Shortcuts/FormattedTextOutput.cs` — Wrap the single `AnyFormattedText` in a one-element `object[]` and delegate to the multi-value `Print` overload, passing through all other parameters (`sep`, `end`, `file`, `flush`, `style`, `output`, `colorDepth`, `styleTransformation`, `includeDefaultPygmentsStyle`). Include full XML doc comments per contract.
- [ ] T009 [US1] Run US1 tests and verify all pass — Execute `dotnet test --filter "FullyQualifiedName~Stroke.Tests.Shortcuts.FormattedTextOutput"` and confirm all US1 acceptance scenarios pass. Fix any failures.

**Checkpoint**: `FormattedTextOutput.Print` works for all input types (string, HTML, ANSI, FormattedText), sep/end/file/flush/style/output/colorDepth parameters, edge cases (zero values, empty sep, IList conversion, output+file conflict). Independently testable via captured output.

---

## Phase 4: User Story 2 — Print While Application Is Running (Priority: P2)

**Goal**: When a developer calls `FormattedTextOutput.Print` while an Application is running, it automatically dispatches through `RunInTerminal.RunAsync` to coordinate with the application display lifecycle.

**Independent Test**: Create a real `Application` with `DummyInput`, start it on a background thread, call `FormattedTextOutput.Print`, verify the printed text appears in the captured output stream. No mocks.

### Tests for User Story 2

- [ ] T010 [US2] Write tests for running-app dispatch in `tests/Stroke.Tests/Shortcuts/FormattedTextOutputTests.cs` — Test: (1) When `AppContext.GetAppOrNull()` returns non-null (running app), Print dispatches through `RunInTerminal.RunAsync` and text is observable in output (US2-AS1). Create a real `Application<object?>` with `DummyInput` on a background thread, call `FormattedTextOutput.Print`, verify text in captured output. (2) When no Application is running, Print executes render directly (US2-AS2) — this is already implicitly tested by US1 tests but add an explicit test confirming `AppContext.GetAppOrNull()` returns null.

### Implementation for User Story 2

> **Note**: The running-app dispatch logic is already implemented in T007 (the multi-value `Print` overload). This phase validates it works correctly with a real Application.

- [ ] T011 [US2] Run US2 tests and verify running-app dispatch works — Execute the US2 tests. The dispatch logic (`AppContext.GetAppOrNull()` → `RunInTerminal.RunAsync`) was implemented in T007. This task validates that the integration with a real running Application produces correct results. Fix any failures.

**Checkpoint**: `FormattedTextOutput.Print` correctly dispatches through `RunInTerminal.RunAsync` when a real Application is running (SC-004). Printing while an app is running produces observable output without corrupting the app's subsequent rendering.

---

## Phase 5: User Story 3 — Print a Layout Container Non-Interactively (Priority: P2)

**Goal**: A developer can render a complex layout container to the terminal as a one-shot display without user interaction by calling `FormattedTextOutput.PrintContainer`.

**Independent Test**: Call `FormattedTextOutput.PrintContainer` with a simple container (e.g., `Frame(TextArea(...))`) and capture output via `OutputFactory.Create(stdout: stringWriter)`. Verify the container renders and the method returns without hanging.

### Tests for User Story 3

- [ ] T012 [P] [US3] Write tests for `PrintContainer` in `tests/Stroke.Tests/Shortcuts/FormattedTextOutputTests.cs` — Test: (1) Container renders to default output and method returns without hanging (US3-AS1). (2) Custom `file` TextWriter redirects container output (US3-AS2). (3) Custom style uses merged style (US3-AS3). (4) Empty container with no visible content completes normally (US3-AS4). (5) PrintContainer works correctly while another Application is already running — creates its own temporary Application independently (edge case). Use real containers from the widget/layout system. Verify output contains rendered content via `StringWriter` capture.

### Implementation for User Story 3

- [ ] T013 [US3] Implement `PrintContainer(AnyContainer, ...)` in `src/Stroke/Shortcuts/FormattedTextOutput.cs` — Port from Python lines 167-199: (1) Resolve output: if `file` non-null use `OutputFactory.Create(stdout: file)`, else use `AppContext.GetAppSession().Output`. (2) Create Application: `new Application<object?>(layout: new Layout(container: container), output: output, input: new DummyInput(), style: CreateMergedStyle(style, includeDefaultPygmentsStyle))`. (3) Run with try/catch: `app.Run(inThread: true)` wrapped in `try { ... } catch (EndOfStreamException) { }` — the `EndOfStreamException` is the expected termination signal from `DummyInput`. Include full XML doc comments per contract.
- [ ] T014 [US3] Run US3 tests and verify all pass — Execute `dotnet test --filter "FullyQualifiedName~Stroke.Tests.Shortcuts.FormattedTextOutput"` focusing on PrintContainer tests. Confirm US3 acceptance scenarios pass (container renders, terminates cleanly, no hanging, EndOfStreamException caught silently). Fix any failures.

**Checkpoint**: `FormattedTextOutput.PrintContainer` renders layout containers non-interactively (SC-005). Works with custom file output, custom styles, and empty containers. Terminates cleanly via DummyInput → EndOfStreamException pattern.

---

## Phase 6: User Story 4 — Terminal Control: Clear, SetTitle, ClearTitle (Priority: P3)

**Goal**: Simple utility functions to clear the terminal screen, set the window title, and clear the title, delegating to the current session's `IOutput`.

**Independent Test**: Capture raw output via `Vt100Output.FromPty(stringWriter)` and verify correct VT100 escape sequences (`\x1b[2J` for erase, `\x1b[0;0H` for cursor home, `\x1b]2;...\x07` for title).

### Tests for User Story 4

- [ ] T015 [P] [US4] Write tests for `TerminalUtils` in `tests/Stroke.Tests/Shortcuts/TerminalUtilsTests.cs` — Test: (1) `Clear()` emits erase screen + cursor home + flush escape sequences (US4-AS1, SC-006). (2) `SetTitle("My App")` emits title-setting escape sequence (US4-AS2, SC-007). (3) `ClearTitle()` calls `SetTitle("")` to reset (US4-AS3, SC-007). Use `Vt100Output.FromPty(new StringWriter())` to capture raw escape sequences and inspect via string assertions. Verify `\x1b[2J` (erase screen), `\x1b[0;0H` (cursor home), `\x1b]2;My App\x07` (set title) sequences.

### Implementation for User Story 4

- [ ] T016 [P] [US4] Implement `TerminalUtils` static class in `src/Stroke/Shortcuts/TerminalUtils.cs` — Port from Python lines 217-239. Three methods, all delegating to `AppContext.GetAppSession().Output`: (1) `Clear()` → `output.EraseScreen()` + `output.CursorGoto(0, 0)` + `output.Flush()`. (2) `SetTitle(string text)` → `output.SetTitle(text)`. (3) `ClearTitle()` → `SetTitle("")`. Include XML doc comments per contract. Mark class and all methods as `public static`. Document thread safety (stateless, inherently thread-safe).
- [ ] T017 [US4] Run US4 tests and verify all pass — Execute `dotnet test --filter "FullyQualifiedName~Stroke.Tests.Shortcuts.TerminalUtils"` and confirm all US4 acceptance scenarios pass. Verify escape sequences match expected patterns. Fix any failures.

**Checkpoint**: `TerminalUtils.Clear`, `SetTitle`, and `ClearTitle` emit correct VT100 escape sequences (SC-006, SC-007). Independently verifiable via raw output inspection.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Coverage validation, style precedence verification, and final quality checks

- [ ] T018 Verify style merging precedence (SC-009) — Add a test in `tests/Stroke.Tests/Shortcuts/FormattedTextOutputTests.cs` that verifies style merge order: user style overrides Pygments style which overrides default UI style. Create real `Style` instances with conflicting rules and verify the rendered output reflects correct precedence (highest-priority style wins). This validates FR-005 and SC-009.
- [ ] T019 Run full test suite and verify 80% line coverage (SC-008) — Execute `dotnet test --collect:"XPlat Code Coverage"` for `FormattedTextOutput` and `TerminalUtils` classes. Verify at least 80% line coverage. Identify and fill any coverage gaps with additional targeted tests if needed.
- [ ] T020 Verify all 6 public API methods are callable (SC-001) — Confirm `FormattedTextOutput.Print` (single), `FormattedTextOutput.Print` (multi), `FormattedTextOutput.PrintContainer`, `TerminalUtils.Clear`, `TerminalUtils.SetTitle`, `TerminalUtils.ClearTitle` are all public static and compile correctly. Verify overload resolution: calling `Print("hello")` resolves to the `AnyFormattedText` overload (not `object[]`), and calling `Print(new object[] { "a", "b" })` resolves to the multi-value overload. Run `dotnet build` and verify no warnings.
- [ ] T021 Run complete `dotnet build` and `dotnet test` — Final validation: build entire solution with zero warnings, run complete test suite, confirm all tests pass. Verify no file exceeds 1,000 LOC (Constitution X).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — `CreateMergedStyle` must exist before any Print/PrintContainer work
- **US1 (Phase 3)**: Depends on Phase 2 — core Print implementation
- **US2 (Phase 4)**: Depends on Phase 3 (T007 specifically) — validates running-app dispatch already built in T007
- **US3 (Phase 5)**: Depends on Phase 2 only — PrintContainer is independent of Print (different code path)
- **US4 (Phase 6)**: Depends on Phase 1 only — TerminalUtils has no dependency on FormattedTextOutput
- **Polish (Phase 7)**: Depends on all user story phases being complete

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational (Phase 2) for `CreateMergedStyle` → **MVP scope**
- **US2 (P2)**: Depends on US1 (the dispatch logic is in the `Print` method implemented in T007)
- **US3 (P2)**: Depends on Foundational (Phase 2) only — can run in parallel with US1
- **US4 (P3)**: No dependency on other stories — can run in parallel with all phases after Setup

### Within Each User Story

- Tests MUST be written FIRST and FAIL before implementation
- Implementation tasks follow dependency order within the story
- Story complete = all tests pass for that story's acceptance scenarios

### Parallel Opportunities

**Phase 3 + Phase 5 + Phase 6**: After Foundational (Phase 2) completes, US1 (Phase 3), US3 (Phase 5), and US4 (Phase 6) can all start in parallel since they touch different methods/files:
- US1: `FormattedTextOutput.Print` implementation + tests in `FormattedTextOutputTests.cs`
- US3: `FormattedTextOutput.PrintContainer` implementation + tests in `FormattedTextOutputTests.cs`
- US4: `TerminalUtils` implementation in `TerminalUtils.cs` + tests in `TerminalUtilsTests.cs`

> Note: US1 and US3 share `FormattedTextOutputTests.cs` so they cannot write tests fully in parallel, but implementation code touches different methods. US4 is fully independent (different source and test files).

**Within Phase 3**: T004 and T005 (tests) can run in parallel, then T006 → T007 → T008 are sequential.

**Within Phase 6**: T015 (tests) and T016 (implementation) can run in parallel since they're different files.

---

## Parallel Example: After Foundational Phase

```text
# After Phase 2 (CreateMergedStyle) completes, launch in parallel:

Agent 1 (US1 — Print):
  T004 → T005 → T006 → T007 → T008 → T009

Agent 2 (US4 — TerminalUtils):
  T015 + T016 (parallel) → T017

# After US1 completes:
Agent 1 (US2 — Running App Dispatch):
  T010 → T011

# After Phase 2 completes (can also run with Agent 1):
Agent 3 (US3 — PrintContainer):
  T012 → T013 → T014
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 2: Foundational — `CreateMergedStyle` (T003)
3. Complete Phase 3: User Story 1 — `Print` (T004-T009)
4. **STOP and VALIDATE**: Run US1 tests independently, verify all acceptance scenarios pass
5. This gives a working `Print` function — the most commonly used API

### Incremental Delivery

1. Setup + Foundational → `CreateMergedStyle` ready
2. Add US1 (Print) → Test → Validate (MVP!)
3. Add US4 (TerminalUtils) → Test → Validate (independent, simple)
4. Add US3 (PrintContainer) → Test → Validate (uses `CreateMergedStyle`)
5. Add US2 (Running App Dispatch) → Test → Validate (requires US1's Print)
6. Polish: coverage check, style precedence verification, final build

### Total Scope

- **21 tasks** across 7 phases
- **US1**: 6 tasks (2 test + 3 impl + 1 verify)
- **US2**: 2 tasks (1 test + 1 verify — dispatch logic built in US1)
- **US3**: 3 tasks (1 test + 1 impl + 1 verify)
- **US4**: 3 tasks (1 test + 1 impl + 1 verify)
- **Setup**: 2 tasks
- **Foundational**: 1 task
- **Polish**: 4 tasks

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- All tests use real infrastructure per Constitution VIII — `OutputFactory.Create`, `Vt100Output.FromPty`, real `Application` with `DummyInput`
- Estimated ~150 LOC implementation + ~400 LOC tests, well under 1,000 LOC per file (Constitution X)
- Python source reference: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/shortcuts/utils.py`
- Spec reference: `/specs/046-shortcut-utils/spec.md`
- Contract reference: `/specs/046-shortcut-utils/contracts/shortcut-utils-api.md`
