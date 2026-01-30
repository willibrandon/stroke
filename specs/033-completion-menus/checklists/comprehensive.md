# Comprehensive Pre-Implementation Checklist: Completion Menus

**Purpose**: Full requirements quality review covering completeness, clarity, consistency, coverage, API fidelity, thread safety, performance, and layout integration before implementation begins.
**Created**: 2026-01-30
**Feature**: [spec.md](../spec.md) | [plan.md](../plan.md)

## Requirement Completeness

- [x] CHK001 Are all 5 Python PTK classes from `layout/menus.py` accounted for in the spec? (`CompletionsMenuControl`, `CompletionsMenu`, `MultiColumnCompletionMenuControl`, `MultiColumnCompletionsMenu`, `_SelectedCompletionMetaControl`) [Completeness, Spec Key Entities]
- [x] CHK002 Are both module-level Python functions (`_get_menu_item_fragments`, `_trim_formatted_text`) mapped to `MenuUtils` static methods? [Completeness, Contract menu-utils.md]
- [x] CHK003 Are all internal helper methods documented for `CompletionsMenuControl`? (`_show_meta`, `_get_menu_width`, `_get_menu_meta_width`, `_get_menu_item_meta_fragments`) [Completeness, Contract completions-menu-control.md] — **STRENGTHENED**: Added `ShowMeta`, `GetMenuWidth`, `GetMenuMetaWidth`, `GetMenuItemMetaFragments` to the C# class signature section.
- [x] CHK004 Are all internal helper methods documented for `MultiColumnCompletionMenuControl`? (`_get_column_width`, grouper logic) [Completeness, Contract multi-column-completion-menu-control.md] — **STRENGTHENED**: Added `GetColumnWidth` with caching semantics to the C# class signature section.
- [x] CHK005 Are all constructor parameters for `CompletionsMenu` specified with types, defaults, and behavior? (`maxHeight`, `scrollOffset`, `extraFilter`, `displayArrows`, `zIndex`) [Completeness, Spec FR-002]
- [x] CHK006 Are all constructor parameters for `MultiColumnCompletionsMenu` specified with types, defaults, and behavior? (`minRows`, `suggestedMaxColumnWidth`, `showMeta`, `extraFilter`, `zIndex`) [Completeness, Spec FR-004]
- [x] CHK007 Are requirements defined for the `Reset()` method on `MultiColumnCompletionMenuControl`? [Completeness, Contract multi-column-completion-menu-control.md]
- [x] CHK008 Are all 3 prerequisite changes documented with exact file paths and required modifications? (unseal `ConditionalContainer`, unseal `HSplit`, update `ScrollbarMargin`) [Completeness, quickstart.md]
- [x] CHK009 Are all style class names exhaustively listed? (`completion-menu`, `completion-menu.completion`, `completion-menu.completion.current`, `completion-menu.meta.completion`, `completion-menu.meta.completion.current`, `completion-menu.multi-column-meta`, `completion`, `scrollbar`) [Completeness, data-model.md]
- [x] CHK010 Is the `cursorPosition` behavior of `CompletionsMenuControl.CreateContent` specified? (Python line 121: `Point(0, index ?? 0)`) [Completeness, Contract completions-menu-control.md]

## Requirement Clarity

- [x] CHK011 Is the `PreferredWidth` hardcoded max of 500 explicitly specified for `CompletionsMenuControl`? (Python line 70-71 passes 500 to internal width calculations) [Clarity, Contract completions-menu-control.md]
- [x] CHK012 Is the meta column width sampling cap of 200 completions clearly specified with the sampling behavior? [Clarity, Spec FR-012]
- [x] CHK013 Is the `SelectedCompletionMetaControl` preferred width optimization threshold (30+ completions) clearly defined with the exact short-circuit behavior? [Clarity, Spec FR-018]
- [x] CHK014 Is the distinction between `DisplayMeta` (formatted) and `DisplayMetaText` (plain text) clearly specified for the meta existence check vs. rendering? [Clarity, Contract selected-completion-meta-control.md] — **STRENGTHENED**: Added comprehensive DisplayMeta vs DisplayMetaText usage documentation to spec.md FR-012.
- [x] CHK015 Is the `TrimFormattedText` algorithm clearly specified with the remaining width calculation (`maxWidth - 3`) and the return value semantics (`maxWidth - remainingWidth`)? [Clarity, Contract menu-utils.md]
- [x] CHK016 Is the `GetMenuItemFragments` assembly order and padding calculation clearly specified? (`[("", " ")] + trimmedText + [("", padding)]` with style applied) [Clarity, Contract menu-utils.md]
- [x] CHK017 Is the multi-column `CreateContent` scroll adjustment formula clearly specified? (`scroll = min(selectedColumn, max(scroll, selectedColumn - visibleColumns + 1))`) [Clarity, Contract multi-column-completion-menu-control.md]
- [x] CHK018 Is "column width divided to fit more columns" quantified with the exact division formula? (`columnWidth //= columnWidth // suggestedMaxColumnWidth`) [Clarity, Spec Scenario 5.4]
- [x] CHK019 Is the `CompletionsMenu` window configuration fully specified? (`Dimension(min: 8)` width, `Dimension(min: 1, max: maxHeight)` height, `dontExtendWidth: true`, `style: "class:completion-menu"`) [Clarity, Contract completions-menu.md]

