# Release Gate Checklist: Vi Key Bindings

**Purpose**: Comprehensive requirements quality validation across spec, contracts, data model, and plan — covering full spec quality, operator composition, mode transitions, and port fidelity
**Created**: 2026-01-31
**Completed**: 2026-01-31
**Feature**: [spec.md](../spec.md)
**Depth**: Release gate
**Audience**: Reviewer (PR / implementation start gate)

## Requirement Completeness

- [x] CHK001 - Are requirements defined for all ~151 binding registrations identified in the Python source, or is the "~151" count explicitly reconciled with the bindings enumerated in the contracts? **Resolution**: Count reconciled via Python source analysis. Precise breakdown now in SC-008: 112 direct `@handle`/`handle()` + 74 text objects (42 explicit + 32 dynamic) + 14 operators. Updated in spec, plan, research, quickstart.
- [x] CHK002 - Are inner paragraph (`ip`) and inner sentence (`is`, `as`) text objects listed in the text object binding table? **Resolution**: Python source confirmed: `ip`, `is`, `as` do NOT exist in Python Prompt Toolkit. Only `ap` is implemented. FR-007 corrected to remove them. Text object table updated.
- [x] CHK003 - Is the dot command (`.`) repeat behavior addressed? **Resolution**: Python source confirmed: the dot command does NOT exist in Python Prompt Toolkit. US-7 removed, FR-010 removed, SC-005 removed, Edge Case 5 removed. Port fidelity: not inventing APIs absent from Python.
- [x] CHK004 - Are requirements defined for `Ctrl-W` (delete word backward) in insert mode? **Resolution**: Python vi.py confirmed: `Ctrl-W` is NOT in vi.py — handled by basic/readline bindings. FR-016 (was FR-017) corrected. Insert mode table notes the exclusion.
- [x] CHK005 - Are requirements defined for `Ctrl-H` (backspace) in insert mode? **Resolution**: Same as CHK004 — `Ctrl-H` NOT in vi.py. FR-016 corrected.
- [x] CHK006 - Is the register selection prefix `"x` documented? **Resolution**: FR-011 updated with 3-key sequence mechanism (`"`, Any, operator/paste). Operator table has `"x,d`/`"x,c`/`"x,y`. Misc table has `",{reg},p`/`",{reg},P`. Valid names: a-z, 0-9.
- [x] CHK007 - Are scroll commands fully enumerated with delegation clarified? **Resolution**: Misc table now has z-commands (z,z/z,t/z,b) with note: "Ctrl-F/B/D/U/E/Y/PageDown/PageUp already in PageNavigationBindings.LoadViPageNavigationBindings() — NOT duplicated." Assumptions section updated.
- [x] CHK008 - Is `Ctrl-O` (quick normal mode) fully specified? **Resolution**: Mode switch table updated with full detail: `ViState.TemporaryNavigationMode = true`, KeyProcessor manages return, persists during operator-pending/count accumulation, edge case for sub-mode changes documented. Data model updated.
- [x] CHK009 - Is `G` key fully specified for both behaviors? **Resolution**: Navigation table: "G → Linewise. ALSO has separate @handle('G', filter=has_arg)". Misc table: "G | has_arg → GoToHistory(arg-1)". Edge Cases: "G goes to history entry (0-indexed), NOT document line. gg goes to document line."
- [x] CHK010 - Are requirements for `Ctrl-W` in insert mode distinct from BasicBindings? **Resolution**: Moot — `Ctrl-W` is NOT in vi.py. No conflict exists. FR-016 documents the exclusion.

## Requirement Clarity

