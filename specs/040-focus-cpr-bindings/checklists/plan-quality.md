# Plan Quality Checklist: Focus & CPR Bindings

**Purpose**: Thorough requirements quality validation across spec, plan, contracts, and Python source — covering plan quality, API fidelity, edge cases/assumptions, and test readiness
**Created**: 2026-01-31
**Feature**: [spec.md](../spec.md)

## Requirement Completeness

- [x] CHK001 - Are all public APIs from Python `focus.py` `__all__` exports (`focus_next`, `focus_previous`) accounted for in spec functional requirements? [Completeness, Spec §FR-001, §FR-002]
  - PASS: FR-001 covers `FocusNext` (maps to `focus_next`), FR-002 covers `FocusPrevious` (maps to `focus_previous`). Both `__all__` exports accounted for.
- [x] CHK002 - Are all public APIs from Python `cpr.py` `__all__` exports (`load_cpr_bindings`) accounted for in spec functional requirements? [Completeness, Spec §FR-004]
  - PASS: FR-004 covers `LoadCprBindings` (maps to `load_cpr_bindings`). The single `__all__` export is accounted for.
- [x] CHK003 - Is the CPR handler's internal parsing logic (row/col extraction) captured as a requirement, not just implied? [Completeness, Spec §FR-005]
  - STRENGTHENED: FR-005 now explicitly states: "Parsing MUST strip the 2-character prefix (`\x1b[`) and 1-character suffix (`R`), then split on `;` to extract row and column as integers."
- [x] CHK004 - Is the CPR handler's call to `renderer.report_absolute_cursor_row(row)` captured as a requirement? [Completeness, Spec §FR-006]
  - STRENGTHENED: FR-006 now explicitly names the method: "MUST report the parsed row value to the renderer via `Renderer.ReportAbsoluteCursorRow(row)`" and notes the column is parsed but unused.
- [x] CHK005 - Are handler function return types (`NotImplementedOrNone?`) specified in the contracts? [Completeness, Contracts §FocusFunctions]
  - PASS: Contracts define `public static NotImplementedOrNone? FocusNext(KeyPressEvent @event)` and `public static NotImplementedOrNone? FocusPrevious(KeyPressEvent @event)`.
- [x] CHK006 - Are namespace placement decisions documented with rationale for both classes? [Completeness, Plan §Project Structure]
  - PASS: Plan §Summary states: "Both classes are stateless and inherently thread-safe. The focus functions live in `Stroke.Application.Bindings` because they depend on `Application.Layout` (layer 7). The CPR bindings also live in `Stroke.Application.Bindings` because they depend on `Application.Renderer` (layer 7)."
- [x] CHK007 - Does the spec document the `saveBefore` behavior as a distinct functional requirement, not just mentioned in acceptance scenarios? [Completeness, Spec §FR-007]
  - STRENGTHENED: FR-007 now reads: "MUST be registered with save-before disabled (`saveBefore` callback returns `false` for all events)" with explicit rationale.

## Requirement Clarity

- [x] CHK008 - Is the CPR data format (`\x1b[<row>;<col>R`) specified precisely enough for unambiguous parsing, including the exact character offsets? [Clarity, Spec §FR-005]
  - STRENGTHENED: FR-005 now includes explicit character offsets: "strip the 2-character prefix (`\x1b[`) and 1-character suffix (`R`), then split on `;`."
- [x] CHK009 - Is "visible focusable window" defined or cross-referenced to an existing definition in the Layout system (Feature 29)? [Clarity, Spec §FR-001]
  - STRENGTHENED: FR-001 now defines: "A 'visible focusable window' is a window returned by the layout's visible-focusable-windows traversal (as defined in Feature 29's Layout system)."
- [x] CHK010 - Is the wrap-around direction for focus navigation described with sufficient precision (forward: last→first, backward: first→last)? [Clarity, Spec §FR-003]
  - STRENGTHENED: FR-003 now states: "advancing past the last focusable window returns to the first, and moving before the first returns to the last. This wrapping is implemented by the layout's focus methods via modular arithmetic."
- [x] CHK011 - Is "save-before disabled" clearly defined as `saveBefore: _ => false` rather than left as prose? [Clarity, Spec §FR-007]
  - STRENGTHENED: FR-007 now includes: "`saveBefore` callback returns `false` for all events" — explicit callback semantics, not just prose.
- [x] CHK012 - Is the distinction between "handler functions" (FocusFunctions — no binding loader) and "binding loader" (CprBindings — factory pattern) clearly articulated in the spec? [Clarity, Spec §FR-001 vs §FR-004]
  - STRENGTHENED: FR-001/FR-002 now explicitly state "(not a binding loader)" and FR-004 names the binding loader pattern: "a binding loader method (`LoadCprBindings`) that returns a `KeyBindings` instance."
