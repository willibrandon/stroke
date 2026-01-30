# Feature Specification: Input Processors

**Feature Branch**: `031-input-processors`
**Created**: 2026-01-29
**Status**: Draft
**Input**: User description: "Implement the processor system that transforms fragments before BufferControl renders them to the screen. Processors can insert content, highlight text, and transform the display."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Fragment Transformation Pipeline (Priority: P1)

A terminal application developer needs a processor pipeline that transforms styled text fragments before they are rendered to the screen. The pipeline must support chaining multiple processors together, where each processor receives the output of the previous one, and cursor position mappings are correctly composed across the chain.

**Why this priority**: The processor pipeline is the foundational infrastructure that all individual processors depend on. Without the core `IProcessor` interface, `TransformationInput`, `Transformation`, and `MergeProcessors`, no other processor can function.

**Independent Test**: Can be fully tested by creating simple processors that insert/remove characters and verifying that chained position mappings correctly translate cursor positions between source and display coordinates.

**Acceptance Scenarios**:

1. **Given** a list of fragments and an identity processor, **When** the processor is applied, **Then** the fragments are returned unchanged and position mappings are identity functions.
2. **Given** two processors that each shift content by a fixed offset, **When** they are merged and applied, **Then** the combined position mapping correctly composes both offsets.
3. **Given** a `TransformationInput` with all required fields, **When** a processor accesses its properties, **Then** all values (buffer control, document, line number, fragments, width, height, source-to-display function, get-line function) are correctly available.
4. **Given** an empty list of processors, **When** `MergeProcessors` is called, **Then** a `DummyProcessor` is returned that passes fragments through unchanged.
5. **Given** a single processor, **When** `MergeProcessors` is called, **Then** that same processor is returned without wrapping.

---

### User Story 2 - Password Masking and Text Insertion (Priority: P1)

A terminal application developer building a password prompt needs to mask all input characters with a replacement character. Additionally, developers building REPL prompts need to insert styled text before or after the user input (e.g., prompt prefixes like ">>> " or suffixes).

**Why this priority**: Password masking and prompt text insertion are among the most commonly used processors in real terminal applications and are essential for building basic interactive prompts.

**Independent Test**: Can be tested by creating a `PasswordProcessor` with a given mask character, applying it to fragments containing text, and verifying all characters are replaced. `BeforeInput` and `AfterInput` can be tested by verifying text insertion at the correct line and correct position mapping offsets.

**Acceptance Scenarios**:

1. **Given** fragments containing "hello", **When** `PasswordProcessor` with default "*" mask is applied, **Then** the output fragments contain "*****" with preserved styles.
2. **Given** fragments containing "abc", **When** `PasswordProcessor` with "." mask is applied, **Then** the output fragments contain "..." with preserved styles.
3. **Given** multi-line content, **When** `BeforeInput` is applied, **Then** text is prepended only to the first line (line 0) and source-to-display/display-to-source mappings are correctly offset by the prefix length.
4. **Given** multi-line content, **When** `AfterInput` is applied, **Then** text is appended only to the last line.
5. **Given** `BeforeInput` with a callable text argument, **When** the processor is applied, **Then** the callable is invoked to get the current text value.
6. **Given** fragments containing multi-byte Unicode text (e.g., CJK characters), **When** `PasswordProcessor` is applied, **Then** each character (not each byte) is replaced with the mask character, preserving styles.

---

### User Story 3 - Search and Selection Highlighting (Priority: P1)

A terminal application developer needs to highlight search matches and text selections within the displayed content. Search matches should be highlighted with a distinguishable style, with the current match (under cursor) having a distinct style. Selected text should be highlighted across line boundaries.

**Why this priority**: Search highlighting and selection visualization are critical interactive features that make the text editing experience usable. They are required for both Emacs (incremental search) and Vi (visual mode) editing modes.

**Independent Test**: Can be tested by constructing fragments, a document with known cursor position and search state, and verifying that the correct character positions receive the appropriate style class annotations.

**Acceptance Scenarios**:

