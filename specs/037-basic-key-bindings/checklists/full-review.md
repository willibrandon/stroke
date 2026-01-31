# Full Review Checklist: Basic Key Bindings

**Purpose**: Comprehensive requirements quality validation across spec, plan, contracts, research, and data model
**Created**: 2026-01-31
**Feature**: [spec.md](../spec.md)

## Requirement Completeness

- [x] CHK001 - Are all binding groups from Python `basic.py` (lines 47-257) enumerated in the spec's functional requirements? [Completeness, Spec §FR-001 through §FR-018] — **Verified**: All 14 groups mapped to FR-001 through FR-018.
- [x] CHK002 - Is the exact count of ignored keys specified, and does it match the Python source's ~80 decorator applications (lines 47-136)? [Completeness, Spec §FR-002] — **Fixed**: Specified exact count of 90 with detailed subcategory breakdown.
- [x] CHK003 - Are the 16 named command references explicitly listed in requirements, and does this match the full set used in `basic.py`? [Completeness, Spec §FR-018, Data Model §Named Commands] — **Fixed**: All 16 commands enumerated in FR-018.
- [x] CHK004 - Is the return type of `LoadBasicBindings()` specified as `KeyBindings` (concrete class) rather than `IKeyBindingsBase` (interface), and is this consistent with the Python source? [Completeness, Spec §FR-001, Contract §Public API] — **Verified**: FR-001 already specifies `KeyBindings` (concrete), matching Python source.
- [x] CHK005 - Are requirements defined for the binding registration *order*, given that Python registers ignored keys first to establish priority over later `Any` bindings? [Gap] — **Fixed**: Added FR-019 specifying registration order and explaining priority semantics.
- [x] CHK006 - Does the spec address what happens when `NamedCommands.GetByName()` throws `KeyNotFoundException` for a missing command? [Gap, Spec §Assumptions] — **Fixed**: Added edge case documenting that KeyNotFoundException propagates as a programming error.

## Requirement Clarity

- [x] CHK007 - Is the `HasTextBeforeCursor` filter semantics unambiguous — does it check `buffer.Text.Length > 0` (any text in buffer) or `buffer.Document.TextBeforeCursor.Length > 0` (text before cursor specifically)? The Python source checks `bool(get_app().current_buffer.text)` which is any text in buffer, but the spec name says "before cursor". [Ambiguity, Spec §Key Entities] — **Fixed**: Key Entities now explicitly states `buffer.Text.Length > 0` and explains the name is retained for Python API fidelity despite checking full buffer text.
- [x] CHK008 - Is the filter composition `HasTextBeforeCursor & InsertMode` clearly specified as AND (both must be true), matching the Python `has_text_before_cursor & insert_mode`? [Clarity, Spec §FR-010] — **Fixed**: FR-010 now uses explicit filter expression `HasTextBeforeCursor & InsertMode` with parenthetical clarification.
- [x] CHK009 - Is the `eager: true` parameter on the quoted insert binding (FR-015) explained — what does eager matching mean and why is it required for quoted insert? [Clarity, Spec §FR-015] — **Fixed**: FR-015 now explains eager matching semantics and why it gives quoted insert priority over self-insert.
- [x] CHK010 - Is the namespace placement (`Stroke.Application.Bindings` vs `Stroke.KeyBinding.Bindings`) explicitly specified in the spec or only resolved in research? [Clarity, Research §R-007] — **Fixed**: Key Entities now specifies `Stroke.Application.Bindings` namespace with rationale and notes the features doc discrepancy is resolved.
- [x] CHK011 - Is the Ctrl+J re-dispatch behavior specified clearly enough — does the requirement explain that `KeyProcessor` must be cast from `object` to access `Feed()`? [Clarity, Spec §FR-012, Research §R-005] — **Fixed**: FR-012 now includes the `Feed()` call signature and implementation note about the `object` → `KeyProcessor` cast.

## Requirement Consistency

