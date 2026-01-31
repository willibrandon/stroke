# Comprehensive Requirements Quality Checklist: Named Commands

**Purpose**: Deep audit of requirement completeness, clarity, consistency, and coverage across spec, plan, contracts, and data model for the named commands feature
**Created**: 2026-01-30
**Feature**: [spec.md](../spec.md)
**Depth**: Deep — exhaustive audit across all artifacts
**Focus**: Port fidelity, API contract quality, behavioral completeness

## Port Fidelity — Requirement Completeness

- [x] CHK001 - Are all 49 command names from the Python source explicitly enumerated in the spec's functional requirements? ~~The spec says "40+" in SC-001 but the plan says 49.~~ **Fixed**: SC-001 now says "All 49 Readline command names" with exact per-category breakdown (10+6+9+10+3+4+7). FR-001 says "49 built-in commands". [Spec §SC-001, §FR-001]
- [x] CHK002 - Is the `end-of-file` command listed in the spec's functional requirements? ~~FR-007 covers text modification but lists only 8 commands; data-model.md lists 9 including `end-of-file`.~~ **Fixed**: FR-007 now lists all 9 text modification commands including `end-of-file`. FR-007a defines its behavior. Data model updated to "Text Modification Commands (9)". [Spec §FR-007]
- [x] CHK003 - Is `operate-and-get-next` listed in both FR-006 (history) and FR-011 (misc)? ~~The data model lists it under both categories.~~ **Fixed**: Aligned to Python source structure. FR-006 now has 6 history commands (without `operate-and-get-next`). FR-011 has 7 misc commands (with `operate-and-get-next`). Data model, contracts, quickstart, research all updated. [Spec §FR-006, §FR-011, Data Model, Contracts, Quickstart, Research]
- [x] CHK004 - Are requirements defined for the `CompletionBindings` helper class? ~~The spec did not mention these helpers.~~ **Fixed**: Added FR-023 requiring `CompletionBindings` static class with `GenerateCompletions` and `DisplayCompletionsLikeReadline`. Added User Story 6 with acceptance scenarios. [Spec §FR-023, §US-6]
- [x] CHK005 - Is the `KeyPressEventExtensions.GetApp()` extension method documented in the spec? ~~Contracts defined it but spec had no requirement.~~ **Fixed**: Added FR-024 requiring `GetApp()` extension method with `InvalidOperationException` for null/wrong type. [Spec §FR-024]
- [x] CHK006 - Are the specific Python source files being ported explicitly identified? ~~The spec said "Python Prompt Toolkit" generically.~~ **Fixed**: Added `**Python Source**` header to spec listing `named_commands.py` (692 lines, 49 commands) and `completion.py` (two public functions). [Spec header]
- [x] CHK007 - Is the exact count of commands per category specified in requirements? ~~FR-005 said "all movement commands" without count.~~ **Fixed**: Every FR category heading now includes the count (e.g., "Movement Commands (10 commands — FR-005)"). SC-001 includes the full breakdown. [Spec §FR-005 through §FR-011, §SC-001]
- [x] CHK008 - Are requirements for `quoted-insert` behavior defined? ~~No explicit requirement.~~ **Fixed**: Added FR-007g specifying `QuotedInsert` MUST set `Application.QuotedInsert` to `true`. [Spec §FR-007g]

## Port Fidelity — Behavioral Accuracy

