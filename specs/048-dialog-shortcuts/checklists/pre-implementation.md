# Pre-Implementation Checklist: Dialog Shortcut Functions

**Purpose**: Comprehensive requirements quality validation before implementation — covers API fidelity, thread safety, contracts quality, and edge case completeness
**Created**: 2026-02-02
**Feature**: [spec.md](../spec.md) | [plan.md](../plan.md) | [contracts/dialogs-api.md](../contracts/dialogs-api.md)
**Audience**: Author (pre-implementation gate)
**Depth**: Full coverage

## API Fidelity — Python→C# Port Completeness

- [x] CHK001 - Are all 9 Python symbols (`yes_no_dialog`, `button_dialog`, `input_dialog`, `message_dialog`, `radiolist_dialog`, `checkboxlist_dialog`, `progress_dialog`, `_create_app`, `_return_none`) accounted for in the contracts? [Completeness, Spec §FR-020]
- [x] CHK002 - Are parameter names for each dialog function documented with their Python→C# naming transformations (e.g., `yes_text`→`yesText`, `ok_text`→`okText`)? [Clarity, Contracts §Factory Methods]
- [x] CHK003 - Are default parameter values specified for every parameter, matching the Python source defaults (e.g., `yes_text="Yes"`, `ok_text="Ok"` vs `ok_text="OK"` — note the casing difference between message/radio/checkbox "Ok" and input "OK")? [Consistency, Spec §FR-020]
- [x] CHK004 - Is the return type for each factory method explicitly specified, including nullability (e.g., `Application<string?>` for InputDialog, `Application<IReadOnlyList<T>?>` for CheckboxListDialog)? [Clarity, Contracts §Factory Methods]
- [x] CHK005 - Are the async wrapper method signatures specified with matching return types (e.g., `Task<bool>` for `YesNoDialogAsync`, `Task` for `MessageDialogAsync`)? [Completeness, Contracts §Async Methods]
- [x] CHK006 - Is the distinction between `Application<object?>` (MessageDialog, ProgressDialog) and typed returns (`Application<bool>`, `Application<string?>`) clearly documented? [Clarity, Contracts §Factory Methods]
- [x] CHK007 - Are the `buttons` parameter types consistent between Python (list of tuples) and C# (IReadOnlyList of value tuples) for ButtonDialog, RadioListDialog, and CheckboxListDialog? [Consistency, Contracts]
- [x] CHK008 - Is the tuple element ordering documented — ButtonDialog uses `(string Text, T Value)` while RadioListDialog/CheckboxListDialog use `(T Value, AnyFormattedText Label)` matching Python's ordering? [Clarity, Contracts §Factory Methods]

## API Fidelity — Behavioral Equivalence

- [x] CHK009 - Is the `_create_app` behavior fully specified: tab/s-tab bindings, merge with `DefaultKeyBindings.Load()`, mouse support=true, full-screen=true? [Completeness, Plan §D5]
- [x] CHK010 - Is the duplication of tab/s-tab bindings addressed — Dialog widget already has tab/s-tab with `~HasCompletions` filter, and `_create_app` adds additional unconditional tab/s-tab? Is this intentional dual-binding documented? [Clarity, Plan §D5]
- [x] CHK011 - Is the `ReturnNone` cancel handler mechanism clearly specified — using `AppContext.GetApp().Exit()` vs capturing the specific app instance? [Clarity, Plan §D2]
- [x] CHK012 - Is the InputDialog's accept handler behavior fully specified: focus transfer to OK button via `Layout.Focus(okButton.Window)`, returns `true` to keep text? [Completeness, Spec §FR-016, Plan §D6]
- [x] CHK013 - Is the ProgressDialog's `with_background=True` requirement documented consistently with all other dialog types — Python source confirms ALL 7 dialogs pass `with_background=True`? [Consistency, Spec §FR-011]
- [x] CHK014 - Are the `Label` constructor defaults documented — `dontExtendHeight` defaults to `true` in Stroke, so explicit `dontExtendHeight: true` may be redundant but should match Python's explicit `dont_extend_height=True`? [Clarity, Research §R5]

## Thread Safety Requirements