1. **Given** a line containing "foo bar foo" with search text "foo", **When** `HighlightSearchProcessor` is applied, **Then** both occurrences of "foo" receive the "search" style class.
2. **Given** the cursor is positioned on the first "foo" match, **When** `HighlightSearchProcessor` is applied, **Then** the match under the cursor receives "search.current" style class while other matches receive "search" style class.
3. **Given** `HighlightIncrementalSearchProcessor`, **When** applied, **Then** it uses "incsearch" and "incsearch.current" style classes and reads search text from the search buffer.
4. **Given** a document with a selection spanning columns 3-7 on a line, **When** `HighlightSelectionProcessor` is applied, **Then** characters at display positions 3-6 receive the "selected" style class.
5. **Given** an empty line within a selection range, **When** `HighlightSelectionProcessor` is applied, **Then** a single space with "selected" style class is inserted to visualize the selection.
6. **Given** a selection spanning multiple lines (e.g., from line 1 column 3 to line 3 column 5), **When** `HighlightSelectionProcessor` is applied to each line independently, **Then** line 1 is highlighted from column 3 to end, line 2 is fully highlighted, and line 3 is highlighted from start to column 5, with a trailing space appended on lines where the selection extends past the line end.

---

### User Story 4 - Tab Handling and Whitespace Visualization (Priority: P2)

A terminal application developer needs to render tab characters as a configurable number of spaces (aligned to tab stops) and optionally visualize leading and trailing whitespace with replacement characters.

**Why this priority**: Tab handling is essential for correct text display in code editors, and whitespace visualization aids in identifying formatting issues. These are common features in text editing applications.

**Independent Test**: Can be tested by creating fragments containing tab characters at various column positions, applying `TabsProcessor`, and verifying the correct number of replacement characters and accurate position mappings.

**Acceptance Scenarios**:

1. **Given** fragments with a tab at column 0 and tab width 4, **When** `TabsProcessor` is applied, **Then** the tab is replaced with the first-char separator followed by 3 second-char separators (4 total characters to next tab stop).
2. **Given** fragments with a tab at column 2 and tab width 4, **When** `TabsProcessor` is applied, **Then** the tab is replaced with 2 characters (to reach column 4, the next tab stop).
3. **Given** `TabsProcessor` with custom tab width and characters, **When** applied, **Then** source-to-display and display-to-source position mappings correctly translate positions across tab expansions.
4. **Given** fragments with leading spaces, **When** `ShowLeadingWhiteSpaceProcessor` is applied, **Then** leading spaces are replaced with the configured visible character while non-leading spaces are unchanged.
5. **Given** fragments with trailing spaces, **When** `ShowTrailingWhiteSpaceProcessor` is applied, **Then** trailing spaces are replaced with the configured visible character while non-trailing spaces are unchanged.

---

### User Story 5 - Bracket Matching (Priority: P2)

A terminal application developer building a code editor or REPL needs matching brackets to be highlighted when the cursor is on or adjacent to a bracket character. The system should search within a configurable distance for the matching bracket.

**Why this priority**: Bracket matching is a standard code editing feature that significantly improves readability and helps users identify structure in code.

**Independent Test**: Can be tested by constructing a document with nested brackets, positioning the cursor on a bracket, and verifying that both the cursor bracket and its match receive appropriate style classes.

**Acceptance Scenarios**:

1. **Given** a document containing "(hello)" with cursor on "(", **When** `HighlightMatchingBracketProcessor` is applied to the line, **Then** both "(" and ")" positions receive bracket-matching style classes.
2. **Given** a document containing "([{}])" with cursor on "[", **When** applied, **Then** the matching "]" is highlighted, not ")" or "}".
3. **Given** the cursor is right after a closing bracket, **When** applied, **Then** the processor checks the character before the cursor and highlights the matching pair.
4. **Given** the matching bracket is beyond `maxCursorDistance`, **When** applied, **Then** no highlighting occurs.
5. **Given** the application is in the "done" state, **When** applied, **Then** no highlighting occurs.

---

### User Story 6 - Conditional and Dynamic Processors (Priority: P2)

A terminal application developer needs to conditionally enable/disable processors based on application state (e.g., only highlight search when search mode is active) and dynamically select processors at runtime.

**Why this priority**: Conditional and dynamic processors provide the composability layer that allows complex processor configurations to be built from simple pieces, which is essential for real-world prompt configurations.

