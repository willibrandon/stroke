# Tasks: Base Widgets

**Input**: Design documents from `/specs/045-base-widgets/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Tests ARE included — spec.md requires ≥80% code coverage (SC-001) and test-mapping.md maps 2 Button tests as minimum.

**Organization**: Tasks grouped by user story (7 stories from spec.md). Stories map to the quickstart.md implementation phases.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create directory structure and verify Window supports dynamic dimensions needed by Label and ProgressBar.

- [X] T001 Create widget source directories: `src/Stroke/Widgets/Base/`, `src/Stroke/Widgets/Lists/`, `src/Stroke/Widgets/Dialogs/`
- [X] T002 Create widget test directories: `tests/Stroke.Tests/Widgets/Base/`, `tests/Stroke.Tests/Widgets/Lists/`, `tests/Stroke.Tests/Widgets/Dialogs/`
- [X] T003 Add Window constructor overloads accepting `Func<Dimension?>` for width/height and `Func<string?>` for char and `Func<string>` for style in `src/Stroke/Layout/Containers/Window.cs`. The internal fields `_widthGetter`, `_heightGetter`, `_styleGetter`, `_charGetter` already exist as `Func<>` types. Preferred approach: add optional `Func<>` parameters (e.g., `widthGetter: Func<Dimension?>? = null`) that take precedence over the static parameters when provided, avoiding constructor overload ambiguity. This is required by Label (dynamic width) and ProgressBar (dynamic weights). See research.md RT-3.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement Border constants and line widgets — these have zero inter-widget dependencies and are required by Frame, VerticalLine, HorizontalLine, and ProgressBar.

**⚠️ CRITICAL**: Frame (US3) and line widgets (US6) depend on Border constants.

- [X] T004 [P] Implement `Border` static class with 6 Unicode box-drawing constants in `src/Stroke/Widgets/Base/Border.cs`. Constants: Horizontal (─ U+2500), Vertical (│ U+2502), TopLeft (┌ U+250C), TopRight (┐ U+2510), BottomLeft (└ U+2514), BottomRight (┘ U+2518). See contract `contracts/border.md`.
- [X] T005 [US6] Implement `VerticalLine` widget (Window with width=1, char=Border.Vertical, style="class:line,vertical-line") and `HorizontalLine` widget (Window with height=1, char=Border.Horizontal, style="class:line,horizontal-line") in `src/Stroke/Widgets/Base/VerticalLine.cs` and `src/Stroke/Widgets/Base/HorizontalLine.cs`. Both implement `IMagicContainer`. Depends on T004 (Border constants). See contract `contracts/line-widgets.md`.
- [X] T006 [P] Write tests for Border constants and line widgets in `tests/Stroke.Tests/Widgets/Base/BorderTests.cs` and `tests/Stroke.Tests/Widgets/Base/LineWidgetTests.cs`. Verify: (1) all 6 Border constants match expected Unicode code points, (2) VerticalLine.PtContainer() returns Window with correct width/char/style, (3) HorizontalLine.PtContainer() returns Window with correct height/char/style.

**Checkpoint**: Foundation ready — user story implementation can now begin.

---

## Phase 3: User Story 6 — Label, Box, and Line Widgets (Priority: P3 but FOUNDATIONAL) 🎯 MVP

> **Note**: Despite P3 priority in the spec, Label and Box are foundational dependencies required by US1 (TextArea uses Label indirectly via ProgressBar), US2 (Button is peer), US3 (Frame needs Label for title, Box is prerequisite for Dialog), and US5 (Dialog needs Box). They must be implemented first.

**Goal**: Implement Label, Box, and ProgressBar — the static/utility widgets that other stories depend on.

**Independent Test**: Create each widget, verify container output, dimensions, and style strings.

### Implementation for User Story 6

- [X] T007 [P] [US6] Implement `Label` widget in `src/Stroke/Widgets/Base/Label.cs`. Constructor params: text (AnyFormattedText), style (string, default ""), width (Dimension?, default null), dontExtendHeight (FilterOrBool, default true), dontExtendWidth (FilterOrBool, default false), align (WindowAlign, default Left), wrapLines (FilterOrBool, default true). Writable property: Text. Auto-calculate preferred width using `Func<Dimension?>` getter with `UnicodeWidth.GetWidth()` for longest line when no explicit width. Style: `"class:label " + style`. Implements `IMagicContainer`. See contract `contracts/label.md`.
- [X] T008 [P] [US6] Implement `Box` widget in `src/Stroke/Widgets/Base/Box.cs`. Constructor params: body, padding, paddingLeft/Right/Top/Bottom, width, height, style, @char, modal, keyBindings. Writable properties: Body, Padding, PaddingLeft/Right/Top/Bottom. Padding resolution: each side = `PaddingLeft ?? Padding` etc. Layout: HSplit[Window(top), VSplit[Window(left), DynamicContainer(body), Window(right)], Window(bottom)]. Body wrapped in DynamicContainer for runtime changes. Uses `Func<Dimension?>` getters for padding dimensions. Implements `IMagicContainer`. See contract `contracts/box.md`.
- [X] T009 [P] [US6] Implement `ProgressBar` widget in `src/Stroke/Widgets/Base/ProgressBar.cs`. No-arg constructor, default percentage=60. Lock-protected `_percentage` field. Setter MUST atomically update both `_percentage` and `Label.Text` within same lock scope. Layout: FloatContainer with content Window(height=1), two Floats — one for Label("60%") centered, one for VSplit with weighted used/unused bar windows. Uses `Func<Dimension?>` getters for dynamic weights. Styles: `class:progress-bar.used` / `class:progress-bar`. Not clamped. Implements `IMagicContainer`. See contract `contracts/progress-bar.md`.
- [X] T010 [US6] Write tests for Label in `tests/Stroke.Tests/Widgets/Base/LabelTests.cs`. Verify: (1) default construction with text, (2) PtContainer() returns Window, (3) style is "class:label " + custom, (4) auto-width calculation for single-line text, (5) auto-width for multi-line text uses longest line, (6) empty text gives preferred width 0, (7) Text property is get/set, (8) explicit width overrides auto-calculation.
- [X] T011 [US6] Write tests for Box in `tests/Stroke.Tests/Widgets/Base/BoxTests.cs`. Verify: (1) PtContainer() returns HSplit, (2) padding fallback logic (per-side overrides overall), (3) all-null padding produces no padding windows, (4) Body is get/set and DynamicContainer reflects changes, (5) Box with uniform padding creates symmetric windows.
- [X] T012 [US6] Write tests for ProgressBar in `tests/Stroke.Tests/Widgets/Base/ProgressBarTests.cs`. Verify: (1) default percentage is 60, (2) Label.Text shows "60%", (3) setting Percentage updates both field and label atomically, (4) PtContainer() returns FloatContainer, (5) percentage 0 gives weight=0 for used bar, (6) percentage 100 gives weight=0 for unused bar, (7) negative percentage is not clamped (label shows "-5%"), (8) percentage >100 is not clamped.

**Checkpoint**: Label, Box, ProgressBar, VerticalLine, HorizontalLine all functional.

---

## Phase 4: User Story 1 — Text Input with TextArea (Priority: P1)

**Goal**: Implement the high-level text input widget wrapping Buffer + BufferControl + Window with 22 configurable parameters.

**Independent Test**: Create TextArea with various configurations, verify Buffer receives text, Document updates correctly, Window renders with expected margins and styles.

### Implementation for User Story 1

- [X] T013 [US1] Implement `TextArea` widget in `src/Stroke/Widgets/Base/TextArea.cs`. 22 constructor parameters (see contract `contracts/text-area.md`). Store FilterOrBool fields for runtime mutation, bridge to `Func<bool>` via lambdas for Buffer/BufferControl: `readOnly: () => FilterUtils.IsTrue(this.ReadOnly)`. Compose: Buffer (with name, document, completer, history, acceptHandler, readOnly, multiline), BufferControl (with buffer, lexer, inputProcessors, searchBufferControl, previewSearch, focusOnClick), Window (with bufferControl, margins, style, width, height, wrapLines, dontExtendWidth, dontExtendHeight). Margins: optional NumberedMargin (when lineNumbers=true), optional ScrollbarMargin (when scrollbar=true). InputProcessors: optional PasswordProcessor (when password=true), optional AppendAutoSuggestion, optional ConditionalProcessor, plus user-provided processors. Writable properties: Completer, CompleteWhileTyping, Lexer, AutoSuggest, ReadOnly, WrapLines, Validator, Text, Document, AcceptHandler. Text setter: null → empty Document. Document setter: bypass readOnly via `Buffer.SetDocument(value, bypassReadonly: true)`. Implements `IMagicContainer` returning Window. See data-model.md TextArea thread safety (CLR atomic writes, no Lock needed).
- [X] T014 [US1] Write tests for TextArea in `tests/Stroke.Tests/Widgets/Base/TextAreaTests.cs`. Verify: (1) default TextArea creates with empty text, (2) Text get/set round-trips, (3) Document get/set with bypass readOnly, (4) Text null setter produces empty document, (5) multiline=false sets height to D.Exact(1), (6) password=true adds PasswordProcessor, (7) readOnly=true prevents Buffer modification, (8) Text setter works when readOnly=true (via Document bypass), (9) lineNumbers=true adds NumberedMargin, (10) scrollbar=true adds ScrollbarMargin, (11) FilterOrBool properties are runtime-mutable (set ReadOnly to true then false, verify Buffer reflects), (12) PtContainer() returns Window, (13) completer wiring via DynamicCompleter, (14) name parameter maps to Buffer.Name.

**Checkpoint**: TextArea fully functional and tested.

---

## Phase 5: User Story 2 — Button Interaction (Priority: P1)

**Goal**: Implement clickable button widget with Enter/Space/mouse handlers.

**Independent Test**: Create Button, simulate key presses and mouse clicks, verify handler invocation.

### Implementation for User Story 2

- [X] T015 [US2] Implement `Button` widget in `src/Stroke/Widgets/Base/Button.cs`. Constructor params: text (string), handler (Action?, default null), width (int, default 12), leftSymbol (string, default "<"), rightSymbol (string, default ">"). Read-only: LeftSymbol, RightSymbol, Width. Writable: Text, Handler. Text fragment generation: [(style+arrow, leftSymbol, mouseHandler), ([SetCursorPosition], ""), (style+text, centeredText, mouseHandler), (style+arrow, rightSymbol, mouseHandler)]. Centering algorithm: availableWidth = Width - UnicodeWidth.GetWidth(LeftSymbol) - UnicodeWidth.GetWidth(RightSymbol) + (Text.Length - UnicodeWidth.GetWidth(Text)); padLeft to (availableWidth + Text.Length)/2; padRight to fill. Key bindings: Space and Enter invoke Handler (with null-check). Mouse: MOUSE_UP invokes Handler. Style: `class:button.focused` when focused, `class:button` when not. Sub-styles: `class:button.arrow`, `class:button.text`. Window with `dontExtendWidth: true`, `dontExtendHeight: true`, `width: width`. Implements `IMagicContainer`. See contract `contracts/button.md`, data-model.md Button thread safety.
- [X] T016 [US2] Write tests for Button in `tests/Stroke.Tests/Widgets/Base/ButtonTests.cs`. Verify: (1) default button with text "OK" and width 12 (mapped test: DefaultButton), (2) custom symbols "[" and "]" (mapped test: CustomButton), (3) Enter key invokes handler, (4) Space key invokes handler, (5) MOUSE_UP invokes handler, (6) null handler does not throw on Enter/Space/click, (7) Text is get/set, (8) Handler is get/set, (9) PtContainer() returns Window, (10) button width < symbols still renders without error, (11) handler invocation count matches expected (3 triggers = 3 calls).

**Checkpoint**: Button fully functional and tested.

---

## Phase 6: User Story 3 — Frame and Shadow Decoration (Priority: P2)

**Goal**: Implement Frame (border with optional title) and Shadow (visual depth) decorators.

**Independent Test**: Create Frame around a Label, verify border characters, wrap in Shadow and verify Float windows.

### Implementation for User Story 3

- [X] T017 [P] [US3] Implement `Frame` widget in `src/Stroke/Widgets/Base/Frame.cs`. Constructor params: body (AnyContainer), title (AnyFormattedText, default ""), style (string, default ""), width (Dimension?, null), height (Dimension?, null), keyBindings (IKeyBindingsBase?, null), modal (bool, false). Writable: Title, Body. Layout: HSplit with style `"class:frame " + style`, passing width/height/keyBindings/modal. Top row: ConditionalContainer — with title: VSplit[Window(Border.TopLeft), Window(Border.Horizontal), Window("|"), Label(Template(" {} ").Format(title), style="class:frame.label", dontExtendWidth=true), Window("|"), Window(Border.Horizontal), Window(Border.TopRight)]; without title: VSplit[Window(Border.TopLeft), Window(Border.Horizontal), Window(Border.TopRight)]. Filter: `Condition(() => ...)` checking if title is non-empty. Middle row: VSplit[Window(Border.Vertical), DynamicContainer(() => this.Body), Window(Border.Vertical)]. Bottom row: VSplit[Window(Border.BottomLeft), Window(Border.Horizontal), Window(Border.BottomRight)]. All border windows use style `"class:frame.border"`. Body wrapped in DynamicContainer for runtime changes. Implements `IMagicContainer` returning HSplit. See contract `contracts/frame.md`, data-model.md Frame.
- [X] T018 [P] [US3] Implement `Shadow` widget in `src/Stroke/Widgets/Base/Shadow.cs`. Constructor param: body (AnyContainer). Layout: FloatContainer with body as content, two Floats: bottom shadow Float(bottom=-1, height=1, left=1, right=-1, transparent=true) with Window(style="class:shadow"), right shadow Float(bottom=-1, top=1, width=1, right=-1, transparent=true) with Window(style="class:shadow"). Stateless after construction. Implements `IMagicContainer` returning FloatContainer. See contract `contracts/shadow.md`.
- [X] T019 [US3] Write tests for Frame in `tests/Stroke.Tests/Widgets/Base/FrameTests.cs`. Verify: (1) PtContainer() returns HSplit, (2) frame with title shows Label in top border, (3) frame without title shows continuous horizontal border, (4) Border constants used in window char properties (TopLeft, TopRight, BottomLeft, BottomRight, Horizontal, Vertical), (5) Title is get/set — switching from non-empty to empty swaps ConditionalContainer content, (6) Body is get/set — DynamicContainer reflects runtime changes, (7) style cascades as "class:frame " + custom, (8) border windows use "class:frame.border" style, (9) title Label uses "class:frame.label" style.
- [X] T020 [US3] Write tests for Shadow in `tests/Stroke.Tests/Widgets/Base/ShadowTests.cs`. Verify: (1) PtContainer() returns FloatContainer, (2) FloatContainer has exactly 2 Float elements, (3) Float coordinates match spec (bottom=-1,height=1,left=1,right=-1 and bottom=-1,top=1,width=1,right=-1), (4) both Floats have transparent=true, (5) Float content windows use "class:shadow" style.

**Checkpoint**: Frame and Shadow fully functional, ready for Dialog composition.

---

## Phase 7: User Story 4 — RadioList and CheckboxList Selection (Priority: P2)

**Goal**: Implement DialogList\<T\> base with full keyboard/mouse handling, RadioList\<T\>, CheckboxList\<T\>.

**Independent Test**: Create lists with values, simulate navigation, verify selection state and formatted text output.

### Implementation for User Story 4

- [X] T021 [US4] Implement `DialogList<T>` base class in `src/Stroke/Widgets/Lists/DialogList.cs`. Constructor params: values (IReadOnlyList<(T, AnyFormattedText)>), defaultValues (IReadOnlyList<T>?, null), selectOnFocus (bool, false), openCharacter (""), selectCharacter ("*"), closeCharacter (""), containerStyle (""), defaultStyle (""), numberStyle (""), selectedStyle (""), checkedStyle (""), multipleSelection (false), showScrollbar (true), showCursor (true), showNumbers (false). Validation: empty values throws ArgumentException. State: Lock-protected _selectedIndex, CurrentValue, CurrentValues (List<T>). Default value resolution: first matching defaultValue in values, else first item. Key bindings: 8 groups per FR-015 — Up/k (clamp 0), Down/j (clamp last), PageUp/PageDown (by visible lines from Window.RenderInfo), Enter/Space (toggle selection), 1-9 (number jump when showNumbers), character jump (case-insensitive, cycles on repeat). Mouse: MOUSE_UP on row index. Text fragment generation per contract: [open][select/space][close] [number] [label] per item with mouse handler, SetCursorPosition on selected. _HandleEnter: compound operation under single lock — read _selectedIndex, lookup value, add/remove from CurrentValues (multi) or set CurrentValue (single). FormattedTextControl + Window with optional ScrollbarMargin. Implements `IMagicContainer`. See contract `contracts/dialog-list.md`, data-model.md thread safety.
- [X] T022 [P] [US4] Implement `RadioList<T>` in `src/Stroke/Widgets/Lists/RadioList.cs`. Extends DialogList\<T\>. Constructor params: values, default (T?, null → first item), showNumbers, selectOnFocus, plus style overrides. Always passes multipleSelection=false. Converts single default to defaultValues list. Default styles: openCharacter="(", selectCharacter="*", closeCharacter=")", containerStyle="class:radio-list", defaultStyle="class:radio", selectedStyle="class:radio-selected", checkedStyle="class:radio-checked", numberStyle="class:radio-number". See contract `contracts/radio-list.md`.
- [X] T023 [P] [US4] Implement `CheckboxList<T>` in `src/Stroke/Widgets/Lists/CheckboxList.cs`. Extends DialogList\<T\>. Constructor params: values, defaultValues, plus style overrides only (no showNumbers, selectOnFocus, showCursor, showScrollbar — use base defaults). Always passes multipleSelection=true. Default styles: openCharacter="[", selectCharacter="*", closeCharacter="]", containerStyle="class:checkbox-list", defaultStyle="class:checkbox", selectedStyle="class:checkbox-selected", checkedStyle="class:checkbox-checked". See contract `contracts/checkbox-list.md`.
- [X] T024 [US4] Write tests for DialogList in `tests/Stroke.Tests/Widgets/Lists/DialogListTests.cs`. Verify: (1) empty values throws ArgumentException, (2) default value not in list falls back to first, (3) Up/k navigation with clamping at 0, (4) Down/j navigation with clamping at last, (5) Enter toggles single-select (sets CurrentValue), (6) Space toggles multi-select (adds/removes from CurrentValues), (7) number shortcut 1-9 jumps (when showNumbers=true), (8) number keys ignored when showNumbers=false, (9) character jump to first matching item, (10) character jump cycles on repeated presses, (11) MOUSE_UP selects row, (12) PtContainer() returns Window, (13) thread safety — concurrent access does not corrupt state, (14) _HandleEnter compound operation is atomic (read+lookup+modify under single lock), (15) duplicate values selected by index not value equality, (16) selectOnFocus=true auto-selects focused item when focus enters the list, (17) showCursor=false hides the SetCursorPosition marker in formatted text output.
- [X] T025 [US4] Write tests for RadioList and CheckboxList in `tests/Stroke.Tests/Widgets/Lists/RadioListTests.cs` and `tests/Stroke.Tests/Widgets/Lists/CheckboxListTests.cs`. RadioList: (1) single-select invariant — selecting B deselects A, (2) default value is selected, (3) null default selects first item, (4) radio-button styles applied, (5) multipleSelection always false. CheckboxList: (1) multi-select — toggle A and C, both in CurrentValues, (2) toggle A again removes it, (3) checkbox styles applied, (4) multipleSelection always true, (5) defaultValues pre-selected.

**Checkpoint**: All selection widgets functional with full keyboard/mouse interaction.

---

## Phase 8: User Story 7 — Checkbox Convenience Wrapper (Priority: P3)

**Goal**: Implement single-checkbox toggle wrapping CheckboxList\<string\>.

**Independent Test**: Create Checkbox, toggle state, verify `Checked` property.

### Implementation for User Story 7

- [X] T026 [US7] Implement `Checkbox` in `src/Stroke/Widgets/Lists/Checkbox.cs`. Extends CheckboxList\<string\> with single item `("value", text)`. Constructor params: text (AnyFormattedText), checked (bool, default false). Checked property: get returns `CurrentValues.Contains("value")`; set adds/removes "value" from CurrentValues. Override `ShowScrollbar` to false at class level (property hiding with `new` keyword). See contract `contracts/checkbox.md`.
- [X] T027 [US7] Write tests for Checkbox in `tests/Stroke.Tests/Widgets/Lists/CheckboxTests.cs`. Verify: (1) initial checked=true → Checked returns true, (2) initial checked=false → Checked returns false, (3) Space toggles Checked to true, (4) programmatic Checked=false updates visual state, (5) ShowScrollbar is false (class-level override), (6) PtContainer() returns Window, (7) single item in values list.

**Checkpoint**: Checkbox convenience wrapper complete.

---

## Phase 9: User Story 5 — Dialog Composition (Priority: P2)

**Goal**: Implement the Dialog widget composing Frame + Shadow + Box + Buttons with focus cycling.

**Independent Test**: Create Dialog with body and buttons, verify container hierarchy and focus cycling key bindings.

### Implementation for User Story 5

- [X] T028 [US5] Implement `Dialog` widget in `src/Stroke/Widgets/Dialogs/Dialog.cs`. Constructor params: body (AnyContainer), title (AnyFormattedText, default ""), buttons (IReadOnlyList<Button>?, null), modal (bool, true), width (Dimension?, null), withBackground (bool, false). Writable: Body, Title. Layout with buttons: Shadow(Frame(title: () => this.Title, body: HSplit[Box(body: DynamicContainer(() => this.Body), padding=D(preferred=1,max=1), paddingBottom=0), Box(body: VSplit(buttons, padding=1, keyBindings=buttonsKb), height=D(min=1,max=3,preferred=3))], style="class:dialog.body", keyBindings=kb, modal=modal)). Without buttons (null or empty): Shadow(Frame(title, body: DynamicContainer(() => this.Body), style, keyBindings, modal)). Key bindings (kb): Tab → FocusFunctions.FocusNext (filtered by ~hasCompletions), Shift-Tab → FocusFunctions.FocusPrevious (filtered by ~hasCompletions). Button key bindings (buttonsKb, when >1 button): Left → FocusFunctions.FocusPrevious, Right → FocusFunctions.FocusNext. withBackground=true: wrap in Box(body=frame, style="class:dialog", width=width). Implements `IMagicContainer`. See contract `contracts/dialog.md`, data-model.md Dialog.
- [X] T029 [US5] Write tests for Dialog in `tests/Stroke.Tests/Widgets/Dialogs/DialogTests.cs`. Verify: (1) Dialog with buttons creates Shadow > Frame > HSplit hierarchy, (2) Dialog without buttons uses body directly in Frame, (3) null buttons equivalent to empty list, (4) Tab/Shift-Tab key bindings registered with ~hasCompletions filter, (5) Left/Right key bindings registered for buttons (when >1 button), (6) withBackground=true wraps in Box with "class:dialog" style, (7) Body is get/set — DynamicContainer reflects changes, (8) Title is get/set — Frame title updates, (9) PtContainer() returns correct container type, (10) Dialog with 3 buttons has focus cycling between them.

**Checkpoint**: Dialog composition complete.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, coverage check, and cross-cutting validation.

- [X] T030 Verify all 15 widget classes implement `IMagicContainer` — write a single integration test in `tests/Stroke.Tests/Widgets/Base/IMagicContainerIntegrationTests.cs` that constructs each widget and asserts `PtContainer()` returns non-null, and embeds each in HSplit and VSplit containers.
- [X] T031 Verify thread safety for DialogList and ProgressBar — add concurrent tests in `tests/Stroke.Tests/Widgets/Lists/DialogListTests.cs` and `tests/Stroke.Tests/Widgets/Base/ProgressBarTests.cs` that use multiple threads to mutate and read state simultaneously, verifying no exceptions or corrupted state.
- [X] T032 Verify no source file exceeds 1000 LOC — run line count on all new files in `src/Stroke/Widgets/Base/`, `src/Stroke/Widgets/Lists/`, `src/Stroke/Widgets/Dialogs/`.
- [X] T033 Run full test suite and verify ≥80% code coverage for all new widget files. Command: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --collect:"XPlat Code Coverage" --filter "FullyQualifiedName~Widgets"`. Add additional tests if coverage is below target.
- [X] T034 Run quickstart.md verification checklist — confirm all 17 items pass.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion (directory structure + Window overloads)
- **US6 (Phase 3)**: Depends on Phase 2 (Border constants for ProgressBar; Window overloads for Label/Box/ProgressBar)
- **US1 (Phase 4)**: Depends on Phase 2 (Window overloads). Can run in parallel with Phase 3.
- **US2 (Phase 5)**: Depends on Phase 2 only. Can run in parallel with Phases 3-4.
- **US3 (Phase 6)**: Depends on Phase 2 (Border) + Phase 3 (Label for title). BLOCKED until Label is done.
- **US4 (Phase 7)**: Depends on Phase 1 only. Can run in parallel with Phases 3-6.
- **US7 (Phase 8)**: Depends on Phase 7 (CheckboxList must be complete).
- **US5 (Phase 9)**: Depends on Phase 3 (Box), Phase 5 (Button), Phase 6 (Frame, Shadow). BLOCKED until those are done.
- **Polish (Phase 10)**: Depends on ALL user story phases being complete.

