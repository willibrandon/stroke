# Tasks: Vi Key Bindings

**Input**: Design documents from `/specs/043-vi-key-bindings/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/text-object.md, contracts/vi-bindings.md, quickstart.md

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Foundation Types

**Purpose**: Create the core types that all Vi bindings depend on: TextObjectType enum, TextObject class, and OperatorFuncDelegate update. No dependencies on other phases.

- [ ] T001 [P] Implement `TextObjectType` enum with 4 values (Exclusive, Inclusive, Linewise, Block) in `src/Stroke/KeyBinding/TextObjectType.cs` per contract `contracts/text-object.md`
- [ ] T002 [P] Implement `TextObjectType` unit tests (value existence, exhaustive coverage, all 4 values) in `tests/Stroke.Tests/KeyBinding/TextObjectTypeTests.cs`
- [ ] T003 Implement `TextObject` sealed class with constructor, Start/End/Type properties, SelectionType computed property (Exclusive/Inclusive→Characters, Linewise→Lines, Block→Block), Sorted(), OperatorRange(Document), GetLineNumbers(Buffer), Cut(Buffer) in `src/Stroke/KeyBinding/TextObject.cs` per contract `contracts/text-object.md`. OperatorRange semantics: Exclusive → from/to direct; Inclusive → to += 1; Linewise → expand to full line boundaries; Block → same as Exclusive
- [ ] T004 Implement `TextObject` unit tests (Sorted with positive/negative offsets, OperatorRange for all 4 TextObjectType values, GetLineNumbers, Cut with characterwise and linewise, SelectionType mapping) in `tests/Stroke.Tests/KeyBinding/TextObjectTests.cs`
- [ ] T005 Update `OperatorFuncDelegate` parameter type from `object? textObject` to `TextObject textObject` in `src/Stroke/KeyBinding/OperatorFuncDelegate.cs` per contract `contracts/text-object.md`. Verify no compile errors in downstream code (ViState.OperatorFunc references)

**Checkpoint**: Foundation types ready. TextObjectType, TextObject, and OperatorFuncDelegate are available for all binding implementations.

---

## Phase 2: ViBindings Scaffolding

**Purpose**: Create the main ViBindings partial class file with `LoadViBindings()` entry point, condition helper filters, and transform function definitions. Depends on Phase 1 (TextObject/TextObjectType).

**⚠️ CRITICAL**: All partial class files in Phases 3-8 depend on this scaffolding being complete.

- [ ] T006 Create `ViBindings` static partial class in `src/Stroke/Application/Bindings/ViBindings.cs` with: (1) 5 `IFilter` condition helpers — `IsReturnable` (current buffer has accept handler), `InBlockSelection` (selection type is Block), `DigraphSymbol1Given` (first digraph char entered), `SearchBufferIsEmpty` (search buffer text is empty), `TildeOperatorFilter` (ViState.TildeOperator is true); (2) `ViTransformFunctions` static array with 5 entries — (g,? → rot13), (g,u → lowercase), (g,U → uppercase), (g,~ → swap case), (~ → swap case with TildeOperatorFilter); (3) `LoadViBindings()` method that creates a `KeyBindings` instance, calls partial-file registration methods (placeholders initially), and wraps in `ConditionalKeyBindings` gated on `ViFilters.ViMode`, returning `IKeyBindingsBase`. Follow pattern from `EmacsBindings.cs`

**Checkpoint**: Scaffolding ready. Registration methods in partial files can now be implemented.

---

## Phase 3: Registration Helpers & Operators (US2 foundation)

**Purpose**: Implement the `RegisterTextObject` and `RegisterOperator` helper methods and all operator handler functions. This is the foundation for US1 (navigation via text objects), US2 (operators + motions), and US4 (text objects). Depends on Phase 2.

- [ ] T007 Implement `RegisterTextObject` private static helper in `src/Stroke/Application/Bindings/ViBindings.Operators.cs` that registers up to 3 bindings per text object: (1) operator-pending mode (`ViWaitingForTextObjectMode`) — if `ViState.OperatorFunc` is not null, multiply counts via `(ViState.OperatorArg ?? 1) * (event.Arg ?? 1)`, call handler to get TextObject, call OperatorFunc, then clear OperatorFunc/OperatorArg; (2) navigation mode (`ViNavigationMode`) — move cursor by text object's start offset (unless `noMoveHandler=true`); (3) selection mode (`ViSelectionMode`) — extend selection by text object's start offset (unless `noSelectionHandler=true`). Parameters: `KeyBindings kb, KeyOrChar[] keys, Func<KeyPressEvent, TextObject> handler, FilterOrBool filter = default, bool noMoveHandler = false, bool noSelectionHandler = false, bool eager = false`
- [ ] T008 Implement `RegisterOperator` private static helper in `src/Stroke/Application/Bindings/ViBindings.Operators.cs` that registers 2 bindings per operator: (1) navigation mode — stores `operatorFunc` in `ViState.OperatorFunc`, stores `event.Arg` in `ViState.OperatorArg`; (2) selection mode — creates `TextObject` from current `SelectionState` and calls `operatorFunc` immediately. Parameters: `KeyBindings kb, KeyOrChar[] keys, OperatorFuncDelegate operatorFunc, FilterOrBool filter = default, bool eager = false`
- [ ] T009 Implement `CreateDeleteAndChangeOperators` factory method in `src/Stroke/Application/Bindings/ViBindings.Operators.cs` that registers 4 operators: (d → delete), (c → change: delete + enter insert mode with cursor at start of deleted range), ("Any",d → delete to named register), ("Any",c → change with named register). Delete operator: calls `textObject.Cut(buffer)`, sets document, stores clipboard data. Change operator: same as delete but also sets `InputMode = Insert`
- [ ] T010 Implement `CreateTransformHandler` factory method in `src/Stroke/Application/Bindings/ViBindings.Operators.cs` that iterates `ViTransformFunctions` and registers 5 transform operators (g,? / g,u / g,U / g,~ / ~) via `RegisterOperator`. Each transform operator applies its string transform function to the text object range using `Buffer.TransformRegion`
- [ ] T011 Implement remaining explicit operator registrations in `src/Stroke/Application/Bindings/ViBindings.Operators.cs`: (y → yank), ("Any",y → yank to named register), (> → indent via `BufferOperations.Indent`), (< → unindent via `BufferOperations.Unindent`), (g,q → reshape via `BufferOperations.ReshapeText`). Total: 14 operator registrations (4 from CreateDeleteAndChangeOperators + 5 from CreateTransformHandler + 5 explicit)
- [ ] T012 Add the `RegisterOperators` partial method call point in `ViBindings.cs` `LoadViBindings()` and implement the `RegisterOperators(KeyBindings kb)` internal method in `ViBindings.Operators.cs` that orchestrates calling CreateDeleteAndChangeOperators, CreateTransformHandler, and explicit operator registrations

**Checkpoint**: Registration helpers and all 14 operators are ready. Text object and navigation registrations can now proceed.

---

## Phase 4: US1 — Navigate Text in Vi Normal Mode (Priority: P1) 🎯 MVP

**Goal**: All Vi navigation motions (h, j, k, l, w, b, e, W, B, E, 0, $, ^, gg, G, {, }, %, |, H, M, L, +, -, n, N, ge, gE, gm, g_, arrow keys, Backspace, Enter) move the cursor correctly. Depends on Phase 3 (RegisterTextObject helper).

**Independent Test**: Place cursor at various positions in multi-line documents and verify each motion key moves cursor to correct position.

### Tests for US1

- [ ] T013 [P] [US1] Implement navigation binding registration tests in `tests/Stroke.Tests/Application/Bindings/ViBindings/LoadViBindingsNavigationTests.cs`: verify `LoadViBindings()` returns `ConditionalKeyBindings`, verify bindings exist for all navigation keys (h, l, j, k, w, W, b, B, e, E, 0, ^, $, gg, G, {, }, %, |, H, M, L, ge, gE, gm, g_, +, -, Enter, n, N, (, ), arrow keys, Backspace, Space). Use `GetBindings()` helper pattern from EmacsBindings tests. Supplements mapped test `CursorMovements` in `ViModeTests.cs`

### Implementation for US1

- [ ] T014 [US1] Implement all text-object-based navigation handler methods in `src/Stroke/Application/Bindings/ViBindings.TextObjects.cs` — word motions: w (FindNextWordBeginning, Exclusive), W (WORD variant), b (FindPreviousWordBeginning, Exclusive), B (WORD), e (FindNextWordEnding, Inclusive), E (WORD), ge (FindPreviousWordEnding, Inclusive), gE (WORD). Register each via `RegisterTextObject` with appropriate type
- [ ] T015 [US1] Implement line motion text object handlers in `src/Stroke/Application/Bindings/ViBindings.TextObjects.cs` — 0 (GetStartOfLinePosition hard, Exclusive), ^ (GetStartOfLinePosition afterWhitespace, Exclusive), $ (GetEndOfLinePosition, Inclusive), | (go to column from GetColumnCursorPosition, Exclusive), gm (middle of line, Exclusive), g_ (LastNonBlankOfCurrentLinePosition, Exclusive)
- [ ] T016 [US1] Implement document/screen motion text object handlers in `src/Stroke/Application/Bindings/ViBindings.TextObjects.cs` — gg (first line / with arg: document line n, Linewise), G (last line, Linewise; note: G-with-arg for history is separate in Misc), { (previous paragraph, Exclusive), } (next paragraph, Exclusive), % (FindMatchingBracketPosition, Inclusive), H (FirstVisibleLine, Linewise), M (CenterVisibleLine, Linewise), L (LastVisibleLine, Linewise)
- [ ] T017 [US1] Implement character find text object handlers in `src/Stroke/Application/Bindings/ViBindings.TextObjects.cs` — f,{char} (find forward, Inclusive, stores CharacterFind in ViState.LastCharacterFind), F,{char} (find backward, Exclusive), t,{char} (till forward, Inclusive), T,{char} (till backward, Exclusive), ; (repeat last find using ViState.LastCharacterFind, type matches original), , (reverse last find). Each uses `Keys.Any` for the character argument
- [ ] T018 [US1] Implement search motion text object handlers in `src/Stroke/Application/Bindings/ViBindings.TextObjects.cs` — n (search next, Exclusive, via `SearchOperations`), N (search previous, Exclusive). Note: these are text objects registered via `RegisterTextObject` in `LoadViBindings()`, NOT in `SearchBindings`. Must respect `ViFilters.ViSearchDirectionReversed` so that n/N swap direction when search direction is reversed (FR-021)
- [ ] T019 [US1] Implement h/l text object handlers in `src/Stroke/Application/Bindings/ViBindings.TextObjects.cs` — h and Left (move left, Exclusive), l, Space, and Right (move right, Exclusive). Register via `RegisterTextObject`
- [ ] T020 [US1] Implement direct navigation handlers in `src/Stroke/Application/Bindings/ViBindings.Navigation.cs` — j/k text objects (Linewise, noSelectionHandler=true), Down/Ctrl-N and Up/Ctrl-P (direct handlers for arrow navigation, not text objects), Backspace (move left in navigation mode), Enter/+ (start of next line via GetStartOfLinePosition on next line), - (start of previous line), sentence handlers ( and ) (register bindings with no-op handlers returning zero-offset TextObject, matching Python's `# TODO` stubs)
- [ ] T021 [US1] Add the `RegisterTextObjects(KeyBindings kb)` and `RegisterNavigation(KeyBindings kb)` partial method call points in `ViBindings.cs` `LoadViBindings()` and implement the orchestration methods in their respective files