- [x] CHK013 - Is FR-008 ("accept a key press event parameter") specific enough about the `KeyPressEvent` type and access pattern (`GetApp()`)? [Clarity, Spec §FR-008]
  - STRENGTHENED: FR-008 now reads: "MUST accept a `KeyPressEvent` parameter and access the application's layout through the event's application reference (via `GetApp().Layout`)."

## Requirement Consistency

- [x] CHK014 - Are `Keys.CPRResponse` references consistent across spec (§FR-005), plan (§RT-04), contracts, and the actual enum value in `Keys.cs:794`? [Consistency, Cross-Artifact]
  - STRENGTHENED: FR-004 now explicitly names `Keys.CPRResponse`. Contracts use `Keys.CPRResponse`. Plan uses `Keys.CPRResponse`. All match the actual enum value at `Keys.cs:794`.
- [x] CHK015 - Is the handler signature pattern (`NotImplementedOrNone?`) consistent with all existing binding handlers (ScrollBindings, SearchBindings, AutoSuggestBindings, BasicBindings)? [Consistency, Contracts vs Codebase]
  - PASS: Contracts specify `NotImplementedOrNone?` return type. Verified against: `ScrollBindings.ScrollForward`, `SearchBindings.AbortSearch`, `AutoSuggestBindings.AcceptSuggestion`, `BasicBindings` handlers — all use the same pattern.
- [x] CHK016 - Does the plan's layer assignment (Application layer 7) align with Constitution III's layered architecture and the placement of all other Application.Bindings files? [Consistency, Plan §Constitution Check III]
  - PASS: Constitution III lists Application as layer 7. All existing binding files (ScrollBindings, SearchBindings, AutoSuggestBindings, BasicBindings, PageNavigationBindings) are in `src/Stroke/Application/Bindings/`.
- [x] CHK017 - Are the focus function delegation targets (`Layout.FocusNext()`, `Layout.FocusPrevious()`) consistent with the actual Layout API signatures at `Layout.cs:362-391`? [Consistency, Contracts vs Layout.cs]
  - PASS: Verified `Layout.FocusPrevious()` at `Layout.cs:362` and `Layout.FocusNext()` at `Layout.cs:379` — both are `public void` with no parameters.
- [x] CHK018 - Does the contracts' `LoadCprBindings()` return type (`KeyBindings`) match the Python source return type and the pattern used by `LoadAutoSuggestBindings()`? [Consistency, Contracts vs Python cpr.py:15]
  - PASS: Python `load_cpr_bindings() -> KeyBindings` returns `KeyBindings`. Contracts specify `KeyBindings` return type. `AutoSuggestBindings.LoadAutoSuggestBindings()` also returns `KeyBindings`.
- [x] CHK019 - Is the spec's Key Entities section (FocusFunctions, CprBindings, KeyPressEvent) consistent with the contracts' class definitions? [Consistency, Spec §Key Entities vs Contracts]
  - PASS: Spec Key Entities lists FocusFunctions (static class with focus methods), CprBindings (static class with binding loader), KeyPressEvent (event object). Contracts define the same two static classes with matching responsibilities.

## API Fidelity (Python ↔ C# Cross-Check)

- [x] CHK020 - Does `FocusFunctions.FocusNext` exactly mirror `focus_next(event)` semantics: single delegation to `event.app.layout.focus_next()` with no additional logic? [API Fidelity, Python focus.py:13-18]
  - PASS: Python `focus_next` body is `event.app.layout.focus_next()` (one line). Contracts/spec define `FocusNext` as delegating to `Layout.FocusNext()` — exact semantic mirror.
- [x] CHK021 - Does `FocusFunctions.FocusPrevious` exactly mirror `focus_previous(event)` semantics: single delegation to `event.app.layout.focus_previous()` with no additional logic? [API Fidelity, Python focus.py:21-26]
  - PASS: Python `focus_previous` body is `event.app.layout.focus_previous()` (one line). Contracts/spec define `FocusPrevious` as delegating to `Layout.FocusPrevious()` — exact semantic mirror.
- [x] CHK022 - Does `CprBindings.LoadCprBindings()` mirror `load_cpr_bindings()` exactly: create KeyBindings, add one CPRResponse binding with `save_before=False`, parse data, report row? [API Fidelity, Python cpr.py:15-30]
  - PASS: Python creates `KeyBindings()`, adds `Keys.CPRResponse` with `save_before=lambda e: False`, handler parses `event.data[2:-1].split(";")`, calls `event.app.renderer.report_absolute_cursor_row(row)`. Contracts specify the identical sequence.
