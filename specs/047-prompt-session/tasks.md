# Tasks: Prompt Session

**Input**: Design documents from `/specs/047-prompt-session/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: Included — spec references 80% line coverage target (SC-008) and test-mapping.md defines 6 mapped tests + concurrency tests.

**Organization**: Tasks grouped by user story (11 stories) with setup, foundational, and polish phases.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Exact file paths included in all descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Cross-feature prerequisite changes and shared types needed by all user stories

- [X] T001 Make `Application.RefreshInterval` settable with Lock protection in `src/Stroke/Application/Application.cs` — change from `{ get; }` to Lock-protected `{ get; set; }` property (cross-feature change per plan.md Complexity Tracking; required by FR-010 per-prompt override of refreshInterval)
- [X] T002 [P] Create `CompleteStyle` enum (Column, MultiColumn, ReadlineLike) in `src/Stroke/Shortcuts/CompleteStyle.cs` per contract `contracts/complete-style.md` → FR-001
- [X] T003 [P] Create `KeyboardInterruptException` sealed class in `src/Stroke/Shortcuts/KeyboardInterruptException.cs` per contract `contracts/internal-helpers.md` — three constructor overloads (parameterless, message, message+inner) → FR-027, FR-037
- [X] T004 [P] Create `EOFException` sealed class in `src/Stroke/Shortcuts/EOFException.cs` per contract `contracts/internal-helpers.md` — three constructor overloads (parameterless, message, message+inner) → FR-027, FR-037
- [X] T005 [P] Create `PromptContinuationCallable` delegate in `src/Stroke/Shortcuts/PromptContinuationCallable.cs` — `delegate AnyFormattedText PromptContinuationCallable(int promptWidth, int lineNumber, int wrapCount)` per contract `contracts/internal-helpers.md` → FR-013, FR-030

**Checkpoint**: Shared types ready — PromptSession implementation can begin

---

## Phase 2: Foundational (PromptSession Core — Blocking Prerequisites)

**Purpose**: PromptSession constructor, properties, and DynCond — MUST complete before any user story work

**⚠️ CRITICAL**: All user stories depend on the PromptSession core being functional

- [X] T006 Implement `PromptSession<TResult>` constructor and all 36 Lock-protected mutable properties in `src/Stroke/Shortcuts/PromptSession.cs` — 44 constructor parameters per contract `contracts/prompt-session.md`; `FilterOrBool` defaults with `HasValue` sentinel detection for wrapLines/completeWhileTyping/validateWhileTyping/includeDefaultPygmentsStyle (Edge Case 7); exception type validation at construction time via reflection (FR-037); viMode→EditingMode.Vi precedence (Edge Case 1); History defaults to InMemoryHistory, Clipboard defaults to InMemoryClipboard; computed delegation properties: EditingMode delegates to App.EditingMode (get/set), Input delegates to App.Input (get), Output delegates to App.Output (get) → FR-002, FR-003, FR-016, FR-022, FR-023, FR-037
- [X] T007 Implement `DynCond` factory method in `src/Stroke/Shortcuts/PromptSession.cs` — creates `Condition` lambdas that capture session instance and read Lock-protected `FilterOrBool` properties at render time, resolving via `ToFilter()` → FR-016
- [X] T008 Implement `CreateDefaultBuffer` in `src/Stroke/Shortcuts/PromptSession.Buffers.cs` — Buffer with accept handler that exits App with buffer text; DynamicCompleter whose lambda returns `ThreadedCompleter(completer)` when `completeInThread && completer != null`, otherwise returns completer directly (ThreadedCompleter wraps the actual completer, not the DynamicCompleter); DynamicValidator; DynamicAutoSuggest; completeWhileTyping as Condition (true when completeWhileTyping AND NOT enableHistorySearch AND NOT ReadlineLike per FR-018); History reference → FR-004, FR-018, FR-031, FR-032
- [X] T009 Implement `CreateSearchBuffer` in `src/Stroke/Shortcuts/PromptSession.Buffers.cs` — simple Buffer for incremental search → FR-005
- [X] T010 Implement `CreateLayout` in `src/Stroke/Shortcuts/PromptSession.Layout.cs` — FloatContainer with HSplit containing: multiline prompt area (ConditionalContainer), main input Window (BufferControl with 7 processors: HighlightIncrementalSearchProcessor, HighlightSelectionProcessor, ConditionalProcessor(PasswordProcessor), ConditionalProcessor(DisplayMultipleCursors), AppendAutoSuggestion, ConditionalProcessor(HighlightMatchingBracketProcessor), BeforeInput; BufferControl also receives `DynamicLexer(() => session.Lexer)` as its lexer parameter), search buffer control (non-multiline), Floats for CompletionsMenu/MultiColumnCompletionsMenu (visibility gated on CompleteStyle)/RPrompt, ValidationToolbar, SystemToolbar, SearchToolbar (multiline), ArgToolbar (multiline), bottom toolbar Window, optional Frame wrapper (showFrame); GetDefaultBufferControlHeight with reserveSpaceForMenu logic (Edge Case 4) → FR-006, FR-012, FR-017, FR-025, FR-028, FR-029, FR-031
- [X] T011 Implement `SplitMultilinePrompt` internal static helper in `src/Stroke/Shortcuts/PromptSession.Layout.cs` — returns (HasBeforeFragments, Before, FirstInputLine) tuple; uses LayoutUtils.ExplodeTextFragments for newline detection (Edge Case 8) → FR-012, FR-029
- [X] T012 Implement `RPrompt` internal Window subclass in `src/Stroke/Shortcuts/PromptSession.Layout.cs` — FormattedTextControl with WindowAlign.Right, style "class:rprompt" per contract `contracts/internal-helpers.md` → FR-028
- [X] T013 Implement `CreateApplication` in `src/Stroke/Shortcuts/PromptSession.Application.cs` — Application<TResult> with: DynamicStyle, DynamicClipboard, DynamicCursorShapeConfig, style transformation merger (user + SwapLightAndDarkStyleTransformation conditional on swapLightAndDarkColors), eraseWhenDone passthrough; merged key bindings in priority order: inner [auto-suggest, conditional open-in-editor, prompt-specific], outer [DynamicKeyBindings(user)] → FR-007, FR-024, FR-031, FR-033, FR-034
- [X] T014 Implement `CreatePromptBindings` in `src/Stroke/Shortcuts/PromptSession.Application.cs` — prompt-specific key bindings: Enter (accept in single-line or when multiline filter allows), Ctrl-C (Activator.CreateInstance of InterruptException → App.Exit with exception), Ctrl-D (EOF on empty buffer via Activator.CreateInstance of EofException), Tab (DisplayCompletionsLikeReadline when ReadlineLike style), Ctrl-Z (suspend gated on SuspendToBackgroundSupported AND enableSuspend per FR-041) → FR-011, FR-027, FR-041
- [X] T015 Implement helper methods in `src/Stroke/Shortcuts/PromptSession.Helpers.cs` — GetPrompt, GetContinuation (Edge Case 9), GetLinePrefix, GetArgText, InlineArg per contract `contracts/prompt-session.md` → FR-013, FR-030

**Checkpoint**: PromptSession core infrastructure complete — all owned objects (Buffer, Layout, Application) created in constructor. User story implementation can now begin.

---

## Phase 3: User Story 1 — Simple Single-Line Prompt (Priority: P1) 🎯 MVP

**Goal**: Developer can display a prompt, collect single-line text input, and handle Ctrl-C/Ctrl-D exceptions

**Independent Test**: Create PromptSession → call Prompt → type text → press Enter → verify returned string

### Tests for User Story 1

- [X] T016 [P] [US1] Write `CompleteStyleTests` in `tests/Stroke.Tests/Shortcuts/CompleteStyleTests.cs` — enum value existence and count tests → FR-001
- [X] T017 [P] [US1] Write `PromptSessionTests` constructor and property default tests in `tests/Stroke.Tests/Shortcuts/PromptSessionTests.cs` — verify 44 constructor params, Lock-protected property get/set, DynCond resolution, FilterOrBool HasValue defaults, exception type validation (FR-037), viMode precedence (Edge Case 1), computed delegation properties (EditingMode/Input/Output delegate to App), eraseWhenDone passed to CreateApplication (not stored as mutable property) → FR-002, FR-003, FR-016, FR-022, FR-023, FR-024, FR-037
- [X] T018 [P] [US1] Write `PromptSessionBindingsTests` in `tests/Stroke.Tests/Shortcuts/PromptSessionBindingsTests.cs` — test Enter accept, Ctrl-C throws KeyboardInterruptException, Ctrl-D throws EOFException on empty buffer, Ctrl-D no-op on non-empty buffer → FR-011, FR-027

### Implementation for User Story 1

- [X] T019 [US1] Implement `Prompt` (blocking) method in `src/Stroke/Shortcuts/PromptSession.Prompt.cs` — per-prompt override logic (explicit property-by-property null checks per Research R4), DefaultBuffer.Reset with default_ document (FR-038), App.RefreshInterval assignment, dumb terminal placeholder branch (`// Dumb terminal branch — implemented in T040`) returning early if conditions met, AddPreRunCallables, App.Run() with setExceptionHandler/handleSigint/inThread/inputHook passthrough, exception propagation (FR-039) → FR-008, FR-010, FR-036, FR-038, FR-039, FR-040
- [X] T020 [US1] Implement `PromptAsync` method in `src/Stroke/Shortcuts/PromptSession.Prompt.cs` — same override logic as Prompt, App.RunAsync() with handleSigint passthrough (no inThread/inputHook), exception propagation → FR-009, FR-010, FR-039, FR-040
- [X] T021 [US1] Implement `AddPreRunCallables` in `src/Stroke/Shortcuts/PromptSession.Prompt.cs` — append to App.PreRunCallables: execute preRun first, then if acceptDefault schedule DefaultBuffer.ValidateAndHandle via CallSoon → FR-035, FR-040