- [x] CHK009 - Is `redraw-current-line` explicitly documented as a no-op? **Fixed**: FR-005e states "`redraw-current-line` MUST be a no-op (defined by Readline but not implemented in Python Prompt Toolkit)". [Spec §FR-005e]
- [x] CHK010 - Is the `clear-screen` behavior defined? **Fixed**: FR-005e states "`clear-screen` MUST call the renderer's clear method to clear the screen". [Spec §FR-005e]
- [x] CHK011 - Are the word boundary definitions for `forward-word` and `backward-word` specified? **Fixed**: FR-005d now states "words are composed of letters and digits (not whitespace-delimited WORDs)". [Spec §FR-005d]
- [x] CHK012 - Is the distinction between `unix-word-rubout` (WORD=true, whitespace boundary) and `backward-kill-word` (word=false, non-alphanumeric boundary) clearly specified? **Fixed**: FR-008c specifies "whitespace as the word boundary, i.e., `WORD=true`". FR-008d specifies "non-alphanumeric characters as the boundary (i.e., `WORD=false`)". FR-026 documents the delegation pattern. [Spec §FR-008c, §FR-008d, §FR-026]
- [x] CHK013 - Are requirements for the `delete-char` bell/notification behavior defined? **Fixed**: FR-007b states "If nothing is deleted (e.g., cursor at end of buffer), the application MUST call `Output.Bell()`". [Spec §FR-007b]
- [x] CHK014 - Are requirements for the `unix-word-rubout` bell behavior defined? **Fixed**: FR-008c states "If nothing can be deleted, the application MUST call `Output.Bell()`". Also in Edge Cases: "`unix-word-rubout` when nothing to delete: Application calls `Output.Bell()`". [Spec §FR-008c, §Edge Cases]
- [x] CHK015 - Is `end-of-file` behavior clearly distinguished from "delete at end of buffer"? **Fixed**: FR-007a explicitly states "`end-of-file` MUST exit the application by calling `Application.Exit()`". This is distinct from `delete-char`. [Spec §FR-007a]
- [x] CHK016 - Are `yank-nth-arg` and `yank-last-arg` behaviors adequately specified? **Fixed**: FR-015a and FR-015b specify behaviors including `event.ArgPresent` check and delegation to `Buffer.YankNthArg()`/`Buffer.YankLastArg()`. [Spec §FR-015a, §FR-015b]

## API Contract Quality — GetByName

- [x] CHK017 - Is the case sensitivity of command name lookup specified? **Fixed**: FR-002 states "Lookup MUST be case-sensitive (i.e., 'Forward-Char' is not equivalent to 'forward-char')". [Spec §FR-002]
- [x] CHK018 - Is the `GetByName` error message format specified in the spec? **Fixed**: FR-003 specifies the exact message format: `Unknown Readline command: '{name}'`. [Spec §FR-003]
- [x] CHK019 - Is the distinction between `ArgumentNullException` for null and `KeyNotFoundException` for empty string clearly justified? **Fixed**: FR-003 explicitly specifies: `KeyNotFoundException` for unregistered names, `ArgumentNullException` for null. US-1 acceptance scenarios 4 and 5 test both cases. [Spec §FR-003, §US-1]
- [x] CHK020 - Are requirements for whitespace-only name inputs defined? **Fixed**: Edge Cases section includes `GetByName("  ")` (whitespace): Throws `KeyNotFoundException`. [Spec §Edge Cases]

## API Contract Quality — Register

- [x] CHK021 - Is the thread safety behavior of concurrent `Register` calls specified? **Fixed**: NFR-002 specifies "For concurrent `Register` calls to the same name, last-writer-wins semantics apply". Edge Cases section also addresses concurrent access. [Spec §NFR-002, §Edge Cases]
- [x] CHK022 - Does `Register` validate command name format? ~~FR-022 vs FR-004 conflict.~~ **Fixed**: FR-022 now explicitly states "Custom commands registered via `Register` are not subject to this naming constraint". FR-004 only validates null, empty, and whitespace. [Spec §FR-022, §FR-004]
- [x] CHK023 - Is the `recordInMacro` parameter's default value explicitly specified in requirements? **Fixed**: FR-004 now includes the full signature: `Register(string name, KeyHandlerCallable handler, bool recordInMacro = true)`. [Spec §FR-004]
- [x] CHK024 - Are requirements for `Register` error behavior defined in the spec? **Fixed**: FR-004 now specifies `ArgumentNullException` for null name/handler and `ArgumentException` for empty/whitespace name. [Spec §FR-004]

