# Full Sweep Checklist: Base Widgets

**Purpose**: Comprehensive pre-implementation requirements quality validation across API fidelity, thread safety, composition, edge cases, and measurability
**Created**: 2026-02-01
**Completed**: 2026-02-01
**Feature**: [spec.md](../spec.md)
**Audience**: Author (pre-implementation self-review)
**Depth**: Full sweep — all quality dimensions

## API Fidelity & Completeness

- [x] CHK001 - Are all 15 widget classes from Python `widgets/base.py` (lines 101-1081) and `widgets/dialogs.py` (lines 29-108) accounted for in the requirements? [Completeness, Spec §FR-001–FR-024]
  > ✅ Verified: Border (FR-001), TextArea (FR-002–FR-004), Label (FR-005–FR-006), Button (FR-007–FR-009), Frame (FR-010–FR-012), Shadow (FR-013), Box (FR-014), DialogList (FR-015), RadioList (FR-016), CheckboxList (FR-017), Checkbox (FR-018), ProgressBar (FR-019), VerticalLine/HorizontalLine (FR-020), Dialog (FR-021). All 15 classes accounted for.

- [x] CHK002 - Is the `_DialogList<T>` naming convention documented — specifically that the Python underscore-prefix convention for "internal" base classes maps to a public `DialogList<T>` in C#? [Clarity, Spec §FR-015]
  > ✅ Fixed: FR-015 now explicitly states: "C# public name for Python's internal `_DialogList`; the Python underscore-prefix convention for 'internal' base classes maps to a public generic class in C#." Naming normalized to `DialogList<T>` across spec.md, plan.md, data-model.md, and research.md.

- [x] CHK003 - Are all mutable properties from Python (`text`, `document`, `accept_handler` on TextArea; `text`, `handler` on Button; `title`, `body` on Frame/Dialog; `percentage` on ProgressBar) explicitly specified as get/set in the requirements? [Completeness, Contracts]
  > ✅ Fixed: Added FR-024 that comprehensively lists all get/set properties across every widget: TextArea (10), Button (2), Frame (2), Dialog (2), Label (1), ProgressBar (1), Box (6), DialogList (4), Checkbox (1).

- [x] CHK004 - Are the `selectOnFocus` parameter and its behavior (auto-select when focus enters the list) documented for `DialogList<T>`? [Gap, Contract dialog-list.md]
  > ✅ Fixed: FR-015 constructor parameters section now documents `selectOnFocus` with behavior description: "when true, auto-selects the focused item when focus enters the list."

- [x] CHK005 - Is it specified that `RadioList<T>` always overrides `multipleSelection` to `false` regardless of what's passed in the constructor? [Clarity, Contract radio-list.md]
  > ✅ Fixed: FR-016 now states: "RadioList MUST always override `multipleSelection` to `false` in the base class constructor call, regardless of any value passed by the caller."

- [x] CHK006 - Are all 8 key binding groups for `DialogList<T>` (Up/k, Down/j, PageUp, PageDown, Enter, Space, number shortcuts, character jump) individually specified with their exact behaviors? [Completeness, Spec §SC-009]
  > ✅ Fixed: FR-015 now contains a numbered list of all 8 key binding groups with exact behaviors: (1) Up/k with clamping, (2) Down/j with clamping, (3) PageUp by visible lines, (4) PageDown by visible lines, (5) Enter toggle, (6) Space toggle, (7) Number shortcuts 1-9 with showNumbers gate, (8) Character jump with cycling.

- [x] CHK007 - Is the `showNumbers` parameter's interaction with number key shortcuts (1-9) explicitly specified — what happens when `showNumbers=false` but a number key is pressed? [Clarity, Contract dialog-list.md]
  > ✅ Fixed: FR-015 key binding group 7 now states: "Only active when `showNumbers=true`; when `showNumbers=false`, number key presses are ignored (fall through to character jump)."

- [x] CHK008 - Are all TextArea constructor parameters (22 parameters) individually documented with their default values and behaviors? [Completeness, Contract text-area.md]
  > ✅ Fixed: FR-002 now lists all 22 parameters by name and references the contract: "See contract text-area.md for full parameter defaults and behaviors." The contract already documents all parameters with defaults.

