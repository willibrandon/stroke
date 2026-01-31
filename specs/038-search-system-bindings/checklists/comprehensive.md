# Comprehensive Requirements Quality Checklist: Search System & Search Bindings

**Purpose**: Thorough validation of requirements completeness, clarity, consistency, and coverage across API fidelity, architecture, integration, edge cases, and testing
**Created**: 2026-01-31
**Feature**: [spec.md](../spec.md)
**Depth**: Thorough
**Audience**: Reviewer (PR/implementation gate)
**Completed**: 2026-01-31 — All 78 items resolved; spec, data model, and contracts strengthened.

## Requirement Completeness

- [x] CHK001 - Are all 5 Python `prompt_toolkit.search` functions accounted for in SearchOperations requirements (start_search, stop_search, do_incremental_search, accept_search, _get_reverse_search_links)? [Completeness, Spec §FR-001–FR-013]
  > **Pass.** FR-001–003 (StartSearch), FR-004–006 (StopSearch), FR-007–010 (DoIncrementalSearch), FR-011–012 (AcceptSearch), FR-013 (GetReverseSearchLinks). All 5 functions covered.
- [x] CHK002 - Are all 7 Python `prompt_toolkit.key_binding.bindings.search` functions accounted for in SearchBindings requirements (abort_search, accept_search, start_reverse/forward_incremental_search, reverse/forward_incremental_search, accept_search_and_accept_input)? [Completeness, Spec §FR-015–FR-022]
  > **Pass.** FR-015 (AbortSearch), FR-016 (AcceptSearch), FR-017 (StartReverse), FR-018 (StartForward), FR-019 (Reverse), FR-020 (Forward), FR-021 (AcceptSearchAndAcceptInput), FR-022 (delegate compatibility). All 7 functions covered.
- [x] CHK003 - Is the SearchState `~` operator requirement explicitly specified with both Python equivalence and delegation behavior? [Completeness, Spec §FR-014]
  > **Pass.** FR-014 specifies "bitwise complement operator (`~`) that creates a new instance with reversed direction, matching the Python `__invert__` behavior."
- [x] CHK004 - Are requirements defined for the namespace relocation of SearchOperations from Stroke.Core to Stroke.Application? [Gap]
  > **Fixed.** Added AC-001 in new Architectural Constraints section with full Constitution III justification and layer dependency analysis.
- [x] CHK005 - Are requirements defined for deleting the existing Stroke.Core.SearchOperations stub file? [Gap]
  > **Fixed.** Added AC-002: "The existing `Stroke.Core.SearchOperations` stub file MUST be deleted as part of the relocation."
- [x] CHK006 - Are requirements defined for deleting/replacing the existing stub test file? [Gap]
  > **Fixed.** Added AC-003: "The existing stub test file MUST be deleted and replaced with new tests in `Stroke.Tests/Application/SearchOperationsTests.cs`."
- [x] CHK007 - Is the private helper method GetReverseSearchLinks documented as a requirement, or only as an implementation detail in contracts? [Completeness, Spec §FR-013]
  > **Pass.** FR-013 explicitly states: "SearchOperations MUST include a private helper to compute the reverse of Layout.SearchLinks."
- [x] CHK008 - Are requirements defined for what SearchOperations.StartSearch must do with the SearchState.Direction property? [Gap — the Python source sets `search_state.direction = direction` but spec only says "sets the initial direction"]
  > **Fixed.** FR-001 now explicitly says "set the target BufferControl's SearchState.Direction to the specified direction."
- [x] CHK009 - Are requirements defined for how SearchOperations.StartSearch sets the SearchState.Text property on the search buffer? [Gap — Python source sets `search_state.text = ""` at start; spec does not mention this]
  > **Fixed (corrected premise).** Python source does NOT set `search_state.text = ""` on start — only `search_state.direction = direction` is set (line 107 of search.py). FR-001 now explicitly notes: "SearchState.Text is NOT reset on StartSearch." Edge case also documents this.