## API Contract Quality — Internal Registration

- [x] CHK025 - Is the `RegisterInternal` method distinguished from public `Register`? **Fixed**: Assumptions section documents `RegisterInternal` as the internal method that creates Bindings and adds to ConcurrentDictionary. FR-025 documents the Binding construction defaults. [Spec §Assumptions, §FR-025]
- [x] CHK026 - Is the `Keys.Any` placeholder key sequence documented as intentional? **Fixed**: FR-025 explicitly states "`Keys = [Keys.Any]` (a placeholder key sequence, since named commands are looked up by name rather than key sequence)". [Spec §FR-025]
- [x] CHK027 - Are the default Binding property values documented in requirements? **Fixed**: FR-025 lists all defaults: "Filter = Always, Eager = Never, IsGlobal = Never, SaveBefore = _ => true, RecordInMacro = Always (except `call-last-kbd-macro` which uses `Never`)". [Spec §FR-025]

## Behavioral Completeness — Movement Commands

- [x] CHK028 - Is `beginning-of-line` behavior on a multi-line buffer clearly specified? **Fixed**: FR-005b states "`beginning-of-line` MUST move cursor to the start of the current line (not position 0 in a multi-line buffer)". [Spec §FR-005b]
- [x] CHK029 - Are `forward-char` and `backward-char` boundary behaviors defined? **Fixed**: FR-005c states "At buffer boundaries, the cursor does not move (no-op)". Edge Cases section specifies exact behavior for both. [Spec §FR-005c, §Edge Cases]
- [x] CHK030 - Are `forward-word` and `backward-word` repeat count behaviors specified? **Fixed**: FR-005d states "Both MUST respect `event.Arg` as repeat count". FR-012 generalizes this for all movement/editing commands. [Spec §FR-005d, §FR-012]

## Behavioral Completeness — Kill/Yank Commands

- [x] CHK031 - Is "consecutive kill concatenation" precisely defined? **Fixed**: FR-014 now specifies "determined by `event.IsRepeat`". [Spec §FR-014]
- [x] CHK032 - Is the kill concatenation direction specified? **Fixed**: FR-014 now specifies direction: "For forward-killing commands (`kill-word`), new text is appended: `previousClipboard + newText`. For backward-killing commands (`unix-word-rubout`, `backward-kill-word`), new text is prepended: `newText + previousClipboard`". [Spec §FR-014]
- [x] CHK033 - Is the `yank-pop` behavior fully specified? **Fixed**: FR-015c provides complete 3-step description: "(1) restoring the document to its state before the last paste (`DocumentBeforePaste`), (2) calling `Clipboard.Rotate()`, (3) pasting the new top of the clipboard using Emacs paste mode". [Spec §FR-015c]
- [x] CHK034 - Are requirements for `yank-pop` when no prior `yank` defined? **Fixed**: FR-015c states "If `DocumentBeforePaste` is null (no preceding yank), `yank-pop` is a no-op". Also in Edge Cases. [Spec §FR-015c, §Edge Cases]
- [x] CHK035 - Is `delete-horizontal-space` behavior specified? **Fixed**: FR-008e states "MUST delete all tabs and spaces (characters `\t` and ` ` only) around the cursor position (both before and after). This command does NOT place deleted text on the clipboard." [Spec §FR-008e]
- [x] CHK036 - Is `unix-line-discard` behavior at column 0 clearly defined? **Fixed**: FR-008f states "When the cursor is already at column 0 and not at position 0, it MUST delete one character backward (the preceding newline)". [Spec §FR-008f]

## Behavioral Completeness — History Commands