- [x] CHK009 - Is the `name` parameter on TextArea documented — does it map to `Buffer.Name` for identification? [Gap, Contract text-area.md]
  > ✅ Fixed: FR-002 now includes `name` in the parameter list and states: "The `name` parameter maps to `Buffer.Name` for identification."

- [x] CHK010 - Are the `@char` parameter semantics for Box specified — what fill character is used for padding windows? [Clarity, Contract box.md]
  > ✅ Fixed: FR-014 now documents `@char` (string?, fill character for padding Windows — null means no fill character).

- [x] CHK011 - Is it specified that `Checkbox.ShowScrollbar` must be overridden to `false` at the class level (not instance level)? [Clarity, Spec §FR-018, Contract checkbox.md]
  > ✅ Fixed: FR-018 now states: "MUST override `ShowScrollbar` to `false` at the class level (as a `new` property or override, not via constructor parameter), matching the Python class-level `show_scrollbar = False` attribute."

- [x] CHK012 - Are the `numberStyle` parameter for DialogList and its CSS class name documented? [Gap, Contract dialog-list.md]
  > ✅ Fixed: FR-015 constructor parameters now include `numberStyle (style for the "N. " number prefix, e.g., class:radio-number)`. FR-016 lists `class:radio-number` in RadioList's style parameters.

## Requirement Clarity & Precision

- [x] CHK013 - Is FR-022 accurate? It states widgets implement `IContainer` with `GetContainer()`, but plan.md and contracts specify `IMagicContainer` with `PtContainer()`. Is this inconsistency resolved? [Conflict, Spec §FR-022 vs Plan §Key Design Decisions]
  > ✅ Fixed: FR-022 rewritten to: "All widgets MUST implement the `IMagicContainer` interface, returning their inner container from `PtContainer()`. This is the C# equivalent of Python's `__pt_container__()` protocol. Widgets do NOT implement `IContainer` directly."

- [x] CHK014 - Is "centered between left and right symbols" in FR-008 quantified with the exact centering algorithm (pad-left then pad-right to `Width - symbolWidths`)? [Clarity, Spec §FR-008]
  > ✅ Fixed: FR-008 now contains the complete 3-step centering algorithm: (1) compute available width using UnicodeWidth, (2) pad-left to `(availableWidth + Text.Length) / 2`, (3) pad-right to fill remaining space.

- [x] CHK015 - Is it specified which Unicode width calculation to use for Button text centering — `UnicodeWidth.GetWidth()` from Wcwidth, matching the Python `fragment_list_width` calculation? [Clarity, Contract button.md]
  > ✅ Fixed: FR-008 now explicitly references `UnicodeWidth.GetWidth()` in the centering algorithm formula. The dependencies table also lists `UnicodeWidth` from `Stroke.Utilities` (Feature 024) used by Button and Label.

- [x] CHK016 - Is "transparent Float windows" in FR-013 clarified — does `transparent=true` mean the Float renders content but allows underlying content to show through where no characters are drawn? [Clarity, Spec §FR-013]
  > ✅ Fixed: FR-013 now defines transparency: "`transparent=true` on the Float — the Float renders its content but allows underlying content to show through where no characters are drawn by the Float's content window." Also specifies exact Float coordinates with cell-based offset explanation.

- [x] CHK017 - Is the ProgressBar default value of 60% explicitly stated in the spec requirements, or only in the contract? [Completeness, Contract progress-bar.md]
  > ✅ Fixed: FR-019 now states: "Default percentage MUST be 60 (matching Python)."

- [x] CHK018 - Is the term "configurable padding (overall and per-side)" in FR-014 clarified with the fallback logic: per-side overrides general padding, with null meaning no padding on that side? [Clarity, Spec §FR-014, Contract box.md]
  > ✅ Fixed: FR-014 now specifies: "Padding resolution uses fallback logic: each side resolves as `PaddingLeft ?? Padding`, `PaddingRight ?? Padding`, etc. When both overall Padding and all per-side paddings are null, no padding Windows are created."

- [x] CHK019 - Is the phrase "keyboard navigation (Up/Down/j/k)" in FR-015 clear that `j`/`k` are Vi-style navigation aliases (not additional bindings), and are they always active regardless of editing mode? [Clarity, Spec §FR-015]
  > ✅ Fixed: FR-015 key binding group 2 now states: "The `j`/`k` keys are Vi-style navigation aliases and are always active regardless of editing mode."

