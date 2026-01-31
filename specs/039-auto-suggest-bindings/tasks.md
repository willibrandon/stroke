# Tasks: Auto Suggest Bindings

**Input**: Design documents from `/specs/039-auto-suggest-bindings/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/auto-suggest-bindings.md, quickstart.md

**Tests**: Included per Constitution VIII (Real-World Testing) and SC-005 (80% coverage target).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup

**Purpose**: Create the source and test file skeletons with correct namespace, usings, and class declarations.

- [x] T001 Create `src/Stroke/Application/Bindings/AutoSuggestBindings.cs` with namespace `Stroke.Application.Bindings`, static class declaration, XML doc comments from contract, required usings (`Stroke.Application`, `Stroke.AutoSuggest`, `Stroke.Core`, `Stroke.Filters`, `Stroke.Input`, `Stroke.KeyBinding`, `System.Text.RegularExpressions`), and stub methods returning `null`
- [x] T002 Create `tests/Stroke.Tests/Application/Bindings/AutoSuggestBindingsTests.cs` with namespace `Stroke.Tests.Application.Bindings`, sealed test class implementing `IDisposable`, `SimplePipeInput`/`DummyOutput` fields, `Dispose()` method, `CreateEnvironment()` helper (Buffer + BufferControl + Window + Layout + Application + AppContext.SetApp scope), and `CreateEvent()` helper following the pattern in `SearchBindingsTests.cs`
- [x] T003 Verify build succeeds: `dotnet build src/Stroke/Stroke.csproj` and `dotnet build tests/Stroke.Tests/Stroke.Tests.csproj`

**Checkpoint**: Skeleton files compile. No functionality yet.

---

## Phase 2: Foundational — SuggestionAvailable Filter (FR-004, FR-007)

**Purpose**: Implement the `SuggestionAvailable` private filter that ALL bindings depend on. This is the blocking prerequisite for every user story.

**Why foundational**: Every binding registration uses `SuggestionAvailable` as its filter (or composed with `EmacsMode`). No user story can be implemented or tested without this filter.

- [x] T004 Implement `SuggestionAvailable` private static `IFilter` field in `AutoSuggestBindings.cs` using `new Condition(() => { ... })` with three-condition conjunction: `buffer.Suggestion is not null && buffer.Suggestion.Text.Length > 0 && buffer.Document.IsCursorAtTheEnd` (per contract §Internal Members)
- [x] T005 [P] Write filter-positive tests in `AutoSuggestBindingsTests.cs`: test that `SuggestionAvailable` evaluates to `true` when all three conditions are met (non-null suggestion, non-empty text, cursor at end) — since the filter is private, test indirectly by calling `LoadAutoSuggestBindings()` and verifying binding activation via the returned `KeyBindings` instance (depends on T001 stub providing a minimal `LoadAutoSuggestBindings()` that registers at least one binding)
- [x] T006 [P] Write filter-negative tests in `AutoSuggestBindingsTests.cs`: test that filter evaluates to `false` when (a) no suggestion is set, (b) suggestion text is empty string, (c) cursor is NOT at the end of the buffer — three separate test methods, each tested indirectly via `LoadAutoSuggestBindings()` binding behavior (same indirect approach as T005)
- [x] T007 Verify filter tests pass: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~AutoSuggestBindings"`

**Checkpoint**: Filter logic is implemented and tested. User story implementation can begin.

---

## Phase 3: User Story 1 — Accept Full Auto Suggestion (Priority: P1) 🎯 MVP

**Goal**: Ctrl-F, Ctrl-E, and Right arrow each insert the complete suggestion text into the buffer when a suggestion is available and the cursor is at the end.

**Independent Test**: Create a buffer with text "git" and suggestion " commit -m 'fix bug'", call `AcceptSuggestion` handler, verify buffer text becomes "git commit -m 'fix bug'".

**Spec refs**: FR-001, FR-002, SC-001

### Tests for User Story 1

- [x] T008 [P] [US1] Write `AcceptSuggestion_InsertsSuggestionText` test in `AutoSuggestBindingsTests.cs`: buffer with text "git" at cursor position 3, suggestion " commit -m 'fix bug'", call `AcceptSuggestion(event)`, assert buffer text equals "git commit -m 'fix bug'"
- [x] T009 [P] [US1] Write `AcceptSuggestion_ReturnsNull_WhenSuggestionIsNull` test in `AutoSuggestBindingsTests.cs`: buffer with no suggestion set, call `AcceptSuggestion(event)`, assert return value is `null` and buffer text is unchanged (null-guard per contract)
- [x] T010 [P] [US1] Write `AcceptSuggestion_SingleCharacterSuggestion` test in `AutoSuggestBindingsTests.cs`: buffer with suggestion "x", call handler, assert single character is inserted (edge case from spec)