### User Story Dependencies

```
Phase 1 (Setup)
  └── Phase 2 (Foundation: Border + Lines + Window overloads)
        ├── Phase 3 (US6: Label, Box, ProgressBar) ←── can start after Phase 2
        │     └── Phase 6 (US3: Frame, Shadow) ←── needs Label from US6
        │           └── Phase 9 (US5: Dialog) ←── needs Frame, Shadow, Box, Button
        ├── Phase 4 (US1: TextArea) ←── can start after Phase 2
        ├── Phase 5 (US2: Button) ←── can start after Phase 2
        │     └── Phase 9 (US5: Dialog) ←── needs Button
        └── Phase 7 (US4: DialogList, RadioList, CheckboxList) ←── can start after Phase 1
              └── Phase 8 (US7: Checkbox) ←── needs CheckboxList
```

### Parallel Opportunities

**After Phase 2 completes**, the following can run in parallel:
- US1 (TextArea) — Phase 4
- US2 (Button) — Phase 5
- US4 (DialogList hierarchy) — Phase 7
- US6 (Label, Box, ProgressBar) — Phase 3

**Within Phases**, tasks marked [P] can run in parallel:
- Phase 2: T004, T005, T006 (Border, Lines, Tests) — all different files
- Phase 3: T007, T008, T009 (Label, Box, ProgressBar) — all different files
- Phase 6: T017, T018 (Frame, Shadow) — different files
- Phase 7: T022, T023 (RadioList, CheckboxList) — different files, but depend on T021 (DialogList base)