- [x] CHK015 - Is the `setPercentage` callback's thread-safety mechanism documented — direct Lock-protected `ProgressBar.Percentage` setter + thread-safe `app.Invalidate()`? [Completeness, Plan §D3]
- [x] CHK016 - Is the `logText` callback's thread-marshaling mechanism documented — `_actionChannel.Writer.TryWrite()` to marshal `Buffer.InsertText()` to the async context? [Completeness, Plan §D3, Research §R3]
- [x] CHK017 - Is the `_actionChannel` null-check requirement specified — channel is `null` before `RunAsync()` starts, matching Python's `if loop is not None:` guard? [Edge Case, Research §R6]
- [x] CHK018 - Is the `_actionChannel` access pattern documented — `internal` field, accessible within same assembly? [Clarity, Research §R6]
- [x] CHK019 - Is the `finally { app.Exit(); }` requirement for ProgressDialog callback specified, ensuring terminal recovery even on exception? [Completeness, Spec §FR-018]
- [x] CHK020 - Is the background task lifecycle specified — `PreRunCallables` → `CreateBackgroundTask` → `Task.Run(callback)` → `finally { Exit() }`? [Completeness, Plan §D3, Research §R4]
- [x] CHK021 - Is the timing constraint documented — `CreateBackgroundTask` only works during `RunAsync` (after `_backgroundTasksCts` is initialized), hence `PreRunCallables` is the correct scheduling point? [Clarity, Research §R4]

## Contracts Quality — Completeness

- [x] CHK022 - Does the contracts document specify XML doc comments for all 7 factory methods and 7 async wrappers? [Completeness, Contracts]
- [x] CHK023 - Are the private helper methods (`CreateApp<T>`, `ReturnNone`) documented in the contracts with their purpose and behavior? [Completeness, Contracts §Private Methods]
- [x] CHK024 - Is `CreateApp` specified as generic (`CreateApp<T>`) to support different `Application<T>` return types across dialog functions? [Clarity, Contracts §Private Methods]
- [x] CHK025 - Are the `IContainer` vs `AnyContainer` parameter types for `CreateApp` clearly specified — Python uses `AnyContainer`, the contract shows `IContainer`? [Consistency, Contracts §Private Methods]
- [x] CHK026 - Is the `runCallback` parameter for ProgressDialog specified as `Action<Action<int>, Action<string>>?` with nullable default, matching Python's `lambda *a: None` default? [Completeness, Contracts §ProgressDialog]

## Contracts Quality — Generic Constraints

