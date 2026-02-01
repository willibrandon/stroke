# Feature Specification: Open in Editor Bindings

**Feature Branch**: `041-open-in-editor-bindings`
**Created**: 2026-01-31
**Status**: Draft
**Input**: User description: "Feature 64: Open in Editor Bindings - Implement key bindings for opening the current buffer content in an external editor, typically used for editing complex multi-line input."

## Clarifications

### Session 2026-01-31

- Q: Editor resolution order — spec had $EDITOR > $VISUAL but Python checks $VISUAL first. Which order? → A: Match Python exactly: $VISUAL > $EDITOR > /usr/bin/editor > /usr/bin/nano > /usr/bin/pico > /usr/bin/vi > /usr/bin/emacs (POSIX convention, faithful port).
- Q: Should edit-and-execute-command auto-accept input after editing? Python calls validate_and_handle=True. → A: Match Python: auto-validate and accept after edit (faithful port).
- Correction: Multiple spec behaviors were updated to match Python source exactly — read-only buffer guard, trailing newline stripping, editor exit code check, shell-split editor command parsing. These were spec generation errors, not ambiguities.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Emacs User Opens Buffer in External Editor (Priority: P1)

A developer using Emacs key bindings is composing a complex multi-line command in their terminal application. They press Ctrl-X followed by Ctrl-E to open the current buffer content in their configured external editor ($EDITOR). The terminal UI is temporarily suspended while the editor is active. After editing and saving, the modified content replaces the buffer text and the terminal UI resumes.

**Why this priority**: This is the primary use case. Emacs mode is the default editing mode in most prompt toolkit applications. The Ctrl-X Ctrl-E binding is a well-known Emacs/Bash convention for editing the current command line in an external editor.

**Independent Test**: Can be tested by setting up key bindings in Emacs mode, triggering Ctrl-X Ctrl-E, and verifying the binding is registered with the correct filter conditions and handler. The actual editor launch relies on the `edit-and-execute-command` named command.

**Acceptance Scenarios**:

1. **Given** the application is in Emacs editing mode with no active selection, **When** the user presses Ctrl-X followed by Ctrl-E, **Then** the `edit-and-execute-command` named command is invoked.
2. **Given** the application is in Emacs editing mode with an active text selection, **When** the user presses Ctrl-X followed by Ctrl-E, **Then** the binding does not trigger.
3. **Given** the application is in Vi editing mode, **When** the user presses Ctrl-X followed by Ctrl-E, **Then** the binding does not trigger.

---

### User Story 2 - Vi User Opens Buffer in External Editor (Priority: P1)

A developer using Vi key bindings is in navigation mode composing input. They press 'v' to open the current buffer content in their configured external editor. The terminal UI is temporarily suspended while the editor runs. After editing and saving, the modified content replaces the buffer and the terminal UI resumes.

**Why this priority**: Vi mode is equally important as Emacs mode. The 'v' binding in navigation mode is the standard Vi convention for opening the command line in an external editor.

**Independent Test**: Can be tested by setting up key bindings in Vi mode, triggering 'v' in navigation mode, and verifying the binding is registered with the correct filter and handler.

**Acceptance Scenarios**:

1. **Given** the application is in Vi navigation mode, **When** the user presses 'v', **Then** the `edit-and-execute-command` named command is invoked.
2. **Given** the application is in Vi insert mode, **When** the user presses 'v', **Then** the binding does not trigger and 'v' is inserted as text.

---

### User Story 3 - Combined Binding Loader (Priority: P2)

An application developer building a terminal application wants to load all open-in-editor bindings at once without needing to separately load Emacs and Vi bindings. They call a single function that returns a merged set of both Emacs and Vi editor-opening key bindings.

**Why this priority**: This is a convenience API that combines the P1 bindings. It simplifies application setup by providing a single entry point.

**Independent Test**: Can be tested by calling the combined loader and verifying it returns a merged key bindings object containing both Emacs and Vi bindings.