**Checkpoint**: US1 complete — single-line prompt with Ctrl-C/Ctrl-D works. Run `dotnet test --filter "FullyQualifiedName~Shortcuts"` to verify.

---

## Phase 4: User Story 2 — Session Reuse with History (Priority: P1) 🎯 MVP

**Goal**: Developer creates one PromptSession and calls Prompt() repeatedly; history persists and Up/Down arrows navigate previous entries

**Independent Test**: Create session → enter "first" → call Prompt again → press Up → verify "first" appears

### Tests for User Story 2

- [X] T022 [US2] Write session reuse tests in `tests/Stroke.Tests/Shortcuts/PromptSessionReuseTests.cs` — verify History persists across Prompt calls, buffer resets between calls (text, completion state, cursor per FR-038), session properties persist, per-prompt overrides apply permanently → US-2, US-6, FR-038

### Implementation for User Story 2

- [X] T023 [US2] Integration-verify buffer reset behavior in `src/Stroke/Shortcuts/PromptSession.Prompt.cs` — run through the Prompt→Reset→Prompt cycle: confirm DefaultBuffer.Reset() clears text/completion/cursor at start of each call while History persists; add any missing edge case handling discovered during integration (e.g., completion state cleanup, cursor position reset to 0) → FR-038

**Checkpoint**: US1+US2 complete — REPL-style session reuse with history works.

