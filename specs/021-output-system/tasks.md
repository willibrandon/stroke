# Tasks: Output System

**Input**: Design documents from `/specs/021-output-system/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/IOutput.md

**Tests**: Included (Constitution VIII requires 80% test coverage with real implementations)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Output/` and `src/Stroke/CursorShapes/`
- **Tests**: `tests/Stroke.Tests/Output/` and `tests/Stroke.Tests/CursorShapes/`
- **Internal**: `src/Stroke/Output/Internal/` for cache classes

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and namespace structure

- [ ] T001 Create Output namespace directory structure in src/Stroke/Output/
- [ ] T002 [P] Create CursorShapes namespace directory structure in src/Stroke/CursorShapes/
- [ ] T003 [P] Create Internal directory for caches in src/Stroke/Output/Internal/
- [ ] T004 [P] Create test directory structure in tests/Stroke.Tests/Output/
- [ ] T005 [P] Create test directory structure in tests/Stroke.Tests/CursorShapes/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core enums and interfaces that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T006 Implement ColorDepth enum with Depth1Bit, Depth4Bit, Depth8Bit, Depth24Bit values in src/Stroke/Output/ColorDepth.cs
- [ ] T007 Implement ColorDepthExtensions static class with FromEnvironment() method (reads NO_COLOR, STROKE_COLOR_DEPTH, TERM) in src/Stroke/Output/ColorDepth.cs
- [ ] T008 [P] Implement CursorShape enum with NeverChange, Block, Beam, Underline, BlinkingBlock, BlinkingBeam, BlinkingUnderline values in src/Stroke/CursorShapes/CursorShape.cs
- [ ] T009 [P] Implement IOutput interface with all 35 methods and 2 properties (37 total members) in src/Stroke/Output/IOutput.cs
- [ ] T010 [P] Implement ICursorShapeConfig interface in src/Stroke/CursorShapes/ICursorShapeConfig.cs
- [ ] T011 [P] Write ColorDepthTests testing enum values, FromEnvironment(), and Default property in tests/Stroke.Tests/Output/ColorDepthTests.cs
- [ ] T012 [P] Write CursorShapeTests testing enum values and DECSCUSR code mappings in tests/Stroke.Tests/CursorShapes/CursorShapeTests.cs

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Terminal Output with VT100 Escape Sequences (Priority: P1) 🎯 MVP

**Goal**: Write formatted text to terminal with proper cursor control, color output, and screen management using VT100/ANSI escape sequences

**Independent Test**: Create Vt100Output with StringWriter, verify all escape sequences match VT100 Escape Sequence Reference table

### Tests for User Story 1

- [ ] T013 [P] [US1] Write Vt100OutputTests for Write(), WriteRaw(), Flush() buffer behavior in tests/Stroke.Tests/Output/Vt100OutputTests.cs
- [ ] T014 [P] [US1] Write Vt100OutputCursorTests for CursorGoto, CursorUp/Down/Forward/Backward sequences in tests/Stroke.Tests/Output/Vt100OutputCursorTests.cs
- [ ] T015 [P] [US1] Write Vt100OutputScreenTests for EraseScreen, EraseEndOfLine, EraseDown sequences in tests/Stroke.Tests/Output/Vt100OutputScreenTests.cs

### Implementation for User Story 1

