# Plan Quality, Faithful Port & Implementation Readiness Checklist: Open in Editor Bindings

**Purpose**: Validate plan.md, contracts, and research against spec requirements; verify faithful alignment with Python source; confirm all prerequisites and design decisions are complete before implementation.
**Created**: 2026-01-31
**Feature**: [spec.md](../spec.md) | [plan.md](../plan.md) | [research.md](../research.md) | [contracts/](../contracts/)

## Plan Quality — Spec-to-Plan Traceability

- [x] CHK001 - Are all 14 functional requirements (FR-001 through FR-014) traceable to plan decisions or existing infrastructure references? [Completeness, Spec §FR-001–FR-014] — Added "Requirements Traceability" section to plan.md with full FR mapping table.
- [x] CHK002 - Does the plan explicitly account for FR-004 through FR-014 (named command, editor resolution, temp files, read-only guard, shell-split, auto-validate) by referencing existing implementations, or are they silently assumed? [Completeness, Spec §FR-004–FR-014] — Plan traceability table shows each FR-004–FR-014 as EXISTING with file:line references.
- [x] CHK003 - Are all 9 success criteria (SC-001 through SC-009) addressed by the plan's scope — either as new implementation items or as existing infrastructure? [Traceability, Spec §SC-001–SC-009] — Added SC traceability table to plan.md mapping each criterion to new tests or existing code.
- [x] CHK004 - Does the plan document which spec requirements are satisfied by *existing* code vs. *new* code to avoid redundant implementation? [Clarity, Plan §Summary] — Plan traceability table clearly marks 5 NEW vs. 9 EXISTING requirements with summary.
- [x] CHK005 - Are the 6 assumptions from the spec explicitly validated in the research phase with concrete file locations? [Completeness, Spec §Assumptions] — Added R7 to research.md validating all 6 assumptions with file paths and line numbers.

## Plan Quality — Contract Consistency

- [x] CHK006 - Does the contract's method signature for `LoadEmacsOpenInEditorBindings` match the api-mapping.md return type of `KeyBindings`? [Consistency, Contract §Class, api-mapping.md] — Verified: both contract and api-mapping.md specify `KeyBindings`.
- [x] CHK007 - Does the contract's method signature for `LoadViOpenInEditorBindings` match the api-mapping.md return type of `KeyBindings`? [Consistency, Contract §Class, api-mapping.md] — Verified: both specify `KeyBindings`.
- [x] CHK008 - Does the contract's method signature for `LoadOpenInEditorBindings` match the api-mapping.md return type of `IKeyBindingsBase`? [Consistency, Contract §Class, api-mapping.md] — Verified: both specify `IKeyBindingsBase`.
- [x] CHK009 - Is the namespace in the contract (`Stroke.Application.Bindings`) consistent with the api-mapping.md specification? [Consistency, Contract §Module, api-mapping.md] — Verified: api-mapping.md line 1373 specifies `Stroke.Application.Bindings`.
- [x] CHK010 - Does the contract's Emacs filter (`EmacsFilters.EmacsMode & AppFilters.HasSelection.Invert()`) faithfully represent the Python source's `emacs_mode & ~has_selection`? [Consistency, Contract §Emacs Binding] — Verified: `Invert()` = `~`, `&` operator = `&`. Exact semantic match.

## Plan Quality — Research Completeness

- [x] CHK011 - Does the research document resolve the filter application strategy (per-binding vs. ConditionalKeyBindings wrapper) with a clear decision and rationale? [Clarity, Research §R2] — Research R2 explicitly decides per-binding filter (matching Python source) over ConditionalKeyBindings wrapper.
- [x] CHK012 - Does the research document address whether the combined loader should wrap with `BufferHasFocus` (as PageNavigationBindings does) or not (as Python source does)? [Clarity, Research §R2] — Research R2 point 2 explicitly states: "Python combined loader uses `merge_key_bindings` directly — no outer `ConditionalKeyBindings`."
- [x] CHK013 - Is the decision to use `Add<Binding>` (not `Add<KeyHandlerCallable>`) justified by research showing `NamedCommands.GetByName` returns a `Binding`? [Clarity, Research §R5] — Research R5 confirms `GetByName` returns `Binding` with BasicBindings.cs example.
- [x] CHK014 - Does the research confirm that `Keys.ControlX` and `Keys.ControlE` enum values exist in the Stroke `Keys` enum? [Completeness, Research §R4] — Added R8 to research.md confirming `Keys.ControlE` (line 64) and `Keys.ControlX` (line 159) in `src/Stroke/Input/Keys.cs`.

## Faithful Port — Python Source Alignment

