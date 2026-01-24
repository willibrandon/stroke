# Feature Specification: Immutable Document Text Model

**Feature Branch**: `002-immutable-document`
**Created**: 2026-01-23
**Status**: Draft
**Input**: User description: "Implement the immutable Document class that represents text with cursor position and selection state. This is the core text representation used throughout Stroke."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Query Text Around Cursor (Priority: P1)

A developer building a text editor needs to access text before and after the cursor position to implement features like auto-completion, syntax highlighting, and command parsing.

**Why this priority**: This is the most fundamental capability - without cursor-relative text access, no text editing features can be built.

**Independent Test**: Can be fully tested by creating a Document with sample text and cursor position, then verifying all text access properties return correct substrings.

**Acceptance Scenarios**:

1. **Given** a Document with text "hello world" and cursor at position 5, **When** accessing TextBeforeCursor, **Then** returns "hello"
2. **Given** a Document with text "hello world" and cursor at position 5, **When** accessing TextAfterCursor, **Then** returns " world"
3. **Given** a Document with text "line1\nline2\nline3" and cursor in the middle of line2, **When** accessing CurrentLine, **Then** returns "line2"
4. **Given** a Document with text "  indented" and cursor anywhere on the line, **When** accessing LeadingWhitespaceInCurrentLine, **Then** returns "  "

---

### User Story 2 - Navigate by Words (Priority: P1)

A developer implementing Vi/Emacs keybindings needs to find word boundaries to support w, b, e, W, B, E motions (Vi) and M-f, M-b (Emacs) word navigation commands.

**Why this priority**: Word navigation is essential for efficient text editing and is used constantly in both Vi and Emacs modes.

**Independent Test**: Can be tested by creating Documents with various text patterns and verifying word boundary methods return correct relative positions.

**Acceptance Scenarios**:

1. **Given** a Document with text "hello world" and cursor at start, **When** calling FindNextWordBeginning, **Then** returns position of 'w' relative to cursor
2. **Given** a Document with text "hello world" and cursor at 'w', **When** calling FindPreviousWordBeginning, **Then** returns negative offset to 'h'
3. **Given** a Document with text "hello-world test" and cursor at start with WORD=false, **When** calling FindNextWordBeginning, **Then** returns position of '-' (punctuation is separate word)
4. **Given** a Document with text "hello-world test" and cursor at start with WORD=true, **When** calling FindNextWordBeginning, **Then** returns position of 't' in "test" (WORD skips all non-whitespace)
5. **Given** a Document with text "word  " and cursor in word, **When** calling GetWordBeforeCursor, **Then** returns the partial word before cursor

---

### User Story 3 - Navigate by Lines (Priority: P1)

A developer implementing arrow key navigation needs to move the cursor up and down while maintaining column position where possible.

**Why this priority**: Line navigation is fundamental to multi-line text editing and required for basic cursor movement.

**Independent Test**: Can be tested by creating multi-line Documents and verifying cursor movement methods return correct relative positions.

**Acceptance Scenarios**:

1. **Given** a Document with text "short\nlonger line\nshort" and cursor at column 10 of line 2, **When** calling GetCursorUpPosition, **Then** returns position that moves to end of line 1 (column 5)
2. **Given** a Document with text "longer line\nshort" and cursor at column 10 of line 1, **When** calling GetCursorDownPosition with preferredColumn=10, **Then** returns position at end of line 2
3. **Given** a Document with cursor on first line, **When** accessing OnFirstLine, **Then** returns true
4. **Given** a Document with cursor on last line, **When** accessing OnLastLine, **Then** returns true

---

### User Story 4 - Search Within Document (Priority: P2)

A developer implementing incremental search (Ctrl+S in Emacs, / in Vi) needs to find text matches forward and backward from the cursor position.

**Why this priority**: Search is essential for efficient navigation but builds upon basic cursor position functionality.

