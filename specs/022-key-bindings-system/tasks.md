# Tasks: Key Bindings System

**Input**: Design documents from `/specs/022-key-bindings-system/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/key-bindings-api.md
**Tests**: Included (per Constitution VIII - Real-World Testing)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Based on plan.md structure:
- **Source**: `src/Stroke/KeyBinding/`
- **Tests**: `tests/Stroke.Tests/KeyBinding/`

---

## Phase 1: Setup

**Purpose**: Project structure and directory initialization

- [ ] T001 Create `src/Stroke/KeyBinding/` directory if not exists
- [ ] T002 Create `tests/Stroke.Tests/KeyBinding/` directory if not exists
- [ ] T003 Verify existing dependencies available: `SimpleCache`, `IFilter`, `FilterOrBool`, `FilterUtils`, `Keys` enum, `NotImplementedOrNone`

---

## Phase 2: Foundational (Core Types)

**Purpose**: Implement core types that ALL user stories depend on. MUST complete before any user story.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Foundational Types (No Story Dependencies)

- [ ] T004 [P] Implement `KeyOrChar` readonly record struct in `src/Stroke/KeyBinding/KeyOrChar.cs` per contracts/key-bindings-api.md: private fields `_key` (Keys?) and `_char` (char?), properties IsKey/IsChar/Key/Char, implicit conversions from Keys/char/string, value equality
- [ ] T005 [P] Implement `KeyPress` readonly record struct in `src/Stroke/KeyBinding/KeyPress.cs` per FR-046 to FR-048: properties Key (KeyOrChar) and Data (string), constructor with default Data behavior (char → char.ToString(), Keys → enum name), value equality
- [ ] T006 [P] Implement `KeyHandlerCallable` delegate in `src/Stroke/KeyBinding/KeyHandlerCallable.cs`: `NotImplementedOrNone? KeyHandlerCallable(KeyPressEvent @event)`
- [ ] T007 [P] Implement `AsyncKeyHandlerCallable` delegate in `src/Stroke/KeyBinding/KeyHandlerCallable.cs`: `Task<NotImplementedOrNone?> AsyncKeyHandlerCallable(KeyPressEvent @event)`
- [ ] T008 [P] Implement `IKeyBindingsBase` interface in `src/Stroke/KeyBinding/IKeyBindingsBase.cs` per contracts: Version (object), Bindings (IReadOnlyList<Binding>), GetBindingsForKeys, GetBindingsStartingWithKeys
- [ ] T009 Implement `Binding` sealed class in `src/Stroke/KeyBinding/Binding.cs` per contracts: immutable, constructor with validation (ArgumentException for empty keys, ArgumentNullException for null handler), default filters per FR-055 (filter=Always, eager=Never, isGlobal=Never, recordInMacro=Always), Call method for handler invocation
- [ ] T010 [P] Implement `KeyBindingsProxy` abstract base class in `src/Stroke/KeyBinding/KeyBindingsProxy.cs` per data-model.md: _bindings2 (IKeyBindingsBase), _lastVersion (object), abstract UpdateCache method, thread-safe via Lock

### Foundational Tests

- [ ] T011 [P] Test `KeyOrChar` in `tests/Stroke.Tests/KeyBinding/KeyOrCharTests.cs`: construction from Keys/char/string, IsKey/IsChar properties, Key/Char accessors with exceptions, implicit conversions, equality
- [ ] T012 [P] Test `KeyPress` in `tests/Stroke.Tests/KeyBinding/KeyPressTests.cs`: construction with/without data, default data behavior per FR-047, equality
- [ ] T013 [P] Test `Binding` in `tests/Stroke.Tests/KeyBinding/BindingTests.cs`: construction with all parameters, validation exceptions, default filter values, Call method

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Register Single Key Bindings (Priority: P1) 🎯 MVP

**Goal**: Create KeyBindings registry, add bindings for single keys and multi-key sequences, retrieve bindings by exact match

**Independent Test**: Create registry, add binding for Ctrl+C, verify handler returned when querying for Ctrl+C

### Tests for User Story 1

- [ ] T014 [P] [US1] Test add single key binding in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: add Ctrl+C, verify Bindings.Count == 1
- [ ] T015 [P] [US1] Test query exact match in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: add Ctrl+X binding, GetBindingsForKeys([Ctrl+X]) returns it
- [ ] T016 [P] [US1] Test multi-key sequence in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: add Ctrl+X Ctrl+C, query returns binding
- [ ] T017 [P] [US1] Test registration order in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: add handler1 then handler2 for same key, verify FIFO order in results

### Implementation for User Story 1

- [ ] T018 [US1] Implement `KeyBindings` class in `src/Stroke/KeyBinding/KeyBindings.cs`: private fields (_lock, _bindings list, _version int, _forKeysCache SimpleCache<ImmutableArray<KeyOrChar>, IReadOnlyList<Binding>> with maxSize=10000, _startingCache with maxSize=1000)
- [ ] T019 [US1] Implement `KeyBindings.Add` method: returns Func<T,T> decorator, creates Binding, adds to _bindings, increments version, clears caches (atomic via lock per FR-033)
- [ ] T020 [US1] Implement `KeyBindings.Version` property: returns _version, atomic reads per FR-037
- [ ] T021 [US1] Implement `KeyBindings.Bindings` property: returns snapshot of _bindings as IReadOnlyList
- [ ] T022 [US1] Implement `KeyBindings.GetBindingsForKeys`: uses cache, returns exact matches sorted by Keys.Any count (fewer = higher priority per FR-024), O(1) hits O(n) misses per FR-050

**Checkpoint**: User Story 1 complete - can register and retrieve single/multi-key bindings

---

## Phase 4: User Story 2 - Conditional Key Bindings with Filters (Priority: P2)

**Goal**: Bindings conditionally active based on filters, ConditionalKeyBindings wrapper

**Independent Test**: Create binding with filter returning false, verify not active; with filter returning true, verify active

### Tests for User Story 2

- [ ] T023 [P] [US2] Test Always filter active in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: binding with Always filter is always active
- [ ] T024 [P] [US2] Test Never filter optimized away in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: binding with Never filter not stored, Bindings.Count unchanged per FR-025
- [ ] T025 [P] [US2] Test conditional filter active in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: binding with Condition(() => true) is active
- [ ] T026 [P] [US2] Test conditional filter inactive in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: binding with Condition(() => false) is inactive
- [ ] T027 [P] [US2] Test ConditionalKeyBindings wrapper in `tests/Stroke.Tests/KeyBinding/ConditionalKeyBindingsTests.cs`: wrapper filter false makes all bindings inactive

### Implementation for User Story 2

- [ ] T028 [US2] Implement Never filter optimization in `KeyBindings.Add`: check if filter is Never, return identity decorator without storing per FR-025
- [ ] T029 [US2] Implement `ConditionalKeyBindings` class in `src/Stroke/KeyBinding/ConditionalKeyBindings.cs`: extends KeyBindingsProxy, applies filter composition using AND per FR-026 and FR-056, Version/Bindings/GetBindingsForKeys/GetBindingsStartingWithKeys delegate to cached _bindings2

**Checkpoint**: User Story 2 complete - conditional bindings work with filters

---

## Phase 5: User Story 3 - Eager Matching for Immediate Execution (Priority: P3)

**Goal**: Mark bindings as eager for immediate execution, eager flag determines if prefix waiting should be bypassed

**Independent Test**: Create eager Ctrl+X binding and non-eager Ctrl+X Ctrl+C binding, verify GetBindingsForKeys returns eager binding and GetBindingsStartingWithKeys returns both

### Tests for User Story 3

- [ ] T030 [P] [US3] Test eager binding exact match in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: add eager Ctrl+X, GetBindingsForKeys([Ctrl+X]) returns it with Eager=true
- [ ] T031 [P] [US3] Test eager with longer sequence in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: add eager Ctrl+X and Ctrl+X Ctrl+C, GetBindingsStartingWithKeys([Ctrl+X]) returns both
- [ ] T032 [P] [US3] Test eager filter dynamic evaluation in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: binding with dynamic eager filter, verify Eager filter evaluated at query time

### Implementation for User Story 3

- [ ] T033 [US3] Implement `KeyBindings.GetBindingsStartingWithKeys`: uses _startingCache, returns bindings with sequences longer than input prefix, O(1) hits O(n) misses per FR-050
- [ ] T034 [US3] Verify eager flag passed through Binding constructor and Add method, composed with OR per FR-027

**Checkpoint**: User Story 3 complete - eager matching data available for KeyProcessor

---

## Phase 6: User Story 4 - Merge and Compose Key Bindings (Priority: P4)

**Goal**: Merge multiple KeyBindings registries, combined registry reflects changes from originals

**Independent Test**: Create two registries with different bindings, merge them, verify bindings from both accessible

### Tests for User Story 4

- [ ] T035 [P] [US4] Test merge non-overlapping keys in `tests/Stroke.Tests/KeyBinding/MergedKeyBindingsTests.cs`: merge two registries, verify bindings from both present
- [ ] T036 [P] [US4] Test merge overlapping keys in `tests/Stroke.Tests/KeyBinding/MergedKeyBindingsTests.cs`: same key in both, verify both bindings returned
- [ ] T037 [P] [US4] Test merged version tracking in `tests/Stroke.Tests/KeyBinding/MergedKeyBindingsTests.cs`: original changes, merged Version changes

### Implementation for User Story 4

- [ ] T038 [US4] Implement `MergedKeyBindings` class in `src/Stroke/KeyBinding/MergedKeyBindings.cs`: extends KeyBindingsProxy, Registries property, Version is composite of child versions, Bindings concatenates in registry order
- [ ] T039 [US4] Implement `KeyBindingUtils.Merge` static methods in `src/Stroke/KeyBinding/KeyBindingUtils.cs`: IEnumerable and params overloads, returns MergedKeyBindings
- [ ] T040 [US4] Implement `IKeyBindingsBase.Merge` extension method in `src/Stroke/KeyBinding/KeyBindingsExtensions.cs` per FR-040

**Checkpoint**: User Story 4 complete - registries can be merged

---

## Phase 7: User Story 5 - Global Key Bindings (Priority: P5)

**Goal**: Mark bindings as global, GlobalOnlyKeyBindings wrapper exposes only global bindings

**Independent Test**: Create global and non-global bindings, wrap in GlobalOnlyKeyBindings, verify only global exposed

### Tests for User Story 5

- [ ] T041 [P] [US5] Test global binding included in `tests/Stroke.Tests/KeyBinding/GlobalOnlyKeyBindingsTests.cs`: isGlobal=true binding included in GlobalOnlyKeyBindings
- [ ] T042 [P] [US5] Test non-global binding excluded in `tests/Stroke.Tests/KeyBinding/GlobalOnlyKeyBindingsTests.cs`: isGlobal=false binding excluded
- [ ] T043 [P] [US5] Test dynamic isGlobal filter in `tests/Stroke.Tests/KeyBinding/GlobalOnlyKeyBindingsTests.cs`: filter state change affects global status

### Implementation for User Story 5

- [ ] T044 [US5] Implement `GlobalOnlyKeyBindings` class in `src/Stroke/KeyBinding/GlobalOnlyKeyBindings.cs`: extends KeyBindingsProxy, filters to bindings where IsGlobal.Invoke() returns true at cache update time
- [ ] T045 [US5] Implement `IKeyBindingsBase.GlobalOnly` extension method in `src/Stroke/KeyBinding/KeyBindingsExtensions.cs`
- [ ] T046 [US5] Verify isGlobal flag passed through Binding constructor and Add method, composed with OR per FR-028

**Checkpoint**: User Story 5 complete - global bindings can be filtered

---

## Phase 8: User Story 6 - Dynamic Key Bindings (Priority: P6)

**Goal**: DynamicKeyBindings delegates to a callable that returns a registry at runtime

**Independent Test**: Create DynamicKeyBindings with callable returning registry A, verify bindings from A; change callable to return B, verify bindings from B

### Tests for User Story 6

- [ ] T047 [P] [US6] Test dynamic returns registry bindings in `tests/Stroke.Tests/KeyBinding/DynamicKeyBindingsTests.cs`: callable returns A, bindings from A returned
- [ ] T048 [P] [US6] Test dynamic switches registry in `tests/Stroke.Tests/KeyBinding/DynamicKeyBindingsTests.cs`: callable changes to B, bindings from B returned
- [ ] T049 [P] [US6] Test dynamic null returns empty in `tests/Stroke.Tests/KeyBinding/DynamicKeyBindingsTests.cs`: callable returns null, empty IReadOnlyList returned (not null), Version stable per US6 acceptance

### Implementation for User Story 6

- [ ] T050 [US6] Implement `DynamicKeyBindings` class in `src/Stroke/KeyBinding/DynamicKeyBindings.cs`: extends KeyBindingsProxy, GetKeyBindings property (Func<IKeyBindingsBase?>), _dummy empty KeyBindings fallback, callable invocation within lock per FR-053, cache invalidation on different registry per FR-054, Version tracks identity + version of returned registry

**Checkpoint**: User Story 6 complete - dynamic registry switching works

---

## Phase 9: User Story 7 - Remove Key Bindings (Priority: P7)

**Goal**: Remove bindings by handler reference or key sequence, error on non-existent removal

**Independent Test**: Add binding, remove it, verify not returned in queries

### Tests for User Story 7

- [ ] T051 [P] [US7] Test remove by handler in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: add binding, remove by handler, binding gone
- [ ] T052 [P] [US7] Test remove by keys in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: add binding, remove by keys, binding gone
- [ ] T053 [P] [US7] Test remove non-existent throws in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: remove non-existent binding throws InvalidOperationException per FR-022
- [ ] T054 [P] [US7] Test remove all matching handlers in `tests/Stroke.Tests/KeyBinding/KeyBindingsTests.cs`: multiple bindings with same handler, remove removes all

### Implementation for User Story 7

- [ ] T055 [US7] Implement `KeyBindings.Remove(KeyHandlerCallable handler)` in `src/Stroke/KeyBinding/KeyBindings.cs`: atomic removal per FR-034, throws InvalidOperationException if not found
- [ ] T056 [US7] Implement `KeyBindings.Remove(params KeyOrChar[] keys)` in `src/Stroke/KeyBinding/KeyBindings.cs`: atomic removal per FR-034, throws InvalidOperationException if not found
- [ ] T057 [US7] Implement backwards compatibility aliases `RemoveBinding` in `src/Stroke/KeyBinding/KeyBindings.cs`

**Checkpoint**: User Story 7 complete - bindings can be removed

---

## Phase 10: User Story 8 - Save Before Handler and Macro Recording (Priority: P8)

**Goal**: saveBefore callback invoked before handler, recordInMacro controls macro recording

**Independent Test**: Create binding with saveBefore returning true, verify callback invoked; create binding with recordInMacro=false, verify flag available

### Tests for User Story 8

- [ ] T058 [P] [US8] Test saveBefore callback invoked in `tests/Stroke.Tests/KeyBinding/BindingTests.cs`: binding with saveBefore, verify callback invoked with event
- [ ] T059 [P] [US8] Test saveBefore default true in `tests/Stroke.Tests/KeyBinding/BindingTests.cs`: default saveBefore returns true
- [ ] T060 [P] [US8] Test recordInMacro filter in `tests/Stroke.Tests/KeyBinding/BindingTests.cs`: binding with recordInMacro=false, verify RecordInMacro filter is Never
- [ ] T061 [P] [US8] Test recordInMacro default Always in `tests/Stroke.Tests/KeyBinding/BindingTests.cs`: default recordInMacro is Always per FR-055

### Implementation for User Story 8

- [ ] T062 [US8] Verify `Binding.SaveBefore` property returns callback, default is `_ => true`
- [ ] T063 [US8] Verify `Binding.RecordInMacro` property returns IFilter, default is Always per FR-055
- [ ] T064 [US8] Implement saveBefore invocation in `Binding.Call` method: invoke saveBefore before handler, exception propagates per edge case spec

**Checkpoint**: User Story 8 complete - save/macro features available

---

## Phase 11: Supporting Types

**Purpose**: Implement remaining types needed for complete API

### KeyPressEvent

- [ ] T065 Implement `KeyPressEvent` class in `src/Stroke/KeyBinding/KeyPressEvent.cs` per contracts: WeakReference<KeyProcessor>, KeySequence, PreviousKeySequence, IsRepeat, Arg (default 1, clamp to 1M), ArgPresent, App, CurrentBuffer, Data, AppendToArgCount method
- [ ] T066 [P] Test `KeyPressEvent` in `tests/Stroke.Tests/KeyBinding/KeyPressEventTests.cs`: construction, Arg behavior, AppendToArgCount

### KeyBindingDecorator

- [ ] T067 Implement `KeyBindingDecorator.Create` static method in `src/Stroke/KeyBinding/KeyBindingDecorator.cs` per FR-039: returns Func<KeyHandlerCallable, Binding> with pre-configured settings
- [ ] T068 [P] Test `KeyBindingDecorator` in `tests/Stroke.Tests/KeyBinding/KeyBindingDecoratorTests.cs`: Create with various parameters, verify Binding created with settings

### KeyBindingUtils.ParseKey

- [ ] T069 Implement `KeyBindingUtils.ParseKey` in `src/Stroke/KeyBinding/KeyBindingUtils.cs` per FR-029: handle c-x → ControlX, m-x → AltX, s-x → ShiftX aliases, special names (space, tab, enter)
- [ ] T070 [P] Test `KeyBindingUtils.ParseKey` in `tests/Stroke.Tests/KeyBinding/KeyBindingUtilsTests.cs`: all alias patterns, special names, invalid input throws ArgumentException

### Extension Methods

- [ ] T071 Implement `WithFilter` extension method in `src/Stroke/KeyBinding/KeyBindingsExtensions.cs`: wraps in ConditionalKeyBindings

---

## Phase 12: Thread Safety & Edge Cases

**Purpose**: Verify thread safety and edge case handling per Constitution XI and spec Edge Cases section

### Thread Safety Tests

- [ ] T072 [P] Test concurrent reads in `tests/Stroke.Tests/KeyBinding/ThreadSafetyTests.cs`: multiple threads calling GetBindingsForKeys simultaneously
- [ ] T073 [P] Test concurrent add/read in `tests/Stroke.Tests/KeyBinding/ThreadSafetyTests.cs`: one thread adding while others reading per FR-030
- [ ] T074 [P] Test atomic add in `tests/Stroke.Tests/KeyBinding/ThreadSafetyTests.cs`: verify add is atomic per FR-033
- [ ] T075 [P] Test atomic remove in `tests/Stroke.Tests/KeyBinding/ThreadSafetyTests.cs`: verify remove is atomic per FR-034
- [ ] T076 [P] Test version atomicity in `tests/Stroke.Tests/KeyBinding/ThreadSafetyTests.cs`: verify no torn reads per FR-037
- [ ] T077 [P] Test cache thread safety in `tests/Stroke.Tests/KeyBinding/ThreadSafetyTests.cs`: concurrent queries don't corrupt cache per FR-036
- [ ] T078 [P] Test handler self-modification in `tests/Stroke.Tests/KeyBinding/ThreadSafetyTests.cs`: handler calls Add/Remove on its own registry without deadlock per FR-057/FR-058

### Edge Case Tests

- [ ] T079 [P] Test empty key sequence in `tests/Stroke.Tests/KeyBinding/EdgeCaseTests.cs`: GetBindingsForKeys([]) returns empty, GetBindingsStartingWithKeys([]) returns all per FR-005/FR-006
- [ ] T080 [P] Test empty key sequence registration in `tests/Stroke.Tests/KeyBinding/EdgeCaseTests.cs`: Add with empty keys throws ArgumentException
- [ ] T081 [P] Test null handler in `tests/Stroke.Tests/KeyBinding/EdgeCaseTests.cs`: Binding constructor with null handler throws ArgumentNullException
- [ ] T082 [P] Test Keys.Any wildcard in `tests/Stroke.Tests/KeyBinding/EdgeCaseTests.cs`: Any matches any key, fewer Any = higher priority per FR-023/FR-024
- [ ] T083 [P] Test multiple Keys.Any in `tests/Stroke.Tests/KeyBinding/EdgeCaseTests.cs`: [Any, Any] matches any two-key sequence per edge case spec
- [ ] T084 [P] Test filter composition AND in `tests/Stroke.Tests/KeyBinding/EdgeCaseTests.cs`: verify truth table for filter AND composition per spec
- [ ] T085 [P] Test filter composition OR for eager in `tests/Stroke.Tests/KeyBinding/EdgeCaseTests.cs`: verify truth table for eager OR composition per spec
- [ ] T086 [P] Test filter composition OR for isGlobal in `tests/Stroke.Tests/KeyBinding/EdgeCaseTests.cs`: verify truth table for isGlobal OR composition per spec
- [ ] T087 [P] Test Unicode character keys in `tests/Stroke.Tests/KeyBinding/EdgeCaseTests.cs`: KeyOrChar with emoji and CJK characters per edge case spec
- [ ] T088 [P] Test filter exception propagation in `tests/Stroke.Tests/KeyBinding/EdgeCaseTests.cs`: filter throws, exception propagates per edge case spec
- [ ] T089 [P] Test DynamicKeyBindings callable exception in `tests/Stroke.Tests/KeyBinding/EdgeCaseTests.cs`: callable throws, exception propagates per edge case spec
- [ ] T090 [P] Test saveBefore exception in `tests/Stroke.Tests/KeyBinding/EdgeCaseTests.cs`: saveBefore throws, handler NOT executed per edge case spec

---

## Phase 13: Performance Validation

**Purpose**: Verify performance requirements per SC-001, SC-002, SC-006

- [ ] T091 Add performance benchmark for GetBindingsForKeys in `tests/Stroke.Benchmarks/KeyBinding/KeyBindingBenchmarks.cs`: <1ms p99 for 1000 bindings with warm cache per SC-001
- [ ] T092 Add performance benchmark for cache hit rate in `tests/Stroke.Benchmarks/KeyBinding/KeyBindingBenchmarks.cs`: >95% hit rate for 100 bindings, 20 queries round-robin per SC-002
- [ ] T093 Add performance benchmark for 10,000 bindings in `tests/Stroke.Benchmarks/KeyBinding/KeyBindingBenchmarks.cs`: <10ms p99 per SC-006

---

## Phase 14: Polish & Documentation

**Purpose**: Documentation, cleanup, and validation

- [ ] T094 [P] Add XML documentation to all public types per DR-001
- [ ] T095 [P] Add thread safety documentation per DR-002 to: KeyBindings, ConditionalKeyBindings, MergedKeyBindings, DynamicKeyBindings, GlobalOnlyKeyBindings
- [ ] T096 [P] Add exception documentation per DR-003 to all public methods that throw
- [ ] T097 [P] Document filter composition behavior on KeyBindings.Add per DR-004
- [ ] T098 [P] Document cache eviction policy (LRU via SimpleCache) on KeyBindings class per DR-005
- [ ] T099 Validate quickstart.md examples compile and run correctly
- [ ] T100 Run `dotnet test --collect:"XPlat Code Coverage"` and verify >80% line coverage per SC-004

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Stories (Phase 3-10)**: All depend on Foundational phase completion
  - US1 (Phase 3): Required first - implements core KeyBindings class
  - US2-US8 (Phase 4-10): Depend on US1 completion (build on KeyBindings)
  - US3-US8 can proceed in parallel after US2 if needed
- **Supporting Types (Phase 11)**: Can proceed after Phase 2, independent of user stories
- **Thread Safety (Phase 12)**: Depends on all implementation complete
- **Performance (Phase 13)**: Depends on all implementation complete
- **Polish (Phase 14)**: Depends on all other phases complete

### User Story Dependencies

| Story | Phase | Depends On | Can Start After |
|-------|-------|------------|-----------------|
| US1 | 3 | Foundational (Phase 2) | Phase 2 complete |
| US2 | 4 | US1 (Phase 3) | Phase 3 complete |
| US3 | 5 | US1 (Phase 3) | Phase 3 complete |
| US4 | 6 | US1 (Phase 3) | Phase 3 complete |
| US5 | 7 | US1 (Phase 3) | Phase 3 complete |
| US6 | 8 | US1 (Phase 3) | Phase 3 complete |
| US7 | 9 | US1 (Phase 3) | Phase 3 complete |
| US8 | 10 | US1 (Phase 3) | Phase 3 complete |

### Parallel Opportunities

**Within each phase**, tasks marked [P] can run in parallel:
- Phase 2: T004-T010 (foundational types) can be parallelized
- Phase 2: T011-T013 (foundational tests) can be parallelized
- Phase 3+: Tests within each story can be parallelized

**Across phases** (if team capacity allows):
- After Phase 3 (US1), Phases 4-10 (US2-US8) can proceed in parallel
- Phase 11 (Supporting Types) can proceed independently after Phase 2

---

## Parallel Example: Phase 2 Foundational

```bash
# Launch all foundational types in parallel:
Task: "Implement KeyOrChar in src/Stroke/KeyBinding/KeyOrChar.cs"
Task: "Implement KeyPress in src/Stroke/KeyBinding/KeyPress.cs"
Task: "Implement delegates in src/Stroke/KeyBinding/KeyHandlerCallable.cs"
Task: "Implement IKeyBindingsBase in src/Stroke/KeyBinding/IKeyBindingsBase.cs"
Task: "Implement KeyBindingsProxy in src/Stroke/KeyBinding/KeyBindingsProxy.cs"