- [ ] T016 [US1] Implement Vt100Output base class with _stdout, _buffer, _lock fields in src/Stroke/Output/Vt100Output.cs
- [ ] T017 [US1] Implement Write() with \x1b→? replacement, WriteRaw() verbatim, Flush() buffer-to-stdout in src/Stroke/Output/Vt100Output.cs
- [ ] T018 [US1] Implement CursorGoto(row, col) with \x1b[{row};{col}H sequence in src/Stroke/Output/Vt100Output.Cursor.cs
- [ ] T019 [US1] Implement CursorUp/Down/Forward/Backward with optimized n=1 sequences in src/Stroke/Output/Vt100Output.Cursor.cs
- [ ] T020 [US1] Implement EraseScreen (\x1b[2J), EraseEndOfLine (\x1b[K), EraseDown (\x1b[J) in src/Stroke/Output/Vt100Output.cs
- [ ] T021 [US1] Implement GetSize() using Console.WindowWidth/WindowHeight with 80×24 fallback in src/Stroke/Output/Vt100Output.cs
- [ ] T022 [US1] Implement Fileno(), Encoding property, GetDefaultColorDepth() using TERM detection in src/Stroke/Output/Vt100Output.cs
- [ ] T023 [US1] Implement FromPty() factory method for Vt100Output construction in src/Stroke/Output/Vt100Output.cs
- [ ] T024 [US1] Add thread-safety with Lock and EnterScope() pattern for all mutable state in src/Stroke/Output/Vt100Output.cs

**Checkpoint**: User Story 1 complete - VT100 output with cursor/screen control works

---

## Phase 4: User Story 2 - Color Depth Management (Priority: P1)

**Goal**: Detect terminal color capabilities and adapt output (1-bit to 24-bit true color)

**Independent Test**: Set environment variables, verify color depth detection; test color code generation at each depth

### Tests for User Story 2

- [ ] T025 [P] [US2] Write SixteenColorCacheTests for RGB→16-color mapping with saturation and exclusion in tests/Stroke.Tests/Output/Internal/SixteenColorCacheTests.cs
- [ ] T026 [P] [US2] Write TwoFiftySixColorCacheTests for RGB→256-color mapping (cube + grayscale) in tests/Stroke.Tests/Output/Internal/TwoFiftySixColorCacheTests.cs
- [ ] T027 [P] [US2] Write EscapeCodeCacheTests for Attrs→escape sequence caching in tests/Stroke.Tests/Output/Internal/EscapeCodeCacheTests.cs
- [ ] T028 [P] [US2] Write Vt100OutputColorTests for SetAttributes at each color depth in tests/Stroke.Tests/Output/Vt100OutputColorTests.cs

### Implementation for User Story 2

- [ ] T029 [US2] Implement SixteenColorCache with Euclidean distance, saturation>30 gray exclusion in src/Stroke/Output/Internal/SixteenColorCache.cs
- [ ] T030 [US2] Implement TwoFiftySixColorCache with 6×6×6 cube (16-231) + grayscale (232-255) in src/Stroke/Output/Internal/TwoFiftySixColorCache.cs
- [ ] T031 [US2] Implement EscapeCodeCache with Dictionary<Attrs, string> per ColorDepth in src/Stroke/Output/Internal/EscapeCodeCache.cs
- [ ] T032 [US2] Implement ANSI 16-color palette RGB values (matching Python PTK FG_ANSI_COLORS) in src/Stroke/Output/Internal/SixteenColorCache.cs
- [ ] T033 [US2] Implement ResetAttributes() with \x1b[0m sequence in src/Stroke/Output/Vt100Output.Colors.cs
- [ ] T034 [US2] Implement SetAttributes(Attrs, ColorDepth) using caches for color code generation in src/Stroke/Output/Vt100Output.Colors.cs
- [ ] T035 [US2] Implement foreground/background collision avoidance (exclude fg color when mapping bg) in src/Stroke/Output/Internal/SixteenColorCache.cs
- [ ] T036 [US2] Add thread-safety to all cache classes using ConcurrentDictionary or Lock in src/Stroke/Output/Internal/

**Checkpoint**: User Story 2 complete - color depth detection and color output work at all depths

---

## Phase 5: User Story 3 - Cursor Shape Control (Priority: P2)

**Goal**: Change cursor appearance (block, beam, underline, blinking variants) for visual mode feedback

**Independent Test**: Call SetCursorShape with various shapes, verify DECSCUSR escape sequences

### Tests for User Story 3

- [ ] T037 [P] [US3] Write CursorShape DECSCUSR mapping tests (Block→2, Beam→6, etc.) in tests/Stroke.Tests/CursorShapes/CursorShapeTests.cs (extend)
- [ ] T038 [P] [US3] Write Vt100Output SetCursorShape/ResetCursorShape tests in tests/Stroke.Tests/Output/Vt100OutputCursorTests.cs (extend)

### Implementation for User Story 3

- [ ] T039 [US3] Implement SetCursorShape(CursorShape) with DECSCUSR sequences (\x1b[N q) in src/Stroke/Output/Vt100Output.Cursor.cs
- [ ] T040 [US3] Implement _cursorShapeChanged flag tracking in src/Stroke/Output/Vt100Output.Cursor.cs
- [ ] T041 [US3] Implement ResetCursorShape() with \x1b[0 q (only if shape was changed) in src/Stroke/Output/Vt100Output.Cursor.cs
- [ ] T042 [US3] Implement NeverChange handling (no-op in SetCursorShape) in src/Stroke/Output/Vt100Output.Cursor.cs

**Checkpoint**: User Story 3 complete - cursor shape control works with proper state tracking

---

## Phase 6: User Story 4 - Terminal Feature Toggling (Priority: P2)

**Goal**: Enable/disable mouse support, alternate screen, bracketed paste mode

**Independent Test**: Call feature toggle methods, verify correct escape sequences

### Tests for User Story 4

- [ ] T043 [P] [US4] Write alternate screen enter/exit tests in tests/Stroke.Tests/Output/Vt100OutputTests.cs (extend)
- [ ] T044 [P] [US4] Write mouse mode enable/disable tests in tests/Stroke.Tests/Output/Vt100OutputTests.cs (extend)
- [ ] T045 [P] [US4] Write bracketed paste tests in tests/Stroke.Tests/Output/Vt100OutputTests.cs (extend)
- [ ] T046 [P] [US4] Write title setting/clearing tests in tests/Stroke.Tests/Output/Vt100OutputTests.cs (extend)

### Implementation for User Story 4

- [ ] T047 [US4] Implement EnterAlternateScreen() with \x1b[?1049h\x1b[H in src/Stroke/Output/Vt100Output.cs
- [ ] T048 [US4] Implement QuitAlternateScreen() with \x1b[?1049l in src/Stroke/Output/Vt100Output.cs
- [ ] T049 [US4] Implement EnableMouseSupport() with basic+drag+urxvt+SGR modes in src/Stroke/Output/Vt100Output.cs
- [ ] T050 [US4] Implement DisableMouseSupport() with all modes disabled in src/Stroke/Output/Vt100Output.cs
- [ ] T051 [US4] Implement EnableBracketedPaste() with \x1b[?2004h in src/Stroke/Output/Vt100Output.cs
- [ ] T052 [US4] Implement DisableBracketedPaste() with \x1b[?2004l in src/Stroke/Output/Vt100Output.cs
- [ ] T053 [US4] Implement SetTitle(title) with \x1b]2;{escaped}\x07, stripping ESC and BEL, skip for linux/eterm-color TERM in src/Stroke/Output/Vt100Output.cs
- [ ] T054 [US4] Implement ClearTitle() with \x1b]2;\x07 in src/Stroke/Output/Vt100Output.cs
- [ ] T055 [US4] Implement Bell() with \x07 (conditional on _enableBell) in src/Stroke/Output/Vt100Output.cs
- [ ] T056 [US4] Implement DisableAutowrap() and EnableAutowrap() in src/Stroke/Output/Vt100Output.cs
- [ ] T057 [US4] Implement AskForCpr() with \x1b[6n and RespondsToCpr property in src/Stroke/Output/Vt100Output.cs
- [ ] T058 [US4] Implement ResetCursorKeyMode() with \x1b[?1l in src/Stroke/Output/Vt100Output.cs

**Checkpoint**: User Story 4 complete - all terminal features toggleable

---

## Phase 7: User Story 5 - Platform-Agnostic Output Factory (Priority: P2)

**Goal**: Auto-select appropriate output implementation based on platform and terminal detection

**Independent Test**: Test with various TTY/non-TTY scenarios, verify correct output type returned

### Tests for User Story 5

- [ ] T059 [P] [US5] Write OutputFactoryTests for TTY→Vt100Output in tests/Stroke.Tests/Output/OutputFactoryTests.cs
- [ ] T060 [P] [US5] Write OutputFactoryTests for redirected→PlainTextOutput in tests/Stroke.Tests/Output/OutputFactoryTests.cs
- [ ] T061 [P] [US5] Write OutputFactoryTests for null stdout→DummyOutput in tests/Stroke.Tests/Output/OutputFactoryTests.cs
- [ ] T062 [P] [US5] Write OutputFactoryTests for alwaysPreferTty stderr fallback in tests/Stroke.Tests/Output/OutputFactoryTests.cs

### Implementation for User Story 5

- [ ] T063 [US5] Implement OutputFactory.Create() with platform and TTY detection in src/Stroke/Output/OutputFactory.cs
- [ ] T064 [US5] Implement stdout null check → DummyOutput return in src/Stroke/Output/OutputFactory.cs
- [ ] T065 [US5] Implement Console.IsOutputRedirected check → PlainTextOutput for redirected in src/Stroke/Output/OutputFactory.cs
- [ ] T066 [US5] Implement alwaysPreferTty parameter with stderr fallback logic in src/Stroke/Output/OutputFactory.cs

**Checkpoint**: User Story 5 complete - factory auto-selects correct output type

---

## Phase 8: User Story 6 - Plain Text Output for Redirected Streams (Priority: P3)

**Goal**: Write plain text without escape sequences when output is redirected to file/pipe

**Independent Test**: Create PlainTextOutput with StringWriter, verify no escape sequences written

### Tests for User Story 6

- [ ] T067 [P] [US6] Write PlainTextOutputTests for Write/WriteRaw (no escaping needed) in tests/Stroke.Tests/Output/PlainTextOutputTests.cs
- [ ] T068 [P] [US6] Write PlainTextOutputTests for CursorForward→spaces, CursorDown→newlines in tests/Stroke.Tests/Output/PlainTextOutputTests.cs
- [ ] T069 [P] [US6] Write PlainTextOutputTests for color/attribute methods as no-ops in tests/Stroke.Tests/Output/PlainTextOutputTests.cs

### Implementation for User Story 6

- [ ] T070 [US6] Implement PlainTextOutput class with _stdout, _buffer, _lock fields in src/Stroke/Output/PlainTextOutput.cs
- [ ] T071 [US6] Implement Write() and WriteRaw() both adding text directly to buffer in src/Stroke/Output/PlainTextOutput.cs
- [ ] T072 [US6] Implement Flush() writing buffer to stdout in src/Stroke/Output/PlainTextOutput.cs
- [ ] T073 [US6] Implement CursorForward(n) writing n spaces in src/Stroke/Output/PlainTextOutput.cs
- [ ] T074 [US6] Implement CursorDown(n) writing n newlines in src/Stroke/Output/PlainTextOutput.cs
- [ ] T075 [US6] Implement all escape-sequence methods as no-ops in src/Stroke/Output/PlainTextOutput.cs
- [ ] T076 [US6] Implement GetSize()→Size(40,80), GetDefaultColorDepth()→Depth1Bit in src/Stroke/Output/PlainTextOutput.cs
- [ ] T077 [US6] Add thread-safety with Lock for buffer access in src/Stroke/Output/PlainTextOutput.cs

**Checkpoint**: User Story 6 complete - redirected output is clean plain text

---

## Phase 9: User Story 7 - Testing with DummyOutput (Priority: P3)

**Goal**: No-op output implementation for unit testing without terminal dependencies

**Independent Test**: Create DummyOutput, verify all methods complete without error

### Tests for User Story 7

- [ ] T078 [P] [US7] Write DummyOutputTests for all methods complete without error in tests/Stroke.Tests/Output/DummyOutputTests.cs
- [ ] T079 [P] [US7] Write DummyOutputTests for GetSize()→Size(40,80) in tests/Stroke.Tests/Output/DummyOutputTests.cs
- [ ] T080 [P] [US7] Write DummyOutputTests for Fileno()→NotImplementedException in tests/Stroke.Tests/Output/DummyOutputTests.cs
- [ ] T081 [P] [US7] Write DummyOutputTests for GetDefaultColorDepth()→Depth1Bit in tests/Stroke.Tests/Output/DummyOutputTests.cs

### Implementation for User Story 7

- [ ] T082 [US7] Implement DummyOutput class (stateless) in src/Stroke/Output/DummyOutput.cs
- [ ] T083 [US7] Implement all output methods as no-ops (empty method bodies) in src/Stroke/Output/DummyOutput.cs
- [ ] T084 [US7] Implement GetSize()→new Size(40, 80) in src/Stroke/Output/DummyOutput.cs
- [ ] T085 [US7] Implement Fileno()→throw NotImplementedException in src/Stroke/Output/DummyOutput.cs
- [ ] T086 [US7] Implement GetDefaultColorDepth()→ColorDepth.Depth1Bit in src/Stroke/Output/DummyOutput.cs
- [ ] T087 [US7] Implement Encoding→"utf-8", RespondsToCpr→false in src/Stroke/Output/DummyOutput.cs

**Checkpoint**: User Story 7 complete - DummyOutput available for testing

---

## Phase 10: User Story 8 - Thread-Safe Concurrent Output (Priority: P2)

**Goal**: Safe concurrent write/flush operations without data corruption or race conditions

**Independent Test**: Spawn multiple threads calling Write/Flush concurrently, verify no exceptions or corruption

### Tests for User Story 8

- [ ] T088 [P] [US8] Write concurrent Write() tests (100 threads × 1000 cycles) in tests/Stroke.Tests/Output/Vt100OutputConcurrencyTests.cs
- [ ] T089 [P] [US8] Write concurrent Write()+Flush() interleaving tests in tests/Stroke.Tests/Output/Vt100OutputConcurrencyTests.cs
- [ ] T090 [P] [US8] Write concurrent HideCursor/ShowCursor state consistency tests in tests/Stroke.Tests/Output/Vt100OutputConcurrencyTests.cs
- [ ] T091 [P] [US8] Write concurrent EscapeCodeCache access tests in tests/Stroke.Tests/Output/Internal/EscapeCodeCacheConcurrencyTests.cs

### Implementation for User Story 8

- [ ] T092 [US8] Verify all Vt100Output mutable state protected by Lock (T024 completed) in src/Stroke/Output/Vt100Output.cs
- [ ] T093 [US8] Verify all cache classes use ConcurrentDictionary (T036 completed) in src/Stroke/Output/Internal/
- [ ] T094 [US8] Add _cursorVisible state protection with Lock in src/Stroke/Output/Vt100Output.Cursor.cs
- [ ] T095 [US8] Document thread safety guarantees in XML comments for all mutable classes

**Checkpoint**: User Story 8 complete - thread-safe concurrent access verified

---

## Phase 11: User Story 9 - Cursor Shape Configuration (Priority: P3)

**Goal**: Configure cursor shape based on editing mode (Vi navigation/insert) or application state

**Independent Test**: Create cursor shape configs, verify correct shapes for different states

### Tests for User Story 9

- [ ] T096 [P] [US9] Write SimpleCursorShapeConfigTests in tests/Stroke.Tests/CursorShapes/SimpleCursorShapeConfigTests.cs
- [ ] T097 [P] [US9] Write ModalCursorShapeConfigTests (Vi nav→Block, insert→Beam) in tests/Stroke.Tests/CursorShapes/ModalCursorShapeConfigTests.cs
- [ ] T098 [P] [US9] Write DynamicCursorShapeConfigTests in tests/Stroke.Tests/CursorShapes/DynamicCursorShapeConfigTests.cs

### Implementation for User Story 9

- [ ] T099 [US9] Implement SimpleCursorShapeConfig with fixed CursorShape property in src/Stroke/CursorShapes/SimpleCursorShapeConfig.cs
- [ ] T100 [US9] Implement ModalCursorShapeConfig with Vi/Emacs mode detection in src/Stroke/CursorShapes/ModalCursorShapeConfig.cs
- [ ] T101 [US9] Implement DynamicCursorShapeConfig with Func<ICursorShapeConfig?> wrapper in src/Stroke/CursorShapes/DynamicCursorShapeConfig.cs

**Checkpoint**: User Story 9 complete - cursor shape configuration available

---

## Phase 12: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, edge cases, and documentation

- [ ] T102 [P] Implement FlushStdout helper class for immediate write-and-flush in src/Stroke/Output/FlushStdout.cs
- [ ] T103 [P] Write FlushStdoutTests in tests/Stroke.Tests/Output/FlushStdoutTests.cs
- [ ] T104 [P] Implement Windows-specific optional methods (ScrollBufferToPrompt, GetRowsBelowCursorPosition) in src/Stroke/Output/Vt100Output.cs
- [ ] T105 [P] Add HideCursor/ShowCursor state optimization (no duplicate sequences) in src/Stroke/Output/Vt100Output.Cursor.cs
- [ ] T106 [P] Add cursor movement edge case handling (amount=0→no-op, negative→treat as 0) in src/Stroke/Output/Vt100Output.Cursor.cs
- [ ] T107 [P] Add null argument validation (ArgumentNullException) for Write/WriteRaw in src/Stroke/Output/Vt100Output.cs
- [ ] T108 Add XML documentation comments to all public types and members
- [ ] T109 Run test coverage report and verify ≥80% coverage for all output classes
- [ ] T110 Run quickstart.md examples as integration validation
- [ ] T111 [P] Add memory profiling test to verify color cache memory usage ≤10KB (NFR-003) in tests/Stroke.Tests/Output/Internal/CacheMemoryTests.cs
- [ ] T112 [P] Implement I/O exception resilience in Flush() - log and continue on write failure (NFR-006) in src/Stroke/Output/Vt100Output.cs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Stories (Phase 3-11)**: All depend on Foundational phase completion
  - US1 (P1) and US2 (P1) can run in parallel after Foundational
  - US3-9 can run after their priorities allow
- **Polish (Phase 12)**: Depends on all user stories being complete

### User Story Dependencies

| Story | Priority | Dependencies | Can Start After |
|-------|----------|--------------|-----------------|
| US1 - VT100 Output | P1 | Foundational | Phase 2 |
| US2 - Color Depth | P1 | Foundational | Phase 2 |
| US3 - Cursor Shape | P2 | US1 (cursor methods exist) | Phase 3 |
| US4 - Terminal Features | P2 | US1 (Vt100Output exists) | Phase 3 |
| US5 - Output Factory | P2 | US1, US6, US7 (all output types) | Phases 3, 8, 9 |
| US6 - Plain Text | P3 | Foundational | Phase 2 |
| US7 - Dummy Output | P3 | Foundational | Phase 2 |
| US8 - Thread Safety | P2 | US1, US2 (mutable classes exist) | Phases 3, 4 |
| US9 - Cursor Config | P3 | Foundational | Phase 2 |

### Parallel Opportunities

**Phase 2 (Foundational)**:
```
T006 ColorDepth || T008 CursorShape || T009 IOutput || T010 ICursorShapeConfig
T011 ColorDepthTests || T012 CursorShapeTests
```

**Phase 3 (US1)**:
```
T013 Vt100OutputTests || T014 CursorTests || T015 ScreenTests
```

**Phase 4 (US2)**:
```
T025 SixteenColorCacheTests || T026 TwoFiftySixColorCacheTests || T027 EscapeCodeCacheTests || T028 ColorTests
```

**Phase 10 (US8)**:
```
T088 ConcurrentWriteTests || T089 InterleaveTests || T090 CursorStateTests || T091 CacheConcurrencyTests
```

---

## Implementation Strategy

### MVP First (User Stories 1-2 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (VT100 Output)
4. Complete Phase 4: User Story 2 (Color Depth)
5. **STOP and VALIDATE**: Test escape sequences against VT100 Reference table
6. Deploy/demo if ready - basic terminal output works

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1 + US2 → Core VT100 output with colors (MVP!)
3. US3 + US4 → Cursor shapes + terminal features
4. US5 + US6 + US7 → Factory + plain text + dummy
5. US8 → Thread safety verification
6. US9 → Cursor configuration
7. Polish → Documentation, edge cases, coverage validation

### Suggested MVP Scope

**Minimum Viable Product**: User Stories 1 and 2 (Phases 1-4)
- VT100 escape sequences for cursor and screen control
- Color depth detection and color output at all depths
- This enables basic terminal rendering

---

## Summary

| Metric | Value |
|--------|-------|
| Total Tasks | 112 |
| Setup Phase | 5 tasks |
| Foundational Phase | 7 tasks |
| User Story Tasks | 89 tasks (across 9 stories) |
| Polish Phase | 11 tasks |
| Parallel Opportunities | 54 tasks marked [P] |
| Source Files | 15 files |
| Test Files | 16 files |

### Tasks per User Story

| User Story | Priority | Task Count |
|------------|----------|------------|
| US1 - VT100 Output | P1 | 12 |
| US2 - Color Depth | P1 | 12 |
| US3 - Cursor Shape | P2 | 6 |
| US4 - Terminal Features | P2 | 16 |
| US5 - Output Factory | P2 | 8 |
| US6 - Plain Text | P3 | 11 |
| US7 - Dummy Output | P3 | 10 |
| US8 - Thread Safety | P2 | 8 |
| US9 - Cursor Config | P3 | 6 |

### Independent Test Criteria per Story

| Story | Independent Test Criteria |
|-------|---------------------------|
| US1 | StringWriter captures correct VT100 escape sequences for all operations |
| US2 | Environment variables correctly detected; color codes match Python PTK at all depths |
| US3 | DECSCUSR sequences match reference (\x1b[2 q for Block, etc.) |
| US4 | All feature toggle sequences match VT100 Reference table |
| US5 | Correct output type returned for TTY/non-TTY/null scenarios |
| US6 | No escape sequences in output; spaces/newlines for cursor movement |
| US7 | All methods complete without error; default values returned |
| US8 | 100 threads × 1000 cycles with no exceptions or corruption |
| US9 | Correct cursor shape returned for Vi nav/insert modes |
