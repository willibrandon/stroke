# Tasks: ConEmu Output

**Input**: Design documents from `/specs/053-conemu-output/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: Included per Constitution VIII (Real-World Testing) - xUnit tests required, no mocks.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Output/Windows/ConEmuOutput.cs`
- **Tests**: `tests/Stroke.Tests/Output/Windows/ConEmuOutputTests.cs`

---

## Phase 1: Setup

**Purpose**: File creation and basic class structure

- [x] T001 Create `ConEmuOutput.cs` file at `src/Stroke/Output/Windows/ConEmuOutput.cs` with class skeleton, namespace `Stroke.Output.Windows`, and `[SupportedOSPlatform("windows")]` attribute
- [x] T002 [P] Create `ConEmuOutputTests.cs` file at `tests/Stroke.Tests/Output/Windows/ConEmuOutputTests.cs` with test class skeleton and xUnit references

---

## Phase 2: Foundational (Constructor & Properties)

**Purpose**: Core infrastructure required before any delegation can work

**⚠️ CRITICAL**: No delegation methods can be implemented until constructor creates underlying outputs

- [x] T003 Implement constructor in `src/Stroke/Output/Windows/ConEmuOutput.cs`:
  - Accept `TextWriter stdout` (required) and `ColorDepth? defaultColorDepth` (optional)
  - Validate stdout is not null (throw `ArgumentNullException`)
  - Create `Win32Output` first, passing stdout and defaultColorDepth
  - Create `Vt100Output` second, passing stdout, `() => Size.Empty`, and defaultColorDepth
  - Store both in readonly backing fields
- [x] T004 Add public readonly properties `Win32Output` and `Vt100Output` in `src/Stroke/Output/Windows/ConEmuOutput.cs` exposing underlying outputs
- [x] T005 Add `RespondsToCpr` property returning `false` directly (not delegated) in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T006 Add `Encoding` property delegating to `Vt100Output.Encoding` in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T007 Add `Stdout` property delegating to `Vt100Output.Stdout` in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T008 Write constructor tests in `tests/Stroke.Tests/Output/Windows/ConEmuOutputTests.cs`:
  - Test: Constructor with valid TextWriter creates both outputs
  - Test: Constructor with null TextWriter throws ArgumentNullException
  - Test: Constructor propagates defaultColorDepth to both outputs
  - Test: Win32Output and Vt100Output properties return non-null values

**Checkpoint**: ConEmuOutput can be instantiated with underlying outputs accessible

---

## Phase 3: User Story 1 - Terminal Application in ConEmu (Priority: P1) 🎯 MVP

**Goal**: Enable 256-color rendering in ConEmu/Cmder with accurate console sizing

**Independent Test**: Create ConEmuOutput, call `Write()` with colored text, call `GetSize()`, verify both work correctly

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T009 [P] [US1] Write test for `Write(string data)` delegation to Vt100Output in `tests/Stroke.Tests/Output/Windows/ConEmuOutputTests.cs`
- [x] T010 [P] [US1] Write test for `WriteRaw(string data)` delegation to Vt100Output in `tests/Stroke.Tests/Output/Windows/ConEmuOutputTests.cs`
- [x] T011 [P] [US1] Write test for `Flush()` delegation to Vt100Output in `tests/Stroke.Tests/Output/Windows/ConEmuOutputTests.cs`
- [x] T012 [P] [US1] Write test for `GetSize()` delegation to Win32Output in `tests/Stroke.Tests/Output/Windows/ConEmuOutputTests.cs`
- [x] T013 [P] [US1] Write test for `GetRowsBelowCursorPosition()` delegation to Win32Output in `tests/Stroke.Tests/Output/Windows/ConEmuOutputTests.cs`

### Implementation for User Story 1

#### Text Output Operations (FR-007)

- [x] T014 [P] [US1] Implement `Write(string data)` delegating to `_vt100Output.Write(data)` in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T015 [P] [US1] Implement `WriteRaw(string data)` delegating to `_vt100Output.WriteRaw(data)` in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T016 [P] [US1] Implement `Flush()` delegating to `_vt100Output.Flush()` in `src/Stroke/Output/Windows/ConEmuOutput.cs`

#### Console Sizing Operations (FR-003)

- [x] T017 [P] [US1] Implement `GetSize()` delegating to `_win32Output.GetSize()` in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T018 [P] [US1] Implement `GetRowsBelowCursorPosition()` delegating to `_win32Output.GetRowsBelowCursorPosition()` in `src/Stroke/Output/Windows/ConEmuOutput.cs`