**Independent Test**: Can be tested by creating a `ConditionalProcessor` with a filter that can be toggled, applying it, and verifying the inner processor is only applied when the filter evaluates to true. `DynamicProcessor` can be tested by providing a callable that returns different processors.

**Acceptance Scenarios**:

1. **Given** a `ConditionalProcessor` wrapping a `PasswordProcessor` with a filter that returns true, **When** applied, **Then** the password masking is applied.
2. **Given** a `ConditionalProcessor` with a filter that returns false, **When** applied, **Then** fragments pass through unchanged.
3. **Given** a `DynamicProcessor` whose callable returns a `PasswordProcessor`, **When** applied, **Then** password masking is applied.
4. **Given** a `DynamicProcessor` whose callable returns null, **When** applied, **Then** fragments pass through unchanged (DummyProcessor behavior).

---

### User Story 7 - Auto-Suggestion, Arg Display, Multiple Cursors, and Reverse Search (Priority: P3)

A terminal application developer needs specialized processors for appending auto-suggestion text after the input, displaying the repeat-count argument prefix (e.g., "2x"), showing multiple cursors in Vi block insert mode, and rendering the reverse-search prompt.

**Why this priority**: These are specialized processors for specific interactive features. They build on the core pipeline and are needed for full editing mode parity but are less universally used than the P1/P2 processors.

**Independent Test**: Can be tested individually by constructing appropriate document/buffer state and verifying each processor's specific output.

**Acceptance Scenarios**:

1. **Given** a buffer with a suggestion "world" and the cursor at the end of the last line, **When** `AppendAutoSuggestion` is applied, **Then** "world" is appended with the "auto-suggestion" style class.
2. **Given** a buffer with no suggestion, **When** `AppendAutoSuggestion` is applied, **Then** an empty string is appended (no visible change).
3. **Given** the key processor has an active arg value of "3", **When** `ShowArg` is applied to line 0, **Then** the text "(arg: 3) " is prepended with appropriate style classes.
4. **Given** Vi block insert mode is active with multiple cursor positions on a line, **When** `DisplayMultipleCursors` is applied, **Then** each cursor position receives the "multiple-cursors" style class.
5. **Given** a search buffer control in reverse search mode, **When** `ReverseSearchProcessor` is applied to line 0, **Then** the output includes the reverse search prompt format "(reverse-i-search)`<query>': <matched line>".
6. **Given** a buffer with a suggestion "world" but the cursor is NOT at the end of the document, **When** `AppendAutoSuggestion` is applied to the last line, **Then** an empty string is appended (no visible suggestion text).
7. **Given** the key processor has a null arg value, **When** `ShowArg` is applied to line 0, **Then** no text is prepended (empty fragment list returned).
8. **Given** Vi block insert mode is NOT active, **When** `DisplayMultipleCursors` is applied, **Then** fragments pass through unchanged.

---

### Edge Cases

- What happens when a processor receives an empty fragment list? It should return an empty fragment list (or insert content if that is its purpose, e.g., empty line in selection).
- What happens when source-to-display or display-to-source is called with an out-of-range position? The mapping functions should handle boundary positions gracefully.
- What happens when `TabsProcessor` encounters a tab at exactly a tab stop boundary? It should expand to a full tab width.
- What happens when `HighlightSearchProcessor` encounters a case-insensitive search? It should use case-insensitive regex matching.
- What happens when `HighlightMatchingBracketProcessor` encounters unmatched brackets? No highlighting should occur.
- What happens when `BeforeInput` text is a callable that returns formatted text? The processor should evaluate the callable and convert the result to formatted text.
- What happens when `MergeProcessors` contains nested `_MergedProcessor` instances? The position mapping chain should still compose correctly.
- What happens when `DisplayMultipleCursors` has a cursor position beyond the end of the line? A space character with cursor style should be appended.
- What happens when `ReverseSearchProcessor` cannot find the main buffer? It should return the fragments unchanged.
- What happens when `TabsProcessor` has a tab stop width of 1? Each tab should expand to exactly 1 character (the first-char separator only).
- What happens when `ShowLeadingWhiteSpaceProcessor` encounters a line that is entirely whitespace? All space characters should be replaced with the visible character, since all spaces are "leading" when there are no non-space characters.