- [x] CHK012 - Is the spec's FR-002 ignored keys list consistent with the contract's Section 1 ignored keys enumeration? Both should list identical keys. [Consistency, Spec §FR-002, Contract §1] — **Fixed**: Both now specify exactly 90 bindings. Contract updated from "~80" to "90".
- [x] CHK013 - Does the spec's FR-004 editing bindings list match the contract's Section 3 exactly (7 bindings: Ctrl+K, Ctrl+U, Backspace, Delete, Ctrl+Delete, Ctrl+T, Ctrl+W)? [Consistency, Spec §FR-004, Contract §3] — **Verified**: Both list identical 7 bindings.
- [x] CHK014 - Is the plan's namespace (`Stroke.Application.Bindings`) consistent with the contract's namespace and the features doc (`59-basicbindings.md` says `Stroke.KeyBinding.Bindings`)? [Conflict, Plan §Project Structure, Contract §Public API, Research §R-007] — **Fixed**: Spec Key Entities now explicitly declares `Stroke.Application.Bindings` with rationale and notes the features doc discrepancy is resolved.
- [x] CHK015 - Are the filter compositions in the contract's Filter Compositions table consistent with those specified in the spec's functional requirements? [Consistency, Contract §Filter Compositions, Spec §FR-010/011/007/009/015] — **Fixed**: FR-010 and FR-011 now use explicit filter expressions matching the contract table. All 6 compositions verified consistent.
- [x] CHK016 - Is the `saveBefore` parameter usage consistent across all artifacts — spec FR-016, contract sections 3/4, and research R-009 all listing the same four bindings (Backspace, Delete, Ctrl+Delete, self-insert)? [Consistency] — **Verified**: All three artifacts list identical four bindings.

## Acceptance Criteria Quality

- [x] CHK017 - Are acceptance scenarios for User Story 1 (self-insert) measurable with specific buffer positions, and do they cover both Emacs and Vi insert modes? [Measurability, Spec §US-1] — **Verified**: Scenarios specify exact buffer contents and cursor positions for both Emacs (scenario 1) and Vi (scenarios 2-3) modes.
- [x] CHK018 - Are acceptance scenarios for User Story 3 (ignored keys) representative — do 3 examples adequately cover ~80 ignored key bindings, or should the criteria specify exhaustive coverage? [Measurability, Spec §US-3] — **Fixed**: Added note that unit tests MUST verify all 90 ignored keys exhaustively.
- [x] CHK019 - Is SC-007 ("80% test coverage") measurable against the implementation file specifically, or against the entire feature including test infrastructure? [Measurability, Spec §SC-007] — **Fixed**: SC-007 now specifies "80% line coverage for `BasicBindings.cs`" explicitly.
- [x] CHK020 - Does SC-008 ("integrates correctly with existing key processor infrastructure") have measurable acceptance criteria, or is "integrates correctly" too vague? [Ambiguity, Spec §SC-008] — **Fixed**: SC-008 now specifies concrete verification: addable to KeyProcessor via MergedKeyBindings, bindings discoverable, expected count of 118.

## Scenario Coverage

- [x] CHK021 - Are requirements defined for the interaction between ignored key bindings and readline bindings that share the same key (e.g., Home is both ignored AND bound to beginning-of-line)? Is the overriding behavior specified? [Coverage, Spec §FR-002/FR-003] — **Fixed**: Added edge case explaining that later-registered specific bindings override earlier ignored bindings, with FR-019 establishing the registration order.
- [x] CHK022 - Are requirements defined for what happens when Up/Down `AutoUp`/`AutoDown` are called with `event.Arg` > 1 (repetition count)? [Coverage, Spec §FR-008] — **Fixed**: FR-008 now specifies `count` set to `event.Arg`. Edge case added for Vi repetition count > 1.
- [x] CHK023 - Are requirements defined for the interaction between the quoted insert handler (Keys.Any with `InQuotedInsert` + eager) and the self-insert handler (Keys.Any with `InsertMode`)? Is binding priority/eager matching specified? [Coverage, Spec §FR-005/FR-015] — **Fixed**: Edge case added explaining eager wins over non-eager regardless of registration order. FR-015 already documents this.
- [x] CHK024 - Are requirements specified for the Delete key's behavior when *both* a selection is active AND the user is in insert mode? Which handler wins — FR-009 (cut selection) or FR-004 (delete-char)? [Coverage, Spec §FR-004/FR-009] — **Fixed**: Edge case added specifying FR-009 wins (later registration = higher priority), cutting the selection.

## Edge Case Coverage

