# Tasks: Prompt Examples (Complete Set)

**Input**: Design documents from `/specs/065-prompt-examples/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: No xUnit tests required. Examples are verified via TUI Driver or manual testing per spec.md scope boundaries.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story. Within each story, examples follow the bottom-up dependency order from quickstart.md.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create subdirectories and verify the existing project builds cleanly before adding any new examples.

- [ ] T001 Verify existing project builds: `dotnet build examples/Stroke.Examples.Prompts/Stroke.Examples.Prompts.csproj`
- [ ] T002 [P] Create `examples/Stroke.Examples.Prompts/History/` subdirectory
- [ ] T003 [P] Create `examples/Stroke.Examples.Prompts/WithFrames/` subdirectory

**Checkpoint**: Project builds, all 4 existing examples still work, subdirectories exist.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: No foundational tasks needed — the Stroke library is fully implemented and the project infrastructure already exists. All user stories can begin immediately after Phase 1.

**⚠️ SKIP**: Proceed directly to user story phases.

---

## Phase 3: User Story 1 — Developer Runs Basic Prompt Examples (Priority: P1) 🎯 MVP

**Goal**: Implement 12 new basic prompt examples (#2–#13) that demonstrate core PromptSession API features: defaults, Vi mode, passwords, multiline, confirmation, placeholders, mouse, wrapping, REPL loops, and TTY enforcement.

**Independent Test**: Run each example with `dotnet run --project examples/Stroke.Examples.Prompts -- <name>`, type input, press Enter, verify output.

### Implementation for User Story 1

- [ ] T004 [P] [US1] Implement GetInputWithDefault example in `examples/Stroke.Examples.Prompts/GetInputWithDefault.cs` — single prompt with `default` parameter pre-filled with current username
- [ ] T005 [P] [US1] Implement GetInputViMode example in `examples/Stroke.Examples.Prompts/GetInputViMode.cs` — prompt with `editingMode: EditingMode.Vi`
- [ ] T006 [P] [US1] Implement GetPassword example in `examples/Stroke.Examples.Prompts/GetPassword.cs` — prompt with `isPassword: true`
- [ ] T007 [P] [US1] Implement GetMultilineInput example in `examples/Stroke.Examples.Prompts/GetMultilineInput.cs` — prompt with `multiline: true` and prompt continuation
- [ ] T008 [P] [US1] Implement AcceptDefault example in `examples/Stroke.Examples.Prompts/AcceptDefault.cs` — prompt with `acceptDefault: true` that auto-accepts
- [ ] T009 [P] [US1] Implement ConfirmationPrompt example in `examples/Stroke.Examples.Prompts/ConfirmationPrompt.cs` — `Prompt.Confirm()` returning bool
- [ ] T010 [P] [US1] Implement PlaceholderText example in `examples/Stroke.Examples.Prompts/PlaceholderText.cs` — prompt with `placeholder` parameter showing gray text
- [ ] T011 [P] [US1] Implement MouseSupport example in `examples/Stroke.Examples.Prompts/MouseSupport.cs` — multiline prompt with `mouseSupport: true`
- [ ] T012 [P] [US1] Implement NoWrapping example in `examples/Stroke.Examples.Prompts/NoWrapping.cs` — prompt with `wrapLines: false` for horizontal scrolling
- [ ] T013 [P] [US1] Implement MultilinePrompt example in `examples/Stroke.Examples.Prompts/MultilinePrompt.cs` — basic multiline input variant
- [ ] T014 [P] [US1] Implement OperateAndGetNext example in `examples/Stroke.Examples.Prompts/OperateAndGetNext.cs` — REPL loop with `PromptSession` and history navigation (FR-018: break on EOFException)
- [ ] T015 [P] [US1] Implement EnforceTtyInputOutput example in `examples/Stroke.Examples.Prompts/EnforceTtyInputOutput.cs` — prompt that opens /dev/tty (or Windows console) directly when stdin is piped, matching Python's `create_pipe_input()` pattern
- [ ] T016 [US1] Add US1 routing entries to `examples/Stroke.Examples.Prompts/Program.cs` — add 12 entries: `get-input-with-default`, `get-input-vi-mode`, `get-password`, `get-multiline-input`, `accept-default`, `confirmation-prompt`, `placeholder-text`, `mouse-support`, `no-wrapping`, `multiline-prompt`, `operate-and-get-next`, `enforce-tty-input-output`
- [ ] T017 [US1] Build and verify US1: `dotnet build` succeeds and all 16 examples (4 existing + 12 new) are listed in usage output

**Checkpoint**: 16 examples total (4 existing + 12 new). All basic prompts work independently.

---

## Phase 4: User Story 2 — Developer Explores Styling and Formatting Examples (Priority: P1)

**Goal**: Implement 9 styling examples (#14–#22) demonstrating colors, toolbars, right prompts, dynamic clocks, cursor shapes, terminal titles, and theme toggling.

**Independent Test**: Launch each styling example and visually verify colors, toolbar placement, and dynamic updates render correctly.

### Implementation for User Story 2

- [ ] T018 [P] [US2] Implement GetPasswordWithToggle example in `examples/Stroke.Examples.Prompts/GetPasswordWithToggle.cs` — password prompt with Ctrl-T toggle between masked/visible via custom key binding
- [ ] T019 [P] [US2] Implement ColoredPrompt example in `examples/Stroke.Examples.Prompts/ColoredPrompt.cs` — 3 variants: style tuples, Html, Ansi (FR-010)
- [ ] T020 [P] [US2] Implement BottomToolbar example in `examples/Stroke.Examples.Prompts/BottomToolbar.cs` — 7 toolbar variants: fixed text, callable, HTML, ANSI, styled, token tuples, multiline (FR-009)
- [ ] T021 [P] [US2] Implement RightPrompt example in `examples/Stroke.Examples.Prompts/RightPrompt.cs` — prompt with `rprompt` parameter for right-aligned text
- [ ] T022 [P] [US2] Implement ClockInput example in `examples/Stroke.Examples.Prompts/ClockInput.cs` — prompt with `refreshInterval` showing live time updates
- [ ] T023 [P] [US2] Implement FancyZshPrompt example in `examples/Stroke.Examples.Prompts/FancyZshPrompt.cs` — REPL loop with dynamic width padding between left/right prompt parts (FR-017, FR-018: break on EOFException)
- [ ] T024 [P] [US2] Implement TerminalTitle example in `examples/Stroke.Examples.Prompts/TerminalTitle.cs` — sets terminal window title via escape sequence
- [ ] T025 [P] [US2] Implement SwapLightDarkColors example in `examples/Stroke.Examples.Prompts/SwapLightDarkColors.cs` — `SwapLightAndDarkStyleTransformation` with Ctrl-T toggle
- [ ] T026 [P] [US2] Implement CursorShapes example in `examples/Stroke.Examples.Prompts/CursorShapes.cs` — demonstrates block, underline, beam, and `ModalCursorShapeConfig`
- [ ] T027 [US2] Add US2 routing entries to `examples/Stroke.Examples.Prompts/Program.cs` — add 9 entries: `get-password-with-toggle`, `colored-prompt`, `bottom-toolbar`, `right-prompt`, `clock-input`, `fancy-zsh-prompt`, `terminal-title`, `swap-light-dark-colors`, `cursor-shapes`
- [ ] T028 [US2] Build and verify US2: `dotnet build` succeeds and all 25 examples are listed in usage output

**Checkpoint**: 25 examples total. All styling/formatting examples render correctly.

---

## Phase 5: User Story 3 — Developer Uses Completion Examples (Priority: P1)

**Goal**: Implement 10 new auto-completion examples (#32–#42) in the `AutoCompletion/` subdirectory demonstrating triggers, styles, colors, merging, fuzzy matching, multi-column, nesting, and threaded loading.

**Independent Test**: Launch each completion example, type partial input, trigger completion (Tab or Ctrl-Space), verify completions appear correctly.

### Implementation for User Story 3

- [ ] T029 [P] [US3] Implement ControlSpaceTrigger example in `examples/Stroke.Examples.Prompts/AutoCompletion/ControlSpaceTrigger.cs` — completion triggered by Ctrl-Space instead of Tab
- [ ] T030 [P] [US3] Implement ReadlineStyle example in `examples/Stroke.Examples.Prompts/AutoCompletion/ReadlineStyle.cs` — `CompleteStyle.ReadlineLike` display
- [ ] T031 [P] [US3] Implement ColoredCompletions example in `examples/Stroke.Examples.Prompts/AutoCompletion/ColoredCompletions.cs` — completions with per-item color styling
- [ ] T032 [P] [US3] Implement FormattedCompletions example in `examples/Stroke.Examples.Prompts/AutoCompletion/FormattedCompletions.cs` — HTML-formatted display text and meta descriptions in custom completer
- [ ] T033 [P] [US3] Implement MergedCompleters example in `examples/Stroke.Examples.Prompts/AutoCompletion/MergedCompleters.cs` — `CompletionUtils.Merge()` combining multiple completers
- [ ] T034 [P] [US3] Implement FuzzyCustomCompleter example in `examples/Stroke.Examples.Prompts/AutoCompletion/FuzzyCustomCompleter.cs` — custom `ICompleter` wrapped in `FuzzyCompleter`
- [ ] T035 [P] [US3] Implement MultiColumn example in `examples/Stroke.Examples.Prompts/AutoCompletion/MultiColumn.cs` — `CompleteStyle.MultiColumn` grid display
- [ ] T036 [P] [US3] Implement MultiColumnWithMeta example in `examples/Stroke.Examples.Prompts/AutoCompletion/MultiColumnWithMeta.cs` — multi-column with metadata descriptions
- [ ] T037 [P] [US3] Implement NestedCompletion example in `examples/Stroke.Examples.Prompts/AutoCompletion/NestedCompletion.cs` — `NestedCompleter.FromNestedDict()` for hierarchical commands
- [ ] T038 [P] [US3] Implement SlowCompletions example in `examples/Stroke.Examples.Prompts/AutoCompletion/SlowCompletions.cs` — custom `ICompleter` with 200ms delay, `completeInThread: true`, loading counter in toolbar (FR-013)
- [ ] T039 [US3] Add US3 routing entries to `examples/Stroke.Examples.Prompts/Program.cs` — add 10 entries: `auto-completion/control-space-trigger`, `auto-completion/readline-style`, `auto-completion/colored-completions`, `auto-completion/formatted-completions`, `auto-completion/merged-completers`, `auto-completion/fuzzy-custom-completer`, `auto-completion/multi-column`, `auto-completion/multi-column-with-meta`, `auto-completion/nested-completion`, `auto-completion/slow-completions`
- [ ] T040 [US3] Build and verify US3: `dotnet build` succeeds and all 35 examples are listed in usage output

**Checkpoint**: 35 examples total. All completion variants work with correct triggers and display modes.

---

## Phase 6: User Story 4 — Developer Uses Key Binding and Editing Mode Examples (Priority: P2)

**Goal**: Implement 5 key binding examples (#23–#27) demonstrating custom keys, Vi operators/text objects, system prompts, mode switching, and autocorrection.

**Independent Test**: Launch each example, press documented key combinations, verify expected behavior.

### Implementation for User Story 4

- [ ] T041 [P] [US4] Implement CustomKeyBinding example in `examples/Stroke.Examples.Prompts/CustomKeyBinding.cs` — F4 insertion, multi-key sequences (xy→z, abc→d), Ctrl-T with RunInTerminal, Ctrl-K async handler (FR-011)
- [ ] T042 [P] [US4] Implement CustomViOperator example in `examples/Stroke.Examples.Prompts/CustomViOperator.cs` — custom 'R' operator (reverse text) and 'A' text object (select all) in Vi mode (FR-016)
- [ ] T043 [P] [US4] Implement SystemPrompt example in `examples/Stroke.Examples.Prompts/SystemPrompt.cs` — `enableSystemPrompt: true`, `enableSuspend: true` for Meta-! and Ctrl-Z
- [ ] T044 [P] [US4] Implement SwitchViEmacs example in `examples/Stroke.Examples.Prompts/SwitchViEmacs.cs` — F4 toggles `EditingMode` with toolbar showing current mode
- [ ] T045 [P] [US4] Implement Autocorrection example in `examples/Stroke.Examples.Prompts/Autocorrection.cs` — space-triggered auto-correction via custom key binding
- [ ] T046 [US4] Add US4 routing entries to `examples/Stroke.Examples.Prompts/Program.cs` — add 5 entries: `custom-key-binding`, `custom-vi-operator`, `system-prompt`, `switch-vi-emacs`, `autocorrection`
- [ ] T047 [US4] Build and verify US4: `dotnet build` succeeds and all 40 examples are listed in usage output

**Checkpoint**: 40 examples total. All key binding examples respond to documented key combinations.

---

## Phase 7: User Story 5 — Developer Uses History and Suggestion Examples (Priority: P2)

**Goal**: Implement 4 history/suggestion examples (#29–#30, #43–#44) demonstrating persistent history, slow history, partial matching, and multi-line auto-suggestions.

**Independent Test**: Launch each example, verify history persistence across runs and suggestion rendering.

### Implementation for User Story 5

- [ ] T048 [P] [US5] Implement PersistentHistory example in `examples/Stroke.Examples.Prompts/History/PersistentHistory.cs` — `FileHistory` with temp file for cross-session persistence (FR-014)
- [ ] T049 [P] [US5] Implement SlowHistory example in `examples/Stroke.Examples.Prompts/History/SlowHistory.cs` — custom `IHistory` with simulated delay + `ThreadedHistory` for background loading
- [ ] T050 [P] [US5] Implement UpArrowPartialMatch example in `examples/Stroke.Examples.Prompts/UpArrowPartialMatch.cs` — `enableHistorySearch: true` for partial string matching on up-arrow
- [ ] T051 [P] [US5] Implement MultilineAutosuggest example in `examples/Stroke.Examples.Prompts/MultilineAutosuggest.cs` — custom `IAutoSuggest` + custom `IProcessor` for multi-line suggestion rendering (FR-015)
- [ ] T052 [US5] Add US5 routing entries to `examples/Stroke.Examples.Prompts/Program.cs` — add 4 entries: `history/persistent-history`, `history/slow-history`, `up-arrow-partial-match`, `multiline-autosuggest`
- [ ] T053 [US5] Build and verify US5: `dotnet build` succeeds and all 44 examples are listed in usage output

**Checkpoint**: 44 examples total. History persists across runs, suggestions render correctly.

---

## Phase 8: User Story 6 — Developer Uses Validation, Lexing, and Grammar Examples (Priority: P2)

**Goal**: Implement 4 validation/lexing examples (#45–#48) demonstrating input validation, grammar-based REPL, HTML syntax highlighting, and custom rainbow lexer.

**Independent Test**: Launch each example, verify validation messages, syntax highlighting colors, and grammar-based evaluation.

### Implementation for User Story 6

- [ ] T054 [P] [US6] Implement InputValidation example in `examples/Stroke.Examples.Prompts/InputValidation.cs` — `ValidatorBase.FromCallable()` checking for '@' character
- [ ] T055 [P] [US6] Implement RegularLanguage example in `examples/Stroke.Examples.Prompts/RegularLanguage.cs` — calculator REPL with `Grammar.Compile()`, `GrammarCompleter`, `GrammarLexer`, `GrammarValidator`, add/sub/mul/div/sin/cos operations (FR-012, FR-018: break on EOFException)
- [ ] T056 [P] [US6] Implement HtmlInput example in `examples/Stroke.Examples.Prompts/HtmlInput.cs` — prompt with `PygmentsLexer` for HTML syntax highlighting
- [ ] T057 [P] [US6] Implement CustomLexer example in `examples/Stroke.Examples.Prompts/CustomLexer.cs` — custom `ILexer` with rainbow character coloring
- [ ] T058 [US6] Add US6 routing entries to `examples/Stroke.Examples.Prompts/Program.cs` — add 4 entries: `input-validation`, `regular-language`, `html-input`, `custom-lexer`
- [ ] T059 [US6] Build and verify US6: `dotnet build` succeeds and all 48 examples are listed in usage output

**Checkpoint**: 48 examples total. Validation, highlighting, and grammar REPL work correctly.

---

## Phase 9: User Story 7 — Developer Uses Advanced Feature Examples (Priority: P3)

**Goal**: Implement 5 advanced examples (#49–#53) demonstrating async prompts, stdout patching, input hooks, shell integration markers, and system clipboard.

**Independent Test**: Launch each example and verify the specific advanced behavior.

### Implementation for User Story 7

- [ ] T060 [P] [US7] Implement AsyncPrompt example in `examples/Stroke.Examples.Prompts/AsyncPrompt.cs` — background tasks printing above prompt using `StdoutPatching.PatchStdout()` and `PromptAsync()`
- [ ] T061 [P] [US7] Implement PatchStdout example in `examples/Stroke.Examples.Prompts/PatchStdout.cs` — `using (StdoutPatching.PatchStdout())` with background thread writing every second
- [ ] T062 [P] [US7] Implement InputHook example in `examples/Stroke.Examples.Prompts/InputHook.cs` — `inputHook` parameter demonstrating event loop integration
- [ ] T063 [P] [US7] Implement ShellIntegration example in `examples/Stroke.Examples.Prompts/ShellIntegration.cs` — OSC 133 escape markers (A/B/C/D) around prompts for iTerm2/FinalTerm
- [ ] T064 [P] [US7] Implement SystemClipboard example in `examples/Stroke.Examples.Prompts/SystemClipboard.cs` — prompt with system clipboard integration for yank/paste
- [ ] T065 [US7] Add US7 routing entries to `examples/Stroke.Examples.Prompts/Program.cs` — add 5 entries: `async-prompt`, `patch-stdout`, `input-hook`, `shell-integration`, `system-clipboard`
- [ ] T066 [US7] Build and verify US7: `dotnet build` succeeds and all 53 examples are listed in usage output

**Checkpoint**: 53 examples total. Advanced features work correctly.

---

## Phase 10: User Story 8 — Developer Uses Frame Examples (Priority: P3)

**Goal**: Implement 3 frame examples (#54–#56) in the `WithFrames/` subdirectory demonstrating frame borders, dynamic frame styling, and frames with completion.

**Independent Test**: Launch each frame example and verify border rendering, color changes, and completion coexistence.

### Implementation for User Story 8

- [ ] T067 [P] [US8] Implement BasicFrame example in `examples/Stroke.Examples.Prompts/WithFrames/BasicFrame.cs` — prompt with `showFrame: true` for border decoration
- [ ] T068 [P] [US8] Implement GrayFrameOnAccept example in `examples/Stroke.Examples.Prompts/WithFrames/GrayFrameOnAccept.cs` — frame color transition to gray on Enter using `AppFilters.IsDone`
- [ ] T069 [P] [US8] Implement FrameWithCompletion example in `examples/Stroke.Examples.Prompts/WithFrames/FrameWithCompletion.cs` — frame + completion menu + bottom toolbar combined
- [ ] T070 [US8] Add US8 routing entries to `examples/Stroke.Examples.Prompts/Program.cs` — add 3 entries: `with-frames/basic-frame`, `with-frames/gray-frame-on-accept`, `with-frames/frame-with-completion`
- [ ] T071 [US8] Build and verify US8: `dotnet build` succeeds and all 56 examples are listed in usage output

**Checkpoint**: 56 examples total. All frame examples render borders correctly.

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Final routing update with backward-compatibility aliases, usage help update, and comprehensive verification.

- [ ] T072 Add backward-compatibility aliases to `examples/Stroke.Examples.Prompts/Program.cs` — add `auto-completion/basic-completion` → `Autocompletion.Run` and `auto-completion/fuzzy-word-completer` → `FuzzyWordCompleterExample.Run` aliases (total: 58 routing entries)
- [ ] T073 Update Program.cs XML doc comments and `ShowUsage()` to reflect all 56 examples organized by category in `examples/Stroke.Examples.Prompts/Program.cs`
- [ ] T074 Full build verification: `dotnet build examples/Stroke.Examples.Prompts/Stroke.Examples.Prompts.csproj` succeeds with zero warnings
- [ ] T075 Verify routing completeness: run `dotnet run --project examples/Stroke.Examples.Prompts` and confirm 58 routing entries are listed alphabetically
- [ ] T076 TUI Driver spot-check: verify representative examples via TUI Driver (SC-010) — for each: launch with `tui_launch`, wait for prompt within 5s (SC-002), send input via `tui_send_text`, press Enter, verify echoed output via `tui_text`. Examples: GetInput, BottomToolbar, ColoredPrompt, CustomKeyBinding, InputValidation, RegularLanguage, SlowCompletions, WithFrames/BasicFrame, ConfirmationPrompt
- [ ] T077 Verify all example files are ≤ 200 LOC (FR-020) and follow the class contract from `specs/065-prompt-examples/contracts/example-contract.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Skipped — no blocking prerequisites
- **User Stories (Phase 3–10)**: All depend on Phase 1 completion only
  - User stories CAN proceed in parallel (all examples use independent files)
  - Recommended sequential order: P1 stories first (US1–US3), then P2 (US4–US6), then P3 (US7–US8)