## Requirement Consistency

- [x] CHK020 Is the minimum width consistent between `CompletionsMenuControl.MinWidth` (7) and `CompletionsMenu` window width `Dimension(min: 8)` (7 + 1 scrollbar)? [Consistency, Spec FR-013 vs. Contract completions-menu.md]
- [x] CHK021 Is the z-index value consistent across both containers? (Both default to `100_000_000` = `10^8`) [Consistency, Spec FR-002 vs. FR-004]
- [x] CHK022 Is the `IsFocusable => false` requirement consistent across all 3 controls? [Consistency, Spec FR-016]
- [x] CHK023 Are the `HasCompletions & ~IsDone` filter components consistent between `CompletionsMenu` and `MultiColumnCompletionsMenu`? [Consistency, Contract completions-menu.md vs. multi-column-completions-menu.md]
- [x] CHK024 Is the mouse handler return value (`NotImplementedOrNone.None`) consistent across both menu controls? [Consistency, research.md R-004] — **STRENGTHENED**: Added per-event return value documentation (`NotImplementedOrNone.None` for handled events, `NotImplementedOrNone.NotImplemented` for all other events) to both menu control contracts.
- [x] CHK025 Are style string formats consistent between single-column and multi-column for current/normal items? [Consistency, Contract menu-utils.md vs. data-model.md]

## API Fidelity (Python PTK Faithfulness)

- [x] CHK026 Does each public API in `CompletionsMenu` match the Python `__init__` signature parameter-by-parameter? (`max_height`, `scroll_offset`, `extra_filter`, `display_arrows`, `z_index`) [Fidelity, Contract completions-menu.md]
- [x] CHK027 Does each public API in `MultiColumnCompletionsMenu` match the Python `__init__` signature parameter-by-parameter? (`min_rows`, `suggested_max_column_width`, `show_meta`, `extra_filter`, `z_index`) [Fidelity, Contract multi-column-completions-menu.md]
- [x] CHK028 Does the Python `scroll_offset: int | Callable[[], int]` parameter have a documented C# equivalent? (Contract shows `int` only; Python accepts callable) [Fidelity, Contract completions-menu.md] — **STRENGTHENED**: Added XML doc clarification that `int` is used in `CompletionsMenu`; `ScrollOffsets` constructor handles both `int` and `Func<int>` overloads.
- [x] CHK029 Does the `MultiColumnCompletionsMenu` constructor correctly avoid setting `style="class:completion-menu"` on the window, matching the Python comment at lines 657-660? [Fidelity, Contract multi-column-completions-menu.md]
- [x] CHK030 Are the `__all__` exports correctly mapped to `public` visibility? (`CompletionsMenu`, `MultiColumnCompletionsMenu` public; all others `internal`) [Fidelity, Contracts]
- [x] CHK031 Is the `anyCompletionHasMeta` condition checking `DisplayMeta` (formatted, not plain text), matching Python behavior? [Fidelity, Contract multi-column-completions-menu.md]
- [x] CHK032 Does the `SelectedCompletionMetaControl` check `DisplayMetaText` for existence but render `DisplayMeta`, matching Python's behavior? [Fidelity, Contract selected-completion-meta-control.md]
- [x] CHK033 Are all Python line references in contracts accurate and traceable to the Python source? [Fidelity, All contracts]
- [x] CHK034 Is the inheritance hierarchy faithful? `CompletionsMenu : ConditionalContainer` and `MultiColumnCompletionsMenu : HSplit` matching Python's MRO? [Fidelity, research.md R-002]

## Thread Safety & Performance