- [x] CHK023 - Are there any APIs invented in the contracts that do not exist in the Python source (Constitution I: "forbidden behaviors")? [API Fidelity, Constitution I]
  - PASS: Contracts define exactly 3 APIs: `FocusNext`, `FocusPrevious`, `LoadCprBindings`. All map 1:1 to Python exports. No invented APIs.
- [x] CHK024 - Are there any APIs from the Python source omitted from the contracts? [API Fidelity, Constitution I]
  - PASS: Python `focus.py.__all__` = `["focus_next", "focus_previous"]` — both present. Python `cpr.py.__all__` = `["load_cpr_bindings"]` — present. No omissions.
- [x] CHK025 - Is the CPR parsing logic (`data[2:-1].split(";")` → `Data[2..^1].Split(';')`) a faithful translation without embellishment? [API Fidelity, Python cpr.py:25 vs Contracts]
  - PASS: Contracts §CPR Data Format shows `data[2..^1]` → `Split(';')` which is the C# range-syntax equivalent of Python `data[2:-1].split(";")`. No embellishment.

## Acceptance Criteria Quality & Test Readiness

- [x] CHK026 - Are the focus navigation acceptance scenarios (User Story 1: 4 scenarios with A/B/C windows) specific enough to derive exact test assertions with expected focus targets? [Measurability, Spec §User Story 1]
  - PASS: All 4 scenarios specify: initial focus (A, C, A, B), action (focus-next or focus-previous), and exact expected target (B, A-wrap, C-wrap, A). Directly translatable to test assertions.
- [x] CHK027 - Are the CPR acceptance scenarios (User Story 2: row 35/col 1, row 1/col 80) specific enough to derive exact test inputs and expected `ReportAbsoluteCursorRow` arguments? [Measurability, Spec §User Story 2]
  - PASS: Scenario 1: input (35,1) → expect report(35). Scenario 2: input (1,80) → expect report(1). Scenario 3: saveBefore=false. All directly translatable.
- [x] CHK028 - Is SC-001 ("3+ window layouts") specific about the test configuration (number of windows, which has initial focus)? [Measurability, Spec §SC-001]
  - STRENGTHENED: SC-001 now references "3-window layouts as specified in User Story 1 acceptance scenarios (A→B, C→A wrap, A→C wrap, B→A)."
- [x] CHK029 - Is SC-002 ("at least 3 different row/column combinations") specific about which combinations and why they provide adequate coverage? [Measurability, Spec §SC-002]
  - STRENGTHENED: SC-002 now enumerates "typical (35,1), boundary (1,80), and mid-range (100,40) values" with coverage rationale.
- [x] CHK030 - Can SC-003 ("handle edge cases gracefully without exceptions") be objectively verified — is "gracefully" quantified (e.g., no-op, focus unchanged, no exception thrown)? [Measurability, Spec §SC-003]
  - STRENGTHENED: SC-003 now defines: "zero windows results in a no-op with unchanged layout state; one window results in focus remaining on the same window."
- [x] CHK031 - Is SC-004 ("100% fidelity") measurable — are the exact Python APIs enumerated so fidelity can be audited? [Measurability, Spec §SC-004]
  - STRENGTHENED: SC-004 now enumerates: "All 3 public APIs from Python Prompt Toolkit are ported: `focus_next` → `FocusNext`, `focus_previous` → `FocusPrevious`, `load_cpr_bindings` → `LoadCprBindings`."
- [x] CHK032 - Is SC-005 ("at least 80%") consistent with Constitution VIII's 80% target, and is the scope of measurement defined (per-file, per-class, per-feature)? [Measurability, Spec §SC-005]
  - STRENGTHENED: SC-005 now specifies: "this feature's new source files (`FocusFunctions.cs`, `CprBindings.cs`) reaches at least 80%, measured per-feature."
- [x] CHK033 - Are the single-window acceptance scenarios (User Story 3) specific about expected state after invocation (focus remains, no exception, no state change)? [Test Readiness, Spec §User Story 3]
  - STRENGTHENED: User Story 3 acceptance scenarios now include "and no exception is thrown" in both scenarios.
- [x] CHK034 - Are the zero-window acceptance scenarios (User Story 4) specific about expected state (no exception, layout state unchanged)? [Test Readiness, Spec §User Story 4]
  - PASS: Already states "no exception occurs and layout state is unchanged" — sufficiently specific.

## Edge Case Coverage

- [x] CHK035 - Is the decision to trust terminal CPR data (no defensive parsing) explicitly documented as a deliberate design choice matching Python behavior? [Edge Case, Spec §Assumptions, §Edge Cases]
  - STRENGTHENED: Assumptions section now references Python source directly: "matching Python Prompt Toolkit behavior, which performs `event.data[2:-1].split(";")` without try/catch." Edge Cases section now notes: "Malformed data will result in a parsing exception (undefined behavior), matching the Python source which performs no defensive validation."
