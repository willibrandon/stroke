# Feature Specification: Buffer (Mutable Text Container)

**Feature Branch**: `007-mutable-buffer`
**Created**: 2026-01-24
**Status**: Draft
**Input**: User description: "Implement the mutable Buffer class that wraps an immutable Document and provides text editing operations, undo/redo, completion state, and history management."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Text Editing (Priority: P1)

As a terminal application developer, I need a mutable text container that wraps an immutable Document so that I can provide interactive text editing capabilities to users while maintaining document integrity through immutability at the core.

**Why this priority**: This is the foundation of all interactive text editing. Without basic text insertion, deletion, and cursor movement, no other editing features can function.

**Independent Test**: Can be fully tested by creating a Buffer, inserting text at various positions, deleting text, and verifying the resulting Document state accurately reflects all changes.

**Acceptance Scenarios**:

1. **Given** an empty Buffer, **When** the user inserts "Hello World", **Then** the Buffer's text property equals "Hello World" and cursor position is at the end of the inserted text.
2. **Given** a Buffer with text "Hello World" and cursor at position 5, **When** the user inserts " Beautiful", **Then** the text becomes "Hello Beautiful World" with cursor at position 15.
3. **Given** a Buffer with text "Hello World" and cursor at position 11, **When** the user deletes 5 characters before cursor, **Then** the text becomes "Hello " and cursor is at position 6.
4. **Given** a Buffer with text "Hello World" and cursor at position 6, **When** the user deletes 5 characters forward, **Then** the text becomes "Hello " with cursor remaining at position 6.

---

### User Story 2 - Undo/Redo Operations (Priority: P1)

As a terminal application user, I need the ability to undo and redo my text changes so that I can recover from mistakes and experiment with different edits without fear of losing work.

**Why this priority**: Undo/redo is essential for any usable text editor. Users expect this functionality and it directly impacts their confidence in making changes.

**Independent Test**: Can be fully tested by performing a sequence of edits, calling undo multiple times to verify state restoration, and calling redo to re-apply changes.

**Acceptance Scenarios**:

1. **Given** a Buffer where the user typed "Hello", then typed " World", **When** the user performs undo, **Then** the text reverts to "Hello" with appropriate cursor position.
2. **Given** a Buffer that was just undone from "Hello World" to "Hello", **When** the user performs redo, **Then** the text returns to "Hello World".
3. **Given** a Buffer with undo history containing multiple states, **When** the user performs multiple undo operations, **Then** each undo restores the previous state in reverse chronological order.
4. **Given** a Buffer where undo was performed, **When** the user types new text, **Then** the redo stack is cleared and the new edit starts a fresh undo chain.

---

### User Story 3 - Cursor Navigation (Priority: P1)

As a terminal application user, I need to navigate through text using cursor movement operations so that I can position the cursor where I want to make edits.

**Why this priority**: Cursor navigation is fundamental to text editing. Users must be able to move to different positions in the text to perform insertions, deletions, and selections.

**Independent Test**: Can be fully tested by creating a Buffer with multiline text and verifying cursor position after each navigation operation.

**Acceptance Scenarios**:

1. **Given** a Buffer with text "Hello World" and cursor at position 6, **When** cursor moves left 3 times, **Then** cursor position is 3.
2. **Given** a Buffer with text "Hello World" and cursor at position 0, **When** cursor moves right 5 times, **Then** cursor position is 5.
3. **Given** a Buffer with multiline text "Line1\nLine2\nLine3" and cursor on Line2, **When** cursor moves up, **Then** cursor moves to Line1 at the corresponding column.
4. **Given** a Buffer with multiline text and cursor on the first line, **When** cursor moves up, **Then** cursor remains on the first line (boundary behavior).

---

### User Story 4 - History Navigation (Priority: P2)

As a terminal application user, I need to navigate through my command history so that I can recall and reuse previous inputs without retyping them.

**Why this priority**: History navigation significantly improves productivity for REPL-style applications. Users frequently need to recall previous commands.

**Independent Test**: Can be fully tested by populating a Buffer with history entries and navigating backward/forward through them.

**Acceptance Scenarios**:

1. **Given** a Buffer with history entries ["cmd1", "cmd2", "cmd3"], **When** the user navigates backward through history, **Then** they see "cmd3", then "cmd2", then "cmd1" in sequence.
2. **Given** a Buffer currently showing a history entry, **When** the user navigates forward, **Then** they return toward more recent entries.
3. **Given** a Buffer with EnableHistorySearch enabled and partial text "cm", **When** navigating history, **Then** only entries starting with "cm" are shown.
4. **Given** a Buffer showing the current (newest) entry, **When** the user navigates forward, **Then** the Buffer remains at the current entry.