**Checkpoint**: All Vi navigation motions work. Cursor movement can be tested independently with known documents and starting positions.

---

## Phase 5: US2 — Delete, Change, and Yank with Operators + Motions (Priority: P1) 🎯 MVP

**Goal**: Operators (d, c, y) compose with motions and text objects. Doubled operators (dd, yy, cc) work as special cases. Count multiplication works (2d3w = 6 words). Depends on Phase 3 (operators) and Phase 4 (text objects/motions).

**Independent Test**: Set up documents with known content, execute operator+motion combinations, verify resulting document text, cursor position, and register content.

### Tests for US2

- [ ] T022 [P] [US2] Implement operator binding tests in `tests/Stroke.Tests/Application/Bindings/ViBindings/LoadViBindingsOperatorTests.cs`: verify operator registrations exist (d, c, y, >, <, g,q, g,?, g,U, g,u, g,~, ~), verify delete handler produces correct document after `dw`/`d$`/`dd`, verify change handler enters insert mode, verify yank stores to clipboard without modifying text, verify count multiplication (2d3w). Supplements mapped test `Operators` in `ViModeTests.cs`

### Implementation for US2

- [ ] T023 [US2] Implement doubled-key special-case bindings in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — dd (delete line, store as linewise), yy and Y (yank line as linewise), cc and S (change line: delete content after leading whitespace, enter insert), C (change to end of line, equivalent to c$), D (delete to end of line, equivalent to d$). Each is a direct `@handle` registration gated on `vi_navigation_mode`, NOT operator+motion composition. Follow `save_before=True` pattern for undo boundaries
- [ ] T024 [US2] Implement register-aware paste from named registers in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — ",{reg},p and ",{reg},P (3-key sequences using `Keys.Any` for register name). Retrieves `ClipboardData` from `ViState.GetNamedRegister` and pastes with same logic as p/P