**Independent Test**: Can be tested by creating Documents with known text patterns and verifying search methods return correct match positions.

**Acceptance Scenarios**:

1. **Given** a Document with text "foo bar foo baz", **When** calling Find("foo"), **Then** returns position of second "foo" relative to cursor at start
2. **Given** a Document with text "FOO bar foo", **When** calling Find("foo", ignoreCase=true), **Then** returns position of "FOO"
3. **Given** a Document with text "foo bar foo" and cursor at end, **When** calling FindBackwards("foo"), **Then** returns negative offset to last "foo"
4. **Given** a Document with text "line1\nfoo\nline3" and cursor at start of line 2, **When** calling Find("line", inCurrentLine=true), **Then** returns null (not in current line)
5. **Given** a Document with text "aaa", **When** calling FindAll("a"), **Then** returns list [0, 1, 2]

---

### User Story 5 - Handle Selection Ranges (Priority: P2)

A developer implementing visual selection (Vi visual mode, Shift+arrow keys) needs to track selection state and extract selected text for cut/copy operations.

**Why this priority**: Selection handling enables clipboard operations which are core to text editing, but depends on basic cursor functionality.

**Independent Test**: Can be tested by creating Documents with SelectionState and verifying selection range methods return correct boundaries.

**Acceptance Scenarios**:

1. **Given** a Document with selection from position 5 to cursor at position 10, **When** calling SelectionRange, **Then** returns (5, 10)
2. **Given** a Document with LINES selection spanning lines 2-4, **When** calling SelectionRanges, **Then** returns range covering complete lines including line starts
3. **Given** a Document with BLOCK selection, **When** calling SelectionRanges, **Then** yields multiple ranges, one per line in the block
4. **Given** a Document with selection, **When** calling CutSelection, **Then** returns new Document without selected text and ClipboardData containing cut text

---

### User Story 6 - Match Brackets (Priority: P2)

A developer implementing bracket matching (% in Vi, C-M-f/C-M-b in Emacs) needs to find matching pairs of brackets for navigation and validation.

**Why this priority**: Bracket matching is important for code editing but is a specialized navigation feature.

**Independent Test**: Can be tested by creating Documents with nested brackets and verifying bracket matching methods return correct positions.

**Acceptance Scenarios**:

1. **Given** a Document with text "(foo (bar) baz)" and cursor on opening '(', **When** calling FindMatchingBracketPosition, **Then** returns position of closing ')'
2. **Given** a Document with text "[nested {brackets}]" and cursor on '{', **When** calling FindMatchingBracketPosition, **Then** returns position of matching '}'
3. **Given** a Document with text "(unmatched" and cursor on '(', **When** calling FindEnclosingBracketRight, **Then** returns null
4. **Given** a Document with text "code)" and cursor before ')', **When** calling FindEnclosingBracketLeft, **Then** returns null (no opening bracket)

---

### User Story 7 - Paste Clipboard Data (Priority: P2)

A developer implementing paste operations needs to insert clipboard content respecting the selection type (characters, lines, or block) and paste mode (before/after cursor, Emacs style).

**Why this priority**: Paste operations complete the clipboard workflow but depend on selection functionality.

**Independent Test**: Can be tested by creating Documents and ClipboardData with various types, then verifying paste produces correct new Documents.

**Acceptance Scenarios**:

1. **Given** a Document and ClipboardData with CHARACTERS type, **When** calling PasteClipboardData with PasteMode.Emacs, **Then** inserts text at cursor position
2. **Given** a Document and ClipboardData with LINES type, **When** calling PasteClipboardData with PasteMode.ViBefore, **Then** inserts complete line above current line
3. **Given** a Document and ClipboardData with BLOCK type, **When** calling PasteClipboardData, **Then** inserts text as rectangular block starting at cursor column
4. **Given** a Document and ClipboardData, **When** calling PasteClipboardData with count=3, **Then** pastes the text 3 times

---

### User Story 8 - Navigate by Paragraphs (Priority: P3)