- [x] CHK027 - Are generic type constraints for `ButtonDialog<T>`, `RadioListDialog<T>`, `CheckboxListDialog<T>` specified or documented as unconstrained (matching Python's `TypeVar("_T")`)? [Clarity, Contracts]
- [x] CHK028 - Is the nullability of `RadioListDialog<T>`'s return type (`Application<T?>`) compatible with both reference and value types — does this require `where T : class` or is `T?` handled differently? [Ambiguity, Contracts §RadioListDialog]
- [x] CHK029 - Is the mismatch between api-mapping.md (`Task<T>` for RadioListDialog) and the cancel-returns-null requirement (`Task<T?>`) addressed? [Conflict, Spec §FR-019 vs api-mapping.md]

## Edge Case Coverage

- [x] CHK030 - Is the empty buttons list behavior specified for ButtonDialog — dialog renders with body text only, no button row? [Edge Case, Spec §Edge Cases]
- [x] CHK031 - Is the empty values list behavior specified for RadioListDialog — empty list displayed, OK returns default value? [Edge Case, Spec §Edge Cases]
- [x] CHK032 - Is the empty values list behavior specified for CheckboxListDialog — empty list displayed, OK returns empty list? [Edge Case, Spec §Edge Cases]
- [x] CHK033 - Is the null `runCallback` behavior for ProgressDialog defined — what happens if callback is null (default)? Does the dialog immediately exit? [Edge Case, Gap]
- [x] CHK034 - Is the behavior when ProgressDialog callback calls both `setPercentage` and `logText` concurrently from multiple threads addressed? [Edge Case, Gap]
- [x] CHK035 - Are custom button text requirements specified — e.g., `yesText="Confirm"` should display "Confirm" instead of "Yes"? [Edge Case, Spec §Edge Cases]
- [x] CHK036 - Is the behavior defined when InputDialog's `default_` parameter contains multi-line text but `multiline=False`? [Edge Case, Gap]
- [x] CHK037 - Is the behavior specified when a ProgressDialog is created but never run (factory method called but `RunAsync` never invoked)? Does `logText` gracefully handle the null channel? [Edge Case, Research §R6]

## Scenario Coverage — Acceptance Criteria

- [x] CHK038 - Do all 7 user stories have independently testable acceptance scenarios? [Coverage, Spec §User Scenarios]
- [x] CHK039 - Are cancel/dismiss scenarios covered for all dialog types that support cancel (InputDialog, RadioListDialog, CheckboxListDialog)? [Coverage, Spec §FR-019]
- [x] CHK040 - Is the Tab/Shift-Tab focus navigation scenario specified as testable for ALL dialog types (not just YesNoDialog)? [Coverage, Spec §FR-009]
- [x] CHK041 - Are mouse interaction requirements specified with testable acceptance criteria for any dialog type? [Gap, Spec §FR-013]
- [x] CHK042 - Is the ProgressDialog callback exception scenario (FR-018) covered by an acceptance test — callback throws, app still exits? [Coverage, Spec §US7]
- [x] CHK043 - Are the async wrapper methods covered by acceptance criteria, or are they implicitly covered by the factory method tests? [Gap, Contracts §Async Methods]

## Dependency & Assumption Validation

- [x] CHK044 - Is the assumption that `Dialog` widget's `withBackground=true` wraps in `Box` validated against the existing implementation? [Assumption, Spec §Assumptions]
- [x] CHK045 - Is the assumption that `Application<T>.Exit()` with no arguments exits with `default(T)` validated? [Assumption, Plan §D2]
- [x] CHK046 - Is the assumption that `TextArea.AcceptHandler` receives `Func<Buffer, bool>` (not `Action<Buffer>`) validated against the existing TextArea implementation? [Assumption, Plan §D6]
- [x] CHK047 - Is the assumption that `_actionChannel` is `internal` (not `private`) validated? [Assumption, Research §R6]
- [x] CHK048 - Is the assumption that `FocusFunctions.FocusNext` and `FocusPrevious` have the correct delegate signature for `KeyBindings.Add` validated? [Assumption, Plan §D5]

## Non-Functional Requirements

- [x] CHK049 - Is a file size estimate documented confirming `Dialogs.cs` will stay under 1,000 LOC? [Measurability, Plan §Technical Context]
- [x] CHK050 - Is the 80% test coverage target addressed with a testing strategy that covers all 7 dialog types? [Measurability, Spec §SC-003]
- [x] CHK051 - Are XML doc comment requirements specified for all public methods on the `Dialogs` class? [Completeness, Constitution §Technical Standards]

## Notes

- This checklist complements the existing `requirements.md` which covers general spec quality.
- **All 51 items resolved** on 2026-02-02. No remaining gaps or conflicts.

### Resolution Summary

**API Fidelity (CHK001-008)**: All 9 symbols, parameter names, default values (including Ok/OK casing), return type nullability, async wrappers, tuple types, and tuple ordering are now documented in the spec's new "Port Mapping Reference" section.

**Behavioral Equivalence (CHK009-014)**: CreateApp behavior, dual tab binding rationale, ReturnNone mechanism, InputDialog accept handler (FR-016 strengthened), withBackground consistency, and Label defaults are now documented in the spec's new "Behavioral Specification" section.

**Thread Safety (CHK015-021)**: setPercentage (Lock-protected), logText (_actionChannel marshaling), null-check requirement (FR-022), _actionChannel access pattern, finally block (FR-018), background task lifecycle, and timing constraint (FR-023) are now documented in the spec's new "Thread Safety Specification" section.

**Contracts Quality (CHK022-026)**: XML docs present for all 14 public methods and 2 private helpers. CreateApp is generic. IContainer vs AnyContainer rationale added to contracts Private Methods section. runCallback nullable default specified.

**Generic Constraints (CHK027-029)**: Unconstrained generics documented. T? nullability for RadioListDialog works for both reference and value types without `where T : class`. **CHK029 RESOLVED**: Spec explicitly states `Task<T?>` is correct for RadioListDialogAsync, taking precedence over api-mapping.md's `Task<T>`.

**Edge Cases (CHK030-037)**: All 8 edge cases now covered in spec. Added: null runCallback (CHK033, no-op then exit), concurrent set+log (CHK034, independently thread-safe), multi-line default_ (CHK036, passed as-is), never-run ProgressDialog (CHK037, null-check drops silently). Mouse interaction edge case also added (CHK041).

**Scenario Coverage (CHK038-043)**: All 7 stories have testable scenarios (CHK038). Cancel scenarios confirmed for Input/Radio/Checkbox (CHK039). Tab/Shift-Tab scenarios added to all 7 user stories (CHK040). Mouse interaction documented as edge case (CHK041). Exception scenario added to US7 (CHK042). Async wrapper testing strategy documented (CHK043, implicit coverage).

**Dependency Validation (CHK044-048)**: All 5 assumptions validated against codebase with file/line references in spec's updated Assumptions section.

**Non-Functional (CHK049-051)**: File size estimate (NF-001), 80% coverage strategy (NF-002), and XML doc requirements (NF-003, FR-024) added to spec.