- [x] CHK015 - Does the plan's function count (3) match the Python source's `__all__` export list (3 functions)? [Completeness, Python §open_in_editor.py] — Verified: plan specifies 3 functions, Python `__all__` exports 3 functions.
- [x] CHK016 - Are the Python function names faithfully mapped to C# PascalCase equivalents following the `snake_case` → `PascalCase` convention? [Clarity, Spec §Key Entities] — Verified: `load_open_in_editor_bindings` → `LoadOpenInEditorBindings`, etc.
- [x] CHK017 - Does the Emacs binding's key sequence (`c-x`, `c-e`) map to the correct Stroke key enum values (`Keys.ControlX`, `Keys.ControlE`)? [Consistency, Python line 37] — Verified: `Keys.ControlX` (line 159) and `Keys.ControlE` (line 64) in Keys.cs.
- [x] CHK018 - Does the Vi binding use a character literal `'v'` (not a `Keys` enum value) matching the Python source's `"v"` string? [Consistency, Python line 49] — Verified: contract specifies `['v']` as character literal via `new KeyOrChar('v')`.
- [x] CHK019 - Does the combined loader use `MergedKeyBindings` as the equivalent of Python's `merge_key_bindings`? [Consistency, Python line 23] — Verified: contract specifies `MergedKeyBindings`, research R6 confirms mapping.
- [x] CHK020 - Is the Python source's import of `emacs_mode`, `has_selection`, `vi_navigation_mode` from `prompt_toolkit.filters` correctly mapped to Stroke's `EmacsFilters.EmacsMode`, `AppFilters.HasSelection`, `ViFilters.ViNavigationMode`? [Consistency, Python line 7] — Verified: all three filter mappings confirmed in EmacsFilters.cs, AppFilters.cs, ViFilters.cs.
- [x] CHK021 - Is the Python source's import of `get_by_name` from `.named_commands` correctly mapped to Stroke's `NamedCommands.GetByName`? [Consistency, Python line 10] — Verified: `NamedCommands.GetByName` in NamedCommands.cs.
- [x] CHK022 - Does the spec accurately reflect that User Story 4 (edit-and-execute-command) is already implemented, not new to this feature? [Clarity, Spec §User Story 4] — Fixed: added "(Already Implemented)" annotation to User Story 4 title, added note clarifying existing infrastructure, updated scope boundaries to separate new vs existing code.

## Faithful Port — Behavioral Fidelity

- [x] CHK023 - Does the existing `EditAndExecuteCommand` in `NamedCommands.Misc.cs` call `OpenInEditorAsync(validateAndHandle: true)` matching the Python source's `buff.open_in_editor(validate_and_handle=True)`? [Consistency, Spec §FR-004] — Verified: `NamedCommands.Misc.cs:118` calls `OpenInEditorAsync(validateAndHandle: true)`.
- [x] CHK024 - Does the existing `Buffer.OpenInEditorAsync` check $VISUAL before $EDITOR, matching the Python source's `_open_file_in_editor` resolution order? [Consistency, Spec §FR-005] — Verified: `Buffer.ExternalEditor.cs:188-192` checks `VISUAL` then `EDITOR`, matching Python lines 1623-1628.
- [x] CHK025 - Does the existing `Buffer.OpenInEditorAsync` strip trailing newline matching the Python source's `text = text[:-1]` behavior? [Consistency, Spec §FR-008] — Verified: `Buffer.ExternalEditor.cs:41-44` uses `text[..^1]`, matching Python's `text[:-1]`.
- [x] CHK026 - Does the existing `Buffer.OpenInEditorAsync` only read back content when exit code is 0, matching the Python source? [Consistency, Spec §FR-008] — Verified: `Buffer.ExternalEditor.cs:36` gates on `if (success)`, matching Python's `if success:` (line 1594).
- [x] CHK027 - Does the existing `Buffer.OpenInEditorAsync` call `ValidateAndHandle()` when `validateAndHandle: true`, matching the Python source's `validate_and_handle()` call? [Consistency, Spec §FR-013] — Verified: `Buffer.ExternalEditor.cs:57-59` calls `ValidateAndHandle()`, matching Python lines 1606-1607.
- [x] CHK028 - Does the existing `Buffer.OpenFileInEditor` try all fallback editors in order and catch exceptions to try the next, matching the Python source's `except Exception` pattern? [Consistency, Spec §FR-014] — Verified: `Buffer.ExternalEditor.cs:241-244` catches `Exception` (broader than Python's `OSError`, acceptable .NET adaptation since .NET throws various exception types for process failures).

## Implementation Readiness — Dependency Verification