A developer implementing paragraph navigation ({ and } in Vi) needs to find paragraph boundaries defined by empty lines.

**Why this priority**: Paragraph navigation is useful but less frequently used than word and line navigation.

**Independent Test**: Can be tested by creating Documents with multiple paragraphs separated by empty lines and verifying paragraph methods return correct positions.

**Acceptance Scenarios**:

1. **Given** a Document with text "para1\n\npara2\n\npara3" and cursor in para2, **When** calling StartOfParagraph, **Then** returns position at start of para2
2. **Given** a Document with text "para1\n\npara2" and cursor in para1, **When** calling EndOfParagraph, **Then** returns position at end of para1 before empty line
3. **Given** a Document with trailing empty lines, **When** calling EmptyLineCountAtTheEnd, **Then** returns correct count of empty lines

---

### Edge Cases

#### Cursor Position Edge Cases
- **EC-001**: Cursor at end of text (position == text.Length): Document allows this as valid insertion point. CurrentChar returns empty, IsCursorAtTheEnd returns true.
- **EC-002**: Cursor at position 0: CharBeforeCursor returns empty. OnFirstLine returns true if no newlines before cursor.
- **EC-003**: Cursor at newline character: CurrentLine returns the line before the newline. IsCursorAtTheEndOfLine returns true.

#### Empty/Whitespace Document Edge Cases
- **EC-004**: Empty Document (text == ""): Cursor at position 0 is valid. Lines returns single empty string. LineCount returns 1.
- **EC-005**: Whitespace-only Document: Word operations return null/empty when no words exist. Lines preserves whitespace structure.
- **EC-006**: Document with only newlines: Each newline creates a new line. LineCount equals newline count + 1.

#### Search Edge Cases
- **EC-007**: Empty search pattern: Returns position 0 for Find, empty list for FindAll (match Python behavior).
- **EC-008**: Search pattern not found: Returns null (not -1 or exception).
- **EC-009**: Search at end of document: Find returns null, FindBackwards searches backward from cursor.

#### Navigation Edge Cases
- **EC-010**: Negative count parameters: Word navigation reverses direction. GetCursorLeftPosition with negative count moves right.
- **EC-011**: Count exceeds available positions: Returns maximum available movement (clamp to document boundaries).
- **EC-012**: Preferred column exceeds line length: Vertical movement positions cursor at end of shorter line.

#### Bracket Edge Cases
- **EC-013**: Unmatched bracket: FindMatchingBracketPosition returns null.
- **EC-014**: No enclosing bracket: FindEnclosingBracketLeft/Right returns null.
- **EC-015**: Nested brackets of same type: Matching respects nesting depth.
- **EC-016**: Mixed bracket types: Each type matched independently (e.g., `([)]` finds `)` for `(`, not `]`).

#### Selection Edge Cases
- **EC-017**: No selection (Selection == null): SelectionRange throws or returns (cursor, cursor) based on method.
- **EC-018**: Selection with cursor before origin: Range normalizes to (cursor, origin).
- **EC-019**: Block selection on lines of different lengths: Each line's range respects line boundaries.