---

## Phase 5: User Story 3 — One-Shot Prompt Function (Priority: P2)

**Goal**: Developer calls static `Prompt.Prompt("Name: ")` for quick one-shot input without managing a session

**Independent Test**: Call `Prompt.Prompt("Name: ")` → type text → verify returned string

### Tests for User Story 3

- [X] T024 [P] [US3] Write `PromptTests` in `tests/Stroke.Tests/Shortcuts/PromptTests.cs` — static Prompt creates temp session with History param only, delegates all other params to session.Prompt(); PromptAsync equivalent → FR-019, FR-036

### Implementation for User Story 3

- [X] T025 [US3] Implement `Prompt.Prompt` static method in `src/Stroke/Shortcuts/Prompt.cs` — creates `PromptSession<string>(history: history)`, calls `session.Prompt(message, ...)` with all remaining params per contract `contracts/prompt.md` → FR-019
- [X] T026 [US3] Implement `Prompt.PromptAsync` static method in `src/Stroke/Shortcuts/Prompt.cs` — same pattern as Prompt but calls session.PromptAsync → FR-019

**Checkpoint**: US3 complete — one-shot prompt works.

---

## Phase 6: User Story 4 — Autocompletion Display (Priority: P2)

**Goal**: Developer provides a completer and chooses a CompleteStyle; completions appear in the correct format