## Clarifications

### Session 2026-01-29

- Q: Python's `ShowTrailingWhiteSpaceProcessor` uses the style class `"class:training-whitespace"` (missing the 'l' in 'trailing'). Should the C# port preserve the Python typo or use the corrected spelling? → A: Fix the typo. Use `"class:trailing-whitespace"` in the C# port. This is a documented deviation from Python Prompt Toolkit per Constitution I — the original has a clear bug (missing 'l' in 'trailing'), and correcting it improves developer experience.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an `IProcessor` interface with a single `ApplyTransformation` method that accepts `TransformationInput` and returns `Transformation`.
- **FR-002**: System MUST provide `TransformationInput` as a data carrier containing buffer control, document, line number, source-to-display function, fragments, width, height, and optional get-line function.
- **FR-003**: System MUST provide `Transformation` as a result type containing transformed fragments and optional source-to-display and display-to-source position mapping functions, defaulting to identity functions when not provided.
- **FR-004**: `DummyProcessor` MUST return fragments unchanged with identity position mappings.
- **FR-005**: `PasswordProcessor` MUST replace each character in every fragment with a configurable mask character (default "*"), preserving fragment styles and handlers.
- **FR-006**: `HighlightSearchProcessor` MUST find all matches of the search text in a line's text using `Regex.Escape` to treat the search text as a literal pattern, explode fragments to single characters, and apply "search" style class (appended as `" class:search "`) to matches and "search.current" style class (appended as `" class:search.current "`) to the match under the cursor.
- **FR-007**: `HighlightSearchProcessor` MUST support case-insensitive matching when the search state's ignore-case setting is active.
- **FR-008**: `HighlightSearchProcessor` MUST not highlight when the application is in the "done" state or when search text is empty.
- **FR-009**: `HighlightIncrementalSearchProcessor` MUST extend `HighlightSearchProcessor` with "incsearch"/"incsearch.current" style classes and read search text from the search buffer rather than search state.
- **FR-010**: `HighlightSelectionProcessor` MUST highlight selected characters using the "selected" style class based on the document's selection range at the current line.
- **FR-011**: `HighlightSelectionProcessor` MUST insert a visible space with "selected" style when a selection covers an empty line.
- **FR-012**: `HighlightSelectionProcessor` MUST append a space with "selected" style when the selection extends past the end of a line.
- **FR-013**: `BeforeInput` MUST prepend styled text to line 0 only and provide correct position mapping functions that account for the inserted prefix length.
- **FR-014**: `BeforeInput` MUST support both plain text and callable text sources (via `AnyFormattedText`).
- **FR-015**: `AfterInput` MUST append styled text to the last line only (the line at index `Document.LineCount - 1`).
- **FR-016**: `AfterInput` MUST support both plain text and callable text sources (via `AnyFormattedText`).
- **FR-017**: `ShowArg` MUST extend `BeforeInput` and display "(arg: N) " using style fragments `[("class:prompt.arg", "(arg: "), ("class:prompt.arg.text", "<N>"), ("class:prompt.arg", ") ")]` when the key processor has an active arg value, and return an empty fragment list when arg is null.
- **FR-018**: `AppendAutoSuggestion` MUST append the buffer's suggestion text to the last line (line at index `Document.LineCount - 1`) with configurable style (default "class:auto-suggestion") only when a suggestion exists and the cursor is at the end of the document. When either condition is not met, an empty string MUST be appended (preserving the fragment structure).
- **FR-019**: `TabsProcessor` MUST replace tab characters with column-aligned sequences using configurable tab width (default 4) and separator characters (default "|" for first character, "\u2508" for remaining), with a configurable style (default "class:tab"). Tab alignment MUST expand each tab to fill columns up to the next multiple of the tab-stop width (e.g., a tab at column 0 with width 4 fills columns 0-3; a tab at column 2 with width 4 fills columns 2-3).
- **FR-020**: `TabsProcessor` MUST provide source-to-display and display-to-source position mapping functions that correctly account for variable-width tab expansions.
- **FR-021**: `ShowLeadingWhiteSpaceProcessor` MUST replace leading space characters with a configurable visible character using a configurable style (default "class:leading-whitespace").
- **FR-022**: `ShowTrailingWhiteSpaceProcessor` MUST replace trailing space characters with a configurable visible character using a configurable style (default "class:trailing-whitespace").
- **FR-023**: `HighlightMatchingBracketProcessor` MUST highlight matching bracket pairs when the cursor is on a bracket (`Document.CurrentChar` is in the bracket set) or immediately after a closing bracket (`Document.CharBeforeCursor` is a closing bracket in the bracket set), searching within a configurable distance (default 1000 characters). When checking the character before the cursor, the processor creates a temporary document with cursor position decremented by 1 to locate the match.
- **FR-024**: `HighlightMatchingBracketProcessor` MUST support configurable bracket characters (default "[](){}<>") and apply "matching-bracket.cursor" and "matching-bracket.other" style classes.
- **FR-025**: `HighlightMatchingBracketProcessor` MUST cache highlight positions per render cycle to avoid redundant computation.
- **FR-026**: `HighlightMatchingBracketProcessor` MUST not highlight when the application is in the "done" state.
- **FR-027**: `DisplayMultipleCursors` MUST highlight multiple cursor positions on a line with "multiple-cursors" style class when Vi block insert mode is active.
- **FR-028**: `ConditionalProcessor` MUST apply its wrapped processor only when the associated filter evaluates to true; otherwise, it MUST pass fragments through unchanged.
- **FR-029**: `DynamicProcessor` MUST invoke its callable to obtain a processor at each application, falling back to `DummyProcessor` when the callable returns null.
- **FR-030**: `MergeProcessors` MUST combine multiple processors into a single processor (`MergedProcessor`) that applies each in sequence. Position mapping composition MUST use list-based function chaining: a mutable `List<Func<int, int>>` accumulates each processor's `SourceToDisplay` function, and the `SourceToDisplay` passed to each processor iterates the full list so far. After all processors execute, the initial incoming `SourceToDisplay` is removed from the list so the returned mapping reflects only the merged processor's own transformations. `DisplayToSource` functions chain in reverse order.
- **FR-031**: `MergeProcessors` MUST return a `DummyProcessor` for an empty list and the single processor directly for a single-element list.
- **FR-032**: `ReverseSearchProcessor` MUST format the reverse search display on line 0 using these styled fragments: `[("class:prompt.search", "("), ("class:prompt.search", direction_text), ("class:prompt.search", ")\x60"), ("class:prompt.search.text", query_text), ("", "': ")]` followed by the matched line content from the main buffer with incremental search highlighting applied.
- **FR-033**: `ReverseSearchProcessor` MUST display "(i-search)" for forward direction and "(reverse-i-search)" for backward direction.
- **FR-034**: All processors MUST operate on `StyleAndTextTuples` (lists of style-text fragment pairs) and preserve fragment handlers where applicable.
- **FR-035**: Fragment explosion (splitting multi-character fragments into single-character fragments) MUST be available as a shared utility for processors that need per-character manipulation.
- **FR-036**: `BufferControl.DefaultInputProcessors` MUST return, in this order: `HighlightSearchProcessor`, `HighlightIncrementalSearchProcessor`, `HighlightSelectionProcessor`, `DisplayMultipleCursors`. This list is instantiated once per `BufferControl` instance.
- **FR-037**: `ReverseSearchProcessor` MUST exclude the following processor types when filtering the main control's processor list: `HighlightSearchProcessor`, `HighlightSelectionProcessor`, `BeforeInput`, `AfterInput`. When encountering a `MergedProcessor`, it MUST recursively filter each sub-processor. When encountering a `ConditionalProcessor`, it MUST filter the inner processor and rewrap if accepted.