- [x] CHK035 Are all mutable fields of `MultiColumnCompletionMenuControl` documented in the data model with their types and initial values? [Completeness, data-model.md]
- [x] CHK036 Is the Lock scope specified for `MultiColumnCompletionMenuControl`? (Which methods acquire the lock, what operations are atomic?) [Clarity, research.md R-005] — **STRENGTHENED**: Added "Lock Scope Per Method" table to multi-column contract specifying which methods acquire the lock and their read/write accesses.
- [x] CHK037 Are compound read-modify-write scenarios documented for `MultiColumnCompletionMenuControl`? (e.g., `CreateContent` reads and writes render state; `MouseHandler` reads render state) [Coverage, Constitution XI] — **STRENGTHENED**: Added "Concurrency Notes" section documenting mutual exclusion between CreateContent and MouseHandler.
- [x] CHK038 Is the interaction between `CreateContent` (writes render state) and `MouseHandler` (reads render state) specified for concurrent access? [Coverage, Gap] — **STRENGTHENED**: Documented that both acquire the same lock, so they are mutually exclusive.
- [x] CHK039 Is the `ConditionalWeakTable` caching strategy specified with GC behavior documentation? [Clarity, research.md R-001]
- [x] CHK040 Is the meta width sampling cap (200 completions) specified with justification for the threshold? [Clarity, Spec FR-012] — **STRENGTHENED**: Added rationale to FR-012: "iterating all completions each render pass is O(n) and unacceptable for large completion sets of 1000+; 200 provides a statistically representative width sample while keeping render latency bounded."
- [x] CHK041 Is the preferred width optimization (30+ completions returns full width) specified with the performance rationale? [Clarity, Spec FR-018]
- [x] CHK042 Are thread safety tests specified for concurrent `CreateContent`/`MouseHandler`/`GetKeyBindings` access on `MultiColumnCompletionMenuControl`? [Coverage, plan.md project structure] — **STRENGTHENED**: Added 4 specific thread safety test scenarios to plan.md.

## Scenario Coverage

- [x] CHK043 Are requirements defined for the zero-completions scenario across all controls? (Empty list behavior) [Coverage, Spec Edge Cases]
- [x] CHK044 Are requirements defined for the single-completion scenario? (Degenerate case with 1 item) [Coverage, Gap] — **STRENGTHENED**: Added edge case to spec.md: "exactly one completion" renders normally with single item.
- [x] CHK045 Are requirements defined for completions with empty display text? [Coverage, Gap] — **STRENGTHENED**: Added edge case to spec.md: empty display text treated as zero-width, minimum width floor applies.
- [x] CHK046 Are requirements defined for the scenario where `maxAvailableWidth` is extremely small (< RequiredMargin)? [Coverage, Gap] — **STRENGTHENED**: Added edge case to spec.md: `max(1, ...)` guard ensures at least one column.
- [x] CHK047 Are requirements defined for the multi-column control receiving `height = 0`? [Coverage, Spec Edge Cases]
- [x] CHK048 Are requirements defined for scroll boundary behavior? (Cannot scroll past first/last column) [Coverage, Spec Edge Cases]
- [x] CHK049 Are requirements defined for the scenario where all completions lack meta text? [Coverage, Spec Story 2.2, Story 7.2]
- [x] CHK050 Are requirements for the `CompletionsMenuControl` mouse click at an invalid row (beyond completion count) specified? [Coverage, Gap] — **STRENGTHENED**: Added edge case to spec.md: handler silently returns without modifying state when click index exceeds list length.

## Edge Case Coverage

- [x] CHK051 Is the text trimming behavior specified when `maxWidth <= 3`? (Not enough room for "...") [Edge Case, Gap] — **STRENGTHENED**: Added edge case to spec.md and contract menu-utils.md: `remainingWidth <= 0`, result is "..." truncated to maxWidth chars.
- [x] CHK052 Is the behavior specified when completion display text contains wide (CJK) characters that cross the trim boundary? [Edge Case, Gap] — **STRENGTHENED**: Added edge case to spec.md and contract menu-utils.md: CJK character excluded entirely when its 2-column width exceeds remaining.
- [x] CHK053 Is the behavior specified for multi-column layout when `width - RequiredMargin < columnWidth`? (Column width clamping) [Edge Case, Contract multi-column-completion-menu-control.md]
- [x] CHK054 Is the behavior specified when `visibleColumns` exceeds `totalColumns`? [Edge Case, Gap] — **STRENGTHENED**: Added edge case to spec.md: scroll stays at 0, no arrows shown, all columns rendered.
- [x] CHK055 Is the behavior specified for the meta control when the selected completion changes during rendering? [Edge Case, Gap] — **STRENGTHENED**: Added edge case to spec.md: each method reads CompletionState independently at invocation time; no cross-call consistency guarantee by design.
- [x] CHK056 Is the behavior specified when the `CompletionState` transitions to null between `PreferredWidth` and `CreateContent` calls? [Edge Case, Gap] — **STRENGTHENED**: Added edge case to spec.md: each method checks for null and returns 0/empty content.

