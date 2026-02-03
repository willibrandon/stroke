# Tasks: Win32 Console Types

**Input**: Design documents from `/specs/051-win32-console-types/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/ ✓

**Tests**: Included per spec.md Success Criteria (SC-001 through SC-008)

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Paths relative to repository root

## Path Conventions

```text
src/Stroke/Input/Windows/
├── ConsoleApi.cs           # [EXISTING] P/Invoke wrapper - extend
├── Win32Types/             # [NEW] Namespace for Win32 struct types
│   ├── Coord.cs
│   ├── SmallRect.cs
│   ├── ConsoleScreenBufferInfo.cs
│   ├── KeyEventRecord.cs
│   ├── MouseEventRecord.cs
│   ├── WindowBufferSizeRecord.cs
│   ├── MenuEventRecord.cs
│   ├── FocusEventRecord.cs
│   ├── InputRecord.cs
│   ├── CharInfo.cs
│   ├── SecurityAttributes.cs
│   ├── EventType.cs
│   ├── ControlKeyState.cs
│   ├── MouseEventFlags.cs
│   ├── MouseButtonState.cs
│   ├── ConsoleInputMode.cs
│   └── ConsoleOutputMode.cs
└── StdHandles.cs           # [NEW] Standard handle constants

tests/Stroke.Tests/Input/Windows/Win32Types/
├── CoordTests.cs
├── SmallRectTests.cs
├── ConsoleScreenBufferInfoTests.cs
├── KeyEventRecordTests.cs
├── MouseEventRecordTests.cs
├── WindowBufferSizeRecordTests.cs
├── MenuEventRecordTests.cs
├── FocusEventRecordTests.cs
├── InputRecordTests.cs
├── CharInfoTests.cs
├── SecurityAttributesTests.cs
├── EnumTests.cs            # Tests for all enum types
├── StdHandlesTests.cs
└── NativeMethodsTests.cs   # P/Invoke tests (Windows-only)
```

---

## Phase 1: Setup

**Purpose**: Create directory structure and foundational files

- [X] T001 Create Win32Types directory at src/Stroke/Input/Windows/Win32Types/
- [X] T002 Create Win32Types test directory at tests/Stroke.Tests/Input/Windows/Win32Types/

---

## Phase 2: User Story 1 - Define Console Data Structures (Priority: P1) 🎯 MVP

**Goal**: Implement all 11 C# struct types that match native Windows structures byte-for-byte for correct P/Invoke marshalling.

**Independent Test**: Create struct instances, verify sizes via `Marshal.SizeOf<T>()`, confirm field offsets for union struct. No Windows runtime required.

### Core Building Block Structs (no dependencies)

- [X] T003 [P] [US1] Implement Coord struct in src/Stroke/Input/Windows/Win32Types/Coord.cs (4 bytes: X, Y as short; constructor; IEquatable<Coord>)
- [X] T004 [P] [US1] Implement SmallRect struct in src/Stroke/Input/Windows/Win32Types/SmallRect.cs (8 bytes: Left, Top, Right, Bottom as short; Width/Height properties; IEquatable<SmallRect>)
- [X] T005 [P] [US1] Implement CharInfo struct in src/Stroke/Input/Windows/Win32Types/CharInfo.cs (4 bytes: UnicodeChar, Attributes; constructor; IEquatable<CharInfo>)
- [X] T006 [P] [US1] Implement SecurityAttributes struct in src/Stroke/Input/Windows/Win32Types/SecurityAttributes.cs (12/24 bytes: Length, SecurityDescriptor, InheritHandle; Create() factory)

### Tests for Core Building Block Structs

- [X] T007 [P] [US1] Create tests for Coord in tests/Stroke.Tests/Input/Windows/Win32Types/CoordTests.cs (construction, equality, size = 4 bytes)
- [X] T008 [P] [US1] Create tests for SmallRect in tests/Stroke.Tests/Input/Windows/Win32Types/SmallRectTests.cs (construction, Width/Height, equality, size = 8 bytes)
- [X] T009 [P] [US1] Create tests for CharInfo in tests/Stroke.Tests/Input/Windows/Win32Types/CharInfoTests.cs (construction, equality, size = 4 bytes)
- [X] T010 [P] [US1] Create tests for SecurityAttributes in tests/Stroke.Tests/Input/Windows/Win32Types/SecurityAttributesTests.cs (Create() factory, size = 12 or 24 bytes per platform)

### Event Record Structs (depend on Coord)

- [X] T011 [P] [US1] Implement WindowBufferSizeRecord struct in src/Stroke/Input/Windows/Win32Types/WindowBufferSizeRecord.cs (4 bytes: Size as Coord)
- [X] T012 [P] [US1] Implement MenuEventRecord struct in src/Stroke/Input/Windows/Win32Types/MenuEventRecord.cs (4 bytes: CommandId as uint)
- [X] T013 [P] [US1] Implement FocusEventRecord struct in src/Stroke/Input/Windows/Win32Types/FocusEventRecord.cs (4 bytes: SetFocus as int; HasFocus property)

### Tests for Simple Event Records

- [X] T014 [P] [US1] Create tests for WindowBufferSizeRecord in tests/Stroke.Tests/Input/Windows/Win32Types/WindowBufferSizeRecordTests.cs (size = 4 bytes)
- [X] T015 [P] [US1] Create tests for MenuEventRecord in tests/Stroke.Tests/Input/Windows/Win32Types/MenuEventRecordTests.cs (size = 4 bytes)
- [X] T016 [P] [US1] Create tests for FocusEventRecord in tests/Stroke.Tests/Input/Windows/Win32Types/FocusEventRecordTests.cs (size = 4 bytes, HasFocus property)

### Complex Event Record Structs (depend on enums from US2 - but can define with uint placeholders initially)

- [X] T017 [US1] Implement KeyEventRecord struct in src/Stroke/Input/Windows/Win32Types/KeyEventRecord.cs (16 bytes: KeyDown, RepeatCount, VirtualKeyCode, VirtualScanCode, UnicodeChar, ControlKeyState; IsKeyDown property)
- [X] T018 [US1] Implement MouseEventRecord struct in src/Stroke/Input/Windows/Win32Types/MouseEventRecord.cs (16 bytes: MousePosition as Coord, ButtonState, ControlKeyState, EventFlags)

### Tests for Complex Event Records

- [X] T019 [P] [US1] Create tests for KeyEventRecord in tests/Stroke.Tests/Input/Windows/Win32Types/KeyEventRecordTests.cs (size = 16 bytes, IsKeyDown property)
- [X] T020 [P] [US1] Create tests for MouseEventRecord in tests/Stroke.Tests/Input/Windows/Win32Types/MouseEventRecordTests.cs (size = 16 bytes)

### Union Struct (depends on all event records)

- [X] T021 [US1] Implement InputRecord union struct in src/Stroke/Input/Windows/Win32Types/InputRecord.cs (20 bytes: LayoutKind.Explicit; EventType at offset 0; all event records at offset 4)
- [X] T022 [US1] Create tests for InputRecord in tests/Stroke.Tests/Input/Windows/Win32Types/InputRecordTests.cs (size = 20 bytes, field offsets via Marshal.OffsetOf)

### Composite Struct (depends on Coord, SmallRect)

- [X] T023 [US1] Implement ConsoleScreenBufferInfo struct in src/Stroke/Input/Windows/Win32Types/ConsoleScreenBufferInfo.cs (22 bytes: Size, CursorPosition as Coord; Attributes as ushort; Window as SmallRect; MaximumWindowSize as Coord)
- [X] T024 [US1] Create tests for ConsoleScreenBufferInfo in tests/Stroke.Tests/Input/Windows/Win32Types/ConsoleScreenBufferInfoTests.cs (size = 22 bytes)

**Checkpoint**: All 11 struct types implemented and verified. US1 acceptance scenarios satisfied.

---

## Phase 3: User Story 2 - Flags Enums for Console State (Priority: P2)

**Goal**: Define all flags enums with correct hex values for interpreting console input state.

**Independent Test**: Combine flag values with bitwise OR, verify individual flags with bitwise AND, verify exact hex values match Windows API.

### Enum Implementations

- [X] T025 [P] [US2] Implement EventType enum in src/Stroke/Input/Windows/Win32Types/EventType.cs (ushort: KeyEvent=0x0001, MouseEvent=0x0002, WindowBufferSizeEvent=0x0004, MenuEvent=0x0008, FocusEvent=0x0010)
- [X] T026 [P] [US2] Implement ControlKeyState flags enum in src/Stroke/Input/Windows/Win32Types/ControlKeyState.cs (uint with [Flags]: 10 values from None=0x0000 to EnhancedKey=0x0100)
- [X] T027 [P] [US2] Implement MouseEventFlags flags enum in src/Stroke/Input/Windows/Win32Types/MouseEventFlags.cs (uint with [Flags]: 5 values from None=0x0000 to MouseHWheeled=0x0008)
- [X] T028 [P] [US2] Implement MouseButtonState flags enum in src/Stroke/Input/Windows/Win32Types/MouseButtonState.cs (uint with [Flags]: 6 values from None=0x0000 to FromLeft4thButtonPressed=0x0010)
- [X] T029 [P] [US2] Implement ConsoleInputMode flags enum in src/Stroke/Input/Windows/Win32Types/ConsoleInputMode.cs (uint with [Flags]: 10 values from None=0x0000 to EnableVirtualTerminalInput=0x0200)
- [X] T030 [P] [US2] Implement ConsoleOutputMode flags enum in src/Stroke/Input/Windows/Win32Types/ConsoleOutputMode.cs (uint with [Flags]: 6 values from None=0x0000 to EnableLvbGridWorldwide=0x0010)

### Enum Tests

- [X] T031 [US2] Create comprehensive enum tests in tests/Stroke.Tests/Input/Windows/Win32Types/EnumTests.cs (verify all hex values, [Flags] behavior, bitwise operations: LeftCtrlPressed | ShiftPressed = 0x0018, EnableProcessedInput | EnableMouseInput = 0x0011)

**Checkpoint**: All 6 enum types implemented with verified hex values. US2 acceptance scenarios satisfied.

---

## Phase 4: User Story 4 - Standard Handle Constants (Priority: P2)

**Goal**: Provide named constants for standard console handles.

**Independent Test**: Verify constant values match Windows API definitions (-10, -11, -12).

- [X] T032 [P] [US4] Implement StdHandles static class in src/Stroke/Input/Windows/StdHandles.cs (STD_INPUT_HANDLE=-10, STD_OUTPUT_HANDLE=-11, STD_ERROR_HANDLE=-12)
- [X] T033 [P] [US4] Create tests for StdHandles in tests/Stroke.Tests/Input/Windows/Win32Types/StdHandlesTests.cs (verify constant values)

**Checkpoint**: US4 complete. Handle constants available for P/Invoke calls.

---

## Phase 5: User Story 3 - P/Invoke Native Functions (Priority: P3)

**Goal**: Extend ConsoleApi with P/Invoke declarations for console operations using the new struct types.

**Independent Test (Windows only)**: Call GetStdHandle(STD_OUTPUT_HANDLE) and verify non-null handle returned. GetConsoleScreenBufferInfo populates struct fields.

### P/Invoke Implementations

- [X] T034 [US3] Add GetConsoleScreenBufferInfo P/Invoke to src/Stroke/Input/Windows/ConsoleApi.cs (LibraryImport, SetLastError=true, out ConsoleScreenBufferInfo)
- [X] T035 [US3] Add ReadConsoleInput P/Invoke to src/Stroke/Input/Windows/ConsoleApi.cs (LibraryImport, EntryPoint="ReadConsoleInputW", [Out] InputRecord[], out count)
- [X] T036 [US3] Add WriteConsoleOutput P/Invoke to src/Stroke/Input/Windows/ConsoleApi.cs (LibraryImport, EntryPoint="WriteConsoleOutputW", [In] CharInfo[], ref SmallRect)
- [X] T037 [US3] Add SetConsoleCursorPosition P/Invoke to src/Stroke/Input/Windows/ConsoleApi.cs (LibraryImport, Coord parameter)

### P/Invoke Tests (Windows-only, conditional)

- [X] T038 [US3] Create P/Invoke tests in tests/Stroke.Tests/Input/Windows/Win32Types/NativeMethodsTests.cs (Windows-only: GetStdHandle returns valid handle, GetConsoleScreenBufferInfo populates fields, CA1416 analyzer warning on non-Windows)
- [X] T038a [US3] Verify existing ConsoleApi methods still function after extension (GetStdHandle, GetConsoleMode, SetConsoleMode, CreateEvent, SetEvent, ResetEvent, CloseHandle, WaitForMultipleObjects)

**Checkpoint**: All P/Invoke methods implemented. US3 acceptance scenarios satisfied on Windows. Existing ConsoleApi functionality verified.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, documentation, and cleanup

- [X] T039 Run all struct size verification tests to confirm SC-001
- [X] T040 Verify InputRecord field offsets match SC-002 (Marshal.OffsetOf verification)
- [X] T041 Verify enum values match Microsoft documentation per SC-003
- [X] T042 Verify P/Invoke calls work on Windows per SC-004
- [X] T043 Verify types compile on non-Windows platforms per SC-005
- [X] T044 Verify all Python win32_types.py types are ported per SC-006
- [X] T045 [P] Ensure XML documentation on all public types per NFR-007
- [X] T046 Run quickstart.md code examples as validation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies - start immediately
- **Phase 2 (US1)**: Depends on Phase 1 - CRITICAL for all other phases
- **Phase 3 (US2)**: Can start after T003 (Coord), but enum values are independent
- **Phase 4 (US4)**: Can start after Phase 1 - independent of structs
- **Phase 5 (US3)**: Depends on US1 (structs), US2 (enums), US4 (handles)
- **Phase 6 (Polish)**: Depends on all user stories complete

### User Story Dependencies

```
US1 (Structs) ──┬──► US3 (P/Invoke)
                │
