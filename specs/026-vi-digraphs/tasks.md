# Tasks: Vi Digraphs

**Input**: Design documents from `/specs/026-vi-digraphs/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/Digraphs.md ✓, quickstart.md ✓

**Tests**: Required per SC-004 (80% code coverage target)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

Per plan.md Project Structure:
- Source: `src/Stroke/KeyBinding/`
- Tests: `tests/Stroke.Tests/KeyBinding/`

---

## Phase 1: Setup

**Purpose**: Verify project structure exists and create source file skeleton

- [ ] T001 Verify KeyBinding namespace exists in `src/Stroke/KeyBinding/` directory
- [ ] T002 Create `Digraphs.cs` skeleton with class definition and XML documentation in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T003 Create `DigraphsTests.cs` test file skeleton in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`

---

## Phase 2: Foundational (Digraph Dictionary Data)

**Purpose**: Port the 1,300+ digraph mappings from Python Prompt Toolkit - this is the data foundation ALL user stories depend on

**⚠️ CRITICAL**: No user story can function until the dictionary data is populated

- [ ] T004 Read Python source file at `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/digraphs.py` and extract all DIGRAPHS dictionary entries
- [ ] T005 Create private static method `CreateDigraphDictionary()` that returns `FrozenDictionary<(char, char), int>` in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T006 Populate dictionary with control characters (0x00-0x1F, ~32 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T007 [P] Populate dictionary with ASCII printable characters (0x20-0x7F, ~30 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T008 [P] Populate dictionary with Latin-1 Supplement characters (0x80-0xFF, ~100 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T009 [P] Populate dictionary with Latin Extended-A characters (0x100-0x17F, ~100 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T010 [P] Populate dictionary with Latin Extended-B characters (0x180-0x24F, ~30 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T011 [P] Populate dictionary with Greek and Coptic characters (0x370-0x3FF, ~80 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T012 [P] Populate dictionary with Cyrillic characters (0x400-0x4FF, ~100 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T013 [P] Populate dictionary with Hebrew characters (0x5D0-0x5EA, ~30 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T014 [P] Populate dictionary with Arabic characters (0x600-0x6FF, ~60 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T015 [P] Populate dictionary with currency symbols (0x20A0-0x20BF, ~10 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T016 [P] Populate dictionary with mathematical operators (0x2200-0x22FF, ~50 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T017 [P] Populate dictionary with box drawing characters (0x2500-0x257F, ~60 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T018 [P] Populate dictionary with Hiragana characters (0x3040-0x309F, ~90 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T019 [P] Populate dictionary with Katakana characters (0x30A0-0x30FF, ~90 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T020 [P] Populate dictionary with Bopomofo characters (0x3100-0x312F, ~40 entries) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T021 [P] Populate dictionary with Latin ligatures and remaining characters (0xFB00-0xFB06 and miscellaneous) in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T022 Initialize static `_map` field with `CreateDigraphDictionary().ToFrozenDictionary()` in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T023 Add entry count verification test to confirm 1,300+ entries exist in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`

**Checkpoint**: Dictionary foundation ready - all 1,300+ mappings ported from Python source

---

## Phase 3: User Story 1 - Insert Special Character via Digraph (Priority: P1) 🎯 MVP

**Goal**: Enable lookup of Unicode code points from two-character digraph sequences

**Independent Test**: Call `Digraphs.Lookup('E', 'u')` and verify it returns 0x20AC (Euro sign)

### Tests for User Story 1

- [ ] T024 [P] [US1] Test Lookup returns Euro sign code point for ('E', 'u') in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T025 [P] [US1] Test Lookup returns Greek pi code point for ('p', '*') in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T026 [P] [US1] Test Lookup returns left arrow code point for ('<', '-') in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T027 [P] [US1] Test Lookup returns box drawing horizontal for ('h', 'h') in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T028 [P] [US1] Test Lookup is case-sensitive (('a', '*') vs ('A', '*') return different values) in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`

### Implementation for User Story 1

- [ ] T029 [US1] Implement `Lookup(char char1, char char2)` method returning `int?` in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T030 [US1] Add XML documentation for Lookup method with examples in `src/Stroke/KeyBinding/Digraphs.cs`

**Checkpoint**: User Story 1 complete - Lookup method works for valid digraphs

---

## Phase 4: User Story 2 - Handle Invalid Digraph Gracefully (Priority: P2)

**Goal**: Return null for non-existent digraphs without throwing exceptions

**Independent Test**: Call `Digraphs.Lookup('Z', 'Z')` and verify it returns null

### Tests for User Story 2

- [ ] T031 [P] [US2] Test Lookup returns null for ('Z', 'Z') in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T032 [P] [US2] Test Lookup returns null for ('!', '@') in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T033 [P] [US2] Test Lookup returns null for reversed order ('u', 'E') in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T034 [P] [US2] Test Lookup does not throw for any invalid input in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`

### Implementation for User Story 2

- [ ] T035 [US2] Verify Lookup implementation uses TryGetValue for null-returning behavior (no changes if already implemented correctly) in `src/Stroke/KeyBinding/Digraphs.cs`

**Checkpoint**: User Story 2 complete - Invalid digraphs return null, no exceptions

---

## Phase 5: User Story 3 - Convert Code Point to Character String (Priority: P2)

**Goal**: Provide a convenience method to get the actual Unicode string, handling surrogate pairs correctly

**Independent Test**: Call `Digraphs.GetString('E', 'u')` and verify it returns "€"

### Tests for User Story 3

- [ ] T036 [P] [US3] Test GetString returns "€" for ('E', 'u') in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T037 [P] [US3] Test GetString returns "π" for ('p', '*') in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T038 [P] [US3] Test GetString returns null for invalid digraph ('Z', 'Z') in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T039 [P] [US3] Test GetString handles BMP characters correctly in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T040 [P] [US3] Test GetString uses char.ConvertFromUtf32 which handles surrogate pairs correctly (per research.md, Python source has no code points >0xFFFF, so verify implementation is ready for future additions) in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`

### Implementation for User Story 3

- [ ] T041 [US3] Implement `GetString(char char1, char char2)` method returning `string?` using `char.ConvertFromUtf32()` in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T042 [US3] Add XML documentation for GetString method with examples in `src/Stroke/KeyBinding/Digraphs.cs`

**Checkpoint**: User Story 3 complete - GetString method works for valid digraphs and returns null for invalid

---

## Phase 6: User Story 4 - Access Full Digraph Dictionary (Priority: P3)

**Goal**: Expose the complete digraph dictionary as a read-only collection for enumeration

**Independent Test**: Access `Digraphs.Map` and verify it contains 1,300+ entries including the ('E', 'u') → 0x20AC mapping

### Tests for User Story 4

- [ ] T043 [P] [US4] Test Map property returns non-null dictionary in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T044 [P] [US4] Test Map contains expected entry count (1,300+) in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T045 [P] [US4] Test Map contains Euro sign mapping (('E', 'u'), 0x20AC) in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T046 [P] [US4] Test Map is enumerable and can filter by code point range (e.g., Greek letters) in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T047 [P] [US4] Test Map returns same instance on multiple accesses (reference equality) in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`

### Implementation for User Story 4

- [ ] T048 [US4] Implement `Map` property returning `IReadOnlyDictionary<(char Char1, char Char2), int>` in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T049 [US4] Add XML documentation for Map property with enumeration example in `src/Stroke/KeyBinding/Digraphs.cs`

**Checkpoint**: User Story 4 complete - Map property exposes full dictionary for enumeration

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, documentation, and code quality

- [ ] T050 [P] Add category coverage tests verifying each Unicode block has expected approximate entry count in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T051 [P] Add thread safety test calling Lookup from multiple threads concurrently in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T052 [P] Add edge case tests for control characters (code points 0x00-0x1F) in `tests/Stroke.Tests/KeyBinding/DigraphsTests.cs`
- [ ] T053 Verify XML documentation is complete for all public members in `src/Stroke/KeyBinding/Digraphs.cs`
- [ ] T054 Run quickstart.md examples to verify they work correctly
- [ ] T055 Verify code coverage meets 80% target (SC-004)
- [ ] T056 Final review: verify file size is under 1,000 LOC or document exception per Constitution X

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories (no lookups work without data)
- **User Story 1 (Phase 3)**: Depends on Foundational - core Lookup method
- **User Story 2 (Phase 4)**: Depends on US1 - tests null behavior of Lookup
- **User Story 3 (Phase 5)**: Depends on US1 - GetString uses Lookup internally
- **User Story 4 (Phase 6)**: Depends on Foundational - Map exposes the dictionary
- **Polish (Phase 7)**: Depends on all user stories complete

### User Story Dependencies

```
Foundational (Phase 2) ──┬──► US1 (Phase 3) ──► US2 (Phase 4)
                         │                   ↘
                         │                    US3 (Phase 5)
                         │
                         └──► US4 (Phase 6)
```

- **User Story 1 (P1)**: Depends only on Foundational
- **User Story 2 (P2)**: Depends on US1 (tests Lookup null behavior)
- **User Story 3 (P2)**: Depends on US1 (GetString calls Lookup)
- **User Story 4 (P3)**: Depends only on Foundational (can parallel with US1)

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Implementation then makes tests pass
- Documentation added with implementation

### Parallel Opportunities

- T007-T021 (dictionary population by Unicode block) can all run in parallel
- All tests within a user story marked [P] can run in parallel
- US1 and US4 can run in parallel after Foundational phase (if team capacity)
- All Polish phase tasks marked [P] can run in parallel

---

## Parallel Example: Foundational Phase

```bash
# After T006 (control characters) is done, launch all other blocks in parallel:
T007: Populate ASCII printable characters
T008: Populate Latin-1 Supplement characters
T009: Populate Latin Extended-A characters
T010: Populate Latin Extended-B characters
T011: Populate Greek and Coptic characters
T012: Populate Cyrillic characters
T013: Populate Hebrew characters
T014: Populate Arabic characters
T015: Populate currency symbols
T016: Populate mathematical operators
T017: Populate box drawing characters
T018: Populate Hiragana characters
T019: Populate Katakana characters
T020: Populate Bopomofo characters
T021: Populate Latin ligatures and remaining
```

## Parallel Example: User Story 1 Tests

```bash
# Launch all US1 tests in parallel:
T024: Test Lookup returns Euro sign code point
T025: Test Lookup returns Greek pi code point
T026: Test Lookup returns left arrow code point
T027: Test Lookup returns box drawing horizontal
T028: Test Lookup is case-sensitive
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: Foundational (T004-T023) - port all 1,300+ digraphs
3. Complete Phase 3: User Story 1 (T024-T030)
4. **STOP and VALIDATE**: Test `Digraphs.Lookup('E', 'u')` returns 0x20AC
5. MVP is functional - users can look up digraph code points

### Incremental Delivery

1. Complete Setup + Foundational → Dictionary data ready
2. Add User Story 1 → Lookup method works → **MVP Complete!**
3. Add User Story 2 → Null handling verified → Deploy/Demo
4. Add User Story 3 → GetString convenience method → Deploy/Demo
5. Add User Story 4 → Map property for enumeration → Deploy/Demo
6. Polish → Coverage, docs, edge cases → **Feature Complete!**

---

## Notes

- [P] tasks = different files or independent dictionary sections, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Foundational phase is data-heavy but parallelizable by Unicode block
- File may approach 1,000 LOC due to 1,300+ dictionary entries - this is acceptable per Constitution X exception for data files