## Layout Integration

- [x] CHK057 Is the `ConditionalContainer` unsealing impact assessed? (Are there downstream effects of removing `sealed`?) [Dependency, quickstart.md] — **STRENGTHENED**: Added impact assessment to research.md: no virtual methods, no downstream sealed-type dependencies, minimal risk.
- [x] CHK058 Is the `HSplit` unsealing impact assessed? (Are there downstream effects of removing `sealed`?) [Dependency, quickstart.md] — **STRENGTHENED**: Added impact assessment to research.md: no pattern matching, no sealed-type assumptions, minimal risk.
- [x] CHK059 Is the `ScrollbarMargin` parameter type change from `bool` to `FilterOrBool` backward-compatible? (Existing callers passing `bool`?) [Dependency, quickstart.md] — **STRENGTHENED**: Added backward compatibility analysis to research.md: `FilterOrBool` has implicit `bool` conversion, source- and binary-compatible.
- [x] CHK060 Is the filter composition `extraFilter & HasCompletions & ~IsDone` specified with evaluation semantics? (Short-circuit, lazy?) [Clarity, Contract completions-menu.md]
- [x] CHK061 Is the `anyCompletionHasMeta` Condition lambda capture specified? (It accesses `AppContext.GetApp().CurrentBuffer.CompleteState` dynamically) [Clarity, Contract multi-column-completions-menu.md]
- [x] CHK062 Are the `Window` constructor parameters fully specified for both the single-column and multi-column containers? [Completeness, Contracts]
- [x] CHK063 Is the `ScrollOffsets` usage specified with both top and bottom offsets set to the same `scrollOffset` value? [Clarity, Contract completions-menu.md]

## Dependencies & Assumptions

- [x] CHK064 Are all 26 external dependencies listed in research.md verified as existing and accessible? [Dependency, research.md]
- [x] CHK065 Is the assumption that `Buffer.CompleteState` provides access to completions list, current index, and current completion validated against the actual Buffer API? [Assumption, Spec Assumptions]
- [x] CHK066 Is the assumption that `AppContext.GetApp()` is accessible during control rendering validated? [Assumption, Spec Assumptions]
- [x] CHK067 Is the assumption that `FormattedTextUtils.ToFormattedText` accepts a style parameter for applying styles to fragment lists validated? [Assumption, Contract menu-utils.md] — **STRENGTHENED**: Added explicit note in research.md dependency table confirming the `style` parameter overload requirement.
- [x] CHK068 Is the assumption that `ExplodedList` provides single-character fragment iteration validated? [Assumption, Contract menu-utils.md]
- [x] CHK069 Are the `GetCWidth` / `UnicodeWidth.GetWidth` APIs available for character width calculation as assumed? [Assumption, Contract selected-completion-meta-control.md]

## Notes

- Check items off as completed: `[x]`
- Add comments or findings inline
- Link to relevant resources or documentation
- Items are numbered sequentially for easy reference
- Focus areas: Comprehensive, API Fidelity, Thread Safety & Performance, Layout Integration
- Depth: Standard | Audience: Pre-implementation reviewer | Timing: Before coding begins

## Summary of Strengthening Actions

| Action | Files Modified | Items Addressed |
|--------|---------------|-----------------|
| Added internal method signatures to C# class contracts | completions-menu-control.md, multi-column-completion-menu-control.md | CHK003, CHK004 |
| Added DisplayMeta vs DisplayMetaText usage documentation | spec.md (FR-012) | CHK014 |
| Added per-event mouse handler return values | completions-menu-control.md, multi-column-completion-menu-control.md | CHK024 |
| Clarified scroll_offset callable support | completions-menu.md | CHK028 |
| Added Lock Scope Per Method table and Concurrency Notes | multi-column-completion-menu-control.md | CHK036, CHK037, CHK038 |
| Added 200-sampling rationale to FR-012 | spec.md | CHK040 |
| Added thread safety test scenarios | plan.md | CHK042 |
| Added 10 new edge cases to spec | spec.md (Edge Cases section) | CHK044, CHK045, CHK046, CHK050, CHK051, CHK052, CHK054, CHK055, CHK056 |
| Added TrimFormattedText edge case behaviors | menu-utils.md | CHK051, CHK052 |
| Added unsealing impact assessments | research.md | CHK057, CHK058 |
| Added ScrollbarMargin backward compatibility analysis | research.md | CHK059 |
| Added FormattedTextUtils style overload requirement | research.md | CHK067 |