**Acceptance Scenarios**:

1. **Given** the combined binding loader is called, **When** the returned bindings are inspected, **Then** both Emacs (Ctrl-X Ctrl-E) and Vi ('v' in navigation mode) bindings are present.
2. **Given** the combined bindings are loaded into an application, **When** the user switches between Emacs and Vi modes, **Then** the appropriate binding activates based on the current mode.

---

### User Story 4 - Edit-and-Execute Named Command (Priority: P1) *(Already Implemented)*

> **Note**: This user story documents the behavior of the `edit-and-execute-command` named command and `Buffer.OpenInEditorAsync`, both of which are **already implemented** in Features 034 and 007 respectively. This story is included for completeness and to document the end-to-end behavior that the new key bindings (User Stories 1–3) depend on. No new code is needed for this story.

When the open-in-editor binding is triggered, the `edit-and-execute-command` named command delegates to `Buffer.OpenInEditorAsync(validateAndHandle: true)`. This checks the buffer is not read-only, writes the current buffer content to a temporary file, launches the user's preferred editor (determined by $VISUAL, $EDITOR, or fallback executables), suspends the terminal UI during editing, and upon successful editor exit (return code 0) reads the modified content back (stripping any trailing newline), updates the buffer, then auto-validates and accepts the input. The temporary file is always cleaned up.

**Why this priority**: This is the core functionality that makes the bindings useful. Without the named command implementation, the key bindings have no effect. Already implemented in `NamedCommands.Misc.cs` and `Buffer.ExternalEditor.cs`.

**Independent Test**: Can be tested by invoking the named command directly and verifying: temporary file creation with buffer content, editor environment variable resolution, buffer update after editing, and temporary file cleanup.

**Acceptance Scenarios**:

1. **Given** the buffer is not read-only and contains text "hello world", **When** `edit-and-execute-command` is invoked, **Then** a temporary file is created containing "hello world".
2. **Given** the buffer is read-only, **When** `edit-and-execute-command` is invoked, **Then** an error is raised and the buffer remains unchanged.
3. **Given** $VISUAL is set to a valid editor command, **When** `edit-and-execute-command` is invoked, **Then** the $VISUAL editor is launched with the temporary file path (command is shell-split to handle arguments and spaces).
4. **Given** $VISUAL is not set but $EDITOR is set, **When** `edit-and-execute-command` is invoked, **Then** the $EDITOR editor is used as fallback.
5. **Given** neither $VISUAL nor $EDITOR is set on a Unix system, **When** `edit-and-execute-command` is invoked, **Then** the system tries /usr/bin/editor, /usr/bin/nano, /usr/bin/pico, /usr/bin/vi, /usr/bin/emacs in order, using the first available.
6. **Given** none of the editor candidates are available, **When** `edit-and-execute-command` is invoked, **Then** the command fails gracefully and the buffer remains unchanged.
7. **Given** the editor exits with return code 0, **When** the content is read back, **Then** any trailing newline is stripped, the buffer document is updated with cursor at the end, and the input is auto-validated and accepted.
8. **Given** the editor exits with a non-zero return code, **When** the command processes the result, **Then** the buffer remains unchanged (content is not read back).
9. **Given** `edit-and-execute-command` is invoked, **When** the command completes (regardless of success or failure), **Then** the temporary file is deleted.
10. **Given** `edit-and-execute-command` is invoked, **When** the editor is running, **Then** the terminal UI is suspended and the editor has direct terminal access.

---

### Edge Cases