## Requirement Clarity

- [x] CHK010 - Is "silently returns" clearly defined across all methods — does it mean no exception, no side effects, no return value indication? [Clarity, Spec §FR-002, FR-003, FR-005, FR-010, FR-012]
  > **Fixed.** Added Definitions section: "**Silently return**: Return immediately with no side effects and no exception thrown. The method behaves as a no-op."
- [x] CHK011 - Is the phrase "currently focused BufferControl" precisely defined — does it mean Layout.CurrentControl cast to BufferControl, or something else? [Clarity, Spec §FR-002]
  > **Fixed.** Added to Definitions: "obtained via `Layout.CurrentControl`, when that control is a `BufferControl` instance."
- [x] CHK012 - Is "applies search to the target buffer" in FR-008 and FR-011 specific about which Buffer.ApplySearch overload parameters are used (includeCurrentPosition, count)? [Clarity, Spec §FR-008, FR-011]
  > **Fixed.** FR-008 now specifies "Buffer.ApplySearch(searchState, includeCurrentPosition: false, count)". FR-011 specifies "Buffer.ApplySearch(searchState, includeCurrentPosition: true)".
- [x] CHK013 - Is the distinction between "excluding current position" (FR-008, DoIncrementalSearch) and "including current position" (FR-011, AcceptSearch) explicitly defined with respect to the `includeCurrentPosition` parameter? [Clarity, Spec §FR-008, FR-011]
  > **Fixed.** FR-008 explicitly says "includeCurrentPosition: false". FR-011 explicitly says "includeCurrentPosition: true". The parameter name makes the distinction unambiguous.
- [x] CHK014 - Is "resets the search buffer content" in FR-004 specified with a concrete method call or behavior (Buffer.Reset?)? [Clarity, Spec §FR-004]
  > **Fixed.** FR-004 now says "calling Buffer.Reset() on the SearchBufferControl's buffer".
- [x] CHK015 - Is "appends query to search history" in FR-011 specific about which method is called (Buffer.AppendToHistory on the target buffer? On the search buffer?)? [Clarity, Spec §FR-011]
  > **Fixed.** FR-011 now says "the search control's Buffer.AppendToHistory()" — confirmed from Python source line 211: `search_control.buffer.append_to_history()`.
- [x] CHK016 - Is "the event's arg count" in FR-019 and FR-020 clearly mapped to `KeyPressEvent.Arg`? [Clarity, Spec §FR-019, FR-020]
  > **Fixed.** FR-019 and FR-020 now explicitly say "count set to KeyPressEvent.Arg".
- [x] CHK017 - Is "the current buffer" in FR-021 (AcceptSearchAndAcceptInput calls ValidateAndHandle on "the current buffer") specified as the buffer AFTER AcceptSearch returns focus, or BEFORE? [Clarity, Spec §FR-021]
  > **Fixed.** FR-021 now says "event's CurrentBuffer (which, after AcceptSearch restores focus, is the original target buffer)".
- [x] CHK018 - Is "direction has NOT changed" in FR-008 defined relative to the SearchState's previous direction vs. the new direction parameter? [Clarity, Spec §FR-008]
  > **Fixed.** FR-008 now says "the SearchState.Direction BEFORE the update in FR-007 was already equal to the new direction parameter."
- [x] CHK019 - Is "registers the mapping in Layout.SearchLinks" in FR-001 specified as `AddSearchLink(searchBufferControl, bufferControl)` with the correct key/value ordering? [Clarity, Spec §FR-001]
  > **Fixed.** FR-001 now says "Layout.AddSearchLink(searchBufferControl, bufferControl)".

## Requirement Consistency