**Independent Test**: Create session with WordCompleter + each CompleteStyle → verify menu type visibility

### Tests for User Story 4

- [X] T027 [US4] Write completion display tests in `tests/Stroke.Tests/Shortcuts/PromptSessionLayoutTests.cs` — verify CompletionsMenu Float visible when Column, MultiColumnCompletionsMenu visible when MultiColumn, Tab triggers DisplayCompletionsLikeReadline when ReadlineLike; completeWhileTyping Condition logic (true AND NOT historySearch AND NOT ReadlineLike per FR-018); reserveSpaceForMenu=0 behavior (Edge Case 4); completeInThread wraps in ThreadedCompleter → FR-006, FR-017, FR-018, FR-025, FR-032

### Implementation for User Story 4

- [X] T028 [US4] Integration-verify layout completion menu visibility in `src/Stroke/Shortcuts/PromptSession.Layout.cs` — exercise all 3 CompleteStyle values end-to-end: confirm Column→CompletionsMenu visible, MultiColumn→MultiColumnCompletionsMenu visible, ReadlineLike→neither visible; fix any filter logic gaps found during integration → FR-017, FR-018

**Checkpoint**: US4 complete — all three completion styles work correctly.

---

## Phase 7: User Story 5 — Confirmation Prompt (Priority: P2)

**Goal**: Developer calls `Confirm("Delete?")` and gets boolean result for y/Y (true) or n/N (false); all other keys ignored

**Independent Test**: Call Confirm → press "y" → verify returns true

### Tests for User Story 5

- [X] T029 [P] [US5] Write confirm tests in `tests/Stroke.Tests/Shortcuts/PromptTests.cs` — CreateConfirmSession bindings (y/Y→true, n/N→false, Keys.Any→no-op), Confirm delegates to CreateConfirmSession.Prompt, ConfirmAsync delegates to PromptAsync, custom suffix → FR-020, FR-021

### Implementation for User Story 5

- [X] T030 [US5] Implement `Prompt.CreateConfirmSession` in `src/Stroke/Shortcuts/Prompt.cs` — KeyBindings with y/Y/n/N bindings + Keys.Any catch-all; merges message+suffix via FormattedTextUtils.Merge; returns PromptSession<bool> per contract `contracts/prompt.md` → FR-021
- [X] T031 [US5] Implement `Prompt.Confirm` and `ConfirmAsync` in `src/Stroke/Shortcuts/Prompt.cs` — Confirm calls CreateConfirmSession.Prompt(), ConfirmAsync calls CreateConfirmSession.PromptAsync() → FR-020

**Checkpoint**: US5 complete — confirmation prompt works.

---

## Phase 8: User Story 6 — Per-Prompt Parameter Overrides (Priority: P2)

**Goal**: Developer changes prompt settings per Prompt() call; non-null values update session state permanently

**Independent Test**: Create session with "> " → call Prompt with "sql> " → verify change persists

### Tests for User Story 6

- [X] T032 [US6] Write per-prompt override tests in `tests/Stroke.Tests/Shortcuts/PromptSessionPromptTests.cs` — non-null message updates permanently, null preserves current, completer/style/validator overrides persist, all ~36 overridable params tested for null vs non-null behavior; also test inThread=true passes through to App.Run (FR-026), setExceptionHandler passes through → FR-010, FR-026

### Implementation for User Story 6

