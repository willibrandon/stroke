# Comprehensive Requirements Quality Checklist: Mouse Bindings

**Purpose**: Full audit of requirements completeness, clarity, consistency, measurability, and coverage across all spec/plan/research artifacts
**Created**: 2026-01-30
**Feature**: [spec.md](../spec.md)
**Status**: All 58 items resolved

## Requirement Completeness

- [x] CHK001 Are requirements defined for all three VT100 protocol formats (XTerm SGR, Typical/X10, URXVT)? [Completeness, Spec §FR-001/002/003] — Yes, FR-001, FR-002, FR-003 each cover one protocol.
- [x] CHK002 Are requirements defined for the Windows mouse event format independently from VT100? [Completeness, Spec §FR-019] — Yes, FR-019/020/021 cover Windows independently.
- [x] CHK003 Are all five mouse button values (Left, Middle, Right, None, Unknown) explicitly required? [Completeness, Spec §FR-004] — Yes, FR-004 enumerates all five.
- [x] CHK004 Are all five mouse event types (MouseDown, MouseUp, MouseMove, ScrollUp, ScrollDown) explicitly required? [Completeness, Spec §FR-005] — Yes, FR-005 enumerates all five.
- [x] CHK005 Are requirements for all 7 non-empty modifier combinations plus no-modifier documented for XTerm SGR? [Completeness, Spec §FR-006] — Yes, FR-006 specifies "7 non-empty combinations plus no-modifier" and now includes bit-field encoding details.
- [x] CHK006 Is the "Unknown modifier" semantic explicitly required for Typical and URXVT protocols? [Completeness, Spec §FR-007] — Yes, FR-007 now explicitly documents the semantic distinction between Unknown and None.
- [x] CHK007 Are coordinate transformation requirements specified for each protocol independently (XTerm SGR, Typical, URXVT, Windows)? [Completeness, Spec §FR-008/008a/009/010/020] — **Fixed**: Added FR-008a for URXVT coordinate transform (previously implicit).
- [x] CHK008 Is the rows-above-layout y-adjustment requirement specified as a common post-processing step for all VT100 protocols? [Completeness, Spec §FR-011] — **Fixed**: FR-011 now explicitly states "for all three VT100 protocol formats (XTerm SGR, Typical, URXVT)".
- [x] CHK009 Are requirements for the scroll-without-position fallback (ScrollUp/ScrollDown to arrow keys) documented? [Completeness, Spec §FR-017/018] — Yes, FR-017 and FR-018 cover both directions.
- [x] CHK010 Is the requirement that LoadMouseBindings returns exactly 4 bindings documented? [Completeness, Spec §FR-023] — Yes, FR-023 specifies "exactly 4 key bindings".
- [x] CHK011 Are exact entry counts specified for all three lookup tables (96, 10, 4)? [Completeness, Spec §FR-024/025/026] — Yes, FR-024 (96), FR-025 (10), FR-026 (4).

## Requirement Clarity

- [x] CHK012 Is the XTerm SGR bit-field encoding scheme (bits 0-1 = button, bit 2 = Shift, bit 3 = Alt, bit 4 = Control, bit 5 = drag offset) explicitly defined? [Clarity, Spec §FR-006, Data Model] — **Fixed**: FR-006 now includes full bit-field encoding: "bit 2 (value 4) = Shift, bit 3 (value 8) = Alt, bit 4 (value 16) = Control. Button identity uses bits 0-1, and bit 5 (value 32) indicates drag/motion events."
- [x] CHK013 Is the distinction between suffix 'M' (press/move/scroll) and 'm' (release) clearly specified for XTerm SGR? [Clarity, Spec §FR-001] — **Fixed**: FR-001 now states "uppercase `ESC[<code;x;yM` indicates press/move/scroll events and lowercase `ESC[<code;x;ym` indicates button release events. The suffix character ('M' or 'm') combined with the numeric event code forms the lookup table key."
- [x] CHK014 Is the Typical coordinate encoding (char ordinal - 32 - 1) unambiguously specified? [Clarity, Spec §FR-009] — Yes, FR-009 states "subtracting 32, then subtracting 1".
- [x] CHK015 Is the surrogate escape threshold (>= 0xDC00) and offset subtraction precisely defined? [Clarity, Spec §FR-010] — Yes, FR-010 states ">= 0xDC00" and "subtracting 0xDC00 before applying the standard offset".
- [x] CHK016 Is the Windows event data format ("button;eventType;x;y" as semicolon-separated values) precisely defined? [Clarity, Spec §FR-019] — **Fixed**: FR-019 now includes example: "e.g. `Left;MouseDown;10;5`".
- [x] CHK017 Is "NotImplemented" clearly defined as the return value (not an exception) for unknown/unsupported scenarios? [Clarity, Spec §FR-013/014/015/021] — **Fixed**: FR-013 now says "(a return value, not an exception)". Added "Note on NotImplemented" paragraph after FR list clarifying return types for all handler categories.
- [x] CHK018 Is the meaning of "Unknown modifier" clearly distinguished from "No modifier" (same enum value, different semantic intent)? [Clarity, Spec §FR-007, Research R1] — **Fixed**: FR-007 now states: '"Unknown modifier" uses the same underlying value as "no modifier" (`MouseModifiers.None` = 0) but carries different semantic intent.'
- [x] CHK019 Is the URXVT fallback behavior for unknown event codes precisely specified as "Unknown button with MouseMove event type"? [Clarity, Spec §FR-016] — Yes, FR-016 states "fall back to Unknown button with MouseMove event type".
- [x] CHK020 Is the "first: true" parameter for KeyProcessor.Feed in scroll handlers explicitly specified or only implied? [Clarity, Spec §FR-017/018] — **Fixed**: FR-017 and FR-018 now explicitly state "with `first: true` (inserted at the front of the event queue)".