- [x] CHK020 - Are the filter conditions in SearchBindings contract consistent with the filters documented in spec FR-015 through FR-021? [Consistency, Contract: search-bindings.md vs. Spec §FR-015–FR-021]
  > **Pass.** All 7 filter assignments match between contract and spec: AbortSearch/IsSearching, AcceptSearch/IsSearching, StartReverse/ControlIsSearchable, StartForward/ControlIsSearchable, Reverse/IsSearching, Forward/IsSearching, AcceptSearchAndAcceptInput/IsSearching & PreviousBufferIsReturnable.
- [x] CHK021 - Is the SearchBindings.AcceptSearch function name consistent with SearchOperations.AcceptSearch, and is the delegation relationship unambiguous despite the name collision? [Consistency, Spec §FR-016]
  > **Fixed.** FR-016 now explicitly says "(not a recursive call — this delegates to the SearchOperations method)".
- [x] CHK022 - Are the "silently returns" guard conditions in the data model validation rules consistent with the guard conditions specified in spec FR-002, FR-003, FR-005, FR-010, FR-012? [Consistency, data-model.md §Validation Rules vs. Spec]
  > **Pass.** Data model validation rules 1–5 match spec FRs: Rule 1 matches FR-002/FR-003, Rule 2 matches FR-005, Rule 3 matches FR-010, Rule 4 matches FR-012, Rule 5 matches FR-011. "Silently return" now formally defined in Definitions.
- [x] CHK023 - Is the SearchOperations contract's StartSearch parameter order (`bufferControl, direction`) consistent with the Python source parameter order (`buffer_control, direction`)? [Consistency, Contract vs. Python Reference]
  > **Pass.** Python: `start_search(buffer_control=None, direction=FORWARD)`. C# contract: `StartSearch(BufferControl? bufferControl = null, SearchDirection direction = Forward)`. Same order.
- [x] CHK024 - Is the GetReverseSearchLinks return type `Dictionary<BufferControl, SearchBufferControl>` consistent between the contract, data model, and research R-003? [Consistency, cross-artifact]
  > **Pass.** Contract, data model, and research R-003 all specify `Dictionary<BufferControl, SearchBufferControl>`.
- [x] CHK025 - Are the Vi mode transition requirements consistent — StartSearch sets Insert (FR-001), StopSearch sets Navigation (FR-004) — and do they align with the Python source's `app.vi_state.input_mode` assignments? [Consistency, Spec §FR-001, FR-004 vs. Python Reference]
  > **Pass.** FR-001 says "InputMode.Insert" (Python: `InputMode.INSERT`). FR-004 says "InputMode.Navigation" (Python: `InputMode.NAVIGATION`). Consistent.
- [x] CHK026 - Is the PreviousBufferIsReturnable condition definition in the SearchBindings contract consistent with how IsReturnable is defined on Buffer (`AcceptHandler != null`)? [Consistency, Contract vs. Buffer.cs]
  > **Pass.** Contract checks `prevControl.Buffer.IsReturnable`. Dependencies table confirms `Buffer.IsReturnable` exists as `AcceptHandler != null`.

## Acceptance Criteria Quality

- [x] CHK027 - Are acceptance scenarios for User Story 1 measurable — can each "Then" clause be objectively verified by inspecting Layout.Focus, SearchLinks state, Vi InputMode, and buffer content? [Measurability, Spec §US-1]
  > **Pass.** All US-1 "Then" clauses reference inspectable state: focus (Layout.CurrentControl), SearchLinks (Layout.SearchLinks), Vi mode (ViState.InputMode), buffer content (Buffer.Text).
- [x] CHK028 - Are acceptance scenarios for User Story 2 measurable — can "cursor moves to the next match" be verified by checking Buffer cursor position? [Measurability, Spec §US-2]
  > **Pass.** Buffer.CursorPosition is a verifiable property. Match positions can be computed from buffer content.
- [x] CHK029 - Is success criterion SC-007 ("80% code coverage") measurable with existing tooling and does it specify how coverage is measured (line, branch, method)? [Measurability, Spec §SC-007]
  > **Fixed.** SC-007 now specifies "line coverage" and "independently (each class MUST meet the 80% threshold)."