### Key Entities

- **IProcessor**: Interface defining the contract for fragment transformation. Each processor receives input about the current rendering context and returns transformed fragments with position mappings.
- **TransformationInput**: Data carrier providing rendering context to processors: the buffer control, document state, line number, current position mapping, fragments, viewport dimensions, and optional cross-line access.
- **Transformation**: Result of processor application containing the transformed fragments and bidirectional position mapping functions (source-to-display and display-to-source).
- **Fragment (StyleAndTextTuple)**: A tuple of style string and text content, optionally with a mouse handler. Fragments are the atomic display units that processors manipulate.
- **Position Mapping Functions**: Bidirectional `Func<int, int>` functions that translate cursor column indices between coordinate spaces. *Source-to-display* maps a zero-based column index in the original document line text to the corresponding column index in the displayed text after transformations (e.g., tab expansion shifts subsequent columns right). *Display-to-source* performs the reverse mapping. Identity functions (`i => i`) are used when a processor does not alter content length.
- **Merged Processor**: An internal processor type that wraps multiple processors and applies them sequentially, composing their position mappings into a single coherent mapping chain.
- **Style Class Naming Convention**: Processors use two distinct patterns for style strings. *ClassName properties* (e.g., `ClassName = "search"`) store the class name without prefix; these are prepended with `" class:"` when appended to fragment style strings (e.g., `style + " class:search "`). *Style properties* (e.g., `Style = "class:auto-suggestion"`) store the complete style string used directly as a fragment style.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 20 processor types (IProcessor, TransformationInput, Transformation, DummyProcessor, HighlightSearchProcessor, HighlightIncrementalSearchProcessor, HighlightSelectionProcessor, PasswordProcessor, HighlightMatchingBracketProcessor, DisplayMultipleCursors, BeforeInput, AfterInput, AppendAutoSuggestion, ShowArg, ConditionalProcessor, DynamicProcessor, ShowLeadingWhiteSpaceProcessor, ShowTrailingWhiteSpaceProcessor, TabsProcessor, ReverseSearchProcessor, plus ProcessorUtils) are implemented with 100% API fidelity to Python Prompt Toolkit.
- **SC-002**: Position mapping accuracy is verified for all processors that modify content length (`BeforeInput`, `AfterInput`, `TabsProcessor`, `ReverseSearchProcessor`, and `MergedProcessor` chaining): chaining two or more processors that each shift positions produces the correct composed bidirectional mapping for both source-to-display and display-to-source directions.
- **SC-003**: Unit test coverage reaches 80% or higher across all processor implementations, including: all concrete processor types, `ExplodedList`, `ExplodeTextFragments`, `ProcessorUtils.MergeProcessors`, and `MergedProcessor`. Prerequisite changes to `BufferControl`, `Layout`, and `AppFilters` are tested in their respective existing test suites.
- **SC-004**: Fragment explosion correctly splits multi-character fragments into single-character fragments, preserving styles and mouse handlers, for all processors that require per-character manipulation (`HighlightSearchProcessor`, `HighlightIncrementalSearchProcessor`, `HighlightSelectionProcessor`, `HighlightMatchingBracketProcessor`, `DisplayMultipleCursors`).
- **SC-005**: `MergeProcessors` correctly composes an arbitrary number of processors with accurate bidirectional position mappings.
- **SC-006**: All conditional and dynamic processors correctly delegate to their wrapped/returned processors or fall back to identity behavior.