#### Clipboard Edge Cases
- **EC-020**: Cut with no selection: Returns unchanged Document and empty ClipboardData.
- **EC-021**: Paste with count == 0: Returns unchanged Document.
- **EC-022**: Paste LINES type into document: Inserts complete lines, adds newlines as needed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Document MUST be immutable - all mutation operations return new Document instances
- **FR-002**: Document MUST store text content, cursor position, and optional selection state
- **FR-003**: Document MUST validate that cursor position is within valid range (0 to text length inclusive)
- **FR-004**: Document MUST default cursor position to end of text when not specified
- **FR-005**: Document MUST provide character access at and before cursor position
- **FR-006**: Document MUST provide text sections (before cursor, after cursor, current line portions)
- **FR-007**: Document MUST provide line-based access (all lines, lines from current, line count)
- **FR-008**: Document MUST provide cursor position as both absolute index and row/column coordinates
- **FR-009**: Document MUST translate between absolute index and row/column positions
- **FR-010**: Document MUST support forward and backward text search with case-insensitive option
- **FR-011**: Document MUST support search constrained to current line only
- **FR-012**: Document MUST support finding nth occurrence of search pattern
- **FR-013**: Document MUST support word navigation distinguishing between "word" (alphanumeric) and "WORD" (non-whitespace)
- **FR-014**: Document MUST support custom regex patterns for word boundary detection
- **FR-015**: Document MUST provide word under cursor and word before cursor extraction
- **FR-016**: Document MUST support cursor movement left, right, up, and down with count parameter
- **FR-017**: Document MUST support preferred column retention during vertical movement
- **FR-018**: Document MUST support bracket matching for (), [], {}, and <> pairs
- **FR-019**: Document MUST support finding enclosing brackets in both directions
- **FR-020**: Document MUST provide document navigation (start, end, start/end of line)
- **FR-021**: Document MUST support selection range calculation for CHARACTERS, LINES, and BLOCK types
- **FR-022**: Document MUST support cutting selection and returning clipboard data
- **FR-023**: Document MUST support pasting clipboard data with EMACS, VI_BEFORE, and VI_AFTER modes
- **FR-024**: Document MUST support paragraph navigation based on empty line boundaries
- **FR-025**: Document MUST support inserting text before or after document content
- **FR-026**: Document MUST implement value equality based on text, cursor position, and selection
- **FR-027**: Document MUST share cached line data between instances with identical text (flyweight pattern)
- **FR-028**: Document MUST lazily compute line arrays and line start indexes
- **FR-029**: Document MUST implement GetHashCode consistent with Equals for dictionary/set usage
- **FR-030**: Document MUST provide HasMatchAtCurrentPosition method to check if substring exists at cursor
- **FR-031**: Document MUST provide LinesFromCurrent property returning lines from current line to end
- **FR-032**: Document MUST provide GetColumnCursorPosition method to move cursor to specific column
- **FR-033**: CurrentChar and CharBeforeCursor MUST return '\0' when position is invalid
- **FR-034**: Document MUST provide FindStartOfPreviousWord method for backward word navigation
- **FR-035**: Document MUST provide FindBoundariesOfCurrentWord method returning (start, end) offsets
- **FR-036**: All word navigation methods MUST support optional custom regex pattern parameter

### Key Entities

- **Document**: Immutable representation of text with cursor position and optional selection state. Core data structure for all text operations. Sealed class that cannot be inherited.
- **SelectionState**: Tracks selection origin point and type (CHARACTERS, LINES, BLOCK). Contains OriginalCursorPosition (int), Type (SelectionType), and ShiftMode (bool). ShiftMode is mutable via EnterShiftMode() to match Python Prompt Toolkit behavior exactly.
- **SelectionType**: Enumeration with values Characters, Lines, Block corresponding to Vi visual modes (v, V, Ctrl+v).
- **PasteMode**: Enumeration with values Emacs, ViBefore, ViAfter determining paste insertion point.
- **ClipboardData**: Contains copied/cut text (string) and its selection type (SelectionType) for paste operations. Produced by CutSelection, consumed by PasteClipboardData.
- **DocumentCache**: Internal shared cache for line arrays computed from text. Contains nullable Lines (immutable string array) and LineIndexes (int array). Enables flyweight pattern for memory efficiency.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 50+ Document methods match Python Prompt Toolkit behavior for identical inputs
- **SC-002**: Creating 1000 Document instances with identical text reuses a single cached line array
- **SC-003**: Line arrays and indexes are computed lazily - accessing only cursor position does not trigger line parsing
- **SC-004**: All word operations correctly distinguish between "word" and "WORD" modes per Vi conventions
- **SC-005**: Selection operations correctly handle all three selection types (CHARACTERS, LINES, BLOCK)
- **SC-006**: Unit tests achieve 80% code coverage of Document class
- **SC-007**: Document equality correctly identifies identical and different instances
- **SC-008**: All search operations support case-insensitive matching
- **SC-009**: Bracket matching handles nested brackets of the same and different types
- **SC-010**: Paste operations correctly handle all three selection types and three paste modes