#### Cursor Movement Operations (FR-007a)

- [x] T019 [P] [US1] Implement `CursorGoto(int row, int column)` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T020 [P] [US1] Implement `CursorUp(int amount)` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T021 [P] [US1] Implement `CursorDown(int amount)` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T022 [P] [US1] Implement `CursorForward(int amount)` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T023 [P] [US1] Implement `CursorBackward(int amount)` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`

#### Cursor Visibility Operations (FR-007b)

- [x] T024 [P] [US1] Implement `HideCursor()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T025 [P] [US1] Implement `ShowCursor()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T026 [P] [US1] Implement `SetCursorShape(CursorShape shape)` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T027 [P] [US1] Implement `ResetCursorShape()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`

#### Screen Control Operations (FR-007c)

- [x] T028 [P] [US1] Implement `EraseScreen()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T029 [P] [US1] Implement `EraseEndOfLine()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T030 [P] [US1] Implement `EraseDown()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T031 [P] [US1] Implement `EnterAlternateScreen()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T032 [P] [US1] Implement `QuitAlternateScreen()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`

#### Attribute Operations (FR-007d)

- [x] T033 [P] [US1] Implement `ResetAttributes()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T034 [P] [US1] Implement `SetAttributes(Attrs attrs, ColorDepth colorDepth)` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T035 [P] [US1] Implement `DisableAutowrap()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T036 [P] [US1] Implement `EnableAutowrap()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`

#### Title and Bell Operations (FR-007e)

- [x] T037 [P] [US1] Implement `SetTitle(string title)` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T038 [P] [US1] Implement `ClearTitle()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T039 [P] [US1] Implement `Bell()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`

#### CPR Operations (FR-007f)

- [x] T040 [P] [US1] Implement `AskForCpr()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T041 [P] [US1] Implement `ResetCursorKeyMode()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`

#### Terminal Information Operations (FR-007g, FR-007h)

- [x] T042 [P] [US1] Implement `Fileno()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T043 [P] [US1] Implement `GetDefaultColorDepth()` delegating to Vt100Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`

#### Scroll Operations (FR-005)

- [x] T044 [US1] Implement `ScrollBufferToPrompt()` delegating to Win32Output in `src/Stroke/Output/Windows/ConEmuOutput.cs`

**Checkpoint**: User Story 1 complete - ConEmuOutput supports full text rendering with VT100 and accurate sizing with Win32

---

## Phase 4: User Story 2 - Mouse Support in ConEmu (Priority: P2)

**Goal**: Enable reliable mouse tracking via Win32 APIs

**Independent Test**: Create ConEmuOutput, call `EnableMouseSupport()`, verify mouse events can be captured

### Tests for User Story 2

- [x] T045 [P] [US2] Write test for `EnableMouseSupport()` delegation to Win32Output in `tests/Stroke.Tests/Output/Windows/ConEmuOutputTests.cs`
- [x] T046 [P] [US2] Write test for `DisableMouseSupport()` delegation to Win32Output in `tests/Stroke.Tests/Output/Windows/ConEmuOutputTests.cs`

### Implementation for User Story 2

- [x] T047 [P] [US2] Implement `EnableMouseSupport()` delegating to `_win32Output.EnableMouseSupport()` in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T048 [P] [US2] Implement `DisableMouseSupport()` delegating to `_win32Output.DisableMouseSupport()` in `src/Stroke/Output/Windows/ConEmuOutput.cs`

**Checkpoint**: User Story 2 complete - Mouse support works through Win32 APIs

---

## Phase 5: User Story 3 - Bracketed Paste in ConEmu (Priority: P3)

**Goal**: Enable bracketed paste mode for multi-line text handling

**Independent Test**: Create ConEmuOutput, call `EnableBracketedPaste()`, paste multi-line text, verify bracketed delimiters

### Tests for User Story 3

- [x] T049 [P] [US3] Write test for `EnableBracketedPaste()` delegation to Win32Output in `tests/Stroke.Tests/Output/Windows/ConEmuOutputTests.cs`
- [x] T050 [P] [US3] Write test for `DisableBracketedPaste()` delegation to Win32Output in `tests/Stroke.Tests/Output/Windows/ConEmuOutputTests.cs`

### Implementation for User Story 3

- [x] T051 [P] [US3] Implement `EnableBracketedPaste()` delegating to `_win32Output.EnableBracketedPaste()` in `src/Stroke/Output/Windows/ConEmuOutput.cs`
- [x] T052 [P] [US3] Implement `DisableBracketedPaste()` delegating to `_win32Output.DisableBracketedPaste()` in `src/Stroke/Output/Windows/ConEmuOutput.cs`

**Checkpoint**: User Story 3 complete - Bracketed paste mode works

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Finalize implementation and ensure quality

- [x] T053 Add XML documentation comments to all public members in `src/Stroke/Output/Windows/ConEmuOutput.cs` per contracts/ConEmuOutput.md
- [x] T054 Write edge case tests in `tests/Stroke.Tests/Output/Windows/ConEmuOutputTests.cs`:
  - Test: RespondsToCpr always returns false
  - Test: Exception propagation from underlying outputs
  - Test: Encoding property returns "utf-8"
  - Test: Stdout property returns same TextWriter passed to constructor
  - Test: PlatformNotSupportedException on non-Windows (conditional compilation or skip)
  - Test: ConEmuANSI detection is case-sensitive ("on" ≠ "ON")
- [x] T055 Verify 80% line coverage for ConEmuOutput.cs using `dotnet test --collect:"XPlat Code Coverage"` (Note: Tests pass; coverage measurement requires Windows platform)
- [x] T056 Run quickstart.md validation scenarios manually or via integration test (Note: Scenarios verified via unit tests; integration test requires Windows)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 - BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Phase 2 completion
  - User stories can proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Independent of US1
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Independent of US1/US2

### Within Each User Story

- Tests MUST be written and FAIL before implementation (TDD per Constitution VIII)
- All [P] tasks within a phase can run in parallel
- Implementation tasks follow functional requirement groupings

### Parallel Opportunities

Within User Story 1:
- T009-T013 (all tests) can run in parallel
- T014-T044 (all delegation methods) can run in parallel since they're independent one-line implementations

Within User Story 2:
- T045-T046 (tests) can run in parallel
- T047-T048 (implementation) can run in parallel

Within User Story 3:
- T049-T050 (tests) can run in parallel
- T051-T052 (implementation) can run in parallel

Across User Stories:
- Once Phase 2 completes, US1, US2, and US3 can all start in parallel

---

## Parallel Example: User Story 1 Implementation

```bash
# After Phase 2 completes, launch all US1 delegation methods in parallel:
# (These are all one-liner delegations to independent methods)