US2 (Enums) ────┤
                │
US4 (Handles) ──┘
```

- **US1 (P1)**: Start after Phase 1 - No dependencies on other stories
- **US2 (P2)**: Can run in parallel with US1 - Enums have no struct dependencies
- **US4 (P2)**: Can run in parallel with US1, US2 - Simple constants
- **US3 (P3)**: Depends on US1, US2, US4 - P/Invoke needs all types defined

### Within User Story 1 (Internal Dependencies)

```
Coord, SmallRect, CharInfo, SecurityAttributes (T003-T006) ── parallel
         │
         ▼
WindowBufferSizeRecord, MenuEventRecord, FocusEventRecord (T011-T013) ── parallel
         │
         ▼
KeyEventRecord, MouseEventRecord (T017-T018) ── after enums ideally, but can use uint
         │
         ▼
InputRecord (T021) ── needs all event records
ConsoleScreenBufferInfo (T023) ── needs Coord, SmallRect
```

### Parallel Opportunities

**Phase 1**: T001, T002 can run in parallel

**Phase 2 (US1)**:
- T003-T006 can all run in parallel (core building blocks)
- T007-T010 can all run in parallel (tests for building blocks)
- T011-T013 can run in parallel (simple event records)
- T014-T016 can run in parallel (tests for simple events)
- T019-T020 can run in parallel (tests for complex events)

**Phase 3 (US2)**: T025-T030 can all run in parallel (independent enums)

**Phase 4 (US4)**: T032-T033 can run in parallel

**Phase 5 (US3)**: T034-T037 can run in parallel (different P/Invoke methods)

---

## Parallel Example: Core Building Blocks

```bash
# Launch all core building block struct implementations together:
Task: "Implement Coord struct in src/Stroke/Input/Windows/Win32Types/Coord.cs"
Task: "Implement SmallRect struct in src/Stroke/Input/Windows/Win32Types/SmallRect.cs"
Task: "Implement CharInfo struct in src/Stroke/Input/Windows/Win32Types/CharInfo.cs"
Task: "Implement SecurityAttributes struct in src/Stroke/Input/Windows/Win32Types/SecurityAttributes.cs"