**Checkpoint**: All operator+motion combinations and doubled-key operators work. Delete, change, yank produce correct results with proper register storage.

---

## Phase 6: US3 — Switch Between Vi Modes (Priority: P1) 🎯 MVP

**Goal**: Mode switching keys (i, I, a, A, o, O, v, V, Ctrl-V, R, r, Escape, Insert, Ctrl-O) correctly transition between modes with proper cursor positioning. Depends on Phase 2 (scaffolding).

**Independent Test**: Press mode-switch keys and verify ViState.InputMode changes and cursor position adjusts correctly.

### Tests for US3

- [ ] T025 [P] [US3] Implement mode switch binding tests in `tests/Stroke.Tests/Application/Bindings/ViBindings/LoadViBindingsModeSwitchTests.cs`: verify all mode switch bindings exist (i, I, a, A, o, O, v, V, Ctrl-V, R, r, Escape, Insert), verify `i` sets InputMode=Insert with cursor unchanged, verify `a` sets InputMode=Insert with cursor+1, verify `o` inserts line below and enters insert, verify Escape from insert moves cursor left by 1 (clamped at 0), verify Escape from selection clears selection with cursor staying, verify read-only gating on insert/replace entries. Note: mode switching is tested indirectly by mapped tests `TempNavigationMode`, `BlockEditing`, and others in `ViModeTests.cs`; this file provides focused supplementary tests

### Implementation for US3

- [ ] T026 [US3] Implement Escape handlers in `src/Stroke/Application/Bindings/ViBindings.ModeSwitch.cs` — 4 Escape entries: (1) from insert/replace mode → Navigation, cursor left by 1 via `GetCursorLeftPosition()` (clamped at col 0); (2) from selection mode → Navigation, `Buffer.ExitSelection()`, cursor stays; (3) from navigation with pending operator → clear `ViState.OperatorFunc`/`OperatorArg`, clear selection if present; (4) from navigation without pending op → clear selection if present. Use appropriate mode filters for each
- [ ] T027 [US3] Implement insert mode entry handlers in `src/Stroke/Application/Bindings/ViBindings.ModeSwitch.cs` — i (cursor stays), I (cursor to first non-blank via `GetStartOfLinePosition(afterWhitespace: true)`), a (cursor right one), A (cursor to end of line), o (`Buffer.InsertLineBelow()`), O (`Buffer.InsertLineAbove()`). All gated on `vi_navigation_mode & ~is_read_only`. All set `InputMode = Insert` with `save_before=True` for undo boundaries
- [ ] T028 [US3] Implement visual mode entry and replace mode entry handlers in `src/Stroke/Application/Bindings/ViBindings.ModeSwitch.cs` — v (set `SelectionState` with Characters), V (Lines), Ctrl-V (Block). R (set `InputMode = Replace`, gated on `~is_read_only`), r (set `InputMode = ReplaceSingle`, gated on `~is_read_only`). Insert key toggle (Navigation↔Insert)
- [ ] T029 [US3] Add the `RegisterModeSwitch(KeyBindings kb)` partial method call point in `ViBindings.cs` `LoadViBindings()` and implement the orchestration method in `ViBindings.ModeSwitch.cs`