- [x] CHK036 - Are requirements specified for the scenario where `@event.Data` is null, empty, or malformed for a CPR response event? [Edge Case, Gap]
  - STRENGTHENED: New edge case added: "What happens when `KeyPressEvent.Data` is null or empty for a CPR response event? This cannot occur in practice because the input parser only generates `Keys.CPRResponse` events when a valid CPR escape sequence is detected. No defensive guard is added, matching Python behavior."
- [x] CHK037 - Is the behavior defined when focus navigation is called while windows are transitioning visibility (becoming non-visible during traversal)? [Edge Case, Gap]
  - STRENGTHENED: New edge case added: "What happens when windows change visibility during focus traversal? The layout's focus methods capture the visible focusable window list at invocation time (within a lock scope), so visibility changes during traversal do not affect the current operation."
- [x] CHK038 - Is the behavior defined when CPR response contains extreme values (e.g., row=0, row=99999, col=0)? [Edge Case, Gap]
  - STRENGTHENED: New edge case added: "What happens when CPR response contains extreme row/column values (e.g., row=0, row=99999)? The parsed value is passed directly to `Renderer.ReportAbsoluteCursorRow()` without bounds checking, matching Python behavior."
- [x] CHK039 - Does the spec address whether focus functions should be no-ops or throw when `Layout` is null or not yet initialized? [Edge Case, Gap]
  - STRENGTHENED: New edge case added: "What happens when focus functions are called before the application is fully initialized? `GetApp()` throws `InvalidOperationException` if `KeyPressEvent.App` is null, which is the standard behavior for all binding handlers and cannot occur during normal key processing."

## Dependencies & Assumptions

- [x] CHK040 - Is the dependency on `Layout.FocusNext()`/`FocusPrevious()` (Feature 29) documented and validated as existing in the codebase? [Dependency, Spec §Dependencies]
  - STRENGTHENED: Dependencies section now names exact methods: "`Layout.FocusNext()` and `Layout.FocusPrevious()` methods for focus traversal." Assumptions section now includes validation: "**Validated**: `Layout.cs:379` and `Layout.cs:362`."
- [x] CHK041 - Is the dependency on `Renderer.ReportAbsoluteCursorRow()` (Feature 30) documented and validated as existing in the codebase? [Dependency, Spec §Dependencies]
  - STRENGTHENED: Dependencies section now names exact method: "`Renderer.ReportAbsoluteCursorRow(int row)` for CPR response reporting." Assumptions section includes: "**Validated**: `Renderer.cs:471`."
- [x] CHK042 - Is the dependency on `Keys.CPRResponse` enum constant (Feature 11) documented and validated as existing in the codebase? [Dependency, Spec §Dependencies]
  - STRENGTHENED: Dependencies section now names exact constant: "`Keys.CPRResponse` enum constant for CPR response binding registration." Assumptions section includes: "**Validated**: `Keys.cs:794`."
- [x] CHK043 - Is the dependency on the `saveBefore` parameter in `KeyBindings.Add` (Feature 22) documented? [Dependency, Spec §Dependencies]
  - STRENGTHENED: Dependencies section now names the parameter: "the `saveBefore` parameter on `KeyBindings.Add<T>()`." Assumptions section includes: "**Validated**: `KeyBindings.cs:81` and `Binding.cs:69`."
- [x] CHK044 - Is the assumption that Layout's focus methods handle zero-window and single-window cases internally validated against the actual implementation? [Assumption, Spec §Assumptions]
  - STRENGTHENED: Assumptions now include: "**Validated**: both methods exist, are thread-safe (use `_lock.EnterScope()`), and handle zero-window (no-op via `windows.Count > 0` guard) and single-window (wraps to same index) cases internally."
- [x] CHK045 - Is the assumption that `Renderer.ReportAbsoluteCursorRow()` is thread-safe validated against the actual implementation? [Assumption, Plan §Constitution Check XI]
  - STRENGTHENED: Assumptions now include: "**Validated**: `Renderer.cs:471` — method exists and is thread-safe (uses `_cprLock.EnterScope()`)."

## Notes

- All 45 items completed on 2026-01-31
- Items marked PASS required no spec changes (already adequate)
- Items marked STRENGTHENED had spec/plan/contracts improved to address the quality gap
- Strengthened areas: FR-001 through FR-008 (clarity + specificity), SC-001 through SC-005 (measurability), Edge Cases (4 new scenarios added), Assumptions (codebase validation evidence), Dependencies (exact method names), User Story 3 (explicit no-exception clause)
