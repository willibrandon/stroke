# Tasks: Full-Screen Examples

**Input**: Design documents from `/specs/064-fullscreen-examples/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: No unit tests requested. Verification via TUI Driver MCP tools (manual end-to-end).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the project, solution entry, and routing infrastructure

- [ ] T001 Create project file `examples/Stroke.Examples.FullScreen/Stroke.Examples.FullScreen.csproj` with net10.0 target and Stroke project reference per contracts/example-interface.md
- [ ] T002 Create directory structure: `examples/Stroke.Examples.FullScreen/SimpleDemos/` and `examples/Stroke.Examples.FullScreen/ScrollablePanes/`
- [ ] T003 Add Stroke.Examples.FullScreen project to `examples/Stroke.Examples.sln` solution file
- [ ] T004 Create Program.cs router at `examples/Stroke.Examples.FullScreen/Program.cs` with case-insensitive dictionary routing for all 25 examples, --help flag, and categorized usage display per contracts/example-interface.md
- [ ] T005 Verify project builds with `dotnet build examples/Stroke.Examples.FullScreen` (Program.cs will reference example classes that don't exist yet — use stub routing or compile check after Phase 2)

**Checkpoint**: Project scaffolding complete. Build may fail until example classes are created (expected).

---

## Phase 2: User Story 1 — Run Basic Full-Screen Application (Priority: P1) 🎯 MVP

**Goal**: Implement the three simplest examples (HelloWorld, DummyApp, NoLayout) that demonstrate the most basic Application<T> usage patterns

**Independent Test**: Run `dotnet run -- HelloWorld` → framed text area shows "Hello world!"; Ctrl+C exits cleanly

### Implementation for User Story 1

- [ ] T006 [P] [US1] Implement HelloWorld example at `examples/Stroke.Examples.FullScreen/HelloWorld.cs` — port of `hello-world.py`: Box wrapping Frame wrapping TextArea ("Hello world!\nPress control-c to quit."), KeyBindings with Ctrl+C exit, Application fullScreen=true. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/hello-world.py`
- [ ] T007 [P] [US1] Implement DummyApp example at `examples/Stroke.Examples.FullScreen/DummyApp.cs` — port of `dummy-app.py`: minimal Application with fullScreen=false, no layout, immediate exit on any key. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/dummy-app.py`
- [ ] T008 [P] [US1] Implement NoLayout example at `examples/Stroke.Examples.FullScreen/NoLayout.cs` — port of `no-layout.py`: Application with fullScreen=true, no layout (null), exits on Ctrl+C. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/no-layout.py`
- [ ] T009 [US1] Build and verify all 3 US1 examples compile and run: `dotnet run --project examples/Stroke.Examples.FullScreen -- HelloWorld`, `DummyApp`, `NoLayout`

**Checkpoint**: Simplest full-screen examples working. Developers can see basic Application structure.

---

## Phase 3: User Story 2 — Interactive Widget Demonstration (Priority: P1)

**Goal**: Implement the Buttons example demonstrating widget interaction, focus navigation, and event handling

**Independent Test**: Run `dotnet run -- Buttons` → Tab navigates between buttons; Enter activates; text area updates; Exit button quits

### Implementation for User Story 2

- [ ] T010 [US2] Implement Buttons example at `examples/Stroke.Examples.FullScreen/Buttons.cs` — port of `buttons.py`: 4 Button widgets (Button 1-3 + Exit), Label instruction text, TextArea for output, HSplit+VSplit layout with Box padding, Style with class selectors (left-pane, right-pane, button, button-arrow, button focused, text-area focused), Tab/Shift+Tab focus navigation via FocusFunctions, click handlers updating TextArea.Text, Exit handler calling App.Exit(). Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/buttons.py`
- [ ] T011 [US2] Verify Buttons example: Tab cycles focus across all 4 buttons + text area, Enter activates each button, Exit button exits app

**Checkpoint**: Widget interaction pattern established. Focus navigation working.

---

## Phase 4: User Story 3 — REPL Calculator Pattern (Priority: P1)

**Goal**: Implement the Calculator example demonstrating the accept_handler REPL pattern

**Independent Test**: Run `dotnet run -- Calculator` → type "4 + 4" + Enter → shows "In: 4 + 4" / "Out: 8"

### Implementation for User Story 3

- [ ] T012 [US3] Implement Calculator example at `examples/Stroke.Examples.FullScreen/Calculator.cs` — port of `calculator.py`: HSplit layout (output TextArea + separator Window + input TextArea + SearchToolbar), input TextArea with height=1, prompt=">>> ", multiline=false, search_field=searchToolbar. AcceptHandler evaluates expressions using DataTable.Compute(), appends "In:" and "Out:" to output, handles errors gracefully. Style with output-field, input-field, line classes. KeyBindings: Ctrl+C and Ctrl+Q to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/calculator.py`
- [ ] T013 [US3] Verify Calculator: expression evaluation (4+4=8, 10*5=50), error handling (invalid expression shows error message not crash), Ctrl+C exits