---

### User Story 5 - Selection Operations (Priority: P2)

As a terminal application user, I need to select portions of text so that I can copy, cut, or transform selected regions.

**Why this priority**: Selection is essential for clipboard operations and text transformations. It enables efficient editing of existing text.

**Independent Test**: Can be fully tested by starting a selection, moving the cursor to expand the selection, and then copying or cutting the selected text.

**Acceptance Scenarios**:

1. **Given** a Buffer with text "Hello World", **When** the user starts a character selection at position 0 and moves cursor to position 5, **Then** "Hello" is selected.
2. **Given** a Buffer with selected text, **When** the user copies the selection, **Then** a ClipboardData object is returned containing the selected text.
3. **Given** a Buffer with selected text "Hello", **When** the user cuts the selection, **Then** the selected text is removed from the Buffer and returned as ClipboardData.
4. **Given** a Buffer with an active selection, **When** the user exits selection mode, **Then** the selection state is cleared and no text is selected.

---

### User Story 6 - Clipboard Integration (Priority: P2)

As a terminal application user, I need to paste clipboard content into the Buffer so that I can efficiently transfer text from other sources or from previous cut/copy operations.

**Why this priority**: Clipboard operations complete the editing workflow. Users need to paste previously copied or cut content.

**Independent Test**: Can be fully tested by creating ClipboardData with various content types and pasting into a Buffer at different positions.

**Acceptance Scenarios**:

1. **Given** a Buffer with text "Hello World" and ClipboardData containing "Beautiful ", **When** pasting at position 6 in Emacs mode, **Then** text becomes "Hello Beautiful World".
2. **Given** a Buffer with text "Hello" and ClipboardData containing a full line, **When** pasting in Vi-after mode, **Then** the line is inserted after the current line.
3. **Given** a Buffer with text "Hello" and ClipboardData containing a full line, **When** pasting in Vi-before mode, **Then** the line is inserted before the current line.
4. **Given** ClipboardData and a count of 3, **When** pasting, **Then** the clipboard content is inserted 3 times.

---

### User Story 7 - Completion State Management (Priority: P2)

As a terminal application developer, I need the Buffer to manage completion state so that I can provide autocompletion features that help users enter text more efficiently.

**Why this priority**: Autocompletion is a key productivity feature that users expect in modern terminal applications.

**Independent Test**: Can be fully tested by starting completion with a list of completions, navigating through them, selecting one, and verifying the Buffer text updates correctly.

**Acceptance Scenarios**:

1. **Given** a Buffer with text "hel" and completions ["hello", "help", "helmet"], **When** completion state is initialized, **Then** the original document is preserved and completions are available for navigation.
2. **Given** a Buffer with active completion state, **When** the user navigates to the next completion, **Then** the selected completion changes and the Buffer text updates to reflect the selection.
3. **Given** a Buffer with active completion state, **When** the user cancels completion, **Then** the Buffer reverts to the original text before completion started.
4. **Given** a Buffer with a selected completion, **When** the user applies the completion, **Then** the completion text replaces the appropriate portion and completion state is cleared.

---

### User Story 8 - Text Transformation (Priority: P3)

As a terminal application user, I need to apply transformations to lines or regions of text so that I can efficiently modify existing content (e.g., uppercase, indent, dedent).

**Why this priority**: Text transformation enhances editing capabilities but is less frequently used than basic editing operations.

**Independent Test**: Can be fully tested by applying transformation functions to specific lines or regions and verifying the output.

**Acceptance Scenarios**:

1. **Given** a Buffer with multiline text, **When** transforming lines 0-2 with an uppercase function, **Then** only those lines are uppercased while other lines remain unchanged.
2. **Given** a Buffer with text on the current line, **When** applying transform_current_line with a reverse function, **Then** only the current line is reversed.
3. **Given** a Buffer with text "hello world", **When** transforming the region from position 0 to 5 with uppercase, **Then** the result is "HELLO world".

---

### User Story 9 - Read-Only Mode (Priority: P3)

As a terminal application developer, I need to mark a Buffer as read-only so that I can prevent accidental modifications to certain content.

**Why this priority**: Read-only mode is important for displaying content that should not be modified, but it's a specialized use case.

**Independent Test**: Can be fully tested by setting a Buffer to read-only and verifying that edit operations throw an exception.

