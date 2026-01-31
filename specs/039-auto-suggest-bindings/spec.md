# Feature Specification: Auto Suggest Bindings

**Feature Branch**: `039-auto-suggest-bindings`
**Created**: 2026-01-31
**Status**: Draft
**Input**: User description: "Implement key bindings for accepting and partially accepting Fish-style auto suggestions"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Accept Full Auto Suggestion (Priority: P1)

A developer is typing a command in a terminal REPL powered by Stroke. The system offers a Fish-style auto suggestion based on their history or context. The suggestion appears dimmed after the cursor. The developer presses the Right arrow key to accept the entire suggestion, which inserts the full suggested text into the buffer.

**Why this priority**: Full suggestion acceptance is the primary use case for auto suggest. Without it, the suggestion feature provides no user value since there is no way to accept suggestions.

**Independent Test**: Can be fully tested by creating a buffer with a suggestion, simulating a Right arrow key press, and verifying the suggestion text is inserted into the buffer.

**Acceptance Scenarios**:

1. **Given** a buffer with text "git" and a suggestion of " commit -m 'fix bug'", **When** the user presses Right arrow, **Then** the buffer text becomes "git commit -m 'fix bug'"
2. **Given** a buffer with text "git" and a suggestion of " commit -m 'fix bug'", **When** the user presses Ctrl-F, **Then** the buffer text becomes "git commit -m 'fix bug'"
3. **Given** a buffer with text "git" and a suggestion of " commit -m 'fix bug'", **When** the user presses Ctrl-E, **Then** the buffer text becomes "git commit -m 'fix bug'"

---

### User Story 2 - Accept Partial Suggestion by Word (Priority: P2)

A developer sees a long auto suggestion and wants to accept only the next word or path segment rather than the full suggestion. In Emacs editing mode, they press Escape followed by F to accept the next word boundary from the suggestion. This allows incremental acceptance through command arguments and file path segments.

**Why this priority**: Partial acceptance gives users fine-grained control over suggestion adoption, which is essential for long suggestions where the user may want to diverge after a few words.

**Independent Test**: Can be fully tested by creating a buffer with a multi-word suggestion, simulating Escape-F in Emacs mode, and verifying only the first word segment is inserted.

**Acceptance Scenarios**:

1. **Given** a buffer with suggestion "commit -m 'message'" in Emacs mode, **When** the user presses Escape then F, **Then** only "commit " is inserted into the buffer
2. **Given** a buffer with suggestion "home/user/documents/" in Emacs mode, **When** the user presses Escape then F, **Then** only "home/" is inserted into the buffer
3. **Given** a buffer with suggestion "commit -m 'message'" in Vi mode, **When** the user presses Escape then F, **Then** the partial acceptance binding does not trigger (Vi mode excluded)

---

### User Story 3 - Bindings Inactive When No Suggestion (Priority: P1)

A developer is typing normally with no auto suggestion visible. The Right arrow key, Ctrl-F, and Ctrl-E must retain their standard behavior (cursor movement, character search, move to end of line) rather than attempting to accept a nonexistent suggestion. The auto suggest bindings only activate when a suggestion is present and the cursor is at the end of the buffer.

**Why this priority**: Preventing interference with standard key behavior is critical. Without proper filtering, the auto suggest bindings would break normal editing when no suggestion is available.

**Independent Test**: Can be fully tested by creating a buffer with no suggestion and verifying that Right arrow, Ctrl-F, and Ctrl-E do not trigger suggestion acceptance.

**Acceptance Scenarios**:

1. **Given** a buffer with no suggestion, **When** the user presses Right arrow, **Then** the auto suggest binding does not activate and the key passes through to normal handling
2. **Given** a buffer with a suggestion but the cursor is NOT at the end of the buffer, **When** the user presses Right arrow, **Then** the auto suggest binding does not activate and the key passes through to normal handling
3. **Given** a buffer with an empty suggestion (empty text), **When** the user presses Right arrow, **Then** the auto suggest binding does not activate

---

### User Story 4 - Bindings Override Vi Right Arrow When Suggestion Available (Priority: P2)

A developer using Vi editing mode has an auto suggestion visible while the cursor is at the end of the buffer. When they press the Right arrow, the auto suggest binding takes priority over Vi's standard right arrow movement because the auto suggest bindings are loaded after Vi bindings.

**Why this priority**: Proper binding priority is essential for Vi mode users who expect suggestion acceptance to work the same as in Emacs mode when a suggestion is available.

**Independent Test**: Can be fully tested by ensuring auto suggest bindings load after Vi bindings and verifying that Right arrow accepts the suggestion when one is available.

**Acceptance Scenarios**:

1. **Given** a buffer in Vi navigation mode with a suggestion visible and cursor at end, **When** the user presses Right arrow, **Then** the suggestion is accepted (auto suggest binding wins over Vi binding)
2. **Given** a buffer in Vi navigation mode with no suggestion, **When** the user presses Right arrow, **Then** the Vi right arrow behavior executes normally

---

### Edge Cases

- What happens when the suggestion text is a single character? The full acceptance inserts that single character. For partial acceptance, the single character has no word boundary, so the entire suggestion text ("x") is accepted as the first non-empty segment.
- What happens when the partial suggestion has no word boundaries (e.g., "abc")? The regex produces no capturing matches, so `Regex.Split` returns the entire string as a single element. The first non-empty segment is the full suggestion text, effectively behaving the same as full acceptance.
- What happens when the suggestion consists only of whitespace? The regex `[^\s/]+` requires at least one non-whitespace/non-slash character, so it produces no matches. `Regex.Split` returns the whitespace string as a single element. The first non-empty segment is the whitespace string itself.
- What happens when the suggestion contains path separators at the start (e.g., "/usr/bin")? `Regex.Split` produces `["/", "usr/", "bin"]`. The first non-empty element is `"/"` — just the leading path separator. This is because the regex pattern `[^\s/]+` cannot match "/" characters, so the leading "/" falls into the text-before-first-match position in the split result.
- What happens if the buffer's suggestion property becomes null between the filter check and the handler execution? The handler has a null-check guard (`if (suggestion is not null)`) and safely does nothing, returning `null`.
- What happens when `Buffer.Suggestion` is non-null but `Suggestion.Text` is an empty string? The `SuggestionAvailable` filter checks `Suggestion.Text.Length > 0`, so the binding does not activate. The key event passes through to normal handling.
- What happens if `AppContext.GetApp()` fails because no application is current? The `SuggestionAvailable` filter calls `AppContext.GetApp()` which throws if no app is set. However, key bindings are only evaluated within an active application event loop, so this condition cannot occur during normal operation. No defensive handling is needed.
- What happens if `Regex.Split` produces only empty strings (e.g., theoretically if the pattern matched the entire input as delimiters)? The `First(x => !string.IsNullOrEmpty(x))` call would throw `InvalidOperationException`. However, with the pattern `([^\s/]+(?:\s+|/))` this cannot occur: the pattern requires non-empty captured groups, and `Regex.Split` always includes any non-matched remainder. The suggestion text being non-empty (guaranteed by filter) ensures at least one non-empty element.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `LoadAutoSuggestBindings` method that returns a key bindings collection containing all auto suggest key bindings
- **FR-002**: System MUST bind Ctrl-F, Ctrl-E, and Right arrow to accept the full suggestion text when a suggestion is available and the cursor is at the end of the buffer
- **FR-003**: System MUST bind Escape followed by F to accept the next word segment of the suggestion, active only in Emacs editing mode when a suggestion is available and the cursor is at the end of the buffer
- **FR-004**: System MUST define a "suggestion available" filter condition that evaluates to true only when all three conditions are met: the current buffer has a non-null suggestion, the suggestion text is non-empty, and the cursor is at the end of the document
- **FR-005**: System MUST use word boundary splitting that respects both whitespace boundaries and path separator (/) boundaries for partial acceptance
- **FR-006**: The word boundary pattern MUST match sequences of non-whitespace/non-slash characters followed by whitespace or a slash, consistent with the Python Prompt Toolkit behavior
- **FR-007**: System MUST ensure auto suggest bindings do not interfere with normal key behavior when no suggestion is available (bindings must be conditional on the suggestion available filter)
- **FR-008**: System MUST load auto suggest bindings after Vi bindings so that suggestion acceptance takes priority over Vi cursor movement when a suggestion is available

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All three full-acceptance keys (Ctrl-F, Ctrl-E, Right arrow) insert the complete suggestion text when a suggestion is available and the cursor is at the end of the buffer
- **SC-002**: Partial acceptance (Escape-F) inserts exactly one word/path segment from the suggestion in Emacs mode
- **SC-003**: None of the auto suggest bindings activate when no suggestion is present, the suggestion text is empty, or the cursor is not at the end of the buffer
- **SC-004**: Auto suggest bindings override Vi right arrow behavior when a suggestion is available
- **SC-005**: Unit tests achieve at least 80% code coverage of the auto suggest bindings module
- **SC-006**: Word boundary splitting correctly handles command arguments (whitespace-separated), path segments (slash-separated), and mixed content

## Assumptions

- The `Buffer.Suggestion` property and `Suggestion` class already exist from prior feature work (auto-suggest system)
- The `Document.IsCursorAtTheEnd` property already exists from the immutable Document implementation
- The `Condition` constructor (`new Condition(() => ...)`) and Emacs mode filter are available from the filter system
- The key bindings system supports multi-key sequences (e.g., Escape then F) and filter parameters
- The application context provides access to the current application and focused buffer