**Checkpoint**: All mode transitions work correctly. Can enter/exit insert, replace, visual, and navigation modes with proper cursor behavior.

---

## Phase 7: US4 — Select Text with Text Objects (Priority: P2)

**Goal**: All inner/around text objects (iw, aw, iW, aW, ap, i"/a", i'/a', i`/a`, i(/a(, ib/ab, i[/a[, i{/a{, iB/aB, i</a<) work correctly with operators and in visual mode. Depends on Phase 3 (RegisterTextObject) and Phase 4 (word motions for reference).

**Independent Test**: Position cursor inside various structures and verify correct range is selected/operated on.

### Tests for US4

- [ ] T030 [P] [US4] Implement text object binding tests in `tests/Stroke.Tests/Application/Bindings/ViBindings/LoadViBindingsTextObjectTests.cs`: verify all text object bindings exist (iw, aw, iW, aW, ap, i", a", i', a', i`, a`, i(, a(, i[, a[, i{, a{, i<, a<, ib, ab, iB, aB), verify `diw` deletes word under cursor, verify `ci"` deletes content between quotes and enters insert, verify `da(` deletes including parentheses, verify text objects work in visual mode (extend selection). Supplements mapped test `TextObjects` in `ViModeTests.cs`

### Implementation for US4

- [ ] T031 [US4] Implement inner/around word text objects in `src/Stroke/Application/Bindings/ViBindings.TextObjects.cs` — iw (FindBoundariesOfCurrentWord, Exclusive, noMoveHandler=true), aw (FindBoundariesOfCurrentWord with includeTrailingWhitespace=true, Exclusive, noMoveHandler=true), iW (WORD variant), aW (WORD variant), ap (paragraph + blank lines, Exclusive, noMoveHandler=true). Note: `ip` is NOT implemented per Python source
- [ ] T032 [US4] Implement `create_ci_ca_handles` factory method in `src/Stroke/Application/Bindings/ViBindings.TextObjects.cs` that generates 32 dynamic text object registrations for quote/bracket pairs: (1) for each of `"`, `'`, `` ` `` → register i,{char} (content between quotes) and a,{char} (content including quotes); (2) for each of `(`, `[`, `{`, `<` → register i,{char} (content between brackets via FindEnclosingBracketLeft/Right) and a,{char} (including brackets); (3) register aliases: i,b/a,b (same as i,(/a,()), i,B/a,B (same as i,{/a,{). All registered with noMoveHandler=true, noSelectionHandler=false

**Checkpoint**: All text objects work with operators and visual mode. Can select/operate on structured text regions.

---

## Phase 8: US5 — Find Characters with f/F/t/T/;/, (Priority: P2)

**Goal**: Character find (f, F, t, T) with repeat (;, ,) works as motions and with operators. Already implemented as text objects in Phase 4 (T017). This phase adds tests.

**Independent Test**: Use f/F/t/T to jump to characters, verify ; and , repeat correctly.

### Tests for US5

- [ ] T033 [US5] Add character find handler behavior tests to `tests/Stroke.Tests/Application/Bindings/ViBindings/LoadViBindingsTextObjectTests.cs`: verify `fo` moves cursor to 'o', verify `Fo` moves backward, verify `tw` moves to position before 'w', verify `;` repeats last find, verify `,` reverses last find, verify `dt)` deletes correctly, verify `f` at cursor position finds next occurrence (not current), verify `f` with no match leaves cursor unchanged, verify CharacterFind is stored in ViState.LastCharacterFind even on no-match. Character find is tested indirectly by mapped test `CursorMovements` in `ViModeTests.cs`; this provides focused US5 coverage

**Checkpoint**: Character find motions work correctly in all scenarios.

---

## Phase 9: US6 — Paste, Undo, Redo, and Register Operations (Priority: P2)

**Goal**: Paste (p/P) with linewise/characterwise modes, undo (u), redo (Ctrl-R), and named registers ("a-"z, "0-"9) all work. Depends on Phase 3 (operators populate registers) and Phase 6 (mode switching for register paste integration).

**Independent Test**: Yank text, paste from registers, undo/redo, verify state.

### Tests for US6

- [ ] T034 [P] [US6] Implement paste/undo/redo/register tests in `tests/Stroke.Tests/Application/Bindings/ViBindings/LoadViBindingsMiscTests.cs`: verify p pastes characterwise after cursor, verify p pastes linewise below current line with cursor at first char of pasted line, verify P pastes before, verify u undoes last edit, verify Ctrl-R redoes, verify `"ap` pastes from register a, verify register selection 3-key sequence works, verify empty register paste is no-op. Follow test-mapping.md test `CharacterPaste`

### Implementation for US6

- [ ] T035 [US6] Implement paste handlers in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — p (paste after cursor: characterwise → insert after cursor position; linewise → paste below current line with cursor at first char of first pasted line), P (paste before: characterwise → insert before cursor; linewise → paste above with cursor at first char). Both use `Buffer.Paste(data, PasteMode.ViAfter/ViBefore)` or manual insertion per Python logic. Gated on `vi_navigation_mode`
- [ ] T036 [US6] Implement undo/redo handlers in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — u (`Buffer.Undo()` with count), Ctrl-R (`Buffer.Redo()` with count). Gated on `vi_navigation_mode`
- [ ] T037 [US6] Implement register selection handler in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — " followed by `Keys.Any` (register name a-z, 0-9) sets `ViState.NamedRegister` for the next operator/paste command. This is a 2-key prefix that modifies the next command's register target

**Checkpoint**: Paste, undo, redo, and registers all work. Full editing workflow: yank → paste → undo → redo.

---

## Phase 10: US7 — Vi Search with / and ? (Priority: P3)

**Goal**: Search next (n), search previous (N), word search (#, *) work as navigation motions in ViBindings. Note: /, ?, Ctrl-S, Ctrl-R, Enter are already in SearchBindings.LoadViSearchBindings() (feature 038). Depends on Phase 4 (n/N already registered as text objects in T018).

**Independent Test**: Perform searches and verify cursor jumps to matches.

### Tests for US7

- [ ] T038 [US7] Add search-related tests to `tests/Stroke.Tests/Application/Bindings/ViBindings/LoadViBindingsNavigationTests.cs`: verify n and N bindings exist in LoadViBindings() (not SearchBindings), verify * and # bindings exist, verify n moves to next search match, verify N moves to previous, verify * initiates forward search for word under cursor, verify # initiates backward search. Supplements mapped test `CursorMovements` in `ViModeTests.cs`

### Implementation for US7

- [ ] T039 [US7] Implement * and # handlers in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — * (search word under cursor forward: get word at cursor via `Document.GetWordBeforeCursor`/`GetWordUnderCursor`, call `Buffer.StartSearch` or equivalent forward search), # (same but backward). Both gated on `vi_navigation_mode`

**Checkpoint**: Vi search navigation fully working. n/N repeat searches, */# search word under cursor.

---

## Phase 11: US8 — Macro Recording and Playback (Priority: P3)

**Goal**: Macro recording (q{register}), stop (q), playback (@{register}), repeat (@@) all work. Depends on Phase 2 (scaffolding).

**Independent Test**: Record a macro, play it back, verify same edits occur.

### Tests for US8

- [ ] T040 [P] [US8] Implement macro tests in `tests/Stroke.Tests/Application/Bindings/ViBindings/LoadViBindingsMiscTests.cs`: verify q,a starts recording to register a, verify q stops recording, verify @,a plays back, verify @,@ replays last, verify 3@a replays 3 times, verify macro with mode switches (qa → i → type → Esc → q) replays correctly. Follow test-mapping.md test `Macros`

### Implementation for US8

- [ ] T041 [US8] Implement macro recording/playback handlers in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — q,{reg} (start recording: `ViState.RecordingRegister = registerName`, gated on `vi_navigation_mode & ~vi_recording_macro`), q (stop recording: store `ViState.CurrentRecording` to named register, clear recording state, gated on `vi_navigation_mode & vi_recording_macro`), @,{reg} (play macro: feed stored keystrokes to `KeyProcessor`, gated on `vi_navigation_mode`), @,@ (replay last macro played)

**Checkpoint**: Macro system fully operational.

---

## Phase 12: US9 — Visual Mode Selection and Operations (Priority: P2)

**Goal**: Visual character/line/block modes work. Motions extend selection. Operators apply to selection. Toggle behavior: same key exits, different key switches type. Depends on Phase 6 (mode switching for v/V/Ctrl-V entry) and Phase 3 (operators).

**Independent Test**: Enter visual mode, extend selection with motions, apply operator, verify correct range affected.

### Tests for US9

- [ ] T042 [P] [US9] Implement visual mode binding tests in `tests/Stroke.Tests/Application/Bindings/ViBindings/LoadViBindingsVisualModeTests.cs`: verify j/k extend selection, verify x cuts selection, verify J joins selected lines, verify v toggles (exits if Characters, switches if Lines/Block), verify V toggles (exits if Lines, switches otherwise), verify Ctrl-V toggles, verify I/A in block selection enter InsertMultiple, verify Escape clears selection. Supplements mapped tests `VisualLineCopy`, `VisualEmptyLine`, `BlockEditing`, `BlockEditing_EmptyLines` in `ViModeTests.cs`

### Implementation for US9

- [ ] T043 [US9] Implement visual mode selection extension handlers in `src/Stroke/Application/Bindings/ViBindings.VisualMode.cs` — j (extend selection down), k (extend selection up). Both gated on `vi_selection_mode`. These are separate from navigation j/k text objects (different handlers for different modes)
- [ ] T044 [US9] Implement visual mode operation handlers in `src/Stroke/Application/Bindings/ViBindings.VisualMode.cs` — x (cut selection to clipboard), J (join selected lines with space via `Buffer.JoinSelectedLines(" ")`), g,J (join without space via `Buffer.JoinSelectedLines("")`). All gated on `vi_selection_mode`
- [ ] T045 [US9] Implement visual mode toggle handlers in `src/Stroke/Application/Bindings/ViBindings.VisualMode.cs` — v (if Characters → ExitSelection; else → switch to Characters), V (if Lines → exit; else → switch to Lines), Ctrl-V (if Block → exit; else → switch to Block). Gated on `vi_selection_mode`
- [ ] T046 [US9] Implement block selection insert/append and auto-word-extend handlers in `src/Stroke/Application/Bindings/ViBindings.VisualMode.cs` — I (enter InsertMultiple at block start, gated on `vi_selection_mode & in_block_selection`), A (enter InsertMultiple appending at block end, same gate), a,w/a,W (extend selection to word/WORD boundary using `FindBoundariesOfCurrentWord`, gated on `vi_selection_mode`). See contracts/vi-bindings.md Visual Mode section for filter details
- [ ] T047 [US9] Add the `RegisterVisualMode(KeyBindings kb)` partial method call point in `ViBindings.cs` `LoadViBindings()` and implement the orchestration method in `ViBindings.VisualMode.cs`

**Checkpoint**: Visual mode fully functional with selection, operations, and sub-mode toggling.

---

## Phase 13: US10 — Indentation, Case Transforms, and Miscellaneous Commands (Priority: P3)

**Goal**: All remaining commands: x, X, s, J, g,J, ~, >>, <<, guu, gUU, g~~, #, *, Ctrl-A, Ctrl-X, scroll z commands, digraph input, Ctrl-O, numeric arguments, unknown text object catch-all. Depends on Phases 3-12.

**Independent Test**: Execute each command on known text and verify transformation result.

### Tests for US10

- [ ] T048 [P] [US10] Implement misc command tests in `tests/Stroke.Tests/Application/Bindings/ViBindings/LoadViBindingsMiscTests.cs`: verify x deletes char after cursor, verify X deletes before, verify s substitutes, verify >> indents, verify << unindents, verify ~ swaps case at cursor (when TildeOperator=false), verify guu lowercases line, verify gUU uppercases, verify g~~ toggles, verify numeric args (1-9 accumulate, 0 appends when has_arg), verify Ctrl-A increments number, verify Ctrl-X decrements. Supplements mapped tests `CharacterDeleteAfterCursor`, `CharacterDeleteBeforeCursor` in `ViModeTests.cs`

### Implementation for US10

- [ ] T049 [US10] Implement single-character operations in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — x (delete char after cursor with count, store in clipboard, gated on `vi_navigation_mode`), X (delete char before cursor, gated on `vi_navigation_mode`), s (delete char + enter insert, gated on `vi_navigation_mode & ~is_read_only`)
- [ ] T050 [US10] Implement navigation-mode join handlers in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — J (join next line with space via `Buffer.JoinNextLine(" ")`, gated on `vi_navigation_mode`), g,J (join without space via `Buffer.JoinNextLine("")`, gated on `vi_navigation_mode`). Note: distinct from visual-mode J/g,J in VisualMode.cs
- [ ] T051 [US10] Implement standalone tilde and doubled-key case/indent transforms in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — ~ standalone (swap case at cursor and move right, gated on `vi_navigation_mode & ~tilde_operator`), guu (lowercase entire line), gUU (uppercase entire line), g~~ (swap case entire line), >> (indent line via `BufferOperations.Indent`), << (unindent line via `BufferOperations.Unindent`). All gated on `vi_navigation_mode`
- [ ] T052 [US10] Implement increment/decrement handlers in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — Ctrl-A (find number at/after cursor, increment by count, gated on `vi_navigation_mode`), Ctrl-X (decrement by count, gated on `vi_navigation_mode`). No-op if no number found
- [ ] T053 [US10] Implement scroll z-command handlers in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — z,z (scroll cursor to center), z,t / z,+ / z,Enter (scroll cursor to top), z,b / z,- (scroll cursor to bottom). All gated on `vi_navigation_mode | vi_selection_mode`. Note: Ctrl-F/B/D/U/E/Y/PageDown/PageUp are already in PageNavigationBindings — do NOT duplicate
- [ ] T054 [US10] Implement numeric argument handlers in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — digits 1-9 (accumulate count, gated on `vi_navigation_mode | vi_selection_mode | vi_waiting_for_text_object_mode`; 9 bindings from loop), 0 (append to count when `has_arg`, same gate). Follow Python's `for n in "123456789"` pattern
- [ ] T055 [US10] Implement G-with-arg (history navigation), Ctrl-O (quick normal mode), and unknown text object catch-all in `src/Stroke/Application/Bindings/ViBindings.Misc.cs` — G with `has_arg` filter (go to nth history line via `Buffer.GoToHistory(arg - 1)`), Ctrl-O (set `ViState.TemporaryNavigationMode = true`, gated on `vi_insert_mode | vi_replace_mode`), Keys.Any catch-all (sound bell, do NOT cancel operator state, gated on `vi_waiting_for_text_object_mode`)
- [ ] T056 [US10] Add the `RegisterMisc(KeyBindings kb)` partial method call point in `ViBindings.cs` `LoadViBindings()` and implement the orchestration method in `ViBindings.Misc.cs`

**Checkpoint**: All miscellaneous commands operational. Vi editing mode feature-complete.

---

## Phase 14: Insert Mode Bindings

**Purpose**: Implement all Vi-specific insert mode bindings, replace mode handlers, insert-multiple mode handlers, and digraph input. Depends on Phase 6 (mode switching).

### Tests

- [ ] T057 [P] Implement insert mode binding tests in `tests/Stroke.Tests/Application/Bindings/ViBindings/LoadViBindingsInsertModeTests.cs`: verify Ctrl-V delegates to quoted-insert named command, verify Ctrl-N/Ctrl-P trigger completion, verify Ctrl-T/Ctrl-D indent/unindent, verify Enter delegates to accept-line, verify Ctrl-K enters digraph mode, verify replace mode Any handler overwrites, verify replace-single Any handler replaces + returns to navigation, verify insert-multiple handlers work. Supplements mapped test `Digraphs` in `ViModeTests.cs`

### Implementation

- [ ] T058 Implement insert mode completion and editing bindings in `src/Stroke/Application/Bindings/ViBindings.InsertMode.cs` — Ctrl-V (quoted-insert via NamedCommands), Ctrl-N (complete next), Ctrl-P (complete previous), Ctrl-G / Ctrl-Y (accept completion), Ctrl-E (cancel completion), Enter (accept-line when `is_returnable & ~is_multiline`), Ctrl-T (indent via `BufferOperations.Indent`), Ctrl-D (unindent via `BufferOperations.Unindent`), Ctrl-X Ctrl-L (line completion stub), Ctrl-X Ctrl-F (filename completion stub). All gated on `vi_insert_mode`
- [ ] T059 Implement replace mode, replace-single, and insert-multiple mode handlers in `src/Stroke/Application/Bindings/ViBindings.InsertMode.cs` — Any in replace mode (`InsertText(overwrite: true)`), Any in replace-single mode (insert with overwrite, cursor back by 1, `InputMode = Navigation`), Any/Backspace/Delete/Left/Right/Up/Down in insert-multiple mode. Gated on respective mode filters
- [ ] T060 Implement digraph mode handlers in `src/Stroke/Application/Bindings/ViBindings.InsertMode.cs` — Ctrl-K (set `ViState.WaitingForDigraph = true`, gated on `vi_insert_mode | vi_replace_mode`), Any when `vi_digraph_mode & ~digraph_symbol_1_given` (store first symbol in `ViState.DigraphSymbol1`), Any when `vi_digraph_mode & digraph_symbol_1_given` (look up digraph pair from `Digraphs`, insert character, reset digraph state)
- [ ] T061 Add the `RegisterInsertMode(KeyBindings kb)` partial method call point in `ViBindings.cs` `LoadViBindings()` and implement the orchestration method in `ViBindings.InsertMode.cs`

**Checkpoint**: All insert, replace, and digraph mode bindings operational.

---

## Phase 15: Integration Tests

**Purpose**: End-to-end tests verifying multi-key sequences, operator+motion combos, and the 13 mapped test cases from test-mapping.md. Depends on all implementation phases.

- [ ] T062 [P] Implement Vi bindings integration tests in `tests/Stroke.Tests/Application/Bindings/ViBindings/ViBindingsIntegrationTests.cs`: (1) verify `LoadViBindings()` total binding count matches expected, (2) test multi-key operator+motion sequences (dw, cw, yy, 2d3w count multiplication), (3) test mode transition sequences (i → type → Escape → verify navigation), (4) test visual mode → operator sequence (v → motion → d), (5) test register workflow ("ayy → "ap), (6) test Ctrl-O temporary navigation mode
- [ ] T063 [P] Implement the 13 mapped Vi-specific test cases from test-mapping.md in `tests/Stroke.Tests/Application/ViModeTests.cs` (per Constitution IX, test-mapping.md): `CursorMovements`, `Operators`, `TextObjects`, `Digraphs`, `BlockEditing`, `BlockEditing_EmptyLines`, `VisualLineCopy`, `VisualEmptyLine`, `CharacterDeleteAfterCursor`, `CharacterDeleteBeforeCursor`, `CharacterPaste`, `TempNavigationMode`, `Macros` — 13 tests total. Each creates real Buffer/Application environment and verifies end-to-end behavior. This is the authoritative test file per test-mapping.md

**Checkpoint**: All 13 mapped tests pass. Integration verified.

---

## Phase 16: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across all Vi binding files.

- [ ] T064 Verify all source files under 1,000 LOC: `ViBindings.cs`, `ViBindings.Navigation.cs`, `ViBindings.Operators.cs`, `ViBindings.TextObjects.cs`, `ViBindings.ModeSwitch.cs`, `ViBindings.InsertMode.cs`, `ViBindings.VisualMode.cs`, `ViBindings.Misc.cs`, `TextObject.cs`, `TextObjectType.cs`. Split any file exceeding limit
- [ ] T065 Verify all public types have XML documentation comments: `TextObject`, `TextObjectType`, `ViBindings.LoadViBindings()`, `OperatorFuncDelegate`. Verify thread-safety documentation on `TextObject` (immutable) and `ViBindings` (stateless)
- [ ] T066 Run full test suite and verify no regressions. Verify test count increases by at least 13 (mapped tests) plus TextObject/TextObjectType unit tests. Target: 80% coverage for Vi binding source files
- [ ] T067 Verify `ConditionalKeyBindings` gate: `LoadViBindings()` result must be gated on `ViFilters.ViMode` so all Vi bindings are only active when application is in Vi editing mode. Verify existing `LoadViSearchBindings()` in `SearchBindings.cs` is NOT duplicated. Audit all text-modifying bindings (mode switches to insert/replace, operators d/c/x/s/p/P, indent/unindent, case transforms, join) for `~is_read_only` filter presence per FR-024

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1** (Foundation Types): No dependencies — can start immediately
- **Phase 2** (Scaffolding): Depends on Phase 1 — BLOCKS all binding implementations
- **Phase 3** (Registration Helpers & Operators): Depends on Phase 2 — BLOCKS text objects and operator usage
- **Phase 4** (US1 Navigation): Depends on Phase 3 (RegisterTextObject helper)
- **Phase 5** (US2 Operators+Motions): Depends on Phase 3 (operators) + Phase 4 (motions/text objects)
- **Phase 6** (US3 Mode Switching): Depends on Phase 2 (scaffolding only)
- **Phase 7** (US4 Text Objects): Depends on Phase 3 (RegisterTextObject)
- **Phase 8** (US5 Character Find): Tests only — implementation in Phase 4 (T017)
- **Phase 9** (US6 Paste/Undo/Registers): Depends on Phase 3 + Phase 6
- **Phase 10** (US7 Search): Tests mostly — n/N in Phase 4 (T018), */# in this phase
- **Phase 11** (US8 Macros): Depends on Phase 2
- **Phase 12** (US9 Visual Mode): Depends on Phase 3 + Phase 6
- **Phase 13** (US10 Misc): Depends on Phases 3-6
- **Phase 14** (Insert Mode): Depends on Phase 6
- **Phase 15** (Integration Tests): Depends on all implementation phases
- **Phase 16** (Polish): Depends on all phases

### User Story Dependencies

- **US1** (Navigation, P1): Needs Phase 1 → 2 → 3 → 4
- **US2** (Operators, P1): Needs Phase 1 → 2 → 3 → 4 → 5
- **US3** (Mode Switch, P1): Needs Phase 1 → 2 → 6 (parallel with Phases 3-4)
- **US4** (Text Objects, P2): Needs Phase 1 → 2 → 3 → 7
- **US5** (Char Find, P2): Tests only — impl in Phase 4
- **US6** (Paste/Undo, P2): Needs Phase 1 → 2 → 3 → 6 → 9
- **US7** (Search, P3): n/N in Phase 4, */# in Phase 10
- **US8** (Macros, P3): Needs Phase 1 → 2 → 11
- **US9** (Visual Mode, P2): Needs Phase 1 → 2 → 3 → 6 → 12
- **US10** (Misc, P3): Needs Phase 1 → 2 → 3 → 6 → 13

### Parallel Opportunities

After Phase 2 completes:
- **Phase 3** and **Phase 6** can run in parallel (different files: Operators.cs vs ModeSwitch.cs)
- **Phase 11** (Macros in Misc.cs) can start after Phase 2 (if Misc.cs handlers are isolated)

After Phase 3 completes:
- **Phase 4** (TextObjects.cs, Navigation.cs) and **Phase 7** (more TextObjects.cs) can be combined
- **Phase 12** (VisualMode.cs) and **Phase 14** (InsertMode.cs) can run in parallel (different files)

Within each phase:
- All tasks marked [P] can run in parallel
- Test tasks [P] can run in parallel with each other

---

## Implementation Strategy

### MVP First (P1 User Stories)

1. Complete Phase 1: Foundation Types (TextObject, TextObjectType)
2. Complete Phase 2: ViBindings Scaffolding
3. Complete Phase 3: Registration Helpers & Operators
4. Complete Phase 4: US1 Navigation (all motions)
5. Complete Phase 5: US2 Operators+Motions (dd, yy, cc, C, D)
6. Complete Phase 6: US3 Mode Switching (i, a, v, Escape, etc.)
7. **STOP and VALIDATE**: Test navigation, operators, and mode switching independently
8. Deploy/verify MVP: cursor movement, delete/change/yank, mode transitions

### Incremental Delivery

1. Phases 1-6 → MVP with navigation + operators + mode switching
2. Add Phase 7 (US4 Text Objects) → iw, aw, ci", da( etc.
3. Add Phase 9 (US6 Paste/Undo) → p, P, u, Ctrl-R, registers
4. Add Phase 12 (US9 Visual Mode) → v, V, Ctrl-V + operations
5. Add Phase 14 (Insert Mode) → Ctrl-V, Ctrl-N, digraphs
6. Add Phases 10, 11, 13 (P3 stories) → search, macros, misc
7. Phase 15 (Integration Tests) → full verification
8. Phase 16 (Polish) → LOC check, docs, coverage

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Total files: 2 new types + 8 partial class files + 1 delegate update = 11 source files
- Total test files: 2 unit test + 7 supplementary binding test + 1 integration + 1 mapped (ViModeTests.cs) = 11 test files
- Python reference: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/vi.py` (2,233 lines)
