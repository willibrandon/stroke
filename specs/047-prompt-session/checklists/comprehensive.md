# Comprehensive Requirements Quality Checklist: Prompt Session

**Purpose**: Validate completeness, clarity, consistency, measurability, and coverage of all requirements across spec.md, plan.md, contracts/, research.md, and data-model.md for Feature 047 — Prompt Session.
**Created**: 2026-02-01
**Completed**: 2026-02-01
**Feature**: [spec.md](../spec.md)
**Depth**: Thorough (~57 items)
**Focus**: API fidelity, composition/integration, edge cases, thread safety, non-functional requirements

## Requirement Completeness

- [x] CHK001 Are all public APIs from Python `prompt_toolkit.shortcuts.prompt` (module-level functions, `CompleteStyle`, `PromptSession` class) accounted for in the spec and contracts? — **YES**: `__all__` exports (PromptSession, prompt, confirm, create_confirm_session, CompleteStyle) all covered by FR-001, FR-002, FR-019..FR-021 and contracts.
- [x] CHK002 Are requirements defined for all 40+ constructor parameters listed in the Python source (`__init__` lines 378-484), including their default values? — **FIXED**: FR-003 updated to enumerate all 44 parameters explicitly with their names. Default values documented in data-model.md and contract.
- [x] CHK003 Are requirements documented for every private/internal method in the Python `PromptSession` class (`_dyncond`, `_create_default_buffer`, `_create_search_buffer`, `_create_layout`, `_create_application`, `_create_prompt_bindings`, `_dumb_prompt`, `_add_pre_run_callables`, helper methods)? — **YES**: All 14 methods documented in contract §prompt-session.md with Python→C# naming table.
- [x] CHK004 Are requirements specified for all three `Prompt` static methods (`Prompt`, `Confirm`, `CreateConfirmSession`) and their async variants? — **YES**: Contract §prompt-functions.md specifies Prompt, PromptAsync, CreateConfirmSession, Confirm, ConfirmAsync (5 methods).
- [x] CHK005 Is the `PromptContinuationCallable` delegate fully specified with parameter names, types, and return type? — **YES**: Contract §internal-helpers.md has full delegate definition with `promptWidth`, `lineNumber`, `wrapCount` parameters.
- [x] CHK006 Are requirements defined for all computed properties (`EditingMode`, `Input`, `Output`) and their delegation to `Application`? — **YES**: FR-022 (Input, Output) and FR-023 (EditingMode) cover delegation. Contract and data-model both document these.
- [x] CHK007 Are requirements specified for the `EraseWhenDone` parameter and its effect on terminal cleanup after prompt submission? — **YES**: FR-024 specifies eraseWhenDone. Data-model note clarifies it's passed to CreateApplication, not stored as mutable property.
- [x] CHK008 Are requirements defined for the `InputHook` parameter in `Prompt()` (present in Python's `run()` but only for synchronous mode)? — **FIXED**: Added FR-036 specifying InputHook behavior (sync Prompt only, passed through to Application.Run()).

## Requirement Clarity

- [x] CHK009 Is the behavior of `FilterOrBool` parameters with default `true` (e.g., `wrapLines`, `completeWhileTyping`, `validateWhileTyping`, `includeDefaultPygmentsStyle`) clearly specified, given that `default(FilterOrBool)` is falsy in C#? — **FIXED**: Added Edge Case 7 documenting the sentinel-detection pattern using `FilterOrBool.HasValue`.
- [x] CHK010 Is the `DynCond` factory method clearly specified — how it creates `Condition` lambdas that capture the session and read Lock-protected properties at render time? — **FIXED**: FR-016 updated with explicit Lock-protected read and ToFilter() resolution details.
- [x] CHK011 Is the behavior of per-prompt overrides clearly defined — that non-null values update session state permanently (current + future calls), not just the current call? — **YES**: FR-010 explicitly states "updates the session attribute for current and future calls". User Story 6 reinforces with acceptance scenarios.
- [x] CHK012 Is "dumb terminal mode" clearly defined — specifically what "no explicit output" means vs. having an explicit `IOutput` provided? — **FIXED**: FR-014 updated with explicit conditions: `_output` is null AND `IsDumbTerminal()` is true. Clarifies that explicit IOutput overrides detection.
- [x] CHK013 Is the `default_` parameter clearly specified for both `string` and `Document` input forms, including cursor position behavior for `Document`? — **FIXED**: Edge Case 2 updated to specify that string wraps in `new Document(default_)` with cursor at position 0, while Document preserves its cursor position.
- [x] CHK014 Is the term "merged key bindings" quantified — exactly which binding sources are merged and in what order of priority? — **FIXED**: FR-007 updated with the exact merge structure: inner merge [auto-suggest, conditional open-in-editor, prompt-specific], outer merge with [DynamicKeyBindings(user)]. User bindings have highest priority (last in merge).
- [x] CHK015 Is the behavior of `completeWhileTyping` being "automatically disabled" when `enableHistorySearch` is true or `completeStyle` is `ReadlineLike` clearly specified as a runtime condition vs. a constructor override? — **FIXED**: FR-018 updated to specify it's a runtime `Condition` lambda, not a constructor-time override.
- [x] CHK016 Is the `TempfileSuffix` / `Tempfile` union type (`string | Func<string>`) clearly defined with runtime type checking behavior? — **YES**: Research §R6 defines the inline pattern. Contract shows `object?` type with comment. Edge Cases don't need additional specification.

## Requirement Consistency

- [x] CHK017 Do the constructor parameter names in `prompt-session.md` contract consistently match the property names in the data model? — **YES**: Cross-checked all parameter names between contract and data-model; they match consistently (PascalCase properties, camelCase params).
- [x] CHK018 Are the default values in the data-model.md consistent with those in the contract's constructor signature and the Python source? — **YES**: Verified all defaults across data-model, contract, and Python source lines 378-424. All consistent.
- [x] CHK019 Is the `History` property listed as both mutable (data-model.md "Lock-Protected") and immutable ("Set in constructor, shared with DefaultBuffer" in contract)? Is this inconsistency resolved? — **FIXED**: Moved History from "State Fields (Mutable, Lock-Protected)" to "Owned Objects" in data-model.md. History is set once in constructor and NOT updated per-prompt (not in Python's `_fields` tuple, not in `prompt()` override logic).
- [x] CHK020 Do the `Prompt.Prompt()` parameters match the `PromptSession.Prompt()` parameters exactly (minus `history`, `input`, `output`, `interrupt/eof`)? — **YES**: Cross-checked. Prompt.Prompt passes `history` to session constructor and all other params to `session.Prompt()`. Parameter lists match.
- [x] CHK021 Are the 7 source file names in `plan.md` Project Structure consistent with the partial class responsibilities described in the contracts? — **YES**: 8 source files (CompleteStyle.cs, PromptSession.cs, .Layout.cs, .Buffers.cs, .Application.cs, .Prompt.cs, .Helpers.cs, Prompt.cs) all map to documented responsibilities.
- [x] CHK022 Is the assumption that "KeyboardInterruptException and EOFException are already defined" (Spec §Assumptions) consistent with the research finding that they do NOT exist and must be created? — **FIXED**: Updated spec assumption to state they "do NOT exist yet and MUST be created as part of this feature" with references to Research §R2 and Contract §internal-helpers.md.

## Acceptance Criteria Quality

- [x] CHK023 Can SC-001 ("under 5 lines of code") be objectively measured against the quickstart examples? — **YES**: Quickstart shows 2-line one-shot prompt and 1-line session creation + 1-line Prompt call. Both under 5 lines.
- [x] CHK024 Is SC-002 ("1000+ prompt calls without degradation") defined with specific degradation metrics (memory growth, latency increase, etc.)? — **FIXED**: SC-002 updated with specific metrics: no unbounded memory growth, no latency increase per-prompt, history recall remains responsive. Also added note about PreRunCallables not accumulating.
- [x] CHK025 Is SC-004 ("<16ms keystroke response") measurable in the test environment, and are the measurement conditions specified (terminal type, host load, etc.)? — **FIXED**: SC-004 updated with measurement conditions: synchronous completer, standard terminal, no artificially slow validators/processors.
- [x] CHK026 Is SC-009 ("1:1 fidelity to Python") verifiable — is there a concrete comparison methodology (e.g., side-by-side API listing, behavior comparison matrix)? — **FIXED**: SC-009 updated with verification method: every public API in Python's `__all__` exports has a corresponding C# API with matching semantics.
- [x] CHK027 Does SC-008 ("80% test coverage") specify which tool measures coverage and whether it's line, branch, or method coverage? — **FIXED**: SC-008 updated to specify line coverage measured by `dotnet test --collect:"XPlat Code Coverage"`.

## Scenario Coverage — Primary Flows

- [x] CHK028 Are requirements specified for the full prompt lifecycle: constructor -> Prompt() -> Application.Run() -> user input -> Enter -> result returned? — **YES**: FR-002 (constructor), FR-004 (buffer accept handler exits App with result), FR-007 (Application creation), FR-008/FR-009 (Prompt/PromptAsync), FR-011 (Enter binding). Data-model state transitions document the flow.
- [x] CHK029 Are requirements for session reuse explicitly defined — what state resets between Prompt() calls (buffer, completion) vs. what persists (history, session properties)? — **FIXED**: Data-model state transitions updated with explicit "State that RESETS" and "State that PERSISTS" sections. Added FR-038 for buffer reset behavior.
- [x] CHK030 Are requirements specified for all three `CompleteStyle` rendering paths, including the conditional visibility of `CompletionsMenu` vs. `MultiColumnCompletionsMenu` vs. `DisplayCompletionsLikeReadline`? — **YES**: FR-001 (three values), FR-017 (reserveSpaceForMenu), FR-018 (runtime condition). Layout contract details the Float visibility conditions based on `multi_column_complete_style` Condition.

## Scenario Coverage — Alternate Flows

- [x] CHK031 Are requirements defined for the `viMode` convenience parameter — that when `true` it overrides `editingMode` to `EditingMode.Vi`? — **YES**: Edge Case 1 specifies viMode precedence. Also documented in contract constructor notes.
- [x] CHK032 Are requirements specified for `acceptDefault` behavior — how the default value is submitted without user interaction, and whether pre-run callbacks still execute? — **FIXED**: FR-035 updated to specify that pre_run executes first, then acceptDefault schedules `ValidateAndHandle()` via `CallSoon` so the default displays before auto-submission.
- [x] CHK033 Are requirements defined for all confirmation prompt key binding behaviors: y/Y -> true, n/N -> false, Keys.Any -> no-op? — **YES**: FR-020, FR-021, User Story 5 scenarios 1-3, and contract §prompt-functions.md CreateConfirmSession all specify the four binding groups.

## Scenario Coverage — Exception/Error Flows

- [x] CHK034 Are requirements defined for `Ctrl-C` behavior: which exception type is thrown, how it's configurable via `interruptException`, and how `handleSigint` modifies this behavior? — **FIXED**: FR-027 updated with explicit details: Activator.CreateInstance for exception creation, handleSigint passed to Application.Run().
- [x] CHK035 Are requirements specified for `Ctrl-D` behavior: only triggers EOF on empty buffer, does nothing with text present? — **YES**: FR-011 specifies "Ctrl-D (EOF on empty buffer)". User Story 1 Scenarios 4 and 5 explicitly cover both cases.
- [x] CHK036 Are error scenarios defined for invalid `interruptException` / `eofException` types (e.g., types without parameterless constructors, non-Exception types)? — **FIXED**: Added FR-037 specifying constructor-time validation: types must be concrete, assignable to Exception, with parameterless constructor. Updated data-model validation rules.
- [x] CHK037 Are requirements specified for what happens when `Activator.CreateInstance` fails for custom exception types? — **FIXED**: FR-037 mandates constructor-time validation, so Activator.CreateInstance failure at Ctrl-C/D time is prevented. Data-model validation rules updated with specific check requirements.

## Scenario Coverage — Recovery/State Management

- [x] CHK038 Are buffer reset requirements defined between successive `Prompt()` calls — specifically that `DefaultBuffer.Reset()` is called with the new `default_` document? — **FIXED**: Added FR-038 specifying buffer reset with Document wrapping. Data-model state transitions also document this.
- [x] CHK039 Are requirements defined for what happens when the prompt Application exits via `App.Exit()` with an exception — how does that propagate through `Prompt()`/`PromptAsync()`? — **FIXED**: Added FR-039 specifying exception propagation from Application.Run()/RunAsync() through Prompt()/PromptAsync().
- [x] CHK040 Are `PreRunCallables` list management requirements specified — particularly that callables are cleared or managed between successive `Prompt()` calls to avoid accumulation? — **FIXED**: Added FR-040 specifying that Application.RunAsync() consumes and clears pre-run callables. Data-model state transitions updated with note about non-accumulation. SC-002 references this.

## Edge Case Coverage

- [x] CHK041 Are requirements defined for `reserveSpaceForMenu = 0` — the spec mentions "no space is reserved" but does not define the resulting layout behavior (jumps? overflow?). — **FIXED**: Edge Case 4 updated to specify that `GetDefaultBufferControlHeight()` returns `Dimension()` (no min constraint) when space is 0, allowing dynamic layout expansion when completions appear.
- [x] CHK042 Are requirements specified for empty prompt message (`message = ""` or `default`) — is the prompt correctly rendered with no prefix? — **YES**: User Story 1 Scenario 2 covers "no message" case. Default value is `""` in constructor.
- [x] CHK043 Are requirements defined for multiline prompt messages containing only newlines (e.g., `"\n\n"`) — what appears in "before" vs. "firstInputLine"? — **FIXED**: Added Edge Case 8 specifying SplitMultilinePrompt behavior for newline-only messages.
- [x] CHK044 Are requirements specified for `PromptContinuation` when the prompt is NOT multiline — is it silently ignored or is there a validation error? — **FIXED**: Added Edge Case 9 specifying silent ignore behavior — continuation is only called for lines after the first, which only occurs in multiline mode.
- [x] CHK045 Are requirements defined for calling `Prompt()` when a `Prompt()` call is already active on the same session? — **FIXED**: Edge Case 5 and 10 clarify this is undefined behavior, documented as a constraint but not enforced at runtime.

## Non-Functional Requirements — Thread Safety

- [x] CHK046 Are thread safety requirements clearly defined for all 36 mutable properties — specifically that each property uses `Lock` + `EnterScope()` pattern? — **FIXED**: Updated spec Assumptions to require Lock+EnterScope for all mutable properties per Constitution XI. Contract §prompt-session.md already specifies "Lock-Protected" for all mutable properties.
- [x] CHK047 Is the "single active prompt" constraint documented as an assumption vs. a runtime-enforced invariant, and is the expected behavior when violated specified? — **FIXED**: Edge Cases 5 and 10 now specify: documented constraint (not runtime-enforced), behavior is undefined if violated (potential state corruption). Matches Python behavior.
- [x] CHK048 Are thread safety requirements specified for the `DynCond` pattern — given that conditions are evaluated on the render thread but properties are set on the caller thread? — **FIXED**: Spec Assumptions updated to document that per-property Lock ensures safe cross-thread access between render thread reads (via DynCond) and caller thread writes.

## Non-Functional Requirements — Performance & Platform

- [x] CHK049 Are performance requirements for layout construction specified — given that `_CreateLayout()` builds a complex tree with 10+ containers, is lazy construction or caching required? — **RESOLVED**: Data-model state transitions clarify that Layout is built ONCE in the constructor, not per-prompt. No lazy construction needed — the one-time cost is acceptable.
- [x] CHK050 Are cross-platform requirements for `EnableSuspend` (Ctrl-Z) defined — particularly that it's only functional on Unix systems and silently ignored on Windows? — **FIXED**: Added FR-041 specifying the `SuspendToBackgroundSupported()` gate and platform behavior.

## Dependencies & Assumptions

- [x] CHK051 Is the assumption that `Application.RefreshInterval` needs a setter validated against the plan to modify Application? Does the spec document this as a cross-feature change? — **FIXED**: Plan complexity tracking updated with explicit "Cross-feature change" note for Application<TResult> modification.
- [x] CHK052 Are all 9 dynamic wrapper types (DynamicCompleter, DynamicValidator, DynamicAutoSuggest, DynamicLexer, DynamicStyle, DynamicStyleTransformation, DynamicClipboard, DynamicKeyBindings, DynamicCursorShapeConfig) verified as existing dependencies? — **FIXED**: FR-031 updated to list all 9 types explicitly (was missing DynamicCursorShapeConfig).
- [x] CHK053 Is the dependency on `LayoutUtils.ExplodeTextFragments()` for `_SplitMultilinePrompt` explicitly documented in the spec or plan? — **FIXED**: Added to plan complexity tracking as an existing dependency from Feature 029.
- [x] CHK054 Is the assumption that `InputHook` delegate type exists validated, and is the fallback behavior specified if it doesn't? — **YES**: Spec assumption updated to reference `Application/EventLoop` namespace. Python imports from `prompt_toolkit.eventloop.InputHook`. Already exists in Stroke.

## Ambiguities & Traceability

- [x] CHK055 Is there a clear 1:1 mapping between the 35 functional requirements (FR-001..FR-035) and the Python source code lines they correspond to? — **ADDRESSED**: FRs map to Python source sections but exact line-by-line mapping is impractical for a 1,538-line file. Key FRs reference specific Python line ranges where critical (e.g., FR-003 references `__init__` lines 378-424). Now 41 FRs (FR-001..FR-041).
- [x] CHK056 Does the spec define what "faithful port" means for this specific feature — line-by-line translation, behavioral equivalence, or API signature matching? — **FIXED**: Added assumption defining "faithful port" as behavioral equivalence: every public API in Python's `__all__` exports has a corresponding C# API with matching observable behavior.
- [x] CHK057 Are the 7 test files in plan.md traceable to specific functional requirements and user stories? — **FIXED**: Updated plan test file listing with FR and US references for each test file.

## Notes

- Check items off as completed: `[x]`
- Add comments or findings inline
- Items are numbered sequentially (CHK001-CHK057) for easy reference
- `[Spec §FR-NNN]` references functional requirements in spec.md
- `[Contract §filename.md]` references contract documents
- `[Research §RN]` references research decisions
- `[Gap]` marks items checking for missing requirements
- `[Conflict]` marks items checking for inconsistent requirements