# Then sequentially (Binding depends on KeyOrChar, IFilter):
Task: "Implement Binding in src/Stroke/KeyBinding/Binding.cs"

# Then launch all foundational tests in parallel:
Task: "Test KeyOrChar in tests/Stroke.Tests/KeyBinding/KeyOrCharTests.cs"
Task: "Test KeyPress in tests/Stroke.Tests/KeyBinding/KeyPressTests.cs"
Task: "Test Binding in tests/Stroke.Tests/KeyBinding/BindingTests.cs"
```

---

## Parallel Example: After US1 Complete

```bash
# Launch US2-US8 implementation in parallel (different files):
Task: "[US2] Implement ConditionalKeyBindings in src/Stroke/KeyBinding/ConditionalKeyBindings.cs"
Task: "[US4] Implement MergedKeyBindings in src/Stroke/KeyBinding/MergedKeyBindings.cs"
Task: "[US5] Implement GlobalOnlyKeyBindings in src/Stroke/KeyBinding/GlobalOnlyKeyBindings.cs"
Task: "[US6] Implement DynamicKeyBindings in src/Stroke/KeyBinding/DynamicKeyBindings.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test US1 independently - can register and retrieve bindings
5. Deploy/demo if ready - basic key binding system works

### Incremental Delivery

1. Complete Setup + Foundational → Core types ready
2. Add US1 → Test independently → MVP!
3. Add US2 → Conditional bindings work
4. Add US3-US8 → Full feature parity
5. Add Thread Safety/Performance validation
6. Polish documentation

### Full Parallel Strategy

With multiple developers after US1:
- Developer A: US2 (Conditional) + US3 (Eager)
- Developer B: US4 (Merge) + US5 (Global)
- Developer C: US6 (Dynamic) + US7 (Remove)
- Developer D: US8 (Save/Macro) + Supporting Types

---

## Summary

| Category | Task Count |
|----------|------------|
| Phase 1: Setup | 3 |
| Phase 2: Foundational | 13 |
| Phase 3: US1 (MVP) | 9 |
| Phase 4: US2 | 7 |
| Phase 5: US3 | 5 |
| Phase 6: US4 | 6 |
| Phase 7: US5 | 6 |
| Phase 8: US6 | 4 |
| Phase 9: US7 | 7 |
| Phase 10: US8 | 7 |
| Phase 11: Supporting Types | 7 |
| Phase 12: Thread Safety & Edge Cases | 19 |
| Phase 13: Performance | 3 |
| Phase 14: Polish | 7 |
| **Total** | **100** |

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests pass after implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