- What happens when the editor process fails to start (invalid editor command)? The system tries the next editor in the fallback list. If all fail, the buffer remains unchanged and the temporary file is cleaned up.
- What happens when the temporary file cannot be created (disk full, permissions)? An appropriate error is surfaced and the buffer remains unchanged.
- What happens when the editor exits with a non-zero return code? The buffer is NOT updated (content is not read back), but the temporary file is still cleaned up.
- What happens when the editor exits without saving changes? The editor returns 0, so the unchanged file content is read back — the buffer gets the same text it had before (no visible change), and input is auto-accepted.
- What happens when the temporary file is deleted externally while the editor is running? The read-back fails gracefully and the buffer remains unchanged.
- What happens when the buffer is empty? An empty temporary file is created and the editor opens with no content.
- What happens when the buffer is read-only? An error is raised immediately and no editor is launched.
- What happens when $VISUAL contains arguments (e.g., "code --wait")? The command string is shell-split so "code" is the executable and "--wait" is an argument.
- What happens if the `edit-and-execute-command` named command is not registered when the binding loader is called? The `NamedCommands.GetByName` call throws `KeyNotFoundException`. This cannot happen in practice because the named command is registered in the `NamedCommands` static constructor (Feature 034), which runs before any binding loaders are invoked during application setup.

## Requirements *(mandatory)*

### Functional Requirements

**New Code (this feature)**:

- **FR-001**: System MUST provide a function to load Emacs open-in-editor key bindings that registers Ctrl-X Ctrl-E with the `edit-and-execute-command` handler, filtered to Emacs mode with no active selection.
- **FR-002**: System MUST provide a function to load Vi open-in-editor key bindings that registers 'v' with the `edit-and-execute-command` handler, filtered to Vi navigation mode.
- **FR-003**: System MUST provide a combined function that merges both Emacs and Vi open-in-editor bindings into a `MergedKeyBindings` instance (implementing `IKeyBindingsBase`) via `merge_key_bindings` equivalent, with no outer `ConditionalKeyBindings` wrapper.
- **FR-009**: The Emacs binding MUST NOT trigger when a text selection is active.
- **FR-010**: The Vi binding MUST only trigger in Vi navigation mode, not in insert, visual, or other Vi modes.

**Existing Infrastructure (already implemented, documented for completeness)**:

- **FR-004**: System MUST implement the `edit-and-execute-command` named command that delegates to `Buffer.OpenInEditorAsync(validateAndHandle: true)`, which checks the buffer is not read-only, writes buffer content to a temporary file, launches an external editor, reads modified content back on success (exit code 0), strips trailing newline, updates the buffer, and auto-validates/accepts the input. *(Implemented in Feature 034 — `NamedCommands.Misc.cs`)*
- **FR-005**: System MUST resolve the editor command by checking $VISUAL first, then $EDITOR, then falling back through /usr/bin/editor, /usr/bin/nano, /usr/bin/pico, /usr/bin/vi, /usr/bin/emacs in order, using the first available executable. On Windows, the fallback list should use platform-appropriate defaults. *(Implemented in Feature 007 — `Buffer.ExternalEditor.cs`)*
- **FR-006**: System MUST suspend the terminal UI while the external editor is running, providing the editor with direct terminal access. *(Implemented in Feature 007 — `Buffer.ExternalEditor.cs`)*
- **FR-007**: System MUST clean up the temporary file after the editor exits, regardless of success or failure. *(Implemented in Feature 007 — `Buffer.ExternalEditor.cs`)*
- **FR-008**: System MUST update the buffer document with the edited content (trailing newline stripped) and position the cursor at the end of the text, only when the editor exits with return code 0. *(Implemented in Feature 007 — `Buffer.ExternalEditor.cs`)*
- **FR-011**: System MUST raise an error if `open_in_editor` is called on a read-only buffer. *(Implemented in Feature 007 — `Buffer.ExternalEditor.cs`)*
- **FR-012**: System MUST shell-split the $VISUAL and $EDITOR values to support editor commands containing arguments and spaces. *(Implemented in Feature 007 — `Buffer.ExternalEditor.cs`)*
- **FR-013**: System MUST auto-validate and accept the input after successfully updating the buffer from the editor (matching Python's `validate_and_handle=True` behavior). *(Implemented in Feature 007 — `Buffer.ExternalEditor.cs`)*
- **FR-014**: If an editor candidate fails to execute (e.g., file not found), the system MUST try the next candidate in the fallback list rather than failing immediately. *(Implemented in Feature 007 — `Buffer.ExternalEditor.cs`)*

### Key Entities

- **OpenInEditorBindings**: Static class providing three binding loader functions (Emacs, Vi, and combined).
- **edit-and-execute-command**: Named command registered in the NamedCommands registry that handles the editor launch lifecycle (temp file, editor process, content read-back, cleanup).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All three binding loader functions return valid key binding objects with the correct number of bindings: Emacs loader returns `KeyBindings` with 1 binding, Vi loader returns `KeyBindings` with 1 binding, combined loader returns `IKeyBindingsBase` (via `MergedKeyBindings`) whose flattened `.Bindings` collection contains 2 bindings total.
- **SC-002**: Emacs binding only activates in Emacs mode without selection; Vi binding only activates in Vi navigation mode.
- **SC-003**: The editor environment variable resolution follows the correct priority order ($VISUAL > $EDITOR > fallback editor list) in all cases.
- **SC-004**: Temporary files are created with correct buffer content and cleaned up after editor exit in all code paths (success, failure, exception).
- **SC-005**: Buffer document is updated with edited content (trailing newline stripped) only on editor exit code 0, followed by auto-validation and acceptance.
- **SC-006**: Unit test coverage reaches 80% or higher for the open-in-editor bindings module (16 tests covering all 3 public methods, all filter conditions, all key sequences, and all handler references — achieving near-100% coverage of the ~60 LOC implementation).
- **SC-007**: The implementation faithfully ports all three Python Prompt Toolkit functions from `open_in_editor.py` with matching semantics.
- **SC-008**: Read-only buffers are rejected before any file I/O or editor launch occurs.
- **SC-009**: Editor commands with spaces/arguments in $VISUAL/$EDITOR are correctly parsed via shell splitting.

## Assumptions

- The `NamedCommands` registry (Feature 034) is already implemented and supports the `GetByName` method for retrieving registered commands.
- The `KeyBindings` class (Feature 022) supports the `Add` method with key sequences, handlers, and filter parameters.
- Application filters (`EmacsMode`, `ViNavigationMode`, `HasSelection`) are available from the Filters module (Features 012/032).
- The `Application.RunInTerminalAsync` method (Feature 030) is implemented and correctly suspends/resumes the UI.
- The `MergedKeyBindings` or equivalent merge mechanism exists for combining multiple `KeyBindings` objects.
- The `Buffer.Document` property is settable, allowing the buffer content to be updated after editing.

## Scope Boundaries

**In Scope (New Code)**:
- Three binding loader functions (Emacs, Vi, combined) — FR-001, FR-002, FR-003
- Emacs binding selection guard filter — FR-009
- Vi binding navigation mode filter — FR-010
- Unit tests for all new binding loaders — SC-006

**In Scope (Existing Infrastructure, Documented for Completeness)**:
- `edit-and-execute-command` named command — FR-004 *(implemented in Feature 034)*
- Editor environment variable resolution with platform-aware defaults — FR-005 *(implemented in Feature 007)*
- Terminal UI suspension during editing — FR-006 *(implemented in Feature 007)*
- Temporary file lifecycle management — FR-007 *(implemented in Feature 007)*
- Buffer update with trailing newline stripping — FR-008 *(implemented in Feature 007)*
- Read-only buffer guard — FR-011 *(implemented in Feature 007)*
- Shell-split editor command parsing — FR-012 *(implemented in Feature 007)*
- Auto-validate and accept after edit — FR-013 *(implemented in Feature 007)*
- Editor fallback list traversal — FR-014 *(implemented in Feature 007)*

**Out of Scope**:
- Syntax highlighting or file type detection for the temporary file
- Undo/redo integration for the external edit operation
- Custom tempfile naming via `Buffer.Tempfile` / `Buffer.TempfileSuffix` properties (these are Buffer-level features, not binding-level)