- [x] CHK037 - Is `accept-line` behavior defined? **Fixed**: FR-006a states "MUST call the buffer's validate-and-handle method to submit the current input for validation and acceptance". [Spec §FR-006a]
- [x] CHK038 - Are `previous-history` and `next-history` boundary behaviors specified? **Fixed**: FR-006b states "At the oldest/newest boundary, the buffer's own history navigation handles the limit". [Spec §FR-006b]
- [x] CHK039 - Is `operate-and-get-next` behavior precisely defined? **Fixed**: FR-021 provides detailed 3-step algorithm: "(1) compute the next working index as `Buffer.WorkingIndex + 1`; (2) call `ValidateAndHandle()`; (3) append a callable to `Application.PreRunCallables` that sets `Buffer.WorkingIndex` to the computed index (if it's within bounds)". [Spec §FR-021]
- [x] CHK040 - Is `reverse-search-history` behavior adequately specified? **Fixed**: FR-006d provides complete behavior: "checking if the current layout control is a `BufferControl` with a `SearchBufferControl`, setting `CurrentSearchState.Direction` to `Backward`, and making the `SearchBufferControl` the current control". [Spec §FR-006d]

## Behavioral Completeness — Text Modification Commands

- [x] CHK041 - Is the `transpose-chars` three-case behavior specified consistently? **Fixed**: FR-007e defines all three cases: "(1) at position 0, do nothing; (2) at end of buffer or when the character at cursor is a newline, swap the two characters before the cursor; (3) otherwise, move cursor right by one position then swap the two characters before the cursor". Matches data model. [Spec §FR-007e]
- [x] CHK042 - Are the word case command behaviors defined for mixed content? **Fixed**: FR-007f states "Words are defined by `FindNextWordEnding()` (letters and digits)". The case transformation applies to whatever text falls in that range, including numbers and punctuation that happen to be between word boundaries. [Spec §FR-007f]
- [x] CHK043 - Is `self-insert` behavior with null/empty `event.Data` defined? **Fixed**: Edge Cases section states "`self-insert` when `event.Data` is empty/null: Inserts empty string repeated by arg count, which is a no-op". [Spec §Edge Cases]
- [x] CHK044 - Is `capitalize-word` semantics defined precisely? **Fixed**: FR-007f states "`capitalize-word` MUST title-case the same range" and "These commands use overwrite mode to replace the text in-place and advance the cursor". The title-case behavior comes from `string.Title()` equivalent. [Spec §FR-007f]

## Behavioral Completeness — Completion Commands

- [x] CHK045 - Are completion command behaviors defined beyond their names? **Fixed**: Added FR-009a, FR-009b, FR-009c with full behavioral descriptions for each completion command. [Spec §FR-009a, §FR-009b, §FR-009c]
- [x] CHK046 - Is `GenerateCompletions` behavior specified? **Fixed**: FR-009b describes: "(1) if completion state already exists, advances to the next completion; (2) otherwise, starts completion with common part insertion". [Spec §FR-009b]
- [x] CHK047 - Is `DisplayCompletionsLikeReadline` blocking behavior specified? **Fixed**: FR-009a describes: "(1) generates completions synchronously (blocking); (2) if exactly one completion, inserts it; (3) if multiple completions with a common suffix, inserts the common suffix; (4) if multiple completions with no common suffix, displays them above the prompt in columns". [Spec §FR-009a]
- [x] CHK048 - Are requirements for completion commands when no completer is configured defined? **Fixed**: FR-009a states "If no completer is configured on the buffer, it returns immediately". Edge Cases section also covers this. [Spec §FR-009a, §Edge Cases]

## Behavioral Completeness — Macro Commands

- [x] CHK049 - Is `start-kbd-macro` behavior when already recording defined? **Fixed**: Edge Cases section states "Delegates to `EmacsState.StartMacro()` which handles this per its own implementation". [Spec §Edge Cases]
- [x] CHK050 - Is `end-kbd-macro` behavior when not recording defined? **Fixed**: Edge Cases section states "Delegates to `EmacsState.EndMacro()` which handles this per its own implementation". [Spec §Edge Cases]
- [x] CHK051 - Is `call-last-kbd-macro` behavior when no macro recorded defined? **Fixed**: FR-020 states "If no macro has been recorded (`EmacsState.Macro` is null/empty), it does nothing". Edge Cases confirms. [Spec §FR-020, §Edge Cases]
- [x] CHK052 - Is `print-last-kbd-macro` output format specified? **Fixed**: FR-010c states "printing each `KeyPress` in the macro. The output format is one `KeyPress` per line (matching Python's `print(k)` for each key press)". [Spec §FR-010c]