### Implementation for User Story 1

- [x] T011 [US1] Implement `AcceptSuggestion` handler in `AutoSuggestBindings.cs`: read `event.CurrentBuffer.Suggestion`, null-guard (`if (suggestion is not null)`), call `buffer.InsertText(suggestion.Text)`, return `null` (per contract §Public API, maps to Python's `_accept`)
- [x] T012 [US1] Implement `LoadAutoSuggestBindings()` factory method in `AutoSuggestBindings.cs` with the first 3 binding registrations: Ctrl-F → `AcceptSuggestion` (filter: `SuggestionAvailable`), Ctrl-E → `AcceptSuggestion` (filter: `SuggestionAvailable`), Right → `AcceptSuggestion` (filter: `SuggestionAvailable`) — using `kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.X)], filter: new FilterOrBool(SuggestionAvailable))(AcceptSuggestion)` pattern
- [x] T013 [US1] Write `LoadAutoSuggestBindings_ReturnsKeyBindingsWithFullAcceptBindings` test in `AutoSuggestBindingsTests.cs`: call `LoadAutoSuggestBindings()`, verify the returned `KeyBindings` instance is not null and contains bindings (assert count or exercise bindings via key processor)
- [x] T014 [US1] Verify US1 tests pass: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~AutoSuggestBindings"`

**Checkpoint**: Full suggestion acceptance works for all three keys. MVP is functional.

---

## Phase 4: User Story 2 — Accept Partial Suggestion by Word (Priority: P2)

**Goal**: Escape+F inserts exactly one word/path segment from the suggestion in Emacs mode, using regex-based word boundary splitting.

**Independent Test**: Create a buffer with suggestion "commit -m 'message'" in Emacs mode, call `AcceptPartialSuggestion` handler, verify only "commit " is inserted.

**Spec refs**: FR-003, FR-005, FR-006, SC-002, SC-006

### Tests for User Story 2

- [x] T015 [P] [US2] Write `AcceptPartialSuggestion_InsertsFirstWordSegment` test in `AutoSuggestBindingsTests.cs`: buffer with suggestion "commit -m 'message'", call `AcceptPartialSuggestion(event)`, assert buffer text ends with "commit " (first non-empty segment from regex split)
- [x] T016 [P] [US2] Write `AcceptPartialSuggestion_InsertsPathSegment` test in `AutoSuggestBindingsTests.cs`: buffer with suggestion "home/user/documents/", call handler, assert only "home/" is inserted
- [x] T017 [P] [US2] Write `AcceptPartialSuggestion_LeadingSpaceSuggestion` test in `AutoSuggestBindingsTests.cs`: buffer with suggestion " commit -m 'fix'", call handler, assert only " " (single space) is inserted (per corrected data model)
- [x] T018 [P] [US2] Write `AcceptPartialSuggestion_LeadingSlashSuggestion` test in `AutoSuggestBindingsTests.cs`: buffer with suggestion "/home/user/", call handler, assert only "/" is inserted (per corrected contract word boundary table)
- [x] T019 [P] [US2] Write `AcceptPartialSuggestion_NoWordBoundary` test in `AutoSuggestBindingsTests.cs`: buffer with suggestion "abc", call handler, assert entire "abc" is inserted (single segment, no boundary match — behaves like full accept per spec edge case)
- [x] T020 [P] [US2] Write `AcceptPartialSuggestion_WhitespaceOnlySuggestion` test in `AutoSuggestBindingsTests.cs`: buffer with suggestion "   " (whitespace), call handler, assert entire "   " is inserted (per spec edge case)
- [x] T021 [P] [US2] Write `AcceptPartialSuggestion_ReturnsNull_WhenSuggestionIsNull` test in `AutoSuggestBindingsTests.cs`: buffer with no suggestion, call handler, assert return `null` and buffer unchanged (null-guard)

### Implementation for User Story 2

- [x] T022 [US2] Implement `AcceptPartialSuggestion` handler in `AutoSuggestBindings.cs`: read suggestion, null-guard, apply `Regex.Split(@"([^\s/]+(?:\s+|/))", suggestion.Text)`, select first non-empty element via `.First(x => !string.IsNullOrEmpty(x))`, call `buffer.InsertText(firstSegment)`, return `null` (per contract §Public API, maps to Python's `_fill`)
- [x] T023 [US2] Add 4th binding registration to `LoadAutoSuggestBindings()`: Escape+F → `AcceptPartialSuggestion` with composed filter `((Filter)SuggestionAvailable).And(EmacsFilters.EmacsMode)` (per contract §Key Binding Registration)
- [x] T024 [US2] Write `LoadAutoSuggestBindings_ContainsFourBindings` test in `AutoSuggestBindingsTests.cs`: call factory, verify the returned `KeyBindings` contains exactly 4 bindings
- [x] T025 [US2] Verify US2 tests pass: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~AutoSuggestBindings"`

**Checkpoint**: Partial word-segment acceptance works with correct regex splitting for all edge cases.

---

## Phase 5: User Story 3 — Bindings Inactive When No Suggestion (Priority: P1)

**Goal**: Auto suggest bindings do not activate when no suggestion is present, the suggestion text is empty, or the cursor is not at the end of the buffer. Keys pass through to normal handling.

**Independent Test**: Create a buffer with no suggestion, process Right arrow key through the auto suggest KeyBindings, verify the handler is NOT invoked (key falls through).

**Spec refs**: FR-004, FR-007, SC-003

**Note**: The core filter logic was implemented in Phase 2 (Foundational). This phase adds integration-level tests that verify bindings don't fire through the full binding registration, not just the filter predicate.

### Tests for User Story 3

- [x] T026 [P] [US3] Write `FullAcceptBinding_DoesNotActivate_WhenNoSuggestion` test in `AutoSuggestBindingsTests.cs`: buffer with no suggestion, verify `AcceptSuggestion` handler is not reached when processing Right arrow key through the returned `KeyBindings` (handler returns `NotImplemented` or binding doesn't match)
- [x] T027 [P] [US3] Write `FullAcceptBinding_DoesNotActivate_WhenSuggestionTextEmpty` test in `AutoSuggestBindingsTests.cs`: buffer with suggestion whose text is empty string, verify binding does not activate
- [x] T028 [P] [US3] Write `FullAcceptBinding_DoesNotActivate_WhenCursorNotAtEnd` test in `AutoSuggestBindingsTests.cs`: buffer with text "git co" at cursor position 3 (not at end) and a valid suggestion, verify binding does not activate
- [x] T029 [P] [US3] Write `PartialAcceptBinding_DoesNotActivate_WhenNoSuggestion` test in `AutoSuggestBindingsTests.cs`: same as T026 but for Escape+F binding

**Checkpoint**: All negative filter scenarios verified at binding integration level.

---

## Phase 6: User Story 4 — Bindings Override Vi Right Arrow (Priority: P2)

**Goal**: Auto suggest bindings loaded after Vi bindings take priority over Vi right arrow movement when a suggestion is available.

**Independent Test**: Verify that `LoadAutoSuggestBindings()` returns bindings that include Right arrow with the `SuggestionAvailable` filter, and document in XML comments that loading order determines priority (per FR-008, Python L25-27 comment).

**Spec refs**: FR-008, SC-004

### Tests for User Story 4

- [x] T030 [US4] Write `LoadAutoSuggestBindings_RightArrowBinding_HasSuggestionAvailableFilter` test in `AutoSuggestBindingsTests.cs`: verify the Right arrow binding in the returned `KeyBindings` is conditional on the `SuggestionAvailable` filter (when suggestion is present, it activates; when absent, it falls through — enabling Vi bindings loaded earlier to handle the key)
- [x] T031 [US4] Verify XML doc remark on `LoadAutoSuggestBindings()` confirms that callers must load these bindings AFTER Vi bindings for correct priority (per contract §Key Binding Registration notes and Python L25-27) — this remark should already exist from T001's stub creation using contract XML docs; verify and strengthen if needed

**Checkpoint**: Vi override behavior is documented and the conditional activation enabling fallthrough is tested.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, code quality, and documentation.

- [x] T032 Verify all tests pass: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~AutoSuggestBindings"`
- [x] T033 Run full test suite to verify no regressions: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj`
- [x] T034 Verify file sizes: `AutoSuggestBindings.cs` should be ~80 LOC (well under 1,000 LOC limit per Constitution X), `AutoSuggestBindingsTests.cs` should be ~250 LOC
- [x] T035 Verify XML doc comments are present on all public members per Constitution standards: class summary, `AcceptSuggestion`, `AcceptPartialSuggestion`, `LoadAutoSuggestBindings` — each with `<summary>`, `<param>`, `<returns>`, `<remarks>` tags matching the contract (includes Vi loading order remark from T031)
- [x] T036 Measure code coverage: run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~AutoSuggestBindings" --collect:"XPlat Code Coverage"` and verify ≥80% line coverage of `AutoSuggestBindings.cs` per SC-005
- [x] T037 Run quickstart.md validation: verify build command `dotnet build src/Stroke/Stroke.csproj` succeeds and filtered test command `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~AutoSuggestBindings"` passes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Phase 2 — MVP, implement first
- **US2 (Phase 4)**: Depends on Phase 2 — can run in parallel with US1 (different handler method)
- **US3 (Phase 5)**: Depends on Phase 2 (filter tests) and Phase 3 (needs bindings registered) — integration-level filter verification
- **US4 (Phase 6)**: Depends on Phase 3 (needs Right arrow binding registered)
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2. Independent — no dependencies on other stories.
- **US2 (P2)**: Can start after Phase 2. Independent — different handler method, different key binding.
- **US3 (P1)**: Depends on Phase 3 completion (needs bindings registered to test filter integration).
- **US4 (P2)**: Depends on Phase 3 completion (needs Right arrow binding registered to test override).

### Within Each User Story

- Tests written first (T008-T010 before T011-T012)
- Handler implementation before binding registration
- Binding registration before integration tests

### Parallel Opportunities

**Within Phase 1**: T001 and T002 can run in parallel (different files)
**Within Phase 2**: T005 and T006 can run in parallel (different test methods, same file)
**Within Phase 3**: T008, T009, T010 can run in parallel (different test methods)
**Within Phase 4**: T015-T021 can all run in parallel (different test methods)
**Within Phase 5**: T026-T029 can all run in parallel (different test methods)
**Cross-story**: US1 and US2 can run in parallel after Phase 2 (different handler implementations in different methods of the same file)

---

## Parallel Example: User Story 2

```bash
# Launch all US2 tests together (7 test methods, all [P]):
Task: "AcceptPartialSuggestion_InsertsFirstWordSegment test"
Task: "AcceptPartialSuggestion_InsertsPathSegment test"
Task: "AcceptPartialSuggestion_LeadingSpaceSuggestion test"
Task: "AcceptPartialSuggestion_LeadingSlashSuggestion test"
Task: "AcceptPartialSuggestion_NoWordBoundary test"
Task: "AcceptPartialSuggestion_WhitespaceOnlySuggestion test"
Task: "AcceptPartialSuggestion_ReturnsNull_WhenSuggestionIsNull test"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: Foundational filter (T004-T007)
3. Complete Phase 3: User Story 1 — full accept (T008-T014)
4. **STOP and VALIDATE**: Test US1 independently — all 3 keys insert full suggestion
5. Proceed to remaining stories

### Incremental Delivery

1. Phase 1 + Phase 2 → Foundation ready (filter works)
2. Add US1 (Phase 3) → Full accept works → MVP
3. Add US2 (Phase 4) → Partial accept works → Feature complete for Emacs users
4. Add US3 (Phase 5) → Filter integration verified → Defensive coverage
5. Add US4 (Phase 6) → Vi override documented and tested → Full feature
6. Phase 7 → Polish → Ship

### Single-Developer Strategy

Since this feature involves only 2 files (~80 + ~250 LOC):

1. Implement sequentially: Phase 1 → 2 → 3 → 4 → 5 → 6 → 7
2. Each phase is a natural commit point
3. Total: 37 tasks across 7 phases

---

## Notes

- [P] tasks = different files or independent test methods, no dependencies
- [Story] label maps task to specific user story for traceability
- Both source files are in `Stroke.Application.Bindings` namespace (not `Stroke.KeyBinding.Bindings`) per Constitution III
- Test patterns follow `SearchBindingsTests.cs` with `CreateEnvironment()` + `CreateEvent()` helpers
- All handlers return `NotImplementedOrNone?` (`null` on success) per `KeyHandlerCallable` delegate
- Regex pattern `@"([^\s/]+(?:\s+|/))"` is character-for-character identical to Python's `r"([^\s/]+(?:\s+|/))"`
- No mocks, no fakes, no FluentAssertions per Constitution VIII