---

## Parallel Example: After Phase 2

```bash
# Launch 4 user stories in parallel (different directories, no dependencies):
Agent 1: "Implement Label in src/Stroke/Widgets/Base/Label.cs" (T007)
Agent 2: "Implement TextArea in src/Stroke/Widgets/Base/TextArea.cs" (T013)
Agent 3: "Implement Button in src/Stroke/Widgets/Base/Button.cs" (T015)
Agent 4: "Implement DialogList in src/Stroke/Widgets/Lists/DialogList.cs" (T021)
```

## Parallel Example: Phase 7 Selection Widgets

```bash
# After DialogList (T021) completes, launch RadioList and CheckboxList in parallel:
Agent 1: "Implement RadioList in src/Stroke/Widgets/Lists/RadioList.cs" (T022)
Agent 2: "Implement CheckboxList in src/Stroke/Widgets/Lists/CheckboxList.cs" (T023)
```

---

## Implementation Strategy

### MVP First (Label + Button + Frame)

1. Complete Phase 1: Setup (directories + Window overloads)
2. Complete Phase 2: Foundation (Border + Lines)
3. Complete Phase 3: US6 (Label, Box, ProgressBar)
4. Complete Phase 5: US2 (Button)
5. **STOP and VALIDATE**: Label, Box, Button, ProgressBar, Lines all work independently
6. Complete Phase 6: US3 (Frame + Shadow)
7. **VALIDATE**: Frame wraps content with borders correctly

### Full Incremental Delivery

1. Setup + Foundation → directories and constants ready
2. US6 (Label, Box, ProgressBar) → utility widgets ready
3. US1 (TextArea) + US2 (Button) → core interactive widgets ready (parallel)
4. US3 (Frame, Shadow) → decorators ready
5. US4 (RadioList, CheckboxList) + US7 (Checkbox) → selection widgets ready (parallel with 3-4)
6. US5 (Dialog) → composition widget ready (needs all above)
7. Polish → coverage, thread safety verification, LOC check

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- DynamicContainer accepts `Func<AnyContainer>?` (confirmed from source), NOT `Func<IContainer>` — spec runtime assumption corrected
- Window constructor needs `Func<Dimension?>` overloads (T003) — internal fields already `Func<>` types
- Total: 34 tasks across 10 phases
- Test-writing tasks: 14 (T006, T010, T011, T012, T014, T016, T019, T020, T024, T025, T027, T029, T030, T031)
- Implementation tasks: 17 (T001, T002, T003, T004, T005, T007, T008, T009, T013, T015, T017, T018, T021, T022, T023, T026, T028)
- Verification/validation tasks: 3 (T032, T033, T034)
