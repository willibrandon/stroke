# Feature Specification: Synchronized Output (DEC Mode 2026)

**Feature Branch**: `067-synchronized-output`
**Created**: 2026-02-07
**Status**: Draft
**Input**: User description: "Fix Terminal Resize Flicker via Synchronized Output (DEC Mode 2026)"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Flicker-Free Terminal Resize (Priority: P1)

A developer is using a Stroke-based REPL (such as a database shell or interactive prompt) and resizes the terminal window by dragging the corner. The content re-renders at the new dimensions without any visible blank frame or flicker between the old content disappearing and the new content appearing.

**Why this priority**: Resize flicker is the most user-visible rendering artifact. Every terminal resize produces a jarring blank-frame flash that breaks visual continuity. Eliminating this directly improves perceived quality for all users across all Stroke applications.

**Independent Test**: Can be tested by launching any Stroke prompt example, resizing the terminal window, and observing that content transitions smoothly without a visible blank frame.

**Acceptance Scenarios**:

1. **Given** a Stroke application is running with content displayed, **When** the user resizes the terminal window, **Then** the content re-renders at the new size without any visible blank frame between the old and new content.
2. **Given** a Stroke application is running with a multi-line prompt and bottom toolbar, **When** the user resizes the terminal 5 or more times within 2 seconds, **Then** each resize produces a clean redraw with no accumulated visual artifacts.
3. **Given** a Stroke application is running in a terminal that does not support Mode 2026, **When** the user resizes the terminal window, **Then** the application still re-renders correctly (behavior is no worse than before this feature).

---

### User Story 2 - Atomic Render Updates During Normal Operation (Priority: P2)

A developer is using a Stroke-based application with a live-updating display (e.g., a clock in the prompt, a progress indicator, or auto-completion menu). Each render cycle commits to the terminal atomically, preventing partial-frame artifacts where the top of the screen shows new content while the bottom still shows old content (screen tearing).

**Why this priority**: While less dramatic than resize flicker, render tearing during normal updates degrades the polish of interactive applications. Synchronized output wrapping all render flushes addresses both resize and normal rendering in one mechanism.

**Independent Test**: Can be tested by launching a Stroke prompt with a live clock (e.g., `fancy-zsh-prompt` example) and observing that the clock updates without any partial-render artifacts.

**Acceptance Scenarios**:

1. **Given** a Stroke application with a continuously updating display element, **When** the display element updates, **Then** the terminal shows each frame as a complete, consistent image with no partial updates visible.
2. **Given** a Stroke application performing a screen-clear operation, **When** the clear is executed, **Then** the erase and subsequent content appear as a single atomic update.

---

### User Story 3 - Graceful Degradation on Older Terminals (Priority: P2)

A developer is using a Stroke-based application on an older terminal emulator that does not support DEC Mode 2026 (e.g., older versions of Terminal.app, or a basic Linux console). The application continues to function exactly as it did before this feature — no errors, no garbled output, no behavioral changes.

**Why this priority**: Stroke targets cross-platform compatibility. The synchronized output enhancement must not break any existing terminal. Unsupported terminals must silently ignore the escape sequences.

**Independent Test**: Can be tested by launching any Stroke example in a terminal known not to support Mode 2026 and verifying all functionality works identically to the pre-feature behavior.

**Acceptance Scenarios**:

1. **Given** a Stroke application running on a terminal that does not support Mode 2026, **When** the application renders output, **Then** the escape sequences are silently ignored and rendering works normally.
2. **Given** a Stroke application running on a terminal that does not support Mode 2026, **When** the user resizes the terminal, **Then** the application re-renders with the same behavior as before this feature (no regression).

---

### User Story 4 - Consistent Rendering Across All Output Backends (Priority: P3)

A developer is using a Stroke-based application on Windows with the legacy Win32 console, ConEmu, or Windows Terminal. The synchronized output feature applies correctly to each output backend — VT100-capable backends emit Mode 2026 markers, while the legacy Win32 backend correctly ignores them.

**Why this priority**: Stroke supports multiple output backends. The feature must integrate correctly with all of them without causing regressions on any platform.

**Independent Test**: Can be tested by running a Stroke application under each output backend (VT100, Windows Terminal/Win10, ConEmu, Win32, PlainText, Dummy) and verifying correct behavior.

**Acceptance Scenarios**:

1. **Given** a Stroke application using VT100 output, **When** the application renders, **Then** the render output is wrapped in DEC Mode 2026 begin/end markers.
2. **Given** a Stroke application using Win32 legacy console output, **When** the application renders, **Then** no Mode 2026 escape sequences are emitted.
3. **Given** a Stroke application using PlainText output (piped to a file), **When** the application writes output, **Then** no Mode 2026 escape sequences contaminate the file output.

---

### Edge Cases