- [X] T033 [US6] Audit per-prompt override logic completeness in `src/Stroke/Shortcuts/PromptSession.Prompt.cs` — systematically compare all ~36 overridable parameters against Python source lines 966-1041; verify each has `if (param is not null) this.Property = param` in both Prompt and PromptAsync; flag and fix any missing or mismatched overrides → FR-010

**Checkpoint**: US6 complete — per-prompt overrides work for all parameters.

---

## Phase 9: User Story 7 — Multiline Input (Priority: P3)

**Goal**: Developer enables multiline mode; Enter inserts newline; message splits above/inline; continuation text renders

**Independent Test**: Create multiline session → type text → press Enter → verify newline inserted

### Tests for User Story 7

- [X] T034 [US7] Write multiline and layout tests in `tests/Stroke.Tests/Shortcuts/PromptSessionLayoutTests.cs` — SplitMultilinePrompt with "Line1\nLine2\n> " (HasBefore=true, Before="Line1\nLine2\n", FirstInputLine="> "); newline-only prompt "\n\n" (Edge Case 8); continuation callback receives correct width/lineNumber/wrapCount; continuation without multiline is ignored (Edge Case 9); showFrame wraps in Frame widget → FR-012, FR-013, FR-029

### Implementation for User Story 7

- [X] T035 [US7] Integration-verify multiline layout in `src/Stroke/Shortcuts/PromptSession.Layout.cs` — exercise multiline=true end-to-end: confirm ConditionalContainer shows prompt area above input, SearchToolbar/ArgToolbar positioned below input (not replacing prompt), continuation text renders correctly for wrapped lines; fix any layout gaps discovered → FR-006, FR-012, FR-013

**Checkpoint**: US7 complete — multiline input with continuation text works.

---

## Phase 10: User Story 8 — Password Input (Priority: P3)

**Goal**: Developer sets isPassword; typed characters display as asterisks but Prompt returns actual text

**Independent Test**: Create session with isPassword → type "secret" → verify display shows "******", return is "secret"

### Tests for User Story 8

- [X] T036 [US8] Write password display test in `tests/Stroke.Tests/Shortcuts/PromptSessionLayoutTests.cs` — verify PasswordProcessor included in layout processors when isPassword is true via DynCond → FR-006

### Implementation for User Story 8

- [X] T037 [US8] Integration-verify PasswordProcessor in `src/Stroke/Shortcuts/PromptSession.Layout.cs` — exercise isPassword=true end-to-end: confirm ConditionalProcessor(PasswordProcessor) activates when DynCond(isPassword) is true, asterisks display for typed text, Prompt still returns actual text; fix any wiring issues → FR-006

**Checkpoint**: US8 complete — password masking works.

---

## Phase 11: User Story 9 — Dumb Terminal Fallback (Priority: P3)

**Goal**: Prompt falls back to minimal mode in dumb terminals (TERM=dumb) with character echo

**Independent Test**: Simulate dumb terminal → call Prompt → verify character-by-character echo

### Tests for User Story 9

- [X] T038 [US9] Write dumb terminal tests in `tests/Stroke.Tests/Shortcuts/PromptSessionPromptTests.cs` — _output null + IsDumbTerminal → DumbPrompt; explicit output provided → NOT dumb mode; DumbPrompt echoes typed characters via OnTextChanged subscription → FR-014

### Implementation for User Story 9

- [X] T039 [US9] Implement `DumbPrompt` in `src/Stroke/Shortcuts/PromptSession.Application.cs` — creates temporary Application with DummyOutput, writes prompt message to real output, subscribes to DefaultBuffer.OnTextChanged to echo characters, writes "\r\n" when done; returns IDisposable for cleanup → FR-014
- [X] T040 [US9] Complete dumb terminal branch in `src/Stroke/Shortcuts/PromptSession.Prompt.cs` — replace the placeholder from T019/T020 with full implementation: if `_output==null && PlatformUtils.IsDumbTerminal()` → call `DumbPrompt` (from T039) and return its result instead of `App.Run()`/`App.RunAsync()` → FR-014