- [x] CHK011 - Is SC-002 specific enough to verify objectively? **Resolution**: SC-002 updated with verification method: "unit tests that set up a Buffer with known text, execute the operator+motion via the handler methods, and assert the resulting Document text, cursor position, and ClipboardData."
- [x] CHK012 - Is "~52 text objects" quantified precisely? **Resolution**: Changed to "74 text object registrations (42 explicit @text_object + 32 dynamic via create_ci_ca_handles)" throughout all artifacts (spec, plan, research, quickstart, contracts).
- [x] CHK013 - Is `TextObject.OperatorRange()` for Inclusive type precise? **Resolution**: contracts/text-object.md updated: Exclusive → `to = cursor + max(start, end)`, Inclusive → same + 1, Linewise → expand to line boundaries, Block → same as Exclusive.
- [x] CHK014 - Is cursor behavior at column 0 on Escape specified? **Resolution**: US-3 Scenario 4 updated: "If cursor is at position 0, cursor stays at position 0." Edge Cases updated. Mode switch table: "clamped at col 0."
- [x] CHK015 - Is count interaction defined? **Resolution**: FR-015 updated: "counts are multiplied (operator_arg × motion_arg = 6)". Data model Operator-Pending Flow step 7 shows the multiplication formula. `0` dual behavior documented.
- [x] CHK016 - Is linewise paste cursor positioning specified? **Resolution**: FR-012 updated: "cursor MUST be positioned at the first character of the first pasted line." US-6 Scenario 2 updated. Misc table p/P entries clarified.
- [x] CHK017 - Are filter conditions precisely specified? **Resolution**: All binding tables now have explicit Filter columns showing the exact mode filter for each binding. Navigation table has Mode Filter + Notes columns.
- [x] CHK018 - Is `tilde_operator` configuration specified? **Resolution**: FR-017 updated: "when ViState.TildeOperator is true (mirrors Vim's tildeop option)." Transform Functions section added 5-entry table with tilde conditional behavior. Misc table: `~ | vi_navigation_mode & ~tilde_operator`.

## Requirement Consistency

- [x] CHK019 - Are `J` and `g,J` consistently assigned across tables? **Resolution**: Visual mode table header: "Keys J, g,J, x also appear in Misc table — the mode filter (vi_selection_mode vs vi_navigation_mode) disambiguates." Both tables have explicit filter columns.
- [x] CHK020 - Is `x` key consistent between visual and navigation? **Resolution**: Visual x: `vi_selection_mode` (cut selection). Misc x: `vi_navigation_mode` (delete char). Both tables have explicit filter columns.
- [x] CHK021 - Is `~` behavior consistent across operator and misc tables? **Resolution**: Two distinct handlers with mutually exclusive filters. Operator table: `~ | TildeOperatorFilter` (when TildeOperator=true). Misc table: `~ | vi_navigation_mode & ~tilde_operator` (when TildeOperator=false). Transform Functions section documents both.
- [x] CHK022 - Does FR-003 return type match contract? **Resolution**: FR-003 corrected from `KeyBindingsBase` to `IKeyBindingsBase`. FR-004 also corrected.
- [x] CHK023 - Are h/l navigation bindings consistent with US-1? **Resolution**: Navigation table now shows "Navigation + Selection" and notes "Registered as text object" — RegisterTextObject creates both nav and selection handlers. This is faithful to Python where text objects get all three binding types.
- [x] CHK024 - Is `0` key dual behavior consistent? **Resolution**: Navigation table: "Dual behavior: digit-0 when has_arg." Misc table: explicit `0` entry with `(... & has_arg)` filter. Edge Cases §8 updated. Data model mentions `0` pattern.

## Operator Composition Quality

- [x] CHK025 - Is operator-pending lifecycle fully specified? **Resolution**: Data model expanded from 8 to 12 steps including count multiplication, cancellation behavior (unknown key = bell, NOT cancelled; Escape = cancel via InputMode setter), and doubled-key note.
- [x] CHK026 - Are dd/cc/yy mechanisms specified? **Resolution**: Data model: "dd, cc, yy, >>, <<, guu, gUU, g~~ are all special-case @handle bindings, NOT operator+motion composition." Misc table entries explicitly noted. US-2 description updated.
- [x] CHK027 - Is count multiplication specified? **Resolution**: Data model step 7: `event._arg = (ViState.OperatorArg ?? 1) * (event.Arg ?? 1)`. FR-015 updated with `2d3w` example. US-2 description updated.
- [x] CHK028 - Is RegisterTextObject with both no-flags specified? **Resolution**: Contracts RegisterTextObject section: "up to 3 handler registrations." Text object table split into "Operator-pending text objects" (noMoveHandler=true, e.g., iw, aw) and "Motion text objects" (all 3 handlers). FR-018 detailed.
- [x] CHK029 - Is operator cancellation specified? **Resolution**: Data model: "unrecognized key → bell, operator NOT cancelled. Escape required." Edge Cases updated. Misc table: Keys.Any catch-all entry.
- [x] CHK030 - Is RegisterOperator selection handler specified? **Resolution**: Contracts RegisterOperator: "selection — creates TextObject from current SelectionState and executes operator immediately." FR-019 detailed.
- [x] CHK031 - Is change operator cursor positioning specified? **Resolution**: Operator table now has "Cursor After" column. Change: "Start of deleted range (in insert mode)." Key Entities updated.
- [x] CHK032 - Is text object type per motion specified? **Resolution**: Navigation table now has Type column (e.g., w=Exclusive, e=Inclusive, $=Inclusive, j=Linewise). Text object table has Type column for all entries.