- What happens when a render completes in the middle of a synchronized output block and an exception occurs? The end marker must still be sent to avoid leaving the terminal in buffered mode.
- What happens when the terminal auto-disables Mode 2026 after a timeout (e.g., Ghostty's 1-second timeout) during a very slow render? The render completes normally — the terminal simply commits what it has buffered.
- What happens when multiple resize events arrive faster than the render cycle can process them? The invalidation system coalesces them, and only the final size triggers a complete redraw.
- What happens when `Erase()` is called from application exit while synchronized output is active? The erase is wrapped in its own synchronized output block for atomic visual cleanup.
- What happens when the cursor position is stale after a resize (pointing to coordinates outside the new terminal dimensions)? Absolute cursor positioning is used instead of relative movement, which works from any position.
- What happens when the output object is disposed during an active synchronized output block? The dispose proceeds normally; no end marker is sent since the underlying stream is closing. The terminal resets its buffering state when the connection closes.
- What happens when the underlying writer throws an exception during a synchronized flush? The try/finally around the synchronized output block ensures `EndSynchronizedOutput()` is still called to clear the flag. The existing `Flush()` exception handling (swallowing IOException) is unchanged.
- What happens if the process crashes mid-synchronized-output, leaving the terminal in buffered mode? Modern terminals auto-reset Mode 2026 when the writing process exits or the PTY closes. Ghostty additionally uses a 1-second safety timer. No application-level mitigation is needed.
- What happens when Mode 2026 markers are emitted alongside other DEC private mode changes (Mode 1049 alternate screen, Mode 2004 bracketed paste)? Mode 2026 only affects output buffering and does not interact with or override other mode settings. Each DEC private mode operates independently.
- What happens when `Flush()` encounters an empty buffer while synchronized output is active? The flush is a no-op — no Mode 2026 markers are emitted around empty content.
- What happens when `ResetForResize()` is called twice in succession before any render occurs? The second reset is idempotent — all state fields are already at their initial values after the first reset.
- What happens on the first render after application startup when the last screen is already null (without a preceding resize)? The full-redraw path with absolute cursor positioning is used, same as after a resize. This is the normal startup path.
- What happens when a future `IOutput` implementation is added? The interface contract requires implementing the begin and end methods. No-op is the correct default for non-VT100 backends.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The output interface MUST expose methods to begin and end a synchronized output region for controlling DEC Mode 2026.
- **FR-002**: VT100-based output MUST emit the Mode 2026 begin marker (`\x1b[?2026h`) before buffered content and the end marker (`\x1b[?2026l`) after buffered content when synchronized output is active during a call to `Flush()`. When `Flush()` encounters an empty buffer, no markers are emitted.
- **FR-003**: VT100-based output MUST NOT emit Mode 2026 markers when synchronized output is not active.
- **FR-004**: Legacy Win32 console output MUST implement synchronized output methods as no-ops (the legacy console API has no equivalent).
- **FR-005**: Plain text output MUST implement synchronized output methods as no-ops. Mode 2026 escape sequences MUST never appear in plain text output regardless of whether a higher-layer component has activated a synchronized output region.
- **FR-006**: Dummy output MUST implement synchronized output methods as no-ops.
- **FR-007**: Windows 10 hybrid output MUST delegate synchronized output methods to its internal VT100 output instance.
- **FR-008**: ConEmu hybrid output MUST delegate synchronized output methods to its internal VT100 output instance.
- **FR-009**: The renderer's main render method MUST wrap its render-and-flush sequence — including screen diff output, cursor shape updates, and the flush — in a synchronized output begin/end pair. One-time setup operations (alternate screen activation, bracketed paste, cursor key mode, mouse support) that occur before the first content write MAY be outside the synchronized output block.
- **FR-010**: The renderer's erase method MUST wrap its erase-and-flush sequence in a synchronized output begin/end pair.
- **FR-011**: The renderer's clear method MUST wrap its clear-and-flush sequence in a synchronized output begin/end pair. The clear method MUST NOT create nested synchronized output blocks — it MUST inline any erase logic rather than delegating to the separately-wrapped erase method.
- **FR-012**: On terminal resize, the application MUST NOT perform any immediate terminal output. Instead, it MUST reset renderer state, request an absolute cursor position report (a deferred flag set, not immediate I/O), and invalidate to schedule a redraw. The actual erase and redraw occur atomically inside the next render call's synchronized output block.
- **FR-013**: The renderer MUST expose a state-reset method that resets all rendering state to initial values without performing any terminal I/O: cursor position (to origin), last screen (to null), last size (to null), last style (to null), cursor shape (to null), mouse handlers (to new empty instance), min available height (to zero), cursor key mode reset (to false), mouse support enabled (to false).
- **FR-014**: On a full-redraw path (previous screen is null or width changed), the screen diff algorithm MUST use absolute cursor positioning (`\x1b[H` — Cursor Home, row 1 column 1) instead of relative cursor movement to ensure correct positioning when cursor coordinates may be stale after a resize. This applies to all full-redraw triggers, including the first render after application startup.
- **FR-015**: Synchronized output MUST be thread-safe — the synchronized output flag in VT100-based output MUST be protected by the implementation's existing per-instance lock (the same lock used by `Flush()`). The begin and end methods MUST only hold the lock for the duration of the flag mutation, not for the entire begin-to-end region. Renderer methods that wrap output in synchronized blocks are called from the application's event loop thread; concurrent calls to `Render()` from multiple threads are not supported by the existing renderer design.
- **FR-016**: The synchronized output begin and end methods MUST be idempotent. Multiple consecutive begin calls without an intervening end MUST be safe (the flag remains active). Multiple consecutive end calls without an intervening begin MUST be safe (the flag remains inactive).
- **FR-017**: All synchronized output regions MUST use try/finally to guarantee that `EndSynchronizedOutput()` is called even when an exception occurs during rendering. The terminal MUST NOT be left in buffered mode due to an unhandled exception.

### Non-Functional Requirements

- **NFR-001**: The Mode 2026 escape sequences MUST impose negligible overhead — less than 20 bytes per render flush (8-byte begin marker + 8-byte end marker = 16 bytes actual).
- **NFR-002**: Terminals that do not support Mode 2026 MUST silently ignore the sequences with no visible effect — this is guaranteed by the DEC Private Mode specification.
- **NFR-003**: The existing test suite (9,311+ tests) MUST continue to pass with no regressions.
- **NFR-004**: All changes MUST maintain thread safety consistent with the project's thread safety requirements.
- **NFR-005**: New synchronized output tests MUST achieve at least 80% branch coverage of the added code, consistent with the project's testing standards.

### Key Entities

- **Synchronized Output Region**: A logical bracket around terminal output where the terminal buffers all received data and commits it atomically as a single frame. Delimited by Mode 2026 begin and end markers.
- **Renderer State**: The collection of cached state the renderer uses to compute differential screen updates — includes cursor position, last rendered screen, last terminal size, last style, cursor shape, mouse handlers, and various mode flags.

## Assumptions

- Mode 2026 is a well-established standard supported by all major modern terminal emulators (Windows Terminal v1.23+, iTerm2, Kitty, Alacritty, Warp, Ghostty, WezTerm, foot, Contour, mintty).
- Terminals that do not support Mode 2026 silently ignore the escape sequences per the DEC Private Mode specification — this is how all DEC private modes have worked since VT100.
- The existing invalidation/coalescing mechanism in Stroke's event loop handles rapid resize events appropriately (multiple resize events within a single render cycle are collapsed into one redraw).
- This is a documented enhancement over the shared Python Prompt Toolkit rendering architecture, not a deviation from API compatibility. No public APIs are changed.
- `RequestAbsoluteCursorPosition()` in the renderer sets an in-memory flag that causes the CPR query (`\x1b[6n`) to be sent during the next `Render()` call — it does not perform immediate terminal I/O.
- `Renderer.Render()` is called from the application's event loop thread. Concurrent calls to `Render()` from multiple threads are not supported by the existing renderer design.
- Windows10Output's per-instance lock and its internal Vt100Output's per-instance lock are independent. The delegation pattern (Windows10Output delegates `BeginSynchronizedOutput()`/`EndSynchronizedOutput()` to Vt100Output) does not create lock ordering dependencies because Windows10Output's `Flush()` acquires its own lock and then calls `Vt100Output.Flush()` which acquires Vt100Output's lock — this nested lock acquisition order is always consistent (outer then inner), preventing deadlock.
- The synchronized output flag in Vt100Output depends on the existing `_lock` field (of type `System.Threading.Lock`). This is an implementation coupling that must be maintained.
- Mode 2026 render cycles in Stroke complete in single-digit milliseconds under normal conditions, well within terminal timeout thresholds (e.g., Ghostty's 1-second safety timer). Slow completers or complex layouts may extend render time, but the try/finally pattern ensures the end marker is always sent regardless of duration.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Terminal resize produces zero visible blank frames between old content disappearing and new content appearing, on terminals that support Mode 2026. Verified by capturing raw VT100 output and confirming that erase and redraw sequences occur within the same Mode 2026 begin/end block.
- **SC-002**: Every non-empty render flush emitted by VT100-based output while synchronized output has been activated via `BeginSynchronizedOutput()` begins with `\x1b[?2026h` and ends with `\x1b[?2026l`.
- **SC-003**: Non-VT100 output implementations (Win32, PlainText, Dummy) emit zero Mode 2026 escape sequences.
- **SC-004**: The resize handler performs zero terminal output operations — only in-memory state resets and a deferred cursor position report flag set.
- **SC-005**: Full redraw path uses absolute cursor positioning instead of relative movement.
- **SC-006**: All existing tests (9,311+) pass with zero regressions after the changes.
- **SC-007**: All existing examples (102+) render correctly after the changes, verified by launching each example, performing basic interactions, and confirming no rendering regressions via TUI driver text capture.
- **SC-008**: Every `Renderer.Render()`, `Erase()`, and `Clear()` call wraps its output operations in a synchronized output begin/end pair, verified by capturing VT100 output and checking for Mode 2026 markers.
- **SC-009**: On terminals that do not support Mode 2026, all Stroke functionality works identically to pre-feature behavior — the Mode 2026 escape sequences are silently ignored by the terminal with no visible effect.