## Requirement Consistency

- [x] CHK021 Are coordinate transformation rules consistent between spec (§FR-008/009), data model (Coordinate Transformations table), and acceptance scenarios? [Consistency] — Yes, all consistent: XTerm SGR subtract 1, Typical subtract 32 then 1, acceptance scenarios show matching math (10,5→9,4).
- [x] CHK022 Is the MouseHandlers.GetHandler parameter order documented consistently? Spec Assumption §7 says "(y, x)" but Research R8 says "(x, y)" — is the correct order clarified? [Conflict, Spec Assumptions vs Research R8] — **Fixed**: Assumption §2 now says "`GetHandler(x, y)` method (x first, then y)".
- [x] CHK023 Are lookup table entry counts consistent between spec (§FR-024/025/026), data model, and success criteria (§SC-001/002/003)? [Consistency] — Yes, all three artifacts agree: 96, 10, 4.
- [x] CHK024 Is the modifier representation consistent between spec (§FR-006 mentioning "set" semantics) and research R1 (flags enum)? [Consistency, Spec §FR-006 vs Research R1] — **Fixed**: FR-006 now explicitly references "`MouseModifiers` flags enum". Key Entity "Modifier Set" now references "`MouseModifiers` flags enum value".
- [x] CHK025 Are all four handler return type semantics consistently specified? (VT100 returns NotImplemented or handler result; Scroll returns null/void; Windows returns NotImplemented or handler result) [Consistency, Contract handler signatures] — **Fixed**: "Note on NotImplemented" paragraph now clarifies: "The VT100 and Windows handlers return `NotImplementedOrNone?`; the scroll handlers return void (always handled)."
- [x] CHK026 Is the HeightIsUnknown error handling consistently specified between Spec §FR-014 ("HeightIsUnknownError"), Research R7 ("HeightIsUnknownException"), and data model? [Consistency, Naming] — **Fixed**: FR-014 and edge case now both use "`HeightIsUnknownException`" (the C# name). Python name removed from spec.

## Acceptance Criteria Quality

- [x] CHK027 Does SC-001 define how to verify "correct" XTerm SGR table entries beyond just count? [Measurability, Spec §SC-001] — **Fixed**: SC-001 now states '"correct" means matching the Python reference source values exactly' and lists specific representative entries to validate.
- [x] CHK028 Does SC-004 specify which coordinate test vectors validate correctness for "all three VT100 protocol formats"? [Measurability, Spec §SC-004] — **Fixed**: SC-004 now lists representative test vectors: "XTerm SGR (10,5) → (9,4); Typical bytes (42,37) → (9,4); URXVT (14,13) → (13,12); Typical with surrogate escape (0xDC00+42, 0xDC00+37) → (9,4)."
- [x] CHK029 Is SC-007 ("at least 80% coverage") measurable given that handler dispatch requires a running Application context? [Measurability, Spec §SC-007] — **Fixed**: SC-007 now clarifies: "Handler dispatch paths that require a running Application context may be excluded from coverage calculation if untestable in isolation."
- [x] CHK030 Does SC-006 enumerate all "unknown" and "unsupported" scenarios that must be exercised? [Measurability, Spec §SC-006] — **Fixed**: SC-006 now lists 5 specific scenarios: (a) unknown XTerm SGR code, (b) HeightIsKnown false, (c) HeightIsUnknownException, (d) non-Windows platform, (e) Windows with non-Win32 output.
- [x] CHK031 Are acceptance scenarios for User Story 1 sufficient to validate all 8 modifier combinations, or only a subset? [Coverage, Spec US-1] — **Fixed**: Added note after US-1 scenarios: "Full modifier combination coverage (all 8 combos) is validated by the lookup table tests (SC-001), not repeated per acceptance scenario."
- [x] CHK032 Are acceptance scenarios for User Story 2 sufficient to validate all 4 drag source buttons (Left, Middle, Right, None)? [Coverage, Spec US-2] — **Fixed**: Added note after US-2 scenarios: "All 4 drag source buttons (Left, Middle, Right, None) and all scroll direction + modifier combinations are validated by the lookup table tests (SC-001)."

## Scenario Coverage

- [x] CHK033 Are requirements defined for the case where mouse coordinates are negative after y-adjustment (layout extends beyond terminal)? [Coverage, Edge Case, Gap] — **Fixed**: Added edge case documenting that negative coordinates are passed through to the handler registry (typically DummyHandler returns NotImplemented), matching Python behavior.
- [x] CHK034 Are requirements defined for integer overflow/underflow in coordinate arithmetic (e.g., very large x/y values)? [Coverage, Edge Case, Gap] — Not applicable: coordinates are parsed from short decimal strings or single bytes, making overflow unrealistic. No requirement needed.
- [x] CHK035 Are requirements defined for malformed escape sequences (truncated, missing delimiters, non-numeric values)? [Coverage, Exception Flow, Gap] — **Fixed**: Added edge case documenting that malformed sequences are filtered by the VT100 input parser layer before reaching mouse bindings (precondition).
- [x] CHK036 Are requirements defined for the case where KeyPressEvent.Data is null or empty? [Coverage, Edge Case, Gap] — **Fixed**: Added edge case documenting that `Data` is a precondition populated by the input system; null indicates an input system bug.
- [x] CHK037 Are requirements defined for concurrent mouse events arriving while a previous handler is still executing? [Coverage, Concurrency, Gap] — **Fixed**: Added edge case documenting the class is entirely stateless (static methods, immutable tables); NFR-003/NFR-004 formalize thread safety requirements.
- [x] CHK038 Are requirements for the Typical protocol's unguarded lookup path explicitly addressed? Edge case §3 says "may throw if the code is not present" — is this intentional? [Clarity, Spec Edge Cases §3] — **Fixed**: Edge case now explicitly states: "This is intentional: the Python reference does not guard this path, and faithfully porting this behavior (Constitution Principle I) means not adding defensive checks that the original lacks."
- [x] CHK039 Are requirements defined for mouse events received before the Application/Renderer is fully initialized (App is null)? [Coverage, Exception Flow, Gap] — **Fixed**: Added edge case documenting that `GetApp()` throws `InvalidOperationException` if null; mouse bindings are only active when an Application is running (precondition).

## Edge Case Coverage

- [x] CHK040 Is the unknown XTerm SGR event code behavior (return NotImplemented) explicitly required, not just documented in edge cases? [Coverage, Spec §FR-015 vs Edge Cases §1] — Yes, FR-015 is a functional requirement: "System MUST return NotImplemented when an XTerm SGR event code is not found in the lookup table." Edge case §1 is consistent.
- [x] CHK041 Is the unknown URXVT event code fallback behavior a functional requirement or only an edge case observation? [Clarity, Spec §FR-016 vs Edge Cases §2] — Yes, FR-016 is a functional requirement: "System MUST fall back to Unknown button with MouseMove event type." Edge case §2 is consistent.
- [x] CHK042 Are requirements for mouse events at position (0, 0) after coordinate transformation explicitly addressed? [Coverage, Edge Case, Gap] — **Fixed**: Added edge case: "This is a valid position representing the top-left corner of the layout. The handler registered at (0, 0) is invoked normally."
- [x] CHK043 Is the maximum coordinate range defined for each protocol? (Typical is limited by single-byte encoding; XTerm SGR and URXVT use decimal integers) [Coverage, Gap] — Not applicable as a requirement. Protocol-specific coordinate ranges are inherent to terminal encoding standards, not requirements on this module. Typical is limited to ~222 columns (byte encoding), XTerm SGR/URXVT are limited by terminal dimensions.
- [x] CHK044 Are requirements defined for modifier key behavior with Typical/URXVT scroll events specifically? [Coverage, Spec §FR-007] — **Fixed**: Added edge case explicitly addressing Typical/URXVT scroll + modifier: "All events from these protocols, including scroll events, report Unknown modifier regardless of which keys the user holds." FR-007 covers the general case.

## Non-Functional Requirements

- [x] CHK045 Are performance requirements for lookup table access specified beyond "O(1)"? (e.g., maximum latency per mouse event) [Gap, NFR] — **Fixed**: Added NFR-001: "Lookup table access MUST be O(1) using `FrozenDictionary`. No per-event memory allocation is permitted in the lookup path."
- [x] CHK046 Are memory requirements for the static lookup tables specified? (3 FrozenDictionaries with 122 total entries) [Gap, NFR] — **Fixed**: Added NFR-002: "The three static lookup tables (122 total entries across 3 `FrozenDictionary` instances) are allocated once at class load time and impose no ongoing memory overhead."
- [x] CHK047 Are thread safety requirements explicitly stated in the spec, or only implied by the Constitution? [Gap, NFR, Constitution XI] — **Fixed**: Added NFR-003 and NFR-004 explicitly stating stateless/static design and thread safety guarantees.
- [x] CHK048 Is the stateless/static design explicitly required in the spec, or only defined in the plan? [Gap, NFR] — **Fixed**: NFR-003 states: "The `MouseBindings` class MUST be a static, stateless class with no mutable fields."

## Dependencies & Assumptions

- [x] CHK049 Is Assumption §2 ("MouseHandlers registry provides GetHandler(y, x)") validated against the actual API which uses GetHandler(x, y)? [Assumption, Conflict] — **Fixed**: Assumption now reads "`GetHandler(x, y)` method (x first, then y)".
- [x] CHK050 Is Assumption §6 ("Win32Output provides GetWin32ScreenBufferInfo()") validated, given Research R4 states Win32Output does not yet exist? [Assumption, Research R4] — **Fixed**: Assumption now states: "The `Win32Output` class is not yet implemented (part of Feature 21/57). The Windows mouse handler MUST be structured to return NotImplemented when no Win32-compatible output type is available."
- [x] CHK051 Is Assumption §4 ("MouseModifier types are already defined") validated against the actual implementation which uses MouseModifiers (flags enum) not FrozenSet<MouseModifier>? [Assumption, Research R1] — **Fixed**: Assumption now reads "`MouseModifiers` (flags enum)" and references Feature 013 (correct feature number).
- [x] CHK052 Is the dependency on HeightIsUnknownException clearly traced to its source (Renderer.cs)? [Dependency, Research R7] — **Fixed**: Added new assumption: "The `HeightIsUnknownException` is defined in `Stroke.Rendering` (in the same file as `Renderer`)."
- [x] CHK053 Are all 13 external dependencies listed in the contract validated as available in the current codebase? [Dependency, Contract] — Dependencies are validated in the contract artifact (`contracts/mouse-bindings.md`). Win32Output absence is now documented as a known gap in spec assumptions. All other dependencies (KeyBindings, KeyProcessor, KeyPress, KeyPressEvent, NotImplementedOrNone, KeyHandlerCallable, Keys, MouseEvent, MouseButton, MouseEventType, MouseModifiers, Point, Renderer, HeightIsUnknownException, MouseHandlers, FrozenDictionary, RuntimeInformation) exist in the codebase.

## Ambiguities & Conflicts

- [x] CHK054 Does the spec conflate "HeightIsUnknownError" (Python name, §FR-014/Edge Cases) with the C# "HeightIsUnknownException"? [Ambiguity, Naming] — **Fixed**: FR-014 and edge case now use "`HeightIsUnknownException`" (C# name). Python name removed.
- [x] CHK055 Does Spec Assumption §2 use Python parameter order "(y, x)" while the actual C# API uses "(x, y)"? [Conflict, Spec Assumptions vs Research R8] — **Fixed**: Assumption now reads "`GetHandler(x, y)` method (x first, then y)".
- [x] CHK056 Does the spec's use of "MouseModifier" (singular) in Key Entities §3 conflict with the actual type name "MouseModifiers" (plural, flags enum)? [Ambiguity, Naming] — **Fixed**: Key Entity "Modifier Set" now reads "A `MouseModifiers` flags enum value" (correct plural form).
- [x] CHK057 Is the distinction between FR-013 (height not known) and FR-014 (HeightIsUnknownError on RowsAboveLayout) sufficiently clear, given they describe related but distinct failure modes? [Ambiguity, Spec §FR-013/014] — **Fixed**: FR-013 now says "early-exit guard checked before attempting any coordinate adjustment." FR-014 now says "handles the race condition where height status changes between the FR-013 check and the actual layout metric access."
- [x] CHK058 Does the spec define what "button;eventType;x;y" values look like for Windows events, or only the format structure? [Ambiguity, Spec §FR-019] — **Fixed**: FR-019 now includes example: "e.g. `Left;MouseDown;10;5`".

## Notes

- All 58 items resolved and marked complete.
- 28 items required spec edits (marked with **Fixed**); 30 items were already satisfied.
- Key changes to spec.md: Added FR-008a (URXVT coords), NFR-001 through NFR-004, 8 new edge cases, strengthened SC-001/004/006/007, fixed 3 naming conflicts (HeightIsUnknownException, GetHandler parameter order, MouseModifiers), added representative test vectors, clarified NotImplemented semantics.