## Mode Transition Quality

- [x] CHK033 - Is ReplaceSingle transition fully specified? **Resolution**: Mode switch table: "insert w/ overwrite, cursor back by 1 = on replaced char." Data model state machine: "ReplaceSingle ──(any char)──→ Navigation [insert w/ overwrite, cursor back by 1 = on replaced char]."
- [x] CHK034 - Is Escape during operator-pending specified? **Resolution**: Mode switch table now has separate Escape entry for "Navigation (with pending op) → Navigation" that "Clears ViState.OperatorFunc, ViState.OperatorArg." Data model has explicit OperatorPending ──(Escape)──→ Navigation line.
- [x] CHK035 - Is InsertMultiple mode fully specified? **Resolution**: Data model: "Selection(Block) ──(I)──→ InsertMultiple [block-only]" and "InsertMultiple ──(Escape)──→ Navigation [applies buffered edits across all block lines]." Visual mode table: I/A gated on `in_block_selection`.
- [x] CHK036 - Is Ctrl-O return path specified for mode-change edge cases? **Resolution**: Mode switch table Ctrl-O entry: "If the command itself triggers a mode change (e.g., entering visual mode), the temporary flag persists until that sub-mode completes." Edge Cases updated.
- [x] CHK037 - Is visual mode sub-mode toggling fully specified? **Resolution**: Visual mode table: "same key = exit (ExitSelection), different key = switch type." Data model state machine has full toggle pattern for all 3 sub-modes.
- [x] CHK038 - Are read-only filters consistently applied? **Resolution**: FR-024 updated: "MUST gate all text-modifying bindings (mode switches to insert/replace, operators d/c/x/s/p/P, indent/unindent, case transforms, join)." Mode switch table: `[gated on ~is_read_only]` for i/I/a/A/o/O/R.
- [x] CHK039 - Is Escape from visual mode cursor positioning specified? **Resolution**: Mode switch table: "Escape from Selection → cursor stays, ExitSelection()." Edge Cases: "from visual/selection mode, cursor stays at current position (no left-by-one)."
- [x] CHK040 - Are undo boundaries specified? **Resolution**: Data model: "Undo boundaries: Mode transitions that use save_before=True create undo save points." US-6 Scenario 4 updated with undo granularity note.

## Port Fidelity

- [x] CHK041 - Is binding-by-binding reconciliation done? **Resolution**: Python source analyzed: 100 @handle (109 runtime) + 16 handle() + 42 @text_object + 32 dynamic text_object + 8 @operator source (14 runtime). SC-008 now has precise breakdown. Plan and research updated.
- [x] CHK042 - Does spec account for Python's return structure? **Resolution**: Python `load_vi_bindings()` returns a single `KeyBindings` object, not multiple groups. Stroke wraps in single `ConditionalKeyBindings`. FR-003 confirms faithful port.
- [x] CHK043 - Are all 5 Python condition helpers represented? **Resolution**: Contracts list all 5: IsReturnable, InBlockSelection, DigraphSymbol1Given, SearchBufferIsEmpty, TildeOperatorFilter. Matches Python's 5 `@Condition` definitions.
- [x] CHK044 - Are factory functions faithfully represented? **Resolution**: RegisterTextObject params match Python: keys, filter, noMoveHandler, noSelectionHandler, eager. RegisterOperator params match: keys, filter, eager. FR-018/FR-019 detailed.
- [x] CHK045 - Are scroll bindings delegation clarified? **Resolution**: Python vi.py has NO Ctrl-F/B/D/U bindings — those are in a separate scroll module. Stroke has them in PageNavigationBindings.LoadViPageNavigationBindings(). z-commands (zz/zt/zb) ARE in vi.py and go in ViBindings.Misc.
- [x] CHK046 - Is `_get_vi_state()` helper accounted for? **Resolution**: Python source confirmed: `_get_vi_state()` does NOT exist. Vi state accessed directly via `event.app.vi_state` / `get_app().vi_state`. C# equivalent: `@event.GetApp().ViState`. No action needed.
- [x] CHK047 - Are all Keys.Any handlers accounted for? **Resolution**: Insert mode table now lists ALL Keys.Any handlers with explicit filters: vi_replace_mode, vi_replace_single_mode, vi_insert_multiple_mode, vi_digraph_mode × 2 (first/second symbol), vi_waiting_for_text_object_mode (catch-all). Plus f/F/t/T and register sequences.
- [x] CHK048 - Are vi_transform_functions faithfully mapped? **Resolution**: Transform Functions section now has 5-entry table matching Python exactly: g?, gu, gU, g~, ~ (with TildeOperatorFilter). Tilde conditional behavior documented.