- [x] CHK020 - Are the exact style strings for each widget class documented (e.g., `class:button`, `class:button.focused`, `class:button.arrow`, `class:button.text`)? The spec mentions some but not all sub-styles. [Completeness, Spec §FR-009, Contracts]
  > ✅ Fixed: FR-009 now lists all Button sub-styles: `class:button`, `class:button.focused`, `class:button.arrow`, `class:button.text`. FR-005 specifies Label: `class:label`. FR-010 specifies Frame: `class:frame`, `class:frame.border`, `class:frame.label`. FR-013 specifies Shadow: `class:shadow`. FR-016 specifies RadioList styles. FR-017 specifies CheckboxList styles. FR-019 specifies ProgressBar: `class:progress-bar.used`, `class:progress-bar`. FR-020 specifies lines: `class:line,vertical-line`, `class:line,horizontal-line`. FR-021 specifies Dialog: `class:dialog.body`, `class:dialog`.

## Consistency Across Artifacts

- [x] CHK021 - Does the spec's FR-022 (`IContainer`/`GetContainer()`) align with the plan's decision that all widgets implement `IMagicContainer`/`PtContainer()`? One artifact must be corrected. [Conflict, Spec §FR-022 vs Plan]
  > ✅ Fixed: Same as CHK013. FR-022 corrected to `IMagicContainer`/`PtContainer()`. All artifacts now consistent.