## Behavioral Completeness — Miscellaneous Commands

- [x] CHK053 - Is `insert-comment` behavior for multi-line buffers clearly specified? **Fixed**: FR-017 states "prepend `#` to every line" and "remove the leading `#` from each line that starts with `#`". US-7 scenarios 3 and 4 demonstrate multi-line behavior. [Spec §FR-017, §US-7]
- [x] CHK054 - Is the uncommenting behavior fully specified? **Fixed**: FR-017 specifies "remove the leading `#` from each line that starts with `#` (lines without a leading `#` are left unchanged)". [Spec §FR-017]
- [x] CHK055 - Is the R-006 decision documented in spec requirements? **Fixed**: FR-017 explicitly states "MUST use `event.Arg != 1` to determine behavior (faithful to Python source)". [Spec §FR-017]
- [x] CHK056 - Is `vi-editing-mode` behavior specified? **Fixed**: FR-011b states "MUST set `Application.EditingMode` to `EditingMode.Vi`". [Spec §FR-011b]
- [x] CHK057 - Is `emacs-editing-mode` behavior specified? **Fixed**: FR-011b states "MUST set `Application.EditingMode` to `EditingMode.Emacs`". [Spec §FR-011b]
- [x] CHK058 - Is `prefix-meta` behavior adequately specified? **Fixed**: FR-011c states "MUST feed an Escape `KeyPress` into the key processor at the current position (using `first: true`), enabling keyboards without a Meta key to produce Meta-modified key sequences". [Spec §FR-011c]
- [x] CHK059 - Is `edit-and-execute-command` fire-and-forget behavior documented? **Fixed**: FR-011d states "The async call is fire-and-forget (matching Python source, which does not await the call). If the editor operation fails, the error is handled by `Buffer.OpenInEditorAsync` internally." [Spec §FR-011d]

## Acceptance Criteria Quality

- [x] CHK060 - Can SC-001 be objectively measured? ~~"40+" was vague.~~ **Fixed**: SC-001 now says "All 49 Readline command names" with exact per-category breakdown: "(10 movement + 6 history + 9 text modification + 10 kill/yank + 3 completion + 4 macro + 7 miscellaneous)". [Spec §SC-001]
- [x] CHK061 - Can SC-002 be objectively verified? ~~"same result" was vague.~~ **Fixed**: SC-002 now specifies "the same buffer state (text content and cursor position), clipboard state, and application state". [Spec §SC-002]
- [x] CHK062 - Are SC-003 boundary values specified? **Fixed**: SC-003 now lists "(0, -1, and the maximum arg value clamped by `KeyPressEvent`)". [Spec §SC-003]
- [x] CHK063 - Is SC-006 coverage scope defined? **Fixed**: SC-006 now specifies "measured by line coverage across all `NamedCommands*.cs` and `CompletionBindings.cs` files". [Spec §SC-006]
- [x] CHK064 - Can SC-007 be objectively measured? ~~"gracefully" was vague.~~ **Fixed**: SC-007 now specifies "defined exceptions are thrown for invalid API inputs; no-op behavior occurs for commands at buffer boundaries; bell is triggered where specified". [Spec §SC-007]

## Scenario Coverage