**Checkpoint**: REPL pattern working. Accept handler demonstrated.

---

## Phase 5: User Story 4 — Split Screen with Reactive Updates (Priority: P2)

**Goal**: Implement the SplitScreen example demonstrating Buffer.OnTextChanged reactive pattern

**Independent Test**: Run `dotnet run -- SplitScreen` → type "hello" in left pane → "olleh" appears in right pane

### Implementation for User Story 4

- [ ] T014 [US4] Implement SplitScreen example at `examples/Stroke.Examples.FullScreen/SplitScreen.cs` — port of `split-screen.py`: two Buffers (left, right), BufferControl for each, VSplit with vertical separator Window(width=1, char='|'), HSplit with titlebar (FormattedTextControl returning styled tuples "Hello world" + "(Press [Ctrl-Q] to quit.)"), horizontal separator, body. Buffer.OnTextChanged handler reverses left text into right buffer. KeyBindings: Ctrl+C/Ctrl+Q eager exit. mouse_support=true. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/split-screen.py`
- [ ] T015 [US4] Verify SplitScreen: typing in left pane produces reversed text in right pane, title bar renders, Ctrl+Q exits

**Checkpoint**: Reactive buffer pattern working.

---

## Phase 6: User Story 5 — File Viewer with Syntax Highlighting (Priority: P2)

**Goal**: Implement the Pager example with read-only viewing, line numbers, scrollbar, search, and syntax highlighting

**Independent Test**: Run `dotnet run -- Pager` → shows source code with line numbers; `/` triggers search; `q` exits

### Implementation for User Story 5

- [ ] T016 [US5] Implement Pager example at `examples/Stroke.Examples.FullScreen/Pager.cs` — port of `pager.py`: reads its own source file (Pager.cs), TextArea (read_only=true, scrollbar=true, line_numbers=true, search_field=searchToolbar, lexer=PygmentsLexer for C#), status bar Window with FormattedTextControl showing filename + cursor position + help text, HSplit (status bar + text area + search toolbar). KeyBindings: Ctrl+C and 'q' to exit. Style with status, status.position, status.key, not-searching classes. enable_page_navigation_bindings=true, mouse_support=true. SearchToolbar with text_if_not_searching. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/pager.py`
- [ ] T017 [US5] Verify Pager: file content displays with line numbers and scrollbar, '/' opens search, 'q' exits

**Checkpoint**: Read-only viewer with syntax highlighting and search working.

---

## Phase 7: User Story 6 — Full Widget Showcase (Priority: P2)

**Goal**: Implement the FullScreenDemo showing all widget types with menus

**Independent Test**: Run `dotnet run -- FullScreenDemo` → menus appear; radio list, checkboxes, progress bar, dialog all functional

### Implementation for User Story 6