### Assumptions (Verified)

The following assumptions have been verified against the codebase:

- **VERIFIED**: `StyleAndTextTuple` (singular) is a `readonly record struct` in `Stroke.FormattedText`. The Python `StyleAndTextTuples` list alias does not exist as a named type; C# code uses `IReadOnlyList<StyleAndTextTuple>` or `List<StyleAndTextTuple>` directly. The `FormattedText` class implements `IReadOnlyList<StyleAndTextTuple>`.
- **VERIFIED**: `Document` class exposes all required members: `SelectionRangeAtLine`, `FindMatchingBracketPosition`, `CursorPositionRow`, `CursorPositionCol`, `CurrentChar`, `CharBeforeCursor`, `IsCursorAtTheEnd`, `LineCount`, `Lines`, `TranslateIndexToPosition`, `TranslateRowColToIndex`.
- **VERIFIED**: `IFilter` interface, `FilterOrBool` struct, and `FilterUtils.ToFilter` utility exist in `Stroke.Filters`.
- **VERIFIED**: `AnyFormattedText` struct with implicit conversions, `FormattedTextUtils.ToFormattedText`, `FragmentListLen`, and `FragmentListToText` all exist in `Stroke.FormattedText`.
- **VERIFIED**: `SimpleCache<TKey, TValue>` class exists in `Stroke.Core` with thread-safe `Lock` and configurable `maxSize` (default 8).
- **VERIFIED**: `Application<TResult>` class exists with `IsDone`, `RenderCounter`, `KeyProcessor`, and `Layout` properties. `KeyProcessor` has `Arg` property. `SearchState` has `Text`, `Direction`, and `IgnoreCase()`. `SearchDirection` enum exists. `Buffer` has `Suggestion` and `MultipleCursorPositions` properties.
- **VERIFIED**: `BufferControl` exposes `Buffer` and `Lexer` properties. `SearchBufferControl` extends `BufferControl` with `SearcherSearchState` and `IgnoreCase` properties.
- **VERIFIED**: `Layout` class has `SearchLinks` dictionary (`Dictionary<SearchBufferControl, BufferControl>`) for linking search controls to their target buffer controls.