- [x] CHK029 - Is `NamedCommands.GetByName("edit-and-execute-command")` confirmed to resolve successfully (command is registered in `NamedCommands.Misc.cs`)? [Completeness, Research §R1] — Verified: `RegisterInternal("edit-and-execute-command", EditAndExecuteCommand)` at NamedCommands.Misc.cs:22.
- [x] CHK030 - Is `EmacsFilters.EmacsMode` confirmed to exist with the correct semantics (returns true only in Emacs editing mode)? [Completeness, Research §R1] — Verified: `EmacsFilters.cs:18` — `new Condition(() => AppContext.GetApp().EditingMode == EditingMode.Emacs)`.
- [x] CHK031 - Is `ViFilters.ViNavigationMode` confirmed to exist with the correct semantics (returns true only in Vi navigation mode, not insert/visual/other)? [Completeness, Research §R1] — Verified: `ViFilters.cs:26` — checks Vi mode, not operator-pending, not digraph-wait, InputMode is Navigation.
- [x] CHK032 - Is `AppFilters.HasSelection` confirmed to exist with an `Invert()` method for negation? [Completeness, Research §R1] — Verified: `AppFilters.cs:25` defines `HasSelection`, `IFilter.Invert()` declared in `IFilter.cs:51`.
- [x] CHK033 - Is `MergedKeyBindings` confirmed to accept a `params IKeyBindingsBase[]` constructor? [Completeness, Research §R1] — Verified: `MergedKeyBindings.cs:42` — `public MergedKeyBindings(params IKeyBindingsBase[] registries)`.
- [x] CHK034 - Is the `KeyBindings.Add<Binding>` overload confirmed to accept a `filter:` parameter of type `FilterOrBool`? [Completeness, Research §R3] — Verified: `KeyBindings.cs:76` — `public Func<T, T> Add<T>(KeyOrChar[] keys, FilterOrBool filter = default, ...)`.

## Implementation Readiness — Design Decision Completeness

- [x] CHK035 - Is the decision documented for whether `LoadEmacsOpenInEditorBindings` returns `KeyBindings` (with per-binding filter) or `ConditionalKeyBindings` (with wrapper filter)? [Clarity, Research §R2] — Research R2 and R6 explicitly decide: returns `KeyBindings` with per-binding filter, not ConditionalKeyBindings wrapper.
- [x] CHK036 - Is the filter composition approach specified with enough precision to implement without ambiguity (operator vs. method chaining, `FilterOrBool` wrapping)? [Clarity, Research §R3] — Research R3 specifies: `EmacsFilters.EmacsMode` AND `AppFilters.HasSelection.Invert()` composed via operators, passed as `FilterOrBool`. Includes BasicBindings.cs example code.
- [x] CHK037 - Are the test categories and expected assertions specified in the quickstart, covering loader counts, key sequences, filter behavior, and handler identity? [Completeness, Quickstart §Step 2] — Quickstart Step 2 lists 10 test categories covering all areas.
- [x] CHK038 - Is the test environment setup pattern documented (e.g., `CreateEnvironment` helper with `AppContext.SetApp` scope) matching existing test conventions? [Completeness, Quickstart §Step 2] — Added `CreateEnvironment` helper with full code example to quickstart, following AutoSuggestBindingsTests convention.

## Edge Cases & Gaps

- [x] CHK039 - Does the spec address what happens if `NamedCommands.GetByName("edit-and-execute-command")` throws `KeyNotFoundException` at binding registration time? [Edge Case, Gap] — Added edge case to spec explaining that NamedCommands static constructor guarantees registration before any loader is called.
- [x] CHK040 - Are requirements clear on whether the combined loader produces exactly 2 bindings (via merge flattening) or 2 sub-registries (preserving structure)? [Clarity, Spec §SC-001] — Clarified SC-001 to specify: combined loader returns `IKeyBindingsBase` via `MergedKeyBindings` whose flattened `.Bindings` collection contains 2 bindings total.
- [x] CHK041 - Is the FR numbering gap (FR-009/FR-010 appearing after FR-014) acknowledged and non-confusing for implementers? [Clarity, Spec §Functional Requirements] — Reorganized FRs into two groups: "New Code" (FR-001–003, FR-009–010) and "Existing Infrastructure" (FR-004–008, FR-011–014), eliminating confusing ordering.
- [x] CHK042 - Does the spec differentiate which requirements are in-scope for this feature (FR-001 through FR-003, FR-009, FR-010) vs. already-implemented infrastructure (FR-004 through FR-008, FR-011 through FR-014)? [Clarity, Spec §Scope Boundaries] — Updated scope boundaries to separate "In Scope (New Code)" from "In Scope (Existing Infrastructure, Documented for Completeness)" with FR cross-references and feature annotations.

## Notes

- Check items off as completed: `[x]`
- Items CHK001–CHK014: Plan quality validation
- Items CHK015–CHK028: Faithful port verification against Python source
- Items CHK029–CHK038: Implementation readiness confirmation
- Items CHK039–CHK042: Edge cases and identified gaps
- The existing `requirements.md` checklist covers spec quality; this checklist covers plan/port/readiness quality