- [ ] T018 [US6] Implement FullScreenDemo example at `examples/Stroke.Examples.FullScreen/FullScreenDemo.cs` — port of `full-screen-demo.py`: MenuContainer with File menu (New, Open submenu with nested items, Save, Save as, separator, Exit), Edit menu (Undo, Cut, Copy, Paste, Delete, separator, Find, Find next, Replace, Go To, Select All, Time/Date), View menu (Status Bar), Info menu (About). Body contains VSplit with Frame+Label, Dialog, TextArea with HtmlLexer; VSplit with Frame+ProgressBar, Frame+Checkboxes, Frame+RadioList; Box with Yes/No buttons. FloatContainer with CompletionsMenu float. WordCompleter for animal names. Style with window.border, shadow, menu-bar, menu, button-bar classes. Tab/Shift+Tab focus. mouse_support=true. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/full-screen-demo.py`
- [ ] T019 [US6] Verify FullScreenDemo: menus open/close, radio list selection works, checkboxes toggle, text area accepts input with completion, Yes/No buttons work

**Checkpoint**: Widget showcase working with menus and all widget types.

---

## Phase 8: User Story 7 — Text Editor with Menus (Priority: P2)

**Goal**: Implement the TextEditor example with file operations, menus, dialogs, and status bar

**Independent Test**: Run `dotnet run -- TextEditor` → File menu works; editing with search; status bar shows cursor position

### Implementation for User Story 7

- [ ] T020 [US7] Implement TextEditor example at `examples/Stroke.Examples.FullScreen/TextEditor.cs` — port of `text-editor.py`: ApplicationState class (show_status_bar, current_path), TextInputDialog class with Future pattern (TextArea+OK+Cancel buttons, PathCompleter for Open), MessageDialog class with Future pattern. TextArea with DynamicLexer (PygmentsLexer.FromFilename), scrollbar, line_numbers, SearchToolbar. MenuContainer with File (New/Open/Save/SaveAs/Exit), Edit (Undo/Cut/Copy/Paste/Delete/Find/FindNext/Replace/GoTo/SelectAll/TimeDate), View (Status Bar), Info (About). ConditionalContainer for status bar. show_dialog_as_float async pattern. Status bar with cursor position. Style with status, shadow. Ctrl+C focuses menu. enable_page_navigation_bindings=true, mouse_support=true. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/text-editor.py`
- [ ] T021 [US7] Verify TextEditor: File→New clears text, File→Open shows dialog with path completion, Edit→Find activates search, status bar shows position, File→Exit quits

**Checkpoint**: Full text editor working. Most complex example complete.

---

## Phase 9: AnsiArtAndTextArea (Priority: P2)

**Goal**: Implement the AnsiArtAndTextArea example with embedded ANSI art

**Independent Test**: Run `dotnet run -- AnsiArtAndTextArea` → ANSI art renders on left; text area on right is editable

### Implementation for AnsiArtAndTextArea

- [ ] T022 Implement AnsiArtAndTextArea example at `examples/Stroke.Examples.FullScreen/AnsiArtAndTextArea.cs` — port of `ansi-art-and-textarea.py`: large embedded ANSI art string constant, FormattedTextControl rendering ANSI art, TextArea on right side, VSplit layout, KeyBindings Ctrl+C to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/ansi-art-and-textarea.py`
- [ ] T023 Verify AnsiArtAndTextArea: ANSI art displays with colors, text area is editable, Ctrl+C exits

**Checkpoint**: All P2 main examples complete.

---

## Phase 10: User Story 8 — Layout Alignment Examples (Priority: P3)

**Goal**: Implement alignment demos (HorizontalSplit, VerticalSplit, Alignment, HorizontalAlign, VerticalAlign)

**Independent Test**: Run each alignment example → content positioned correctly; 'q' exits

### Implementation for User Story 8

- [ ] T024 [P] [US8] Implement HorizontalSplit at `examples/Stroke.Examples.FullScreen/SimpleDemos/HorizontalSplit.cs` — port of `horizontal-split.py`: HSplit with two FormattedTextControl windows separated by Window(height=1, char='-'), 'q' to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/simple-demos/horizontal-split.py`
- [ ] T025 [P] [US8] Implement VerticalSplit at `examples/Stroke.Examples.FullScreen/SimpleDemos/VerticalSplit.cs` — port of `vertical-split.py`: VSplit with two FormattedTextControl windows separated by Window(width=1, char='|'), 'q' to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/simple-demos/vertical-split.py`
- [ ] T026 [P] [US8] Implement Alignment at `examples/Stroke.Examples.FullScreen/SimpleDemos/Alignment.cs` — port of `alignment.py`: HSplit with 3 windows using WindowAlign.Left, Center, Right, LIPSUM text, separators between panes, 'q' to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/simple-demos/alignment.py`
- [ ] T027 [P] [US8] Implement HorizontalAlign at `examples/Stroke.Examples.FullScreen/SimpleDemos/HorizontalAlign.cs` — port of `horizontal-align.py`: demonstrates HorizontalAlign enum (Left, Center, Right, Justify) with labeled sections, 'q' to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/simple-demos/horizontal-align.py`
- [ ] T028 [P] [US8] Implement VerticalAlign at `examples/Stroke.Examples.FullScreen/SimpleDemos/VerticalAlign.cs` — port of `vertical-align.py`: demonstrates VerticalAlign enum (Top, Center, Bottom, Justify) with labeled sections, 'q' to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/simple-demos/vertical-align.py`
- [ ] T029 [US8] Verify all 5 alignment examples build and run with correct positioning

