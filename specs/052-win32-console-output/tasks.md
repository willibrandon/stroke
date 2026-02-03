# Tasks: Win32 Console Output

**Input**: Design documents from `/specs/052-win32-console-output/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/Win32Output.md

**Tests**: Per Constitution VIII, real-world tests are required. Tests use xUnit with no mocks.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

---

## Phase 1: Setup (P/Invoke Infrastructure)

**Purpose**: Extend ConsoleApi with required P/Invoke methods and add Coord.ToInt32() helper

- [ ] T001 Add kernel32 P/Invoke methods to `src/Stroke/Input/Windows/ConsoleApi.cs`: SetConsoleTextAttribute, FillConsoleOutputCharacterW, FillConsoleOutputAttribute, WriteConsoleW, SetConsoleTitleW, CreateConsoleScreenBuffer, SetConsoleActiveScreenBuffer, SetConsoleWindowInfo, GetConsoleWindow
- [ ] T002 [P] Add user32.dll RedrawWindow P/Invoke and RDW_INVALIDATE constant to `src/Stroke/Input/Windows/ConsoleApi.cs`
- [ ] T003 [P] Add access constants (GENERIC_READ, GENERIC_WRITE, CONSOLE_TEXTMODE_BUFFER) to `src/Stroke/Input/Windows/ConsoleApi.cs`
- [ ] T004 [P] Add Coord.ToInt32() extension method to `src/Stroke/Input/Windows/Win32Types/Coord.cs` with packing logic `(Y << 16) | (X & 0xFFFF)`

**Checkpoint**: P/Invoke infrastructure ready for Win32Output implementation

---

## Phase 2: Foundational Types

**Purpose**: Color constants and exception types that all user stories depend on

**⚠️ CRITICAL**: Win32Output cannot be implemented until this phase completes

- [ ] T005 Create `src/Stroke/Output/Windows/ForegroundColor.cs` with static color constants (Black=0x0000, Blue=0x0001, Green=0x0002, Cyan=0x0003, Red=0x0004, Magenta=0x0005, Yellow=0x0006, Gray=0x0007, Intensity=0x0008)
- [ ] T006 [P] Create `src/Stroke/Output/Windows/BackgroundColor.cs` with static color constants (Black=0x0000, Blue=0x0010, Green=0x0020, Cyan=0x0030, Red=0x0040, Magenta=0x0050, Yellow=0x0060, Gray=0x0070, Intensity=0x0080)
- [ ] T007 [P] Create `src/Stroke/Output/NoConsoleScreenBufferError.cs` with context-aware message (checks TERM env var for xterm, suggests winpty or cmd.exe)
- [ ] T008 Create `src/Stroke/Output/Windows/ColorLookupTable.cs` with thread-safe cache, 17 ANSI color mappings, 16-color RGB distance matching, LookupFgColor/LookupBgColor methods

**Checkpoint**: Foundational types ready - user story implementation can begin

---

## Phase 3: User Story 1 - Terminal Developer Uses Legacy Windows Console (Priority: P1) 🎯 MVP

**Goal**: Basic Win32Output with Write, Flush, and cursor operations on legacy Windows console

**Independent Test**: Create Win32Output instance, call Write/Flush/CursorGoto, verify text appears at correct position

### Tests for User Story 1

- [ ] T009 [P] [US1] Create test file `tests/Stroke.Tests/Output/Windows/Win32OutputTests.cs` with constructor tests (platform check, console detection, exception scenarios)
- [ ] T010 [P] [US1] Add Write/WriteRaw/Flush tests to `tests/Stroke.Tests/Output/Windows/Win32OutputTests.cs` verifying buffering and character-by-character output
- [ ] T011 [P] [US1] Add cursor positioning tests (CursorGoto, CursorUp/Down/Forward/Backward) to `tests/Stroke.Tests/Output/Windows/Win32OutputTests.cs`

### Implementation for User Story 1

- [ ] T012 [US1] Create `src/Stroke/Output/Windows/Win32Output.cs` with constructor (platform check, handle acquisition, default attributes capture), IOutput implementation stub
- [ ] T013 [US1] Implement Write, WriteRaw, Flush methods in `src/Stroke/Output/Windows/Win32Output.cs` with thread-safe buffer and character-by-character WriteConsoleW
- [ ] T014 [US1] Implement CursorGoto, CursorUp, CursorDown, CursorForward, CursorBackward in `src/Stroke/Output/Windows/Win32Output.cs` using SetConsoleCursorPosition
- [ ] T015 [US1] Implement GetSize, GetRowsBelowCursorPosition in `src/Stroke/Output/Windows/Win32Output.cs` with visible window vs buffer width handling
- [ ] T016 [US1] Implement property accessors (Encoding, Fileno, RespondsToCpr, Stdout, UseCompleteWidth, DefaultColorDepth, GetDefaultColorDepth) in `src/Stroke/Output/Windows/Win32Output.cs`

**Checkpoint**: Win32Output MVP functional - basic text output and cursor control on Windows console

---

## Phase 4: User Story 2 - Application Displays Styled Text with Color Mapping (Priority: P1)

**Goal**: Full color attribute support including ANSI names, RGB mapping, and reverse/hidden attributes

**Independent Test**: Set various foreground/background colors (ANSI names and RGB hex), verify correct Win32 attributes applied

### Tests for User Story 2

- [ ] T017 [P] [US2] Create test file `tests/Stroke.Tests/Output/Windows/ColorLookupTableTests.cs` with ANSI color lookup tests (17 colors)
- [ ] T018 [P] [US2] Add RGB distance matching tests to `tests/Stroke.Tests/Output/Windows/ColorLookupTableTests.cs` verifying closest color selection
- [ ] T019 [P] [US2] Add cache thread safety tests to `tests/Stroke.Tests/Output/Windows/ColorLookupTableTests.cs` (10+ threads, 1000+ operations)
- [ ] T020 [P] [US2] Create test file `tests/Stroke.Tests/Output/Windows/Win32OutputColorTests.cs` with SetAttributes/ResetAttributes tests

### Implementation for User Story 2

- [ ] T021 [US2] Create `src/Stroke/Output/Windows/Win32Output.Colors.cs` partial class with SetAttributes implementation (color depth handling, attribute combination)
- [ ] T022 [US2] Implement reverse attribute bit-swap in `src/Stroke/Output/Windows/Win32Output.Colors.cs` (swap foreground/background 4-bit values)
- [ ] T023 [US2] Implement hidden text flag management in `src/Stroke/Output/Windows/Win32Output.Colors.cs` and integrate with Write method space replacement
- [ ] T024 [US2] Implement ResetAttributes in `src/Stroke/Output/Windows/Win32Output.Colors.cs` using saved default attributes

**Checkpoint**: Full color support functional - ANSI names, RGB mapping, reverse, hidden all working

---

## Phase 5: User Story 3 - Full-Screen Application Uses Alternate Buffer (Priority: P2)

**Goal**: Alternate screen buffer support for full-screen applications

**Independent Test**: Enter alternate screen, write content, exit, verify original content preserved

### Tests for User Story 3

- [ ] T025 [P] [US3] Add alternate screen tests to `tests/Stroke.Tests/Output/Windows/Win32OutputTests.cs` (enter, exit, idempotency, content preservation)

### Implementation for User Story 3

- [ ] T026 [US3] Implement EnterAlternateScreen in `src/Stroke/Output/Windows/Win32Output.cs` using CreateConsoleScreenBuffer + SetConsoleActiveScreenBuffer
- [ ] T027 [US3] Implement QuitAlternateScreen in `src/Stroke/Output/Windows/Win32Output.cs` with handle restoration and CloseHandle cleanup
- [ ] T028 [US3] Add _inAlternateScreen, _originalHandle, _alternateHandle state management with proper locking in `src/Stroke/Output/Windows/Win32Output.cs`

**Checkpoint**: Alternate screen buffer functional - full-screen apps can preserve terminal content

---

## Phase 6: User Story 4 - Application Clears and Erases Screen Content (Priority: P2)

**Goal**: Screen erase operations for UI updates

**Independent Test**: Write content, call erase methods, verify expected regions cleared

### Tests for User Story 4

- [ ] T029 [P] [US4] Add erase operation tests to `tests/Stroke.Tests/Output/Windows/Win32OutputTests.cs` (EraseScreen, EraseEndOfLine, EraseDown)

### Implementation for User Story 4

- [ ] T030 [US4] Implement EraseScreen in `src/Stroke/Output/Windows/Win32Output.cs` using FillConsoleOutputCharacterW + FillConsoleOutputAttribute, cursor home
- [ ] T031 [US4] Implement EraseEndOfLine in `src/Stroke/Output/Windows/Win32Output.cs` (cursor X to line end)
- [ ] T032 [US4] Implement EraseDown in `src/Stroke/Output/Windows/Win32Output.cs` (cursor position to buffer end)

**Checkpoint**: Screen erase operations functional - dynamic UI updates supported

---

## Phase 7: User Story 5 - Application Enables Mouse Input (Priority: P3)

**Goal**: Mouse input support via console mode flags

**Independent Test**: Enable mouse support, verify console mode flags correctly set

### Tests for User Story 5

- [ ] T033 [P] [US5] Add mouse support tests to `tests/Stroke.Tests/Output/Windows/Win32OutputTests.cs` (enable, disable, mode flag verification)

### Implementation for User Story 5

- [ ] T034 [US5] Implement EnableMouseSupport in `src/Stroke/Output/Windows/Win32Output.cs` (ENABLE_MOUSE_INPUT | ~ENABLE_QUICK_EDIT_MODE on stdin)
- [ ] T035 [US5] Implement DisableMouseSupport in `src/Stroke/Output/Windows/Win32Output.cs` (~ENABLE_MOUSE_INPUT on stdin)

**Checkpoint**: Mouse support functional - interactive applications can receive mouse events

---

## Phase 8: User Story 6 - Platform Detection and Graceful Failure (Priority: P1)

**Goal**: Clear error messages for non-Windows or non-console contexts

**Independent Test**: Attempt Win32Output on various contexts, verify appropriate exceptions with helpful messages

### Tests for User Story 6

- [ ] T036 [P] [US6] Add platform detection tests to `tests/Stroke.Tests/Output/Windows/Win32OutputTests.cs` (non-Windows, non-console scenarios)
- [ ] T037 [P] [US6] Add NoConsoleScreenBufferError message tests to `tests/Stroke.Tests/Output/NoConsoleScreenBufferErrorTests.cs` (xterm detection, winpty suggestion)

### Implementation for User Story 6

(Platform detection is implemented in T012 constructor; this phase adds remaining edge case handling)

- [ ] T038 [US6] Verify constructor throws PlatformNotSupportedException on non-Windows in `src/Stroke/Output/Windows/Win32Output.cs`
- [ ] T039 [US6] Verify constructor throws NoConsoleScreenBufferError when GetConsoleScreenBufferInfo fails in `src/Stroke/Output/Windows/Win32Output.cs`

**Checkpoint**: Platform detection functional - developers get actionable error messages

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Remaining IOutput methods, documentation, and cross-story integration

- [ ] T040 Implement remaining no-op methods in `src/Stroke/Output/Windows/Win32Output.cs`: SetCursorShape, ResetCursorShape, DisableAutowrap, EnableAutowrap, EnableBracketedPaste, DisableBracketedPaste, ResetCursorKeyMode, AskForCpr, ScrollBufferToPrompt
- [ ] T041 [P] Implement HideCursor, ShowCursor via SetConsoleCursorInfo in `src/Stroke/Output/Windows/Win32Output.cs`
- [ ] T042 [P] Implement SetTitle, ClearTitle via SetConsoleTitleW in `src/Stroke/Output/Windows/Win32Output.cs`
- [ ] T043 [P] Implement Bell method in `src/Stroke/Output/Windows/Win32Output.cs` (write '\a' or MessageBeep)
- [ ] T044 [P] Implement static Win32RefreshWindow in `src/Stroke/Output/Windows/Win32Output.cs` using GetConsoleWindow + RedrawWindow
- [ ] T045 Add thread safety documentation (XML comments) to all public types per Constitution XI
- [ ] T046 Run quickstart.md validation scenarios manually on Windows
- [ ] T047 [P] Run `dotnet test --collect:"XPlat Code Coverage"` and verify ≥80% coverage per SC-007

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion - BLOCKS all user stories
- **User Stories (Phases 3-8)**: All depend on Foundational phase completion
  - US1 (Phase 3) can start after Phase 2
  - US2 (Phase 4) depends on US1 (needs Win32Output base class)
  - US3-6 (Phases 5-8) can proceed in parallel after US1
- **Polish (Phase 9)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Core Win32Output - all other stories depend on this
- **User Story 2 (P1)**: Color system - depends on US1 for Win32Output class
- **User Story 3 (P2)**: Alternate screen - depends on US1 only
- **User Story 4 (P2)**: Screen erase - depends on US1 only
- **User Story 5 (P3)**: Mouse support - depends on US1 only
- **User Story 6 (P1)**: Error handling - partially in US1 constructor, remainder independent

### Within Each User Story

- Tests written first (TDD approach per Constitution VIII)
- Core implementation after tests
- Integration and edge cases last

### Parallel Opportunities

Within Phase 1:
- T002, T003, T004 can run in parallel (different methods/files)

Within Phase 2:
- T005, T006, T007 can run in parallel (different files)

Within each User Story:
- All test tasks marked [P] can run in parallel
- Implementation tasks typically sequential within a story

Across User Stories (after US1 complete):
- US3, US4, US5, US6 can proceed in parallel

---

## Parallel Example: Phase 2 Foundational

```bash
# Launch in parallel:
Task: "Create src/Stroke/Output/Windows/ForegroundColor.cs"
Task: "Create src/Stroke/Output/Windows/BackgroundColor.cs"
Task: "Create src/Stroke/Output/NoConsoleScreenBufferError.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (P/Invoke)
2. Complete Phase 2: Foundational (color constants, exception)
3. Complete Phase 3: User Story 1 (basic Win32Output)
4. **STOP and VALIDATE**: Test on Windows cmd.exe
5. Deploy/demo basic console output