**Checkpoint**: US9 complete — dumb terminal fallback works.

---

## Phase 12: User Story 10 — Asynchronous Prompt (Priority: P3)

**Goal**: Developer awaits PromptAsync() without blocking the event loop

**Independent Test**: Call PromptAsync in async context → type text → await result

### Tests for User Story 10

- [X] T041 [US10] Write async prompt tests in `tests/Stroke.Tests/Shortcuts/PromptSessionPromptTests.cs` — PromptAsync returns Task<TResult>, Ctrl-C throws KeyboardInterruptException from Task, per-prompt overrides work in async → FR-009

### Implementation for User Story 10

No additional implementation needed — PromptAsync implemented in T020. This phase validates async-specific behavior.

**Checkpoint**: US10 complete — async prompt works.

---

## Phase 13: User Story 11 — Default Value and Auto-Accept (Priority: P3)

**Goal**: Developer pre-fills buffer with default text; optionally auto-accepts without user interaction

**Independent Test**: Call Prompt with default "hello" → verify buffer contains "hello"; with acceptDefault → verify auto-submit

### Tests for User Story 11

- [X] T042 [US11] Write default value and accept-default tests in `tests/Stroke.Tests/Shortcuts/PromptSessionPromptTests.cs` — default_ as string wraps in Document; default_ as Document preserves cursor; acceptDefault triggers ValidateAndHandle via CallSoon (FR-035); preRun callback executes before acceptDefault → FR-015, FR-035

### Implementation for User Story 11

No additional implementation needed — default_ and acceptDefault handling implemented in T019/T020/T021. This phase validates the specific behavior.

**Checkpoint**: US11 complete — default values and auto-accept work.

---

## Phase 14: Polish & Cross-Cutting Concerns

**Purpose**: Thread safety verification, concurrency tests, build validation, coverage check

- [X] T043 [P] Write `PromptSessionConcurrencyTests` in `tests/Stroke.Tests/Shortcuts/PromptSessionConcurrencyTests.cs` — concurrent property reads/writes across threads verify Lock protection; DynCond read from render thread while main thread writes; no deadlocks under contention → Constitution XI, FR-016
- [X] T044 [P] Verify all source files stay under 1,000 LOC — check `PromptSession.cs`, `PromptSession.Layout.cs`, `PromptSession.Buffers.cs`, `PromptSession.Application.cs`, `PromptSession.Prompt.cs`, `PromptSession.Helpers.cs`, `PromptFunctions.cs`; split if any exceed limit → Constitution X
- [X] T045 Verify `dotnet build src/Stroke/Stroke.csproj` compiles with zero warnings
- [X] T046 Verify `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~Shortcuts"` passes all tests; confirm success criteria SC-001 (≤5 lines), SC-002 (no accumulation), SC-003 (3 styles), SC-004 (16ms), SC-005 (dumb terminal), SC-006 (overrides), SC-007 (confirm y/n) are exercised by at least one passing test
- [X] T047 Verify test coverage ≥80% via `dotnet test --collect:"XPlat Code Coverage"` across PromptSession, PromptFunctions, CompleteStyle, KeyboardInterruptException, EOFException → SC-008
- [X] T048 Run quickstart.md validation — verify all code examples from `specs/047-prompt-session/quickstart.md` are consistent with implemented API signatures
- [X] T049 Verify 1:1 Python API fidelity — compare all public APIs in Python's `prompt_toolkit.shortcuts.prompt.__all__` against implemented C# types: CompleteStyle, PromptSession, prompt (→PromptFunctions.Prompt), confirm (→PromptFunctions.Confirm), create_confirm_session (→PromptFunctions.CreateConfirmSession) → SC-009, SC-010

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 completion — BLOCKS all user stories
- **Phases 3-4 (US1, US2 — P1)**: Depend on Phase 2; US2 depends on US1 (session reuse requires Prompt to work)
- **Phases 5-8 (US3-US6 — P2)**: Depend on Phase 2; can proceed in parallel after US1
- **Phases 9-13 (US7-US11 — P3)**: Depend on Phase 2; can proceed in parallel after US1
- **Phase 14 (Polish)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 1 (Setup)
    └─→ Phase 2 (Foundational: T006-T015)
         └─→ Phase 3: US1 (T016-T021) ← MVP
              ├─→ Phase 4: US2 (T022-T023) ← MVP
              ├─→ Phase 5: US3 (T024-T026)
              ├─→ Phase 6: US4 (T027-T028)
              ├─→ Phase 7: US5 (T029-T031)
              ├─→ Phase 8: US6 (T032-T033)
              ├─→ Phase 9: US7 (T034-T035)
              ├─→ Phase 10: US8 (T036-T037)
              ├─→ Phase 11: US9 (T038-T040)
              ├─→ Phase 12: US10 (T041)
              ├─→ Phase 13: US11 (T042)
              └─→ Phase 14 (Polish: T043-T049)