# Launch all enum implementations together:
Task: "Implement EventType enum in src/Stroke/Input/Windows/Win32Types/EventType.cs"
Task: "Implement ControlKeyState flags enum in src/Stroke/Input/Windows/Win32Types/ControlKeyState.cs"
Task: "Implement MouseEventFlags flags enum in src/Stroke/Input/Windows/Win32Types/MouseEventFlags.cs"
Task: "Implement MouseButtonState flags enum in src/Stroke/Input/Windows/Win32Types/MouseButtonState.cs"
Task: "Implement ConsoleInputMode flags enum in src/Stroke/Input/Windows/Win32Types/ConsoleInputMode.cs"
Task: "Implement ConsoleOutputMode flags enum in src/Stroke/Input/Windows/Win32Types/ConsoleOutputMode.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 2: US1 - Structs (T003-T024)
3. **STOP and VALIDATE**: Run all size verification tests
4. Struct types are fully usable for manual testing

### Incremental Delivery

1. Setup → Directory structure ready
2. Add US1 (Structs) → Test sizes → Core type layer complete (MVP!)
3. Add US2 (Enums) → Test values → Semantic layer for state interpretation
4. Add US4 (Handles) → Test values → Constants for P/Invoke
5. Add US3 (P/Invoke) → Test on Windows → Full native interop capability
6. Polish → Verify all success criteria

### Parallel Team Strategy

With multiple developers:

1. All complete Phase 1 together
2. Once directories exist:
   - Developer A: US1 Core Building Blocks (T003-T010)
   - Developer B: US2 Enums (T025-T031)
   - Developer C: US4 Handles (T032-T033)
3. Developer A continues: US1 Event Records (T011-T024)
4. All converge: US3 P/Invoke (requires US1, US2, US4)
5. All: Phase 6 Polish

---

## Notes

- All struct sizes are verified against Microsoft documentation
- `[StructLayout(LayoutKind.Sequential)]` for most structs
- `[StructLayout(LayoutKind.Explicit, Size = 20)]` for InputRecord union
- All flags enums use `[Flags]` attribute
- P/Invoke uses `[LibraryImport]` not `[DllImport]` per research.md decision
- All P/Invoke methods marked `[SupportedOSPlatform("windows")]`
- Tests use `Marshal.SizeOf<T>()` for size verification
- Tests use `Marshal.OffsetOf<T>()` for union offset verification
- Windows-only tests conditionally skip on non-Windows platforms