**Checkpoint**: Layout alignment concepts demonstrated.

---

## Phase 11: User Story 9 — Float Positioning Examples (Priority: P3)

**Goal**: Implement Floats and FloatTransparency examples

**Independent Test**: Run `dotnet run -- Floats` → 5 positioned floats visible; 'q' exits

### Implementation for User Story 9

- [ ] T030 [P] [US9] Implement Floats at `examples/Stroke.Examples.FullScreen/SimpleDemos/Floats.cs` — port of `floats.py`: FloatContainer with LIPSUM background, 5 Frame-wrapped floats (left=0, right=0, bottom=0, top=0, center), quit text float at top=6. 'q' to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/simple-demos/floats.py`
- [ ] T031 [P] [US9] Implement FloatTransparency at `examples/Stroke.Examples.FullScreen/SimpleDemos/FloatTransparency.cs` — port of `float-transparency.py`: FloatContainer with LIPSUM background, left float (transparent=false), right float (transparent=true), HTML formatted labels, quit text. 'q' to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/simple-demos/float-transparency.py`
- [ ] T032 [US9] Verify both float examples render correctly

**Checkpoint**: Float positioning and transparency demonstrated.

---

## Phase 12: User Story 10 — Focus Management Examples (Priority: P3)

**Goal**: Implement Focus example with hotkey-based focus switching

**Independent Test**: Run `dotnet run -- Focus` → press a/b/c/d to switch focus between 4 windows

### Implementation for User Story 10