- **Polish (Phase 11)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 1 — No dependencies on other stories
- **US2 (P1)**: Can start after Phase 1 — No dependencies on other stories
- **US3 (P1)**: Can start after Phase 1 — No dependencies on other stories
- **US4 (P2)**: Can start after Phase 1 — No dependencies on other stories
- **US5 (P2)**: Can start after Phase 1 — No dependencies on other stories
- **US6 (P2)**: Can start after Phase 1 — No dependencies on other stories
- **US7 (P3)**: Can start after Phase 1 — No dependencies on other stories
- **US8 (P3)**: Can start after Phase 1 — No dependencies on other stories

### Within Each User Story

- All example files within a story are parallelizable ([P] marked)
- Routing update (Program.cs) depends on all example files in that story being complete
- Build verification depends on routing update being complete

### Parallel Opportunities

- **Maximum parallelism**: All [P]-marked example tasks within any story can run in parallel (up to 12 simultaneous tasks in US1)
- **Cross-story parallelism**: All 8 user stories can run in parallel since each creates independent files
- **Program.cs serialization**: Routing updates within each story must be sequential (one story at a time modifies Program.cs), OR all routing can be done in a single final task

---

## Parallel Example: User Story 1

```bash
# Launch all 12 basic prompt examples in parallel:
Task: "Implement GetInputWithDefault in GetInputWithDefault.cs"
Task: "Implement GetInputViMode in GetInputViMode.cs"
Task: "Implement GetPassword in GetPassword.cs"
Task: "Implement GetMultilineInput in GetMultilineInput.cs"
Task: "Implement AcceptDefault in AcceptDefault.cs"
Task: "Implement ConfirmationPrompt in ConfirmationPrompt.cs"
Task: "Implement PlaceholderText in PlaceholderText.cs"
Task: "Implement MouseSupport in MouseSupport.cs"
Task: "Implement NoWrapping in NoWrapping.cs"
Task: "Implement MultilinePrompt in MultilinePrompt.cs"
Task: "Implement OperateAndGetNext in OperateAndGetNext.cs"
Task: "Implement EnforceTtyInputOutput in EnforceTtyInputOutput.cs"

# Then sequentially:
Task: "Add US1 routing entries to Program.cs"
Task: "Build and verify US1"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (verify build, create subdirectories)
2. Complete Phase 3: User Story 1 (12 basic prompt examples)
3. **STOP and VALIDATE**: Test all 16 examples independently
4. This delivers the foundational examples that every new user needs

### Incremental Delivery

1. Phase 1 → Setup ready
2. Phase 3 (US1) → 16 basic examples → Test → **MVP!**
3. Phase 4 (US2) → 25 styled examples → Test
4. Phase 5 (US3) → 35 completion examples → Test
5. Phase 6 (US4) → 40 key binding examples → Test
6. Phase 7 (US5) → 44 history/suggestion examples → Test
7. Phase 8 (US6) → 48 validation/lexing examples → Test
8. Phase 9 (US7) → 53 advanced examples → Test
9. Phase 10 (US8) → 56 frame examples → Test
10. Phase 11 → Polish, aliases, TUI verification → **Complete!**

Each story adds value without breaking previous stories.

---

## Notes

- All 52 new example files are independent — maximum parallelism possible
- Program.cs is the only shared file — routing updates must be serialized
- Existing 4 examples (GetInput, AutoSuggestion, Autocompletion, FuzzyWordCompleter) are NOT modified
- Each example MUST follow the class contract: `public static class` with `public static void Run()`, catch `KeyboardInterruptException`/`EOFException`
- Each example MUST reference its Python source in XML doc comments
- Max 200 LOC per file (FR-020)
- All examples use `namespace Stroke.Examples.Prompts;` (flat namespace, no sub-namespaces)