```

### Within Each User Story

- Test tasks can run in parallel (different files)
- Tests written → then implementation → then verification
- Models/types before services/logic
- Core implementation before integration

### Parallel Opportunities

**Phase 1**: T002, T003, T004, T005 can all run in parallel (different files)
**Phase 2**: T008+T009 can run in parallel (both in Buffers.cs but T009 is trivial); T010, T011, T012 are in Layout.cs (sequential); T013, T014 are in Application.cs (sequential)
**Phase 3 tests**: T016, T017, T018 can run in parallel (different test files)
**Phase 5-13**: After US1+US2 complete, US3-US11 can run in parallel (independent features)
**Phase 14**: T043 and T044 can run in parallel

---

## Parallel Example: Phase 1 (Setup)

```
# Launch all setup tasks together (different files):
Task T002: "Create CompleteStyle enum in src/Stroke/Shortcuts/CompleteStyle.cs"
Task T003: "Create KeyboardInterruptException in src/Stroke/Shortcuts/KeyboardInterruptException.cs"
Task T004: "Create EOFException in src/Stroke/Shortcuts/EOFException.cs"
Task T005: "Create PromptContinuationCallable delegate in src/Stroke/Shortcuts/PromptContinuationCallable.cs"
```

## Parallel Example: Phase 3 Tests (US1)

```
# Launch all US1 test files together (different files):
Task T016: "CompleteStyleTests in tests/Stroke.Tests/Shortcuts/CompleteStyleTests.cs"
Task T017: "PromptSessionTests in tests/Stroke.Tests/Shortcuts/PromptSessionTests.cs"
Task T018: "PromptSessionBindingsTests in tests/Stroke.Tests/Shortcuts/PromptSessionBindingsTests.cs"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup (T001-T005)
2. Complete Phase 2: Foundational (T006-T015)
3. Complete Phase 3: User Story 1 (T016-T021)
4. Complete Phase 4: User Story 2 (T022-T023)
5. **STOP and VALIDATE**: Run `dotnet test --filter "FullyQualifiedName~Shortcuts"` — basic REPL prompt works
6. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 + 2 → Foundation ready
2. US1 + US2 → REPL works (MVP!)
3. US3 → One-shot prompt works
4. US4 → Autocompletion works
5. US5 → Confirmation prompt works
6. US6 → Per-prompt overrides verified
7. US7-US11 → Advanced features (multiline, password, dumb terminal, async, defaults)
8. Polish → Thread safety verified, coverage checked, API fidelity confirmed

### File → Task Traceability