### Assumptions (Incorrect - Require Work in This Feature)

The following assumptions were found to be **incorrect**. These items do not exist in the codebase and must be created as part of this feature:

- **MISSING: `ExplodeTextFragments` utility** - Python Prompt Toolkit defines this in `layout/utils.py`. It splits multi-character fragments into single-character fragments. This function does not exist anywhere in Stroke and must be implemented (likely in `Stroke.Layout` namespace as a utility, mirroring Python's `layout.utils.explode_text_fragments`). → Contract: [utility-types.md](contracts/utility-types.md), FR-035.
- **MISSING: `BufferControl.SearchState` property** - Python's `BufferControl` exposes `search_state` for accessing the current search state. Stroke's `BufferControl` does not have this property. Processors like `HighlightSearchProcessor` need it to get search text. Must be added to `BufferControl`. → Contract: [prerequisite-changes.md](contracts/prerequisite-changes.md), FR-006.
- **MISSING: `BufferControl.SearchBufferControl` property** - Python's `BufferControl` exposes `search_buffer_control` to access the linked `SearchBufferControl`. Stroke's `BufferControl` does not have this. `ReverseSearchProcessor` needs it. Must be added to `BufferControl`. → Contract: [prerequisite-changes.md](contracts/prerequisite-changes.md), FR-032.
- **MISSING: `BufferControl.SearchBuffer` property** - Python's `BufferControl` exposes `search_buffer` (the Buffer from the linked SearchBufferControl). `HighlightIncrementalSearchProcessor` needs it to read the search buffer text. Must be added to `BufferControl`. → Contract: [prerequisite-changes.md](contracts/prerequisite-changes.md), FR-009.
- **MISSING: `BufferControl.InputProcessors` property** - Python's `BufferControl` stores `input_processors` as a list. Stroke's `BufferControl` does not expose this. `ReverseSearchProcessor` needs it to filter processors from the main control. Must be added to `BufferControl`. → Contract: [prerequisite-changes.md](contracts/prerequisite-changes.md), FR-036, FR-037.
- **MISSING: `Layout.SearchTargetBufferControl` property** - Python's `Layout` exposes `search_target_buffer_control` which returns the `BufferControl` that is the target of the current search. `ReverseSearchProcessor` uses it to find the main buffer. Stroke's `Layout` class has `SearchLinks` (dictionary) and `CurrentSearchBufferControl` but not a direct `SearchTargetBufferControl` property. Must be added. → Contract: [prerequisite-changes.md](contracts/prerequisite-changes.md), FR-032.
- **MISSING: `ViInsertMultipleMode` filter** - Python has a `vi_insert_multiple_mode` filter function. No equivalent exists in Stroke. `DisplayMultipleCursors` needs it. Must be implemented as a built-in filter in `Stroke.Filters`. → Contract: [prerequisite-changes.md](contracts/prerequisite-changes.md), FR-027.
- **MISSING: `BufferControl.CreateContent` overload with `preview_search` parameter** - Python's `BufferControl.create_content` accepts a `preview_search` parameter. Stroke's `BufferControl.CreateContent(int width, int height)` does not have this parameter. `ReverseSearchProcessor` needs it. Must be added as an overload or optional parameter. → Contract: [prerequisite-changes.md](contracts/prerequisite-changes.md), FR-032.