- [ ] T033 [US10] Implement Focus at `examples/Stroke.Examples.FullScreen/SimpleDemos/Focus.cs` — port of `focus.py`: 4 BufferControl windows (left_top, right_top, left_bottom, right_bottom) with LIPSUM text, instruction bar at top, HSplit+VSplit grid with separators. KeyBindings: 'q' exit, 'a'/'b'/'c'/'d' focus specific windows via Layout.Focus(), Tab/Shift+Tab cycle. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/simple-demos/focus.py`
- [ ] T034 [US10] Verify Focus example: all 4 hotkeys switch focus correctly

**Checkpoint**: Programmatic focus management demonstrated.

---

## Phase 13: User Story 11 — Scrollable Pane Examples (Priority: P3)

**Goal**: Implement ScrollablePanes examples

**Independent Test**: Run `dotnet run -- SimpleExample` → scroll through 20 TextAreas with mouse/keyboard

### Implementation for User Story 11

- [ ] T035 [P] [US11] Implement SimpleExample at `examples/Stroke.Examples.FullScreen/ScrollablePanes/SimpleExample.cs` — port of `simple-example.py`: Frame wrapping ScrollablePane wrapping HSplit with 20 Frame+TextArea widgets, Tab/Shift+Tab focus, Ctrl+C exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/scrollable-panes/simple-example.py`
- [ ] T036 [P] [US11] Implement WithCompletionMenu at `examples/Stroke.Examples.FullScreen/ScrollablePanes/WithCompletionMenu.cs` — port of `with-completion-menu.py`: ScrollablePane with TextAreas having WordCompleter, FloatContainer with CompletionsMenu float, Tab/Shift+Tab focus, Ctrl+C exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/scrollable-panes/with-completion-menu.py`
- [ ] T037 [US11] Verify both scrollable pane examples scroll correctly

**Checkpoint**: Scrollable pane pattern demonstrated.

---

## Phase 14: User Story 12 — Margin and Line Prefix Examples (Priority: P3)

**Goal**: Implement Margins and LinePrefixes examples

**Independent Test**: Run `dotnet run -- Margins` → line numbers and scrollbar visible

### Implementation for User Story 12

- [ ] T038 [P] [US12] Implement Margins at `examples/Stroke.Examples.FullScreen/SimpleDemos/Margins.cs` — port of `margins.py`: HSplit with instruction bar + Window with BufferControl and LIPSUM text, left_margins=[NumberedMargin, ScrollbarMargin], right_margins=[ScrollbarMargin, ScrollbarMargin]. 'q'/Ctrl+C to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/simple-demos/margins.py`
- [ ] T039 [P] [US12] Implement LinePrefixes at `examples/Stroke.Examples.FullScreen/SimpleDemos/LinePrefixes.cs` — port of `line-prefixes.py`: Window with BufferControl and get_line_prefix callback for custom prefixes, 'q' to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/simple-demos/line-prefixes.py`
- [ ] T040 [US12] Verify Margins shows numbered lines and scrollbars; LinePrefixes shows custom prefixes

**Checkpoint**: Editor chrome features demonstrated.

---

## Phase 15: User Story 13 — Cursor Highlighting and AutoCompletion Examples (Priority: P3)

**Goal**: Implement ColorColumn, CursorHighlight, and AutoCompletion examples

**Independent Test**: Run each → cursor highlighting visible; completion menu works

### Implementation for User Story 13

- [ ] T041 [P] [US13] Implement ColorColumn at `examples/Stroke.Examples.FullScreen/SimpleDemos/ColorColumn.cs` — port of `colorcolumn.py`: TextArea with ColorColumn markers at specific positions, 'q'/Ctrl+C to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/simple-demos/colorcolumn.py`
- [ ] T042 [P] [US13] Implement CursorHighlight at `examples/Stroke.Examples.FullScreen/SimpleDemos/CursorHighlight.cs` — port of `cursorcolumn-cursorline.py`: TextArea with cursorLine=true, cursorColumn=true, 'q'/Ctrl+C to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/simple-demos/cursorcolumn-cursorline.py`
- [ ] T043 [P] [US13] Implement AutoCompletion at `examples/Stroke.Examples.FullScreen/SimpleDemos/AutoCompletion.cs` — port of `autocompletion.py`: TextArea with WordCompleter (animal names), FloatContainer with CompletionsMenu, Ctrl+C to exit. Reference: `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/simple-demos/autocompletion.py`
- [ ] T044 [US13] Verify all 3 examples: ColorColumn shows column markers, CursorHighlight highlights cursor position, AutoCompletion shows completion menu on typing

**Checkpoint**: All SimpleDemos complete. All 13 simple-demos implemented.

---

## Phase 16: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and integration

- [ ] T045 Update Program.cs router at `examples/Stroke.Examples.FullScreen/Program.cs` to ensure all 25 example entries are correctly wired (verify dictionary entries match class names and namespaces)
- [ ] T046 Build all examples with zero warnings: `dotnet build examples/Stroke.Examples.FullScreen`
- [ ] T047 Run each of the 25 examples to verify they launch, render, and exit without exceptions (manual TUI verification pass)
- [ ] T048 Verify --help output lists all 25 examples in categorized format
- [ ] T049 Verify unknown example name produces helpful error with exit code 1

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **US1 (Phase 2)**: Depends on Setup (Phase 1) — FIRST user story
- **US2 (Phase 3)**: Can start after US1 (uses same widget patterns)
- **US3 (Phase 4)**: Can start after US1 (independent pattern)
- **US4-US7 (Phases 5-9)**: Can start after US1 (P2 stories)
- **US8-US13 (Phases 10-15)**: Can start after US1 (P3 stories, all independent)
- **Polish (Phase 16)**: Depends on ALL user stories being complete

### User Story Dependencies

- **US1 (P1)** HelloWorld/DummyApp/NoLayout: No dependencies — foundation
- **US2 (P1)** Buttons: Best after US1 (builds on widget concepts)
- **US3 (P1)** Calculator: Best after US1 (builds on layout concepts)
- **US4 (P2)** SplitScreen: Can start after US1
- **US5 (P2)** Pager: Can start after US1
- **US6 (P2)** FullScreenDemo: Can start after US2 (uses buttons, widgets)
- **US7 (P2)** TextEditor+AnsiArt: Can start after US6 (uses menus, dialogs)
- **US8 (P3)** Alignment: Can start after US1 (basic layout)
- **US9 (P3)** Floats: Can start after US1 (basic layout)
- **US10 (P3)** Focus: Can start after US1 (basic layout)
- **US11 (P3)** ScrollablePanes: Can start after US1 (basic layout)
- **US12 (P3)** Margins/LinePrefixes: Can start after US1 (basic layout)
- **US13 (P3)** ColorColumn/CursorHL/AutoCompletion: Can start after US1 (basic layout)

### Within Each User Story

- Port from Python reference first
- Build and verify before moving to next story
- Each example must handle KeyboardInterrupt/EOFException gracefully

### Parallel Opportunities

Within Phase 1:
- T001 and T002 can run in parallel

Within each user story phase:
- Tasks marked [P] can run in parallel (different files)

Across user stories (after Phase 2 setup):
- US2, US3, US4, US5 can all start in parallel
- US8 through US13 can all start in parallel (all P3 SimpleDemos)

---

## Parallel Example: Phase 10 (User Story 8)

```bash
# Launch all alignment examples in parallel (different files, no dependencies):
Task: "Implement HorizontalSplit at examples/.../SimpleDemos/HorizontalSplit.cs"
Task: "Implement VerticalSplit at examples/.../SimpleDemos/VerticalSplit.cs"
Task: "Implement Alignment at examples/.../SimpleDemos/Alignment.cs"
Task: "Implement HorizontalAlign at examples/.../SimpleDemos/HorizontalAlign.cs"
Task: "Implement VerticalAlign at examples/.../SimpleDemos/VerticalAlign.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T005)
2. Complete Phase 2: US1 — HelloWorld, DummyApp, NoLayout (T006-T009)
3. **STOP and VALIDATE**: All 3 basic examples build and run
4. Developers can now see the foundational Application pattern