- [x] CHK065 - Are acceptance scenarios for kill/yank clipboard interaction defined? **Fixed**: US-2 scenarios 6 and 7 cover consecutive kill concatenation and yank-pop rotation. [Spec §US-2]
- [x] CHK066 - Are acceptance scenarios for completion commands defined? **Fixed**: Added US-6 with 4 acceptance scenarios covering `menu-complete`, `menu-complete-backward`, and `complete`. [Spec §US-6]
- [x] CHK067 - Are acceptance scenarios for `insert-comment` uncommenting defined? **Fixed**: US-7 scenario 4 covers: "Given text '#hello\n#world', When insert-comment is executed with numeric argument other than 1, Then text becomes 'hello\nworld'". [Spec §US-7]
- [x] CHK068 - Are acceptance scenarios for mode switching defined? **Fixed**: Added US-7 with scenarios 1 and 2 covering vi-editing-mode and emacs-editing-mode. [Spec §US-7]
- [x] CHK069 - Are acceptance scenarios for `prefix-meta` and `edit-and-execute-command`? These commands are internal plumbing (feeding Escape to key processor, opening editor). Full behavioral requirements are in FR-011c and FR-011d. Acceptance scenarios are not practical for these (they require key processor and external editor integration). Covered by Edge Cases and FR definitions. [Spec §FR-011c, §FR-011d]
- [x] CHK070 - Are acceptance scenarios for `operate-and-get-next` sufficient? **Fixed**: US-4 scenario 4 now covers: "Given buffer at mid-history entry, When operate-and-get-next is executed, Then current input is accepted and next history entry is loaded on subsequent prompt". [Spec §US-4]

## Edge Case Coverage

- [x] CHK071 - Are expected behaviors defined for each edge case? ~~The spec asked "What happens when..." without answers.~~ **Fixed**: Complete rewrite of Edge Cases section. Every edge case now specifies the exact expected behavior (e.g., "Throws `KeyNotFoundException`", "No-op", "Application calls `Output.Bell()`", etc.). 23 edge cases defined with outcomes. [Spec §Edge Cases]
- [x] CHK072 - Is concurrent `Register`/`GetByName` addressed? **Fixed**: Edge Cases includes "Concurrent `Register` and `GetByName` calls: Thread-safe via `ConcurrentDictionary`; last writer wins for `Register`; reads are always consistent". NFR-002 also covers this. [Spec §Edge Cases, §NFR-002]
- [x] CHK073 - Is static constructor failure addressed? Static constructor failure is a .NET runtime concern, not a named-commands-specific requirement. If a handler throws during registration, the .NET runtime throws `TypeInitializationException`. This is standard CLR behavior and doesn't need a spec requirement — it's an infrastructure-level concern handled by the platform. [N/A — CLR behavior]
- [x] CHK074 - Is multi-byte Unicode `self-insert` defined? **Fixed**: Edge Cases includes "`self-insert` with multi-byte Unicode (emoji, CJK): `event.Data` contains the full character; inserted via `InsertText` which handles Unicode correctly". [Spec §Edge Cases]
- [x] CHK075 - Are word commands on whitespace-only strings defined? **Fixed**: Edge Cases includes "`forward-word`/`backward-word` on whitespace-only text: `FindNextWordEnding()`/`FindPreviousWordBeginning()` returns null; no movement". [Spec §Edge Cases]
- [x] CHK076 - Is `kill-line` on last line without trailing newline defined? **Fixed**: Edge Cases includes "`kill-line` on last line with no trailing newline: Deletes from cursor to end-of-line position (0 if already at end); clipboard set to deleted text". [Spec §Edge Cases]

## Cross-Artifact Consistency