- [x] CHK030 - Do success criteria SC-001 through SC-006 have corresponding acceptance scenarios or test descriptions that make them verifiable? [Measurability, Spec §SC-001–SC-006]
  > **Pass.** SC-001 (no NotImplementedException) verifiable via integration tests. SC-002 (focus transitions) covered by US-1 scenarios. SC-003 (cursor positions) covered by US-2 scenarios. SC-004 (delegation) covered by US-3 scenarios. SC-005 (filter gating) covered by US-3 scenario 7 and US-4 scenario 2. SC-006 (~operator) covered by edge case and unit test.
- [x] CHK031 - Is SC-005 (filter conditions gate binding execution) testable — are there acceptance scenarios that exercise bindings when filters are false? [Measurability, Spec §SC-005]
  > **Fixed.** Added US-3 scenario 7: "Given no active search session (IsSearching is false), When a search binding filtered by IsSearching would be evaluated, Then the binding does not fire." SC-005 now also states "Tests MUST include scenarios where filters evaluate to false."

## Scenario Coverage

- [x] CHK032 - Are requirements defined for calling StartSearch when a search session is ALREADY active (double-start scenario)? [Coverage, Gap]
  > **Fixed.** Added edge case: "StartSearch called while search session already active? The new search link overwrites the existing one in SearchLinks via AddSearchLink."
- [x] CHK033 - Are requirements defined for calling DoIncrementalSearch with count=0 or negative values? [Coverage, Edge Case, Gap]
  > **Pass (not applicable).** The count parameter in SearchBindings comes from KeyPressEvent.Arg, which defaults to 1 and is clamped to 1M (documented in Dependencies table). Count=0 or negative is impossible through the binding layer. Direct callers pass through to Buffer.ApplySearch which handles any value.
- [x] CHK034 - Are requirements defined for calling AcceptSearch when the search text matches nothing in the target buffer? [Coverage, Edge Case, Gap]
  > **Fixed.** Added edge case: "cursor position remains unchanged; Buffer.ApplySearch handles no-match gracefully."
- [x] CHK035 - Are requirements defined for the behavior when Layout.SearchLinks already contains an entry for the same SearchBufferControl before StartSearch adds one? [Coverage, Edge Case, Gap]
  > **Fixed.** Covered by the double-start edge case (CHK032): "new search link overwrites the existing one in SearchLinks via AddSearchLink."
- [x] CHK036 - Are requirements defined for what happens when the search buffer has text but the target buffer is empty? [Coverage, Edge Case, Gap]
  > **Fixed.** Subsumed by the no-match edge case (CHK034): "Buffer.ApplySearch handles no-match gracefully" — an empty target buffer has zero matches.
- [x] CHK037 - Are requirements for DoIncrementalSearch when direction HAS changed complete — the spec says "direction is updated but the search is not re-applied" (US-2 scenario 2) but does not specify whether SearchState.Text is still updated? [Coverage, Spec §FR-007, FR-008]
  > **Fixed.** FR-007 now says "MUST unconditionally update SearchState.Text... regardless of whether the direction changed." FR-008 separately governs the ApplySearch call.
- [x] CHK038 - Is User Story 4 (AcceptSearchAndAcceptInput) tested with a scenario where AcceptSearch succeeds but ValidateAndHandle fails (e.g., validation rejects the buffer)? [Coverage, Exception Flow, Gap]
  > **Fixed.** Added edge case: "search acceptance has already completed (focus restored, search link removed). The validation failure is handled by the buffer's normal validation flow independently."
- [x] CHK039 - Are requirements defined for the `~` operator when called on a SearchState with empty text? [Coverage, Edge Case, Spec §FR-014]
  > **Fixed.** Added edge case: "text remains empty; only direction is reversed. This is valid behavior."