**Acceptance Scenarios**:

1. **Given** a Buffer in read-only mode, **When** the user attempts to insert text, **Then** an EditReadOnlyBufferException is thrown.
2. **Given** a Buffer in read-only mode, **When** the user attempts to delete text, **Then** an EditReadOnlyBufferException is thrown.
3. **Given** a Buffer in read-only mode with bypass_readonly option, **When** setting a document with bypass_readonly=true, **Then** the document is set successfully without exception.

---

### User Story 10 - Validation (Priority: P3)

As a terminal application developer, I need the Buffer to support validation so that I can provide feedback to users when their input is invalid.

**Why this priority**: Validation improves user experience by catching errors before submission, but it requires a validator implementation.

**Independent Test**: Can be fully tested by setting a validator on the Buffer and verifying validation results for valid and invalid inputs.

**Acceptance Scenarios**:

1. **Given** a Buffer with a validator that rejects empty input, **When** validating an empty Buffer, **Then** validation fails and ValidationState is Invalid.
2. **Given** a Buffer with valid content according to its validator, **When** validating, **Then** validation succeeds and ValidationState is Valid.
3. **Given** a Buffer with validate_while_typing enabled, **When** text changes, **Then** validation runs asynchronously and updates validation state.

---

### User Story 11 - Auto-Suggest Integration (Priority: P3)

As a terminal application developer, I need the Buffer to integrate with auto-suggest providers so that I can show suggestions to users as they type.

**Why this priority**: Auto-suggest enhances user experience but requires an auto-suggest implementation to be useful.

**Independent Test**: Can be fully tested by setting an auto-suggest provider and verifying suggestions appear after text insertion.

**Acceptance Scenarios**:

1. **Given** a Buffer with an auto-suggest provider, **When** the user types text, **Then** a suggestion is generated and stored in the Buffer's Suggestion property.
2. **Given** a Buffer with a current suggestion, **When** the text changes, **Then** the previous suggestion is cleared and a new one is generated.

---

### User Story 12 - External Editor (Priority: P3)

As a terminal application user, I need to open the current Buffer content in an external editor so that I can use a full-featured editor for complex edits.

**Why this priority**: External editor support is valuable for power users but is an advanced feature.

**Independent Test**: Can be fully tested by verifying the async task is created and file operations are performed correctly.

**Acceptance Scenarios**:

1. **Given** a Buffer with content, **When** open_in_editor is called, **Then** the content is written to a temporary file and the editor is invoked.
2. **Given** a read-only Buffer, **When** open_in_editor is called, **Then** an EditReadOnlyBufferException is thrown.

---

### Edge Cases