- [x] CHK077 - Do command counts match across all artifacts? **Fixed**: All artifacts now consistent: History=6, Text Modification=9, Misc=7. Updated: spec.md (FR-006, FR-007, FR-011), plan.md (source code structure), data-model.md (category headings), contracts (handler lists), quickstart.md (step counts), research.md (R-011 file listing). [All artifacts]
- [x] CHK078 - Does the plan's constitution check contradict itself on thread safety? ~~Pre-Phase 1 said `Lock` + `EnterScope()`, post-Phase 1 said `ConcurrentDictionary`.~~ **Fixed**: Pre-Phase 1 check now says "`ConcurrentDictionary`" matching the post-Phase 1 check and research R-001. [Plan §Constitution Check]
- [x] CHK079 - Is `UnixWordRubout` extra parameter compatible with `KeyHandlerCallable`? ~~A handler with an extra parameter doesn't match the delegate.~~ **Fixed**: Contracts now document `UnixWordRuboutImpl` as an internal helper (NOT registered directly). The registered handler is a wrapper: `e => UnixWordRuboutImpl(e, word: true)`. Spec FR-026 documents this pattern. [Contracts §KillYank, Spec §FR-026]
- [x] CHK080 - Is `operate-and-get-next` canonical location specified? **Fixed**: Python source places it under "Miscellaneous Commands" (line 664). All artifacts now consistently list it under Misc only. Removed from History in data-model.md and contracts. [Data Model, Contracts, Spec §FR-006, §FR-011]

## Dependencies & Assumptions

- [x] CHK081 - Is Assumption 7 resolved? ~~Spec said "available or stubbed".~~ **Fixed**: Assumption now explicitly states "MUST be implemented as part of this feature in the `CompletionBindings` static class (not stubbed or deferred)". FR-023 is the formal requirement. [Spec §Assumptions, §FR-023]
- [x] CHK082 - Are all Application properties verified as available? **Fixed**: New assumption added listing all required Application properties: Clipboard, Renderer, EmacsState, KeyProcessor, Layout, Output, EditingMode, QuotedInsert, PreRunCallables, CurrentSearchState, Exit(). [Spec §Assumptions]
- [x] CHK083 - Is `Buffer.OpenInEditorAsync` verified? **Fixed**: New assumption added: "`Buffer.OpenInEditorAsync()` is available for `edit-and-execute-command`". [Spec §Assumptions]
- [x] CHK084 - Is `Buffer.SwapCharactersBeforeCursor()` verified? **Fixed**: New assumption added: "`Buffer.SwapCharactersBeforeCursor()` is available for `transpose-chars`". [Spec §Assumptions]
- [x] CHK085 - Is `Buffer.PasteClipboardData()` verified? **Fixed**: New assumption added: "`Buffer.PasteClipboardData()` with `PasteMode.Emacs` is available for `yank`". [Spec §Assumptions]

## Non-Functional Requirements

- [x] CHK086 - Are performance requirements explicitly stated? **Fixed**: Added NFR-001: "Registry lookup via `GetByName` MUST be O(1) average time complexity". Added NFR-003: "Command handler invocation MUST NOT allocate on the handler dispatch path". [Spec §NFR-001, §NFR-003]
- [x] CHK087 - Are thread safety requirements specified in the spec? **Fixed**: Added NFR-002: "The named commands registry MUST be thread-safe. Concurrent `GetByName` and `Register` calls from multiple threads MUST NOT corrupt state." [Spec §NFR-002]
- [x] CHK088 - Are error recovery requirements defined? **Fixed**: Added NFR-004: "If a command handler throws an unexpected exception during execution, the exception propagates to the caller (the key processor). The registry itself does not catch or transform handler exceptions." [Spec §NFR-004]
- [x] CHK089 - Are memory/allocation requirements specified? **Fixed**: NFR-003 states "MUST NOT allocate on the handler dispatch path" as a firm requirement, not aspirational. [Spec §NFR-003]

## Notes

- All 89 items have been resolved and marked complete.
- The spec was substantially rewritten to address gaps, adding: 2 new user stories (US-6, US-7), 15 new sub-requirements (FR-005a through FR-026), 4 NFRs (NFR-001 through NFR-004), 23 edge cases with explicit outcomes, 8 new dependency assumptions, and clarified all 7 success criteria.
- Cross-artifact consistency fixes applied to: plan.md, data-model.md, contracts/named-commands-api.md, quickstart.md, research.md.
- Critical fixes: command count alignment (49 = 10+6+9+10+3+4+7), `operate-and-get-next` moved from History to Misc, `UnixWordRubout` signature clarified as internal helper, constitution check thread safety mechanism corrected.