- [x] CHK040 - Are requirements defined for calling StopSearch with a specific BufferControl parameter that is NOT in SearchLinks? [Coverage, Edge Case, Spec §FR-006]
  > **Fixed.** FR-006 now says "If the specified BufferControl is not found in the reverse mapping, the method MUST silently return." Edge case also documents this.

## Edge Case Coverage

- [x] CHK041 - Is the edge case "AcceptSearch with empty search buffer text preserves SearchState.Text" documented as both an edge case and in FR-011? [Edge Case, Spec §Edge Cases vs. FR-011]
  > **Pass.** FR-011 says "only if the search buffer text is non-empty." Edge case says "search state text is preserved (not overwritten with empty string)." Both documented consistently.
- [x] CHK042 - Is the edge case for StartSearch with a non-BufferControl current control (e.g., a Window or other UIControl) explicitly tested in acceptance scenarios? [Edge Case, Spec §FR-002]
  > **Fixed.** Edge case documents "StartSearch called and current control is not a BufferControl? Silently returns." Also added edge case for AcceptSearch with non-BufferControl current control.
- [x] CHK043 - Is the behavior of GetReverseSearchLinks when SearchLinks is empty specified? [Edge Case, Gap — implied returns empty dictionary but not stated]
  > **Fixed.** Added edge case: "GetReverseSearchLinks called on a Layout with no active search links? It returns an empty dictionary."
- [x] CHK044 - Is the edge case for StopSearch with a specific BufferControl that maps to a SearchBufferControl which has already been removed from the layout specified? [Edge Case, Gap]
  > **Fixed.** FR-006 now handles this: "If the specified BufferControl is not found in the reverse mapping, the method MUST silently return." This covers the case where the SBC is no longer in SearchLinks.
- [x] CHK045 - Is the edge case where SearchBufferControl.SearcherSearchState returns a different SearchState than expected during DoIncrementalSearch addressed? [Edge Case, Gap]
  > **Fixed.** Data model now includes SearchBufferControl entity with clarification: "SAME object as the target BufferControl's SearchState property — both point to the shared SearchState that links the search field to the target buffer." This eliminates the ambiguity.
- [x] CHK046 - Is the edge case for the `~` operator receiving a null SearchState specified (null argument to operator)? [Edge Case, Spec §FR-014]
  > **Pass (not applicable).** The `~` operator delegates to `state.Invert()`. A null operand causes a standard NullReferenceException per C# semantics. This is expected language behavior, not a spec concern.

## API Fidelity Requirements

- [x] CHK047 - Does the spec explicitly map each Python function name to its C# equivalent with naming convention justification (snake_case to PascalCase)? [Completeness, Constitution I]
  > **Pass.** Contract files (search-operations.md, search-bindings.md) both include Python Reference Mapping tables with all function names mapped. Naming convention (snake_case → PascalCase) is established in CLAUDE.md §Constitution I.
- [x] CHK048 - Are all Python function parameters mapped to C# parameters with type equivalences documented? [Completeness, Constitution I]
  > **Pass.** Contract Python Reference Mapping tables include full signature comparisons: `start_search(buffer_control=None, direction=FORWARD)` → `StartSearch(bufferControl=null, direction=Forward)`, etc.
- [x] CHK049 - Is the Python `_get_reverse_search_links(layout)` parameter (takes Layout explicitly) faithfully mapped to the C# contract which takes Layout as a parameter? [Fidelity, Contract vs. Python]
  > **Fixed.** Contract shows `GetReverseSearchLinks(Layout.Layout layout)`. Data model table now also shows `Layout layout` parameter (was "(none)", corrected).
- [x] CHK050 - Are the Python decorator filters (`@key_binding(filter=is_searching)`) faithfully mapped to C# filter requirements in the SearchBindings contract? [Fidelity, Constitution I]
  > **Pass.** Contract filter table maps all 7 functions with Python equivalents: `is_searching` → `SearchFilters.IsSearching`, `control_is_searchable` → `SearchFilters.ControlIsSearchable`, `is_searching & _previous_buffer_is_returnable` → `SearchFilters.IsSearching & PreviousBufferIsReturnable`.