- [x] CHK022 - Is the `Box.keyBindings` parameter consistent between the contract (which includes it) and the spec (which doesn't mention it)? [Consistency, Contract box.md vs Spec §FR-014]
  > ✅ Fixed: FR-014 now documents `keyBindings` parameter: "keyBindings (IKeyBindingsBase?, passed as `keyBindings=null` to the inner HSplit matching Python behavior)."

- [x] CHK023 - Are the Frame constructor parameters (`style`, `keyBindings`, `modal`) consistent between the contract and the spec? The spec only mentions body, title, and border characters. [Consistency, Contract frame.md vs Spec §FR-010]
  > ✅ Fixed: FR-010 now lists all 7 constructor parameters: `body`, `title`, `style`, `width`, `height`, `keyBindings`, `modal` — with defaults for each. Specifies the HSplit passes through width, height, keyBindings, and modal.

- [x] CHK024 - Is the Dialog's `has_completions` filter behavior (preventing Tab from cycling focus when completion menu is open) documented in both the spec and contract consistently? [Consistency, Contract dialog.md vs Spec §FR-021]
  > ✅ Fixed: FR-021 now states: "Tab/Shift-Tab MUST cycle focus forward/backward, gated by a `~hasCompletions` filter (Tab/Shift-Tab are suppressed when a completion menu is open, allowing the completion system to handle Tab)."

- [x] CHK025 - Is the naming `_DialogList<T>` vs `DialogList<T>` consistent across spec, plan, data-model, and contracts? The spec uses `_DialogList<T>` while contracts use `DialogList<T>`. [Consistency, Spec §FR-015 vs Contract dialog-list.md]
  > ✅ Fixed: All artifacts now use `DialogList<T>` consistently. Updated: spec.md (FR-015, FR-016, FR-017, Key Entities), plan.md (5 references), data-model.md (3 references), research.md (4 references). The Python origin `_DialogList` is noted in FR-015 as context.

- [x] CHK026 - Are the CheckboxList constructor parameters consistent with DialogList's base constructor? The CheckboxList contract omits `showNumbers`, `selectOnFocus`, `showCursor`, `showScrollbar` — is this intentional or a gap? [Consistency, Contract checkbox-list.md vs dialog-list.md]
  > ✅ Fixed: FR-017 now documents this as intentional: "CheckboxList intentionally omits `showNumbers`, `selectOnFocus`, `showCursor`, `showScrollbar` from its constructor — these use the base class defaults (`false`, `false`, `true`, `true` respectively). This matches the Python source where `CheckboxList.__init__` only passes style parameters."

## Thread Safety Requirements

- [x] CHK027 - Are thread safety requirements for `DialogList<T>` specific about which operations are atomic vs. which compound operations require external synchronization? [Clarity, Data-Model §DialogList]
  > ✅ Fixed: Data-model.md DialogList section now specifies: individual property reads/writes are independently atomic (each acquires/releases lock); compound operations like _HandleEnter require single lock scope; callers performing cross-property compound operations must synchronize externally.

- [x] CHK028 - Is it specified that the `_HandleEnter` logic in DialogList (read selectedIndex → lookup value → modify CurrentValues) must be protected by a single lock acquisition? [Completeness, Contract dialog-list.md §Thread Safety]
  > ✅ Fixed: Both data-model.md and dialog-list.md contract now specify: "The `_HandleEnter` logic (read `_selectedIndex` → lookup `values[_selectedIndex].Value` → add/remove from `CurrentValues` or set `CurrentValue`) MUST execute under a single lock acquisition."

- [x] CHK029 - Are thread safety implications of `ProgressBar.Percentage` setter documented — specifically that setting the percentage also updates `Label.Text`, and both must happen within the same lock scope? [Clarity, Contract progress-bar.md §Thread Safety]
  > ✅ Fixed: Both data-model.md and progress-bar.md contract now specify the compound operation must be atomic: `using (_lock.EnterScope()) { _percentage = value; Label.Text = $"{value}%"; }`. Ensures a reader never sees inconsistent percentage/label state.

- [x] CHK030 - Is it documented that TextArea's mutable properties (`Completer`, `Lexer`, `AutoSuggest`, `ReadOnly`, `WrapLines`, `Validator`) are reference-type fields with atomic writes, and that this is sufficient because they are read via lambda closures? [Clarity, Data-Model §TextArea]
  > ✅ Fixed: Data-model.md TextArea thread safety section now explains: fields are reference-type with CLR-guaranteed atomic writes, read via lambda closures captured in Buffer/BufferControl constructors, no Lock needed, no compound read-modify-write operations.

- [x] CHK031 - Is it specified that `Button.Handler` and `Button.Text` writes are atomic (reference-type) and no additional synchronization is needed? [Gap, Data-Model §Button]
  > ✅ Fixed: Data-model.md Button thread safety section now documents: both `Handler` (Action?) and `Text` (string) are reference-type with CLR atomic writes. FormattedTextControl reads via lambda. Null-check guard on Handler prevents stale invocation. No Lock needed.

- [x] CHK032 - Are the thread safety requirements for `DialogList.CurrentValues` (a `List<T>`) clear about whether concurrent add/remove operations are safe, or whether the lock must always be held for any mutation? [Clarity, Contract dialog-list.md]
  > ✅ Fixed: Both data-model.md and dialog-list.md contract now specify: "All mutations (Add, Remove, Clear, reassignment) MUST be performed while holding `_lock`. Concurrent read-without-lock is NOT safe because `List<T>` is not thread-safe." The `Contains` query also requires the lock.

## Edge Case & Scenario Coverage

- [x] CHK033 - Is the behavior specified when `RadioList` is passed a `default` value that is null (reference type) vs. when T is a value type? [Coverage, Edge Case, Contract radio-list.md]
  > ✅ Fixed: Edge Cases section now specifies: "When `default` is null, `defaultValues: null` is passed to the base class, which falls back to the first item. For value types, `default(T)` is used (e.g., 0 for int); if that value is not in the values list, falls back to first item."

- [x] CHK034 - Is the behavior specified when `TextArea.Text` setter is called with null? Does it set an empty Document or throw? [Gap, Edge Case]
  > ✅ Fixed: Added to Edge Cases: "TextArea.Text setter with null MUST set an empty Document (equivalent to `new Document("")`), not throw." Also added to FR-004.

- [x] CHK035 - Is it defined what happens when `Button.Width` is smaller than the combined width of `LeftSymbol + RightSymbol`? [Gap, Edge Case]
  > ✅ Fixed: FR-008 now specifies: "When Width is smaller than the combined width of LeftSymbol + RightSymbol, the text area has zero or negative width — the symbols are still rendered, matching Python behavior." Also added to Edge Cases section.

- [x] CHK036 - Is the behavior specified when `Frame.Title` transitions from a non-empty value to an empty value at runtime — does the ConditionalContainer switch reactively? [Coverage, Spec §Edge Cases]
  > ✅ Fixed: Edge Cases section now specifies: "The ConditionalContainer reactively switches to the no-title top border row. The Condition filter re-evaluates when the layout system calls PreferredWidth/PreferredHeight/WriteToScreen." Also added to FR-011.

- [x] CHK037 - Are the Float coordinate values for Shadow documented for both floats — specifically the negative offset values (`bottom=-1`, `right=-1`) and whether they are pixel/cell offsets or relative positions? [Clarity, Contract shadow.md]
  > ✅ Fixed: FR-013 now documents both floats with exact coordinates and states: "All offset values are cell-based offsets, not relative positions." Each float's position is described in terms of cell offsets relative to the body.

- [x] CHK038 - Is it specified what happens when `Box.Padding` and all per-side paddings are null? Does the body fill the entire space with no padding windows? [Gap, Edge Case, Contract box.md]
  > ✅ Fixed: FR-014 now specifies: "When both overall Padding and all per-side paddings are null, no padding Windows are created — the body fills the entire space." Also added to Edge Cases section.

- [x] CHK039 - Is the `DialogList` character jump behavior specified when multiple items start with the same character — does it cycle through them or stay on the first match? [Coverage, Spec §US-4 scenario 5]
  > ✅ Fixed: FR-015 key binding group 8 now states: "jump to the first item whose label starts with that character (case-insensitive). When multiple items start with the same character, cycles through them on repeated presses." Also added to Edge Cases section.

- [x] CHK040 - Is the ProgressBar behavior specified when `Percentage` is set to negative values — the spec mentions "may produce unexpected layouts" but doesn't define what `D(weight=negative)` means? [Clarity, Spec §Edge Cases]
  > ✅ Fixed: Edge Cases section now specifies: "`D(weight=negative)` produces a Dimension with weight ≤ 0, which the layout system treats as zero allocation." The label still updates (e.g., "-5%").

- [x] CHK041 - Is it specified what `Dialog` does when the buttons list is empty vs. null? Both may result in "no buttons" but through different code paths. [Gap, Edge Case, Contract dialog.md]
  > ✅ Fixed: FR-021 now specifies: "When buttons list is null or empty, the body is used directly as frame content without a button row." Edge Cases section adds: "Null and empty are functionally equivalent."

- [x] CHK042 - Is the behavior specified when `DialogList.Values` contains duplicate `T` values — does selection compare by reference or by value equality? [Gap, Edge Case]
  > ✅ Fixed: Added to Edge Cases: "Selection operates by index, not by value equality. The _HandleEnter logic reads `values[selectedIndex].Value`. CurrentValues uses List<T> default equality (reference equality for classes, value equality for structs) for Contains/Remove operations."

## Acceptance Criteria Quality

- [x] CHK043 - Is SC-002 measurable — "can be used interchangeably in any container layout" — what specific containers must accept widgets, and how is this verified without running the full application? [Measurability, Spec §SC-002]
  > ✅ Fixed: SC-002 now specifies exact containers (HSplit, VSplit, FloatContainer, ConditionalContainer, DynamicContainer, Frame, Box, Shadow, Dialog) and verification method: "unit tests that construct each widget and call PtContainer() returning a non-null IContainer, and by composition tests embedding widgets in at least HSplit and VSplit containers."

- [x] CHK044 - Is SC-003 verifiable — "all configuration combinations without runtime errors" — is the expected test matrix defined? With 7+ boolean/optional parameters, combinatorial testing needs bounds. [Measurability, Spec §SC-003]
  > ✅ Fixed: SC-003 now defines 6 specific representative combinations that cover key interaction points: (multiline+scrollbar+lineNumbers), (single-line), (password), (readOnly+text setter), (completer+completeWhileTyping), (lexer+validator). Explicitly states "exhaustive combinatorial testing of all 7+ boolean parameters is not required."

- [x] CHK045 - Is SC-009 measurable — "works identically to the Python Prompt Toolkit implementation" — what specific behavioral assertions define "identical"? Is there a reference test suite or comparison method? [Measurability, Spec §SC-009]
  > ✅ Fixed: SC-009 now defines "identical" operationally: "same cursor position after same key sequence, same selection state after same toggle sequence." Specifies 8 tests mapping to 8 key binding groups as the verification method.

- [x] CHK046 - Is the 80% code coverage target in SC-001 achievable given that only 2 button tests are mapped in `test-mapping.md` (lines 708-726)? Are additional tests beyond the mapping required? [Measurability, Spec §SC-001, Plan §Constitution Check IX]
  > ✅ Fixed: SC-001 now states: "test-mapping.md maps only 2 Button tests (lines 708-726). Additional tests beyond the mapping MUST be written as needed to reach the coverage target — the mapping is a minimum, not a ceiling."

- [x] CHK047 - Can SC-007 "renders correct box-drawing characters for all border positions" be objectively verified via unit tests without a real terminal renderer? [Measurability, Spec §SC-007]
  > ✅ Fixed: SC-007 now specifies verification method: "(1) construct a Frame and inspect the border Window `char` properties matching `Border.*` constants, (2) verify ConditionalContainer switches between title and no-title top rows when Title transitions." These are structural assertions, not rendering assertions.

## Dependencies & Assumptions

- [x] CHK048 - Is the assumption that `SearchToolbar` (Feature 044) is available validated — does TextArea's `searchField` parameter depend on a specific SearchToolbar API surface? [Assumption, Spec §Assumptions]
  > ✅ Fixed: Assumptions section now states: "SearchToolbar (Feature 044) exposes a `Control` property returning `SearchBufferControl`, which TextArea extracts for search integration. TextArea's `searchField` parameter depends on this specific API surface." Dependency table pins to Feature 044.

- [x] CHK049 - Is the assumption that `DynamicContainer` accepts a `Func<IContainer>` (not `Func<AnyContainer>`) validated? The plan shows `() => this.Body.ToContainer()` but the contract shows `() => this.Body`. [Assumption, Research RT-7]
  > ✅ Fixed: Assumptions section now states: "DynamicContainer accepts a `Func<IContainer>` parameter (not `Func<AnyContainer>`). Callers must use `() => this.Body.ToContainer()` to convert AnyContainer to IContainer. This is validated by Research RT-7."

- [x] CHK050 - Is the assumption that `Window` constructor overloads accepting `Func<Dimension?>` will be added documented as a dependency? Research RT-3 identifies this need but it's not in the spec requirements. [Dependency, Research RT-3]
  > ✅ Fixed: Assumptions section now documents: "Window constructor currently accepts `Dimension?` for width/height. For dynamic dimensions, Window constructor overloads accepting `Func<Dimension?>` MAY need to be added as a prerequisite. Research RT-3 identifies this need. If such overloads are not available, widgets will use the existing lambda-wrapping approach internally."

- [x] CHK051 - Are the existing infrastructure dependencies (FormattedTextControl, BufferControl, HSplit, VSplit, FloatContainer, ConditionalContainer, DynamicContainer, Float) version-pinned or feature-pinned to specific prior feature implementations? [Dependency, Spec §Assumptions]
  > ✅ Fixed: Assumptions section now contains a comprehensive dependency table with 24 rows, each pinned to a specific feature number (e.g., HSplit/VSplit from Feature 029, Buffer from Feature 007, etc.).

- [x] CHK052 - Is it documented that `Template` formatted text type is required for Frame's title rendering (`Template(" {} ").Format(this.Title)`)? [Dependency, Contract frame.md]
  > ✅ Fixed: Dependency table includes `Template` from `Stroke.FormattedText` (Feature 015) with usage note: "Frame (title rendering: `Template(" {} ").Format(this.Title)`)."

- [x] CHK053 - Does the spec or plan document the `FocusFunctions.FocusNext` and `FocusFunctions.FocusPrevious` dependency used by Dialog's Tab/Shift-Tab and Button Left/Right navigation? [Dependency, Contract dialog.md §Notes]
  > ✅ Fixed: Dependency table includes `FocusFunctions.FocusNext`, `FocusFunctions.FocusPrevious` from `Stroke.KeyBinding.Bindings` (Feature 040). FR-021 now states: "Focus navigation uses `FocusFunctions.FocusNext` and `FocusFunctions.FocusPrevious`."

## Notes

- All 53 items completed on 2026-02-01
- Items are numbered sequentially for easy reference
- **CHK013/CHK021**: Resolved — FR-022 corrected from `IContainer`/`GetContainer()` to `IMagicContainer`/`PtContainer()`. All artifacts now consistent.
- **CHK025**: Resolved — `_DialogList<T>` normalized to `DialogList<T>` across all 4 artifacts (spec, plan, data-model, research).
- **CHK050**: Documented as conditional dependency — Window overloads for `Func<Dimension?>` may be needed but the existing internal lambda-wrapping approach is an alternative.