| Source File | Tasks |
|-------------|-------|
| `src/Stroke/Application/Application.cs` | T001 |
| `src/Stroke/Shortcuts/CompleteStyle.cs` | T002 |
| `src/Stroke/Shortcuts/KeyboardInterruptException.cs` | T003 |
| `src/Stroke/Shortcuts/EOFException.cs` | T004 |
| `src/Stroke/Shortcuts/PromptContinuationCallable.cs` | T005 |
| `src/Stroke/Shortcuts/PromptSession.cs` | T006, T007 |
| `src/Stroke/Shortcuts/PromptSession.Buffers.cs` | T008, T009 |
| `src/Stroke/Shortcuts/PromptSession.Layout.cs` | T010, T011, T012, T028, T035, T037 |
| `src/Stroke/Shortcuts/PromptSession.Application.cs` | T013, T014, T039 |
| `src/Stroke/Shortcuts/PromptSession.Prompt.cs` | T019, T020, T021, T023, T033, T040 |
| `src/Stroke/Shortcuts/PromptSession.Helpers.cs` | T015 |
| `src/Stroke/Shortcuts/Prompt.cs` | T025, T026, T030, T031 |
| `tests/Stroke.Tests/Shortcuts/CompleteStyleTests.cs` | T016 |
| `tests/Stroke.Tests/Shortcuts/PromptSessionTests.cs` | T017, T022 |
| `tests/Stroke.Tests/Shortcuts/PromptSessionLayoutTests.cs` | T027, T034, T036 |
| `tests/Stroke.Tests/Shortcuts/PromptSessionBindingsTests.cs` | T018 |
| `tests/Stroke.Tests/Shortcuts/PromptSessionPromptTests.cs` | T032, T038, T041, T042 |
| `tests/Stroke.Tests/Shortcuts/PromptTests.cs` | T024, T029 |
| `tests/Stroke.Tests/Shortcuts/PromptSessionConcurrencyTests.cs` | T043 |

### FR → Task Traceability

| FR | Tasks | Description |
|----|-------|-------------|
| FR-001 | T002, T016 | CompleteStyle enum |
| FR-002 | T006, T017 | PromptSession class |
| FR-003 | T006, T017 | 44 constructor params |
| FR-004 | T008 | Default buffer with accept handler |
| FR-005 | T009 | Search buffer |
| FR-006 | T010, T027, T034, T036 | Layout construction |
| FR-007 | T013 | Application with merged key bindings |
| FR-008 | T019 | Prompt (blocking) |
| FR-009 | T020, T041 | PromptAsync |
| FR-010 | T019, T020, T032, T033 | Per-prompt overrides |
| FR-011 | T014, T018 | Prompt key bindings |
| FR-012 | T010, T011, T034 | Multiline prompt splitting |
| FR-013 | T015, T034 | Prompt continuation |
| FR-014 | T039, T040, T038 | Dumb terminal |
| FR-015 | T042 | Default text / acceptDefault |
| FR-016 | T007, T017, T043 | DynCond dynamic conditions |
| FR-017 | T010, T027 | reserveSpaceForMenu |
| FR-018 | T008, T027 | completeWhileTyping Condition |
| FR-019 | T025, T026, T024 | Static Prompt/PromptAsync |
| FR-020 | T031, T029 | Confirm function |
| FR-021 | T030, T029 | CreateConfirmSession |
| FR-022 | T006, T017 | Input/Output delegation |
| FR-023 | T006, T017 | EditingMode delegation |
| FR-024 | T013, T017 | eraseWhenDone |
| FR-025 | T010, T027 | showFrame |
| FR-026 | T019, T032 | inThread parameter |
| FR-027 | T003, T004, T014, T018 | Configurable exception types |
| FR-028 | T012 | RPrompt window |
| FR-029 | T011, T034 | SplitMultilinePrompt |
| FR-030 | T005, T015 | PromptContinuationText |
| FR-031 | T008, T010, T013 | 9 dynamic wrapper types |
| FR-032 | T008, T027 | ThreadedCompleter wrapping |
| FR-033 | T013 | Style transformation merger |
| FR-034 | T013 | SwapLightAndDark conditional |
| FR-035 | T021, T042 | PreRunCallables / acceptDefault |
| FR-036 | T019, T024 | InputHook parameter |
| FR-037 | T006, T017 | Exception type validation |
| FR-038 | T019, T020, T022, T023 | Buffer reset between calls |
| FR-039 | T019, T020 | Exception propagation |
| FR-040 | T021 | PreRunCallables lifecycle |
| FR-041 | T014, T018 | Ctrl-Z suspend gating |