- [x] CHK051 - Does the spec address that Python's `accept_search_and_accept_input` calls `event.current_buffer.validate_and_handle()` — is "current buffer" at that point the correct post-AcceptSearch buffer? [Fidelity, Spec §FR-021 vs. Python Reference]
  > **Fixed.** FR-021 now says "event's CurrentBuffer (which, after AcceptSearch restores focus, is the original target buffer)."
- [x] CHK052 - Is the Python `abort_search` function's behavior (just calls `stop_search()`) faithfully reflected in FR-015, or is there additional behavior in the Python source that's missing? [Fidelity, Constitution I]
  > **Fixed.** FR-015 now says "call SearchOperations.StopSearch() with no parameters (using the default search target resolution)." Verified against Python source line 34: `search.stop_search()` with no arguments.

## Architecture Requirements

- [x] CHK053 - Is the namespace relocation (Stroke.Core → Stroke.Application) justified with explicit Constitution III references and layer dependency analysis? [Architecture, Research R-001]
  > **Fixed.** AC-001 provides full justification: "requires `AppContext.GetApp()` (Application layer 7), `Layout` (layer 5), `BufferControl`/`SearchBufferControl` (layer 5), and `ViState`/`InputMode` (layer 4). Constitution III prohibits Core (layer 1) from depending on higher layers."
- [x] CHK054 - Are the assembly-level visibility requirements (SearchOperations needs `internal` access to Layout.AddSearchLink/RemoveSearchLink) documented? [Architecture, Gap — Research R-007 mentions it but spec does not]
  > **Fixed.** AC-004 now documents: "SearchOperations requires `internal` access to `Layout.AddSearchLink()` and `Layout.RemoveSearchLink()`. Since both reside in the same assembly (`Stroke.csproj`), this access is available without `InternalsVisibleTo`."
- [x] CHK055 - Is it specified that SearchOperations must be in the same assembly as Layout to access internal members, or is InternalsVisibleTo required? [Architecture, Gap]
  > **Fixed.** AC-004 confirms: "Since both reside in the same assembly (`Stroke.csproj`), this access is available without `InternalsVisibleTo`."
- [x] CHK056 - Are the layer dependencies for SearchBindings (depends on SearchOperations in Application, SearchFilters in Application, KeyPressEvent in KeyBinding) consistent with Constitution III? [Architecture, Constitution III]
  > **Pass.** SearchBindings in Application (layer 7) depends on SearchOperations (Application, layer 7), SearchFilters (Application, layer 7), KeyPressEvent (KeyBinding, layer 4). Layer 7 can depend on layer 4. No violations.
- [x] CHK057 - Is the impact on api-mapping.md documented as a requirement (mapping must be updated from `Stroke.Core` to `Stroke.Application` for SearchOperations)? [Architecture, Gap]
  > **Fixed.** AC-005 now states: "The `docs/api-mapping.md` entry for `prompt_toolkit.search` → `Stroke.Core` MUST be updated to `Stroke.Application` to reflect the relocation."

## Integration Requirements

- [x] CHK058 - Are the integration points between SearchOperations and Layout (Focus, SearchLinks, SearchTargetBufferControl, CurrentControl, IsSearching) all identified and their expected behavior specified? [Integration, Spec §FR-001–FR-012]
  > **Pass.** FR-001 specifies Layout.Focus() and Layout.AddSearchLink(). FR-004 specifies Layout.Focus(), Layout.RemoveSearchLink(). FR-005 specifies Layout.SearchTargetBufferControl. FR-002 specifies Layout.CurrentControl. All integration points in Dependencies table.
