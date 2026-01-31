# Tasks: Mouse Bindings

**Input**: Design documents from `/specs/036-mouse-bindings/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/mouse-bindings.md, quickstart.md

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Expose the Renderer's private cursor position field needed by the Windows mouse handler, and verify all dependency types exist.

- [ ] T001 Add `internal Point CursorPos => _cursorPos;` property to `Renderer` class in `src/Stroke/Rendering/Renderer.cs` (one-line addition per contracts/mouse-bindings.md §Renderer Internal Property)
- [ ] T002 Verify all dependency types compile: `MouseEvent`, `MouseButton`, `MouseEventType`, `MouseModifiers` (Stroke.Input), `MouseHandlers` (Stroke.Layout), `HeightIsUnknownException` (Stroke.Rendering), `KeyHandlerCallable` (Stroke.KeyBinding), `NotImplementedOrNone` (Stroke.KeyBinding), `KeyPressEventExtensions.GetApp()` (Stroke.KeyBinding.Bindings) — run `dotnet build src/Stroke/Stroke.csproj` to confirm

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the `MouseBindings.cs` file with modifier constants, button/event-type aliases, and the three lookup tables. These are shared data structures used by all user stories.

**⚠️ CRITICAL**: No user story handler work can begin until the lookup tables are populated and verified.

- [ ] T003 Create `src/Stroke/KeyBinding/Bindings/MouseBindings.cs` with the static class skeleton: namespace `Stroke.KeyBinding.Bindings`, XML doc comments per contract, required `using` directives (`System.Collections.Frozen`, `System.Runtime.InteropServices`, `Stroke.Input`, `Stroke.Core.Primitives`, `Stroke.KeyBinding.Bindings`, `Stroke.Rendering`, `Stroke.Layout`)
- [ ] T004 Add 9 modifier constants to `MouseBindings.cs`: `NoModifier`, `Shift`, `Alt`, `ShiftAlt`, `Control`, `ShiftControl`, `AltControl`, `ShiftAltControl`, `UnknownModifier` — all `private const MouseModifiers` per contracts/mouse-bindings.md §Modifier Constants
- [ ] T005 Add 5 button convenience aliases to `MouseBindings.cs`: `Left = MouseButton.Left`, `Middle = MouseButton.Middle`, `Right = MouseButton.Right`, `NoButton = MouseButton.None`, `UnknownButton = MouseButton.Unknown` — matching Python source lines 44-48
- [ ] T006 Add 5 event type convenience aliases to `MouseBindings.cs`: `ScrollUp = MouseEventType.ScrollUp`, `ScrollDown = MouseEventType.ScrollDown`, `MouseDown = MouseEventType.MouseDown`, `MouseMove = MouseEventType.MouseMove`, `MouseUp = MouseEventType.MouseUp` — matching Python source lines 28-32
- [ ] T007 Populate the `XtermSgrMouseEvents` static readonly `FrozenDictionary<(int Code, char Suffix), (MouseButton Button, MouseEventType EventType, MouseModifiers Modifiers)>` with all 108 entries in `MouseBindings.cs` — faithfully porting `xterm_sgr_mouse_events` dict (Python lines 50-158): 24 mouse-up ('m' suffix), 24 mouse-down ('M'), 32 drag/move ('M'), 16 scroll ('M'). Each entry maps `(code, suffix)` → `(button, eventType, modifiers)`.
- [ ] T008 Populate the `TypicalMouseEvents` static readonly `FrozenDictionary<int, (MouseButton Button, MouseEventType EventType, MouseModifiers Modifiers)>` with all 10 entries in `MouseBindings.cs` — faithfully porting `typical_mouse_events` dict (Python lines 160-173): codes 32-35 (down/up), 64-67 (drag/move), 96-97 (scroll). All use `UnknownModifier`.
- [ ] T009 Populate the `UrxvtMouseEvents` static readonly `FrozenDictionary<int, (MouseButton Button, MouseEventType EventType, MouseModifiers Modifiers)>` with all 4 entries in `MouseBindings.cs` — faithfully porting `urxvt_mouse_events` dict (Python lines 175-180): codes 32, 35, 96, 97. All use `UnknownModifier`.
- [ ] T010 Add `LoadMouseBindings()` public static method and all 4 handler method signatures to `MouseBindings.cs`: create `HandleVt100MouseEvent`, `HandleScrollUp`, `HandleScrollDown`, `HandleWindowsMouseEvent` as `private static` methods with correct signatures per contracts/mouse-bindings.md §Handler Signatures (bodies return `NotImplementedOrNone.NotImplemented` or `null` until implemented in later phases). `LoadMouseBindings()` registers all 4 bindings using the quickstart.md §Binding Registration pattern and returns the `KeyBindings` instance.
- [ ] T011 Run `dotnet build src/Stroke/Stroke.csproj` to verify the foundational code compiles

**Checkpoint**: Lookup tables and class skeleton are complete. Handler implementations can now proceed.

---

## Phase 3: User Story 1 — VT100 Mouse Click Handling (Priority: P1) 🎯 MVP

**Goal**: Parse XTerm SGR mouse click sequences (button up/down events), transform coordinates, check renderer height, and dispatch to mouse handler registry.

**Independent Test**: Feed raw XTerm SGR escape sequences into the binding system and verify correct (button, position, modifiers) dispatch.

### Tests for User Story 1

- [ ] T012 [P] [US1] Create `tests/Stroke.Tests/KeyBinding/Bindings/MouseBindingsLookupTableTests.cs` with tests validating XTerm SGR table: total entry count (108), representative entries per SC-001 (code 0/'M' → Left/MouseDown/None, code 2/'m' → Right/MouseUp/None, code 36/'M' → Left/MouseMove/Shift, code 64/'M' → None/ScrollUp/None), all 24 mouse-up entries use 'm' suffix, all 84 non-up entries use 'M' suffix
- [ ] T013 [P] [US1] Create `tests/Stroke.Tests/KeyBinding/Bindings/MouseBindingsTests.cs` with tests for: `LoadMouseBindings()` returns exactly 4 bindings (SC-008), XTerm SGR coordinate transform (10,5) → (9,4) per SC-004, unknown SGR event code returns NotImplemented (SC-006a), handler returns NotImplemented when renderer `HeightIsKnown` is false (SC-006b), handler catches `HeightIsUnknownException` from `RowsAboveLayout` and returns NotImplemented (SC-006c)

### Implementation for User Story 1

- [ ] T014 [US1] Implement `HandleVt100MouseEvent` private static method in `src/Stroke/KeyBinding/Bindings/MouseBindings.cs` — XTerm SGR path only (the `else` branch where `data.StartsWith('<')`): parse `ESC[<code;x;yM/m` format, look up `(code, suffix)` in `XtermSgrMouseEvents` using `TryGetValue` and return NotImplemented on miss (FR-015) — this is the idiomatic C# equivalent of Python's `try/except KeyError` pattern, subtract 1 from x and y (FR-008), check `HeightIsKnown` guard (FR-013), subtract `RowsAboveLayout` with `HeightIsUnknownException` catch (FR-014), dispatch via `MouseHandlers.GetHandler(x, y)` (FR-012). Reference Python lines 218-284.
- [ ] T015 [US1] Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~MouseBindings"` to verify US1 tests pass

**Checkpoint**: XTerm SGR mouse click handling works end-to-end with coordinate transforms and handler dispatch.

---

## Phase 4: User Story 2 — Mouse Drag and Scroll Events (Priority: P2)

**Goal**: XTerm SGR drag (mouse-move) and scroll events are already covered by the US1 lookup table and handler — they flow through the same `HandleVt100MouseEvent` path. This phase adds test coverage specific to drag and scroll event types.

**Independent Test**: Feed drag escape sequences (code 32+) and scroll sequences (code 64+) and verify correct event type, button, and modifier dispatch.

### Tests for User Story 2

- [ ] T016 [P] [US2] Add drag and scroll test cases to `tests/Stroke.Tests/KeyBinding/Bindings/MouseBindingsLookupTableTests.cs`: verify drag entries (codes 32-63) map to MouseMove, scroll entries (codes 64-93) map to ScrollUp/ScrollDown, modifier combinations on drag/scroll entries (e.g., code 36/'M' → Shift, code 81/'M' → Control)
- [ ] T017 [P] [US2] Add XTerm SGR drag coordinate transform test to `tests/Stroke.Tests/KeyBinding/Bindings/MouseBindingsTests.cs`: `ESC[<32;15;8M` → left-button MouseMove at (14, 7), `ESC[<64;10;5M` → ScrollUp at (9, 4)

**Checkpoint**: All XTerm SGR event types (click, drag, scroll) verified with correct button, modifier, and coordinate handling.

---

## Phase 5: User Story 3 — Legacy Mouse Protocol Support (Priority: P3)

**Goal**: Add Typical (X10) and URXVT protocol parsing paths to the VT100 handler, including surrogate escape handling for Typical format.

**Independent Test**: Feed Typical-format (`ESC[M` + 3 bytes) and URXVT-format (`ESC[code;x;yM` without `<`) sequences and verify correct parsing and dispatch.

### Tests for User Story 3

- [ ] T018 [P] [US3] Add Typical and URXVT lookup table tests to `tests/Stroke.Tests/KeyBinding/Bindings/MouseBindingsLookupTableTests.cs`: Typical table has 10 entries (SC-002) with correct values per data-model table, URXVT table has 4 entries (SC-003) with correct values per data-model table
- [ ] T019 [P] [US3] Add Typical and URXVT handler tests to `tests/Stroke.Tests/KeyBinding/Bindings/MouseBindingsTests.cs`: Typical coordinate transform bytes (42, 37) → (9, 4) per SC-004, Typical surrogate escape (0xDC00+42, 0xDC00+37) → (9, 4) per SC-004, URXVT coordinate transform (14, 13) → (13, 12) per SC-004, URXVT unknown code fallback to Unknown/MouseMove (FR-016)

### Implementation for User Story 3

- [ ] T020 [US3] Implement the Typical (X10) parsing path in `HandleVt100MouseEvent` in `src/Stroke/KeyBinding/Bindings/MouseBindings.cs` — the `if (@event.Data[2] == 'M')` branch: extract 3 bytes as char ordinals (FR-002), look up in `TypicalMouseEvents` (unguarded per Python reference and edge case documentation), handle surrogate escapes >= 0xDC00 (FR-010), subtract 32 then subtract 1 from x and y (FR-009). Reference Python lines 201-217.
- [ ] T021 [US3] Implement the URXVT parsing path in `HandleVt100MouseEvent` in `src/Stroke/KeyBinding/Bindings/MouseBindings.cs` — the `else` branch where `!sgr`: parse `code;x;yM` format, look up in `UrxvtMouseEvents` using `TryGetValue` with manual fallback to `(UnknownButton, MouseMove, UnknownModifier)` on miss (FR-016, FR-003), subtract 1 from x and y (FR-008a). Reference Python lines 244-252.
- [ ] T022 [US3] Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~MouseBindings"` to verify US3 tests pass

**Checkpoint**: All three VT100 mouse protocol formats (XTerm SGR, Typical, URXVT) parse and dispatch correctly.

---

## Phase 6: User Story 4 — Scroll Events Without Position (Priority: P3)

**Goal**: ScrollUp and ScrollDown key events (without cursor position data) are converted to Up and Down arrow key presses fed into the key processor.

**Independent Test**: Feed ScrollUp/ScrollDown key events and verify Up/Down arrow key presses are injected into the key processor with `first: true`.

### Tests for User Story 4

- [ ] T023 [P] [US4] Add scroll handler tests to `tests/Stroke.Tests/KeyBinding/Bindings/MouseBindingsTests.cs`: verify ScrollUp binding exists for `Keys.ScrollUp`, verify ScrollDown binding exists for `Keys.ScrollDown`, verify `LoadMouseBindings()` registers both scroll key bindings (SC-005)

### Implementation for User Story 4

- [ ] T024 [US4] Implement `HandleScrollUp` and `HandleScrollDown` private static methods in `src/Stroke/KeyBinding/Bindings/MouseBindings.cs` — each feeds a `new KeyPress(Keys.Up)` / `new KeyPress(Keys.Down)` into `@event.GetApp().KeyProcessor.Feed(..., first: true)` per FR-017/FR-018 and quickstart.md §Scroll Handler Pattern. Reference Python lines 286-300.
- [ ] T025 [US4] Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~MouseBindings"` to verify US4 tests pass

**Checkpoint**: Scroll-without-position events correctly convert to arrow key presses.

---

## Phase 7: User Story 5 — Windows Mouse Event Handling (Priority: P3)

**Goal**: Parse Windows mouse event format, adjust coordinates using Win32 screen buffer info, and dispatch. Returns NotImplemented on non-Windows platforms or when no Win32-compatible output is available.

**Independent Test**: Feed Windows-format mouse event strings and verify parsing, coordinate adjustment, and NotImplemented return on non-Windows platforms.

### Tests for User Story 5

- [ ] T026 [P] [US5] Add Windows handler tests to `tests/Stroke.Tests/KeyBinding/Bindings/MouseBindingsTests.cs`: verify WindowsMouseEvent binding exists for `Keys.WindowsMouseEvent`, verify handler returns NotImplemented on non-Windows platform (SC-006d), verify `LoadMouseBindings()` includes the Windows binding (SC-008)

### Implementation for User Story 5

- [ ] T027 [US5] Implement `HandleWindowsMouseEvent` private static method in `src/Stroke/KeyBinding/Bindings/MouseBindings.cs` — outer guard `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)` returns NotImplemented if false (FR-021), parse `button;eventType;x;y` from `@event.Data.Split(';')` (FR-019), check output type for Win32-compatible (return NotImplemented if not — FR-019), compute `rowsAboveCursor` using `Renderer.CursorPos` (FR-020), dispatch via `MouseHandlers.GetHandler(x, y)`. Reference Python lines 302-346.
- [ ] T028 [US5] Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~MouseBindings"` to verify US5 tests pass

**Checkpoint**: Windows mouse events parse and dispatch correctly; non-Windows returns NotImplemented.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, build, and coverage.

- [ ] T029 [P] Verify file size of `src/Stroke/KeyBinding/Bindings/MouseBindings.cs` does not exceed 1,000 LOC (Constitution X) — split if needed
- [ ] T030 [P] Verify file sizes of `tests/Stroke.Tests/KeyBinding/Bindings/MouseBindingsLookupTableTests.cs` and `tests/Stroke.Tests/KeyBinding/Bindings/MouseBindingsTests.cs` do not exceed 1,000 LOC each
- [ ] T031 Run full test suite `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj` to verify no regressions and measure coverage with `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --collect:"XPlat Code Coverage"` — verify MouseBindings module meets SC-007 (≥80% coverage)
- [ ] T032 Run `dotnet build src/Stroke/Stroke.csproj` in Release configuration to confirm clean build
- [ ] T033 Run quickstart.md build verification commands: `dotnet build src/Stroke/Stroke.csproj && dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~MouseBindings"`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup (T001, T002) — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational (T003-T011) — MVP delivery point
- **US2 (Phase 4)**: Depends on US1 (Phase 3) — extends XTerm SGR test coverage
- **US3 (Phase 5)**: Depends on Foundational (T003-T011) — independent of US1/US2 handler, but shares `HandleVt100MouseEvent` method
- **US4 (Phase 6)**: Depends on Foundational (T003-T011) — independent of US1/US2/US3
- **US5 (Phase 7)**: Depends on Setup (T001) and Foundational (T003-T011) — independent of VT100 stories
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — no dependencies on other stories
- **US2 (P2)**: Depends on US1 (extends the same handler method)
- **US3 (P3)**: Can start after Phase 2 — adds to the same handler but different code paths
- **US4 (P3)**: Can start after Phase 2 — completely independent handler methods
- **US5 (P3)**: Can start after Phase 2 — completely independent handler method

### Within Each User Story

- Tests written first (marked [P] where independent)
- Implementation follows tests
- Build/test verification at end of each story

### Parallel Opportunities

- T004, T005, T006 can run in parallel (independent constants/aliases sections of same file — but since they're in the same file, they should be sequential)
- T007, T008, T009 can conceptually run in parallel (independent lookup tables — but same file, so sequential)
- T012, T013 can run in parallel (different test files)
- T016, T017 can run in parallel (different test files)
- T018, T019 can run in parallel (different test files)
- US3, US4, US5 can all start after Phase 2 if working in parallel (different handler methods)
- T029, T030 can run in parallel (checking different files)

---

## Parallel Example: User Story 1

```
# Launch US1 test files in parallel:
Task: T012 "XTerm SGR lookup table tests in tests/Stroke.Tests/KeyBinding/Bindings/MouseBindingsLookupTableTests.cs"
Task: T013 "XTerm SGR handler tests in tests/Stroke.Tests/KeyBinding/Bindings/MouseBindingsTests.cs"

# Then implement handler (sequential):
Task: T014 "Implement HandleVt100MouseEvent XTerm SGR path"
Task: T015 "Verify US1 tests pass"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 2: Foundational (T003-T011)
3. Complete Phase 3: User Story 1 (T012-T015)
4. **STOP and VALIDATE**: XTerm SGR mouse clicks work end-to-end
5. This covers the most common mouse protocol format used by modern terminals

### Incremental Delivery

1. Setup + Foundational → Lookup tables and class ready
2. Add US1 → XTerm SGR click handling (MVP!)
3. Add US2 → Drag and scroll test coverage
4. Add US3 → Legacy protocol support (Typical + URXVT)
5. Add US4 → Scroll-without-position fallback
6. Add US5 → Windows platform support
7. Each story adds terminal compatibility without breaking previous stories

---

## Notes

- All 3 lookup tables are in the same file (`MouseBindings.cs`) — foundational tasks (T004-T009) are sequential despite being logically independent
- The VT100 handler (`HandleVt100MouseEvent`) is shared across US1, US2, and US3 — US2 adds only tests (no new implementation), US3 adds Typical/URXVT code paths to the same method
- Win32Output does not yet exist (Feature 21/57) — the Windows handler (US5) is structured to return NotImplemented when no Win32-compatible output type is available (R4)
- Python reference: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/mouse.py`
- Total: 33 tasks across 8 phases