## Assumptions

- SelectionState, SelectionType, and PasteMode types are defined in Stroke.Core namespace
- ClipboardData type is defined in Stroke.Core namespace
- The vi_mode() filter function for selection boundary behavior will be addressed in the Filters module
- Word regex patterns follow Python Prompt Toolkit definitions exactly for compatibility

## Implementation Constraints

### Immutability Requirements

- **IC-001**: Document class MUST be sealed to prevent inheritance that could violate immutability
- **IC-002**: Document MUST use private backing fields (`_text`, `_cursorPosition`, `_selection`, `_cache`) with read-only public properties
- **IC-003**: Lines property MUST return an immutable collection type to prevent external mutation
- **IC-004**: DocumentCache line arrays MUST be immutable once computed

### Caching Requirements

- **IC-005**: Document instances with identical text MUST share a single DocumentCache instance (flyweight pattern)
- **IC-006**: Cache sharing MUST use weak references to allow garbage collection when no Documents reference the cache
- **IC-007**: Line array computation MUST use cumulative sum algorithm for O(n) initialization
- **IC-008**: Line index lookup MUST use binary search for O(log n) row/column translation

### Word Navigation Patterns

- **IC-009**: "word" pattern MUST match alphanumeric sequences OR punctuation sequences (excluding whitespace): `([a-zA-Z0-9_]+|[^a-zA-Z0-9_\s]+)`
- **IC-010**: "WORD" pattern MUST match any non-whitespace sequence: `([^\s]+)`
- **IC-011**: Word patterns with trailing whitespace MUST include optional whitespace suffix
- **IC-012**: All six word regex patterns from Python Prompt Toolkit MUST be ported exactly

### Architecture Constraints

- **IC-013**: Document and all dependency types MUST reside in Stroke.Core namespace (layer 1)
- **IC-014**: Document MUST NOT depend on any types outside Stroke.Core
- **IC-015**: All public APIs MUST match Python Prompt Toolkit signatures (adjusted for C# naming conventions)

### Error Handling

- **IC-016**: Invalid cursor position (negative or > text.Length) MUST throw ArgumentOutOfRangeException
- **IC-017**: Null text parameter MUST be treated as empty string (match Python behavior)
- **IC-018**: Methods that can fail to find a match MUST return null (not throw exceptions)

## Traceability

### API Mapping Reference

All Document APIs are mapped from Python Prompt Toolkit in `docs/api-mapping.md` lines 591-725:
- 20 properties mapped (Text through EmptyLineCountAtTheEnd)
- 30+ methods mapped (GetWordBeforeCursor through EndOfParagraph)
- Naming convention: `snake_case` → `PascalCase`

### Test Mapping Reference

Document tests are mapped from Python Prompt Toolkit in `docs/test-mapping.md`:
- 12 core test methods in DocumentTests class
- Test naming convention: `test_foo_bar` → `FooBar`
- Additional tests required for selection, paste, word navigation, brackets, paragraphs

### Constitution Compliance

| Principle | Compliance |
|-----------|------------|
| I. Faithful Port | All 50+ APIs match Python Prompt Toolkit exactly |
| II. Immutability | Document sealed, immutable with flyweight caching |
| III. Layered Architecture | Stroke.Core only, zero external dependencies |
| VI. Performance | Lazy evaluation, flyweight pattern, O(log n) lookups |
| VIII. Real-World Testing | xUnit tests, no mocks, 80% coverage target |
| IX. Planning Documents | Follows api-mapping.md and test-mapping.md |