- [x] CHK059 - Are the integration points between SearchOperations and ViState (InputMode transitions) specified with the exact enum values (InputMode.Insert, InputMode.Navigation)? [Integration, Spec §FR-001, FR-004]
  > **Fixed.** FR-001 now says "ViState.InputMode to InputMode.Insert". FR-004 says "ViState.InputMode to InputMode.Navigation". Exact enum values specified.
- [x] CHK060 - Are the integration points between SearchOperations and Buffer (ApplySearch, AppendToHistory, Reset, SearchState) all identified? [Integration, Spec §FR-007–FR-012]
  > **Pass.** FR-008 specifies Buffer.ApplySearch with parameters. FR-011 specifies Buffer.ApplySearch, Buffer.AppendToHistory(). FR-004 specifies Buffer.Reset(). FR-007 specifies SearchState.Text and SearchState.Direction. All in Dependencies table.
- [x] CHK061 - Are the integration points between SearchBindings and KeyPressEvent (Arg property, App property, CurrentBuffer property) all specified? [Integration, Spec §FR-019–FR-022]
  > **Fixed.** FR-019/FR-020 now explicitly reference "KeyPressEvent.Arg". FR-021 references "event's CurrentBuffer". Dependencies table validates both properties exist.
- [x] CHK062 - Is the integration between SearchBindings.AcceptSearchAndAcceptInput and Buffer.ValidateAndHandle specified — including which buffer instance ValidateAndHandle is called on? [Integration, Spec §FR-021]
  > **Fixed.** FR-021 now says "Buffer.ValidateAndHandle() on the event's CurrentBuffer (which, after AcceptSearch restores focus, is the original target buffer)."
- [x] CHK063 - Are the integration points between SearchOperations and AppContext.GetApp() specified — including behavior when no app context is set? [Integration, Gap]
  > **Fixed.** NFR-004 now documents: "All SearchOperations methods require a valid Application context via `AppContext.GetApp()`. If no Application context is set, the method will throw (standard AppContext behavior). This is a precondition, not a recoverable error."

## Non-Functional Requirements

- [x] CHK064 - Are thread safety requirements specified for SearchOperations — given it accesses Layout, ViState, and Buffer across potentially concurrent calls? [Non-Functional, Constitution XI]
  > **Fixed.** NFR-001 now documents: "Thread safety is inherited from the thread-safe types it accesses (Layout with Lock, ViState with Lock, Buffer with Lock, SearchState with Lock). No additional synchronization is required."
