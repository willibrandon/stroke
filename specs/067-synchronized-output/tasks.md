# Tasks: Synchronized Output (DEC Mode 2026)

**Input**: Design documents from `/specs/067-synchronized-output/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Yes — NFR-005 requires 80% branch coverage of new code. Two new test files specified in plan.md.

**Organization**: Tasks are grouped by user story. The IOutput interface extension and Vt100Output core implementation are shared infrastructure that all four user stories depend on — they are placed in the Foundational phase (no story label, per template convention). US2's dedicated phase (Phase 5) covers test verification of the atomic render behavior that the Foundational + US1 phases make possible.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Branch creation and orientation

- [ ] T001 Create feature branch `067-synchronized-output` from `main`

---

## Phase 2: Foundational (IOutput Interface + Vt100Output Core)

**Purpose**: Core synchronized output infrastructure that ALL user stories depend on — the IOutput interface extension and the Vt100Output real implementation. Maps to FR-001, FR-002, FR-003, FR-015, FR-016.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T002 Add `BeginSynchronizedOutput()` and `EndSynchronizedOutput()` to the `IOutput` interface in `src/Stroke/Output/IOutput.cs` per contract `contracts/ioutput-extension.md` (FR-001). Add XML doc comments. This will cause build errors in all 6 implementations — that is expected and resolved in subsequent tasks.
- [ ] T003 Implement `BeginSynchronizedOutput()` and `EndSynchronizedOutput()` in `src/Stroke/Output/Vt100Output.cs` (FR-002, FR-003, FR-015, FR-016, NFR-004). Add `private bool _synchronizedOutput` field (defaults to false). Begin sets flag to true under `_lock`, End sets to false under `_lock`. Lock held only for flag mutation duration. Both methods are idempotent.
- [ ] T004 Modify `Flush()` in `src/Stroke/Output/Vt100Output.cs` to check `_synchronizedOutput` flag. When true and buffer is non-empty, prepend `\x1b[?2026h` and append `\x1b[?2026l` to the concatenated buffer content before writing to `_stdout`. When false or buffer is empty, behavior is unchanged (FR-002, FR-003).

**Checkpoint**: Vt100Output now supports synchronized output. Build still fails on other IOutput implementations (expected — resolved in US4 phase). Run Vt100Output-specific tests to verify no regressions.

---

## Phase 3: User Story 4 — Consistent Rendering Across All Output Backends (Priority: P3)

**Goal**: All 6 IOutput implementations compile with the new interface methods. Non-VT100 backends get no-ops, hybrid backends delegate.

**Independent Test**: Build succeeds. Non-VT100 outputs emit zero Mode 2026 sequences. Hybrid outputs delegate correctly.

**Note**: This phase is placed before US1/US2/US3 because the build must compile before any other work can proceed. Although US4 is P3 priority, the no-op stubs are trivially small and required to unblock everything else.

### Implementation for User Story 4

- [ ] T005 [P] [US4] Add no-op `BeginSynchronizedOutput()` and `EndSynchronizedOutput()` to `src/Stroke/Output/DummyOutput.cs` (FR-006)
- [ ] T006 [P] [US4] Add no-op `BeginSynchronizedOutput()` and `EndSynchronizedOutput()` to `src/Stroke/Output/PlainTextOutput.cs` (FR-005). Mode 2026 sequences must never appear in plain text output.
- [ ] T007 [P] [US4] Add no-op `BeginSynchronizedOutput()` and `EndSynchronizedOutput()` to `src/Stroke/Output/Windows/Win32Output.cs` (FR-004)
- [ ] T008 [P] [US4] Add delegating `BeginSynchronizedOutput()` and `EndSynchronizedOutput()` to `src/Stroke/Output/Windows/Windows10Output.cs` — delegate to `_vt100Output` (FR-007)
- [ ] T009 [P] [US4] Add delegating `BeginSynchronizedOutput()` and `EndSynchronizedOutput()` to `src/Stroke/Output/Windows/ConEmuOutput.cs` — delegate to `_vt100Output` (FR-008)

**Checkpoint**: Full build succeeds (`dotnet build src/Stroke/Stroke.csproj`). All IOutput implementations satisfy the interface. Run full test suite to verify zero regressions (NFR-003).

---

## Phase 4: User Story 1 — Flicker-Free Terminal Resize (Priority: P1) 🎯 MVP

**Goal**: Terminal resize produces zero visible blank frames. The resize handler performs zero immediate terminal I/O — it only resets state and schedules a redraw. The next render cycle erases and redraws atomically inside a synchronized output block.

**Independent Test**: Launch any Stroke prompt example, resize the terminal window, observe content transitions smoothly without a visible blank frame.

### Implementation for User Story 1

- [ ] T010 [US1] Add `ResetForResize()` method to `src/Stroke/Rendering/Renderer.cs` per contract `contracts/renderer-extension.md` (FR-013). Reset all 9 state fields to initial values: `_cursorPos` to origin, `_lastScreen` to null, `_lastSize` to null, `_lastStyle` to null, `_lastCursorShape` to null, `MouseHandlers` to new empty instance, `_minAvailableHeight` to zero, `_cursorKeyModeReset` to false, `_mouseSupportEnabled` to false. No terminal I/O.
- [ ] T011 [US1] Change `OnResize()` in `src/Stroke/Application/Application.RunAsync.cs` to call `Renderer.ResetForResize()` instead of `Renderer.Erase(leaveAlternateScreen: false)` (FR-012). Keep `Renderer.RequestAbsoluteCursorPosition()` and `Invalidate()` calls.
- [ ] T012 [US1] Change the full-redraw path in `src/Stroke/Rendering/Renderer.Diff.cs` to use absolute cursor positioning `output.WriteRaw("\x1b[H")` instead of `MoveCursor(new Point(0, 0))` when `isDone`, `previousScreen is null`, or `previousWidth != width` (FR-014). Update `currentPos` to `new Point(0, 0)` after the write.
- [ ] T013 [US1] Wrap `Renderer.Render()` in `src/Stroke/Rendering/Renderer.cs` with `BeginSynchronizedOutput()`/`EndSynchronizedOutput()` using try/finally (FR-009, FR-017). Place begin after setup operations (alternate screen, bracketed paste, cursor key mode, mouse support) and before screen diff. Place end in finally block after flush. Per contract `contracts/renderer-extension.md`.
- [ ] T014 [US1] Wrap `Renderer.Erase()` in `src/Stroke/Rendering/Renderer.cs` with `BeginSynchronizedOutput()`/`EndSynchronizedOutput()` using try/finally (FR-010, FR-017). Per contract.
- [ ] T015 [US1] Wrap `Renderer.Clear()` in `src/Stroke/Rendering/Renderer.cs` with `BeginSynchronizedOutput()`/`EndSynchronizedOutput()` using try/finally (FR-011, FR-017). Inline erase logic — do NOT delegate to `Erase()` to avoid nested sync blocks. Per contract.

**Checkpoint**: Build succeeds. Full test suite passes (NFR-003). Resize no longer calls `Erase()` directly. The full-redraw path uses `\x1b[H` instead of relative movement. All render/erase/clear operations are wrapped in synchronized output blocks with try/finally.

---

## Phase 5: User Story 2 — Atomic Render Updates During Normal Operation (Priority: P2)

**Goal**: Every render cycle commits to the terminal atomically. This is already functionally complete from Phase 2 (Vt100Output) + Phase 4 (Renderer wrapping). This phase adds dedicated test coverage to verify atomic behavior.

**Independent Test**: Launch a Stroke prompt with a live clock (e.g., `fancy-zsh-prompt` example), observe clock updates without partial-render artifacts.

### Tests for User Story 2

- [ ] T016 [US2] Create `tests/Stroke.Tests/Output/Vt100OutputSynchronizedOutputTests.cs` with xUnit tests (NFR-004, NFR-005). Use `StringWriter` as stdout for output capture. Test cases:
  - Begin sets flag, Flush wraps output in `\x1b[?2026h`...`\x1b[?2026l` markers (FR-002, SC-002)
  - End clears flag, Flush emits no markers (FR-003)
  - Empty buffer produces no markers even when flag is set (FR-002 empty-buffer behavior)
  - Begin is idempotent — multiple begins keep flag true (FR-016)
  - End is idempotent — multiple ends keep flag false (FR-016)
  - Markers appear in correct order wrapping content (SC-002)
  - Marker overhead is exactly 16 bytes per flush (8-byte begin + 8-byte end) (NFR-001)
  - Thread safety: Begin/End under concurrent access with existing lock (FR-015, NFR-004)
  - Target: ≥80% branch coverage of new Vt100Output code (NFR-005)

**Checkpoint**: All Vt100Output synchronized output tests pass. SC-002, SC-003 verifiable from test output.

---

## Phase 6: User Story 3 — Graceful Degradation on Older Terminals (Priority: P2)

**Goal**: On terminals that don't support Mode 2026, the escape sequences are silently ignored with no visible effect. This is already functionally complete — the DEC Private Mode spec guarantees silent ignore. This phase adds test coverage for non-VT100 backends and renderer integration.

**Independent Test**: Launch any Stroke example in a terminal known not to support Mode 2026 — all functionality works identically to pre-feature behavior.

### Tests for User Story 3

- [ ] T017 [US3] Create `tests/Stroke.Tests/Rendering/RendererSynchronizedOutputTests.cs` with xUnit tests (NFR-005). Test cases cover both renderer wrapping (shared US1/US2 infrastructure) and graceful degradation (US3-specific):
  - **Renderer wrapping (shared infrastructure — verifies US1/US2 behavior):**
    - Render wraps output in sync markers (capture via Vt100Output + StringWriter) (FR-009, SC-008)
    - Erase wraps output in sync markers (FR-010, SC-008)
    - Clear wraps output in sync markers without nesting (FR-011, SC-008)
    - ResetForResize resets all 9 state fields to initial values (FR-013, SC-004)
    - ResetForResize performs zero terminal I/O (FR-012, SC-004)
    - Full-redraw path uses `\x1b[H` absolute positioning (FR-014, SC-005)
  - **Graceful degradation (US3-specific):**
    - DummyOutput: Begin/End are no-ops, no markers in output (FR-006, SC-003)
    - PlainTextOutput: Begin/End are no-ops, no markers contaminate output (FR-005, SC-003)
  - Target: ≥80% branch coverage of new Renderer code (NFR-005)

**Checkpoint**: All renderer synchronized output tests pass. SC-004, SC-005, SC-008 verifiable from test output.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Full regression verification and visual confirmation

- [ ] T018 Run full test suite (`dotnet test tests/Stroke.Tests/Stroke.Tests.csproj`) — all 9,311+ existing tests must pass with zero regressions (NFR-003, SC-006)
- [ ] T019 Run quickstart.md validation: build with `dotnet build src/Stroke/Stroke.csproj`, run sync-specific tests with `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~SynchronizedOutput"`
- [ ] T020 Visual verification via TUI driver (SC-001, SC-007): launch a representative set of Stroke examples (at minimum: `get-input`, `fancy-zsh-prompt`, `full-screen/calculator`, `full-screen/text-editor`), resize the terminal for each, and confirm no visible blank frame or flicker. Compare with `main` branch behavior. A full sweep of all 102+ examples is deferred to CI; this task verifies the key interaction patterns (simple prompt, live-updating prompt, full-screen app).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **US4 — All Backends (Phase 3)**: Depends on Foundational — BLOCKS remaining stories (build must compile)
- **US1 — Resize Fix (Phase 4)**: Depends on US4 (build compiles) — the MVP
- **US2 — Atomic Render Tests (Phase 5)**: Depends on US1 (Renderer wrapping complete)
- **US3 — Degradation Tests (Phase 6)**: Depends on US1 (Renderer wrapping complete)
- **Polish (Phase 7)**: Depends on all user stories complete

### User Story Dependencies

```
Phase 1: Setup
    ↓
Phase 2: Foundational (IOutput + Vt100Output)
    ↓
Phase 3: US4 — Backend stubs [T005-T009 all parallel]
    ↓
Phase 4: US1 — Resize fix [T010-T015 sequential within Renderer]
    ↓ ↓
Phase 5: US2 tests [P]    Phase 6: US3 tests [P]
    ↓ ↓
Phase 7: Polish
```

### Parallel Opportunities

- **Phase 3 (US4)**: T005, T006, T007, T008, T009 — all touch different files, fully parallel
- **Phase 5 + Phase 6**: US2 tests and US3 tests can run in parallel (different test files)
- **Phase 4 within**: T010 must come before T011 (ResetForResize before OnResize uses it). T012 is independent. T013/T014/T015 are independent of each other but depend on T002/T004.

---

## Parallel Example: Phase 3 (US4 Backend Stubs)

```
# All 5 tasks in parallel — different files, no dependencies:
Task T005: "Add no-op methods to DummyOutput.cs"
Task T006: "Add no-op methods to PlainTextOutput.cs"
Task T007: "Add no-op methods to Win32Output.cs"
Task T008: "Add delegation methods to Windows10Output.cs"
Task T009: "Add delegation methods to ConEmuOutput.cs"
```

## Parallel Example: Phase 5 + Phase 6 (Test Files)

```
# Both test tasks in parallel — different files:
Task T016: "Vt100Output synchronized output tests"
Task T017: "Renderer synchronized output tests"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (branch)
2. Complete Phase 2: Foundational (IOutput interface + Vt100Output)
3. Complete Phase 3: US4 backend stubs (build compiles)
4. Complete Phase 4: US1 resize fix (Renderer + OnResize + ScreenDiff)
5. **STOP and VALIDATE**: Build succeeds, full test suite passes, resize no longer flickers

### Incremental Delivery

1. Setup + Foundational + US4 stubs → Build compiles
2. Add US1 (resize fix) → Test independently → Validate (MVP!)
3. Add US2 tests → Verify atomic render coverage
4. Add US3 tests → Verify graceful degradation coverage
5. Polish → Full regression + visual verification

---

## Summary

| Metric | Value |
|--------|-------|
| Total tasks | 20 |
| Phase 1 (Setup) | 1 task |
| Phase 2 (Foundational) | 3 tasks |
| Phase 3 (US4 Backends) | 5 tasks (all parallel) |
| Phase 4 (US1 Resize Fix) | 6 tasks |
| Phase 5 (US2 Tests) | 1 task |
| Phase 6 (US3 Tests) | 1 task |
| Phase 7 (Polish) | 3 tasks |
| Parallel opportunities | 3 (Phase 3 all-parallel, Phase 5+6 parallel, Phase 4 partial) |
| Files modified | 10 existing + 2 new test files |
| Estimated new/changed LOC | ~200 |