- What happens when cursor position exceeds text length? The cursor is clamped to the text length.
- How does the system handle empty text with cursor operations? Cursor remains at position 0.
- What happens when undo is called with empty undo stack? Nothing happens (no-op).
- What happens when navigating history with empty history? Working index remains unchanged.
- How are concurrent async operations (completion, validation, suggestion) handled? Each operation type allows only one concurrent execution; new requests are ignored until the current one completes, and operations retry if document changes during execution.
- How does case-insensitive search work? SearchState has an IgnoreCaseFilter property (Func<bool>) that determines whether search comparisons ignore case. When enabled, pattern matching uses case-insensitive comparison.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Buffer MUST wrap an immutable Document and expose text and cursor_position properties that update the underlying Document.
- **FR-002**: Buffer MUST support text insertion at the current cursor position with optional overwrite mode.
- **FR-003**: Buffer MUST support deletion of characters before and after the cursor.
- **FR-004**: Buffer MUST maintain an undo stack that captures text and cursor position after each significant edit.
- **FR-005**: Buffer MUST maintain a redo stack that is populated when undo is performed and cleared when new edits are made.
- **FR-006**: Buffer MUST support cursor movement operations: left, right, up, down.
- **FR-007**: Buffer MUST support history navigation with optional prefix-based filtering (enable_history_search).
- **FR-008**: Buffer MUST support character, line, and block selection types.
- **FR-009**: Buffer MUST support copy and cut operations that return ClipboardData.
- **FR-010**: Buffer MUST support paste operations with Emacs and Vi paste modes.
- **FR-011**: Buffer MUST manage CompletionState for autocompletion functionality.
- **FR-012**: Buffer MUST support navigation through completions (next, previous, go to index).
- **FR-013**: Buffer MUST support cancellation of completion (revert to original text).
- **FR-014**: Buffer MUST support applying a completion to update the text.
- **FR-015**: Buffer MUST support line transformation via callback functions.
- **FR-016**: Buffer MUST support region transformation via callback functions.
- **FR-017**: Buffer MUST enforce read-only mode by throwing EditReadOnlyBufferException on edit attempts.
- **FR-018**: Buffer MUST support bypass_readonly option for programmatic document updates.
- **FR-019**: Buffer MUST support synchronous validation via the validate method.
- **FR-020**: Buffer MUST support asynchronous validation via validate_while_typing.
- **FR-021**: Buffer MUST track ValidationState (Valid, Invalid, Unknown) and ValidationError.
- **FR-022**: Buffer MUST support auto-suggestion integration with async suggestion retrieval.
- **FR-023**: Buffer MUST support opening content in an external editor via open_in_editor.
- **FR-024**: Buffer MUST fire events on text change, text insert, cursor position change, completions change, and suggestion set.
- **FR-025**: Buffer MUST maintain working_lines (history + current text) for editable history traversal.
- **FR-026**: Buffer MUST support the yank-nth-arg and yank-last-arg Emacs operations.
- **FR-027**: Buffer MUST support line joining (join_next_line, join_selected_lines).
- **FR-028**: Buffer MUST support character swapping (swap_characters_before_cursor).
- **FR-029**: Buffer MUST support newline insertion with optional margin copying.
- **FR-030**: Buffer MUST support inserting lines above and below the current line.
- **FR-031**: Buffer MUST support auto_up and auto_down that intelligently choose between cursor movement and history navigation.
- **FR-032**: Buffer MUST support search operations (document_for_search, get_search_position, apply_search) with SearchState.
- **FR-033**: BufferOperations MUST provide indent function to add indentation to a range of lines.
- **FR-034**: BufferOperations MUST provide unindent function to remove indentation from a range of lines.
- **FR-035**: BufferOperations MUST provide reshape_text function (Vi 'gq' operator) to reformat text within a width.
- **FR-036**: Buffer MUST be thread-safe for all mutable state operations per Constitution XI.

### Key Entities

- **Buffer**: The mutable text container that wraps an immutable Document and provides all editing operations. Contains references to completer, auto-suggest, history, and validator.
- **CompletionState**: Immutable state object tracking the original document, list of completions, and currently selected completion index.
- **YankNthArgState**: State object for tracking yank-nth-arg/yank-last-arg Emacs operations, including history position, argument index, and previously inserted word.
- **ValidationState**: Enum representing the validation state (Valid, Invalid, Unknown).
- **EditReadOnlyBufferException**: Exception thrown when attempting to edit a read-only Buffer.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 35+ Buffer methods function correctly according to Python Prompt Toolkit semantics.
- **SC-002**: Undo/redo operations correctly restore text and cursor position across 100% of test cases.
- **SC-003**: History navigation correctly filters entries when EnableHistorySearch is active.
- **SC-004**: Completion state management correctly maintains original document and allows full navigation through completions.
- **SC-005**: Selection operations correctly handle character, line, and block selection types.
- **SC-006**: Paste operations correctly handle all three paste modes (Emacs, Vi-before, Vi-after).
- **SC-007**: Read-only mode blocks 100% of edit operations unless bypass_readonly is specified.
- **SC-008**: Event handlers fire on all appropriate state changes.
- **SC-009**: Async operations (completion, suggestion, validation) run without blocking and handle document changes correctly.
- **SC-010**: Unit tests achieve minimum 80% code coverage.
- **SC-011**: All thread-safe operations pass concurrent access tests.

## Assumptions

1. The `Document` class from Feature 01 is fully implemented and available.
2. The `SelectionState`, `SelectionType`, and `PasteMode` types from Feature 02 are available.
3. The `ClipboardData` type from Feature 03 is available.
4. The `Suggestion` and `IAutoSuggest` types from Feature 04 are available.
5. The `FastDictCache` from Feature 06 is available for Document caching.
6. The `IHistory` interface will be defined (Feature 07) with methods for loading and appending history.
7. The `ICompleter`, `Completion`, and `CompleteEvent` types will be defined (Feature 08).
8. The `IValidator`, `ValidationError` types will be defined (Feature 09).
9. The `SearchState` and `SearchDirection` types will be defined (Feature 10).
10. Filter parameters (complete_while_typing, etc.) will accept both bool and `Func<bool>` predicates.
11. Indentation uses 4 spaces per indent level (matching Python Prompt Toolkit default).
12. External editor detection uses VISUAL and EDITOR environment variables, falling back to common editor paths.