### Incremental Delivery

1. Phase 1 + 2 → Infrastructure ready
2. + User Story 1 → Basic output (MVP!)
3. + User Story 2 → Color support
4. + User Story 3 → Alternate screen
5. + User Story 4 → Erase operations
6. + User Story 5 → Mouse support
7. + User Story 6 → Error handling polish
8. + Phase 9 → Production ready

### File Count Summary

| Directory | Files |
|-----------|-------|
| `src/Stroke/Output/Windows/` | 5 files (Win32Output.cs, Win32Output.Colors.cs, ColorLookupTable.cs, ForegroundColor.cs, BackgroundColor.cs) |
| `src/Stroke/Output/` | 1 file (NoConsoleScreenBufferError.cs) |
| `src/Stroke/Input/Windows/` | 1 file modified (ConsoleApi.cs) |
| `src/Stroke/Input/Windows/Win32Types/` | 1 file modified (Coord.cs) |
| `tests/Stroke.Tests/Output/Windows/` | 3 files (Win32OutputTests.cs, Win32OutputColorTests.cs, ColorLookupTableTests.cs) |
| `tests/Stroke.Tests/Output/` | 1 file (NoConsoleScreenBufferErrorTests.cs) |

**Total**: 8 source files, 4 test files, 47 tasks

---

## Notes

- All tasks follow Constitution principles (I: Faithful Port, VIII: Real-World Testing, XI: Thread Safety)
- Tests use xUnit with no mocks per Constitution VIII
- Thread safety via `System.Threading.Lock` per Constitution XI
- Character-by-character output to avoid Windows Console rendering artifacts (per research.md)
- RGB distance matching uses Euclidean squared distance (per data-model.md)