## Scenario Coverage

- [x] CHK049 - Are boundary motions specified? **Resolution**: Edge Cases §1 expanded: "w past end of document, b at start of document, gg on first line" — cursor stays, motion returns zero-offset TextObject.
- [x] CHK050 - Is empty document + dd specified? **Resolution**: Edge Cases updated: "On a completely empty buffer (no text at all), dd is a no-op."
- [x] CHK051 - Is visual mode + multi-line operators specified? **Resolution**: FR-014 updated. Visual mode table has operators. RegisterOperator selection handler creates TextObject from SelectionState and executes immediately.
- [x] CHK052 - Is nested bracket handling specified? **Resolution**: Edge Cases updated: bracket text objects use `FindEnclosingBracketLeft/Right` which handles nesting by finding the innermost matching pair. `ci"` outside quotes → zero-range, no-op.
- [x] CHK053 - Are text objects at buffer extremes specified? **Resolution**: Edge Cases covers boundary motions returning zero-offset TextObject. `a"` with no quotes → zero-range, no-op per FindEnclosingBracketLeft returning null.
- [x] CHK054 - Is visual block mode + text objects specified? **Resolution**: Visual mode table has I/A for block insert/append. RegisterTextObject selection handler extends selection. For motions like `iw` in block mode, the selection extend handler applies normally.
- [x] CHK055 - Is macro recording with mode switches specified? **Resolution**: US-8 (was US-9) Scenario 5 added: macro with i, type "test", Escape replays including mode transitions.
- [x] CHK056 - Are register semantics specified? **Resolution**: FR-011 specifies valid register names (a-z, 0-9 per Python's `vi_register_names`). Note added: Python does NOT implement special registers ("", "-, numbered "0-"9). Only named registers.

## Edge Case Coverage

- [x] CHK057 - Is f{char} at cursor position specified? **Resolution**: Edge Cases: "f searches forward from position after cursor; finds next occurrence, not current." US-5 Scenario 7 added with explicit example.
- [x] CHK058 - Is ci" outside quotes specified? **Resolution**: Edge Cases: "zero-range, no-op."
- [x] CHK059 - Is text object at end of document specified? **Resolution**: Edge Cases: "OperatorRange clamps to document length."
- [x] CHK060 - Is yy on last line specified? **Resolution**: Edge Cases: "line text yanked with linewise paste mode."
- [x] CHK061 - Is linewise p on last line specified? **Resolution**: Edge Cases: "new line added after last line, cursor at first character of pasted line."
- [x] CHK062 - Is Ctrl-A/Ctrl-X for edge cases specified? **Resolution**: Edge Cases: "no number at cursor → no-op."
- [x] CHK063 - Is ;/, with no previous find specified? **Resolution**: Edge Cases: "if LastCharacterFind is null, motion is no-op (zero-offset TextObject)."

## Acceptance Criteria Quality

- [x] CHK064 - Is SC-001 objectively measurable? **Resolution**: SC-001 updated: "Verification method: unit tests with known documents, cursor positions, and expected offsets derived from reading the Python source logic."
- [x] CHK065 - Is SC-007 (was SC-008) scope clarified? **Resolution**: Updated: "measured against the Vi binding source files only (ViBindings.*.cs, TextObject.cs, TextObjectType.cs)."
- [x] CHK066 - Is SC-008 (was SC-009) verifiable? **Resolution**: Updated with precise breakdown (112 + 74 + 14) derived from Python source analysis. No longer approximate.
- [x] CHK067 - Are 13 Vi-specific tests listed? **Resolution**: SC-010 added: lists all 13 test names from test-mapping.md.
- [x] CHK068 - Does SC-009 (was SC-010) define specific test inputs? **Resolution**: Updated with specific examples: "TextObject(5, type=Exclusive) with cursor at 10 → OperatorRange returns (10, 15)."

## Dependencies & Assumptions

- [x] CHK069 - Is named register assumption validated? **Resolution**: Assumptions section now lists specific ViState API signatures: `GetNamedRegister(string)`, `SetNamedRegister(string, ClipboardData?)`, `ClearNamedRegister(string)`, `GetNamedRegisterNames()`.
- [x] CHK070 - Is named commands assumption validated? **Resolution**: Assumptions section lists specific commands needed: `quoted-insert` (Ctrl-V), `accept-line` (Enter), `edit-and-execute-command` (already in OpenInEditorBindings). All confirmed present in 49-command registry.
- [x] CHK071 - Is TransformLines/TransformRegion dependency documented? **Resolution**: Assumptions section now documents `Buffer.TransformRegion`, `Buffer.TransformLines`, `Buffer.TransformCurrentLine`, and `BufferOperations.Indent`/`Unindent`/`ReshapeText` with method signatures.
- [x] CHK072 - Is FindEnclosingBracket dependency documented? **Resolution**: Assumptions section now documents `Document.FindEnclosingBracketLeft(leftChar, rightChar)`, `FindEnclosingBracketRight(leftChar, rightChar)`, `FindMatchingBracketPosition` with bracket pairs supported.
- [x] CHK073 - Is WindowRenderInfo dependency documented? **Resolution**: Assumptions section now documents `WindowRenderInfo.FirstVisibleLine(afterScrollOffset)`, `CenterVisibleLine(...)`, `LastVisibleLine(beforeScrollOffset)` with specific parameter usage for H/M/L motions.
- [x] CHK074 - Is SearchOperations sufficiency validated? **Resolution**: Assumptions section clarifies: n/N/*/# must be in ViBindings (not SearchBindings). SearchBindings handles //?/Ctrl-S/etc only.

## Cross-Artifact Consistency

- [x] CHK075 - Does quickstart.md align with task dependencies? **Resolution**: All phases now have explicit dependency annotations: "Phase 2 depends on Phase 1", "Phase 3 depends on Phase 2", "Phase 4 depends on Phase 3", etc.
- [x] CHK076 - Does plan structure align with quickstart? **Resolution**: Plan has 8 source files, quickstart has 8 phases. File names match between artifacts. Both reference the same partial class split.
- [x] CHK077 - Does data model state machine account for all transitions? **Resolution**: State machine expanded to include: visual toggle (same key = exit, different = switch), InsertMultiple entry/exit, Ctrl-O temporary mode, operator cancellation (unknown key = stays pending), read-only gating.
- [x] CHK078 - Do LOC estimates remain under 1,000? **Resolution**: Research.md updated with safety margin note: "All files estimated well under 1,000 LOC. Largest (TextObjects at ~700) has ~30% margin for growth."
- [x] CHK079 - Does RegisterTextObject filter align with navigation table? **Resolution**: Text object table split into "Operator-pending text objects" (noMoveHandler=true) and "Motion text objects" (all 3 handlers). Navigation table now explicitly marks "(via text object)" items with Type and filter notes.

## Notes

- All 79 items completed on 2026-01-31
- Major findings that drove spec corrections:
  - **Dot command (`.`) absent from Python Prompt Toolkit** — US-7, FR-010, SC-005, Edge Case 5 removed
  - **`ip`, `is`, `as` text objects absent from Python** — FR-007 corrected (only `ap` exists)
  - **`Ctrl-W`, `Ctrl-H` absent from vi.py** — FR-016 corrected (handled by basic bindings)
  - **Binding count "~151" was inaccurate** — reconciled to 112 + 74 + 14 precise breakdown
  - **`dd`/`cc`/`yy` are special-case bindings**, not operator+motion composition
  - **Count interaction is multiplication**: `2d3w` → 6 words (not 3 words twice)
  - **`G` with arg goes to history**, not document line; `gg` with arg goes to document line
  - **Operator cancellation via unknown key**: bell only, operator NOT cancelled (Escape required)