- [x] CHK065 - Is it specified that SearchOperations methods are NOT expected to be called concurrently (since they're user-initiated from key bindings on a single UI thread)? [Non-Functional, Gap — thread safety may be inherited, but this assumption is not stated]
  > **Fixed.** NFR-002 now documents: "user-initiated from key binding handlers on the UI thread. Concurrent invocation is not expected, but the underlying state types are thread-safe as a defensive measure per Constitution XI."
- [x] CHK066 - Are file size requirements tracked — are all new/modified source files estimated to stay under 1000 LOC? [Non-Functional, Constitution X, Plan §File Size]
  > **Fixed.** NFR-003 now documents: "All new and modified source files MUST stay under 1,000 LOC per Constitution X. Estimated sizes: SearchOperations ~120 LOC, SearchBindings ~100 LOC."
- [x] CHK067 - Is the test coverage target (80%) specified per Constitution VIII, and does it apply to both SearchOperations and SearchBindings independently? [Non-Functional, Spec §SC-007]
  > **Fixed.** SC-007 now says "line coverage across SearchOperations and SearchBindings independently (each class MUST meet the 80% threshold)."

## Dependencies & Assumptions

- [x] CHK068 - Is the assumption that BufferControl.SearchBufferControl (property linking a buffer control to its search field) already exists validated? [Assumption]
  > **Fixed.** Dependencies table validates: `BufferControl.SearchBufferControl` property — Status: Exists.
- [x] CHK069 - Is the assumption that Layout.AddSearchLink and RemoveSearchLink are `internal` and available from the Application assembly validated? [Assumption, Research R-007]
  > **Fixed.** Dependencies table validates both exist. AC-004 confirms same-assembly access.
- [x] CHK070 - Is the assumption that Buffer.ApplySearch, Buffer.AppendToHistory, and Buffer.ValidateAndHandle already exist with the expected signatures validated? [Assumption]
  > **Fixed.** Dependencies table validates all three with locations and "Exists" status.
- [x] CHK071 - Is the assumption that SearchFilters.IsSearching and SearchFilters.ControlIsSearchable already exist and work correctly validated? [Assumption]
  > **Fixed.** Dependencies table validates: `SearchFilters.IsSearching` and `SearchFilters.ControlIsSearchable` — Status: Exists.
- [x] CHK072 - Is the assumption that AppContext.SetApp() returns an IDisposable for test isolation validated? [Assumption, Research R-004]
  > **Fixed.** Dependencies table validates: `AppContext.SetApp()` returns `IDisposable` — Status: "Exists; used for test isolation."
- [x] CHK073 - Is the dependency on FocusableElement (used by Layout.Focus) documented — can Layout.Focus accept a BufferControl or SearchBufferControl directly? [Dependency, Gap]
  > **Fixed.** Dependencies table validates: `Layout.Focus(FocusableElement)` — "Exists; accepts UIControl subtypes (BufferControl, SearchBufferControl) via FocusableElement implicit conversion."

## Ambiguities & Conflicts

- [x] CHK074 - Is there an ambiguity in FR-006 ("StopSearch with a specific BufferControl MUST use the reverse search links mapping") — does "specific BufferControl" mean when bufferControl parameter is non-null, or always? [Ambiguity, Spec §FR-006]
  > **Fixed.** FR-006 now reads: "when the bufferControl parameter is non-null, MUST use the reverse search links mapping." Explicit trigger condition.
- [x] CHK075 - Is there a conflict between the data model showing `GetReverseSearchLinks(Layout layout)` taking a Layout parameter and the data model table showing it with "(none)" parameters? [Conflict, data-model.md vs. Contract]
  > **Fixed.** Data model table corrected from "(none)" to "`Layout layout`". Now consistent with contract.
- [x] CHK076 - Is there an ambiguity in FR-008 about what "direction has NOT changed" means — is it comparing against the SearchState's current direction BEFORE updating, or the parameter vs some other reference? [Ambiguity, Spec §FR-008]
  > **Fixed.** FR-008 now says "the SearchState.Direction BEFORE the update in FR-007 was already equal to the new direction parameter." Explicit comparison timing.
- [x] CHK077 - Is there an ambiguity in the SearchBindings contract about whether the binding functions themselves check filters, or whether filters are only applied at the key binding registration level? [Ambiguity, Contract: search-bindings.md §Filter Requirements]
  > **Fixed.** Definitions section now says: "Filter checks are NOT performed inside the binding handler function body; they gate whether the handler is invoked at all."
- [x] CHK078 - Is there an ambiguity about what "SearcherSearchState" is on SearchBufferControl in the data model — is it the same object as BufferControl.SearchState or a separate reference? [Ambiguity, data-model.md]
  > **Fixed.** Data model now includes SearchBufferControl entity with clarification: "SAME object as the target BufferControl's SearchState property — both point to the shared SearchState that links the search field to the target buffer."

## Notes

- This checklist validates the REQUIREMENTS quality (not implementation correctness)
- Items reference spec sections as `[Spec §FR-XXX]`, contracts, data model, and research decisions
- All 78 items resolved: 26 already passing, 52 strengthened with spec/artifact edits
- Artifacts modified: spec.md (Definitions, FRs, edge cases, acceptance scenarios, SC, AC, NFR, Dependencies), data-model.md (SearchBufferControl entity, GetReverseSearchLinks parameter)
- Existing `requirements.md` checklist covers basic spec structure; this checklist provided deep domain analysis