- [x] CHK025 - Does the spec define behavior when `@event.CurrentBuffer` is null (possible per `KeyPressEvent` constructor)? All inline handlers access `CurrentBuffer!` with null-forgiving. [Edge Case, Gap] — **Fixed**: Edge case added explaining CurrentBuffer is always non-null during key dispatch; null-forgiving operator is safe.
- [x] CHK026 - Does the spec define behavior when `@event.App` is null and the Ctrl+J handler tries to cast `KeyProcessor` from `object`? [Edge Case, Gap] — **Fixed**: Edge case added explaining both KeyProcessor and App are always available during key dispatch.
- [x] CHK027 - Are edge cases documented for the bracketed paste handler when the paste data is empty string, or contains only `\r\n` sequences? [Edge Case, Spec §US-5] — **Fixed**: Edge case added covering empty string (no-op) and pure `\r\n` (normalized to `\n`).
- [x] CHK028 - Is the edge case defined for self-insert with non-printable characters that pass the `InsertMode` filter but have no useful `Data` property? [Edge Case, Spec §FR-005] — **Fixed**: Edge case added explaining ignored bindings catch control keys before self-insert; residual non-printables are inserted as-is.
- [x] CHK029 - Does the spec address the edge case where Ctrl+Z's `event.Data` produces a control character (ASCII 26) — is this the intended literal insertion behavior? [Edge Case, Spec §FR-013] — **Fixed**: Edge case added with explicit confirmation this is intentional, citing Python source documentation and the system bindings override mechanism.

## Cross-Artifact Consistency

- [x] CHK030 - Does the data model's binding count (~100 total across 14 groups) reconcile with the contract's enumerated bindings and the spec's 18 functional requirements? [Consistency, Data Model, Contract] — **Fixed**: Updated all artifacts to exact count of 118 bindings (90 ignored + 28 specific). Data model, contract, plan, and spec now use consistent counts.
- [x] CHK031 - Does the research decision R-001 (use `Add<Binding>` for named commands) align with the quickstart code examples, which show both `Add<Binding>` and `Add<KeyHandlerCallable>` patterns? [Consistency, Research §R-001, Quickstart] — **Verified**: Quickstart correctly demonstrates `Add<Binding>` for named commands and `Add<KeyHandlerCallable>` for inline handlers, matching R-001.
- [x] CHK032 - Is the features doc (`59-basicbindings.md`) reconciled with the plan — specifically the namespace conflict and return type (`KeyBindingsBase` in features doc vs `KeyBindings` in contract)? [Conflict, Plan, Features Doc] — **Fixed**: Spec Assumptions section now explicitly documents the features doc discrepancies (namespace and return type) as resolved during planning.

## Dependencies & Assumptions

- [x] CHK033 - Are all 5 assumptions in the spec validated against the current codebase (NamedCommands has all 16 commands, AppFilters/ViFilters/EmacsFilters exist, Buffer methods exist, KeyProcessor.Feed exists, Application.QuotedInsert exists)? [Assumption, Spec §Assumptions] — **Verified**: All 5 original assumptions validated against codebase during research phase. NamedCommands.GetByName confirmed at NamedCommands.cs:47.
- [x] CHK034 - Is the assumption that `AppContext.GetApp()` will not throw during filter evaluation documented, given that filters are called during key processing when an app must be active? [Assumption, Gap] — **Fixed**: Added to Assumptions: "During key processing, `AppContext.GetApp()` will not throw because filters are evaluated only when an active application is dispatching key events."
- [x] CHK035 - Is the dependency on `Filter.Invoke()` method for the Enter handler's paste mode check documented in the spec or plan? [Dependency, Research §R-010] — **Fixed**: Added to Assumptions: "The `Filter` base class provides an `Invoke()` method for runtime evaluation, used by the Enter handler to check `InPasteMode` dynamically."

## Notes

- Check items off as completed: `[x]`
- Items reference spec sections (§FR-XXX, §US-X, §SC-XXX), research decisions (§R-XXX), and cross-artifact markers ([Gap], [Ambiguity], [Conflict], [Assumption])
- This checklist validates requirements quality across: spec.md, plan.md, research.md, data-model.md, contracts/basic-bindings.md, quickstart.md