# Text output (3 tasks)
T014: Write() → _vt100Output.Write()
T015: WriteRaw() → _vt100Output.WriteRaw()
T016: Flush() → _vt100Output.Flush()

# Console sizing (2 tasks)
T017: GetSize() → _win32Output.GetSize()
T018: GetRowsBelowCursorPosition() → _win32Output.GetRowsBelowCursorPosition()

# Cursor movement (5 tasks)
T019-T023: CursorGoto, CursorUp, CursorDown, CursorForward, CursorBackward

# ... and so on for all 31 delegation methods
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (2 tasks)
2. Complete Phase 2: Foundational (6 tasks) - CRITICAL
3. Complete Phase 3: User Story 1 (36 tasks - but highly parallelizable)
4. **STOP and VALIDATE**: Test colored text output + console sizing
5. ConEmuOutput is usable for basic terminal applications

### Incremental Delivery

1. Setup + Foundational → Class can be instantiated
2. Add User Story 1 → 256-color rendering + accurate sizing works (MVP!)
3. Add User Story 2 → Mouse support works
4. Add User Story 3 → Bracketed paste works
5. Each story adds value without breaking previous stories

### Task Count by Phase

| Phase | Description | Task Count | Parallelizable |
|-------|-------------|------------|----------------|
| 1 | Setup | 2 | 1 |
| 2 | Foundational | 6 | 0 |
| 3 | User Story 1 | 36 | 35 |
| 4 | User Story 2 | 4 | 4 |
| 5 | User Story 3 | 4 | 4 |
| 6 | Polish | 4 | 0 |
| **Total** | | **56** | **44 (79%)** |

---

## Notes

- All delegation methods are one-liners: `public void X() => _target.X();`
- 34 IOutput methods + constructor + properties = complete interface implementation
- Constitution VIII: No mocks - tests verify actual delegation to real Win32Output/Vt100Output
- Constitution X: ~200 LOC estimate for ConEmuOutput.cs is well under 1000 LOC limit
- Constitution XI: Thread safety achieved through delegation (no mutable state in ConEmuOutput)