### Incremental Delivery

1. Setup + US1 → Foundation ready (3 examples)
2. Add US2 (Buttons) → Widget interaction (4 examples)
3. Add US3 (Calculator) → REPL pattern (5 examples)
4. Add US4-US7 → P2 stories (10 examples total)
5. Add US8-US13 → P3 stories (25 examples total)
6. Polish → Final validation

### Recommended Sequence for Single Developer

1. Phase 1: Setup (T001-T005)
2. Phase 2: US1 — Basic apps (T006-T009)
3. Phase 3: US2 — Buttons (T010-T011)
4. Phase 4: US3 — Calculator (T012-T013)
5. Phase 10: US8 — All 5 alignment SimpleDemos in parallel (T024-T029)
6. Phase 11: US9 — Both float SimpleDemos in parallel (T030-T032)
7. Phase 12: US10 — Focus SimpleDemos (T033-T034)
8. Phase 14: US12 — Margins + LinePrefixes in parallel (T038-T040)
9. Phase 15: US13 — ColorColumn + CursorHL + AutoCompletion in parallel (T041-T044)
10. Phase 5: US4 — SplitScreen (T014-T015)
11. Phase 6: US5 — Pager (T016-T017)
12. Phase 13: US11 — ScrollablePanes in parallel (T035-T037)
13. Phase 7: US6 — FullScreenDemo (T018-T019)
14. Phase 9: US7b — AnsiArtAndTextArea (T022-T023)
15. Phase 8: US7 — TextEditor (T020-T021)
16. Phase 16: Polish (T045-T049)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- All examples must catch KeyboardInterrupt and EOFException
- Python reference files are at `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/`
- Commit after each phase or logical group
- Stop at any checkpoint to validate story independently
- No unit tests — verification is via TUI Driver MCP tools and manual run
